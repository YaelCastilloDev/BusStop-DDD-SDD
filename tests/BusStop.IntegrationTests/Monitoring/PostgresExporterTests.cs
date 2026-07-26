using System.Net;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace BusStop.IntegrationTests.Monitoring;

[Collection("PostgreSQL")]
public class PostgresExporterTests : IntegrationTestBase, IAsyncLifetime
{
    private IContainer? _exporterContainer;
    private string _exporterUrl = string.Empty;
    private static readonly HttpClient _httpClient = new();

    public PostgresExporterTests(PostgreSqlFixture fixture) : base(fixture) { }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        var pgPort = _fixture.Container.GetMappedPublicPort(5432);
        var dataSourceName = $"postgresql://postgres:postgres@host.docker.internal:{pgPort}/busstop_test?sslmode=disable";

        _exporterContainer = new ContainerBuilder("quay.io/prometheuscommunity/postgres-exporter:v0.17.0")
            .WithPortBinding(9187, true)
            .WithEnvironment("DATA_SOURCE_NAME", dataSourceName)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(r => r.ForPort(9187)))
            .Build();

        await _exporterContainer.StartAsync();
        var mappedPort = _exporterContainer.GetMappedPublicPort(9187);
        _exporterUrl = $"http://localhost:{mappedPort}";

        await WaitForExporterAsync();
    }

    private async Task WaitForExporterAsync()
    {
        for (var i = 0; i < 15; i++)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_exporterUrl}/metrics", Current.CancellationToken);
                if (response.StatusCode == HttpStatusCode.OK) return;
            }
            catch { }

            await Task.Delay(1000, Current.CancellationToken);
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_exporterContainer is not null)
            await _exporterContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task MetricsEndpoint_Returns200()
    {
        var response = await _httpClient.GetAsync($"{_exporterUrl}/metrics", Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MetricsEndpoint_ContainsPgUp()
    {
        var response = await _httpClient.GetAsync($"{_exporterUrl}/metrics", Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(Current.CancellationToken);

        body.ShouldContain("pg_up");
    }
}

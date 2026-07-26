namespace BusStop.IntegrationTests.Monitoring;

public class OtelCollectorConfigTests
{
    [Fact]
    public void ConfigFile_Exists()
    {
        var configPath = FindConfigFile("containers/otel-collector/config.yml");
        File.Exists(configPath).ShouldBeTrue();
    }

    [Fact]
    public void ConfigFile_ContainsOtlpReceiver()
    {
        var configPath = FindConfigFile("containers/otel-collector/config.yml");
        var content = File.ReadAllText(configPath);

        content.ShouldContain("otlp");
        content.ShouldContain("4317");
        content.ShouldContain("4318");
    }

    [Fact]
    public void ConfigFile_ContainsPrometheusExporter()
    {
        var configPath = FindConfigFile("containers/otel-collector/config.yml");
        var content = File.ReadAllText(configPath);

        content.ShouldContain("prometheus");
        content.ShouldContain("8889");
    }

    [Fact]
    public void ConfigFile_ContainsMetricsPipeline()
    {
        var configPath = FindConfigFile("containers/otel-collector/config.yml");
        var content = File.ReadAllText(configPath);

        content.ShouldContain("pipelines");
        content.ShouldContain("metrics");
    }

    private static string FindConfigFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath);
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", relativePath));
    }
}

namespace BusStop.IntegrationTests.Monitoring;

public class GrafanaConfigTests
{
    [Fact]
    public void DatasourceConfig_Exists()
    {
        var configPath = FindConfigFile("containers/grafana/datasources/datasource.yml");
        File.Exists(configPath).ShouldBeTrue();
    }

    [Fact]
    public void DatasourceConfig_ReferencesPrometheus()
    {
        var configPath = FindConfigFile("containers/grafana/datasources/datasource.yml");
        var content = File.ReadAllText(configPath);

        content.ShouldContain("prometheus");
        content.ShouldContain("Prometheus");
        content.ShouldContain("isDefault");
    }

    [Fact]
    public void DashboardProviderConfig_Exists()
    {
        var configPath = FindConfigFile("containers/grafana/dashboards/dashboard.yml");
        File.Exists(configPath).ShouldBeTrue();
    }

    [Fact]
    public void DashboardProviderConfig_Valid()
    {
        var configPath = FindConfigFile("containers/grafana/dashboards/dashboard.yml");
        var content = File.ReadAllText(configPath);

        content.ShouldContain("apiVersion");
        content.ShouldContain("providers");
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

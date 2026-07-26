namespace BusStop.IntegrationTests.Monitoring;

public class PrometheusConfigTests
{
    [Fact]
    public void ConfigFile_Exists()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "containers", "prometheus", "prometheus.yml");

        if (!File.Exists(configPath))
            configPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
                "..", "..", "containers", "prometheus", "prometheus.yml");

        if (!File.Exists(configPath))
        {
            configPath = FindConfigFile("containers/prometheus/prometheus.yml");
        }

        File.Exists(configPath).ShouldBeTrue($"Expected prometheus.yml at: {configPath}");
    }

    [Fact]
    public void ConfigFile_ContainsScrapeTargets()
    {
        var configPath = FindConfigFile("containers/prometheus/prometheus.yml");
        var content = File.ReadAllText(configPath);

        content.ShouldContain("postgres-exporter");
        content.ShouldContain("otel-collector");
        content.ShouldContain("prometheus");
    }

    [Fact]
    public void ConfigFile_ContainsRequiredJobs()
    {
        var configPath = FindConfigFile("containers/prometheus/prometheus.yml");
        var content = File.ReadAllText(configPath);

        content.ShouldContain("job_name");
        content.ShouldContain("scrape_interval");
        content.ShouldContain("targets");
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

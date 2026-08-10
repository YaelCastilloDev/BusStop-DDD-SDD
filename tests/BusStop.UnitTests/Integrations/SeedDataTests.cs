using Ardalis.Result;
using BusStop.Core.CountryAggregate;

namespace BusStop.UnitTests.Integrations;

public class SeedDataTests
{
    /// <summary>
    /// Documents a bug in SeedData.cs lines 260-262 where <c>Country.Create(kvp.Value, kvp.Key).Value</c>
    /// is called without checking <c>.IsSuccess</c> first. In Ardalis.Result v10, accessing
    /// <c>.Value</c> on a failed result returns <c>default(T)</c> (null for reference types)
    /// instead of throwing. This means a single bad seed entry silently introduces a null
    /// Country into the list, which will later fail when <c>dbContext.Countries.AddRange</c>
    /// is called — but only at SaveChangesAsync time, with a confusing error.
    /// The fix is to check <c>.IsSuccess</c> before accessing <c>.Value</c>
    /// or filter out failed results.
    /// </summary>
    [Fact]
    public void CountryCreate_ReturnsNull_WhenInvalidSeedEntry()
    {
        // Arrange: Create a Country with invalid input that produces a failed Result.
        // This simulates what would happen if a seed entry had bad data.
        var failedResult = Country.Create("", "");

        // Assert: The creation itself should produce a failed result, not throw.
        failedResult.IsSuccess.ShouldBeFalse();

        // Act: Accessing .Value on a failed Result<Country> returns null (Ardalis.Result v10).
        // This is the bug: SeedData.InitializeAsync() calls .Value unconditionally,
        // so a bad seed entry results in a null Country being added to the list.
        var country = failedResult.Value;

        // Assert: .Value on a failed Result returns null/default.
        // When SeedData adds this null to dbContext.Countries and calls SaveChangesAsync,
        // it will fail with a confusing error (e.g., NullReferenceException or EF Core
        // complaining about a null entity).
        country.ShouldBeNull();
    }

    /// <summary>
    /// Documents the expected correct pattern: check <c>.IsSuccess</c> before accessing <c>.Value</c>.
    /// If SeedData used this pattern, bad entries would be skipped instead of introducing nulls.
    /// </summary>
    [Fact]
    public void CountryCreate_SafeAccess_ChecksIsSuccessFirst()
    {
        // Arrange
        var seedEntries = new Dictionary<string, string>
        {
            ["US"] = "United States",
            ["BAD"] = "", // invalid entry
        };

        // Act: The safe pattern filters failed results instead of calling .Value blindly.
        var countries = seedEntries
            .Select(kvp => Country.Create(kvp.Value, kvp.Key))
            .Where(r => r.IsSuccess)
            .Select(r => r.Value)
            .ToList();

        // Assert: Only the valid entry is included; invalid entry is silently skipped.
        countries.Count.ShouldBe(1);
        countries[0].IsoCode.ShouldBe("US");
        countries[0].Name.ShouldBe("United States");
    }
}

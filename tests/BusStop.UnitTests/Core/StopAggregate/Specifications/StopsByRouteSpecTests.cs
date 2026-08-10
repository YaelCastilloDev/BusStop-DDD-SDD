using BusStop.Core.StopAggregate;
using BusStop.Core.StopAggregate.Specifications;

namespace BusStop.UnitTests.Core.StopAggregate.Specifications;

public class StopsByRouteSpecTests
{
    [Fact]
    public void Does_Not_Filter_Deleted_Stops_InDatabase_Query()
    {
        var spec = new StopsByRouteSpec(1);

        spec.WhereExpressions.Count().ShouldBe(1);
    }
}

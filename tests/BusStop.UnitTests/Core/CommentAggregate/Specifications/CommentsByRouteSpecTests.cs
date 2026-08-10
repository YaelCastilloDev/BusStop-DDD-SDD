using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;

namespace BusStop.UnitTests.Core.CommentAggregate.Specifications;

public class CommentsByRouteSpecTests
{
    [Fact]
    public void Does_Not_Filter_Moderated_Comments_InDatabase_Query()
    {
        var spec = new CommentsByRouteSpec(1);

        spec.WhereExpressions.Count().ShouldBe(1);
    }
}

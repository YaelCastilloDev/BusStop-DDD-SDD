using BusStop.Core.Interfaces;
using BusStop.UseCases.Stops.Create;

namespace BusStop.UnitTests.UseCases.Stops.Create;

public class CreateStopHandlerTests
{
    [Fact]
    public void CreateStopCommand_Implements_IRequireAuthenticatedUser()
    {
        var commandType = typeof(CreateStopCommand);

        commandType.GetInterfaces().ShouldContain(i => i == typeof(IRequireAuthenticatedUser));
    }
}

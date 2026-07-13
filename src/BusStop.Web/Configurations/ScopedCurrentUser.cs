using BusStop.Core.Interfaces;

namespace BusStop.Web.Configurations;

public sealed class ScopedCurrentUser : ICurrentUser
{
    public long Id { get; internal set; }
}

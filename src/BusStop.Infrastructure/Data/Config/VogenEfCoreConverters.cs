using BusStop.Core.ContributorAggregate;
using Vogen;

namespace BusStop.Infrastructure.Data.Config;

[EfCoreConverter<ContributorId>]
[EfCoreConverter<ContributorName>]
internal partial class VogenEfCoreConverters;

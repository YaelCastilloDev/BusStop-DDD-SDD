using BusStop.UseCases.Countries;
using BusStop.UseCases.Countries.List;
using BusStop.Web.Extensions;

namespace BusStop.Web.Countries;

public sealed class List(IMediator mediator) : Endpoint<EmptyRequest, IEnumerable<CountryResponse>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get("/countries");
    AllowAnonymous();
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
  {
    var query = new ListCountriesQuery();
    var result = await _mediator.Send(query, ct);

    await this.ToOkResultAsync(result, ct);
  }
}

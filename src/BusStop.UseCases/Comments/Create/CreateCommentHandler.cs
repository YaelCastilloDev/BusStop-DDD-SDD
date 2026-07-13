using BusStop.Core.CommentAggregate;
using BusStop.Core.Interfaces;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;

namespace BusStop.UseCases.Comments.Create;

public sealed class CreateCommentHandler(
  IRepository<Comment> repository,
  ICurrentUser currentUser,
  IReadRepository<Route> routeRepository) : ICommandHandler<CreateCommentCommand, Result<CommentResponse>>
{
  public async ValueTask<Result<CommentResponse>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
  {
    if (currentUser.Id <= 0)
      return Result<CommentResponse>.NotFound("User not found.");

    var routeResult = await routeRepository.FindRequiredAsync(new RouteByIdSpec(new RouteId(request.RouteId)), "Route not found.", cancellationToken);
    if (!routeResult.IsSuccess)
      return Result<CommentResponse>.NotFound("Route not found.");
    var route = routeResult.Value;

    return await repository.CreateAsync(Comment.Create(request.Content, currentUser.Id, request.RouteId), c => c.ToResponse(), cancellationToken);
  }
}

using BusStop.Core.CommentAggregate;
using BusStop.Core.Interfaces;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;
using BusStop.Core.UserAggregate;
using BusStop.UseCases.Users;

namespace BusStop.UseCases.Comments.Create;

public sealed class CreateCommentHandler(
  IRepository<Comment> repository,
  IReadRepository<User> userRepository,
  IReadRepository<Route> routeRepository) : ICommandHandler<CreateCommentCommand, Result<CommentResponse>>
{
  public async ValueTask<Result<CommentResponse>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
  {
    var userResult = await userRepository.GetUserByExternalIdAsync(request.Sub, cancellationToken);
    if (!userResult.IsSuccess)
      return Result<CommentResponse>.NotFound("User not found.");
    var user = userResult.Value;

    var routeResult = await routeRepository.FindRequiredAsync(new RouteByIdSpec(new RouteId(request.RouteId)), "Route not found.", cancellationToken);
    if (!routeResult.IsSuccess)
      return Result<CommentResponse>.NotFound("Route not found.");
    var route = routeResult.Value;

    return await repository.CreateAsync(Comment.Create(request.Content, user.Id, request.RouteId), c => c.ToResponse(), cancellationToken);
  }
}

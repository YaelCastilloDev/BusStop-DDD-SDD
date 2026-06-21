namespace BusStop.UseCases.Comments.GetByRoute;

public sealed record GetCommentsByRouteQuery(long RouteId) : IQuery<Result<List<CommentResponse>>>;

namespace BusStop.Core.CommentAggregate.Specifications;

public sealed class CommentByIdSpec : Specification<Comment>
{
  public CommentByIdSpec(CommentId commentId) =>
    Query.Where(c => c.Id == commentId.Value);
}

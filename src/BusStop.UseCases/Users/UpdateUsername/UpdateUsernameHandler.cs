using BusStop.Core.Errors;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Users.UpdateUsername;

public sealed class UpdateUsernameHandler(
    IRepository<User> repository,
    ICurrentUser currentUser) : ICommandHandler<UpdateUsernameCommand, Result<UserResponse>>
{
    public async ValueTask<Result<UserResponse>> Handle(UpdateUsernameCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Id <= 0)
            return Result<UserResponse>.NotFound("User not found.");

        var userResult = await repository.FindRequiredAsync(
            new UserByIdSpec(new UserId(currentUser.Id)),
            "User not found.",
            cancellationToken);

        if (!userResult.IsSuccess)
            return Result<UserResponse>.NotFound("User not found.");

        var user = userResult.Value;

        var existingUser = await repository.FirstOrDefaultAsync(
            new UserByUsernameSpec(request.NewUsername),
            cancellationToken);

        if (existingUser is not null && existingUser.Id != user.Id)
            return Result<UserResponse>.Error(new ErrorList([UserErrors.UsernameAlreadyTaken]));

        var updateResult = user.UpdateUsername(new Username(request.NewUsername));
        if (!updateResult.IsSuccess)
            return Result<UserResponse>.Error(new ErrorList(updateResult.Errors));

        await repository.UpdateAsync(user, cancellationToken);

        return user.ToResponse();
    }
}

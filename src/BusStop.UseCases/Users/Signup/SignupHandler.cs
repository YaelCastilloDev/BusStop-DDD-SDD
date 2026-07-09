using Ardalis.Result;
using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Users.Signup;

public sealed class SignupHandler(IKeycloakAdminService keycloakAdmin) : ICommandHandler<SignupCommand, Result>
{
    public async ValueTask<Result> Handle(SignupCommand request, CancellationToken ct)
    {
        return await keycloakAdmin.CreateUserAsync(request.Email, request.Password, ct);
    }
}

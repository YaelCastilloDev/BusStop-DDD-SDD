using Ardalis.Result;
using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Users.Signup;

public sealed class SignupHandler(IKeycloakAdminService keycloakAdmin) : ICommandHandler<SignupCommand, Result>
{
    public async ValueTask<Result> Handle(SignupCommand request, CancellationToken ct)
    {
        var errors = new List<string>();

        if (!IsValidEmail(request.Email))
            errors.Add("Invalid email format.");

        if (request.Password.Length < 8)
            errors.Add("Password must be at least 8 characters.");

        if (errors.Count > 0)
            return Result.Error(new ErrorList(errors));

        return await keycloakAdmin.CreateUserAsync(request.Email, request.Password, ct);
    }

    private static bool IsValidEmail(string email)
    {
        return email.Contains('@') && email.Contains('.');
    }
}

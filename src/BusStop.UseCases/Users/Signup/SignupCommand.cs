using Ardalis.Result;
using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Users.Signup;

public sealed record SignupCommand(string Email, string Password) : ICommand<Result>;

using System.Reflection;
using Ardalis.SharedKernel;
using Ardalis.Specification;
using BusStop.Core.Interfaces;
using BusStop.Core.StopAggregate;
using BusStop.Infrastructure.Data;
using BusStop.UseCases.Users.Signup;
using BusStop.Web.Configurations;
using Mediator;
using NetArchTest.Rules;

namespace BusStop.UnitTests.Architecture;

public class ArchitectureTests
{
    private static readonly Assembly CoreAssembly = typeof(Stop).Assembly;
    private static readonly Assembly UseCasesAssembly = typeof(SignupCommand).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(AppDbContext).Assembly;
    private static readonly Assembly WebAssembly = Assembly.Load("BusStop.Web");

    // ── Layer Dependencies ────────────────────────────────────────

    [Fact]
    public void Core_ShouldNot_DependOn_Infrastructure()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("BusStop.Infrastructure")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Core_ShouldNot_DependOn_UseCases()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("BusStop.UseCases")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Core_ShouldNot_DependOn_EntityFramework()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Core_ShouldNot_DependOn_AspNetCore()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void UseCases_ShouldNot_DependOn_Infrastructure()
    {
        var result = Types.InAssembly(UseCasesAssembly)
            .ShouldNot()
            .HaveDependencyOn("BusStop.Infrastructure")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Infrastructure_ShouldNot_DependOn_Web()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("BusStop.Web")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ── Aggregate Folder Structure ────────────────────────────────

    [Fact]
    public void DomainEvents_ShouldResideIn_EventsNamespace()
    {
        var result = Types.InAssembly(CoreAssembly)
            .That()
            .Inherit(typeof(DomainEventBase))
            .Should()
            .ResideInNamespaceEndingWith(".Events")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void DomainEventHandlers_ShouldResideIn_HandlersNamespace()
    {
        var result = Types.InAssembly(CoreAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .ResideInNamespace("BusStop.Core")
            .Should()
            .ResideInNamespaceEndingWith(".Handlers")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Specifications_ShouldResideIn_SpecificationsNamespace()
    {
        var result = Types.InAssembly(CoreAssembly)
            .That()
            .Inherit(typeof(Specification<>))
            .Should()
            .ResideInNamespaceEndingWith(".Specifications")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ── Inheritance ───────────────────────────────────────────────

    [Fact]
    public void AggregateRoots_ShouldInheritFrom_EntityBase()
    {
        var result = Types.InAssembly(CoreAssembly)
            .That()
            .ImplementInterface(typeof(IAggregateRoot))
            .Should()
            .Inherit(typeof(EntityBase<>))
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void ValueObjects_ShouldInheritFrom_ValueObject()
    {
        var result = Types.InAssembly(CoreAssembly)
            .That()
            .ResideInNamespace("BusStop.Core")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .And()
            .DoNotInherit(typeof(DomainEventBase))
            .And()
            .DoNotInherit(typeof(Specification<>))
            .And()
            .DoNotImplementInterface(typeof(IAggregateRoot))
            .And()
            .DoNotHaveNameEndingWith("Errors")
            .And()
            .DoNotHaveNameEndingWith("Exception")
            .And()
            .DoNotHaveNameEndingWith("Handler")
            .And()
            .HaveNameEndingWith("Id")
            .Or()
            .HaveNameEndingWith("Name")
            .Or()
            .HaveNameEndingWith("Content")
            .Or()
            .HaveNameEndingWith("Location")
            .Or()
            .HaveNameEndingWith("Reaction")
            .Should()
            .Inherit(typeof(ValueObject))
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Entities_WithIdSuffix_ShouldInheritFromValueObject()
    {
        var result = Types.InAssembly(CoreAssembly)
            .That()
            .HaveNameEndingWith("Id")
            .And()
            .ResideInNamespace("BusStop.Core")
            .Should()
            .Inherit(typeof(ValueObject))
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ── Naming ────────────────────────────────────────────────────

    [Fact]
    public void DomainEvents_ShouldHaveName_EndingWithEvent()
    {
        var result = Types.InAssembly(CoreAssembly)
            .That()
            .Inherit(typeof(DomainEventBase))
            .Should()
            .HaveNameEndingWith("Event")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Specifications_ShouldHaveName_EndingWithSpec()
    {
        var result = Types.InAssembly(CoreAssembly)
            .That()
            .Inherit(typeof(Specification<>))
            .Should()
            .HaveNameEndingWith("Spec")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ── Access Modifiers ──────────────────────────────────────────

    [Fact]
    public void DomainEvents_ShouldBe_Sealed()
    {
        var result = Types.InAssembly(CoreAssembly)
            .That()
            .Inherit(typeof(DomainEventBase))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void ValueObjects_ShouldBe_Sealed()
    {
        var result = Types.InAssembly(CoreAssembly)
            .That()
            .Inherit(typeof(ValueObject))
            .And()
            .ResideInNamespace("BusStop.Core")
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ── Forbidden References ──────────────────────────────────────

    [Fact]
    public void Core_ShouldNot_Reference_MediatR()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("MediatR")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void UseCases_ShouldNot_Reference_MediatR()
    {
        var result = Types.InAssembly(UseCasesAssembly)
            .ShouldNot()
            .HaveDependencyOn("MediatR")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ── Pattern Compliance ──────────────────────────────────────────

    [Fact]
    public void AllAggregateRoots_MustFollow_CreatePattern()
    {
        var aggTypes = CoreAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IAggregateRoot).IsAssignableFrom(t))
            .ToList();

        var violations = new List<string>();

        foreach (var type in aggTypes)
        {
            var hasPrivateCtor = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .Any(c => c.GetParameters().Length == 0 && c.IsPrivate);
            if (!hasPrivateCtor)
                violations.Add($"{type.Name}: missing private parameterless constructor");

            var createMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .FirstOrDefault(m => m.Name == "Create" &&
                    m.ReturnType.IsGenericType &&
                    m.ReturnType.GetGenericTypeDefinition().Name == "Result`1");
            if (createMethod is null)
                violations.Add($"{type.Name}: missing public static Create method returning Result<T>");

            var voidMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.ReturnType == typeof(void) && !m.IsSpecialName)
                .Select(m => m.Name)
                .ToList();
            foreach (var m in voidMethods)
                violations.Add($"{type.Name}.{m}(): returns void instead of Result");
        }

        violations.ShouldBeEmpty(string.Join("\n", violations));
    }

    [Fact]
    public void AllEndpoints_MustHave_Validator()
    {
        var violations = new List<string>();

        Type[] types;
        try
        {
            types = WebAssembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t is not null).ToArray()!;
        }

        foreach (var type in types.Where(t => t is { IsAbstract: false, IsClass: true }))
        {
            if (type.BaseType is not { IsGenericType: true })
                continue;

            var baseName = type.BaseType.GetGenericTypeDefinition().Name;

            if (baseName == "EndpointWithoutRequest`1")
                continue;

            if (baseName is "Endpoint`1" or "Endpoint`2")
            {
                var requestType = type.BaseType.GetGenericArguments()[0];

                if (requestType.Name == "EmptyRequest")
                    continue;

                var hasValidator = types
                    .Any(t => t.BaseType is { IsGenericType: true } bt &&
                        bt.GetGenericTypeDefinition().Name == "Validator`1" &&
                        bt.GetGenericArguments()[0] == requestType);

                if (!hasValidator)
                    violations.Add($"{type.Name}: missing Validator<{requestType.Name}>");
            }
        }

        violations.ShouldBeEmpty(string.Join("\n", violations));
    }

    // ── Resilience Patterns ───────────────────────────────────────

    [Fact]
    public void AllQueryRequestTypes_ShouldImplement_IIdempotentRequest()
    {
        var violations = new List<string>();

        var queryTypes = Types.InAssembly(UseCasesAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        foreach (var type in queryTypes)
        {
            if (!typeof(IIdempotentRequest).IsAssignableFrom(type))
                violations.Add($"{type.Name}: implements IQuery<> but not IIdempotentRequest");
        }

        violations.ShouldBeEmpty(string.Join("\n", violations));
    }

    [Fact]
    public void NoCommandRequestTypes_ShouldImplement_IIdempotentRequest()
    {
        var violations = new List<string>();

        var commandTypes = Types.InAssembly(UseCasesAssembly)
            .That()
            .ImplementInterface(typeof(ICommand<>))
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        foreach (var type in commandTypes)
        {
            if (typeof(IIdempotentRequest).IsAssignableFrom(type))
                violations.Add($"{type.Name}: implements ICommand<> but also implements IIdempotentRequest (should not)");
        }

        violations.ShouldBeEmpty(string.Join("\n", violations));
    }

    [Fact]
    public void ResilienceBehavior_ShouldExist_InWebAssembly()
    {
        var result = Types.InAssembly(WebAssembly)
            .That()
            .HaveName("ResilienceBehavior`2")
            .Should()
            .ImplementInterface(typeof(Mediator.IPipelineBehavior<,>))
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void MediatorConfig_Registers_ResilienceBehavior_WithoutErrors()
    {
        // Verify that MediatorConfig.AddMediatorSourceGen executes successfully,
        // confirming that ResilienceBehavior (listed in its PipelineBehaviors array)
        // is a valid type that can be registered in the DI container.
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger>();

        var exception = Record.Exception(() =>
            MediatorConfig.AddMediatorSourceGen(services, logger));

        exception.ShouldBeNull(
            "MediatorConfig.AddMediatorSourceGen must register ResilienceBehavior without errors");
    }

    [Fact]
    public void Core_ShouldNot_DependOn_Polly()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("Polly.Core")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Core_ShouldNot_DependOn_Web()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("BusStop.Web")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void ResilienceBehavior_ShouldDependOn_Polly()
    {
        var result = Types.InAssembly(WebAssembly)
            .That()
            .HaveName("ResilienceBehavior`2")
            .Should()
            .HaveDependencyOn("Polly")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void IIdempotentRequest_ShouldResideIn_CoreAssembly()
    {
        var type = typeof(IIdempotentRequest);
        type.Assembly.ShouldBe(CoreAssembly,
            "IIdempotentRequest must be defined in BusStop.Core assembly");
    }
}

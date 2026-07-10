using System.Reflection;
using Ardalis.SharedKernel;
using Ardalis.Specification;
using BusStop.Core.StopAggregate;
using BusStop.Infrastructure.Data;
using BusStop.UseCases.Users.Signup;
using NetArchTest.Rules;

namespace BusStop.UnitTests.Architecture;

public class ArchitectureTests
{
    private static readonly Assembly CoreAssembly = typeof(Stop).Assembly;
    private static readonly Assembly UseCasesAssembly = typeof(SignupCommand).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(AppDbContext).Assembly;

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
}

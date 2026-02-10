using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.Behaviors;
using Xunit;

namespace MyPhotoBooth.UnitTests.Common.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Mock<ILogger<ValidationBehavior<TestRequest, Result>>> _loggerMock;
    private readonly ValidationBehavior<TestRequest, Result> _behavior;
    private readonly Mock<IValidator<TestRequest>> _validatorMock;

    public ValidationBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<ValidationBehavior<TestRequest, Result>>>();
        _validatorMock = new Mock<IValidator<TestRequest>>();
        var validators = new List<IValidator<TestRequest>> { _validatorMock.Object };
        _behavior = new ValidationBehavior<TestRequest, Result>(validators, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_NoValidators_ReturnsNextResult()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result>(
            Enumerable.Empty<IValidator<TestRequest>>(),
            _loggerMock.Object
        );

        var request = new TestRequest();
        var expectedResult = Result.Success();

        Task<Result> Next() => Task.FromResult(expectedResult);
        var nextMock = new Mock<RequestHandlerDelegate<Result>>();
        nextMock.Setup(x => x()).Returns(Next);

        // Act
        var result = await behavior.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsNextResult()
    {
        // Arrange
        var request = new TestRequest();
        var expectedResult = Result.Success();

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Task<Result> Next() => Task.FromResult(expectedResult);
        var nextMock = new Mock<RequestHandlerDelegate<Result>>();
        nextMock.Setup(x => x()).Returns(Next);

        // Act
        var result = await _behavior.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ReturnsFailureResult()
    {
        // Arrange
        var request = new TestRequest();
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Property1", "Error 1"),
            new ValidationFailure("Property2", "Error 2")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        Task<Result> Next() => Task.FromResult(Result.Success());
        var nextMock = new Mock<RequestHandlerDelegate<Result>>();
        nextMock.Setup(x => x()).Returns(Next);

        // Act
        var result = await _behavior.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Property1");
        result.Error.Should().Contain("Error 1");
        nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_MultipleValidatorsAllValid_ReturnsNextResult()
    {
        // Arrange
        var validator1Mock = new Mock<IValidator<TestRequest>>();
        var validator2Mock = new Mock<IValidator<TestRequest>>();

        validator1Mock
            .Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        validator2Mock
            .Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, Result>(
            new List<IValidator<TestRequest>> { validator1Mock.Object, validator2Mock.Object },
            _loggerMock.Object
        );

        var request = new TestRequest();
        var expectedResult = Result.Success();

        Task<Result> Next() => Task.FromResult(expectedResult);
        var nextMock = new Mock<RequestHandlerDelegate<Result>>();
        nextMock.Setup(x => x()).Returns(Next);

        // Act
        var result = await behavior.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        nextMock.Verify(x => x(), Times.Once);
    }

    public class TestRequest : IRequest<Result>
    {
    }
}

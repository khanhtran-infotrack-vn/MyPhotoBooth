using MediatR;
using CSharpFunctionalExtensions;

namespace MyPhotoBooth.Application.Common.Requests;

public interface IRequest<TResult> : MediatR.IRequest<Result<TResult>>
{
}

public interface ICommand<TResult> : IRequest<TResult>
{
}

public interface IQuery<TResult> : IRequest<TResult>
{
}

public interface ICommand : MediatR.IRequest<Result>
{
}

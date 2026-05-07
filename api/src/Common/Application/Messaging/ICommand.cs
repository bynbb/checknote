namespace Checknote.Common.Application.Messaging;

using Checknote.Common.Domain;
using MediatR;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

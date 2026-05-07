namespace Checknote.Common.Application.Messaging;

using Checknote.Common.Domain;
using MediatR;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

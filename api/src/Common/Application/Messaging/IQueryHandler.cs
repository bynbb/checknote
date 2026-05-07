namespace Checknote.Common.Application.Messaging;

using Checknote.Common.Domain;
using MediatR;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}

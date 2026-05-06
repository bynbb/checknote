namespace Checknote.Common.Application.Messaging;

public interface IQueryHandler<in TQuery, out TResponse>
{
    TResponse Handle(TQuery query);
}

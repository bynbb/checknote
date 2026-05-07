namespace Checknote.Modules.Todos.Domain.Todos;

using Checknote.Common.Domain;

public static class TodoErrors
{
    public static readonly Error ReservedTitle = Error.Validation(
        "Todos.ReservedTitle",
        "The todo title 88888 is reserved for validating domain error handling.");
}

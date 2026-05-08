namespace Checknote.Modules.Todos.Application.Todos.SaveTaskList;

using FluentValidation;

internal sealed class SaveTaskListCommandValidator : AbstractValidator<SaveTaskListCommand>
{
    public SaveTaskListCommandValidator()
    {
        RuleFor(command => command.Todos)
            .NotNull()
            .WithErrorCode("Todos.TaskListRequired")
            .WithMessage("The task list is required.");

        RuleForEach(command => command.Todos!)
            .SetValidator(new SaveTaskListTodoValidator());
    }
}

internal sealed class SaveTaskListTodoValidator : AbstractValidator<SaveTaskListTodo>
{
    public SaveTaskListTodoValidator()
    {
        RuleFor(todo => todo.Title)
            .NotEmpty()
            .WithErrorCode("Todos.TitleRequired")
            .WithMessage("The todo title is required.")
            .MaximumLength(200)
            .WithErrorCode("Todos.TitleTooLong")
            .WithMessage("The todo title cannot exceed 200 characters.");
    }
}

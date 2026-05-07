namespace Checknote.Api.UnitTests.Modules.Todos.Application;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Checknote.Api.UnitTests.Support;
using Checknote.Common.Application;
using Checknote.Common.Domain;
using Checknote.Modules.Todos.Application.Abstractions;
using Checknote.Modules.Todos.Application.Todos.GetTodos;
using Checknote.Modules.Todos.Application.Todos.SaveTaskList;
using Checknote.Modules.Todos.Domain.Todos;
using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Application.Users.GetCurrentUser;
using Checknote.Modules.Users.Domain.Users;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

internal static class MediatRDispatchTests
{
    public static async Task Run()
    {
        Todo first = new(1, "Plan the slice", false);
        Todo second = new(2, "Ship the slice", true);
        FakeTodoRepository todoRepository = new([first, second]);
        User currentUser = new("user-1", "Ada Lovelace", "ada@example.test");

        ServiceCollection services = new();
        services.AddSingleton<ITodoRepository>(todoRepository);
        services.AddSingleton<IUserRepository>(new FakeUserRepository(currentUser));
        services.AddApplication(typeof(GetTodosQueryHandler).Assembly);

        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        ISender sender = provider.GetRequiredService<ISender>();

        Result<IReadOnlyCollection<Todo>> todosResult = await sender.Send(new GetTodosQuery());
        TestAssert.True(todosResult.IsSuccess, "GetTodosQuery should succeed.");
        TestAssert.Equal(2, todosResult.Value.Count, "GetTodosQuery todo count");
        TestAssert.Same(first, todosResult.Value.First(), "GetTodosQuery should return repository todos.");

        Todo replacement = new(3, "Replace through CQRS", false);
        Result saveResult = await sender.Send(new SaveTaskListCommand([replacement]));
        TestAssert.True(saveResult.IsSuccess, "SaveTaskListCommand should succeed.");
        TestAssert.Equal(1, todoRepository.SavedTodos.Count, "SaveTaskListCommand saved todo count");
        TestAssert.Same(replacement, todoRepository.SavedTodos.Single(), "SaveTaskListCommand should save command todos.");

        Result<User> userResult = await sender.Send(new GetCurrentUserQuery());
        TestAssert.True(userResult.IsSuccess, "GetCurrentUserQuery should succeed.");
        TestAssert.Same(currentUser, userResult.Value, "GetCurrentUserQuery should return repository user.");
    }

    private sealed class FakeTodoRepository : ITodoRepository
    {
        private readonly IReadOnlyCollection<Todo> todos;

        public FakeTodoRepository(IReadOnlyCollection<Todo> todos)
        {
            this.todos = todos;
            SavedTodos = [];
        }

        public IReadOnlyCollection<Todo> SavedTodos { get; private set; }

        public IReadOnlyCollection<Todo> GetTodos() => todos;

        public void SaveTodos(IReadOnlyCollection<Todo> todosToSave)
        {
            SavedTodos = todosToSave;
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User currentUser;

        public FakeUserRepository(User currentUser)
        {
            this.currentUser = currentUser;
        }

        public User GetCurrentUser() => currentUser;
    }
}

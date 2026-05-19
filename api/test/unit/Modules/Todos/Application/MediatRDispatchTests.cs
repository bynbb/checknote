namespace Checknote.Api.UnitTests.Modules.Todos.Application;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Checknote.Api.UnitTests.Support;
using Checknote.Common.Application;
using Checknote.Common.Domain;
using Checknote.Modules.Todos.Application.Abstractions;
using Checknote.Modules.Todos.Application.Todos.GetTodos;
using Checknote.Modules.Todos.Application.Todos.SaveTaskList;
using Checknote.Modules.Todos.Application.Todos.SearchTodos;
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
        User currentUser = User.Create(
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            "keycloak-subject-1",
            "Ada Lovelace",
            "ada@example.test");
        AuthenticatedUser authenticatedUser = new(
            currentUser.IdentityId,
            currentUser.Name,
            currentUser.Email);
        FakeUserRepository userRepository = new(currentUser);

        ServiceCollection services = new();
        services.AddSingleton<ITodoRepository>(todoRepository);
        services.AddSingleton<ICurrentUserProvider>(new FakeCurrentUserProvider(authenticatedUser));
        services.AddSingleton<IUserRepository>(userRepository);
        services.AddApplication(
            Checknote.Modules.Todos.Application.AssemblyReference.Assembly,
            Checknote.Modules.Users.Application.AssemblyReference.Assembly);

        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        ISender sender = provider.GetRequiredService<ISender>();

        Result<IReadOnlyCollection<Todo>> todosResult = await sender.Send(new GetTodosQuery());
        TestAssert.True(todosResult.IsSuccess, "GetTodosQuery should succeed.");
        TestAssert.Equal(2, todosResult.Value.Count, "GetTodosQuery todo count");
        TestAssert.Same(first, todosResult.Value.First(), "GetTodosQuery should return repository todos.");

        Result<IReadOnlyCollection<Todo>> matchingSearchResult = await sender.Send(new SearchTodosQuery("ship"));
        TestAssert.True(matchingSearchResult.IsSuccess, "SearchTodosQuery should succeed.");
        TestAssert.Equal(1, matchingSearchResult.Value.Count, "SearchTodosQuery matching todo count");
        TestAssert.Same(second, matchingSearchResult.Value.Single(), "SearchTodosQuery should return matching todo.");

        Result<IReadOnlyCollection<Todo>> nonMatchingSearchResult = await sender.Send(new SearchTodosQuery("missing"));
        TestAssert.True(nonMatchingSearchResult.IsSuccess, "SearchTodosQuery non-matching search should succeed.");
        TestAssert.Equal(0, nonMatchingSearchResult.Value.Count, "SearchTodosQuery non-matching todo count");

        Result<IReadOnlyCollection<Todo>> emptySearchResult = await sender.Send(new SearchTodosQuery("   "));
        TestAssert.True(emptySearchResult.IsSuccess, "SearchTodosQuery empty search should succeed.");
        TestAssert.Equal(0, emptySearchResult.Value.Count, "SearchTodosQuery empty search todo count");

        Result saveResult = await sender.Send(new SaveTaskListCommand([
            new SaveTaskListTodo(3, "Replace through CQRS", false),
        ]));
        TestAssert.True(saveResult.IsSuccess, "SaveTaskListCommand should succeed.");
        TestAssert.Equal(1, todoRepository.SavedTodos.Count, "SaveTaskListCommand saved todo count");
        TestAssert.Equal(1, todoRepository.SaveCallCount, "SaveTaskListCommand should call the repository.");
        Todo savedTodo = todoRepository.SavedTodos.Single();
        TestAssert.Equal(3, savedTodo.Id, "SaveTaskListCommand saved todo id");
        TestAssert.Equal("Replace through CQRS", savedTodo.Title, "SaveTaskListCommand saved todo title");
        TestAssert.Equal(false, savedTodo.IsCompleted, "SaveTaskListCommand saved todo completion state");
        TestAssert.True(savedTodo.Embedding.HasValue, "SaveTaskListCommand should populate a todo vector.");
        Microsoft.Data.SqlTypes.SqlVector<float> savedVector = savedTodo.Embedding.GetValueOrDefault();
        TestAssert.Equal(3, savedVector.Length, "SaveTaskListCommand todo vector length");

        Todo changedTitleTodo = Todo.Create(3, "Replace through CQRS with new text", false).Value;
        TestAssert.True(changedTitleTodo.Embedding.HasValue, "Changed title todo should populate a vector.");
        Microsoft.Data.SqlTypes.SqlVector<float> changedVector = changedTitleTodo.Embedding.GetValueOrDefault();
        TestAssert.True(
            !savedVector.Memory.Span.SequenceEqual(changedVector.Memory.Span),
            "Changing todo text should change the todo vector.");

        Result invalidTitleResult = await sender.Send(new SaveTaskListCommand([
            new SaveTaskListTodo(4, "   ", false),
        ]));
        TestAssert.True(invalidTitleResult.IsFailure, "SaveTaskListCommand should reject invalid command input.");
        TestAssert.Equal("General.Validation", invalidTitleResult.Error.Code, "Invalid command error code");
        TestAssert.True(invalidTitleResult.Error is ValidationError, "Invalid command should return a validation error.");
        TestAssert.Equal(1, todoRepository.SaveCallCount, "Invalid command input should not call the repository.");

        Result reservedTitleResult = await sender.Send(new SaveTaskListCommand([
            new SaveTaskListTodo(5, "88888", false),
        ]));
        TestAssert.True(reservedTitleResult.IsFailure, "SaveTaskListCommand should reject invalid domain input.");
        TestAssert.Equal("General.Validation", reservedTitleResult.Error.Code, "Invalid domain error code");
        TestAssert.True(reservedTitleResult.Error is ValidationError, "Invalid domain input should return a validation error.");
        TestAssert.Equal(1, todoRepository.SaveCallCount, "Invalid domain input should not call the repository.");

        Result nullTaskListResult = await sender.Send(new SaveTaskListCommand(null));
        TestAssert.True(nullTaskListResult.IsFailure, "SaveTaskListCommand should reject a null task list.");
        TestAssert.Equal(1, todoRepository.SaveCallCount, "Null task list should not call the repository.");

        Result<User> userResult = await sender.Send(new GetCurrentUserQuery());
        TestAssert.True(userResult.IsSuccess, "GetCurrentUserQuery should succeed.");
        TestAssert.Same(currentUser, userResult.Value, "GetCurrentUserQuery should return current user.");
    }

    private sealed class FakeTodoRepository : ITodoRepository
    {
        private readonly IReadOnlyCollection<Todo> todos;

        public FakeTodoRepository(IReadOnlyCollection<Todo> todos)
        {
            this.todos = todos;
            SavedTodos = [];
        }

        public int SaveCallCount { get; private set; }

        public IReadOnlyCollection<Todo> SavedTodos { get; private set; }

        public IReadOnlyCollection<Todo> GetTodos() => todos;

        public IReadOnlyCollection<Todo> SearchTodos(string searchText)
        {
            return todos
                .Where(todo => todo.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        public void SaveTodos(IReadOnlyCollection<Todo> todosToSave)
        {
            SaveCallCount++;
            SavedTodos = todosToSave;
        }
    }

    private sealed class FakeCurrentUserProvider : ICurrentUserProvider
    {
        private readonly AuthenticatedUser currentUser;

        public FakeCurrentUserProvider(AuthenticatedUser currentUser)
        {
            this.currentUser = currentUser;
        }

        public Result<AuthenticatedUser> GetCurrentUser() => Result.Success(currentUser);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User currentUser;

        public FakeUserRepository(User currentUser)
        {
            this.currentUser = currentUser;
        }

        public Task<User?> GetByIdentityIdAsync(
            string identityId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(currentUser.IdentityId == identityId ? currentUser : null);
        }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(currentUser.Email == email ? currentUser : null);
        }

        public void Insert(User user)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

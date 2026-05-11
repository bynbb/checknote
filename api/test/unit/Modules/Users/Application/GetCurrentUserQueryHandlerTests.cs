namespace Checknote.Api.UnitTests.Modules.Users.Application;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Checknote.Api.UnitTests.Support;
using Checknote.Common.Domain;
using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Application.Users.GetCurrentUser;
using Checknote.Modules.Users.Domain.Users;

internal static class GetCurrentUserQueryHandlerTests
{
    public static async Task Run()
    {
        await CreatesLocalUserProjectionFromAuthenticatedIdentity();
        await UpdatesExistingUserProjectionFromAuthenticatedIdentity();
        await RejectsEmailAssignedToAnotherIdentity();
        await StopsWhenAuthenticationContextIsInvalid();
    }

    private static async Task CreatesLocalUserProjectionFromAuthenticatedIdentity()
    {
        AuthenticatedUser authenticatedUser = new(
            "keycloak-subject-1",
            "checknote-handle",
            "user@example.test");
        FakeCurrentUserProvider currentUserProvider = new(Result.Success(authenticatedUser));
        FakeUserRepository userRepository = new();

        Result<User> result = await new GetCurrentUserQueryHandler(
            currentUserProvider,
            userRepository).Handle(new GetCurrentUserQuery(), CancellationToken.None);

        TestAssert.True(result.IsSuccess, "Current-user query should create a local user projection.");
        TestAssert.Equal(
            authenticatedUser.IdentityId,
            result.Value.IdentityId,
            "Created user identity id");
        TestAssert.Equal(authenticatedUser.Name, result.Value.Name, "Created user name");
        TestAssert.Equal(authenticatedUser.Email, result.Value.Email, "Created user email");
        TestAssert.True(result.Value.Id != Guid.Empty, "Created user should have a Checknote user id.");
        TestAssert.Equal(1, userRepository.InsertCount, "Created user insert count");
        TestAssert.Equal(1, userRepository.SaveChangesCount, "Created user save count");
    }

    private static async Task UpdatesExistingUserProjectionFromAuthenticatedIdentity()
    {
        Guid userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        User existingUser = User.Create(
            userId,
            "keycloak-subject-1",
            "old-handle",
            "old@example.test");
        AuthenticatedUser authenticatedUser = new(
            "keycloak-subject-1",
            "new-handle",
            "new@example.test");
        FakeCurrentUserProvider currentUserProvider = new(Result.Success(authenticatedUser));
        FakeUserRepository userRepository = new(existingUser);

        Result<User> result = await new GetCurrentUserQueryHandler(
            currentUserProvider,
            userRepository).Handle(new GetCurrentUserQuery(), CancellationToken.None);

        TestAssert.True(result.IsSuccess, "Current-user query should update an existing local user.");
        TestAssert.Equal(userId, result.Value.Id, "Updated user keeps Checknote user id");
        TestAssert.Equal("new-handle", result.Value.Name, "Updated user name");
        TestAssert.Equal("new@example.test", result.Value.Email, "Updated user email");
        TestAssert.Equal(0, userRepository.InsertCount, "Existing user insert count");
        TestAssert.Equal(1, userRepository.SaveChangesCount, "Existing user save count");
    }

    private static async Task RejectsEmailAssignedToAnotherIdentity()
    {
        User existingUser = User.Create(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            "different-keycloak-subject",
            "existing-handle",
            "user@example.test");
        AuthenticatedUser authenticatedUser = new(
            "keycloak-subject-1",
            "checknote-handle",
            "user@example.test");
        FakeCurrentUserProvider currentUserProvider = new(Result.Success(authenticatedUser));
        FakeUserRepository userRepository = new(existingUser);

        Result<User> result = await new GetCurrentUserQueryHandler(
            currentUserProvider,
            userRepository).Handle(new GetCurrentUserQuery(), CancellationToken.None);

        TestAssert.True(result.IsFailure, "Duplicate current-user email should fail.");
        TestAssert.Equal(
            GetCurrentUserErrors.EmailAlreadyAssigned.Code,
            result.Error.Code,
            "Duplicate current-user email error code");
        TestAssert.Equal(0, userRepository.InsertCount, "Duplicate email insert count");
        TestAssert.Equal(0, userRepository.SaveChangesCount, "Duplicate email save count");
    }

    private static async Task StopsWhenAuthenticationContextIsInvalid()
    {
        FakeCurrentUserProvider currentUserProvider = new(
            Result.Failure<AuthenticatedUser>(GetCurrentUserErrors.MissingEmailClaim));
        FakeUserRepository userRepository = new();

        Result<User> result = await new GetCurrentUserQueryHandler(
            currentUserProvider,
            userRepository).Handle(new GetCurrentUserQuery(), CancellationToken.None);

        TestAssert.True(result.IsFailure, "Invalid auth context should fail.");
        TestAssert.Equal(0, userRepository.LookupCount, "Invalid auth context lookup count");
        TestAssert.Equal(0, userRepository.SaveChangesCount, "Invalid auth context save count");
    }

    private sealed class FakeCurrentUserProvider : ICurrentUserProvider
    {
        private readonly Result<AuthenticatedUser> result;

        public FakeCurrentUserProvider(Result<AuthenticatedUser> result)
        {
            this.result = result;
        }

        public Result<AuthenticatedUser> GetCurrentUser()
        {
            return result;
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<User> users;

        public FakeUserRepository(params User[] users)
        {
            this.users = users.ToList();
        }

        public int InsertCount { get; private set; }

        public int LookupCount { get; private set; }

        public int SaveChangesCount { get; private set; }

        public Task<User?> GetByIdentityIdAsync(
            string identityId,
            CancellationToken cancellationToken = default)
        {
            LookupCount++;
            return Task.FromResult(users.SingleOrDefault(user => user.IdentityId == identityId));
        }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            LookupCount++;
            return Task.FromResult(users.SingleOrDefault(
                user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)));
        }

        public void Insert(User user)
        {
            InsertCount++;
            users.Add(user);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }
    }
}

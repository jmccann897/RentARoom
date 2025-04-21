using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using RentARoom.Models;
using RentARoom.Services.IServices;
using RentARoom.Utility;
using System.Linq.Expressions;
using System.Security.Claims;

namespace RentARoom.Tests.RentARoom.UnitTests
{
    [Trait("Category", "Unit")]
    public class UserServiceTests
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserService _userService;

        private readonly ApplicationUser _agentUser;
        private readonly ApplicationUser _adminUser;
        private readonly ApplicationUser _standardUser;
        private readonly ApplicationUser _duplicateUser;
        private readonly ApplicationUser _testUser;
        private readonly string _nonExistentUserId = "non-existent-id";

        public UserServiceTests()
        {
            // Set up mock userStore for userManager
            var userStore = Substitute.For<IUserStore<ApplicationUser>>();
            // Set up mock userManager
            _userManager = Substitute.For<UserManager<ApplicationUser>>(userStore, null, null, null, null, null, null, null, null);

            // Set up mock userService
            _userService = new UserService(_userManager);

            // Set up mock users
            _agentUser = new ApplicationUser { Id = "1", UserName = "agentUser", Email = "agent@example.com" };
            _adminUser = new ApplicationUser { Id = "2", UserName = "adminUser", Email = "admin@example.com" };
            _standardUser = new ApplicationUser { Id = "3", UserName = "standardUser", Email = "standard@example.com" };
            _duplicateUser = new ApplicationUser { Id = "4", UserName = "duplicateUser", Email = "duplicate@example.com" };
            _testUser = new ApplicationUser { Id = "5", UserName = "testUser", Email = "test@example.com", PhoneNumber = "1234567890" };
        }

        [Fact]
        public async Task UserService_GetAdminAndAgentUsersAsync_Should_ReturnOnlyAdminAndAgents()
        {
            // Arrange
            _userManager.GetUsersInRoleAsync(SD.Role_Agent).Returns(new List<ApplicationUser> { _agentUser, _duplicateUser });
            _userManager.GetUsersInRoleAsync(SD.Role_Admin).Returns(new List<ApplicationUser> { _adminUser, _duplicateUser });

            // Act
            var result = await _userService.GetAdminAndAgentUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count); // Ensuring distinct users
            Assert.Contains(_agentUser, result);
            Assert.Contains(_adminUser, result);
            Assert.Contains(_duplicateUser, result);

            // Ensure GetUsersInRoleAsync was called exactly once per role
            await _userManager.Received(1).GetUsersInRoleAsync(SD.Role_Agent);
            await _userManager.Received(1).GetUsersInRoleAsync(SD.Role_Admin);

            // Ensure no unexpected calls to GetUsersInRoleAsync (e.g., for other roles)
            await _userManager.DidNotReceive().GetUsersInRoleAsync(Arg.Is<string>(role => role != SD.Role_Agent && role != SD.Role_Admin));
        }

        [Fact]
        public async Task UserService_GetAllUsers_Should_ReturnAllUsers()
        {
            // Arrange
            _userManager.GetUsersInRoleAsync(SD.Role_Agent).Returns(new List<ApplicationUser> { _agentUser, _duplicateUser });
            _userManager.GetUsersInRoleAsync(SD.Role_Admin).Returns(new List<ApplicationUser> { _adminUser, _duplicateUser });
            _userManager.GetUsersInRoleAsync(SD.Role_User).Returns(new List<ApplicationUser> { _standardUser, _duplicateUser });

            // Act
            var result = await _userService.GetAllUsers();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count); // Expecting distinct users only
            Assert.Contains(_agentUser, result);
            Assert.Contains(_adminUser, result);
            Assert.Contains(_standardUser, result);
            Assert.Contains(_duplicateUser, result); // Should be included only once

            // Verify GetUsersInRoleAsync was called exactly once per role
            await _userManager.Received(1).GetUsersInRoleAsync(SD.Role_Agent);
            await _userManager.Received(1).GetUsersInRoleAsync(SD.Role_Admin);
            await _userManager.Received(1).GetUsersInRoleAsync(SD.Role_User);

            // Ensure no unexpected role calls
            await _userManager.DidNotReceive().GetUsersInRoleAsync(Arg.Is<string>(role =>
                role != SD.Role_Agent && role != SD.Role_Admin && role != SD.Role_User));
        }

        [Fact]
        public async Task UserService_GetUserById_Should_ReturnUserDTO_WhenUserExists()
        {
            // Arrange
            _userManager.FindByIdAsync(_testUser.Id).Returns(_testUser);
            _userManager.GetRolesAsync(_testUser).Returns(new List<string> { SD.Role_User });

            // Act
            var result = await _userService.GetUserById(_testUser.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testUser.Id, result.Id);
            Assert.Equal(_testUser.Name, result.Name);
            Assert.Equal(_testUser.Email, result.Email);
            Assert.Equal(_testUser.PhoneNumber, result.PhoneNumber);
            Assert.Equal(SD.Role_User, result.Role);

            // Verify that FindByIdAsync and GetRolesAsync were called exactly once
            await _userManager.Received(1).FindByIdAsync(_testUser.Id);
            await _userManager.Received(1).GetRolesAsync(_testUser);
        }
        [Fact]
        public async Task UserService_GetUserById_Should_ReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            _userManager.FindByIdAsync(_nonExistentUserId).Returns((ApplicationUser)null);

            // Act
            var result = await _userService.GetUserById(_nonExistentUserId);

            // Assert
            Assert.Null(result);

            // Verify that FindByIdAsync was called once and GetRolesAsync was not called
            await _userManager.Received(1).FindByIdAsync(_nonExistentUserId);
            await _userManager.DidNotReceive().GetRolesAsync(Arg.Any<ApplicationUser>());
        }

        [Fact]
        public async Task UserService_GetUserById_Should_ReturnFirstRole_WhenMultipleRolesExist()
        {
            // Arrange
            _userManager.FindByIdAsync(_testUser.Id).Returns(_testUser);
            _userManager.GetRolesAsync(_testUser).Returns(new List<string> { SD.Role_Admin, SD.Role_Agent });

            // Act
            var result = await _userService.GetUserById(_testUser.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(SD.Role_Admin, result.Role); // Should return the first role

            // Verify that FindByIdAsync and GetRolesAsync were called once
            await _userManager.Received(1).FindByIdAsync(_testUser.Id);
            await _userManager.Received(1).GetRolesAsync(_testUser);
        }

        [Fact]
        public async Task UserService_IsUserAdmin_Should_ReturnTrue_WhenUserIsAdmin()
        {
            // Arrange
            _userManager.FindByIdAsync(_adminUser.Id).Returns(_adminUser);
            _userManager.IsInRoleAsync(_adminUser, SD.Role_Admin).Returns(true);

            // Act
            var result = await _userService.IsUserAdmin(_adminUser.Id);

            // Assert
            Assert.True(result);

            // Verify that FindByIdAsync and IsInRoleAsync called once
            await _userManager.Received(1).FindByIdAsync(_adminUser.Id);
            await _userManager.Received(1).IsInRoleAsync(_adminUser, SD.Role_Admin);
        }
        [Fact]
        public async Task UserService_IsUserAdmin_Should_ReturnFalse_WhenUserDoesntExist()
        {
            // Arrange
            _userManager.FindByIdAsync(_nonExistentUserId).Returns((ApplicationUser)null);

            // Act
            var result = await _userService.IsUserAdmin(_nonExistentUserId);

            // Assert
            Assert.False(result);

            // Verify FindByIdAsync was called but IsInRoleAsync was not
            await _userManager.Received(1).FindByIdAsync(_nonExistentUserId);
            await _userManager.DidNotReceive().IsInRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
        }

        [Fact]
        public async Task UserService_IsUserAdmin_Should_ReturnFalse_WhenUserIsNotAdmin()
        {
            // Arrange
            _userManager.FindByIdAsync(_agentUser.Id).Returns(_agentUser);
            _userManager.IsInRoleAsync(_agentUser, SD.Role_Admin).Returns(false);

            // Act
            var result = await _userService.IsUserAdmin(_agentUser.Id);

            // Assert
            Assert.False(result);

            // Verify FindByIdAsync and IsInRoleAsync both called once
            await _userManager.Received(1).FindByIdAsync(_agentUser.Id);
            await _userManager.Received(1).IsInRoleAsync(_agentUser, SD.Role_Admin);

        }

        [Fact]
        public async Task UserService_GetAllUsersWithRoles_Should_ReturnAllUserWithRoles()
        {
            // Arrange
            // Create test users
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", Name = "John Doe", Email = "john@example.com", PhoneNumber = "1234567890" },
                new ApplicationUser { Id = "2", Name = "Jane Smith", Email = "jane@example.com", PhoneNumber = "0987654321" }
            };

            // Define roles for each user
            var roles = new Dictionary<string, List<string>>
            {
                { "1", new List<string> { "Admin" } },
                { "2", new List<string> { "User" } }
            };

            // Mock DbSet to support async operations -- mocking the db query
            var userDbSet = CreateMockDbSet(users);
            _userManager.Users.Returns(userDbSet);

            // Mock the GetRolesAsync method to return roles for the users
            _userManager.GetRolesAsync(Arg.Any<ApplicationUser>()).Returns(callInfo =>
            {
                var user = callInfo.Arg<ApplicationUser>();
                return Task.FromResult((IList<string>)roles[user.Id]);
            });

            // Act
            var result = await _userService.GetAllUsersWithRoles();

            // Assert
            // Verify number of users and their roles
            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.Id == "1" && u.Role == "Admin");
            Assert.Contains(result, u => u.Id == "2" && u.Role == "User");
        }




        // This should probably set a role for default if user doesnt have one
        [Fact]
        public async Task UserService_GetAllUsersWithRoles_Should_HandleUsersWithoutRoles()
        {
            // Arrange
            // Create users with no roles
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", Name = "John Doe", Email = "john@example.com", PhoneNumber = "1234567890" },
                new ApplicationUser { Id = "2", Name = "Jane Smith", Email = "jane@example.com", PhoneNumber = "0987654321" }
            };

            // No roles assigned to any user
            var roles = new Dictionary<string, List<string>>
            {
                { "1", new List<string>() }, // Empty role list
                { "2", new List<string>() }
            };

            // Mock Users DbSet
            var userDbSet = CreateMockDbSet(users);
            _userManager.Users.Returns(userDbSet);

            // Mock role retrieval to return empty lists
            _userManager.GetRolesAsync(Arg.Any<ApplicationUser>()).Returns(callInfo =>
            {
                var user = callInfo.Arg<ApplicationUser>();
                return Task.FromResult((IList<string>)roles[user.Id]); // Returns empty list
            });

            // Act
            var result = await _userService.GetAllUsersWithRoles();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.Id == "1" && string.IsNullOrEmpty(u.Role));
            Assert.Contains(result, u => u.Id == "2" && string.IsNullOrEmpty(u.Role));
        }

        [Fact]
        public async Task UserService_GetAllUsersWithRoles_Should_ReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            // No users
            var users = new List<ApplicationUser>(); // Empty list

            // Mock Users DbSet
            var userDbSet = CreateMockDbSet(users);
            _userManager.Users.Returns(userDbSet);

            // Mock role retrieval (though it should never be called)
            _userManager.GetRolesAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult((IList<string>)new List<string>()));

            // Act
            var result = await _userService.GetAllUsersWithRoles();

            // Assert
            Assert.Empty(result); // Ensure an empty list is returned
        }

        [Fact]
        public async Task UserService_DeleteUser_Should_DeleteUserAndReturnTrue_WhenUserExistsAndDeletionSucceeds()
        {
            // Arrange
            _userManager.FindByIdAsync(_testUser.Id).Returns(_testUser);
            _userManager.DeleteAsync(_testUser).Returns(IdentityResult.Success);

            // Act
            var result = await _userService.DeleteUser(_testUser.Id);

            // Assert
            Assert.True(result);

            // Verify FindByIdAsync and DeleteAsync were called once
            await _userManager.Received(1).FindByIdAsync(_testUser.Id);
            await _userManager.Received(1).DeleteAsync(_testUser);
        }

        [Fact]
        public async Task UserService_DeleteUser_Should_ReturnFalse_WhenUserDoesntExist()
        {
            // Arrange
            _userManager.FindByIdAsync(_nonExistentUserId).Returns((ApplicationUser)null);

            // Act 
            var result = await _userService.DeleteUser(_nonExistentUserId);

            // Assert
            Assert.False(result);

            // Verify FindByIdAsync was called once and DeleteAsync never called
            await _userManager.Received(1).FindByIdAsync(_nonExistentUserId);
            await _userManager.DidNotReceive().DeleteAsync(Arg.Any<ApplicationUser>());
        }

        [Fact]
        public async Task UserService_DeleteUser_Should_ReturnFalse_WhenDeletionFails()
        {
            // Arrange
            _userManager.FindByIdAsync(_testUser.Id).Returns(_testUser);
            _userManager.DeleteAsync(_testUser).Returns(IdentityResult.Failed());

            // Act
            var result = await _userService.DeleteUser(_testUser.Id);

            // Assert
            Assert.False(result);

            // Verify FindByIdAsync and DeleteAsync were called once
            await _userManager.Received(1).FindByIdAsync(_testUser.Id);
            await _userManager.Received(1).DeleteAsync(_testUser);
        }

        [Fact]
        public async Task UserService_GetCurrentUserAsync_Should_ReturnUser_WhenUserIsFound()
        {
            // Arrange
            // Setup a mock ClaimsPrincipal with a user identifier
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "some-user-id")
            }));

            var mockUser = new ApplicationUser { Id = "some-user-id", UserName = "testuser" };
            _userManager.GetUserAsync(claimsPrincipal).Returns(Task.FromResult(mockUser));

            // Act
            var result = await _userService.GetCurrentUserAsync(claimsPrincipal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.UserName);
            Assert.Equal("some-user-id", result.Id);
            await _userManager.Received(1).GetUserAsync(claimsPrincipal); // Verify the call to GetUserAsync

        }

        [Fact]
        public async Task UserService_GetCurrentUserAsync_Should_ReturnNull_WhenUserIsNotFound()
        {
            // Arrange
            // Setup a mock ClaimsPrincipal with a user identifier
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "some-user-id")
            }));
            _userManager.GetUserAsync(claimsPrincipal).Returns(Task.FromResult<ApplicationUser>(null));

            // Act
            var result = await _userService.GetCurrentUserAsync(claimsPrincipal);

            // Assert
            Assert.Null(result);
            await _userManager.Received(1).GetUserAsync(claimsPrincipal); // Verify the call to GetUserAsync
        }

        [Fact]
        public async Task UserService_GetCurrentUserAsync_Should_ThrowArgumentNullException_WhenClaimsPrincipalIsNull()
        {
            // Arrange
            ClaimsPrincipal nullClaimsPrincipal = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.GetCurrentUserAsync(nullClaimsPrincipal));
            Assert.Equal("ClaimsPrincipal cannot be null. (Parameter 'user')", exception.Message); // Ensure ArgumentNullException is thrown
        }

        [Fact]
        public async Task UserService_GetUserByEmailAsync_Should_ReturnUser_WhenUserExists()
        {
            // Arrange
            var email = "testuser@example.com";
            var expectedUser = new ApplicationUser { UserName = "testuser", Email = email };

            // Mock the FindByEmailAsync method
            _userManager.FindByEmailAsync(email).Returns(Task.FromResult(expectedUser));

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            Assert.NotNull(result); // Should return a user
            Assert.Equal(expectedUser.Email, result.Email); // Ensure the returned user's email matches
        }

        [Fact]
        public async Task UserService_GetUserByEmailAsync_Should_ReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "nonexistentuser@example.com";

            // Mock the FindByEmailAsync method to return null
            _userManager.FindByEmailAsync(email).Returns(Task.FromResult<ApplicationUser>(null));

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            Assert.Null(result); // Should return null when the user does not exist
        }

        [Fact]
        public async Task UserService_GetUserByEmailAsync_Should_ThrowArgumentException_WhenEmailIsEmpty()
        {
            // Arrange
            string emptyEmail = string.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userService.GetUserByEmailAsync(emptyEmail));
            Assert.Equal("Email cannot be null or empty. (Parameter 'email')", exception.Message); // Ensure proper exception message
        }

        [Fact]
        public async Task UserService_GetUserByEmailAsync_Should_ThrowArgumentException_WhenEmailIsNull()
        {
            // Arrange
            string nullEmail = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userService.GetUserByEmailAsync(nullEmail));
            Assert.Equal("Email cannot be null or empty. (Parameter 'email')", exception.Message); // Ensure proper exception message
        }

        [Fact]
        public async Task UserService_GetUserByIdAsync_Should_ReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = "12345";
            var expectedUser = new ApplicationUser { Id = userId, UserName = "testuser", Email = "testuser@example.com" };

            // Mock the FindByIdAsync method
            _userManager.FindByIdAsync(userId).Returns(Task.FromResult(expectedUser));

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result); // Should return a user
            Assert.Equal(expectedUser.Id, result.Id); // Ensure the returned user's ID matches
            Assert.Equal(expectedUser.UserName, result.UserName); // Ensure the returned user's username matches
        }

        [Fact]
        public async Task UserService_GetUserByIdAsync_Should_ReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "nonexistentuser123";

            // Mock the FindByIdAsync method to return null
            _userManager.FindByIdAsync(userId).Returns(Task.FromResult<ApplicationUser>(null));

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result); // Should return null when the user does not exist
        }

        [Fact]
        public async Task UserService_GetUserByIdAsync_Should_ThrowArgumentNullException_WhenIdIsNull()
        {
            // Arrange
            string nullId = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userService.GetUserByIdAsync(nullId));
            Assert.Equal("Id cannot be null or empty. (Parameter 'id')", exception.Message); // Ensure proper exception message
        }

        [Fact]
        public async Task UserService_GetUserByIdAsync_Should_ThrowArgumentException_WhenIdIsEmpty()
        {
            // Arrange
            string emptyId = string.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userService.GetUserByIdAsync(emptyId));
            Assert.Equal("Id cannot be null or empty. (Parameter 'id')", exception.Message); // Ensure proper exception message
        }

        [Theory]
        [InlineData("existing@example.com", true)]
        [InlineData("notfound@example.com", false)]
        public async Task UserService_CheckUserEmailExistsAsync_Should_ReturnCorrectResult(string email, bool expectedResult)
        {
            // Arrange
            var existingUser = expectedResult ? new ApplicationUser { Email = email } : null;
            _userManager.FindByEmailAsync(email).Returns(Task.FromResult(existingUser));

            // Act
            var result = await _userService.CheckUserEmailExistsAsync(email);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task UserService_CheckUserEmailExistsAsync_Should_ReturnFalse_WhenEmailDoesNotExist()
        {
        }

        [Fact]
        public async Task UserService_CheckUserEmailExistsAsync_Should_ThrowArgumentException_WhenEmailIsNull()
        {
        }

        [Fact]
        public async Task UserService_CheckUserEmailExistsAsync_Should_ThrowArgumentException_WhenEmailIsEmpty()
        {
        }
        // #region Helper methods

        // Helper method to create mock DbSet that support async opertations -- mimic response from db (IQueryable<ApplicationUser>)
        private static DbSet<T> CreateMockDbSet<T>(List<T> elements) where T : class
        {
            var queryable = elements.AsQueryable();

            var mockDbSet = Substitute.For<DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>>();

            // Set up IQueryable implementation
            ((IQueryable<T>)mockDbSet).Provider.Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            ((IQueryable<T>)mockDbSet).Expression.Returns(queryable.Expression);
            ((IQueryable<T>)mockDbSet).ElementType.Returns(queryable.ElementType);
            ((IQueryable<T>)mockDbSet).GetEnumerator().Returns(queryable.GetEnumerator());

            // Set up Async enumeration support
            ((IAsyncEnumerable<T>)mockDbSet).GetAsyncEnumerator(Arg.Any<CancellationToken>())
                .Returns(new TestAsyncEnumerator<T>(elements.GetEnumerator()));

            return mockDbSet;
        }

        // Custom async enumerator to simulate EF core async behaviour
        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return ValueTask.CompletedTask;
            }
            public T Current => _inner.Current;
            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
        }
    

        public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
            public object Execute(Expression expression) => _inner.Execute(expression);
            //public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            {
                return new TestAsyncEnumerable<TResult>(expression);
            }

            public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                var result = Execute<TResult>(expression);
                return Task.FromResult(result);
            }

            TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                return Execute<TResult>(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                var compiled = Expression.Lambda<Func<TResult>>(expression).Compile();
                return compiled();
            }
        }

        public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        // #endregion
    }
}
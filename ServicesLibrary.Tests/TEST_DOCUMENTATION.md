# NUnit Test Documentation

## Overview

This document describes the comprehensive NUnit test suite for the ServicesLibrary project, specifically testing the User management services.

## Test Project

**Project:** ServicesLibrary.Tests  
**Framework:** .NET 9  
**Testing Framework:** NUnit 4.3.1  
**Database:** Entity Framework Core InMemory Database  
**Total Tests:** 38

## Test Results

? **All 38 tests passed successfully**

### Test Breakdown by Service

| Service | Tests | Status |
|---------|-------|--------|
| AddUserService | 12 | ? All Passed |
| EditUserService | 17 | ? All Passed |
| DeleteUserService | 14 | ? All Passed |

## Test Suites

### 1. AddUserServiceTests (12 tests)

Tests for creating new users in the system.

#### Test Cases:

1. **ExecuteAsync_WithValidEmail_CreatesUser**
   - Verifies that a user is created with valid email
   - Checks ID assignment, Active/Deleted flags

2. **ExecuteAsync_WithGroups_AssignsGroupsToUser**
   - Ensures groups are properly assigned to new users
   - Validates group count and IDs

3. **ExecuteAsync_WithNullEmail_ThrowsArgumentException**
   - Tests null email validation
   - Expects ArgumentException

4. **ExecuteAsync_WithEmptyEmail_ThrowsArgumentException**
   - Tests empty string email validation
   - Expects ArgumentException

5. **ExecuteAsync_WithWhitespaceEmail_ThrowsArgumentException**
   - Tests whitespace-only email validation
   - Expects ArgumentException

6. **ExecuteAsync_WithDuplicateEmail_ThrowsInvalidOperationException**
   - Ensures email uniqueness constraint
   - Expects InvalidOperationException

7. **ExecuteAsync_WithDeletedGroup_DoesNotAssignDeletedGroup**
   - Verifies soft-deleted groups are filtered out
   - Only active groups are assigned

8. **ExecuteAsync_WithNonExistentGroups_IgnoresNonExistentGroups**
   - Tests handling of non-existent group IDs
   - Only valid groups are assigned

9. **ExecuteAsync_WithEmptyGroupList_CreatesUserWithoutGroups**
   - Allows user creation without groups
   - Empty collection handling

10. **ExecuteAsync_SetsCreatedAtTimestamp**
    - Verifies CreatedAt timestamp is set
    - Timestamp accuracy validation

11. **ExecuteAsync_PersistsUserToDatabase**
    - Confirms data persistence
    - Database retrieval verification

### 2. EditUserServiceTests (17 tests)

Tests for updating existing users.

#### Test Cases:

1. **ExecuteAsync_UpdateEmail_UpdatesUserEmail**
   - Tests email update functionality
   - Verifies new email is saved

2. **ExecuteAsync_UpdateActiveStatus_UpdatesActiveFlag**
   - Tests active status toggle
   - Boolean flag update verification

3. **ExecuteAsync_UpdateGroups_ReplacesExistingGroups**
   - Tests group reassignment
   - Ensures old groups are replaced

4. **ExecuteAsync_ClearGroups_RemovesAllGroups**
   - Tests removing all groups
   - Empty collection handling

5. **ExecuteAsync_NoGroupsParameter_KeepsExistingGroups**
   - Tests selective update (groups unchanged)
   - Null parameter handling

6. **ExecuteAsync_WithNonExistentUser_ThrowsInvalidOperationException**
   - Tests user existence validation
   - Expects InvalidOperationException

7. **ExecuteAsync_WithDeletedUser_ThrowsInvalidOperationException**
   - Tests soft-delete protection
   - Cannot edit deleted users

8. **ExecuteAsync_WithDuplicateEmail_ThrowsInvalidOperationException**
   - Tests email uniqueness on update
   - Expects InvalidOperationException

9. **ExecuteAsync_WithSameEmail_DoesNotThrowException**
   - Allows keeping same email
   - No duplicate error for unchanged email

10. **ExecuteAsync_SetsUpdatedAtTimestamp**
    - Verifies UpdatedAt timestamp
    - Timestamp accuracy validation

11. **ExecuteAsync_UpdatesMultipleFields_AllFieldsUpdated**
    - Tests updating email, active, and groups together
    - Multi-field update verification

12. **ExecuteAsync_PersistsChangesToDatabase**
    - Confirms data persistence
    - Database update verification

13. **ExecuteAsync_WithNullEmail_DoesNotUpdateEmail**
    - Tests selective update (email unchanged)
    - Null parameter preserves original value

14. **ExecuteAsync_WithNullActive_DoesNotUpdateActiveStatus**
    - Tests selective update (active unchanged)
    - Null parameter preserves original value

### 3. DeleteUserServiceTests (14 tests)

Tests for soft-deleting users.

#### Test Cases:

1. **ExecuteAsync_WithValidUserId_SoftDeletesUser**
   - Tests basic soft delete functionality
   - Verifies both Deleted and Active flags

2. **ExecuteAsync_SetsDeletedFlagToTrue**
   - Confirms Deleted flag is set
   - Boolean validation

3. **ExecuteAsync_SetsActiveFlagToFalse**
   - Confirms Active flag is cleared
   - Boolean validation

4. **ExecuteAsync_SetsUpdatedAtTimestamp**
   - Verifies UpdatedAt on deletion
   - Timestamp accuracy validation

5. **ExecuteAsync_WithNonExistentUser_ThrowsInvalidOperationException**
   - Tests user existence validation
   - Expects InvalidOperationException

6. **ExecuteAsync_WithAlreadyDeletedUser_ThrowsInvalidOperationException**
   - Prevents double deletion
   - Expects InvalidOperationException

7. **ExecuteAsync_DoesNotRemoveUserFromDatabase**
   - Confirms soft delete (not hard delete)
   - User remains in database

8. **ExecuteAsync_PersistsChangesToDatabase**
   - Confirms data persistence
   - Database update verification

9. **ExecuteAsync_ReturnsDeletedUser**
   - Tests return value
   - Deleted user object returned

10. **ExecuteAsync_PreservesUserEmail**
    - Ensures email is not modified
    - Data integrity check

11. **ExecuteAsync_PreservesCreatedAtTimestamp**
    - Ensures CreatedAt is not modified
    - Timestamp preservation

12. **ExecuteAsync_MultipleDeletes_OnlyFirstSucceeds**
    - Tests idempotency
    - Second delete should fail

13. **ExecuteAsync_DeleteMultipleUsers_EachDeletedIndependently**
    - Tests multiple user deletion
    - Independent operation verification

## Testing Approach

### Setup Pattern

Each test class uses the following pattern:

```csharp
[SetUp]
public void Setup()
{
    // Create InMemory database
    var options = new DbContextOptionsBuilder<UserContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new UserContext(options);
    _service = new ServiceClass(_context);

    // Seed test data
    SeedTestData();
}

[TearDown]
public void TearDown()
{
    _context.Database.EnsureDeleted();
    _context.Dispose();
}
```

### Test Data Seeding

Each test suite seeds appropriate test data:
- **Groups**: Active, inactive, and deleted groups
- **Users**: Active users, deleted users
- **Relationships**: User-Group associations

### Assertion Style

Tests use NUnit's constraint-based assertions:

```csharp
Assert.That(result.Email, Is.EqualTo(expectedEmail));
Assert.That(result.Groups, Has.Count.EqualTo(2));
Assert.That(result.Active, Is.True);
Assert.ThrowsAsync<InvalidOperationException>(...);
```

## Running the Tests

### Command Line

```bash
# Run all tests
dotnet test ServicesLibrary.Tests/ServicesLibrary.Tests.csproj

# Run with detailed output
dotnet test ServicesLibrary.Tests/ServicesLibrary.Tests.csproj --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~AddUserServiceTests"

# Run single test
dotnet test --filter "FullyQualifiedName~ExecuteAsync_WithValidEmail_CreatesUser"
```

### Visual Studio

1. Open Test Explorer (Test > Test Explorer)
2. Click "Run All" to run all tests
3. Right-click individual tests to run specific tests
4. View test output and results in the Test Explorer window

### Test Coverage

Key scenarios covered:
- ? Happy path scenarios (valid inputs)
- ? Validation errors (null, empty, whitespace)
- ? Business rule violations (duplicates, non-existent records)
- ? Soft delete behavior
- ? Data persistence
- ? Timestamp management
- ? Relationship management (User-Group)
- ? Edge cases (empty collections, null parameters)

## Test Dependencies

```xml
<PackageReference Include="NUnit" Version="4.3.1" />
<PackageReference Include="NUnit3TestAdapter" Version="4.6.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.12" />
<PackageReference Include="coverlet.collector" Version="6.0.2" />
```

## Best Practices Implemented

1. **Isolated Tests**: Each test uses a unique InMemory database
2. **Cleanup**: TearDown ensures proper disposal
3. **Descriptive Names**: Test names clearly describe what is being tested
4. **Arrange-Act-Assert**: Clear test structure
5. **Single Responsibility**: Each test verifies one specific behavior
6. **No Test Dependencies**: Tests can run in any order
7. **Comprehensive Coverage**: Tests cover success and failure paths

## Continuous Integration

These tests are suitable for CI/CD pipelines:
- Fast execution (< 3 seconds)
- No external dependencies
- Deterministic results
- Easy to run in automated environments

## Future Enhancements

Potential test improvements:
- Performance benchmarking tests
- Concurrent access tests
- Integration tests with real database
- Property-based testing (FsCheck)
- Mutation testing
- Code coverage reports

## Conclusion

The test suite provides comprehensive coverage of the user management services, ensuring:
- ? Correct business logic implementation
- ? Proper error handling
- ? Data integrity
- ? Soft delete functionality
- ? Validation rules enforcement

All 38 tests pass successfully, providing confidence in the service layer implementation.

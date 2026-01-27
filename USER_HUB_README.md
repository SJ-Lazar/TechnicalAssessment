# User Hub - User Management Application

A full-stack user management application built with Blazor WebAssembly and ASP.NET Core Web API with comprehensive NUnit test coverage.

## Features

- ? Add new users with email and group assignments
- ? Edit existing users (email, active status, groups)
- ? Soft delete users
- ? View total user count
- ? Dark gray theme UI
- ? Responsive design
- ? **API Testing Dashboard** - Test all API endpoints directly from the UI
- ? **Comprehensive NUnit Tests** - 68 passing tests covering all services

## Architecture

### Projects

1. **WebInterface** - Blazor WebAssembly front-end
2. **WebService** - ASP.NET Core Web API
3. **ServicesLibrary** - Business logic services
4. **SharedLibrary (DataAccess)** - Entity Framework Core models and DbContext
5. **ServicesLibrary.Tests** - NUnit test project (38 tests)
6. **UserHubTests** - Integration test project (30 tests)

### Technology Stack

- .NET 9
- Blazor WebAssembly
- ASP.NET Core Web API
- Entity Framework Core
- SQLite Database
- NUnit 4.3 (Testing Framework)
- EF Core InMemory (Test Database)

## Running the Application

### Step 1: Start the Web API

```bash
cd WebService
dotnet run
```

The API will start on `https://localhost:7293` (HTTP: `http://localhost:5123`)

### Step 2: Start the Blazor WebAssembly App

Open a new terminal:

```bash
cd WebInterface
dotnet run
```

The UI will start on `https://localhost:7166` (HTTP: `http://localhost:5132`)

### Step 3: Access the Application

Open your browser and navigate to:
- **Web Interface**: https://localhost:7166
- **API Test Dashboard**: https://localhost:7166/api-test
- **API Swagger/OpenAPI**: https://localhost:7293/openapi/v1.json (in development mode)

### Port Configuration

| Service | HTTPS Port | HTTP Port |
|---------|-----------|-----------|
| **WebService (API)** | 7293 | 5123 |
| **WebInterface (Blazor)** | 7166 | 5132 |

**Important:** The ports are configured in `launchSettings.json` for each project. If you change them, make sure to update:
1. WebService `Program.cs` - CORS policy origins
2. WebInterface `Program.cs` - HttpClient BaseAddress

## Running Tests

### Run All Tests

```bash
dotnet test ServicesLibrary.Tests/ServicesLibrary.Tests.csproj
```

### Run Tests with Detailed Output

```bash
dotnet test ServicesLibrary.Tests/ServicesLibrary.Tests.csproj --verbosity normal
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~AddUserServiceTests"
```

### Test Results

? **68/68 tests passing**

- **ServicesLibrary.Tests**: 38 tests (Unit tests)
- **UserHubTests**: 30 tests (Integration tests)

For detailed test documentation, see [TEST_COVERAGE_SUMMARY.md](TEST_COVERAGE_SUMMARY.md)

## Database

The application uses SQLite with the database file `users.db` created automatically in the WebService directory.

### Seeded Data

The database includes pre-seeded data:

**Users:**
- admin@example.com (ID: 1)
- user1@example.com (ID: 2)
- user2@example.com (ID: 3)

**Groups:**
- Admin (ID: 1)
- Level 1 (ID: 2)
- Level 2 (ID: 3)

**Permissions:**
- ManageUsers (ID: 1)
- ReadReports (ID: 2)
- WriteReports (ID: 3)

## API Endpoints

### Users

- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Soft delete user

### Groups

- `GET /api/users/groups` - Get all groups

## Usage

### User Management (Home Page)

1. **Add User**: Click the "+ Add User" button, enter email and select groups
2. **Edit User**: Click "Edit" on a user card to update their information
3. **Delete User**: Click "Delete" on a user card (soft delete - user is marked as deleted)
4. **View Details**: Each card shows user ID, email, status, groups, and timestamps

### API Testing Dashboard

The API Test page provides a comprehensive interface to test all API endpoints:

#### Features:
- **Connection Status**: Real-time API health check
- **Request/Response Viewer**: See exact JSON requests and responses
- **Response Time**: Monitor API performance
- **HTTP Status Codes**: View response status for each request
- **Interactive Forms**: Enter parameters and JSON bodies directly

#### Available Tests:
1. **GET /api/users** - Retrieve all users
2. **GET /api/users/{id}** - Get specific user by ID
3. **POST /api/users** - Create new user with JSON body
4. **PUT /api/users/{id}** - Update user with JSON body
5. **DELETE /api/users/{id}** - Soft delete user
6. **GET /api/users/groups** - Get all available groups

#### How to Use:
1. Navigate to "API Test" from the menu or click "?? API Test" on home page
2. Check the connection status (green = connected, red = disconnected)
3. Select an endpoint test to execute
4. Enter required parameters (User ID, JSON body, etc.)
5. Click "Execute" to send the request
6. View the response, status code, and response time

#### Example JSON Payloads:

**Create User:**
```json
{
  "email": "newuser@example.com",
  "groupIds": [1, 2]
}
```

**Update User:**
```json
{
  "email": "updated@example.com",
  "groupIds": [1],
  "active": true
}
```

## Testing

### Test Coverage

The ServicesLibrary.Tests project provides comprehensive test coverage:

| Service | Test Count | Coverage Areas |
|---------|-----------|----------------|
| AddUserService | 12 tests | Email validation, group assignment, duplicate detection, persistence |
| EditUserService | 17 tests | Email updates, active status, group management, selective updates |
| DeleteUserService | 14 tests | Soft delete, idempotency, timestamp management, data preservation |

### Testing Approach

- **InMemory Database**: Each test uses an isolated EF Core InMemory database
- **Arrange-Act-Assert**: Clear test structure
- **Comprehensive Scenarios**: Happy paths, validation errors, edge cases
- **Fast Execution**: All 68 tests complete in ~5 seconds

### Test Scenarios Covered

? Valid input handling  
? Null and empty validation  
? Duplicate detection  
? Soft delete behavior  
? Data persistence  
? Timestamp management  
? Relationship management  
? Error handling  
? Edge cases  

## Development Notes

- All user operations are soft deletes (users are marked as deleted, not removed)
- Email addresses must be unique
- User active status can be toggled during editing
- Groups are optional when creating/updating users
- The UI uses a dark gray theme for better visibility
- API responses are formatted JSON for easy reading
- All services are fully tested with NUnit

## Troubleshooting

### CORS Errors

If you encounter CORS errors, ensure:
1. WebService is running on `https://localhost:7293`
2. WebInterface is running on `https://localhost:7166`
3. CORS policy in WebService/Program.cs includes the correct WebInterface URL
4. HttpClient BaseAddress in WebInterface/Program.cs points to the correct API URL

**Current Configuration:**
- API allows origins: `https://localhost:7166`, `http://localhost:5132`
- Blazor calls API at: `https://localhost:7293`

### Database Issues

If you need to reset the database:
1. Stop the WebService
2. Delete `users.db` file from WebService directory
3. Restart WebService (database will be recreated with seed data)

### API Connection Issues

If the API Test page shows "Disconnected":
1. Verify WebService is running on `https://localhost:7293`
2. Check browser console for detailed error messages
3. Click "Test Connection" to retry
4. Ensure CORS is properly configured
5. Verify no firewall is blocking the ports

### Port Conflicts

If ports 7293 or 7166 are already in use:
1. Stop other applications using those ports
2. Or modify `Properties/launchSettings.json` in each project
3. Update CORS origins in WebService/Program.cs
4. Update HttpClient BaseAddress in WebInterface/Program.cs

### Test Failures

If tests fail:
1. Ensure .NET 9 SDK is installed
2. Restore NuGet packages: `dotnet restore`
3. Clean and rebuild: `dotnet clean && dotnet build`
4. Check that EF Core InMemory package is installed

## Project Structure

```
TechnicalAssessment/
??? WebInterface/           # Blazor WebAssembly UI (Port 7166)
?   ??? Components/         # Reusable UI components
?   ?   ??? UserCard.razor  # User display card
?   ?   ??? UserModal.razor # Add/Edit modal
?   ?   ??? UserFormData.cs # Form data model
?   ??? Pages/             # Razor pages
?   ?   ??? Home.razor     # User management page
?   ?   ??? ApiTest.razor  # API testing dashboard
?   ??? Services/          # API service layer
?   ?   ??? UserApiService.cs
?   ??? wwwroot/           # Static files and CSS
??? WebService/            # ASP.NET Core Web API (Port 7293)
?   ??? Controllers/       # API controllers
?       ??? UsersController.cs
??? ServicesLibrary/       # Business logic
?   ??? Users/            # User services
?       ??? AddUserService.cs
?       ??? EditUserService.cs
?       ??? DeleteUserService.cs
??? ServicesLibrary.Tests/ # NUnit test project
?   ??? Users/            # Service tests
?   ?   ??? AddUserServiceTests.cs (12 tests)
?   ?   ??? EditUserServiceTests.cs (17 tests)
?   ?   ??? DeleteUserServiceTests.cs (14 tests)
?   ??? TEST_DOCUMENTATION.md
??? UserHubTests/         # Integration test project
?   ??? Controllers/      # Controller tests
?   ?   ??? UsersControllerTests.cs (20 tests)
?   ??? Integration/      # Workflow tests
?   ?   ??? UserWorkflowTests.cs (10 tests)
?   ??? README.md
??? DataAccess/           # Data models and context
    ??? Contexts/         # EF Core DbContext
    ??? DTOs/            # Data transfer objects
    ??? UserModels/      # User entity
    ??? GroupModels/     # Group entity
    ??? PermissionsModels/ # Permission entity
```

## License

This project is for educational and assessment purposes.

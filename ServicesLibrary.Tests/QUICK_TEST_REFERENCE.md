# Quick Test Reference Guide

## Running All Tests

```bash
# Basic test run
dotnet test ServicesLibrary.Tests/ServicesLibrary.Tests.csproj

# With detailed output
dotnet test ServicesLibrary.Tests/ServicesLibrary.Tests.csproj --verbosity normal

# With very detailed output
dotnet test ServicesLibrary.Tests/ServicesLibrary.Tests.csproj --logger "console;verbosity=detailed"
```

## Running Specific Tests

```bash
# Run only AddUserService tests
dotnet test --filter "FullyQualifiedName~AddUserServiceTests"

# Run only EditUserService tests
dotnet test --filter "FullyQualifiedName~EditUserServiceTests"

# Run only DeleteUserService tests
dotnet test --filter "FullyQualifiedName~DeleteUserServiceTests"

# Run a single specific test
dotnet test --filter "FullyQualifiedName~ExecuteAsync_WithValidEmail_CreatesUser"

# Run tests containing specific text
dotnet test --filter "Name~Email"
```

## Test with Coverage (Optional)

```bash
# Install coverage tool (one-time)
dotnet tool install --global dotnet-coverage

# Run tests with coverage
dotnet coverage collect "dotnet test ServicesLibrary.Tests/ServicesLibrary.Tests.csproj"
```

## Visual Studio

1. **Open Test Explorer:**
   - Menu: `Test` ? `Test Explorer`
   - Shortcut: `Ctrl+E, T`

2. **Run Tests:**
   - Click "Run All" (green play button)
   - Right-click specific test ? "Run"
   - Keyboard: `Ctrl+R, A` (all tests)

3. **Debug Tests:**
   - Right-click test ? "Debug"
   - Set breakpoints in test code

## Expected Output

```
Test Run Successful.
Total tests: 38
     Passed: 38
 Total time: ~2.3 Seconds
```

## Troubleshooting

### Tests Not Discovered
```bash
# Clean and rebuild
dotnet clean
dotnet build
dotnet test
```

### Package Issues
```bash
# Restore packages
dotnet restore ServicesLibrary.Tests/ServicesLibrary.Tests.csproj
```

### Build Errors
```bash
# Check .NET version
dotnet --version  # Should be 9.0.x

# Rebuild solution
dotnet build
```

## Test Organization

```
ServicesLibrary.Tests/
??? Users/
?   ??? AddUserServiceTests.cs      (12 tests)
?   ??? EditUserServiceTests.cs     (17 tests)
?   ??? DeleteUserServiceTests.cs   (14 tests)
??? TEST_DOCUMENTATION.md
??? TEST_EXECUTION_SUMMARY.md
??? QUICK_TEST_REFERENCE.md (this file)
```

## CI/CD Integration

### GitHub Actions Example
```yaml
- name: Run Tests
  run: dotnet test --no-build --verbosity normal
```

### Azure DevOps Example
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Tests'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
```

## Quick Stats

- **Total Tests:** 38
- **Test Projects:** 1
- **Execution Time:** ~2-3 seconds
- **Dependencies:** NUnit, EF InMemory
- **Success Rate:** 100%

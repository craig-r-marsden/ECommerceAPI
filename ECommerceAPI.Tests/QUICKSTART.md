# Quick Start Guide for Running Tests

## Running Tests in Visual Studio

### Using Test Explorer
1. Open **Test Explorer** (Test → Test Explorer or Ctrl+E, T)
2. Click **Run All** to run all tests
3. Click individual tests to run them separately
4. Right-click on test groups to run by category

### Using Solution Explorer
1. Right-click on the test project (`ECommerceAPI.Tests`)
2. Select **Run Tests** from the context menu

### Keyboard Shortcuts
- **Ctrl + R, A** - Run all tests in solution
- **Ctrl + R, T** - Run tests in current context
- **Ctrl + R, Ctrl + T** - Debug tests in current context

## Running Tests from Command Line

### Basic Commands
```bash
# Run all tests
dotnet test

# Run tests in specific project
dotnet test ECommerceAPI.Tests/ECommerceAPI.Tests.csproj

# Run without rebuilding
dotnet test --no-build

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

### Filtering Tests
```bash
# Run tests by name pattern
dotnet test --filter "ProductsService"

# Run tests by category
dotnet test --filter "FullyQualifiedName~Services"

# Run specific test method
dotnet test --filter "FullyQualifiedName~GetProductAsync_WhenProductExists"
```

### Code Coverage
```bash
# Collect code coverage
dotnet test --collect:"XPlat Code Coverage"

# With specific settings
dotnet test --collect:"Code Coverage" --settings:coverage.runsettings
```

## Test Organization

```
ECommerceAPI.Tests/
├── Controllers/
│   ├── ProductsControllerTests.cs      (7 tests)
│   └── InventoryControllerTests.cs     (4 tests)
├── Services/
│   ├── ProductsServiceTests.cs         (7 tests)
│   └── InventoryServiceTests.cs        (7 tests)
├── Middleware/
│   └── CorrelationIdMiddlewareTests.cs (8 tests)
├── README.md
├── TEST_SUMMARY.md
└── QUICKSTART.md (this file)
```

## Debugging Tests

### In Visual Studio
1. Set breakpoints in your test code
2. Right-click on test in Test Explorer
3. Select **Debug Selected Tests**

### From Command Line
Not directly supported, but you can:
1. Open Visual Studio
2. Use Test Explorer to debug
3. Or attach debugger to `dotnet test` process

## Continuous Integration

Add to your CI/CD pipeline:
```yaml
# Example for GitHub Actions
- name: Run tests
  run: dotnet test --logger trx --results-directory TestResults

- name: Publish test results
  uses: actions/upload-artifact@v3
  with:
    name: test-results
    path: TestResults
```

## Viewing Test Results

Test results are displayed in:
- **Test Explorer** (Visual Studio)
- **Console output** (command line)
- **TRX files** (when using `--logger trx`)
- **Code coverage reports** (when collecting coverage)

## Tips for Test Development

1. **Follow AAA Pattern**: Arrange, Act, Assert
2. **Name tests descriptively**: Method_Scenario_ExpectedResult
3. **One assertion per test** (when possible)
4. **Keep tests fast**: Use mocks for external dependencies
5. **Test edge cases**: null, empty, error conditions

## Common Issues

### Tests not showing in Test Explorer
- Rebuild the solution
- Close and reopen Test Explorer
- Clean and rebuild: `dotnet clean && dotnet build`

### Tests failing in CI but passing locally
- Check for hardcoded paths or dependencies
- Verify database state (use in-memory DB for tests)
- Check for timing issues (use deterministic test data)

### Slow test execution
- Check if tests are running sequentially
- Reduce test data set size
- Use mock objects instead of real dependencies

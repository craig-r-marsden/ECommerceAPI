# ECommerceAPI.Tests

This is the test project for the ECommerceAPI application. It contains comprehensive unit tests for all major components.

## Test Structure

### Services Tests
- **ProductsServiceTests.cs**: Tests for the ProductsService business logic
  - Product retrieval with and without inventory data
  - Product creation
  - Fetching all products
  - Edge cases and error handling

- **InventoryServiceTests.cs**: Tests for the InventoryService HTTP client
  - Successful HTTP requests
  - Failed HTTP requests (404, network errors)
  - Exception handling
  - Correlation ID header propagation

### Controllers Tests
- **ProductsControllerTests.cs**: Tests for the ProductsController endpoints
  - GET /api/products/{id} endpoint
  - POST /api/products endpoint
  - GET /api/products endpoint
  - Model validation
  - HTTP status codes (200, 201, 400, 404)

- **InventoryControllerTests.cs**: Tests for the InventoryController (mock inventory API)
  - GET /api/inventory/{productId} endpoint
  - Random data generation
  - Correlation ID logging
  - Data validation (price and stock ranges)

### Middleware Tests
- **CorrelationIdMiddlewareTests.cs**: Tests for the correlation ID middleware
  - Correlation ID generation
  - Existing correlation ID propagation
  - Header management
  - Logging verification

## Test Coverage

The test suite covers:
- ✅ Happy path scenarios
- ✅ Error and exception handling
- ✅ Null/empty data scenarios
- ✅ HTTP status code validation
- ✅ Logging behavior
- ✅ Correlation ID tracking
- ✅ Database interactions (using in-memory database)
- ✅ HTTP client behavior (using mocked HttpMessageHandler)

## Testing Frameworks & Libraries

- **xUnit**: Main testing framework
- **Moq**: Mocking framework for dependencies
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing
- **Microsoft.AspNetCore.Mvc.Testing**: ASP.NET Core testing utilities

## Running Tests

Run all tests:
```bash
dotnet test
```

Run tests with detailed output:
```bash
dotnet test --logger "console;verbosity=detailed"
```

Run tests with code coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

Run specific test:
```bash
dotnet test --filter "FullyQualifiedName~ProductsServiceTests.GetProductAsync_WhenProductExists"
```

## Test Patterns Used

1. **Arrange-Act-Assert (AAA)**: All tests follow the AAA pattern for clarity
2. **Mocking**: External dependencies are mocked using Moq
3. **In-Memory Database**: EF Core in-memory provider for database testing
4. **HttpMessageHandler Mocking**: Protected method mocking for HttpClient testing

## Adding New Tests

When adding new features to the API:
1. Create corresponding test classes in the appropriate folder (Services, Controllers, Middleware)
2. Follow the existing naming convention: `{ClassName}Tests`
3. Use the AAA pattern for test structure
4. Mock external dependencies
5. Test both success and failure scenarios
6. Verify logging when appropriate

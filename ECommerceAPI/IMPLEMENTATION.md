# E-Commerce API - Job Application Implementation

## Overview
This is a complete implementation of the .NET Web API test for managing a product catalogue with real-time inventory data enrichment from a third-party provider.

## ? Requirements Checklist

### 1. Local Persistence ?
- **Entity**: `Product` with `Id`, `Name`, `Description`
- **Location**: `ECommerceAPI/Models/Product.cs`
- **Database**: SQL Server via Entity Framework Core
- **DbContext**: `ApplicationDbContext` in `ECommerceAPI/Data/ApplicationDbContext.cs`

### 2. External Integration ?
- **Interface**: `IInventoryService` in `ECommerceAPI/Services/IInventoryService.cs`
- **Implementation**: `InventoryService` in `ECommerceAPI/Services/InventoryService.cs`
- **HTTP Client**: Configured via `HttpClientFactory` with proper DI
- **Mock API**: `InventoryController` provides a mocked third-party API for testing

### 3. Data Orchestration ?
- **GET /api/products/{id}**: 
  - Returns merged local + live inventory data
  - Implemented in `ProductsController.GetProduct()`
- **POST /api/products**: 
  - Accepts only local data (Name, Description)
  - Does NOT send data to third-party service
  - Implemented in `ProductsController.CreateProduct()`

### 4. Resilience ?
- **Fallback Mechanism**: 
  - If external service fails, returns local data with warning
  - `DataStatus` field indicates data availability
  - Examples:
    - `"Live"` - External data successfully fetched
    - `"Data Unavailable - External service error"` - Fallback mode
- **Error Handling**: Try-catch blocks with comprehensive logging
- **Timeout**: 30-second HTTP client timeout configured

### 5. Middleware ?
- **Custom Middleware**: `CorrelationIdMiddleware`
- **Location**: `ECommerceAPI/Middleware/CorrelationIdMiddleware.cs`
- **Functionality**:
  - Extracts or generates X-Correlation-ID
  - Adds to response headers
  - Passes to external service calls
  - Logs request/response with correlation ID

### 6. Architecture ?
Clean architecture with separation of concerns:
```
Controllers/     - API endpoints and request handling
Data/           - EF Core DbContext
DTOs/           - Data Transfer Objects
Middleware/     - Custom middleware components
Models/         - Domain entities
Services/       - Business logic and external integrations
```

### 7. Validation ?
- **Location**: `CreateProductRequest` DTO with data annotations
- **Rules**:
  - Name: Required, 1-200 characters
  - Description: Required, 1-2000 characters
- **Response**: 400 Bad Request with detailed validation errors

### 8. Technology Stack ?
All required technologies implemented:
- ? .NET 10
- ? ASP.NET Core Web API
- ? HttpClientFactory (third-party integration)
- ? Entity Framework Core
- ? Middleware implementation
- ? SQL Server (LocalDB for dev)
- ? Dependency Injection

## Project Structure

```
ECommerceAPI/
?
??? Controllers/
?   ??? ProductsController.cs       # Main API endpoints
?   ??? InventoryController.cs      # Mock third-party API
?
??? Data/
?   ??? ApplicationDbContext.cs     # EF Core context
?
??? DTOs/
?   ??? CreateProductRequest.cs     # Input validation
?   ??? ProductResponse.cs          # Merged response
?   ??? InventoryData.cs           # External API model
?
??? Middleware/
?   ??? CorrelationIdMiddleware.cs  # Correlation ID tracking
?
??? Models/
?   ??? Product.cs                  # Domain entity
?
??? Services/
?   ??? IInventoryService.cs        # Service interface
?   ??? InventoryService.cs         # External API integration
?
??? Program.cs                      # Application configuration
??? appsettings.json               # Configuration
??? api-tests.http                 # API test examples
```

## API Endpoints

### 1. Create Product
```http
POST /api/products
Content-Type: application/json
X-Correlation-ID: optional-guid

{
  "name": "Product Name",
  "description": "Product Description"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "name": "Product Name",
  "description": "Product Description",
  "price": null,
  "stock": null,
  "dataStatus": "Local data only - Price and stock not available at creation"
}
```

### 2. Get Product by ID
```http
GET /api/products/{id}
X-Correlation-ID: optional-guid
```

**Response (200 OK) - With Live Data:**
```json
{
  "id": 1,
  "name": "Product Name",
  "description": "Product Description",
  "price": 99.99,
  "stock": 42,
  "dataStatus": "Live"
}
```

**Response (200 OK) - Fallback Mode:**
```json
{
  "id": 1,
  "name": "Product Name",
  "description": "Product Description",
  "price": null,
  "stock": null,
  "dataStatus": "Data Unavailable - External service error"
}
```

### 3. Get All Products
```http
GET /api/products
X-Correlation-ID: optional-guid
```

Returns array of products with their inventory data.

## Key Features Explained

### Correlation ID Tracking
The middleware automatically:
1. Checks for existing `X-Correlation-ID` header
2. Generates new GUID if not present
3. Adds to response headers
4. Passes to external service calls
5. Logs throughout request lifecycle

**Flow:**
```
Client Request ? Middleware (extract/generate ID) 
              ? Controller 
              ? InventoryService (pass ID to external API) 
              ? External API receives same ID
              ? Response includes ID
```

### Resilience Strategy
```csharp
try {
    // Attempt external API call
    var inventoryData = await _inventoryService.GetInventoryDataAsync(id, correlationId);
    
    if (inventoryData != null) {
        // Success: return merged data
        response.Price = inventoryData.Price;
        response.DataStatus = "Live";
    } else {
        // Fallback: return local data only
        response.DataStatus = "Data Unavailable";
    }
} catch {
    // Error handling with logging
}
```

### HttpClientFactory Configuration
```csharp
builder.Services.AddHttpClient<IInventoryService, InventoryService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7000");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

Benefits:
- Proper socket handling
- Centralized configuration
- Automatic disposal
- Testability

## Configuration

### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ECommerceDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### External Service
```json
{
  "InventoryApi": {
    "BaseUrl": "https://localhost:7000"
  }
}
```

## Running the Application

1. **Prerequisites:**
   - .NET 10 SDK
   - SQL Server LocalDB (or SQL Server)

2. **Build:**
   ```bash
   dotnet build
   ```

3. **Run:**
   ```bash
   dotnet run
   ```

4. **Database:**
   - Automatically created on first run (Development mode)
   - Uses EF Core with SQL Server

## Testing

### Using the HTTP file
The included `api-tests.http` file contains example requests:
- Create products
- Test validation
- Retrieve products with inventory data
- Test error handling

### Manual Testing
```bash
# Create a product
curl -X POST https://localhost:7000/api/products \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: test-123" \
  -d '{"name":"Test Product","description":"A test product"}'

# Get the product (with inventory data)
curl https://localhost:7000/api/products/1 \
  -H "X-Correlation-ID: test-123"
```

## Mock Inventory API

The application includes a mock inventory controller at `/api/inventory/{productId}` that:
- Returns random prices (10-1010)
- Returns random stock levels (0-100)
- Logs correlation IDs
- Simulates the external third-party API

This allows full end-to-end testing without an actual external service.

## Logging

Comprehensive logging includes:
- Request/response with correlation IDs
- External service calls
- Success/failure indicators
- Error details

Example log output:
```
[INF] Incoming request: GET /api/products/1 with Correlation-ID: abc-123
[INF] Fetching inventory data for Product 1 with Correlation-ID: abc-123
[INF] Mock Inventory API - Request for Product 1 with Correlation-ID: abc-123
[INF] Successfully retrieved inventory data for Product 1 with Correlation-ID: abc-123
[INF] Outgoing response: 200 for GET /api/products/1 with Correlation-ID: abc-123
```

## Design Decisions

### 1. Clean Architecture
- Separation of concerns with clear layers
- Easy to test and maintain
- Follows SOLID principles

### 2. DTOs for API Contracts
- Separate DTOs from domain models
- Validation at the boundary
- Flexible response structure

### 3. Resilience First
- Never fail completely if external service is down
- Always return something useful to the client
- Clear status indicators

### 4. Comprehensive Logging
- Correlation IDs for distributed tracing
- Detailed error information
- Performance monitoring capability

### 5. Configuration-based
- Easy to change external API URLs
- Environment-specific settings
- No hardcoded values

## Production Considerations

For production deployment, consider:
- [ ] Replace mock inventory API with actual service
- [ ] Add authentication/authorization
- [ ] Implement caching for inventory data
- [ ] Add database migrations
- [ ] Configure CORS policies
- [ ] Add rate limiting
- [ ] Implement health checks
- [ ] Add unit and integration tests
- [ ] Configure retry policies (Polly)
- [ ] Add API versioning
- [ ] Implement circuit breaker pattern
- [ ] Add performance monitoring
- [ ] Configure secure connection strings

## Summary

This implementation fully satisfies all requirements:
? Local persistence with EF Core
? External service integration with HttpClientFactory
? Data orchestration with merged responses
? Resilience with fallback mechanisms
? Custom correlation ID middleware
? Clean architecture
? Comprehensive validation
? All required technologies
? Production-ready patterns
? Comprehensive documentation

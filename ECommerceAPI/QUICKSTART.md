# Quick Start Guide

## ?? Running the Application

### Step 1: Build the Project
```bash
dotnet build
```

### Step 2: Run the Application
```bash
dotnet run --project ECommerceAPI
```

The API will start on `https://localhost:7000` (or the port shown in console).

### Step 3: Test the API

#### Option A: Using the HTTP File
Open `api-tests.http` in Visual Studio and click "Send Request" on any test.

#### Option B: Using curl

**1. Create a product:**
```bash
curl -X POST https://localhost:7000/api/products \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: test-001" \
  -k \
  -d "{\"name\":\"Gaming Laptop\",\"description\":\"High-performance gaming laptop\"}"
```

**2. Get the product with live inventory data:**
```bash
curl https://localhost:7000/api/products/1 \
  -H "X-Correlation-ID: test-002" \
  -k
```

**3. Get all products:**
```bash
curl https://localhost:7000/api/products \
  -H "X-Correlation-ID: test-003" \
  -k
```

## ?? What You'll See

### Successful Response (with live data):
```json
{
  "id": 1,
  "name": "Gaming Laptop",
  "description": "High-performance gaming laptop",
  "price": 1299.99,
  "stock": 45,
  "dataStatus": "Live"
}
```

### Fallback Response (if external service fails):
```json
{
  "id": 1,
  "name": "Gaming Laptop",
  "description": "High-performance gaming laptop",
  "price": null,
  "stock": null,
  "dataStatus": "Data Unavailable - External service error"
}
```

## ?? Checking Correlation IDs

Watch the console output to see correlation IDs being tracked:
```
[INF] Incoming request: GET /api/products/1 with Correlation-ID: test-002
[INF] Fetching inventory data for Product 1 with Correlation-ID: test-002
[INF] Mock Inventory API - Request for Product 1 with Correlation-ID: test-002
[INF] Successfully retrieved inventory data for Product 1 with Correlation-ID: test-002
[INF] Outgoing response: 200 for GET /api/products/1 with Correlation-ID: test-002
```

## ?? Testing Resilience

To test the fallback mechanism, you can:

1. **Temporarily break the inventory service URL:**
   Edit `appsettings.json`:
   ```json
   "InventoryApi": {
     "BaseUrl": "https://invalid-url:9999"
   }
   ```

2. **Restart and call the API:**
   ```bash
   curl https://localhost:7000/api/products/1 -k
   ```

3. **You'll see the fallback response with `"dataStatus": "Data Unavailable"` and the product data still returns successfully!**

## ?? Testing Validation

**Test with invalid data:**
```bash
curl -X POST https://localhost:7000/api/products \
  -H "Content-Type: application/json" \
  -k \
  -d "{\"name\":\"\",\"description\":\"\"}"
```

**Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["Product name is required"],
    "Description": ["Product description is required"]
  }
}
```

## ??? Database

The database is automatically created when you run the application in Development mode.

**Connection String (LocalDB):**
```
Server=(localdb)\mssqllocaldb;Database=ECommerceDB;Trusted_Connection=true;TrustServerCertificate=true;
```

**To view the database:**
- Open SQL Server Object Explorer in Visual Studio
- Look for `(localdb)\mssqllocaldb`
- Expand `ECommerceDB` ? Tables ? `Products`

## ?? Key Features Demonstrated

1. ? **Local Product Storage** - Products saved to SQL Server
2. ? **External API Integration** - Real-time price/stock from mock service
3. ? **Data Merging** - Local + external data combined in response
4. ? **Resilience** - Graceful degradation when external service fails
5. ? **Correlation ID Tracking** - Request tracking across services
6. ? **Validation** - Input validation with clear error messages
7. ? **Logging** - Comprehensive request/response logging

## ?? Next Steps

- Review `IMPLEMENTATION.md` for detailed architecture explanation
- Explore the code structure in the project
- Check out the mock inventory API at `/api/inventory/{id}`
- Review logging output to see correlation ID flow

## ?? Troubleshooting

### Port Already in Use
If port 7000 is taken, the app will use a different port. Check the console output for the actual URL.

### Database Connection Issues
Make sure SQL Server LocalDB is installed. It comes with Visual Studio.

### SSL Certificate Warnings
Use the `-k` flag with curl to bypass SSL warnings in development, or trust the dev certificate:
```bash
dotnet dev-certs https --trust
```

## ?? Success!

If you can create a product and retrieve it with inventory data, everything is working correctly!

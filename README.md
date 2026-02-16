# E-Commerce API

## Setup Instructions

1. Ensure db connection string is set to a valid value in `appsettings.Development.json` (server will need to be changed if SQL Server Express LocalDB is not installed)
2. Build and run app (db will be created with seed data automatically using the connection set in `appsettings.Development.json`)

## Documentation and Testing

Documentation for the API can be viewed in Swagger: 

https://localhost:7025/swagger/index.html

Endpoints can be tested in Swagger, or using a pre-defined set of tests in Postman by importing the requests in `LRQA Dev Challenge.postman_collection.json`.

The fallback mechanism can be tested by changing the port in the InventoryApi > BaseUrl setting in `appsettings.Development.json` to an invalid value, e.g. "https://localhost:7020".

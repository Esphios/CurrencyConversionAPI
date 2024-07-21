# Currency Conversion Service

## Overview
This project is a currency conversion service designed to automatically convert values between different currencies. The application supports two modes of operation:

1. **On-Demand Conversion**: Converts between a specific pair of currencies.
2. **Bulk Conversion**: Updates stale calculations in the system, potentially processing millions of records daily.

## Features
- **On-Demand Conversion**: Provides quick conversions between specified currency pairs.
- **Bulk Conversion**: Efficiently updates large volumes of records.
- **Caching**: Uses in-memory caching to improve performance.
- **Error Recovery**: Provides fallback mechanisms to ensure outdated conversions are available when up-to-date data is not.

## Requirements
- .NET 6
- Visual Studio (or compatible IDE)
- Access to the European Central Bank (ECB) exchange rate API: `https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml`

## Project Structure
- **CurrencyConversionService**: Contains the core service implementation and related utilities.
- **CurrencyConversionService.Tests**: Contains unit and integration tests for the service.

## Getting Started

### Prerequisites
Ensure you have the following installed:
- .NET 6 SDK
- Visual Studio or another IDE that supports .NET development

### Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/your-repo/currency-conversion-service.git
   cd currency-conversion-service
   ```
2. Restore the NuGet packages:
   ```bash
   dotnet restore
   ```
### Configuration
Update the `appsettings.json` file with the appropriate configuration for the ECB rates URL:
```json
{
  "CurrencyConverter": {
    "EcbRatesUrl": "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```
## Running the Service
1. Open the solution in Visual Studio.
2. Set the `CurrencyConversionService` project as the startup project.
3. Run the project.

### Running the Tests
1. Open the solution in Visual Studio.
2. Build the solution to restore and build the projects.
3. In Test Explorer, run all tests.

### Usage
The service exposes the following endpoints:

- **Convert Currency** (POST `/api/currencyconverter/convert`)
  - Request: A list of `ConversionIn` objects containing `FromCurrency`, `ToCurrency`, and `Amount`.
  - Response: A list of `ConversionOut` objects with the converted amounts.

- **Update Rates** (POST `/api/currencyconverter/update-rates`)
  - This endpoint triggers an update of currency rates from the external source.

### Sample Request
**Convert Currency**:
```json
[
  {
    "FromCurrency": "USD",
    "ToCurrency": "EUR",
    "Amount": 100
  }
]
```
### Update Rates:
- No body required.

### Implementation Notes
- **Caching**: In-memory caching is used to store currency rates for 24 hours to improve performance.
- **Error Handling**: The service includes error handling to provide fallback mechanisms.
- **Dependency Injection**: The service can be registered with an IoC container for easy dependency injection.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

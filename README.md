# Currency Conversion Service

## Overview
This project is a currency conversion service that fetches exchange rates from the European Central Bank and provides methods for converting currencies on-demand and in bulk.

## Setup
1. Open the solution in Visual Studio.
2. Ensure the `CurrencyConversionService` and `CurrencyConversionService.Tests` projects are loaded.

## Running Tests
1. Open the Test Explorer in Visual Studio.
2. Click "Run All" to execute the unit and integration tests.

## Configuration
You can configure the service using the `CurrencyConverterOptions` class. The default ECB rates URL is set to `https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml`.

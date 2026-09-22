FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

COPY ["CurrencyConversionService/CurrencyConversionService.csproj", "CurrencyConversionService/"]
RUN dotnet restore "CurrencyConversionService/CurrencyConversionService.csproj"

COPY . .
WORKDIR /src/CurrencyConversionService
RUN dotnet publish "CurrencyConversionService.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=3 \
    CMD bash -c 'exec 3<>/dev/tcp/127.0.0.1/8080 && printf "GET /health HTTP/1.0\r\nHost: localhost\r\n\r\n" >&3 && IFS= read -r status <&3 && [[ "$status" == *" 200 "* ]]'

ENTRYPOINT ["dotnet", "CurrencyConversionService.dll"]

# AGENTS.md

## Visão Geral

Este repositório contém uma API ASP.NET Core para conversão de moedas e um projeto
de testes xUnit. O fluxo principal recebe pares de moedas, consulta a cotação do
ECB, usa cache em memória e retorna os valores convertidos.

## Tecnologias Principais

| Categoria | Tecnologia | Evidência | Uso |
| --- | --- | --- | --- |
| Runtime | .NET 8 | `CurrencyConversionService/*.csproj` | API e testes |
| Web API | ASP.NET Core | `CurrencyConversionService/Program.cs`, `Startup.cs` | Pipeline HTTP e DI |
| Integração HTTP | `HttpClient` | `Services/CurrencyConverterService.cs` | Consulta ao ECB |
| Cache | `IMemoryCache` | `Startup.cs`, `ServiceCollectionExtensions.cs` | Cache de taxas |
| Serialização | Newtonsoft.Json | `CurrencyConversionService.csproj` | JSON e compatibilidade |
| Documentação | Swashbuckle | `Startup.cs` | Swagger |
| Testes | xUnit, Moq, RichardSzalay.MockHttp | `CurrencyConversionService.Tests/*.csproj` | Testes de controller e service |

## Estrutura do Repositório

- `CurrencyConversionService.sln`: solução principal.
- `CurrencyConversionService/`: API, composition root, controllers, models e services.
- `CurrencyConversionService/Interfaces/`: contratos usados pelos controllers.
- `CurrencyConversionService/Models/Enums/Currency.cs`: lista de moedas aceitas.
- `CurrencyConversionService.Tests/UnitTests/`: cobertura de service e controller.

## Setup e Ambiente

- Configuração principal: `CurrencyConversionService/appsettings.json`.
- Integração externa: `CurrencyConverter:EcbRatesUrl`.
- O cache usa a chave `"Rates"` com janela de 24 horas.
- Não mova configuração para valores hardcoded fora de `appsettings.json` e DI.

## Comandos de Desenvolvimento

```bash
dotnet restore CurrencyConversionService.sln
dotnet build CurrencyConversionService.sln
dotnet test CurrencyConversionService.sln
dotnet run --project .\CurrencyConversionService\CurrencyConversionService.csproj
```

## Convenções e Limites

- Preserve a separação `controller -> interface -> service`.
- Não mova regra de negócio para controller.
- Mudanças na integração com o ECB devem preservar o fallback para cache válido.
- Novas moedas devem ser adicionadas primeiro em `Models/Enums/Currency.cs` e cobertas por testes.
- Alterações no comportamento da API devem ser refletidas nos testes em `CurrencyConversionService.Tests`.

## Peculiaridades do Projeto

- O projeto ainda usa o padrão `Program.cs` + `Startup.cs`, mesmo sendo .NET 8.
- O comportamento correto depende de fallback para dados de cache quando o ECB falha.
- O histórico de commits é informal; não há evidência forte de Conventional Commits.

# Lookify

Biblioteca .NET para consulta de **CEP** e **CNPJ**, com fallback automático entre múltiplos
provedores públicos.

## Recursos

- Consulta de CEP, com fallback entre **ViaCEP** e **BrasilAPI**.
- Consulta de CNPJ, com fallback entre **BrasilAPI**, **ReceitaWS** e **Publica (CNPJ.ws)**.
- Fallback automático: se um provedor falhar, o próximo da lista é tentado, na ordem configurada.
- Cada provedor pode ser habilitado/desabilitado e reordenado individualmente.
- Integração nativa com o padrão de DI do .NET (`IHttpClientFactory`, `IOptions<T>`, `ILogger`).

## Instalação

```bash
dotnet add package Lookify
```

## Uso

Registre os serviços necessários e `LookifyService`:

```csharp
services.AddHttpClient();
services.Configure<LookifyOptions>(options => { /* opcional, ver seção Configuração */ });
services.AddTransient<LookifyService>();
```

`LookifyService` expõe duas propriedades, uma para cada tipo de consulta:

```csharp
public sealed class LookifyService {
    public ICepLookifyService Cep { get; }
    public ICnpjLookifyService Cnpj { get; }
}
```

### Consultar um CEP

```csharp
public sealed class MeuServico(LookifyService lookify) {
    public async Task<string?> ObterCidadeAsync(string cep)
    {
        try {
            var resultado = await lookify.Cep.ConsultAsync(cep);
            return resultado.City;
        }
        catch (Exception ex) {
            // todos os provedores falharam
            return null;
        }
    }
}
```

### Consultar um CNPJ

```csharp
public sealed class MeuServico(LookifyService lookify) {
    public async Task<string?> ObterRazaoSocialAsync(string cnpj)
    {
        try {
            var resultado = await lookify.Cnpj.ConsultAsync(cnpj);
            return resultado.CompanyName;
        }
        catch (Exception ex) {
            // todos os provedores falharam
            return null;
        }
    }
}
```

## Consulta de CEP

```csharp
Task<CepLookifyResultDto> ConsultAsync(string zipCode, CancellationToken cancellationToken = default)
```

O CEP informado é sanitizado (mantendo só dígitos) e precisa resultar em exatamente **8 dígitos**,
senão uma `ArgumentException` é lançada antes de qualquer chamada de rede.

Provedores disponíveis (`CepLookifyProviderEnum`): `ViaCep`, `BrasilApi`.

Campos de `CepLookifyResultDto`:

| Campo          | Tipo      | Descrição                          |
|----------------|-----------|-------------------------------------|
| `ZipCode`      | `string?` | CEP formatado pelo provedor         |
| `Street`       | `string?` | Logradouro                          |
| `Complement`   | `string?` | Complemento                         |
| `Neighborhood` | `string?` | Bairro                              |
| `City`         | `string?` | Cidade                              |
| `State`        | `string?` | UF                                  |
| `IbgeCityCode` | `string?` | Código IBGE do município            |

## Consulta de CNPJ

```csharp
Task<CnpjLookifyResultDto> ConsultAsync(string cnpj, CancellationToken cancellationToken = default)
```

O CNPJ informado é sanitizado (mantendo só dígitos) e precisa resultar em exatamente
**14 dígitos**, senão uma `ArgumentException` é lançada antes de qualquer chamada de rede. Não há
validação de dígito verificador.

Provedores disponíveis (`CnpjLookifyProviderEnum`): `BrasilApi`, `ReceitaWs`, `Publica`.

Campos de `CnpjLookifyResultDto`, agrupados por categoria (nem todo provedor preenche todos os
campos — o que um não retorna, fica `null`):

**Identificação**

| Campo                              | Tipo      |
|-------------------------------------|-----------|
| `Cnpj`                               | `string?` |
| `CompanyName`                        | `string?` |
| `TradeName`                          | `string?` |
| `Phone`                              | `string?` |
| `Email`                              | `string?` |
| `CompanyType`                        | `string?` |
| `HeadquartersOrBranchIdentifier`     | `int?`    |
| `HeadquartersOrBranchDescription`    | `string?` |
| `LegalNatureCode`                    | `string?` |
| `LegalNatureDescription`             | `string?` |
| `ShareCapital`                       | `string?` |
| `CompanySizeCode`                    | `string?` |
| `CompanySizeDescription`             | `string?` |
| `ActivityStartDate`                  | `DateOnly?` |
| `UpdatedAt`                          | `DateTimeOffset?` |

**Situação cadastral**

| Campo                                    | Tipo        |
|--------------------------------------------|-------------|
| `RegistrationStatusCode`                    | `string?`   |
| `RegistrationStatusDescription`             | `string?`   |
| `RegistrationStatusReasonCode`               | `string?`   |
| `RegistrationStatusReasonDescription`        | `string?`   |
| `RegistrationStatusDate`                     | `DateOnly?` |
| `SpecialSituation`                           | `string?`   |
| `SpecialSituationDate`                       | `DateOnly?` |

**Simples Nacional / MEI**

| Campo             | Tipo        |
|--------------------|-------------|
| `IsSimpleOptIn`     | `bool?`     |
| `SimpleOptInDate`   | `DateOnly?` |
| `SimpleOptOutDate`  | `DateOnly?` |
| `IsMeiOptIn`        | `bool?`     |

**CNAE**

| Campo                   | Tipo                              |
|--------------------------|------------------------------------|
| `PrimaryCnaeCode`         | `string?`                          |
| `PrimaryCnaeDescription`  | `string?`                          |
| `SecondaryCnaes`          | `List<CnpjLookifySecondaryCnae>`   |

`CnpjLookifySecondaryCnae`: `Code` (`string?`), `Description` (`string?`).

**Endereço**

| Campo                    | Tipo      |
|---------------------------|-----------|
| `StreetTypeDescription`    | `string?` |
| `Street`                   | `string?` |
| `Number`                   | `string?` |
| `Complement`                | `string?` |
| `Neighborhood`              | `string?` |
| `ZipCode`                   | `string?` |
| `State`                     | `string?` |
| `City`                      | `string?` |
| `ForeignCityName`           | `string?` |
| `Country`                   | `string?` |
| `PrimaryPhoneAreaCode`      | `string?` |
| `SecondaryPhoneAreaCode`    | `string?` |
| `FaxAreaCode`               | `string?` |

**Sócios**

`Partners`: `List<CnpjLookifyPartner>`, com `Identifier`, `Name`, `Document`, `QualificationCode`,
`QualificationDescription`, `EntryDate` (`DateOnly?`), `Country`, `LegalRepresentativeDocument`,
`LegalRepresentativeName`, `LegalRepresentativeQualificationCode`, `AgeGroup` (todos `string?`,
exceto `EntryDate`).

## Configuração (`LookifyOptions`)

```csharp
public sealed class LookifyOptions {
    public string UserAgent { get; set; } = "Lookify/1.0";
    public TimeSpan TimeOut { get; set; } = TimeSpan.FromMinutes(3);
    // ...
}
```

- `UserAgent`: enviado nas requisições aos provedores.
- `TimeOut`: tempo limite das requisições HTTP.
- `CepProviders` / `CnpjProviders`: listas que definem **quais provedores participam e em que
  ordem** o fallback é tentado. Por padrão:
  - CEP: `[ViaCep, BrasilApi]`
  - CNPJ: `[BrasilApi, ReceitaWs, Publica]`
- Uma propriedade `CepLookifyProviderOptions`/`CnpjLookifyProviderOptions` por provedor
  (`ViaCep`, `BrasilApi` para CEP; `CnpjBrasilApi`, `CnpjReceitaWs`, `CnpjPublica` para CNPJ), cada
  uma com `Enabled` (bool) e `BaseAddress` (string) — um provedor desabilitado é pulado mesmo que
  apareça em `CepProviders`/`CnpjProviders`.

### Configurando via código

```csharp
services.Configure<LookifyOptions>(options => {
    options.CnpjPublica.Enabled = false;
    options.CnpjProviders = [
        CnpjLookifyProviderEnum.ReceitaWs,
        CnpjLookifyProviderEnum.BrasilApi
    ];
});
```

### Configurando via `appsettings.json`

```json
{
  "Lookify": {
    "UserAgent": "MinhaApp/1.0",
    "CnpjProviders": ["ReceitaWs", "BrasilApi"],
    "CnpjPublica": { "Enabled": false }
  }
}
```

```csharp
services.Configure<LookifyOptions>(configuration.GetSection("Lookify"));
```

## Comportamento de fallback

Ao consultar, os provedores habilitados são tentados **na ordem definida** em `CepProviders`/
`CnpjProviders`:

1. Se um provedor falhar (erro HTTP, timeout, falha de desserialização, etc.), a falha é logada via
   `ILogger` e o próximo provedor da lista é tentado.
2. O resultado do **primeiro provedor que responder com sucesso** é retornado.
3. Se **todos** os provedores falharem, é lançada uma `InvalidOperationException` agregando as
   falhas de cada um (`AggregateException`).

## Licença

[MIT](LICENSE.txt)

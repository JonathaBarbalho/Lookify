# Lookify

Biblioteca .NET para consulta de **CEP**, **CNPJ** e **placa de veículo**, com fallback automático
entre múltiplos provedores.

## Recursos

- Consulta de CEP, com fallback entre **ViaCEP** e **BrasilAPI**.
- Consulta de CNPJ, com fallback entre **BrasilAPI**, **ReceitaWS** e **Publica (CNPJ.ws)**.
- Consulta de placa de veículo via **PlacaFipe** (provedor pago, exige token).
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

`LookifyService` expõe três propriedades, uma para cada tipo de consulta:

```csharp
public sealed class LookifyService {
    public ICepLookifyService Cep { get; }
    public ICnpjLookifyService Cnpj { get; }
    public IVehiclePlateLookifyService VehiclePlate { get; }
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

### Consultar uma placa

```csharp
public sealed class MeuServico(LookifyService lookify) {
    public async Task<string?> ObterModeloAsync(string placa)
    {
        try {
            var resultado = await lookify.VehiclePlate.ConsultAsync(placa);
            return resultado.Model;
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

## Consulta de Placa

```csharp
Task<VehiclePlateLookifyResultDto> ConsultAsync(string plate, CancellationToken cancellationToken = default)
```

A placa informada é sanitizada (mantendo só letras e dígitos, convertidos para maiúsculas) e
precisa corresponder ao formato antigo (`LLL9999`) ou Mercosul (`LLL9L99`), senão uma
`ArgumentException` é lançada antes de qualquer chamada de rede.

Provedores disponíveis (`VehiclePlateLookifyProviderEnum`): `PlacaFipe`.

> A consulta de placa é um serviço **pago**, fornecido pela plataforma
> [PlacaFipe](https://api.placafipe.com.br/) — é preciso contratar um plano lá para obter o token.
> O token é configurado em `LookifyOptions.PlacaFipe.Token`, por padrão lido da variável de
> ambiente `LOOKIFY_PLACAFIPE_TOKEN`. Nunca commite o token no código ou em `appsettings.json`
> (só em `appsettings.Development.json`, fora do controle de versão — ver "Configurando via
> appsettings.json").

Campos de `VehiclePlateLookifyResultDto`:

| Campo             | Tipo        | Descrição                          |
|--------------------|-------------|--------------------------------------|
| `Plate`            | `string?`   | Placa                                |
| `Brand`            | `string?`   | Marca                                |
| `Model`            | `string?`   | Modelo                               |
| `ManufactureYear`  | `int?`      | Ano de fabricação                    |
| `ModelYear`        | `int?`      | Ano do modelo                        |
| `Color`            | `string?`   | Cor                                  |
| `Chassis`          | `string?`   | Chassi                               |
| `Engine`           | `string?`   | Motor                                |
| `City`             | `string?`   | Município                            |
| `State`            | `string?`   | UF                                   |
| `Segment`          | `string?`   | Segmento do veículo                  |
| `SubSegment`       | `string?`   | Subsegmento do veículo               |
| `Displacement`     | `string?`   | Cilindradas                          |
| `Fuel`             | `string?`   | Combustível                          |
| `FipeMatches`      | `List<VehiclePlateLookifyFipeMatch>` | Correspondências na tabela FIPE |

`VehiclePlateLookifyFipeMatch`:

| Campo             | Tipo        | Descrição                          |
|--------------------|-------------|--------------------------------------|
| `Similarity`       | `decimal?`  | Similaridade com o veículo consultado |
| `Correspondence`   | `decimal?`  | Correspondência com o veículo consultado |
| `Brand`            | `string?`   | Marca                                |
| `Model`            | `string?`   | Modelo                               |
| `ModelYear`        | `string?`   | Ano do modelo                        |
| `FipeCode`         | `string?`   | Código FIPE                          |
| `BrandCode`        | `string?`   | Código da marca                      |
| `ModelCode`        | `string?`   | Código do modelo                     |
| `ReferenceMonth`   | `string?`   | Mês de referência da tabela FIPE     |
| `Fuel`             | `string?`   | Combustível                          |
| `Value`            | `string?`   | Valor FIPE                           |
| `ValueUnit`        | `string?`   | Unidade do valor                     |

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
- `CepProviders` / `CnpjProviders` / `VehiclePlateProviders`: listas que definem **quais
  provedores participam e em que ordem** o fallback é tentado. Por padrão:
  - CEP: `[ViaCep, BrasilApi]`
  - CNPJ: `[BrasilApi, ReceitaWs, Publica]`
  - Placa: `[PlacaFipe]`
- Uma propriedade `CepLookifyProviderOptions`/`CnpjLookifyProviderOptions`/
  `VehiclePlateLookifyProviderOptions` por provedor (`ViaCep`, `BrasilApi` para CEP;
  `CnpjBrasilApi`, `CnpjReceitaWs`, `CnpjPublica` para CNPJ; `PlacaFipe` para placa), cada uma com
  `Enabled` (bool) e `BaseAddress` (string) — um provedor desabilitado é pulado mesmo que apareça
  em `CepProviders`/`CnpjProviders`/`VehiclePlateProviders`. `PlacaFipe` tem ainda `Token` (string),
  lido por padrão da variável de ambiente `LOOKIFY_PLACAFIPE_TOKEN`.

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

Para segredos como o `Token` do `PlacaFipe`, use o padrão de camadas do `appsettings`: mantenha
`appsettings.json` versionado com o campo vazio (documenta a chave) e coloque o valor real em
`appsettings.Development.json` (ou outro `appsettings.{Environment}.json`), **fora do controle de
versão** — é assim que o `LookifyConsoleTester` deste repositório está configurado.

## Comportamento de fallback

Ao consultar, os provedores habilitados são tentados **na ordem definida** em `CepProviders`/
`CnpjProviders`/`VehiclePlateProviders`:

1. Se um provedor falhar (erro HTTP, timeout, falha de desserialização, etc.), a falha é logada via
   `ILogger` e o próximo provedor da lista é tentado.
2. O resultado do **primeiro provedor que responder com sucesso** é retornado.
3. Se **todos** os provedores falharem, é lançada uma `InvalidOperationException` agregando as
   falhas de cada um (`AggregateException`).

## Licença

[MIT](LICENSE.txt)

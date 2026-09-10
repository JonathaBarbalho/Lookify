# Lookify

Biblioteca .NET para consulta de **CEP**, **CNPJ**, **placa de veículo**, **tabela FIPE**,
**localidades do IBGE** e **bancos**, com fallback automático entre múltiplos provedores.

## Recursos

- Consulta de CEP, com fallback entre **ViaCEP**, **BrasilAPI**, **OpenCEP** e **AwesomeAPI**.
- Consulta de CNPJ, com fallback entre **BrasilAPI**, **ReceitaWS**, **Publica (CNPJ.ws)** e
  **MinhaReceita**.
- Consulta de placa de veículo via **PlacaFipe** (provedor pago, exige token — não existe
  alternativa gratuita real no Brasil).
- Consulta da tabela FIPE (tabelas de referência, marcas, modelos, anos e valores de veículos), com
  fallback entre **BrasilAPI** e **Parallelum**.
- Consulta de estados, municípios e regiões do **IBGE**, com fallback entre a API oficial
  (`servicodados.ibge.gov.br`) e o espelho da **BrasilAPI**.
- Consulta de bancos brasileiros (código, ISPB, nome, endereço da sede) via **BrasilAPI**.
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

`LookifyService` expõe uma propriedade para cada tipo de consulta:

```csharp
public sealed class LookifyService {
    public ICepLookifyService Cep { get; }
    public ICnpjLookifyService Cnpj { get; }
    public IVehiclePlateLookifyService VehiclePlate { get; }
    public IFipeLookifyService Fipe { get; }
    public IIbgeLookifyService Ibge { get; }
    public IBankLookifyService Bank { get; }
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

### Consultar a tabela FIPE

```csharp
public sealed class MeuServico(LookifyService lookify) {
    public async Task<string?> ObterValorFipeAsync(
        FipeVehicleType tipo, string codigoMarca, string codigoModelo, string codigoAno)
    {
        try {
            var resultado = await lookify.Fipe.GetVehiclePriceAsync(tipo, codigoMarca, codigoModelo, codigoAno);
            return resultado.Value;
        }
        catch (Exception ex) {
            // todos os provedores falharam
            return null;
        }
    }
}
```

### Consultar localidades do IBGE

```csharp
public sealed class MeuServico(LookifyService lookify) {
    public async Task<List<IbgeCityLookifyResultDto>> ObterMunicipiosAsync(string uf)
    {
        return await lookify.Ibge.GetCitiesByStateAsync(uf);
    }
}
```

### Consultar um banco

```csharp
public sealed class MeuServico(LookifyService lookify) {
    public async Task<string?> ObterNomeDoBancoAsync(int codigoBanco)
    {
        try {
            var resultado = await lookify.Bank.GetBankByCodeAsync(codigoBanco);
            return resultado.Name;
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

Provedores disponíveis (`CepLookifyProviderEnum`): `ViaCep`, `BrasilApi`, `OpenCep`, `AwesomeApi`.

Campos de `CepLookifyResultDto` (nem todo provedor preenche todos os campos — o que um não
retorna, fica `null`):

| Campo          | Tipo       | Descrição                          |
|----------------|------------|-------------------------------------|
| `ZipCode`      | `string?`  | CEP formatado pelo provedor         |
| `Street`       | `string?`  | Logradouro                          |
| `Complement`   | `string?`  | Complemento                         |
| `Neighborhood` | `string?`  | Bairro                              |
| `City`         | `string?`  | Cidade                              |
| `State`        | `string?`  | UF                                  |
| `IbgeCityCode` | `string?`  | Código IBGE do município (preenchido por `BrasilApi`, `OpenCep` e `AwesomeApi`) |
| `Latitude`     | `decimal?` | Latitude (preenchida por `BrasilApi` e `AwesomeApi`) |
| `Longitude`    | `decimal?` | Longitude (preenchida por `BrasilApi` e `AwesomeApi`) |
| `Ddd`          | `string?`  | DDD telefônico (preenchido só por `AwesomeApi`) |

## Consulta de CNPJ

```csharp
Task<CnpjLookifyResultDto> ConsultAsync(string cnpj, CancellationToken cancellationToken = default)
```

O CNPJ informado é sanitizado (mantendo só dígitos) e precisa resultar em exatamente
**14 dígitos**, senão uma `ArgumentException` é lançada antes de qualquer chamada de rede. Não há
validação de dígito verificador.

Provedores disponíveis (`CnpjLookifyProviderEnum`): `BrasilApi`, `ReceitaWs`, `Publica`,
`MinhaReceita`.

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
>
> Diferente de CEP/CNPJ/FIPE/IBGE, este domínio tem só um provedor propositalmente: dado de placa
> (marca/modelo/chassi por placa) é controlado por Detran/Denatran e não existe fonte gratuita e
> pública equivalente no Brasil — qualquer alternativa real também é paga.

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

## Consulta de Tabela FIPE

Diferente de CEP/CNPJ/Placa, a tabela FIPE não é uma consulta por identificador único — é uma API
de catálogo hierárquico (marca → modelo → ano → valor), então `IFipeLookifyService` expõe vários
métodos em vez de um único `ConsultAsync`:

```csharp
Task<List<FipeReferenceTableLookifyResultDto>> GetReferenceTablesAsync(CancellationToken cancellationToken = default);

Task<List<FipeBrandLookifyResultDto>> GetBrandsAsync(FipeVehicleType vehicleType, int? referenceTable = null, CancellationToken cancellationToken = default);

Task<List<FipeModelLookifyResultDto>> GetModelsAsync(FipeVehicleType vehicleType, string brandCode, int? referenceTable = null, CancellationToken cancellationToken = default);

Task<List<FipeModelYearLookifyResultDto>> GetModelYearsAsync(FipeVehicleType vehicleType, string brandCode, string modelCode, int? referenceTable = null, CancellationToken cancellationToken = default);

Task<FipeVehiclePriceLookifyResultDto> GetVehiclePriceAsync(FipeVehicleType vehicleType, string brandCode, string modelCode, string yearCode, int? referenceTable = null, CancellationToken cancellationToken = default);

Task<List<FipeVehiclePriceLookifyResultDto>> GetPriceByFipeCodeAsync(string fipeCode, int? referenceTable = null, CancellationToken cancellationToken = default);
```

`brandCode`/`modelCode`/`yearCode`/`fipeCode` são obrigatórios (não podem ser `null`/vazios) nos
métodos que os recebem, senão uma `ArgumentException` é lançada antes de qualquer chamada de rede.
`referenceTable` é opcional — quando omitido, o provedor usa a tabela de referência mais recente.
`GetPriceByFipeCodeAsync` retorna uma **lista**, pois um mesmo código FIPE pode corresponder a mais
de um ano/modelo.

`FipeVehicleType`: `Cars`, `Motorcycles`, `Trucks`.

Provedores disponíveis (`FipeLookifyProviderEnum`): `BrasilApi`, `Parallelum`.

> `GetPriceByFipeCodeAsync` só é suportado pelo provedor `BrasilApi` — o `Parallelum` não tem um
> endpoint de busca direta por código FIPE (só a cadeia marca→modelo→ano). Se `Parallelum` for o
> provedor corrente na hora de chamar `GetPriceByFipeCodeAsync`, uma `NotSupportedException` é
> lançada para esse provedor especificamente (e o fallback segue para o próximo da lista, se houver).

Campos de `FipeReferenceTableLookifyResultDto`:

| Campo   | Tipo      | Descrição                          |
|---------|-----------|-------------------------------------|
| `Code`  | `int?`    | Código da tabela de referência      |
| `Month` | `string?` | Mês/ano da tabela                   |

Campos de `FipeBrandLookifyResultDto` e `FipeModelLookifyResultDto` (mesmo formato):

| Campo  | Tipo      | Descrição                |
|--------|-----------|---------------------------|
| `Code` | `string?` | Código da marca/modelo    |
| `Name` | `string?` | Nome da marca/modelo      |

Campos de `FipeModelYearLookifyResultDto`:

| Campo   | Tipo      | Descrição                                  |
|---------|-----------|----------------------------------------------|
| `Code`  | `string?` | Código do ano (usado em `GetVehiclePriceAsync`) |
| `Label` | `string?` | Descrição do ano/combustível                |

Campos de `FipeVehiclePriceLookifyResultDto`:

| Campo             | Tipo      | Descrição                          |
|--------------------|-----------|--------------------------------------|
| `FipeCode`         | `string?` | Código FIPE                          |
| `Brand`            | `string?` | Marca                                |
| `Model`            | `string?` | Modelo                               |
| `ModelYear`        | `int?`    | Ano do modelo                        |
| `Fuel`             | `string?` | Combustível                          |
| `FuelAcronym`      | `string?` | Sigla do combustível                 |
| `Value`            | `string?` | Valor FIPE (formatado, ex.: "R$ 31.982,00") |
| `ReferenceMonth`   | `string?` | Mês de referência da tabela FIPE     |
| `VehicleTypeCode`  | `int?`    | Código do tipo de veículo            |
| `RequestDate`      | `string?` | Data/hora da consulta, formatada pelo provedor |

## Consulta de Localidades (IBGE)

Assim como a FIPE, localidades do IBGE são uma consulta de catálogo, não de identificador único:

```csharp
Task<List<IbgeStateLookifyResultDto>> GetStatesAsync(CancellationToken cancellationToken = default);

Task<IbgeStateLookifyResultDto> GetStateAsync(string uf, CancellationToken cancellationToken = default);

Task<List<IbgeCityLookifyResultDto>> GetCitiesByStateAsync(string uf, CancellationToken cancellationToken = default);

Task<List<IbgeCityLookifyResultDto>> GetAllCitiesAsync(CancellationToken cancellationToken = default);

Task<List<IbgeRegionLookifyResultDto>> GetRegionsAsync(CancellationToken cancellationToken = default);
```

`uf` é sanitizado (maiúsculas, sem espaços) e precisa conter exatamente **2 letras**, senão uma
`ArgumentException` é lançada antes de qualquer chamada de rede.

Provedores disponíveis (`IbgeLookifyProviderEnum`): `Ibge` (API oficial `servicodados.ibge.gov.br`),
`BrasilApi` (espelho da mesma base).

> `GetAllCitiesAsync` (todos os ~5.570 municípios do Brasil numa única chamada, sem filtro de UF)
> só é suportado pelo provedor `Ibge` — a `BrasilApi` só lista município por UF, nunca todos de uma
> vez. Se `BrasilApi` for o provedor corrente, uma `NotSupportedException` é lançada para esse
> provedor especificamente. Além disso, o retorno de `GetCitiesByStateAsync`/`GetAllCitiesAsync` via
> `BrasilApi` é mais raso que o oficial: só `Id`/`Name`/`StateUf` vêm preenchidos (sem
> `StateId`/`StateName`/`RegionId`/`RegionName`/`RegionAcronym`), porque a BrasilAPI não devolve a
> cadeia microrregião→mesorregião→UF que o IBGE oficial devolve.

Campos de `IbgeStateLookifyResultDto`:

| Campo           | Tipo      | Descrição            |
|------------------|-----------|------------------------|
| `Id`             | `int?`    | Código IBGE do estado  |
| `Name`           | `string?` | Nome do estado         |
| `Uf`             | `string?` | Sigla (UF)             |
| `RegionId`       | `int?`    | Código da região       |
| `RegionName`     | `string?` | Nome da região         |
| `RegionAcronym`  | `string?` | Sigla da região        |

Campos de `IbgeCityLookifyResultDto`:

| Campo           | Tipo      | Descrição            |
|------------------|-----------|------------------------|
| `Id`             | `int?`    | Código IBGE do município |
| `Name`           | `string?` | Nome do município      |
| `StateId`        | `int?`    | Código IBGE do estado  |
| `StateUf`        | `string?` | Sigla do estado (UF)   |
| `StateName`      | `string?` | Nome do estado         |
| `RegionId`       | `int?`    | Código da região       |
| `RegionName`     | `string?` | Nome da região         |
| `RegionAcronym`  | `string?` | Sigla da região        |

Campos de `IbgeRegionLookifyResultDto`:

| Campo      | Tipo      | Descrição            |
|------------|-----------|------------------------|
| `Id`       | `int?`    | Código IBGE da região  |
| `Name`     | `string?` | Nome da região (ex.: "Sudeste") |
| `Acronym`  | `string?` | Sigla da região (ex.: "SE")     |

## Consulta de Bancos

Assim como FIPE e IBGE, bancos são uma consulta de catálogo (listar todos, ou um pelo código),
não de identificador único:

```csharp
Task<List<BankLookifyResultDto>> GetAllBanksAsync(CancellationToken cancellationToken = default);

Task<BankLookifyResultDto> GetBankByCodeAsync(int code, CancellationToken cancellationToken = default);
```

`code` é o código numérico de compensação do banco (ex.: `1` para o Banco do Brasil) — não é o
ISPB. Nem todo banco tem código de compensação (alguns, como "Selic" e "Bacen", aparecem só com
ISPB); nesses casos `GetBankByCodeAsync` não encontra o registro.

Provedores disponíveis (`BankLookifyProviderEnum`): `BrasilApi`.

Campos de `BankLookifyResultDto`:

| Campo         | Tipo      | Descrição                          |
|----------------|-----------|-------------------------------------|
| `Code`         | `int?`    | Código de compensação (pode ser `null`) |
| `Ispb`         | `string?` | Código ISPB (identificador no SPB)  |
| `Name`         | `string?` | Nome (curto)                        |
| `FullName`     | `string?` | Nome completo                       |
| `Cnpj`         | `string?` | CNPJ da instituição                 |
| `Street`       | `string?` | Logradouro da sede                  |
| `Number`       | `string?` | Número da sede                      |
| `Complement`   | `string?` | Complemento da sede                 |
| `District`     | `string?` | Bairro da sede                      |
| `City`         | `string?` | Cidade da sede                      |
| `State`        | `string?` | UF da sede                          |
| `ZipCode`      | `string?` | CEP da sede                         |
| `LogoUrl`      | `string?` | URL do logo do banco                |

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
- `CepProviders` / `CnpjProviders` / `VehiclePlateProviders` / `FipeProviders` / `IbgeProviders` /
  `BankProviders`: listas que definem **quais provedores participam e em que ordem** o fallback é
  tentado. Por padrão:
  - CEP: `[ViaCep, BrasilApi, OpenCep, AwesomeApi]`
  - CNPJ: `[BrasilApi, ReceitaWs, Publica, MinhaReceita]`
  - Placa: `[PlacaFipe]`
  - FIPE: `[BrasilApi, Parallelum]`
  - IBGE: `[Ibge, BrasilApi]`
  - Bancos: `[BrasilApi]`
- Uma propriedade `CepLookifyProviderOptions`/`CnpjLookifyProviderOptions`/
  `VehiclePlateLookifyProviderOptions`/`FipeLookifyProviderOptions`/`IbgeLookifyProviderOptions`/
  `BankLookifyProviderOptions` por provedor (`ViaCep`, `BrasilApi`, `CepOpenCep`, `CepAwesomeApi`
  para CEP; `CnpjBrasilApi`, `CnpjReceitaWs`, `CnpjPublica`, `CnpjMinhaReceita` para CNPJ;
  `PlacaFipe` para placa; `FipeBrasilApi`, `FipeParallelum` para FIPE; `Ibge`, `IbgeBrasilApi` para
  localidades; `BankBrasilApi` para bancos), cada uma com `Enabled` (bool) e `BaseAddress` (string)
  — um provedor desabilitado é pulado mesmo que apareça em `CepProviders`/`CnpjProviders`/
  `VehiclePlateProviders`/`FipeProviders`/`IbgeProviders`/`BankProviders`. `PlacaFipe` tem ainda
  `Token` (string), lido por padrão da variável de ambiente `LOOKIFY_PLACAFIPE_TOKEN`.

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
`CnpjProviders`/`VehiclePlateProviders`/`FipeProviders`/`IbgeProviders`/`BankProviders`:

1. Se um provedor falhar (erro HTTP, timeout, falha de desserialização, etc.), a falha é logada via
   `ILogger` e o próximo provedor da lista é tentado.
2. O resultado do **primeiro provedor que responder com sucesso** é retornado.
3. Se **todos** os provedores falharem, é lançada uma `InvalidOperationException` agregando as
   falhas de cada um (`AggregateException`).

## Licença

[MIT](LICENSE.txt)

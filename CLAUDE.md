# Lookify — Instruções do projeto

## Versionamento e empacotamento a cada commit

Sempre que um commit for feito neste repositório, antes de finalizar o commit:

1. **Incrementar a versão** em `Lookify/Lookify.csproj` (propriedade `<Version>`), usando **patch
   automático** (ex.: `1.1.0` → `1.1.1`) — sem perguntar. Só usar minor ou major se eu pedir isso
   explicitamente na mensagem do pedido (ex.: "esse commit é minor" ou "sobe pra 2.0.0").
2. **Gerar o pacote NuGet** dessa nova versão, rodando:
   ```bash
   dotnet pack Lookify/Lookify.csproj -c Release
   ```
   O `.nupkg`/`.snupkg` resultante fica em `Lookify/bin/Release/` — isso só **compila o pacote
   localmente**, não publica no NuGet.org. Publicar (`dotnet nuget push`) é uma ação separada e
   sempre exige pedido explícito meu, mesmo com o commit já feito.
3. Incluir o bump de versão no próprio commit (mesmo diff), não como commit separado.

Isso vale para qualquer commit no repositório, não só mudanças na lib `Lookify` em si. A
autorização de commit continua seguindo a regra global (só commitar com autorização explícita da
minha mensagem atual) — este processo roda como parte do commit já autorizado, não dispensa a
autorização.

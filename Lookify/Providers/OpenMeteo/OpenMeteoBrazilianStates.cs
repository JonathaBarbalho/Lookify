namespace Lookify.Providers.OpenMeteo;

internal static class OpenMeteoBrazilianStates {

    private static readonly Dictionary<string, string> NamesByUf = new(StringComparer.OrdinalIgnoreCase) {
        ["AC"] = "Acre",
        ["AL"] = "Alagoas",
        ["AP"] = "Amapá",
        ["AM"] = "Amazonas",
        ["BA"] = "Bahia",
        ["CE"] = "Ceará",
        ["DF"] = "Distrito Federal",
        ["ES"] = "Espírito Santo",
        ["GO"] = "Goiás",
        ["MA"] = "Maranhão",
        ["MT"] = "Mato Grosso",
        ["MS"] = "Mato Grosso do Sul",
        ["MG"] = "Minas Gerais",
        ["PA"] = "Pará",
        ["PB"] = "Paraíba",
        ["PR"] = "Paraná",
        ["PE"] = "Pernambuco",
        ["PI"] = "Piauí",
        ["RJ"] = "Rio de Janeiro",
        ["RN"] = "Rio Grande do Norte",
        ["RS"] = "Rio Grande do Sul",
        ["RO"] = "Rondônia",
        ["RR"] = "Roraima",
        ["SC"] = "Santa Catarina",
        ["SP"] = "São Paulo",
        ["SE"] = "Sergipe",
        ["TO"] = "Tocantins"
    };

    public static string? GetName(
        string? uf) =>
        uf is not null && NamesByUf.TryGetValue(uf, out var name) ? name : null;

    public static string? GetUf(
        string? name)
    {
        if (name is null) {
            return null;
        }

        foreach (var (uf, ufName) in NamesByUf) {
            if (string.Equals(ufName, name, StringComparison.OrdinalIgnoreCase)) {
                return uf;
            }
        }

        return null;
    }
}

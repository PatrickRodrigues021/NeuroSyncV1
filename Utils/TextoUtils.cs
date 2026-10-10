namespace NeuroSync.Utils;

public static class TextoUtils
{
    /// <summary>
    /// Extrai o primeiro nome ou título + nome (ex: "Dra. Mariana", "Rodrigo")
    /// </summary>
    public static string ExtrairPrimeiroNome(string? nomeCompleto)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto)) return "Usuário";
        var partes = nomeCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return "Usuário";

        string[] titulos = ["dr.", "dra.", "dr", "dra", "prof.", "profa.", "prof", "profa"];
        return titulos.Contains(partes[0].ToLower()) && partes.Length > 1
            ? $"{partes[0]} {partes[1]}"
            : partes[0];
    }
}
namespace NeuroSync.Utils;

public static class AgendaUtils
{
    /// <summary>
    /// Calcula o próximo horário sugerido com base no parâmetro informado ou na hora do dia
    /// </summary>
    public static DateTime ObterDataSugerida(string? dataInformadaStr)
    {
        var agora = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(dataInformadaStr) && DateTime.TryParse(dataInformadaStr, out var dataInformada))
        {
            return dataInformada;
        }

        if (agora.Hour >= 8 && agora.Hour < 18)
        {
            return DateTime.Today.AddHours(agora.Hour + 1);
        }

        var dia = agora.Hour >= 18 ? DateTime.Today.AddDays(1) : DateTime.Today;
        return dia.AddHours(9);
    }
}
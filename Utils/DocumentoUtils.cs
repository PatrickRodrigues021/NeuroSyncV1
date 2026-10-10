using System.IO;

namespace NeuroSync.Utils;

public static class DocumentoUtils
{
    /// <summary>
    /// Remove caracteres inválidos para nomeação segura de arquivos exportados
    /// </summary>
    public static string SanitizarNomeArquivo(string nomeOriginal)
    {
        if (string.IsNullOrWhiteSpace(nomeOriginal)) return "documento";
        var invalidos = Path.GetInvalidFileNameChars();
        var partes = nomeOriginal.Split(invalidos, StringSplitOptions.RemoveEmptyEntries);
        return string.Join("_", partes).Replace(" ", "_");
    }
}
namespace NeuroSync.Models;

/// <summary>
/// Modelo de exibição para a página de erro não tratado da aplicação.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Identificador único da requisição com erro para diagnóstico e rastreamento.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Indica se o identificador da requisição deve ser visível na página.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

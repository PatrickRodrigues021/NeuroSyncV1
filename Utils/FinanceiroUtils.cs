namespace NeuroSync.Utils;

public static class FinanceiroUtils
{
    public static string ObterCorCategoria(string cat) => cat switch
    {
        "Energia Elétrica" => "#F59E0B",
        "Água" => "#06B6D4",
        "Telefonia" => "#8B5CF6",
        "Internet" => "#3B82F6",
        "Material de Escritório" => "#EC4899",
        "Sistemas" => "#6366F1",
        "Aluguel" => "#10B981",
        "Limpeza" => "#14B8A6",
        _ => "#64748B"
    };

    public static string ObterIconeCategoria(string cat) => cat switch
    {
        "Energia Elétrica" => "bi-lightning-charge-fill",
        "Água" => "bi-droplet-fill",
        "Telefonia" => "bi-telephone-fill",
        "Internet" => "bi-wifi",
        "Material de Escritório" => "bi-box-seam-fill",
        "Sistemas" => "bi-laptop",
        "Aluguel" => "bi-building",
        "Limpeza" => "bi-stars",
        _ => "bi-receipt"
    };
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Representa um anexo, laudo, exame ou relatório digitalizado arquivado no prontuário do paciente.
/// Mapeada para a tabela 'anexo' do banco de dados SQLite.
/// </summary>
[Table("anexo")]
public class Anexo
{
    [Key]
    [Column("id_anexo")]
    public int IdAnexo { get; set; }

    /// <summary>
    /// Chave estrangeira para o paciente proprietário do documento.
    /// </summary>
    [Required]
    [Column("id_paciente")]
    public int PacienteId { get; set; }
    
    [ForeignKey("PacienteId")]
    public Paciente? Paciente { get; set; }

    /// <summary>
    /// Nome original do arquivo anexado.
    /// </summary>
    [Required]
    [Column("nome_arquivo")]
    public string NomeArquivo { get; set; } = string.Empty;

    /// <summary>
    /// Caminho relativo onde o arquivo físico foi armazenado no servidor/disco.
    /// </summary>
    [Required]
    [Column("caminho_arquivo")]
    public string CaminhoArquivo { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora da realização do upload.
    /// </summary>
    [Column("data_upload")]
    public DateTime DataUpload { get; set; } = DateTime.Now;
}
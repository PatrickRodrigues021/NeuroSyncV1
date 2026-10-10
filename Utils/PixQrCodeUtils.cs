using System.Globalization;
using System.Text;
using QRCoder;

namespace NeuroSync.Utils;

public static class PixQrCodeUtils
{
    /// <summary>
    /// Gera a string do código Pix Copia e Cola no padrão do Banco Central (EMV QRCPS)
    /// </summary>
    public static string GerarPixCopiaECola(string chavePix, string nomeRecebedor, string cidade, decimal valor, string? txId = null)
    {
        txId = string.IsNullOrWhiteSpace(txId) ? "***" : txId;
        
        var payload = new StringBuilder();
        payload.Append("000201"); // Payload Format Indicator
        
        // Merchant Account Information (Pix)
        var gui = "0014br.gov.bcb.pix";
        var chave = $"01{chavePix.Length:D2}{chavePix}";
        var merchantAccount = $"{gui}{chave}";
        payload.Append($"26{merchantAccount.Length:D2}{merchantAccount}");

        payload.Append("52040000"); // Merchant Category Code
        payload.Append("5303986");  // Transaction Currency (BRL)
        
        string valStr = valor.ToString("F2", CultureInfo.InvariantCulture);
        payload.Append($"54{valStr.Length:D2}{valStr}"); // Transaction Amount
        
        payload.Append("5802BR"); // Country Code

        string nome = RemoverAcentos(nomeRecebedor.Length > 25 ? nomeRecebedor[..25] : nomeRecebedor);
        payload.Append($"59{nome.Length:D2}{nome}"); // Merchant Name

        string cid = RemoverAcentos(cidade.Length > 15 ? cidade[..15] : cidade);
        payload.Append($"60{cid.Length:D2}{cid}"); // Merchant City

        // Additional Data Field Template (TxID)
        var additionalData = $"05{txId.Length:D2}{txId}";
        payload.Append($"62{additionalData.Length:D2}{additionalData}");

        payload.Append("6304"); // CRC16 Header

        string crc = CalcularCRC16(payload.ToString());
        payload.Append(crc);

        return payload.ToString();
    }

    /// <summary>
    /// Converte a string Pix para uma imagem Base64 do QR Code usando a biblioteca QRCoder
    /// </summary>
    public static string GerarQrCodeBase64(string textoPayload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(textoPayload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        byte[] qrCodeBytes = qrCode.GetGraphic(20);
        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
    }

    private static string RemoverAcentos(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        var normalizedString = texto.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }

    private static string CalcularCRC16(string payload)
    {
        ushort crc = 0xFFFF;
        byte[] bytes = Encoding.UTF8.GetBytes(payload);

        foreach (byte b in bytes)
        {
            crc ^= (ushort)(b << 8);
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ 0x1021);
                else
                    crc = (ushort)(crc << 1);
            }
        }
        return crc.ToString("X4");
    }
}
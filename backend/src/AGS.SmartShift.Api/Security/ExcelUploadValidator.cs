namespace AGS.SmartShift.Api.Security;

public static class ExcelUploadValidator
{
    private static readonly byte[] ZipLocalHeader = [0x50, 0x4B, 0x03, 0x04];
    private static readonly byte[] OleCompoundHeader = [0xD0, 0xCF, 0x11, 0xE0];

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-excel",
        "application/octet-stream",
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".xlsx",
        ".xls",
    };

    public static bool TryValidate(IFormFile file, out string? error)
    {
        error = null;

        if (file.Length == 0)
        {
            error = "File Excel trống.";
            return false;
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            error = "Chỉ chấp nhận file .xlsx hoặc .xls.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(file.ContentType) && !AllowedContentTypes.Contains(file.ContentType))
        {
            error = "Định dạng file không hợp lệ.";
            return false;
        }

        if (!HasRecognizedMagicHeader(file))
        {
            error = "Nội dung file không khớp định dạng Excel.";
            return false;
        }

        return true;
    }

    private static bool HasRecognizedMagicHeader(IFormFile file)
    {
        Span<byte> header = stackalloc byte[4];
        using var stream = file.OpenReadStream();
        var read = stream.Read(header);
        if (read < 4)
        {
            return false;
        }

        if (header.SequenceEqual(ZipLocalHeader))
        {
            return true;
        }

        return header.SequenceEqual(OleCompoundHeader);
    }
}

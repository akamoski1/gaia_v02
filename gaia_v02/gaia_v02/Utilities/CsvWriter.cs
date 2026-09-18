using System.Text;

namespace gaia_v02.Utilities;

/// <summary>
/// Simple CSV writer with proper field escaping for commas, quotes, and newlines.
/// </summary>
public sealed class CsvWriter
{
    private readonly List<string> _lines = [];

    /// <summary>Writes a header row with field names.</summary>
    public void WriteHeader(params string[] headers)
    {
        var escaped = headers.Select(EscapeField).ToArray();
        _lines.Add(string.Join(",", escaped));
    }

    /// <summary>Writes a data row with values.</summary>
    public void WriteRecord(params object?[] values)
    {
        var escaped = values.Select(v => EscapeField(v?.ToString() ?? "")).ToArray();
        _lines.Add(string.Join(",", escaped));
    }

    /// <summary>Returns all written lines as a read-only list.</summary>
    public IReadOnlyList<string> GetLines() => _lines.AsReadOnly();

    /// <summary>
    /// Escapes a field value for CSV: wraps in quotes and doubles internal quotes
    /// if the field contains comma, quote, or newline characters.
    /// </summary>
    private static string EscapeField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return field;

        // If field contains comma, quote, or newline, wrap in quotes and escape internal quotes
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}

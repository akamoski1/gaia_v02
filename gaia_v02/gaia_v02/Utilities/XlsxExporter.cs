using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace gaia_v02.Utilities;

/// <summary>
/// Pure .NET XLSX exporter using only standard libraries (no NuGet dependencies).
/// Creates Office Open XML files that can be opened in Excel and other spreadsheet applications.
/// </summary>
public static class XlsxExporter
{
    /// <summary>
    /// Creates an XLSX file from CSV lines (header + data rows).
    /// </summary>
    /// <param name="csvFilePath">Path to the source CSV file</param>
    /// <param name="xlsxFilePath">Path where the XLSX file will be created</param>
    /// <param name="sheetName">Name of the worksheet</param>
    public static void ConvertCsvToXlsx(string csvFilePath, string xlsxFilePath, string sheetName = "Data")
    {
        if (!File.Exists(csvFilePath))
            throw new FileNotFoundException($"CSV file not found: {csvFilePath}");

        // Parse CSV file
        var lines = File.ReadAllLines(csvFilePath);
        if (lines.Length == 0)
            throw new ArgumentException("CSV file is empty");

        // Extract headers from first line (already CSV-safe)
        var headers = ParseCsvLine(lines[0]);

        // Extract data rows
        var rows = new List<object[]>();
        for (int i = 1; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i]);

            // Convert to appropriate types
            var row = new object[fields.Count];
            for (int j = 0; j < fields.Count; j++)
            {
                row[j] = ConvertField(fields[j]);
            }
            rows.Add(row);
        }

        // Create XLSX
        CreateXlsx(xlsxFilePath, sheetName, headers.ToArray(), rows);
    }

    /// <summary>
    /// Creates an XLSX file with given headers and data rows.
    /// </summary>
    public static void CreateXlsx(
        string fileName,
        string sheetName,
        string[] headers,
        IEnumerable<object[]> rows)
    {
        using FileStream file = File.Create(fileName);

        using ZipArchive zip = new(
            file,
            ZipArchiveMode.Create);

        WriteContentTypes(zip);
        WriteRootRelationships(zip);
        WriteWorkbook(zip, sheetName);
        WriteWorkbookRelationships(zip);
        WriteStyles(zip);
        WriteWorksheet(zip, sheetName, headers, rows);
    }

    // -----------------------------------------------------------------------
    // Private Implementation Methods
    // -----------------------------------------------------------------------

    /// <summary>
    /// Parse a single CSV line, respecting RFC 4180 quoting rules.
    /// </summary>
    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Escaped quote ("" becomes ")
                    current.Append('"');
                    i++; // Skip next quote
                }
                else
                {
                    // Toggle quote state
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                // End of field
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        // Add final field
        fields.Add(current.ToString());

        return fields;
    }

    /// <summary>
    /// Convert a CSV field value to appropriate .NET type.
    /// </summary>
    private static object ConvertField(string field)
    {
        if (string.IsNullOrWhiteSpace(field))
            return "";

        field = field.Trim();

        // Try integer
        if (int.TryParse(field, NumberStyles.Any, CultureInfo.InvariantCulture, out int intVal))
            return intVal;

        // Try decimal
        if (decimal.TryParse(field, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decVal))
            return decVal;

        // Try boolean
        if (bool.TryParse(field, out bool boolVal))
            return boolVal;

        // Try date/time
        if (DateTime.TryParse(field, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dateVal))
            return dateVal;

        // Default to string
        return field;
    }

    private static void WriteContentTypes(ZipArchive zip)
    {
        const string xml = """
<?xml version="1.0" encoding="UTF-8"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels"
           ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml"
           ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml"
            ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml"
            ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/styles.xml"
            ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
</Types>
""";

        WriteEntry(zip, "[Content_Types].xml", xml);
    }

    private static void WriteRootRelationships(ZipArchive zip)
    {
        const string xml = """
<?xml version="1.0" encoding="UTF-8"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship
      Id="rId1"
      Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"
      Target="xl/workbook.xml"/>
</Relationships>
""";

        WriteEntry(zip, "_rels/.rels", xml);
    }

    private static void WriteWorkbook(
        ZipArchive zip,
        string sheetName)
    {
        string escapedName = EscapeAttribute(sheetName);

        string xml = $"""
<?xml version="1.0" encoding="UTF-8"?>
<workbook
    xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
    xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">

  <sheets>
    <sheet name="{escapedName}"
           sheetId="1"
           r:id="rId1"/>
  </sheets>

</workbook>
""";

        WriteEntry(zip, "xl/workbook.xml", xml);
    }

    private static void WriteWorkbookRelationships(ZipArchive zip)
    {
        const string xml = """
<?xml version="1.0" encoding="UTF-8"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">

  <Relationship
      Id="rId1"
      Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"
      Target="worksheets/sheet1.xml"/>

  <Relationship
      Id="rId2"
      Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"
      Target="styles.xml"/>

</Relationships>
""";

        WriteEntry(zip, "xl/_rels/workbook.xml.rels", xml);
    }

    private static void WriteStyles(ZipArchive zip)
    {
        const string xml = """
<?xml version="1.0" encoding="UTF-8"?>
<styleSheet
    xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">

  <fonts count="2">

    <font>
      <sz val="11"/>
      <name val="Calibri"/>
    </font>

    <font>
      <b/>
      <sz val="11"/>
      <name val="Calibri"/>
    </font>

  </fonts>

  <fills count="3">

    <fill>
      <patternFill patternType="none"/>
    </fill>

    <fill>
      <patternFill patternType="gray125"/>
    </fill>

    <fill>
      <patternFill patternType="solid">
        <fgColor rgb="D9EAF7"/>
        <bgColor indexed="64"/>
      </patternFill>
    </fill>

  </fills>

  <borders count="2">

    <border>
      <left/>
      <right/>
      <top/>
      <bottom/>
      <diagonal/>
    </border>

    <border>
      <left style="thin"/>
      <right style="thin"/>
      <top style="thin"/>
      <bottom style="thin"/>
      <diagonal/>
    </border>

  </borders>

  <cellStyleXfs count="1">
    <xf numFmtId="0"
        fontId="0"
        fillId="0"
        borderId="0"/>
  </cellStyleXfs>

  <cellXfs count="2">

    <xf numFmtId="0"
        fontId="0"
        fillId="0"
        borderId="1"
        xfId="0"/>

    <xf numFmtId="0"
        fontId="1"
        fillId="2"
        borderId="1"
        xfId="0"
        applyFont="1"
        applyFill="1"/>

  </cellXfs>

</styleSheet>
""";

        WriteEntry(zip, "xl/styles.xml", xml);
    }

    private static void WriteWorksheet(
        ZipArchive zip,
        string sheetName,
        string[] headers,
        IEnumerable<object[]> rows)
    {
        using MemoryStream stream = new();

        using (XmlWriter writer = XmlWriter.Create(
            stream,
            new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = true
            }))
        {
            writer.WriteStartDocument();

            writer.WriteStartElement(
                "worksheet",
                "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

            // Freeze the header row.
            writer.WriteStartElement("sheetViews");
            writer.WriteStartElement("sheetView");
            writer.WriteAttributeString("workbookViewId", "0");

            writer.WriteStartElement("pane");
            writer.WriteAttributeString("ySplit", "1");
            writer.WriteAttributeString("topLeftCell", "A2");
            writer.WriteAttributeString("activePane", "bottomLeft");
            writer.WriteAttributeString("state", "frozen");
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndElement();

            // Column widths.
            writer.WriteStartElement("cols");

            for (int i = 0; i < headers.Length; i++)
            {
                writer.WriteStartElement("col");
                writer.WriteAttributeString("min", (i + 1).ToString());
                writer.WriteAttributeString("max", (i + 1).ToString());
                writer.WriteAttributeString("width", "18");
                writer.WriteAttributeString("customWidth", "1");
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            writer.WriteStartElement("sheetData");

            // Header.
            writer.WriteStartElement("row");
            writer.WriteAttributeString("r", "1");

            for (int i = 0; i < headers.Length; i++)
            {
                WriteStringCell(
                    writer,
                    CellReference(i + 1, 1),
                    headers[i],
                    1);
            }

            writer.WriteEndElement();

            int rowNumber = 2;

            foreach (object[] row in rows)
            {
                writer.WriteStartElement("row");
                writer.WriteAttributeString(
                    "r",
                    rowNumber.ToString());

                for (int i = 0; i < row.Length; i++)
                {
                    WriteCell(
                        writer,
                        CellReference(i + 1, rowNumber),
                        row[i]);
                }

                writer.WriteEndElement();

                rowNumber++;
            }

            writer.WriteEndElement();

            // AutoFilter covering the entire data set.
            int lastRow = rowNumber - 1;

            writer.WriteStartElement("autoFilter");
            writer.WriteAttributeString(
                "ref",
                $"A1:{ColumnName(headers.Length)}{lastRow}");
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        stream.Position = 0;

        ZipArchiveEntry entry =
            zip.CreateEntry("xl/worksheets/sheet1.xml");

        using Stream output = entry.Open();
        stream.CopyTo(output);
    }

    private static void WriteCell(
        XmlWriter writer,
        string reference,
        object? value)
    {
        if (value == null)
        {
            WriteStringCell(writer, reference, "");
            return;
        }

        if (value is string text)
        {
            WriteStringCell(writer, reference, text);
            return;
        }

        if (value is DateTime date)
        {
            WriteStringCell(
                writer,
                reference,
                date.ToString(
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture));

            return;
        }

        if (value is bool boolean)
        {
            writer.WriteStartElement("c");
            writer.WriteAttributeString("r", reference);
            writer.WriteAttributeString("t", "b");

            writer.WriteStartElement("v");
            writer.WriteString(boolean ? "1" : "0");
            writer.WriteEndElement();

            writer.WriteEndElement();
            return;
        }

        if (value is IFormattable formattable)
        {
            writer.WriteStartElement("c");
            writer.WriteAttributeString("r", reference);

            writer.WriteStartElement("v");
            writer.WriteString(
                formattable.ToString(
                    null,
                    CultureInfo.InvariantCulture));

            writer.WriteEndElement();
            writer.WriteEndElement();

            return;
        }

        WriteStringCell(
            writer,
            reference,
            value.ToString() ?? "");
    }

    private static void WriteStringCell(
        XmlWriter writer,
        string reference,
        string value,
        int style = 0)
    {
        writer.WriteStartElement("c");
        writer.WriteAttributeString("r", reference);
        writer.WriteAttributeString("t", "inlineStr");

        if (style != 0)
        {
            writer.WriteAttributeString(
                "s",
                style.ToString());
        }

        writer.WriteStartElement("is");
        writer.WriteStartElement("t");

        // Preserve leading/trailing spaces.
        if (value.StartsWith(" ") ||
            value.EndsWith(" "))
        {
            writer.WriteAttributeString(
                "xml:space",
                "preserve");
        }

        writer.WriteString(value);

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static string CellReference(
        int column,
        int row)
    {
        return $"{ColumnName(column)}{row}";
    }

    private static string ColumnName(int column)
    {
        StringBuilder result = new();

        while (column > 0)
        {
            column--;

            result.Insert(
                0,
                (char)('A' + column % 26));

            column /= 26;
        }

        return result.ToString();
    }

    private static string EscapeAttribute(string value)
    {
        using StringWriter stringWriter = new();

        using XmlWriter writer = XmlWriter.Create(
            stringWriter,
            new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            });

        writer.WriteStartElement("x");
        writer.WriteAttributeString("value", value);
        writer.WriteEndElement();
        writer.Flush();

        string result = stringWriter.ToString();

        int start = result.IndexOf("value=\"") + 7;
        int end = result.LastIndexOf("\"");

        return result.Substring(start, end - start);
    }

    private static void WriteEntry(
        ZipArchive zip,
        string path,
        string content)
    {
        ZipArchiveEntry entry = zip.CreateEntry(path);

        using Stream stream = entry.Open();
        using StreamWriter writer = new(
            stream,
            new UTF8Encoding(false));

        writer.Write(content);
    }
}

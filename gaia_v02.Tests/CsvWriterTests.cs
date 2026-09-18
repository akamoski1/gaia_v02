using Xunit;
using gaia_v02.Utilities;

namespace gaia_v02.Tests;

public class CsvWriterTests
{
    [Fact]
    public void WriteHeader_SingleColumn_CorrectFormat()
    {
        var writer = new CsvWriter();
        writer.WriteHeader("Name");
        var lines = writer.GetLines();

        Assert.Single(lines);
        Assert.Equal("Name", lines[0]);
    }

    [Fact]
    public void WriteHeader_MultipleColumns_CorrectFormat()
    {
        var writer = new CsvWriter();
        writer.WriteHeader("Name", "Age", "City");
        var lines = writer.GetLines();

        Assert.Single(lines);
        Assert.Equal("Name,Age,City", lines[0]);
    }

    [Fact]
    public void WriteRecord_SimpleValues_CorrectFormat()
    {
        var writer = new CsvWriter();
        writer.WriteHeader("Name", "Age");
        writer.WriteRecord("Alice", 30);
        var lines = writer.GetLines();

        Assert.Equal(2, lines.Count);
        Assert.Equal("Name,Age", lines[0]);
        Assert.Equal("Alice,30", lines[1]);
    }

    [Fact]
    public void WriteRecord_ValueWithComma_ProperlyEscaped()
    {
        var writer = new CsvWriter();
        writer.WriteHeader("Name", "Address");
        writer.WriteRecord("Smith, John", "Main St");
        var lines = writer.GetLines();

        Assert.Equal(2, lines.Count);
        // Field with comma should be quoted
        Assert.Equal("\"Smith, John\",Main St", lines[1]);
    }

    [Fact]
    public void WriteRecord_ValueWithQuote_ProperlyEscaped()
    {
        var writer = new CsvWriter();
        writer.WriteHeader("Name", "Quote");
        writer.WriteRecord("Alice", "He said \"Hello\"");
        var lines = writer.GetLines();

        Assert.Equal(2, lines.Count);
        // Quote should be doubled and field quoted
        Assert.Equal("Alice,\"He said \"\"Hello\"\"\"", lines[1]);
    }

    [Fact]
    public void WriteRecord_ValueWithNewline_ProperlyEscaped()
    {
        var writer = new CsvWriter();
        writer.WriteHeader("Name", "Notes");
        writer.WriteRecord("Bob", "Line1\nLine2");
        var lines = writer.GetLines();

        Assert.Equal(2, lines.Count);
        // Newline should trigger quoting
        Assert.Equal("Bob,\"Line1\nLine2\"", lines[1]);
    }

    [Fact]
    public void WriteRecord_MultipleRecords_CorrectFormat()
    {
        var writer = new CsvWriter();
        writer.WriteHeader("Name", "Age");
        writer.WriteRecord("Alice", 30);
        writer.WriteRecord("Bob", 25);
        writer.WriteRecord("Charlie", 35);
        var lines = writer.GetLines();

        Assert.Equal(4, lines.Count);
        Assert.Equal("Name,Age", lines[0]);
        Assert.Equal("Alice,30", lines[1]);
        Assert.Equal("Bob,25", lines[2]);
        Assert.Equal("Charlie,35", lines[3]);
    }

    [Fact]
    public void WriteRecord_NullValue_HandledAsEmpty()
    {
        var writer = new CsvWriter();
        writer.WriteHeader("A", "B", "C");
        writer.WriteRecord("1", null, "3");
        var lines = writer.GetLines();

        Assert.Equal(2, lines.Count);
        Assert.Equal("1,,3", lines[1]);
    }

    [Fact]
    public void WriteRecord_DoubleValue_FormattedAsString()
    {
        var writer = new CsvWriter();
        writer.WriteHeader("Value");
        writer.WriteRecord(3.14159);
        var lines = writer.GetLines();

        Assert.Equal(2, lines.Count);
        Assert.Contains("3.14159", lines[1]);
    }
}

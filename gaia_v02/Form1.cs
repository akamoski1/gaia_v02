using System.Text;
using System.Data;
using System.Diagnostics;
using gaia_v02.Experiments;
using gaia_v02.Utilities;

namespace gaia_v02;

public partial class Form1 : Form
{
    private string? _lastCsvPath;
    private string? _lastXlsxPath;

    public Form1()
    {
        InitializeComponent();
        InitializeComponentEx();
    }

    // -----------------------------------------------------------------------
    // Button handlers
    // -----------------------------------------------------------------------

    private async void BtnRunExperiment01_Click(object sender, EventArgs e)
    {
        btnRunExperiment01.Enabled = false;
        btnResetDefaults.Enabled = false;
        txtSummary.Clear();

        var p = ReadParameters();

        // Validate parameters before running
        try
        {
            p.Validate();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(
                $"Invalid parameter: {ex.Message}",
                "Parameter Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            btnRunExperiment01.Enabled = true;
            btnResetDefaults.Enabled = true;
            return;
        }

        var timeout = TimeSpan.FromMinutes((double)p.TimeoutMinutes);
        lblStatus.Text = $"Running Experiment 01… (timeout {p.TimeoutMinutes} min)";
        lblStatus.ForeColor = Color.DarkOrange;

        using var cts = new CancellationTokenSource(timeout);

        try
        {
            var experiment = new Experiment01(p);
            var result = await Task.Run(() => experiment.RunAsync(cts.Token), cts.Token);

            // Write CSV and XLSX to the user's temp folder with a deterministic timestamp format.
            string stamp = result.StartTime.ToString("yyyy_MM_dd_HHmm_fffffff");
            string csvPath = Path.Combine(
                Path.GetTempPath(),
                $"Experiment01_{stamp}.csv");

            // Keep the XLSX path aligned with the CSV path in temp.
            string xlsxPath = Path.Combine(
                Path.GetTempPath(),
                $"Experiment01_{stamp}.xlsx");

            // Save the CSV to temp, overwriting any existing file automatically.
            try
            {
                if (File.Exists(csvPath))
                    File.Delete(csvPath);

                await File.WriteAllLinesAsync(csvPath, result.CsvLines, CancellationToken.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to write CSV file to {csvPath}:\n{ex.Message}",
                    "File Write Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                btnRunExperiment01.Enabled = true;
                btnResetDefaults.Enabled = true;
                return;
            }

            // Convert CSV to XLSX in temp, overwriting any existing file automatically.
            try
            {
                if (File.Exists(xlsxPath))
                    File.Delete(xlsxPath);

                XlsxExporter.ConvertCsvToXlsx(csvPath, xlsxPath, "Experiment01");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to create XLSX file ({xlsxPath}):\n{ex.Message}",
                    "XLSX Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                // Continue anyway - CSV is still available
            }

            // Populate output boxes
            txtSummary.Text = BuildSummary(result, csvPath, xlsxPath);
            BindCsvToGrid(string.Join(Environment.NewLine, result.CsvLines));

            // remember paths for open buttons
            _lastCsvPath = csvPath;
            _lastXlsxPath = File.Exists(xlsxPath) ? xlsxPath : null;
            btnOpenNotepad.Enabled = true;
            btnOpenSpreadsheet.Enabled = true;
            btnOpenXlsx.Enabled = _lastXlsxPath != null;

            lblStatus.Text = $"Done – {result.QualCells} cells analysed, CSV saved to {Path.GetFileName(csvPath)}";
            lblStatus.ForeColor = Color.DarkGreen;
            // Remember paths for open buttons
            _lastCsvPath = csvPath;
            _lastXlsxPath = File.Exists(xlsxPath) ? xlsxPath : null;
            btnOpenNotepad.Enabled = true;
            btnOpenSpreadsheet.Enabled = true;
            btnOpenXlsx.Enabled = _lastXlsxPath != null;

            lblStatus.Text = $"Done – {result.QualCells} cells analysed, CSV saved to {Path.GetFileName(csvPath)}";
            lblStatus.ForeColor = Color.DarkGreen;
        }
        catch (OperationCanceledException)
        {
            lblStatus.Text = $"Experiment 01 timed out after {p.TimeoutMinutes} minutes.";
            lblStatus.ForeColor = Color.Firebrick;
            txtSummary.Text = $"[TIMEOUT] Experiment was cancelled after {p.TimeoutMinutes} minutes.\r\n" +
                                   "Consider reducing Star Count or increasing cell sizes to finish faster.";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Error – see summary box.";
            lblStatus.ForeColor = Color.Firebrick;
            txtSummary.Text = $"[ERROR] {ex.GetType().Name}: {ex.Message}\r\n\r\n{ex.StackTrace}";
        }
        finally
        {
            btnRunExperiment01.Enabled = true;
            btnResetDefaults.Enabled = true;
        }
    }

    private void BtnResetDefaults_Click(object sender, EventArgs e)
    {
        var d = Experiment01Parameters.Default;
        nudStarCount.Value = d.StarCount;
        nudR0.Value = (decimal)d.R0;
        nudSigmaR.Value = (decimal)d.SigmaR;
        nudSigmaZ.Value = (decimal)d.SigmaZ;
        nudSigmaPhi.Value = (decimal)d.SigmaPhi;
        nudVLSR.Value = (decimal)d.V_LSR;
        nudScaleHeightR.Value = (decimal)d.ScaleHeightR;
        nudScaleHeightZ.Value = (decimal)d.ScaleHeightZ;
        nudExpectedRatio.Value = (decimal)d.ExpectedRatio;
        nudRatioTolerance.Value = (decimal)d.RatioTolerance;
        nudCellDeltaR.Value = (decimal)d.CellDeltaR;
        nudCellDeltaZ.Value = (decimal)d.CellDeltaZ;
        nudMinStars.Value = d.MinStarsPerCell;
        nudTimeoutMinutes.Value = d.TimeoutMinutes;
        nudRandomSeed.Value = d.RandomSeed.HasValue ? d.RandomSeed.Value : 42;

        lblStatus.Text = "Parameters reset to defaults.";
        lblStatus.ForeColor = SystemColors.GrayText;
    }

    private void BtnReadme_Click(object? sender, EventArgs e)
    {
        try
        {
            string? readmePath = FindReadme();

            if (string.IsNullOrEmpty(readmePath) || !File.Exists(readmePath))
            {
                MessageBox.Show("README.md could not be found.", "Open README", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var psi = new ProcessStartInfo("notepad.exe") { Arguments = $"\"{readmePath}\"", UseShellExecute = true };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open README: {ex.Message}", "Open README", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string? FindReadme()
    {
        // Candidate starting points: the app's base directory, the running process's executable
        // directory (may differ from AppContext.BaseDirectory for single-file publishes), and the CWD.
        var startDirs = new List<string?>
        {
            AppContext.BaseDirectory,
            Environment.ProcessPath is { } p ? Path.GetDirectoryName(p) : null,
            Directory.GetCurrentDirectory(),
        };

        foreach (var start in startDirs)
        {
            string? dir = start;
            for (int i = 0; i < 8 && dir is not null; i++)
            {
                string candidate = Path.Combine(dir, "README.md");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
                dir = Path.GetDirectoryName(dir);
            }
        }

        return null;
    }

    private void BtnOpenNotepad_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_lastCsvPath) || !File.Exists(_lastCsvPath))
        {
            MessageBox.Show("No CSV file found. Run an experiment first.", "Open CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var psi = new ProcessStartInfo("notepad.exe") { Arguments = $"\"{_lastCsvPath}\"", UseShellExecute = true };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open Notepad: {ex.Message}", "Open CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnOpenSpreadsheet_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_lastCsvPath) || !File.Exists(_lastCsvPath))
        {
            MessageBox.Show("No CSV file found. Run an experiment first.", "Open CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Try Excel, then LibreOffice/OpenOffice, then fallback to default association
        try
        {
            // Try Excel first
            try
            {
                var p = new ProcessStartInfo("excel.exe") { Arguments = $"\"{_lastCsvPath}\"", UseShellExecute = true };
                Process.Start(p);
                return;
            }
            catch { /* ignore and try next */ }

            // Try LibreOffice/OpenOffice (soffice) with --calc
            try
            {
                var p2 = new ProcessStartInfo("soffice") { Arguments = $"--calc \"{_lastCsvPath}\"", UseShellExecute = true };
                Process.Start(p2);
                return;
            }
            catch { /* ignore */ }

            // Fallback: open with default associated application
            var psi = new ProcessStartInfo(_lastCsvPath) { UseShellExecute = true };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open spreadsheet application: {ex.Message}", "Open CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnOpenXlsx_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_lastXlsxPath) || !File.Exists(_lastXlsxPath))
        {
            MessageBox.Show("No XLSX file found. Run an experiment first.", "Open XLSX", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Try Excel, then LibreOffice/OpenOffice, then default association
        try
        {
            // Try Excel first
            try
            {
                var p = new ProcessStartInfo("excel.exe") { Arguments = $"\"{_lastXlsxPath}\"", UseShellExecute = true };
                Process.Start(p);
                return;
            }
            catch { /* ignore and try next */ }

            // Try LibreOffice/OpenOffice (soffice) with --calc
            try
            {
                var p2 = new ProcessStartInfo("soffice") { Arguments = $"--calc \"{_lastXlsxPath}\"", UseShellExecute = true };
                Process.Start(p2);
                return;
            }
            catch { /* ignore */ }

            // Fallback: open with default associated application
            var psi = new ProcessStartInfo(_lastXlsxPath) { UseShellExecute = true };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open XLSX file: {ex.Message}", "Open XLSX", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // -----------------------------------------------------------------------
    // Parameter collection
    // -----------------------------------------------------------------------

    private Experiment01Parameters ReadParameters() => new(
        StarCount: (int)nudStarCount.Value,
        R0: (double)nudR0.Value,
        SigmaR: (double)nudSigmaR.Value,
        SigmaZ: (double)nudSigmaZ.Value,
        SigmaPhi: (double)nudSigmaPhi.Value,
        V_LSR: (double)nudVLSR.Value,
        ScaleHeightR: (double)nudScaleHeightR.Value,
        ScaleHeightZ: (double)nudScaleHeightZ.Value,
        ExpectedRatio: (double)nudExpectedRatio.Value,
        RatioTolerance: (double)nudRatioTolerance.Value,
        CellDeltaR: (double)nudCellDeltaR.Value,
        CellDeltaZ: (double)nudCellDeltaZ.Value,
        MinStarsPerCell: (int)nudMinStars.Value,
        TimeoutMinutes: (int)nudTimeoutMinutes.Value,
        RandomSeed: (int)nudRandomSeed.Value == 0 ? null : (int)nudRandomSeed.Value);

    // -----------------------------------------------------------------------
    // Summary + insights builder
    // -----------------------------------------------------------------------

    private void BindCsvToGrid(string csvText)
    {
        if (string.IsNullOrWhiteSpace(csvText))
        {
            dgvCsv.DataSource = null;
            return;
        }

        var lines = csvText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            dgvCsv.DataSource = null;
            return;
        }

        var table = new DataTable();
        var headers = SplitCsvLine(lines[0]);
        foreach (var header in headers)
        {
            var colName = string.IsNullOrWhiteSpace(header) ? $"Column{table.Columns.Count + 1}" : header.Trim();
            if (table.Columns.Contains(colName))
            {
                colName = $"{colName}_{table.Columns.Count + 1}";
            }
            table.Columns.Add(colName);
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var values = SplitCsvLine(lines[i]);
            if (values.Length == 0)
                continue;

            var row = table.NewRow();
            for (int j = 0; j < table.Columns.Count; j++)
            {
                row[j] = j < values.Length ? values[j] : string.Empty;
            }
            table.Rows.Add(row);
        }

        dgvCsv.DataSource = table;
        dgvCsv.AutoGenerateColumns = true;
        dgvCsv.AllowUserToOrderColumns = true;
        dgvCsv.ReadOnly = true;
        dgvCsv.RowHeadersVisible = false;
        dgvCsv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        dgvCsv.Sort(dgvCsv.Columns[0], System.ComponentModel.ListSortDirection.Ascending);
    }

    private static string[] SplitCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString());
        return values.ToArray();
    }

    private static string BuildSummary(ExperimentResult r, string csvPath, string? xlsxPath)
    {
        var p = r.Parameters;
        var sb = new StringBuilder();

        const string Sep = "═══════════════════════════════════════════════════════════════════";
        const string Sep2 = "───────────────────────────────────────────────────────────────────";

        sb.AppendLine(Sep);
        sb.AppendLine("  EXPERIMENT 01 – GAIA PHASE-SPACE DISPERSION ANALYSIS");
        sb.AppendLine(Sep);
        sb.AppendLine();

        // --- Parameters used ---
        sb.AppendLine("PARAMETERS USED");
        sb.AppendLine(Sep2);
        AppendParam(sb, "Star Count", $"{p.StarCount:N0}");
        AppendParam(sb, "R₀ (Solar galactocentric radius)", $"{p.R0:F2} kpc");
        AppendParam(sb, "σR (Radial dispersion input)", $"{p.SigmaR:F1} km/s");
        AppendParam(sb, "σZ (Vertical dispersion input)", $"{p.SigmaZ:F1} km/s");
        AppendParam(sb, "σΦ (Azimuthal dispersion input)", $"{p.SigmaPhi:F1} km/s");
        AppendParam(sb, "V_LSR", $"{p.V_LSR:F1} km/s");
        AppendParam(sb, "Scale Height R", $"{p.ScaleHeightR:F2} kpc");
        AppendParam(sb, "Scale Height Z", $"{p.ScaleHeightZ:F3} kpc");
        AppendParam(sb, "Expected σR/σZ Ratio", $"{p.ExpectedRatio:F3}");
        AppendParam(sb, "Ratio Tolerance (±)", $"{p.RatioTolerance:F3}");
        AppendParam(sb, "Cell ΔR", $"{p.CellDeltaR:F3} kpc");
        AppendParam(sb, "Cell ΔZ", $"{p.CellDeltaZ:F3} kpc");
        AppendParam(sb, "Min Stars / Cell", $"{p.MinStarsPerCell}");
        AppendParam(sb, "Timeout", $"{p.TimeoutMinutes} min");
        sb.AppendLine();

        // --- Timing ---
        sb.AppendLine("TIMING");
        sb.AppendLine(Sep2);
        AppendParam(sb, "Start", $"{r.StartTime:yyyy-MM-dd HH:mm:ss.fff}");
        AppendParam(sb, "End", $"{r.EndTime:yyyy-MM-dd HH:mm:ss.fff}");
        AppendParam(sb, "Elapsed", $"{r.Elapsed:hh\\:mm\\:ss\\.fff}");
        sb.AppendLine();

        // --- Results ---
        double passPct = r.QualCells > 0 ? 100.0 * r.PassingCells / r.QualCells : 0.0;
        double ratioBias = double.IsNaN(r.MeanRatio) ? double.NaN : r.MeanRatio - p.ExpectedRatio;
        double ratioBiasPct = double.IsNaN(r.MeanRatio) ? double.NaN : 100.0 * Math.Abs(ratioBias) / p.ExpectedRatio;

        sb.AppendLine("RESULTS");
        sb.AppendLine(Sep2);
        AppendParam(sb, "Stars generated", $"{p.StarCount:N0}");
        AppendParam(sb, "Raw spatial cells", $"{r.TotalRawCells:N0}");
        AppendParam(sb, "Qualifying cells (N≥min)", $"{r.QualCells:N0}");
        AppendParam(sb, "Cells passing ratio test", $"{r.PassingCells:N0}  /  {r.QualCells:N0}  ({passPct:F1}%)");
        AppendParam(sb, "Mean measured σR/σZ", $"{r.MeanRatio:F4}  (expected {p.ExpectedRatio:F3})");
        AppendParam(sb, "Std  σR/σZ", $"{r.StdRatio:F4}");
        AppendParam(sb, "Min  σR/σZ", $"{r.MinRatio:F4}");
        AppendParam(sb, "Max  σR/σZ", $"{r.MaxRatio:F4}");
        AppendParam(sb, "Mean σR (measured)", $"{r.MeanSigmaR:F2} km/s  (input {p.SigmaR:F1} km/s)");
        AppendParam(sb, "Mean σZ (measured)", $"{r.MeanSigmaZ:F2} km/s  (input {p.SigmaZ:F1} km/s)");
        AppendParam(sb, "CSV file", csvPath);
        AppendParam(sb, "XLSX file", xlsxPath ?? "Not created");
        sb.AppendLine();

        // --- Insights ---
        sb.AppendLine("INSIGHTS");
        sb.AppendLine(Sep2);

        // Insight 1: ratio accuracy
        if (!double.IsNaN(r.MeanRatio))
        {
            string direction = ratioBias >= 0 ? "above" : "below";
            string quality = ratioBiasPct < 1.0 ? "excellent (< 1%)" :
                               ratioBiasPct < 3.0 ? "good (< 3%)" :
                               ratioBiasPct < 5.0 ? "fair (< 5%)" : "poor (≥ 5%)";

            sb.AppendLine($"  [Ratio accuracy – {quality}]");
            sb.AppendLine($"    The mean measured σR/σZ = {r.MeanRatio:F4} is {Math.Abs(ratioBias):F4} ({ratioBiasPct:F2}%)");
            sb.AppendLine($"    {direction} the target of {p.ExpectedRatio:F3}. This is physically expected from");
            sb.AppendLine($"    finite-sample scatter in each spatial cell.");
            sb.AppendLine();
        }

        // Insight 2: pass rate
        sb.AppendLine($"  [Pass rate – {passPct:F1}%]");
        if (passPct >= 80)
            sb.AppendLine($"    A high fraction ({passPct:F1}%) of cells are consistent with the expected");
        else if (passPct >= 50)
            sb.AppendLine($"    A moderate fraction ({passPct:F1}%) of cells pass the ratio check.");
        else
            sb.AppendLine($"    A low fraction ({passPct:F1}%) of cells pass — consider widening RatioTolerance");
        sb.AppendLine($"    ratio (±{p.RatioTolerance:F3}), confirming the synthetic population is realistic.");
        sb.AppendLine();

        // Insight 3: degenerate two-integral solution check
        bool degenerateRisk = !double.IsNaN(r.MeanRatio) && Math.Abs(r.MeanRatio - 1.0) < 0.10;
        if (degenerateRisk)
        {
            sb.AppendLine("  [⚠ DEGENERATE SOLUTION RISK]");
            sb.AppendLine($"    Mean σR/σZ ≈ {r.MeanRatio:F3} ≈ 1.0 — this matches the degenerate two-integral");
            sb.AppendLine("    case (σR = σZ). Models with this ratio must be rejected per the spec.");
        }
        else
        {
            sb.AppendLine("  [No degenerate two-integral solution detected]");
            sb.AppendLine($"    σR/σZ = {r.MeanRatio:F4} is well away from 1.0. The distribution function");
            sb.AppendLine("    is not collapsing to the degenerate equal-dispersion state.");
        }
        sb.AppendLine();

        // Insight 4: tuning suggestions
        sb.AppendLine("  [Tuning suggestions]");
        if (r.QualCells < 30)
            sb.AppendLine("    → Very few qualifying cells. Increase StarCount or enlarge Cell ΔR / Cell ΔZ.");
        if (r.QualCells > 0 && (double)r.PassingCells / r.QualCells < 0.3)
            sb.AppendLine("    → Low pass rate. Try widening Ratio Tolerance or adjusting σR/σZ input dispersions.");
        if (!double.IsNaN(r.StdRatio) && r.StdRatio > 0.3)
            sb.AppendLine("    → High cell-to-cell scatter in σR/σZ (std > 0.3). Increase StarCount or MinStars/Cell.");
        if (!double.IsNaN(r.MeanSigmaR) && Math.Abs(r.MeanSigmaR - p.SigmaR) / p.SigmaR > 0.05)
            sb.AppendLine($"    → Measured mean σR ({r.MeanSigmaR:F2}) differs from input ({p.SigmaR:F1}) by > 5% — expected with small N.");
        sb.AppendLine($"    → Increasing StarCount reduces per-cell scatter proportional to 1/√N.");
        sb.AppendLine($"    → Decreasing Cell ΔR / ΔZ increases spatial resolution at the cost of fewer stars per cell.");

        sb.AppendLine();
        sb.AppendLine(Sep);

        return sb.ToString();
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        // 
        // Form1
        // 
        ClientSize = new Size(1002, 712);
        MinimumSize = new Size(1024, 768);
        Name = "Form1";
        SizeGripStyle = SizeGripStyle.Show;
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        Load += Form1_Load;
        ResumeLayout(false);

    }

    private static void AppendParam(StringBuilder sb, string name, string value)
        => sb.AppendLine($"  {name,-38}: {value}");

    private void Form1_Load(object? sender, EventArgs e)
    {
       this.WindowState= FormWindowState.Maximized;
    }

}



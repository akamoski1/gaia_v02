using System.Diagnostics;
using gaia_v02.Domain;

namespace gaia_v02.Experiments;

/// <summary>
/// User-configurable parameters for Experiment01.
/// All values have physically motivated defaults drawn from the spec.
/// </summary>
public sealed record Experiment01Parameters(
    int    StarCount,
    double R0,
    double SigmaR,
    double SigmaZ,
    double SigmaPhi,
    double V_LSR,
    double ScaleHeightR,
    double ScaleHeightZ,
    double ExpectedRatio,
    double RatioTolerance,
    double CellDeltaR,
    double CellDeltaZ,
    int    MinStarsPerCell,
    int    TimeoutMinutes)
{
    /// <summary>Returns a new instance carrying all documented default values.</summary>
    public static Experiment01Parameters Default => new(
        StarCount      : 50_000,
        R0             : 8.0,
        SigmaR         : 38.0,
        SigmaZ         : 20.0,
        SigmaPhi       : 28.0,
        V_LSR          : 220.0,
        ScaleHeightR   : 2.5,
        ScaleHeightZ   : 0.3,
        ExpectedRatio  : 1.93,
        RatioTolerance : 0.15,
        CellDeltaR     : 0.50,
        CellDeltaZ     : 0.15,
        MinStarsPerCell: 20,
        TimeoutMinutes : 5);
}

/// <summary>
/// Experiment01 – Minimal phase-space pipeline proof-of-concept.
///
/// Steps:
///   1. Generate synthetic Gaia-like stellar phase-space data.
///   2. Partition stars into spatial cells near the galactic midplane.
///   3. Compute velocity dispersions (σR, σZ) per cell and the ratio σR/σZ.
///   4. Flag cells that match the physically expected ratio (~1.93).
///   5. Write per-cell results to a CSV file next to the EXE.
///
/// A CancellationToken with a 5-minute timeout is honoured throughout.
/// </summary>
public sealed class Experiment01
{
    private readonly Experiment01Parameters _p;

    public Experiment01(Experiment01Parameters parameters) => _p = parameters;

    public async Task<ExperimentResult> RunAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var startTime = DateTime.Now;

        var csvLines = new List<string>();
        var log = new List<string>();

        log.Add($"[Experiment01] Started at {startTime:yyyy-MM-dd HH:mm:ss}");
        log.Add($"[Experiment01] Generating {_p.StarCount:N0} synthetic stars...");

        await Task.Yield(); // allow UI to update before blocking work begins

        // --- Step 1: Generate synthetic stars ---
        cancellationToken.ThrowIfCancellationRequested();
        var stars = GenerateStars(cancellationToken);
        log.Add($"[Experiment01] Generated {stars.Count:N0} stars.");

        // --- Step 2: Bin into spatial cells ---
        cancellationToken.ThrowIfCancellationRequested();
        var cells = BinIntoCells(stars, cancellationToken);
        log.Add($"[Experiment01] Partitioned into {cells.Count} raw spatial cells.");

        // --- Steps 3 & 4: Compute dispersions and evaluate ratio ---
        cancellationToken.ThrowIfCancellationRequested();
        var cellResults = ComputeDispersions(cells, cancellationToken);
        int passCount = cellResults.Count(c => c.PassesRatioCheck);
        log.Add($"[Experiment01] Qualifying cells (N≥{_p.MinStarsPerCell}): {cellResults.Count}");
        log.Add($"[Experiment01] Cells passing σR/σZ ratio check: {passCount} / {cellResults.Count}");

        // --- Step 5: Build CSV ---
        csvLines.Add("CellR_kpc,CellZ_kpc,StarCount,SigmaR_kms,SigmaZ_kms,Ratio_SigmaR_SigmaZ,ExpectedRatio,PassesCheck");
        foreach (var c in cellResults)
        {
            cancellationToken.ThrowIfCancellationRequested();
            csvLines.Add(
                $"{c.CellR:F2},{c.CellZ:F3},{c.N}," +
                $"{c.SigmaR:F4},{c.SigmaZ:F4},{c.Ratio:F4}," +
                $"{_p.ExpectedRatio:F2},{(c.PassesRatioCheck ? "YES" : "NO")}");
        }

        sw.Stop();
        var endTime = DateTime.Now;
        log.Add($"[Experiment01] Finished at {endTime:yyyy-MM-dd HH:mm:ss}");

        // Aggregate statistics
        var ratios = cellResults.Where(c => !double.IsNaN(c.Ratio)).Select(c => c.Ratio).ToList();
        double meanRatio  = ratios.Count > 0 ? ratios.Average() : double.NaN;
        double stdRatio   = ratios.Count > 1 ? Math.Sqrt(ratios.Sum(r => Math.Pow(r - meanRatio, 2)) / ratios.Count) : double.NaN;
        double minRatio   = ratios.Count > 0 ? ratios.Min() : double.NaN;
        double maxRatio   = ratios.Count > 0 ? ratios.Max() : double.NaN;
        double meanSigmaR = cellResults.Count > 0 ? cellResults.Average(c => c.SigmaR) : double.NaN;
        double meanSigmaZ = cellResults.Count > 0 ? cellResults.Average(c => c.SigmaZ) : double.NaN;

        return new ExperimentResult(
            Parameters   : _p,
            CsvLines     : csvLines,
            LogLines     : log,
            CellResults  : cellResults,
            StartTime    : startTime,
            EndTime      : endTime,
            Elapsed      : sw.Elapsed,
            TotalRawCells: cells.Count,
            QualCells    : cellResults.Count,
            PassingCells : passCount,
            MeanRatio    : meanRatio,
            StdRatio     : stdRatio,
            MinRatio     : minRatio,
            MaxRatio     : maxRatio,
            MeanSigmaR   : meanSigmaR,
            MeanSigmaZ   : meanSigmaZ);
    }

    // -----------------------------------------------------------------------
    private List<StarPhaseSpace> GenerateStars(CancellationToken ct)
    {
        var rng   = new Random(42);
        var stars = new List<StarPhaseSpace>(_p.StarCount);

        for (int i = 0; i < _p.StarCount; i++)
        {
            if (i % 5_000 == 0) ct.ThrowIfCancellationRequested();

            double r = _p.R0 + SampleExponential(rng, _p.ScaleHeightR) - _p.ScaleHeightR;
            r = Math.Max(0.5, r);
            double z    = SampleLaplace(rng, _p.ScaleHeightZ);
            double vr   = SampleGaussian(rng, 0.0, _p.SigmaR);
            double vz   = SampleGaussian(rng, 0.0, _p.SigmaZ);
            double vphi = SampleGaussian(rng, _p.V_LSR, _p.SigmaPhi);

            stars.Add(new StarPhaseSpace(r, z, vr, vz, vphi));
        }

        return stars;
    }

    private Dictionary<(int iR, int iZ), List<StarPhaseSpace>> BinIntoCells(
        List<StarPhaseSpace> stars, CancellationToken ct)
    {
        var cells = new Dictionary<(int, int), List<StarPhaseSpace>>();

        foreach (var s in stars)
        {
            ct.ThrowIfCancellationRequested();
            int iR = (int)Math.Floor(s.R / _p.CellDeltaR);
            int iZ = (int)Math.Floor(s.Z / _p.CellDeltaZ);
            if (!cells.TryGetValue((iR, iZ), out var list))
            {
                list = [];
                cells[(iR, iZ)] = list;
            }
            list.Add(s);
        }

        return cells;
    }

    private List<CellResult> ComputeDispersions(
        Dictionary<(int iR, int iZ), List<StarPhaseSpace>> cells,
        CancellationToken ct)
    {
        var results = new List<CellResult>(cells.Count);

        foreach (var kvp in cells)
        {
            ct.ThrowIfCancellationRequested();
            var list = kvp.Value;
            if (list.Count < _p.MinStarsPerCell) continue;

            double cellR = (kvp.Key.iR + 0.5) * _p.CellDeltaR;
            double cellZ = (kvp.Key.iZ + 0.5) * _p.CellDeltaZ;
            double sigR  = StandardDeviation(list, s => s.VR);
            double sigZ  = StandardDeviation(list, s => s.VZ);
            double ratio = sigZ > 0.0 ? sigR / sigZ : double.NaN;
            bool passes  = !double.IsNaN(ratio) &&
                           Math.Abs(ratio - _p.ExpectedRatio) <= _p.RatioTolerance;

            results.Add(new CellResult(cellR, cellZ, list.Count, sigR, sigZ, ratio, passes));
        }

        results.Sort((a, b) =>
        {
            int c = a.CellR.CompareTo(b.CellR);
            return c != 0 ? c : a.CellZ.CompareTo(b.CellZ);
        });

        return results;
    }

    // -----------------------------------------------------------------------
    // Sampling helpers (Box-Muller, inverse-CDF exponential and Laplace)

    private static double SampleGaussian(Random rng, double mean, double sigma)
    {
        double u1 = 1.0 - rng.NextDouble();
        double u2 = 1.0 - rng.NextDouble();
        double z  = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return mean + sigma * z;
    }

    private static double SampleExponential(Random rng, double scale)
        => -scale * Math.Log(1.0 - rng.NextDouble());

    private static double SampleLaplace(Random rng, double scale)
    {
        double u = rng.NextDouble() - 0.5;
        return -scale * Math.Sign(u) * Math.Log(1.0 - 2.0 * Math.Abs(u));
    }

    private static double StandardDeviation(List<StarPhaseSpace> list, Func<StarPhaseSpace, double> selector)
    {
        double mean     = list.Average(selector);
        double variance = list.Sum(s => Math.Pow(selector(s) - mean, 2)) / list.Count;
        return Math.Sqrt(variance);
    }
}

// -----------------------------------------------------------------------

public sealed record CellResult(
    double CellR,
    double CellZ,
    int    N,
    double SigmaR,
    double SigmaZ,
    double Ratio,
    bool   PassesRatioCheck);

public sealed record ExperimentResult(
    Experiment01Parameters    Parameters,
    IReadOnlyList<string>     CsvLines,
    IReadOnlyList<string>     LogLines,
    IReadOnlyList<CellResult> CellResults,
    DateTime                  StartTime,
    DateTime                  EndTime,
    TimeSpan                  Elapsed,
    int                       TotalRawCells,
    int                       QualCells,
    int                       PassingCells,
    double                    MeanRatio,
    double                    StdRatio,
    double                    MinRatio,
    double                    MaxRatio,
    double                    MeanSigmaR,
    double                    MeanSigmaZ);


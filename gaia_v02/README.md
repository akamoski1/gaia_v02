gaia_v02 — Phase-space toy experiment (Gaia-like)
=================================================

Project summary
---------------
gaia_v02 is a minimal, Windows Forms proof-of-concept that generates synthetic, Gaia-like stellar phase-space samples in cylindrical galactic coordinates and analyses local kinematic dispersions. The experiment partitions stars into spatial cells (R, Z) near the midplane, computes velocity dispersions in each cell (σR and σZ), and flags cells whose σR/σZ ratio matches an expected physical value.

This repository is intended as a small research/teaching/demo app to show sampling, simple spatial binning, and dispersion statistics on synthetic data.

Key files
---------
- gaia_v02/Experiments/Experiment01.cs — core experiment pipeline (generation, binning, dispersion calculation, CSV output).
- gaia_v02/Domain/StarPhaseSpace.cs — immutable record type for a single star's phase-space coordinates (R, Z, VR, VZ, VPhi).
- gaia_v02/Form1.cs and Form1.Designer.cs — WinForms UI that exposes parameters and runs the experiment, writes CSV next to the EXE.
- gaia_v02/Program.cs — WinForms program entry point.

High-level algorithm (Experiment01)
-----------------------------------
1. Generate N synthetic stars (GenerateStars):
   - R (galactocentric radius) is sampled as R0 + Exponential(scale=ScaleHeightR) − ScaleHeightR, floored to a minimum (0.5 kpc in code).
   - Z (height above plane) is sampled from a Laplace distribution with scale = ScaleHeightZ.
   - Velocities VR, VZ, and VPhi are sampled from Gaussians (normal distributions). VPhi is centered on V_LSR.

2. Partition the stars into rectangular spatial cells in (R, Z): index iR = floor(R / CellDeltaR) and iZ = floor(Z / CellDeltaZ). Each unique (iR, iZ) defines a cell.

3. For each cell that has at least MinStarsPerCell stars, compute:
   - σR = Standard deviation of VR over the cell.
   - σZ = Standard deviation of VZ over the cell.
   - Ratio = σR / σZ (NaN if σZ == 0).
   - Passes check if |Ratio − ExpectedRatio| ≤ RatioTolerance.

4. Save per-cell results into a CSV file and present a summary in the UI.

### Pseudocode overview

```
GenerateStars(N):
    for i in 1..N:
        R    = clamp(R0 + Exponential(hR) - hR, min=0.5)
        Z    = Laplace(0, hZ)
        VR   = Normal(0,      sigmaR)
        VZ   = Normal(0,      sigmaZ)
        VPhi = Normal(V_LSR,  sigmaPhi)
    return stars

BinIntoCells(stars):
    for s in stars:
        iR = floor(s.R / deltaR)
        iZ = floor(s.Z / deltaZ)
        cells[(iR, iZ)].add(s)
    return cells

ComputeDispersions(cells):
    for (iR, iZ), list in cells:
        if len(list) < MinStarsPerCell: continue
        sigmaR = stddev(list.VR)
        sigmaZ = stddev(list.VZ)
        ratio  = sigmaR / sigmaZ
        passes = |ratio - ExpectedRatio| <= RatioTolerance
        emit CellResult(cellR, cellZ, N, sigmaR, sigmaZ, ratio, passes)
```

Sampling and math details
-------------------------

### Coordinate system

Stars are represented in cylindrical galactocentric coordinates via `StarPhaseSpace`:
- R — galactocentric radius [kpc]
- Z — height above the galactic midplane [kpc]
- VR, VZ, VPhi — radial, vertical, and azimuthal velocity components [km/s]

This is a standard simplification used in Galactic dynamics: instead of full 6D (x, y, z, vx, vy, vz) Cartesian phase space, the axisymmetric cylindrical representation (R, Z, VR, VZ, VPhi) is used because the synthetic model assumes axisymmetry (no dependence on the azimuthal angle φ itself, only on R and Z).

### Radial density profile — exponential disk

Real disk galaxies (including the Milky Way) have surface/volume density profiles that fall off approximately exponentially with radius:

  ρ(R) ∝ exp(−R / hR)

where hR is the radial scale length (`ScaleHeightR`, default 2.5 kpc). To draw R-offsets consistent with this profile, the code samples from an Exponential(scale = hR) distribution and re-centers it about R0 (the Solar radius):

  R = R0 + Exponential(hR) − hR

using inverse-CDF sampling (see below). The result is clamped to a minimum of 0.5 kpc to avoid unphysical radii near/at the galactic center.

### Vertical density profile — Laplace (double-exponential) disk

The vertical stellar density of a thin/thick galactic disk is commonly modeled as (double-sided) exponential in |Z|:

  ρ(Z) ∝ exp(−|Z| / hZ)

This is exactly the Laplace distribution with scale hZ (`ScaleHeightZ`, default 0.3 kpc), so Z is drawn directly from Laplace(0, hZ).

### Velocity distributions — Schwarzschild (Gaussian) velocity ellipsoid

Local stellar velocities in the solar neighborhood are well approximated, to first order, by independent Gaussians in each cylindrical velocity component — the classical "Schwarzschild velocity ellipsoid" approximation:

  VR   ~ N(0,     σR²)
  VZ   ~ N(0,     σZ²)
  VPhi ~ N(V_LSR, σPhi²)

VPhi is centered on the Local Standard of Rest circular speed `V_LSR` (default 220 km/s), while VR and VZ are centered on zero (no net radial/vertical streaming motion in this simplified model).

### Why σR/σZ ≈ 1.93 is physically expected

In a Milky Way–like disk in near-equilibrium, the vertical and radial velocity dispersions are linked (approximately) through the epicyclic approximation and the disk's vertical/radial force balance. Empirically and in dynamical disk models, the ratio σR/σZ for the thin/thick disk populations is observed to cluster around ~1.8–2.0, commonly cited near 1.93 for the solar neighborhood. `ExpectedRatio` (default 1.93) and `RatioTolerance` (default 0.15) encode this empirical/theoretical expectation so the experiment can flag which spatial cells are "dynamically consistent" with a relaxed exponential disk.

### Random variate generation

**Gaussian (normal) sampling — Box–Muller transform.**
Given two independent uniform(0,1) samples u1, u2 (with u1, u2 ∈ (0,1], avoiding exact 0 to prevent `ln(0)`), a standard normal variate z is computed by:

  z = sqrt(−2 ln u1) · cos(2π u2)

This is the polar (trigonometric) form of the Box–Muller transform, which converts two independent uniform samples into one standard normal deviate exactly (in the ideal, infinite-precision case), by exploiting the fact that if (X, Y) are i.i.d. standard normal, then R² = X² + Y² is Exponential(2) and θ = atan2(Y, X) is Uniform(0, 2π); inverting this relationship yields the formula above. The code returns `mean + sigma * z`.

**Exponential sampling — inverse CDF.**
The exponential distribution's CDF is F(x) = 1 − exp(−x/scale). Setting u = F(x) and solving for x (inverse-CDF / inverse-transform sampling) gives:

  x = −scale · ln(1 − u),  u ~ Uniform(0,1)

**Laplace sampling — inverse CDF.**
The Laplace (double exponential) distribution is symmetric exponential decay on both sides of zero. Using u shifted into (−0.5, +0.5), the inverse CDF is:

  x = −scale · sign(u) · ln(1 − 2|u|)

This produces a value that decays exponentially in |x| with scale `scale`, matching ρ(Z) ∝ exp(−|Z|/hZ) above.

### Dispersion (standard deviation) calculation

For each spatial cell, the pipeline computes the *population* standard deviation of a velocity component x (VR or VZ) over the N stars in that cell:

  mean = (1/N) ∑ᵢ xᵢ
  variance = (1/N) ∑ᵢ (xᵢ − mean)²
  σ = sqrt(variance)

Note: this uses the population denominator N (not the Bessel-corrected N−1), which slightly underestimates the true dispersion for small N. This is consistent across all cells in the current code, so cell-to-cell comparisons remain valid, but absolute σ values are biased low, especially for cells near `MinStarsPerCell`.

### Ratio test

For each qualifying cell (N ≥ MinStarsPerCell):

  Ratio = σR / σZ   (NaN if σZ == 0)
  PassesCheck = |Ratio − ExpectedRatio| ≤ RatioTolerance

This is a simple symmetric-tolerance band test (not a statistical significance test); it does not account for the sampling uncertainty of σR/σZ, which itself depends on N (smaller cells have noisier ratio estimates).

CSV output
----------
Each output CSV row contains the following columns:
- CellR_kpc — center radius of the cell (kpc)
- CellZ_kpc — center height of the cell (kpc)
- StarCount — number of stars in the cell
- SigmaR_kms — σR (km/s)
- SigmaZ_kms — σZ (km/s)
- Ratio_SigmaR_SigmaZ — σR/σZ (or NaN)
- ExpectedRatio — the target ratio provided by parameters
- PassesCheck — YES/NO whether the ratio is within tolerance

Default parameters (from Experiment01Parameters.Default)
-----------------------------------------------------
- StarCount: 50,000
- R0 (Solar radius): 8.0 kpc
- SigmaR: 38.0 km/s
- SigmaZ: 20.0 km/s
- SigmaPhi: 28.0 km/s
- V_LSR: 220.0 km/s
- ScaleHeightR: 2.5 kpc
- ScaleHeightZ: 0.3 kpc
- ExpectedRatio: 1.93
- RatioTolerance: 0.15
- CellDeltaR: 0.50 kpc
- CellDeltaZ: 0.15 kpc
- MinStarsPerCell: 20
- TimeoutMinutes: 5

Computational complexity & performance notes
-------------------------------------------
- Generating N stars is O(N) in time and O(N) memory for the in-memory list.
- Binning into cells is O(N) average-case (dictionary insertion per star). The number of distinct cells M is bounded by the spatial range and cell size; memory usage for the dictionary is O(N) in the worst case.
- Computing dispersions iterates only over populated cells and computes averages and sums over each cell's stars — total cost is O(N) across all cells.
- The default RNG seed (42) is fixed in code for deterministic reproducibility. If you want stochastic runs, seed with a variable (e.g., time-based seed).

How to build and run
--------------------
1. Open the solution gaia_v02.slnx in Visual Studio 2022/2024/2026 (project targets .NET 10).
2. Build and run the Windows Forms app. The UI exposes all experiment parameters and provides a Run button. The experiment will write a CSV next to the EXE (AppContext.BaseDirectory) with the timestamped filename Experiment01_YYYYMMDD_HHMMSS.csv.

Notes and potential improvements
-------------------------------
- Consider using streaming / chunked processing to reduce peak memory if StarCount is extremely large.
- If you want unbiased sample standard deviations for small N, use the sample variance (divide by N−1) instead of N.
- The ratio pass/fail check uses a fixed tolerance band and ignores the sampling uncertainty of σR/σZ, which scales roughly as O(1/√N); consider weighting cells by N or using a statistical test (e.g., an F-test on variances) for a more rigorous check.
- Add parallelism: generation and binning are mostly embarrassingly parallel and could be parallelized (careful with RNG concurrency and dictionary concurrency).
- Consider using more realistic galactic kinematic models (non-gaussian tails, radial dependence in dispersions, or correlations between velocity components) for advanced studies.

Files / symbols to inspect for customization
-------------------------------------------
- Experiment01.GenerateStars — change sampling distributions and seed.
- Experiment01.BinIntoCells — change cell geometry (hex/overlapping windows) or indexing.
- Experiment01.ComputeDispersions — change statistic (robust estimators, median absolute deviation, bootstrapped errors).

License / attribution
---------------------
This repository is provided as-is for demonstration and research. There is no embedded third-party data; generated data is synthetic.

Contact
-------
For code questions, inspect Experiment01.cs and Form1.* in the gaia_v02 project.

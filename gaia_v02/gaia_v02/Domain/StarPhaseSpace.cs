namespace gaia_v02.Domain;

/// <summary>
/// Immutable phase-space coordinates for a single star in cylindrical galactic coordinates.
/// R = galactocentric radius [kpc], Z = height above midplane [kpc],
/// VR = radial velocity [km/s], VZ = vertical velocity [km/s], VPhi = azimuthal velocity [km/s].
/// </summary>
public readonly record struct StarPhaseSpace(
    double R,
    double Z,
    double VR,
    double VZ,
    double VPhi);

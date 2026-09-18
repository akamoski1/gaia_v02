using Xunit;
using gaia_v02.Experiments;

namespace gaia_v02.Tests;

public class Experiment01ParametersValidationTests
{
    [Fact]
    public void Validate_ValidParameters_DoesNotThrow()
    {
        var p = Experiment01Parameters.Default;
        p.Validate(); // Should not throw
    }

    [Fact]
    public void Validate_ZeroStarCount_ThrowsArgumentException()
    {
        var p = Experiment01Parameters.Default with { StarCount = 0 };
        Assert.Throws<ArgumentException>(() => p.Validate());
    }

    [Fact]
    public void Validate_NegativeStarCount_ThrowsArgumentException()
    {
        var p = Experiment01Parameters.Default with { StarCount = -100 };
        Assert.Throws<ArgumentException>(() => p.Validate());
    }

    [Fact]
    public void Validate_R0OutOfRange_ThrowsArgumentException()
    {
        var p = Experiment01Parameters.Default with { R0 = 0.05 };
        Assert.Throws<ArgumentException>(() => p.Validate());
    }

    [Fact]
    public void Validate_NegativeSigmaR_ThrowsArgumentException()
    {
        var p = Experiment01Parameters.Default with { SigmaR = -10 };
        Assert.Throws<ArgumentException>(() => p.Validate());
    }

    [Fact]
    public void Validate_NegativeSigmaZ_ThrowsArgumentException()
    {
        var p = Experiment01Parameters.Default with { SigmaZ = -5 };
        Assert.Throws<ArgumentException>(() => p.Validate());
    }

    [Fact]
    public void Validate_ZeroCellDeltaR_ThrowsArgumentException()
    {
        var p = Experiment01Parameters.Default with { CellDeltaR = 0 };
        Assert.Throws<ArgumentException>(() => p.Validate());
    }

    [Fact]
    public void Validate_ZeroCellDeltaZ_ThrowsArgumentException()
    {
        var p = Experiment01Parameters.Default with { CellDeltaZ = 0 };
        Assert.Throws<ArgumentException>(() => p.Validate());
    }

    [Fact]
    public void Validate_ZeroTimeoutMinutes_ThrowsArgumentException()
    {
        var p = Experiment01Parameters.Default with { TimeoutMinutes = 0 };
        Assert.Throws<ArgumentException>(() => p.Validate());
    }

    [Fact]
    public void RandomSeed_DefaultValue_Is42()
    {
        var p = Experiment01Parameters.Default;
        Assert.NotNull(p.RandomSeed);
        Assert.Equal(42, p.RandomSeed.Value);
    }

    [Fact]
    public void RandomSeed_CanBeSetToNull()
    {
        var p = Experiment01Parameters.Default with { RandomSeed = null };
        Assert.Null(p.RandomSeed);
    }
}

using FPVPulse.Core.Data;

namespace FPVPulse.Core.Tests.Data;

public class RacePilotTests
{
    [Fact]
    public void RacePilot_CanBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var racePilot = new RacePilot();

        // Assert
        Assert.Equal(Guid.Empty, racePilot.RacePilotId);
        Assert.Equal(Guid.Empty, racePilot.RaceId);
        Assert.Equal(Guid.Empty, racePilot.PilotId);
        Assert.Equal(Guid.Empty, racePilot.QualifyingRaceId);
        Assert.False(racePilot.IsCheckedIn);
        Assert.Equal((uint)0, racePilot.StartPosition);
        Assert.Equal((uint)0, racePilot.QualifyingPosition);
        Assert.Equal((uint)0, racePilot.FinishingPosition);
        Assert.False(racePilot.IsAdvaning);
        Assert.False(racePilot.IsDead);
        Assert.Equal((uint)0, racePilot.FlasStartCount);
        Assert.Equal(RacePilotFlag.None, racePilot.Flags);
    }

    [Fact]
    public void RacePilot_CanSetAllProperties()
    {
        // Arrange
        var racePilotId = Guid.NewGuid();
        var raceId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var qualifyingRaceId = Guid.NewGuid();

        // Act
        var racePilot = new RacePilot
        {
            RacePilotId = racePilotId,
            RaceId = raceId,
            PilotId = pilotId,
            QualifyingRaceId = qualifyingRaceId,
            IsCheckedIn = true,
            StartPosition = 2,
            QualifyingPosition = 3,
            FinishingPosition = 1,
            IsAdvaning = true,
            IsDead = false,
            FlasStartCount = 1,
            Flags = RacePilotFlag.IronMan
        };

        // Assert
        Assert.Equal(racePilotId, racePilot.RacePilotId);
        Assert.Equal(raceId, racePilot.RaceId);
        Assert.Equal(pilotId, racePilot.PilotId);
        Assert.Equal(qualifyingRaceId, racePilot.QualifyingRaceId);
        Assert.True(racePilot.IsCheckedIn);
        Assert.Equal((uint)2, racePilot.StartPosition);
        Assert.Equal((uint)3, racePilot.QualifyingPosition);
        Assert.Equal((uint)1, racePilot.FinishingPosition);
        Assert.True(racePilot.IsAdvaning);
        Assert.False(racePilot.IsDead);
        Assert.Equal((uint)1, racePilot.FlasStartCount);
        Assert.Equal(RacePilotFlag.IronMan, racePilot.Flags);
    }

    [Theory]
    [InlineData(RacePilotFlag.None)]
    [InlineData(RacePilotFlag.IronMan)]
    public void RacePilot_CanSetDifferentFlags(RacePilotFlag flag)
    {
        // Arrange & Act
        var racePilot = new RacePilot { Flags = flag };

        // Assert
        Assert.Equal(flag, racePilot.Flags);
    }

    [Fact]
    public void RacePilot_CanCheckIn()
    {
        // Arrange
        var racePilot = new RacePilot();

        // Act
        racePilot.IsCheckedIn = true;

        // Assert
        Assert.True(racePilot.IsCheckedIn);
    }

    [Fact]
    public void RacePilot_CanTrackFalseStarts()
    {
        // Arrange
        var racePilot = new RacePilot();

        // Act
        racePilot.FlasStartCount = 2;

        // Assert
        Assert.Equal((uint)2, racePilot.FlasStartCount);
    }
}

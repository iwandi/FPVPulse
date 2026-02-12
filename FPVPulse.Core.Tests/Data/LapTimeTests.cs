using FPVPulse.Core.Data;

namespace FPVPulse.Core.Tests.Data;

public class LapTimeTests
{
    [Fact]
    public void LapTime_CanBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var lapTime = new LapTime();

        // Assert
        Assert.Equal(Guid.Empty, lapTime.LapTimeId);
        Assert.Equal(Guid.Empty, lapTime.RacePilotId);
        Assert.Equal((uint)0, lapTime.LapNumber);
        Assert.Equal(TimeSpan.Zero, lapTime.Time);
        Assert.Equal((uint)0, lapTime.Position);
        Assert.Equal(TimeSpan.Zero, lapTime.DeltaAhead);
        Assert.Equal(TimeSpan.Zero, lapTime.DeltaBehind);
        Assert.Equal(LapTimeFlag.None, lapTime.Flags);
    }

    [Fact]
    public void LapTime_CanSetAllProperties()
    {
        // Arrange
        var lapTimeId = Guid.NewGuid();
        var racePilotId = Guid.NewGuid();
        var time = TimeSpan.FromSeconds(45.5);
        var deltaAhead = TimeSpan.FromSeconds(2.3);
        var deltaBehind = TimeSpan.FromSeconds(1.2);

        // Act
        var lapTime = new LapTime
        {
            LapTimeId = lapTimeId,
            RacePilotId = racePilotId,
            LapNumber = 3,
            Time = time,
            Position = 2,
            DeltaAhead = deltaAhead,
            DeltaBehind = deltaBehind,
            Flags = LapTimeFlag.FastestRaceLap
        };

        // Assert
        Assert.Equal(lapTimeId, lapTime.LapTimeId);
        Assert.Equal(racePilotId, lapTime.RacePilotId);
        Assert.Equal((uint)3, lapTime.LapNumber);
        Assert.Equal(time, lapTime.Time);
        Assert.Equal((uint)2, lapTime.Position);
        Assert.Equal(deltaAhead, lapTime.DeltaAhead);
        Assert.Equal(deltaBehind, lapTime.DeltaBehind);
        Assert.Equal(LapTimeFlag.FastestRaceLap, lapTime.Flags);
    }

    [Theory]
    [InlineData(LapTimeFlag.None)]
    [InlineData(LapTimeFlag.FastestRaceLap)]
    [InlineData(LapTimeFlag.FastestStageLap)]
    [InlineData(LapTimeFlag.FastestCompetitionLap)]
    public void LapTime_CanSetDifferentFlags(LapTimeFlag flag)
    {
        // Arrange & Act
        var lapTime = new LapTime { Flags = flag };

        // Assert
        Assert.Equal(flag, lapTime.Flags);
    }

    [Fact]
    public void LapTime_CanCombineFlags()
    {
        // Arrange
        var combinedFlags = LapTimeFlag.FastestRaceLap | LapTimeFlag.FastestStageLap;

        // Act
        var lapTime = new LapTime { Flags = combinedFlags };

        // Assert
        Assert.Equal(combinedFlags, lapTime.Flags);
        Assert.True(lapTime.Flags.HasFlag(LapTimeFlag.FastestRaceLap));
        Assert.True(lapTime.Flags.HasFlag(LapTimeFlag.FastestStageLap));
    }
}

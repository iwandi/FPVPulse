using FPVPulse.Core.Data;

namespace FPVPulse.Core.Tests.Data;

public class RaceTests
{
    [Fact]
    public void Race_CanBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var race = new Race();

        // Assert
        Assert.Equal(Guid.Empty, race.RaceId);
        Assert.Equal(Guid.Empty, race.ReRunRaceId);
        Assert.Equal(Guid.Empty, race.StageId);
        Assert.Equal(string.Empty, race.Name);
        Assert.Equal(DateTime.MinValue, race.ScheduledStartTime);
        Assert.Equal(DateTime.MinValue, race.StartTime);
        Assert.Equal(DateTime.MinValue, race.EndTime);
        Assert.False(race.IsCompleted);
        Assert.False(race.IsInvalid);
    }

    [Fact]
    public void Race_CanSetAllProperties()
    {
        // Arrange
        var raceId = Guid.NewGuid();
        var reRunRaceId = Guid.NewGuid();
        var stageId = Guid.NewGuid();
        var scheduledStart = DateTime.UtcNow.AddHours(1);
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow.AddMinutes(5);

        // Act
        var race = new Race
        {
            RaceId = raceId,
            ReRunRaceId = reRunRaceId,
            StageId = stageId,
            Name = "Final Heat 1",
            ScheduledStartTime = scheduledStart,
            StartTime = startTime,
            EndTime = endTime,
            IsCompleted = true,
            IsInvalid = false
        };

        // Assert
        Assert.Equal(raceId, race.RaceId);
        Assert.Equal(reRunRaceId, race.ReRunRaceId);
        Assert.Equal(stageId, race.StageId);
        Assert.Equal("Final Heat 1", race.Name);
        Assert.Equal(scheduledStart, race.ScheduledStartTime);
        Assert.Equal(startTime, race.StartTime);
        Assert.Equal(endTime, race.EndTime);
        Assert.True(race.IsCompleted);
        Assert.False(race.IsInvalid);
    }

    [Fact]
    public void Race_CanBeMarkedAsCompleted()
    {
        // Arrange
        var race = new Race();

        // Act
        race.IsCompleted = true;

        // Assert
        Assert.True(race.IsCompleted);
    }

    [Fact]
    public void Race_CanBeMarkedAsInvalid()
    {
        // Arrange
        var race = new Race();

        // Act
        race.IsInvalid = true;

        // Assert
        Assert.True(race.IsInvalid);
    }
}

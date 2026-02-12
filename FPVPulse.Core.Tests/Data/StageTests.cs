using FPVPulse.Core.Data;

namespace FPVPulse.Core.Tests.Data;

public class StageTests
{
    [Fact]
    public void Stage_CanBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var stage = new Stage();

        // Assert
        Assert.Equal(Guid.Empty, stage.StageId);
        Assert.Equal(Guid.Empty, stage.CompetitionId);
        Assert.Equal(Guid.Empty, stage.RuleSetId);
        Assert.Equal(StageType.Practice, stage.StageType);
        Assert.Equal(RaceMode.Continues, stage.RaceMode);
    }

    [Fact]
    public void Stage_CanSetAllProperties()
    {
        // Arrange
        var stageId = Guid.NewGuid();
        var competitionId = Guid.NewGuid();
        var ruleSetId = Guid.NewGuid();

        // Act
        var stage = new Stage
        {
            StageId = stageId,
            CompetitionId = competitionId,
            RuleSetId = ruleSetId,
            StageType = StageType.Final,
            RaceMode = RaceMode.SheduledRaces
        };

        // Assert
        Assert.Equal(stageId, stage.StageId);
        Assert.Equal(competitionId, stage.CompetitionId);
        Assert.Equal(ruleSetId, stage.RuleSetId);
        Assert.Equal(StageType.Final, stage.StageType);
        Assert.Equal(RaceMode.SheduledRaces, stage.RaceMode);
    }

    [Theory]
    [InlineData(StageType.Practice)]
    [InlineData(StageType.Qualifying)]
    [InlineData(StageType.Final)]
    public void Stage_CanSetDifferentStageTypes(StageType stageType)
    {
        // Arrange & Act
        var stage = new Stage { StageType = stageType };

        // Assert
        Assert.Equal(stageType, stage.StageType);
    }

    [Theory]
    [InlineData(RaceMode.Continues)]
    [InlineData(RaceMode.SheduledRaces)]
    [InlineData(RaceMode.AdHocRaces)]
    public void Stage_CanSetDifferentRaceModes(RaceMode raceMode)
    {
        // Arrange & Act
        var stage = new Stage { RaceMode = raceMode };

        // Assert
        Assert.Equal(raceMode, stage.RaceMode);
    }
}

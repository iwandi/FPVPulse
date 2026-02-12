using FPVPulse.Core.Data;

namespace FPVPulse.Core.Tests.Data;

public class RuleSetTests
{
    [Fact]
    public void RuleSet_CanBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var ruleSet = new RuleSet();

        // Assert
        Assert.Equal(Guid.Empty, ruleSet.RuleSetId);
        Assert.Equal(string.Empty, ruleSet.Name);
        Assert.Equal(string.Empty, ruleSet.Description);
        Assert.Equal((uint)0, ruleSet.PilotsPerRace);
        Assert.Equal((uint)0, ruleSet.BumpUpCount);
        Assert.Equal((uint)0, ruleSet.Lives);
        Assert.Equal((uint)0, ruleSet.BestOfCount);
        Assert.Equal(RuleSetShape.None, ruleSet.Shape);
        Assert.Equal(RuleSetFlag.None, ruleSet.Flags);
    }

    [Fact]
    public void RuleSet_CanSetAllProperties()
    {
        // Arrange
        var ruleSetId = Guid.NewGuid();

        // Act
        var ruleSet = new RuleSet
        {
            RuleSetId = ruleSetId,
            Name = "Standard Racing",
            Description = "Standard FPV racing rules",
            PilotsPerRace = 6,
            BumpUpCount = 2,
            Lives = 3,
            BestOfCount = 3,
            Shape = RuleSetShape.Ladder,
            Flags = RuleSetFlag.IronManMode
        };

        // Assert
        Assert.Equal(ruleSetId, ruleSet.RuleSetId);
        Assert.Equal("Standard Racing", ruleSet.Name);
        Assert.Equal("Standard FPV racing rules", ruleSet.Description);
        Assert.Equal((uint)6, ruleSet.PilotsPerRace);
        Assert.Equal((uint)2, ruleSet.BumpUpCount);
        Assert.Equal((uint)3, ruleSet.Lives);
        Assert.Equal((uint)3, ruleSet.BestOfCount);
        Assert.Equal(RuleSetShape.Ladder, ruleSet.Shape);
        Assert.Equal(RuleSetFlag.IronManMode, ruleSet.Flags);
    }

    [Theory]
    [InlineData(RuleSetShape.None)]
    [InlineData(RuleSetShape.Ladder)]
    [InlineData(RuleSetShape.Tree)]
    public void RuleSet_CanSetDifferentShapes(RuleSetShape shape)
    {
        // Arrange & Act
        var ruleSet = new RuleSet { Shape = shape };

        // Assert
        Assert.Equal(shape, ruleSet.Shape);
    }

    [Theory]
    [InlineData(RuleSetFlag.None)]
    [InlineData(RuleSetFlag.IronManMode)]
    public void RuleSet_CanSetDifferentFlags(RuleSetFlag flag)
    {
        // Arrange & Act
        var ruleSet = new RuleSet { Flags = flag };

        // Assert
        Assert.Equal(flag, ruleSet.Flags);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    public void RuleSet_CanSetDifferentPilotCounts(uint pilotCount)
    {
        // Arrange & Act
        var ruleSet = new RuleSet { PilotsPerRace = pilotCount };

        // Assert
        Assert.Equal(pilotCount, ruleSet.PilotsPerRace);
    }
}

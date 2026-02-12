using FPVPulse.Core.Data;

namespace FPVPulse.Core.Tests.Data;

public class CompetitionTests
{
    [Fact]
    public void Competition_CanBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var competition = new Competition();

        // Assert
        Assert.Equal(Guid.Empty, competition.CompetitionId);
        Assert.Equal(Guid.Empty, competition.EventId);
    }

    [Fact]
    public void Competition_CanSetProperties()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        // Act
        var competition = new Competition
        {
            CompetitionId = competitionId,
            EventId = eventId
        };

        // Assert
        Assert.Equal(competitionId, competition.CompetitionId);
        Assert.Equal(eventId, competition.EventId);
    }
}

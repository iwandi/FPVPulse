using FPVPulse.Core.Data;

namespace FPVPulse.Core.Tests.Data;

public class EventTests
{
    [Fact]
    public void Event_CanBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var evt = new Event();

        // Assert
        Assert.Equal(Guid.Empty, evt.EventId);
    }

    [Fact]
    public void Event_CanSetEventId()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var evt = new Event
        {
            EventId = eventId
        };

        // Assert
        Assert.Equal(eventId, evt.EventId);
    }
}

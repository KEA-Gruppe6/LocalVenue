using LocalVenue.Core;
using LocalVenue.Core.Entities;
using LocalVenue.Core.Services;
using Moq;
using Xunit;

namespace LocalVenue.Tests;

public class SeatServiceTest
{
    private readonly SeatService _seatService;
    private readonly Mock<VenueContext> _venueContextMock;

    public SeatServiceTest()
    {
        _venueContextMock = new Mock<VenueContext>();
        _seatService = new SeatService(_venueContextMock.Object);
    }

    [Fact]
    public async Task TestSeatServiceGetById()
    {
        // Arrange
        var seatId = 1;
        var expectedSeat = new Seat { SeatId = seatId, Section = "Front", Row = 1, Number = 1 };

        _venueContextMock.Setup(c => c.Seats).Returns(new List<Seat> { expectedSeat });

        // Act
        var result = await _seatService.GetSeat(seatId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSeat.SeatId, result.SeatId);
        Assert.Equal(expectedSeat.Section, result.Section);
        Assert.Equal(expectedSeat.Row, result.Row);
        Assert.Equal(expectedSeat.Number, result.Number);
    }
}
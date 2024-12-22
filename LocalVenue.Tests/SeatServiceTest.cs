using LocalVenue.Core;
using LocalVenue.Core.Entities;
using LocalVenue.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LocalVenue.Tests;

public class SeatServiceTest
{
    private readonly DbContextOptions<VenueContext> _options;
    public SeatServiceTest()
    {
        _options = new DbContextOptionsBuilder<VenueContext>()
            .UseInMemoryDatabase("LocalVenue")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }
    
    [Fact]
    public async Task TestSeatServiceGetById()
    {
        await using var context = new VenueContext(_options);
        
        var service = new SeatService(context);
        
        var seatId = 1;
        
        context.Seats.Add(new Seat { SeatId = seatId, Section = "Front", Row = 1, Number = 1 });

        await context.SaveChangesAsync();
        
        // Arrange
  
        var expectedSeat = new Seat { SeatId = seatId, Section = "Front", Row = 1, Number = 1 };
        // Act
        var result = await service.GetSeat(seatId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSeat.SeatId, result.SeatId);
        Assert.Equal(expectedSeat.Section, result.Section);
        Assert.Equal(expectedSeat.Row, result.Row);
        Assert.Equal(expectedSeat.Number, result.Number);
    }
}
using LocalVenue.Core;
using LocalVenue.Core.Entities;
using LocalVenue.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LocalVenue.Tests;

public class SeatServiceTest
{
    private readonly ServiceProvider serviceProvider;

    public SeatServiceTest()
    {
        var services = new ServiceCollection();
        services.AddDbContextFactory<VenueContext>(options =>
        {
            options.UseInMemoryDatabase("InMemoryDb")
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        });

        serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task TestSeatServiceGetById()
    {
        // Arrange
        var seatId = 1;
        var expectedSeat = new Seat { SeatId = seatId, Section = "Front", Row = 1, Number = 1 };

        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<VenueContext>>();
        
        await using (var context = await dbContextFactory.CreateDbContextAsync())
        {
            context.Seats.Add(expectedSeat);
            await context.SaveChangesAsync();
        }

        // Act
        var contextFactoryRetrieve = serviceProvider.GetRequiredService<IDbContextFactory<VenueContext>>();
    
        var service = new SeatService(contextFactoryRetrieve);
        var result = await service.GetSeat(seatId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSeat.SeatId, result.SeatId);
        Assert.Equal(expectedSeat.Section, result.Section);
        Assert.Equal(expectedSeat.Row, result.Row);
        Assert.Equal(expectedSeat.Number, result.Number);
        
    }

    [Theory]
    [InlineData(2, "Front", 1, 2)]
    public async Task TestSeatServiceAddSeat(int seatId, string section, int row, int number)
    {
        var expectedSeat = new Seat { SeatId = seatId, Section = section, Row = row, Number = number };

        var contextFactoryRetrieve = serviceProvider.GetRequiredService<IDbContextFactory<VenueContext>>();

        var service = new SeatService(contextFactoryRetrieve);
        var result = await service.AddSeat(expectedSeat);

        Assert.NotNull(result);
        Assert.Equal(expectedSeat.SeatId, result.SeatId);
        Assert.Equal(expectedSeat.Section, result.Section);
        Assert.Equal(expectedSeat.Row, result.Row);
        Assert.Equal(expectedSeat.Number, result.Number);
    }

    [Fact]
    public async Task TestSeatServiceDeleteSeatSuccess()
    {
        // Arrange
        var seatId = 9999;
        var expectedSeat = new Seat { SeatId = seatId, Section = "Front", Row = 1, Number = 1 };

        await AddSeat(expectedSeat);

        var contextFactoryRetrieve = serviceProvider.GetRequiredService<IDbContextFactory<VenueContext>>();

        var service = new SeatService(contextFactoryRetrieve);

        // Act
        var result = await service.DeleteSeat(seatId);

        Assert.NotNull(result);
        Assert.Equal(expectedSeat.SeatId, result.SeatId);
        Assert.Equal(expectedSeat.Section, result.Section);
        Assert.Equal(expectedSeat.Row, result.Row);
        Assert.Equal(expectedSeat.Number, result.Number);
    }

    [Fact]
    public async Task TestSeatServiceDeleteSeatNotFoundFail()
    {
        // Arrange
        var seatId = int.MaxValue;

        var contextFactoryRetrieve = serviceProvider.GetRequiredService<IDbContextFactory<VenueContext>>();

        var service = new SeatService(contextFactoryRetrieve);

        // Act && Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteSeat(seatId));
    }


    private async Task<Seat> AddSeat(Seat expectedSeat)
    {
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<VenueContext>>();

        await using (var context = await dbContextFactory.CreateDbContextAsync())
        {
            expectedSeat = context.Seats.Add(expectedSeat).Entity;
            await context.SaveChangesAsync();
        }

        return expectedSeat;
    }
}
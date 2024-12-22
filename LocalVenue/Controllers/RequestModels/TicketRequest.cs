using LocalVenue.Core.Enums;

namespace LocalVenue.RequestModels;

public class TicketRequest
{
    public long ShowId { get; set; }

    public long SeatId { get; set; }

    public decimal Price { get; set; }

    public Status Status { get; set; }

    public string? CustomerId { get; set; }
}
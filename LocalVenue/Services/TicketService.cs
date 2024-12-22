using LocalVenue.Core.Entities;
using LocalVenue.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LocalVenue.Core.Services;

public class TicketService : GenericCRUDService<Ticket>, ITicketService
{
    private readonly VenueContext _context;

    public TicketService(VenueContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Ticket> GetTicket(long id)
    {
        return await base.GetItem(id, ticket => ticket.Show!, ticket => ticket.Customer!, ticket => ticket.Seat!);
    }

    public async Task<Ticket> AddTicket(Ticket ticket)
    {
        //Check if the ticket's seat, customer, and show exist
        if (await _context.Seats.FindAsync(ticket.SeatId) == null)
        {
            throw new ArgumentException($"Seat with id '{ticket.SeatId}' does not exist");
        }
        if (!string.IsNullOrWhiteSpace(ticket.CustomerId))
        {
            if (await _context.Users.FindAsync(ticket.CustomerId) == null)
            {
                throw new ArgumentException($"Customer with id '{ticket.CustomerId}' does not exist");
            }
        }
        else
        {
            ticket.CustomerId = null;
        }
        if (await _context.Shows.FindAsync(ticket.ShowId) == null)
        {
            throw new ArgumentException($"Show with id '{ticket.ShowId}' does not exist");
        }

        //Check if the ticket for show and seat already exists
        if (await _context.Tickets.AnyAsync(t => t.ShowId == ticket.ShowId && t.SeatId == ticket.SeatId))
        {
            throw new ArgumentException($"Ticket for show '{ticket.ShowId}' already has seat '{ticket.SeatId}' assigned");
        }

        return await base.AddItem(ticket, ticket => ticket.Show!, ticket => ticket.Customer!, ticket => ticket.Seat!);
    }

    public async Task<Ticket> UpdateTicket(Ticket ticket)
    {
        if (await _context.Seats.FindAsync(ticket.SeatId) == null)
        {
            throw new ArgumentException($"Seat with id '{ticket.SeatId}' does not exist");
        }
        if (!string.IsNullOrWhiteSpace(ticket.CustomerId))
        {
            if (await _context.Users.FindAsync(ticket.CustomerId) == null)
            {
                throw new ArgumentException($"Customer with id '{ticket.CustomerId}' does not exist");
            }
        }
        else
        {
            ticket.CustomerId = null;
        }
        if (await _context.Shows.FindAsync(ticket.ShowId) == null)
        {
            throw new ArgumentException($"Show with id '{ticket.ShowId}' does not exist");
        }

        return await base.UpdateItem(ticket, ticket => ticket.Show!, ticket => ticket.Customer!, ticket => ticket.Seat!);
    }

    public async Task<Ticket> DeleteTicket(long id)
    {
        return await base.DeleteItem(id, ticket => ticket.Show!, ticket => ticket.Customer!, ticket => ticket.Seat!);
    }
}
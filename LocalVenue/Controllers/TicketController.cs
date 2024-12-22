using AutoMapper;
using LocalVenue.Core.Entities;
using LocalVenue.Core.Interfaces;
using LocalVenue.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LocalVenue.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IMapper _mapper;

    public TicketController(ITicketService ticketService, IMapper mapper)
    {
        _ticketService = ticketService;
        _mapper = mapper;
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Gets a ticket by ID", Description = "Retrieves a specific ticket by its ID.")]
    [SwaggerResponse(200, "Returns the ticket", typeof(Ticket))]
    [SwaggerResponse(404, "If the ticket is not found", typeof(string))]
    [SwaggerResponse(400, "If there is an error", typeof(string))]
    public async Task<ActionResult<Ticket>> GetTicket(long id)
    {
        try
        {
            var ticket = await _ticketService.GetTicket(id);
            return Ok(ticket);
        }
        catch (Exception e)
        {
            if (e is KeyNotFoundException)
            {
                return NotFound($"Ticket with id {id} not found");
            }
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Creates a new ticket", Description = "Creates a new ticket with the provided details.")]
    [SwaggerResponse(201, "Returns the created ticket", typeof(Ticket))]
    [SwaggerResponse(400, "If there is an error", typeof(string))]
    [SwaggerResponse(409, "If a ticket for assigned show with assigned seat exists", typeof(string))]
    public async Task<ActionResult<Ticket>> AddTicket(TicketRequest ticketRequestDTO)
    {
        var ticket = _mapper.Map<Ticket>(ticketRequestDTO);
        try
        {
            var createdTicket = await _ticketService.AddTicket(ticket);
            string host = HttpContext.Request.Host.Value;
            string uri = $"{Request.Path}/{createdTicket.TicketId}";
            return Created(uri, createdTicket);
        }
        catch (Exception e)
        {
            if (e.Message.Contains($"Ticket for show '{ticket.ShowId}' already has seat '{ticket.SeatId}' assigned"))
            {
                return Conflict(e.Message);
            }
            return BadRequest(e.Message);
        }
    }

    [HttpPut]
    [SwaggerOperation(Summary = "Updates an existing ticket", Description = "Updates the details of an existing ticket.")]
    [SwaggerResponse(200, "Returns the updated ticket", typeof(Ticket))]
    [SwaggerResponse(400, "If there is an error", typeof(string))]
    [SwaggerResponse(404, "If the ticket is not found", typeof(string))]
    public async Task<ActionResult<Ticket>> UpdateTicket([FromBody] TicketRequest ticketRequestDTO)
    {
        var ticket = _mapper.Map<Ticket>(ticketRequestDTO);
        try
        {
            var updatedTicket = await _ticketService.UpdateTicket(ticket);
            return Ok(updatedTicket);
        }
        catch (Exception e)
        {
            if (e is KeyNotFoundException)
            {
                return NotFound($"No ticket found with id {ticket.TicketId}");
            }
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Deletes a ticket", Description = "Deletes a specific ticket by its ID.")]
    [SwaggerResponse(200, "Returns the deleted ticket", typeof(Ticket))]
    [SwaggerResponse(400, "If there is an error", typeof(string))]
    public async Task<ActionResult<Ticket>> DeleteTicket(int id)
    {
        try
        {
            var ticket = await _ticketService.DeleteTicket(id);
            return Ok(ticket);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}

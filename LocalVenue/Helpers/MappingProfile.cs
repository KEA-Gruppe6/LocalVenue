using AutoMapper;
using LocalVenue.Core.Entities;
using LocalVenue.RequestModels;

namespace LocalVenue.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TicketRequest, Ticket>()
            .ForMember(dest => dest.TicketId, opt => opt.MapFrom(src => 0));

        CreateMap<ShowRequest, Show>()
            .ForMember(dest => dest.ShowId, opt => opt.MapFrom(src => 0));
    }
}
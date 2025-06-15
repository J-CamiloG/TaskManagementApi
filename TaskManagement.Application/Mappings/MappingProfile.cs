using AutoMapper;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // State mappings
        CreateMap<State, StateDto>();
        CreateMap<CreateStateDto, State>();
        CreateMap<UpdateStateDto, State>();

        // Task mappings
        CreateMap<TaskItem, TaskDto>()
            .ForMember(dest => dest.StateName, opt => opt.MapFrom(src => src.State.Name));
        CreateMap<CreateTaskDto, TaskItem>();
        CreateMap<UpdateTaskDto, TaskItem>();
    }
}
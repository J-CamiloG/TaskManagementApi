using AutoMapper;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Services;

public class StateService : IStateService
{
    private readonly IStateRepository _stateRepository;
    private readonly IMapper _mapper;

    public StateService(IStateRepository stateRepository, IMapper mapper)
    {
        _stateRepository = stateRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StateDto>> GetAllAsync()
    {
        var states = await _stateRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<StateDto>>(states);
    }

    public async Task<StateDto?> GetByIdAsync(int id)
    {
        var state = await _stateRepository.GetByIdAsync(id);
        return state == null ? null : _mapper.Map<StateDto>(state);
    }

    public async Task<StateDto> CreateAsync(CreateStateDto createStateDto)
    {
        // Validar que no exista un estado con el mismo nombre
        if (await _stateRepository.ExistsAsync(createStateDto.Name))
        {
            throw new InvalidOperationException($"Ya existe un estado con el nombre '{createStateDto.Name}'");
        }

        var state = _mapper.Map<State>(createStateDto);
        var createdState = await _stateRepository.CreateAsync(state);
        return _mapper.Map<StateDto>(createdState);
    }

    public async Task<StateDto> UpdateAsync(int id, UpdateStateDto updateStateDto)
    {
        var existingState = await _stateRepository.GetByIdAsync(id);
        if (existingState == null)
        {
            throw new KeyNotFoundException($"Estado con ID {id} no encontrado");
        }

        // Validar que no exista otro estado con el mismo nombre
        if (await _stateRepository.ExistsAsync(updateStateDto.Name) && 
            existingState.Name != updateStateDto.Name)
        {
            throw new InvalidOperationException($"Ya existe un estado con el nombre '{updateStateDto.Name}'");
        }

        _mapper.Map(updateStateDto, existingState);
        var updatedState = await _stateRepository.UpdateAsync(existingState);
        return _mapper.Map<StateDto>(updatedState);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var state = await _stateRepository.GetByIdAsync(id);
        if (state == null)
        {
            return false;
        }

        // Verificar que no tenga tareas asociadas
        if (await _stateRepository.HasTasksAsync(id))
        {
            throw new InvalidOperationException("No se puede eliminar el estado porque tiene tareas asociadas");
        }

        return await _stateRepository.DeleteAsync(id);
    }
}
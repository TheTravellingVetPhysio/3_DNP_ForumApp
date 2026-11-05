using System;
using ApiContracts;

namespace BlazorApp.Services;

public interface ISubforumService
{
    Task<List<SubforumDTO>> GetAllSubforumsAsync();

    Task<SubforumDTO> GetSubforumByIdAsync(int subforumId);

    Task<SubforumDTO> CreateSubforumAsync(CreateSubforumRequest request);

    Task DeleteSubforumAsync(int subforumId);
}

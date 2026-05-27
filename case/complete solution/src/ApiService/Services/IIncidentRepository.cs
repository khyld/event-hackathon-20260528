using AgenticIncidentService.ApiService.Models;

namespace AgenticIncidentService.ApiService.Services;

public interface IIncidentRepository
{
    IReadOnlyList<Incident> GetAll();
    Incident? GetById(string id);
    Incident Add(CreateIncidentRequest request);
}

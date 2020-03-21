using MismeAPI.Common.DTO.Request.Concept;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IConceptService
    {
        Task<IEnumerable<Concept>> GetConceptsAsync();

        Task<Concept> AddConceptAsync(int loggedUser, AddConceptRequest concept);

        Task DeleteConceptAsync(int loggedUser, int id);

        Task<Concept> EditConceptAsync(int loggedUser, UpdateConceptRequest concept, int id);

        Task ChangeConceptPollOrderAsync(int loggedUser, PollOrderRequest pollOrderRequest, int id);
    }
}
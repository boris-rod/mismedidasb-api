using MismeAPI.Data.Entities;
using MismeAPI.Data.Repository;
using System;
using System.Threading.Tasks;

namespace MismeAPI.Data.UoW
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> UserRepository { get; set; }
        IGenericRepository<PersonalData> PersonalDataRepository { get; set; }
        IGenericRepository<UserPersonalData> UserPersonalDataRepository { get; set; }
        IGenericRepository<Poll> PollRepository { get; set; }
        IGenericRepository<Question> QuestionRepository { get; set; }
        IGenericRepository<Answer> AnswerRepository { get; set; }
        IGenericRepository<UserAnswer> UserAnswerRepository { get; set; }

        Task<int> CommitAsync();
    }
}
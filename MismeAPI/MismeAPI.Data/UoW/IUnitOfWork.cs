using MismeAPI.Data.Entities;
using MismeAPI.Data.Repository;
using System;
using System.Threading.Tasks;

namespace MismeAPI.Data.UoW
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> UserRepository { get; set; }
        IGenericRepository<UserToken> UserTokenRepository { get; set; }

        IGenericRepository<Poll> PollRepository { get; set; }
        IGenericRepository<Question> QuestionRepository { get; set; }
        IGenericRepository<Answer> AnswerRepository { get; set; }
        IGenericRepository<UserAnswer> UserAnswerRepository { get; set; }
        IGenericRepository<Concept> ConceptRepository { get; set; }
        IGenericRepository<UserPoll> UserPollRepository { get; set; }
        IGenericRepository<Tag> TagRepository { get; set; }
        IGenericRepository<Dish> DishRepository { get; set; }
        IGenericRepository<DishTag> DishTagRepository { get; set; }
        IGenericRepository<Eat> EatRepository { get; set; }
        IGenericRepository<EatDish> EatDishRepository { get; set; }
        IGenericRepository<Device> DeviceRepository { get; set; }

        Task<int> CommitAsync();
    }
}
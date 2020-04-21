using MismeAPI.Data.Entities;
using MismeAPI.Data.Repository;
using System;
using System.Threading.Tasks;

namespace MismeAPI.Data.UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MismeContext _context;

        private bool disposed = false;

        public UnitOfWork(MismeContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            UserRepository = UserRepository ?? new GenericRepository<User>(_context);
            UserTokenRepository = UserTokenRepository ?? new GenericRepository<UserToken>(_context);
            PollRepository = PollRepository ?? new GenericRepository<Poll>(_context);
            AnswerRepository = AnswerRepository ?? new GenericRepository<Answer>(_context);
            QuestionRepository = QuestionRepository ?? new GenericRepository<Question>(_context);
            UserAnswerRepository = UserAnswerRepository ?? new GenericRepository<UserAnswer>(_context);
            ConceptRepository = ConceptRepository ?? new GenericRepository<Concept>(_context);
            UserConceptRepository = UserConceptRepository ?? new GenericRepository<UserConcept>(_context);
            TagRepository = TagRepository ?? new GenericRepository<Tag>(_context);
            DishRepository = DishRepository ?? new GenericRepository<Dish>(_context);
            DishTagRepository = DishTagRepository ?? new GenericRepository<DishTag>(_context);
            EatRepository = EatRepository ?? new GenericRepository<Eat>(_context);
            EatDishRepository = EatDishRepository ?? new GenericRepository<EatDish>(_context);
            DeviceRepository = DeviceRepository ?? new GenericRepository<Device>(_context);
            TipRepository = TipRepository ?? new GenericRepository<Tip>(_context);
            ReminderRepository = ReminderRepository ?? new GenericRepository<Reminder>(_context);
        }

        public IGenericRepository<User> UserRepository { get; set; }
        public IGenericRepository<UserToken> UserTokenRepository { get; set; }

        public IGenericRepository<Poll> PollRepository { get; set; }
        public IGenericRepository<Answer> AnswerRepository { get; set; }
        public IGenericRepository<Question> QuestionRepository { get; set; }
        public IGenericRepository<UserAnswer> UserAnswerRepository { get; set; }
        public IGenericRepository<Concept> ConceptRepository { get; set; }
        public IGenericRepository<UserConcept> UserConceptRepository { get; set; }
        public IGenericRepository<Tag> TagRepository { get; set; }
        public IGenericRepository<Dish> DishRepository { get; set; }
        public IGenericRepository<DishTag> DishTagRepository { get; set; }
        public IGenericRepository<Eat> EatRepository { get; set; }
        public IGenericRepository<EatDish> EatDishRepository { get; set; }
        public IGenericRepository<Device> DeviceRepository { get; set; }
        public IGenericRepository<Tip> TipRepository { get; set; }
        public IGenericRepository<Reminder> ReminderRepository { get; set; }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                this.disposed = true;
            }
        }
    }
}
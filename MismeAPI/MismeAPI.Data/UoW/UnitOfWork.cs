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
            PersonalDataRepository = PersonalDataRepository ?? new GenericRepository<PersonalData>(_context);
            UserPersonalDataRepository = UserPersonalDataRepository ?? new GenericRepository<UserPersonalData>(_context);
            PollRepository = PollRepository ?? new GenericRepository<Poll>(_context);
            AnswerRepository = AnswerRepository ?? new GenericRepository<Answer>(_context);
            QuestionRepository = QuestionRepository ?? new GenericRepository<Question>(_context);
            UserAnswerRepository = UserAnswerRepository ?? new GenericRepository<UserAnswer>(_context);
        }

        public IGenericRepository<User> UserRepository { get; set; }
        public IGenericRepository<PersonalData> PersonalDataRepository { get; set; }
        public IGenericRepository<UserPersonalData> UserPersonalDataRepository { get; set; }
        public IGenericRepository<Poll> PollRepository { get; set; }
        public IGenericRepository<Answer> AnswerRepository { get; set; }
        public IGenericRepository<Question> QuestionRepository { get; set; }
        public IGenericRepository<UserAnswer> UserAnswerRepository { get; set; }

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
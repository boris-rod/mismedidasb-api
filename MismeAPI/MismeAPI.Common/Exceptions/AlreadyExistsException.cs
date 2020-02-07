namespace MismeAPI.Common.Exceptions
{
    public class AlreadyExistsException : BaseException
    {
        public AlreadyExistsException(string msg) : base(msg)
        {
        }
    }
}
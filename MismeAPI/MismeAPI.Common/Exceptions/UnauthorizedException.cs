namespace MismeAPI.Common.Exceptions
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string msg) : base(msg)
        {
        }
    }
}
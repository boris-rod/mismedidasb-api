namespace MismeAPI.Common.Exceptions
{
    public class InvalidDataException : BaseException
    {
        public InvalidDataException(string msg, string field) : base(msg)
        {
            Field = field;
        }

        public InvalidDataException(string msg) : base(msg)
        {
        }

        public string Field { get; set; }
    }
}
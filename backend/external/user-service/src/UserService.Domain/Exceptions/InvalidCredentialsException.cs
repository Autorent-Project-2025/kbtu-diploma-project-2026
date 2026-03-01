namespace UserService.Domain.Exceptions
{
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException()
            : base("Invalid Credentials")
        {
        }

        public InvalidCredentialsException(string message)
            : base(message)
        {
        }
    }
}

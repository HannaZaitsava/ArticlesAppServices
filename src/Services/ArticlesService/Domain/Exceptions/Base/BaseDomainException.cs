namespace ArticlesService.Domain.Exceptions.Base
{
    public abstract class BaseDomainException : Exception
    {        
        protected BaseDomainException()
        { }

        protected BaseDomainException(string message)
            : base(message)
        { }

        protected BaseDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}

namespace CollectManagement.Application.Exceptions;

public class BadCredentialException : ApplicationException
{
    public BadCredentialException(string Message): base(Message)
    {
        
    }
}
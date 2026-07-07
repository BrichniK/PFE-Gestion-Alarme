namespace CollectManagement.Application.Exceptions;

public class UnAuthorizedException : ApplicationException
{
    public UnAuthorizedException(string Message): base(Message)
    {
        
    }
}
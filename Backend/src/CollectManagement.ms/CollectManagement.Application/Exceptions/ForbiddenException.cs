namespace CollectManagement.Application.Exceptions;

public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string Message): base(Message)
    {
        
    }
}
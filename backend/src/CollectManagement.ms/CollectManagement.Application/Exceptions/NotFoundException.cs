namespace CollectManagement.Application.Exceptions;

public class NotFoundException : ApplicationException
{
    public NotFoundException(string name, object key)
        : base($"{name} ({key}) is not found")
    {
    }

    public NotFoundException(string message) 
        : base(message)
    {
            
    }
}

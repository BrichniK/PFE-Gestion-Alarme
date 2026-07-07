namespace CollectManagement.Domain.Common;

public interface IStronglyTypedId
{
    Ulid Value { get; }
}
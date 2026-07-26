namespace CollectManagement.Domain.Common;

public interface IStronglyTypeId
{
    Ulid Value { get; }
}
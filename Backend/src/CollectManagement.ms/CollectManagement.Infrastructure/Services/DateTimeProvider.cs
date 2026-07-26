using CollectManagement.Application.Interfaces.Services;

namespace CollectManagement.Infrastructure.Services;

public class DateTimeProvider: IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}
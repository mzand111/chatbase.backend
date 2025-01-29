using MZBase.Infrastructure;
using System;

namespace ChatBase.Backend.Infrastructure;

public class DateTimeProviderService : IDateTimeProviderService
{
    public DateTime GetNow()
    {
        return DateTime.UtcNow;
    }
}

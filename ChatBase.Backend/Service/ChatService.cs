using Microsoft.Extensions.Logging;
using MZBase.Infrastructure.Service;

namespace ChatBase.Backend.Service;

public class ChatService : BaseBusinessService<ChatService>
{
    public ChatService(ILogger<ChatService> logger) : base(logger)
    {
    }
}

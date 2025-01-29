using ChatBase.Backend.Data.Chat.Outputs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBase.Backend.Infrastructure.Chat
{
    public interface IChatMessageRepository
    {
        Task<List<UserLatestMessages>> GetUserContactsListAsync(string userName);
    }
}

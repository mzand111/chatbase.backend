using ChatBase.Backend.Data.Chat;
using ChatBase.Backend.Domain.Chat;
using MZBase.Domain;
using MZBase.EntityFrameworkCore;
using MZBase.Infrastructure;

namespace ChatBase.Backend.Infrastructure.Chat;

public class ChatUnitOfWork : UnitOfWorkAsync<ChatDataContext>, IChatUnitOfWork
{
    public IChatMessageRepository ChatMessages { get; private set; }
    public ChatUnitOfWork(ChatDataContext dataContext) : base(dataContext)
    {
        ChatMessages = new ChatMessageRepository(_dbContext);
    }
    public ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimKey> GetRepo<TModel, TDBModel, PrimKey>()
            where TModel : Model<PrimKey>
            where TDBModel : TModel, IConvertibleDBModelEntity<TModel>, new()
            where PrimKey : struct
    {
        ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimKey> repo = null;

        if (typeof(TModel) == typeof(ChatMessage))
        {
            repo = ChatMessages as ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimKey>;

        }
        return repo;
    }
}

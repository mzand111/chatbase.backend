using MZBase.Infrastructure;

namespace ChatBase.Backend.Infrastructure.Chat
{
    public interface IChatUnitOfWork : IDynamicTestableUnitOfWorkAsync
    {
        IChatMessageRepository ChatMessages { get; }
    }
}

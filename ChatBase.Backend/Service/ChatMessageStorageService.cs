using ChatBase.Backend.Data.Chat;
using ChatBase.Backend.Domain.Chat;
using ChatBase.Backend.Infrastructure.Chat;
using Microsoft.Extensions.Logging;
using MZBase.EntityFrameworkCore;
using MZBase.Infrastructure;

namespace ChatBase.Backend.Service
{
    public class ChatMessageStorageService : EFCoreStorageBusinessService<ChatMessage, ChatMessageEntity, int, ChatUnitOfWork, ChatDataContext>
    {
        public ChatMessageStorageService(ChatUnitOfWork unitOfWork, IDateTimeProviderService dateTimeProvider, ILogger<ChatMessage> logger)
            : base(unitOfWork, dateTimeProvider, logger)
        {
        }
    }
    //public class ChatMessageStorageService : BaseStorageBusinessService<ChatMessage, int>
    //{
    //    private readonly ChatUnitOfWork _unitOfWork;
    //    private readonly ILDRCompatibleRepositoryAsync<ChatMessage, ChatMessageEntity, int> _baseRepo;

    //    public ChatMessageStorageService(ChatUnitOfWork unitOfWork,
    //          IDateTimeProviderService dateTimeProvider,
    //          ILogger<ChatMessage> logger
    //        ) : base(logger, dateTimeProvider, 600)
    //    {
    //        _unitOfWork = unitOfWork;
    //        _baseRepo = _unitOfWork.GetRepo<ChatMessage, ChatMessageEntity, int>();
    //    }
    //    public override async Task<int> AddAsync(ChatMessage item)
    //    {
    //        if (item == null)
    //        {
    //            var ex = new ServiceArgumentNullException("Input parameter was null:" + nameof(item));
    //            LogAdd(null, null, ex);
    //            throw ex;
    //        }

    //        await ValidateOnAddAsync(item);

    //        var g = _baseRepo.Insert(item);
    //        try
    //        {
    //            await _unitOfWork.CommitAsync();

    //            LogAdd(item, "Successfully add item with ,ID:" +
    //              g?.ID.ToString()

    //             );
    //            return g.ID;
    //        }
    //        catch (Exception ex)
    //        {
    //            LogAdd(item, "", ex);
    //            throw new ServiceStorageException("Error adding item", ex);
    //        }
    //    }

    //    public override async Task<LinqDataResult<ChatMessage>> ItemsAsync(LinqDataRequest request)
    //    {
    //        try
    //        {
    //            var f = await _baseRepo.AllItemsAsync(request);
    //            LogRetrieveMultiple(null, request);
    //            return f;
    //        }
    //        catch (Exception ex)
    //        {
    //            LogRetrieveMultiple(null, request, ex);
    //            throw new ServiceStorageException("Error retrieving items ", ex);
    //        }
    //    }

    //    public override async Task ModifyAsync(ChatMessage item)
    //    {
    //        if (item == null)
    //        {
    //            var exception = new ServiceArgumentNullException(typeof(ChatMessage).Name);
    //            LogModify(item, null, exception);
    //            throw exception;
    //        }

    //        var currentItem = await _baseRepo.GetByIdAsync(item.ID);
    //        if (currentItem == null)
    //        {
    //            var noObj = new ServiceObjectNotFoundException(typeof(ChatMessage).Name + " Not Found");
    //            LogModify(item, null, noObj);
    //            throw noObj;
    //        }
    //        await ValidateOnModifyAsync(item, currentItem);
    //        currentItem.SetFieldsFromDomainModel(item);
    //        try
    //        {
    //            await _unitOfWork.CommitAsync();
    //            LogModify(item, "Successfully modified item with ,ID:" +
    //               item.ID.ToString()
    //             );
    //        }

    //        catch (Exception ex)
    //        {
    //            LogModify(item, "", ex);
    //            throw new ServiceStorageException("Error modifying item", ex);
    //        }
    //    }

    //    public override async Task RemoveByIdAsync(int ID)
    //    {
    //        var itemToDelete = await _baseRepo.FirstOrDefaultAsync(ss => ss.ID == ID);

    //        if (itemToDelete == null)
    //        {
    //            var f = new ServiceObjectNotFoundException(typeof(ChatMessage).Name + " not found");
    //            LogRemove(ID, "Item With This Id Not Found", f);
    //            throw f;
    //        }
    //        _baseRepo.Delete(itemToDelete);
    //        try
    //        {
    //            await _unitOfWork.CommitAsync();
    //            LogRemove(ID, "Item Deleted Successfully", null);
    //        }

    //        catch (Exception ex)
    //        {
    //            var innerEx = new ServiceStorageException("Error deleting item with id" + ID.ToString(), ex);
    //            LogRemove(ID, null, ex);
    //            throw innerEx;
    //        }
    //    }

    //    public override async Task<ChatMessage> RetrieveByIdAsync(int ID)
    //    {
    //        ChatMessageEntity? item;
    //        try
    //        {
    //            item = await _baseRepo.FirstOrDefaultAsync(ss => ss.ID == ID);
    //        }
    //        catch (Exception ex)
    //        {
    //            LogRetrieveSingle(ID, ex);
    //            throw new ServiceStorageException("Error loading item", ex);
    //        }
    //        if (item == null)
    //        {
    //            var f = new ServiceObjectNotFoundException(typeof(ChatMessage).Name + " not found by id");
    //            LogRetrieveSingle(ID, f);
    //            throw f;
    //        }
    //        LogRetrieveSingle(ID);
    //        return item.GetDomainObject();
    //    }

    //    protected override async Task ValidateOnAddAsync(ChatMessage item)
    //    {

    //    }

    //    protected override async Task ValidateOnModifyAsync(ChatMessage recievedItem, ChatMessage storageItem)
    //    {

    //    }
    //}
}

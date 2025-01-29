using ChatBase.Backend.Data.Chat;
using ChatBase.Backend.Domain.Chat;
using ChatBase.Backend.Domain.Identity;
using ChatBase.Backend.Infrastructure.Chat;
using Microsoft.Extensions.Logging;
using MZBase.EntityFrameworkCore;
using MZBase.Infrastructure;
using MZBase.Infrastructure.Service.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBase.Backend.Service
{
    public class ChatMessageStorageService : EFCoreStorageBusinessService<ChatMessage, ChatMessageEntity, int, ChatUnitOfWork, ChatDataContext>
    {
        private readonly ApplicationUserManager _userManager;

        public ChatMessageStorageService(ChatUnitOfWork unitOfWork, IDateTimeProviderService dateTimeProvider, ILogger<ChatMessage> logger, ApplicationUserManager userManager)
            : base(unitOfWork, dateTimeProvider, logger)
        {
            _userManager = userManager;
        }

        protected override async Task ValidateOnAddAsync(ChatMessage item)
        {
            List<ModelFieldValidationResult> _validationErrors = new List<ModelFieldValidationResult>();
            await DoCommonValidationsAsync(_validationErrors, item);
            if (item.ReceiveTime.HasValue)
            {
                _validationErrors.Add(new ModelFieldValidationResult()
                {
                    Code = _logBaseID + 1,
                    FieldName = nameof(item.ReceiveTime),
                    ValidationMessage = "The Field Can Not Have Value When Being Added"
                });
            }
            if (item.ViewTime.HasValue)
            {
                _validationErrors.Add(new ModelFieldValidationResult()
                {
                    Code = _logBaseID + 1,
                    FieldName = nameof(item.ViewTime),
                    ValidationMessage = "The Field Can Not Have Value When Being Added"
                });
            }


            if (_validationErrors.Any())
            {
                var exp = new ServiceModelValidationException(_validationErrors, "Error validating the model");
                LogAdd(item, "Error in Adding item when validating:" + exp.JSONFormattedErrors, exp);
                throw exp;
            }
        }

        protected override Task ValidateOnModifyAsync(ChatMessage receivedItem, ChatMessage storageItem)
        {
            throw new System.NotImplementedException();
        }
        private async Task DoCommonValidationsAsync(List<ModelFieldValidationResult> validationErrors, ChatMessage item)
        {
            if (string.IsNullOrWhiteSpace(item.FromUserId))
            {
                validationErrors.Add(new ModelFieldValidationResult()
                {
                    Code = _logBaseID + 1,
                    FieldName = nameof(item.FromUserId),
                    ValidationMessage = "The Field Can Not Be Empty"
                });
            }
            else
            {
                var user = await _userManager.FindByNameAsync(item.FromUserId);
                if (user == null)
                {
                    validationErrors.Add(new ModelFieldValidationResult()
                    {
                        Code = _logBaseID + 1,
                        FieldName = nameof(item.FromUserId),
                        ValidationMessage = "Sender user could not be found"
                    });
                }
                else
                {
                    if (user.RemoveTime.HasValue)
                    {
                        validationErrors.Add(new ModelFieldValidationResult()
                        {
                            Code = _logBaseID + 1,
                            FieldName = nameof(item.FromUserId),
                            ValidationMessage = "Sender user is removed"
                        });
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(item.ToUserId))
            {
                validationErrors.Add(new ModelFieldValidationResult()
                {
                    Code = _logBaseID + 1,
                    FieldName = nameof(item.ToUserId),
                    ValidationMessage = "The Field Can Not Be Empty"
                });
            }
            else
            {
                var user = await _userManager.FindByNameAsync(item.ToUserId);
                if (user == null)
                {
                    validationErrors.Add(new ModelFieldValidationResult()
                    {
                        Code = _logBaseID + 1,
                        FieldName = nameof(item.ToUserId),
                        ValidationMessage = "Receiver user could not be found"
                    });
                }
                else
                {
                    if (user.RemoveTime.HasValue)
                    {
                        validationErrors.Add(new ModelFieldValidationResult()
                        {
                            Code = _logBaseID + 1,
                            FieldName = nameof(item.ToUserId),
                            ValidationMessage = "Receiver user is removed"
                        });
                    }
                }
            }
            if (item.ReplyToID.HasValue && item.ForwardID.HasValue)
            {
                validationErrors.Add(new ModelFieldValidationResult()
                {
                    Code = _logBaseID + 1,
                    FieldName = nameof(item.ReplyToID),
                    ValidationMessage = $"Both '{nameof(item.ReplyToID)}' and '{nameof(item.ForwardID)}', can not have value at the same time."
                });
            }
            else
            {
                if (item.ReplyToID.HasValue)
                {
                    var itemToReply = await _baseRepo.FirstOrDefaultAsync(uu => uu.ID == item.ReplyToID);
                    if (itemToReply == null)
                    {
                        validationErrors.Add(new ModelFieldValidationResult()
                        {
                            Code = _logBaseID + 1,
                            FieldName = nameof(item.ReplyToID),
                            ValidationMessage = $"Base massage addressed by '{nameof(item.ReplyToID)}' field was not found"
                        });
                    }
                    else
                    {
                        if (itemToReply.ToUserId != item.FromUserId)
                        {
                            validationErrors.Add(new ModelFieldValidationResult()
                            {
                                Code = _logBaseID + 1,
                                FieldName = nameof(item.ReplyToID),
                                ValidationMessage = $"Base massage addressed by '{nameof(item.ReplyToID)}' does not belong to the user"
                            });
                        }
                        if (itemToReply.FromUserId != item.ToUserId)
                        {
                            validationErrors.Add(new ModelFieldValidationResult()
                            {
                                Code = _logBaseID + 1,
                                FieldName = nameof(item.ReplyToID),
                                ValidationMessage = $"Base massage addressed by '{nameof(item.ReplyToID)}' is not sent by the user being replied"
                            });
                        }
                    }
                }
                if (item.ForwardID.HasValue)
                {
                    var itemToForward = await _baseRepo.FirstOrDefaultAsync(uu => uu.ID == item.ForwardID);
                    if (itemToForward == null)
                    {
                        validationErrors.Add(new ModelFieldValidationResult()
                        {
                            Code = _logBaseID + 1,
                            FieldName = nameof(item.ForwardID),
                            ValidationMessage = $"Base massage addressed by '{nameof(item.ForwardID)}' field was not found"
                        });
                    }
                    else
                    {
                        if (itemToForward.ToUserId != item.FromUserId)
                        {
                            validationErrors.Add(new ModelFieldValidationResult()
                            {
                                Code = _logBaseID + 1,
                                FieldName = nameof(item.ForwardID),
                                ValidationMessage = $"Base massage addressed by '{nameof(item.ForwardID)}' does not belong to the user"
                            });
                        }
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(item.Body))
            {
                validationErrors.Add(new ModelFieldValidationResult()
                {
                    Code = _logBaseID + 1,
                    FieldName = nameof(item.Body),
                    ValidationMessage = "The Field Can Not Be Empty"
                });
            }

        }
    }

}

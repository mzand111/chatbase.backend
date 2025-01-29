using ChatBase.Backend.Controllers.Base;
using ChatBase.Backend.Data.Chat.Outputs;
using ChatBase.Backend.Domain.Chat;
using ChatBase.Backend.Lib.Dto.Chat;
using ChatBase.Backend.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MZBase.Infrastructure;
using MZBase.Infrastructure.Service.Exceptions;
using MZSimpleDynamicLinq.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBase.Backend.Controllers;

[Route("[controller]")]
[ApiController]
public class ChatController : BaseController
{
    private readonly ChatMessageStorageService _storageService;
    private readonly IDateTimeProviderService _dateTimeProviderService;

    public ChatController(ChatMessageStorageService storageService, IDateTimeProviderService dateTimeProviderService)
    {
        _storageService = storageService;
        _dateTimeProviderService = dateTimeProviderService;
    }

    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Post(ChatMessage_Add message)
    {
        try
        {
            ChatMessage m = new ChatMessage();
            m.MetaData = message.MetaData;
            m.ForwardDetails = message.ForwardDetails;
            m.ForwardedFromGroup = message.ForwardedFromGroup;
            m.ForwardID = message.ForwardID;
            m.ToUserId = message.ToUserId;
            m.Body = message.Body;
            m.Type = message.Type;
            m.ForwardID = message.ForwardID;
            m.ReplyToID = message.ReplyTo;

            m.SendTime = _dateTimeProviderService.GetNow();
            m.FromUserId = UserName;

            var g = await _storageService.AddAsync(m);
            return Ok(g);
        }
        catch (ServiceException ex)
        {
            if (ex is ServiceModelValidationException)
            {
                return StatusCode(500, ex.Message + ", " + (ex as ServiceModelValidationException).JSONFormattedErrors);
            }
            return StatusCode(500, ex.ToServiceExceptionString());
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LinqDataResult<ChatMessage>>> GetAll(int pageSize, int pageNumber)
    {
        LinqDataRequest request = new LinqDataRequest
        {
            Skip = pageNumber,
            Take = pageSize,
        };
        var sort = new List<Sort>();
        sort.Add(new Sort()
        {
            Dir = "Desc",
            Field = "SendTime"
        });
        request.Sort = sort;
        try
        {
            var g = await _storageService.ItemsAsync(request);
            return Ok(g);
        }
        catch (ServiceException ex)
        {
            return StatusCode(500, ex.ToServiceExceptionString());
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("GetUserContactsList")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<UserLatestMessages>>> GetUserContactsList()
    {
        try
        {
            var g = await _storageService.GetUserContactsList(UserName);
            return Ok(g);
        }
        catch (ServiceException ex)
        {
            return StatusCode(500, ex.ToServiceExceptionString());
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

}
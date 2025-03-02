using ChatBase.Backend.Domain.Chat;
using ChatBase.Backend.Domain.Identity;
using ChatBase.Backend.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBase.Backend.Lib;

[SignalRHub]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SignalRHub : Hub
{
    // Client methods:
    // addChatMessage: Called when a new message is received.
    // addChatHistoryMessage: Called for each message in the history after the server method 'GetChatHistory' is called.
    // noMoreHistory: Called after the server method 'GetChatHistory' is called and all history messages have been sent to the client using the 'addChatHistoryMessage' method.
    // chatMessageReceived: Called to acknowledge message reception.
    // chatMessageSeen: Called when a message is seen by the receiver. The client should call the server method 'ConfirmSeen' when a message is seen by the receiver for this client method to be called.
    // isOnline
    // isOffline

    private readonly PresenceTracker _presenceTracker;
    private readonly ChatMessageStorageService _chatMessageStorageService;
    private readonly UserManager<ApplicationUser> _userManager;

    private string LoggedInUserName
        => this.Context.User.Identity.Name;

    public SignalRHub(PresenceTracker presenceTracker,
        ChatMessageStorageService chatMessageStorageService,
        UserManager<ApplicationUser> userManager)
    {
        _presenceTracker = presenceTracker;
        _chatMessageStorageService = chatMessageStorageService;
        _userManager = userManager;
    }

    [SignalRHidden]
    public override async Task OnConnectedAsync()
    {
        await _presenceTracker.ConnectionOpened(Context.User.Identity.Name, Context.ConnectionId);
        await base.OnConnectedAsync();
        await Clients.AllExcept(Context.User.Identity.Name).SendAsync("isOnline", Context.User.Identity.Name);
    }

    [SignalRHidden]
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _presenceTracker.ConnectionClosed(Context.User.Identity.Name, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
        await Clients.AllExcept(Context.User.Identity.Name).SendAsync("isOffline", Context.User.Identity.Name);
    }

    /// <summary>
    /// Sends a chat message to a specified user.
    /// </summary>
    /// <param name="who">The recipient of the message.</param>
    /// <param name="message">The message content.</param>
    public async Task SendChatMessage(string who, string message)
    {
        var whoConnectionId = await _presenceTracker.GetUserConnectionId(who);

        if (_userManager.Users.Any(uu => uu.UserName.ToLower() == who.ToLower()))
        {
            string userName = LoggedInUserName.ToLower();
            DateTime now = DateTime.Now;

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new InvalidOperationException("Message is empty");
            }

            ChatMessage prv = new ChatMessage()
            {
                FromUserId = userName.ToLower(),
                SendTime = now,
                Body = message,
                Type = ChatMessageType.Text,
                ToUserId = who.ToLower(),
            };

            var _id = await _chatMessageStorageService.AddAsync(prv);
            prv.ID = _id;

            if (whoConnectionId != null)
            {
                await Clients.Client(whoConnectionId).SendAsync("addChatMessage", prv.ID, userName.ToLower(), message, now.ToString("yyyy-MM-dd HH:mm"));
            }
            await Clients.Caller.SendAsync("chatMessageReceived", prv.ID, who.ToLower(), message, now);
        }
    }

    /// <summary>
    /// Retrieves the chat history with a specified user up to a certain date.
    /// </summary>
    /// <param name="who">The user to retrieve chat history with.</param>
    /// <param name="time">The date up to which to retrieve chat history.</param>
    public async Task GetChatHistory(string who, DateTimeOffset time)
    {
        string userName = LoggedInUserName;
        if (String.IsNullOrWhiteSpace(userName))
        {
            return;
        }

        DateTime dt = time.Date;
        string currentUser = userName.ToLower();
        who = who.ToLower();

        if (currentUser.ToLower() == who.ToLower())
        {
            return;
        }

        List<ChatMessage> mm = await _chatMessageStorageService.GetWithDate(who, currentUser, dt);

        if (mm.Count == 0)
        {
            await Clients.Caller.SendAsync("noMoreHistory");
        }
        else
        {
            foreach (var m in mm)
            {
                await Clients.Caller.SendAsync("addChatHistoryMessage", m.ID, m.FromUserId.ToLower(), m.ToUserId.ToLower(), m.Body, m.SendTime, m.Type,
                    m.ViewTime.HasValue ? 1 : 0, m.ForwardDetails);
            }
        }
    }

    /// <summary>
    /// Retrieves recent chat messages with a specified user starting from a certain message ID.
    /// </summary>
    /// <param name="who">The user to retrieve recent chat messages with.</param>
    /// <param name="fromID">The message ID to start retrieving from.</param>
    public async Task GetRecentChat(string who, int? fromID)
    {
        string userName = LoggedInUserName.ToLower();
        if (String.IsNullOrWhiteSpace(userName))
        {
            return;
        }

        string currentUser = userName.ToLower();
        who = who.ToLower();

        List<ChatMessage> mm = await _chatMessageStorageService.GetWithFromId(who, currentUser, fromID);
        if (mm.Count == 0)
        {
            await Clients.Caller.SendAsync("noMoreHistory", who);
        }
        else
        {
            foreach (ChatMessage m in mm)
            {
                await Clients.Caller.SendAsync("addChatHistoryMessage", m.ID, m.FromUserId.ToLower(), m.ToUserId.ToLower(), m.Body, m.SendTime.ToString("yyyy-MM-dd HH:mm"), m.ViewTime.HasValue ? 1 : 0, m.ForwardDetails);
            }
            await Clients.Caller.SendAsync("noMoreHistory", who);
        }
    }

    /// <summary>
    /// Confirms that a message has been seen by the receiver.
    /// </summary>
    /// <param name="ID">The ID of the message that has been seen.</param>
    public async Task ConfirmSeen(int ID)
    {
        string userName = LoggedInUserName.ToLower();
        if (String.IsNullOrWhiteSpace(userName))
        {
            return;
        }

        string currentUser = userName.ToLower();

        var viewdItem = await _chatMessageStorageService.RetrieveByIdAsync(ID);

        if (viewdItem != null)
        {
            if (viewdItem.ToUserId.ToLower() == currentUser.ToLower())
            {
                string who = viewdItem.FromUserId;

                List<ChatMessage> mm = await _chatMessageStorageService.GetUnreadWithFromUserName(ID, who, currentUser);
                DateTimeOffset date = DateTimeOffset.UtcNow;
                foreach (var m in mm)
                {
                    m.ViewTime = date;
                    await _chatMessageStorageService.ModifyAsync(m);
                    if (_presenceTracker.GetUserConnectionId(currentUser) != null)
                    {
                        await Clients.User(who).SendAsync("chatMessageSeen", m.ID, userName.ToLower(), 1);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the total count of unseen messages for the logged-in user.
    /// </summary>
    /// <returns>The total count of unseen messages.</returns>
    public async Task<int> GetTotalUnSeenCount()
    {
        string userName = LoggedInUserName.ToLower();
        if (String.IsNullOrWhiteSpace(userName))
        {
            return 0;
        }
        string currentUser = userName.ToLower();
        var g1 = await _chatMessageStorageService.UnReadMessageCount(currentUser);
        return g1;
    }
}


public class PresenceTracker
{
    private static readonly Dictionary<string, int> onlineUsers = new Dictionary<string, int>();
    private static Dictionary<string, string> connectionIds = new Dictionary<string, string>();

    public Task<ConnectionOpenedResult> ConnectionOpened(string userId, string connectionId)
    {
        bool joined = false;
        lock (onlineUsers)
        {
            if (onlineUsers.ContainsKey(userId))
            {
                onlineUsers[userId] += 1;
            }
            else
            {
                onlineUsers.Add(userId, 1);
                joined = true;
            }

            connectionIds[userId] = connectionId;
        }

        return Task.FromResult(new ConnectionOpenedResult { UserJoined = joined });
    }

    public Task<ConnectionClosedResult> ConnectionClosed(string userId, string connectionId)
    {
        bool left = false;
        lock (onlineUsers)
        {
            if (onlineUsers.ContainsKey(userId))
            {
                onlineUsers[userId] -= 1;
                if (onlineUsers[userId] <= 0)
                {
                    onlineUsers.Remove(userId);
                    left = true;
                }
            }

            if (connectionIds.ContainsKey(userId))
            {
                connectionIds.Remove(userId);
            }
        }

        return Task.FromResult(new ConnectionClosedResult { UserLeft = left });
    }

    public Task<string?> GetUserConnectionId(string userName)
    {
        lock (onlineUsers)
        {
            if (onlineUsers.ContainsKey(userName))
            {
                return Task.FromResult(connectionIds[userName]);
            }
            else
            {
                return Task.FromResult((string?)null);
            }
        }
    }

    public Task<string[]> GetOnLineUsers()
    {
        lock (onlineUsers)
        {
            return Task.FromResult(onlineUsers.Keys.ToArray());
        }
    }
    public Task<bool> IsOnLine(string userName)
    {
        lock (onlineUsers)
        {
            return Task.FromResult(onlineUsers.ContainsKey(userName));
        }
    }
}

public class ConnectionOpenedResult
{
    public bool UserJoined { get; set; }
}

public class ConnectionClosedResult
{
    public bool UserLeft { get; set; }
}

public class HubExceptionFilter : IHubFilter
{
    public async ValueTask<object> InvokeMethodAsync(
        HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        return await next(invocationContext);
    }
}
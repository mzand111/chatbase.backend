using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    private readonly PresenceTracker presenceTracker;

    public SignalRHub(PresenceTracker presenceTracker)
    {
        this.presenceTracker = presenceTracker;
    }

    [SignalRHidden]
    public override async Task OnConnectedAsync()
    {
        await presenceTracker.ConnectionOpened(Context.User.Identity.Name, Context.ConnectionId);
        await base.OnConnectedAsync();
    }


    [SignalRHidden]
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await presenceTracker.ConnectionClosed(Context.User.Identity.Name, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    ///     This method is called when a message is received with the specified priority
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="message"></param>
    /// <param name="priority"></param>
    /// <returns></returns>
    public async Task ReceiveMessage([SignalRHidden] string userId, string message, int priority)
    {
        await Clients.User(userId).SendAsync("ReceiveMessage", message, priority);
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

    public Task<string[]> GetOnlineUsers()
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
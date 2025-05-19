using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace WSSubscription.UserIdProviders
{
    public class SubUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Usa el claim "sub" del JWT como identificador de usuario en SignalR
            return connection.User?.FindFirst("sub")?.Value;
        }
    }
}

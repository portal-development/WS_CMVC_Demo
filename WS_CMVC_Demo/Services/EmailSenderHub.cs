using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WS_CMVC_Demo.Services
{
    public interface ITypedHubClient
    {
        Task Sended(string UserId, string Email, bool Fail, string ErrorMessage);
    }

    [Authorize(Roles = "emailsender")]
    public class EmailSenderHub : Hub<ITypedHubClient>
    {

    }
}

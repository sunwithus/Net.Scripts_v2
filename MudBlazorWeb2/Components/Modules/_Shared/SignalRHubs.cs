//SignalRHubs.cs

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.TodoList;

namespace MudBlazorWeb2.Components.Modules._Shared
{
    public class ReplicatorHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }

    public class TodoHub : Hub
    {
        public async Task BroadcastUpdate(TodoItem Todos)
        {
            await Clients.All.SendAsync("UpdateTodos", Todos);
        }
    }

}
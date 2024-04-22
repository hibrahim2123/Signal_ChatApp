using Microsoft.AspNetCore.SignalR;
using SignalApi.Data;
using SignalApi.Models;

namespace SignalApi.Hubs
{
    public class ChatHub : Hub
    {
        public async Task GetNickName(string nickName)
        {
            Client client = new Client()
            {
                ConnectionId = Context.ConnectionId,
                NickName = nickName
            };

            ClientSource.Clients.Add(client);

            await Clients.Others.SendAsync("clientJoined", nickName);
            await Clients.All.SendAsync("clients",ClientSource.Clients);
        }
        public  async Task SendMessageAsync(string message , string clientName)
        {
            clientName = clientName.Trim();
            Client? senderClient = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if(clientName == "Tümü")
            {
                await Clients.Others.SendAsync("receiveMessage",message,senderClient?.NickName);
            }
            else
            {
                Client? client = ClientSource.Clients.FirstOrDefault(c => c.NickName == clientName);
                await Clients.Client(client.ConnectionId).SendAsync("receiveMessage", message,senderClient?.NickName);
            }
        }

        public async Task AddGroup(string groupName)
        {   
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            Group group = new Group { GroupName = groupName };
            group.Clients.Add(ClientSource.Clients.FirstOrDefault((c) => c.ConnectionId == Context.ConnectionId));

            GroupSource.Groups.Add(group);
            
            await Clients.All.SendAsync("groups", GroupSource.Groups);
        }

        public async Task AddClientGroup(IEnumerable<string> groupNames)
        {
            Client? client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            foreach(var groupName in groupNames) {
                Group? _group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == groupName);
                
                bool result = _group.Clients.Any(c => c.ConnectionId == Context.ConnectionId);
                if(!result)
                {
                    _group.Clients.Add(client);
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                }
                else
                {
                    Client myClient = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
                    await Clients.Caller.SendAsync("alertMessage", myClient?.NickName);
                }
            }

        }

        public async Task GetClientGroup(string groupName)
        {
            if(groupName == "-1")
            {
                await Clients.Caller.SendAsync("clients", ClientSource.Clients);
                return;

            }
            Group group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == groupName);

            await Clients.Caller.SendAsync("clients", group.Clients);
       }

        public async Task SendMessageToGroupAsync(string groupName,string message)
        {
            await Clients.Group(groupName).SendAsync("receiveMessage", message, ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId).NickName);

        }
    }
}

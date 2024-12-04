namespace WebRTCWebApp.Server.Hubs {
    using Microsoft.AspNetCore.SignalR;

    public class ChatHub : Hub {
        private static Dictionary<string, string> Users = new Dictionary<string, string>();
        public override async Task OnConnectedAsync() {
            string username = Context.GetHttpContext().Request.Query["username"];
            string invitationCode = Context.GetHttpContext().Request.Query["invitationCode"];

            await Groups.AddToGroupAsync(Context.ConnectionId, invitationCode);
            Users[Context.ConnectionId] = username;
            Context.Items["invitationCode"] = invitationCode; // Add this line

            await Clients.Group(invitationCode).SendAsync("ReceiveMessage", string.Empty, $"{username} joined the party.");
        }


        public override async Task OnDisconnectedAsync(Exception? exception) {
            if (Users.TryGetValue(Context.ConnectionId, out var username)) {
                Users.Remove(Context.ConnectionId);

                string invitationCode = Context.Items["invitationCode"] as string;
                if (!string.IsNullOrEmpty(invitationCode)) {
                    await Clients.Group(invitationCode).SendAsync("ReceiveMessage", string.Empty, $"{username} left.");
                }
            }
        }

        public async Task AddMessageToChat(string username, string message, string invitationCode) {
            await Clients.Group(invitationCode).SendAsync("ReceiveMessage", username, message);

        }
    }
}
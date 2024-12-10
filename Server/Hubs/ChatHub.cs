namespace WebRTCWebApp.Server.Hubs {
    using System.Text.Json;
    using Microsoft.AspNetCore.SignalR;

    public class ChatHub : Hub {
        private static Dictionary<string, string> Users = new Dictionary<string, string>();
        private static Dictionary<string, int> InvitationCodeUserCounts = new Dictionary<string, int>();

        public override async Task OnConnectedAsync() {
            string username = Context.GetHttpContext().Request.Query["username"];
            string invitationCode = Context.GetHttpContext().Request.Query["invitationCode"];

            // Ensure the invitation code exists in the user count dictionary
            if (!InvitationCodeUserCounts.ContainsKey(invitationCode)) {
                InvitationCodeUserCounts[invitationCode] = 0;
            }

            // Check if the number of users for this invitation code is already 2
            if (InvitationCodeUserCounts[invitationCode] >= 2) {
                await Clients.Caller.SendAsync("ReceiveMessage", string.Empty, "This invitation code has reached its user limit.");
                Context.Abort();
                return;
            }

            // Add the user to the group and update the user count
            await Groups.AddToGroupAsync(Context.ConnectionId, invitationCode);
            Users[Context.ConnectionId] = username;
            Context.Items["invitationCode"] = invitationCode;
            InvitationCodeUserCounts[invitationCode]++;

            await Clients.Group(invitationCode).SendAsync("ReceiveMessage", string.Empty, $"{username} joined the party.");
        }

        public override async Task OnDisconnectedAsync(Exception? exception) {
            if (Users.TryGetValue(Context.ConnectionId, out var username)) {
                Users.Remove(Context.ConnectionId);

                string invitationCode = Context.Items["invitationCode"] as string;
                if (!string.IsNullOrEmpty(invitationCode)) {
                    // Decrement user count for the invitation code
                    if (InvitationCodeUserCounts.ContainsKey(invitationCode)) {
                        InvitationCodeUserCounts[invitationCode] = Math.Max(0, InvitationCodeUserCounts[invitationCode] - 1);

                        // Remove the invitation code if no users are left
                        if (InvitationCodeUserCounts[invitationCode] == 0) {
                            InvitationCodeUserCounts.Remove(invitationCode);
                        }
                    }

                    await Clients.Group(invitationCode).SendAsync("ReceiveMessage", string.Empty, $"{username} left.");
                }
            }
        }

        public async Task AddMessageToChat(string username, string message, string invitationCode) {
            await Clients.Group(invitationCode).SendAsync("ReceiveMessage", username, message);
        }

        public async Task SendOffer(string groupId, string offer) {
            await Clients.OthersInGroup(groupId).SendAsync("ReceiveSignal", "offer", offer);
        }

        public async Task SendAnswer(string groupId, string answer) {
            await Clients.OthersInGroup(groupId).SendAsync("ReceiveSignal", "answer", answer);
        }

        public async Task SendIceCandidate(string groupId, string candidate) {
            await Clients.OthersInGroup(groupId).SendAsync("ReceiveSignal", "ice-candidate", candidate);
        }
    }
}

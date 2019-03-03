using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SignalRChat.SignalRModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SignalRChat
{
    //
    public class ChatHub : Hub
    {
        static List<MessageGroup> activeMessageGroups = new List<MessageGroup>();
        static List<SignalRUser> authenticatedUsers = new List<SignalRUser>();


        private bool IsValidClient(string bikeId, string token, string clientType)
        {
            try
            {
                if (clientType == "C/JS") // Client/Javascript
                {
                    return true;
                    //if (Context.User.Identity.IsAuthenticated == true)
                    //{
                    //    return true;
                    //}
                }
                //
                if (clientType == "C/DT")
                {
                    // Search through DB First then return true/false
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsAuthenticated(string connectionId)
        {
            var user = authenticatedUsers.Where(x => x.ConnectionID == connectionId).FirstOrDefault();
            if (user != null)
            {
                return true;
            }
            return false;
        }

        private void SendToGroup(string groupName, string actionName, object message)
        {
            try
            {
                bool groupExits = activeMessageGroups.Exists(x => x.GroupName == groupName);
                //
                if (groupExits)
                {
                    var group = Clients.Group(groupName) as GroupProxy;
                    if (group != null)
                    {
                        group.Invoke(actionName, message);
                    }
                }
            }
            catch
            {
            }
        }

        public async Task subscribeToMany(string[] groupNames)
        {
            for (int i = 0; i < groupNames.Count(); i++)
            {
                string groupName = groupNames[i];
                var existingGroup = activeMessageGroups.Where(x => x.CreatedBy == Context.ConnectionId && x.GroupName == groupName).FirstOrDefault();
                if (existingGroup == null)
                {
                    MessageGroup mGroup = new MessageGroup();
                    mGroup.CreatedBy = Context.ConnectionId;
                    mGroup.GroupName = groupName;
                    //
                    activeMessageGroups.Add(mGroup);
                }
                //
                await Groups.Add(Context.ConnectionId, groupName);
            }
        }

        public async Task subscribeTo(string groupName)
        {
            //if (IsAuthenticated(Context.ConnectionId) == false)
            //{
            //    // Do Not Process This Message
            //    return;
            //}
            //
            try
            {
                var existingGroup = activeMessageGroups.Where(x => x.CreatedBy == Context.ConnectionId).FirstOrDefault();
                if (existingGroup == null)
                {
                    MessageGroup mGroup = new MessageGroup();
                    mGroup.CreatedBy = Context.ConnectionId;
                    mGroup.GroupName = groupName;
                    //
                    activeMessageGroups.Add(mGroup);
                }
                else
                {
                    try
                    {
                        await Groups.Remove(Context.ConnectionId, existingGroup.GroupName);
                    }
                    catch { }
                    existingGroup.GroupName = groupName;
                }
                //
                await Groups.Add(Context.ConnectionId, groupName);
            }
            catch
            {
            }
        }

        public async Task unsubscribeAll()
        {
            //if (IsAuthenticated(Context.ConnectionId) == false)
            //{
            //    // Do Not Process This Message
            //    return;
            //}
            //
            try
            {
                var joinedGroups = activeMessageGroups.Where(x => x.CreatedBy == Context.ConnectionId).ToList();
                for (int i = 0; i < joinedGroups.Count; i++)
                {
                    try
                    {
                        MessageGroup mGroup = joinedGroups[i];
                        await Groups.Remove(Context.ConnectionId, mGroup.GroupName);
                    }
                    catch { }
                }
            }
            catch { }
        }

        public void SendCustomMessage(Custom directMessage)
        {
            //if (IsAuthenticated(Context.ConnectionId) == false)
            //{
            //    // Do Not Process This Message
            //    return;
            //}
            //
            SendToGroup(directMessage.SendTo, "custom", directMessage);
        }
        
        public override Task OnConnected()
        {
            // Here we need to look for Js client
            string clientType = Context.QueryString["Type"];
            string token = Context.QueryString["Token"];
            string mail = Context.QueryString["Email"];
            bool isValid = IsValidClient(mail, token, clientType);
            if (isValid == true)
            {
                var user = authenticatedUsers.Where(x => x.ConnectionID == Context.ConnectionId).FirstOrDefault();
                if (user != null)
                {
                    user.Email = mail;
                    user.Token = token;
                    user.ConnectionID = Context.ConnectionId;
                }
                else
                {
                    SignalRUser srUser = new SignalRUser();
                    srUser.Email = mail;
                    srUser.Token = token;
                    srUser.ConnectionID = Context.ConnectionId;
                    authenticatedUsers.Add(srUser);
                }
                return base.OnConnected();
            }
            return new Task(() => { }); // Do Nothing
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                authenticatedUsers.RemoveAll(x => x.ConnectionID == Context.ConnectionId);
                activeMessageGroups.RemoveAll(x => x.CreatedBy == Context.ConnectionId);
            }
            catch { }
            return base.OnDisconnected(stopCalled);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using ChatManager.Models;
using System;

namespace ChatManager.Controllers
{
    [OnlineUsers.UserAccess]
    public class ChatRoomController : Controller
    {
        // GET: ChatRoom
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetFriendsList(bool forceRefresh = false)
        {
            if (forceRefresh || DB.Relationships.HasChanged)
            {
                //We only display users we are friends with or have already conversed with
                User onlineUser = OnlineUsers.GetSessionUser();
                return PartialView(DB.Users.SortedUsers().Where((u) =>
                    DB.Relationships.Get((u.Id, onlineUser.Id)).Status == RelationshipStatus.Friend ||
                    DB.Messages.ToList().Any((m) => (m.From == onlineUser.Id && m.To == u.Id) ||
                        (m.From == u.Id && m.To == onlineUser.Id))));
            }
            return null;
        }

        public ActionResult GetConversation(bool forceRefresh = false)
        {
            if (forceRefresh || DB.Relationships.HasChanged || DB.Messages.HasChanged)
            {
                //We only get messages that concern us and the selected user
                User onlineUser = OnlineUsers.GetSessionUser();

                IEnumerable<Message> messages = DB.Messages.ToList().Where((m) =>
                    (m.From == onlineUser.Id && m.To == TargetUser) ||
                    (m.From == TargetUser && m.To == onlineUser.Id));

                //We mark messages as read
                foreach (Message message in messages)
                    if (message.To == onlineUser.Id && message.ReadTime == DateTime.MinValue)
                        message.ReadTime = DateTime.Now;

                return PartialView(messages);
            }
            return null;
        }

        public string GetMessage(int id)
        {
            return DB.Messages.Get(id).Content;
        }

        #region Actions
        public void SetCurrentTarget(int target)
        {
            TargetUser = target;
        }

        public void Send(string message)
        {
            if (TargetUser == 0)
                return;

            Message newMessage = new Message();
            newMessage.Content = message;
            newMessage.From = OnlineUsers.GetSessionUser().Id;
            newMessage.To = TargetUser;

            DB.Messages.Add(newMessage);

            OnlineUsers.AddNotification(TargetUser, $"Vous avez reçu un message de {OnlineUsers.GetSessionUser().GetFullName()}.");
        }

        public void UpdateMessage(int id, string message)
        {
            Message oldMessage = DB.Messages.Get(id);

            oldMessage.Content = message;
            oldMessage.SendTime = DateTime.Now;
            oldMessage.ReadTime = DateTime.MinValue;

            DB.Messages.Update(oldMessage);
        }

        public void DeleteMessage(int id)
        {
            DB.Messages.Delete(id);
        }
        #endregion

        public static int TargetUser
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["TargetUser"] == null)
                    System.Web.HttpContext.Current.Session["TargetUser"] = 0;
                return (int)System.Web.HttpContext.Current.Session["TargetUser"];
            }
            set
            {
                System.Web.HttpContext.Current.Session["TargetUser"] = value;
            }
        }
    }
}
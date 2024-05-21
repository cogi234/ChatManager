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
        #region public

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
                    (m.From == TargetUser && m.To == onlineUser.Id))
                    .OrderBy((m) => m.SendTime);

                //We mark messages as read
                foreach (Message message in messages)
                    if (message.To == onlineUser.Id && message.ReadTime == DateTime.MinValue)
                        message.ReadTime = DateTime.Now;

                return PartialView(messages);
            }
            return null;
        }

        #endregion

        #region admin

        [OnlineUsers.AdminAccess]
        public ActionResult AdminIndex()
        {
            return View();
        }

        [OnlineUsers.AdminAccess]
        public ActionResult GetAllMessages(bool forceRefresh = false)
        {
            if (forceRefresh || DB.Messages.HasChanged)
            {
                IEnumerable<Message> messages = DB.Messages.ToList()
                    .OrderBy((m) => m.From > m.To ? $"{m.From},{m.To}" : $"{m.To},{m.From}")
                    .ThenByDescending((m) => m.SendTime);

                return PartialView(messages);
            }
            return null;
        }

        #endregion

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

            int sessionUserId = OnlineUsers.GetSessionUser().Id;

            if (DB.Relationships.Get((sessionUserId, TargetUser)).Status != RelationshipStatus.Friend)
                return;

            Message newMessage = new Message();
            newMessage.Content = message;
            newMessage.From = sessionUserId;
            newMessage.To = TargetUser;

            DB.Messages.Add(newMessage);

            OnlineUsers.AddNotification(TargetUser, $"Vous avez reçu un message de {OnlineUsers.GetSessionUser().GetFullName()}.");
        }

        public void UpdateMessage(int id, string message)
        {
            Message oldMessage = DB.Messages.Get(id);

            oldMessage.Content = message;
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
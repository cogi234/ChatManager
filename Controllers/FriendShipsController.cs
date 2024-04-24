using ChatManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MoviesDBManager.Controllers
{
    [OnlineUsers.UserAccess]
    public class FriendShipsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetRelationships(bool forceRefresh = false)
        {
            if (forceRefresh || OnlineUsers.HasChanged() || DB.Users.HasChanged || DB.Relationships.HasChanged)
            {
                return PartialView(DB.Users.SortedUsers().Where((u) => u.Verified));
            }
            return null;
        }

        public void SendRequest(int userid)
        {
            User user = DB.Users.Get(userid);
            User currentUser = OnlineUsers.GetSessionUser();
            if (user != null)
            {
                Relationship currentRelationship = DB.Relationships.Get((currentUser.Id, user.Id));
                if (currentRelationship.Status == RelationshipStatus.None)
                {
                    Relationship currentReverseRelationship = DB.Relationships.Get((user.Id, currentUser.Id));
                    if (currentReverseRelationship.Status == RelationshipStatus.Denied)
                    {
                        currentReverseRelationship.Status = RelationshipStatus.None;
                        DB.Relationships.Update(currentReverseRelationship);
                    }
                    currentRelationship.Status = RelationshipStatus.Request;
                    DB.Relationships.Update(currentRelationship);
                }
            }
        }
        public void RemoveRequest(int userid)
        {
            User user = DB.Users.Get(userid);
            User currentUser = OnlineUsers.GetSessionUser();
            if (user != null)
            {
                Relationship currentRelationship = DB.Relationships.Get((currentUser.Id, user.Id));
                if (currentRelationship.Status == RelationshipStatus.Request)
                {
                    currentRelationship.Status = RelationshipStatus.None;
                    DB.Relationships.Update(currentRelationship);
                }
                else if (currentRelationship.Status == RelationshipStatus.Friend)
                {
                    Relationship currentReverseRelationship = DB.Relationships.Get((user.Id, currentUser.Id));
                    currentReverseRelationship.Status = RelationshipStatus.None;
                    DB.Relationships.Update(currentReverseRelationship);
                    currentRelationship.Status = RelationshipStatus.None;
                    DB.Relationships.Update(currentRelationship);
                }
            }
        }
        public void AcceptRequest(int userid)
        {
            User user = DB.Users.Get(userid);
            User currentUser = OnlineUsers.GetSessionUser();
            if (user != null)
            {
                Relationship currentRelationship = DB.Relationships.Get((currentUser.Id, user.Id));
                Relationship currentReverseRelationship = DB.Relationships.Get((user.Id, currentUser.Id));
                currentReverseRelationship.Status = RelationshipStatus.Friend;
                DB.Relationships.Update(currentReverseRelationship);
                currentRelationship.Status = RelationshipStatus.Friend;
                DB.Relationships.Update(currentRelationship);
            }
        }
        public void DenyRequest(int userid)
        {
            User user = DB.Users.Get(userid);
            User currentUser = OnlineUsers.GetSessionUser();
            if (user != null)
            {
                Relationship currentReverseRelationship = DB.Relationships.Get((user.Id, currentUser.Id));
                currentReverseRelationship.Status = RelationshipStatus.Denied;
                DB.Relationships.Update(currentReverseRelationship);
            }
        }
    }
}
using ChatManager.Models;
using Microsoft.Ajax.Utilities;
using System.Collections.Generic;
using System.Linq;
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

        public ActionResult GetRelationships(bool forceRefresh = false, string search = "",
            bool displayBlocked = true, bool displayFriend = true, bool displayDenied = true,
            bool displayWaiting = true, bool displayRequest = true, bool displayNoRelation = true)
        {
            if (forceRefresh || OnlineUsers.HasChanged() || DB.Users.HasChanged || DB.Relationships.HasChanged)
            {
                User currentUser = OnlineUsers.GetSessionUser();
                IEnumerable<User> users = DB.Users.SortedUsers().Where((u) => u.Verified && u.Id != currentUser.Id);

                //We filter on the search
                if (!search.IsNullOrWhiteSpace())
                    users = users.Where((u) => u.GetFullName().ToLower().Contains(search.ToLower()));
                //We filter blocked users
                if (!displayBlocked)
                    users = users.Where((u) => !u.Blocked);
                //We filter friends
                if (!displayFriend)
                    users = users.Where((u) => DB.Relationships.Get((currentUser.Id, u.Id)).Status != RelationshipStatus.Friend);
                //We filter denied relationships
                if (!displayDenied)
                    users = users.Where((u) => DB.Relationships.Get((currentUser.Id, u.Id)).Status != RelationshipStatus.Denied &&
                            DB.Relationships.Get((u.Id, currentUser.Id)).Status != RelationshipStatus.Denied);
                //We filter waiting requests
                if (!displayWaiting)
                    users = users.Where((u) => DB.Relationships.Get((currentUser.Id, u.Id)).Status != RelationshipStatus.Request);
                //We filter requests from users
                if (!displayRequest)
                    users = users.Where((u) => DB.Relationships.Get((u.Id, currentUser.Id)).Status != RelationshipStatus.Request);
                //We filter users with no relation
                if (!displayNoRelation)
                    users = users.Where((u) => DB.Relationships.Get((currentUser.Id, u.Id)).Status != RelationshipStatus.None &&
                            DB.Relationships.Get((u.Id, currentUser.Id)).Status != RelationshipStatus.None);

                return PartialView(users);
            }
            return null;
        }

        #region Request processing
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
        #endregion
    }
}
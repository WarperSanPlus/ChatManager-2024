using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Models
{
    public static class OnlineUsers
    {
        public static List<int> ConnectedUsersId
        {
            get
            {
                if (HttpRuntime.Cache["OnLineUsers"] == null)
                    HttpRuntime.Cache["OnLineUsers"] = new List<int>();
                return (List<int>)HttpRuntime.Cache["OnLineUsers"];
            }
        }

        public static List<(int, int)> SessionEnCour = new List<(int, int)>();

        #region Status change management

        private static string SerialNumber
        {
            get
            {
                if (HttpRuntime.Cache["OnLineUsersSerialNumber"] == null)
                    SetHasChanged();
                return (string)HttpRuntime.Cache["OnLineUsersSerialNumber"];
            }
            set => HttpRuntime.Cache["OnLineUsersSerialNumber"] = value;
        }

        public static bool HasChanged()
        {
            if (HttpContext.Current.Session["SerialNumber"] == null)
            {
                HttpContext.Current.Session["SerialNumber"] = SerialNumber;
                return true;
            }

            var sessionSerialNumber = (string)HttpContext.Current.Session["SerialNumber"];
            HttpContext.Current.Session["SerialNumber"] = SerialNumber;
            return SerialNumber != sessionSerialNumber;
        }

        public static void SetHasChanged() => SerialNumber = Guid.NewGuid().ToString();

        #endregion Status change management

        #region Session management

        public static void AddSessionUser(int userId)
        {
            HttpContext.Current.Session["UserId"] = userId;
            ConnectedUsersId.Add(userId);
            var enter = new Entry
            {
                IdUser = userId
            };
            _ = EntryRepository.Instance.Create(enter);

            SetHasChanged();
        }

        public static void RemoveSessionUser(bool isLegit = false)
        {
            var user = GetSessionUser();
            if (user != null)
            {
                _ = ConnectedUsersId.Remove(user.Id);
                HttpContext.Current?.Session.Abandon();

                var NouvelEntrer = EntryRepository.Instance.GetEntrer(user.Id);
                if (NouvelEntrer != null)
                {
                    NouvelEntrer.End = isLegit ? DateTime.Now : NouvelEntrer.Start;
                    _ = EntryRepository.Instance.Update(NouvelEntrer);
                }
            }

            SetHasChanged();
        }

        public static bool IsOnLine(int userId) => ConnectedUsersId.Contains(userId);

        public static User GetSessionUser()
        {
            var id = HttpContext.Current.Session["UserId"];

            return id == null ? null : UsersRepository.GetUser((int)id);
        }

        public static bool Write_Access()
        {
            var sessionUser = OnlineUsers.GetSessionUser();
            return sessionUser != null && (sessionUser.IsPowerUser || sessionUser.IsAdmin);
        }

        #endregion Session management

        #region Notifications handling

        private static List<Notification> Notifications
        {
            get
            {
                if (HttpRuntime.Cache["Notifications"] == null)
                    HttpRuntime.Cache["Notifications"] = new List<Notification>();
                return (List<Notification>)HttpRuntime.Cache["Notifications"];
            }
        }

        public static void AddNotification(int TargetUserId, string Message)
        {
            var user = UsersRepository.GetUser(TargetUserId);

            if (user == null || !IsOnLine(user.Id))
                return;

            Notifications.Add(new Notification() { TargetUserId = TargetUserId, Message = Message });
        }

        public static List<string> PopNotifications(int TargetUserId)
        {
            var notificationMessages = new List<string>();
            var notifications = Notifications.Where(n => n.TargetUserId == TargetUserId).OrderBy(n => n.Created).ToList();
            foreach (var notification in notifications)
            {
                if (IsOnLine(notification.TargetUserId))
                    notificationMessages.Add(notification.Message);
                _ = Notifications.Remove(notification);
            }

            return notificationMessages;
        }

        #endregion Notifications handling

        #region Access control

        public class UserAccess : AuthorizeAttribute
        {
            private bool ServerSideResponseHandling { get; set; }

            public UserAccess(bool serverSideResponseHandling = true) => this.ServerSideResponseHandling = serverSideResponseHandling;

            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                var sessionUser = OnlineUsers.GetSessionUser();
                if (sessionUser != null)
                {
                    if (sessionUser.Blocked)
                    {
                        RemoveSessionUser(true);
                        if (this.ServerSideResponseHandling)
                        {
                            httpContext.Response.Redirect("~/Accounts/Login?message=Compte bloqué!");
                            return false;
                        }
                        else
                        {
                            httpContext.Response.StatusCode = 403; // Forbiden status
                        }
                    }

                    return true;
                }

                httpContext.Response.Redirect("~/Accounts/Login?message=Accès non autorisé!");
                return false;
            }
        }

        public class PowerUserAccess : AuthorizeAttribute
        {
            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                var sessionUser = OnlineUsers.GetSessionUser();
                if (sessionUser != null && (sessionUser.IsPowerUser || sessionUser.IsAdmin))
                {
                    return true;
                }
                else
                {
                    httpContext.Response.Redirect("~/Accounts/Login?message=Accès non autorisé!", true);
                }

                return false;
            }
        }

        public class AdminAccess : AuthorizeAttribute
        {
            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                var sessionUser = OnlineUsers.GetSessionUser();
                if (sessionUser != null && sessionUser.IsAdmin)
                {
                    return true;
                }
                else
                {
                    httpContext.Response.Redirect("~/Accounts/Login?message=Accès non autorisé!");
                }

                return true;
            }
        }

        #endregion Access control
    }
}
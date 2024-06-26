﻿using Mail;
using Models;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Controllers
{
    public class AccountsController : Controller
    {
        #region Account creation

        [HttpPost]
        public JsonResult EmailAvailable(string email)
        {
            var user = OnlineUsers.GetSessionUser();
            if (user != null && user.IsAdmin)
                return this.Json(true);

            var excludedId = user?.Id ?? 0;
            return this.Json(UsersRepository.Instance.EmailAvailable(email, excludedId));
        }

        [HttpPost]
        public JsonResult EmailExist(string email) => this.Json(UsersRepository.Instance.EmailExist(email));

        public ActionResult Subscribe()
        {
            this.ViewBag.Genders = SelectListUtilities<Gender>.Convert(DB.GetRepo<Gender>().ToList());
            var user = new User();
            return this.View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Subscribe(User user)
        {
            user.UserTypeId = 3; // self subscribed user
            if (this.ModelState.IsValid)
            {
                if (user.Avatar == Models.User.Default_Avatar)
                {
                    // required avatar image
                    this.ModelState.AddModelError("Avatar", "Veuillez choisir votre avatar");
                    this.ViewBag.Genders = SelectListUtilities<Gender>.Convert(DB.GetRepo<Gender>().ToList());
                }
                else
                {
                    user = UsersRepository.Instance.Create(user);
                    if (user != null)
                    {
                        this.SendEmailVerification(user, user.Email);
                        return this.RedirectToAction("SubscribeDone/" + user.Id.ToString());
                    }
                    else
                    {
                        return this.RedirectToAction("Report", "Errors", new { message = "Échec de création de compte" });
                    }
                }
            }

            return this.View(user);
        }

        public ActionResult SubscribeDone(int id)
        {
            var newlySubscribedUser = UsersRepository.Instance.Get(id);
            return newlySubscribedUser != null ? this.View(newlySubscribedUser) : (ActionResult)this.RedirectToAction("Login");
        }

        #endregion Account creation

        #region Account Verification

        public void SendEmailVerification(User user, string newEmail)
        {
            if (user.Id != 0)
            {
                var unverifiedEmail = UsersRepository.Instance.Add_UnverifiedEmail(user.Id, newEmail);
                if (unverifiedEmail != null)
                {
                    var verificationUrl = this.Url.Action("VerifyUser", "Accounts", null, this.Request.Url.Scheme);
                    var Link = @"<br/><a href='" + verificationUrl + "?code=" + unverifiedEmail.VerificationCode + @"' > Confirmez votre inscription...</a>";

                    var suffixe = "";
                    if (user.GenderId == 2)
                    {
                        suffixe = "e";
                    }

                    var Subject = "Movies Database - Vérification d'inscription...";

                    var Body = "Bonjour " + user.GetFullName(true) + @",<br/><br/>";
                    Body += @"Merci de vous être inscrit" + suffixe + " au site ChatManager. <br/>";
                    Body += @"Pour utiliser votre compte vous devez confirmer votre inscription en cliquant sur le lien suivant : <br/>";
                    Body += Link;
                    Body += @"<br/><br/>Ce courriel a été généré automatiquement, veuillez ne pas y répondre.";
                    Body += @"<br/><br/>Si vous éprouvez des difficultés ou s'il s'agit d'une erreur, veuillez le signaler à <a href='mailto:"
                         + SMTP.OwnerEmail + "'>" + SMTP.OwnerName + "</a> (Webmestre du site ChatManager)";

                    SMTP.SendEmail(user.GetFullName(), unverifiedEmail.Email, Subject, Body);
                }
            }
        }

        public ActionResult VerifyDone(int id)
        {
            var newlySubscribedUser = UsersRepository.Instance.Get(id);
            return newlySubscribedUser != null ? this.View(newlySubscribedUser) : (ActionResult)this.RedirectToAction("Login");
        }

        public ActionResult VerifyError() => this.View();

        public ActionResult AlreadyVerified() => this.View();

        public ActionResult VerifyUser(string code)
        {
            var UnverifiedEmail = UsersRepository.Instance.FindUnverifiedEmail(code);
            if (UnverifiedEmail != null)
            {
                var newlySubscribedUser = UsersRepository.Instance.Get(UnverifiedEmail.UserId);

                if (newlySubscribedUser != null)
                {
                    if (UsersRepository.Instance.EmailVerified(newlySubscribedUser.Email))
                        return this.RedirectToAction("AlreadyVerified");

                    if (UsersRepository.Instance.Verify_User(newlySubscribedUser.Id, code))
                        return this.RedirectToAction("VerifyDone/" + newlySubscribedUser.Id);
                }
                else
                {
                    _ = this.RedirectToAction("VerifyError");
                }
            }

            return this.RedirectToAction("VerifyError");
        }

        #endregion Account Verification

        #region EmailChange

        public ActionResult EmailChangedAlert()
        {
            OnlineUsers.RemoveSessionUser(true);
            return this.View();
        }

        public ActionResult CommitEmailChange(string code) => UsersRepository.Instance.ChangeEmail(code)
                ? this.RedirectToAction("EmailChanged")
                : (ActionResult)this.RedirectToAction("EmailChangedError");

        public ActionResult EmailChanged() => this.View();

        public ActionResult EmailChangedError() => this.View();

        public void SendEmailChangedVerification(User user, string newEmail)
        {
            if (user.Id == 0)
                return;

            var unverifiedEmail = UsersRepository.Instance.Add_UnverifiedEmail(user.Id, newEmail);
            if (unverifiedEmail == null)
                return;

            var verificationUrl = this.Url.Action("CommitEmailChange", "Accounts", null, this.Request.Url.Scheme);
            var Link = @"<br/><a href='" + verificationUrl + "?code=" + unverifiedEmail.VerificationCode + @"' > Confirmez votre adresse...</a>";

            var Subject = "ChatManager - Confirmation de changement de courriel...";

            var Body = "Bonjour " + user.GetFullName(true) + @",<br/><br/>";
            Body += @"Vous avez modifié votre adresse de courriel. <br/>";
            Body += @"Pour que ce changement soit pris en compte, vous devez confirmer cette adresse en cliquant sur le lien suivant : <br/>";
            Body += Link;
            Body += @"<br/><br/>Ce courriel a été généré automatiquement, veuillez ne pas y répondre.";
            Body += @"<br/><br/>Si vous éprouvez des difficultés ou s'il s'agit d'une erreur, veuillez le signaler à <a href='mailto:"
                    + SMTP.OwnerEmail + "'>" + SMTP.OwnerName + "</a> (Webmestre du site ChatManager)";

            SMTP.SendEmail(user.GetFullName(), unverifiedEmail.Email, Subject, Body);
        }

        #endregion EmailChange

        #region ResetPassword

        public ActionResult ResetPasswordCommand() => this.View();

        [HttpPost]
        public ActionResult ResetPasswordCommand(string Email)
        {
            if (this.ModelState.IsValid)
            {
                this.SendResetPasswordCommandEmail(Email);
                return this.RedirectToAction("ResetPasswordCommandAlert");
            }

            return this.View(Email);
        }

        public void SendResetPasswordCommandEmail(string email)
        {
            var resetPasswordCommand = UsersRepository.Instance.Add_ResetPasswordCommand(email);
            if (resetPasswordCommand != null)
            {
                var user = UsersRepository.Instance.Get(resetPasswordCommand.UserId);
                var verificationUrl = this.Url.Action("ResetPassword", "Accounts", null, this.Request.Url.Scheme);
                var Link = @"<br/><a href='" + verificationUrl + "?code=" + resetPasswordCommand.VerificationCode + @"' > Réinitialisation de mot de passe...</a>";

                var Subject = "Répertoire de films - Réinitialisaton ...";

                var Body = "Bonjour " + user.GetFullName(true) + @",<br/><br/>";
                Body += @"Vous avez demandé de réinitialiser votre mot de passe. <br/>";
                Body += @"Procédez en cliquant sur le lien suivant : <br/>";
                Body += Link;
                Body += @"<br/><br/>Ce courriel a été généré automatiquement, veuillez ne pas y répondre.";
                Body += @"<br/><br/>Si vous éprouvez des difficultés ou s'il s'agit d'une erreur, veuillez le signaler à <a href='mailto:"
                     + SMTP.OwnerEmail + "'>" + SMTP.OwnerName + "</a> (Webmestre du site [nom de l'application])";

                SMTP.SendEmail(user.GetFullName(), user.Email, Subject, Body);
            }
        }

        public ActionResult ResetPassword(string code)
        {
            var resetPasswordCommand = UsersRepository.Instance.Find_ResetPasswordCommand(code);
            return resetPasswordCommand != null ? this.View(new PasswordView() { Code = code }) : (ActionResult)this.RedirectToAction("ResetPasswordError");
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult ResetPassword(PasswordView passwordView)
        {
            if (this.ModelState.IsValid)
            {
                var resetPasswordCommand = UsersRepository.Instance.Find_ResetPasswordCommand(passwordView.Code);
                return resetPasswordCommand != null
                    ? UsersRepository.Instance.ResetPassword(resetPasswordCommand.UserId, passwordView.Password)
                        ? this.RedirectToAction("ResetPasswordSuccess")
                        : (ActionResult)this.RedirectToAction("ResetPasswordError")
                    : this.RedirectToAction("ResetPasswordError");
            }

            return this.View(passwordView);
        }

        public ActionResult ResetPasswordCommandAlert() => this.View();

        public ActionResult ResetPasswordSuccess() => this.View();

        public ActionResult ResetPasswordError() => this.View();

        #endregion ResetPassword

        #region Profil

        [OnlineUsers.UserAccess]
        public ActionResult Profil()
        {
            var userToEdit = OnlineUsers.GetSessionUser().Clone();

            if (userToEdit == null)
                return null;

            this.ViewBag.Genders = new SelectList(DB.GetRepo<Gender>().ToList(), "Id", "Name", userToEdit.GenderId);
            this.Session["UnchangedPasswordCode"] = Guid.NewGuid().ToString().Substring(0, 12);
            userToEdit.Password = userToEdit.ConfirmPassword = (string)this.Session["UnchangedPasswordCode"];
            return this.View(userToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Profil(User user)
        {
            var currentUser = OnlineUsers.GetSessionUser();
            user.Id = currentUser.Id;
            user.Verified = currentUser.Verified;
            user.UserTypeId = currentUser.UserTypeId;
            user.Blocked = currentUser.Blocked;
            user.CreationDate = currentUser.CreationDate;

            var newEmail = "";
            if (this.ModelState.IsValid)
            {
                if (user.Password == (string)this.Session["UnchangedPasswordCode"])
                    user.Password = user.ConfirmPassword = currentUser.Password;

                if (user.Email != currentUser.Email)
                {
                    newEmail = user.Email;
                    user.Email = user.ConfirmEmail = currentUser.Email;
                }

                if (UsersRepository.Instance.Update(user))
                {
                    if (newEmail == "")
                        return this.RedirectToAction(nameof(Profil));
                    //return this.Redirect((string)this.Session["LastAction"]);

                    this.SendEmailChangedVerification(user, newEmail);
                    return this.RedirectToAction("EmailChangedAlert");
                }
            }

            this.ViewBag.Genders = new SelectList(DB.GetRepo<Gender>().ToList(), "Id", "Name", user.GenderId);
            return this.View(currentUser);
        }

        [OnlineUsers.AdminAccess]
        public ActionResult Edit(int userId)
        {
            var userToEdit = UsersRepository.GetUser(userId);

            if (userToEdit == null)
                return null;

            userToEdit = userToEdit.Clone();

            this.Session["UnchangedPasswordCode"] = Guid.NewGuid().ToString().Substring(0, 12);
            this.Session["IDsessModif"] = userId;
            userToEdit.Password = userToEdit.ConfirmPassword = (string)this.Session["UnchangedPasswordCode"];
            return this.View(userToEdit);
        }
        [OnlineUsers.AdminAccess]
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Edit(User user)
        {
            var id = (int)this.Session["IDsessModif"];
            var oldUser = UsersRepository.Instance.Get(id);

            if (this.ModelState.IsValid && oldUser != null)
            {
                user.Id = id;
               
                if(oldUser.UserTypeId == 1)
                {
                    user.UserTypeId = oldUser.UserTypeId;
                }
                user.Password = user.ConfirmPassword = oldUser.Password;
                user.GenderId = oldUser.GenderId;

                _ = UsersRepository.Instance.Update(user);
            }

            return this.Edit(id);
        }

        #endregion Profil

        #region Login and Logout

        public ActionResult ExpiredSession()
        {
            OnlineUsers.RemoveSessionUser();
            return this.Redirect("/Accounts/Login?message=Session expirée, veuillez vous reconnecter.");
        }

        public ActionResult Login(string message)
        {
            this.ViewBag.Message = message;
            OnlineUsers.RemoveSessionUser();
            return this.View(new LoginCredential());
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Login(LoginCredential loginCredential)
        {
            if (this.ModelState.IsValid)
            {
                var repo = UsersRepository.Instance;

                if (repo.EmailBlocked(loginCredential.Email))
                {
                    this.ModelState.AddModelError("Email", "Ce compte est bloqué.");
                    return this.View(loginCredential);
                }

                if (!repo.EmailVerified(loginCredential.Email))
                {
                    this.ModelState.AddModelError("Email", "Ce courriel n'est pas vérifié.");
                    return this.View(loginCredential);
                }

                var user = repo.GetUser(loginCredential);

                if (user == null)
                {
                    this.ModelState.AddModelError("Password", "Mot de passe incorrect.");
                    return this.View(loginCredential);
                }

                OnlineUsers.AddSessionUser(user.Id);
                OnlineUsers.AddNotification(user.Id, "Heureux de vous revoir");
                return this.RedirectToAction("Index", "ChatRoom");
            }

            return this.View(loginCredential);
        }

        public ActionResult Logout()
        {
            OnlineUsers.RemoveSessionUser(true);
            return this.RedirectToAction("Login");
        }

        [OnlineUsers.AdminAccess]
        public ActionResult LoginsJournal() =>
            // this.ViewBag.Entries = EntrerRepository.GetEntrers();

            this.View();

        [OnlineUsers.AdminAccess]
        public ActionResult GetEntries(bool forceRefresh = false)
        {
            var repo = EntryRepository.Instance;

            if (!forceRefresh && !repo.HasChanged && !UsersRepository.Instance.HasChanged)
                return null;

            var entries = repo.GetEntrers();

            foreach (var item in entries)
                item.User = UsersRepository.GetUser(item.IdUser);

            return this.PartialView(entries.GroupBy(e => e.Start.Date));
        }

        #endregion Login and Logout

        [OnlineUsers.AdminAccess]
        public ActionResult GroupEmail(string status = "")
        {
            this.ViewBag.SelectedUsers = new List<int>();
            this.ViewBag.Users = UsersRepository.Instance.SortedUsers();
            this.ViewBag.Status = status;
            return this.View(new GroupEmail() { Message = "Bonjour [Nom]," });
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult GroupEmail(GroupEmail groupEmail)
        {
            if (this.ModelState.IsValid)
            {
                groupEmail.Send();
                return this.RedirectToAction("GroupEmail", new { status = "Message envoyé avec succès." });
            }

            this.ViewBag.Users = UsersRepository.Instance.SortedUsers();
            return this.View(groupEmail);
        }

        #region Administrator actions

        public JsonResult NeedUpdate() => this.Json(OnlineUsers.HasChanged(), JsonRequestBehavior.AllowGet);

        [OnlineUsers.AdminAccess]
        public void SendBlockStatusEMail(User user)
        {
            var currentAdmin = OnlineUsers.GetSessionUser();
            if (user != null)
            {
                if (user.Blocked)
                {
                    var Subject = "Movies Database - Compte bloqué...";

                    var Body = "Désolé " + user.GetFullName(true) + @"<br/><br/>";
                    Body += @"Votre compte a été bloqué par le modérateur suite à des abus de votre part. <br/>";
                    Body += @"Pour pour plus d'informations veuillez écrire à <a href='mailto:" + currentAdmin.Email + @"'>" + currentAdmin.GetFullName(false) + "</a><br/>";

                    SMTP.SendEmail(user.GetFullName(), user.Email, Subject, Body);
                }
                else
                {
                    var Subject = "Movies Database - Compte débloqué...";

                    var Body = "Bonjour " + user.GetFullName(true) + @"<br/><br/>";
                    Body += @"Votre compte a été débloqué par le modérateur <br/><br/>";
                    Body += @"Bonne journée! <br/>";

                    SMTP.SendEmail(user.GetFullName(), user.Email, Subject, Body);
                }
            }
        }

        [OnlineUsers.AdminAccess]
        public JsonResult ChangeUserBlockedStatus(int userid, bool blocked)
        {
            var user = UsersRepository.Instance.Get(userid);
            user.Blocked = blocked;
            this.SendBlockStatusEMail(user);
            return this.Json(UsersRepository.Instance.Update(user), JsonRequestBehavior.AllowGet);
        }

        [OnlineUsers.AdminAccess]
        public JsonResult PromoteUser(int userid)
        {
            var user = UsersRepository.Instance.Get(userid);
            if (user != null)
            {
                user.UserTypeId--;
                if (user.UserTypeId <= 0)
                    user.UserTypeId = 3;
                _ = UsersRepository.Instance.Update(user);
            }

            return this.Json(UsersRepository.Instance.Update(user), JsonRequestBehavior.AllowGet);
        }

        [OnlineUsers.AdminAccess]
        public void SendDeletedAccountEMail(User user)
        {
            var currentAdmin = OnlineUsers.GetSessionUser();
            if (user != null)
            {
                var Subject = "Movies Database - Compte bloqué...";

                var Body = "Désolé " + user.GetFullName(true) + @"<br/><br/>";
                Body += @"Votre compte a été retiré par le modérateur suite à des abus de votre part. <br/>";
                Body += @"Pour pour plus d'informations veuillez écrire à <a href='mailto:" + currentAdmin.Email + @"'>" + currentAdmin.GetFullName(false) + "</a><br/>";

                SMTP.SendEmail(user.GetFullName(), user.Email, Subject, Body);
            }
        }

        [OnlineUsers.AdminAccess]
        public JsonResult Delete(int userid)
        {
            var userToDelete = UsersRepository.Instance.Get(userid);
            if (userToDelete != null)
            {
                // this.SendDeletedAccountEMail(userToDelete);
                return this.Json(UsersRepository.Instance.Delete(userid), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [OnlineUsers.AdminAccess]
        public ActionResult UsersList() => this.View();

        [OnlineUsers.AdminAccess]
        public ActionResult GetUsersList(bool forceRefresh = false) => forceRefresh || OnlineUsers.HasChanged() || UsersRepository.Instance.HasChanged
                ? this.PartialView(UsersRepository.Instance.SortedUsers())
                : (ActionResult)null;

        #endregion Administrator actions
    }
}
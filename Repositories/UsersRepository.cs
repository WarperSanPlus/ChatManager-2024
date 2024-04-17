using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Repositories
{
    public class UsersRepository : Repository<User>
    {
        public static UsersRepository Instance => (UsersRepository)DB.GetRepo<User>();

        public static User GetUser(int id) => Instance.Get(id);

        public User Create(User user)
        {
            try
            {
                user.Id = base.Add(user);
                return user;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Add user failed : Message - {ex.Message}");
            }

            return null;
        }

        /// <inheritdoc/>
        public override bool Delete(int userId)
        {
            try
            {
                User userToDelete = this.Get(userId);

                if (userToDelete != null)
                {
                    this.BeginTransaction();
                    this.RemoveUnverifiedEmails(userId);
                    this.RemoveResetPasswordCommands(userId);
                    _ = base.Delete(userId);
                    this.EndTransaction();

                    EntryRepository.Instance.UtilisateurEntrerSupprimer(userId);

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Remove user failed : Message - {ex.Message}");
                this.EndTransaction();
            }

            return false;
        }

        /// <returns><see cref="User"/> with the given id or null an error occured</returns>
        public User FindUser(int id)
        {
            User user = null;
            try
            {
                user = this.Get(id);
                if (user != null)
                {
                    user.ConfirmEmail = user.Email;
                    user.ConfirmPassword = user.Password;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Find user failed : Message - {ex.Message}");
            }

            return user;
        }

        /// <returns>
        /// All the users sorted by <see cref="User.FirstName"/>
        /// then by <see cref="User.LastName"/>
        /// </returns>
        public IEnumerable<User> SortedUsers() => this.ToList().OrderBy(u => u.FirstName).ThenBy(u => u.LastName);

        public bool Verify_User(int userId, string code)
        {
            User user = this.Get(userId);

            if (user == null)
                return false;

            Repository<UnverifiedEmail> emailsRepo = DB.GetRepo<UnverifiedEmail>();

            // take the last email verification request
            UnverifiedEmail unverifiedEmail = emailsRepo.ToList().Where(u => u.UserId == userId).FirstOrDefault();

            if (unverifiedEmail == null)
                return false;

            if (unverifiedEmail.VerificationCode != code)
                return false;

            try
            {
                user.Email = user.ConfirmEmail = unverifiedEmail.Email;
                user.Verified = true;

                this.BeginTransaction();
                _ = base.Update(user);
                _ = emailsRepo.Delete(unverifiedEmail.Id);
                this.EndTransaction();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Verify_User failed : Message - {ex.Message}");
                this.EndTransaction();
            }

            return false;
        }

        public bool ChangeEmail(string code)
        {
            UnverifiedEmail unverifiedEmail = this.FindUnverifiedEmail(code);

            if (unverifiedEmail == null)
                return false;

            User user = this.Get(unverifiedEmail.UserId);

            if (user == null)
                return false;

            try
            {
                user.Email = user.ConfirmEmail = unverifiedEmail.Email;
                user.Verified = true;

                this.BeginTransaction();
                _ = base.Update(user);
                _ = DB.GetRepo<UnverifiedEmail>().Delete(unverifiedEmail.Id);
                this.EndTransaction();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Verify_User failed : Message - {ex.Message}");
                this.EndTransaction();
            }

            return false;
        }

        public bool EmailAvailable(string email, int excludedId = 0)
        {
            User user = this.ToList().Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault();
            return user == null || user.Id == excludedId || user.Email.ToLower() != email.ToLower();
        }

        public bool EmailExist(string email) => this.ToList().Any(u => u.Email.ToLower() == email.ToLower());

        public bool EmailBlocked(string email)
        {
            User user = this.ToList().Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault();
            return user == null || user.Blocked;
        }

        public bool EmailVerified(string email)
        {
            User user = this.ToList().Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault();
            return user != null && user.Verified;
        }

        public UnverifiedEmail Add_UnverifiedEmail(int userId, string email)
        {
            UnverifiedEmail unverifiedEmail = null;
            try
            {
                this.BeginTransaction();
                this.RemoveUnverifiedEmails(userId);
                unverifiedEmail = new UnverifiedEmail() { UserId = userId, Email = email, VerificationCode = Guid.NewGuid().ToString() };
                unverifiedEmail.Id = DB.GetRepo<UnverifiedEmail>().Add(unverifiedEmail);
                this.EndTransaction();
            }
            catch (Exception ex)
            {
                this.EndTransaction();
                System.Diagnostics.Debug.WriteLine($"Add_UnverifiedEmail failed : Message - {ex.Message}");
            }

            return unverifiedEmail;
        }

        public UnverifiedEmail FindUnverifiedEmail(string code) => DB.GetRepo<UnverifiedEmail>().ToList().Where(u => u.VerificationCode == code).FirstOrDefault();

        private void RemoveUnverifiedEmails(int userId)
        {
            Repository<UnverifiedEmail> repository = DB.GetRepo<UnverifiedEmail>();

            var UnverifiedEmails = repository.ToList().Where(u => u.UserId == userId).ToList();
            foreach (UnverifiedEmail UnverifiedEmail in UnverifiedEmails)
                _ = repository.Delete(UnverifiedEmail.Id);
        }

        public ResetPasswordCommand Add_ResetPasswordCommand(string email)
        {
            try
            {
                User user = this.ToList().Where(u => u.Email == email).FirstOrDefault();
                if (user != null)
                {
                    this.BeginTransaction();
                    this.RemoveResetPasswordCommands(user.Id); // Flush previous request
                    var resetPasswordCommand =
                        new ResetPasswordCommand() { UserId = user.Id, VerificationCode = Guid.NewGuid().ToString() };

                    resetPasswordCommand.Id = DB.GetRepo<ResetPasswordCommand>().Add(resetPasswordCommand);
                    this.EndTransaction();
                    return resetPasswordCommand;
                }

                return null;
            }
            catch (Exception ex)
            {
                this.EndTransaction();
                System.Diagnostics.Debug.WriteLine($"Add_ResetPasswordCommand failed : Message - {ex.Message}");
                return null;
            }
        }

        public ResetPasswordCommand Find_ResetPasswordCommand(string verificationCode) => DB.GetRepo<ResetPasswordCommand>().ToList().Where(r => r.VerificationCode == verificationCode).FirstOrDefault();

        private void RemoveResetPasswordCommands(int userId)
        {
            Repository<ResetPasswordCommand> repository = DB.GetRepo<ResetPasswordCommand>();

            var ResetPasswordCommands = repository.ToList().Where(r => r.UserId == userId).ToList();
            foreach (ResetPasswordCommand ResetPasswordCommand in ResetPasswordCommands)
                _ = repository.Delete(ResetPasswordCommand.Id);
        }

        public bool ResetPassword(int userId, string password)
        {
            User user = this.Get(userId);
            if (user != null)
            {
                user.Password = user.ConfirmPassword = password;
                try
                {
                    this.BeginTransaction();
                    this.RemoveResetPasswordCommands(user.Id);
                    var result = base.Update(user);
                    this.EndTransaction();
                    return result;
                }
                catch (Exception ex)
                {
                    this.EndTransaction();
                    System.Diagnostics.Debug.WriteLine($"ResetPassword failed : Message - {ex.Message}");
                }
            }

            return false;
        }

        public User GetUser(LoginCredential loginCredential)
        {
            User user = this.ToList().Where(u => (u.Email.ToLower() == loginCredential.Email.ToLower()) &&
                                            (u.Password == loginCredential.Password))
                                .FirstOrDefault();
            return user;
        }
    }
}
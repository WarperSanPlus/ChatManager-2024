using Mail;
using Repositories;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class GroupEmail
    {
        public List<int> SelectedUsers { get; set; }

        [Display(Name = "Sujet"), Required(ErrorMessage = "Obligatoire")]
        public string Subject { get; set; }

        [Display(Name = "Message"), Required(ErrorMessage = "Obligatoire")]
        public string Message { get; set; }

        public void Send()
        {
            if (this.SelectedUsers == null)
                return;

            foreach (var userId in this.SelectedUsers)
            {
                User user = UsersRepository.GetUser(userId);
                var personalizedMessage = this.Message.Replace("[Nom]", user.GetFullName(true)).Replace("\r\n", @"<br>");
                SMTP.SendEmail(user.GetFullName(), user.Email, this.Subject, personalizedMessage);
            }
        }
    }
}
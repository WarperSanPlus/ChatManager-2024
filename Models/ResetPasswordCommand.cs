using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class ResetPasswordCommand
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string VerificationCode { get; set; }
    }
}
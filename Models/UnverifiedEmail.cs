namespace Models
{
    public class UnverifiedEmail : BaseModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string VerificationCode { get; set; }
        public int UserId { get; set; }
    }
}
namespace SendEmail.Models
{
    public class Email
    {
        public string ReceiverName { get; set; }
        public string ReceiverEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}

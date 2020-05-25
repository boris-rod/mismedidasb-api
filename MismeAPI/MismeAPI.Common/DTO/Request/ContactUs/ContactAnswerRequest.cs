namespace MismeAPI.Common.DTO.Request.ContactUs
{
    public class ContactAnswerRequest
    {
        public int ContactUsId { get; set; }
        public string UserEmail { get; set; }
        public string Body { get; set; }
    }
}
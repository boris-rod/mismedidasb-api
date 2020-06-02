using System;

namespace MismeAPI.Common.DTO.Response.ContactUs
{
    public class ContactUsResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Read { get; set; }
        public string Priority { get; set; }
        public int PriorityId { get; set; }
        public bool IsAnswered { get; set; }
    }
}
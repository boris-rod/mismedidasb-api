using System;

namespace MismeAPI.Common.DTO.Response
{
    public class AnswerResponse
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int Weight { get; set; }
        public int Order { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
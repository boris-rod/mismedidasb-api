using System;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Response
{
    public class QuestionResponse
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public int Order { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public List<AnswerResponse> Answers { get; set; }
    }
}
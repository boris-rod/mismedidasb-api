using System;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Response
{
    public class PollResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public List<QuestionResponse> Questions { get; set; }
    }
}
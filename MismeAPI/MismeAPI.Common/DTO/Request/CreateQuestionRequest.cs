using System.ComponentModel.DataAnnotations;

namespace MismeAPI.Common.DTO.Request
{
    public class CreateQuestionRequest
    {
        public int PollId { get; set; }
        public int Order { get; set; }

        [Required]
        public string Title { get; set; }
    }
}
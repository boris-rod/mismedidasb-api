using System;

namespace MismeAPI.Common.DTO.Response
{
    public class TipResponse
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
        public string TipPositionString { get; set; }
        public int TipPosition { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
namespace MismeAPI.Common.DTO.Request.Tip
{
    public class AddTipRequest
    {
        public int PollId { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
        public int TipPosition { get; set; }
    }
}
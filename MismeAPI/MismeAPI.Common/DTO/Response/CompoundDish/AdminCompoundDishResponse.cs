namespace MismeAPI.Common.DTO.Response.CompoundDish
{
    public class AdminCompoundDishResponse : CompoundDishResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
    }
}
namespace MismeAPI.Common.DTO.Request
{
    public class UpdateDishRequest : CreateDishRequest
    {
        public int Id { get; set; }
        public string RemovedImage { get; set; }

        //for reward system purposes
        public int UserId { get; set; }
    }
}
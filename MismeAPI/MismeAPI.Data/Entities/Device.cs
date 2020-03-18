using MismeAPI.Data.Entities.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("device")]
    public class Device
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        //public int DeviceId { get; set; }
        public string Token { get; set; }

        public DeviceTypeEnum Type { get; set; }
    }
}
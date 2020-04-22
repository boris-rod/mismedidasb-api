using System;

namespace MismeAPI.Common.DTO.Response
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
        public string Avatar { get; set; }
        public string AvatarMimeType { get; set; }
        public string Role { get; set; }
        public int RoleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? DisabledAt { get; set; }
        public double KCal { get; set; }
        public double IMC { get; set; }
        public DateTime? FirstHealthMeasured { get; set; }
        public string Language { get; set; }
    }
}
using MismeAPI.Data.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace MismeAPI.Data.Entities
{
    public class User
    {
        public User()
        {
        }

        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public RoleEnum Role { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public StatusEnum Status { get; set; }

        public DateTime? LastLoggedIn { get; set; }
        public string Avatar { get; set; }
        public string AvatarMimeType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response.User
{
    public class UserAdminResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public double KCal { get; set; }
        public double IMC { get; set; }
        public int Age { get; set; }
        public int Sex { get; set; }

        /// <summary>
        /// User's height in cm
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// User's weight in Kg
        /// </summary>
        public double Weight { get; set; }

        public DateTime? HealthMeasuresLastUpdate { get; set; }
        public DateTime? ValueMeasuresLastUpdate { get; set; }
        public DateTime? WellnessMeasuresLastUpdate { get; set; }
        public DateTime? LastPlanedEat { get; set; }
        public DateTime? LastAccessAt { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Response
{
    public class EatResponse
    {
        public int Id { get; set; }
        public ICollection<EatDishResponse> EatDishResponse { get; set; }
        public ICollection<EatCompoundDishResponse> EatCompoundDishResponse { get; set; }
        public int EatTypeId { get; set; }
        public string EatType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public double IMC { get; set; }
        public double KCal { get; set; }
    }
}
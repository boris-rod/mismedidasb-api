using System;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Menu
{
    public class MenuEatResponse
    {
        public int Id { get; set; }
        public int EatTypeId { get; set; }
        public string EatType { get; set; }
        public bool IsBalanced { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public ICollection<MenuEatDishResponse> EatDishResponse { get; set; }
        public ICollection<MenuEatCompoundDishResponse> EatCompoundDishResponse { get; set; }
    }
}

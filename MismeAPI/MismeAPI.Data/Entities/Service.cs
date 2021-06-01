using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MismeAPI.Data.Entities
{
    [Table("service")]
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

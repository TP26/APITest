using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace APITest.Models
{
    [PrimaryKey(nameof(Id))]
    public class CoOrdinates
    {
        [Key]
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}

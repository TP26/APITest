using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace APITest.Models
{
    [PrimaryKey(nameof(Id))]
    public class Configuration
    {
        [Key]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int CoOrdId { get; set; }
    }
}

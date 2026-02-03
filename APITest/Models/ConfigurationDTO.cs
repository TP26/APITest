using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace APITest.Models
{
    [PrimaryKey(nameof(Id))]
    public class ConfigurationDTO
    {
        [Key]
        public int Id { get; set; }
        public List<ItemDTO> items { get; set; }
        public Category category { get; set; }
        public CoOrdinates coOrd { get; set; }
    }
}

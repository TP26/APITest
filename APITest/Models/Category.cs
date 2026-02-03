using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace APITest.Models
{
    [PrimaryKey(nameof(Id))]
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

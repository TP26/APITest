using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITest.Models
{
    [PrimaryKey(nameof(Id))]
    public class ConfigurationItemLists
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ConfigurationId { get; set; }
        public int ItemId { get; set; }
    }
}

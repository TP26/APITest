using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace APITest.Models
{
    [PrimaryKey(nameof(ConfigurationId))]
    public class ConfigurationItemLists
    {
        [Key]
        public int ConfigurationId { get; set; }
        public int ItemId { get; set; }
    }
}

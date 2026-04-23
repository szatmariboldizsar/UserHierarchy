using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class UserHierarchy
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long UserId { get; set; }

        public long? ParentId { get; set; }

        [Required]
        public int SortOrder { get; set; }
    }
}

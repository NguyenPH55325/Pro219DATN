using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pro219.DAL.Models
{
    [Table("Address")]
    public class Address
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        //WardCode
        [Required]
        [MaxLength(200)]        
        public string Ward { get; set; } = string.Empty;

        //ProvinceCode
        [Required]
        [MaxLength(100)]
        public string Province { get; set; } = string.Empty;

        //DistrictCode
        [Required]
        [MaxLength(100)]
        public string District { get; set; } = string.Empty;

        public string WardName { get; set; } = string.Empty;
        public string ProvinceName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? OtherInfo { get; set; }

        public bool IsDefault { get; set; }

        public byte? Status { get; set; }

        public bool? Delete { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public DateTime? DeleteAt { get; set; }

        [MaxLength(255)]
        public string? UpdateBy { get; set; }

        // Foreign key navigation property
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatVeMayBay.Models
{
    [Table("SanBay")]
    public class SanBay
    {
        [Key]
        public int IDSanBay { get; set; }

        [StringLength(100)]
        public string TenSanBay { get; set; } = string.Empty;

        [StringLength(100)]
        public string DiaDiem { get; set; } = string.Empty;
    }
}

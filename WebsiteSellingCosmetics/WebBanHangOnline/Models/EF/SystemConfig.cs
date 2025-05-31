using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHangOnline.Models.EF
{
    [Table("SystemConfig")]
    public class SystemConfig : CommonAbstract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên cấu hình không được để trống!")]
        [StringLength(150)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Thứ tự không được để trống!")]
        public int Position { get; set; }

        [StringLength(200)]
        public string Alias { get; set; }
    }
}
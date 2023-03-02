using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Items
    {
        [Key]
        public int Id { get; set; }
        public string ItemName { get; set; }=string.Empty;
        public decimal Rating { get; set; }
        public decimal Price { get; set; }
        public string imageSource { get; set; }=    string.Empty;
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace lapshop.Domains
{
    public class TbContactMessage
    {
        [Key]
        public int MessageId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
    }
}

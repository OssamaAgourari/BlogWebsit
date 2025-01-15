using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CategoryId { get; set; }

        // Navigation property
        public Category Category { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all comments for a post (this will be displayed with the post details)
        public IActionResult Index(int postId)
        {
            var post = _context.Posts
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefault(p => p.Id == postId);

            if (post == null)
            {
                return NotFound();
            }

            return View(post.Comments);
        }

        // Create a new comment (only for logged-in users)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int postId, string commentText)
        {
            if (string.IsNullOrEmpty(commentText))
            {
                return RedirectToAction("Details", "Post", new { id = postId });
            }

            var post = _context.Posts.Find(postId);
            if (post == null)
            {
                return NotFound();
            }

            // Create a new comment
            var comment = new Comment
            {
                Text = commentText,
                CreatedAt = DateTime.Now,
                PostId = postId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            // Add the comment to the database
            _context.Comments.Add(comment);
            _context.SaveChanges();

            return RedirectToAction("Details", "Post", new { id = postId });
        }

        // Delete a comment (only for logged-in users)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var comment = _context.Comments.Find(id);
            if (comment == null)
            {
                return NotFound();
            }

            // Check if the current user is the owner of the comment
            if (comment.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Unauthorized();
            }

            // Remove the comment from the database
            _context.Comments.Remove(comment);
            _context.SaveChanges();

            return RedirectToAction("Details", "Post", new { id = comment.PostId });
        }
    }
}

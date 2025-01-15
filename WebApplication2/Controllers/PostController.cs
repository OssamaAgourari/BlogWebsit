using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class PostController : Controller
    {
        // GET: PostController
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all posts (accessible to everyone)
        public IActionResult Index(int? categoryId)
        {
            // Fetch all categories to populate the dropdown list
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");

            // Get the posts, and filter by category if categoryId is provided
            var posts = _context.Posts.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                posts = posts.Where(p => p.CategoryId == categoryId.Value);
            }

            return View(posts.ToList());
        }

        // Display a single post and its comments
        public IActionResult Details(int id)
        {
            var post = _context.Posts
        .Include(p => p.Category)
        .Include(p => p.Comments)
        .FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Pass the logged-in user's ID to the view
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.UserId = userId;

            return View(post);
        }

        // Create a new post (only for logged-in users)
        [Authorize]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
            return View();
        }

        // Handle post creation
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Post post)
        {
            
                // Set CreatedAt manually
                post.CreatedAt = DateTime.Now;

                // Capture the logged-in user's ID
                post.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Save to the database
                _context.Posts.Add(post);
                _context.SaveChanges();
                
            

            // Reload categories if model state fails
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
            return RedirectToAction("Index");
        }

        // Edit a post (only for logged-in users)
        [Authorize]
        public IActionResult Edit(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Ensure the logged-in user is the one who created the post
            if (post.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Unauthorized();
            }

            // Pass the list of categories to the view for selection
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);

            return View(post);
        }

        // Handle post edit
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            // Ensure the logged-in user is the one who created the post
            var existingPost = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (existingPost == null || existingPost.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Unauthorized();
            }

            // Update the post
            
                existingPost.Title = post.Title;
                existingPost.Content = post.Content;
                existingPost.CategoryId = post.CategoryId;
                existingPost.CreatedAt = post.CreatedAt; // Keep the original creation date, or update if needed

                _context.SaveChanges();
            

            // If model validation fails, reload categories for the dropdown
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);
            return RedirectToAction(nameof(Details), new { id = existingPost.Id });
            return View(post);
        }

        // Delete a post (only for logged-in users)
        [Authorize]
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // Handle post deletion
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var post = _context.Posts.Find(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

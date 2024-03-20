using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookClient.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;
using System.Net.Http.Headers;

namespace BookClient.Controllers
{
    public class UsersController : Controller
    {
        private readonly BookStoreContext _context;
		private readonly HttpClient client;
		private string UserUrl = "https://localhost:7006/odata/Users";

		public UsersController(BookStoreContext context)
        {
            _context = context;
			client = new HttpClient();
			var contentType = new MediaTypeWithQualityHeaderValue("application/json");
			client.DefaultRequestHeaders.Accept.Add(contentType);
		}

        // GET: Users
        public async Task<IActionResult> Index()
        {
              return _context.Users != null ? 
                          View(await _context.Users.ToListAsync()) :
                          Problem("Entity set 'BookStoreContext.Users'  is null.");
        }

		public async Task<IActionResult> Login()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(string Email, string Password)
		{
			string email, pass;
			var conf = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();
			email = conf.GetSection("Admin").GetSection("Email").Value.ToString();
			pass = conf.GetSection("Admin").GetSection("Password").Value.ToString();

			if (email == Email && pass == Password)
			{
				HttpContext.Session.SetInt32("Role", 1);
				HttpContext.Session.SetString("Email", email);
				HttpContext.Session.SetInt32("MemberId", 0);
				return RedirectToAction("Index", "Home");
			}
			try
			{
				HttpResponseMessage response = await client.GetAsync(UserUrl);
				response.EnsureSuccessStatusCode();
				string strData = await response.Content.ReadAsStringAsync();
				var data = JObject.Parse(strData);
				List<User> members = JsonConvert.DeserializeObject<List<User>>(data["value"].ToString());
				List<User> users = members.Where(m => m.Email == Email
					&& m.Password == Password).ToList();
				if (users.Count == 0) return View();
				else
				{
					HttpContext.Session.SetInt32("Role", 0);
					HttpContext.Session.SetString("Email", users[0].Email);
					HttpContext.Session.SetInt32("MemberId", users[0].UserId);
					return RedirectToAction("Index", "Home");
				}
			}
			catch
			{
				return View();
			}

		}

		// GET: Users/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,FullName,Email,PhoneNumber,Address,Password,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FullName,Email,PhoneNumber,Address,Password,Role")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'BookStoreContext.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}

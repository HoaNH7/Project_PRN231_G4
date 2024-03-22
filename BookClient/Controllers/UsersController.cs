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
using System.Text;
using BookClient.ViewModel;

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

        //public async Task<IActionResult> Register()
        //{
        //}

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

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            try
            {
                var existingUserQuery = $"{UserUrl}?$filter=Email eq '{Email}' and eq '{Password}'";
                HttpResponseMessage existingUserResponse = await client.GetAsync(existingUserQuery);
                existingUserResponse.EnsureSuccessStatusCode();
                string existingUserData = await existingUserResponse.Content.ReadAsStringAsync();
                var existingUsers = JsonConvert.DeserializeObject<List<User>>(JObject.Parse(existingUserData)["value"].ToString());

                if (existingUsers.Any(u => u.Email == Email && u.Password == Password))
                {
                    if (existingUsers.Any(u => u.Role == "admin"))
                    {
                        HttpContext.Session.SetString("Role", "admin");
                    }
                    else
                    {
                        HttpContext.Session.SetString("Role", "user");
                    }
                    HttpContext.Session.SetString("Email", Email);
                    HttpContext.Session.SetInt32("UserId", 0);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "User with this email already exists.";
                    return RedirectToAction("Login", "Users");
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                return View();
            }

        }

        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string Email, string Password, string rePassword)
        {
            try
            {
                var existingUserQuery = $"{UserUrl}?$filter=Email eq '{Email}'";
                HttpResponseMessage existingUserResponse = await client.GetAsync(existingUserQuery);
                existingUserResponse.EnsureSuccessStatusCode();
                string existingUserData = await existingUserResponse.Content.ReadAsStringAsync();
                var existingUsers = JsonConvert.DeserializeObject<List<User>>(JObject.Parse(existingUserData)["value"].ToString());

                if (existingUsers.Any(u => u.Email == Email))
                {
                    TempData["ErrorMessage"] = "User with this email already exists.";
                    TempData["Email"] = Email;
                    return RedirectToAction("Register","Users");
                }

                var newUser = new RegisterViewModel
                {
                    Email = Email,
                    Password = Password,
                    RePassword = rePassword,
                    Role = "user"
                };


                if (Password != rePassword && Email == null)
                {
                    TempData["Password"] = "Passwords do not match.";
                    return RedirectToAction("Register", "Users"); 
                }
                string userDataJson = JsonConvert.SerializeObject(newUser);
                var content = new StringContent(userDataJson, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(UserUrl, content);
                response.EnsureSuccessStatusCode();

                return RedirectToAction("Login", "Users");
            }
            catch (Exception ex)
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
            try
            {
                string strData = JsonConvert.SerializeObject(user);
                var content = new StringContent(strData, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync(UserUrl, content);
                res.EnsureSuccessStatusCode();
                return RedirectToAction("Index");
            }
            catch
            {
                return NotFound();
            }
            //if (ModelState.IsValid)
            //{
            //    _context.Add(user);
            //    await _context.SaveChangesAsync();
            //    return RedirectToAction(nameof(Index));
            //}
            //return View(user);
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

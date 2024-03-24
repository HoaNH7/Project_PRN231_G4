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
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 5)
        {
            HttpResponseMessage boRes = await client.GetAsync(UserUrl);
            string strDataBo = await boRes.Content.ReadAsStringAsync();
            var dataBo = JObject.Parse(strDataBo);
            List<User> us = JsonConvert.DeserializeObject<List<User>>(dataBo["value"].ToString());

            if (!string.IsNullOrEmpty(search))
            {
                us = us.Where(p =>
                    p.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                   p.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                   p.Address.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var filteredUser = us.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Model = filteredUser;

            int totalUserCount = us.Count; // Tổng số User
            ViewBag.PageNumber = page; // Số trang hiện tại
            ViewBag.PageSize = pageSize; // Kích thước trang
            ViewBag.TotalBooksCount = totalUserCount; // Tổng số sách
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalUserCount / pageSize); // Tổng số trang
            ViewBag.SearchQuery = search;
            return View(filteredUser);
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

                var logUser = existingUsers.FirstOrDefault(u => u.Email == Email && u.Password == Password);
                if(logUser != null)
                {
                    if (logUser.Role == "admin")
                    {
                        HttpContext.Session.SetString("Role", "admin");
                    }
                    if (logUser.Role == "user")
                    {
                        HttpContext.Session.SetString("Role", "user");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Email or password invalid.";
                    return RedirectToAction("Login", "Users");
                }
                HttpContext.Session.SetString("Email", Email);
                HttpContext.Session.SetInt32("UserId", 0);
                return RedirectToAction("Index", "Home");
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
                    return RedirectToAction("Register", "Users");
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
        public async Task<IActionResult> Details(int? id, User user)
        {

            if (id == null)
            {
                return NotFound();
            }

            HttpResponseMessage response = await client.GetAsync($"{UserUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                string strData = await response.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                User us = JsonSerializer.Deserialize<User>(strData, options);
                return View(us);
            }
            else
            {
                return NotFound();
            }
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
                string strData = JsonSerializer.Serialize(user);
                var content = new StringContent(strData, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync(UserUrl, content);
                return RedirectToAction("Index");

            }
            catch
            {
                return View(user);
            }
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            HttpResponseMessage res = await client.GetAsync($"{UserUrl}/{id}");
            if (res.IsSuccessStatusCode)
            {
                string strData = await res.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                User us = JsonSerializer.Deserialize<User>(strData, options);
                return View(us);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FullName,Email,PhoneNumber,Address,Password,Role")] User user)
        {
            try
            {
                string strData = JsonConvert.SerializeObject(user);
                var content = new StringContent(strData, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PutAsync($"{UserUrl}/{id}", content);
                res.EnsureSuccessStatusCode();
                return RedirectToAction("Index");
            }
            catch
            {
                return View(user);
            }
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                HttpResponseMessage res = await client.GetAsync($"{UserUrl}/{id}");
                res.EnsureSuccessStatusCode();
                string strData = await res.Content.ReadAsStringAsync();
                User us = JsonSerializer.Deserialize<User>(strData);
                return View(us);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                HttpResponseMessage res = await client.DeleteAsync($"{UserUrl}/{id}");
                res.EnsureSuccessStatusCode();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
                return View();
            }
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}

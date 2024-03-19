using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookClient.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace BookClient.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookStoreContext _context;
        private readonly HttpClient client;
        private string CategoryUrl = "https://localhost:7006/odata/Categories";
        private string BookUrl = "https://localhost:7006/odata/Books";

        public BooksController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
        }

        // GET: Books
        public async Task<IActionResult> Index(string search)
        {
            HttpResponseMessage boRes = await client.GetAsync(BookUrl);
            string strDataBo = await boRes.Content.ReadAsStringAsync();
            var dataBo = JObject.Parse(strDataBo);
            List<Book> books = JsonConvert.DeserializeObject<List<Book>>(dataBo["value"].ToString());

            HttpResponseMessage catRes = await client.GetAsync(CategoryUrl);
            string strDataCat = await catRes.Content.ReadAsStringAsync();
            var dataCat = JObject.Parse(strDataCat);
            List<Category> categories = JsonConvert.DeserializeObject<List<Category>>(dataCat["value"].ToString());

            // Gán thông tin category cho mỗi sản phẩm
            foreach (var book in books)
            {
                book.Category = categories.FirstOrDefault(c => c.CategoryId == book.CategoryId);
            }

            if (!string.IsNullOrEmpty(search))
            {
                // Lọc sản phẩm dựa trên truy vấn tìm kiếm
                //books = books.Where(p =>
                //    p.BookName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                //    p.UnitPrice.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ||
                //    p.Category.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase)
                //).ToList();
            }
            ViewBag.SearchQuery = search;
            return View(books);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id, Book books)
        {
            HttpResponseMessage response = await client.GetAsync($"{BookUrl}/{id}");
            string strData = await response.Content.ReadAsStringAsync();
            books = JsonConvert.DeserializeObject<Book>(strData);
            return View(books);
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            HttpResponseMessage response = await client.GetAsync(CategoryUrl);
            response.EnsureSuccessStatusCode(); // Đảm bảo yêu cầu thành công

            string strData = await response.Content.ReadAsStringAsync();
            var dataCat = JObject.Parse(strData);
            List<Category> categories = JsonConvert.DeserializeObject<List<Category>>(dataCat["value"].ToString());

            return categories;
        }

        // GET: Books/Create
        public async Task<IActionResult> Create()
        {
            List<Category> categories = await GetCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,Title,Author,ImageUrl,CategoryId,Price,Description")] Book book)
        {
            try
            {
                List<Category> categories = await GetCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
                string strData = JsonConvert.SerializeObject(book);
                var content = new StringContent(strData, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync(BookUrl, content);
                res.EnsureSuccessStatusCode();
                return RedirectToAction("Index");
            }
            catch
            {
                return View(book);
            }
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                List<Category> categories = await GetCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
                HttpResponseMessage res = await client.GetAsync($"{BookUrl}/{id}");
                res.EnsureSuccessStatusCode();
                string strData = await res.Content.ReadAsStringAsync();
                Book books = JsonConvert.DeserializeObject<Book>(strData);
                return View(books);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookId,Title,Author,ImageUrl,CategoryId,Price,Description")] Book book)
        {
            try
            {
                List<Category> categories = await GetCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
                string strData = JsonConvert.SerializeObject(book);
                var content = new StringContent(strData, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PutAsync($"{BookUrl}/{id}", content);
                res.EnsureSuccessStatusCode();
                return RedirectToAction("Index");
            }
            catch
            {
                return View(book);
            }
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                HttpResponseMessage res = await client.GetAsync($"{BookUrl}/{id}");
                res.EnsureSuccessStatusCode();
                string strData = await res.Content.ReadAsStringAsync();
                Book books = JsonConvert.DeserializeObject<Book>(strData);
                return View(books);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, Book books)
        {
            try
            {
                HttpResponseMessage res = await client.DeleteAsync($"{BookUrl}/{id}");
                res.EnsureSuccessStatusCode();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
                return View();
            }
        }

    }
}

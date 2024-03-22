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
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;

namespace BookClient.Controllers
{
    public class BooksController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient client;
        private string CategoryUrl = "https://localhost:7006/odata/Categories";
        private string BookUrl = "https://localhost:7006/odata/Books";

        public BooksController(IConfiguration configuration)
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            _configuration = configuration;
        }

        // GET: Books
        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 5)
        {
            HttpResponseMessage boRes = await client.GetAsync(BookUrl);
            string strDataBo = await boRes.Content.ReadAsStringAsync();
            var dataBo = JObject.Parse(strDataBo);
            List<Book> books = JsonConvert.DeserializeObject<List<Book>>(dataBo["value"].ToString());

            HttpResponseMessage catRes = await client.GetAsync(CategoryUrl);
            string strDataCat = await catRes.Content.ReadAsStringAsync();
            var dataCat = JObject.Parse(strDataCat);
            List<Category> categories = JsonConvert.DeserializeObject<List<Category>>(dataCat["value"].ToString());

            foreach (var book in books)
            {
                book.Category = categories.FirstOrDefault(c => c.CategoryId == book.CategoryId);
            }

            if (!string.IsNullOrEmpty(search))
            {
               books = books.Where(b =>
                   b.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                   b.Author.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                   b.Category.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase)
               ).ToList();
            }
            var filteredBooks = books.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Model = filteredBooks;

            int totalBooksCount = books.Count; // Tổng số sách
            ViewBag.PageNumber = page; // Số trang hiện tại
            ViewBag.PageSize = pageSize; // Kích thước trang
            ViewBag.TotalBooksCount = totalBooksCount; // Tổng số sách
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalBooksCount / pageSize); // Tổng số trang
            ViewBag.SearchQuery = search;
            return View(filteredBooks);
        }


        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id, Book books)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Tạo một đối tượng book để chứa thông tin sách
            Book book;

            // Gửi request để lấy thông tin sách từ API
            HttpResponseMessage bookResponse = await client.GetAsync($"{BookUrl}/{id}");

            if (bookResponse.IsSuccessStatusCode)
            {
                // Đọc và chuyển đổi dữ liệu sách từ response
                string bookData = await bookResponse.Content.ReadAsStringAsync();
                book = JsonConvert.DeserializeObject<Book>(bookData);
            }
            else
            {
                // Trả về NotFound nếu không tìm thấy sách
                return NotFound();
            }

            // Tạo một đối tượng category để chứa thông tin danh mục
            Category category;

            // Gửi request để lấy thông tin danh mục từ API
            HttpResponseMessage categoryResponse = await client.GetAsync($"{CategoryUrl}/{book.CategoryId}");

            if (categoryResponse.IsSuccessStatusCode)
            {
                // Đọc và chuyển đổi dữ liệu danh mục từ response
                string categoryData = await categoryResponse.Content.ReadAsStringAsync();
                category = JsonConvert.DeserializeObject<Category>(categoryData);
            }
            else
            {
                // Nếu không tìm thấy danh mục, gán danh mục là null
                category = null;
            }

            // Gán thông tin danh mục vào sách
            book.Category = category;

            // Truyền đối tượng sách với thông tin danh mục vào view
            return View(book);
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
        public async Task<IActionResult> Remove(int? id)
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id, Book books)
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

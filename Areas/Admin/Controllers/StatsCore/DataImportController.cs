using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http.Headers;

namespace T1EsportsWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DataImportController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _pythonApiBase;

        public DataImportController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _pythonApiBase = configuration["PythonApi:BaseUrl"];
        }

        // GET: /Admin/DataImport
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/DataImport/DownloadTemplate
        public IActionResult DownloadTemplate()
        {
            // Đường dẫn đến file template (đặt trong wwwroot/templates)
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "ImportTemplate.xlsx");
            Console.WriteLine(filePath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"File not found at {filePath}");
            }
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ImportTemplate.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> Preview(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file");

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // Đưa con trỏ về đầu để đọc dữ liệu

                var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(memoryStream);

                // Thiết lập header đúng chuẩn Excel
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                content.Add(fileContent, "file", file.FileName);

                // Sử dụng _httpClient đã có sẵn của Class
                var response = await _httpClient.PostAsync($"{_pythonApiBase}/preview", content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, responseString);
                }

                return Content(responseString, "application/json");
            }
            catch (Exception ex)
            {
                // Log lỗi ra Console để bạn dễ debug
                Console.WriteLine($"==> LỖI KẾT NỐI PYTHON: {ex.Message}");
                return StatusCode(500, $"Không thể kết nối tới Server Python tại {_pythonApiBase}. Chi tiết: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file, string server, string database, string authType, string username, string password)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            var formData = new MultipartFormDataContent();
            var fileContent = new StreamContent(memoryStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            formData.Add(fileContent, "file", file.FileName);
            formData.Add(new StringContent(server ?? ""), "server");
            formData.Add(new StringContent(database ?? ""), "database");
            formData.Add(new StringContent(authType ?? "windows"), "auth_type");
            formData.Add(new StringContent(username ?? ""), "username");
            formData.Add(new StringContent(password ?? ""), "password");

            var response = await _httpClient.PostAsync($"{_pythonApiBase}/import", formData);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
    }
}
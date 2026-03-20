using Microsoft.AspNetCore.Mvc;
using T1EsportsWeb.Models;
using System.Collections.Generic;

namespace T1EsportsWeb.Areas.Admin.Controllers.ShopExtensions
{
    public class RosterController : Controller
    {
        public IActionResult Index()
        {
            // Thay đổi ImageUrl thành đường dẫn file ảnh trong máy của bạn
            var t1Roster = new List<Player>
            {
                new Player { Id = 1, IngameName = "Doran", Role = "Top Lane", SignatureChampion = "Jayce", ImageUrl = "/images/Doran.jpg" },
                new Player { Id = 2, IngameName = "Oner", Role = "Jungle", SignatureChampion = "Lee Sin", ImageUrl = "/images/Oner.jpg" },
                new Player { Id = 3, IngameName = "Faker", Role = "Mid Lane", SignatureChampion = "Azir", ImageUrl = "/images/Faker.jpg" },
                new Player { Id = 4, IngameName = "Peyz", Role = "ADC", SignatureChampion = "Kaisa", ImageUrl = "/images/peyz.jpg" },
                new Player { Id = 5, IngameName = "Keria", Role = "Support", SignatureChampion = "Thresh", ImageUrl = "/images/Keria.jpg" }
            };

            return View(t1Roster);
        }

        // Thêm hàm này vào trong RosterController
        public IActionResult Details(int id)
        {
            // Giả lập dữ liệu của Faker để thiết kế giao diện
            var playerDetail = new Player
            {
                Id = 3,
                IngameName = "Faker",
                Role = "Mid Lane",
                SignatureChampion = "Azir, Orianna, Ryze",
                ImageUrl = "/images/faker.jpg"
                // Sau này mình sẽ thêm các thuộc tính list trận đấu vào đây
            };

            return View(playerDetail);
        }
    }
}
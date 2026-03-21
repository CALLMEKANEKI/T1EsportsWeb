using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using T1EsportsWeb.Models;
using System;
using System.Linq;

namespace T1EsportsWeb.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly T1DbContext _context;

        public ChatController(T1DbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SendMessage(string messageContent, string receiver)
        {
            if (string.IsNullOrWhiteSpace(messageContent)) return BadRequest();

            string senderName = (User.IsInRole("Admin") || User.IsInRole("Staff")) ? "Admin" : User.Identity.Name;

            var msg = new ChatMessage
            {
                SenderUsername = senderName,
                ReceiverUsername = receiver,
                MessageContent = messageContent,
                Timestamp = DateTime.Now,
                IsRead = false // Mặc định là chưa đọc
            };

            _context.ChatMessages.Add(msg);
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public IActionResult GetMessages(string withUser)
        {
            string currentUser = (User.IsInRole("Admin") || User.IsInRole("Staff")) ? "Admin" : User.Identity.Name;

            var msgs = _context.ChatMessages
                .Where(m => (m.SenderUsername == currentUser && m.ReceiverUsername == withUser) ||
                            (m.SenderUsername == withUser && m.ReceiverUsername == currentUser) ||
                            (currentUser == "Admin" && m.ReceiverUsername == "Admin" && m.SenderUsername == withUser) ||
                            (currentUser == "Admin" && m.SenderUsername == "Admin" && m.ReceiverUsername == withUser))
                .OrderBy(m => m.Timestamp)
                .ToList();

            return Json(msgs);
        }

        // --- TÍNH NĂNG MỚI: ĐẾM TIN NHẮN CHƯA ĐỌC ---
        [HttpGet]
        public IActionResult GetUnreadCount()
        {
            var currentUser = User.Identity.Name;
            var count = _context.ChatMessages
                .Count(m => m.ReceiverUsername == currentUser && m.SenderUsername == "Admin" && !m.IsRead);
            return Json(new { count = count });
        }

        // --- TÍNH NĂNG MỚI: ĐÁNH DẤU ĐÃ ĐỌC ---
        [HttpPost]
        public IActionResult MarkAsRead()
        {
            var currentUser = User.Identity.Name;
            var unreadMsgs = _context.ChatMessages
                .Where(m => m.ReceiverUsername == currentUser && m.SenderUsername == "Admin" && !m.IsRead)
                .ToList();

            foreach (var msg in unreadMsgs) { msg.IsRead = true; }
            _context.SaveChanges();
            return Ok();
        }
    }
}
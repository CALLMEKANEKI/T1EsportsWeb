# 🎮 Dự án Website T1 Shop (T1 Esports)

Chào mừng các bạn đến với dự án quản lý cửa hàng T1 Esports! Trang web được xây dựng bằng ASP.NET Core MVC, tích hợp đầy đủ chức năng từ cửa hàng, giỏ hàng, đến hệ thống quản trị (Dashboard) và Chatbox hỗ trợ khách hàng Real-time.

Dự án được khởi tạo và phát triển bởi: **Nguyễn Phi Anh**

---

## 🛠️ Yêu cầu môi trường
Để chạy được project này, máy tính của bạn cần cài đặt sẵn:
- **Visual Studio 2022** (Có workload ASP.NET and web development).
- **SQL Server** & **SQL Server Management Studio (SSMS)**.

---

## 🚀 Hướng dẫn Cài đặt & Chạy Project cho Team

Các thành viên trong nhóm vui lòng làm đúng theo 3 bước sau để đồng bộ Code và Database nhé:

### Bước 1: Tải Code về máy (Clone)
1. Mở Visual Studio 2022 -> Chọn **Clone a repository**.
2. Dán link GitHub của repo này vào và chọn thư mục lưu trên máy.
3. Nhấn **Clone** và đợi Visual Studio tải code về.

### Bước 2: Phục hồi Database (Quan trọng ⚠️)
Vì database nằm ở máy của Phi Anh, các bạn cần chạy file script để tạo lại toàn bộ dữ liệu (Sản phẩm, User, Chat...) trên máy cá nhân:
1. Mở **SQL Server Management Studio (SSMS)** và kết nối vào Server của bạn.
2. Mở file `T1EsportsWeb_DB.sql` (nằm trong thư mục `Database` của project vừa tải về) bằng SSMS.
3. Nhấn phím **F5** (hoặc nút **Execute**) để chạy lệnh. 
👉 *Hệ thống báo "Command(s) completed successfully" là bạn đã có trọn bộ Database giống hệt bản gốc!*

### Bước 3: Đổi chuỗi kết nối (Connection String)
Để code nhận diện được Database vừa tạo trên máy bạn:
1. Mở file `appsettings.json` trong Visual Studio.
2. Tìm đến dòng `"DefaultConnection"`.
3. Thay đổi giá trị `Server=...` thành Tên Server SQL của máy bạn (Ví dụ: `Server=.\SQLEXPRESS` hoặc `Server=TEN-MAY-TINH-CUA-BAN`).
4. Lưu file lại (**Ctrl + S**).

### Bước 4: Chạy Website
Nhấn **Ctrl + F5** (hoặc nút Run) trên Visual Studio để khởi động trang web và tận hưởng thành quả!

---
*Nếu trong quá trình cài đặt gặp lỗi kết nối Database hoặc lỗi thư viện, hãy nhắn tin ngay cho Phi Anh để được hỗ trợ gỡ lỗi nhé! Chúc cả nhóm code thật mượt! ❤️*
# T1EsportsWeb - Website thống kê hiệu suất thi đấu của đội tuyển T1 (Liên Minh Huyền Thoại)

## 📌 Giới thiệu
Đây là đồ án môn học Lập trình Web của sinh viên Trường Giang, xây dựng một website thống kê toàn diện về đội tuyển T1 – một trong những đội tuyển Liên Minh Huyền Thoại (LMHT) thành công nhất lịch sử. Website cung cấp các số liệu chi tiết về các trận đấu, thành tích theo giải đấu, thống kê về tướng và tuyển thủ, lịch sử đối đầu, cùng với hệ thống quản trị (Admin) để cập nhật dữ liệu.

## 🛠 Công nghệ sử dụng
- **Backend**: ASP.NET Core MVC (.NET 8.0), C#
- **Frontend**: HTML/CSS/JS, Bootstrap 5, Chart.js, Font Awesome
- **Database**: SQL Server, Entity Framework Core (Code-first / Database-first)
- **Authentication**: Cookie Authentication, phân quyền Admin/Staff/User
- **Python Integration**: Flask API (riêng biệt) để import dữ liệu từ file Excel
- **Khác**: Repository Pattern, Dependency Injection, Session, AJAX

## 🚀 Chức năng chính
Website được chia làm 4 phần chính dành cho người dùng thông thường và một khu vực quản trị riêng.

### 1. Trang chủ
- Giới thiệu về đội tuyển T1, các thành tích nổi bật.
- Điều hướng đến các chức năng chính (đội hình, dashboard, lịch sử đấu).

### 2. Tuyển thủ (Roster)
- Hiển thị danh sách tuyển thủ của T1 (phân biệt đội hình hiện tại và cựu thành viên).
- Mỗi tuyển thủ có một trang dashboard riêng với các thống kê:
  - Tổng số series đã đấu, số trận thắng/thua, tỷ lệ thắng.
  - Danh sách các tướng đã sử dụng (kèm số lần pick, số trận thắng, tỷ lệ thắng).
  - Thống kê theo giải đấu, theo đối thủ (có bộ lọc).

### 3. Dashboard đội tuyển T1
Tổng hợp các thống kê chuyên sâu dưới dạng biểu đồ và bảng:
- **Thành tích**: Số series quốc nội/quốc tế, số game, BO3/BO5, tỷ lệ thắng theo side (Blue/Red).
- **Series và Games theo giải đấu**: Biểu đồ cột chồng, hiển thị số thắng/thua, phân biệt màu theo khu vực (KR/INT).
- **Winrate theo phiên bản (Patch)**: Biểu đồ đường.
- **Thống kê tuyển thủ theo vị trí**: Biểu đồ cột với bộ lọc role.
- **Lịch sử đối đầu**: Thống kê tỷ lệ thắng trước từng đội, phân biệt quốc nội/quốc tế.
- **Thống kê tướng (Champion)**: Bảng pick/ban của T1 và của đối thủ, có bộ lọc theo giải đấu và đối thủ.
- **Dự đoán tướng T1 sẽ chọn**: Dựa trên lịch sử đối đầu (có bộ lọc đối thủ và vị trí).

### 4. Lịch sử đấu (Match History)
- Danh sách các series (loạt trận) của T1, có thể lọc theo ngày và giải đấu.
- Mỗi series có thể mở rộng để xem chi tiết các game (ván đấu).
- Mỗi game hiển thị thông tin: patch, side, kết quả, link YouTube, và nút "Chi tiết" để xem lineup (đội hình) và bans (tướng bị cấm) của cả hai đội (kèm ảnh tướng, ảnh tuyển thủ, tooltip).

### 5. Khu vực quản trị (Admin)
- Dành cho người dùng có role Admin hoặc Staff.
- **Quản lý các bảng master**: Đội tuyển (Teams), Giải đấu (Tournaments), Tướng (Champions), Tuyển thủ (Players) – CRUD đầy đủ, có upload ảnh, phân trang, lọc.
- **Import dữ liệu lịch sử đấu**:
  - Upload file Excel có cấu trúc giống dữ liệu đã cào từ lol.fandom.com.
  - Xem trước dữ liệu (preview) trước khi import.
  - Gọi Python API để xử lý và insert vào SQL Server.
- **Quản lý sản phẩm (Shop)**: CRUD sản phẩm, quản lý đơn hàng, voucher.
- **Chăm sóc khách hàng (Chat)**: Phòng chat đơn giản giữa admin và người dùng.
- **Quản lý người dùng và phân quyền**: Phân vai trò Admin / Staff / User.

## 📥 Hướng dẫn cài đặt và chạy

### Yêu cầu hệ thống
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB hoặc Express)
- [Python 3.9+](https://www.python.org/downloads/) (cho API import)
- Visual Studio 2022 hoặc VS Code

### Các bước thực hiện

1. **Clone dự án**
   ```bash
   git clone https://github.com/your-repo/T1EsportsWeb.git
   cd T1EsportsWeb

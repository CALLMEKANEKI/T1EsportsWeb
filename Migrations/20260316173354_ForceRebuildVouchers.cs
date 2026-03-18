using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace T1EsportsWeb.Migrations
{
    /// <inheritdoc />
    public partial class ForceRebuildVouchers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Dùng lệnh SQL ép xóa bảng cũ nếu nó còn bị kẹt
            migrationBuilder.Sql("IF OBJECT_ID('dbo.UserVouchers', 'U') IS NOT NULL DROP TABLE dbo.UserVouchers;");
            migrationBuilder.Sql("IF OBJECT_ID('dbo.Vouchers', 'U') IS NOT NULL DROP TABLE dbo.Vouchers;");

            // 2. Tạo lại bảng Vouchers chuẩn xịn
            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    CostPoints = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    EventCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.Id);
                });

            // 3. Tạo lại bảng UserVouchers chuẩn xịn
            migrationBuilder.CreateTable(
                name: "UserVouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    VoucherId = table.Column<int>(type: "int", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoucherCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVouchers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Vouchers",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "EventCode",
                table: "Vouchers");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Vouchers",
                newName: "VoucherName");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Vouchers",
                newName: "VoucherId");

            migrationBuilder.AlterColumn<int>(
                name: "VoucherId",
                table: "Vouchers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vouchers",
                table: "Vouchers",
                column: "VoucherId");
        }
    }
}

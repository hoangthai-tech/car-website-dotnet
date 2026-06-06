using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWebsite.Migrations
{
    /// <inheritdoc />
    public partial class ClearSampleImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Cars] SET [ImagesJson] = '[]'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}

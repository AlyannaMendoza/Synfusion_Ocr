using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYNCFUSION_TRIAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ocrResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileMetadataId = table.Column<int>(type: "int", nullable: false),
                    ExtractedText = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ocrResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ocrResults_fileMetadata_FileMetadataId",
                        column: x => x.FileMetadataId,
                        principalTable: "fileMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ocrResults_FileMetadataId",
                table: "ocrResults",
                column: "FileMetadataId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ocrResults");
        }
    }
}

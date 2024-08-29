using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SYNCFUSION_TRIAL.Models;

namespace SYNCFUSION_TRIAL.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FileData> fileData { get; set; }
        public DbSet<FileMetadata> fileMetadata { get; set; }
        public DbSet<OcrResult> ocrResults { get; set; }
    }
}

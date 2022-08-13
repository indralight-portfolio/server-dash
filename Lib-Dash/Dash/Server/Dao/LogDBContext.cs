#if Common_Server
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Dash.Model.Rdb;

#nullable disable

namespace Dash.Server.Dao
{
    public partial class LogDBContext : DbContext
    {
        public LogDBContext()
        {
        }

        public LogDBContext(DbContextOptions options)
            : base(options)
        {
        }

        public LogDBContext(DbContextOptions<LogDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<GameLog> GameLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_general_ci");

            modelBuilder.Entity<GameLog>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Time })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Country).IsFixedLength(true);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
#endif
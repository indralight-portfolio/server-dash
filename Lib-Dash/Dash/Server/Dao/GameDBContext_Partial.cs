#if Common_Server
using Dash.Model;
using Dash.Model.Cache;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Dash.Server.Dao
{
    public partial class GameDBContext : DbContext
    {
        // DB에서 쿼리 결과 를 모델로 변환하기 위해 DBContext 에 등록해 줘야 한다.
        public virtual DbSet<TableSchema> TableSchema { get; set; }
        public virtual DbSet<SumResult> SumModel { get; set; }
        public virtual DbSet<AccountGuild> AccountGuild { get; set; }
        public virtual DbSet<FriendClientCacheModel> FriendClientCacheModel { get; set; }
        public virtual DbSet<GuildMemberClientModel> GuildMemberClientModel { get; set; }
        public virtual DbSet<SearchPlayerModel> SearchPlayerModel { get; set; }
        public virtual DbSet<AuthRef> AuthRef { get; set; }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TableSchema>(entity =>
            {
                entity.HasNoKey();
            });
            modelBuilder.Entity<SumResult>(entity =>
            {
                entity.HasNoKey();
            });
            modelBuilder.Entity<AccountGuild>(entity =>
            {
                entity.HasKey(e => e.OidAccount)
                    .HasName("PRIMARY");
            });

            modelBuilder.Entity<FriendClientCacheModel>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.OidFriend })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<GuildMemberClientModel>(entity =>
            {
                entity.HasKey(e => new { e.OidGuild, e.OidAccount })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<SearchPlayerModel>(entity =>
            {
                entity.HasKey(e => e.OidAccount)
                    .HasName("PRIMARY");
            });

            modelBuilder.Entity<AuthRef>(entity =>
            {
                entity.HasKey(e => e.AuthId)
                    .HasName("PRIMARY");
            });
        }
    }
}
#endif
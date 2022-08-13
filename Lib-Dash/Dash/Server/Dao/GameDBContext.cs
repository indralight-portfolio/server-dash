#if Common_Server
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Dash.Model.Rdb;

#nullable disable

namespace Dash.Server.Dao
{
    public partial class GameDBContext : DbContext
    {
        public GameDBContext()
        {
        }

        public GameDBContext(DbContextOptions options)
            : base(options)
        {
        }

        public GameDBContext(DbContextOptions<GameDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<AccountExtra> AccountExtra { get; set; }
        public virtual DbSet<Attendance> Attendance { get; set; }
        public virtual DbSet<Auth> Auth { get; set; }
        public virtual DbSet<Character> Character { get; set; }
        public virtual DbSet<Collection> Collection { get; set; }
        public virtual DbSet<CollectionHistory> CollectionHistory { get; set; }
        public virtual DbSet<CompletedMission> CompletedMission { get; set; }
        public virtual DbSet<ConquestScore> ConquestScore { get; set; }
        public virtual DbSet<ConquestScoreRecord> ConquestScoreRecord { get; set; }
        public virtual DbSet<Consume> Consume { get; set; }
        public virtual DbSet<Coupon> Coupon { get; set; }
        public virtual DbSet<CouponUse> CouponUse { get; set; }
        public virtual DbSet<DailyReward> DailyReward { get; set; }
        public virtual DbSet<Deck> Deck { get; set; }
        public virtual DbSet<EpisodeClear> EpisodeClear { get; set; }
        public virtual DbSet<EpisodeEntryCount> EpisodeEntryCount { get; set; }
        public virtual DbSet<EquipRune> EquipRune { get; set; }
        public virtual DbSet<Equipment> Equipment { get; set; }
        public virtual DbSet<Friend> Friend { get; set; }
        public virtual DbSet<GachaHistory> GachaHistory { get; set; }
        public virtual DbSet<GameEvent> GameEvent { get; set; }
        public virtual DbSet<Guild> Guild { get; set; }
        public virtual DbSet<GuildMember> GuildMember { get; set; }
        public virtual DbSet<HiveItem> HiveItem { get; set; }
        public virtual DbSet<IapReceipt> IapReceipt { get; set; }
        public virtual DbSet<IssueSerial> IssueSerial { get; set; }
        public virtual DbSet<Mail> Mail { get; set; }
        public virtual DbSet<MailTarget> MailTarget { get; set; }
        public virtual DbSet<MailTemplate> MailTemplate { get; set; }
        public virtual DbSet<Money> Money { get; set; }
        public virtual DbSet<MultipleDeck> MultipleDeck { get; set; }
        public virtual DbSet<PeriodOverride> PeriodOverride { get; set; }
        public virtual DbSet<Progress> Progress { get; set; }
        public virtual DbSet<ReservedMail> ReservedMail { get; set; }
        public virtual DbSet<ReservedMailSent> ReservedMailSent { get; set; }
        public virtual DbSet<Retention> Retention { get; set; }
        public virtual DbSet<SeasonPass> SeasonPass { get; set; }
        public virtual DbSet<ShopHistory> ShopHistory { get; set; }
        public virtual DbSet<ShopReceipt> ShopReceipt { get; set; }
        public virtual DbSet<TimeResourceReward> TimeResourceReward { get; set; }
        public virtual DbSet<TimeReward> TimeReward { get; set; }
        public virtual DbSet<WorldMissionScore> WorldMissionScore { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_general_ci");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.OidAccount)
                    .HasName("PRIMARY");

                entity.Property(e => e.Country).IsFixedLength(true);

                entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP(3)");

                entity.Property(e => e.LatestLogon).HasDefaultValueSql("CURRENT_TIMESTAMP(3)");

                entity.Property(e => e.Level).HasDefaultValueSql("'1'");
            });

            modelBuilder.Entity<AccountExtra>(entity =>
            {
                entity.HasKey(e => e.OidAccount)
                    .HasName("PRIMARY");

                entity.Property(e => e.OidAccount).ValueGeneratedNever();

                entity.Property(e => e.Language).IsFixedLength(true);
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<Auth>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.AuthType })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<Character>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<Collection>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<CollectionHistory>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<CompletedMission>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.MissionId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<ConquestScore>(entity =>
            {
                entity.HasKey(e => e.OidAccount)
                    .HasName("PRIMARY");

                entity.Property(e => e.OidAccount).ValueGeneratedNever();
            });

            modelBuilder.Entity<ConquestScoreRecord>(entity =>
            {
                entity.HasKey(e => new { e.SeasonId, e.OidAccount })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<Consume>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasComment("소모할 수 있는 아이템");
            });

            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.Property(e => e.Code).IsFixedLength(true);

                entity.Property(e => e.End).HasDefaultValueSql("'2050-12-31 00:00:00.000'");

                entity.Property(e => e.Start).HasDefaultValueSql("'1970-01-01 00:00:00.000'");
            });

            modelBuilder.Entity<CouponUse>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.CouponId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<DailyReward>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<Deck>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<EpisodeClear>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.EpisodeId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<EpisodeEntryCount>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<EquipRune>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.CharacterId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Serial })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<Friend>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.OidFriend })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<GachaHistory>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<GameEvent>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.EventId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<Guild>(entity =>
            {
                entity.HasKey(e => e.OidGuild)
                    .HasName("PRIMARY");

                entity.Property(e => e.Name).IsFixedLength(true);
            });

            modelBuilder.Entity<GuildMember>(entity =>
            {
                entity.HasKey(e => new { e.OidGuild, e.OidAccount })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<HiveItem>(entity =>
            {
                entity.HasKey(e => e.TransactionId)
                    .HasName("PRIMARY");
            });

            modelBuilder.Entity<IapReceipt>(entity =>
            {
                entity.HasKey(e => e.TransactionId)
                    .HasName("PRIMARY");

                entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP(3)");
            });

            modelBuilder.Entity<IssueSerial>(entity =>
            {
                entity.HasKey(e => e.OidAccount)
                    .HasName("PRIMARY");

                entity.Property(e => e.OidAccount).ValueGeneratedNever();
            });

            modelBuilder.Entity<Mail>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP(3)");
            });

            modelBuilder.Entity<MailTarget>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.TemplateId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<MailTemplate>(entity =>
            {
                entity.Property(e => e.End).HasDefaultValueSql("'2050-12-31 00:00:00.000'");

                entity.Property(e => e.Start).HasDefaultValueSql("'1970-01-01 00:00:00.000'");
            });

            modelBuilder.Entity<Money>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Type })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<MultipleDeck>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.DeckId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<PeriodOverride>(entity =>
            {
                entity.HasKey(e => new { e.Type, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<Progress>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Key })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<ReservedMailSent>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<Retention>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Date })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.Property(e => e.Date).HasDefaultValueSql("'1900-01-01'");

                entity.Property(e => e.Country)
                    .HasDefaultValueSql("'US'")
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<SeasonPass>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<ShopHistory>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.ProductId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<ShopReceipt>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<TimeResourceReward>(entity =>
            {
                entity.HasKey(e => e.OidAccount)
                    .HasName("PRIMARY");

                entity.Property(e => e.OidAccount).ValueGeneratedNever();
            });

            modelBuilder.Entity<TimeReward>(entity =>
            {
                entity.HasKey(e => new { e.OidAccount, e.RewardType })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<WorldMissionScore>(entity =>
            {
                entity.HasKey(e => e.OidAccount)
                    .HasName("PRIMARY");

                entity.Property(e => e.OidAccount).ValueGeneratedNever();

                entity.Property(e => e.CoopRewardedScore).HasComment("협력 보상 획득 했을때 점수");

                entity.Property(e => e.CoopScore).HasComment("협력점수");

                entity.Property(e => e.IsRankingRewarded).HasComment("랭킹보상 획득 여부");

                entity.Property(e => e.RewardedScore).HasComment("경쟁 보상 획득 했을때 점수");

                entity.Property(e => e.Score).HasComment("경쟁점수");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
#endif
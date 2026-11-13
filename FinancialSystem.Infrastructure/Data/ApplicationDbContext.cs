using Microsoft.EntityFrameworkCore;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<GSecTrade> GSecTrades { get; set; }
        public DbSet<FixedDeposit> FixedDeposits { get; set; }
        public DbSet<AppUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table-Per-Hierarchy (TPH) for Instrument
            modelBuilder.Entity<Instrument>()
                .HasDiscriminator<string>("InstrumentType")
                .HasValue<GSec>("GSec")
                .HasValue<MutualFund>("MutualFund")
                .HasValue<FixedDepositInstrument>("FixedDeposit");

            // Define precision for decimal columns to avoid warnings
            modelBuilder.Entity<Order>().Property(o => o.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Trade>().Property(t => t.TradePrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<MutualFund>().Property(m => m.Nav).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<GSec>().Property(g => g.CouponRate).HasColumnType("decimal(18,2)");

            // GSecTrade decimal precision (6 dp — bond price/yield/BPI standard)
            modelBuilder.Entity<GSecTrade>().Property(g => g.FaceValue).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<GSecTrade>().Property(g => g.Price).HasColumnType("decimal(18,6)");
            modelBuilder.Entity<GSecTrade>().Property(g => g.Yield).HasColumnType("decimal(10,6)");
            modelBuilder.Entity<GSecTrade>().Property(g => g.DealValue).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<GSecTrade>().Property(g => g.BrokenPeriod).HasColumnType("decimal(18,6)");

            // FixedDeposit decimal precision
            modelBuilder.Entity<FixedDeposit>().Property(f => f.Principal).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<FixedDeposit>().Property(f => f.InterestRate).HasColumnType("decimal(8,4)");
            modelBuilder.Entity<FixedDeposit>().Property(f => f.InterestAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<FixedDeposit>().Property(f => f.Tds).HasColumnType("decimal(18,2)");
        }
    }
}

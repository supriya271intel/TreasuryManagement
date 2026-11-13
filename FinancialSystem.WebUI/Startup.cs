using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Application.Services;
using FinancialSystem.Application.Factories;
using FinancialSystem.Infrastructure.Data;
using FinancialSystem.Integration.Services;
using Microsoft.EntityFrameworkCore;

namespace FinancialSystem.WebUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            // Authentication & Session for 2FA login flow
            services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Clean Architecture DI setup
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<ICbsApiService>(CbsApiClient.Instance);
            services.AddSingleton<IPolicyMatcher, MockPolicyMatcher>();
            services.AddTransient<ISettlementFacade, SettlementFacade>();
            services.AddTransient<ITradeProcessorFactory, TradeProcessorFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext dbContext)
        {
            dbContext.Database.Migrate();

            if (!dbContext.Orders.Any())
            {
                // --- Instruments ---
                var gsec1  = new FinancialSystem.Domain.Entities.GSec        { Name = "7.10% GOI 2031",          Isin = "IN0020020011", CouponRate = 7.10m };
                var gsec2  = new FinancialSystem.Domain.Entities.GSec        { Name = "6.54% GOI 2032",          Isin = "IN0020020022", CouponRate = 6.54m };
                var gsec3  = new FinancialSystem.Domain.Entities.GSec        { Name = "8.20% GOI 2034",          Isin = "IN0020020033", CouponRate = 8.20m };
                var mf1    = new FinancialSystem.Domain.Entities.MutualFund  { Name = "HDFC Bluechip Fund",       Isin = "INF179K01AA0", Nav = 45.20m  };
                var mf2    = new FinancialSystem.Domain.Entities.MutualFund  { Name = "SBI Nifty 50 Index Fund",  Isin = "INF200K01CN2", Nav = 189.75m };
                var mf3    = new FinancialSystem.Domain.Entities.MutualFund  { Name = "Mirae Asset Large Cap",    Isin = "INF769K01EW3", Nav = 98.40m  };
                var mf4    = new FinancialSystem.Domain.Entities.MutualFund  { Name = "Axis Long Term Equity",    Isin = "INF846K01131", Nav = 72.60m  };

                var now = System.DateTime.Now;

                // --- Orders ---
                var order1 = new FinancialSystem.Domain.Entities.Order { Instrument = gsec1, Price = 100.50m,  Quantity = 5000,  Status = FinancialSystem.Domain.Entities.OrderStatus.Matched,  CreatedAt = now.AddDays(-5).AddHours(-3) };
                var order2 = new FinancialSystem.Domain.Entities.Order { Instrument = gsec1, Price = 100.45m,  Quantity = 5000,  Status = FinancialSystem.Domain.Entities.OrderStatus.Matched,  CreatedAt = now.AddDays(-5).AddHours(-3).AddMinutes(-10) };
                var order3 = new FinancialSystem.Domain.Entities.Order { Instrument = gsec2, Price = 98.20m,   Quantity = 2500,  Status = FinancialSystem.Domain.Entities.OrderStatus.Matched,  CreatedAt = now.AddDays(-4).AddHours(-1) };
                var order4 = new FinancialSystem.Domain.Entities.Order { Instrument = gsec2, Price = 98.15m,   Quantity = 2500,  Status = FinancialSystem.Domain.Entities.OrderStatus.Matched,  CreatedAt = now.AddDays(-4).AddHours(-1).AddMinutes(-5) };
                var order5 = new FinancialSystem.Domain.Entities.Order { Instrument = gsec3, Price = 102.30m,  Quantity = 1000,  Status = FinancialSystem.Domain.Entities.OrderStatus.Pending,  CreatedAt = now.AddHours(-2) };
                var order6 = new FinancialSystem.Domain.Entities.Order { Instrument = mf1,   Price = 45.20m,   Quantity = 3000,  Status = FinancialSystem.Domain.Entities.OrderStatus.Matched,  CreatedAt = now.AddDays(-3).AddHours(-6) };
                var order7 = new FinancialSystem.Domain.Entities.Order { Instrument = mf1,   Price = 45.18m,   Quantity = 3000,  Status = FinancialSystem.Domain.Entities.OrderStatus.Matched,  CreatedAt = now.AddDays(-3).AddHours(-6).AddMinutes(-15) };
                var order8 = new FinancialSystem.Domain.Entities.Order { Instrument = mf2,   Price = 189.75m,  Quantity = 800,   Status = FinancialSystem.Domain.Entities.OrderStatus.Matched,  CreatedAt = now.AddDays(-2).AddHours(-4) };
                var order9 = new FinancialSystem.Domain.Entities.Order { Instrument = mf2,   Price = 189.70m,  Quantity = 800,   Status = FinancialSystem.Domain.Entities.OrderStatus.Matched,  CreatedAt = now.AddDays(-2).AddHours(-4).AddMinutes(-8) };
                var order10 = new FinancialSystem.Domain.Entities.Order { Instrument = mf3,  Price = 98.40m,   Quantity = 1500,  Status = FinancialSystem.Domain.Entities.OrderStatus.Pending,  CreatedAt = now.AddHours(-1) };
                var order11 = new FinancialSystem.Domain.Entities.Order { Instrument = mf4,  Price = 72.60m,   Quantity = 4000,  Status = FinancialSystem.Domain.Entities.OrderStatus.Rejected, CreatedAt = now.AddDays(-1).AddHours(-2) };
                var order12 = new FinancialSystem.Domain.Entities.Order { Instrument = gsec3, Price = 102.10m, Quantity = 2000,  Status = FinancialSystem.Domain.Entities.OrderStatus.Pending,  CreatedAt = now.AddMinutes(-30) };

                dbContext.Orders.AddRange(order1, order2, order3, order4, order5, order6, order7, order8, order9, order10, order11, order12);

                // --- Trades (matched order pairs) ---
                dbContext.Trades.AddRange(
                    new FinancialSystem.Domain.Entities.Trade
                    {
                        BuyOrder = order1, SellOrder = order2,
                        TradePrice = 100.475m, TradeQuantity = 5000,
                        Status = FinancialSystem.Domain.Entities.SettlementStatus.Settled,
                        TradeDate = now.AddDays(-5).AddHours(-2)
                    },
                    new FinancialSystem.Domain.Entities.Trade
                    {
                        BuyOrder = order3, SellOrder = order4,
                        TradePrice = 98.175m, TradeQuantity = 2500,
                        Status = FinancialSystem.Domain.Entities.SettlementStatus.Settled,
                        TradeDate = now.AddDays(-4)
                    },
                    new FinancialSystem.Domain.Entities.Trade
                    {
                        BuyOrder = order6, SellOrder = order7,
                        TradePrice = 45.19m, TradeQuantity = 3000,
                        Status = FinancialSystem.Domain.Entities.SettlementStatus.Pending,
                        TradeDate = now.AddDays(-3).AddHours(-5)
                    },
                    new FinancialSystem.Domain.Entities.Trade
                    {
                        BuyOrder = order8, SellOrder = order9,
                        TradePrice = 189.725m, TradeQuantity = 800,
                        Status = FinancialSystem.Domain.Entities.SettlementStatus.Pending,
                        TradeDate = now.AddDays(-2).AddHours(-3)
                    }
                );

                dbContext.SaveChanges();
            }

            // --- Fixed Deposits & Instruments Seeding ---
            if (!dbContext.FixedDeposits.Any())
            {
                var now = System.DateTime.Now;

                var fdInst1 = new FinancialSystem.Domain.Entities.FixedDepositInstrument { Name = "HDFC Bank Term Deposit 1Y", Isin = "FD-HDFC-2026-001" };
                var fdInst2 = new FinancialSystem.Domain.Entities.FixedDepositInstrument { Name = "SBI Amrit Kalash 400D", Isin = "FD-SBI-2026-002" };
                var fdInst3 = new FinancialSystem.Domain.Entities.FixedDepositInstrument { Name = "ICICI Golden Years FD", Isin = "FD-ICICI-2026-003" };
                var fdInst4 = new FinancialSystem.Domain.Entities.FixedDepositInstrument { Name = "Kotak Mahindra Active FD", Isin = "FD-KOTAK-2026-004" };

                var fd1 = new FinancialSystem.Domain.Entities.FixedDeposit
                {
                    FdName = fdInst1.Name,
                    FdNumber = fdInst1.Isin,
                    DepositDate = now.AddDays(-15),
                    MaturityDate = now.AddDays(350),
                    NumberOfDays = 365,
                    Principal = 500000m,
                    InterestRate = 7.25m,
                    InterestAmount = 36250m,
                    InterestFrequency = FinancialSystem.Domain.Entities.InterestFrequency.Quarterly,
                    Tds = 3625m,
                    CreatedAt = now.AddDays(-15),
                    UpdatedAt = now.AddDays(-15)
                };

                var fd2 = new FinancialSystem.Domain.Entities.FixedDeposit
                {
                    FdName = fdInst2.Name,
                    FdNumber = fdInst2.Isin,
                    DepositDate = now.AddDays(-10),
                    MaturityDate = now.AddDays(390),
                    NumberOfDays = 400,
                    Principal = 1000000m,
                    InterestRate = 7.60m,
                    InterestAmount = 76000m,
                    InterestFrequency = FinancialSystem.Domain.Entities.InterestFrequency.AtMaturity,
                    Tds = 7600m,
                    CreatedAt = now.AddDays(-10),
                    UpdatedAt = now.AddDays(-10)
                };

                var fd3 = new FinancialSystem.Domain.Entities.FixedDeposit
                {
                    FdName = fdInst3.Name,
                    FdNumber = fdInst3.Isin,
                    DepositDate = now.AddDays(-5),
                    MaturityDate = now.AddDays(725),
                    NumberOfDays = 730,
                    Principal = 250000m,
                    InterestRate = 7.10m,
                    InterestAmount = 35500m,
                    InterestFrequency = FinancialSystem.Domain.Entities.InterestFrequency.HalfYearly,
                    Tds = 3550m,
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now.AddDays(-5)
                };

                var fd4 = new FinancialSystem.Domain.Entities.FixedDeposit
                {
                    FdName = fdInst4.Name,
                    FdNumber = fdInst4.Isin,
                    DepositDate = now.AddDays(-2),
                    MaturityDate = now.AddDays(180),
                    NumberOfDays = 182,
                    Principal = 750000m,
                    InterestRate = 6.90m,
                    InterestAmount = 25875m,
                    InterestFrequency = FinancialSystem.Domain.Entities.InterestFrequency.Monthly,
                    Tds = 2587.50m,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-2)
                };

                dbContext.FixedDeposits.AddRange(fd1, fd2, fd3, fd4);

                var fdOrder1 = new FinancialSystem.Domain.Entities.Order
                {
                    Instrument = fdInst1,
                    Quantity = 1,
                    Price = 500000m,
                    Status = FinancialSystem.Domain.Entities.OrderStatus.Matched,
                    CreatedAt = now.AddDays(-5),
                    Isin = fdInst1.Isin,
                    FdMaturityDate = fd1.MaturityDate,
                    FdMaturityAmount = fd1.Principal,
                    FdRoi = fd1.InterestRate
                };
                var fdOrder2 = new FinancialSystem.Domain.Entities.Order
                {
                    Instrument = fdInst1,
                    Quantity = 1,
                    Price = 500000m,
                    Status = FinancialSystem.Domain.Entities.OrderStatus.Matched,
                    CreatedAt = now.AddDays(-5),
                    Isin = fdInst1.Isin,
                    FdMaturityDate = fd1.MaturityDate,
                    FdMaturityAmount = fd1.Principal,
                    FdRoi = fd1.InterestRate
                };
                var fdOrder3 = new FinancialSystem.Domain.Entities.Order
                {
                    Instrument = fdInst2,
                    Quantity = 1,
                    Price = 1000000m,
                    Status = FinancialSystem.Domain.Entities.OrderStatus.Pending,
                    CreatedAt = now.AddDays(-1),
                    Isin = fdInst2.Isin,
                    FdMaturityDate = fd2.MaturityDate,
                    FdMaturityAmount = fd2.Principal,
                    FdRoi = fd2.InterestRate
                };

                dbContext.Orders.AddRange(fdOrder1, fdOrder2, fdOrder3);

                dbContext.Trades.Add(new FinancialSystem.Domain.Entities.Trade
                {
                    BuyOrder = fdOrder1,
                    SellOrder = fdOrder2,
                    TradePrice = 500000m,
                    TradeQuantity = 1,
                    Status = FinancialSystem.Domain.Entities.SettlementStatus.Settled,
                    TradeDate = now.AddDays(-2).AddHours(-1)
                });

                dbContext.SaveChanges();
            }

            // Ensure full instrument catalog exists
            if (!dbContext.Instruments.OfType<FinancialSystem.Domain.Entities.GSec>().Any(g => g.Name.Contains("6.79%")))
            {
                dbContext.Instruments.AddRange(
                    new FinancialSystem.Domain.Entities.GSec { Name = "7.38% GOI 2027", Isin = "IN0020210303", CouponRate = 7.38m },
                    new FinancialSystem.Domain.Entities.GSec { Name = "6.79% GOI 2034", Isin = "IN0020210505", CouponRate = 6.79m }
                );
                dbContext.SaveChanges();
            }

            if (!dbContext.Instruments.OfType<FinancialSystem.Domain.Entities.MutualFund>().Any(m => m.Name.Contains("Kotak")))
            {
                dbContext.Instruments.AddRange(
                    new FinancialSystem.Domain.Entities.MutualFund { Name = "Kotak Flexi Cap",     Isin = "INF174K01LS2", Nav = 63.45m },
                    new FinancialSystem.Domain.Entities.MutualFund { Name = "ICICI Pru Value",     Isin = "INF109K012R5", Nav = 45.20m },
                    new FinancialSystem.Domain.Entities.MutualFund { Name = "Nippon India Growth", Isin = "INF204K01FH3", Nav = 185.30m }
                );
                dbContext.SaveChanges();
            }

            // Seed demo users for 2FA login with user-wise business dates
            if (!dbContext.Users.Any())
            {
                var today = DateTime.Today;
                dbContext.Users.AddRange(
                    new FinancialSystem.Domain.Entities.AppUser
                    {
                        Username = "admin",
                        Password = "password123",
                        FullName = "Supriya (Administrator)",
                        Email = "admin@financialos.com",
                        PhoneNumber = "+91 98765 43210",
                        Role = "Administrator",
                        BusinessDate = today, // Current Date
                        PreferredOtpChannel = "Mobile",
                        IsTwoFactorEnabled = true
                    },
                    new FinancialSystem.Domain.Entities.AppUser
                    {
                        Username = "trader1",
                        Password = "password123",
                        FullName = "Rahul Sharma (Chief Trader)",
                        Email = "rahul.trader@financialos.com",
                        PhoneNumber = "+91 98220 12345",
                        Role = "Senior Trader",
                        BusinessDate = today.AddDays(-1), // Backdated desk
                        PreferredOtpChannel = "Email",
                        IsTwoFactorEnabled = true
                    },
                    new FinancialSystem.Domain.Entities.AppUser
                    {
                        Username = "settlement",
                        Password = "password123",
                        FullName = "Priya Nair (Settlement Officer)",
                        Email = "priya.settlement@financialos.com",
                        PhoneNumber = "+91 99887 76655",
                        Role = "Settlement Manager",
                        BusinessDate = today.AddDays(1), // Forward value date desk
                        PreferredOtpChannel = "Mobile",
                        IsTwoFactorEnabled = true
                    }
                );
                dbContext.SaveChanges();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}

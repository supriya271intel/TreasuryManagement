using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Entities.Portfolio;

namespace FinancialSystem.WebUI.Controllers
{
    [Authorize]
    public class PortfolioController : Controller
    {
        private readonly IUnitOfWork _uow;

        public PortfolioController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _uow.Orders.GetAllAsync();

            // Build the Composite tree using the Composite Pattern
            var root = new AssetPortfolio { PortfolioName = "Total Portfolio" };

            var fixedIncome = new AssetPortfolio { PortfolioName = "Fixed Income" };
            var equities = new AssetPortfolio { PortfolioName = "Equities & Funds" };

            foreach (var order in orders)
            {
                var asset = new SingleAsset
                {
                    Name = order.Instrument?.Name ?? "Unknown",
                    Isin = order.Instrument?.Isin ?? "-",
                    InstrumentType = order.Instrument?.GetType().Name ?? "Unknown",
                    Value = order.Price,
                    Quantity = order.Quantity
                };

                if (order.Instrument is GSec || order.Instrument is FixedDepositInstrument)
                    fixedIncome.Add(asset);
                else
                    equities.Add(asset);
            }

            if (fixedIncome.Children.Count > 0) root.Add(fixedIncome);
            if (equities.Children.Count > 0) root.Add(equities);

            return View(root);
        }
    }
}

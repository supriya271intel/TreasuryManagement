using System.Collections.Generic;
using System.Linq;

namespace FinancialSystem.Domain.Entities.Portfolio
{

// Composite Pattern Implementation
public interface IAssetComponent
{
    string Name { get; }
    decimal CalculateValue();
}

public class SingleAsset : IAssetComponent
{
    public string Name { get; set; }
    public string Isin { get; set; }
    public string InstrumentType { get; set; }
    public decimal Value { get; set; }
    public int Quantity { get; set; }

    public decimal CalculateValue() => Value * Quantity;
}

public class AssetPortfolio : IAssetComponent
{
    public string Name => PortfolioName;
    public string PortfolioName { get; set; }
    private readonly List<IAssetComponent> _children = new List<IAssetComponent>();
    public IReadOnlyList<IAssetComponent> Children => _children;

    public void Add(IAssetComponent component) => _children.Add(component);

    public decimal CalculateValue()
    {
        return _children.Sum(child => child.CalculateValue());
    }
}

}

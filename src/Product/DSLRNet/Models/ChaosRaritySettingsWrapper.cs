namespace DSLRNet.Models;
using DSLRNet.Core.Config;

public class ChaosRaritySettingsWrapper : BaseModel<ChaosRaritySettings>
{
    public ChaosRaritySettingsWrapper(ChaosRaritySettings settings)
    {
        OriginalObject = settings;
        Early = new(settings.Early);
        Mid = new(settings.Mid);
        Late = new(settings.Late);
        End = new(settings.End);
    }

    public ChaosRarityChancesWrapper Early { get; }

    public ChaosRarityChancesWrapper Mid { get; }

    public ChaosRarityChancesWrapper Late { get; }

    public ChaosRarityChancesWrapper End { get; }
}

public class ChaosRarityChancesWrapper : BaseModel<ChaosRarityChances>
{
    private readonly ChaosRarityChances _chances;

    public ChaosRarityChancesWrapper(ChaosRarityChances chances)
    {
        _chances = chances;
        OriginalObject = _chances;
    }

    public double Common
    {
        get => _chances.Common;
        set => SetChance(RarityTier.Common, value, nameof(Common));
    }

    public double Uncommon
    {
        get => _chances.Uncommon;
        set => SetChance(RarityTier.Uncommon, value, nameof(Uncommon));
    }

    public double Rare
    {
        get => _chances.Rare;
        set => SetChance(RarityTier.Rare, value, nameof(Rare));
    }

    public double Mythical
    {
        get => _chances.Mythical;
        set => SetChance(RarityTier.Mythical, value, nameof(Mythical));
    }

    public double Legendary
    {
        get => _chances.Legendary;
        set => SetChance(RarityTier.Legendary, value, nameof(Legendary));
    }

    public double Total => Math.Round(_chances.Total, 2);

    private void SetChance(RarityTier tier, double value, string propertyName)
    {
        float newValue = (float)Math.Round(value, 2);

        if (_chances.Get(tier) != newValue)
        {
            _chances.Set(tier, newValue);
            OnPropertyChanged(propertyName);
            OnPropertyChanged(nameof(Total));
        }
    }
}

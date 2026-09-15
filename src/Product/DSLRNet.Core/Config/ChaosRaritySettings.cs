namespace DSLRNet.Core.Config;

using DSLRNet.Core.DAL;
using IniParser.Model;

public enum RarityTier { Common, Uncommon, Rare, Mythical, Legendary }

public class ChaosRarityChances
{
    public float Common { get; set; }

    public float Uncommon { get; set; }

    public float Rare { get; set; }

    public float Mythical { get; set; }

    public float Legendary { get; set; }

    public float Total => Common + Uncommon + Rare + Mythical + Legendary;

    public float Get(RarityTier tier)
    {
        return tier switch
        {
            RarityTier.Common => Common,
            RarityTier.Uncommon => Uncommon,
            RarityTier.Rare => Rare,
            RarityTier.Mythical => Mythical,
            _ => Legendary
        };
    }

    public void Set(RarityTier tier, float value)
    {
        switch (tier)
        {
            case RarityTier.Common: Common = value; break;
            case RarityTier.Uncommon: Uncommon = value; break;
            case RarityTier.Rare: Rare = value; break;
            case RarityTier.Mythical: Mythical = value; break;
            default: Legendary = value; break;
        }
    }
}

/// <summary>
/// Chance in percent of each rarity tier dropping when chaos loot is enabled, per game stage.
/// Every rarity can drop anywhere, but rare tiers stay rare.
/// </summary>
public class ChaosRaritySettings
{
    private const string SectionBase = "Settings.ItemLotGeneratorSettings.ChaosRarityChances";

    public ChaosRarityChances Early { get; set; } = new() { Common = 35f, Uncommon = 51f, Rare = 13.3f, Mythical = 0.5f, Legendary = 0.2f };

    public ChaosRarityChances Mid { get; set; } = new() { Common = 20f, Uncommon = 47f, Rare = 30.5f, Mythical = 2f, Legendary = 0.5f };

    public ChaosRarityChances Late { get; set; } = new() { Common = 12f, Uncommon = 38f, Rare = 43f, Mythical = 5f, Legendary = 2f };

    public ChaosRarityChances End { get; set; } = new() { Common = 8f, Uncommon = 32f, Rare = 47f, Mythical = 10f, Legendary = 3f };

    // tiers are keyed by rarity id rather than name so renamed rarity setups (BrainRotEdition) keep working
    public Dictionary<RarityTier, List<int>> TierRarityIds { get; set; } = new()
    {
        { RarityTier.Common, [0] },
        { RarityTier.Uncommon, [1, 2, 3] },
        { RarityTier.Rare, [4, 5, 6] },
        { RarityTier.Mythical, [7, 8, 9] },
        { RarityTier.Legendary, [10] }
    };

    public ChaosRarityChances Get(GameStage stage)
    {
        return stage switch
        {
            GameStage.Early => Early,
            GameStage.Mid => Mid,
            GameStage.Late => Late,
            _ => End
        };
    }

    public void Initialize(IniData data)
    {
        foreach (GameStage stage in Enum.GetValues<GameStage>())
        {
            string section = $"{SectionBase}.{stage}";
            if (!data.Sections.ContainsSection(section))
            {
                continue;
            }

            ChaosRarityChances chances = Get(stage);

            foreach (RarityTier tier in Enum.GetValues<RarityTier>())
            {
                string key = tier.ToString();

                // accept both 4.5 and 4,5 for hand edited files, without treating the comma as a thousands separator
                if (data[section].ContainsKey(key)
                    && float.TryParse(data[section][key].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                {
                    chances.Set(tier, Math.Max(0f, value));
                }
            }
        }
    }

    public void WriteTo(IniData data)
    {
        foreach (GameStage stage in Enum.GetValues<GameStage>())
        {
            ChaosRarityChances chances = Get(stage);

            foreach (RarityTier tier in Enum.GetValues<RarityTier>())
            {
                data[$"{SectionBase}.{stage}"][tier.ToString()] = chances.Get(tier).ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}

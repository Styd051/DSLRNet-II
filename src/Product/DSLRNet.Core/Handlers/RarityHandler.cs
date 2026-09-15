namespace DSLRNet.Core.Handlers;

using DSLRNet.Core.DAL;
using DSLRNet.Core.Extensions;

public class RarityHandler(
    RandomProvider randomProvider,
    ParamEditsRepository dataRepository,
    DataAccess dataAccess,
    IOptions<Settings> settingsOptions) : BaseHandler(dataRepository)
{
    private readonly RandomProvider randomProvider = randomProvider;
    private RarityIconMappingConfig iconMappingConfig = new();
    private Settings settings = settingsOptions.Value;
    private readonly Dictionary<int, RaritySetup> RarityConfigs = dataAccess.RaritySetup.GetAll().ToDictionary(d => d.ID);

    public Dictionary<int, int> CountByRarity { get; set; } = [];

    public int ChooseRarityFromIdSet(IntValueRange range, GameStage gameStage)
    {
        int finalId;

        if (this.settings.ItemLotGeneratorSettings.ChaosLootEnabled)
        {
            finalId = this.ChooseChaosRarity(gameStage);
        }
        else
        {
            List<WeightedValue<int>> weightedValues = [];

            foreach (int x in range.ToRangeOfValues())
            {
                weightedValues.Add(new WeightedValue<int> { Value = this.GetNearestRarity(x).ID, Weight = this.RarityConfigs[x].SelectionWeight });
            }

            finalId = this.randomProvider.NextWeightedValue(weightedValues);
        }

        if (!CountByRarity.TryGetValue(finalId, out int _))
        {
            CountByRarity[finalId] = 0;
        }

        CountByRarity[finalId]++;

        return finalId;
    }

    private int ChooseChaosRarity(GameStage gameStage)
    {
        ChaosRaritySettings chaosSettings = this.settings.ItemLotGeneratorSettings.ChaosRarityChances;
        ChaosRarityChances chances = chaosSettings.Get(gameStage);

        // percentages can be fractional (0.5%), scale them up to integer weights
        List<WeightedValue<RarityTier>> tierWeights = Enum.GetValues<RarityTier>()
            .Where(tier => chaosSettings.TierRarityIds[tier].Any(this.RarityConfigs.ContainsKey))
            .Select(tier => new WeightedValue<RarityTier> { Value = tier, Weight = (int)Math.Round(chances.Get(tier) * 100) })
            .ToList();

        if (tierWeights.Sum(d => d.Weight) <= 0)
        {
            return this.randomProvider.GetRandomItem(this.RarityConfigs.Keys);
        }

        RarityTier chosenTier = this.randomProvider.NextWeightedValue(tierWeights);

        List<WeightedValue<int>> rarityWeights = chaosSettings.TierRarityIds[chosenTier]
            .Where(this.RarityConfigs.ContainsKey)
            .Select(id => new WeightedValue<int> { Value = id, Weight = this.RarityConfigs[id].SelectionWeight })
            .ToList();

        return this.randomProvider.NextWeightedValue(rarityWeights);
    }

    public List<bool> GetRarityEffectChances(int rarityId, float chanceMultiplier)
    {
        List<bool> chanceEvaluations = [];

        RaritySetup rarity = this.GetNearestRarity(rarityId);

        chanceEvaluations.Add(this.randomProvider.PassesPercentCheck(rarity.SpEffectChance0 * chanceMultiplier));
        chanceEvaluations.Add(this.randomProvider.PassesPercentCheck(rarity.SpEffectChance1 * chanceMultiplier));
        chanceEvaluations.Add(this.randomProvider.PassesPercentCheck(rarity.SpEffectChance2 * chanceMultiplier));
        chanceEvaluations.Add(this.randomProvider.PassesPercentCheck(rarity.SpEffectChance3 * chanceMultiplier));

        return chanceEvaluations;
    }

    public IntValueRange GetStatRequiredAdditionRange(int rarityId)
    {
        RaritySetup rarity = this.GetNearestRarity(rarityId);
        return new IntValueRange(rarity.StatReqAddMin, rarity.StatReqAddMax);
    }

    public IntValueRange GetDamageAdditionRange(int rarityId)
    {
        RaritySetup rarity = this.GetNearestRarity(rarityId);
        return new IntValueRange(rarity.WeaponDmgAddMin, rarity.WeaponDmgAddMax);
    }

    public IntValueRange GetWeaponScalingRange(int rarityId)
    {
        RaritySetup rarity = this.GetNearestRarity(rarityId);
        return new IntValueRange(rarity.ScalingMin, rarity.ScalingMax);
    }

    public FloatValueRange GetShieldGuardRateRange(int rarityId)
    {
        RaritySetup rarity = this.GetNearestRarity(rarityId);
        return new FloatValueRange(rarity.ShieldGuardRateMultMin, rarity.ShieldGuardRateMultMax);
    }

    public float GetArmorCutRateAddition(int rarityId)
    {
        RaritySetup rarity = this.GetNearestRarity(rarityId);
        FloatValueRange range = new(rarity.ArmorCutRateAddMin, rarity.ArmorCutRateAddMax);
        return (float)Math.Round(this.randomProvider.Next(range), 4);
    }

    public float GetArmorResistMultiplier(int rarityid)
    {
        RaritySetup rarity = this.GetNearestRarity(rarityid);
        FloatValueRange range = new(rarity.ArmorResistMinMult, rarity.ArmorResistMaxMult);

        return (float)this.randomProvider.Next(range);
    }

    public byte GetRarityParamValue(int rarityId)
    {
        return (byte)this.GetNearestRarity(rarityId).RarityParamValue;
    }

    public IntValueRange GetSpeffectPowerRange(int rarityId)
    {
        RaritySetup rarity = this.GetNearestRarity(rarityId);

        return new IntValueRange(
            Math.Clamp(rarity.SpEffectPowerMin - 10, 0, rarity.SpEffectPowerMax),
            rarity.SpEffectPowerMax);
    }

    public string GetRarityName(int rarityId, bool withColor)
    {
        RaritySetup rarity = this.GetNearestRarity(rarityId);

        if (withColor)
        {
            return rarity.Name.WrapTextWithProperties(color: rarity.ColorHex);
        }

        return rarity.Name;
    }

    public int GetSellValue(int rarityid)
    {
        RaritySetup rarity = this.GetNearestRarity(rarityid);
        return this.randomProvider.NextInt(rarity.SellValueMin, rarity.SellValueMax);
    }

    public float GetRandomizedWeight(float originalWeight, int rarityId)
    {
        RaritySetup rarity = this.GetNearestRarity(rarityId);
        return originalWeight * (float)this.randomProvider.Next(rarity.WeightMultMin, rarity.WeightMultMax);
    }

    public ushort GetIconId(ushort iconId, int rarityId, bool isUnique = false)
    {
        if (rarityId == 0)
        {
            return iconId;
        }

        if (isUnique)
        {
            rarityId = -1;
        }

        RarityIconMapping? options = this.iconMappingConfig.IconSheets.Where(d => d.IconMappings.RarityIds.Contains(rarityId)).Select(s => s.IconMappings)
            .Where(d => d.IconReplacements.Any(d => d.OriginalIconId == iconId)).FirstOrDefault();

        return options?.IconReplacements.FirstOrDefault(s => s.OriginalIconId == iconId)?.NewIconId ?? iconId;
    }

    public void UpdateIconMapping(RarityIconMappingConfig config)
    {
        this.iconMappingConfig = config;
    }

    private RaritySetup GetNearestRarity(int rarityId = 0)
    {
        if (rarityId == -1)
        {
            return RarityConfigs[this.randomProvider.GetRandomItem(this.RarityConfigs.Keys.ToList())];
        }

        int nearestRarity = rarityId;

        if (!this.RarityConfigs.ContainsKey(rarityId))
        {
            foreach (int x in this.RarityConfigs.Keys)
            {
                if (rarityId <= x)
                {
                    nearestRarity = x;
                }
            }
        }
        return RarityConfigs[nearestRarity];
    }
}

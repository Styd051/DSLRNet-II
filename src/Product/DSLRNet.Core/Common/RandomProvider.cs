namespace DSLRNet.Core.Common;

public class RandomProvider(int seed)
{
    private readonly Random random = new(seed);

    public int NextInt(IntValueRange range)
    {
        return this.NextInt(range.Min, range.Max);
    }

    public int NextInt(int minimum, int maximum)
    {
        return this.random.Next(minimum, maximum + 1);
    }

    public float Next(FloatValueRange range)
    {
        return (float)this.Next(range.Min, range.Max);
    }

    public double Next(double minimum = 0, double maximum = 1)
    {
        double value = this.random.NextDouble();

        return value * (maximum - minimum) + minimum;
    }

    public T NextWeightedValue<T>(List<WeightedValue<T>> values)
    {
        int weightTotal = values.Sum(d => d.Weight);

        if (weightTotal <= 0)
        {
            return values.First().Value;
        }

        // roll in [1, total] so each value is picked with exactly its weight and zero weights are never picked
        int weightedResult = this.NextInt(1, weightTotal);

        foreach (WeightedValue<T> val in values)
        {
            if (weightedResult <= val.Weight)
            {
                return val.Value;
            }
            else
            {
                weightedResult -= val.Weight;
            }
        }

        return values.First().Value;
    }

    public T GetRandomItem<T>(IEnumerable<T> values)
    {
        int itemNumber = this.NextInt(0, values.Count() - 1);

        return values.ElementAt(itemNumber);
    }

    public List<T> GetRandomItems<T>(IEnumerable<T> values, int count)
    {
        return GetRandomizedList(values).Take(count).ToList();
    }

    public List<T> GetRandomizedList<T>(IEnumerable<T> source)
    {
        return [.. source.OrderBy(d => this.NextInt(0, 1000))];
    }

    public bool PassesPercentCheck(int percent)
    {
        if (percent < 100)
        {
            return this.NextInt(0, 99) < percent;
        }
        else
        {
            return true;
        }
    }

    public bool PassesPercentCheck(double percent)
    {
        if (percent < 1.0)
        {
            return this.Next() < percent;
        }
        else
        {
            return true;
        }
    }
}
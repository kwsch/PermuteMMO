namespace PermuteMMO.Lib;

public static class PermuteDump
{
    public static IEnumerable<string> Dump(PermuteMeta meta)
    {
        var results = meta.Results;
        var groups = results.GroupBy(z => z.Advances.Length);
        foreach (var g in groups)
        {
            var step = g.Key;
            var entities = g.ToArray();
            var first = entities[0];
            var adv = step == 0 ? Advance.RG : first.Advances[step - 1];

            foreach (var line in GetLines(step, adv, entities))
                yield return line;
        }
    }

    private static IEnumerable<string> GetLines(int step, Advance adv, PermuteResult[] entities)
    {
        yield return "===================";
        yield return $"Step {step}: {adv}";
        yield return string.Join('|', entities[0].Advances);
        foreach (var entity in entities)
        {
            foreach (var line in entity.Entity.GetLines())
                yield return line;
            yield return "";
        }
        yield return "";
    }
}
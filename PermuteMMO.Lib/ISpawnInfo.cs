namespace PermuteMMO.Lib;

// I wanted to have savedata spawners inherit this, but can't on ref structs. oh well!
public interface ISpawnInfo
{
    int BaseCount { get; }
    int BonusCount { get; }
    ulong BaseTable { get; }
    ulong BonusTable { get; }
    bool HasBonus => BonusTable is not (0 or 0xCBF29CE484222645);
}

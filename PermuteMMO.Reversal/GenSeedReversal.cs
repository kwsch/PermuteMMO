using Microsoft.Z3;
using PKHeX.Core;

namespace PermuteMMO.Reversal;

public static class GenSeedReversal
{
    public static IList<ulong> GetAllGenSeeds(ulong seed)
    {
        var seeds = FindPotentialGenSeeds(seed);

        var result = new List<ulong>();
        foreach (var _seed in seeds)
            result.Add(_seed);
        return result;
    }

    public static IEnumerable<ulong> FindPotentialGenSeeds(ulong seed)
    {
        using var ctx = new Context(new Dictionary<string, string> { { "model", "true" } });
        var exp = CreateGenSeedModel(ctx, seed, out var s0);

        while (Check(ctx, exp) is { } x)
        {
            foreach (var kvp in x.Consts)
            {
                var tmp = (BitVecNum)kvp.Value;
                yield return tmp.UInt64;
                exp = ctx.MkAnd(exp, ctx.MkNot(ctx.MkEq(s0, x.Evaluate(s0))));
            }
        }
    }

    public static IList<ulong> GetAllGroupSeeds(ulong seed1, ulong seed2, ulong seed3, ulong seed4)
    {
        var seeds = FindPotentialGroupSeeds(seed1, seed2, seed3, seed4);

        var result = new List<ulong>();
        foreach (var _seed in seeds)
            result.Add(_seed);
        return result;
    }

    public static IEnumerable<ulong> FindPotentialGroupSeeds(ulong seed1, ulong seed2, ulong seed3, ulong seed4)
    {
        using var ctx = new Context(new Dictionary<string, string> { { "model", "true" } });

        var exp = CreateGroupSeedModel(ctx, seed1, seed2, seed3, seed4, out var s0);

        while (Check(ctx, exp) is { } m)
        {
            foreach (var kvp in m.Consts)
            {
                var tmp = (BitVecNum)kvp.Value;
                yield return tmp.UInt64;
                exp = ctx.MkAnd(exp, ctx.MkNot(ctx.MkEq(s0, m.Evaluate(s0))));
            }
        }
    }

    private static BoolExpr CreateGenSeedModel(Context ctx, ulong seed, out BitVecExpr s0)
    {
        s0 = ctx.MkBVConst("s0", 64);
        BitVecExpr s1 = ctx.MkBV(Xoroshiro128Plus.XOROSHIRO_CONST, 64);

        var real_seed = ctx.MkBV(seed, 64);

        AdvanceSymbolicNext(ctx, ref s0, ref s1); // slot
        var genseed_check = AdvanceSymbolicNext(ctx, ref s0, ref s1); // genseed

        return ctx.MkEq(real_seed, genseed_check);
    }

    private static BitVecExpr AdvanceSymbolicNext(Context ctx, ref BitVecExpr s0, ref BitVecExpr s1)
    {
        var and_val = ctx.MkBV(0xFFFFFFFFFFFFFFFF, 64);
        var res = ctx.MkBVAND(ctx.MkBVAdd(s0, s1), and_val);
        s1 = ctx.MkBVXOR(s0, s1);
        var tmp = ctx.MkBVRotateLeft(24, s0);
        var tmp2 = ctx.MkBV(1 << 16, 64);
        s0 = ctx.MkBVXOR(tmp, ctx.MkBVXOR(s1, ctx.MkBVMul(s1, tmp2)));
        s1 = ctx.MkBVRotateLeft(37, s1);
        return res;
    }

    private static Model? Check(Context ctx, BoolExpr cond)
    {
        Solver solver = ctx.MkSolver();
        solver.Assert(cond);
        Status q = solver.Check();
        if (q != Status.SATISFIABLE)
            return null;
        return solver.Model;
    }

    private static BoolExpr CreateGroupSeedModel(Context ctx, ulong seed1, ulong seed2, ulong seed3, ulong seed4, out BitVecExpr s0)
    {
        s0 = ctx.MkBVConst("s0", 64);
        BitVecExpr s1 = ctx.MkBV(Xoroshiro128Plus.XOROSHIRO_CONST, 64);

        var real_seed1 = ctx.MkBV(seed1, 64);
        var real_seed2 = ctx.MkBV(seed2, 64);
        var real_seed3 = ctx.MkBV(seed3, 64);
        var real_seed4 = ctx.MkBV(seed4, 64);

        var genseed_check1 = AdvanceSymbolicNext(ctx, ref s0, ref s1); // genseed1
        AdvanceSymbolicNext(ctx, ref s0, ref s1); // unknown
        var genseed_check2 = AdvanceSymbolicNext(ctx, ref s0, ref s1); // genseed2
        AdvanceSymbolicNext(ctx, ref s0, ref s1); // unknown
        var genseed_check3 = AdvanceSymbolicNext(ctx, ref s0, ref s1); // genseed3
        AdvanceSymbolicNext(ctx, ref s0, ref s1); // unknown
        var genseed_check4 = AdvanceSymbolicNext(ctx, ref s0, ref s1); // genseed4
        AdvanceSymbolicNext(ctx, ref s0, ref s1); // unknown

        var exp1 = ctx.MkEq(real_seed1, genseed_check1);
        var exp2 = ctx.MkEq(real_seed2, genseed_check2);
        var exp3 = ctx.MkEq(real_seed3, genseed_check3);
        var exp4 = ctx.MkEq(real_seed4, genseed_check4);
        return ctx.MkAnd(exp1, exp2, exp3, exp4);
    }
}

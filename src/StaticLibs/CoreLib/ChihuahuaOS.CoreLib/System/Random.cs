using System.Runtime.CompilerServices;

namespace System;

public static class Random
{
    private const int STATE_SIZE = 312;
    private const ulong MIDDLE = 156;
    private const ulong INIT_SHIFT = 62;
    private const ulong TWIST_MASK = 0xb5026f5aa96619e9;
    private const ulong INIT_FACT = 6364136223846793005;
    private const int SHIFT1 = 29;
    private const ulong MASK1 = 0x5555555555555555;
    private const int SHIFT2 = 17;
    private const ulong MASK2 = 0x71d67fffeda60000;
    private const int SHIFT3 = 37;
    private const ulong MASK3 = 0xfff7eee000000000;
    private const int SHIFT4 = 43;
    private const ulong LOWER_MASK = 0x7fffffff;
    private const ulong UPPER_MASK = ~LOWER_MASK;

    private static int _nextLcg = 1;

    private static MersenneTwisterStateArray _mersenneState;
    private static int _mersenneStateIndex = STATE_SIZE + 1;

    /// <summary>
    /// Uses the Linear Congruential Generator to get a random value. Fast, but mediocre in randomness.
    /// Needs initialization using <see cref="SeedLcg"/>.
    /// </summary>
    /// <returns></returns>
    public static int NextLcg()
    {
        const int A = 1103515245;
        const int C = 12345;
        const int M = 1 << 31;

        _nextLcg = _nextLcg * A + C;
        return _nextLcg % M;
    }

    /// <summary>
    /// Uses the Linear Congruential Generator to get a random value in a range. Fast, but mediocre in randomness.
    /// Needs initialization using <see cref="SeedLcg"/>.
    /// </summary>
    /// <returns></returns>
    public static int NextLcg(int minValue, int maxValue)
    {
        const int A = 1103515245;
        const int C = 12345;

        _nextLcg = _nextLcg * A + C;

        int delta = Math.Abs(maxValue - minValue);
        return minValue + _nextLcg % delta;
    }

    public static void SeedLcg(int seed)
    {
        _nextLcg = seed;
    }


    /// <summary>
    /// Uses the Mersenne twister to get a random value. Slower, but much better in randomness. Needs initialization
    /// using <see cref="SeedMersenne"/>.
    /// </summary>
    /// <returns></returns>
    public static ulong NextMersenne()
    {
        if (_mersenneStateIndex >= STATE_SIZE)
        {
            MersenneTwist();
        }

        ulong y = _mersenneState[_mersenneStateIndex];
        y ^= (y >> SHIFT1) & MASK1;
        y ^= (y << SHIFT2) & MASK2;
        y ^= (y << SHIFT3) & MASK3;
        y ^= y >> SHIFT4;

        _mersenneStateIndex++;
        return y;
    }

    /// <summary>
    /// Uses the Mersenne twister to get a random value in range. Slower, but much better in randomness. Needs
    /// initialization using <see cref="SeedMersenne"/>.
    /// </summary>
    /// <returns></returns>
    public static ulong NextMersenne(ulong minValue, ulong maxValue)
    {
        if (_mersenneStateIndex >= STATE_SIZE)
        {
            MersenneTwist();
        }

        ulong y = _mersenneState[_mersenneStateIndex];
        y ^= (y >> SHIFT1) & MASK1;
        y ^= (y << SHIFT2) & MASK2;
        y ^= (y << SHIFT3) & MASK3;
        y ^= y >> SHIFT4;

        _mersenneStateIndex++;

        ulong delta = maxValue - minValue;
        if (maxValue < minValue)
        {
            delta = minValue - maxValue;
        }

        return minValue + y % delta;
    }

    public static void SeedMersenne(ulong seed)
    {
        _mersenneStateIndex = STATE_SIZE;
        _mersenneState[0] = seed;
        for (int i = 1; i < STATE_SIZE; i++)
        {
            _mersenneState[i] = INIT_FACT * (_mersenneState[i - 1] ^ (_mersenneState[i - 1] >> (int)INIT_SHIFT)) +
                                (ulong)i;
        }
    }

    private static void MersenneTwist()
    {
        for (int i = 0; i < STATE_SIZE; i++)
        {
            ulong x = (_mersenneState[i] & UPPER_MASK) | (_mersenneState[(i + 1) % STATE_SIZE] & LOWER_MASK);
            x = (x >> 1) ^ ((x & 1UL) != 0 ? TWIST_MASK : 0);
            _mersenneState[i] = _mersenneState[(int)((ulong)i + MIDDLE) % STATE_SIZE] ^ x;
        }

        _mersenneStateIndex = 0;
    }


    [InlineArray(STATE_SIZE)]
    private struct MersenneTwisterStateArray
    {
        private ulong _val;
    }
}

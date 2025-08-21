using System.Collections.Generic;
using static Core.Foundation.Utils.RandomUtil;

namespace Core.Foundation.Utils
{
    public sealed class RngHubStateless
    {
        private readonly ulong _slotSeed;
        private readonly Dictionary<string, ulong> _counters;

        public RngHubStateless(ulong slotSeed, Dictionary<string, ulong> counters)
        { _slotSeed = slotSeed; _counters = counters ?? new(); }

        private ulong Bump(string name)
        {
            if (!_counters.TryGetValue(name, out var c)) c = 0;
            c++; _counters[name] = c;
            return Mix3(_slotSeed, StringHash64(name), c);
        }

        public int NextInt(string name, int min, int max)
        {
            var g = new GameRandom(Bump(name));
            return g.Next(min, max);
        }

        public float NextFloat01(string name)
        {
            var g = new GameRandom(Bump(name));
            return g.NextFloat();
        }

        public bool Chance(string name, float percent)
        {
            var g = new GameRandom(Bump(name));
            return g.Chance(percent);
        }

        public int WeightedChoice(string name, float[] weights)
        {
            var g = new GameRandom(Bump(name));
            return g.WeightedChoice(weights);
        }
    }
}
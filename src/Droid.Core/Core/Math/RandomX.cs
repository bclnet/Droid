namespace Droid.Core
{
    public struct RandomX
    {
        public const int MAX_RAND = 0x7fff;

        public RandomX(int seed = 0)
            => Seed = seed;
        
        public int Seed;

        /// <summary>
        /// random integer in the range [0, MAX_RAND]
        /// </summary>
        /// <returns></returns>
        public int RandomInt()
        {
            Seed = 69069 * Seed + 1;
            return Seed & MAX_RAND;
        }
        /// <summary>
        /// random integer in the range [0, max]
        /// </summary>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        public int RandomInt(int max)
            => max == 0 ? 0 : RandomInt() % max;

        /// <summary>
        /// random number in the range [0f, 1f]
        /// </summary>
        /// <returns></returns>
        public float RandomFloat()
            => RandomInt() / (float)(MAX_RAND + 1);
        /// <summary>
        /// random number in the range [-1f, 1f]
        /// </summary>
        /// <returns></returns>
        public float CRandomFloat()
            => 2f * (RandomFloat() - 0.5f);
    }

    public struct Random2
    {
        const int MAX_RAND = 0x7fff;
        const uint IEEE_ONE = 0x3f800000;
        const uint IEEE_MASK = 0x007fffff;

        public Random2(uint seed = 0)
            => Seed = seed;

        public uint Seed;

        /// <summary>
        /// random integer in the range [0, MAX_RAND]
        /// </summary>
        /// <returns></returns>
        public int RandomInt()
        {
            Seed = 1664525 * Seed + 1013904223;
            return (int)Seed & MAX_RAND;
        }

        /// <summary>
        /// random integer in the range [0, max]
        /// </summary>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        public int RandomInt(int max)
            => max == 0 ? 0 : (RandomInt() >> (16 - MathX.BitsForInteger(max))) % max;

        /// <summary>
        /// random number in the range [0f, 1f]
        /// </summary>
        /// <returns></returns>
        public float RandomFloat()
        {
            Seed = 1664525 * Seed + 1013904223;
            return reinterpret.cast_float(IEEE_ONE | (Seed & IEEE_MASK)) - 1f;
        }

        /// <summary>
        /// random number in the range [-1f, 1f]
        /// </summary>
        /// <returns></returns>
        public float CRandomFloat()
        {
            Seed = 1664525 * Seed + 1013904223;
            return 2f * reinterpret.cast_float(IEEE_ONE | (Seed & IEEE_MASK)) - 3f;
        }
    }
}
namespace Droid
{
    public static class Platform
    {
        public const int MAX_STRING_CHARS = 1024;

        // maximum world size
        public const int MAX_WORLD_COORD = (128 * 1024);
        public const int MIN_WORLD_COORD = (-128 * 1024);
        public const int MAX_WORLD_SIZE = (MAX_WORLD_COORD - MIN_WORLD_COORD);
    }
}
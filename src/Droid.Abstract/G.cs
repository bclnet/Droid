using Droid.Framework;
using Droid.Render;
using Droid.Sound;
using Droid.UI;

namespace Droid
{
    // https://github.com/WaveEngine/OpenGL.NET
    public static class G
    {
        public static IUserInterfaceManager uiManager;
        public static ISoundSystem soundSystem;
        public static IRenderSystem renderSystem;
        public static ImageManager globalImages;     // pointer to global list for the rest of the system
        public static DeclManager declManager;
    }
}
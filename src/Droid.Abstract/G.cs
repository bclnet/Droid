using Droid.Framework;
using Droid.Render;
using Droid.Sound;
using Droid.UI;
//using GL_INDEX_TYPE = System.UInt32; // GL_UNSIGNED_INT
//using GlIndex = System.Int32;

namespace Droid
{
    // https://github.com/WaveEngine/OpenGL.NET
    public static class G
    {
        public static IUserInterfaceManager uiManager;
        public static ISoundSystem soundSystem;
        public static IRenderSystem renderSystem; // public static RenderSystemLocal tr; 
        public static ImageManager globalImages = new();     // pointer to global list for the rest of the system
        public static DeclManager declManager;
        public static VertexCacheX vertexCache = new();
    }
}
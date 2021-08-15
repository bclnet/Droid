using Gengine.Framework;
using Gengine.Render;
using Gengine.Sound;
using Gengine.UI;
//using GL_INDEX_TYPE = System.UInt32; // GL_UNSIGNED_INT
//using GlIndex = System.Int32;

namespace Gengine
{
    // https://github.com/WaveEngine/OpenGL.NET
    public static class Lib
    {
        public const string ENGINE_VERSION = "Doom3Quest 1.1.6";	// printed in console
        public const int BUILD_NUMBER = 1304;

        public static IUserInterfaceManager uiManager;
        public static ISoundSystem soundSystem;
        public static IRenderSystem renderSystem; // public static RenderSystemLocal tr; 
        public static ImageManager globalImages = new();     // pointer to global list for the rest of the system
        public static DeclManager declManager;
        public static VertexCacheX vertexCache = new();
        public static EventLoop eventLoop = new();
    }
}
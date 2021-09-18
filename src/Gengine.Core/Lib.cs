using Gengine.CM;
using Gengine.Framework;
using Gengine.Render;
using Gengine.Sound;
using Gengine.UI;
using System;
using System.Runtime.CompilerServices;
//using GL_INDEX_TYPE = System.UInt32; // GL_UNSIGNED_INT
//using GlIndex = System.Int32;
[assembly: InternalsVisibleTo("Gengine.Sound")]
[assembly: InternalsVisibleTo("Gengine.FrameworkDeclare")]

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
        public static IRenderModelManager renderModelManager;
        public static ImageManager globalImages = new();     // pointer to global list for the rest of the system
        public static DeclManager declManager;
        public static VertexCacheX vertexCache = new();
        public static ISession session;
        public static EventLoop eventLoop = new();
        public static ICollisionModelManager collisionModelManager;

        public static IGame game;
        public static IGameEdit gameEdit;

        public static string R_GetVidModeListString(bool addCustom) => throw new NotImplementedException();
        public static string R_GetVidModeValsString(bool addCustom) => throw new NotImplementedException();
    }
}
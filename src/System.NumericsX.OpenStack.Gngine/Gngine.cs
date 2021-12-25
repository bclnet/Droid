using System.NumericsX.OpenStack.Gngine.CM;
using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.Gngine.Framework.Async;
using System.NumericsX.OpenStack.Gngine.Render;
using System.NumericsX.OpenStack.Gngine.UI;
using System.Runtime.InteropServices;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine
{
    public struct GameCallbacks
    {
        public Action<object, CmdArgs> reloadImagesCB;
        public object reloadImagesUserArg;

        // called when Game DLL is unloaded (=> the registered callbacks become invalid)
        public void Reset() => throw new NotImplementedException();
    }

    public unsafe static partial class Gngine
    {
        public const string ENGINE_VERSION = "Gngine 1.0.0";	// printed in console
        public const int BUILD_NUMBER = 1000;

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
        public static INetworkSystem networkSystem;

        public static IGame game;
        public static IGameEdit gameEdit;
        public static GameCallbacks gameCallbacks;

        //: TODO-MOVE
        public static readonly IRenderSystem tr;
        public static readonly Glconfig glConfig;

        public static string R_GetVidModeListString(bool addCustom) => throw new NotImplementedException();
        public static string R_GetVidModeValsString(bool addCustom) => throw new NotImplementedException();
        public static string R_ClearCommandChain() => throw new NotImplementedException();

        //: TODO-SET
        public static Action GL_CheckErrors;
        public static Action<int> GL_SelectTexture;
        public static Action<FrameData> R_FreeDeferredTriSurfs;
    }
}
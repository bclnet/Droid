namespace Gengine.Render
{
    static unsafe partial class TR
    {
        public static readonly BackEndState backEnd = new();
        public static readonly RenderSystemLocal tr = new();
        public static readonly Glconfig glConfig;     // outside of TR since it shouldn't be cleared during ref re-init
    }
}
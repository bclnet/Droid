namespace Gengine.Render
{
    static partial class R
    {
        public static int frameCount; // = tr.frameCount;
    }

    //public struct GLstate
    //{
    //    public CT faceCulling;
    //    public int glStateBits;
    //    public bool forceGlState;      // the next GL_State will ignore glStateBits and set everything
    //    public int currentTexture;

    //    public ShaderProgram currentProgram;
    //}

    public class BackEndCounters
    {
        public int c_surfaces;
        public int c_shaders;
        public int c_vertexes;
        public int c_indexes;      // one set per pass
        public int c_totalIndexes; // counting all passes

        public int c_drawElements;
        public int c_drawIndexes;
        public int c_drawVertexes;
        public int c_drawRefIndexes;
        public int c_drawRefVertexes;

        public int c_shadowElements;
        public int c_shadowIndexes;
        public int c_shadowVertexes;

        public int c_vboIndexes;

        public int msec;           // total msec for backend run
    }

    // all state modified by the back end is separated from the front end state
    public class BackEndState
    {
        public int frameCount;     // used to track all images used in a frame
        public ViewDef viewDef;
        public BackEndCounters pc;

        //// Current states, for optimizations
        //public ViewEntity currentSpace;       // for detecting when a matrix must change
        //public ScreenRect currentScissor; // for scissor clipping, local inside renderView viewport
        //public bool currentRenderCopied;   // true if any material has already referenced _currentRender

        //// our OpenGL state deltas
        //public GLstate glState;

        public int c_copyFrameBuffer;
    }

    public class ViewDef
    {
    }
}
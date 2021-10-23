using System.Runtime.InteropServices;
using WaveEngine.Bindings.OpenGLES;
using static Gengine.Render.QGL;
using static System.NumericsX.OpenStack.OpenStack;
using static WaveEngine.Bindings.OpenGLES.GL;

namespace Gengine.Render
{
    public unsafe class FrameBuffer
    {
        const int FRAMEBUFFER_POOL_SIZE = 5;

        static uint* m_framebuffer = (uint*)Marshal.AllocHGlobal(FRAMEBUFFER_POOL_SIZE * sizeof(uint));
        static uint* m_depthbuffer = (uint*)Marshal.AllocHGlobal(FRAMEBUFFER_POOL_SIZE * sizeof(uint));

        static int m_framebuffer_width, m_framebuffer_height;
        static uint* m_framebuffer_texture = (uint*)Marshal.AllocHGlobal(FRAMEBUFFER_POOL_SIZE * sizeof(uint));

        static int drawFboId = 0;
        static int currentFramebufferIndex = 0;

        static void R_InitFrameBuffer()
        {
            m_framebuffer_width = glConfig.vidWidth;
            m_framebuffer_height = glConfig.vidHeight;

            for (var i = 0; i < FRAMEBUFFER_POOL_SIZE; ++i)
            {
                // Create texture
                glGenTextures(1, &m_framebuffer_texture[i]);
                glBindTexture(TextureTarget.Texture2d, m_framebuffer_texture[i]);

                glTexImage2D(TextureTarget.Texture2d, 0, (int)InternalFormat.Rgba, m_framebuffer_width, m_framebuffer_height, 0, PixelFormat.Rgba, GL_UNSIGNED_BYTE, null);
                glTexParameteri(TextureTarget.Texture2d, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
                glTexParameteri(TextureTarget.Texture2d, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

                // Create framebuffer
                glGenFramebuffers(1, &m_framebuffer[i]);

                // Create renderbuffer
                glGenRenderbuffers(1, &m_depthbuffer[i]);
                glBindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depthbuffer[i]);
                glRenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8Oes, m_framebuffer_width, m_framebuffer_height);
            }
        }

        static void R_FrameBufferStart()
        {
            if (currentFramebufferIndex == 0) glGetIntegerv(GetPName.GL_FRAMEBUFFER_BINDING, &drawFboId);

            // Render to framebuffer
            glBindFramebuffer(FramebufferTarget.Framebuffer, m_framebuffer[currentFramebufferIndex]);
            glBindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            glFramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, m_framebuffer_texture[currentFramebufferIndex], 0);

            // Attach combined depth+stencil
            glFramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, m_depthbuffer[currentFramebufferIndex]);
            glFramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, m_depthbuffer[currentFramebufferIndex]);

            var result = glCheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (result != FramebufferStatus.FramebufferComplete) common.Error($"Error binding Framebuffer: {result}\n");

            glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            qglClear(GL_COLOR_BUFFER_BIT);

            // Increment index in case this gets called again
            currentFramebufferIndex++;
        }

        static void R_FrameBufferEnd()
        {
            currentFramebufferIndex--;
            glBindFramebuffer(FramebufferTarget.Framebuffer, currentFramebufferIndex == 0 ? (uint)drawFboId : m_framebuffer[currentFramebufferIndex - 1]);
        }
    }
}
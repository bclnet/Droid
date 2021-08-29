using System;
using System.Collections.Generic;
using GLuint = System.Int32;
using GLenum = System.Int32;
using Gengine.Library.Core;
using Gengine.Framework;

namespace Gengine.Render
{
    public enum ImageState
    {
        IS_UNLOADED,   // no gl texture number
        IS_PARTIAL,        // has a texture number and the low mip levels loaded
        IS_LOADED      // has a texture number and the full mip hierarchy
    }

    public static partial class ImageX
    {
        const int MAX_TEXTURE_LEVELS = 14;

        // surface description flags
        const uint DDSF_CAPS = 0x00000001;
        const uint DDSF_HEIGHT = 0x00000002;
        const uint DDSF_WIDTH = 0x00000004;
        const uint DDSF_PITCH = 0x00000008;
        const uint DDSF_PIXELFORMAT = 0x00001000;
        const uint DDSF_MIPMAPCOUNT = 0x00020000;
        const uint DDSF_LINEARSIZE = 0x00080000;
        const uint DDSF_DEPTH = 0x00800000;

        // pixel format flags
        const uint DDSF_ALPHAPIXELS = 0x00000001;
        const uint DDSF_FOURCC = 0x00000004;
        const uint DDSF_RGB = 0x00000040;
        const uint DDSF_RGBA = 0x00000041;

        // our extended flags
        const uint DDSF_ID_INDEXCOLOR = 0x10000000;
        const uint DDSF_ID_MONOCHROME = 0x20000000;

        // dwCaps1 flags
        const uint DDSF_COMPLEX = 0x00000008;
        const uint DDSF_TEXTURE = 0x00001000;
        const uint DDSF_MIPMAP = 0x00400000;

        static uint DDS_MAKEFOURCC(uint a, uint b, uint c, uint d) => ((a) | ((b) << 8) | ((c) << 16) | ((d) << 24));
    }

    struct DdsFilePixelFormat
    {
        public uint dwSize;
        public uint dwFlags;
        public uint dwFourCC;
        public uint dwRGBBitCount;
        public uint dwRBitMask;
        public uint dwGBitMask;
        public uint dwBBitMask;
        public uint dwABitMask;
    }

    unsafe struct DdsFileHeader
    {
        public uint dwSize;
        public uint dwFlags;
        public uint dwHeight;
        public uint dwWidth;
        public uint dwPitchOrLinearSize;
        public uint dwDepth;
        public uint dwMipMapCount;
        public fixed uint dwReserved1[11];
        public DdsFilePixelFormat ddspf;
        public uint dwCaps1;
        public uint dwCaps2;
        public fixed uint dwReserved2[3];
    }

    //#define MAX_IMAGE_NAME	256

    public unsafe class Image
    {
        public enum TF
        {
            LINEAR,
            NEAREST,
            DEFAULT             // use the user-specified r_textureFilter
        }

        public enum TR
        {
            REPEAT,
            CLAMP,
            CLAMP_TO_BORDER,        // this should replace TR_CLAMP_TO_ZERO and TR_CLAMP_TO_ZERO_ALPHA,
                                    // but I don't want to risk changing it right now
            CLAMP_TO_ZERO,      // guarantee 0,0,0,255 edge for projected textures, set AFTER image format selection
            CLAMP_TO_ZERO_ALPHA // guarantee 0 alpha edge for projected textures, set AFTER image format selection
        }

        // increasing numeric values imply more information is stored
        public enum TD
        {
            SPECULAR,            // may be compressed, and always zeros the alpha channel
            DIFFUSE,             // may be compressed
            DEFAULT,             // will use compressed formats when possible
            BUMP,                // may be compressed with 8 bit lookup
            HIGH_QUALITY         // either 32 bit or a component format, no loss at all
        }

        public enum TT
        {
            DISABLED,
            _2D,
            CUBIC,
            RECT
        }

        public enum CF
        {
            _2D,          // not a cube map
            NATIVE,      // _px, _nx, _py, etc, directly sent to GL
            CAMERA       // _forward, _back, etc, rotated and flipped as needed before sending to GL
        }

        public Image()
        {
            texnum = TEXTURE_NOT_LOADED;
            purgePending = false;
            type = TT.DISABLED;
            frameUsed = 0;
            classification = 0;
            backgroundLoadInProgress = false;
            bgl.opcode = DLTYPE.FILE;
            bgl.f = null;
            bglNext = null;
            imgName = string.Empty;
            generatorFunction = null;
            allowDownSize = false;
            filter = TF.DEFAULT;
            repeat = TR.REPEAT;
            depth = TD.DEFAULT;
            cubeFiles = CF._2D;
            referencedOutsideLevelLoad = false;
            levelLoadReferenced = false;
            defaulted = false;
            timestamp = DateTime.MinValue;
            bindCount = 0;
            uploadWidth = uploadHeight = uploadDepth = 0;
            internalFormat = 0;
            cacheUsagePrev = cacheUsageNext = null;
            hashNext = null;
            refCount = 0;
            cinematic = null;
            cinmaticNextTime = 0;
        }

        // Makes this image active on the current GL texture unit. automatically enables or disables cube mapping
        // May perform file loading if the image was not preloaded. May start a background image read.
        public bool Bind() => throw new NotImplementedException();

        // for use with fragment programs, doesn't change any enable2D/3D/cube states
        public void BindFragment() => throw new NotImplementedException();

        // deletes the texture object, but leaves the structure so it can be reloaded
        public void PurgeImage() => throw new NotImplementedException();

        // used by callback functions to specify the actual data data goes from the bottom to the top line of the image, as OpenGL expects it
        // These perform an implicit Bind() on the current texture unit
        // FIXME: should we implement cinematics this way, instead of with explicit calls?
        public void GenerateImage(byte[] pic, int width, int height,
                            TF filter, bool allowDownSize,
                            TR repeat, TD depth) => throw new NotImplementedException();
        public void GenerateCubeImage(byte[][] pic, int size,
                                TF filter, bool allowDownSize,
                                TD depth) => throw new NotImplementedException();

        public void CopyFramebuffer(int x, int y, int width, int height, bool useOversizedBuffer) => throw new NotImplementedException();

        public void CopyDepthbuffer(int x, int y, int width, int height) => throw new NotImplementedException();

        public void UploadScratch(void* pic, int width, int height) => throw new NotImplementedException();

        // just for resource tracking
        public void SetClassification(int tag) => throw new NotImplementedException();

        // estimates size of the GL image based on dimensions and storage type
        public int StorageSize() => throw new NotImplementedException();

        // print a one line summary of the image
        public void Print() => throw new NotImplementedException();

        // check for changed timestamp on disk and reload if necessary
        public void Reload(bool force) => throw new NotImplementedException();

        public void AddReference() => refCount++;

        public bool IsLoaded() => throw new NotImplementedException();

        //==========================================================

        public void GetDownsize(out int scaled_width, out int scaled_height) => throw new NotImplementedException();
        public void MakeDefault() => throw new NotImplementedException(); // fill with a grid pattern
        public void SetImageFilterAndRepeat() => throw new NotImplementedException();
        public void ActuallyLoadImage(bool fromBind) => throw new NotImplementedException();
        public int BitsForInternalFormat(int internalFormat) => throw new NotImplementedException();
        public void UploadCompressedNormalMap(int width, int height, byte[] rgba, int mipLevel) => throw new NotImplementedException();
        public void ImageProgramStringToCompressedFileName(string imageProg, string fileName) => throw new NotImplementedException();
        public int NumLevelsForImageSize(int width, int height) => throw new NotImplementedException();

        // data commonly accessed is grouped here
        public const int TEXTURE_NOT_LOADED = -1;
        public GLuint texnum;                  // gl texture binding, will be TEXTURE_NOT_LOADED if not loaded
        public TT type;
        public int frameUsed;              // for texture usage in frame statistics
        public int bindCount;              // incremented each bind

        // background loading information
        public bool backgroundLoadInProgress;  // true if another thread is reading the complete d3t file
        public BackgroundDownload bgl;
        public Image bglNext;               // linked from tr.backgroundImageLoads

        // parameters that define this image
        public string imgName;              // game path, including extension (except for cube maps), may be an image program
        public Action<Image> generatorFunction; // NULL for files
        public bool allowDownSize;         // this also doubles as a don't-partially-load flag
        public TF filter;
        public TR repeat;
        public TD depth;
        public CF cubeFiles;              // determines the naming and flipping conventions for the six images

        public bool referencedOutsideLevelLoad;
        public bool levelLoadReferenced;   // for determining if it needs to be purged
        public bool defaulted;             // true if the default image was generated because a file couldn't be loaded
        public DateTime timestamp;                // the most recent of all images used in creation, for reloadImages command

        public int imageHash;              // for identical-image checking

        public int classification;         // just for resource profiling

        // data for listImages
        public int uploadWidth, uploadHeight, uploadDepth; // after power of two, downsample, and MAX_TEXTURE_SIZE
        public int internalFormat;

        public Image cacheUsagePrev, cacheUsageNext;    // for dynamic cache purging of old images

        public Image hashNext;              // for hash chains to speed lookup

        public int refCount;               // overall ref count

        // If bound to a cinematic
        public Cinematic cinematic;
        public int cinmaticNextTime;

        public bool purgePending = false;
    }

    partial class R
    {
        // data is RGBA
        public static void WriteTGA(string filename, byte[] data, int width, int height, bool flipVertical = false) => throw new NotImplementedException();
        // data is an 8 bit index into palette, which is RGB (no A)
        public static void WritePalTGA(string filename, byte[] data, byte[] palette, int width, int height, bool flipVertical = false) => throw new NotImplementedException();
        // data is in top-to-bottom raster order unless flipVertical is set
    }

    public class ImageManager
    {
        void Init() => throw new NotImplementedException();
        void Shutdown() => throw new NotImplementedException();

        // If the exact combination of parameters has been asked for already, an existing image will be returned, otherwise a new image will be created.
        // Be careful not to use the same image file with different filter / repeat / etc parameters if possible, because it will cause a second copy to be loaded.
        // If the load fails for any reason, the image will be filled in with the default grid pattern.
        // Will automatically resample non-power-of-two images and execute image programs if needed.
        public Image ImageFromFile(string name,
                                Image.TF filter, bool allowDownSize,
                                Image.TR repeat, Image.TD depth, Image.CF cubeMap = Image.CF._2D) => throw new NotImplementedException();

        // look for a loaded image, whatever the parameters
        public Image GetImage(string name) => throw new NotImplementedException();

        // The callback will be issued immediately, and later if images are reloaded or vid_restart
        // The callback function should call one of the idImage::Generate* functions to fill in the data
        public Image ImageFromFunction(string name, Action<Image> generatorFunction) => throw new NotImplementedException();

        // returns the number of bytes of image data bound in the previous frame
        public int SumOfUsedImages() => throw new NotImplementedException();

        // called each frame to allow some cvars to automatically force changes
        public void CheckCvars() => throw new NotImplementedException();

        // purges all the images before a vid_restart
        public void PurgeAllImages() => throw new NotImplementedException();

        // reloads all apropriate images after a vid_restart
        public void ReloadAllImages() => throw new NotImplementedException();

        // disable the active texture unit
        public void BindNull() => throw new NotImplementedException();

        // Mark all file based images as currently unused, but don't free anything.  Calls to ImageFromFile() will
        // either mark the image as used, or create a new image without loading the actual data.
        // Called only by renderSystem::BeginLevelLoad
        public void BeginLevelLoad() => throw new NotImplementedException();

        // Free all images marked as unused, and load all images that are necessary. This architecture prevents us from having the union of two level's
        // worth of data present at one time. Called only by renderSystem::EndLevelLoad
        public void EndLevelLoad() => throw new NotImplementedException();

        public void AddAllocList(Image image) => throw new NotImplementedException();
        public void AddPurgeList(Image iamge) => throw new NotImplementedException();

        public Image GetNextAllocImage() => throw new NotImplementedException();
        public Image GetNextPurgeImage() => throw new NotImplementedException();

        // used to clear and then write the dds conversion batch file
        public void StartBuild() => throw new NotImplementedException();
        public void FinishBuild(bool removeDups = false) => throw new NotImplementedException();
        public void AddDDSCommand(string cmd) => throw new NotImplementedException();

        public void PrintMemInfo(MemInfo mi) => throw new NotImplementedException();

        // cvars
        static CVar image_roundDown;          // round bad sizes down to nearest power of two
        static CVar image_colorMipLevels;     // development aid to see texture mip usage
        static CVar image_downSize;               // controls texture downsampling
        static CVar image_filter;             // changes texture filtering on mipmapped images
        static CVar image_anisotropy;         // set the maximum texture anisotropy if available
        static CVar image_writeNormalTGA;     // debug tool to write out .tgas of the final normal maps
        static CVar image_writeNormalTGAPalletized;       // debug tool to write out palletized versions of the final normal maps
        static CVar image_writeTGA;               // debug tool to write out .tgas of the non normal maps
        static CVar image_preload;                // if 0, dynamically load all images
        static CVar image_showBackgroundLoads;    // 1 = print number of outstanding background loads
        static CVar image_forceDownSize;      // allows the ability to force a downsize
        static CVar image_downSizeSpecular;       // downsize specular
        static CVar image_downSizeSpecularLimit;// downsize specular limit
        static CVar image_downSizeBump;           // downsize bump maps
        static CVar image_downSizeBumpLimit;  // downsize bump limit
        static CVar image_downSizeLimit;      // downsize diffuse limit

        // built-in images
        Image defaultImage;
        Image flatNormalMap;             // 128 128 255 in all pixels
        Image ambientNormalMap;          // tr.ambientLightVector encoded in all pixels
        Image rampImage;                 // 0-255 in RGBA in S
        Image alphaRampImage;                // 0-255 in alpha, 255 in RGB
        Image alphaNotchImage;           // 2x1 texture with just 1110 and 1111 with point sampling
        Image whiteImage;                    // full of 0xff
        Image blackImage;                    // full of 0x00
        Image normalCubeMapImage;            // cube map to normalize STR into RGB
        Image noFalloffImage;                // all 255, but zero clamped
        Image quadraticImage;                //
        Image fogImage;                  // increasing alpha is denser fog
        Image fogEnterImage;             // adjust fogImage alpha based on terminator plane
        Image cinematicImage;
        Image scratchImage;
        Image scratchImage2;
        Image accumImage;
        Image currentRenderImage;            // for SS_POST_PROCESS shaders
        Image scratchCubeMapImage;
        Image specularTableImage;            // 1D intensity texture with our specular function
        Image specular2DTableImage;      // 2D intensity texture with our specular function with variable specularity
        Image borderClampImage;          // white inside, black outside

        Image hudImage;
        Image pdaImage;

        //--------------------------------------------------------

        public Image AllocImage(string name) => throw new NotImplementedException();
        public void SetNormalPalette() => throw new NotImplementedException();
        public void ChangeTextureFilter() => throw new NotImplementedException();

        public List<Image> images = new List<Image>();
        public List<string> ddsList = new List<string>();
        //public HashIndex ddsHash;

        public List<Image> imagesAlloc = new List<Image>(); //List for the backend thread
        public List<Image> imagesPurge = new List<Image>(); //List for the backend thread

        public bool insideLevelLoad;           // don't actually load images now

        public byte[] originalToCompressed = new byte[256]; // maps normal maps to 8 bit textures
        public byte[] compressedPalette = new byte[768];        // the palette that normal maps use

        // default filter modes for images
        public GLenum textureMinFilter;
        public GLenum textureMaxFilter;
        public float textureAnisotropy;

        //public Image[] imageHashTable = new Image[FILE_HASH_SIZE];

        public Image backgroundImageLoads;      // chain of images that have background file loads active
        public Image cacheLRU;                   // head/tail of doubly linked list
        public int totalCachedImageSize;       // for determining when something should be purged

        public int numActiveBackgroundImageLoads;
        public const int MAX_BACKGROUND_IMAGE_LOADS = 8;
    }

    partial class R
    {
        public static int _MakePowerOfTwo(int num) => throw new NotImplementedException();

        #region IMAGEPROCESS
        // FIXME: make an "imageBlock" type to hold byte*,width,height?

        public static byte[] Dropsample(byte[] i, int inwidth, int inheight, int outwidth, int outheight) => throw new NotImplementedException();
        public static byte[] ResampleTexture(byte[] i, int inwidth, int inheight, int outwidth, int outheight) => throw new NotImplementedException();
        public static byte[] MipMapWithAlphaSpecularity(byte[] i, int width, int height) => throw new NotImplementedException();
        public static byte[] MipMap(byte[] i, int width, int height, bool preserveBorder) => throw new NotImplementedException();
        public static byte[] MipMap3D(byte[] i, int width, int height, int depth, bool preserveBorder) => throw new NotImplementedException();

        // these operate in-place on the provided pixels
        public static void SetBorderTexels(byte[] inBase, int width, int height, byte[] border) => throw new NotImplementedException();
        public static void SetBorderTexels3D(byte[] inBase, int width, int height, int depth, byte[] border) => throw new NotImplementedException();
        public static void BlendOverTexture(byte[] data, int pixelCount, byte[] blend) => throw new NotImplementedException();
        public static void HorizontalFlip(byte[] data, int width, int height) => throw new NotImplementedException();
        public static void VerticalFlip(byte[] data, int width, int height) => throw new NotImplementedException();
        public static void RotatePic(byte[] data, int width) => throw new NotImplementedException();

        #endregion

        #region IMAGEFILES

        public static void LoadImage(string name, out byte[] pic, out int width, out int height, out DateTime timestamp, bool makePowerOf2) => throw new NotImplementedException();
        // pic is in top to bottom raster format
        public static bool LoadCubeImages(string cname, Image.CF extensions, out byte[] pic, out int size, out DateTime timestamp) => throw new NotImplementedException();

        #endregion

        #region IMAGEPROGRAM

        public static void LoadImageProgram(string name, out byte[] pic, out int width, out int height, out DateTime timestamp, out Image.TD depth) => throw new NotImplementedException();
        public static string ParsePastImageProgram(Lexer src) => throw new NotImplementedException();

        #endregion
    }
}
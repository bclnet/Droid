using Droid.Framework;
using System;
using System.IO;
using System.Numerics;

namespace Droid.Render
{
    public struct TextureTile
    {
        public int x, y;
    }

    public class TextureLevel
    {
        public const int TILE_PER_LEVEL = 4;
        public const int MAX_MEGA_CHANNELS = 3;     // normal, diffuse, specular
        public const int MAX_LEVELS = 12;
        public const int MAX_LEVEL_WIDTH = 512;
        public const int TILE_SIZE = MAX_LEVEL_WIDTH / TILE_PER_LEVEL;

        public MegaTexture mega;

        public int tileOffset;
        public int tilesWide;
        public int tilesHigh;

        public Image image;
        public TextureTile[,] tileMap = new TextureTile[TILE_PER_LEVEL, TILE_PER_LEVEL];

        public float[] parms = new float[4];

        public void UpdateForCenter(float[] center) => throw new NotImplementedException();
        public void UpdateTile(int localX, int localY, int globalX, int globalY) => throw new NotImplementedException();
        public void Invalidate() => throw new NotImplementedException();
    }

    public struct megaTextureHeader
    {
        public int tileSize;
        public int tilesWide;
        public int tilesHigh;
    }

    public class MegaTexture
    {
        public bool InitFromMegaFile(string fileBase) => throw new NotImplementedException();
        public void SetMappingForSurface(srfTriangles[] tri) => throw new NotImplementedException();   // analyzes xyz and st to create a mapping
        public void BindForViewOrigin(Vector3 origin) => throw new NotImplementedException();  // binds images and sets program parameters
        public void Unbind() => throw new NotImplementedException();                               // removes texture bindings

        //static	void MakeMegaTexture_f( const idCmdArgs &args );
        void SetViewOrigin(Vector3 origin) => throw new NotImplementedException();
        static void GenerateMegaMipMaps(megaTextureHeader header, VFile file) => throw new NotImplementedException();
        static void GenerateMegaPreview(string fileName) => throw new NotImplementedException();

        VFile fileHandle;

        srfTriangles[] currentTriMapping;

        Vector3 currentViewOrigin;

        float[,] localViewToTextureCenter = new float[2, 4];

        int numLevels;
        TextureLevel[] levels = new TextureLevel[TextureLevel.MAX_LEVELS];                // 0 is the highest resolution
        megaTextureHeader header;

        static CVar r_megaTextureLevel;
        static CVar r_showMegaTexture;
        static CVar r_showMegaTextureLabels;
        static CVar r_skipMegaTexture;
        static CVar r_terrainScale;
    }
}
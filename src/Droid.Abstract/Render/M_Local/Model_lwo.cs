using Droid.Core;
using System;
using System.Runtime.InteropServices;

namespace Droid.Render
{
    static partial class ModelXLwo
    {
        // chunk and subchunk IDs
        public const int ID_FORM = 'F' << 24 | 'O' << 16 | 'R' << 8 | 'M';
        public const int ID_LWO2 = 'L' << 24 | 'W' << 16 | 'O' << 8 | '2';
        public const int ID_LWOB = 'L' << 24 | 'W' << 16 | 'O' << 8 | 'B';

        // top-level chunks				
        public const int ID_LAYR = 'L' << 24 | 'A' << 16 | 'Y' << 8 | 'R';
        public const int ID_TAGS = 'T' << 24 | 'A' << 16 | 'G' << 8 | 'S';
        public const int ID_PNTS = 'P' << 24 | 'N' << 16 | 'T' << 8 | 'S';
        public const int ID_BBOX = 'B' << 24 | 'B' << 16 | 'O' << 8 | 'X';
        public const int ID_VMAP = 'V' << 24 | 'M' << 16 | 'A' << 8 | 'P';
        public const int ID_VMAD = 'V' << 24 | 'M' << 16 | 'A' << 8 | 'D';
        public const int ID_POLS = 'P' << 24 | 'O' << 16 | 'L' << 8 | 'S';
        public const int ID_PTAG = 'P' << 24 | 'T' << 16 | 'A' << 8 | 'G';
        public const int ID_ENVL = 'E' << 24 | 'N' << 16 | 'V' << 8 | 'L';
        public const int ID_CLIP = 'C' << 24 | 'L' << 16 | 'I' << 8 | 'P';
        public const int ID_SURF = 'S' << 24 | 'U' << 16 | 'R' << 8 | 'F';
        public const int ID_DESC = 'D' << 24 | 'E' << 16 | 'S' << 8 | 'C';
        public const int ID_TEXT = 'T' << 24 | 'E' << 16 | 'X' << 8 | 'T';
        public const int ID_ICON = 'I' << 24 | 'C' << 16 | 'O' << 8 | 'N';

        // polygon types		          
        public const int ID_FACE = 'F' << 24 | 'A' << 16 | 'C' << 8 | 'E';
        public const int ID_CURV = 'C' << 24 | 'U' << 16 | 'R' << 8 | 'V';
        public const int ID_PTCH = 'P' << 24 | 'T' << 16 | 'C' << 8 | 'H';
        public const int ID_MBAL = 'M' << 24 | 'B' << 16 | 'A' << 8 | 'L';
        public const int ID_BONE = 'B' << 24 | 'O' << 16 | 'N' << 8 | 'E';

        // polygon tags			          
        public const int ID_SURF = 'S' << 24 | 'U' << 16 | 'R' << 8 | 'F';
        public const int ID_PART = 'P' << 24 | 'A' << 16 | 'R' << 8 | 'T';
        public const int ID_SMGP = 'S' << 24 | 'M' << 16 | 'G' << 8 | 'P';

        // envelopes			          
        public const int ID_PRE = 'P' << 24 | 'R' << 16 | 'E' << 8 | ' ';
        public const int ID_POST = 'P' << 24 | 'O' << 16 | 'S' << 8 | 'T';
        public const int ID_KEY = 'K' << 24 | 'E' << 16 | 'Y' << 8 | ' ';
        public const int ID_SPAN = 'S' << 24 | 'P' << 16 | 'A' << 8 | 'N';
        public const int ID_TCB = 'T' << 24 | 'C' << 16 | 'B' << 8 | ' ';
        public const int ID_HERM = 'H' << 24 | 'E' << 16 | 'R' << 8 | 'M';
        public const int ID_BEZI = 'B' << 24 | 'E' << 16 | 'Z' << 8 | 'I';
        public const int ID_BEZ2 = 'B' << 24 | 'E' << 16 | 'Z' << 8 | '2';
        public const int ID_LINE = 'L' << 24 | 'I' << 16 | 'N' << 8 | 'E';
        public const int ID_STEP = 'S' << 24 | 'T' << 16 | 'E' << 8 | 'P';

        // clips				          
        public const int ID_STIL = 'S' << 24 | 'T' << 16 | 'I' << 8 | 'L';
        public const int ID_ISEQ = 'I' << 24 | 'S' << 16 | 'E' << 8 | 'Q';
        public const int ID_ANIM = 'A' << 24 | 'N' << 16 | 'I' << 8 | 'M';
        public const int ID_XREF = 'X' << 24 | 'R' << 16 | 'E' << 8 | 'F';
        public const int ID_STCC = 'S' << 24 | 'T' << 16 | 'C' << 8 | 'C';
        public const int ID_TIME = 'T' << 24 | 'I' << 16 | 'M' << 8 | 'E';
        public const int ID_CONT = 'C' << 24 | 'O' << 16 | 'N' << 8 | 'T';
        public const int ID_BRIT = 'B' << 24 | 'R' << 16 | 'I' << 8 | 'T';
        public const int ID_SATR = 'S' << 24 | 'A' << 16 | 'T' << 8 | 'R';
        public const int ID_HUE = 'H' << 24 | 'U' << 16 | 'E' << 8 | ' ';
        public const int ID_GAMM = 'G' << 24 | 'A' << 16 | 'M' << 8 | 'M';
        public const int ID_NEGA = 'N' << 24 | 'E' << 16 | 'G' << 8 | 'A';
        public const int ID_IFLT = 'I' << 24 | 'F' << 16 | 'L' << 8 | 'T';
        public const int ID_PFLT = 'P' << 24 | 'F' << 16 | 'L' << 8 | 'T';

        // surfaces				          
        public const int ID_COLR = 'C' << 24 | 'O' << 16 | 'L' << 8 | 'R';
        public const int ID_LUMI = 'L' << 24 | 'U' << 16 | 'M' << 8 | 'I';
        public const int ID_DIFF = 'D' << 24 | 'I' << 16 | 'F' << 8 | 'F';
        public const int ID_SPEC = 'S' << 24 | 'P' << 16 | 'E' << 8 | 'C';
        public const int ID_GLOS = 'G' << 24 | 'L' << 16 | 'O' << 8 | 'S';
        public const int ID_REFL = 'R' << 24 | 'E' << 16 | 'F' << 8 | 'L';
        public const int ID_RFOP = 'R' << 24 | 'F' << 16 | 'O' << 8 | 'P';
        public const int ID_RIMG = 'R' << 24 | 'I' << 16 | 'M' << 8 | 'G';
        public const int ID_RSAN = 'R' << 24 | 'S' << 16 | 'A' << 8 | 'N';
        public const int ID_TRAN = 'T' << 24 | 'R' << 16 | 'A' << 8 | 'N';
        public const int ID_TROP = 'T' << 24 | 'R' << 16 | 'O' << 8 | 'P';
        public const int ID_TIMG = 'T' << 24 | 'I' << 16 | 'M' << 8 | 'G';
        public const int ID_RIND = 'R' << 24 | 'I' << 16 | 'N' << 8 | 'D';
        public const int ID_TRNL = 'T' << 24 | 'R' << 16 | 'N' << 8 | 'L';
        public const int ID_BUMP = 'B' << 24 | 'U' << 16 | 'M' << 8 | 'P';
        public const int ID_SMAN = 'S' << 24 | 'M' << 16 | 'A' << 8 | 'N';
        public const int ID_SIDE = 'S' << 24 | 'I' << 16 | 'D' << 8 | 'E';
        public const int ID_CLRH = 'C' << 24 | 'L' << 16 | 'R' << 8 | 'H';
        public const int ID_CLRF = 'C' << 24 | 'L' << 16 | 'R' << 8 | 'F';
        public const int ID_ADTR = 'A' << 24 | 'D' << 16 | 'T' << 8 | 'R';
        public const int ID_SHRP = 'S' << 24 | 'H' << 16 | 'R' << 8 | 'P';
        public const int ID_LINE = 'L' << 24 | 'I' << 16 | 'N' << 8 | 'E';
        public const int ID_LSIZ = 'L' << 24 | 'S' << 16 | 'I' << 8 | 'Z';
        public const int ID_ALPH = 'A' << 24 | 'L' << 16 | 'P' << 8 | 'H';
        public const int ID_AVAL = 'A' << 24 | 'V' << 16 | 'A' << 8 | 'L';
        public const int ID_GVAL = 'G' << 24 | 'V' << 16 | 'A' << 8 | 'L';
        public const int ID_BLOK = 'B' << 24 | 'L' << 16 | 'O' << 8 | 'K';

        // texture layer		          
        public const int ID_TYPE = 'T' << 24 | 'Y' << 16 | 'P' << 8 | 'E';
        public const int ID_CHAN = 'C' << 24 | 'H' << 16 | 'A' << 8 | 'N';
        public const int ID_NAME = 'N' << 24 | 'A' << 16 | 'M' << 8 | 'E';
        public const int ID_ENAB = 'E' << 24 | 'N' << 16 | 'A' << 8 | 'B';
        public const int ID_OPAC = 'O' << 24 | 'P' << 16 | 'A' << 8 | 'C';
        public const int ID_FLAG = 'F' << 24 | 'L' << 16 | 'A' << 8 | 'G';
        public const int ID_PROJ = 'P' << 24 | 'R' << 16 | 'O' << 8 | 'J';
        public const int ID_STCK = 'S' << 24 | 'T' << 16 | 'C' << 8 | 'K';
        public const int ID_TAMP = 'T' << 24 | 'A' << 16 | 'M' << 8 | 'P';

        // texture coordinates	          
        public const int ID_TMAP = 'T' << 24 | 'M' << 16 | 'A' << 8 | 'P';
        public const int ID_AXIS = 'A' << 24 | 'X' << 16 | 'I' << 8 | 'S';
        public const int ID_CNTR = 'C' << 24 | 'N' << 16 | 'T' << 8 | 'R';
        public const int ID_SIZE = 'S' << 24 | 'I' << 16 | 'Z' << 8 | 'E';
        public const int ID_ROTA = 'R' << 24 | 'O' << 16 | 'T' << 8 | 'A';
        public const int ID_OREF = 'O' << 24 | 'R' << 16 | 'E' << 8 | 'F';
        public const int ID_FALL = 'F' << 24 | 'A' << 16 | 'L' << 8 | 'L';
        public const int ID_CSYS = 'C' << 24 | 'S' << 16 | 'Y' << 8 | 'S';

        // image map				      
        public const int ID_IMAP = 'I' << 24 | 'M' << 16 | 'A' << 8 | 'P';
        public const int ID_IMAG = 'I' << 24 | 'M' << 16 | 'A' << 8 | 'G';
        public const int ID_WRAP = 'W' << 24 | 'R' << 16 | 'A' << 8 | 'P';
        public const int ID_WRPW = 'W' << 24 | 'R' << 16 | 'P' << 8 | 'W';
        public const int ID_WRPH = 'W' << 24 | 'R' << 16 | 'P' << 8 | 'H';
        public const int ID_VMAP = 'V' << 24 | 'M' << 16 | 'A' << 8 | 'P';
        public const int ID_AAST = 'A' << 24 | 'A' << 16 | 'S' << 8 | 'T';
        public const int ID_PIXB = 'P' << 24 | 'I' << 16 | 'X' << 8 | 'B';

        // procedural			         
        public const int ID_PROC = 'P' << 24 | 'R' << 16 | 'O' << 8 | 'C';
        public const int ID_COLR = 'C' << 24 | 'O' << 16 | 'L' << 8 | 'R';
        public const int ID_VALU = 'V' << 24 | 'A' << 16 | 'L' << 8 | 'U';
        public const int ID_FUNC = 'F' << 24 | 'U' << 16 | 'N' << 8 | 'C';
        public const int ID_FTPS = 'F' << 24 | 'T' << 16 | 'P' << 8 | 'S';
        public const int ID_ITPS = 'I' << 24 | 'T' << 16 | 'P' << 8 | 'S';
        public const int ID_ETPS = 'E' << 24 | 'T' << 16 | 'P' << 8 | 'S';

        // gradient				          
        public const int ID_GRAD = 'G' << 24 | 'R' << 16 | 'A' << 8 | 'D';
        public const int ID_GRST = 'G' << 24 | 'R' << 16 | 'S' << 8 | 'T';
        public const int ID_GREN = 'G' << 24 | 'R' << 16 | 'E' << 8 | 'N';
        public const int ID_PNAM = 'P' << 24 | 'N' << 16 | 'A' << 8 | 'M';
        public const int ID_INAM = 'I' << 24 | 'N' << 16 | 'A' << 8 | 'M';
        public const int ID_GRPT = 'G' << 24 | 'R' << 16 | 'P' << 8 | 'T';
        public const int ID_FKEY = 'F' << 24 | 'K' << 16 | 'E' << 8 | 'Y';
        public const int ID_IKEY = 'I' << 24 | 'K' << 16 | 'E' << 8 | 'Y';

        // shader				          
        public const int ID_SHDR = 'S' << 24 | 'H' << 16 | 'D' << 8 | 'R';
        public const int ID_DATA = 'D' << 24 | 'A' << 16 | 'T' << 8 | 'A';
    }

    // generic linked list
    public class st_lwNode
    {
        public st_lwNode next, prev;
        object data;
    }

    // plug-in reference
    public class st_lwPlugin
    {
        public st_lwPlugin next, prev;
        public string ord;
        public string name;
        public int flags;
        public object data;
    }

    // envelopes
    public class st_lwKey
    {
        public st_lwKey next, prev;
        public float value;
        public float time;
        public uint shape;               // ID_TCB, ID_BEZ2, etc.
        public float tension;
        public float continuity;
        public float bias;
        public float[] param = new float[4];
    }

    public class st_lwEnvelope
    {
        public st_lwEnvelope next, prev;
        public int index;
        public int type;
        public string name;
        public st_lwKey key;                 // linked list of keys
        public int nkeys;
        public int[] behavior = new int[2];       // pre and post (extrapolation)
        public st_lwPlugin cfilter;             // linked list of channel filters
        public int ncfilters;
    }

    static partial class ModelXLwo
    {
        public const int BEH_RESET = 0;
        public const int BEH_CONSTANT = 1;
        public const int BEH_REPEAT = 2;
        public const int BEH_OSCILLATE = 3;
        public const int BEH_OFFSET = 4;
        public const int BEH_LINEAR = 5;
    }

    // values that can be enveloped
    public struct st_lwEParam
    {
        public float val;
        public int eindex;
    }

    public unsafe struct st_lwVParam
    {
        public fixed float val[3];
        public int eindex;
    }


    // clips
    public struct st_lwClipStill
    {
        public string name;
    }

    public struct st_lwClipSeq
    {
        public string prefix;              // filename before sequence digits
        public string suffix;              // after digits, e.g. extensions
        public int digits;
        public int flags;
        public int offset;
        public int start;
        public int end;
    }

    public struct st_lwClipAnim
    {
        public string name;
        public string server;              // anim loader plug-in
        public object data;
    }

    public struct st_lwClipXRef
    {
        string s;
        public int index;
        public st_lwClip clip;
    }

    public struct st_lwClipCycle
    {
        public string name;
        public int lo;
        public int hi;
    }

    [StructLayout(LayoutKind.Explicit)] //: UNION
    public struct st_lwClip_source
    {
        [FieldOffset(0)] public st_lwClipStill still;
        [FieldOffset(0)] public st_lwClipSeq seq;
        [FieldOffset(0)] public st_lwClipAnim anim;
        [FieldOffset(0)] public st_lwClipXRef xref;
        [FieldOffset(0)] public st_lwClipCycle cycle;
    }
    public class st_lwClip
    {
        public st_lwClip next, prev;
        public int index;
        public uint type;                // ID_STIL, ID_ISEQ, etc.
        public st_lwClip_source source;
        public float start_time;
        public float duration;
        public float frame_rate;
        public st_lwEParam contrast;
        public st_lwEParam brightness;
        public st_lwEParam saturation;
        public st_lwEParam hue;
        public st_lwEParam gamma;
        public int negative;
        public st_lwPlugin ifilter;             // linked list of image filters
        public int nifilters;
        public st_lwPlugin pfilter;             // linked list of pixel filters
        public int npfilters;
    }

    // textures
    public struct st_lwTMap
    {
        public st_lwVParam size;
        public st_lwVParam center;
        public st_lwVParam rotate;
        public st_lwVParam falloff;
        public int fall_type;
        public string ref_object;
        public int coord_sys;
    }

    public struct st_lwImageMap
    {
        public int cindex;
        public int projection;
        public string vmap_name;
        public int axis;
        public int wrapw_type;
        public int wraph_type;
        public st_lwEParam wrapw;
        public st_lwEParam wraph;
        public float aa_strength;
        public int aas_flags;
        public int pblend;
        public st_lwEParam stck;
        public st_lwEParam amplitude;
    }

    static partial class ModelXLwo
    {
        public const int PROJ_PLANAR = 0;
        public const int PROJ_CYLINDRICAL = 1;
        public const int PROJ_SPHERICAL = 2;
        public const int PROJ_CUBIC = 3;
        public const int PROJ_FRONT = 4;

        public const int WRAP_NONE = 0;
        public const int WRAP_EDGE = 1;
        public const int WRAP_REPEAT = 2;
        public const int WRAP_MIRROR = 3;
    }

    public unsafe struct st_lwProcedural
    {
        public int axis;
        public fixed float value[3];
        public string name;
        public object data;
    }

    public class st_lwGradKey
    {
        public st_lwGradKey next, prev;
        public float value;
        public float[] rgba = new float[4];
    }

    public struct st_lwGradient
    {
        public string paramname;
        public string itemname;
        public float start;
        public float end;
        public int repeat;
        public st_lwGradKey[] key;              // array of gradient keys
        public short[] ikey;                    // array of interpolation codes
    }

    [StructLayout(LayoutKind.Explicit)] //: UNION
    public struct st_lwTexture_param
    {
        [FieldOffset(0)] public st_lwImageMap imap;
        [FieldOffset(0)] public st_lwProcedural proc;
        [FieldOffset(0)] public st_lwGradient grad;
    }
    public class st_lwTexture
    {
        public st_lwTexture next, prev;
        public string ord;
        public uint type;
        public uint chan;
        public st_lwEParam opacity;
        public short opac_type;
        public short enabled;
        public short negative;
        public short axis;
        public st_lwTexture_param param;
        public st_lwTMap tmap;
    }

    // values that can be textured

    public struct st_lwTParam
    {
        public float val;
        public int eindex;
        public st_lwTexture tex;                 // linked list of texture layers
    }

    public unsafe struct st_lwCParam
    {
        public fixed float rgb[3];
        public int eindex;
        public st_lwTexture tex;                 // linked list of texture layers
    }

    // surfaces

    public struct st_lwGlow
    {
        public short enabled;
        public short type;
        public st_lwEParam intensity;
        public st_lwEParam size;
    }

    public struct st_lwRMap
    {
        public st_lwTParam val;
        public int options;
        public int cindex;
        public float seam_angle;
    }

    public struct st_lwLine
    {
        public short enabled;
        public ushort flags;
        public st_lwEParam size;
    }

    public class st_lwSurface
    {
        public st_lwSurface next, prev;
        public string name;
        public string srcname;
        public st_lwCParam color;
        public st_lwTParam luminosity;
        public st_lwTParam diffuse;
        public st_lwTParam specularity;
        public st_lwTParam glossiness;
        public st_lwRMap reflection;
        public st_lwRMap transparency;
        public st_lwTParam eta;
        public st_lwTParam translucency;
        public st_lwTParam bump;
        public float smooth;
        public int sideflags;
        public float alpha;
        public int alpha_mode;
        public st_lwEParam color_hilite;
        public st_lwEParam color_filter;
        public st_lwEParam add_trans;
        public st_lwEParam dif_sharp;
        public st_lwEParam glow;
        public st_lwLine line;
        public st_lwPlugin shader;              // linked list of shaders
        public int nshaders;
    }

    // vertex maps

    public class st_lwVMap
    {
        public st_lwVMap next, prev;
        public string name;
        public uint type;
        public int dim;
        public int nverts;
        public int perpoly;
        public int[] vindex;              // array of point indexes 
        public int[] pindex;              // array of polygon indexes
        public float[] val;

        // added by duffy
        public int offset;
    }

    public struct st_lwVMapPt
    {
        public st_lwVMap vmap;
        public int index;               // vindex or pindex element
    }

    // points and polygons

    public unsafe struct st_lwPoint
    {
        public fixed float pos[3];
        public int npols;               // number of polygons sharing the point
        public int[] pol;                 // array of polygon indexes
        public int nvmaps;
        public st_lwVMapPt[] vm;                  // array of vmap references
    }

    public unsafe struct st_lwPolVert
    {
        public int index;               // index into the point array
        public fixed float norm[3];
        public int nvmaps;
        public st_lwVMapPt[] vm;                  // array of vmap references
    }

    public unsafe struct st_lwPolygon
    {
        public st_lwSurface surf;
        public int part;                // part index
        public int smoothgrp;           // smoothing group
        public int flags;
        public uint type;
        public fixed float norm[3];
        public int nverts;
        public st_lwPolVert[] v;                   // array of vertex records
    }

    public struct st_lwPointList
    {
        public int count;
        public int offset;              // only used during reading
        public st_lwPoint[] pt;                  // array of points
    }

    public struct st_lwPolygonList
    {
        public int count;
        public int offset;              // only used during reading
        public int vcount;              // total number of vertices
        public int voffset;             // only used during reading
        public st_lwPolygon[] pol;                 // array of polygons
    }


    // geometry layers

    public class st_lwLayer
    {
        public st_lwLayer next, prev;
        public string name;
        public int index;
        public int parent;
        public int flags;
        public float[] pivot = new float[3];
        public float[] bbox = new float[6];
        public st_lwPointList point;
        public st_lwPolygonList polygon;
        public int nvmaps;
        public st_lwVMap vmap;                // linked list of vmaps
    }


    // tag strings

    public struct st_lwTagList
    {
        public int count;
        public int offset;              // only used during reading
        public string[] tag;                 // array of strings
    }


    // an object

    public struct st_lwObject
    {
        public DateTime timeStamp;
        public st_lwLayer layer;               // linked list of layers
        public st_lwEnvelope env;                 // linked list of envelopes
        public st_lwClip clip;                // linked list of clips
        public st_lwSurface surf;                // linked list of surfaces
        public st_lwTagList taglist;
        public int nlayers;
        public int nenvs;
        public int nclips;
        public int nsurfs;
    }

    static partial class ModelXLwo
    {

        // lwo2.c

        public static st_lwObject lwGetObject(string filename, uint failID, int failpos);
        public static void lwFreeObject(st_lwObject o);
        public static void lwFreeLayer(st_lwLayer layer);

        // pntspols.c

        public static void lwFreePoints(st_lwPointList point);
        public static void lwFreePolygons(st_lwPolygonList plist);
        public static int lwGetPoints(VFile fp, int cksize, st_lwPointList point);
        public static void lwGetBoundingBox(st_lwPointList point, float[] bbox);
        public static int lwAllocPolygons(st_lwPolygonList plist, int npols, int nverts);
        public static int lwGetPolygons(VFile fp, int cksize, st_lwPolygonList plist, int ptoffset);
        public static void lwGetPolyNormals(st_lwPointList point, st_lwPolygonList polygon);
        public static int lwGetPointPolygons(st_lwPointList point, st_lwPolygonList polygon);
        public static int lwResolvePolySurfaces(st_lwPolygonList polygon, st_lwTagList tlist, st_lwSurface[] surf, int[] nsurfs);
        public static void lwGetVertNormals(st_lwPointList point, st_lwPolygonList polygon);
        public static void lwFreeTags(st_lwTagList tlist);
        public static int lwGetTags(VFile fp, int cksize, st_lwTagList tlist);
        public static int lwGetPolygonTags(VFile fp, int cksize, st_lwTagList tlist, st_lwPolygonList plist);

        // vmap.c

        public static void lwFreeVMap(st_lwVMap vmap);
        public static st_lwVMap lwGetVMap(VFile fp, int cksize, int ptoffset, int poloffset, int perpoly);
        public static int lwGetPointVMaps(st_lwPointList point, st_lwVMap vmap);
        public static int lwGetPolyVMaps(st_lwPolygonList polygon, st_lwVMap vmap);

        // clip.c

        public static void lwFreeClip(st_lwClip clip);
        public static st_lwClip lwGetClip(VFile fp, int cksize);
        public static st_lwClip lwFindClip(st_lwClip list, int index);

        // envelope.c

        public static void lwFreeEnvelope(st_lwEnvelope env);
        public static st_lwEnvelope lwGetEnvelope(VFile fp, int cksize);
        public static st_lwEnvelope lwFindEnvelope(st_lwEnvelope list, int index);
        public static float lwEvalEnvelope(st_lwEnvelope env, float time);

        // surface.c

        public static void lwFreePlugin(st_lwPlugin p);
        public static void lwFreeTexture(st_lwTexture t);
        public static void lwFreeSurface(st_lwSurface surf);
        public static int lwGetTHeader(VFile fp, int hsz, st_lwTexture tex);
        public static int lwGetTMap(VFile fp, int tmapsz, st_lwTMap tmap);
        public static int lwGetImageMap(VFile fp, int rsz, st_lwTexture tex);
        public static int lwGetProcedural(VFile fp, int rsz, st_lwTexture tex);
        public static int lwGetGradient(VFile fp, int rsz, st_lwTexture tex);
        public static st_lwTexture lwGetTexture(VFile fp, int bloksz, uint type);
        public static st_lwPlugin lwGetShader(VFile fp, int bloksz);
        public static st_lwSurface lwGetSurface(VFile fp, int cksize);
        public static st_lwSurface lwDefaultSurface();

        // lwob.c

        public static st_lwSurface lwGetSurface5(VFile fp, int cksize, st_lwObject obj);
        public static int lwGetPolygons5(VFile fp, int cksize, st_lwPolygonList plist, int ptoffset);
        public static st_lwObject lwGetObject5(string filename, uint failID, int failpos);

        // list.c

        //public static void lwListFree(object list, void ( * freeNode )(void* ));
        //public static void lwListAdd(object list, void* node);
        //public static void lwListInsert(object vlist, void* vitem, int ( * compare )(void*, void*));

        // vecmath.c

        //public static float dot(float[] a, float[] b);
        //public static void cross(float[] a, float[] b, float[] c);
        //public static void normalize(float[] v);
        //#define vecangle( a, b ) ( float ) idMath::ACos( dot( a, b ) )

        // lwio.c

        public static void set_flen(int i);
        public static int get_flen();
        public static byte[] getbytes(VFile fp, int size);
        public static void skipbytes(VFile fp, int n);
        public static int getI1(VFile fp);
        public static short getI2(VFile fp);
        public static int getI4(VFile fp);
        public static byte getU1(VFile fp);
        public static ushort getU2(VFile fp);
        public static uint getU4(VFile fp);
        public static int getVX(VFile fp);
        public static float getF4(VFile fp);
        public static string getS0(VFile fp);
        public static int sgetI1(byte[] bp);
        public static short sgetI2(byte[] bp);
        public static int sgetI4(byte[]  bp);
        public static byte sgetU1(byte[] bp);
        public static ushort sgetU2(byte[] bp);
        public static uint sgetU4(byte[] bp);
        public static int sgetVX(byte[] bp);
        public static float sgetF4(byte[] bp);
        public static string sgetS0(byte[] bp);
    }
}


using Gengine.Render;
using System;
using System.Collections.Generic;
using System.NumericsX;
using System.NumericsX.Core;

namespace Gengine.Framework
{
    public class ParticleParm
    {
        public ParticleParm() { table = null; from = to = 0.0f; }

        public DeclTable table;
        public float from;
        public float to;

        public float Eval(float frac, RandomX rand) => throw new NotImplementedException();
        public float Integrate(float frac, RandomX rand) => throw new NotImplementedException();
    }

    public enum PDIST
    {
        RECT,             // ( sizeX sizeY sizeZ )
        CYLINDER,         // ( sizeX sizeY sizeZ )
        SPHERE            // ( sizeX sizeY sizeZ ringFraction ) a ringFraction of zero allows the entire sphere, 0.9 would only allow the outer 10% of the sphere
    }

    public enum PDIR
    {
        CONE,              // parm0 is the solid cone angle
        OUTWARD            // direction is relative to offset from origin, parm0 is an upward bias
    }

    public enum PPATH
    {
        STANDARD,
        HELIX,            // ( sizeX sizeY sizeZ radialSpeed climbSpeed )
        FLIES,
        ORBIT,
        DRIP
    }

    public enum POR
    {
        VIEW,
        AIMED,              // angle and aspect are disregarded
        X,
        Y,
        Z
    }

    public struct ParticleGen
    {
        public RenderEntity renderEnt;          // for shaderParms, etc
        public RenderView renderView;
        public int index;               // particle number in the system
        public float frac;              // 0.0 to 1.0
        public Random random;
        public Vector3 origin;              // dynamic smoke particles can have individual origins and axis
        public Matrix3x3 axis;

        public float age;               // in seconds, calculated as fraction * stage->particleLife
        public Random originalRandom;     // needed so aimed particles can reset the random for another origin calculation
        public float animationFrameFrac;    // set by ParticleTexCoords, used to make the cross faded version
    }

    //
    // single particle stage
    //
    public class ParticleStage
    {
        //public ParticleStage();

        public void Default() => throw new NotImplementedException();
        public virtual int NumQuadsPerParticle() => throw new NotImplementedException();    // includes trails and cross faded animations returns the number of verts created, which will range from 0 to 4*NumQuadsPerParticle()
        public virtual int CreateParticle(ParticleGen g, DrawVert[] verts) => throw new NotImplementedException();

        public void ParticleOrigin(ParticleGen g, Vector3 origin) => throw new NotImplementedException();
        public int ParticleVerts(ParticleGen g, Vector3 origin, DrawVert[] verts) => throw new NotImplementedException();
        public void ParticleTexCoords(ParticleGen g, DrawVert[] verts) => throw new NotImplementedException();
        public void ParticleColors(ParticleGen g, DrawVert[] verts) => throw new NotImplementedException();

        public string GetCustomPathName() => throw new NotImplementedException();
        public string GetCustomPathDesc() => throw new NotImplementedException();
        public int NumCustomPathParms() => throw new NotImplementedException();
        public void SetCustomPathType(string p) => throw new NotImplementedException();
        //public void operator= (ParticleStage src);

        //------------------------------

        public Material material;

        public int totalParticles;     // total number of particles, although some may be invisible at a given time
        public float cycles;               // allows things to oneShot ( 1 cycle ) or run for a set number of cycles on a per stage basis

        public int cycleMsec;          // ( particleLife + deadTime ) in msec

        public float spawnBunching;        // 0.0 = all come out at first instant, 1.0 = evenly spaced over cycle time
        public float particleLife;     // total seconds of life for each particle
        public float timeOffset;           // time offset from system start for the first particle to spawn
        public float deadTime;         // time after particleLife before respawning

        //-------------------------------	// standard path parms

        public PDIST distributionType;
        public float[] distributionParms = new float[4];

        public PDIR directionType;
        public float[] directionParms = new float[4];

        public ParticleParm speed;
        public float gravity;              // can be negative to float up
        public bool worldGravity;          // apply gravity in world space
        public bool randomDistribution;        // randomly orient the quad on emission ( defaults to true )
        public bool entityColor;           // force color from render entity ( fadeColor is still valid )

        //------------------------------	// custom path will completely replace the standard path calculations

        public PPATH customPathType;      // use custom C code routines for determining the origin
        public float[] customPathParms = new float[8];

        //--------------------------------

        public Vector3 offset;              // offset from origin to spawn all particles, also applies to customPath

        public int animationFrames;    // if > 1, subdivide the texture S axis into frames and crossfade
        public float animationRate;        // frames per second

        public float initialAngle;     // in degrees, random angle is used if zero ( default )
        public ParticleParm rotationSpeed;       // half the particles will have negative rotation speeds

        public POR orientation;   // view, aimed, or axis fixed
        public float[] orientationParms = new float[4];

        public ParticleParm size;
        public ParticleParm aspect;              // greater than 1 makes the T axis longer

        public Vector4 color;
        public Vector4 fadeColor;           // either 0 0 0 0 for additive, or 1 1 1 0 for blended materials
        public float fadeInFraction;       // in 0.0 to 1.0 range
        public float fadeOutFraction;  // in 0.0 to 1.0 range
        public float fadeIndexFraction;    // in 0.0 to 1.0 range, causes later index smokes to be more faded

        public bool hidden;                // for editor use

        //-----------------------------------

        public float boundsExpansion;  // user tweak to fix poorly calculated bounds

        public Bounds bounds;                // derived
    }

    //
    // group of particle stages
    //
    public class DeclParticle : Decl
    {
        public override int Size() => throw new NotImplementedException();
        public override string DefaultDefinition() => throw new NotImplementedException();
        public override bool Parse(string text, int textLength) => throw new NotImplementedException();
        public override void FreeData() => throw new NotImplementedException();

        public bool Save(string fileName = null) => throw new NotImplementedException();

        public List<ParticleStage> stages = new();
        public Bounds bounds;
        public float depthHack;

        bool RebuildTextSource() => throw new NotImplementedException();
        void GetStageBounds(ParticleStage stage) => throw new NotImplementedException();
        ParticleStage ParseParticleStage(Lexer src) => throw new NotImplementedException();
        void ParseParms(Lexer src, float[] parms, int maxParms) => throw new NotImplementedException();
        void ParseParametric(Lexer src, ParticleParm parm) => throw new NotImplementedException();
        void WriteStage(VFile f, ParticleStage stage) => throw new NotImplementedException();
        void WriteParticleParm(VFile f, ParticleParm parm, string name) => throw new NotImplementedException();
    }
}

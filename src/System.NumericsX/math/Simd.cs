using static System.NumericsX.Platform;

namespace System.NumericsX
{
    public partial class SIMD
    {
        public static ISIMDProcessor processor = null;          // pointer to SIMD processor
        public static ISIMDProcessor generic = null;                // pointer to generic SIMD implementation
        public static ISIMDProcessor Processor = null;

        public static void Init()
        {
            //generic = new SIMD_Generic();
            //generic.cpuid = CPUID_GENERIC;
            //processor = null;
            //SIMDProcessor = generic;
        }

        public static void InitProcessor(string module, bool forceGeneric)
        {
            ISIMDProcessor newProcessor;

            var cpuid = 1; // Lib.sys.ProcessorId;

            if (forceGeneric)
                newProcessor = generic;
            else
            {
                //if (processor == null)
                //{
                //    if ((cpuid & CPUID.ALTIVEC) != 0) processor = new SIMD_AltiVec();
                //    else if ((cpuid & CPUID.MMX) != 0 && (cpuid & CPUID.SSE) != 0 && (cpuid & CPUID.SSE2) != 0 && (cpuid & CPUID.SSE3) != 0) processor = new SIMD_SSE3();
                //    else if ((cpuid & CPUID.MMX) != 0 && (cpuid & CPUID.SSE) != 0 && (cpuid & CPUID.SSE2) != 0) processor = new SIMD_SSE2();
                //    else if ((cpuid & CPUID.MMX) != 0 && (cpuid & CPUID.SSE) != 0) processor = new SIMD_SSE();
                //    else if ((cpuid & CPUID.MMX) != 0 && (cpuid & CPUID._3DNOW) != 0) processor = new SIMD_3DNow();
                //    else if ((cpuid & CPUID.MMX) != 0) processor = new SIMD_MMX();
                //    else processor = generic;
                //    processor.cpuid = cpuid;
                //}
                newProcessor = processor;
            }

            if (newProcessor != Processor)
            {
                Processor = newProcessor;
                Printf($"{module} using {Processor.Name} for SIMD processing\n");
            }

            //if ((cpuid & CPUID.SSE) != 0)
            //{
            //    sys.FPU_SetFTZ(true);
            //    sys.FPU_SetDAZ(true);
            //}
        }

        public static void Shutdown()
        {
            generic = null;
            processor = null;
            Processor = null;
        }

        public const int MIXBUFFER_SAMPLES = 4096;
    }

    public enum SPEAKER
    {
    	LEFT = 0,
    	RIGHT,
    	CENTER,
    	LFE,
    	BACKLEFT,
    	BACKRIGHT
    }

    public unsafe interface ISIMDProcessor
    {
        int cpuid { get; internal set; }
        string Name { get; }

        void Add(float* dst, float constant, float* src, int count);
        void Add(float* dst, float* src0, float* src1, int count);
        void Sub(float* dst, float constant, float* src, int count);
        void Sub(float* dst, float* src0, float* src1, int count);
        void Mul(float* dst, float constant, float* src, int count);
        void Mul(float* dst, float* src0, float* src1, int count);
        void Div(float* dst, float constant, float* src, int count);
        void Div(float* dst, float* src0, float* src1, int count);
        void MulAdd(float* dst, float constant, float* src, int count);
        void MulAdd(float* dst, float* src0, float* src1, int count);
        void MulSub(float* dst, float constant, float* src, int count);
        void MulSub(float* dst, float* src0, float* src1, int count);

        void Dot(float* dst, Vector3 constant, Vector3* src, int count);
        void Dot(float* dst, Vector3 constant, Plane* src, int count);
        void Dot(float* dst, Vector3 constant, DrawVert src, int count);
        void Dot(float* dst, Plane constant, Vector3* src, int count);
        void Dot(float* dst, Plane constant, Plane* src, int count);
        void Dot(float* dst, Plane constant, DrawVert src, int count);
        void Dot(float* dst, Vector3* src0, Vector3* src1, int count);
        void Dot(ref float dot, float* src1, float* src2, int count);

        void CmpGT(byte* dst, float* src0, float constant, int count);
        void CmpGT(byte* dst, byte bitNum, float* src0, float constant, int count);
        void CmpGE(byte* dst, float* src0, float constant, int count);
        void CmpGE(byte* dst, byte bitNum, float* src0, float constant, int count);
        void CmpLT(byte* dst, float* src0, float constant, int count);
        void CmpLT(byte* dst, byte bitNum, float* src0, float constant, int count);
        void CmpLE(byte* dst, float* src0, float constant, int count);
        void CmpLE(byte* dst, byte bitNum, float* src0, float constant, int count);

        void MinMax(out float min, out float max, float[] src, int count);
        void MinMax(out Vector2 min, out Vector2 max, Vector2[] src, int count);
        void MinMax(out Vector3 min, out Vector3 max, Vector3[] src, int count);
        void MinMax(out Vector3 min, out Vector3 max, DrawVert src, int count);
        void MinMax(out Vector3 min, out Vector3 max, DrawVert src, int[] indexes, int count);
        void MinMax(out Vector3 min, out Vector3 max, DrawVert src, short[] indexes, int count);

        void Clamp(float* dst, float* src, float min, float max, int count);
        void ClampMin(float* dst, float* src, float min, int count);
        void ClampMax(float* dst, float* src, float max, int count);

        void Memcpy(void* dst, void* src, int count);
        void Memset(void* dst, int val, int count);

        // these assume 16 byte aligned and 16 byte padded memory
        void Zero16(float* dst, int count);
        void Negate16(float* dst, int count);
        void Copy16(float* dst, float* src, int count);
        void Add16(float* dst, float* src1, float* src2, int count);
        void Sub16(float* dst, float* src1, float* src2, int count);
        void Mul16(float* dst, float* src1, float constant, int count);
        void AddAssign16(float* dst, float* src, int count);
        void SubAssign16(float* dst, float* src, int count);
        void MulAssign16(float* dst, float constant, int count);

        // MatX operations
        void MatX_MultiplyVecX(VectorX dst, MatrixX mat, VectorX vec);
        void MatX_MultiplyAddVecX(VectorX dst, MatrixX mat, VectorX vec);
        void MatX_MultiplySubVecX(VectorX dst, MatrixX mat, VectorX vec);
        void MatX_TransposeMultiplyVecX(VectorX dst, MatrixX mat, VectorX vec);
        void MatX_TransposeMultiplyAddVecX(VectorX dst, MatrixX mat, VectorX vec);
        void MatX_TransposeMultiplySubVecX(VectorX dst, MatrixX mat, VectorX vec);
        void MatX_MultiplyMatX(MatrixX dst, MatrixX m1, MatrixX m2);
        void MatX_TransposeMultiplyMatX(MatrixX dst, MatrixX m1, MatrixX m2);
        void MatX_LowerTriangularSolve(MatrixX L, float* x, float* b, int n, int skip = 0);
        void MatX_LowerTriangularSolveTranspose(MatrixX L, float* x, float* b, int n);
        void MatX_LDLTFactor(MatrixX mat, VectorX invDiag, int n);

        // rendering
        //void BlendJoints(JointQuat* joints, JointQuat* blendJoints, float lerp, int* index, int numJoints);
        //void ConvertJointQuatsToJointMats(JointMat* jointMats, JointQuat* jointQuats, int numJoints);
        //void ConvertJointMatsToJointQuats(JointQuat* jointQuats, JointMat* jointMats, int numJoints);
        //void TransformJoints(JointMat* jointMats, int* parents, int firstJoint, int lastJoint);
        //void UntransformJoints(JointMat* jointMats, int* parents, int firstJoint, int lastJoint);
        //void TransformVerts(DrawVert* verts, int numVerts, JointMat* joints, Vector4* weights, int* index, int numWeights);
        //void TracePointCull(byte* cullBits, byte &totalOr, float radius, Plane* planes, DrawVert* verts, int numVerts);
        //void DecalPointCull(byte* cullBits, Plane* planes, DrawVert* verts, int numVerts);
        //void OverlayPointCull(byte* cullBits, Vector2* texCoords, Plane* planes, DrawVert* verts, int numVerts);
        //void DeriveTriPlanes(Plane* planes, DrawVert* verts, int numVerts, int* indexes, int numIndexes);
        //void DeriveTriPlanes(Plane* planes, DrawVert* verts, int numVerts, short* indexes, int numIndexes);
        //void DeriveTangents(Plane* planes, DrawVert* verts, int numVerts, int* indexes, int numIndexes);
        //void DeriveTangents(Plane* planes, DrawVert* verts, int numVerts, short* indexes, int numIndexes);
        //void DeriveUnsmoothedTangents(DrawVert* verts, dominantTri_s* dominantTris, int numVerts);
        //void NormalizeTangents(DrawVert* verts, int numVerts);
        //void CreateTextureSpaceLightVectors(Vector3* lightVectors, Vector3 lightOrigin, DrawVert* verts, int numVerts, int* indexes, int numIndexes);
        //void CreateSpecularTextureCoords(Vector4* texCoords, Vector3 lightOrigin, Vector3 viewOrigin, DrawVert* verts, int numVerts, int* indexes, int numIndexes);
        //int CreateShadowCache(Vector4* vertexCache, int* vertRemap, Vector3 lightOrigin, DrawVert* verts, int numVerts);
        //int CreateVertexProgramShadowCache(Vector4* vertexCache, DrawVert* verts, int numVerts);

        // sound mixing
        void UpSamplePCMTo44kHz(float* dest, short* pcm, int numSamples, int kHz, int numChannels);
        void UpSampleOGGTo44kHz(float* dest, float** ogg, int numSamples, int kHz, int numChannels);
        void MixSoundTwoSpeakerMono(float* mixBuffer, float* samples, int numSamples, float[] lastV, float[] currentV);
        void MixSoundTwoSpeakerStereo(float* mixBuffer, float* samples, int numSamples, float[] lastV, float[] currentV);
        void MixSoundSixSpeakerMono(float* mixBuffer, float* samples, int numSamples, float[] lastV, float[] currentV);
        void MixSoundSixSpeakerStereo(float* mixBuffer, float* samples, int numSamples, float[] lastV, float[] currentV);
        void MixedSoundToSamples(short* samples, float* mixBuffer, int numSamples);
    }
}

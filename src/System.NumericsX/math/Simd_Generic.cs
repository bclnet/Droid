using System.Diagnostics;

namespace System.NumericsX
{
    public unsafe class SIMD_Generic : ISIMDProcessor
    {
        public string Name => "generic code";

        // dst[i] = constant + src[i];
        public void Add(float* dst, float constant, float* src, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src[_IX + 0] + constant;
                dst[_IX + 1] = src[_IX + 1] + constant;
                dst[_IX + 2] = src[_IX + 2] + constant;
                dst[_IX + 3] = src[_IX + 3] + constant;
            }
            for (; _IX < count; _IX++) { dst[_IX] = src[_IX] + constant; }
        }
        // dst[i] = src0[i] + src1[i];
        public void Add(float* dst, float* src0, float* src1, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] + src1[_IX + 0];
                dst[_IX + 1] = src0[_IX + 1] + src1[_IX + 1];
                dst[_IX + 2] = src0[_IX + 2] + src1[_IX + 2];
                dst[_IX + 3] = src0[_IX + 3] + src1[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] + src1[_IX]; }
        }
        // dst[i] = constant - src[i];
        public void Sub(float* dst, float constant, float* src, int count)
        {
            var c = constant; //: double
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = c - src[_IX + 0];
                dst[_IX + 1] = c - src[_IX + 1];
                dst[_IX + 2] = c - src[_IX + 2];
                dst[_IX + 3] = c - src[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = c - src[_IX]; }
        }
        // dst[i] = src0[i] - src1[i];
        public void Sub(float* dst, float* src0, float* src1, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] - src1[_IX + 0];
                dst[_IX + 1] = src0[_IX + 1] - src1[_IX + 1];
                dst[_IX + 2] = src0[_IX + 2] - src1[_IX + 2];
                dst[_IX + 3] = src0[_IX + 3] - src1[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] - src1[_IX]; }
        }
        // dst[i] = constant * src[i];
        public void Mul(float* dst, float constant, float* src, int count)
        {
            var c = constant; //: double
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = c * src[_IX + 0];
                dst[_IX + 1] = c * src[_IX + 1];
                dst[_IX + 2] = c * src[_IX + 2];
                dst[_IX + 3] = c * src[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = c * src[_IX]; }
        }
        // dst[i] = src0[i] * src1[i];
        public void Mul(float* dst, float* src0, float* src1, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] * src1[_IX + 0];
                dst[_IX + 1] = src0[_IX + 1] * src1[_IX + 1];
                dst[_IX + 2] = src0[_IX + 2] * src1[_IX + 2];
                dst[_IX + 3] = src0[_IX + 3] * src1[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] * src1[_IX]; }
        }
        // dst[i] = constant / src[i];
        public void Div(float* dst, float constant, float* src, int count)
        {
            var c = constant; //: double
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = c / src[_IX + 0];
                dst[_IX + 1] = c / src[_IX + 1];
                dst[_IX + 2] = c / src[_IX + 2];
                dst[_IX + 3] = c / src[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = c / src[_IX]; }
        }
        // dst[i] = src0[i] / src1[i];
        public void Div(float* dst, float* src0, float* src1, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] / src1[_IX + 0];
                dst[_IX + 1] = src0[_IX + 1] / src1[_IX + 1];
                dst[_IX + 2] = src0[_IX + 2] / src1[_IX + 2];
                dst[_IX + 3] = src0[_IX + 3] / src1[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] / src1[_IX]; }
        }
        // dst[i] += constant * src[i];
        public void MulAdd(float* dst, float constant, float* src, int count)
        {
            var c = constant; //: double
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] += c * src[_IX + 0];
                dst[_IX + 1] += c * src[_IX + 1];
                dst[_IX + 2] += c * src[_IX + 2];
                dst[_IX + 3] += c * src[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] += c * src[_IX]; }
        }
        // dst[i] += src0[i] * src1[i];
        public void MulAdd(float* dst, float* src0, float* src1, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] += src0[_IX + 0] * src1[_IX + 0];
                dst[_IX + 1] += src0[_IX + 1] * src1[_IX + 1];
                dst[_IX + 2] += src0[_IX + 2] * src1[_IX + 2];
                dst[_IX + 3] += src0[_IX + 3] * src1[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] += src0[_IX] * src1[_IX]; }
        }
        // dst[i] -= constant * src[i];
        public void MulSub(float* dst, float constant, float* src, int count)
        {
            var c = constant; //: double
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] -= c * src[_IX + 0];
                dst[_IX + 1] -= c * src[_IX + 1];
                dst[_IX + 2] -= c * src[_IX + 2];
                dst[_IX + 3] -= c * src[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] -= c * src[_IX]; }
        }
        // dst[i] -= src0[i] * src1[i];
        public void MulSub(float* dst, float* src0, float* src1, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] -= src0[_IX + 0] * src1[_IX + 0];
                dst[_IX + 1] -= src0[_IX + 1] * src1[_IX + 1];
                dst[_IX + 2] -= src0[_IX + 2] * src1[_IX + 2];
                dst[_IX + 3] -= src0[_IX + 3] * src1[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] -= src0[_IX] * src1[_IX]; }
        }

        // dst[i] = constant * src[i];
        public void Dot(float* dst, Vector3 constant, Vector3* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = constant * src[_IX];
            }
        }
        // dst[i] = constant * src[i].Normal() + src[i][3];
        public void Dot(float* dst, Vector3 constant, Plane* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = constant * src[_IX].Normal + src[_IX].d;
            }
        }
        // dst[i] = constant * src[i].xyz;
        public void Dot(float* dst, Vector3 constant, DrawVert* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = constant * src[_IX].xyz;
            }
        }
        // dst[i] = constant.Normal() * src[i] + constant[3];
        public void Dot(float* dst, Plane constant, Vector3* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = constant.Normal * src[_IX] + constant.d;
            }
        }
        // dst[i] = constant.Normal() * src[i].Normal() + constant[3] * src[i][3];
        public void Dot(float* dst, Plane constant, Plane* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = constant.Normal * src[_IX].Normal + constant.d * src[_IX].d;
            }
        }
        // dst[i] = constant.Normal() * src[i].xyz + constant[3];
        public void Dot(float* dst, Plane constant, DrawVert* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = constant.Normal * src[_IX].xyz + constant.d;
            }
        }
        // dst[i] = src0[i] * src1[i];
        public void Dot(float* dst, Vector3* src0, Vector3* src1, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src0[_IX] * src1[_IX];
            }
        }
        // dot = src1[0] * src2[0] + src1[1] * src2[1] + src1[2] * src2[2] + ...
        public void Dot(float dot, float* src1, float* src2, int count)
        {
#if true
            switch (count)
            {
                case 0: dot = 0f; return;
                case 1: dot = src1[0] * src2[0]; return;
                case 2: dot = src1[0] * src2[0] + src1[1] * src2[1]; return;
                case 3: dot = src1[0] * src2[0] + src1[1] * src2[1] + src1[2] * src2[2]; return;
                default:
                    {
                        int i; double s0, s1, s2, s3;
                        s0 = src1[0] * src2[0];
                        s1 = src1[1] * src2[1];
                        s2 = src1[2] * src2[2];
                        s3 = src1[3] * src2[3];
                        for (i = 4; i < count - 7; i += 8)
                        {
                            s0 += src1[i + 0] * src2[i + 0];
                            s1 += src1[i + 1] * src2[i + 1];
                            s2 += src1[i + 2] * src2[i + 2];
                            s3 += src1[i + 3] * src2[i + 3];
                            s0 += src1[i + 4] * src2[i + 4];
                            s1 += src1[i + 5] * src2[i + 5];
                            s2 += src1[i + 6] * src2[i + 6];
                            s3 += src1[i + 7] * src2[i + 7];
                        }
                        switch (count - i)
                        {
                            default: Debug.Assert(false); goto case 7;
                            case 7: s0 += src1[i + 6] * src2[i + 6]; goto case 6;
                            case 6: s1 += src1[i + 5] * src2[i + 5]; goto case 5;
                            case 5: s2 += src1[i + 4] * src2[i + 4]; goto case 4;
                            case 4: s3 += src1[i + 3] * src2[i + 3]; goto case 3;
                            case 3: s0 += src1[i + 2] * src2[i + 2]; goto case 2;
                            case 2: s1 += src1[i + 1] * src2[i + 1]; goto case 1;
                            case 1: s2 += src1[i + 0] * src2[i + 0]; goto case 0;
                            case 0: break;
                        }
                        double sum;
                        sum = s3;
                        sum += s2;
                        sum += s1;
                        sum += s0;
                        dot = (float)sum;
                        return;
                    }
            }

#else
            dot = 0f;
            for (var i = 0; i < count; i++)
            {
                dot += src1[i] * src2[i];
            }
#endif
        }

        // dst[i] = src0[i] > constant;
        public void CmpGT(byte* dst, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] > constant ? 1 : 0;
                dst[_IX + 1] = src0[_IX + 1] > constant ? 1 : 0;
                dst[_IX + 2] = src0[_IX + 2] > constant ? 1 : 0;
                dst[_IX + 3] = src0[_IX + 3] > constant ? 1 : 0;
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] > constant ? 1 : 0; }
        }
        // dst[i] |= ( src0[i] > constant ) << bitNum;
        public void CmpGT(byte* dst, byte bitNum, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] |= (src0[_IX + 0] > constant ? 1 : 0) << bitNum;
                dst[_IX + 1] |= (src0[_IX + 1] > constant ? 1 : 0) << bitNum;
                dst[_IX + 2] |= (src0[_IX + 2] > constant ? 1 : 0) << bitNum;
                dst[_IX + 3] |= (src0[_IX + 3] > constant ? 1 : 0) << bitNum;
            }
            for (; _IX < count; _IX++) { dst[_IX] |= (src0[_IX] > constant ? 1 : 0) << bitNum; }
        }
        // dst[i] = src0[i] >= constant;
        public void CmpGE(byte* dst, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] >= constant ? 1 : 0;
                dst[_IX + 1] = src0[_IX + 1] >= constant ? 1 : 0;
                dst[_IX + 2] = src0[_IX + 2] >= constant ? 1 : 0;
                dst[_IX + 3] = src0[_IX + 3] >= constant ? 1 : 0;
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] >= constant ? 1 : 0; }
        }
        // dst[i] |= ( src0[i] >= constant ) << bitNum;
        public void CmpGE(byte* dst, byte bitNum, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] |= (src0[_IX + 0] >= constant ? 1 : 0) << bitNum;
                dst[_IX + 1] |= (src0[_IX + 1] >= constant ? 1 : 0) << bitNum;
                dst[_IX + 2] |= (src0[_IX + 2] >= constant ? 1 : 0) << bitNum;
                dst[_IX + 3] |= (src0[_IX + 3] >= constant ? 1 : 0) << bitNum;
            }
            for (; _IX < count; _IX++) { dst[_IX] |= (src0[_IX] >= constant ? 1 : 0) << bitNum; }
        }
        // dst[i] = src0[i] < constant;
        public void CmpLT(byte* dst, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] < constant ? 1 : 0;
                dst[_IX + 1] = src0[_IX + 1] < constant ? 1 : 0;
                dst[_IX + 2] = src0[_IX + 2] < constant ? 1 : 0;
                dst[_IX + 3] = src0[_IX + 3] < constant ? 1 : 0;
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] < constant ? 1 : 0; }
        }
        // dst[i] |= ( src0[i] < constant ) << bitNum;
        public void CmpLT(byte* dst, byte bitNum, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] |= (src0[_IX + 0] < constant ? 1 : 0) << bitNum;
                dst[_IX + 1] |= (src0[_IX + 1] < constant ? 1 : 0) << bitNum;
                dst[_IX + 2] |= (src0[_IX + 2] < constant ? 1 : 0) << bitNum;
                dst[_IX + 3] |= (src0[_IX + 3] < constant ? 1 : 0) << bitNum;
            }
            for (; _IX < count; _IX++) { dst[_IX] |= (src0[_IX] < constant ? 1 : 0) << bitNum; }
        }
        // dst[i] = src0[i] <= constant;
        public void CmpLE(byte* dst, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] <= constant ? 1 : 0;
                dst[_IX + 1] = src0[_IX + 1] <= constant ? 1 : 0;
                dst[_IX + 2] = src0[_IX + 2] <= constant ? 1 : 0;
                dst[_IX + 3] = src0[_IX + 3] <= constant ? 1 : 0;
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] <= constant ? 1 : 0; }
        }
        // dst[i] |= ( src0[i] <= constant ) << bitNum;
        public void CmpLE(byte* dst, byte bitNum, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] |= (src0[_IX + 0] <= constant ? 1 : 0) << bitNum;
                dst[_IX + 1] |= (src0[_IX + 1] <= constant ? 1 : 0) << bitNum;
                dst[_IX + 2] |= (src0[_IX + 2] <= constant ? 1 : 0) << bitNum;
                dst[_IX + 3] |= (src0[_IX + 3] <= constant ? 1 : 0) << bitNum;
            }
            for (; _IX < count; _IX++) { dst[_IX] |= (src0[_IX] <= constant ? 1 : 0) << bitNum; }
        }

        public void MinMax(float min, float max, float* src, int count)
        {
            min = MathX.INFINITY; max = -MathX.INFINITY;
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                if (src[_IX] < min) { min = src[_IX]; }
                if (src[_IX] > max) { max = src[_IX]; }
            }
        }
        public void MinMax(Vector2 min, Vector2 max, Vector2* src, int count)
        {
            min.x = min.y = MathX.INFINITY; max.x = max.y = -MathX.INFINITY;
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                var v = src[_IX];
                if (v.x < min.x) { min.x = v.x; }
                if (v.x > max.x) { max.x = v.x; }
                if (v.y < min.y) { min.y = v.y; }
                if (v.y > max.y) { max.y = v.y; }
            }
        }
        public void MinMax(Vector3 min, Vector3 max, Vector3* src, int count)
        {
            min.x = min.y = min.z = MathX.INFINITY; max.x = max.y = max.z = -MathX.INFINITY;
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                var v = src[_IX];
                if (v.x < min.x) { min.x = v.x; }
                if (v.x > max.x) { max.x = v.x; }
                if (v.y < min.y) { min.y = v.y; }
                if (v.y > max.y) { max.y = v.y; }
                if (v.z < min.z) { min.z = v.z; }
                if (v.z > max.z) { max.z = v.z; }
            }
        }
        public void MinMax(Vector3 min, Vector3 max, DrawVert* src, int count)
        {
            min.x = min.y = min.z = MathX.INFINITY; max.x = max.y = max.z = -MathX.INFINITY;
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                var v = src[_IX].xyz;
                if (v.x < min.x) { min.x = v.x; }
                if (v.x > max.x) { max.x = v.x; }
                if (v.y < min.y) { min.y = v.y; }
                if (v.y > max.y) { max.y = v.y; }
                if (v.z < min.z) { min.z = v.z; }
                if (v.z > max.z) { max.z = v.z; }
            }
        }
        public void MinMax(Vector3 min, Vector3 max, DrawVert* src, int* indexes, int count)
        {
            min.x = min.y = min.z = MathX.INFINITY; max.x = max.y = max.z = -MathX.INFINITY;
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                var v = src[indexes[_IX]].xyz;
                if (v.x < min.x) { min.x = v.x; }
                if (v.x > max.x) { max.x = v.x; }
                if (v.y < min.y) { min.y = v.y; }
                if (v.y > max.y) { max.y = v.y; }
                if (v.z < min.z) { min.z = v.z; }
                if (v.z > max.z) { max.z = v.z; }
            }
        }
        public void MinMax(Vector3 min, Vector3 max, DrawVert* src, short* indexes, int count)
        {
            min.x = min.y = min.z = MathX.INFINITY; max.x = max.y = max.z = -MathX.INFINITY;
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                var v = src[indexes[_IX]].xyz;
                if (v.x < min.x) { min.x = v.x; }
                if (v.x > max.x) { max.x = v.x; }
                if (v.y < min.y) { min.y = v.y; }
                if (v.y > max.y) { max.y = v.y; }
                if (v.z < min.z) { min.z = v.z; }
                if (v.z > max.z) { max.z = v.z; }
            }
        }

        public void Clamp(float* dst, float* src, float min, float max, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src[_IX] < min ? min : src[_IX] > max ? max : src[_IX];
            }
        }
        public void ClampMin(float* dst, float* src, float min, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src[_IX] < min ? min : src[_IX];
            }
        }
        public void ClampMax(float* dst, float* src, float max, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src[_IX] > max ? max : src[_IX];
            }
        }

        /*
                {
                    int _IX; for (_IX = 0; _IX<count; _IX++)
                    {
                        _IX;
                    }
                }
        */

        public void Memcpy(void* dst, void* src, int count)
            => memcpy(dst, src, count);
        public void Memset(void* dst, int val, int count)
            => memset(dst, val, count);

        public void Zero16(float* dst, int count)
            => memset(dst, 0, count * sizeof(float));
        public void Negate16(float* dst, int count)
        {
            uint* ptr = reinterpret_cast<uint*>(dst);
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                ptr[_IX] ^= 1 << 31;		// IEEE 32 bits float sign bit
            }
        }
        public void Copy16(float* dst, float* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src[_IX];
            }
        }
        public void Add16(float* dst, float* src1, float* src2, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src1[_IX] + src2[_IX];
            }
        }
        public void Sub16(float* dst, float* src1, float* src2, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src1[_IX] - src2[_IX];
            }
        }
        public void Mul16(float* dst, float* src1, float constant, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src1[_IX] * constant;
            }
        }
        public void AddAssign16(float* dst, float* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] += src[_IX];
            }
        }
        public void SubAssign16(float* dst, float* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] -= src[_IX];
            }
        }
        public void MulAssign16(float* dst, float constant, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] *= constant;
            }
        }

        public void MatX_MultiplyVecX(VectorX dst, MatrixX mat, VectorX vec)
        {
            int i, j, numRows;
            const float* mPtr, *vPtr;
            float* dstPtr;

            assert(vec.GetSize() >= mat.GetNumColumns());
            assert(dst.GetSize() >= mat.GetNumRows());

            mPtr = mat.ToFloatPtr();
            vPtr = vec.ToFloatPtr();
            dstPtr = dst.ToFloatPtr();
            numRows = mat.GetNumRows();
            switch (mat.GetNumColumns())
            {
                case 1:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] = mPtr[0] * vPtr[0];
                        mPtr++;
                    }
                    break;
                case 2:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] = mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1];
                        mPtr += 2;
                    }
                    break;
                case 3:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] = mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2];
                        mPtr += 3;
                    }
                    break;
                case 4:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] = mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] +
                                    mPtr[3] * vPtr[3];
                        mPtr += 4;
                    }
                    break;
                case 5:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] = mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] +
                                    mPtr[3] * vPtr[3] + mPtr[4] * vPtr[4];
                        mPtr += 5;
                    }
                    break;
                case 6:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] = mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] +
                                    mPtr[3] * vPtr[3] + mPtr[4] * vPtr[4] + mPtr[5] * vPtr[5];
                        mPtr += 6;
                    }
                    break;
                default:
                    int numColumns = mat.GetNumColumns();
                    for (i = 0; i < numRows; i++)
                    {
                        float sum = mPtr[0] * vPtr[0];
                        for (j = 1; j < numColumns; j++)
                        {
                            sum += mPtr[j] * vPtr[j];
                        }
                        dstPtr[i] = sum;
                        mPtr += numColumns;
                    }
                    break;
            }
        }
        public void MatX_MultiplyAddVecX(VectorX dst, MatrixX mat, VectorX vec)
        {
            int i, j, numRows;
            const float* mPtr, *vPtr;
            float* dstPtr;

            assert(vec.GetSize() >= mat.GetNumColumns());
            assert(dst.GetSize() >= mat.GetNumRows());

            mPtr = mat.ToFloatPtr();
            vPtr = vec.ToFloatPtr();
            dstPtr = dst.ToFloatPtr();
            numRows = mat.GetNumRows();
            switch (mat.GetNumColumns())
            {
                case 1:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] += mPtr[0] * vPtr[0];
                        mPtr++;
                    }
                    break;
                case 2:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] += mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1];
                        mPtr += 2;
                    }
                    break;
                case 3:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] += mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2];
                        mPtr += 3;
                    }
                    break;
                case 4:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] += mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] +
                                    mPtr[3] * vPtr[3];
                        mPtr += 4;
                    }
                    break;
                case 5:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] += mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] +
                                    mPtr[3] * vPtr[3] + mPtr[4] * vPtr[4];
                        mPtr += 5;
                    }
                    break;
                case 6:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] += mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] +
                                    mPtr[3] * vPtr[3] + mPtr[4] * vPtr[4] + mPtr[5] * vPtr[5];
                        mPtr += 6;
                    }
                    break;
                default:
                    int numColumns = mat.GetNumColumns();
                    for (i = 0; i < numRows; i++)
                    {
                        float sum = mPtr[0] * vPtr[0];
                        for (j = 1; j < numColumns; j++)
                        {
                            sum += mPtr[j] * vPtr[j];
                        }
                        dstPtr[i] += sum;
                        mPtr += numColumns;
                    }
                    break;
            }
        }
        public void MatX_MultiplySubVecX(VectorX dst, MatrixX mat, VectorX vec)
        {
            int i, j, numRows;
            const float* mPtr, *vPtr;
            float* dstPtr;

            assert(vec.GetSize() >= mat.GetNumColumns());
            assert(dst.GetSize() >= mat.GetNumRows());

            mPtr = mat.ToFloatPtr();
            vPtr = vec.ToFloatPtr();
            dstPtr = dst.ToFloatPtr();
            numRows = mat.GetNumRows();
            switch (mat.GetNumColumns())
            {
                case 1:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] -= mPtr[0] * vPtr[0];
                        mPtr++;
                    }
                    break;
                case 2:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] -= mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1];
                        mPtr += 2;
                    }
                    break;
                case 3:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] -= mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2];
                        mPtr += 3;
                    }
                    break;
                case 4:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] -= mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] +
                                    mPtr[3] * vPtr[3];
                        mPtr += 4;
                    }
                    break;
                case 5:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] -= mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] +
                                    mPtr[3] * vPtr[3] + mPtr[4] * vPtr[4];
                        mPtr += 5;
                    }
                    break;
                case 6:
                    for (i = 0; i < numRows; i++)
                    {
                        dstPtr[i] -= mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] +
                                    mPtr[3] * vPtr[3] + mPtr[4] * vPtr[4] + mPtr[5] * vPtr[5];
                        mPtr += 6;
                    }
                    break;
                default:
                    int numColumns = mat.GetNumColumns();
                    for (i = 0; i < numRows; i++)
                    {
                        float sum = mPtr[0] * vPtr[0];
                        for (j = 1; j < numColumns; j++)
                        {
                            sum += mPtr[j] * vPtr[j];
                        }
                        dstPtr[i] -= sum;
                        mPtr += numColumns;
                    }
                    break;
            }
        }
        public void MatX_TransposeMultiplyVecX(VectorX dst, MatrixX mat, VectorX vec)
        {
            int i, j, numColumns;
            const float* mPtr, *vPtr;
            float* dstPtr;

            assert(vec.GetSize() >= mat.GetNumRows());
            assert(dst.GetSize() >= mat.GetNumColumns());

            mPtr = mat.ToFloatPtr();
            vPtr = vec.ToFloatPtr();
            dstPtr = dst.ToFloatPtr();
            numColumns = mat.GetNumColumns();
            switch (mat.GetNumRows())
            {
                case 1:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] = *(mPtr) * vPtr[0];
                        mPtr++;
                    }
                    break;
                case 2:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] = *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1];
                        mPtr++;
                    }
                    break;
                case 3:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] = *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2];
                        mPtr++;
                    }
                    break;
                case 4:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] = *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] +
                                *(mPtr + 3 * numColumns) * vPtr[3];
                        mPtr++;
                    }
                    break;
                case 5:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] = *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] +
                                *(mPtr + 3 * numColumns) * vPtr[3] + *(mPtr + 4 * numColumns) * vPtr[4];
                        mPtr++;
                    }
                    break;
                case 6:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] = *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] +
                                *(mPtr + 3 * numColumns) * vPtr[3] + *(mPtr + 4 * numColumns) * vPtr[4] + *(mPtr + 5 * numColumns) * vPtr[5];
                        mPtr++;
                    }
                    break;
                default:
                    int numRows = mat.GetNumRows();
                    for (i = 0; i < numColumns; i++)
                    {
                        mPtr = mat.ToFloatPtr() + i;
                        float sum = mPtr[0] * vPtr[0];
                        for (j = 1; j < numRows; j++)
                        {
                            mPtr += numColumns;
                            sum += mPtr[0] * vPtr[j];
                        }
                        dstPtr[i] = sum;
                    }
                    break;
            }
        }
        public void MatX_TransposeMultiplyAddVecX(VectorX dst, MatrixX mat, VectorX vec)
        {
            int i, j, numColumns;
            const float* mPtr, *vPtr;
            float* dstPtr;

            assert(vec.GetSize() >= mat.GetNumRows());
            assert(dst.GetSize() >= mat.GetNumColumns());

            mPtr = mat.ToFloatPtr();
            vPtr = vec.ToFloatPtr();
            dstPtr = dst.ToFloatPtr();
            numColumns = mat.GetNumColumns();
            switch (mat.GetNumRows())
            {
                case 1:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] += *(mPtr) * vPtr[0];
                        mPtr++;
                    }
                    break;
                case 2:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] += *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1];
                        mPtr++;
                    }
                    break;
                case 3:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] += *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2];
                        mPtr++;
                    }
                    break;
                case 4:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] += *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] +
                                *(mPtr + 3 * numColumns) * vPtr[3];
                        mPtr++;
                    }
                    break;
                case 5:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] += *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] +
                                *(mPtr + 3 * numColumns) * vPtr[3] + *(mPtr + 4 * numColumns) * vPtr[4];
                        mPtr++;
                    }
                    break;
                case 6:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] += *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] +
                                *(mPtr + 3 * numColumns) * vPtr[3] + *(mPtr + 4 * numColumns) * vPtr[4] + *(mPtr + 5 * numColumns) * vPtr[5];
                        mPtr++;
                    }
                    break;
                default:
                    int numRows = mat.GetNumRows();
                    for (i = 0; i < numColumns; i++)
                    {
                        mPtr = mat.ToFloatPtr() + i;
                        float sum = mPtr[0] * vPtr[0];
                        for (j = 1; j < numRows; j++)
                        {
                            mPtr += numColumns;
                            sum += mPtr[0] * vPtr[j];
                        }
                        dstPtr[i] += sum;
                    }
                    break;
            }
        }

        public void MatX_TransposeMultiplySubVecX(VectorX dst, MatrixX mat, VectorX vec)
        {
            int i, numColumns;
            const float* mPtr, *vPtr;
            float* dstPtr;

            assert(vec.GetSize() >= mat.GetNumRows());
            assert(dst.GetSize() >= mat.GetNumColumns());

            mPtr = mat.ToFloatPtr();
            vPtr = vec.ToFloatPtr();
            dstPtr = dst.ToFloatPtr();
            numColumns = mat.GetNumColumns();
            switch (mat.GetNumRows())
            {
                case 1:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] -= *(mPtr) * vPtr[0];
                        mPtr++;
                    }
                    break;
                case 2:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] -= *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1];
                        mPtr++;
                    }
                    break;
                case 3:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] -= *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2];
                        mPtr++;
                    }
                    break;
                case 4:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] -= *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] +
                                *(mPtr + 3 * numColumns) * vPtr[3];
                        mPtr++;
                    }
                    break;
                case 5:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] -= *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] +
                                *(mPtr + 3 * numColumns) * vPtr[3] + *(mPtr + 4 * numColumns) * vPtr[4];
                        mPtr++;
                    }
                    break;
                case 6:
                    for (i = 0; i < numColumns; i++)
                    {
                        dstPtr[i] -= *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] +
                                *(mPtr + 3 * numColumns) * vPtr[3] + *(mPtr + 4 * numColumns) * vPtr[4] + *(mPtr + 5 * numColumns) * vPtr[5];
                        mPtr++;
                    }
                    break;
                default:
                    int numRows = mat.GetNumRows();
                    for (i = 0; i < numColumns; i++)
                    {
                        mPtr = mat.ToFloatPtr() + i;
                        float sum = mPtr[0] * vPtr[0];
                        for (int j = 1; j < numRows; j++)
                        {
                            mPtr += numColumns;
                            sum += mPtr[0] * vPtr[j];
                        }
                        dstPtr[i] -= sum;
                    }
                    break;
            }
        }
        // optimizes the following matrix multiplications:
        //
        // NxN * Nx6
        // 6xN * Nx6
        // Nx6 * 6xN
        // 6x6 * 6xN
        // 
        // with N in the range [1-6].
        public void MatX_MultiplyMatX(MatrixX dst, MatrixX m1, MatrixX m2)
        {
            int i, j, k, l, n;
            float* dstPtr;
            const float* m1Ptr, *m2Ptr;
            double sum;

            assert(m1.GetNumColumns() == m2.GetNumRows());

            dstPtr = dst.ToFloatPtr();
            m1Ptr = m1.ToFloatPtr();
            m2Ptr = m2.ToFloatPtr();
            k = m1.GetNumRows();
            l = m2.GetNumColumns();

            switch (m1.GetNumColumns())
            {
                case 1:
                    {
                        if (l == 6)
                        {
                            for (i = 0; i < k; i++)
                            {       // Nx1 * 1x6
                                *dstPtr++ = m1Ptr[i] * m2Ptr[0];
                                *dstPtr++ = m1Ptr[i] * m2Ptr[1];
                                *dstPtr++ = m1Ptr[i] * m2Ptr[2];
                                *dstPtr++ = m1Ptr[i] * m2Ptr[3];
                                *dstPtr++ = m1Ptr[i] * m2Ptr[4];
                                *dstPtr++ = m1Ptr[i] * m2Ptr[5];
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2.ToFloatPtr();
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0];
                                m2Ptr++;
                            }
                            m1Ptr++;
                        }
                        break;
                    }
                case 2:
                    {
                        if (l == 6)
                        {
                            for (i = 0; i < k; i++)
                            {       // Nx2 * 2x6
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[6];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[1] + m1Ptr[1] * m2Ptr[7];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[2] + m1Ptr[1] * m2Ptr[8];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[3] + m1Ptr[1] * m2Ptr[9];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[4] + m1Ptr[1] * m2Ptr[10];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[5] + m1Ptr[1] * m2Ptr[11];
                                m1Ptr += 2;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2.ToFloatPtr();
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[l];
                                m2Ptr++;
                            }
                            m1Ptr += 2;
                        }
                        break;
                    }
                case 3:
                    {
                        if (l == 6)
                        {
                            for (i = 0; i < k; i++)
                            {       // Nx3 * 3x6
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[6] + m1Ptr[2] * m2Ptr[12];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[1] + m1Ptr[1] * m2Ptr[7] + m1Ptr[2] * m2Ptr[13];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[2] + m1Ptr[1] * m2Ptr[8] + m1Ptr[2] * m2Ptr[14];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[3] + m1Ptr[1] * m2Ptr[9] + m1Ptr[2] * m2Ptr[15];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[4] + m1Ptr[1] * m2Ptr[10] + m1Ptr[2] * m2Ptr[16];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[5] + m1Ptr[1] * m2Ptr[11] + m1Ptr[2] * m2Ptr[17];
                                m1Ptr += 3;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2.ToFloatPtr();
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[l] + m1Ptr[2] * m2Ptr[2 * l];
                                m2Ptr++;
                            }
                            m1Ptr += 3;
                        }
                        break;
                    }
                case 4:
                    {
                        if (l == 6)
                        {
                            for (i = 0; i < k; i++)
                            {       // Nx4 * 4x6
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[6] + m1Ptr[2] * m2Ptr[12] + m1Ptr[3] * m2Ptr[18];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[1] + m1Ptr[1] * m2Ptr[7] + m1Ptr[2] * m2Ptr[13] + m1Ptr[3] * m2Ptr[19];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[2] + m1Ptr[1] * m2Ptr[8] + m1Ptr[2] * m2Ptr[14] + m1Ptr[3] * m2Ptr[20];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[3] + m1Ptr[1] * m2Ptr[9] + m1Ptr[2] * m2Ptr[15] + m1Ptr[3] * m2Ptr[21];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[4] + m1Ptr[1] * m2Ptr[10] + m1Ptr[2] * m2Ptr[16] + m1Ptr[3] * m2Ptr[22];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[5] + m1Ptr[1] * m2Ptr[11] + m1Ptr[2] * m2Ptr[17] + m1Ptr[3] * m2Ptr[23];
                                m1Ptr += 4;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2.ToFloatPtr();
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[l] + m1Ptr[2] * m2Ptr[2 * l] +
                                                 m1Ptr[3] * m2Ptr[3 * l];
                                m2Ptr++;
                            }
                            m1Ptr += 4;
                        }
                        break;
                    }
                case 5:
                    {
                        if (l == 6)
                        {
                            for (i = 0; i < k; i++)
                            {       // Nx5 * 5x6
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[6] + m1Ptr[2] * m2Ptr[12] + m1Ptr[3] * m2Ptr[18] + m1Ptr[4] * m2Ptr[24];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[1] + m1Ptr[1] * m2Ptr[7] + m1Ptr[2] * m2Ptr[13] + m1Ptr[3] * m2Ptr[19] + m1Ptr[4] * m2Ptr[25];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[2] + m1Ptr[1] * m2Ptr[8] + m1Ptr[2] * m2Ptr[14] + m1Ptr[3] * m2Ptr[20] + m1Ptr[4] * m2Ptr[26];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[3] + m1Ptr[1] * m2Ptr[9] + m1Ptr[2] * m2Ptr[15] + m1Ptr[3] * m2Ptr[21] + m1Ptr[4] * m2Ptr[27];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[4] + m1Ptr[1] * m2Ptr[10] + m1Ptr[2] * m2Ptr[16] + m1Ptr[3] * m2Ptr[22] + m1Ptr[4] * m2Ptr[28];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[5] + m1Ptr[1] * m2Ptr[11] + m1Ptr[2] * m2Ptr[17] + m1Ptr[3] * m2Ptr[23] + m1Ptr[4] * m2Ptr[29];
                                m1Ptr += 5;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2.ToFloatPtr();
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[l] + m1Ptr[2] * m2Ptr[2 * l] +
                                                 m1Ptr[3] * m2Ptr[3 * l] + m1Ptr[4] * m2Ptr[4 * l];
                                m2Ptr++;
                            }
                            m1Ptr += 5;
                        }
                        break;
                    }
                case 6:
                    {
                        switch (k)
                        {
                            case 1:
                                {
                                    if (l == 1)
                                    {       // 1x6 * 6x1
                                        dstPtr[0] = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[1] + m1Ptr[2] * m2Ptr[2] +
                                                     m1Ptr[3] * m2Ptr[3] + m1Ptr[4] * m2Ptr[4] + m1Ptr[5] * m2Ptr[5];
                                        return;
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    if (l == 2)
                                    {       // 2x6 * 6x2
                                        for (i = 0; i < 2; i++)
                                        {
                                            for (j = 0; j < 2; j++)
                                            {
                                                *dstPtr = m1Ptr[0] * m2Ptr[0 * 2 + j]
                                                        + m1Ptr[1] * m2Ptr[1 * 2 + j]
                                                        + m1Ptr[2] * m2Ptr[2 * 2 + j]
                                                        + m1Ptr[3] * m2Ptr[3 * 2 + j]
                                                        + m1Ptr[4] * m2Ptr[4 * 2 + j]
                                                        + m1Ptr[5] * m2Ptr[5 * 2 + j];
                                                dstPtr++;
                                            }
                                            m1Ptr += 6;
                                        }
                                        return;
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    if (l == 3)
                                    {       // 3x6 * 6x3
                                        for (i = 0; i < 3; i++)
                                        {
                                            for (j = 0; j < 3; j++)
                                            {
                                                *dstPtr = m1Ptr[0] * m2Ptr[0 * 3 + j]
                                                        + m1Ptr[1] * m2Ptr[1 * 3 + j]
                                                        + m1Ptr[2] * m2Ptr[2 * 3 + j]
                                                        + m1Ptr[3] * m2Ptr[3 * 3 + j]
                                                        + m1Ptr[4] * m2Ptr[4 * 3 + j]
                                                        + m1Ptr[5] * m2Ptr[5 * 3 + j];
                                                dstPtr++;
                                            }
                                            m1Ptr += 6;
                                        }
                                        return;
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    if (l == 4)
                                    {       // 4x6 * 6x4
                                        for (i = 0; i < 4; i++)
                                        {
                                            for (j = 0; j < 4; j++)
                                            {
                                                *dstPtr = m1Ptr[0] * m2Ptr[0 * 4 + j]
                                                        + m1Ptr[1] * m2Ptr[1 * 4 + j]
                                                        + m1Ptr[2] * m2Ptr[2 * 4 + j]
                                                        + m1Ptr[3] * m2Ptr[3 * 4 + j]
                                                        + m1Ptr[4] * m2Ptr[4 * 4 + j]
                                                        + m1Ptr[5] * m2Ptr[5 * 4 + j];
                                                dstPtr++;
                                            }
                                            m1Ptr += 6;
                                        }
                                        return;
                                    }
                                }
                            case 5:
                                {
                                    if (l == 5)
                                    {       // 5x6 * 6x5
                                        for (i = 0; i < 5; i++)
                                        {
                                            for (j = 0; j < 5; j++)
                                            {
                                                *dstPtr = m1Ptr[0] * m2Ptr[0 * 5 + j]
                                                        + m1Ptr[1] * m2Ptr[1 * 5 + j]
                                                        + m1Ptr[2] * m2Ptr[2 * 5 + j]
                                                        + m1Ptr[3] * m2Ptr[3 * 5 + j]
                                                        + m1Ptr[4] * m2Ptr[4 * 5 + j]
                                                        + m1Ptr[5] * m2Ptr[5 * 5 + j];
                                                dstPtr++;
                                            }
                                            m1Ptr += 6;
                                        }
                                        return;
                                    }
                                }
                            case 6:
                                {
                                    switch (l)
                                    {
                                        case 1:
                                            {       // 6x6 * 6x1
                                                for (i = 0; i < 6; i++)
                                                {
                                                    *dstPtr = m1Ptr[0] * m2Ptr[0 * 1]
                                                            + m1Ptr[1] * m2Ptr[1 * 1]
                                                            + m1Ptr[2] * m2Ptr[2 * 1]
                                                            + m1Ptr[3] * m2Ptr[3 * 1]
                                                            + m1Ptr[4] * m2Ptr[4 * 1]
                                                            + m1Ptr[5] * m2Ptr[5 * 1];
                                                    dstPtr++;
                                                    m1Ptr += 6;
                                                }
                                                return;
                                            }
                                        case 2:
                                            {       // 6x6 * 6x2
                                                for (i = 0; i < 6; i++)
                                                {
                                                    for (j = 0; j < 2; j++)
                                                    {
                                                        *dstPtr = m1Ptr[0] * m2Ptr[0 * 2 + j]
                                                                + m1Ptr[1] * m2Ptr[1 * 2 + j]
                                                                + m1Ptr[2] * m2Ptr[2 * 2 + j]
                                                                + m1Ptr[3] * m2Ptr[3 * 2 + j]
                                                                + m1Ptr[4] * m2Ptr[4 * 2 + j]
                                                                + m1Ptr[5] * m2Ptr[5 * 2 + j];
                                                        dstPtr++;
                                                    }
                                                    m1Ptr += 6;
                                                }
                                                return;
                                            }
                                        case 3:
                                            {       // 6x6 * 6x3
                                                for (i = 0; i < 6; i++)
                                                {
                                                    for (j = 0; j < 3; j++)
                                                    {
                                                        *dstPtr = m1Ptr[0] * m2Ptr[0 * 3 + j]
                                                                + m1Ptr[1] * m2Ptr[1 * 3 + j]
                                                                + m1Ptr[2] * m2Ptr[2 * 3 + j]
                                                                + m1Ptr[3] * m2Ptr[3 * 3 + j]
                                                                + m1Ptr[4] * m2Ptr[4 * 3 + j]
                                                                + m1Ptr[5] * m2Ptr[5 * 3 + j];
                                                        dstPtr++;
                                                    }
                                                    m1Ptr += 6;
                                                }
                                                return;
                                            }
                                        case 4:
                                            {       // 6x6 * 6x4
                                                for (i = 0; i < 6; i++)
                                                {
                                                    for (j = 0; j < 4; j++)
                                                    {
                                                        *dstPtr = m1Ptr[0] * m2Ptr[0 * 4 + j]
                                                                + m1Ptr[1] * m2Ptr[1 * 4 + j]
                                                                + m1Ptr[2] * m2Ptr[2 * 4 + j]
                                                                + m1Ptr[3] * m2Ptr[3 * 4 + j]
                                                                + m1Ptr[4] * m2Ptr[4 * 4 + j]
                                                                + m1Ptr[5] * m2Ptr[5 * 4 + j];
                                                        dstPtr++;
                                                    }
                                                    m1Ptr += 6;
                                                }
                                                return;
                                            }
                                        case 5:
                                            {       // 6x6 * 6x5
                                                for (i = 0; i < 6; i++)
                                                {
                                                    for (j = 0; j < 5; j++)
                                                    {
                                                        *dstPtr = m1Ptr[0] * m2Ptr[0 * 5 + j]
                                                                + m1Ptr[1] * m2Ptr[1 * 5 + j]
                                                                + m1Ptr[2] * m2Ptr[2 * 5 + j]
                                                                + m1Ptr[3] * m2Ptr[3 * 5 + j]
                                                                + m1Ptr[4] * m2Ptr[4 * 5 + j]
                                                                + m1Ptr[5] * m2Ptr[5 * 5 + j];
                                                        dstPtr++;
                                                    }
                                                    m1Ptr += 6;
                                                }
                                                return;
                                            }
                                        case 6:
                                            {       // 6x6 * 6x6
                                                for (i = 0; i < 6; i++)
                                                {
                                                    for (j = 0; j < 6; j++)
                                                    {
                                                        *dstPtr = m1Ptr[0] * m2Ptr[0 * 6 + j]
                                                                + m1Ptr[1] * m2Ptr[1 * 6 + j]
                                                                + m1Ptr[2] * m2Ptr[2 * 6 + j]
                                                                + m1Ptr[3] * m2Ptr[3 * 6 + j]
                                                                + m1Ptr[4] * m2Ptr[4 * 6 + j]
                                                                + m1Ptr[5] * m2Ptr[5 * 6 + j];
                                                        dstPtr++;
                                                    }
                                                    m1Ptr += 6;
                                                }
                                                return;
                                            }
                                    }
                                }
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2.ToFloatPtr();
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[l] + m1Ptr[2] * m2Ptr[2 * l] +
                                                 m1Ptr[3] * m2Ptr[3 * l] + m1Ptr[4] * m2Ptr[4 * l] + m1Ptr[5] * m2Ptr[5 * l];
                                m2Ptr++;
                            }
                            m1Ptr += 6;
                        }
                        break;
                    }
                default:
                    {
                        for (i = 0; i < k; i++)
                        {
                            for (j = 0; j < l; j++)
                            {
                                m2Ptr = m2.ToFloatPtr() + j;
                                sum = m1Ptr[0] * m2Ptr[0];
                                for (n = 1; n < m1.GetNumColumns(); n++)
                                {
                                    m2Ptr += l;
                                    sum += m1Ptr[n] * m2Ptr[0];
                                }
                                *dstPtr++ = sum;
                            }
                            m1Ptr += m1.GetNumColumns();
                        }
                        break;
                    }
            }
        }
        // optimizes the following tranpose matrix multiplications:
        // 
        // Nx6 * NxN
        // 6xN * 6x6
        // 
        // with N in the range [1-6].
        public void MatX_TransposeMultiplyMatX(MatrixX dst, MatrixX m1, MatrixX m2)
        {
            int i, j, k, l, n;
            float* dstPtr;
            const float* m1Ptr, *m2Ptr;
            double sum;

            assert(m1.GetNumRows() == m2.GetNumRows());

            m1Ptr = m1.ToFloatPtr();
            m2Ptr = m2.ToFloatPtr();
            dstPtr = dst.ToFloatPtr();
            k = m1.GetNumColumns();
            l = m2.GetNumColumns();

            switch (m1.GetNumRows())
            {
                case 1:
                    if (k == 6 && l == 1)
                    {           // 1x6 * 1x1
                        for (i = 0; i < 6; i++)
                        {
                            *dstPtr++ = m1Ptr[0] * m2Ptr[0];
                            m1Ptr++;
                        }
                        return;
                    }
                    for (i = 0; i < k; i++)
                    {
                        m2Ptr = m2.ToFloatPtr();
                        for (j = 0; j < l; j++)
                        {
                            *dstPtr++ = m1Ptr[0] * m2Ptr[0];
                            m2Ptr++;
                        }
                        m1Ptr++;
                    }
                    break;
                case 2:
                    if (k == 6 && l == 2)
                    {           // 2x6 * 2x2
                        for (i = 0; i < 6; i++)
                        {
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 2 + 0] + m1Ptr[1 * 6] * m2Ptr[1 * 2 + 0];
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 2 + 1] + m1Ptr[1 * 6] * m2Ptr[1 * 2 + 1];
                            m1Ptr++;
                        }
                        return;
                    }
                    for (i = 0; i < k; i++)
                    {
                        m2Ptr = m2.ToFloatPtr();
                        for (j = 0; j < l; j++)
                        {
                            *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[k] * m2Ptr[l];
                            m2Ptr++;
                        }
                        m1Ptr++;
                    }
                    break;
                case 3:
                    if (k == 6 && l == 3)
                    {           // 3x6 * 3x3
                        for (i = 0; i < 6; i++)
                        {
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 3 + 0] + m1Ptr[1 * 6] * m2Ptr[1 * 3 + 0] + m1Ptr[2 * 6] * m2Ptr[2 * 3 + 0];
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 3 + 1] + m1Ptr[1 * 6] * m2Ptr[1 * 3 + 1] + m1Ptr[2 * 6] * m2Ptr[2 * 3 + 1];
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 3 + 2] + m1Ptr[1 * 6] * m2Ptr[1 * 3 + 2] + m1Ptr[2 * 6] * m2Ptr[2 * 3 + 2];
                            m1Ptr++;
                        }
                        return;
                    }
                    for (i = 0; i < k; i++)
                    {
                        m2Ptr = m2.ToFloatPtr();
                        for (j = 0; j < l; j++)
                        {
                            *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[k] * m2Ptr[l] + m1Ptr[2 * k] * m2Ptr[2 * l];
                            m2Ptr++;
                        }
                        m1Ptr++;
                    }
                    break;
                case 4:
                    if (k == 6 && l == 4)
                    {           // 4x6 * 4x4
                        for (i = 0; i < 6; i++)
                        {
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 4 + 0] + m1Ptr[1 * 6] * m2Ptr[1 * 4 + 0] + m1Ptr[2 * 6] * m2Ptr[2 * 4 + 0] + m1Ptr[3 * 6] * m2Ptr[3 * 4 + 0];
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 4 + 1] + m1Ptr[1 * 6] * m2Ptr[1 * 4 + 1] + m1Ptr[2 * 6] * m2Ptr[2 * 4 + 1] + m1Ptr[3 * 6] * m2Ptr[3 * 4 + 1];
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 4 + 2] + m1Ptr[1 * 6] * m2Ptr[1 * 4 + 2] + m1Ptr[2 * 6] * m2Ptr[2 * 4 + 2] + m1Ptr[3 * 6] * m2Ptr[3 * 4 + 2];
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 4 + 3] + m1Ptr[1 * 6] * m2Ptr[1 * 4 + 3] + m1Ptr[2 * 6] * m2Ptr[2 * 4 + 3] + m1Ptr[3 * 6] * m2Ptr[3 * 4 + 3];
                            m1Ptr++;
                        }
                        return;
                    }
                    for (i = 0; i < k; i++)
                    {
                        m2Ptr = m2.ToFloatPtr();
                        for (j = 0; j < l; j++)
                        {
                            *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[k] * m2Ptr[l] + m1Ptr[2 * k] * m2Ptr[2 * l] +
                                            m1Ptr[3 * k] * m2Ptr[3 * l];
                            m2Ptr++;
                        }
                        m1Ptr++;
                    }
                    break;
                case 5:
                    if (k == 6 && l == 5)
                    {           // 5x6 * 5x5
                        for (i = 0; i < 6; i++)
                        {
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 5 + 0] + m1Ptr[1 * 6] * m2Ptr[1 * 5 + 0] + m1Ptr[2 * 6] * m2Ptr[2 * 5 + 0] + m1Ptr[3 * 6] * m2Ptr[3 * 5 + 0] + m1Ptr[4 * 6] * m2Ptr[4 * 5 + 0];
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 5 + 1] + m1Ptr[1 * 6] * m2Ptr[1 * 5 + 1] + m1Ptr[2 * 6] * m2Ptr[2 * 5 + 1] + m1Ptr[3 * 6] * m2Ptr[3 * 5 + 1] + m1Ptr[4 * 6] * m2Ptr[4 * 5 + 1];
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 5 + 2] + m1Ptr[1 * 6] * m2Ptr[1 * 5 + 2] + m1Ptr[2 * 6] * m2Ptr[2 * 5 + 2] + m1Ptr[3 * 6] * m2Ptr[3 * 5 + 2] + m1Ptr[4 * 6] * m2Ptr[4 * 5 + 2];
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 5 + 3] + m1Ptr[1 * 6] * m2Ptr[1 * 5 + 3] + m1Ptr[2 * 6] * m2Ptr[2 * 5 + 3] + m1Ptr[3 * 6] * m2Ptr[3 * 5 + 3] + m1Ptr[4 * 6] * m2Ptr[4 * 5 + 3];
                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 5 + 4] + m1Ptr[1 * 6] * m2Ptr[1 * 5 + 4] + m1Ptr[2 * 6] * m2Ptr[2 * 5 + 4] + m1Ptr[3 * 6] * m2Ptr[3 * 5 + 4] + m1Ptr[4 * 6] * m2Ptr[4 * 5 + 4];
                            m1Ptr++;
                        }
                        return;
                    }
                    for (i = 0; i < k; i++)
                    {
                        m2Ptr = m2.ToFloatPtr();
                        for (j = 0; j < l; j++)
                        {
                            *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[k] * m2Ptr[l] + m1Ptr[2 * k] * m2Ptr[2 * l] +
                                            m1Ptr[3 * k] * m2Ptr[3 * l] + m1Ptr[4 * k] * m2Ptr[4 * l];
                            m2Ptr++;
                        }
                        m1Ptr++;
                    }
                    break;
                case 6:
                    if (l == 6)
                    {
                        switch (k)
                        {
                            case 1:                     // 6x1 * 6x6
                                m2Ptr = m2.ToFloatPtr();
                                for (j = 0; j < 6; j++)
                                {
                                    *dstPtr++ = m1Ptr[0 * 1] * m2Ptr[0 * 6] +
                                                m1Ptr[1 * 1] * m2Ptr[1 * 6] +
                                                m1Ptr[2 * 1] * m2Ptr[2 * 6] +
                                                m1Ptr[3 * 1] * m2Ptr[3 * 6] +
                                                m1Ptr[4 * 1] * m2Ptr[4 * 6] +
                                                m1Ptr[5 * 1] * m2Ptr[5 * 6];
                                    m2Ptr++;
                                }
                                return;
                            case 2:                     // 6x2 * 6x6
                                for (i = 0; i < 2; i++)
                                {
                                    m2Ptr = m2.ToFloatPtr();
                                    for (j = 0; j < 6; j++)
                                    {
                                        *dstPtr++ = m1Ptr[0 * 2] * m2Ptr[0 * 6] +
                                                    m1Ptr[1 * 2] * m2Ptr[1 * 6] +
                                                    m1Ptr[2 * 2] * m2Ptr[2 * 6] +
                                                    m1Ptr[3 * 2] * m2Ptr[3 * 6] +
                                                    m1Ptr[4 * 2] * m2Ptr[4 * 6] +
                                                    m1Ptr[5 * 2] * m2Ptr[5 * 6];
                                        m2Ptr++;
                                    }
                                    m1Ptr++;
                                }
                                return;
                            case 3:                     // 6x3 * 6x6
                                for (i = 0; i < 3; i++)
                                {
                                    m2Ptr = m2.ToFloatPtr();
                                    for (j = 0; j < 6; j++)
                                    {
                                        *dstPtr++ = m1Ptr[0 * 3] * m2Ptr[0 * 6] +
                                                    m1Ptr[1 * 3] * m2Ptr[1 * 6] +
                                                    m1Ptr[2 * 3] * m2Ptr[2 * 6] +
                                                    m1Ptr[3 * 3] * m2Ptr[3 * 6] +
                                                    m1Ptr[4 * 3] * m2Ptr[4 * 6] +
                                                    m1Ptr[5 * 3] * m2Ptr[5 * 6];
                                        m2Ptr++;
                                    }
                                    m1Ptr++;
                                }
                                return;
                            case 4:                     // 6x4 * 6x6
                                for (i = 0; i < 4; i++)
                                {
                                    m2Ptr = m2.ToFloatPtr();
                                    for (j = 0; j < 6; j++)
                                    {
                                        *dstPtr++ = m1Ptr[0 * 4] * m2Ptr[0 * 6] +
                                                    m1Ptr[1 * 4] * m2Ptr[1 * 6] +
                                                    m1Ptr[2 * 4] * m2Ptr[2 * 6] +
                                                    m1Ptr[3 * 4] * m2Ptr[3 * 6] +
                                                    m1Ptr[4 * 4] * m2Ptr[4 * 6] +
                                                    m1Ptr[5 * 4] * m2Ptr[5 * 6];
                                        m2Ptr++;
                                    }
                                    m1Ptr++;
                                }
                                return;
                            case 5:                     // 6x5 * 6x6
                                for (i = 0; i < 5; i++)
                                {
                                    m2Ptr = m2.ToFloatPtr();
                                    for (j = 0; j < 6; j++)
                                    {
                                        *dstPtr++ = m1Ptr[0 * 5] * m2Ptr[0 * 6] +
                                                    m1Ptr[1 * 5] * m2Ptr[1 * 6] +
                                                    m1Ptr[2 * 5] * m2Ptr[2 * 6] +
                                                    m1Ptr[3 * 5] * m2Ptr[3 * 6] +
                                                    m1Ptr[4 * 5] * m2Ptr[4 * 6] +
                                                    m1Ptr[5 * 5] * m2Ptr[5 * 6];
                                        m2Ptr++;
                                    }
                                    m1Ptr++;
                                }
                                return;
                            case 6:                     // 6x6 * 6x6
                                for (i = 0; i < 6; i++)
                                {
                                    m2Ptr = m2.ToFloatPtr();
                                    for (j = 0; j < 6; j++)
                                    {
                                        *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 6] +
                                                    m1Ptr[1 * 6] * m2Ptr[1 * 6] +
                                                    m1Ptr[2 * 6] * m2Ptr[2 * 6] +
                                                    m1Ptr[3 * 6] * m2Ptr[3 * 6] +
                                                    m1Ptr[4 * 6] * m2Ptr[4 * 6] +
                                                    m1Ptr[5 * 6] * m2Ptr[5 * 6];
                                        m2Ptr++;
                                    }
                                    m1Ptr++;
                                }
                                return;
                        }
                    }
                    for (i = 0; i < k; i++)
                    {
                        m2Ptr = m2.ToFloatPtr();
                        for (j = 0; j < l; j++)
                        {
                            *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[k] * m2Ptr[l] + m1Ptr[2 * k] * m2Ptr[2 * l] +
                                            m1Ptr[3 * k] * m2Ptr[3 * l] + m1Ptr[4 * k] * m2Ptr[4 * l] + m1Ptr[5 * k] * m2Ptr[5 * l];
                            m2Ptr++;
                        }
                        m1Ptr++;
                    }
                    break;
                default:
                    for (i = 0; i < k; i++)
                    {
                        for (j = 0; j < l; j++)
                        {
                            m1Ptr = m1.ToFloatPtr() + i;
                            m2Ptr = m2.ToFloatPtr() + j;
                            sum = m1Ptr[0] * m2Ptr[0];
                            for (n = 1; n < m1.GetNumRows(); n++)
                            {
                                m1Ptr += k;
                                m2Ptr += l;
                                sum += m1Ptr[0] * m2Ptr[0];
                            }
                            *dstPtr++ = sum;
                        }
                    }
                    break;
            }
        }
        // solves x in Lx = b for the n * n sub-matrix of L if skip > 0 the first skip elements of x are assumed to be valid already
        // L has to be a lower triangular matrix with(implicit) ones on the diagonal x == b is allowed
        public void MatX_LowerTriangularSolve(MatrixX L, float* x, float* b, int n, int skip = 0)
        {
#if true
            int nc;
            float* lptr;

            if (skip >= n)
                return;

            lptr = L.ToFloatPtr();
            nc = L.NumColumns;

            // unrolled cases for n < 8
            if (n < 8)
            {
#define NSKIP( n, s )	((n<<3)|(s&7))
                switch (NSKIP(n, skip))
                {
                    case NSKIP(1, 0):
                        x[0] = b[0];
                        return;
                    case NSKIP(2, 0): x[0] = b[0];
                    case NSKIP(2, 1):
                        x[1] = b[1] - lptr[1 * nc + 0] * x[0];
                        return;
                    case NSKIP(3, 0): x[0] = b[0];
                    case NSKIP(3, 1): x[1] = b[1] - lptr[1 * nc + 0] * x[0];
                    case NSKIP(3, 2):
                        x[2] = b[2] - lptr[2 * nc + 0] * x[0] - lptr[2 * nc + 1] * x[1];
                        return;
                    case NSKIP(4, 0): x[0] = b[0];
                    case NSKIP(4, 1): x[1] = b[1] - lptr[1 * nc + 0] * x[0];
                    case NSKIP(4, 2): x[2] = b[2] - lptr[2 * nc + 0] * x[0] - lptr[2 * nc + 1] * x[1];
                    case NSKIP(4, 3):
                        x[3] = b[3] - lptr[3 * nc + 0] * x[0] - lptr[3 * nc + 1] * x[1] - lptr[3 * nc + 2] * x[2];
                        return;
                    case NSKIP(5, 0): x[0] = b[0];
                    case NSKIP(5, 1): x[1] = b[1] - lptr[1 * nc + 0] * x[0];
                    case NSKIP(5, 2): x[2] = b[2] - lptr[2 * nc + 0] * x[0] - lptr[2 * nc + 1] * x[1];
                    case NSKIP(5, 3): x[3] = b[3] - lptr[3 * nc + 0] * x[0] - lptr[3 * nc + 1] * x[1] - lptr[3 * nc + 2] * x[2];
                    case NSKIP(5, 4):
                        x[4] = b[4] - lptr[4 * nc + 0] * x[0] - lptr[4 * nc + 1] * x[1] - lptr[4 * nc + 2] * x[2] - lptr[4 * nc + 3] * x[3];
                        return;
                    case NSKIP(6, 0): x[0] = b[0];
                    case NSKIP(6, 1): x[1] = b[1] - lptr[1 * nc + 0] * x[0];
                    case NSKIP(6, 2): x[2] = b[2] - lptr[2 * nc + 0] * x[0] - lptr[2 * nc + 1] * x[1];
                    case NSKIP(6, 3): x[3] = b[3] - lptr[3 * nc + 0] * x[0] - lptr[3 * nc + 1] * x[1] - lptr[3 * nc + 2] * x[2];
                    case NSKIP(6, 4): x[4] = b[4] - lptr[4 * nc + 0] * x[0] - lptr[4 * nc + 1] * x[1] - lptr[4 * nc + 2] * x[2] - lptr[4 * nc + 3] * x[3];
                    case NSKIP(6, 5):
                        x[5] = b[5] - lptr[5 * nc + 0] * x[0] - lptr[5 * nc + 1] * x[1] - lptr[5 * nc + 2] * x[2] - lptr[5 * nc + 3] * x[3] - lptr[5 * nc + 4] * x[4];
                        return;
                    case NSKIP(7, 0): x[0] = b[0];
                    case NSKIP(7, 1): x[1] = b[1] - lptr[1 * nc + 0] * x[0];
                    case NSKIP(7, 2): x[2] = b[2] - lptr[2 * nc + 0] * x[0] - lptr[2 * nc + 1] * x[1];
                    case NSKIP(7, 3): x[3] = b[3] - lptr[3 * nc + 0] * x[0] - lptr[3 * nc + 1] * x[1] - lptr[3 * nc + 2] * x[2];
                    case NSKIP(7, 4): x[4] = b[4] - lptr[4 * nc + 0] * x[0] - lptr[4 * nc + 1] * x[1] - lptr[4 * nc + 2] * x[2] - lptr[4 * nc + 3] * x[3];
                    case NSKIP(7, 5): x[5] = b[5] - lptr[5 * nc + 0] * x[0] - lptr[5 * nc + 1] * x[1] - lptr[5 * nc + 2] * x[2] - lptr[5 * nc + 3] * x[3] - lptr[5 * nc + 4] * x[4];
                    case NSKIP(7, 6):
                        x[6] = b[6] - lptr[6 * nc + 0] * x[0] - lptr[6 * nc + 1] * x[1] - lptr[6 * nc + 2] * x[2] - lptr[6 * nc + 3] * x[3] - lptr[6 * nc + 4] * x[4] - lptr[6 * nc + 5] * x[5];
                        return;
                }
                return;
            }

            // process first 4 rows
            switch (skip)
            {
                case 0: x[0] = b[0];
                case 1: x[1] = b[1] - lptr[1 * nc + 0] * x[0];
                case 2: x[2] = b[2] - lptr[2 * nc + 0] * x[0] - lptr[2 * nc + 1] * x[1];
                case 3:
                    x[3] = b[3] - lptr[3 * nc + 0] * x[0] - lptr[3 * nc + 1] * x[1] - lptr[3 * nc + 2] * x[2];
                    skip = 4;
            }

            lptr = L[skip];

            int i, j;
            register double s0, s1, s2, s3;

            for (i = skip; i < n; i++)
            {
                s0 = lptr[0] * x[0];
                s1 = lptr[1] * x[1];
                s2 = lptr[2] * x[2];
                s3 = lptr[3] * x[3];
                for (j = 4; j < i - 7; j += 8)
                {
                    s0 += lptr[j + 0] * x[j + 0];
                    s1 += lptr[j + 1] * x[j + 1];
                    s2 += lptr[j + 2] * x[j + 2];
                    s3 += lptr[j + 3] * x[j + 3];
                    s0 += lptr[j + 4] * x[j + 4];
                    s1 += lptr[j + 5] * x[j + 5];
                    s2 += lptr[j + 6] * x[j + 6];
                    s3 += lptr[j + 7] * x[j + 7];
                }
                switch (i - j)
                {

                    default: Debug.Assert(false); break;
                    case 7: s0 += lptr[j + 6] * x[j + 6]; goto case 6;
                    case 6: s1 += lptr[j + 5] * x[j + 5]; goto case 5;
                    case 5: s2 += lptr[j + 4] * x[j + 4]; goto case 4;
                    case 4: s3 += lptr[j + 3] * x[j + 3]; goto case 3;
                    case 3: s0 += lptr[j + 2] * x[j + 2]; goto case 2;
                    case 2: s1 += lptr[j + 1] * x[j + 1]; goto case 1;
                    case 1: s2 += lptr[j + 0] * x[j + 0]; goto case 0;
                    case 0: break;
                }
                double sum;
                sum = s3;
                sum += s2;
                sum += s1;
                sum += s0;
                sum -= b[i];
                x[i] = -sum;
                lptr += nc;
            }

#else

	int i, j;
	const float *lptr;
	double sum;

	for ( i = skip; i < n; i++ ) {
		sum = b[i];
		lptr = L[i];
		for ( j = 0; j < i; j++ ) {
			sum -= lptr[j] * x[j];
		}
		x[i] = sum;
	}

#endif
        }
        //   solves x in L'x = b for the n * n sub-matrix of L
        // L has to be a lower triangular matrix with(implicit) ones on the diagonal
        // x == b is allowed
        public void MatX_LowerTriangularSolveTranspose(MatrixX L, float* x, float* b, int n)
        {
#if true
            int nc;
            const float* lptr;

            lptr = L.ToFloatPtr();
            nc = L.GetNumColumns();

            // unrolled cases for n < 8
            if (n < 8)
            {
                switch (n)
                {
                    case 0:
                        return;
                    case 1:
                        x[0] = b[0];
                        return;
                    case 2:
                        x[1] = b[1];
                        x[0] = b[0] - lptr[1 * nc + 0] * x[1];
                        return;
                    case 3:
                        x[2] = b[2];
                        x[1] = b[1] - lptr[2 * nc + 1] * x[2];
                        x[0] = b[0] - lptr[2 * nc + 0] * x[2] - lptr[1 * nc + 0] * x[1];
                        return;
                    case 4:
                        x[3] = b[3];
                        x[2] = b[2] - lptr[3 * nc + 2] * x[3];
                        x[1] = b[1] - lptr[3 * nc + 1] * x[3] - lptr[2 * nc + 1] * x[2];
                        x[0] = b[0] - lptr[3 * nc + 0] * x[3] - lptr[2 * nc + 0] * x[2] - lptr[1 * nc + 0] * x[1];
                        return;
                    case 5:
                        x[4] = b[4];
                        x[3] = b[3] - lptr[4 * nc + 3] * x[4];
                        x[2] = b[2] - lptr[4 * nc + 2] * x[4] - lptr[3 * nc + 2] * x[3];
                        x[1] = b[1] - lptr[4 * nc + 1] * x[4] - lptr[3 * nc + 1] * x[3] - lptr[2 * nc + 1] * x[2];
                        x[0] = b[0] - lptr[4 * nc + 0] * x[4] - lptr[3 * nc + 0] * x[3] - lptr[2 * nc + 0] * x[2] - lptr[1 * nc + 0] * x[1];
                        return;
                    case 6:
                        x[5] = b[5];
                        x[4] = b[4] - lptr[5 * nc + 4] * x[5];
                        x[3] = b[3] - lptr[5 * nc + 3] * x[5] - lptr[4 * nc + 3] * x[4];
                        x[2] = b[2] - lptr[5 * nc + 2] * x[5] - lptr[4 * nc + 2] * x[4] - lptr[3 * nc + 2] * x[3];
                        x[1] = b[1] - lptr[5 * nc + 1] * x[5] - lptr[4 * nc + 1] * x[4] - lptr[3 * nc + 1] * x[3] - lptr[2 * nc + 1] * x[2];
                        x[0] = b[0] - lptr[5 * nc + 0] * x[5] - lptr[4 * nc + 0] * x[4] - lptr[3 * nc + 0] * x[3] - lptr[2 * nc + 0] * x[2] - lptr[1 * nc + 0] * x[1];
                        return;
                    case 7:
                        x[6] = b[6];
                        x[5] = b[5] - lptr[6 * nc + 5] * x[6];
                        x[4] = b[4] - lptr[6 * nc + 4] * x[6] - lptr[5 * nc + 4] * x[5];
                        x[3] = b[3] - lptr[6 * nc + 3] * x[6] - lptr[5 * nc + 3] * x[5] - lptr[4 * nc + 3] * x[4];
                        x[2] = b[2] - lptr[6 * nc + 2] * x[6] - lptr[5 * nc + 2] * x[5] - lptr[4 * nc + 2] * x[4] - lptr[3 * nc + 2] * x[3];
                        x[1] = b[1] - lptr[6 * nc + 1] * x[6] - lptr[5 * nc + 1] * x[5] - lptr[4 * nc + 1] * x[4] - lptr[3 * nc + 1] * x[3] - lptr[2 * nc + 1] * x[2];
                        x[0] = b[0] - lptr[6 * nc + 0] * x[6] - lptr[5 * nc + 0] * x[5] - lptr[4 * nc + 0] * x[4] - lptr[3 * nc + 0] * x[3] - lptr[2 * nc + 0] * x[2] - lptr[1 * nc + 0] * x[1];
                        return;
                }
                return;
            }

            int i, j;
            register double s0, s1, s2, s3;
            float* xptr;

            lptr = L.ToFloatPtr() + n * nc + n - 4;
            xptr = x + n;

            // process 4 rows at a time
            for (i = n; i >= 4; i -= 4)
            {
                s0 = b[i - 4];
                s1 = b[i - 3];
                s2 = b[i - 2];
                s3 = b[i - 1];
                // process 4x4 blocks
                for (j = 0; j < n - i; j += 4)
                {
                    s0 -= lptr[(j + 0) * nc + 0] * xptr[j + 0];
                    s1 -= lptr[(j + 0) * nc + 1] * xptr[j + 0];
                    s2 -= lptr[(j + 0) * nc + 2] * xptr[j + 0];
                    s3 -= lptr[(j + 0) * nc + 3] * xptr[j + 0];
                    s0 -= lptr[(j + 1) * nc + 0] * xptr[j + 1];
                    s1 -= lptr[(j + 1) * nc + 1] * xptr[j + 1];
                    s2 -= lptr[(j + 1) * nc + 2] * xptr[j + 1];
                    s3 -= lptr[(j + 1) * nc + 3] * xptr[j + 1];
                    s0 -= lptr[(j + 2) * nc + 0] * xptr[j + 2];
                    s1 -= lptr[(j + 2) * nc + 1] * xptr[j + 2];
                    s2 -= lptr[(j + 2) * nc + 2] * xptr[j + 2];
                    s3 -= lptr[(j + 2) * nc + 3] * xptr[j + 2];
                    s0 -= lptr[(j + 3) * nc + 0] * xptr[j + 3];
                    s1 -= lptr[(j + 3) * nc + 1] * xptr[j + 3];
                    s2 -= lptr[(j + 3) * nc + 2] * xptr[j + 3];
                    s3 -= lptr[(j + 3) * nc + 3] * xptr[j + 3];
                }
                // process left over of the 4 rows
                s0 -= lptr[0 - 1 * nc] * s3;
                s1 -= lptr[1 - 1 * nc] * s3;
                s2 -= lptr[2 - 1 * nc] * s3;
                s0 -= lptr[0 - 2 * nc] * s2;
                s1 -= lptr[1 - 2 * nc] * s2;
                s0 -= lptr[0 - 3 * nc] * s1;
                // store result
                xptr[-4] = s0;
                xptr[-3] = s1;
                xptr[-2] = s2;
                xptr[-1] = s3;
                // update pointers for next four rows
                lptr -= 4 + 4 * nc;
                xptr -= 4;
            }
            // process left over rows
            for (i--; i >= 0; i--)
            {
                s0 = b[i];
                lptr = L[0] + i;
                for (j = i + 1; j < n; j++)
                {
                    s0 -= lptr[j * nc] * x[j];
                }
                x[i] = s0;
            }

#else

	int i, j, nc;
	const float *ptr;
	double sum;

	nc = L.GetNumColumns();
	for ( i = n - 1; i >= 0; i-- ) {
		sum = b[i];
		ptr = L[0] + i;
		for ( j = i + 1; j < n; j++ ) {
			sum -= ptr[j*nc] * x[j];
		}
		x[i] = sum;
	}

#endif
        }
        // in-place factorization LDL' of the n * n sub-matrix of mat the reciprocal of the diagonal elements are stored in invDiag
        public bool MatX_LDLTFactor(MatrixX mat, VectorX invDiag, int n)
        {
#if true

            int i, j, k, nc;
            float* v, *diag, *mptr;
            double s0, s1, s2, s3, sum, d;

            v = (float*)_alloca16(n * sizeof(float));
            diag = (float*)_alloca16(n * sizeof(float));

            nc = mat.GetNumColumns();

            if (n <= 0)
            {
                return true;
            }

            mptr = mat[0];

            sum = mptr[0];

            if (sum == 0.0f)
            {
                return false;
            }

            diag[0] = sum;
            invDiag[0] = d = 1.0f / sum;

            if (n <= 1)
            {
                return true;
            }

            mptr = mat[0];
            for (j = 1; j < n; j++)
            {
                mptr[j * nc + 0] = (mptr[j * nc + 0]) * d;
            }

            mptr = mat[1];

            v[0] = diag[0] * mptr[0]; s0 = v[0] * mptr[0];
            sum = mptr[1] - s0;

            if (sum == 0.0f)
            {
                return false;
            }

            mat[1][1] = sum;
            diag[1] = sum;
            invDiag[1] = d = 1.0f / sum;

            if (n <= 2)
            {
                return true;
            }

            mptr = mat[0];
            for (j = 2; j < n; j++)
            {
                mptr[j * nc + 1] = (mptr[j * nc + 1] - v[0] * mptr[j * nc + 0]) * d;
            }

            mptr = mat[2];

            v[0] = diag[0] * mptr[0]; s0 = v[0] * mptr[0];
            v[1] = diag[1] * mptr[1]; s1 = v[1] * mptr[1];
            sum = mptr[2] - s0 - s1;

            if (sum == 0.0f)
            {
                return false;
            }

            mat[2][2] = sum;
            diag[2] = sum;
            invDiag[2] = d = 1.0f / sum;

            if (n <= 3)
            {
                return true;
            }

            mptr = mat[0];
            for (j = 3; j < n; j++)
            {
                mptr[j * nc + 2] = (mptr[j * nc + 2] - v[0] * mptr[j * nc + 0] - v[1] * mptr[j * nc + 1]) * d;
            }

            mptr = mat[3];

            v[0] = diag[0] * mptr[0]; s0 = v[0] * mptr[0];
            v[1] = diag[1] * mptr[1]; s1 = v[1] * mptr[1];
            v[2] = diag[2] * mptr[2]; s2 = v[2] * mptr[2];
            sum = mptr[3] - s0 - s1 - s2;

            if (sum == 0.0f)
            {
                return false;
            }

            mat[3][3] = sum;
            diag[3] = sum;
            invDiag[3] = d = 1.0f / sum;

            if (n <= 4)
            {
                return true;
            }

            mptr = mat[0];
            for (j = 4; j < n; j++)
            {
                mptr[j * nc + 3] = (mptr[j * nc + 3] - v[0] * mptr[j * nc + 0] - v[1] * mptr[j * nc + 1] - v[2] * mptr[j * nc + 2]) * d;
            }

            for (i = 4; i < n; i++)
            {

                mptr = mat[i];

                v[0] = diag[0] * mptr[0]; s0 = v[0] * mptr[0];
                v[1] = diag[1] * mptr[1]; s1 = v[1] * mptr[1];
                v[2] = diag[2] * mptr[2]; s2 = v[2] * mptr[2];
                v[3] = diag[3] * mptr[3]; s3 = v[3] * mptr[3];
                for (k = 4; k < i - 3; k += 4)
                {
                    v[k + 0] = diag[k + 0] * mptr[k + 0]; s0 += v[k + 0] * mptr[k + 0];
                    v[k + 1] = diag[k + 1] * mptr[k + 1]; s1 += v[k + 1] * mptr[k + 1];
                    v[k + 2] = diag[k + 2] * mptr[k + 2]; s2 += v[k + 2] * mptr[k + 2];
                    v[k + 3] = diag[k + 3] * mptr[k + 3]; s3 += v[k + 3] * mptr[k + 3];
                }
                switch (i - k)
                {

                    default; Debug.Assert(false);
                    case 3: v[k + 2] = diag[k + 2] * mptr[k + 2]; s0 += v[k + 2] * mptr[k + 2];
                    case 2: v[k + 1] = diag[k + 1] * mptr[k + 1]; s1 += v[k + 1] * mptr[k + 1];
                    case 1: v[k + 0] = diag[k + 0] * mptr[k + 0]; s2 += v[k + 0] * mptr[k + 0];
                    case 0: break;
                }
                sum = s3;
                sum += s2;
                sum += s1;
                sum += s0;
                sum = mptr[i] - sum;

                if (sum == 0.0f)
                {
                    return false;
                }

                mat[i][i] = sum;
                diag[i] = sum;
                invDiag[i] = d = 1.0f / sum;

                if (i + 1 >= n)
                {
                    return true;
                }

                mptr = mat[i + 1];
                for (j = i + 1; j < n; j++)
                {
                    s0 = mptr[0] * v[0];
                    s1 = mptr[1] * v[1];
                    s2 = mptr[2] * v[2];
                    s3 = mptr[3] * v[3];
                    for (k = 4; k < i - 7; k += 8)
                    {
                        s0 += mptr[k + 0] * v[k + 0];
                        s1 += mptr[k + 1] * v[k + 1];
                        s2 += mptr[k + 2] * v[k + 2];
                        s3 += mptr[k + 3] * v[k + 3];
                        s0 += mptr[k + 4] * v[k + 4];
                        s1 += mptr[k + 5] * v[k + 5];
                        s2 += mptr[k + 6] * v[k + 6];
                        s3 += mptr[k + 7] * v[k + 7];
                    }
                    switch (i - k)
                    {
                        default; Debug.Assert(false);
                        case 7: s0 += mptr[k + 6] * v[k + 6];
                        case 6: s1 += mptr[k + 5] * v[k + 5];
                        case 5: s2 += mptr[k + 4] * v[k + 4];
                        case 4: s3 += mptr[k + 3] * v[k + 3];
                        case 3: s0 += mptr[k + 2] * v[k + 2];
                        case 2: s1 += mptr[k + 1] * v[k + 1];
                        case 1: s2 += mptr[k + 0] * v[k + 0];
                        case 0: break;
                    }
                    sum = s3;
                    sum += s2;
                    sum += s1;
                    sum += s0;
                    mptr[i] = (mptr[i] - sum) * d;
                    mptr += nc;
                }
            }

            return true;
#else

	int i, j, k, nc;
	float *v, *ptr, *diagPtr;
	double d, sum;

	v = (float *) _alloca16( n * sizeof( float ) );
	nc = mat.GetNumColumns();

	for ( i = 0; i < n; i++ ) {

		ptr = mat[i];
		diagPtr = mat[0];
		sum = ptr[i];
		for ( j = 0; j < i; j++ ) {
			d = ptr[j];
			v[j] = diagPtr[0] * d;
			sum -= v[j] * d;
			diagPtr += nc + 1;
		}

		if ( sum == 0.0f ) {
			return false;
		}

		diagPtr[0] = sum;
		invDiag[i] = d = 1.0f / sum;

		if ( i + 1 >= n ) {
			continue;
		}

		ptr = mat[i+1];
		for ( j = i + 1; j < n; j++ ) {
			sum = ptr[i];
			for ( k = 0; k < i; k++ ) {
				sum -= ptr[k] * v[k];
			}
			ptr[i] = sum * d;
			ptr += nc;
		}
	}

	return true;

#endif
        }

        public void BlendJoints(JointQuat* joints, JointQuat* blendJoints, float lerp, int* index, int numJoints);
        public void ConvertJointQuatsToJointMats(JointMat* jointMats, JointQuat* jointQuats, int numJoints);
        public void ConvertJointMatsToJointQuats(JointQuat* jointQuats, JointMat* jointMats, int numJoints);
        public void TransformJoints(JointMat* jointMats, int* parents, int firstJoint, int lastJoint);
        public void UntransformJoints(JointMat* jointMats, int* parents, int firstJoint, int lastJoint);
        public void TransformVerts(DrawVert* verts, int numVerts, JointMat* joints, idVec4* weights, int* index, int numWeights);
        public void TracePointCull(byte* cullBits, byte &totalOr, float radius, Plane* planes, DrawVert* verts, int numVerts);
        public void DecalPointCull(byte* cullBits, Plane* planes, DrawVert* verts, int numVerts);
        public void OverlayPointCull(byte* cullBits, Vector2* texCoords, Plane* planes, DrawVert* verts, int numVerts);
        public void DeriveTriPlanes(Plane* planes, DrawVert* verts, int numVerts, int* indexes, int numIndexes);
        public void DeriveTriPlanes(Plane* planes, DrawVert* verts, int numVerts, short* indexes, int numIndexes);
        public void DeriveTangents(Plane* planes, DrawVert* verts, int numVerts, int* indexes, int numIndexes);
        public void DeriveTangents(Plane* planes, DrawVert* verts, int numVerts, short* indexes, int numIndexes);
        public void DeriveUnsmoothedTangents(DrawVert* verts, dominantTri_s* dominantTris, int numVerts);
        public void NormalizeTangents(DrawVert* verts, int numVerts);
        public void CreateTextureSpaceLightVectors(Vector3* lightVectors, Vector3 lightOrigin, DrawVert* verts, int numVerts, int* indexes, int numIndexes);
        public void CreateSpecularTextureCoords(idVec4* texCoords, Vector3 lightOrigin, Vector3 viewOrigin, DrawVert* verts, int numVerts, int* indexes, int numIndexes);
        public int CreateShadowCache(idVec4* vertexCache, int* vertRemap, Vector3 lightOrigin, DrawVert* verts, int numVerts);
        public int CreateVertexProgramShadowCache(idVec4* vertexCache, DrawVert* verts, int numVerts);

        public void UpSamplePCMTo44kHz(float* dest, short* pcm, int numSamples, int kHz, int numChannels);
        public void UpSampleOGGTo44kHz(float* dest, float** ogg, int numSamples, int kHz, int numChannels);
        public void MixSoundTwoSpeakerMono(float* mixBuffer, float* samples, int numSamples, float lastV[2], float currentV[2] );
        public void MixSoundTwoSpeakerStereo(float* mixBuffer, float* samples, int numSamples, float lastV[2], float currentV[2] );
        public void MixSoundSixSpeakerMono(float* mixBuffer, float* samples, int numSamples, float lastV[6], float currentV[6] );
        public void MixSoundSixSpeakerStereo(float* mixBuffer, float* samples, int numSamples, float lastV[6], float currentV[6] );
        public void MixedSoundToSamples(short* samples, float* mixBuffer, int numSamples);
    }
}

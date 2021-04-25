#ifndef VMATH_H
#define VMATH_H

#include <math.h>
#include <stdbool.h>

typedef float vec_t;
typedef vec_t vec2_t[2];
typedef vec_t vec3_t[3];
typedef vec_t vec4_t[4];
typedef vec_t vec5_t[5];

typedef int fixed4_t;
typedef int fixed8_t;
typedef int fixed16_t;

#define cmatrix3x4 vec4_t *const
#define cmatrix4x4 vec4_t *const
typedef vec_t matrix3x4[3][4];
typedef vec_t matrix4x4[4][4];

// euler angle order
#define M_PITCH         0
#define M_YAW           1
#define M_ROLL          2

#ifndef M_PI
#define M_PI            (float)3.14159265358979323846
#endif
#ifndef M_PI2
#define M_PI2           (float)6.28318530717958647692
#endif
#define M_PIF           ((float)(M_PI))
#define M_PI2F          ((float)(M_PI2))

#define M_RAD2DEG(x)    ((float)(x) * (float)(180.f / M_PI))
#define M_DEG2RAD(x)    ((float)(x) * (float)(M_PI / 180.f))

#define SIDE_FRONT      0
#define SIDE_BACK       1
#define SIDE_ON         2
#define SIDE_CROSS      -2

#define PLANE_X         0    // 0 - 2 are axial planes
#define PLANE_Y         1    // 3 needs alternate calc
#define PLANE_Z         2
#define PLANE_NONAXIAL  3

#define EQUAL_EPSILON   0.001f
#define STOP_EPSILON    0.1f
#define ON_EPSILON      0.1f

#define M_RAD2STUDIO   (32768.0 / M_PI)
#define M_STUDIO2RAD   (M_PI / 32768.0)
#define M_NANMASK       (255<<23)

#define M_Between(min, a, max)          ((min)<(a)&&(a)<(max))
//#define M_Clamp(min, a, max)		    (M_ClampMin(M_ClampMax(a, max), min))
#define M_Clamp(min, a, max)            ((a)>=(min)?((a)<(max)?(a):(max)):(min))
#define M_ClampMax(a, max)              ((a)<(max)?(a):(max))
#define M_ClampMin(a, min)              ((a)>=(min)?(a):(min))
#define M_CrossProduct(a, b, _)         ((_)[0]=(a)[1]*(b)[2]-(a)[2]*(b)[1], (_)[1]=(a)[2]*(b)[0]-(a)[0]*(b)[2], (_)[2]=(a)[0]*(b)[1]-(a)[1]*(b)[0])
#define M_DotProduct(a, b)              ((a)[0]*(b)[0] + (a)[1]*(b)[1] + (a)[2]*(b)[2])
#define M_DotProductAbs(a, b)           (abs((a)[0]*(b)[0]) + abs((a)[1]*(b)[1]) + abs((a)[2]*(b)[2]))
#define M_DotProductFabs(a, b)          (fabs((a)[0]*(b)[0]) + fabs((a)[1]*(b)[1]) + fabs((a)[2]*(b)[2]))
#define M_IsNan(a)                      (((*(int*)&a)&M_NANMASK)==M_NANMASK)
#define M_Len(a, b)                     sqrtf(powf(a, 2.0f) + powf(b, 2.0f))
#define M_MakeRGBA(_, r, g, b, a)       M_Vector4Set(_, r, g, b, a)
#define M_Matrix3x4_Cpy(_, a)           memcpy(_, a, sizeof(matrix3x4))
#define M_Matrix3x4_LoadIdentity(a)     Matrix3x4_Cpy(a, Matrix3x4_identity)
#define M_Matrix4x4_Cpy(_, a)           memcpy(_, a, sizeof(matrix4x4))
#define M_Matrix4x4_LoadIdentity(a)     Matrix4x4_Cpy(a, Matrix4x4_identity)
#define M_PlaneDiff(point, plane)       (((plane)->type < 3?(point)[(plane)->type]:M_DotProduct((point),(plane)->normal))-(plane)->dist)
#define M_PlaneDist(point, plane)       ((plane)->type < 3?(point)[(plane)->type]:M_DotProduct((point),(plane)->normal))
#define M_Rint(a)                       ((a)<0?((int)((a)-0.5f)):((int)((a)+0.5f)))
#define M_Vector2Add(a, b, _)           ((_)[0]=(a)[0]+(b)[0], (_)[1]=(a)[1]+(b)[1])
#define M_Vector2Avg(a, b, _)           ((_)[0]=((a)[0]+(b)[0])*0.5, (_)[1]=((a)[1]+(b)[1])*0.5)
#define M_Vector2Cpy(a, _)              ((_)[0]=(a)[0], (_)[1]=(a)[1])
#define M_Vector2Dist(a, b)             (((a)[0]-(b)[0])*((a)[0]-(b)[0]) + ((a)[1]-(b)[1])*((a)[1]-(b)[1]) + ((a)[2]-(b)[2])*((a)[2]-(b)[2]))
#define M_Vector2IsNull(a)              ((a)[0]==0.0f && (a)[1]==0.0f)
#define M_Vector2Len(a)                 M_DotProduct(a,a)
#define M_Vector2Lerp(a, lerp, b, _)    ((_)[0]=(a)[0]+(lerp)*((b)[0]-(a)[0]), (_)[1]=(a)[1]+(lerp)*((b)[1]-(a)[1]))
#define M_Vector2Norm(a, _)             { float l=sqrtf(M_DotProduct(a,a)); if(l)l=1.0f/l; _[0]=a[0]*l; _[1]=a[1]*l; _[2]=a[2]*l; }
#define M_Vector2Set(_, a, b)           ((_)[0]=(a), (_)[1]=(b))
#define M_Vector2Sub(a, b, _)           ((_)[0]=(a)[0]-(b)[0], (_)[1]=(a)[1]-(b)[1])
#define M_Vector3Add(a, b, _)           ((_)[0]=(a)[0]+(b)[0], (_)[1]=(a)[1]+(b)[1], (_)[2]=(a)[2]+(b)[2])
#define M_Vector3Avg(a)                 (((a)[0] + (a)[1] + (a)[2]) / 3)
#define M_Vector3Avg2(a, b, _)          ((_)[0]=((a)[0]+(b)[0])*0.5, (_)[1]=((a)[1]+(b)[1])*0.5, (_)[2]=((a)[2]+(b)[2])*0.5)
#define M_Vector3Cmp(a, b)              ((a)[0]==(b)[0] && (a)[1]==(b)[1] && (a)[2]==(b)[2])
#define M_Vector3Cpy(a, _)              ((_)[0]=(a)[0], (_)[1]=(a)[1], (_)[2]=(a)[2])
#define M_Vector3Dist(a, b)             (sqrt(M_Vector2Dist(a,b)))
#define M_Vector3Div(a, d, _)           M_Vector3Scale(a,(1.0f/(d)),_)
#define M_Vector3IsNan(a)               (M_IsNan(a[0]) || M_IsNan(a[1]) || M_IsNan(a[2]))
#define M_Vector3IsNull(a)              ((a)[0]==0.0f && (a)[1]==0.0f && (a)[2]==0.0f)
#define M_Vector3Len(a)                 sqrtf(M_DotProduct(a,a))
#define M_Vector3Lerp(a, lerp, b, _)    ((_)[0]=(a)[0]+(lerp)*((b)[0]-(a)[0]), (_)[1]=(a)[1]+(lerp)*((b)[1]-(a)[1]), (_)[2]=(a)[2]+(lerp)*((b)[2]-(a)[2]))
#define M_Vector3M(f, a, _)             ((_)[0]=(a)[0]*(f), (_)[1]=(a)[1]*(f), (_)[2]=(a)[2]*(f))
#define M_Vector3MA(a, f, b, _)         ((_)[0]=(a)[0]+(b)[0]*(f), (_)[1]=(a)[1]+(b)[1]*(f), (_)[2]=(a)[2]+(b)[2]*(f))
#define M_Vector3MAMAM(fa, a, fb, b, fc, c, _) ((_)[0]=(a)[0]*(fa)+(b)[0]*(fb)+(c)[0]*(fc), (_)[1]=(a)[1]*(fa)+(b)[1]*(fb)+(c)[1]*(fc), (_)[2]=(a)[2]*(fa)+(b)[2]*(ba)+(c)[2]*(fc))
#define M_Vector3Max(a)                 (max((a)[0], max((a)[1], (a)[2])))
#define M_Vector3Neg(a, _)              ((_)[0]=-(a)[0], (_)[1]=-(a)[1], (_)[2]=-(a)[2])
#define M_Vector3Norm(_)                { float l=sqrtf(M_DotProduct(_,_)); if(l)l=1.0f/l; _[0]*=l; _[1]*=l; _[2]*= l; }
#define M_Vector3NormFast(_)            { float l=M::Rsqrt(M_DotProduct(_,_)); _[0]*=l; _[1]*=l; _[2]*=l; }
#define M_Vector3NormLen(_)             M::Vector2NormLen((_),(_))
#define M_Vector3Scale(a, f, _)         ((_)[0]=(a)[0]*(f), (_)[1]=(a)[1]*(f), (_)[2]=(a)[2]*(f))
#define M_Vector3Set(_, a, b, c)        ((_)[0]=(a), (_)[1]=(b), (_)[2]=(c))
#define M_Vector3Snap(_)                ((_)[0]=(int)(_)[0], (_)[1]=(int)(_)[1], (_)[2]=(int)(_)[2]))
#define M_Vector3Sub(a, b, _)           ((_)[0]=(a)[0]-(b)[0], (_)[1]=(a)[1]-(b)[1], (_)[2]=(a)[2]-(b)[2])
#define M_Vector3Zero(_)                ((_)[0]=(_)[1]=(_)[2]=0)
#define M_Vector4Cpy(a, _)              ((_)[0]=(a)[0], (_)[1]=(a)[1], (_)[2]=(a)[2], (_)[3]=(a)[3])
#define M_Vector4Set(_, a, b, c, d)     ((_)[0]=(a), (_)[1]=(b), (_)[2]=(c), (_)[3] = (d))

class M {
public:
    static float NonLinearFilter(float v);

    static void RotateAboutOrigin(float x, float y, float rotation, vec2_t _);

    //static float AngleMod(const float a);
    static void AnglesFromVectors(const vec3_t forward, const vec3_t right, const vec3_t up, vec3_t _);

    static void AnglesFromVectors2(const vec3_t forward, const vec3_t right, const vec3_t up, vec3_t _);

    static void AnglesInterpolate(vec3_t start, vec3_t end, vec3_t _, float frac);

    static void AnglesNorm(vec3_t angels);

    static void AnglesToVectors(const vec3_t angles, vec3_t forward, vec3_t right, vec3_t up);

    static void AnglesQuaternion(const vec3_t angles, vec4_t _);

    static float ApproachValue(float target, float value, float speed);

    //static void BoundsAddPoint(const vec3_t v, vec3_t mins, vec3_t maxs);
    static bool BoundsAndSphereIntersect(const vec3_t mins, const vec3_t maxs, const vec3_t origin, float radius);

    static bool BoundsIntersect(const vec3_t mins1, const vec3_t maxs1, const vec3_t mins2, const vec3_t maxs2);

    //static float BoundsRadius(const vec3_t mins, const vec3_t maxs);
    //static void BoundsZero(vec3_t mins, vec3_t maxs);
    static double ClockTimeInMilliSeconds();

    static unsigned short FloatToHalf(float value);

    static float HalfToFloat(unsigned short value);

    static void Matrix3x4_ConcatTransforms(matrix3x4 _, cmatrix3x4 m1, cmatrix3x4 m2);

    static void Matrix3x4_CreateFromEntity(matrix3x4 _, const vec3_t angles, const vec3_t origin, float scale);

    static void Matrix3x4_FromOriginQuat(matrix3x4 _, const vec4_t quaternion, const vec3_t origin);

    static const matrix3x4 Matrix3x4_Identity;

    static void Matrix3x4_InvertSimple(matrix3x4 _, cmatrix3x4 m);

    static void Matrix3x4_OriginFromMatrix(cmatrix3x4 m, float *_);

    static void Matrix3x4_SetOrigin(matrix3x4 _, float x, float y, float z);

    static void Matrix3x4_TransformPositivePlane(cmatrix3x4 m, const vec3_t normal, float d, vec3_t _, float *dist);

    static void Matrix3x4_VectorIRotate(cmatrix3x4 m, const float v[3], float _[3]);

    static void Matrix3x4_VectorITransform(cmatrix3x4 m, const float v[3], float _[3]);

    static void Matrix3x4_VectorRotate(cmatrix3x4 m, const float v[3], float _[3]);

    static void Matrix3x4_VectorTransform(cmatrix3x4 m, const float v[3], float _[3]);

    static void Matrix4x4_Concat(matrix4x4 _, const matrix4x4 m1, const matrix4x4 m2);

    static void Matrix4x4_ConcatTransforms(matrix4x4 _, cmatrix4x4 m1, cmatrix4x4 m2);

    static void Matrix4x4_ConvertToEntity(cmatrix4x4 m, vec3_t angles, vec3_t origin);

    static void Matrix4x4_CreateFromEntity(matrix4x4 _, const vec3_t angles, const vec3_t origin, float scale);

    static void Matrix4x4_CreateTranslate(matrix4x4 _, double x, double y, double z);

    static void Matrix4x4_FromOriginQuat(matrix4x4 _, const vec4_t quaternion, const vec3_t origin);

    static const matrix4x4 Matrix4x4_Identity;

    static bool Matrix4x4_InvertFull(matrix4x4 _, cmatrix4x4 m);

    static void Matrix4x4_InvertSimple(matrix4x4 _, cmatrix4x4 m);

    static void Matrix4x4_OriginFromMatrix(cmatrix4x4 mat, float *_);

    static void Matrix4x4_SetOrigin(matrix4x4 _, float x, float y, float z);

    static void Matrix4x4_TransformPositivePlane(cmatrix4x4 m, const vec3_t normal, float d, vec3_t _, float *dist);

    static void Matrix4x4_TransformStandardPlane(cmatrix4x4 m, const vec3_t normal, float d, vec3_t _, float *dist);

    static void Matrix4x4_Transpose(matrix4x4 _, cmatrix4x4 m);

    static void Matrix4x4_VectorIRotate(cmatrix4x4 m, const float v[3], float _[3]);

    static void Matrix4x4_VectorITransform(cmatrix4x4 m, const float v[3], float _[3]);

    static void Matrix4x4_VectorRotate(cmatrix4x4 m, const float v[3], float _[3]);

    static void Matrix4x4_VectorTransform(cmatrix4x4 m, const float v[3], float _[3]);

    static int NearestPow(int value, bool roundDown);

    //static int PlaneSignbits(const vec3_t normal);
    static void QuaternionSlerp(const vec4_t p, vec4_t q, float t, vec4_t _);

    static float RangeRemapValue(float value, float a, float b, float c, float d);

    static float Rsqrt(float value);

    static void SinCos(float radians, float *sin, float *cos);

#ifdef XASH_VECTORIZE_SINCOS
    static void SinCosFastVector2(float r1, float r2,
        float* s0, float* s1,
        float* c0, float* c1)
#if defined(__GNUC__)
        __attribute__((nonnull))
#endif
        ;
    static void SinCosFastVector3(float r1, float r2, float r3,
        float* s0, float* s1, float* s2,
        float* c0, float* c1, float* c2)
#if defined(__GNUC__)
        __attribute__((nonnull))
#endif
        ;
    static void SinCosFastVector4(float r1, float r2, float r3, float r4,
        float* s0, float* s1, float* s2, float* s3,
        float* c0, float* c1, float* c2, float* c3)
#if defined(__GNUC__)
        __attribute__((nonnull))
#endif
        ;
#endif

    static float Vector2NormLen(const vec3_t v, vec3_t out);

    static vec3_t Vector3_Origin;

    static void Vector3Angles(const float *forward, float *angles);

    static void Vector3Norm(vec3_t v);

    //static void Vector3RotateAroundPoint(vec3_t dst, const vec3_t dir, const vec3_t point, float degrees);
    static void Vectors(const vec3_t forward, vec3_t right, vec3_t up);
};

#endif // VMATH_H
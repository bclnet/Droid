#ifndef __SIMD_SSE3_H__
#define __SIMD_SSE3_H__

#include "Simd_SSE2.h"

#if defined(__GNUC__) && defined(__SSE3__)
const char* VPCALL SIMD_SSE3_GetName(void);
#elif defined(_MSC_VER) && defined(_M_IX86)
const char* VPCALL SIMD_SSE3_GetName(void);

void VPCALL SIMD_SSE3_TransformVerts(DrawVert* verts, const int numVerts, const JointMat* joints, const Vector4* weights, const int* index, const int numWeights);
#endif

#endif

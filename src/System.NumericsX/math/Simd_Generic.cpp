
#define UNROLL1(Y) { int _IX; for (_IX=0;_IX<count;_IX++) {Y(_IX);} }
#define UNROLL2(Y) { int _IX, _NM = count&0xfffffffe; for (_IX=0;_IX<_NM;_IX+=2){Y(_IX+0);Y(_IX+1);} if (_IX < count) {Y(_IX);}}
#define UNROLL4(Y) { int _IX, _NM = count&0xfffffffc; for (_IX=0;_IX<_NM;_IX+=4){Y(_IX+0);Y(_IX+1);Y(_IX+2);Y(_IX+3);}for(;_IX<count;_IX++){Y(_IX);}}
#define UNROLL8(Y) { int _IX, _NM = count&0xfffffff8; for (_IX=0;_IX<_NM;_IX+=8){Y(_IX+0);Y(_IX+1);Y(_IX+2);Y(_IX+3);Y(_IX+4);Y(_IX+5);Y(_IX+6);Y(_IX+7);} _NM = count&0xfffffffe; for(;_IX<_NM;_IX+=2){Y(_IX); Y(_IX+1);} if (_IX < count) {Y(_IX);} }



/*
============
idSIMD_Generic::BlendJoints
============
*/
void VPCALL idSIMD_Generic::BlendJoints( idJointQuat *joints, const idJointQuat *blendJoints, const float lerp, const int *index, const int numJoints ) {
	int i;

	for ( i = 0; i < numJoints; i++ ) {
		int j = index[i];
		joints[j].q.Slerp( joints[j].q, blendJoints[j].q, lerp );
		joints[j].t.Lerp( joints[j].t, blendJoints[j].t, lerp );
	}
}

/*
============
idSIMD_Generic::ConvertJointQuatsToJointMats
============
*/
void VPCALL idSIMD_Generic::ConvertJointQuatsToJointMats( idJointMat *jointMats, const idJointQuat *jointQuats, const int numJoints ) {
	int i;

	for ( i = 0; i < numJoints; i++ ) {
		jointMats[i].SetRotation( jointQuats[i].q.ToMat3() );
		jointMats[i].SetTranslation( jointQuats[i].t );
	}
}

/*
============
idSIMD_Generic::ConvertJointMatsToJointQuats
============
*/
void VPCALL idSIMD_Generic::ConvertJointMatsToJointQuats( idJointQuat *jointQuats, const idJointMat *jointMats, const int numJoints ) {
	int i;

	for ( i = 0; i < numJoints; i++ ) {
		jointQuats[i] = jointMats[i].ToJointQuat();
	}
}

/*
============
idSIMD_Generic::TransformJoints
============
*/
void VPCALL idSIMD_Generic::TransformJoints( idJointMat *jointMats, const int *parents, const int firstJoint, const int lastJoint ) {
	int i;

	for( i = firstJoint; i <= lastJoint; i++ ) {
		assert( parents[i] < i );
		jointMats[i] *= jointMats[parents[i]];
	}
}

/*
============
idSIMD_Generic::UntransformJoints
============
*/
void VPCALL idSIMD_Generic::UntransformJoints( idJointMat *jointMats, const int *parents, const int firstJoint, const int lastJoint ) {
	int i;

	for( i = lastJoint; i >= firstJoint; i-- ) {
		assert( parents[i] < i );
		jointMats[i] /= jointMats[parents[i]];
	}
}

/*
============
idSIMD_Generic::TransformVerts
============
*/
void VPCALL idSIMD_Generic::TransformVerts( idDrawVert *verts, const int numVerts, const idJointMat *joints, const idVec4 *weights, const int *index, int numWeights ) {
	int i, j;
	const byte *jointsPtr = (byte *)joints;

	for( j = i = 0; i < numVerts; i++ ) {
		idVec3 v;

		v = ( *(idJointMat *) ( jointsPtr + index[j*2+0] ) ) * weights[j];
		while( index[j*2+1] == 0 ) {
			j++;
			v += ( *(idJointMat *) ( jointsPtr + index[j*2+0] ) ) * weights[j];
		}
		j++;

		verts[i].xyz = v;
	}
}

/*
============
idSIMD_Generic::TracePointCull
============
*/
void VPCALL idSIMD_Generic::TracePointCull( byte *cullBits, byte &totalOr, const float radius, const idPlane *planes, const idDrawVert *verts, const int numVerts ) {
	int i;
	byte tOr;

	tOr = 0;

	for ( i = 0; i < numVerts; i++ ) {
		byte bits;
		float d0, d1, d2, d3, t;
		const idVec3 &v = verts[i].xyz;

		d0 = planes[0].Distance( v );
		d1 = planes[1].Distance( v );
		d2 = planes[2].Distance( v );
		d3 = planes[3].Distance( v );

		t = d0 + radius;
		bits  = FLOATSIGNBITSET( t ) << 0;
		t = d1 + radius;
		bits |= FLOATSIGNBITSET( t ) << 1;
		t = d2 + radius;
		bits |= FLOATSIGNBITSET( t ) << 2;
		t = d3 + radius;
		bits |= FLOATSIGNBITSET( t ) << 3;

		t = d0 - radius;
		bits |= FLOATSIGNBITSET( t ) << 4;
		t = d1 - radius;
		bits |= FLOATSIGNBITSET( t ) << 5;
		t = d2 - radius;
		bits |= FLOATSIGNBITSET( t ) << 6;
		t = d3 - radius;
		bits |= FLOATSIGNBITSET( t ) << 7;

		bits ^= 0x0F;		// flip lower four bits

		tOr |= bits;
		cullBits[i] = bits;
	}

	totalOr = tOr;
}

/*
============
idSIMD_Generic::DecalPointCull
============
*/
void VPCALL idSIMD_Generic::DecalPointCull( byte *cullBits, const idPlane *planes, const idDrawVert *verts, const int numVerts ) {
	int i;

	for ( i = 0; i < numVerts; i++ ) {
		byte bits;
		float d0, d1, d2, d3, d4, d5;
		const idVec3 &v = verts[i].xyz;

		d0 = planes[0].Distance( v );
		d1 = planes[1].Distance( v );
		d2 = planes[2].Distance( v );
		d3 = planes[3].Distance( v );
		d4 = planes[4].Distance( v );
		d5 = planes[5].Distance( v );

		bits  = FLOATSIGNBITSET( d0 ) << 0;
		bits |= FLOATSIGNBITSET( d1 ) << 1;
		bits |= FLOATSIGNBITSET( d2 ) << 2;
		bits |= FLOATSIGNBITSET( d3 ) << 3;
		bits |= FLOATSIGNBITSET( d4 ) << 4;
		bits |= FLOATSIGNBITSET( d5 ) << 5;

		cullBits[i] = bits ^ 0x3F;		// flip lower 6 bits
	}
}

/*
============
idSIMD_Generic::OverlayPointCull
============
*/
void VPCALL idSIMD_Generic::OverlayPointCull( byte *cullBits, idVec2 *texCoords, const idPlane *planes, const idDrawVert *verts, const int numVerts ) {
	int i;

	for ( i = 0; i < numVerts; i++ ) {
		byte bits;
		float d0, d1;
		const idVec3 &v = verts[i].xyz;

		texCoords[i][0] = d0 = planes[0].Distance( v );
		texCoords[i][1] = d1 = planes[1].Distance( v );

		bits  = FLOATSIGNBITSET( d0 ) << 0;
		d0 = 1.0f - d0;
		bits |= FLOATSIGNBITSET( d1 ) << 1;
		d1 = 1.0f - d1;
		bits |= FLOATSIGNBITSET( d0 ) << 2;
		bits |= FLOATSIGNBITSET( d1 ) << 3;

		cullBits[i] = bits;
	}
}

/*
============
idSIMD_Generic::DeriveTriPlanes

	Derives a plane equation for each triangle.
============
*/
void VPCALL idSIMD_Generic::DeriveTriPlanes( idPlane *planes, const idDrawVert *verts, const int numVerts, const int *indexes, const int numIndexes ) {
	int i;

	for ( i = 0; i < numIndexes; i += 3 ) {
		const idDrawVert *a, *b, *c;
		float d0[3], d1[3], f;
		idVec3 n;

		a = verts + indexes[i + 0];
		b = verts + indexes[i + 1];
		c = verts + indexes[i + 2];

		d0[0] = b->xyz[0] - a->xyz[0];
		d0[1] = b->xyz[1] - a->xyz[1];
		d0[2] = b->xyz[2] - a->xyz[2];

		d1[0] = c->xyz[0] - a->xyz[0];
		d1[1] = c->xyz[1] - a->xyz[1];
		d1[2] = c->xyz[2] - a->xyz[2];

		n[0] = d1[1] * d0[2] - d1[2] * d0[1];
		n[1] = d1[2] * d0[0] - d1[0] * d0[2];
		n[2] = d1[0] * d0[1] - d1[1] * d0[0];

		f = idMath::RSqrt( n.x * n.x + n.y * n.y + n.z * n.z );

		n.x *= f;
		n.y *= f;
		n.z *= f;

		planes->SetNormal( n );
		planes->FitThroughPoint( a->xyz );
		planes++;
	}
}


/*
============
idSIMD_Generic::DeriveTriPlanes

	Derives a plane equation for each triangle.
============
*/
void VPCALL
idSIMD_Generic::DeriveTriPlanes( idPlane
*planes,
const idDrawVert* verts,
const int numVerts,
const short* indexes,
const int numIndexes
) {
int i;

for (
i = 0;
i<numIndexes;
i += 3 ) {
const idDrawVert* a, * b, * c;
float d0[3], d1[3], f;
idVec3 n;

a = verts + indexes[i + 0];
b = verts + indexes[i + 1];
c = verts + indexes[i + 2];

d0[0] = b->xyz[0] - a->xyz[0];
d0[1] = b->xyz[1] - a->xyz[1];
d0[2] = b->xyz[2] - a->xyz[2];

d1[0] = c->xyz[0] - a->xyz[0];
d1[1] = c->xyz[1] - a->xyz[1];
d1[2] = c->xyz[2] - a->xyz[2];

n[0] = d1[1] * d0[2] - d1[2] * d0[1];
n[1] = d1[2] * d0[0] - d1[0] * d0[2];
n[2] = d1[0] * d0[1] - d1[1] * d0[0];

f = idMath::RSqrt(n.x * n.x + n.y * n.y + n.z * n.z);

n.x *=
f;
n.y *=
f;
n.z *=
f;

planes->
SetNormal( n );
planes->
FitThroughPoint( a
->xyz );
planes++;
}
}

/*
============
idSIMD_Generic::DeriveTangents

	Derives the normal and orthogonal tangent vectors for the triangle vertices.
	For each vertex the normal and tangent vectors are derived from all triangles
	using the vertex which results in smooth tangents across the mesh.
	In the process the triangle planes are calculated as well.
============
*/
void VPCALL idSIMD_Generic::DeriveTangents( idPlane *planes, idDrawVert *verts, const int numVerts, const int *indexes, const int numIndexes ) {
	int i;

	bool *used = (bool *)_alloca16( numVerts * sizeof( used[0] ) );
	memset( used, 0, numVerts * sizeof( used[0] ) );

	idPlane *planesPtr = planes;
	for ( i = 0; i < numIndexes; i += 3 ) {
		idDrawVert *a, *b, *c;
		unsigned int signBit;
		float d0[5], d1[5], f, area;
		idVec3 n, t0, t1;

		int v0 = indexes[i + 0];
		int v1 = indexes[i + 1];
		int v2 = indexes[i + 2];

		a = verts + v0;
		b = verts + v1;
		c = verts + v2;

		d0[0] = b->xyz[0] - a->xyz[0];
		d0[1] = b->xyz[1] - a->xyz[1];
		d0[2] = b->xyz[2] - a->xyz[2];
		d0[3] = b->st[0] - a->st[0];
		d0[4] = b->st[1] - a->st[1];

		d1[0] = c->xyz[0] - a->xyz[0];
		d1[1] = c->xyz[1] - a->xyz[1];
		d1[2] = c->xyz[2] - a->xyz[2];
		d1[3] = c->st[0] - a->st[0];
		d1[4] = c->st[1] - a->st[1];

		// normal
		n[0] = d1[1] * d0[2] - d1[2] * d0[1];
		n[1] = d1[2] * d0[0] - d1[0] * d0[2];
		n[2] = d1[0] * d0[1] - d1[1] * d0[0];

		f = idMath::RSqrt( n.x * n.x + n.y * n.y + n.z * n.z );

		n.x *= f;
		n.y *= f;
		n.z *= f;

		planesPtr->SetNormal( n );
		planesPtr->FitThroughPoint( a->xyz );
		planesPtr++;

		// area sign bit
		area = d0[3] * d1[4] - d0[4] * d1[3];
		signBit = ( *(unsigned int *)&area ) & ( 1 << 31 );

		// first tangent
		t0[0] = d0[0] * d1[4] - d0[4] * d1[0];
		t0[1] = d0[1] * d1[4] - d0[4] * d1[1];
		t0[2] = d0[2] * d1[4] - d0[4] * d1[2];

		f = idMath::RSqrt( t0.x * t0.x + t0.y * t0.y + t0.z * t0.z );
		*(unsigned int *)&f ^= signBit;

		t0.x *= f;
		t0.y *= f;
		t0.z *= f;

		// second tangent
		t1[0] = d0[3] * d1[0] - d0[0] * d1[3];
		t1[1] = d0[3] * d1[1] - d0[1] * d1[3];
		t1[2] = d0[3] * d1[2] - d0[2] * d1[3];

		f = idMath::RSqrt( t1.x * t1.x + t1.y * t1.y + t1.z * t1.z );
		*(unsigned int *)&f ^= signBit;

		t1.x *= f;
		t1.y *= f;
		t1.z *= f;

		if ( used[v0] ) {
			a->normal += n;
			a->tangents[0] += t0;
			a->tangents[1] += t1;
		} else {
			a->normal = n;
			a->tangents[0] = t0;
			a->tangents[1] = t1;
			used[v0] = true;
		}

		if ( used[v1] ) {
			b->normal += n;
			b->tangents[0] += t0;
			b->tangents[1] += t1;
		} else {
			b->normal = n;
			b->tangents[0] = t0;
			b->tangents[1] = t1;
			used[v1] = true;
		}

		if ( used[v2] ) {
			c->normal += n;
			c->tangents[0] += t0;
			c->tangents[1] += t1;
		} else {
			c->normal = n;
			c->tangents[0] = t0;
			c->tangents[1] = t1;
			used[v2] = true;
		}
	}
}


/*
============
idSIMD_Generic::DeriveTangents

	Derives the normal and orthogonal tangent vectors for the triangle vertices.
	For each vertex the normal and tangent vectors are derived from all triangles
	using the vertex which results in smooth tangents across the mesh.
	In the process the triangle planes are calculated as well.
============
*/
void VPCALL idSIMD_Generic::DeriveTangents( idPlane *planes, idDrawVert *verts, const int numVerts, const short *indexes, const int numIndexes ) {
int i;

bool *used = (bool *)_alloca16( numVerts * sizeof( used[0] ) );
memset( used, 0, numVerts * sizeof( used[0] ) );

idPlane *planesPtr = planes;
for ( i = 0; i < numIndexes; i += 3 ) {
idDrawVert *a, *b, *c;
unsigned int signBit;
float d0[5], d1[5], f, area;
idVec3 n, t0, t1;

int v0 = indexes[i + 0];
int v1 = indexes[i + 1];
int v2 = indexes[i + 2];

a = verts + v0;
b = verts + v1;
c = verts + v2;

d0[0] = b->xyz[0] - a->xyz[0];
d0[1] = b->xyz[1] - a->xyz[1];
d0[2] = b->xyz[2] - a->xyz[2];
d0[3] = b->st[0] - a->st[0];
d0[4] = b->st[1] - a->st[1];

d1[0] = c->xyz[0] - a->xyz[0];
d1[1] = c->xyz[1] - a->xyz[1];
d1[2] = c->xyz[2] - a->xyz[2];
d1[3] = c->st[0] - a->st[0];
d1[4] = c->st[1] - a->st[1];

// normal
n[0] = d1[1] * d0[2] - d1[2] * d0[1];
n[1] = d1[2] * d0[0] - d1[0] * d0[2];
n[2] = d1[0] * d0[1] - d1[1] * d0[0];

f = idMath::RSqrt( n.x * n.x + n.y * n.y + n.z * n.z );

n.x *= f;
n.y *= f;
n.z *= f;

planesPtr->SetNormal( n );
planesPtr->FitThroughPoint( a->xyz );
planesPtr++;

// area sign bit
area = d0[3] * d1[4] - d0[4] * d1[3];
signBit = ( *(unsigned int *)&area ) & ( 1 << 31 );

// first tangent
t0[0] = d0[0] * d1[4] - d0[4] * d1[0];
t0[1] = d0[1] * d1[4] - d0[4] * d1[1];
t0[2] = d0[2] * d1[4] - d0[4] * d1[2];

f = idMath::RSqrt( t0.x * t0.x + t0.y * t0.y + t0.z * t0.z );
*(unsigned int *)&f ^= signBit;

t0.x *= f;
t0.y *= f;
t0.z *= f;

// second tangent
t1[0] = d0[3] * d1[0] - d0[0] * d1[3];
t1[1] = d0[3] * d1[1] - d0[1] * d1[3];
t1[2] = d0[3] * d1[2] - d0[2] * d1[3];

f = idMath::RSqrt( t1.x * t1.x + t1.y * t1.y + t1.z * t1.z );
*(unsigned int *)&f ^= signBit;

t1.x *= f;
t1.y *= f;
t1.z *= f;

if ( used[v0] ) {
a->normal += n;
a->tangents[0] += t0;
a->tangents[1] += t1;
} else {
a->normal = n;
a->tangents[0] = t0;
a->tangents[1] = t1;
used[v0] = true;
}

if ( used[v1] ) {
b->normal += n;
b->tangents[0] += t0;
b->tangents[1] += t1;
} else {
b->normal = n;
b->tangents[0] = t0;
b->tangents[1] = t1;
used[v1] = true;
}

if ( used[v2] ) {
c->normal += n;
c->tangents[0] += t0;
c->tangents[1] += t1;
} else {
c->normal = n;
c->tangents[0] = t0;
c->tangents[1] = t1;
used[v2] = true;
}
}
}

/*
============
idSIMD_Generic::DeriveUnsmoothedTangents

	Derives the normal and orthogonal tangent vectors for the triangle vertices.
	For each vertex the normal and tangent vectors are derived from a single dominant triangle.
============
*/
#define DERIVE_UNSMOOTHED_BITANGENT

void VPCALL idSIMD_Generic::DeriveUnsmoothedTangents( idDrawVert *verts, const dominantTri_s *dominantTris, const int numVerts ) {
	int i;

	for ( i = 0; i < numVerts; i++ ) {
		idDrawVert *a, *b, *c;
#ifndef DERIVE_UNSMOOTHED_BITANGENT
		float d3, d8;
#endif
		float d0, d1, d2, d4;
		float d5, d6, d7, d9;
		float s0, s1, s2;
		float n0, n1, n2;
		float t0, t1, t2;
		float t3, t4, t5;

		const dominantTri_s &dt = dominantTris[i];

		a = verts + i;
		b = verts + dt.v2;
		c = verts + dt.v3;

		d0 = b->xyz[0] - a->xyz[0];
		d1 = b->xyz[1] - a->xyz[1];
		d2 = b->xyz[2] - a->xyz[2];
#ifndef DERIVE_UNSMOOTHED_BITANGENT
		d3 = b->st[0] - a->st[0];
#endif
		d4 = b->st[1] - a->st[1];

		d5 = c->xyz[0] - a->xyz[0];
		d6 = c->xyz[1] - a->xyz[1];
		d7 = c->xyz[2] - a->xyz[2];
#ifndef DERIVE_UNSMOOTHED_BITANGENT
		d8 = c->st[0] - a->st[0];
#endif
		d9 = c->st[1] - a->st[1];

		s0 = dt.normalizationScale[0];
		s1 = dt.normalizationScale[1];
		s2 = dt.normalizationScale[2];

		n0 = s2 * ( d6 * d2 - d7 * d1 );
		n1 = s2 * ( d7 * d0 - d5 * d2 );
		n2 = s2 * ( d5 * d1 - d6 * d0 );

		t0 = s0 * ( d0 * d9 - d4 * d5 );
		t1 = s0 * ( d1 * d9 - d4 * d6 );
		t2 = s0 * ( d2 * d9 - d4 * d7 );

#ifndef DERIVE_UNSMOOTHED_BITANGENT
		t3 = s1 * ( d3 * d5 - d0 * d8 );
		t4 = s1 * ( d3 * d6 - d1 * d8 );
		t5 = s1 * ( d3 * d7 - d2 * d8 );
#else
		t3 = s1 * ( n2 * t1 - n1 * t2 );
		t4 = s1 * ( n0 * t2 - n2 * t0 );
		t5 = s1 * ( n1 * t0 - n0 * t1 );
#endif

		a->normal[0] = n0;
		a->normal[1] = n1;
		a->normal[2] = n2;

		a->tangents[0][0] = t0;
		a->tangents[0][1] = t1;
		a->tangents[0][2] = t2;

		a->tangents[1][0] = t3;
		a->tangents[1][1] = t4;
		a->tangents[1][2] = t5;
	}
}

/*
============
idSIMD_Generic::NormalizeTangents

	Normalizes each vertex normal and projects and normalizes the
	tangent vectors onto the plane orthogonal to the vertex normal.
============
*/
void VPCALL idSIMD_Generic::NormalizeTangents( idDrawVert *verts, const int numVerts ) {

	for ( int i = 0; i < numVerts; i++ ) {
		idVec3 &v = verts[i].normal;
		float f;

		f = idMath::RSqrt( v.x * v.x + v.y * v.y + v.z * v.z );
		v.x *= f; v.y *= f; v.z *= f;

		for ( int j = 0; j < 2; j++ ) {
			idVec3 &t = verts[i].tangents[j];

			t -= ( t * v ) * v;
			f = idMath::RSqrt( t.x * t.x + t.y * t.y + t.z * t.z );
			t.x *= f; t.y *= f; t.z *= f;
		}
	}
}

/*
============
idSIMD_Generic::CreateTextureSpaceLightVectors

	Calculates light vectors in texture space for the given triangle vertices.
	For each vertex the direction towards the light origin is projected onto texture space.
	The light vectors are only calculated for the vertices referenced by the indexes.
============
*/
void VPCALL idSIMD_Generic::CreateTextureSpaceLightVectors( idVec3 *lightVectors, const idVec3 &lightOrigin, const idDrawVert *verts, const int numVerts, const int *indexes, const int numIndexes ) {

	bool *used = (bool *)_alloca16( numVerts * sizeof( used[0] ) );
	memset( used, 0, numVerts * sizeof( used[0] ) );

	for ( int i = numIndexes - 1; i >= 0; i-- ) {
		used[indexes[i]] = true;
	}

	for ( int i = 0; i < numVerts; i++ ) {
		if ( !used[i] ) {
			continue;
		}

		const idDrawVert *v = &verts[i];

		idVec3 lightDir = lightOrigin - v->xyz;

		lightVectors[i][0] = lightDir * v->tangents[0];
		lightVectors[i][1] = lightDir * v->tangents[1];
		lightVectors[i][2] = lightDir * v->normal;
	}
}

/*
============
idSIMD_Generic::CreateSpecularTextureCoords

	Calculates specular texture coordinates for the given triangle vertices.
	For each vertex the normalized direction towards the light origin is added to the
	normalized direction towards the view origin and the result is projected onto texture space.
	The texture coordinates are only calculated for the vertices referenced by the indexes.
============
*/
void VPCALL idSIMD_Generic::CreateSpecularTextureCoords( idVec4 *texCoords, const idVec3 &lightOrigin, const idVec3 &viewOrigin, const idDrawVert *verts, const int numVerts, const int *indexes, const int numIndexes ) {

	bool *used = (bool *)_alloca16( numVerts * sizeof( used[0] ) );
	memset( used, 0, numVerts * sizeof( used[0] ) );

	for ( int i = numIndexes - 1; i >= 0; i-- ) {
		used[indexes[i]] = true;
	}

	for ( int i = 0; i < numVerts; i++ ) {
		if ( !used[i] ) {
			continue;
		}

		const idDrawVert *v = &verts[i];

		idVec3 lightDir = lightOrigin - v->xyz;
		idVec3 viewDir = viewOrigin - v->xyz;

		float ilength;

		ilength = idMath::RSqrt( lightDir * lightDir );
		lightDir[0] *= ilength;
		lightDir[1] *= ilength;
		lightDir[2] *= ilength;

		ilength = idMath::RSqrt( viewDir * viewDir );
		viewDir[0] *= ilength;
		viewDir[1] *= ilength;
		viewDir[2] *= ilength;

		lightDir += viewDir;

		texCoords[i][0] = lightDir * v->tangents[0];
		texCoords[i][1] = lightDir * v->tangents[1];
		texCoords[i][2] = lightDir * v->normal;
		texCoords[i][3] = 1.0f;
	}
}

/*
============
idSIMD_Generic::CreateShadowCache
============
*/
int VPCALL idSIMD_Generic::CreateShadowCache( idVec4 *vertexCache, int *vertRemap, const idVec3 &lightOrigin, const idDrawVert *verts, const int numVerts ) {
	int outVerts = 0;

	for ( int i = 0; i < numVerts; i++ ) {
		if ( vertRemap[i] ) {
			continue;
		}
		const float *v = verts[i].xyz.ToFloatPtr();
		vertexCache[outVerts+0][0] = v[0];
		vertexCache[outVerts+0][1] = v[1];
		vertexCache[outVerts+0][2] = v[2];
		vertexCache[outVerts+0][3] = 1.0f;

		// R_SetupProjection() builds the projection matrix with a slight crunch
		// for depth, which keeps this w=0 division from rasterizing right at the
		// wrap around point and causing depth fighting with the rear caps
		vertexCache[outVerts+1][0] = v[0] - lightOrigin[0];
		vertexCache[outVerts+1][1] = v[1] - lightOrigin[1];
		vertexCache[outVerts+1][2] = v[2] - lightOrigin[2];
		vertexCache[outVerts+1][3] = 0.0f;
		vertRemap[i] = outVerts;
		outVerts += 2;
	}
	return outVerts;
}

/*
============
idSIMD_Generic::CreateVertexProgramShadowCache
============
*/
int VPCALL idSIMD_Generic::CreateVertexProgramShadowCache( idVec4 *vertexCache, const idDrawVert *verts, const int numVerts ) {
	for ( int i = 0; i < numVerts; i++ ) {
		const float *v = verts[i].xyz.ToFloatPtr();
		vertexCache[i*2+0][0] = v[0];
		vertexCache[i*2+1][0] = v[0];
		vertexCache[i*2+0][1] = v[1];
		vertexCache[i*2+1][1] = v[1];
		vertexCache[i*2+0][2] = v[2];
		vertexCache[i*2+1][2] = v[2];
		vertexCache[i*2+0][3] = 1.0f;
		vertexCache[i*2+1][3] = 0.0f;
	}
	return numVerts * 2;
}

/*
============
idSIMD_Generic::UpSamplePCMTo44kHz

  Duplicate samples for 44kHz output.
============
*/
void idSIMD_Generic::UpSamplePCMTo44kHz( float *dest, const short *src, const int numSamples, const int kHz, const int numChannels ) {
	if ( kHz == 11025 ) {
		if ( numChannels == 1 ) {
			for ( int i = 0; i < numSamples; i++ ) {
				dest[i*4+0] = dest[i*4+1] = dest[i*4+2] = dest[i*4+3] = (float) src[i+0];
			}
		} else {
			for ( int i = 0; i < numSamples; i += 2 ) {
				dest[i*4+0] = dest[i*4+2] = dest[i*4+4] = dest[i*4+6] = (float) src[i+0];
				dest[i*4+1] = dest[i*4+3] = dest[i*4+5] = dest[i*4+7] = (float) src[i+1];
			}
		}
	} else if ( kHz == 22050 ) {
		if ( numChannels == 1 ) {
			for ( int i = 0; i < numSamples; i++ ) {
				dest[i*2+0] = dest[i*2+1] = (float) src[i+0];
			}
		} else {
			for ( int i = 0; i < numSamples; i += 2 ) {
				dest[i*2+0] = dest[i*2+2] = (float) src[i+0];
				dest[i*2+1] = dest[i*2+3] = (float) src[i+1];
			}
		}
	} else if ( kHz == 44100 ) {
		for ( int i = 0; i < numSamples; i++ ) {
			dest[i] = (float) src[i];
		}
	} else {
		assert( 0 );
	}
}

/*
============
idSIMD_Generic::UpSampleOGGTo44kHz

  Duplicate samples for 44kHz output.
============
*/
void idSIMD_Generic::UpSampleOGGTo44kHz( float *dest, const float * const *ogg, const int numSamples, const int kHz, const int numChannels ) {
	if ( kHz == 11025 ) {
		if ( numChannels == 1 ) {
			for ( int i = 0; i < numSamples; i++ ) {
				dest[i*4+0] = dest[i*4+1] = dest[i*4+2] = dest[i*4+3] = ogg[0][i] * 32768.0f;
			}
		} else {
			for ( int i = 0; i < numSamples >> 1; i++ ) {
				dest[i*8+0] = dest[i*8+2] = dest[i*8+4] = dest[i*8+6] = ogg[0][i] * 32768.0f;
				dest[i*8+1] = dest[i*8+3] = dest[i*8+5] = dest[i*8+7] = ogg[1][i] * 32768.0f;
			}
		}
	} else if ( kHz == 22050 ) {
		if ( numChannels == 1 ) {
			for ( int i = 0; i < numSamples; i++ ) {
				dest[i*2+0] = dest[i*2+1] = ogg[0][i] * 32768.0f;
			}
		} else {
			for ( int i = 0; i < numSamples >> 1; i++ ) {
				dest[i*4+0] = dest[i*4+2] = ogg[0][i] * 32768.0f;
				dest[i*4+1] = dest[i*4+3] = ogg[1][i] * 32768.0f;
			}
		}
	} else if ( kHz == 44100 ) {
		if ( numChannels == 1 ) {
			for ( int i = 0; i < numSamples; i++ ) {
				dest[i*1+0] = ogg[0][i] * 32768.0f;
			}
		} else {
			for ( int i = 0; i < numSamples >> 1; i++ ) {
				dest[i*2+0] = ogg[0][i] * 32768.0f;
				dest[i*2+1] = ogg[1][i] * 32768.0f;
			}
		}
	} else {
		assert( 0 );
	}
}

/*
============
idSIMD_Generic::MixSoundTwoSpeakerMono
============
*/
void VPCALL idSIMD_Generic::MixSoundTwoSpeakerMono( float *mixBuffer, const float *samples, const int numSamples, const float lastV[2], const float currentV[2] ) {
	float sL = lastV[0];
	float sR = lastV[1];
	float incL = ( currentV[0] - lastV[0] ) / MIXBUFFER_SAMPLES;
	float incR = ( currentV[1] - lastV[1] ) / MIXBUFFER_SAMPLES;

	assert( numSamples == MIXBUFFER_SAMPLES );

	for( int j = 0; j < MIXBUFFER_SAMPLES; j++ ) {
		mixBuffer[j*2+0] += samples[j] * sL;
		mixBuffer[j*2+1] += samples[j] * sR;
		sL += incL;
		sR += incR;
	}
}

/*
============
idSIMD_Generic::MixSoundTwoSpeakerStereo
============
*/
void VPCALL idSIMD_Generic::MixSoundTwoSpeakerStereo( float *mixBuffer, const float *samples, const int numSamples, const float lastV[2], const float currentV[2] ) {
	float sL = lastV[0];
	float sR = lastV[1];
	float incL = ( currentV[0] - lastV[0] ) / MIXBUFFER_SAMPLES;
	float incR = ( currentV[1] - lastV[1] ) / MIXBUFFER_SAMPLES;

	assert( numSamples == MIXBUFFER_SAMPLES );

	for( int j = 0; j < MIXBUFFER_SAMPLES; j++ ) {
		mixBuffer[j*2+0] += samples[j*2+0] * sL;
		mixBuffer[j*2+1] += samples[j*2+1] * sR;
		sL += incL;
		sR += incR;
	}
}

/*
============
idSIMD_Generic::MixSoundSixSpeakerMono
============
*/
void VPCALL idSIMD_Generic::MixSoundSixSpeakerMono( float *mixBuffer, const float *samples, const int numSamples, const float lastV[6], const float currentV[6] ) {
	float sL0 = lastV[0];
	float sL1 = lastV[1];
	float sL2 = lastV[2];
	float sL3 = lastV[3];
	float sL4 = lastV[4];
	float sL5 = lastV[5];

	float incL0 = ( currentV[0] - lastV[0] ) / MIXBUFFER_SAMPLES;
	float incL1 = ( currentV[1] - lastV[1] ) / MIXBUFFER_SAMPLES;
	float incL2 = ( currentV[2] - lastV[2] ) / MIXBUFFER_SAMPLES;
	float incL3 = ( currentV[3] - lastV[3] ) / MIXBUFFER_SAMPLES;
	float incL4 = ( currentV[4] - lastV[4] ) / MIXBUFFER_SAMPLES;
	float incL5 = ( currentV[5] - lastV[5] ) / MIXBUFFER_SAMPLES;

	assert( numSamples == MIXBUFFER_SAMPLES );

	for( int i = 0; i < MIXBUFFER_SAMPLES; i++ ) {
		mixBuffer[i*6+0] += samples[i] * sL0;
		mixBuffer[i*6+1] += samples[i] * sL1;
		mixBuffer[i*6+2] += samples[i] * sL2;
		mixBuffer[i*6+3] += samples[i] * sL3;
		mixBuffer[i*6+4] += samples[i] * sL4;
		mixBuffer[i*6+5] += samples[i] * sL5;
		sL0 += incL0;
		sL1 += incL1;
		sL2 += incL2;
		sL3 += incL3;
		sL4 += incL4;
		sL5 += incL5;
	}
}

/*
============
idSIMD_Generic::MixSoundSixSpeakerStereo
============
*/
void VPCALL idSIMD_Generic::MixSoundSixSpeakerStereo( float *mixBuffer, const float *samples, const int numSamples, const float lastV[6], const float currentV[6] ) {
	float sL0 = lastV[0];
	float sL1 = lastV[1];
	float sL2 = lastV[2];
	float sL3 = lastV[3];
	float sL4 = lastV[4];
	float sL5 = lastV[5];

	float incL0 = ( currentV[0] - lastV[0] ) / MIXBUFFER_SAMPLES;
	float incL1 = ( currentV[1] - lastV[1] ) / MIXBUFFER_SAMPLES;
	float incL2 = ( currentV[2] - lastV[2] ) / MIXBUFFER_SAMPLES;
	float incL3 = ( currentV[3] - lastV[3] ) / MIXBUFFER_SAMPLES;
	float incL4 = ( currentV[4] - lastV[4] ) / MIXBUFFER_SAMPLES;
	float incL5 = ( currentV[5] - lastV[5] ) / MIXBUFFER_SAMPLES;

	assert( numSamples == MIXBUFFER_SAMPLES );

	for( int i = 0; i < MIXBUFFER_SAMPLES; i++ ) {
		mixBuffer[i*6+0] += samples[i*2+0] * sL0;
		mixBuffer[i*6+1] += samples[i*2+1] * sL1;
		mixBuffer[i*6+2] += samples[i*2+0] * sL2;
		mixBuffer[i*6+3] += samples[i*2+0] * sL3;
		mixBuffer[i*6+4] += samples[i*2+0] * sL4;
		mixBuffer[i*6+5] += samples[i*2+1] * sL5;
		sL0 += incL0;
		sL1 += incL1;
		sL2 += incL2;
		sL3 += incL3;
		sL4 += incL4;
		sL5 += incL5;
	}
}

/*
============
idSIMD_Generic::MixedSoundToSamples
============
*/
void VPCALL idSIMD_Generic::MixedSoundToSamples( short *samples, const float *mixBuffer, const int numSamples ) {

	for ( int i = 0; i < numSamples; i++ ) {
		if ( mixBuffer[i] <= -32768.0f ) {
			samples[i] = -32768;
		} else if ( mixBuffer[i] >= 32767.0f ) {
			samples[i] = 32767;
		} else {
			samples[i] = (short) mixBuffer[i];
		}
	}
}

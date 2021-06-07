using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Droid.Core.Plane;

namespace Droid.Core
{
    public unsafe struct SurfaceEdge
    {
        public fixed int verts[2];      // edge vertices always with ( verts[0] < verts[1] )
        public fixed int tris[2];       // edge triangles
    }

    public class Surface
    {
        protected List<DrawVert> verts = new();     // vertices
        protected List<int> indexes = new();        // 3 references to vertices for each triangle
        protected List<SurfaceEdge> edges = new();  // edges
        protected List<int> edgeIndexes = new();    // 3 references to edges for each triangle, may be negative for reversed edge

        public Surface() { }
        public Surface(Surface surf)
        {
            this.verts = surf.verts;
            this.indexes = surf.indexes;
            this.edges = surf.edges;
            this.edgeIndexes = surf.edgeIndexes;
        }
        public Surface(DrawVert[] verts, int numVerts, int[] indexes, int numIndexes)
        {
            Debug.Assert(verts != null && indexes != null && numVerts > 0 && numIndexes > 0);
            this.verts.SetNum(numVerts);
            memcpy(this.verts.Ptr(), verts, numVerts * sizeof(verts[0]));
            this.indexes.SetNum(numIndexes);
            memcpy(this.indexes.Ptr(), indexes, numIndexes * sizeof(indexes[0]));
            GenerateEdgeIndexes();
        }

        public DrawVert this[int index]
            => verts[index];
        public static Surface operator +(Surface _, Surface surf)
        {
            var n = _.verts.Count;
            var m = _.indexes.Count;
            _.verts.AddRange(surf.verts);           // merge verts where possible ?
            _.indexes.AddRange(surf.indexes);
            for (var i = m; i < _.indexes.Count; i++)
                _.indexes[i] += n;
            _.GenerateEdgeIndexes();
            return _;
        }

        public int NumIndexes
            => indexes.Count;

        public IList<int> GetIndexes
            => indexes;

        public int NumVertices
            => verts.Count;

        public IList<DrawVert> Vertices
            => verts;

        public IList<int> EdgeIndexes
            => edgeIndexes;

        public IList<SurfaceEdge> Edges
            => edges;

        public virtual void Clear()
        {
            verts.Clear();
            indexes.Clear();
            edges.Clear();
            edgeIndexes.Clear();
        }

        public void SwapTriangles(Surface surf)
        {
            UnsafeX.Swap(ref verts, ref surf.verts);
            UnsafeX.Swap(ref indexes, ref surf.indexes);
            UnsafeX.Swap(ref edges, ref surf.edges);
            UnsafeX.Swap(ref edgeIndexes, ref surf.edgeIndexes);
        }

        public void TranslateSelf(Vector3 translation)
        {
            for (var i = 0; i < verts.Count; i++)
                verts[i].xyz += translation;
        }

        public void RotateSelf(Matrix3x3 rotation)
        {
            for (var i = 0; i < verts.Count; i++)
            {
                verts[i].xyz *= rotation;
                verts[i].normal *= rotation;
                verts[i].tangents[0] *= rotation;
                verts[i].tangents[1] *= rotation;
            }
        }

        static int UpdateVertexIndex(int[] vertexIndexNum, int[] vertexRemap, int[] vertexCopyIndex, int vertNum)
        {
            var s = MathX.INTSIGNBITSET_(vertexRemap[vertNum]);
            vertexIndexNum[0] = vertexRemap[vertNum];
            vertexRemap[vertNum] = vertexIndexNum[s];
            vertexIndexNum[1] += s;
            vertexCopyIndex[vertexRemap[vertNum]] = vertNum;
            return vertexRemap[vertNum];
        }

        // splits the surface into a front and back surface, the surface itself stays unchanged
        // frontOnPlaneEdges and backOnPlaneEdges optionally store the indexes to the edges that lay on the split plane, returns a SIDE_?
        public unsafe int Split(Plane plane, float epsilon, out Surface front, out Surface back, int[] frontOnPlaneEdges = null, int[] backOnPlaneEdges = null)
        {
            float f;
            int* counts = stackalloc int[3];
            //int* edgeSplitVertex;
            int numEdgeSplitVertexes;
            int[][] vertexRemap = new int[2][];
            int[][] vertexIndexNum = new int[2][];
            int[][] vertexCopyIndex = new int[2][];
            int** indexPtr = stackalloc int*[2];
            int* indexNum = stackalloc int[2];
            int[] index;
            int[][] onPlaneEdges = new int[2][];
            int[] numOnPlaneEdges = new int[2];
            int maxOnPlaneEdges;
            int i;
            Surface surface_0, surface_1;
            DrawVert v = new();

            float* dists = stackalloc float[verts.Count];
            byte* sides = stackalloc byte[verts.Count];

            counts[0] = counts[1] = counts[2] = 0;

            // determine side for each vertex
            for (i = 0; i < verts.Count; i++)
            {
                dists[i] = f = plane.Distance(verts[i].xyz);
                if (f > epsilon) sides[i] = SIDE_FRONT;
                else if (f < -epsilon) sides[i] = SIDE_BACK;
                else sides[i] = SIDE_ON;
                counts[sides[i]]++;
            }

            front = back = null;

            // if coplanar, put on the front side if the normals match
            if (counts[SIDE_FRONT] == 0 && counts[SIDE_BACK] == 0)
            {
                f = (verts[indexes[1]].xyz - verts[indexes[0]].xyz).Cross(verts[indexes[0]].xyz - verts[indexes[2]].xyz) * plane.Normal;
                if (MathX.FLOATSIGNBITSET(f))
                {
                    back = new Surface(this);
                    return SIDE_BACK;
                }
                else
                {
                    front = new Surface(this);
                    return SIDE_FRONT;
                }
            }
            // if nothing at the front of the clipping plane
            if (counts[SIDE_FRONT] == 0)
            {
                back = new Surface(this);
                return SIDE_BACK;
            }
            // if nothing at the back of the clipping plane
            if (counts[SIDE_BACK] == 0)
            {
                front = new Surface(this);
                return SIDE_FRONT;
            }

            // allocate front and back surface
            front = surface_0 = new Surface();
            back = surface_1 = new Surface();

            var edgeSplitVertex = stackalloc int[edges.Count];
            numEdgeSplitVertexes = 0;

            maxOnPlaneEdges = 4 * counts[SIDE_ON];
            counts[SIDE_FRONT] = counts[SIDE_BACK] = counts[SIDE_ON] = 0;

            // split edges
            for (i = 0; i < edges.Count; i++)
            {
                var v0 = edges[i].verts[0];
                var v1 = edges[i].verts[1];
                var sidesOr = (sides[v0] | sides[v1]);

                // if both vertexes are on the same side or one is on the clipping plane
                if ((sides[v0] ^ sides[v1]) == 0 || (sidesOr & SIDE_ON) != 0)
                {
                    edgeSplitVertex[i] = -1;
                    counts[sidesOr & SIDE_BACK]++;
                    counts[SIDE_ON] += (sidesOr & SIDE_ON) >> 1;
                }
                else
                {
                    f = dists[v0] / (dists[v0] - dists[v1]);
                    v.LerpAll(verts[v0], verts[v1], f);
                    edgeSplitVertex[i] = numEdgeSplitVertexes++;
                    surface_0.verts.Add(v);
                    surface_1.verts.Add(v);
                }
            }

            // each edge is shared by at most two triangles, as such there can never be more indexes than twice the number of edges
            surface_0.indexes.Resize(((counts[SIDE_FRONT] + counts[SIDE_ON]) * 2) + (numEdgeSplitVertexes * 4));
            surface_1.indexes.Resize(((counts[SIDE_BACK] + counts[SIDE_ON]) * 2) + (numEdgeSplitVertexes * 4));

            // allocate indexes to construct the triangle indexes for the front and back surface
            vertexRemap[0] = new int[verts.Count];
            fixed (void* p = vertexRemap[0])
                unchecked { Unsafe.InitBlock(p, (byte)-1, (uint)verts.Count * sizeof(int)) };
            vertexRemap[1] = new int[verts.Count];
            fixed (void* p = vertexRemap[1])
                unchecked { Unsafe.InitBlock(p, (byte)-1, (uint)verts.Count * sizeof(int)); }

            vertexCopyIndex[0] = new int[numEdgeSplitVertexes + verts.Count];
            vertexCopyIndex[1] = new int[numEdgeSplitVertexes + verts.Count];

            vertexIndexNum[0][0] = vertexIndexNum[1][0] = 0;
            vertexIndexNum[0][1] = vertexIndexNum[1][1] = numEdgeSplitVertexes;

            indexPtr[0] = surface_0.indexes.Ptr();
            indexPtr[1] = surface_1.indexes.Ptr();
            indexNum[0] = surface_0.indexes.Count;
            indexNum[1] = surface_1.indexes.Count;

            maxOnPlaneEdges += 4 * numEdgeSplitVertexes;
            // allocate one more in case no triangles are actually split which may happen for a disconnected surface
            onPlaneEdges[0] = new int[maxOnPlaneEdges + 1];
            onPlaneEdges[1] = new int[maxOnPlaneEdges + 1];
            numOnPlaneEdges[0] = numOnPlaneEdges[1] = 0;

            // split surface triangles
            for (i = 0; i < edgeIndexes.Count; i += 3)
            {
                int s, n;

                var e0 = Math.Abs(edgeIndexes[i + 0]);
                var e1 = Math.Abs(edgeIndexes[i + 1]);
                var e2 = Math.Abs(edgeIndexes[i + 2]);

                var v0 = indexes[i + 0];
                var v1 = indexes[i + 1];
                var v2 = indexes[i + 2];

                switch ((MathX.INTSIGNBITSET_(edgeSplitVertex[e0]) | (MathX.INTSIGNBITSET_(edgeSplitVertex[e1]) << 1) | (MathX.INTSIGNBITSET_(edgeSplitVertex[e2]) << 2)) ^ 7)
                {
                    case 0:
                        {   // no edges split
                            if ((sides[v0] & sides[v1] & sides[v2] & SIDE_ON) != 0)
                            {
                                // coplanar
                                f = (verts[v1].xyz - verts[v0].xyz).Cross(verts[v0].xyz - verts[v2].xyz) * plane.Normal;
                                s = MathX.FLOATSIGNBITSET_(f);
                            }
                            else s = (sides[v0] | sides[v1] | sides[v2]) & SIDE_BACK;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]] = n;
                            numOnPlaneEdges[s] += (sides[v0] & sides[v1]) >> 1;
                            onPlaneEdges[s][numOnPlaneEdges[s]] = n + 1;
                            numOnPlaneEdges[s] += (sides[v1] & sides[v2]) >> 1;
                            onPlaneEdges[s][numOnPlaneEdges[s]] = n + 2;
                            numOnPlaneEdges[s] += (sides[v2] & sides[v0]) >> 1;
                            index = indexPtr[s];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v0);
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v1);
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v2);
                            indexNum[s] = n;
                            break;
                        }
                    case 1:
                        {   // first edge split
                            s = sides[v0] & SIDE_BACK;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]++] = n;
                            index = indexPtr[s];
                            index[n++] = edgeSplitVertex[e0];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v2);
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v0);
                            indexNum[s] = n;
                            s ^= 1;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]++] = n;
                            index = indexPtr[s];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v2);
                            index[n++] = edgeSplitVertex[e0];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v1);
                            indexNum[s] = n;
                            break;
                        }
                    case 2:
                        {   // second edge split
                            s = sides[v1] & SIDE_BACK;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]++] = n;
                            index = indexPtr[s];
                            index[n++] = edgeSplitVertex[e1];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v0);
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v1);
                            indexNum[s] = n;
                            s ^= 1;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]++] = n;
                            index = indexPtr[s];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v0);
                            index[n++] = edgeSplitVertex[e1];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v2);
                            indexNum[s] = n;
                            break;
                        }
                    case 3:
                        {   // first and second edge split
                            s = sides[v1] & SIDE_BACK;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]++] = n;
                            index = indexPtr[s];
                            index[n++] = edgeSplitVertex[e1];
                            index[n++] = edgeSplitVertex[e0];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v1);
                            indexNum[s] = n;
                            s ^= 1;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]++] = n;
                            index = indexPtr[s];
                            index[n++] = edgeSplitVertex[e0];
                            index[n++] = edgeSplitVertex[e1];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v0);
                            index[n++] = edgeSplitVertex[e1];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v2);
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v0);
                            indexNum[s] = n;
                            break;
                        }
                    case 4:
                        {   // third edge split
                            s = sides[v2] & SIDE_BACK;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]++] = n;
                            index = indexPtr[s];
                            index[n++] = edgeSplitVertex[e2];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v1);
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v2);
                            indexNum[s] = n;
                            s ^= 1;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]++] = n;
                            index = indexPtr[s];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v1);
                            index[n++] = edgeSplitVertex[e2];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v0);
                            indexNum[s] = n;
                            break;
                        }
                    case 5:
                        {   // first and third edge split
                            s = sides[v0] & SIDE_BACK;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]++] = n;
                            index = indexPtr[s];
                            index[n++] = edgeSplitVertex[e0];
                            index[n++] = edgeSplitVertex[e2];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v0);
                            indexNum[s] = n;
                            s ^= 1;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]++] = n;
                            index = indexPtr[s];
                            index[n++] = edgeSplitVertex[e2];
                            index[n++] = edgeSplitVertex[e0];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v1);
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v1);
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v2);
                            index[n++] = edgeSplitVertex[e2];
                            indexNum[s] = n;
                            break;
                        }
                    case 6:
                        {   // second and third edge split
                            s = sides[v2] & SIDE_BACK;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]++] = n;
                            index = indexPtr[s];
                            index[n++] = edgeSplitVertex[e2];
                            index[n++] = edgeSplitVertex[e1];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v2);
                            indexNum[s] = n;
                            s ^= 1;
                            n = indexNum[s];
                            onPlaneEdges[s][numOnPlaneEdges[s]++] = n;
                            index = indexPtr[s];
                            index[n++] = edgeSplitVertex[e1];
                            index[n++] = edgeSplitVertex[e2];
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v1);
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v0);
                            index[n++] = UpdateVertexIndex(vertexIndexNum[s], vertexRemap[s], vertexCopyIndex[s], v1);
                            index[n++] = edgeSplitVertex[e2];
                            indexNum[s] = n;
                            break;
                        }
                }
            }

            surface_0.indexes.SetNum(indexNum[0], false);
            surface_1.indexes.SetNum(indexNum[1], false);

            // copy vertexes
            surface_0.verts.SetNum(vertexIndexNum[0][1], false);
            index = vertexCopyIndex[0];
            for (i = numEdgeSplitVertexes; i < surface_0.verts.Count; i++)
                surface_0.verts[i] = verts[index[i]];
            surface_1.verts.SetNum(vertexIndexNum[1][1], false);
            index = vertexCopyIndex[1];
            for (i = numEdgeSplitVertexes; i < surface_1.verts.Count; i++)
                surface_1.verts[i] = verts[index[i]];

            // generate edge indexes
            surface_0.GenerateEdgeIndexes();
            surface_1.GenerateEdgeIndexes();

            if (frontOnPlaneEdges != null)
            {
                memcpy(frontOnPlaneEdges, onPlaneEdges[0], numOnPlaneEdges[0] * sizeof(int));
                frontOnPlaneEdges[numOnPlaneEdges[0]] = -1;
            }

            if (backOnPlaneEdges != null)
            {
                memcpy(backOnPlaneEdges, onPlaneEdges[1], numOnPlaneEdges[1] * sizeof(int));
                backOnPlaneEdges[numOnPlaneEdges[1]] = -1;
            }

            return SIDE_CROSS;
        }

        // cuts off the part at the back side of the plane, returns true if some part was at the front, if there is nothing at the front the number of points is set to zero
        public bool ClipInPlace(Plane plane, float epsilon = Plane.ON_EPSILON, bool keepOn = false)
        {
            float* dists;
            float f;
            byte* sides;
            int counts[3];
            int i;
            int* edgeSplitVertex;
            int* vertexRemap;
            int vertexIndexNum[2];
            int* vertexCopyIndex;
            int* indexPtr;
            int indexNum;
            int numEdgeSplitVertexes;
            idDrawVert v;
            idList<idDrawVert> newVerts;
            idList<int> newIndexes;

            dists = (float*)_alloca(verts.Num() * sizeof(float));
            sides = (byte*)_alloca(verts.Num() * sizeof(byte));

            counts[0] = counts[1] = counts[2] = 0;

            // determine side for each vertex
            for (i = 0; i < verts.Num(); i++)
            {
                dists[i] = f = plane.Distance(verts[i].xyz);
                if (f > epsilon)
                {
                    sides[i] = SIDE_FRONT;
                }
                else if (f < -epsilon)
                {
                    sides[i] = SIDE_BACK;
                }
                else
                {
                    sides[i] = SIDE_ON;
                }
                counts[sides[i]]++;
            }

            // if coplanar, put on the front side if the normals match
            if (!counts[SIDE_FRONT] && !counts[SIDE_BACK])
            {

                f = (verts[indexes[1]].xyz - verts[indexes[0]].xyz).Cross(verts[indexes[0]].xyz - verts[indexes[2]].xyz) * plane.Normal();
                if (FLOATSIGNBITSET(f))
                {
                    Clear();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            // if nothing at the front of the clipping plane
            if (!counts[SIDE_FRONT])
            {
                Clear();
                return false;
            }
            // if nothing at the back of the clipping plane
            if (!counts[SIDE_BACK])
            {
                return true;
            }

            edgeSplitVertex = (int*)_alloca(edges.Num() * sizeof(int));
            numEdgeSplitVertexes = 0;

            counts[SIDE_FRONT] = counts[SIDE_BACK] = 0;

            // split edges
            for (i = 0; i < edges.Num(); i++)
            {
                int v0 = edges[i].verts[0];
                int v1 = edges[i].verts[1];

                // if both vertexes are on the same side or one is on the clipping plane
                if (!(sides[v0] ^ sides[v1]) || ((sides[v0] | sides[v1]) & SIDE_ON))
                {
                    edgeSplitVertex[i] = -1;
                    counts[(sides[v0] | sides[v1]) & SIDE_BACK]++;
                }
                else
                {
                    f = dists[v0] / (dists[v0] - dists[v1]);
                    v.LerpAll(verts[v0], verts[v1], f);
                    edgeSplitVertex[i] = numEdgeSplitVertexes++;
                    newVerts.Append(v);
                }
            }

            // each edge is shared by at most two triangles, as such there can never be
            // more indexes than twice the number of edges
            newIndexes.Resize((counts[SIDE_FRONT] << 1) + (numEdgeSplitVertexes << 2));

            // allocate indexes to construct the triangle indexes for the front and back surface
            vertexRemap = (int*)_alloca(verts.Num() * sizeof(int));
            memset(vertexRemap, -1, verts.Num() * sizeof(int));

            vertexCopyIndex = (int*)_alloca((numEdgeSplitVertexes + verts.Num()) * sizeof(int));

            vertexIndexNum[0] = 0;
            vertexIndexNum[1] = numEdgeSplitVertexes;

            indexPtr = newIndexes.Ptr();
            indexNum = newIndexes.Num();

            // split surface triangles
            for (i = 0; i < edgeIndexes.Num(); i += 3)
            {
                int e0, e1, e2, v0, v1, v2;

                e0 = abs(edgeIndexes[i + 0]);
                e1 = abs(edgeIndexes[i + 1]);
                e2 = abs(edgeIndexes[i + 2]);

                v0 = indexes[i + 0];
                v1 = indexes[i + 1];
                v2 = indexes[i + 2];

                switch ((INTSIGNBITSET(edgeSplitVertex[e0]) | (INTSIGNBITSET(edgeSplitVertex[e1]) << 1) | (INTSIGNBITSET(edgeSplitVertex[e2]) << 2)) ^ 7)
                {
                    case 0:
                        {   // no edges split
                            if ((sides[v0] | sides[v1] | sides[v2]) & SIDE_BACK)
                            {
                                break;
                            }
                            if ((sides[v0] & sides[v1] & sides[v2]) & SIDE_ON)
                            {
                                // coplanar
                                if (!keepOn)
                                {
                                    break;
                                }
                                f = (verts[v1].xyz - verts[v0].xyz).Cross(verts[v0].xyz - verts[v2].xyz) * plane.Normal();
                                if (FLOATSIGNBITSET(f))
                                {
                                    break;
                                }
                            }
                            indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v0);
                            indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v1);
                            indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v2);
                            break;
                        }
                    case 1:
                        {   // first edge split
                            if (!(sides[v0] & SIDE_BACK))
                            {
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v0);
                                indexPtr[indexNum++] = edgeSplitVertex[e0];
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v2);
                            }
                            else
                            {
                                indexPtr[indexNum++] = edgeSplitVertex[e0];
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v1);
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v2);
                            }
                            break;
                        }
                    case 2:
                        {   // second edge split
                            if (!(sides[v1] & SIDE_BACK))
                            {
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v1);
                                indexPtr[indexNum++] = edgeSplitVertex[e1];
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v0);
                            }
                            else
                            {
                                indexPtr[indexNum++] = edgeSplitVertex[e1];
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v2);
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v0);
                            }
                            break;
                        }
                    case 3:
                        {   // first and second edge split
                            if (!(sides[v1] & SIDE_BACK))
                            {
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v1);
                                indexPtr[indexNum++] = edgeSplitVertex[e1];
                                indexPtr[indexNum++] = edgeSplitVertex[e0];
                            }
                            else
                            {
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v0);
                                indexPtr[indexNum++] = edgeSplitVertex[e0];
                                indexPtr[indexNum++] = edgeSplitVertex[e1];
                                indexPtr[indexNum++] = edgeSplitVertex[e1];
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v2);
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v0);
                            }
                            break;
                        }
                    case 4:
                        {   // third edge split
                            if (!(sides[v2] & SIDE_BACK))
                            {
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v2);
                                indexPtr[indexNum++] = edgeSplitVertex[e2];
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v1);
                            }
                            else
                            {
                                indexPtr[indexNum++] = edgeSplitVertex[e2];
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v0);
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v1);
                            }
                            break;
                        }
                    case 5:
                        {   // first and third edge split
                            if (!(sides[v0] & SIDE_BACK))
                            {
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v0);
                                indexPtr[indexNum++] = edgeSplitVertex[e0];
                                indexPtr[indexNum++] = edgeSplitVertex[e2];
                            }
                            else
                            {
                                indexPtr[indexNum++] = edgeSplitVertex[e0];
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v1);
                                indexPtr[indexNum++] = edgeSplitVertex[e2];
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v1);
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v2);
                                indexPtr[indexNum++] = edgeSplitVertex[e2];
                            }
                            break;
                        }
                    case 6:
                        {   // second and third edge split
                            if (!(sides[v2] & SIDE_BACK))
                            {
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v2);
                                indexPtr[indexNum++] = edgeSplitVertex[e2];
                                indexPtr[indexNum++] = edgeSplitVertex[e1];
                            }
                            else
                            {
                                indexPtr[indexNum++] = edgeSplitVertex[e2];
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v1);
                                indexPtr[indexNum++] = edgeSplitVertex[e1];
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v0);
                                indexPtr[indexNum++] = UpdateVertexIndex(vertexIndexNum, vertexRemap, vertexCopyIndex, v1);
                                indexPtr[indexNum++] = edgeSplitVertex[e2];
                            }
                            break;
                        }
                }
            }

            newIndexes.SetNum(indexNum, false);

            // copy vertexes
            newVerts.SetNum(vertexIndexNum[1], false);
            for (i = numEdgeSplitVertexes; i < newVerts.Num(); i++)
            {
                newVerts[i] = verts[vertexCopyIndex[i]];
            }

            // copy back to this surface
            indexes = newIndexes;
            verts = newVerts;

            GenerateEdgeIndexes();

            return true;
        }

        // returns true if each triangle can be reached from any other triangle by a traversal
        public unsafe bool IsConnected
        {
            get
            {
                int i, j;
                int queueStart, queueEnd;
                int curTri, nextTri, edgeNum;
                int* index;

                var numIslands = 0;
                var numTris = indexes.Count / 3;
                var islandNum = stackalloc int[numTris];
                unchecked { Unsafe.InitBlock((void*)islandNum, (byte)-1, (uint)numTris * sizeof(int)); }
                var queue = stackalloc int[numTris];

                for (i = 0; i < numTris; i++)
                {
                    if (islandNum[i] != -1)
                        continue;

                    queueStart = 0;
                    queueEnd = 1;
                    queue[0] = i;
                    islandNum[i] = numIslands;

                    for (curTri = queue[queueStart]; queueStart < queueEnd; curTri = queue[++queueStart])
                    {
                        index = edgeIndexes[curTri * 3];

                        for (j = 0; j < 3; j++)
                        {
                            edgeNum = index[j];
                            nextTri = edges[Math.Abs(edgeNum)].tris[MathX.INTSIGNBITNOTSET_(edgeNum)];

                            if (nextTri == -1)
                                continue;

                            nextTri /= 3;

                            if (islandNum[nextTri] != -1)
                                continue;

                            queue[queueEnd++] = nextTri;
                            islandNum[nextTri] = numIslands;
                        }
                    }
                    numIslands++;
                }

                return numIslands == 1;
            }
        }

        // returns true if the surface is closed
        public bool IsClosed
        {
            get
            {
                for (var i = 0; i < edges.Count; i++)
                    if (edges[i].tris[0] < 0 || edges[i].tris[1] < 0)
                        return false;
                return true;
            }
        }

        // returns true if the surface is a convex hull
        public bool IsPolytope(float epsilon = 0.1f)
        {
            if (!IsClosed)
                return false;

            int i, j; Plane plane = new();

            for (i = 0; i < indexes.Count; i += 3)
            {
                if (!plane.FromPoints(verts[indexes[i + 0]].xyz, verts[indexes[i + 1]].xyz, verts[indexes[i + 2]].xyz))
                    return false;

                for (j = 0; j < verts.Count; j++)
                    if (plane.Side(verts[j].xyz, epsilon) == SIDE_FRONT)
                        return false;
            }
            return true;
        }

        public float PlaneDistance(Plane plane)
        {
            var min = MathX.INFINITY;
            var max = -min;
            for (var i = 0; i < verts.Count; i++)
            {
                var d = plane.Distance(verts[i].xyz);
                if (d < min)
                {
                    min = d;
                    if (MathX.FLOATSIGNBITSET(min) & MathX.FLOATSIGNBITNOTSET(max))
                        return 0f;
                }
                if (d > max)
                {
                    max = d;
                    if (MathX.FLOATSIGNBITSET(min) & MathX.FLOATSIGNBITNOTSET(max))
                        return 0f;
                }
            }
            if (MathX.FLOATSIGNBITNOTSET(min)) return min;
            if (MathX.FLOATSIGNBITSET(max)) return max;
            return 0f;
        }

        public int PlaneSide(Plane plane, float epsilon = Plane.ON_EPSILON)
        {
            var front = false;
            var back = false;
            for (var i = 0; i < verts.Count; i++)
            {
                var d = plane.Distance(verts[i].xyz);
                if (d < -epsilon)
                {
                    if (front)
                        return SIDE_CROSS;
                    back = true;
                    continue;
                }
                else if (d > epsilon)
                {
                    if (back)
                        return SIDE_CROSS;
                    front = true;
                    continue;
                }
            }

            if (back) return SIDE_BACK;
            if (front) return SIDE_FRONT;
            return SIDE_ON;
        }

        // returns true if the line intersects one of the surface triangles
        public bool LineIntersection(Vector3 start, Vector3 end, bool backFaceCull = false)
        {
            RayIntersection(start, end - start, out var scale, false);
            return scale >= 0f && scale <= 1f;
        }

        // intersection point is start + dir * scale
        public unsafe bool RayIntersection(Vector3 start, Vector3 dir, out float scale, bool backFaceCull = false)
        {
            int i, i0, i1, i2, s0, s1, s2; float d, s = 0f;
            Pluecker rayPl = new(), pl = new();
            Plane plane = new();

            var sidedness = stackalloc byte[edges.Count];
            scale = MathX.INFINITY;

            rayPl.FromRay(start, dir);

            // ray sidedness for edges
            for (i = 0; i < edges.Count; i++)
            {
                pl.FromLine(verts[edges[i].verts[1]].xyz, verts[edges[i].verts[0]].xyz);
                d = pl.PermutedInnerProduct(rayPl);
                sidedness[i] = (byte)MathX.FLOATSIGNBITSET_(d);
            }

            // test triangles
            for (i = 0; i < edgeIndexes.Count; i += 3)
            {
                i0 = edgeIndexes[i + 0];
                i1 = edgeIndexes[i + 1];
                i2 = edgeIndexes[i + 2];
                s0 = sidedness[Math.Abs(i0)] ^ MathX.INTSIGNBITSET_(i0);
                s1 = sidedness[Math.Abs(i1)] ^ MathX.INTSIGNBITSET_(i1);
                s2 = sidedness[Math.Abs(i2)] ^ MathX.INTSIGNBITSET_(i2);

                if ((s0 & s1 & s2) != 0)
                {
                    if (!plane.FromPoints(verts[indexes[i + 0]].xyz, verts[indexes[i + 1]].xyz, verts[indexes[i + 2]].xyz))
                        return false;
                    plane.RayIntersection(start, dir, s);
                    if (MathX.Fabs(s) < MathX.Fabs(scale))
                        scale = s;
                }
                else if (!backFaceCull && (s0 | s1 | s2) == 0)
                {
                    if (!plane.FromPoints(verts[indexes[i + 0]].xyz, verts[indexes[i + 1]].xyz, verts[indexes[i + 2]].xyz))
                        return false;
                    plane.RayIntersection(start, dir, s);
                    if (MathX.Fabs(s) < MathX.Fabs(scale))
                        scale = s;
                }
            }

            if (MathX.Fabs(scale) < MathX.INFINITY)
                return true;
            return false;
        }

        // Assumes each edge is shared by at most two triangles.
        protected unsafe void GenerateEdgeIndexes()
        {
            int i, j, i0, i1, i2, s, v0, v1, edgeNum;
            int* index;
            var e = stackalloc SurfaceEdge[3];

            var vertexEdges = stackalloc int[verts.Count];
            unchecked { Unsafe.InitBlock((void*)vertexEdges, (byte)-1, (uint)verts.Count * sizeof(int)); }
            var edgeChain = stackalloc int[indexes.Count];

            edgeIndexes.SetNum(indexes.Count, true);

            edges.Clear();

            // the first edge is a dummy
            e[0].verts[0] = e[0].verts[1] = e[0].tris[0] = e[0].tris[1] = 0;
            edges.Add(e[0]);

            for (i = 0; i < indexes.Count; i += 3)
            {
                index = indexes.Ptr() + i;
                // vertex numbers
                i0 = index[0];
                i1 = index[1];
                i2 = index[2];
                // setup edges each with smallest vertex number first
                s = MathX.INTSIGNBITSET_(i1 - i0);
                e[0].verts[0] = index[s];
                e[0].verts[1] = index[s ^ 1];
                s = MathX.INTSIGNBITSET_(i2 - i1) + 1;
                e[1].verts[0] = index[s];
                e[1].verts[1] = index[s ^ 3];
                s = MathX.INTSIGNBITSET_(i2 - i0) << 1;
                e[2].verts[0] = index[s];
                e[2].verts[1] = index[s ^ 2];
                // get edges
                for (j = 0; j < 3; j++)
                {
                    v0 = e[j].verts[0];
                    v1 = e[j].verts[1];
                    for (edgeNum = vertexEdges[v0]; edgeNum >= 0; edgeNum = edgeChain[edgeNum])
                        if (edges[edgeNum].verts[1] == v1)
                            break;
                    // if the edge does not yet exist
                    if (edgeNum < 0)
                    {
                        e[j].tris[0] = e[j].tris[1] = -1;
                        edgeNum = edges.Add(e[j]);
                        edgeChain[edgeNum] = vertexEdges[v0];
                        vertexEdges[v0] = edgeNum;
                    }
                    // update edge index and edge tri references
                    if (index[j] == v0)
                    {
                        Debug.Assert(edges[edgeNum].tris[0] == -1); // edge may not be shared by more than two triangles
                        edges[edgeNum].tris[0] = i;
                        edgeIndexes[i + j] = edgeNum;
                    }
                    else
                    {
                        Debug.Assert(edges[edgeNum].tris[1] == -1); // edge may not be shared by more than two triangles
                        edges[edgeNum].tris[1] = i;
                        edgeIndexes[i + j] = -edgeNum;
                    }
                }
            }
        }

        protected int FindEdge(int v1, int v2)
        {
            int i, firstVert, secondVert;

            if (v1 < v2)
            {
                firstVert = v1;
                secondVert = v2;
            }
            else
            {
                firstVert = v2;
                secondVert = v1;
            }
            for (i = 1; i < edges.Count; i++)
                if (edges[i].verts[0] == firstVert)
                    if (edges[i].verts[1] == secondVert)
                        break;
            if (i < edges.Count)
                return v1 < v2 ? i : -i;
            return 0;
        }

    }
}
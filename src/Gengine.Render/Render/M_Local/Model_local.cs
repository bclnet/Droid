using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.NumericsX;
using System.Runtime.InteropServices;
using static System.NumericsX.OpenStack.OpenStack;

namespace Gengine.Render
{
    public class RenderModelStatic : IRenderModel
    {
        // the inherited public interface
        public static IRenderModel Alloc();

        public RenderModelStatic();

        public virtual void InitFromFile(string fileName);
        public virtual void PartialInitFromFile(string fileName);
        public virtual void PurgeModel();
        public virtual void Reset() { }
        public virtual void LoadModel();
        public virtual bool IsLoaded { get; }
        public virtual void SetLevelLoadReferenced(bool referenced);
        public virtual bool IsLevelLoadReferenced();
        public virtual void TouchData();
        public virtual void InitEmpty(string name);
        public virtual void AddSurface(ModelSurface surface);
        public virtual void FinishSurfaces();
        public virtual void FreeVertexCache();
        public virtual string Name();
        public virtual void Print();
        public virtual void List();
        public virtual int Memory();
        public virtual DateTime Timestamp();
        public virtual int NumSurfaces();
        public virtual int NumBaseSurfaces();
        public virtual ModelSurface Surface(int surfaceNum);
        public virtual SrfTriangles AllocSurfaceTriangles(int numVerts, int numIndexes);
        public virtual void FreeSurfaceTriangles(ref SrfTriangles tris);
        public virtual SrfTriangles ShadowHull();
        public virtual bool IsStaticWorldModel();
        public virtual DynamicModel IsDynamicModel { get; }
        public virtual bool IsDefaultModel();
        public virtual bool IsReloadable();
        public virtual IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel);
        public virtual int NumJoints();
        public virtual MD5Joint[] GetJoints();
        public virtual JointHandle GetJointHandle(string name);
        public virtual string GetJointName(JointHandle handle);
        public virtual JointQuat GetDefaultPose();
        public virtual int NearestJoint(int surfaceNum, int a, int b, int c);
        public virtual Bounds Bounds(RenderEntity ent);
        public virtual void ReadFromDemoFile(VFileDemo f);
        public virtual void WriteToDemoFile(VFileDemo f);
        public virtual float DepthHack();

        public void MakeDefaultModel();

        public bool LoadASE(string fileName);
        public bool LoadLWO(string fileName);
        public bool LoadFLT(string fileName);
        public bool LoadMA(string filename);

        public bool ConvertASEToModelSurfaces(AseModel ase);
        public bool ConvertLWOToModelSurfaces(St_lwObject lwo);
        public bool ConvertMAToModelSurfaces(MaModel ma);

        public AseModel ConvertLWOToASE(St_lwObject obj, string fileName);

        public bool DeleteSurfaceWithId(int id);
        public void DeleteSurfacesWithNegativeId();
        public bool FindSurfaceWithId(int id, out int surfaceNum);

        public List<ModelSurface> surfaces;
        public Bounds bounds;
        public int overlaysAdded;

        protected int lastModifiedFrame;
        protected int lastArchivedFrame;

        protected string name;
        protected SrfTriangles shadowHull;
        protected bool isStaticWorldModel;
        protected bool defaulted;
        protected bool purged;                  // eventually we will have dynamic reloading
        protected bool fastLoad;                // don't generate tangents and shadow data
        protected bool reloadable;              // if not, reloadModels won't check timestamp
        protected bool levelLoadReferenced; // for determining if it needs to be freed
        protected DateTime timeStamp;

        protected static CVar r_mergeModelSurfaces;   // combine model surfaces with the same material
        protected static CVar r_slopVertex;           // merge xyz coordinates this far apart
        protected static CVar r_slopTexCoord;         // merge texture coordinates this far apart
        protected static CVar r_slopNormal;           // merge normals that dot less than this
    }

    public class MD5Mesh
    {
        const string MD5_SnapshotName = "_MD5_Snapshot_";

        static int c_numVerts = 0;
        static int c_numWeights = 0;
        static int c_numWeightJoints = 0;

        struct VertexWeight
        {
            public int vert;
            public int joint;
            public Vector3 offset;
            public float jointWeight;
        }

        List<Vector2> texCoords;           // texture coordinates
        int numWeights;         // number of weights
        Vector4[] scaledWeights;      // joint weights
        int[] weightIndex;       // pairs of: joint offset + bool true if next weight is for next vertex
        Material shader;               // material applied to mesh
        int numTris;            // number of triangles
        DeformInfo deformInfo;          // used to create srfTriangles_t from base frames and new vertexes
        int surfaceNum;         // number of the static surface created for this mesh

        public MD5Mesh()
        {
            scaledWeights = null;
            weightIndex = null;
            shader = null;
            numTris = 0;
            deformInfo = null;
            surfaceNum = 0;
        }

        public void ParseMesh(Lexer parser, int numJoints, JointMat[] joints)
        {
            idToken token;
            idToken name;
            int num;
            int count;
            int jointnum;
            idStr shaderName;
            int i, j;
            idList<int> tris;
            idList<int> firstWeightForVertex;
            idList<int> numWeightsForVertex;
            int maxweight;
            idList<vertexWeight_t> tempWeights;

            parser.ExpectTokenString("{");

            //
            // parse name
            //
            if (parser.CheckTokenString("name"))
            {
                parser.ReadToken(&name);
            }

            //
            // parse shader
            //
            parser.ExpectTokenString("shader");

            parser.ReadToken(&token);
            shaderName = token;

            shader = declManager.FindMaterial(shaderName);

            //
            // parse texture coordinates
            //
            parser.ExpectTokenString("numverts");
            count = parser.ParseInt();
            if (count < 0)
            {
                parser.Error("Invalid size: %s", token.c_str());
            }

            texCoords.SetNum(count);
            firstWeightForVertex.SetNum(count);
            numWeightsForVertex.SetNum(count);

            numWeights = 0;
            maxweight = 0;
            for (i = 0; i < texCoords.Num(); i++)
            {
                parser.ExpectTokenString("vert");
                parser.ParseInt();

                parser.Parse1DMatrix(2, texCoords[i].ToFloatPtr());

                firstWeightForVertex[i] = parser.ParseInt();
                numWeightsForVertex[i] = parser.ParseInt();

                if (!numWeightsForVertex[i])
                {
                    parser.Error("Vertex without any joint weights.");
                }

                numWeights += numWeightsForVertex[i];
                if (numWeightsForVertex[i] + firstWeightForVertex[i] > maxweight)
                {
                    maxweight = numWeightsForVertex[i] + firstWeightForVertex[i];
                }
            }

            //
            // parse tris
            //
            parser.ExpectTokenString("numtris");
            count = parser.ParseInt();
            if (count < 0)
            {
                parser.Error("Invalid size: %d", count);
            }

            tris.SetNum(count * 3);
            numTris = count;
            for (i = 0; i < count; i++)
            {
                parser.ExpectTokenString("tri");
                parser.ParseInt();

                tris[i * 3 + 0] = parser.ParseInt();
                tris[i * 3 + 1] = parser.ParseInt();
                tris[i * 3 + 2] = parser.ParseInt();
            }

            //
            // parse weights
            //
            parser.ExpectTokenString("numweights");
            count = parser.ParseInt();
            if (count < 0)
            {
                parser.Error("Invalid size: %d", count);
            }

            if (maxweight > count)
            {
                parser.Warning("Vertices reference out of range weights in model (%d of %d weights).", maxweight, count);
            }

            tempWeights.SetNum(count);

            for (i = 0; i < count; i++)
            {
                parser.ExpectTokenString("weight");
                parser.ParseInt();

                jointnum = parser.ParseInt();
                if ((jointnum < 0) || (jointnum >= numJoints))
                {
                    parser.Error("Joint Index out of range(%d): %d", numJoints, jointnum);
                }

                tempWeights[i].joint = jointnum;
                tempWeights[i].jointWeight = parser.ParseFloat();

                parser.Parse1DMatrix(3, tempWeights[i].offset.ToFloatPtr());
            }

            // create pre-scaled weights and an index for the vertex/joint lookup
            scaledWeights = (idVec4*)Mem_Alloc16(numWeights * sizeof(scaledWeights[0]));
            weightIndex = (int*)Mem_Alloc16(numWeights * 2 * sizeof(weightIndex[0]));
            memset(weightIndex, 0, numWeights * 2 * sizeof(weightIndex[0]));

            count = 0;
            for (i = 0; i < texCoords.Num(); i++)
            {
                num = firstWeightForVertex[i];
                for (j = 0; j < numWeightsForVertex[i]; j++, num++, count++)
                {
                    scaledWeights[count].ToVec3() = tempWeights[num].offset * tempWeights[num].jointWeight;
                    scaledWeights[count].w = tempWeights[num].jointWeight;
                    weightIndex[count * 2 + 0] = tempWeights[num].joint * sizeof(idJointMat);
                }
                weightIndex[count * 2 - 1] = 1;
            }

            tempWeights.Clear();
            numWeightsForVertex.Clear();
            firstWeightForVertex.Clear();

            parser.ExpectTokenString("}");

            // update counters
            c_numVerts += texCoords.Num();
            c_numWeights += numWeights;
            c_numWeightJoints++;
            for (i = 0; i < numWeights; i++)
            {
                c_numWeightJoints += weightIndex[i * 2 + 1];
            }

            //
            // build the information that will be common to all animations of this mesh:
            // silhouette edge connectivity and normal / tangent generation information
            //
            //GB Check there are not too many verts
            // DG: windows only has a 1MB stack and it could happen that we try to allocate >1MB here
            //     (in lost mission mod, game/le_hell map), causing a stack overflow
            //     to prevent that, use heap allocation if it's >600KB
            size_t allocaSize = texCoords.Num() * sizeof(idDrawVert);
            idDrawVert* verts;
            if (allocaSize < 600000)
                verts = (idDrawVert*)_alloca16(allocaSize);

            else
                verts = (idDrawVert*)Mem_Alloc16(allocaSize);

            for (i = 0; i < texCoords.Num(); i++)
            {
                verts[i].Clear();
                verts[i].st = texCoords[i];
            }
            TransformVerts(verts, joints);
            deformInfo = R_BuildDeformInfo(texCoords.Num(), verts, tris.Num(), tris.Ptr(), shader.UseUnsmoothedTangents());

            if (allocaSize >= 600000)
                Mem_Free16(verts);
        }

        void TransformVerts(DrawVert[] verts, JointMat[] joints);
            => SIMDProcessor.TransformVerts(verts, texCoords.Num(), entJoints, scaledWeights, weightIndex, numWeights);

        // Special transform to make the mesh seem fat or skinny.  May be used for zombie deaths
        void TransformScaledVerts(DrawVert[] verts, JointMat[] joints, float scale)
        {
            idVec4* scaledWeights = (idVec4*)_alloca16(numWeights * sizeof(scaledWeights[0]));
            SIMDProcessor.Mul(scaledWeights[0].ToFloatPtr(), scale, scaledWeights[0].ToFloatPtr(), numWeights * 4);
            SIMDProcessor.TransformVerts(verts, texCoords.Num(), entJoints, scaledWeights, weightIndex, numWeights);
        }

        public void UpdateSurface(RenderEntity ent, JointMat[] joints, ModelSurface surf)
        {
            int i, base;
            srfTriangles_t* tri;

            tr.pc.c_deformedSurfaces++;
            tr.pc.c_deformedVerts += deformInfo.numOutputVerts;
            tr.pc.c_deformedIndexes += deformInfo.numIndexes;

            surf.shader = shader;

            if (surf.geometry)
            {
                // if the number of verts and indexes are the same we can re-use the triangle surface
                // the number of indexes must be the same to assure the correct amount of memory is allocated for the facePlanes
                if (surf.geometry.numVerts == deformInfo.numOutputVerts && surf.geometry.numIndexes == deformInfo.numIndexes)
                {
                    R_FreeStaticTriSurfVertexCaches(surf.geometry);
                }
                else
                {
                    R_FreeStaticTriSurf(surf.geometry);
                    surf.geometry = R_AllocStaticTriSurf();
                }
            }
            else
            {
                surf.geometry = R_AllocStaticTriSurf();
            }

            tri = surf.geometry;

            // note that some of the data is references, and should not be freed
            tri.deformedSurface = true;
            tri.tangentsCalculated = false;
            tri.facePlanesCalculated = false;

            tri.numIndexes = deformInfo.numIndexes;
            tri.indexes = deformInfo.indexes;
            tri.silIndexes = deformInfo.silIndexes;
            tri.numMirroredVerts = deformInfo.numMirroredVerts;
            tri.mirroredVerts = deformInfo.mirroredVerts;
            tri.numDupVerts = deformInfo.numDupVerts;
            tri.dupVerts = deformInfo.dupVerts;
            tri.numSilEdges = deformInfo.numSilEdges;
            tri.silEdges = deformInfo.silEdges;
            tri.dominantTris = deformInfo.dominantTris;
            tri.numVerts = deformInfo.numOutputVerts;

            if (tri.verts == null)
            {
                R_AllocStaticTriSurfVerts(tri, tri.numVerts);
                for (i = 0; i < deformInfo.numSourceVerts; i++)
                {
                    tri.verts[i].Clear();
                    tri.verts[i].st = texCoords[i];
                }
            }

            if (ent.shaderParms[SHADERPARM_MD5_SKINSCALE] != 0f)
            {
                TransformScaledVerts(tri.verts, entJoints, ent.shaderParms[SHADERPARM_MD5_SKINSCALE]);
            }
            else
            {
                TransformVerts(tri.verts, entJoints);
            }

            // replicate the mirror seam vertexes
            base = deformInfo.numOutputVerts - deformInfo.numMirroredVerts;
            for (i = 0; i < deformInfo.numMirroredVerts; i++)
            {
                tri.verts[base + i] = tri.verts[deformInfo.mirroredVerts[i]];
            }

            R_BoundTriSurf(tri);

            // If a surface is going to be have a lighting interaction generated, it will also have to call
            // R_DeriveTangents() to get normals, tangents, and face planes.  If it only
            // needs shadows generated, it will only have to generate face planes.  If it only
            // has ambient drawing, or is culled, no additional work will be necessary
            if (!r_useDeferredTangents.GetBool())
            {
                // set face planes, vertex normals, tangents
                R_DeriveTangents(tri);
            }
        }

        public Bounds CalcBounds(JointMat[] joints)
        {
            idBounds bounds;

            idDrawVert* verts;
            size_t allocaSize = texCoords.Num() * sizeof(idDrawVert);
            if (allocaSize < 600000)
                verts = (idDrawVert*)_alloca16(allocaSize);
            else
                verts = (idDrawVert*)Mem_Alloc16(allocaSize);

            TransformVerts(verts, entJoints);

            SIMDProcessor.MinMax(bounds[0], bounds[1], verts, texCoords.Num());

            if (allocaSize >= 600000)
                Mem_Free16(verts);
            return bounds;
        }

        public int NearestJoint(int a, int b, int c)
        {
            int i, bestJoint, vertNum, weightVertNum;
            float bestWeight;

            // duplicated vertices might not have weights
            if (a >= 0 && a < texCoords.Num())
            {
                vertNum = a;
            }
            else if (b >= 0 && b < texCoords.Num())
            {
                vertNum = b;
            }
            else if (c >= 0 && c < texCoords.Num())
            {
                vertNum = c;
            }
            else
            {
                // all vertices are duplicates which shouldn't happen
                return 0;
            }

            // find the first weight for this vertex
            weightVertNum = 0;
            for (i = 0; weightVertNum < vertNum; i++)
            {
                weightVertNum += weightIndex[i * 2 + 1];
            }

            // get the joint for the largest weight
            bestWeight = scaledWeights[i].w;
            bestJoint = weightIndex[i * 2 + 0] / sizeof(idJointMat);
            for (; weightIndex[i * 2 + 1] == 0; i++)
            {
                if (scaledWeights[i].w > bestWeight)
                {
                    bestWeight = scaledWeights[i].w;
                    bestJoint = weightIndex[i * 2 + 0] / sizeof(idJointMat);
                }
            }
            return bestJoint;
        }


        public int NumVerts
            => texCoords.Count;

        public int NumTris
            => numTris;

        public int NumWeights
            => numWeights;
    }

    public class RenderModelMD5 : RenderModelStatic
    {
        List<MD5Joint> joints;
        List<JointQuat> defaultPose;
        List<JointMat> invertedDefaultPose;
        List<MD5Mesh> meshes;

        void GetFrameBounds(RenderEntity ent, out Bounds bounds);

        void DrawJoints(RenderEntity ent, ViewDef view)
        {
            int i;
            int num;
            idVec3 pos;
            const idJointMat* joint;
            const idMD5Joint* md5Joint;
            int parentNum;

            num = ent.numJoints;
            joint = ent.joints;
            md5Joint = joints.Ptr();
            for (i = 0; i < num; i++, joint++, md5Joint++)
            {
                pos = ent.origin + joint.ToVec3() * ent.axis;
                if (md5Joint.parent)
                {
                    parentNum = md5Joint.parent - joints.Ptr();
                    session.rw.DebugLine(colorWhite, ent.origin + ent.joints[parentNum].ToVec3() * ent.axis, pos);
                }

                session.rw.DebugLine(colorRed, pos, pos + joint.ToMat3()[0] * 2f * ent.axis);
                session.rw.DebugLine(colorGreen, pos, pos + joint.ToMat3()[1] * 2f * ent.axis);
                session.rw.DebugLine(colorBlue, pos, pos + joint.ToMat3()[2] * 2f * ent.axis);
            }

            idBounds bounds;

            bounds.FromTransformedBounds(ent.bounds, vec3_zero, ent.axis);
            session.rw.DebugBounds(colorMagenta, bounds, ent.origin);

            if ((r_jointNameScale.GetFloat() != 0f) && (bounds.Expand(128f).ContainsPoint(view.renderView.vieworg - ent.origin)))
            {
                idVec3 offset( 0, 0, r_jointNameOffset.GetFloat() );
                float scale;

                scale = r_jointNameScale.GetFloat();
                joint = ent.joints;
                num = ent.numJoints;
                for (i = 0; i < num; i++, joint++)
                {
                    pos = ent.origin + joint.ToVec3() * ent.axis;
                    session.rw.DrawText(joints[i].name, pos + offset, scale, colorWhite, view.renderView.viewaxis, 1);
                }
            }
        }

        void ParseJoint(Lexer parser, MD5Joint joint, JointQuat defaultPose)
        {
            idToken token;
            int num;

            //
            // parse name
            //
            parser.ReadToken(&token);
            joint.name = token;

            //
            // parse parent
            //
            num = parser.ParseInt();
            if (num < 0)
            {
                joint.parent = null;
            }
            else
            {
                if (num >= joints.Num() - 1)
                {
                    parser.Error("Invalid parent for joint '%s'", joint.name.c_str());
                }
                joint.parent = &joints[num];
            }

            //
            // parse default pose
            //
            parser.Parse1DMatrix(3, defaultPose.t.ToFloatPtr());
            parser.Parse1DMatrix(3, defaultPose.q.ToFloatPtr());
            defaultPose.q.w = defaultPose.q.CalcW();
        }

        public override void InitFromFile(string fileName)
        {
            name = fileName;
            LoadModel();
        }

        public override DynamicModel IsDynamicModel
            => DynamicModel.DM_CACHED;

        void CalculateBounds(JointMat[] joints)
        {
            int i;
            idMD5Mesh* mesh;

            bounds.Clear();
            for (mesh = meshes.Ptr(), i = 0; i < meshes.Num(); i++, mesh++)
            {
                bounds.AddBounds(mesh.CalcBounds(entJoints));
            }
        }

        // This calculates a rough bounds by using the joint radii without transforming all the points
        public override Bounds Bounds(RenderEntity ent)
        {
            if (ent == null)
                // this is the bounds for the reference pose
                return bounds;
            return ent.bounds;
        }

        public override void Print()
        {
            const idMD5Mesh* mesh;
            int i;

            common.Printf("%s\n", name.c_str());
            common.Printf("Dynamic model.\n");
            common.Printf("Generated smooth normals.\n");
            common.Printf("    verts  tris weights material\n");
            int totalVerts = 0;
            int totalTris = 0;
            int totalWeights = 0;
            for (mesh = meshes.Ptr(), i = 0; i < meshes.Num(); i++, mesh++)
            {
                totalVerts += mesh.NumVerts();
                totalTris += mesh.NumTris();
                totalWeights += mesh.NumWeights();
                common.Printf("%2i: %5i %5i %7i %s\n", i, mesh.NumVerts(), mesh.NumTris(), mesh.NumWeights(), mesh.shader.GetName());
            }
            common.Printf("-----\n");
            common.Printf("%4i verts.\n", totalVerts);
            common.Printf("%4i tris.\n", totalTris);
            common.Printf("%4i weights.\n", totalWeights);
            common.Printf("%4i joints.\n", joints.Num());
        }

        public override void List()
        {
            int i;
            const idMD5Mesh* mesh;
            int totalTris = 0;
            int totalVerts = 0;

            for (mesh = meshes.Ptr(), i = 0; i < meshes.Num(); i++, mesh++)
            {
                totalTris += mesh.numTris;
                totalVerts += mesh.NumVerts();
            }
            common.Printf(" %4ik %3i %4i %4i %s(MD5)", Memory() / 1024, meshes.Num(), totalVerts, totalTris, Name());

            if (defaulted)
            {
                common.Printf(" (DEFAULTED)");
            }

            common.Printf("\n");
        }

        // models that are already loaded at level start time will still touch their materials to make sure they are kept loaded
        public override void TouchData()
        {
            MD5Mesh mesh; int i;

            for (mesh = meshes.Ptr(), i = 0; i < meshes.Num(); i++, mesh++)
            {
                declManager.FindMaterial(mesh.shader.GetName());
            }
        }

        // frees all the data, but leaves the class around for dangling references, which can regenerate the data with LoadModel()
        public override void PurgeModel()
        {
            purged = true;
            joints.Clear();
            defaultPose.Clear();
            meshes.Clear();
        }

        // used for initial loads, reloadModel, and reloading the data of purged models Upon exit, the model will absolutely be valid, but possibly as a default model
        public override void LoadModel()
        {
            int version;
            int i;
            int num;
            int parentNum;
            idToken token;
            idLexer parser(LEXFL_ALLOWPATHNAMES | LEXFL_NOSTRINGESCAPECHARS );
            idJointQuat* pose;
            idMD5Joint* joint;
            idJointMat* poseMat3;

            if (!purged)
            {
                PurgeModel();
            }
            purged = false;

            if (!parser.LoadFile(name))
            {
                MakeDefaultModel();
                return;
            }

            parser.ExpectTokenString(MD5_VERSION_STRING);
            version = parser.ParseInt();

            if (version != MD5_VERSION)
            {
                parser.Error("Invalid version %d.  Should be version %d\n", version, MD5_VERSION);
            }

            //
            // skip commandline
            //
            parser.ExpectTokenString("commandline");
            parser.ReadToken(&token);

            // parse num joints
            parser.ExpectTokenString("numJoints");
            num = parser.ParseInt();
            joints.SetGranularity(1);
            joints.SetNum(num);
            defaultPose.SetGranularity(1);
            defaultPose.SetNum(num);
            poseMat3 = (idJointMat*)_alloca16(num * sizeof( *poseMat3) );

            // parse num meshes
            parser.ExpectTokenString("numMeshes");
            num = parser.ParseInt();
            if (num < 0)
            {
                parser.Error("Invalid size: %d", num);
            }
            meshes.SetGranularity(1);
            meshes.SetNum(num);

            //
            // parse joints
            //
            parser.ExpectTokenString("joints");
            parser.ExpectTokenString("{");
            pose = defaultPose.Ptr();
            joint = joints.Ptr();
            for (i = 0; i < joints.Num(); i++, joint++, pose++)
            {
                ParseJoint(parser, joint, pose);
                poseMat3[i].SetRotation(pose.q.ToMat3());
                poseMat3[i].SetTranslation(pose.t);
                if (joint.parent)
                {
                    parentNum = joint.parent - joints.Ptr();
                    pose.q = (poseMat3[i].ToMat3() * poseMat3[parentNum].ToMat3().Transpose()).ToQuat();
                    pose.t = (poseMat3[i].ToVec3() - poseMat3[parentNum].ToVec3()) * poseMat3[parentNum].ToMat3().Transpose();
                }
            }
            parser.ExpectTokenString("}");

            //-----------------------------------------
            // create the inverse of the base pose joints to support tech6 style deformation
            // of base pose vertexes, normals, and tangents.
            //
            // vertex * joints * inverseJoints == vertex when joints is the base pose
            // When the joints are in another pose, it gives the animated vertex position
            //-----------------------------------------
            invertedDefaultPose.SetNum(SIMD_ROUND_JOINTS(joints.Num()));
            for (int i = 0; i < joints.Num(); i++)
            {
                invertedDefaultPose[i] = poseMat3[i];
                invertedDefaultPose[i].Invert();
            }
            SIMD_INIT_LAST_JOINT(invertedDefaultPose.Ptr(), joints.Num());

            idStr materialName; // Koz
            bool isPDAmesh = false;

            for (int i = 0; i < meshes.Num(); i++)
            {
                isPDAmesh = false;
                parser.ExpectTokenString("mesh");
                meshes[i].ParseMesh(parser, defaultPose.Num(), poseMat3);

                // Koz begin
                // Remove hands from weapon & pda viewmodels if desired.

                materialName = meshes[i].shader.GetName();
                if (materialName.IsEmpty())
                {
                    meshes[i].shader = null;
                }
                else
                {
                    if (materialName == "textures/common/pda_gui" || materialName == "_pdaImage" || materialName == "_pdaimage")
                    {
                        // Koz pda  - change material to _pdaImage instead of deault
                        // this allows rendering the PDA & swf menus to the model ingame.
                        // if we find this gui, we also need to add a surface to the model, so flag.
                        meshes[i].shader = declManager.FindMaterial("_pdaImage");
                        isPDAmesh = true;
                    }
                }

                if (isPDAmesh)
                {


                    {
                        common.Printf("Load pda model\n");
                        for (int ti = 0; ti < meshes[i].NumVerts(); ti++)
                        {
                            common.Printf("Numverts %d Vert %d %f %f %f : %f %f %f %f\n", meshes[i].NumVerts(), ti, meshes[i].deformInfo.verts[ti].xyz.x,
                                meshes[i].deformInfo.verts[ti].xyz.y,
                                meshes[i].deformInfo.verts[ti].xyz.z,
                                meshes[i].deformInfo.verts[ti].GetTexCoordS(),
                                meshes[i].deformInfo.verts[ti].GetTexCoordT(),
                                meshes[i].deformInfo.verts[ti].st[0],
                                meshes[i].deformInfo.verts[ti].st[1]);
                        }
                    }


                    common.Printf("PDA gui found, creating gui surface for hitscan.\n");

                    modelSurface_t pdasurface;

                    pdasurface.id = 0;
                    pdasurface.shader = declManager.FindMaterial("_pdaImage");

                    srfTriangles_t* pdageometry = AllocSurfaceTriangles(meshes[i].NumVerts(), meshes[i].deformInfo.numIndexes);
                    assert(pdageometry != null);

                    // infinite bounds
                    pdageometry.bounds[0][0] =
                    pdageometry.bounds[0][1] =
                    pdageometry.bounds[0][2] = -99999;
                    pdageometry.bounds[1][0] =
                    pdageometry.bounds[1][1] =
                    pdageometry.bounds[1][2] = 99999;

                    pdageometry.numVerts = meshes[i].NumVerts();
                    pdageometry.numIndexes = meshes[i].deformInfo.numIndexes;

                    for (int zz = 0; zz < pdageometry.numIndexes; zz++)
                    {
                        pdageometry.indexes[zz] = meshes[i].deformInfo.indexes[zz];
                    }

                    for (int zz = 0; zz < pdageometry.numVerts; zz++)
                    {
                        //GB Fix Verts (if needed)
                        pdageometry.verts[zz].xyz = meshes[i].deformInfo.verts[zz].xyz;
                        //pdageometry.verts[zz].SetTexCoord( meshes[i].deformInfo.verts[zz].GetTexCoord() );
                        pdageometry.verts[zz].st = meshes[i].deformInfo.verts[zz].st;
                    }


                    {
                        common.Printf("verify pda model\n");
                        for (int ti = 0; ti < pdageometry.numVerts; ti++)
                        {
                            common.Printf("Numverts %d Vert %d %f %f %f : %f %f %f %f\n", pdageometry.numVerts, ti, pdageometry.verts[ti].xyz.x,
                                pdageometry.verts[ti].xyz.y,
                                pdageometry.verts[ti].xyz.z,
                                pdageometry.verts[ti].GetTexCoordS(),
                                pdageometry.verts[ti].GetTexCoordT(),
                                pdageometry.verts[ti].st[0],
                                pdageometry.verts[ti].st[1]);
                        }
                    }


                    pdasurface.geometry = pdageometry;
                    AddSurface(pdasurface);
                }
                // Koz end PDA
            }


            //
            // calculate the bounds of the model
            //
            CalculateBounds(poseMat3);

            // set the timestamp for reloadmodels
            fileSystem.ReadFile(name, null, &timeStamp);
        }

        public override int Memory()
        {
            int total, i;

            total = sizeof( *this );
            total += joints.MemoryUsed() + defaultPose.MemoryUsed() + meshes.MemoryUsed();

            // count up strings
            for (i = 0; i < joints.Num(); i++)
            {
                total += joints[i].name.DynamicMemoryUsed();
            }

            // count up meshes
            for (i = 0; i < meshes.Num(); i++)
            {
                const idMD5Mesh* mesh = &meshes[i];

                total += mesh.texCoords.MemoryUsed() + mesh.numWeights * (sizeof(mesh.scaledWeights[0]) + sizeof(mesh.weightIndex[0]) * 2);

                // sum up deform info
                total += sizeof(mesh.deformInfo);
                total += R_DeformInfoMemoryUsed(mesh.deformInfo);
            }
            return total;
        }

        public override IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel)
        {
            int i, surfaceNum;
            idMD5Mesh* mesh;
            idRenderModelStatic* staticModel;

            if (cachedModel && !r_useCachedDynamicModels.GetBool())
            {
                delete cachedModel;
                cachedModel = null;
            }

            if (purged)
            {
                common.DWarning("model %s instantiated while purged", Name());
                LoadModel();
            }

            if (!ent.joints)
            {
                common.Printf("idRenderModelMD5::InstantiateDynamicModel: null joints on renderEntity for '%s'\n", Name());
                delete cachedModel;
                return null;
            }
            else if (ent.numJoints != joints.Num())
            {
                common.Printf("idRenderModelMD5::InstantiateDynamicModel: renderEntity has different number of joints than model for '%s'\n", Name());
                delete cachedModel;
                return null;
            }

            tr.pc.c_generateMd5++;

            if (cachedModel)
            {
                assert(dynamic_cast<idRenderModelStatic*>(cachedModel) != null);
                assert(idStr::Icmp(cachedModel.Name(), MD5_SnapshotName) == 0);
                staticModel = static_cast<idRenderModelStatic*>(cachedModel);
            }
            else
            {
                staticModel = new idRenderModelStatic;
                staticModel.InitEmpty(MD5_SnapshotName);
            }

            staticModel.bounds.Clear();

            if (r_showSkel.GetInteger())
            {
                if ((view != null) && (!r_skipSuppress.GetBool() || !ent.suppressSurfaceInViewID || (ent.suppressSurfaceInViewID != view.renderView.viewID)))
                {
                    // only draw the skeleton
                    DrawJoints(ent, view);
                }

                if (r_showSkel.GetInteger() > 1)
                {
                    // turn off the model when showing the skeleton
                    staticModel.InitEmpty(MD5_SnapshotName);
                    return staticModel;
                }
            }

            // create all the surfaces
            for (mesh = meshes.Ptr(), i = 0; i < meshes.Num(); i++, mesh++)
            {
                // avoid deforming the surface if it will be a nodraw due to a skin remapping
                // FIXME: may have to still deform clipping hulls
                const idMaterial* shader = mesh.shader;

                shader = R_RemapShaderBySkin(shader, ent.customSkin, ent.customShader);

                if (!shader || (!shader.IsDrawn() && !shader.SurfaceCastsShadow()))
                {
                    staticModel.DeleteSurfaceWithId(i);
                    mesh.surfaceNum = -1;
                    continue;
                }

                modelSurface_t* surf;

                if (staticModel.FindSurfaceWithId(i, surfaceNum))
                {
                    mesh.surfaceNum = surfaceNum;
                    surf = &staticModel.surfaces[surfaceNum];
                }
                else
                {

                    // Remove Overlays before adding new surfaces
                    idRenderModelOverlay::RemoveOverlaySurfacesFromModel(staticModel);

                    mesh.surfaceNum = staticModel.NumSurfaces();
                    surf = &staticModel.surfaces.Alloc();
                    surf.geometry = null;
                    surf.shader = null;
                    surf.id = i;
                }

                mesh.UpdateSurface(ent, ent.joints, surf);

                staticModel.bounds.AddPoint(surf.geometry.bounds[0]);
                staticModel.bounds.AddPoint(surf.geometry.bounds[1]);
            }

            return staticModel;
        }

        public override int NumJoints
            => joints.Count;

        public override IList<MD5Joint> Joints
            => joints;

        public override JointHandle GetJointHandle(string name)
        {
            const idMD5Joint* joint;
            int i;

            joint = joints.Ptr();
            for (i = 0; i < joints.Num(); i++, joint++)
            {
                if (idStr::Icmp(joint.name.c_str(), name) == 0)
                {
                    return (jointHandle_t)i;
                }
            }

            return INVALID_JOINT;
        }

        public override string GetJointName(JointHandle handle)
        {
            if ((handle < 0) || (handle >= joints.Num()))
            {
                return "<invalid joint>";
            }

            return joints[handle].name;
        }

        public override IList<JointQuat> DefaultPose
            => defaultPose;

        public override int NearestJoint(int surfaceNum, int a, int b, int c)
        {
            int i;
            const idMD5Mesh* mesh;

            if (surfaceNum > meshes.Num())
            {
                common.Error("idRenderModelMD5::NearestJoint: surfaceNum > meshes.Num()");
            }

            for (mesh = meshes.Ptr(), i = 0; i < meshes.Num(); i++, mesh++)
            {
                if (mesh.surfaceNum == surfaceNum)
                {
                    return mesh.NearestJoint(a, b, c);
                }
            }
            return 0;
        }
    }

    public class RenderModelMD3 : RenderModelStatic
    {
        int index;          // model = tr.models[model.index]
        int dataSize;       // just for listing purposes
        Md3Header md3;            // only if type == MOD_MESH
        int numLods;

        public unsafe override void InitFromFile(string fileName)
        {
            int i, j;

            name = fileName;

            var size = fileSystem.ReadFile(fileName, out var buffer, out var _);
            if (size <= 0)
                return;

            md3 = UnsafeX.ReadTSize<Md3Header>(Md3Header.SizeOf, buffer);
            LittleInt(ref md3.ident);
            LittleInt(ref md3.version);
            LittleInt(ref md3.numFrames);
            LittleInt(ref md3.numTags);
            LittleInt(ref md3.numSurfaces);
            LittleInt(ref md3.ofsFrames);
            LittleInt(ref md3.ofsTags);
            LittleInt(ref md3.ofsSurfaces);
            LittleInt(ref md3.ofsEnd);

            if (md3.version != ModelXMd3.MD3_VERSION)
            {
                fileSystem.FreeFile(buffer);
                common.Warning($"InitFromFile: {fileName} has wrong version ({md3.version} should be {ModelXMd3.MD3_VERSION})");
                return;
            }

            size = md3.ofsEnd;
            dataSize += size;

            if (md3.numFrames < 1)
            {
                common.Warning($"InitFromFile: {fileName} has no frames");
                fileSystem.FreeFile(buffer);
                return;
            }

            // swap all the frames
            md3.frames = UnsafeX.ReadTArray<Md3Frame>(buffer, md3.ofsFrames, md3.numFrames);
            for (i = 0; i < md3.frames.Length; i++)
            {
                ref Md3Frame frame = ref md3.frames[i];
                LittleFloat(ref frame.radius);
                LittleVector3(ref frame.bounds[0]);
                LittleVector3(ref frame.bounds[1]);
                LittleVector3(ref frame.localOrigin);
            }

            // swap all the tags
            md3.tags = UnsafeX.ReadTArray<Md3Tag>(buffer, md3.ofsTags, md3.numTags * md3.numFrames);
            for (i = 0; i < md3.tags.Length; i++)
            {
                ref Md3Tag tag = ref md3.tags[i];
                LittleVector3(ref tag.origin);
                LittleVector3(ref tag.axis[0]);
                LittleVector3(ref tag.axis[1]);
                LittleVector3(ref tag.axis[2]);
            }

            // swap all the surfaces
            md3.surfaces = new Md3Surface[md3.numSurfaces];
            var surfOfs = md3.ofsSurfaces;
            for (i = 0; i < md3.surfaces.Length; i++)
            {
                md3.surfaces[i] = UnsafeX.ReadTSize<Md3Surface>(Md3Surface.SizeOf, buffer, surfOfs);
                ref Md3Surface surf = ref md3.surfaces[i];
                LittleInt(ref surf.ident);
                LittleInt(ref surf.flags);
                LittleInt(ref surf.numFrames);
                LittleInt(ref surf.numShaders);
                LittleInt(ref surf.numTriangles);
                LittleInt(ref surf.ofsTriangles);
                LittleInt(ref surf.numVerts);
                LittleInt(ref surf.ofsShaders);
                LittleInt(ref surf.ofsSt);
                LittleInt(ref surf.ofsXyzNormals);
                LittleInt(ref surf.ofsEnd);

                if (surf.numVerts > ModelXMd3.SHADER_MAX_VERTEXES)
                    common.Error($"InitFromFile: {fileName} has more than {ModelXMd3.SHADER_MAX_VERTEXES} verts on a surface ({surf.numVerts})");
                if (surf.numTriangles * 3 > ModelXMd3.SHADER_MAX_INDEXES)
                    common.Error($"InitFromFile: {fileName} has more than {ModelXMd3.SHADER_MAX_INDEXES / 3} triangles on a surface ({surf.numTriangles})");

                // change to surface identifier
                surf.ident = 0;    //SF_MD3;

                // lowercase the surface name so skin compares are faster
                surf.name = surf.name.ToLowerInvariant();

                // strip off a trailing _1 or _2 this is a crutch for q3data being a mess
                j = surf.name.Length;
                if (j > 2 && surf.name[j - 2] == '_')
                    surf.name = surf.name.Remove(j - 2);

                // register the shaders
                surf.shaders = UnsafeX.ReadTArray<Md3Shader>(buffer, surfOfs + surf.ofsShaders, surf.numShaders);
                for (j = 0; j < surf.shaders.Length; j++)
                {
                    ref Md3Shader shader = ref surf.shaders[j];
                    var sh = declManager.FindMaterial(shader.name);
                    shader.shader = sh;
                }

                // swap all the triangles
                surf.tris = UnsafeX.ReadTArray<Md3Triangle>(buffer, surfOfs + surf.ofsTriangles, surf.numTriangles);
                for (j = 0; j < surf.tris.Length; j++)
                {
                    ref Md3Triangle tri = ref surf.tris[j];
                    LittleInt(ref tri.indexes[0]);
                    LittleInt(ref tri.indexes[1]);
                    LittleInt(ref tri.indexes[2]);
                }

                // swap all the ST
                surf.sts = UnsafeX.ReadTArray<Md3St>(buffer, surfOfs + surf.ofsSt, surf.numVerts);
                for (j = 0; j < surf.sts.Length; j++)
                {
                    ref Md3St st = ref surf.sts[j];
                    LittleFloat(ref st.st[0]);
                    LittleFloat(ref st.st[1]);
                }

                // swap all the XyzNormals
                surf.xyzs = UnsafeX.ReadTArray<Md3XyzNormal>(buffer, surfOfs + surf.ofsXyzNormals, surf.numVerts * surf.numFrames);
                for (j = 0; j < surf.xyzs.Length; j++)
                {
                    ref Md3XyzNormal xyz = ref surf.xyzs[j];
                    LittleShort(ref xyz.xyz[0]);
                    LittleShort(ref xyz.xyz[1]);
                    LittleShort(ref xyz.xyz[2]);

                    LittleShort(ref xyz.normal);
                }

                // find the next surface
                surfOfs += surf.ofsEnd;
            }

            fileSystem.FreeFile(buffer);
        }

        public override DynamicModel IsDynamicModel
            => DynamicModel.DM_CACHED;

        void LerpMeshVertexes(SrfTriangles tri, Md3Surface surf, float backlerp, int frame, int oldframe)
        {
            float oldXyzScale, newXyzScale; int vertNum, numVerts;

            ref Md3XyzNormal newXyz = ref surf.xyzs[frame];
            newXyzScale = ModelXMd3.MD3_XYZ_SCALE * (1f - backlerp);

            numVerts = surf.numVerts;

            if (backlerp == 0)
                // just copy the vertexes
                for (vertNum = 0; vertNum < numVerts; vertNum++)
                {

                    var outvert = tri.verts[tri.numVerts];

                    outvert.xyz.x = newXyz.xyz[0] * newXyzScale;
                    outvert.xyz.y = newXyz.xyz[1] * newXyzScale;
                    outvert.xyz.z = newXyz.xyz[2] * newXyzScale;

                    tri.numVerts++;
                }
            else
            {
                // interpolate and copy the vertexes
                ref Md3XyzNormal oldXyz = ref surf.xyzs[oldframe];
                oldXyzScale = ModelXMd3.MD3_XYZ_SCALE * backlerp;

                for (vertNum = 0; vertNum < numVerts; vertNum++)
                {
                    var outvert = tri.verts[tri.numVerts];

                    // interpolate the xyz
                    outvert.xyz.x = oldXyz.xyz[0] * oldXyzScale + newXyz.xyz[0] * newXyzScale;
                    outvert.xyz.y = oldXyz.xyz[1] * oldXyzScale + newXyz.xyz[1] * newXyzScale;
                    outvert.xyz.z = oldXyz.xyz[2] * oldXyzScale + newXyz.xyz[2] * newXyzScale;

                    tri.numVerts++;
                }
            }
        }

        public override IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel)
        {
            int i, j;
            float backlerp;
            //int* triangles;
            //float* texCoords;
            int indexes, numVerts;
            Md3Surface surface;
            int frame, oldframe;
            RenderModelStatic staticModel;

            if (cachedModel != null)
                cachedModel = null;

            staticModel = new RenderModelStatic();
            staticModel.bounds.Clear();

            // TODO: these need set by an entity
            frame = (int)ent.shaderParms[RenderWorldX.SHADERPARM_MD3_FRAME];         // probably want to keep frames < 1000 or so
            oldframe = (int)ent.shaderParms[RenderWorldX.SHADERPARM_MD3_LASTFRAME];
            backlerp = ent.shaderParms[RenderWorldX.SHADERPARM_MD3_BACKLERP];

            for (i = 0; i < md3.numSurfaces; i++)
            {
                surface = md3.surfaces[i];

                var tri = R_AllocStaticTriSurf();
                R_AllocStaticTriSurfVerts(tri, surface.numVerts);
                R_AllocStaticTriSurfIndexes(tri, surface.numTriangles * 3);
                tri.bounds.Clear();

                ModelSurface surf = new();

                surf.geometry = tri;
                surf.shader = surface.shaders[0].shader;

                LerpMeshVertexes(tri, surface, backlerp, frame, oldframe);

                triangles = surface.tris[0].indexes;
                indexes = surface.numTriangles * 3;
                for (j = 0; j < indexes; j++)
                    tri.indexes[j] = triangles[j];
                tri.numIndexes += indexes;

                texCoords = surface.sts;

                numVerts = surface.numVerts;
                for (j = 0; j < numVerts; j++)
                {
                    idDrawVert* stri = &tri.verts[j];
                    stri.st[0] = texCoords[j * 2 + 0];
                    stri.st[1] = texCoords[j * 2 + 1];
                }

                R_BoundTriSurf(tri);

                staticModel.AddSurface(surf);
                staticModel.bounds.AddPoint(surf.geometry.bounds[0]);
                staticModel.bounds.AddPoint(surf.geometry.bounds[1]);
            }

            return staticModel;
        }

        public override Bounds Bounds(RenderEntity ent)
        {
            Bounds ret = new();

            ret.Clear();

            if (ent == null || md3 == null)
            {
                // just give it the editor bounds
                ret.AddPoint(new Vector3(-10, -10, -10));
                ret.AddPoint(new Vector3(10, 10, 10));
                return ret;
            }

            var frame = md3.frames[0];

            ret.AddPoint(frame.bounds[0]);
            ret.AddPoint(frame.bounds[1]);

            return ret;
        }
    }

    public class RenderModelLiquid : RenderModelStatic
    {
        const int LIQUID_MAX_SKIP_FRAMES = 5;
        const int LIQUID_MAX_TYPES = 3;

        public RenderModelLiquid()
        {
            verts_x = 32;
            verts_y = 32;
            scale_x = 256f;
            scale_y = 256f;
            liquid_type = 0;
            density = 0.97f;
            drop_height = 4;
            drop_radius = 4;
            drop_delay = 1000;
            shader = declManager.FindMaterial(null);
            update_tics = 33;  // ~30 hz
            time = 0;
            seed = 0;

            random.SetSeed(0);
        }

        public override void InitFromFile(string fileName)
        {
            int i, x, y; float size_x, size_y, rate;
            Parser parser = new(LEXFL.ALLOWPATHNAMES | LEXFL.NOSTRINGESCAPECHARS);
            List<int> tris = new();

            name = fileName;

            if (!parser.LoadFile(fileName))
            {
                MakeDefaultModel();
                return;
            }

            size_x = scale_x * verts_x;
            size_y = scale_y * verts_y;

            while (parser.ReadToken(out var token))
            {
                if (string.Equals(token, "seed", StringComparison.OrdinalIgnoreCase)) seed = parser.ParseInt();
                else if (string.Equals(token, "size_x", StringComparison.OrdinalIgnoreCase)) size_x = parser.ParseFloat();
                else if (string.Equals(token, "size_y", StringComparison.OrdinalIgnoreCase)) size_y = parser.ParseFloat();
                else if (string.Equals(token, "verts_x", StringComparison.OrdinalIgnoreCase))
                {
                    verts_x = (int)parser.ParseFloat();
                    if (verts_x < 2)
                    {
                        parser.Warning("Invalid # of verts.  Using default model.");
                        MakeDefaultModel();
                        return;
                    }
                }
                else if (string.Equals(token, "verts_y", StringComparison.OrdinalIgnoreCase))
                {
                    verts_y = (int)parser.ParseFloat();
                    if (verts_y < 2)
                    {
                        parser.Warning("Invalid # of verts.  Using default model.");
                        MakeDefaultModel();
                        return;
                    }
                }
                else if (string.Equals(token, "liquid_type", StringComparison.OrdinalIgnoreCase))
                {
                    liquid_type = parser.ParseInt() - 1;
                    if (liquid_type < 0 || liquid_type >= LIQUID_MAX_TYPES)
                    {
                        parser.Warning("Invalid liquid_type.  Using default model.");
                        MakeDefaultModel();
                        return;
                    }
                }
                else if (string.Equals(token, "density", StringComparison.OrdinalIgnoreCase)) density = parser.ParseFloat();
                else if (string.Equals(token, "drop_height", StringComparison.OrdinalIgnoreCase)) drop_height = parser.ParseFloat();
                else if (string.Equals(token, "drop_radius", StringComparison.OrdinalIgnoreCase)) drop_radius = parser.ParseInt();
                else if (string.Equals(token, "drop_delay", StringComparison.OrdinalIgnoreCase)) drop_delay = MathX.SEC2MS(parser.ParseFloat());
                else if (string.Equals(token, "shader", StringComparison.OrdinalIgnoreCase))
                {
                    parser.ReadToken(out token);
                    shader = declManager.FindMaterial(token);
                }
                else if (string.Equals(token, "update_rate", StringComparison.OrdinalIgnoreCase))
                {
                    rate = parser.ParseFloat();
                    if (rate <= 0f || rate > 60f)
                    {
                        parser.Warning("Invalid update_rate.  Must be between 0 and 60.  Using default model.");
                        MakeDefaultModel();
                        return;
                    }
                    update_tics = (int)(1000 / rate);
                }
                else
                {
                    parser.Warning($"Unknown parameter '{token}'.  Using default model.");
                    MakeDefaultModel();
                    return;
                }
            }

            scale_x = size_x / (verts_x - 1);
            scale_y = size_y / (verts_y - 1);

            pages.SetNum(2 * verts_x * verts_y);
            page1 = pages.Ptr();
            page2 = page1 + verts_x * verts_y;

            verts.SetNum(verts_x * verts_y);
            for (i = 0, y = 0; y < verts_y; y++)
            {
                for (x = 0; x < verts_x; x++, i++)
                {
                    page1[i] = 0f;
                    page2[i] = 0f;
                    verts[i].Clear();
                    verts[i].xyz.Set(x * scale_x, y * scale_y, 0f);
                    verts[i].st.Set((float)x / (float)(verts_x - 1), (float)-y / (float)(verts_y - 1));
                }
            }

            tris.SetNum((verts_x - 1) * (verts_y - 1) * 6);
            for (i = 0, y = 0; y < verts_y - 1; y++)
            {
                for (x = 1; x < verts_x; x++, i += 6)
                {
                    tris[i + 0] = y * verts_x + x;
                    tris[i + 1] = y * verts_x + x - 1;
                    tris[i + 2] = (y + 1) * verts_x + x - 1;

                    tris[i + 3] = (y + 1) * verts_x + x - 1;
                    tris[i + 4] = (y + 1) * verts_x + x;
                    tris[i + 5] = y * verts_x + x;
                }
            }

            // build the information that will be common to all animations of this mesh:
            // sil edge connectivity and normal / tangent generation information
            deformInfo = R_BuildDeformInfo(verts.Count, verts.Ptr(), tris.Count, tris.Ptr(), true);

            bounds.Clear();
            bounds.AddPoint(new Vector3(0f, 0f, drop_height * -10f));
            bounds.AddPoint(new Vector3((verts_x - 1) * scale_x, (verts_y - 1) * scale_y, drop_height * 10f));

            // set the timestamp for reloadmodels
            fileSystem.ReadFile(name, out timeStamp);

            Reset();
        }

        public override DynamicModel IsDynamicModel
            => DynamicModel.DM_CONTINUOUS;

        public override IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel)
        {
            RenderModelStatic staticModel; int frames, t; float lerp;

            if (cachedModel != null)
                cachedModel = null;

            if (deformInfo == null)
                return null;

            t = view == null ? 0 : view.renderView.time;

            // update the liquid model
            frames = (t - time) / update_tics;
            if (frames > LIQUID_MAX_SKIP_FRAMES)
            {
                // don't let time accumalate when skipping frames
                time += update_tics * (frames - LIQUID_MAX_SKIP_FRAMES);

                frames = LIQUID_MAX_SKIP_FRAMES;
            }

            while (frames > 0)
            {
                Update();
                frames--;
            }

            // create the surface
            lerp = (t - time) / (float)update_tics;
            var surf = GenerateSurface(lerp);

            staticModel = new RenderModelStatic();
            staticModel.AddSurface(surf);
            staticModel.bounds = surf.geometry.bounds;

            return staticModel;
        }

        public override Bounds Bounds(RenderEntity ent)
            => bounds;

        public override void Reset()
        {
            int i, x, y;

            if (pages.Count < 2 * verts_x * verts_y)
                return;

            nextDropTime = 0;
            time = 0;
            random.SetSeed(seed);

            page1 = pages.Ptr();
            page2 = page1 + verts_x * verts_y;

            for (i = 0, y = 0; y < verts_y; y++)
            {
                for (x = 0; x < verts_x; x++, i++)
                {
                    page1[i] = 0f;
                    page2[i] = 0f;
                    verts[i].xyz.z = 0f;
                }
            }
        }

        public void IntersectBounds(Bounds bounds, float displacement)
        {
            int left = (int)(bounds[0].x / scale_x),
                right = (int)(bounds[1].x / scale_x),
                top = (int)(bounds[0].y / scale_y),
                bottom = (int)(bounds[1].y / scale_y);
            float down = bounds[0].z;
            //up = bounds[1].z;

            if (right < 1 || left >= verts_x || bottom < 1 || top >= verts_x)
                return;

            // Perform edge clipping...
            if (left < 1) left = 1;
            if (right >= verts_x) right = verts_x - 1;
            if (top < 1) top = 1;
            if (bottom >= verts_y) bottom = verts_y - 1;

            for (var cy = top; cy < bottom; cy++)
                for (var cx = left; cx < right; cx++)
                {
                    ref float pos = ref page1[verts_x * cy + cx];
                    if (pos > down) //&& pos < up)
                        pos = down;
            }
        }

        ModelSurface GenerateSurface(float lerp)
        {
            SrfTriangles tri;
            int i, base_;
            DrawVert vert;
            ModelSurface surf;
            float inv_lerp;

            inv_lerp = 1f - lerp;
            vert = verts.Ptr();
            for (i = 0; i < verts.Count; i++, vert++)
                vert.xyz.z = page1[i] * lerp + page2[i] * inv_lerp;

            tr.pc.c_deformedSurfaces++;
            tr.pc.c_deformedVerts += deformInfo.numOutputVerts;
            tr.pc.c_deformedIndexes += deformInfo.numIndexes;

            tri = R_AllocStaticTriSurf();

            // note that some of the data is references, and should not be freed
            tri.deformedSurface = true;

            tri.numIndexes = deformInfo.numIndexes;
            tri.indexes = deformInfo.indexes;
            tri.silIndexes = deformInfo.silIndexes;
            tri.numMirroredVerts = deformInfo.numMirroredVerts;
            tri.mirroredVerts = deformInfo.mirroredVerts;
            tri.numDupVerts = deformInfo.numDupVerts;
            tri.dupVerts = deformInfo.dupVerts;
            tri.numSilEdges = deformInfo.numSilEdges;
            tri.silEdges = deformInfo.silEdges;
            tri.dominantTris = deformInfo.dominantTris;

            tri.numVerts = deformInfo.numOutputVerts;
            R_AllocStaticTriSurfVerts(tri, tri.numVerts);
            SIMDProcessor.Memcpy(tri.verts, verts.Ptr(), deformInfo.numSourceVerts * sizeof(tri.verts[0]));

            // replicate the mirror seam vertexes
            base_ = deformInfo.numOutputVerts - deformInfo.numMirroredVerts;
            for (i = 0; i < deformInfo.numMirroredVerts; i++)
                tri.verts[base_ + i] = tri.verts[deformInfo.mirroredVerts[i]];

            R_BoundTriSurf(tri);

            // If a surface is going to be have a lighting interaction generated, it will also have to call
            // R_DeriveTangents() to get normals, tangents, and face planes.  If it only
            // needs shadows generated, it will only have to generate face planes.  If it only
            // has ambient drawing, or is culled, no additional work will be necessary
            if (!r_useDeferredTangents.Bool)
                // set face planes, vertex normals, tangents
                R_DeriveTangents(tri);

            surf.geometry = tri;
            surf.shader = shader;

            return surf;
        }

        void WaterDrop(int x, int y, float[] page)
        {
            int square;
            int radsquare = drop_radius * drop_radius;
            float invlength = 1f / radsquare;
            float dist;

            if (x < 0) x = 1 + drop_radius + random.RandomInt(verts_x - 2 * drop_radius - 1);
            if (y < 0) y = 1 + drop_radius + random.RandomInt(verts_y - 2 * drop_radius - 1);

            int left = -drop_radius,
                right = drop_radius,
                top = -drop_radius,
                bottom = drop_radius;

            // Perform edge clipping...
            if (x - drop_radius < 1) left -= (x - drop_radius - 1);
            if (y - drop_radius < 1) top -= (y - drop_radius - 1);
            if (x + drop_radius > verts_x - 1) right -= (x + drop_radius - verts_x + 1);
            if (y + drop_radius > verts_y - 1) bottom -= (y + drop_radius - verts_y + 1);

            for (var cy = top; cy < bottom; cy++)
                for (var cx = left; cx < right; cx++)
                {
                    square = cy * cy + cx * cx;
                    if (square < radsquare)
                    {
                        dist = MathX.Sqrt(square * invlength);
                        page[verts_x * (cy + y) + cx + x] += MathX.Cos16((float)(dist * Math.PI * 0.5f)) * drop_height;
                    }
                }
        }

        void Update()
        {
            int x, y;
            float* p2;
            float* p1;
            float value;

            time += update_tics;

            idSwap(page1, page2);

            if (time > nextDropTime)
            {
                WaterDrop(-1, -1, page2);
                nextDropTime = time + drop_delay;
            }
            else if (time < nextDropTime - drop_delay)
            {
                nextDropTime = time + drop_delay;
            }

            p1 = page1;
            p2 = page2;

            switch (liquid_type)
            {
                case 0:
                    for (y = 1; y < verts_y - 1; y++)
                    {
                        p2 += verts_x;
                        p1 += verts_x;
                        for (x = 1; x < verts_x - 1; x++)
                        {
                            value =
                                (p2[x + verts_x] +
                                  p2[x - verts_x] +
                                  p2[x + 1] +
                                  p2[x - 1] +
                                  p2[x - verts_x - 1] +
                                  p2[x - verts_x + 1] +
                                  p2[x + verts_x - 1] +
                                  p2[x + verts_x + 1] +
                                  p2[x]) * (2f / 9f) -
                                p1[x];

                            p1[x] = value * density;
                        }
                    }
                    break;

                case 1:
                    for (y = 1; y < verts_y - 1; y++)
                    {
                        p2 += verts_x;
                        p1 += verts_x;
                        for (x = 1; x < verts_x - 1; x++)
                        {
                            value =
                                (p2[x + verts_x] +
                                  p2[x - verts_x] +
                                  p2[x + 1] +
                                  p2[x - 1] +
                                  p2[x - verts_x - 1] +
                                  p2[x - verts_x + 1] +
                                  p2[x + verts_x - 1] +
                                  p2[x + verts_x + 1]) * 0.25f -
                                p1[x];

                            p1[x] = value * density;
                        }
                    }
                    break;

                case 2:
                    for (y = 1; y < verts_y - 1; y++)
                    {
                        p2 += verts_x;
                        p1 += verts_x;
                        for (x = 1; x < verts_x - 1; x++)
                        {
                            value =
                                (p2[x + verts_x] +
                                  p2[x - verts_x] +
                                  p2[x + 1] +
                                  p2[x - 1] +
                                  p2[x - verts_x - 1] +
                                  p2[x - verts_x + 1] +
                                  p2[x + verts_x - 1] +
                                  p2[x + verts_x + 1] +
                                  p2[x]) * (1f / 9f);

                            p1[x] = value * density;
                        }
                    }
                    break;
            }
        }

        int verts_x;
        int verts_y;
        float scale_x;
        float scale_y;
        int time;
        int liquid_type;
        int update_tics;
        int seed;

        RandomX random;

        Material shader;
        DeformInfo deformInfo;        // used to create srfTriangles_t from base frames and new vertexes
        float density;
        float drop_height;
        int drop_radius;
        float drop_delay;

        List<float> pages;
        float[] page1;
        float[] page2;

        List<DrawVert> verts;

        int nextDropTime;
    }

    public class RenderModelPrt : RenderModelStatic
    {
        readonly DeclParticle particleSystem;

        public RenderModelPrt();

        public virtual void InitFromFile(string fileName);
        public virtual void TouchData();
        public virtual DynamicModel IsDynamicModel();
        public virtual IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel);
        public virtual Bounds Bounds(RenderEntity ent);
        public virtual float DepthHack();
        public virtual int Memory();
    }

    // This is a simple dynamic model that just creates a stretched quad between two points that faces the view, like a dynamic deform tube.
    public class RenderModelBeam : RenderModelStatic
    {
        const string beam_SnapshotName = "_beam_Snapshot_";

        public override DynamicModel IsDynamicModel => DynamicModel.DM_CONTINUOUS;	// regenerate for every view
        public override bool IsLoaded => true;	// don't ever need to load
        public override IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel)
        {
            RenderModelStatic staticModel;
            SrfTriangles tri;
            ModelSurface surf;

            if (cachedModel != null)
                cachedModel = null;

            if (ent == null || viewDef == null)
                return null;

            if (cachedModel != null)
            {
                Debug.Assert(cachedModel is RenderModelStatic);
                Debug.Assert(string.Equals(cachedModel.Name, beam_SnapshotName, StringComparison.OrdinalIgnoreCase));

                staticModel = (RenderModelStatic)cachedModel;
                surf = staticModel.Surface(0);
                tri = surf.geometry;
            }
            else
            {
                staticModel = new RenderModelStatic();
                staticModel.InitEmpty(beam_SnapshotName);

                tri = R_AllocStaticTriSurf();
                R_AllocStaticTriSurfVerts(tri, 4);
                R_AllocStaticTriSurfIndexes(tri, 6);

                tri.verts[0].Clear(); tri.verts[0].st.x = 0; tri.verts[0].st.y = 0;
                tri.verts[1].Clear(); tri.verts[1].st.x = 0; tri.verts[1].st.y = 1;
                tri.verts[2].Clear(); tri.verts[2].st.x = 1; tri.verts[2].st.y = 0;
                tri.verts[3].Clear(); tri.verts[3].st.x = 1; tri.verts[3].st.y = 1;

                tri.indexes[0] = 0;
                tri.indexes[1] = 2;
                tri.indexes[2] = 1;
                tri.indexes[3] = 2;
                tri.indexes[4] = 3;
                tri.indexes[5] = 1;

                tri.numVerts = 4;
                tri.numIndexes = 6;

                surf.geometry = tri;
                surf.id = 0;
                surf.shader = tr.defaultMaterial;
                staticModel.AddSurface(surf);
            }

            Vector3[] target = reinterpret.cast_vec3(ent.shaderParms[RenderWorldX.SHADERPARM_BEAM_END_X]);

            // we need the view direction to project the minor axis of the tube as the view changes
            Vector3 localView, localTarget;
            float modelMatrix[16];
            R_AxisToModelMatrix(ent.axis, ent.origin, modelMatrix);
            R_GlobalPointToLocal(modelMatrix, viewDef.renderView.vieworg, localView);
            R_GlobalPointToLocal(modelMatrix, target, localTarget);

            Vector3 major = localTarget;
            Vector3 minor;

            Vector3 mid = 0.5f * localTarget;
            Vector3 dir = mid - localView;
            minor.Cross(major, dir);
            minor.Normalize();
            if (ent.shaderParms[RenderWorldX.SHADERPARM_BEAM_WIDTH] != 0f)
                minor *= ent.shaderParms[RenderWorldX.SHADERPARM_BEAM_WIDTH] * 0.5f;

            int red = MathX.FtoiFast(ent.shaderParms[RenderWorldX.SHADERPARM_RED] * 255f);
            int green = MathX.FtoiFast(ent.shaderParms[RenderWorldX.SHADERPARM_GREEN] * 255f);
            int blue = MathX.FtoiFast(ent.shaderParms[RenderWorldX.SHADERPARM_BLUE] * 255f);
            int alpha = MathX.FtoiFast(ent.shaderParms[RenderWorldX.SHADERPARM_ALPHA] * 255f);

            tri.verts[0].xyz = minor;
            tri.verts[0].color[0] = red;
            tri.verts[0].color[1] = green;
            tri.verts[0].color[2] = blue;
            tri.verts[0].color[3] = alpha;

            tri.verts[1].xyz = -minor;
            tri.verts[1].color[0] = red;
            tri.verts[1].color[1] = green;
            tri.verts[1].color[2] = blue;
            tri.verts[1].color[3] = alpha;

            tri.verts[2].xyz = localTarget + minor;
            tri.verts[2].color[0] = red;
            tri.verts[2].color[1] = green;
            tri.verts[2].color[2] = blue;
            tri.verts[2].color[3] = alpha;

            tri.verts[3].xyz = localTarget - minor;
            tri.verts[3].color[0] = red;
            tri.verts[3].color[1] = green;
            tri.verts[3].color[2] = blue;
            tri.verts[3].color[3] = alpha;

            R_BoundTriSurf(tri);

            staticModel.bounds = tri.bounds;

            return staticModel;
        }

        public override Bounds Bounds(RenderEntity ent)
        {
            Bounds b = new();

            b.Zero();
            if (ent == null)
            {
                b.ExpandSelf(8f);
            }
            else
            {
                Vector3 target = reinterpret.cast_vec3(ent.shaderParms[RenderWorldX.SHADERPARM_BEAM_END_X]);
                Vector3 localTarget;
                float modelMatrix[16];
                R_AxisToModelMatrix(ent.axis, ent.origin, modelMatrix);
                R_GlobalPointToLocal(modelMatrix, target, localTarget);

                b.AddPoint(localTarget);
                if (ent.shaderParms[RenderWorldX.SHADERPARM_BEAM_WIDTH] != 0f)
                    b.ExpandSelf(ent.shaderParms[RenderWorldX.SHADERPARM_BEAM_WIDTH] * 0.5f);
            }
            return b;
        }
    }

    public class Trail
    {
        const int MAX_TRAIL_PTS = 20;

        public int lastUpdateTime;
        public int duration;

        public Vector3[] pts = new Vector3[MAX_TRAIL_PTS];
        public int numPoints;
    }

    public class RenderModelTrail : RenderModelStatic
    {
        List<Trail> trails;
        int numActive;
        Bounds trailBounds;

        public RenderModelTrail();

        public virtual DynamicModel IsDynamicModel();
        public virtual bool IsLoaded();
        public virtual IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel);
        public virtual Bounds Bounds(RenderEntity ent);

        public int NewTrail(Vector3 pt, int duration);
        public void UpdateTrail(int index, Vector3 pt);
        public void DrawTrail(int index, RenderEntity ent, SrfTriangles tri, float globalAlpha);
    }

    public class RenderModelLightning : RenderModelStatic
    {
        public virtual DynamicModel IsDynamicModel();
        public virtual bool IsLoaded();
        public virtual IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel);
        public virtual Bounds Bounds(RenderEntity ent);
    }

    public class RenderModelSprite : RenderModelStatic
    {
        public virtual DynamicModel IsDynamicModel();
        public virtual bool IsLoaded();
        public virtual IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel);
        public virtual Bounds Bounds(RenderEntity ent);
    }
}
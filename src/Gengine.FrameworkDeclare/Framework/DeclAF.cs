using System;
using System.Collections.Generic;
using System.NumericsX;
using System.NumericsX.Core;

namespace Gengine.Framework
{
    public enum DECLAF_CONSTRAINT
    {
        INVALID,
        FIXED,
        BALLANDSOCKETJOINT,
        UNIVERSALJOINT,
        HINGE,
        SLIDER,
        SPRING
    }

    public enum DECLAF_JOINTMOD
    {
        AXIS,
        ORIGIN,
        BOTH
    }

    public delegate bool GetJointTransform(object model, JointMat frame, string jointName, Vector3 origin, Matrix3x3 axis);

    public class AFVector
    {
        public enum VEC
        {
            COORDS = 0,
            JOINT,
            BONECENTER,
            BONEDIR
        }
        public VEC type;
        public string joint1;
        public string joint2;

        //public AFVector();

        public bool Parse(Lexer src) => throw new NotImplementedException();
        public bool Finish(string fileName, GetJointTransform GetJointTransform, JointMat frame, object model) => throw new NotImplementedException();
        public bool Write(VFile f) => throw new NotImplementedException();
        public string ToString(string s, int precision = 8) => throw new NotImplementedException();
        public Vector3 ToVec3() => vec;

        Vector3 vec;
        bool negate;
    }

    public class DeclAF_Body
    {
        public string name;
        public string jointName;
        public DECLAF_JOINTMOD jointMod;
        public int modelType;
        public AFVector v1, v2;
        public int numSides;
        public float width;
        public float density;
        public AFVector origin;
        public Angles angles;
        public int contents;
        public int clipMask;
        public bool selfCollision;
        public Matrix3x3 inertiaScale;
        public float linearFriction;
        public float angularFriction;
        public float contactFriction;
        public string containedJoints;
        public AFVector frictionDirection;
        public AFVector contactMotorDirection;

        public void SetDefault(DeclAF file) => throw new NotImplementedException();
    }

    public class DeclAF_Constraint
    {
        public string name;
        public string body1;
        public string body2;
        public DECLAF_CONSTRAINT type;
        public float friction;
        public float stretch;
        public float compress;
        public float damping;
        public float restLength;
        public float minLength;
        public float maxLength;
        public AFVector anchor;
        public AFVector anchor2;
        public AFVector[] shaft = new AFVector[2];
        public AFVector axis;
        public enum LIMIT
        {
            NONE = -1,
            CONE,
            PYRAMID
        }
        public AFVector limitAxis;
        float[] limitAngles = new float[3];

        public void SetDefault(DeclAF file) => throw new NotImplementedException();
    }

    public class DeclAF : Decl
    {
        //public DeclAF();

        public override int Size() => throw new NotImplementedException();
        public override string DefaultDefinition() => throw new NotImplementedException();
        public override bool Parse(string text, int textLength) => throw new NotImplementedException();
        public override void FreeData() => throw new NotImplementedException();

        public virtual void Finish(GetJointTransform GetJointTransform, JointMat frame, object model) => throw new NotImplementedException();

        public bool Save() => throw new NotImplementedException();

        public void NewBody(string name) => throw new NotImplementedException();
        public void RenameBody(string oldName, string newName) => throw new NotImplementedException();
        public void DeleteBody(string name) => throw new NotImplementedException();

        public void NewConstraint(string name) => throw new NotImplementedException();
        public void RenameConstraint(string oldName, string newName) => throw new NotImplementedException();
        public void DeleteConstraint(string name) => throw new NotImplementedException();

        public static int ContentsFromString(string str) => throw new NotImplementedException();
        public static string ContentsToString(int contents, out string str) => throw new NotImplementedException();

        public static DECLAF_JOINTMOD JointModFromString(string str) => throw new NotImplementedException();
        public static string JointModToString(DECLAF_JOINTMOD jointMod) => throw new NotImplementedException();

        public bool modified;
        public string model;
        public string skin;
        public float defaultLinearFriction;
        public float defaultAngularFriction;
        public float defaultContactFriction;
        public float defaultConstraintFriction;
        public float totalMass;
        public Vector2 suspendVelocity;
        public Vector2 suspendAcceleration;
        public float noMoveTime;
        public float noMoveTranslation;
        public float noMoveRotation;
        public float minMoveTime;
        public float maxMoveTime;
        public int contents;
        public int clipMask;
        public bool selfCollision;
        public List<DeclAF_Body> bodies = new();
        public List<DeclAF_Constraint> constraints = new();


        bool ParseContents(Lexer src, out int c) => throw new NotImplementedException();
        bool ParseBody(Lexer src) => throw new NotImplementedException();
        bool ParseFixed(Lexer src) => throw new NotImplementedException();
        bool ParseBallAndSocketJoint(Lexer src) => throw new NotImplementedException();
        bool ParseUniversalJoint(Lexer src) => throw new NotImplementedException();
        bool ParseHinge(Lexer src) => throw new NotImplementedException();
        bool ParseSlider(Lexer src) => throw new NotImplementedException();
        bool ParseSpring(Lexer src) => throw new NotImplementedException();
        bool ParseSettings(Lexer src) => throw new NotImplementedException();

        bool WriteBody(VFile f, DeclAF_Body body) => throw new NotImplementedException();
        bool WriteFixed(VFile f, DeclAF_Constraint c) => throw new NotImplementedException();
        bool WriteBallAndSocketJoint(VFile f, DeclAF_Constraint c) => throw new NotImplementedException();
        bool WriteUniversalJoint(VFile f, DeclAF_Constraint c) => throw new NotImplementedException();
        bool WriteHinge(VFile f, DeclAF_Constraint c) => throw new NotImplementedException();
        bool WriteSlider(VFile f, DeclAF_Constraint c) => throw new NotImplementedException();
        bool WriteSpring(VFile f, DeclAF_Constraint c) => throw new NotImplementedException();
        bool WriteConstraint(VFile f, DeclAF_Constraint c) => throw new NotImplementedException();
        bool WriteSettings(VFile f) => throw new NotImplementedException();

        bool RebuildTextSource() => throw new NotImplementedException();
    }
}

using System;
using System.Collections.Generic;
using System.NumericsX;
using System.NumericsX.Core;

namespace Gengine.Framework
{
    public enum FX
    {
        FX_LIGHT,
        FX_PARTICLE,
        FX_DECAL,
        FX_MODEL,
        FX_SOUND,
        FX_SHAKE,
        FX_ATTACHLIGHT,
        FX_ATTACHENTITY,
        FX_LAUNCH,
        FX_SHOCKWAVE
    }

    //
    // single fx structure
    //
    public struct FXSingleAction
    {
        public int type;
        public int sibling;

        public string data;
        public string name;
        public string fire;

        public float delay;
        public float duration;
        public float restart;
        public float size;
        public float fadeInTime;
        public float fadeOutTime;
        public float shakeTime;
        public float shakeAmplitude;
        public float shakeDistance;
        public float shakeImpulse;
        public float lightRadius;
        public float rotate;
        public float random1;
        public float random2;

        public Vector3 lightColor;
        public Vector3 offset;
        public Matrix3x3 axis;

        public bool soundStarted;
        public bool shakeStarted;
        public bool shakeFalloff;
        public bool shakeIgnoreMaster;
        public bool bindParticles;
        public bool explicitAxis;
        public bool noshadows;
        public bool particleTrackVelocity;
        public bool trackOrigin;
    }

    //
    // grouped fx structures
    //
    public class DeclFX : Decl
    {
        public override int Size() => throw new NotImplementedException();
        public override string DefaultDefinition() => throw new NotImplementedException();
        public override bool Parse(string text, int textLength) => throw new NotImplementedException();
        public override void FreeData() => throw new NotImplementedException();
        public override void Print() => throw new NotImplementedException();
        public override void List() => throw new NotImplementedException();

        List<FXSingleAction> events = new();
        string joint;

        void ParseSingleFXAction(Lexer src, FXSingleAction FXAction) => throw new NotImplementedException();
    }
}

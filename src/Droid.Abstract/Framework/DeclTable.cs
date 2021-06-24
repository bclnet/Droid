using System;
using System.Collections.Generic;

namespace Droid.Framework
{
    public class DeclTable : Decl
    {
        public override int Size() => throw new NotImplementedException();
        public override string DefaultDefinition() => throw new NotImplementedException();
        public override bool Parse(string text, int textLength) => throw new NotImplementedException();
        public override void FreeData() => throw new NotImplementedException();

        public float TableLookup(float index) => throw new NotImplementedException();

        bool clamp;
        bool snap;
        List<float> values = new();
    }
}
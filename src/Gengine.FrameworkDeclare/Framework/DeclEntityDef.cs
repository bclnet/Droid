using System;
using System.Collections.Generic;

namespace Gengine.Framework
{
    public class DeclEntityDef : Decl
    {
        public Dictionary<string, string> dict = new();

        public override int Size() => throw new NotImplementedException();
        public override string DefaultDefinition() => throw new NotImplementedException();
        public override bool Parse(string text, int textLength) => throw new NotImplementedException();
        public override void FreeData() => throw new NotImplementedException();
        public override void Print() => throw new NotImplementedException();
    }
}
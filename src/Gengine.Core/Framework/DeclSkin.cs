using Gengine.Render;
using System;
using System.Collections.Generic;

namespace Gengine.Framework
{
    public struct skinMapping
    {
        public Material from;          // 0 == any unmatched shader
        public Material to;
    }

    public class DeclSkin : Decl
    {
        public override int Size() => throw new NotImplementedException();
        public override bool SetDefaultText() => throw new NotImplementedException();
        public override string DefaultDefinition() => throw new NotImplementedException();
        public override bool Parse(string text, int textLength) => throw new NotImplementedException();
        public override void FreeData() => throw new NotImplementedException();

        public Material RemapShaderBySkin(Material shader) => throw new NotImplementedException();

        // model associations are just for the preview dialog in the editor
        public int GetNumModelAssociations() => throw new NotImplementedException();
        public string GetAssociatedModel(int index) => throw new NotImplementedException();

        List<skinMapping> mappings = new();
        List<string> associatedModels = new();
    }
}

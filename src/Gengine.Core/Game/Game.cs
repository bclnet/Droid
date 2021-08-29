using Gengine.Render;
using System.Collections.Generic;
using System.NumericsX;

namespace Gengine.Game
{
    public interface IGame
    {
        void CacheDictionaryMedia(Dictionary<string, string> dict);
    }

    public interface IGameEdit
    {
        int ANIM_GetLength(object modelAnim);
        object ANIM_GetAnimFromEntityDef(string animClass, string animName);
        void ParseSpawnArgsToRenderLight(Dictionary<string, string> spawnArgs, RenderLight rLight);
        void ParseSpawnArgsToRenderEntity(Dictionary<string, string> spawnArgs, RenderEntity worldEntity);
        void ANIM_CreateAnimFrame(IRenderModel hModel, object modelAnim, int numJoints, JointMat[] joints, int v1, Vector3 origin, bool v2);
    }
}
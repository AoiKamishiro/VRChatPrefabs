#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System.Linq;
using UnityEngine.SceneManagement;
using VRC.SDKBase.Editor.BuildPipeline;

namespace online.kamishiro.vrc.udon.gklog
{
    class OptimizeOnBuild : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 0;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType == VRCSDKRequestedBuildType.Avatar) return true;
            SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<PlayerLogSystem>()).ToList().ForEach(x => x.SetMainCam());
            return true;
        }
    }
}
#endif
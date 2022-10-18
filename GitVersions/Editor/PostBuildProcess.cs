using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace BicLib.GitVersions{
    public class PostBuildProcess : MonoBehaviour
    {
        [PostProcessBuild(0)]
        public static void PostProcessBuild(BuildTarget buildTarget, string _pathToBuiltProject){
            var _tag = buildTarget.ToString()+"_"+BicLib.BuildUtil.BuildUtil.GetBuildString();
            GitUtil.Command("tag " + _tag);
            Debug.Log("[BuildUtil] git tag " + _tag);
        }
    }
}
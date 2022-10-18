using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace BicLib.GitVersions{
    public class PostBuildProcess : MonoBehaviour
    {
        [PostProcessBuild(200)]
        public static void PostProcessBuild(BuildTarget buildTarget, string _pathToBuiltProject){
            var _gitStatus = GitUtil.Command("status");

            if(_gitStatus.Contains("nothing to commit") == false){
                GitUtil.Command("add .");
                GitUtil.Command("commit -m \"update resource files auto\"");
                Debug.Log("[BuildUtil] auto commit\n"+_gitStatus);    
            }

            var _tag = buildTarget.ToString()+"_"+BicLib.BuildUtil.BuildUtil.GetBuildString();
            GitUtil.Command("tag " + _tag);
            Debug.Log("[BuildUtil] git tag " + _tag);
        }
    }
}
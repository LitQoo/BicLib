using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;

namespace BicLib.GitVersions{
    public class PostBuildProcess : MonoBehaviour
    {
        [PostProcessBuild(0)]
        public static void PostProcessBuild(BuildTarget buildTarget, string _pathToBuiltProject){
            GitUtil.Command("");

            
            // Causes a log in the Unity editor, but the build still succeeds
            //throw new BuildFailedException("Forced fail");
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace BicLib.GitVersions{
    public class PreBuildProcess : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var _gitStatus = GitUtil.Command("status");

            if(_gitStatus.Contains("nothing to commit") == false){
                Debug.LogError(_gitStatus);
                throw new BuildFailedException("Can't build, commit first");
            }
        }
    }
}
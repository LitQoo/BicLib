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
                if(_gitStatus.Contains("Changes not staged for commit") == true){
                    Debug.LogError(_gitStatus);
                    throw new BuildFailedException("Can't build, commit first");
                }

                if(_gitStatus.Split('\n').Length <= 11 && _gitStatus.Contains("nothing added to commit but untracked files present") == true && _gitStatus.Contains("UnityServicesProjectConfiguration.json") == true){
                    
                }else{
                    Debug.LogError(_gitStatus);
                    throw new BuildFailedException("Can't build, commit first");
                }
            }
        }
    }
}
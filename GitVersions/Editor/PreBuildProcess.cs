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
        static public bool SouldCommit = false; 
        static public string CommitMessage = "update resource files auto";
        public void OnPreprocessBuild(BuildReport report)
        {
            var _gitStatus = GitUtil.Command("status");
            SouldCommit = false;
            CommitMessage = "update resource files auto";
            if(_gitStatus.Contains("nothing to commit") == false){
                if(_gitStatus.Contains("Changes not staged for commit") == true || (_gitStatus.Split('\n').Length <= 11 && _gitStatus.Contains("nothing added to commit but untracked files present") == true && _gitStatus.Contains("UnityServicesProjectConfiguration.json") == true) == false){
                    int option = EditorUtility.DisplayDialogComplex(
                        "Git warning",                 // 제목
                        "커밋하지 않은 내용이 있습니다.", // 메시지
                        "커밋 후 빌드",                          // 첫 번째 버튼 텍스트
                        "커밋하지 않고 빌드",                          // 두 번째 버튼 텍스트
                        "취소"                           // 세 번째 버튼 텍스트
                    );
                    
                    if(option == 2){
                        Debug.LogError(_gitStatus);
                        throw new BuildFailedException("Can't build, commit first");
                    }else if(option == 0){
                        SouldCommit = true;
                        
                        var name = EditorCommitDialog.Show( "Commit", _gitStatus, "update resource files auto");
                        if(string.IsNullOrEmpty(name) == true){
                          throw new BuildFailedException("Cancel build");
                        }

                        CommitMessage = name;
                    }
                }
            }
        }
    }
}
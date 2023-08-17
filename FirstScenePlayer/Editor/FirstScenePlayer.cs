using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

namespace BicLib.FirstScenePlayer{

    [InitializeOnLoad]
    public class FirstScenePlayer
    {
        static class ToolbarStyles
        {
            public static readonly GUIStyle commandButtonStyle;

            public static readonly GUIStyle activeButtonStyle;

            static ToolbarStyles()
            {
                commandButtonStyle = new GUIStyle("Command")
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Normal
                };

                activeButtonStyle = new GUIStyle("Command")
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Bold,
                    
                };

                activeButtonStyle.normal.textColor = Color.blue;
            }
        }

        static FirstScenePlayer()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            EditorApplication.playModeStateChanged += onChangedPlayMode;
        }

        private static void onChangedPlayMode(PlayModeStateChange mode)
        {
            if(mode == PlayModeStateChange.EnteredPlayMode){
                EditorSceneManager.playModeStartScene = null;
            }

            if(mode == PlayModeStateChange.EnteredEditMode){
                isPlayByZero = false;
            }
        }

        static bool isPlayByZero = false;
        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            GUILayout.Label("v"+PlayerSettings.bundleVersion + "(" + PlayerSettings.Android.bundleVersionCode +")");
            if(GUILayout.Button(new GUIContent("1►", "Play Default Scene"), ToolbarStyles.commandButtonStyle))
            {
                if(EditorApplication.isPlaying == true){
                    EditorApplication.isPlaying = false;
                }else{
                    if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        var pathOfFirstScene = EditorBuildSettings.scenes[0].path;
                        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathOfFirstScene);
                        EditorSceneManager.playModeStartScene = sceneAsset;
                        EditorApplication.isPlaying = true;
                        isPlayByZero = true;
                    }
                }
            }
        }
    }
}
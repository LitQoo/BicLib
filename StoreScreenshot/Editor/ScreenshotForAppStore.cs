using System;
using System.Collections;
using System.IO;
using System.Reflection;
using BicUtil.CameraScaler;
using Kyusyukeigo.Helper;
using UnityEditor;
using UnityEngine;
using UnityExtensions;

namespace ScreenshotForAppStore
{
    /// <summary>
    /// The Unity editor extension to capture screenshots for App Store 
    /// </summary>
    [InitializeOnLoadAttribute]
    public class ScreenshotForAppStore : Editor
    {

        static ScreenshotForAppStore(){
            EditorApplication.playModeStateChanged += resetNumber;
        }
        
        static int number = 0;
        static string dir = "";
        private static void resetNumber(PlayModeStateChange obj)
        {
            if(obj == PlayModeStateChange.EnteredPlayMode){
                number = 0;
                dir = DateTime.Now.ToString(PlayerSettings.Android.bundleVersionCode + "_yyyyMMdd_HHmmss");
            }
        }

        class GameViewSize : GameViewSizeHelper.GameViewSize
        {
            internal GameViewSize(int width, int height, string baseText)
            {
                type = GameViewSizeHelper.GameViewSizeType.FixedResolution;
                this.width = width;
                this.height = height;
                this.baseText = baseText;
            }

            public GameViewSize ToLandScape(){
                return new GameViewSize(this.height, this.width, this.baseText);
            }
        }

        static readonly GameViewSize[] _customSizes = {
            new GameViewSize(320, 640, "sample"),
            new GameViewSize(1242, 2208, "Appstore_5.5"),
            new GameViewSize(1242, 2688, "Appstore_6.5"),
            new GameViewSize(2048, 2732, "Appstore_12.9"),
            new GameViewSize(720, 1280, "GooglePlay_16v9_720p"),
            new GameViewSize(1080, 1920, "GooglePlay_16v9_1080p"),
        };

        static IEnumerator CaptureScreenshot(int number, string _directory)
        {

            string directoryName = "Screenshots";

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            directoryName = Path.Combine(directoryName, _directory);

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            
            var editorWindowAssembly = typeof(EditorWindow).Assembly;
            var currentSizeGroupType = GetCurrentSizeGroupType(editorWindowAssembly);
            var gameViewType = editorWindowAssembly.GetType("UnityEditor.GameView");
            var gameViewWindow = EditorWindow.GetWindow(gameViewType);
            bool isLandscape = Screen.height < Screen.width;
            var lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            foreach (var customSize in _customSizes)
            {
                var _size = customSize;
                var _name = customSize.baseText; 

                if(isLandscape == true){
                    _size = customSize.ToLandScape();
                    _name = _name + "_Landscape";
                }

                if (!GameViewSizeHelper.Contains(currentSizeGroupType, _size))
                {
                    GameViewSizeHelper.AddCustomSize(currentSizeGroupType, _size);
                }

                GameViewSizeHelper.ChangeGameViewSize(currentSizeGroupType, _size);

                var filename = Path.Combine(directoryName, $"{_name}_{number}.png");
                EditorApplication.Step();
                EditorApplication.Step();
                ScreenCapture.CaptureScreenshot(filename);
                gameViewWindow.Repaint();
                Debug.Log($">> ScreenshotForAppStore : save to {filename}");
                yield return null;
            }

            GameViewSizeHelper.ChangeGameViewSizeWithoutName(currentSizeGroupType, new GameViewSize(lastScreenSize.x, lastScreenSize.y, "origin"));
        }

        static GameViewSizeGroupType GetCurrentSizeGroupType(Assembly assembly)
        {
            var gameViewType = assembly.GetType("UnityEditor.GameView");
            var currentSizeGroupType = gameViewType.GetProperty("currentSizeGroupType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            return (GameViewSizeGroupType)currentSizeGroupType.GetValue(EditorWindow.GetWindow(gameViewType), null);
        }

        #region MenuItem methods
        [MenuItem("BicLib/CaptureScreenshot #%e", false, 201)]
        static void CaptureScreenshot1(){
            EditorCoroutine.Start(CaptureScreenshot(++number, dir));
        }
        #endregion
    }
}

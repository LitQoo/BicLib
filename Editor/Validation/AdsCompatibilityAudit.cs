using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BicDB.Container;
using BicUtil.Ads;
using BicUtil.TableView;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;

namespace BicLib.Validation
{
    public static class AdsCompatibilityAudit
    {
        [UnityEditor.Callbacks.PostProcessBuild(199)]
        private static void PreserveAutomatedBuildReview(BuildTarget target, string path)
        {
            if (Application.isBatchMode) BicLib.GitVersions.PreBuildProcess.SouldCommit = false;
        }

        public static void AuditAndBuildDeviceDemo()
        {
            Run();
            BuildIOS(true);
        }

        public static void IOS() { BuildIOS(false); }

        private static void BuildIOS(bool demo)
        {
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.appleDeveloperTeamID = "UQ99S96S75";
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            Build(BuildTarget.iOS, demo ? "ios-demo" : "ios", demo);
        }

        public static void Android()
        {
            string password = Environment.GetEnvironmentVariable("BICLIB_KEYSTORE_PASSWORD");
            Require(!string.IsNullOrEmpty(password), "BICLIB_KEYSTORE_PASSWORD is required.");
            var keyPath = Environment.GetEnvironmentVariable("BICLIB_KEYSTORE_PATH");
            if (!string.IsNullOrEmpty(keyPath)) PlayerSettings.Android.keystoreName = keyPath;
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystorePass = password;
            PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("BICLIB_KEY_PASSWORD") ?? password;
            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            Build(BuildTarget.Android, "app.aab", false);
        }

        private static void Build(BuildTarget target, string name, bool demo)
        {
            var root = Environment.GetEnvironmentVariable("BICLIB_RELEASE_ROOT");
            Require(!string.IsNullOrEmpty(root), "BICLIB_RELEASE_ROOT is required.");
            Directory.CreateDirectory(root);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                target = target, locationPathName = Path.Combine(root, name),
                options = demo ? BuildOptions.Development : BuildOptions.None,
                extraScriptingDefines = demo ? new[] { "BICLIB_AD_VALIDATION" } : Array.Empty<string>()
            });
            Debug.Log($"BICLIB_BUILD_RESULT: {target} {report.summary.result}, errors={report.summary.totalErrors}");
            Require(report.summary.result == BuildResult.Succeeded && report.summary.totalErrors == 0,
                "Build failed: " + target);
        }

        public static void Run()
        {
            ValidateInitialization();
            ValidateTables();
            Debug.Log("BICLIB_AD_COMPATIBILITY_PASS: " + Application.identifier);
        }

        private static void Require(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException(message);
        }

        private static void ValidateInitialization()
        {
            var manager = new AdsManager();
            var first = new DelayedPlatform();
            var second = new DelayedPlatform();
            manager.AddAdsPlatform(first);
            manager.AddAdsPlatform(second);
            manager.Initialize();
            Require(first.Starts == 0 && second.Starts == 0, "Lifecycle hook must not be mistaken for SDK initialization.");
            manager.Initialize(true, () =>
            {
                manager.LoadInterstitial("int");
                manager.LoadRewardBased("reward");
                manager.CreateBanner("banner", Color.black, _ => { });
            });
            Require(first.Starts == 1 && second.Starts == 1, "Each SDK must start exactly once.");
            Require(first.Loads == 0 && second.Loads == 0, "Ads loaded before initialization.");
            first.Complete();
            Require(first.Loads == 0 && second.Loads == 0, "Preload did not wait for all platforms.");
            second.Complete();
            Require(first.Loads == 2 && second.Loads == 2 && first.Banners == 1,
                "Both full-screen formats and banner must be prepared after initialization.");
            Debug.Log("BICLIB_STARTUP_PASS: delayed SDK callbacks and banner ordering.");
        }

        private static void ValidateTables()
        {
            int rows = 0, tables = 0;
            var paths = AssetDatabase.GetAllAssetPaths().Where(p =>
                (p.StartsWith("Assets/") || p.StartsWith("Packages/")) &&
                (p.EndsWith(".prefab") || p.EndsWith(".unity"))).ToArray();
            foreach (var path in paths)
            {
                if (path.EndsWith(".unity"))
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(path) != typeof(SceneAsset)) continue;
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    foreach (var root in scene.GetRootGameObjects()) Inspect(root, path, ref rows, ref tables);
                }
                else
                {
                    var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (root != null) Inspect(root, path, ref rows, ref tables);
                }
            }
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            foreach (var orientation in new[] { "Vertical", "Horizontal" })
            {
                var suffix = "/Runtime/TableView/TableView" + orientation + ".prefab";
                var path = paths.Single(p => p.EndsWith(suffix));
                var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                try
                {
                    var table = root.GetComponent<TableView>();
                    foreach (var cell in root.GetComponentsInChildren<TableCell>(true))
                    {
                        cell.SetDataFunction = new TableCell.CellDataChanged();
                        cell.OnRemoveFunction = new TableCell.CellDataChanged();
                    }
                    var data = new List<RecordContainer>();
                    table.SetDBSource(data);
                    foreach (int count in new[] { 0, 1, 100, 0, 1, 100 })
                    {
                        data.Clear();
                        for (int i = 0; i < count; i++) data.Add(new RecordContainer());
                        table.scrollDistance = 0;
                        table.ReloadData();
                        Tick(table);
                        Require(table.isEmpty == (count == 0), "Empty table state mismatch.");
                        if (count > 0) Require(table.GetRow(0) != null, "First row missing.");
                        root.SetActive(false);
                        root.SetActive(true);
                        table.ReloadData();
                        if (count == 100)
                        {
                            foreach (float distance in new[] { 500f, 1000f, 0f })
                            {
                                table.scrollDistance = distance;
                                Tick(table);
                                foreach (var row in root.GetComponentsInChildren<TableRow>())
                                    Require(row.LayoutElement == row.GetComponent<LayoutElement>(), "Reused row layout mismatch.");
                            }
                            Require(table.GetRow(0) != null, "First row missing after reuse.");
                        }
                    }
                    Debug.Log("BICLIB_TABLE_EXERCISE_PASS: " + orientation + " empty/single/reopen/scroll/reuse.");
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            Debug.Log($"BICLIB_TABLE_AUDIT_PASS: assets={paths.Length}, tables={tables}, rows={rows}; includes inactive objects and prefab variants.");
        }

        private static void Inspect(GameObject root, string path, ref int rows, ref int tables)
        {
            tables += root.GetComponentsInChildren<TableView>(true).Length;
            foreach (var row in root.GetComponentsInChildren<TableRow>(true))
            {
                rows++;
                var actual = row.GetComponent<LayoutElement>();
                Require(actual != null, path + ": missing LayoutElement component on " + row.name);
                Require(row.LayoutElement == actual, path + ": incorrect serialized LayoutElement on " + row.name);
            }
        }

        private static void Tick(TableView table)
        {
            Canvas.ForceUpdateCanvases();
            foreach (string method in new[] { "Update", "LateUpdate" })
                typeof(TableView).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(table, null);
        }

        private sealed class DelayedPlatform : IAdsPlatform
        {
            private Action callback;
            private bool ready;
            public int Starts, Loads, Banners;
            public string TermsURL => "";
            public void Initialize(Action completed) { Starts++; callback = completed; }
            public void Complete() { ready = true; callback(); }
            public void LoadInterstitial(object placement) { Require(ready, "SDK not ready."); Loads++; }
            public void LoadRewardBased(object placement) { Require(ready, "SDK not ready."); Loads++; }
            public bool IsReadyInterstitial(object placement) => ready;
            public bool IsReadyRewardBased(object placement) => ready;
            public bool IsReadyBanner(object placement) => ready;
            public IAdsBanner CreateBanner(object placement, Color color, Action<IAdsBanner> loaded) { Banners++; return new DummyBanner(); }
            public void ShowInterstitial(object placement, Action<AdsResult> completed) => completed(AdsResult.Finished);
            public void ShowRewardBased(object placement, Action<AdsResult> completed) => completed(AdsResult.Skipped);
            public void SetUserConsent(bool enabled) { }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace Editor {
  [Obsolete("Obsolete")]
  public class Builder : IPreprocessBuild {
    private static bool BuildComlete;
    private static readonly string[] Secrets = {"androidKeystorePass", "androidKeyaliasName", "androidKeyaliasPass"};
    private static readonly string Eol = Environment.NewLine;

    private static Dictionary<string, string> GetValidatedOptions() {
        ParseCommandLineArguments(out Dictionary<string, string> validatedOptions);

        if(!validatedOptions.TryGetValue("projectPath", out string _)) {
            Console.WriteLine("Missing argument -projectPath");
            EditorApplication.Exit(110);
        }

        if(!validatedOptions.TryGetValue("buildTarget", out string buildTarget)) {
            Console.WriteLine("Missing argument -buildTarget");
            EditorApplication.Exit(120);
        }

        if(!Enum.IsDefined(typeof(BuildTarget), buildTarget ?? string.Empty)) {
            EditorApplication.Exit(121);
        }

        if(!validatedOptions.TryGetValue("customBuildPath", out string _)) {
            Console.WriteLine("Missing argument -customBuildPath");
            EditorApplication.Exit(130);
        }

        const string defaultCustomBuildName = "TestBuild";
        if(!validatedOptions.TryGetValue("customBuildName", out string customBuildName)) {
            Console.WriteLine($"Missing argument -customBuildName, defaulting to {defaultCustomBuildName}.");
            validatedOptions.Add("customBuildName", defaultCustomBuildName);
        } else if(customBuildName == "") {
            Console.WriteLine($"Invalid argument -customBuildName, defaulting to {defaultCustomBuildName}.");
            validatedOptions.Add("customBuildName", defaultCustomBuildName);
        }

        return validatedOptions;
    }

    private static void ParseCommandLineArguments(out Dictionary<string, string> providedArguments) {
        providedArguments = new Dictionary<string, string>();
        string[] args = Environment.GetCommandLineArgs();

        Console.WriteLine(
            $"{Eol}" +
            $"###########################{Eol}" +
            $"#    Parsing settings     #{Eol}" +
            $"###########################{Eol}" +
            $"{Eol}"
        );

        // Extract flags with optional values
        for(int current = 0, next = 1; current < args.Length; current++, next++) {
            // Parse flag
            bool isFlag = args[current].StartsWith("-");
            if(!isFlag) continue;
            string flag = args[current].TrimStart('-');

            // Parse optional value
            bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
            string value = flagHasValue ? args[next].TrimStart('-') : "";
            bool secret = Secrets.Contains(flag);
            string displayValue = secret ? "Hide" : "\"" + value + "\"";

            // Assign
            Console.WriteLine($"Found flag \"{flag}\" with value {displayValue}.");

            if(value == "") {
              Console.WriteLine("value is empty: " + flag);
              continue;
            }

            providedArguments.Add(flag, value);
        }
    }

    [MenuItem("Builds/BuildWebGL")]
    public static void BuildWeb() {
      BuildComlete = false;
      EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
      SetVrPlatform(false);

      var options = new BuildPlayerOptions();

      options.scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
      options.target = BuildTarget.WebGL;
      options.options = BuildOptions.None;
      options.locationPathName = "builds/WebGL/WebGL";
      BuildPipeline.BuildPlayer(options);
    }

    [MenuItem("Builds/BuildVr")]
    public static void BuildVr() {
      SetVrPlatform(true);
      string [] activeSceneList = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
      BuildAndroid(activeSceneList);
    }

    public static void BuildVrWithKey() {
      SetKeys();
      SetVrPlatform(true);
      string[] activeSceneList = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
      BuildAndroid(activeSceneList);
    }

    [MenuItem("Builds/BuildAndroidTelephone")]
    public static void BuildAndroidTelephone() {
      BuildAndroid(EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes));
    }

    private static void AssemblyVr() {
      BuildComlete = false;
      string [] activeSceneList = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

      SetVrPlatform(true);

      //EditorCoroutineUtility.StartCoroutineOwnerless(LevelsReserializer.ProcessScenes(activeSceneList.Select(scene => AssetDatabase.LoadAssetAtPath<SceneAsset>(scene)).ToList(), true));
      //LevelsReserializer.ProcessScenes(activeSceneList.Select(scene => AssetDatabase.LoadAssetAtPath<SceneAsset>(scene)).ToList(), true);
      //yield return new WaitUntil(() => LevelsReserializer.LoadingPrefabComplete);

      BuildAndroid(activeSceneList);

      //yield return new WaitUntil(() => BuildComlete);
      //LevelsReserializer.ProcessScenes(activeSceneList.Select(scene => AssetDatabase.LoadAssetAtPath<SceneAsset>(scene)).ToList(), false);
      //EditorCoroutineUtility.StartCoroutineOwnerless(LevelsReserializer.ProcessScenes(activeSceneList.Select(scene => AssetDatabase.LoadAssetAtPath<SceneAsset>(scene)).ToList(), false));
    }

    private static void SetKeys() {
        Dictionary<string, string> Options = GetValidatedOptions();

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.bundleVersionCode += 1;
        if(Options.TryGetValue("androidKeystoreName", out string keystoreName) && !string.IsNullOrEmpty(keystoreName))
          PlayerSettings.Android.keystoreName = keystoreName;
        if(Options.TryGetValue("androidKeystorePass", out string keystorePass) && !string.IsNullOrEmpty(keystorePass))
            PlayerSettings.Android.keystorePass = keystorePass;
        if(Options.TryGetValue("androidKeyaliasName", out string keyaliasName) && !string.IsNullOrEmpty(keyaliasName))
            PlayerSettings.Android.keyaliasName = keyaliasName;
        if(Options.TryGetValue("androidKeyaliasPass", out string keyaliasPass) && !string.IsNullOrEmpty(keyaliasPass))
            PlayerSettings.Android.keyaliasPass = keyaliasPass;
    }

    private static void BuildAndroid (string [] scenes) {
      //EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

      var options = new BuildPlayerOptions();

      options.scenes = scenes;
      options.target = BuildTarget.Android;
      options.options = BuildOptions.None;
      options.locationPathName = "artifacts/buildka.apk";
      BuildPipeline.BuildPlayer(options);
    }

    private static void SetVrPlatform (bool isVr) {
      AssetDatabase.StartAssetEditing();

      AssetDatabase.SaveAssets();
      AssetDatabase.StopAssetEditing();
    }

    private const string PathToSettings = "Assets/Resources/BuildPlatform.asset";

    public void OnPreprocessBuild (BuildTarget target, string path) {
      BuildComlete = true;
    }

    public int callbackOrder { get; }
  }
}
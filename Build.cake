#addin nuget:?package=Cake.Unity&version=0.9.0

var target = Argument("target", "Build-Android");

Task("Clean-Artifacts")
    .Does(() =>
{
    CleanDirectory($"./artifacts");
});

using static Cake.Unity.Arguments.BuildTarget;

Task("Build-Android")
    .IsDependentOn("Clean-Artifacts")
    .Does(() =>
{
    UnityEditor(2022, 1,
        new UnityEditorArguments
        {
            ExecuteMethod = "Editor.Builder.BuildVr",
            LogFile = "./artifacts/unity.log",
        },
        new UnityEditorSettings
        {
            RealTimeLog = true,
        });
});

RunTarget(target);
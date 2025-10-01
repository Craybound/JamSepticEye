using UnityEditor;
using UnityEngine;

public static class SceneViewSnapper
{
    [MenuItem("Tools/Snap Scene View/Top View %&t")] // Ctrl+Alt+T
    public static void SnapTop()
    {
        SnapView(Vector3.down);
    }

    [MenuItem("Tools/Snap Scene View/Front View %&f")] // Ctrl+Alt+F
    public static void SnapFront()
    {
        SnapView(Vector3.back);
    }

    [MenuItem("Tools/Snap Scene View/Side View %&s")] // Ctrl+Alt+S
    public static void SnapSide()
    {
        SnapView(Vector3.left);
    }

    private static void SnapView(Vector3 direction)
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null) return;

        sceneView.LookAt(sceneView.pivot, Quaternion.LookRotation(direction), sceneView.size);
    }
}

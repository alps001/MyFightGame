
using UnityEditor;
using UnityEngine;

public class InputEditorWindow : EditorWindow
{
    public static InputEditorWindow inputEditorWindow;
    [MenuItem("Window/MyFightGameConfig/Input Editor")]
    public static void Init()
    {
        inputEditorWindow = EditorWindow.GetWindow<InputEditorWindow>(false, "Global", true);
        inputEditorWindow.Show();
        
    }

    public void OnGUI()
    {
        if (GUILayout.Button("Create new Input Configuration"))
            ScriptableObjectUtility.CreateAsset<InputConfig>();
    }
}

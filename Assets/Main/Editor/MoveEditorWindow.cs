
using UnityEditor;
using UnityEngine;

namespace MyFightGame { 
    public class MoveEditorWindow : EditorWindow
    {
        public static MoveEditorWindow moveEditorWindow;
        [MenuItem("Window/MyFightGameConfig/MoveInfo Editor")]
        public static void Init()
        {
            moveEditorWindow = EditorWindow.GetWindow<MoveEditorWindow>(false, "MoveInfo", true);
            moveEditorWindow.Show();
        
        }

        public void OnGUI()
        {
            if (GUILayout.Button("Create new MoveInfo Configuration"))
                ScriptableObjectUtility.CreateAsset<MoveInfo>();
        }
    }
}
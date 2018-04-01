
using UnityEditor;
using UnityEngine;

namespace MyFightGame { 
    public class CharacterEditorWindow : EditorWindow
    {
        public static CharacterEditorWindow characterEditorWindow;
        [MenuItem("Window/MyFightGameConfig/CharacterInfo Editor")]
        public static void Init()
        {
            characterEditorWindow = EditorWindow.GetWindow<CharacterEditorWindow>(false, "Character", true);
            characterEditorWindow.Show();
        
        }

        public void OnGUI()
        {
            if (GUILayout.Button("Create new CharacterInfo Configuration"))
                ScriptableObjectUtility.CreateAsset<CharacterInfo>();
        }
    }
}
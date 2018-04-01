using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace MyFightGame { 

public class GlobalEditorWindow : EditorWindow {
	public static GlobalEditorWindow globalEditorWindow;
	

	[MenuItem("Window/MyFightGameConfig/Global Editor")]
        public static void Init()
        {
            globalEditorWindow = EditorWindow.GetWindow<GlobalEditorWindow>(false, "Global", true);
            globalEditorWindow.Show();

        }

        public void OnGUI()
        {
            if (GUILayout.Button("Create new Global Configuration"))
                ScriptableObjectUtility.CreateAsset<GlobalInfo>();
        }
    }
}
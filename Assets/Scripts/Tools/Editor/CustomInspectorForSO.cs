#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Object), true)]
public class CustomInspectorForSO : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(target is ITableSO itable)
        {
            if(GUILayout.Button("Press to Initialize Data from CSV"))
            {
                itable.DataInitialize();
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
                
        }
    }
}
#endif
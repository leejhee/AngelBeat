using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace novel
{
    public class NovelCharacterProvider : SettingsProvider
    {
        SerializedObject novelChar;
        private string path = NovelEditorUtils.GetNovelResourceDataPath(NovelDataType.Character);

        private bool showCharacter = false;
        //private string currentCharacter = "";
        private NovelCharacterSO currentCharacterData = null;

        public NovelCharacterProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            novelChar = NovelEditorUtils.GetSerializedSettings<NovelCharacterData>(path);
        }

        public override void OnGUI(string searchContext)
        {
            if (novelChar == null)
                novelChar = NovelEditorUtils.GetSerializedSettings<NovelCharacterData>(path);
            Texture icon = EditorGUIUtility.FindTexture("d_UnityEditor.AnimationWindow");

            novelChar.Update();


            if (!showCharacter)
            {
                EditorGUILayout.LabelField("Character List");
                var charProp = novelChar.FindProperty("charDict").FindPropertyRelative("pairs");
                EditorGUI.indentLevel++;
                for (int i = 0; i < charProp.arraySize; i++)
                {
                    var element = charProp.GetArrayElementAtIndex(i);
                    var keyProp = element.FindPropertyRelative("key");
                    var valueProp = element.FindPropertyRelative("value");

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(5);

                    EditorGUILayout.PropertyField(keyProp, GUIContent.none);
                    // 캐릭터 이름 바뀌면 연동된 ScriptableObject 이름도 변경
                    if (GUI.changed)
                    {
                        if (valueProp.objectReferenceValue != null)
                        {
                            var charSO = valueProp.objectReferenceValue as NovelCharacterSO;
                            if (charSO != null && charSO.characterName != keyProp.stringValue)
                            {
                                charSO.characterName = keyProp.stringValue;
                                EditorUtility.SetDirty(charSO);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }
                        }
                    }

                    if (GUILayout.Button(new GUIContent(icon), GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        // 클릭시 해당 캐릭터 상세보기
                        if (valueProp != null)
                        {
                            currentCharacterData = valueProp.objectReferenceValue as NovelCharacterSO;

                            if (currentCharacterData == null)
                            {
                                string assetPath = $"Assets/NovelEngine/Addressable/CharacterData/{keyProp.stringValue}.asset";
                                var existingAsset = AssetDatabase.LoadAssetAtPath<NovelCharacterSO>(assetPath);
                                if (existingAsset != null)
                                {
                                    currentCharacterData = existingAsset;
                                    valueProp.objectReferenceValue = currentCharacterData;
                                    novelChar.ApplyModifiedPropertiesWithoutUndo();
                                    showCharacter = true;
                                    //currentCharacter = keyProp.stringValue;
                                    return;
                                }
                                else
                                {
                                    // 새로운 캐릭터 생성
                                    var newChar = ScriptableObject.CreateInstance<NovelCharacterSO>();
                                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                                    AssetDatabase.CreateAsset(newChar, assetPath);
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();

                                    currentCharacterData = newChar;
                                    valueProp.objectReferenceValue = currentCharacterData;
                                    novelChar.ApplyModifiedPropertiesWithoutUndo();
                                    showCharacter = true;
                                    //currentCharacter = keyProp.stringValue;
                                    return;
                                }
                            }
                            else
                            {
                                showCharacter = true;
                            }

                        }
                        else
                        {
                            Debug.LogError("valueProp is null");
                        }


                            
                    }
                    GUILayout.Space(5);

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);

                if (GUILayout.Button("Add New Character"))
                {
                    string newCharName = "NewCharacter";
                    int suffix = 1;

                    bool keyExists;
                    do
                    {
                        keyExists = false;
                        for (int i = 0; i < charProp.arraySize; i++)
                        {
                            var element = charProp.GetArrayElementAtIndex(i);
                            var keyProp = element.FindPropertyRelative("key");
                            if (keyProp.stringValue == newCharName)
                            {
                                keyExists = true;
                                newCharName = $"NewCharacter{suffix}";
                                suffix++;
                                break;
                            }
                        }
                        if (keyExists)
                        {
                            newCharName = $"NewCharacter{suffix}";
                        }
                    }while (keyExists);

                    charProp.arraySize++;
                    var newElement = charProp.GetArrayElementAtIndex(charProp.arraySize - 1);
                    newElement.FindPropertyRelative("key").stringValue = newCharName;

                    string assetPath = $"Assets/NovelEngine/Addressable/CharacterData/{newCharName}.asset";
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    var newCharSO = ScriptableObject.CreateInstance<NovelCharacterSO>();

                    // 캐릭터 이름 초기화
                    newCharSO.characterName = newCharName;

                    AssetDatabase.CreateAsset(newCharSO, assetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    newElement.FindPropertyRelative("value").objectReferenceValue = newCharSO;

                    novelChar.ApplyModifiedPropertiesWithoutUndo();
                }

                GUILayout.Space(5);

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Back To Character List", GUILayout.Height(25)))
                {
                    showCharacter = false;
                    //currentCharacter = "";
                    currentCharacterData = null;
                    return;
                }

                // 캐릭터 이름
                GUIStyle boldLargeLabel = new GUIStyle(EditorStyles.label);
                boldLargeLabel.fontSize = 20;                 // 글자 크기
                boldLargeLabel.fontStyle = FontStyle.Bold;    // 볼드체
                boldLargeLabel.normal.textColor = Color.white; // 글자색 (옵션)
                GUILayout.Space(10);
                EditorGUILayout.LabelField(currentCharacterData.characterName, boldLargeLabel);

                currentCharacterData.novelName = EditorGUILayout.TextField("Display Name", currentCharacterData.novelName);
                currentCharacterData.body = (Sprite)EditorGUILayout.ObjectField("Body Sprite", currentCharacterData.body, typeof(Sprite), false);
                currentCharacterData.headOffset = EditorGUILayout.Vector2Field("Head Offset", currentCharacterData.headOffset);
                currentCharacterData.faceDict = SerializableDictDrawer.DrawSerializableDict(currentCharacterData.faceDict, "Face Expressions");


                // 캐릭터 삭제
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                Vector2 buttonSize = buttonStyle.CalcSize(new GUIContent("Delete Character"));
                if (GUILayout.Button("Delete Character", buttonStyle, GUILayout.Width(buttonSize.x), GUILayout.Height(25)))
                {
                    var charDictProp = novelChar.FindProperty("charDict").FindPropertyRelative("pairs");

                    for (int i = 0; i < charDictProp.arraySize; i++)
                    {
                        var element = charDictProp.GetArrayElementAtIndex(i);
                        var keyProp = element.FindPropertyRelative("key");
                        var valueProp = element.FindPropertyRelative("value");

                        if (keyProp.stringValue == currentCharacterData.characterName)
                        {
                            // 에셋 삭제
                            var charSO = valueProp.objectReferenceValue as NovelCharacterSO;    // 현재 참조된 ScriptableObject

                            if (charSO != null)
                            {
                                string assetPath = AssetDatabase.GetAssetPath(charSO);
                                if (string.IsNullOrEmpty(assetPath))
                                {
                                    Debug.LogError("Failed to get asset path for deletion.");
                                    return;
                                }
                                else
                                {
                                    AssetDatabase.DeleteAsset(assetPath);
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();
                                }
                            }

                            // 딕셔너리에서 삭제
                            valueProp.objectReferenceValue = null;  // 참조 해제
                            charDictProp.DeleteArrayElementAtIndex(i);  // 요소 삭제

                            novelChar.ApplyModifiedPropertiesWithoutUndo();
                            break;
                        }
                    }
                    showCharacter = false;
                    //currentCharacter = "";
                    currentCharacterData = null;

                }
                GUILayout.Space(5);
                EditorGUILayout.EndHorizontal();
            }

                novelChar.ApplyModifiedPropertiesWithoutUndo();
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new NovelCharacterProvider("Project/Novel/Character", SettingsScope.Project)
            {
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Character" })
            };
        }
    }

}

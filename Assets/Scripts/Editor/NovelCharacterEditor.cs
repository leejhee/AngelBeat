using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;
using System.Collections.Generic;

namespace novel
{
    public class NovelCharacterEditor : EditorWindow
    {
        private static NovelCharacterEditor instance;
        private Vector2 scrollPos;
        private NovelCharacter character;

        [MenuItem("Novel/Character")]
        private static void ShowWindow()
        {
            instance = GetWindow<NovelCharacterEditor>("Edit Character");
        }
        private void OnGUI()
        {
            GUILayout.Label("Novel Character Editor", EditorStyles.boldLabel);
            GUILayout.Space(5);
            GUILayout.Label("캐릭터 이름, 스탠딩 설정");

            character = (NovelCharacter)EditorGUILayout.ObjectField("Character", character, typeof(NovelCharacter), false);


            if (character == null)
            {
                EditorGUILayout.HelpBox("캐릭터 SO를 선택해주세요.", MessageType.Warning);
                return;
            }

            if (character.bodySpirtes == null)
            {
                character.bodySpirtes = new();
            }
            if (character.faceSprites == null)
            {
                character.faceSprites = new();
            }
            if (character.effectSprites == null)
            {
                character.effectSprites = new();
            }

            string newCharacterName = EditorGUILayout.TextField("Character Name", character.characterName);
            if (newCharacterName != character.characterName && !string.IsNullOrWhiteSpace(newCharacterName))
            {
                character.characterName = newCharacterName;
                AutoSave();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Label("Character Body");

            for (int i = 0; i < character.bodySpirtes.Count; i++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                GUILayout.Label($"{i + 1}", GUILayout.Width(25));

                string newBodyName = EditorGUILayout.TextArea(character.bodySpirtes[i].bodyName, GUILayout.Width(100));
                if (newBodyName != character.bodySpirtes[i].bodyName)
                {
                    character.bodySpirtes[i].bodyName = newBodyName;
                    AutoSave();
                }

                GUILayout.Space(10);

                EditorGUILayout.BeginVertical();

                Sprite newBodySprite = (Sprite)EditorGUILayout.ObjectField(character.bodySpirtes[i].bodySprite, typeof(Sprite), false);
                GUILayout.Space(50);
                if (character.bodySpirtes[i].bodySprite != null)
                {
                    GUILayout.Label(character.bodySpirtes[i].bodySprite.texture, GUILayout.Width(100), GUILayout.Height(100));
                }
                
                if (newBodySprite != character.bodySpirtes[i].bodySprite)
                {
                    character.bodySpirtes[i].bodySprite = newBodySprite;
                    AutoSave();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    character.bodySpirtes.RemoveAt(i);
                    AutoSave();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    return;
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Standing Body"))
            {
                CharacterStandingBody newBody = new();

                character.bodySpirtes.Add(newBody);
                AutoSave();
                EditorGUILayout.EndScrollView();
                return;
            }

            GUILayout.Label("Character Face");
            for (int i = 0; i < character.faceSprites.Count; i++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                GUILayout.Label($"{i + 1}", GUILayout.Width(25));

                string newFaceName = EditorGUILayout.TextArea(character.faceSprites[i].faceName, GUILayout.Width(100));
                if (newFaceName != character.faceSprites[i].faceName)
                {
                    character.faceSprites[i].faceName = newFaceName;
                    AutoSave();
                }

                GUILayout.Space(10);

                EditorGUILayout.BeginVertical();

                Sprite newFaceSprite = (Sprite)EditorGUILayout.ObjectField(character.faceSprites[i].faceSprite, typeof(Sprite), false, GUILayout.Width(200));
                GUILayout.Space(50);
                if (character.faceSprites[i].faceSprite != null)
                {
                    GUILayout.Label(character.faceSprites[i].faceSprite.texture, GUILayout.Width(100), GUILayout.Height(100));
                }

                if (newFaceSprite != character.faceSprites[i].faceSprite)
                {
                    character.faceSprites[i].faceSprite = newFaceSprite;
                    AutoSave();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    character.faceSprites.RemoveAt(i);
                    AutoSave();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    return;
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Standing Face"))
            {
                CharacterStandingFace newFace = new();

                character.faceSprites.Add(newFace);
                AutoSave();
                EditorGUILayout.EndScrollView();
                return;
            }

            GUILayout.Label("Character Effect");
            for (int i = 0; i < character.effectSprites.Count; i++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                GUILayout.Label($"{i + 1}", GUILayout.Width(25));

                string newEffectName = EditorGUILayout.TextArea(character.effectSprites[i].effectName, GUILayout.Width(100));
                if (newEffectName != character.effectSprites[i].effectName)
                {
                    character.effectSprites[i].effectName = newEffectName;
                    AutoSave();
                }

                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();
                Sprite newEffectSprite = (Sprite)EditorGUILayout.ObjectField(character.effectSprites[i].effectSprite, typeof(Sprite), false, GUILayout.Width(200));
                GUILayout.Space(50);
                if (character.effectSprites[i].effectSprite != null)
                {
                    GUILayout.Label(character.effectSprites[i].effectSprite.texture, GUILayout.Width(100), GUILayout.Height(100));
                }

                if (newEffectSprite != character.effectSprites[i].effectSprite)
                {
                    character.effectSprites[i].effectSprite = newEffectSprite;
                    AutoSave();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    character.effectSprites.RemoveAt(i);
                    AutoSave();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    return;
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Standing Effect"))
            {
                CharacterStandingEffect newEffect = new();

                character.effectSprites.Add(newEffect);
                AutoSave();
                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUILayout.EndScrollView();
        }


        private void AutoSave()
        {
            if (character == null) return;

            Undo.RecordObject(character, "Auto Save");
            EditorUtility.SetDirty(character);
            AssetDatabase.SaveAssets();
        }
    }
}


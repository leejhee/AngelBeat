#if UNITY_EDITOR
using AngelBeat.Core.SingletonObjects.Managers;
using Character.Unit;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace AngelBeat
{
    public class CharFunctionInjector : EditorWindow
    {
        private List<CharBase> characterList = new();
        private List<ExecutionData> functionDatas = new();
        private string searchFunctionTerm = "";
        private int CasterOrder = -1;
        private int TargetOrder = -1;
        private ExecutionData selectedFunction = null;

        

        Vector2 scrollPos;

        [MenuItem("InGame Tool/캐릭터에 Function 주입")]
        public static void ShowWindow()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("Unity 플레이 후 사용 바랍니다.");
                return;
            }
            EditorWindow.GetWindow(typeof(CharFunctionInjector), false, "Character Function Injector");

        }

        private void OnEnable()
        {
            if (EditorApplication.isPlaying)
            {
                characterList = BattleCharManager.Instance.GetCurrentCharacters();
                var functionlist = global::Core.SingletonObjects.Managers.DataManager.Instance.GetDataList<ExecutionData>();
                for (int i = 0; i < functionlist.Count; i++)
                {
                    var target = functionlist[i] as ExecutionData;
                    functionDatas.Add(target);
                }              
            }            
        }
        

        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("Unity 플레이 후 사용 바랍니다.");
                Close();
            }

            string[] characterNames = characterList.Select(c => $"{c.GetID()} - {c.UnitName}").ToArray();

            EditorGUILayout.LabelField("Function을 주는 캐릭터 선택", EditorStyles.boldLabel);
            CasterOrder = EditorGUILayout.Popup("캐릭터 선택", CasterOrder, characterNames);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Function을 받는 캐릭터 선택", EditorStyles.boldLabel);
            TargetOrder = EditorGUILayout.Popup("캐릭터 선택", TargetOrder, characterNames);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("주입할 Function 선택"), EditorStyles.boldLabel);
            searchFunctionTerm = EditorGUILayout.TextField("Search Field", searchFunctionTerm);

            EditorGUILayout.Space();

            var filteredFunctions = functionDatas
            .Where(f => string.IsNullOrEmpty(searchFunctionTerm) ||
                        f.executionType.ToString().Contains(searchFunctionTerm) ||
                        f.index.ToString().Contains(searchFunctionTerm) ||
                        f.executionDuration.ToString().Contains(searchFunctionTerm))
            .ToList();


            // 검색 결과 출력 (선택 가능)
            if (filteredFunctions.Count > 0)
            {
                GUIStyle headerStyle = new(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                GUIStyle rowStyle = new(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft
                };

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Index", headerStyle, GUILayout.Width(80));
                EditorGUILayout.LabelField("Function Type", headerStyle, GUILayout.Width(200));
                EditorGUILayout.LabelField("Duration", headerStyle, GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                for (int i = 0; i < filteredFunctions.Count; i++)
                {
                    ExecutionData function = filteredFunctions[i];

                    EditorGUILayout.BeginHorizontal(GUI.skin.box);

                    EditorGUILayout.LabelField(function.index.ToString(), rowStyle, GUILayout.Width(80));
                    EditorGUILayout.LabelField(function.executionType.ToString(), rowStyle, GUILayout.Width(200));
                    EditorGUILayout.LabelField(function.executionDuration.ToString(), rowStyle, GUILayout.Width(80));

                    if (GUILayout.Button("선택", GUILayout.Width(80)))
                    {
                        selectedFunction = function;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("검색 결과가 없습니다.", MessageType.Warning);
            }

            GUILayout.FlexibleSpace();

            string result = selectedFunction == null ? "버프를 선택해주세요" : selectedFunction.index.ToString() + "번 버프 적용";

            GUIStyle ButtonStyle = new(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 30, 
                alignment = TextAnchor.MiddleCenter
            };

            if (GUILayout.Button(result, ButtonStyle, GUILayout.ExpandWidth(true)) && 
                selectedFunction != null && CasterOrder > -1 && TargetOrder > -1)
            {
                var caster = characterList[CasterOrder];
                var target = characterList[TargetOrder];
                target.ExecutionInfo.AddExecution(new ExecutionParameter()
                {
                    CastChar = caster,
                    TargetChar = target,
                    ExecutionIndex = selectedFunction.index,
                    eExecutionType = selectedFunction.executionType
                });
                Debug.LogWarning($"{caster.GetID()}번 캐릭터에서 {target.GetID()}번 캐릭터에 {selectedFunction.index}번 function 주입");
            }
        }

        
    }
}

#endif
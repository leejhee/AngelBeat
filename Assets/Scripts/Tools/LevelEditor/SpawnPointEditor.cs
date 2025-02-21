#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[ExecuteInEditMode, RequireComponent(typeof(Grid))]
public class SpawnPointEditor : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemapPrefab;

    private Tilemap _currentTilemapInstance;

    [SerializeField, Header("위의 것부터 앞의 편성 순서 유닛의 스폰 위치")]
    private List<Vector3> AllySpawnPositions = new();
    [SerializeField, Header("위의 것부터 앞의 편성 순서 유닛의 스폰 위치")]
    private List<Vector3> EnemySpawnPositions = new();

    private bool _isAllyMode = false;
    private bool _isEnemyMode = false;

    private void Update()
    {
        if (!Application.isPlaying)
        {
            EnsureTilemapExists();
        }
    }

    private void EnsureTilemapExists()
    {
        if (tilemapPrefab == null) return;

        // 중복 생성 방지
        if (_currentTilemapInstance == null)
        {
            _currentTilemapInstance = Instantiate(tilemapPrefab, transform);
            _currentTilemapInstance.name = "Tilemap_Editor_Instance";
            //_currentTilemapInstance.hideFlags = HideFlags.HideAndDontSave; // 씬 저장 시 포함되지 않도록 설정
        }
    }


    

    private void OnDisable()
    {
        if (_currentTilemapInstance != null)
        {
            DestroyImmediate(_currentTilemapInstance.gameObject);
        }
    }

}
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTestScene : MonoBehaviour
{
    [SerializeField] private CharBase TestChar;
    [SerializeField] private Vector3 testPlayerPoint;
    [SerializeField] private CharBase TestEnemy;
    [SerializeField] private Vector3 testEnemyPoint;

    [SerializeField] private StageField MapPrefab;

    private void Awake()
    {
        GameManager instance = GameManager.Instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(MapPrefab);

        // TODO : 두 로직을 연결하자.

        TestChar = BattleCharManager.Instance.CharGenerate
            (new CharParameter(SystemEnum.eScene.BattleTestScene,
            testPlayerPoint,
            TestChar.Index));

        TestEnemy = BattleCharManager.Instance.CharGenerate
            (new CharParameter(SystemEnum.eScene.BattleTestScene,
            testEnemyPoint,
            TestEnemy.Index));
    }


}

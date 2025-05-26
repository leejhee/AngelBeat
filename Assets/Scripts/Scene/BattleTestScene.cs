using AngelBeat.Core;
using System.Collections.Generic;
using UnityEngine;

public class BattleTestScene : MonoBehaviour
{
    [SerializeField] private List<GameObject> BattleUI;
    
    void Awake()
    {
        foreach(var go in BattleUI)
            UIManager.Instance.ShowUI(go);
    }


}

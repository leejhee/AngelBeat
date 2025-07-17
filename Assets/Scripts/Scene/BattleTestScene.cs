using AngelBeat;
using System.Collections.Generic;
using UnityEngine;

public class BattleTestScene : MonoBehaviour
{
    [SerializeField] private List<GameObject> battleUI;
    
    void Awake()
    {
        foreach(var go in battleUI)
            UIManager.Instance.ShowUI(go);
    }


}

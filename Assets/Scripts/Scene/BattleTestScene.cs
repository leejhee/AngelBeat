using AngelBeat;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BattleTestScene : MonoBehaviour
{
    [SerializeField] private List<GameObject> battleUI;
    
    void Awake()
    {
        foreach(var go in battleUI)
            UIManager.Instance.ShowUI(go);
    }


}

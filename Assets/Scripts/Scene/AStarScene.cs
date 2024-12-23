using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarScene : MonoBehaviour
{
    void Awake()
    {
        GameManager instance = GameManager.Inst;
    }

    void Start()
    {
        UIManager.Instance.ShowSceneUI<UI_AStarScene>();
    }
}

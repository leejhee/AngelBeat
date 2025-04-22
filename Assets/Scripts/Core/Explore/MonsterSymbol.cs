using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonsterSymbol : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("나중에는 로딩으로 하세요~ 씬 바꿈.");
        SceneManager.LoadScene("BattleTestScene");
    }
}

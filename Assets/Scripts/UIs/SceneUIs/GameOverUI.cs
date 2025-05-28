using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(QuitCountDown());
    }
    
    IEnumerator QuitCountDown()
    {
        yield return new WaitForSeconds(3);
        Application.Quit();
    }
}

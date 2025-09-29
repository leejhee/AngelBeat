using novel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class NovelTest : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button button;
    // Start is called before the first frame update
    void Start()
    {
        NovelManager.Init();
        button.onClick.AddListener(OnButtonClick);


    }
    private void OnButtonClick()
    {
        button.gameObject.SetActive(false);
        NovelManager.Instance.PlayScript("test");
    }
}

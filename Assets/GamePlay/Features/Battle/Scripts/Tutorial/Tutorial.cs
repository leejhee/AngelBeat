using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public GameObject tuto;
    public Button firstMoveButton;
        
    public CharPlayer player;
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<CharPlayer>();
        //player.CharMove(new Vector2(transform.position.x + 5, transform.position.y));
    }

    // Update is called once per frame
    void Update()
    {
        // if ()
        // {
        //     Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position,new Vector2(tuto.transform.localScale.x, tuto.transform.localScale.y),0);
        //     foreach (Collider2D col in hit)
        //     {
        //         if (col.CompareTag("Tuto1"))
        //         {
        //             player.CharMove(new Vector2(transform.position.x + 5, transform.position.y));
        //         }
        //     }
        // }
    }

    public void FirstMove()
    {
        player.CharMove(new Vector2(transform.position.x + 10, transform.position.y));
        firstMoveButton.gameObject.SetActive(false);
    }
}

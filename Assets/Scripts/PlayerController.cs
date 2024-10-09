using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region 싱글톤
    private static PlayerController _player;
    public static PlayerController Player { get { return _player; } set { _player = value; } }

    private void Awake()
    {
        if (_player == null)
            _player = this;
        else
            Destroy(gameObject);
    }
    #endregion

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }


    public void SetPlayerMove(bool move)
    {
        anim.SetBool("doMove", move);
    }

    public void StopPlayerMove()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace AngelBeat
{
    public class ExploreScene : MonoBehaviour
    {
        [SerializeField] private GameObject exploreController;
        void Start()
        {
            GameManager instance = GameManager.Instance;
            Instantiate(exploreController);
        }
    }
}


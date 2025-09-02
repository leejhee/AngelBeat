using AngelBeat;
using Core.Scripts.Managers;
using UnityEngine;

namespace Scene
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


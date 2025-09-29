using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Common.Scripts.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        private static TutorialManager instance;
        public static TutorialManager Instance => instance;

        public List<TutorialUnit> tutorialUnits = new();
        

    }
}
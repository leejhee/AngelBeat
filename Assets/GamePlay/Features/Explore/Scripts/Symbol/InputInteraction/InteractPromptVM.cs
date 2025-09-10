using UnityEngine;

namespace GamePlay.Features.Scripts.Explore.Symbol.InputInteraction
{
    public class InteractPromptVM
    {
        public Transform Target;
        public string KeyText;
        public string Message;
        public Vector2 Offset;
        public bool DoClamp = true;
    }
}
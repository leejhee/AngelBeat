using Core.SingletonObjects.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AngelBeat.UI
{
    public class BattleSceneUI : MonoBehaviour
    {
        [SerializeField] private Button moveButton;
        [SerializeField] private TMP_Text turnOwnerText;
        [SerializeField] private GameObject skillButtonPanel;
        [SerializeField] private Button turnEndButton;
        
        private CharBase _turnOwner;
        
        private void Awake()
        {
            EventBus.Instance.SubscribeEvent<OnTurnChanged>(this, OnTurnChange);
            turnEndButton.onClick.AddListener(OnTurnEndClick);
            moveButton.onClick.AddListener(OnClickMove);
        }
        
        private void OnDestroy()
        {
            EventBus.Instance.UnsubscribeEvent(this);
        }
        
        private void OnTurnChange(OnTurnChanged info)
        {
            _turnOwner = info.TurnOwner;
            turnOwnerText.text = _turnOwner.name;
        }

        private void OnTurnEndClick()
        {
            EventBus.Instance.SendMessage(new OnTurnEndInput());
            Debug.Log("turn end input");
        }
        
        private void OnClickMove()
        {
            EventBus.Instance.SendMessage(new OnMoveInput());
            Debug.Log("move input");
        }
        
    }
}




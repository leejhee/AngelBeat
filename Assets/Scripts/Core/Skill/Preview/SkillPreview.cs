using System;
using UnityEngine;

namespace AngelBeat
{
    public class SkillPreview : MonoBehaviour
    {
        private Camera _textureCaptureCamera;
        private SkillModel _previewSkill;
        private GameObject _previewFocusSnapshot;

        private int _range;
        
        public void InitPreview(CharBase focus, SkillModel skillModel)
        {
            _previewFocusSnapshot = focus.CharSnapShot;
            _previewFocusSnapshot.SetActive(true);
            
            _previewSkill = skillModel;
            _range = skillModel.SkillRange;
            
            
        }
        
        private void Update()
        {
            
            Vector2 curPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(curPos, Vector2.zero);
            if(hit.collider)
            {
                
            }
        }
    }
}
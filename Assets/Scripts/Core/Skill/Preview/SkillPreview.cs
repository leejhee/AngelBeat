using System;
using UnityEngine;

namespace AngelBeat
{
    public class SkillPreview : MonoBehaviour
    {
        [SerializeField] private Camera     _textureCaptureCamera;
        [SerializeField] private GameObject _targetPointer;
        [SerializeField] private GameObject _rangeCircle;
        
        private GameObject _previewFocusSnapshot;
        private SkillModel _previewSkill;
        
        private Vector3 _pivot; // 무조건 시전자 중심
        private int _range;
        private SystemEnum.ePivot _pivotType;
        private int _pointerRange;
        private bool _targetable;
        
        public void InitPreview(CharBase focus, SkillModel skillModel)
        {
            _previewSkill = skillModel;
            
            _range = _previewSkill.SkillRange;
            
            _pivot = focus.CharTransform.position;
            _rangeCircle.transform.localScale = new Vector3(_range * 2f, _range * 2f, 1);
            _pivotType = _previewSkill.SkillPivot;
            
            
        }
        
        private void Update()
        {
            Vector2 curPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _targetPointer.transform.position = curPos;
            
            float pointerPivotDist = Vector2.Distance(_pivot, curPos);
            if (pointerPivotDist > _range) return;

            RaycastHit2D hit = Physics2D.Raycast
                (curPos, Vector2.zero, 0f, LayerMask.GetMask("Character"));
            if(hit.collider)
            {
                //TODO : pivot type에 따라 제한할 것
                //TODO : Outlining with capture
                //TODO : 클릭 시 스킬 플레이
                Debug.Log(hit.collider.gameObject.name);
            }
        }
    }
}
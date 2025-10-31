using Core.Scripts.Data;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Keyword;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Threading;
using UnityEngine;

namespace GamePlay.Common.Scripts.Timeline.Marker
{
    public class SkillKeywordMarker : SkillTimeLineMarker
    {
        [SerializeField] private long keywordIndex;
        public override async UniTask BuildTaskAsync(CancellationToken ct)
        {
            #region Validation
            if (keywordIndex == 0)
            {
                Debug.LogError("[SkillKeywordMarker] : 유효한 인덱스를 넣어 주세요.");
                return;
            }
            
            KeywordData data = DataManager.Instance.GetData<KeywordData>(keywordIndex);
            if (data == null)
            {
                Debug.LogError("[SkillKeywordMarker] : 해당 인덱스의 키워드 데이터가 존재하지 않습니다.");
                return;
            }
            #endregion

            foreach (CharBase target in InputParam.Target)
            {
                KeywordInfo keywordInfo = target.KeywordInfo;


            }
            
            


        }
    }
}
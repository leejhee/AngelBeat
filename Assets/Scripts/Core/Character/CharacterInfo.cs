using System;
using UnityEngine;

namespace AngelBeat.Core.Character
{
    /// <summary> 파티에는 이 정보가 저장됩니다. </summary>
    [Serializable]
    public class CharacterInfo
    {
        [SerializeField]
        private long _index;
        private CharData _data;
        private CharStat _stat;
        private Vector3 _curPos;

        public long Index => _index;
        public CharStat Stat => _stat;

        public CharacterInfo(long index)
        {
            _index = index;
            _data = DataManager.Instance.GetData<CharData>(index);
            if(_data == null)
            {
                Debug.LogError("생성자 중 포함되지 않은 캐릭터데이터에 의한 오류");
                return;
            }
            else
            {
                var stat = DataManager.Instance.GetData<CharStatData>(index);
                _stat = new CharStat(stat);
            }

            _curPos = default;
        }

        public CharacterInfo(long index, CharData data, CharStat stat, Vector3 curPos)
        {
            _index = index;
            _data = data;
            _stat = stat;
            _curPos = curPos;
        }
    }
}
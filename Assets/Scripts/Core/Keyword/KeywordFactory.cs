using static SystemEnum;
namespace AngelBeat.Core
{
    public static class KeywordFactory
    {
        public static KeywordBase CreateKeyword(KeywordData data)
        {
            switch (data.KeywordType)
            {
                case eKeyword.Burn: return new Burn(data);
                // 그 외 필요한 키워드들 추가해볼 것
            }

            return null;
        }
    }
}
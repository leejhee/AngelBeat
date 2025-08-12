namespace Core.GameSave.Contracts
{
    /// <summary>
    /// SaveLoadManager 측에서 데이터를 받는 용도로 사용하는 인터페이스.
    /// </summary>
    public interface ISaveSink
    {
        void Upsert(FeatureSnapshot snapshot);
    }

}
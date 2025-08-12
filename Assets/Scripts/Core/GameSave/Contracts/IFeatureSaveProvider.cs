namespace Core.GameSave.Contracts
{
    public interface IFeatureSaveProvider
    {
        FeatureSnapshot Capture();
        string FeatureName { get; }
    }
}
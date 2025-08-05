namespace Core.GameSave
{
    public interface ISavableEntity
    {
        public void Save();
        public void Load();
        public bool IsDirty();
        public void ClearDirty();
        
    }
}
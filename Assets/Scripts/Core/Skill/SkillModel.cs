namespace AngelBeat
{
    public class SkillModel
    {
        private SkillData _skillData;
        
        public bool IsSkillActive { get; private set; }
        public int SkillRange { get; private set; }
        
        public int SkillHitRange { get; private set; }
        
        
        public SkillModel(SkillData skillData)
        {
            _skillData = skillData;
            SkillRange = skillData.skillRange;
            
            IsSkillActive = false;
            
        }
        
        
        
    }
}
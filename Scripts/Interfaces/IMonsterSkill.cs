public interface IMonsterSkill : ISkill
{
    public float Percentage { get; set; }
    public float MpRequirement { get; set; }
    public float SkillDistance { get; set; }
    public float SkillCoolTime { get; set; }
    public bool IsCool { get; }
}

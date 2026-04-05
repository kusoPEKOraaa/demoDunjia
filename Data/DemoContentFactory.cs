using System.Collections.Generic;

namespace DemoDunjia.Data;

public static class DemoContentFactory
{
    public static BattleConfig CreateConfig() => new();

    public static List<ItemData> CreateStartingItems()
    {
        return new List<ItemData>
        {
            ItemData.Additive("sharpen_stone", "磨锋石", 3),
            ItemData.Additive("bone_fang", "断骨齿", 5, exactZones: 1),
            ItemData.Additive("ember_seed", "烈压火种", 4, minZones: 2),
            new ItemData { Id = "membrane", Name = "护膜片", Phase = ItemResolutionPhase.Finisher, ExtraShieldAfterAttack = 3 },
            new ItemData { Id = "shock_chip", Name = "震荡片", Phase = ItemResolutionPhase.OnHit, ExtraInterrupt = 8 },
            new ItemData { Id = "flesh_needle", Name = "裂肉针", Phase = ItemResolutionPhase.OnHit, BleedOnHit = 2 },
            new ItemData { Id = "heavy_weight", Name = "重压砝码", Phase = ItemResolutionPhase.Multiplier, DamageMultiplier = 1.5f },
            new ItemData { Id = "echo_gear", Name = "回响齿轮", Phase = ItemResolutionPhase.ExtraTrigger, EchoZoneDamage = true }
        };
    }

    public static List<TechniqueData> CreateDefaultTechniques()
    {
        return new List<TechniqueData>
        {
            new TechniqueData { Id = "steady_combo", Name = "稳步连击", Sequence = "AAN", BonusDamage = 4 },
            new TechniqueData { Id = "counter_form", Name = "反制姿态", Sequence = "DAN", BonusDamage = 5 }
        };
    }

    public static List<MonsterData> CreateMonsters()
    {
        return new List<MonsterData>
        {
            FangBeast(),
            BurstBug(),
            PoisonSpore(),
            SpineHunter(),
            TriGateBoss()
        };
    }

    private static MonsterData FangBeast() => new()
    {
        Id = "fang_beast",
        Name = "尖齿兽",
        MaxHp = 28,
        Skills = new Dictionary<string, MonsterSkillData>
        {
            ["bite"] = new MonsterSkillData { Id = "bite", Name = "撕咬", ActionType = CombatActionType.Attack, DamagePerHit = 7 },
            ["pounce"] = new MonsterSkillData { Id = "pounce", Name = "猛扑", ActionType = CombatActionType.Attack, DamagePerHit = 8 },
            ["guard_howl"] = new MonsterSkillData { Id = "guard_howl", Name = "低吼", ActionType = CombatActionType.Defend, DamagePerHit = 4, ShieldGain = 4 },
            ["attack"] = new MonsterSkillData { Id = "attack", Name = "撕咬", ActionType = CombatActionType.Attack, DamagePerHit = 7 }
        }
    };

    private static MonsterData BurstBug() => new()
    {
        Id = "burst_bug",
        Name = "蓄爆虫",
        MaxHp = 27,
        Tenacity = 13,
        Skills = new Dictionary<string, MonsterSkillData>
        {
            ["charge"] = new MonsterSkillData { Id = "charge", Name = "聚能", ActionType = CombatActionType.Charge, ChargedSkillId = "blast", IntentText = "下回合释放【爆裂冲击】" },
            ["blast"] = new MonsterSkillData { Id = "blast", Name = "爆裂冲击", ActionType = CombatActionType.Attack, DamagePerHit = 14 },
            ["probe"] = new MonsterSkillData { Id = "probe", Name = "试探撞击", ActionType = CombatActionType.Attack, DamagePerHit = 5 },
            ["attack"] = new MonsterSkillData { Id = "attack", Name = "试探撞击", ActionType = CombatActionType.Attack, DamagePerHit = 5 }
        }
    };

    private static MonsterData PoisonSpore() => new()
    {
        Id = "poison_spore",
        Name = "毒雾孢子",
        MaxHp = 29,
        Skills = new Dictionary<string, MonsterSkillData>
        {
            ["spray"] = new MonsterSkillData { Id = "spray", Name = "喷毒", ActionType = CombatActionType.Attack, DamagePerHit = 4, PoisonOnHit = 2 },
            ["spike"] = new MonsterSkillData { Id = "spike", Name = "毒刺", ActionType = CombatActionType.Attack, DamagePerHit = 3, HitCount = 2, PoisonOnHit = 1 },
            ["shrink"] = new MonsterSkillData { Id = "shrink", Name = "收缩", ActionType = CombatActionType.Defend, ShieldGain = 5 },
            ["attack"] = new MonsterSkillData { Id = "attack", Name = "喷毒", ActionType = CombatActionType.Attack, DamagePerHit = 4, PoisonOnHit = 2 }
        }
    };

    private static MonsterData SpineHunter() => new()
    {
        Id = "spine_hunter",
        Name = "断脊猎手",
        MaxHp = 52,
        Tenacity = 24,
        Skills = new Dictionary<string, MonsterSkillData>
        {
            ["flurry"] = new MonsterSkillData { Id = "flurry", Name = "撕裂连袭", ActionType = CombatActionType.Attack, DamagePerHit = 5, HitCount = 2 },
            ["mark"] = new MonsterSkillData { Id = "mark", Name = "猎杀标记", ActionType = CombatActionType.Attack, DamagePerHit = 0, VulnerableOnHit = 1 },
            ["charge_hunt"] = new MonsterSkillData { Id = "charge_hunt", Name = "蓄猎", ActionType = CombatActionType.Charge, ChargedSkillId = "hunt", IntentText = "下回合释放【扑杀】" },
            ["hunt"] = new MonsterSkillData { Id = "hunt", Name = "扑杀", ActionType = CombatActionType.Attack, DamagePerHit = 18, InterruptValue = 12 },
            ["attack"] = new MonsterSkillData { Id = "attack", Name = "撕裂连袭", ActionType = CombatActionType.Attack, DamagePerHit = 5, HitCount = 2 }
        }
    };

    private static MonsterData TriGateBoss() => new()
    {
        Id = "tri_gate",
        Name = "三相守门者",
        MaxHp = 120,
        Tenacity = 36,
        IsBoss = true,
        Phases = new[] { "攻势相", "蓄势相", "守势相" },
        Skills = new Dictionary<string, MonsterSkillData>
        {
            ["dual"] = new MonsterSkillData { Id = "dual", Name = "双刃", ActionType = CombatActionType.Attack, DamagePerHit = 8 },
            ["core_charge"] = new MonsterSkillData { Id = "core_charge", Name = "巨核蓄能", ActionType = CombatActionType.Charge, ChargedSkillId = "starfall", IntentText = "下回合释放【坠星】" },
            ["starfall"] = new MonsterSkillData { Id = "starfall", Name = "坠星", ActionType = CombatActionType.Attack, DamagePerHit = 20 },
            ["fortify"] = new MonsterSkillData { Id = "fortify", Name = "固守", ActionType = CombatActionType.Defend, ShieldGain = 6 },
            ["heavy"] = new MonsterSkillData { Id = "heavy", Name = "迟缓重击", ActionType = CombatActionType.Attack, DamagePerHit = 12 },
            ["attack"] = new MonsterSkillData { Id = "attack", Name = "双刃", ActionType = CombatActionType.Attack, DamagePerHit = 8 }
        }
    };
}

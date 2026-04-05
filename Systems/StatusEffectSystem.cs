using System;
using System.Collections.Generic;
using DemoDunjia.Data;
using DemoDunjia.Entities;

namespace DemoDunjia.Systems;

public sealed class StatusEffectSystem
{
    public bool ConsumeStun(CombatActor actor)
    {
        var stun = actor.GetStatus(StatusEffectType.Stun);
        if (stun <= 0)
        {
            return false;
        }

        actor.SetStatus(StatusEffectType.Stun, Math.Max(0, stun - 1));
        return true;
    }

    public int ApplyEndTurnDot(CombatActor actor, CombatLog log, string actorSide)
    {
        var total = 0;

        var bleed = actor.GetStatus(StatusEffectType.Bleed);
        if (bleed > 0)
        {
            var damage = bleed;
            actor.ApplyDamage(damage);
            actor.SetStatus(StatusEffectType.Bleed, bleed - 1);
            total += damage;
            log.Add($"[{actorSide}] 流血结算 {damage}，剩余层数 {actor.GetStatus(StatusEffectType.Bleed)}");
        }

        var poison = actor.GetStatus(StatusEffectType.Poison);
        if (poison > 0)
        {
            actor.ApplyDamage(poison);
            total += poison;
            log.Add($"[{actorSide}] 中毒结算 {poison}，层数保持 {poison}");
        }

        return total;
    }

    public int ApplyOutgoingDamageModifiers(CombatActor actor, int value)
    {
        var weak = actor.GetStatus(StatusEffectType.Weak);
        if (weak <= 0) return value;
        actor.SetStatus(StatusEffectType.Weak, weak - 1);
        return (int)MathF.Round(value * 0.75f);
    }

    public int ApplyIncomingDamageModifiers(CombatActor actor, int value)
    {
        var vulnerable = actor.GetStatus(StatusEffectType.Vulnerable);
        if (vulnerable <= 0) return value;
        actor.SetStatus(StatusEffectType.Vulnerable, vulnerable - 1);
        return (int)MathF.Round(value * 1.25f);
    }

    public string BuildStatusText(CombatActor actor)
    {
        var parts = new List<string>();
        foreach (var kv in actor.Statuses)
        {
            if (kv.Value <= 0) continue;
            parts.Add($"{kv.Key}:{kv.Value}");
        }

        return parts.Count == 0 ? "无" : string.Join(", ", parts);
    }
}

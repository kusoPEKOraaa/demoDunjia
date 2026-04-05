using System;
using System.Collections.Generic;
using DemoDunjia.Data;

namespace DemoDunjia.Entities;

public abstract class CombatActor
{
    public string Name { get; protected set; } = "";
    public int MaxHp { get; protected set; }
    public int Hp { get; private set; }
    public int ShieldCap { get; protected set; } = 40;
    public int Shield { get; private set; }
    public int Tenacity { get; protected set; }
    public int InterruptMeter { get; private set; }

    public bool IsCharging { get; set; }
    public bool IsDefending { get; set; }
    public bool DefendSuccessThisAction { get; set; }
    public string PendingIntent { get; set; } = "";

    public readonly Dictionary<StatusEffectType, int> Statuses = new();

    public bool IsDead => Hp <= 0;

    protected void Initialize(string name, int maxHp, int shieldCap, int startShield, int tenacity)
    {
        Name = name;
        MaxHp = maxHp;
        Hp = maxHp;
        ShieldCap = shieldCap;
        Shield = Math.Clamp(startShield, 0, shieldCap);
        Tenacity = tenacity;
    }

    public void GainShield(int amount)
    {
        if (amount <= 0) return;
        Shield = Math.Clamp(Shield + amount, 0, ShieldCap);
    }

    public int ApplyDamage(int amount)
    {
        var value = Math.Max(0, amount);
        var absorbed = Math.Min(Shield, value);
        Shield -= absorbed;
        var hpDamage = value - absorbed;
        Hp = Math.Max(0, Hp - hpDamage);
        return hpDamage;
    }

    public void AddStatus(StatusEffectType type, int layers)
    {
        if (layers <= 0) return;
        if (!Statuses.TryAdd(type, layers))
        {
            Statuses[type] += layers;
        }
    }

    public void SetStatus(StatusEffectType type, int layers)
    {
        Statuses[type] = Math.Max(0, layers);
    }

    public int GetStatus(StatusEffectType type) => Statuses.TryGetValue(type, out var value) ? value : 0;

    public void ClearStatus(StatusEffectType type)
    {
        Statuses.Remove(type);
    }

    public void AddInterrupt(int value)
    {
        InterruptMeter += Math.Max(0, value);
    }

    public void ResetInterrupt()
    {
        InterruptMeter = 0;
    }
}

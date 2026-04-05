using System.Collections.Generic;
using DemoDunjia.Data;
using DemoDunjia.Entities;
using Godot;

namespace DemoDunjia.UI;

public partial class ActorPanel : PanelContainer
{
    [Export] public bool IsPlayer;

    private Label _name = null!;
    private Label _stats = null!;
    private ProgressBar _hpBar = null!;
    private ProgressBar _shieldBar = null!;
    private HBoxContainer _statusRow = null!;
    private Label _subInfo = null!;
    private ColorRect _portrait = null!;

    private readonly PackedScene _badgeScene = GD.Load<PackedScene>("res://Scenes/UI/StatusBadge.tscn");
    private int _lastHp;

    public override void _Ready()
    {
        _name = GetNode<Label>("VBox/Header/Name");
        _stats = GetNode<Label>("VBox/Stats");
        _hpBar = GetNode<ProgressBar>("VBox/Bars/HpBar");
        _shieldBar = GetNode<ProgressBar>("VBox/Bars/ShieldBar");
        _statusRow = GetNode<HBoxContainer>("VBox/StatusRow");
        _subInfo = GetNode<Label>("VBox/SubInfo");
        _portrait = GetNode<ColorRect>("VBox/Header/Portrait");
        _portrait.Color = IsPlayer ? new Color("4c6ef5") : new Color("e03131");
    }

    public void BindPlayer(PlayerCombatActor actor, string weapon, string shield)
    {
        _name.Text = actor.Name;
        _stats.Text = $"HP {actor.Hp}/{actor.MaxHp} | 护盾 {actor.Shield}/{actor.ShieldCap} | 充能 {actor.Charge} | 溢出 {actor.OverflowCharge}";
        _subInfo.Text = $"武器：{weapon}  盾牌：{shield}";
        RefreshBars(actor.Hp, actor.MaxHp, actor.Shield, actor.ShieldCap);
        RefreshStatuses(actor.Statuses, actor.IsCharging, actor.PendingIntent);
    }

    public void BindMonster(MonsterCombatActor actor)
    {
        _name.Text = actor.Name;
        var intent = string.IsNullOrWhiteSpace(actor.PendingIntent) ? "普通行动" : actor.PendingIntent;
        _stats.Text = $"HP {actor.Hp}/{actor.MaxHp} | 蓄力 {(actor.IsCharging ? "是" : "否")} | 阶段 {actor.CurrentPhase}";
        _subInfo.Text = $"意图：{intent}";
        RefreshBars(actor.Hp, actor.MaxHp, actor.Shield, 30);
        RefreshStatuses(actor.Statuses, actor.IsCharging, actor.PendingIntent);
    }

    private void RefreshBars(int hp, int hpMax, int shield, int shieldMax)
    {
        _hpBar.MaxValue = hpMax;
        _hpBar.Value = hp;
        _shieldBar.MaxValue = Mathf.Max(1, shieldMax);
        _shieldBar.Value = shield;

        if (_lastHp > 0 && hp < _lastHp)
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate", new Color(1f, 0.75f, 0.75f), 0.08f);
            tween.TweenProperty(this, "modulate", Colors.White, 0.12f);
        }

        _lastHp = hp;
    }

    private void RefreshStatuses(Dictionary<StatusEffectType, int> statuses, bool isCharging, string pendingIntent)
    {
        foreach (Node child in _statusRow.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var kv in statuses)
        {
            if (kv.Value <= 0) continue;
            var badge = _badgeScene.Instantiate<StatusBadge>();
            badge.Setup(kv.Key.ToString(), kv.Value, StatusTooltip(kv.Key));
            _statusRow.AddChild(badge);
        }

        if (isCharging)
        {
            var badge = _badgeScene.Instantiate<StatusBadge>();
            badge.Setup("蓄力", 1, string.IsNullOrWhiteSpace(pendingIntent) ? "正在蓄力" : pendingIntent);
            _statusRow.AddChild(badge);
        }
    }

    private static string StatusTooltip(StatusEffectType type)
    {
        return type switch
        {
            StatusEffectType.Bleed => "流血：回合结束受层数伤害，层数-1",
            StatusEffectType.Poison => "中毒：回合结束受层数伤害，层数不衰减",
            StatusEffectType.Weak => "虚弱：下次造成伤害-25%",
            StatusEffectType.Vulnerable => "易伤：下次受到伤害+25%",
            StatusEffectType.Stun => "眩晕：下回合空过",
            _ => "状态"
        };
    }
}

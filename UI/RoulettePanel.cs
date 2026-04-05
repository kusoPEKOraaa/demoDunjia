using System.Collections.Generic;
using System.Linq;
using DemoDunjia.Data;
using DemoDunjia.Entities;
using Godot;

namespace DemoDunjia.UI;

public partial class RoulettePanel : PanelContainer
{
    private readonly Dictionary<int, PanelContainer> _zonePanels = new();
    private readonly Dictionary<int, Label> _zoneLabels = new();
    private readonly Dictionary<int, VBoxContainer> _zoneItems = new();
    private readonly Dictionary<int, List<Label>> _itemLabelsByZone = new();
    private Label _chargeHint = null!;

    public override void _Ready()
    {
        _chargeHint = GetNode<Label>("VBox/ChargeHint");

        for (var i = 1; i <= 3; i++)
        {
            var root = GetNode<PanelContainer>($"VBox/Zones/Zone{i}");
            _zonePanels[i] = root;
            _zoneLabels[i] = root.GetNode<Label>("VBox/Title");
            _zoneItems[i] = root.GetNode<VBoxContainer>("VBox/Items");
            _itemLabelsByZone[i] = new List<Label>();
        }
    }

    public void Bind(PlayerCombatActor player, BattleConfig config)
    {
        var triggerCount = player.Charge switch
        {
            <= 0 => 1,
            1 => 2,
            _ => 3
        };

        _chargeHint.Text = $"当前充能 {player.Charge}，本次攻击会触发 {triggerCount} 个区域（顺序 1→2→3）";

        foreach (var zone in _zonePanels.Keys)
        {
            _zoneLabels[zone].Text = $"区域 {zone}（基础 {config.RouletteBaseValuePerZone}）";

            foreach (Node child in _zoneItems[zone].GetChildren())
            {
                child.QueueFree();
            }

            _itemLabelsByZone[zone].Clear();
            foreach (var item in player.RouletteZones[zone])
            {
                var label = new Label { Text = $"• {item.Name}", TooltipText = BuildItemTooltip(item) };
                _zoneItems[zone].AddChild(label);
                _itemLabelsByZone[zone].Add(label);
            }
        }
    }

    public void HighlightZones(IReadOnlyList<int> zones)
    {
        foreach (var panel in _zonePanels.Values)
        {
            panel.Modulate = Colors.White;
        }

        var tween = CreateTween();
        foreach (var z in zones.OrderBy(v => v))
        {
            if (!_zonePanels.TryGetValue(z, out var panel)) continue;
            tween.TweenProperty(panel, "modulate", new Color("fff3bf"), 0.08f);
            tween.TweenProperty(panel, "modulate", Colors.White, 0.08f);
        }
    }

    public void HighlightItems(Godot.Collections.Dictionary itemsByZone)
    {
        foreach (var zoneKey in itemsByZone.Keys)
        {
            var zone = (int)zoneKey;
            if (!_itemLabelsByZone.TryGetValue(zone, out var labels)) continue;
            var arr = (Godot.Collections.Array)itemsByZone[zone];
            var targetNames = arr.Select(name => name.ToString()).ToHashSet();
            foreach (var label in labels)
            {
                var pure = label.Text.Replace("• ", "");
                if (!targetNames.Contains(pure)) continue;

                var tween = CreateTween();
                tween.TweenProperty(label, "modulate", new Color("ffd8a8"), 0.1f);
                tween.TweenProperty(label, "modulate", Colors.White, 0.2f);
            }
        }
    }

    private static string BuildItemTooltip(ItemData item)
    {
        if (item.FlatDamage > 0) return $"伤害 +{item.FlatDamage}";
        if (item.DamageMultiplier > 1f) return $"伤害 x{item.DamageMultiplier:0.##}";
        if (item.ExtraShieldAfterAttack > 0) return $"收尾获得护盾 +{item.ExtraShieldAfterAttack}";
        if (item.ExtraInterrupt > 0) return $"命中额外打断 +{item.ExtraInterrupt}";
        if (item.BleedOnHit > 0) return $"命中附加流血 {item.BleedOnHit}";
        if (item.EchoZoneDamage) return "本区域伤害再触发 1 次";
        return item.Name;
    }
}

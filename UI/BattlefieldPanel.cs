using Godot;
using DemoDunjia.Systems;

namespace DemoDunjia.UI;

public partial class BattlefieldPanel : PanelContainer
{
    private Label _title = null!;
    private Label _feed = null!;

    public override void _Ready()
    {
        _title = GetNode<Label>("VBox/Title");
        _feed = GetNode<Label>("VBox/Feed");
        _feed.Text = "等待玩家行动";
    }

    public void SetPhase(string phase)
    {
        _title.Text = $"战场状态：{phase}";
    }

    public void PushEntry(CombatLogEntry entry)
    {
        if (entry.Category is CombatLogCategory.Damage or CombatLogCategory.Shield or CombatLogCategory.Interrupt or CombatLogCategory.ExtraTurn)
        {
            _feed.Text = entry.Message;
            _feed.Modulate = entry.Category switch
            {
                CombatLogCategory.Damage => new Color("ff8787"),
                CombatLogCategory.Shield => new Color("74c0fc"),
                CombatLogCategory.Interrupt => new Color("ffd43b"),
                CombatLogCategory.ExtraTurn => new Color("8ce99a"),
                _ => Colors.White
            };

            var tween = CreateTween();
            tween.TweenProperty(_feed, "modulate", Colors.White, 0.4f);
        }
    }
}

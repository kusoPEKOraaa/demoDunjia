using DemoDunjia.Systems;
using Godot;

namespace DemoDunjia.UI;

public partial class CombatLogPanel : PanelContainer
{
    private RichTextLabel _log = null!;

    public override void _Ready()
    {
        _log = GetNode<RichTextLabel>("VBox/Log");
    }

    public void Append(CombatLogEntry entry)
    {
        var color = entry.Category switch
        {
            CombatLogCategory.Turn => "#A0C4FF",
            CombatLogCategory.Action => "#F8F9FA",
            CombatLogCategory.Damage => "#FF6B6B",
            CombatLogCategory.Shield => "#74C0FC",
            CombatLogCategory.Status => "#C0A7FF",
            CombatLogCategory.Interrupt => "#FFD43B",
            CombatLogCategory.ExtraTurn => "#69DB7C",
            CombatLogCategory.Technique => "#F59F00",
            CombatLogCategory.Result => "#20C997",
            _ => "#CED4DA"
        };

        _log.AppendText($"[color={color}]• {entry.Message}[/color]\n");
        _log.ScrollToLine(_log.GetLineCount());
    }

    public void Clear()
    {
        _log.Clear();
    }
}

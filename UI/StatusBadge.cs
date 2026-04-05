using Godot;

namespace DemoDunjia.UI;

public partial class StatusBadge : PanelContainer
{
    private Label _text = null!;

    public override void _Ready()
    {
        _text = GetNode<Label>("Label");
    }

    public void Setup(string name, int stacks, string tooltip)
    {
        _text.Text = stacks > 1 ? $"{name} {stacks}" : name;
        TooltipText = tooltip;
    }
}

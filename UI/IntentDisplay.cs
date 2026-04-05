using Godot;

namespace DemoDunjia.UI;

public partial class IntentDisplay : PanelContainer
{
    private Label _phase = null!;
    private Label _intent = null!;
    private Label _banner = null!;

    public override void _Ready()
    {
        _phase = GetNode<Label>("VBox/Phase");
        _intent = GetNode<Label>("VBox/Intent");
        _banner = GetNode<Label>("VBox/Banner");
        _banner.Text = "";
    }

    public void SetPhase(string text)
    {
        _phase.Text = $"当前阶段：{text}";
    }

    public void SetIntent(string text)
    {
        _intent.Text = $"怪物意图：{text}";
    }

    public void FlashBanner(string text)
    {
        _banner.Text = text;
        _banner.Modulate = new Color("ffd43b");
        var tween = CreateTween();
        tween.TweenProperty(_banner, "modulate", Colors.White, 0.5f);
    }
}

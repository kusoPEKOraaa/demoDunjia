using Godot;

namespace DemoDunjia.UI;

public partial class ResultPopup : PanelContainer
{
    [Signal] public delegate void RestartRequestedEventHandler();

    private Label _title = null!;
    private Label _summary = null!;
    private Button _restart = null!;

    public override void _Ready()
    {
        _title = GetNode<Label>("VBox/Title");
        _summary = GetNode<Label>("VBox/Summary");
        _restart = GetNode<Button>("VBox/Buttons/Restart");
        _restart.Pressed += () => EmitSignal(SignalName.RestartRequested);
        Visible = false;
    }

    public void ShowResult(bool victory, string message, int damageDealt, int damageTaken)
    {
        _title.Text = victory ? "胜利" : "失败";
        _summary.Text = victory
            ? $"{message}\n总伤害：{damageDealt}  承受伤害：{damageTaken}\n奖励区（占位）：道具三选一"
            : $"{message}\n总伤害：{damageDealt}  承受伤害：{damageTaken}\n可选择重新开始继续测试";
        Visible = true;
    }

    public void HidePopup()
    {
        Visible = false;
    }
}

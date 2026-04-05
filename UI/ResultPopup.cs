using Godot;

namespace DemoDunjia.UI;

public partial class ResultPopup : PanelContainer
{
    [Signal] public delegate void RestartRequestedEventHandler();

    private Label _title = null!;
    private Label _summary = null!;
    private Label _reward = null!;
    private Button _restart = null!;
    private Button _back = null!;

    public override void _Ready()
    {
        _title = GetNode<Label>("VBox/Title");
        _summary = GetNode<Label>("VBox/Summary");
        _reward = GetNode<Label>("VBox/Reward");
        _restart = GetNode<Button>("VBox/Buttons/Restart");
        _back = GetNode<Button>("VBox/Buttons/Back");

        _restart.Pressed += () => EmitSignal(SignalName.RestartRequested);
        _back.Pressed += () => EmitSignal(SignalName.RestartRequested);
        Visible = false;
    }

    public void ShowResult(bool victory, string message, int damageDealt, int damageTaken)
    {
        _title.Text = victory ? "战斗胜利" : "战斗失败";
        _summary.Text = $"{message}\n总伤害：{damageDealt}  承受伤害：{damageTaken}";
        _reward.Text = victory ? "奖励占位：道具三选一（待接入正式掉落）" : "可重新开始当前测试战斗。";
        _back.Text = victory ? "下一步（占位）" : "返回测试入口（占位）";
        Visible = true;
    }

    public void HidePopup()
    {
        Visible = false;
    }
}

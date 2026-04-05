using DemoDunjia.Data;
using Godot;

namespace DemoDunjia.UI;

public partial class ActionPanel : PanelContainer
{
    [Signal] public delegate void ActionSelectedEventHandler(CombatActionType action);

    private Button _attack = null!;
    private Button _charge = null!;
    private Button _defend = null!;
    private Label _hint = null!;

    public override void _Ready()
    {
        _attack = GetNode<Button>("VBox/Buttons/Attack");
        _charge = GetNode<Button>("VBox/Buttons/Charge");
        _defend = GetNode<Button>("VBox/Buttons/Defend");
        _hint = GetNode<Label>("VBox/Hint");

        _attack.Text = "攻击\n触发轮盘";
        _charge.Text = "蓄力\n+1 充能";
        _defend.Text = "防御\n先获得8护盾";

        _attack.TooltipText = "攻击：按当前充能触发轮盘区域。";
        _charge.TooltipText = "蓄力：提升触发区域数，满层后累积溢出层。";
        _defend.TooltipText = "防御：先加8护盾，再进行防御判定。";

        _attack.Pressed += () => EmitSignal(SignalName.ActionSelected, CombatActionType.Attack);
        _charge.Pressed += () => EmitSignal(SignalName.ActionSelected, CombatActionType.Charge);
        _defend.Pressed += () => EmitSignal(SignalName.ActionSelected, CombatActionType.Defend);
    }

    public void SetInteractable(bool canAct, bool canDefend, bool stunned)
    {
        _attack.Disabled = !canAct;
        _charge.Disabled = !canAct;
        _defend.Disabled = !canAct || !canDefend;

        if (stunned)
        {
            _hint.Text = "玩家眩晕中，本回合无法主动行动";
        }
        else if (!canAct)
        {
            _hint.Text = "当前无法行动";
        }
        else if (!canDefend)
        {
            _hint.Text = "连续防御已达上限（2）";
        }
        else
        {
            _hint.Text = "请选择行动";
        }
    }

    public void ShowExtraActionHint()
    {
        _hint.Text = "追加行动！请再次选择行动";
    }
}

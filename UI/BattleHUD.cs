using DemoDunjia.Combat;
using DemoDunjia.Data;
using DemoDunjia.Systems;
using Godot;

namespace DemoDunjia.UI;

public partial class BattleHUD : Control
{
    private BattleController _battle = null!;

    private Label _playerLabel = null!;
    private Label _monsterLabel = null!;
    private Label _intentLabel = null!;
    private RichTextLabel _logLabel = null!;
    private Button _attackBtn = null!;
    private Button _chargeBtn = null!;
    private Button _defendBtn = null!;
    private readonly StatusEffectSystem _status = new();

    public override void _Ready()
    {
        _battle = GetNode<BattleController>("../BattleController");
        _battle.StateChanged += Refresh;
        _battle.Log.EntryAdded += OnLog;

        _playerLabel = GetNode<Label>("VBox/PlayerInfo");
        _monsterLabel = GetNode<Label>("VBox/MonsterInfo");
        _intentLabel = GetNode<Label>("VBox/IntentInfo");
        _logLabel = GetNode<RichTextLabel>("VBox/LogPanel");
        _attackBtn = GetNode<Button>("VBox/Actions/Attack");
        _chargeBtn = GetNode<Button>("VBox/Actions/Charge");
        _defendBtn = GetNode<Button>("VBox/Actions/Defend");

        _attackBtn.Pressed += () => _battle.PlayerSelectAction(CombatActionType.Attack);
        _chargeBtn.Pressed += () => _battle.PlayerSelectAction(CombatActionType.Charge);
        _defendBtn.Pressed += () => _battle.PlayerSelectAction(CombatActionType.Defend);

        Refresh();
    }

    private void Refresh()
    {
        var p = _battle.Player;
        var m = _battle.Monster;

        _playerLabel.Text = $"玩家 HP {p.Hp}/{p.MaxHp} | 护盾 {p.Shield}/{p.ShieldCap} | 充能 {p.Charge} | 溢出 {p.OverflowCharge}\n状态: {_status.BuildStatusText(p)}";
        _monsterLabel.Text = $"怪物 {m.Name} HP {m.Hp}/{m.MaxHp} | 当前相位 {m.CurrentPhase}\n状态: {_status.BuildStatusText(m)} | 蓄力中: {(m.IsCharging ? "是" : "否")}";
        _intentLabel.Text = $"怪物意图: {(string.IsNullOrEmpty(m.PendingIntent) ? "普通行动" : m.PendingIntent)}";

        _defendBtn.Disabled = !_battle.CanDefend() || p.IsDead || m.IsDead;
        _attackBtn.Disabled = p.IsDead || m.IsDead;
        _chargeBtn.Disabled = p.IsDead || m.IsDead;
    }

    private void OnLog(string line)
    {
        _logLabel.AppendText(line + "\n");
    }
}

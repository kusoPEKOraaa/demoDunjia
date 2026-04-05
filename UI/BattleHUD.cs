using DemoDunjia.Combat;
using DemoDunjia.Data;
using DemoDunjia.Systems;
using Godot;

namespace DemoDunjia.UI;

public partial class BattleHUD : Control
{
    private BattleController _battle = null!;
    private ActorPanel _playerPanel = null!;
    private ActorPanel _monsterPanel = null!;
    private RoulettePanel _roulette = null!;
    private ActionPanel _actions = null!;
    private CombatLogPanel _logPanel = null!;
    private IntentDisplay _intentPanel = null!;
    private ResultPopup _result = null!;

    public override void _Ready()
    {
        _battle = GetNode<BattleController>("../BattleController");
        _playerPanel = GetNode<ActorPanel>("Layout/Bottom/Left/PlayerPanel");
        _monsterPanel = GetNode<ActorPanel>("Layout/Top/MonsterPanel");
        _roulette = GetNode<RoulettePanel>("Layout/Bottom/Center/RoulettePanel");
        _actions = GetNode<ActionPanel>("Layout/Bottom/Center/ActionPanel");
        _logPanel = GetNode<CombatLogPanel>("Layout/Bottom/Right/LogPanel");
        _intentPanel = GetNode<IntentDisplay>("Layout/Center/IntentPanel");
        _result = GetNode<ResultPopup>("ResultPopup");

        _battle.StateChanged += Refresh;
        _battle.PhaseChanged += OnPhaseChanged;
        _battle.RouletteResolved += OnRouletteResolved;
        _battle.BattleEnded += OnBattleEnded;
        _battle.Log.EntryAdded += OnLog;

        _actions.ActionSelected += OnActionSelected;
        _result.RestartRequested += OnRestart;

        Refresh();
    }

    private void Refresh()
    {
        var p = _battle.Player;
        var m = _battle.Monster;

        _playerPanel.BindPlayer(p, _battle.GetWeaponName(), _battle.GetShieldName());
        _monsterPanel.BindMonster(m);
        _roulette.Bind(p, _battle.Config);

        var canAct = !p.IsDead && !m.IsDead;
        _actions.SetInteractable(canAct, _battle.CanDefend());
        _intentPanel.SetIntent(string.IsNullOrWhiteSpace(m.PendingIntent) ? "普通行动" : m.PendingIntent);
        _intentPanel.SetPhase(_battle.PhaseText);
    }

    private void OnActionSelected(int action)
    {
        _battle.PlayerSelectAction((CombatActionType)action);
    }

    private void OnPhaseChanged(string phase)
    {
        _intentPanel.SetPhase(phase);
    }

    private void OnRouletteResolved(int[] zones)
    {
        if (zones.Length == 0) return;
        _roulette.HighlightZones(zones);
    }

    private void OnBattleEnded(bool victory, string message, int damageDealt, int damageTaken)
    {
        _result.ShowResult(victory, message, damageDealt, damageTaken);
    }

    private void OnRestart()
    {
        _result.HidePopup();
        _logPanel.Clear();
        _battle.InitializeBattle();
    }

    private void OnLog(CombatLogEntry entry)
    {
        _logPanel.Append(entry);
        if (entry.Category == CombatLogCategory.ExtraTurn)
        {
            _actions.ShowExtraActionHint();
            _intentPanel.FlashBanner("追加行动！");
        }
        else if (entry.Category == CombatLogCategory.Interrupt && entry.Message.Contains("打断"))
        {
            _intentPanel.FlashBanner("蓄力被打断！");
        }
    }
}

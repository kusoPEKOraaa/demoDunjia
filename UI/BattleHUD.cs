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
    private BattlefieldPanel _battlefieldPanel = null!;
    private ResultPopup _result = null!;

    public override void _Ready()
    {
        _battle = GetNode<BattleController>("../BattleController");
        _playerPanel = GetNode<ActorPanel>("Layout/Root/Bottom/Left/PlayerPanel");
        _monsterPanel = GetNode<ActorPanel>("Layout/Root/Top/MonsterPanel");
        _roulette = GetNode<RoulettePanel>("Layout/Root/Bottom/Center/RoulettePanel");
        _actions = GetNode<ActionPanel>("Layout/Root/Bottom/Center/ActionPanel");
        _logPanel = GetNode<CombatLogPanel>("Layout/Root/Bottom/Right/LogPanel");
        _intentPanel = GetNode<IntentDisplay>("Layout/Root/Top/SideInfo/IntentPanel");
        _battlefieldPanel = GetNode<BattlefieldPanel>("Layout/Root/Top/SideInfo/BattlefieldPanel");
        _result = GetNode<ResultPopup>("ResultPopup");

        _battle.StateChanged += Refresh;
        _battle.PhaseChanged += OnPhaseChanged;
        _battle.RouletteResolved += OnRouletteResolved;
        _battle.RouletteItemsResolved += OnRouletteItemsResolved;
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

        var canAct = !p.IsDead && !m.IsDead && !_battle.IsResolving && !p.HasStatus(StatusEffectType.Stun);
        _actions.SetInteractable(canAct, _battle.CanDefend(), p.HasStatus(StatusEffectType.Stun));
        _intentPanel.SetIntent(string.IsNullOrWhiteSpace(m.PendingIntent) ? "普通行动" : m.PendingIntent);
        _intentPanel.SetPhase(_battle.PhaseText);
        _battlefieldPanel.SetPhase(_battle.PhaseText);
    }

    private void OnActionSelected(CombatActionType action)
    {
        _battle.PlayerSelectAction(action);
    }

    private void OnPhaseChanged(string phase)
    {
        _intentPanel.SetPhase(phase);
        _battlefieldPanel.SetPhase(phase);
    }

    private void OnRouletteResolved(int[] zones)
    {
        if (zones.Length == 0) return;
        _roulette.HighlightZones(zones);
    }


    private void OnRouletteItemsResolved(Godot.Collections.Dictionary itemsByZone)
    {
        _roulette.HighlightItems(itemsByZone);
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
        _battlefieldPanel.PushEntry(entry);

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

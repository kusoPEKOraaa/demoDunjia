using DemoDunjia.Data;
using DemoDunjia.Entities;

namespace DemoDunjia.Systems;

public sealed class ChargeSystem
{
    public void DoCharge(PlayerCombatActor player, BattleConfig config)
    {
        if (player.Charge < config.ChargeCap)
        {
            player.Charge += 1;
            return;
        }

        player.OverflowCharge += 1;
    }

    public void ClearAfterAttack(PlayerCombatActor player)
    {
        player.Charge = 0;
        player.OverflowCharge = 0;
    }

    public void ClearAfterInterrupt(PlayerCombatActor player)
    {
        player.Charge = 0;
        player.OverflowCharge = 0;
    }
}

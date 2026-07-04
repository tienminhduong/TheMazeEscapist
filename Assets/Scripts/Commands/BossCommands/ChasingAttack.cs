using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ChasingAttack : BossCommand
{
    public ChasingAttack(BossController boss) : base(boss) { }
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        //Get player's position
        var attackCount = Random.Range(10, 15);
        var playerObject = boss.playerObject;
        var originCell = BossController.originCell;
        // Spawn warning tiles around the player

        for (int k = 0; k < attackCount; k++)
        {
            var playerCell = GridManager.Instance.WorldToCell(playerObject.transform.position);
            boss.TriggerCreateTile(playerCell, boss.WarningTilePrefab);
            yield return new WaitForSeconds(2f);
        }
        yield return new WaitForSeconds(5f);

        isExecuting = false;
        isCompleted = true;
    }
}

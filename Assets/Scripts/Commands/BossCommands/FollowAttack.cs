using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FollowAttack : BossCommand
{
    public FollowAttack(BossController boss) : base(boss) { }
    List<Vector3Int> directions = new List<Vector3Int>() {
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0)
    };
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        //Get player's position
        var attackCount = Random.Range(5, 8);
        var playerObject = boss.playerObject;
        // Spawn warning tiles around the player
        var playerInitCell = GridManager.Instance.WorldToCell(playerObject.transform.position);

        foreach (var dir in directions)
        {
            var warningTileCell = playerInitCell + dir;
            boss.TriggerCreateTile(warningTileCell, boss.WarningTilePrefab);
        }
        for (int i = 0; i < attackCount; i++)
        {
            var playerCell = GridManager.Instance.WorldToCell(playerObject.transform.position);
            boss.TriggerPencilAttack(1.5f, 20f, directions[i % directions.Count], playerCell);
            yield return new WaitForSeconds(3f);
        }

        yield return new WaitForSeconds(2f);

        isExecuting = false;
        isCompleted = true;
    }
}

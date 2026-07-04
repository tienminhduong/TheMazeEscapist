using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ThreeByThreeAttack : BossCommand
{
    public ThreeByThreeAttack(BossController boss) : base(boss) { }
    private List<Vector3Int> allDirections = new List<Vector3Int>() {
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(-1, -1, 0),
        new Vector3Int(1, -1, 0),
        new Vector3Int(-1, 1, 0),
        new Vector3Int(0, 0, 0)
    };
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        //Get player's position
        var attackCount = Random.Range(4, 7);
        var playerObject = boss.playerObject;
        var originCell = BossController.originCell;
        // Spawn warning tiles around the player

        for (int k = 0; k < attackCount; k++)
        {
            boss.animator.Play("BossAttack");
            var playerCell = GridManager.Instance.WorldToCell(playerObject.transform.position);

            for (int i = 0; i < allDirections.Count; i++)
            {
                var direction = allDirections[i];
                var targetCell = playerCell + direction;

                if (GridManager.Instance.IsWalkable(targetCell))
                {
                    var warningTile = boss.TriggerCreateTile(targetCell, boss.WarningTilePrefab);
                    var warningTileComponent = warningTile.GetComponent<WarningTile>();
                    warningTileComponent.Init(1.5f);
                }
            }

            yield return new WaitForSeconds(3f);
        }
        yield return new WaitForSeconds(1f);

        isExecuting = false;
        isCompleted = true;
    }
}


public class LongPlusAttack : BossCommand
{
    public LongPlusAttack(BossController boss) : base(boss) { }
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        //Get player's position
        var attackCount = Random.Range(4, 7);
        var playerObject = boss.playerObject;
        var originCell = BossController.originCell;
        var size = BossController.size;
        // Spawn warning tiles around the player

        for (int k = 0; k < attackCount; k++)
        {
            boss.animator.Play("BossAttack");
            var playerCell = GridManager.Instance.WorldToCell(playerObject.transform.position);

            for (int i = 0; i < size; i++)
            {
                var targetCellX = new Vector3Int(originCell.x + i, playerCell.y, 0);
                var targetCellY = new Vector3Int(playerCell.x, originCell.y - i, 0);
                if (GridManager.Instance.IsWalkable(targetCellX))
                {
                    var warningTile = boss.TriggerCreateTile(targetCellX, boss.WarningTilePrefab);
                    var warningTileComponent = warningTile.GetComponent<WarningTile>();
                    warningTileComponent.Init(1.5f);
                }
                if (GridManager.Instance.IsWalkable(targetCellY))
                {
                    var warningTile = boss.TriggerCreateTile(targetCellY, boss.WarningTilePrefab);
                    var warningTileComponent = warningTile.GetComponent<WarningTile>();
                    warningTileComponent.Init(1.5f);
                }
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(3f);
        }
        yield return new WaitForSeconds(3f);

        isExecuting = false;
        isCompleted = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class RaisePhase2Walls : BossCommand
{
    public RaisePhase2Walls(BossController boss) : base(boss) { }
    List<int> raiseWallIndexes = new List<int>
    {
        1, 3, 5,
    };
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        boss.playerObject.GetComponent<PlayerController>().LockMoving();
        Debug.Log("Executing RaisePhase2Walls command");
        boss.TriggerCreateTile(BossController.originCell, boss.SlimePrefab);
        boss.TriggerCreateTile(BossController.originCell + new Vector3Int(BossController.size - 1, 0, 0), boss.SlimePrefab);
        boss.TriggerCreateTile(BossController.originCell + new Vector3Int(0, -BossController.size + 1, 0), boss.SlimePrefab);
        boss.TriggerCreateTile(BossController.originCell + new Vector3Int(BossController.size - 1, -BossController.size + 1, 0), boss.SlimePrefab);

        foreach (var i in raiseWallIndexes)
        {
            foreach (var j in raiseWallIndexes)
            {
                Vector3Int cellPos = BossController.originCell + new Vector3Int(i, -j, 0);
                if (GridManager.Instance.IsWalkable(cellPos))
                {
                    GridManager.Instance.RaiseWall(cellPos);
                }
            }
        }
        yield return new WaitForSeconds(2f);
        boss.playerObject.GetComponent<PlayerController>().UnlockMoving();
        isExecuting = false;
        isCompleted = true;
    }
}

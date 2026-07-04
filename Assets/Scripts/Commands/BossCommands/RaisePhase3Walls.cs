using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RaisePhase3Walls : BossCommand
{
    public RaisePhase3Walls(BossController boss) : base(boss) { }
    List<int> raiseWallIndexes = new List<int>
    {
        1, 3, 5,
    };
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        int size = BossController.size;
        Vector3Int originCell = BossController.originCell;
        for (int i = 0; i < size; i++)
        {
            if (i == size / 2)
            {
                for (int j = 0; j < size; j++)
                {
                    boss.TriggerLoweringWall(originCell + new Vector3Int(j, -i, 0));
                }
            }
            else
            {
                for (int j = 0; j < size; j++)
                {
                    boss.TriggerRaisingWall(originCell + new Vector3Int(j, -i, 0));
                }
            }
        }

        for (int i = 0; i < size; i++)
        {
            boss.TriggerCreateTile(originCell + new Vector3Int(i, -size / 2, 0), boss.SlimePrefab);
        }

        yield return new WaitForSeconds(2f);

        isExecuting = false;
        isCompleted = true;
    }
}
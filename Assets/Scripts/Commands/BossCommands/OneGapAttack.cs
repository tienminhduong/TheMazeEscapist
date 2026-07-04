using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class OneGapAttack : BossCommand
{
    public OneGapAttack(BossController boss) : base(boss) { }
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
        var attackCount = Random.Range(2, 4);
        var playerObject = boss.playerObject;
        // Spawn warning tiles around the player

        for (int i = 0; i < attackCount; i++)
        {
            var gapColumnIndex = Mathf.Floor(Random.Range(0, BossController.size));
            for (int j = 0; j < BossController.size; j++)
            {
                if (j != gapColumnIndex)
                    boss.TriggerPencilAttack(1f, 15f, Vector3Int.down, Vector3Int.FloorToInt(new Vector3(BossController.originCell.x + j, BossController.originCell.y, 0)));
            }
            yield return new WaitForSeconds(5f);
        }

        yield return new WaitForSeconds(10f);

        isExecuting = false;
        isCompleted = true;
    }
}

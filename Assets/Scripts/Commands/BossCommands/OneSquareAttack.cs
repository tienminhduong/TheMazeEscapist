using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class OneSquareAttack : BossCommand
{
    public OneSquareAttack(BossController boss) : base(boss) { }
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
        var originCell = BossController.originCell;
        // Spawn warning tiles around the player

        for (int k = 0; k < attackCount; k++)
        {
            // Generate a random square area (position and size)
            var squareSize = 4;

            var squarePosition = Vector3Int.FloorToInt(new Vector3(Random.Range(originCell.x, originCell.x + BossController.size - squareSize + 1), Random.Range(originCell.y - BossController.size, originCell.y + squareSize - 1), 0));
            for (int i = originCell.x; i < originCell.x + BossController.size; i++)
            {
                if (i < squarePosition.x || i >= squarePosition.x + squareSize)
                    boss.TriggerPencilAttack(3f, 30f, Vector3Int.down, new Vector3Int(i, originCell.y, 0));
            }
            for (int j = originCell.y; j > originCell.y - BossController.size; j--)
            {
                if (j > squarePosition.y || j <= squarePosition.y - squareSize)
                    boss.TriggerPencilAttack(3f, 30f, Vector3Int.right, new Vector3Int(originCell.x, j, 0));
            }
            yield return new WaitForSeconds(6f);
        }

        yield return new WaitForSeconds(8f);

        isExecuting = false;
        isCompleted = true;
    }
}

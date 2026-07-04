using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FastDownPencilAttack : BossCommand
{
    public FastDownPencilAttack(BossController boss) : base(boss) { }

    public HashSet<int> GetRandomUniqueNumbers(int count, int min, int max)
    {
        HashSet<int> uniqueNumbers = new HashSet<int>();
        while (uniqueNumbers.Count < count)
        {
            int randomNumber = Random.Range(min, max);
            uniqueNumbers.Add(randomNumber);

        }
        return uniqueNumbers;
    }


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
            var emptyCell = GetRandomUniqueNumbers(3, 0, size);
            for (int i = 0; i < size; i++)
                if (!emptyCell.Contains(i))
                    boss.TriggerPencilAttack(1f, 50f, Vector3Int.down, originCell + new Vector3Int(i, size / 2, 0));
            yield return new WaitForSeconds(1.5f);
        }
        yield return new WaitForSeconds(3f);

        isExecuting = false;
        isCompleted = true;
    }
}

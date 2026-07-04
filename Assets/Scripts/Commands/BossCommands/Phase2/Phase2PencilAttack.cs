using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

public class Phase2PencilAttack : BossCommand
{
    public Phase2PencilAttack(BossController boss) : base(boss) { }
    bool isOdd = true;

    List<List<Vector3Int>> pencilPositions = new List<List<Vector3Int>>()
    {
        new List<Vector3Int>(),
        new List<Vector3Int>()
    };

    public HashSet<int> GetRandomUniqueEvenNumbers(int count, int min, int max)
    {
        HashSet<int> uniqueNumbers = new HashSet<int>();
        while (uniqueNumbers.Count < count)
        {
            int randomNumber = Random.Range(min, max);
            if (randomNumber % 2 == 0)
            {
                uniqueNumbers.Add(randomNumber);
            }
        }
        return uniqueNumbers;
    }

    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        //Get player's position
        var attackCount = Random.Range(4, 6);
        var playerObject = boss.playerObject;
        var originCell = BossController.originCell;
        var size = BossController.size;

        for (int i = 0; i < size; i++)
        {
            pencilPositions[0].Add(originCell + new Vector3Int(i, 0, 0));
            pencilPositions[1].Add(originCell + new Vector3Int(0, -i, 0));
        }

        for (int k = 0; k < attackCount; k++)
        {
            bool vertical = Random.value > 0.5f;
            var randomIndexes = GetRandomUniqueEvenNumbers(Random.Range(1, 2), 0, size);
            for (int i = 0; i < size; i += 2)
            {
                if (!randomIndexes.Contains(i))
                {
                    bool isTopLeft = Random.value > 0.5f;
                    Vector3Int direction = isTopLeft ? (vertical ? Vector3Int.down : Vector3Int.right) : (vertical ? Vector3Int.up : Vector3Int.left);
                    Vector3Int cellPos = pencilPositions[vertical ? 0 : 1][i];
                    boss.TriggerPencilAttack(1.5f, 30, direction, cellPos);
                }
            }
            yield return new WaitForSeconds(2f);
        }

        isExecuting = false;
        isCompleted = true;
    }
}


using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections;

public class ShortCheckerAttack : BossCommand
{
    public ShortCheckerAttack(BossController boss) : base(boss) { }
    bool isOdd = true;
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        //Get player's position
        var attackCount = Random.Range(2, 4);
        var playerObject = boss.playerObject;
        var originCell = BossController.originCell;
        var size = BossController.size;

        for (int k = 0; k < attackCount; k++)
        {
            for (int i = 0; i < size; i += 2)
            {
                for (int j = 0; j < size; j += 2)
                {
                    bool positionIsOdd = i % 4 == j % 4;
                    if (positionIsOdd == isOdd)
                    {
                        var tileObject = boss.TriggerCreateTile(originCell + new Vector3Int(i, -j, 0), boss.WarningTilePrefab);
                        var warningTile = tileObject.GetComponent<WarningTile>();
                        warningTile.Init(0.75f);
                    }
                }
            }
            isOdd = !isOdd;
            yield return new WaitForSeconds(2f);
        }

        isExecuting = false;
        isCompleted = true;
    }
}


using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RandomFourDirectionAttack : BossCommand
{
    public RandomFourDirectionAttack(BossController boss) : base(boss) { }
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
            var randomDirection = directions[Random.Range(0, directions.Count)];
            var playerCell = GridManager.Instance.WorldToCell(playerObject.transform.position);

            boss.TriggerPencilAttack(1.0f, 20f, randomDirection, playerCell);

            yield return new WaitForSeconds(1.5f);
        }
        yield return new WaitForSeconds(4f);

        isExecuting = false;
        isCompleted = true;
    }
}

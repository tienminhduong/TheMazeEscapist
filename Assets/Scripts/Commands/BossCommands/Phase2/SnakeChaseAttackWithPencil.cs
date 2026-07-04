using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class SnakeChaseAttackWithPencil : BossCommand
{
    public SnakeChaseAttackWithPencil(BossController boss) : base(boss) { }
    Vector3Int currentCell;
    Vector3Int currentDirection = Vector3Int.zero;
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        var snakeAttack = new SnakeChaseAttack(boss);
        var pencilAttack = new RandomFourDirectionAttack(boss);

        bool snakeDone = false;
        bool pencilDone = false;

        boss.StartCoroutine(RunAndFlag(snakeAttack.Execute(), () => snakeDone = true));
        boss.StartCoroutine(RunAndFlag(pencilAttack.Execute(), () => pencilDone = true));

        yield return new WaitUntil(() => snakeDone && pencilDone);

        isExecuting = false;
        isCompleted = true;
    }

    IEnumerator RunAndFlag(IEnumerator routine, System.Action onDone)
    {
        yield return routine;
        onDone();
    }
}


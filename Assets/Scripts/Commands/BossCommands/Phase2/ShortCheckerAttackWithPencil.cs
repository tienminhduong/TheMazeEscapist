using System.Collections;
using System.Threading.Tasks;

public class ShortCheckerAttackWithPencil : BossCommand
{
    public ShortCheckerAttackWithPencil(BossController boss) : base(boss) { }
    bool isOdd = true;
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        var checkerAttack = new ShortCheckerAttack(boss);
        var pencilAttack = new RandomFourDirectionAttack(boss);

        // pencilAttack.Execute();
        // await checkerAttack.Execute();

        isExecuting = false;
        isCompleted = true;

        yield break;
    }
}


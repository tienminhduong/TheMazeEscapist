using System.Collections;
using UnityEngine;

public class DoNothing : BossCommand
{
    public DoNothing(BossController boss) : base(boss) { }
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        yield return new WaitForSeconds(4f);

        isExecuting = false;
        isCompleted = true;
    }
}

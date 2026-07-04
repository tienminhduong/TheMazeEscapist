using System.Collections;

public class WinGameCommand : BossCommand
{
    public WinGameCommand(BossController boss) : base(boss) { }
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        WinPoint.OnLevelComplete?.Invoke();

        isExecuting = false;
        isCompleted = true;

        yield break; ;
    }
}

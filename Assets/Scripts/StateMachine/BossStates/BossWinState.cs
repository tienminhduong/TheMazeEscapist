using UnityEngine;

public class BossWinState : BossBaseState
{

    public BossWinState(BossController boss, Animator animator) : base(boss, animator) { }

    public override void OnEnter()
    {
        boss.animator.Play("BossDead");
        boss.spriteBlink.Blink();
        boss.StartCoroutine(WaitForBossAnimation(() =>
        {
            WinPoint.OnLevelComplete?.Invoke();
        }));

    }

    public override void Update()
    {
        // noop
    }

    public override void FixedUpdate()
    {
        // noop
    }

    public override void OnExit()
    {
        // noop
    }
}

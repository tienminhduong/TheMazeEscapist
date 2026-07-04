using System.Collections;
using UnityEngine;

public abstract class BossBaseState : IState
{
    protected readonly BossController boss;
    protected readonly Animator animator;

    protected static readonly int IdleHash = Animator.StringToHash("IdleNormal");

    protected const float crossFadeDuration = 0.1f;

    protected BossBaseState(BossController boss, Animator animator)
    {
        this.boss = boss;
        this.animator = animator;
    }

    protected IEnumerator WaitForBossAnimation(System.Action onComplete)
    {
        yield return null; // let Animator update to the new state first (wait for 1 frame)

        float length = boss.animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(length);

        onComplete?.Invoke();
    }

    public virtual void OnEnter()
    {
        // noop
    }

    public virtual void Update()
    {
        // noop
    }

    public virtual void FixedUpdate()
    {
        // noop
    }

    public virtual void OnExit()
    {
        // noop
    }
}

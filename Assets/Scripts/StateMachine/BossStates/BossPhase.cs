using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPhase : BossBaseState
{
    int health = 3;
    int maxHealth = 3;
    int hitCount = 0;
    List<BossCommand> attackCommands = new List<BossCommand>();
    List<BossCommand> combinedCommands = new List<BossCommand>();
    Vector3Int? endAttackPosition = null;
    BossCommand enterCommand;
    BossCommand exitCommand;
    BossCommand currentCommand;

    Coroutine currentCommandCoroutine;
    Coroutine spawnSwordCoroutine;

    float weaponCooldown = 5f;
    float healCooldown = 30f;

    bool isBossAttacking = false;

    public BossPhase(BossController boss, Animator animator, List<BossCommand> attackCommands, int phaseHealth = 3, BossCommand enterCommand = null, BossCommand exitCommand = null, Vector3Int? endAttackPosition = null, List<BossCommand> combinedCommands = null) : base(boss, animator)
    {
        this.attackCommands = attackCommands;
        this.health = phaseHealth;
        this.maxHealth = this.health;
        this.enterCommand = enterCommand;
        this.exitCommand = exitCommand;
        this.endAttackPosition = endAttackPosition;
        this.combinedCommands = combinedCommands ?? new List<BossCommand>();
    }

    public override void OnEnter()
    {
        Debug.Log("Entering Boss Phase");
        currentCommand = enterCommand;
        Sword.OnSwordEffectTriggered += Hurt;
        SwordAttack.OnSwordAttacked += HurtAnim;
        if (currentCommand != null)
            boss.StartCoroutine(currentCommand.Execute());
        boss.StartCoroutine(SpawnSword(5f));
    }

    public override void Update()
    {
        if (!isBossAttacking && (currentCommand == null || currentCommand.IsCompleted()))
        {
            isBossAttacking = true;
            // Play attack animation
            boss.animator.Play("BossAttack");
            boss.StartCoroutine(WaitForBossAnimation(() =>
            {
                // After animation completes, execute the next command
                currentCommand = (health >= (maxHealth / 2) || combinedCommands.Count == 0) ? attackCommands[Random.Range(0, attackCommands.Count)] : combinedCommands[Random.Range(0, combinedCommands.Count)];
                currentCommandCoroutine = boss.StartCoroutine(currentCommand?.Execute());
                isBossAttacking = false;
            }));
        }
    }

    public override void OnExit()
    {
        Sword.OnSwordEffectTriggered -= Hurt;
        SwordAttack.OnSwordAttacked -= HurtAnim;
        boss.StopCoroutine(currentCommandCoroutine);
        if (exitCommand != null)
            boss.StartCoroutine(exitCommand.Execute());
        if (spawnSwordCoroutine != null)
            boss.StopCoroutine(spawnSwordCoroutine);
    }

    private IEnumerator SpawnSword(float delay = 5f)
    {
        yield return new WaitForSeconds(delay);
        if (health == 1 && endAttackPosition.HasValue)
        {
            boss.TriggerCreateTile(endAttackPosition.Value, boss.SwordPrefab);
        }
        else
        {
            boss.TriggerCreateRandomItem(boss.SwordPrefab);
        }
    }

    private IEnumerator SpawnHeal(float delay = 30f)
    {
        yield return new WaitForSeconds(delay);
        boss.TriggerCreateRandomItem(boss.HealPotionPrefab);
    }

    public void Hurt()
    {
        health = Mathf.Max(0, health - 1);
        Debug.Log($"Boss Phase hurt! Health: {health}/{maxHealth}");
        AudioManager.Instance.PlaySfx("boss_hurt", Vector3.zero);
        spawnSwordCoroutine = boss.StartCoroutine(SpawnSword()); // 5 sec for test
    }

    public void HurtAnim(Vector3 swordPosition)
    {
        // If swordPosition on right flip hurt animation
        if (boss.animator == null) return;
        if (swordPosition.x > boss.transform.position.x)
        {
            boss.animator.Play("BossHurtLeft");
        }
        else
        {
            boss.animator.Play("BossHurtRight");
        }

        boss.spriteBlink.Blink();
        hitCount++;
    }

    public bool IsPhaseEnded()
    {
        return health <= 0;
    }

    // For final phase
    public bool IsTheEnd()
    {
        return health <= 0 && hitCount == maxHealth;
    }
}

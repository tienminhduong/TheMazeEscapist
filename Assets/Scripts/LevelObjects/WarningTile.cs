using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class WarningTile : SpecialTile
{
    [SerializeField] private string soundEffectName = "trash_can_collected";
    [SerializeField] private string soundExplodeEffectName = "explosion";
    public override TileType Type => TileType.Walkable;
    [SerializeField] private Collider2D damageCollider;

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string warningStateName = "Warning";
    [SerializeField] private string flashingStateName = "Flashing";
    [SerializeField] private string explodingStateName = "Exploding";
    [SerializeField] private float baseWarningClipLength = 1f; // length of the Warning clip at speed = 1

    private float warningDuration = 1f;

    void Awake()
    {
        isEffect = true;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        damageCollider = GetComponent<Collider2D>();
        damageCollider.enabled = false;
    }

    public async void Init(float warningDuration)
    {
        if (animator == null)
            return;
        this.warningDuration = warningDuration;

        // Stretch/compress the Warning animation to match the requested duration
        float warningSpeed = baseWarningClipLength / Mathf.Max(warningDuration, 0.0001f);
        animator.speed = warningSpeed;
        animator.Play(warningStateName, 0, 0f);
        AudioManager.Instance.PlaySfx(soundEffectName, transform.position);
        await UniTask.Delay((int)(warningDuration * 1000));

        // Reset speed to normal for the fixed-length animations
        if (animator == null) return;
        animator.speed = 1f;

        animator.Play(flashingStateName, 0, 0f);
        float flashingLength = GetStateLength(flashingStateName);
        await UniTask.Delay((int)(flashingLength * 1000));

        if (damageCollider == null || animator == null) return;
        // Check collision with the player in short amount of time, if player inside then take damage
        damageCollider.enabled = true;

        animator.Play(explodingStateName, 0, 0f);
        float explodingLength = GetStateLength(explodingStateName);
        AudioManager.Instance.PlaySfx(soundExplodeEffectName, transform.position);

        await UniTask.Delay((int)(explodingLength * 1000));
        if (damageCollider == null) return;
        damageCollider.enabled = false;
        Destroy(this.gameObject);
    }

    private float GetStateLength(string stateName)
    {
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name == stateName)
                return clip.length;
        }
        return 0.2f; // fallback default
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Player takes damage only once
            damageCollider.enabled = false;
        }
    }
}
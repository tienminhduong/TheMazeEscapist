using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class StudentCard : SpecialTile, ICollectible
{
    [SerializeField] private WinpointUnlockCondition winpointUnlockCondition;

    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        EyeofTheStorm.OnTouchPlayer += ReleaseCard;
    }

    void OnDisable()
    {
        EyeofTheStorm.OnTouchPlayer -= ReleaseCard;
    }

    private void ReleaseCard()
    {
        WinPoint.OnLockedConditionMet?.Invoke(winpointUnlockCondition.conditionName);
    }

    private void HideCard()
    {
        boxCollider.enabled = false;
        spriteRenderer.enabled = false;
    }

    public override TileType Type => TileType.StudentCard;

    void Start()
    {
        OnInstantiated();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnSpecialTileInteracted?.Invoke(this);
        }
    }

    public void Collect()
    {
        WinPoint.OnUnlockedConditionMet?.Invoke(winpointUnlockCondition.conditionName);
        AudioManager.Instance.PlaySfx("student_card_collected", transform.position);
        HideCard();
    }

    public void Release(Vector3? fromPosition = null)
    {
        if (fromPosition.HasValue)
        {
            var originalPosition = transform.position;
            transform.position = fromPosition.Value;
            spriteRenderer.enabled = true;
            transform.DOMove(originalPosition, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                transform.position = originalPosition;
                boxCollider.enabled = true;
            });
        }
        else
        {
            boxCollider.enabled = true;
            spriteRenderer.enabled = true;
        }
    }
}
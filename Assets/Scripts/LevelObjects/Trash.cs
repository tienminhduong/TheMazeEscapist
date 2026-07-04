using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Trash : SpecialTile, ICollectible
{
    [SerializeField] private string soundEffectName = "trash_can_collected";
    private bool isCollected = false;

    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void HideTrash()
    {
        boxCollider.enabled = false;
        spriteRenderer.enabled = false;
    }

    private void ShowTrash()
    {
        boxCollider.enabled = true;
        spriteRenderer.enabled = true;
    }

    public override TileType Type => TileType.Trash;

    void Start()
    {
        OnInstantiated();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;
        if (collision.CompareTag("Player"))
        {
            OnSpecialTileInteracted?.Invoke(this);
        }
    }

    public async UniTask DiscardTrash(Vector3 recycleBinPosition)
    {
        transform.position = recycleBinPosition;
        ShowTrash();

        await transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutBack).ToUniTask();

        await transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        // HideTrash();
        gameObject.SetActive(false);
    }

    public void Collect()
    {
        isCollected = true;
        HideTrash();
        AudioManager.Instance.PlaySfx(soundEffectName, transform.position);
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
                isCollected = false;
            });
        }
        else
        {
            boxCollider.enabled = true;
            spriteRenderer.enabled = true;
            isCollected = false;
        }
    }
}

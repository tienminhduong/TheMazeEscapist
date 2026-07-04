using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Slime : SpecialTile
{
    [SerializeField] private string soundEffectName = "trash_can_collected";

    public override TileType Type => TileType.Slime;

    void Start()
    {
        OnInstantiated();
        // Fade in the slime tile
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1f, 1f, 1f, 0f); // Start fully transparent
        spriteRenderer.DOFade(1f, 0.5f); // Fade to fully opaque over 1 second
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnSpecialTileInteracted?.Invoke(this);
            AudioManager.Instance.PlaySfx(soundEffectName, transform.position);
        }
    }

    void OnDestroy()
    {
        // Fade out the slime tile before destroying it
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.DOFade(0f, 0.5f).OnComplete(() => Destroy(this.gameObject));
    }
}

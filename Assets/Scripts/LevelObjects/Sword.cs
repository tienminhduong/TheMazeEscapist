using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Sword : Item
{
    [SerializeField] private string soundEffectName = "health_potion_collected";
    [SerializeField] private GameObject swordAttackPrefab;
    public static UnityAction OnSwordEffectTriggered;
    public override TileType Type => TileType.Item;

    private Vector3 bossPosition;
    [SerializeField] private Vector3 bossOffset = new Vector3(0, 10, 0); // Adjust the offset as needed

    void Start()
    {
        OnItemInstantiated?.Invoke(this);

        // Get boss position from the BossController
        var bossController = FindFirstObjectByType<BossController>();
        if (bossController != null)
        {
            bossPosition = bossController.transform.position + bossOffset;
            if (transform.position.x < bossPosition.x)
            {
                transform.localScale = new Vector3(-1, 1, 1); // Flip the sword to face right
            }
        }
        else
        {
            Debug.LogWarning("BossController not found in the scene.");
        }

        // Fade in the sword over 0.5 seconds
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0); // Start fully transparent
            spriteRenderer.DOFade(1f, 0.5f); // Fade to fully opaque over 0.5 seconds
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Sword tile triggered!");
            Sword.OnSwordEffectTriggered?.Invoke();

            // rotate to face the boss
            var directionToBoss = bossPosition - transform.position;
            var rotation = Quaternion.LookRotation(Vector3.forward, directionToBoss);
            var swordAttack = Instantiate(swordAttackPrefab, transform.position, rotation);
            if (transform.localScale.x < 0) swordAttack.gameObject.transform.localScale = new Vector3(-swordAttack.gameObject.transform.localScale.x, swordAttack.gameObject.transform.localScale.y, swordAttack.gameObject.transform.localScale.z); // Flip the sword attack if the sword is facing left
            OnItemInteracted?.Invoke(this);
            AudioManager.Instance.PlaySfx(soundEffectName, transform.position);
            Destroy(gameObject);
        }
    }
}

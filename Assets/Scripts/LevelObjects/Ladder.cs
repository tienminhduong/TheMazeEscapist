using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Ladder : SpecialTile
{
    [SerializeField] private Sprite rightSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite topSprite;
    [SerializeField] private Sprite bottomSprite;
    public override TileType Type => TileType.Ladder;

    [SerializeField] private Vector2 goUpDirection; // Hướng mà player có thể đi qua ladder,
                                      // dùng để xác định hướng di chuyển của player khi tương tác
                                      // với ladder
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ChooseSprite();
    }

    private void ChooseSprite()
    {
        switch(goUpDirection)
        {
            case Vector2 v when v == Vector2.up:
                spriteRenderer.sprite = topSprite;
                break;
            case Vector2 v when v == Vector2.down:
                spriteRenderer.sprite = bottomSprite;
                break;
            case Vector2 v when v == Vector2.right:
                spriteRenderer.sprite = leftSprite;
                break;
            case Vector2 v when v == Vector2.left:
                spriteRenderer.sprite = rightSprite;
                break;
            default:
                Debug.LogWarning("Invalid goUpDirection for Ladder: " + goUpDirection);
                break;
        }
    }    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnInstantiated();
    }

    public bool CanGoIn(Vector2 moveDirection)
    {
        return moveDirection == goUpDirection || moveDirection == goUpDirection * -1;
    }
}

using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Rock : SpecialTile
{
    [SerializeField] private GameObject rock;
    private SpriteRenderer hiddenRock;

    public static UnityAction<Vector3> OnRockEnabled;

    public override TileType Type => TileType.Rock;
    bool isEnabled = false;

    void Start()
    {
        hiddenRock = GetComponent<SpriteRenderer>();
        rock.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Rock tile triggered!");
            EnableRock();
        }
    }

    public void EnableRock()
    {
        if (isEnabled) return;
        rock.SetActive(true);
        hiddenRock.enabled = false;
        AudioManager.Instance.PlaySfx("rock", transform.position);
        rock.transform.localScale = Vector3.zero;
        rock.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
        OnSpecialTileInteracted?.Invoke(this);
        OnRockEnabled?.Invoke(rock.transform.position);
        isEnabled = true;
    }
}
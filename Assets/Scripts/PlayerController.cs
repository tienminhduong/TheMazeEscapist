using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;
//using KingCat.Base;
using System;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] bool lockMoving = false;
    [SerializeField] private float minSwipeDistance = 60f;
    private int externalMovementLocks = 0;
    private Vector2 touchPosition = Vector2.zero;
    private Vector2 releasePosition = Vector2.zero;
    private Vector2 inputPosition = Vector2.zero;
    private bool ignorePrimaryContactUntilRelease = false;

    private Sequence moveSequence;
    private Animator animator;

    public static UnityAction OnLoseGame;
    public static UnityAction OnTurnMove;
    public static UnityAction OnStartMoving;

    public static UnityAction OnPlayerHurt;
    [SerializeField] private int maxHealth = 5;

    private readonly List<ICollectible> collectedItems = new();
    private int currentHealth;

    private bool IsMovementLocked => lockMoving || externalMovementLocks > 0;
    private SpriteBlink spriteBlink;
    [SerializeField] private float blinkDuration = 3f;
    [SerializeField] private List<Sprite> starsSprites; // On and Off
    [SerializeField] private List<Image> starImages;
    int currentStarIndex = 2;
    void OnEnable()
    {
        WinPoint.OnLevelComplete += TouchGoal;
        Portal.OnPlayerTeleport += HandleTeleport;
        OnLoseGame += HandleLoseGame;
        EyeofTheStorm.OnTouchPlayer += HandleTouchStorm;
        SpecialTile.OnSpecialTileInteracted += HandleSpecialTileInteraction;
        HealPotion.OnHealEffectTriggered += Heal;
    }

    void OnDisable()
    {
        WinPoint.OnLevelComplete -= TouchGoal;
        Portal.OnPlayerTeleport -= HandleTeleport;
        OnLoseGame -= HandleLoseGame;
        EyeofTheStorm.OnTouchPlayer -= HandleTouchStorm;
        SpecialTile.OnSpecialTileInteracted -= HandleSpecialTileInteraction;
        HealPotion.OnHealEffectTriggered -= Heal;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteBlink = GetComponent<SpriteBlink>();
        currentHealth = maxHealth;
    }
    private void TouchGoal()
    {
        lockMoving = true;
        moveSequence?.Kill();
    }

    private void HandleLoseGame()
    {
        Debug.Log("Player Lost! Restarting level...");
        SceneController.Instance.TransitionToScene(SceneManager.GetActiveScene().name);
    }

    public void UnlockMoving()
    {
        lockMoving = false;
    }

    public void LockMoving()
    {
        lockMoving = true;
    }

    public void SetExternalMovementLock(bool isLocked)
    {
        externalMovementLocks += isLocked ? 1 : -1;
        externalMovementLocks = Mathf.Max(0, externalMovementLocks);
    }

    public static void SetAllExternalMovementLocks(bool isLocked)
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
            player.SetExternalMovementLock(isLocked);
    }

    public void OnMove(InputValue value)
    {
        if (IsMovementLocked) return;

        var input = value.Get<Vector2>();
        Debug.Log($"Move: {input}");

        var direction = Vector2Int.RoundToInt(input);
        if (direction == Vector2Int.zero)
            return;

        var path = GridManager.Instance.FindPathFromWorld(transform.position, direction);
        MoveWithPath(path);
    }

    public void OnPrimaryContact(InputValue value)
    {
        var isPressed = value.Get<float>() > 0.5f;
        inputPosition = GetPrimaryScreenPosition();

        if (!isPressed && ignorePrimaryContactUntilRelease)
        {
            ignorePrimaryContactUntilRelease = false;
            return;
        }

        if (IsMovementLocked) return;
        //if (EventSystem.current.IsPointerOverGameObject())
        if (EventSystem.current != null)
        {
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.gameObject.tag != "EffectUI")
            {
                Debug.Log("Pointer is over UI, ignoring input.");
                return;
            }
        }

        if (isPressed && RotatableTerrainTapArea.ContainsAnyScreenPosition(inputPosition))
        {
            ignorePrimaryContactUntilRelease = true;
            return;
        }

        if (isPressed)
        {
            //Debug.Log("Primary Contact Started");
            touchPosition = inputPosition;
            //Debug.Log($"Touch Position: {touchPosition}");
        }
        else
        {
            //Debug.Log("Primary Contact Canceled");
            releasePosition = inputPosition;
            //Debug.Log($"Release Position: {releasePosition}");

            var direction = Vector2Int.zero;
            var swipeVector = releasePosition - touchPosition;
            if (swipeVector.sqrMagnitude < minSwipeDistance * minSwipeDistance)
                return;

            if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
            {
                direction.x = swipeVector.x > 0 ? 1 : -1;
            }
            else
            {
                direction.y = swipeVector.y > 0 ? 1 : -1;
            }
            var path = GridManager.Instance.FindPathFromWorld(transform.position, direction);
            MoveWithPath(path);
        }
    }

    public void OnPrimaryPosition(InputValue value)
    {
        inputPosition = value.Get<Vector2>();
    }

    private Vector2 GetPrimaryScreenPosition()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            var touch = touchscreen.primaryTouch;
            if (touch.press.isPressed || touch.press.wasPressedThisFrame || touch.press.wasReleasedThisFrame)
                return touch.position.ReadValue();
        }

        return Mouse.current != null ? Mouse.current.position.ReadValue() : inputPosition;
    }

    public void MoveWithPath(Path path)
    {
        lockMoving = true;
        moveSequence = DOTween.Sequence();
        var currentPos = transform.position;
        bool firstTile = true;
        if (animator != null)
        {
            animator.Play("Walk");
        }
        OnStartMoving?.Invoke();
        foreach (var nodeData in path.directions)
        {
            var stepStartPos = currentPos; // snapshot so closures use the right position
            var dir = nodeData.direction;
            moveSequence.AppendCallback(() =>
            {
                var localScale = transform.localScale;
                if (dir.x != 0)
                {
                    localScale.x = dir.x > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                    transform.localScale = localScale;
                }
            });

            // Add stop time if required
            if (nodeData.stopTime > 0)
            {
                var curCell = GridManager.Instance.WorldToCell(currentPos);
                var nextCell = curCell + (Vector3Int)nodeData.direction;
                moveSequence.AppendCallback(new TweenCallback(() => GridManager.Instance.MakeEffectAtCell(nextCell)));
                moveSequence.AppendInterval(nodeData.stopTime);
            }
            moveSequence.AppendCallback(() =>
            {
                // Check if next square valid
                var currentCell = GridManager.Instance.WorldToCell(stepStartPos);
                var nextCell = currentCell + new Vector3Int(dir.x, dir.y, 0);

                if (!GridManager.Instance.GetGrid().TryGetValue(currentCell, out var cellData) || cellData.specialTile == null)
                {
                    firstTile = false;
                    return;
                }

                if (cellData.specialTile.Type == TileType.Slime && !firstTile)
                {
                    Debug.Log("Player stepped on slime, stopping movement.");
                    moveSequence.Kill();
                    lockMoving = false;
                    return;
                }
                firstTile = false;
            });
            moveSequence.Append(transform.DOMove(stepStartPos + new Vector3(dir.x, dir.y, 0) * path.stepLength, 0.1f)
                .SetEase(Ease.Linear).OnComplete(() =>
                {
                    AudioManager.Instance.PlaySfx("player_move", transform.position);
                }));
            currentPos += new Vector3(dir.x, dir.y, 0) * path.stepLength;
        }
        moveSequence.OnComplete(() =>
        {
            lockMoving = false;
            if (animator != null)
            {
                animator.Play("Idle");
            }
            OnTurnMove?.Invoke();
        });
    }

    public async void HandleTeleport(TeleportData data)
    {
        moveSequence?.Kill();
        lockMoving = true;

        Vector3 currentScale = transform.localScale;
        AudioManager.Instance.PlaySfx("teleport", transform.position);
        await transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).ToUniTask();
        transform.position = data.TargetPosition;
        await transform.DOScale(currentScale, 0.25f).SetEase(Ease.OutBack).ToUniTask();

        lockMoving = false;
        data.LinkedPortal.UnlockPortal();
        OnTurnMove?.Invoke();
    }

    public void TakeDamage()
    {
        if (spriteBlink.IsBlinking) return; //Invincible 
        LoseStar();
        currentHealth -= 1;
        Debug.Log($"Player took 1 damage. Current health: {currentHealth}");

        spriteBlink.Blink(blinkDuration);
        ShakeAnimation();
        AudioManager.Instance.PlaySfx("player_hurt", transform.position);


        if (currentHealth <= 0)
        {
            BossController bossController = FindFirstObjectByType<BossController>();
            if (bossController != null)
            {
                bossController.animator.Play("BossWin");
            }
            OnLoseGame?.Invoke();
        }
    }

    public void HandleCollectItem(ICollectible item)
    {
        if (!collectedItems.Contains(item))
        {
            collectedItems.Add(item);
            item.Collect();
        }
    }

    private void HandleTouchStorm()
    {
        AudioManager.Instance.PlaySfx("whoosh", transform.position);
        ShakeAnimation(() =>
        {
            foreach (var item in collectedItems)
            {
                item.Release(transform.position);
            }
            collectedItems.Clear();
        });
    }

    private void ShakeAnimation(Action onComplete = null)
    {
        moveSequence.Pause();
        //VibrationController.Instance.PlayHeavy();
        transform.DOShakeRotation(1f, new Vector3(0, 0, 30)).OnComplete(() =>
        {
            transform.rotation = Quaternion.identity;
            onComplete?.Invoke();
            moveSequence.Play();
        });
    }

    private void HandleSpecialTileInteraction(SpecialTile tile)
    {
        if (tile is ICollectible collectible)
        {
            HandleCollectItem(collectible);
        }
    }

    public void Heal()
    {
        currentHealth = Mathf.Min(currentHealth + 1, maxHealth);
        Debug.Log($"Player healed 1. Current health: {currentHealth}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            //var enemy = collision.GetComponent<EnemyController>();

            TakeDamage();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            //var enemy = collision.collider.GetComponent<EnemyController>();

            TakeDamage();
        }
    }

    public void LoseStar()
    {
        if (currentStarIndex < 0)
            return;
        starImages[currentStarIndex].rectTransform
            .DOPunchScale(Vector3.one * 0.5f, 0.4f, 8, 0.8f)
            .OnComplete(() =>
            {
                starImages[currentStarIndex--].sprite = starsSprites[1];
            });
    }
}

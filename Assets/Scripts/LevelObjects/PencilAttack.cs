using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

class PencilAttack : MonoBehaviour
{
    public static UnityAction OnPencilAttackTriggered;

    bool canFollow = false;
    float aimingDuration = 0.25f;
    float lockDuration = 1f;
    float speed = 5f;
    bool isMove = false;
    Vector3Int direction;
    Vector3Int initialCellPosition; // row or column depending on direction

    Tween pencilTween;

    private void Start()
    {
        // For testing purposes, trigger the pencil attack after 5 seconds
    }

    public void Initialise(float lockDuration, float speed, Vector3Int direction, Vector3Int initialCellPosition)
    {
        this.lockDuration = lockDuration;
        this.speed = speed;
        this.direction = direction;
        this.initialCellPosition = initialCellPosition;
        SetUpTransform();
    }

    private void SetUpTransform()
    {
        bool isHorizontal = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
        bool isTopLeft = (isHorizontal && direction.x > 0) || (!isHorizontal && direction.y < 0);

        Vector3Int spawnCell = initialCellPosition;
        if (isHorizontal)
        {
            if (isTopLeft)
                spawnCell.x = BossController.originCell.x - 2;
            else
                spawnCell.x = BossController.originCell.x + BossController.size + 1;
        }
        else
        {
            if (isTopLeft)
                spawnCell.y = BossController.originCell.y + 2;
            else
                spawnCell.y = BossController.originCell.y - BossController.size - 1;
        }
        transform.position = GridManager.Instance.GetCellCenteredWorldPosition(spawnCell);

        Vector3 targetJuicePos = transform.position;
        float juiceDistance = 1f; // Adjust this value to control how far the pencil moves during the juice effect

        if (isHorizontal && direction.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
            targetJuicePos += Vector3.right * juiceDistance;
        }
        else if (!isHorizontal && direction.y < 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, -90);
            targetJuicePos += Vector3.up * juiceDistance * 2;
        }
        else if (!isHorizontal && direction.y > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
            targetJuicePos += Vector3.down * juiceDistance;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            targetJuicePos += Vector3.left * juiceDistance;
        }

        // Use tween to fade in the pencil over the aiming duration
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        AudioManager.Instance.PlaySfx("pencil_appear", transform.position);

        pencilTween = DOTween.Sequence()
            .Append(sr.DOColor(originalColor, aimingDuration))
            .AppendInterval(lockDuration)
            .Append(transform.DOMove(targetJuicePos, 0.25f).SetEase(Ease.OutQuad))
            .OnComplete(() =>
            {
                isMove = true;
                AudioManager.Instance.PlaySfx("pencil_attack", transform.position);
            })
            .SetLink(gameObject);
    }

    private void Update()
    {
        if (!isMove) return;
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        // If out of camera bounds, destroy the pencil
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPos.x < -100 || screenPos.x > Screen.width + 100 || screenPos.y < -100 || screenPos.y > Screen.height + 100)
        {
            if (pencilTween != null && pencilTween.IsActive()) pencilTween.Kill();
            Destroy(gameObject);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player hit by pencil attack!");
            if (pencilTween != null && pencilTween.IsActive()) pencilTween.Kill();
            //Destroy(gameObject);
            var pencilCollider = GetComponent<Collider2D>();
            pencilCollider.enabled = false;
        }
    }

    public void TriggerPencilAttack()
    {
        Debug.Log("Pencil attack triggered!");
        OnPencilAttackTriggered?.Invoke();
    }
}
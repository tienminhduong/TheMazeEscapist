using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class SwordAttack : MonoBehaviour
{
    public Vector2 directionToBoss;
    [SerializeField] float speed = 5f;
    public static UnityAction<Vector3> OnSwordAttacked;
    bool isMove = false;
    Vector3 originalRotation;
    Vector3 originalScale;
    // Update is called once per frame
    void Start()
    {
        // translate rotation to vector2
        directionToBoss = transform.rotation * Vector2.up;
        originalRotation = transform.rotation.eulerAngles;
        originalScale = transform.localScale;

        StartAttackWindup();
    }
    void StartAttackWindup()
    {
        // pick left or right 45 degree offset away from original rotation
        float sign = transform.localScale.x > 0 ? -1f : 1f;
        float windupAngleZ = originalRotation.z + (45f * sign);

        // set half scale and rotate away from original instantly
        transform.localScale = originalScale * 0.5f;
        transform.rotation = Quaternion.Euler(originalRotation.x, originalRotation.y, windupAngleZ);

        DOTween.Sequence()
            .Append(transform.DORotate(originalRotation, 0.3f))
            .Join(transform.DOScale(originalScale, 0.3f))
            .OnComplete(() => isMove = true);
    }
    void Update()
    {
        if (!isMove) return;

        gameObject.transform.Translate(directionToBoss * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Boss"))
        {
            OnSwordAttacked?.Invoke(transform.position);
            AudioManager.Instance.PlaySfx("sword_hit", transform.position);
            Destroy(gameObject);
        }
    }
}

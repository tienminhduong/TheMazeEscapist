using DG.Tweening;
using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TurnTimer : MonoBehaviour
{
    [SerializeField] private int turnLimit = 5;
    [SerializeField] private string turnDisplayFormat = "Số lượt còn lại: {0}";
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private List<Sprite> stars;
    [SerializeField] private Image img;

    public static UnityAction OnTimeOut;
    bool animaEnd = false;

    private int currentTurn = 0;

    void Start()
    {
        int curLevel = PlayerProgress.GetCurrentLevelIndex();
        turnLimit = WorldMapRecycler.Instance.GetRefById(curLevel).limitMove;
        turnText.text = string.Format(turnDisplayFormat, turnLimit - currentTurn);
        img.sprite = stars[0];
    }

    void OnEnable()
    {
        PlayerController.OnTurnMove += HandleTurnMove;
    }
    void OnDisable()
    {
        PlayerController.OnTurnMove -= HandleTurnMove;
    }

    private void HandleTurnMove()
    {
        currentTurn++;
        turnText.text = string.Format(turnDisplayFormat, turnLimit - currentTurn);
        if (currentTurn >= turnLimit)
        {
            turnText.text = "NO MORE TURNS !!!";
            //img.sprite = stars[1];
            OnTimeOut?.Invoke();
            //AudioManager.Instance.PlaySfx("lose", Vector3.zero);
            DoAnimation();
        }
    }
    public void DoAnimation()
    {
        if (animaEnd == true) return;
        img.rectTransform
            .DOPunchScale(Vector3.one * 0.5f, 0.4f, 8, 0.8f)
            .OnComplete(() =>
            {
                img.sprite = stars[1];
            });

        turnText.rectTransform.DOShakeAnchorPos(
            1f,
            new Vector2(25f, 0),
            25,
            0,
            false,
            true
        );
        animaEnd = true;
        //DOVirtual.DelayedCall(1f, () =>
        //{
        //    PlayerController.OnLoseGame?.Invoke();
        //});
    }
    //public void DoAnimation()
    //{
    //    turnText.color = Color.red;

    //    RectTransform rect = turnText.rectTransform;

    //    // Lưu world position hiện tại
    //    Vector3 worldPos = rect.position + new Vector3(0, -100, 0);

    //    // Đổi anchor/pivot
    //    rect.anchorMin = new Vector2(0.5f, 0.5f);
    //    rect.anchorMax = new Vector2(0.5f, 0.5f);
    //    rect.pivot = new Vector2(0.5f, 0.5f);

    //    // Gán lại vị trí world để tránh bị jump
    //    rect.position = worldPos;

    //    Sequence seq = DOTween.Sequence();

    //    seq.Append(
    //        rect.DOAnchorPos(Vector2.zero, 1.5f)
    //            .SetEase(Ease.InOutQuart)
    //    );

    //    seq.Append(
    //        turnText.DOFade(0f, 0.25f)
    //                 .SetLoops(6, LoopType.Yoyo)
    //    );

    //    seq.OnComplete(() =>
    //    {
    //        PlayerController.OnLoseGame?.Invoke();
    //    });
    //}
}
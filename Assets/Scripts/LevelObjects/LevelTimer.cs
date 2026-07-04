using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelTimer : MonoBehaviour
{
    [SerializeField] private float timeLimit = 60f;
    private float timeRemaining;
    private bool timerRunning = false;

    [SerializeField] private string timerDisplayFormat = "Thời gian còn lại: {0:0.00}";
    [SerializeField] private TextMeshProUGUI timerText;
    public static UnityAction OnTimeOut;

    [SerializeField] private List<Sprite> stars;
    [SerializeField] private Image img;

    private void Start()
    {
        int curLevel = PlayerProgress.GetCurrentLevelIndex();
        timeLimit = WorldMapRecycler.Instance.GetRefById(curLevel).limitTime;
        timeRemaining = timeLimit;
        StartTimer();
        img.sprite = stars[0];
    }

    void OnEnable()
    {
        WinPoint.OnLevelComplete += StopTimer;
    }

    void OnDisable()
    {
        WinPoint.OnLevelComplete -= StopTimer;
    }

    private void Update()
    {
        if (timerRunning)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                timerRunning = false;
                OnTimeOut?.Invoke();
                //AudioManager.Instance.PlaySfx("lose", Vector3.zero);
                DoAnimation();
            }
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        timerText.text = string.Format(timerDisplayFormat, timeRemaining);
        if (timeRemaining <= 0)
        {
            timerText.text = "TIME OUT!";
            //img.sprite = stars[1];
        }
    }

    public void StartTimer()
    {
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResetTimer()
    {
        timeRemaining = timeLimit;
        UpdateTimerDisplay();
    }

    public void DoAnimation()
    {
        img.rectTransform
            .DOPunchScale(Vector3.one * 0.5f, 0.4f, 8, 0.8f)
            .OnComplete(() =>
            {
                img.sprite = stars[1];
            });

        timerText.rectTransform.DOShakeAnchorPos(
            1f,
            new Vector2(25f, 0),
            25,
            0,
            false,
            true
        );

        //DOVirtual.DelayedCall(1f, () =>
        //{
        //    PlayerController.OnLoseGame?.Invoke();
        //});
    }
    /* old
    public void DoAnimation()
    {
        timerText.color = Color.red;

        RectTransform rect = timerText.rectTransform;

        // Lưu world position hiện tại
        Vector3 worldPos = rect.position + new Vector3(0, -100, 0);

        // Đổi anchor/pivot
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        // Gán lại vị trí world để tránh bị jump
        rect.position = worldPos;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            rect.DOAnchorPos(Vector2.zero, 1.5f)
                .SetEase(Ease.InOutQuart)
        );

        seq.Append(
            timerText.DOFade(0f, 0.25f)
                     .SetLoops(6, LoopType.Yoyo)
        );

        seq.OnComplete(() =>
        {
            PlayerController.OnLoseGame?.Invoke();
        });
    }
    */
}
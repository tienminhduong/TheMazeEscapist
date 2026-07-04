using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SlideShow : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button moveLeft;
    [SerializeField] private Button moveRight;

    [Header("UI")]
    [SerializeField] private CanvasGroup container;
    [SerializeField] private CanvasGroup canvas;

    [SerializeField] private RectTransform[] slides;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private float slideDistance = 1200f;

    public int currentIndex;
    private bool isAnimating;
    public bool haveToOpen = false;
    private void Awake()
    {
        openButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
        moveLeft.onClick.AddListener(MoveLeft);
        moveRight.onClick.AddListener(MoveRight);
    }

    private void Start()
    {
        container.alpha = 0;
        container.blocksRaycasts = false;
        container.interactable = false;

        canvas.alpha = 0;
        canvas.blocksRaycasts = false;
        canvas.interactable = false;

        ShowSlideInstant(currentIndex);

        if (haveToOpen)
        {
            Open();
        }
    }

    private void Open()
    {
        container.DOFade(1, 0.25f);

        container.blocksRaycasts = true;
        container.interactable = true;

        canvas.DOFade(1, 0.25f);

        canvas.blocksRaycasts = true;
        canvas.interactable = true;
    }

    private void Close()
    {
        container.DOFade(0, 0.25f)
            .OnComplete(() =>
            {
                container.blocksRaycasts = false;
                container.interactable = false;
            });

        canvas.DOFade(0, 0.25f)
            .OnComplete(() =>
            {
                canvas.blocksRaycasts = false;
                canvas.interactable = false;
            });
    }

    private void MoveLeft()
    {
        if (isAnimating) return;

        int newIndex = currentIndex <= 0
            ? slides.Length - 1
            : currentIndex - 1;

        ChangeSlide(newIndex, -1);
    }

    private void MoveRight()
    {
        if (isAnimating) return;

        int newIndex = currentIndex >= slides.Length - 1
            ? 0
            : currentIndex + 1;

        ChangeSlide(newIndex, 1);
    }

    private void ChangeSlide(int newIndex, int direction)
    {
        isAnimating = true;

        RectTransform current = slides[currentIndex];
        RectTransform next = slides[newIndex];

        next.gameObject.SetActive(true);

        next.anchoredPosition = new Vector2(direction * slideDistance, 0);

        Sequence seq = DOTween.Sequence();

        seq.Join(
            current.DOAnchorPosX(-direction * slideDistance, slideDuration)
                   .SetEase(Ease.OutCubic)
        );

        seq.Join(
            next.DOAnchorPosX(0, slideDuration)
                .SetEase(Ease.OutCubic)
        );

        seq.OnComplete(() =>
        {
            current.gameObject.SetActive(false);

            current.anchoredPosition = Vector2.zero;
            next.anchoredPosition = Vector2.zero;

            currentIndex = newIndex;

            isAnimating = false;
        });
    }

    private void ShowSlideInstant(int index)
    {
        for (int i = 0; i < slides.Length; i++)
        {
            bool isCurrent = i == index;

            slides[i].gameObject.SetActive(isCurrent);
            slides[i].anchoredPosition = Vector2.zero;
        }
    }
}
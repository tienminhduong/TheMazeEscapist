using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinGameUI : MonoBehaviour
{
    [SerializeField] private Transform popup;

    [SerializeField] private Image[] stars;

    [SerializeField] private Sprite starOn;
    [SerializeField] private Sprite starOff;

    private void OnEnable()
    {
        WinPoint.OnLevelComplete += DoPop;
    }

    private void OnDisable()
    {
        WinPoint.OnLevelComplete -= DoPop;
    }
    private void Start()
    {
        popup.localScale = Vector3.zero;
    }

    private void DoPop()
    {
        popup.localScale = Vector3.zero;

        popup.DOScale(Vector3.one, 1.5f)
             .SetEase(Ease.OutBack)
             .OnComplete(() =>
             {
                 string currentScene = SceneManager.GetActiveScene().name;
                 int currentLevel = int.Parse(currentScene.Replace("Level ", ""));
                 PlayStarAnimation(currentLevel);
             });
    }

    private void PlayStarAnimation(int level)
    {
        List<bool> starData = PlayerProgress.GetStarAtLevel(level);

        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < stars.Length; i++)
        {
            int index = i;

            seq.AppendInterval(0.25f);

            seq.AppendCallback(() =>
            {
                AnimateStar(stars[index], starData[index]);
            });
        }
    }
    private void AnimateStar(Image star, bool earned)
    {
        star.transform.localScale = Vector3.zero;

        if (earned)
        {
            star.sprite = starOn;

            Sequence seq = DOTween.Sequence();

            seq.Append(
                star.transform.DOScale(1.3f, 0.3f)
                    .SetEase(Ease.OutBack)
            );

            seq.Join(
                star.transform.DORotate(
                    new Vector3(0, 360, 0),
                    0.5f,
                    RotateMode.FastBeyond360)
            );

            Vector3 pos = star.transform.localPosition;
            seq.Join(
                    star.transform.DOLocalMoveY(
                    pos.y + 15f,
                    0.15f
                    ).SetEase(Ease.OutQuad)
            );


            seq.Append(
                star.transform.DOScale(1f, 0.15f)
            );

            seq.Join(
                    star.transform.DOLocalMoveY(
                    pos.y,
                    0.2f
                    ).SetEase(Ease.InQuad)
            );

        }
        else
        {
            star.sprite = starOff;

            star.transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();

            seq.Append(
                star.transform.DOScale(1f, 0.25f)
                .SetEase(Ease.OutBack)
            );

            seq.Join(
                star.DOFade(1, 0.25f)
            );
        }
    }
    public void BackToSelectLevel()
    {
        SceneController.Instance.TransitionToScene("LevelSelection");
    }

    public void NextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        int currentLevel = int.Parse(currentScene.Replace("Level ", ""));

        if (currentLevel == 29) BackToSelectLevel();

        if (currentLevel == PlayerProgress.CurrentLevel)
        {
            PlayerProgress.UnlockNextLevel();
        }

        SceneController.Instance.TransitionToScene($"Level {currentLevel + 1}");
    }
    public void RestartLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneController.Instance.TransitionToScene(currentSceneName);
    }

}

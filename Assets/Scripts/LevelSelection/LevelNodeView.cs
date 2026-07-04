using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class LevelNodeView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text levelText;

    [SerializeField]
    private Button button;

    [SerializeField]
    private Image lockImage;

    private int levelId;

    private Tween bounceTween;

    [SerializeField]
    private List<Image> stars;
    [SerializeField]
    private List<Sprite> starSprites;



    public void Setup(
        int id,
        bool unlocked)
    {
        levelId = id;

        levelText.text = id.ToString();

        button.interactable = unlocked;

        if (lockImage != null)
        {
            lockImage.gameObject.SetActive(!unlocked);
        }

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(OnClick);

        SetUpStarStatus();

        SetupCurrentLevelAnimation();
    }
    void SetUpStarStatus()
    {
        List<bool> stars = PlayerProgress.GetStarAtLevel(levelId);
        for(int i = 0; i < this.stars.Count; i++)
        {
            this.stars[i].sprite = stars[i]? starSprites[0]: starSprites[1];
        }    
    }    
    void SetupCurrentLevelAnimation()
    {
        transform.localScale = Vector3.one;

        bounceTween?.Kill();

        if (levelId != PlayerProgress.CurrentLevel)
            return;

        bounceTween =
            transform
                .DOScale(1.15f, 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
    }
    void OnDestroy()
    {
        bounceTween?.Kill();
    }

    public static UnityAction<int> clickLevelNode;
    void OnClick()
    {
        clickLevelNode?.Invoke(levelId);
    }
}
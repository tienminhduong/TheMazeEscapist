using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailPanel : MonoBehaviour
{
    [SerializeField] GameObject detailLevel;
    [SerializeField] Button play;
    [SerializeField] Button close;
    private int selectedLevelId;

    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI required_one;
    [SerializeField] private TextMeshProUGUI required_two;
    [SerializeField] private TextMeshProUGUI required_three;

    [SerializeField] private List<Image> stars;
    [SerializeField] private Sprite starOn;
    [SerializeField] private Sprite starOff;
    private void Start()
    {
        detailLevel.SetActive(false);
    }
    private void OnEnable()
    {
        LevelNodeView.clickLevelNode += OpenDetail;
    }

    private void OnDisable()
    {
        LevelNodeView.clickLevelNode -= OpenDetail;
    }
    private void Awake()
    {
        play.onClick.AddListener(() =>
        {
            PlayLevel(selectedLevelId);
        });

        close.onClick.AddListener(CloseDetail);
    }
    public void OpenDetail(int levelId)
    {
        selectedLevelId = levelId;
        GetRefDetail(levelId);
        detailLevel.SetActive(true);
    }
    public void CloseDetail()
    {
        detailLevel.SetActive(false);
    }
    public void PlayLevel(int levelId)
    {
        SceneController.Instance.TransitionToScene($"Level {levelId}");
    }
    public void GetRefDetail(int levelId)
    {
        int index = levelId - 1;

        LevelNodeData levelDetail = WorldMapRecycler.Instance.GetRefById(levelId);

        levelNameText.text = $"Màn chơi số {levelId}";
        required_one.text = "Hoàn thành màn chơi";
        required_two.text = $"Hoàn thành với {levelDetail.limitMove} lượt";
        required_three.text = $"Hoàn thành trong {levelDetail.limitTime} giây";

        Debug.Log("LIMITS:  " + levelDetail.limitTime + "  " + levelDetail.limitMove);

        List<bool> starData =
            PlayerProgress.GetStarAtLevel(levelId);

        int starCount = 0;

        for (int i = 0; i < stars.Count; i++)
        {
            bool achieved = starData[i];

            stars[i].sprite =
                achieved ? starOn : starOff;

            if (achieved)
                starCount++;
        }

    }
}

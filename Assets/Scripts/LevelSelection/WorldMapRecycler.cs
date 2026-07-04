using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldMapRecycler : MonoBehaviour
{
    public static WorldMapRecycler Instance;
    [SerializeField]
    private List<SectionData> allData;
    private void Awake()
    {
        Instance = this;
    }
    public List<SectionData> AllData => allData;
    [Header("References")]
    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private SectionView sectionPrefab;

    [Header("Settings")]
    [SerializeField]
    private float sectionHeight = 1200f;

    [SerializeField]
    private int visibleCount = 3;


    private readonly List<SectionView>
        activeSections = new();

    private int topIndex;

    private int bottomIndex;

    private void Start()
    {
        if (allData == null ||
            allData.Count == 0)
        {
            Debug.LogError("No Section Data!");

            enabled = false;

            return;
        }
        SetupContent();

        CreateInitialSections();

        ScrollToCurrentLevelSmooth();

        PlayerPrefs.SetString("Level1", "true|true|true");
    }
    public void PlusLevel()
    {
        PlayerProgress.UnlockNextLevel();

        Debug.Log(
            "Cur level " +
            PlayerProgress.CurrentLevel);
    }



    private void Update()
    {
        if (activeSections.Count == 0)
            return;

        CheckRecycleDown();

        CheckRecycleUp();
    }

    void SetupContent()
    {
        float totalHeight =
            allData.Count * sectionHeight;

        content.sizeDelta =
            new Vector2(
                content.sizeDelta.x,
                totalHeight);
    }

    void CreateInitialSections()
    {
        int count =
            Mathf.Min(
                visibleCount,
                allData.Count);

        for (int i = 0; i < count; i++)
        {
            CreateSection(i);
        }

        topIndex = 0;

        bottomIndex = count - 1;
    }

    void CreateSection(int dataIndex)
    {
        SectionView section =
            Instantiate(
                sectionPrefab,
                content);

        RectTransform rect =
            section.Rect;

        rect.anchoredPosition =
            new Vector2(
                0,
                -dataIndex * sectionHeight);

        section.Init(
            dataIndex,
            allData[dataIndex]);

        activeSections.Add(section);
    }

    void CheckRecycleDown()
    {
        if (activeSections.Count == 0)
            return;

        SectionView topSection =
            activeSections[0];

        float scrollY =
            content.anchoredPosition.y;

        float topSectionBottom =
            Mathf.Abs(
                topSection.Rect
                    .anchoredPosition.y)
            + sectionHeight;

        if (scrollY > topSectionBottom)
        {
            RecycleTopToBottom();
        }
    }

    void CheckRecycleUp()
    {
        if (topIndex <= 0)
            return;

        if (activeSections.Count == 0)
            return;

        SectionView bottomSection =
            activeSections[
                activeSections.Count - 1];

        float scrollY =
            content.anchoredPosition.y;

        float bottomTop =
            Mathf.Abs(
                bottomSection.Rect
                    .anchoredPosition.y);

        float viewportHeight =
            scrollRect.viewport.rect.height;

        if (scrollY <
            bottomTop - viewportHeight)
        {
            RecycleBottomToTop();
        }
    }

    void RecycleTopToBottom()
    {
        if (bottomIndex >= allData.Count - 1)
            return;

        SectionView topSection =
            activeSections[0];

        activeSections.RemoveAt(0);

        bottomIndex++;

        float newY =
            -bottomIndex * sectionHeight;

        topSection.Rect
            .anchoredPosition =
            new Vector2(0, newY);

        topSection.Init(
            bottomIndex,
            allData[bottomIndex]);

        activeSections.Add(topSection);

        topIndex++;

        Debug.Log(
            "Recycle Down -> Section "
            + bottomIndex);
    }


    void RecycleBottomToTop()
    {
        SectionView bottomSection =
            activeSections[
                activeSections.Count - 1];

        activeSections.RemoveAt(
            activeSections.Count - 1);

        topIndex--;

        float newY =
            -topIndex * sectionHeight;

        bottomSection.Rect
            .anchoredPosition =
            new Vector2(0, newY);

        bottomSection.Init(
            topIndex,
            allData[topIndex]);

        activeSections.Insert(
            0,
            bottomSection);

        bottomIndex--;

        Debug.Log(
            "Recycle Up -> Section "
            + topIndex);
    }
    void ScrollToCurrentLevelSmooth()
    {
        int currentLevel =
            PlayerProgress.CurrentLevel;

        int sectionIndex =
            (currentLevel - 1) / 3;

        sectionIndex =
            Mathf.Clamp(
                sectionIndex,
                0,
                allData.Count - 1);

        float targetY =
            (allData.Count - 1 - sectionIndex)
            * sectionHeight;

        float maxY =
            content.rect.height -
            scrollRect.viewport.rect.height;

        // Chỉ center nếu không quá gần đầu/cuối
        bool canCenter =
            sectionIndex > 1 &&
            sectionIndex < allData.Count - 2;

        if (canCenter)
        {
            float viewportHalf =
                scrollRect.viewport.rect.height / 2f;

            targetY -=
                viewportHalf -
                sectionHeight / 2f;
        }
        else if (sectionIndex <= 1)
        {
            content.anchoredPosition =
            new Vector2(0, maxY);
            return;
        } 
            

            targetY =
                Mathf.Clamp(
                    targetY,
                    0,
                    maxY);

        content.anchoredPosition =
            new Vector2(0, maxY);

        StartCoroutine(
            SmoothScroll(targetY));
    }
    //void ScrollToCurrentLevelSmooth()
    //{
    //    int currentLevel =
    //        PlayerProgress.CurrentLevel;

    //    int targetIndex =
    //        Mathf.Clamp(
    //            currentLevel - 1,
    //            0,
    //            allData.Count - 1);

    //    float targetY =
    //        (allData.Count - 1 - targetIndex)
    //        * sectionHeight;

    //    float maxY =
    //        content.rect.height -
    //        scrollRect.viewport.rect.height;

    //    targetY =
    //        Mathf.Clamp(
    //            targetY,
    //            0,
    //            maxY);

    //    // bắt đầu từ bottom
    //    content.anchoredPosition =
    //        new Vector2(0, maxY);

    //    StartCoroutine(
    //        SmoothScroll(targetY));
    //}

    IEnumerator SmoothScroll(float targetY)
    {
        //for (int i = 0; i < 20; i++)
        //{
        //    CheckRecycleDown();
        //}

        float duration = 3f;

        float elapsed = 0f;

        float startY =
            content.anchoredPosition.y;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.SmoothStep(
                    0,
                    1,
                    elapsed / duration);

            float newY =
                Mathf.Lerp(
                    startY,
                    targetY,
                    t);

            content.anchoredPosition =
                new Vector2(0, newY);

            yield return null;
        }

        content.anchoredPosition =
            new Vector2(0, targetY);
    }
    public void LoadCurrentLevel()
    {
        SceneManager.LoadScene("Level " + PlayerProgress.CurrentLevel);
    }

    public void ClearDataAndReload()
    {
        PlayerProgress.SetCurrentLevel(1);
        SceneController.Instance.TransitionToScene("LevelSelection");
    }

    //public LevelNodeData GetRefById(int levelId)
    //{
    //    int index = levelId - 1;
    //    var data = WorldMapRecycler.Instance.AllData;
    //    LevelNodeData levelNodeData = data[index / 3].levels[index % 3];
    //    return levelNodeData;
    //}
    public LevelNodeData GetRefById(int levelId)
    {
        int index = levelId - 1;

        var data = WorldMapRecycler.Instance.AllData;

        int sectionIndex = index / 3;
        int levelIndex = index % 3;

        // Đảo lại vì AllData đang bị ngược
        sectionIndex = data.Count - 1 - sectionIndex;

        return data[sectionIndex].levels[levelIndex];
    }
}
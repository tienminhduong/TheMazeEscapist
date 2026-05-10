using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapRecycler : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private SectionView sectionPrefab;

    [Header("Settings")]
    [SerializeField]
    private float sectionHeight = 800f;

    [SerializeField]
    private int visibleCount = 3;

    [SerializeField]
    private List<SectionData> allData;

    private readonly List<SectionView>
        activeSections = new();

    private int topIndex;

    private int bottomIndex;

    private void Start()
    {
        if (allData == null ||
            allData.Count == 0)
        {
            Debug.LogError(
                "No Section Data!");

            enabled = false;

            return;
        }

        PlayerProgress.SetCurrentLevel(1);

        Debug.Log(
            "Cur level " +
            PlayerProgress.CurrentLevel);

        SetupContent();

        CreateInitialSections();
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
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SectionView : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private LevelNodeView nodePrefab;

    [SerializeField]
    private RectTransform nodeContainer;

    [SerializeField]
    private RectTransform pathContainer;

    [SerializeField]
    private Image linePrefab;

    private RectTransform rect;

    public RectTransform Rect => rect;

    private int currentIndex;

    public int CurrentIndex => currentIndex;

    private List<RectTransform> spawnedNodes =
        new();

    private List<GameObject> spawnedLines =
        new();

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Init(
        int sectionIndex,
        SectionData data)
    {
        currentIndex = sectionIndex;

        Clear();

        Debug.Log(
            "Render Section " +
            sectionIndex);

        SpawnNodes(data);

        DrawPaths();
    }

    void Clear()
    {
        spawnedNodes.Clear();

        foreach (GameObject line in spawnedLines)
        {
            Destroy(line);
        }

        spawnedLines.Clear();

        foreach (Transform child in nodeContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in pathContainer)
        {
            Destroy(child.gameObject);
        }
    }

    void SpawnNodes(SectionData data)
    {
        foreach (LevelNodeData level
                 in data.levels)
        {
            LevelNodeView node =
                Instantiate(
                    nodePrefab,
                    nodeContainer);

            RectTransform nodeRect =
                node.GetComponent<RectTransform>();

            nodeRect.anchoredPosition =
                level.position;

            bool unlocked =
                PlayerProgress.IsUnlocked(
                    level.levelId);

            node.Setup(
                level.levelId,
                unlocked);

            spawnedNodes.Add(nodeRect);
        }
    }

    void DrawPaths()
    {
        for (int i = 0;
             i < spawnedNodes.Count - 1;
             i++)
        {
            CreateLine(
                spawnedNodes[i],
                spawnedNodes[i + 1]);
        }
    }

    void CreateLine(
        RectTransform a,
        RectTransform b)
    {
        Image line =
            Instantiate(
                linePrefab,
                pathContainer);

        RectTransform rect =
            line.rectTransform;

        Vector2 dir =
            b.anchoredPosition -
            a.anchoredPosition;

        float distance =
            dir.magnitude;

        rect.sizeDelta =
            new Vector2(
                distance,
                12);

        rect.anchoredPosition =
            (a.anchoredPosition +
             b.anchoredPosition) / 2f;

        float angle =
            Mathf.Atan2(
                dir.y,
                dir.x)
            * Mathf.Rad2Deg;

        rect.rotation =
            Quaternion.Euler(
                0,
                0,
                angle);

        line.transform.SetAsFirstSibling();

        spawnedLines.Add(line.gameObject);
    }
}
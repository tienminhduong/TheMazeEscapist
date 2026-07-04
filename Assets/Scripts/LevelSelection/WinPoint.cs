using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class WinPoint : SpecialTile
{
    public static UnityAction OnLevelComplete;
    [SerializeField] private List<WinpointUnlockCondition> unlockConditions = new();
    public static UnityAction<string> OnUnlockedConditionMet;
    public static UnityAction<string> OnLockedConditionMet;

    private Dictionary<string, bool> conditionStatus = new();
    private int conditionsNotMetCount = 0;
    bool s1 = true, s2 = true, s3 = true;
    public override TileType Type => TileType.WinPoint;

    void Awake()
    {
        if (unlockConditions.Count > 0)
        {
            gameObject.SetActive(false);
            conditionsNotMetCount = unlockConditions.Count;
            foreach (var condition in unlockConditions)
            {
                if (conditionStatus.ContainsKey(condition.conditionName))
                {
                    Debug.LogError($"Duplicate condition: {condition.conditionName} in WinPoint: {gameObject.name}");
                    continue;
                }
                conditionStatus[condition.conditionName] = false;
            }
        }

        OnUnlockedConditionMet += UnlockWinPoint;
        OnLockedConditionMet += LockWinPoint;
        OnLevelComplete += CompletedLevel;
        TurnTimer.OnTimeOut += SetSecStarStatus;
        LevelTimer.OnTimeOut += SetThirdStarStatus;
    }

    private void LockWinPoint(string conditionName)
    {
        if (!conditionStatus.ContainsKey(conditionName) || !conditionStatus[conditionName])
            return;

        conditionStatus[conditionName] = false;
        conditionsNotMetCount++;
        if (conditionsNotMetCount == 1)
        {
            transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }
    }

    void SetSecStarStatus()
    {
        s2 = false;
    }    
    void SetThirdStarStatus()
    {
        s3 = false;
    }    

    void OnDestroy()
    {
        OnUnlockedConditionMet -= UnlockWinPoint;
        OnLockedConditionMet -= LockWinPoint;
    }

    void Start()
    {
        if (unlockConditions.Count == 0)
        {
            gameObject.SetActive(true);
            OnInstantiated();
        }
    }

    private void UnlockWinPoint(string conditionName)
    {
        if (!conditionStatus.ContainsKey(conditionName) || conditionStatus[conditionName])
            return;

        conditionStatus[conditionName] = true;
        conditionsNotMetCount--;

        if (conditionsNotMetCount > 0)
            return;

        gameObject.SetActive(true);
        AudioManager.Instance.PlaySfx("win_point_unlocked", transform.position);
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        OnInstantiated();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CheckLevel();
            OnLevelComplete?.Invoke();
            AudioManager.Instance.PlaySfx("victory", Vector2.zero);
        }
    }
    private void CompletedLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        int currentLevel = int.Parse(currentScene.Replace("Level ", ""));
        PlayerProgress.SetStarAtLevel(currentLevel, s1, s2, s3);
    }    
    private void CheckLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        int currentLevel = int.Parse(currentScene.Replace("Level ", ""));

        if (currentLevel == PlayerProgress.CurrentLevel && currentLevel != 29)
        {
            PlayerProgress.UnlockNextLevel();
        }

    }

}
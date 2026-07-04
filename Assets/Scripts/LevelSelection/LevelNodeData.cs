using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LevelNodeData
{
    public int levelId;

    public Vector2 position;

    public int limitMove;
    public int limitTime;
    //public bool unlocked;
}
public static class PlayerProgress
{
    public static int CurrentLevel =>
        PlayerPrefs.GetInt(
            "CurrentLevel",
            1);

    public static bool IsUnlocked(
        int levelId)
    {
        return levelId <= CurrentLevel;
    }

    public static void SetCurrentLevel(
        int level)
    {
        PlayerPrefs.SetInt(
            "CurrentLevel",
            level);

        PlayerPrefs.Save();
    }

    public static void UnlockNextLevel()
    {
        SetCurrentLevel(CurrentLevel + 1);
    }

    public static void SetStarAtLevel(int level, bool s1, bool s2, bool s3)
    {
        List<bool> newStars = new List<bool>() { s1, s2, s3 };
        List<bool> oldStars = GetStarAtLevel(level);

        int oldCount = CountStars(oldStars);
        int newCount = CountStars(newStars);
        if (newCount >= oldCount)
        {
            string data = string.Join("|", newStars);
            PlayerPrefs.SetString($"Level{level}", data);
            PlayerPrefs.Save();
        }
    }

    private static int CountStars(List<bool> stars)
    {
        return stars.Count(x => x);
    }
    public static List<bool> GetStarAtLevel(int level)
    {
        string[] data = PlayerPrefs
            .GetString($"Level{level}", "false|false|false")
            .Split('|');

        List<bool> result = new List<bool>();

        foreach (string item in data)
        {
            result.Add(bool.Parse(item));
        }

        return result;
    }
    public static int GetNumberOfStarsAt(int level)
    {
        int nos = 0;
        foreach (var item in GetStarAtLevel(level))
        {
            nos += item ? 1 : 0;
        }
        return nos;
    }
    public static int GetCurrentLevelIndex()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string levelId = currentScene.Replace("Level ", "");
        if (!int.TryParse(levelId, out int levelIndex))
        {
            Debug.LogError($"Failed to parse level index from scene name: {currentScene}");
            return -1; // Return an invalid index or handle the error as needed
        }
        return levelIndex;
    }
}
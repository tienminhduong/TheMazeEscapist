using UnityEngine;

[System.Serializable]
public class LevelNodeData
{
    public int levelId;

    public Vector2 position;

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
        SetCurrentLevel(
            CurrentLevel + 1);
    }
}
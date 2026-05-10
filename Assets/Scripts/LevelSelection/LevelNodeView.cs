using TMPro;
using UnityEngine;
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

    public void Setup(
        int id,
        bool unlocked)
    {
        levelId = id;

        levelText.text =
            id.ToString();

        button.interactable =
            unlocked;

        if (lockImage != null)
        {
            lockImage.gameObject
                .SetActive(!unlocked);
        }

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        Debug.Log(
            "Open Level: " +
            levelId);
    }
}
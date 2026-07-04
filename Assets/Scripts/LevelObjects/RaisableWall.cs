using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class RaisableWall : SpecialTile
{
    [SerializeField] private string soundEffectName = "trash_can_collected";
    public override TileType Type => TileType.Wall;
    [SerializeField] private GameObject wallSpriteObject;
    void Start()
    {
        //OnInstantiated();
        if (wallSpriteObject == null)
        {
            wallSpriteObject = transform.GetChild(0).gameObject;
        }
    }

    public void Raise()
    {
        this.gameObject.SetActive(true);
        // Tween y position of wallSpriteObject to newY
        var tween = wallSpriteObject.transform.DOLocalMoveY(0, 1f).SetEase(Ease.OutExpo);
    }

    public void Lower()
    {
        this.gameObject.SetActive(true);
        float newY = 0f;
        var tween = wallSpriteObject.transform.DOLocalMoveY(-1.1f, 1f).SetEase(Ease.InExpo);
        tween.OnComplete(() =>
        {
            Destroy(this.gameObject);
        });

    }
}

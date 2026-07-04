using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class EyeofTheStorm : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 gridAnchor = new(0.5f, 0.5f, 0);
    [SerializeField] private bool moveOnStart = true;

    private Queue<Node> currentPath = new();
    public static event Action OnTouchPlayer;

    private PathFindingLogic pathfindingLogic;
    private Vector3? targetingPosition = null;
    private bool isMoving = false;

    private bool isWandering = true;
    private bool isFindNewPath = false;

    private void Start()
    {
        pathfindingLogic = new PathFindingLogic();
    }

    void OnEnable()
    {
        PlayerController.OnTurnMove += UpdateNewPath;
        SpecialTile.OnSpecialTileInteracted += HandleTrashCollected;
    }

    void OnDisable()
    {
        PlayerController.OnTurnMove -= UpdateNewPath;
        SpecialTile.OnSpecialTileInteracted -= HandleTrashCollected;
    }

    private void UpdateNewPath()
    {
        if (!moveOnStart)
            return;
        currentPath.Clear();
        var path = pathfindingLogic.FindPathFromWorldPos(
            targetingPosition == null ? transform.position : targetingPosition.Value,
            isWandering ? GridManager.Instance.GetRandomWalkableCellPosition() : player.position);
        foreach (var node in path)
        {
            currentPath.Enqueue(node);
        }
    }

    private void MoveWithCurrentPath()
    {
        if (currentPath.Count > 0 && !isMoving)
        {
            isMoving = true;
            PopAndMovePath().OnComplete(() =>
            {
                isMoving = false;
                targetingPosition = null;
                AudioManager.Instance.PlaySfx("wind", transform.position);
            });
        }
    }

    private Tween PopAndMovePath()
    {
        if (currentPath.Count <= 0)
            return null;

        var nextNode = currentPath.Dequeue();
        targetingPosition = GridManager.Instance.CellToWorld(nextNode.position) + gridAnchor;
        if (currentPath.Count == 0)
            UpdateNewPath();
        return transform.DOMove(targetingPosition.Value, 1f / speed).SetEase(Ease.Linear);
    }

    void Update()
    {
        MoveWithCurrentPath();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnTouchPlayer?.Invoke();
            isWandering = true;
        }
    }

    void HandleTrashCollected(SpecialTile data)
    {
        if (data.Type == TileType.Trash || data.Type == TileType.StudentCard)
        {
            isWandering = false;
            moveOnStart = true;
            UpdateNewPath();
        }
    }
}
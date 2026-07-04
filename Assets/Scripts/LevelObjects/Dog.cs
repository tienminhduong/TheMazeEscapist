using System;
using System.Collections.Generic;
using DG.Tweening;
using KingCat.Base;
using UnityEngine;

public class Dog : MonoBehaviour
{
    private List<Vector2> lookDirections = new() { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
    [SerializeField] LayerMask targetLayerMask;
    [SerializeField] float speed = 1f;
    [SerializeField] float patrolSpeed = .5f;
    [SerializeField] Vector3 gridAnchor = new(0.5f, 0.5f, 0);
    [SerializeField] DogState currentState = DogState.Idle;
    [SerializeField] private float patrolInterval = 2f;
    private bool isMoving = false;
    private Vector3 lastMoveDirection = Vector3.zero;
    private Sequence moveSequence;
    private Vector2Int? playerDirection = null;
    private Animator animator;
    private float patrolTimer = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        SwitchState(DogState.Idle);
    }

    void Update()
    {
        UpdateState();
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case DogState.Idle:
                UpdateIdleState();
                break;
            case DogState.Chase:
                UpdateChaseState();
                break;
            case DogState.Patrol:
                UpdatePatrolState();
                break;
        }
        UpdateAnimation();
    }

    private void UpdateChaseState()
    {
        if (isMoving) return;

        if (playerDirection.HasValue)
        {
            var path = GridManager.Instance.FindPathFromWorld(transform.position, playerDirection.Value);
            playerDirection = null; // Reset player direction after using it
            MoveAlongPath(path);
        }

        if (!isMoving)
        {
            Debug.Log("[Dog] Lost sight of player, returning to patrol.");
            SwitchState(DogState.Patrol);
        }
    }

    private void UpdatePatrolState()
    {
        if (FindPlayer() != null)
        {
            Debug.Log("[Dog] Player detected, switching to chase state.");
            SwitchState(DogState.Chase);
            CancelMovement();
            SnapToGrid();
        }
        if (!isMoving)
        {
            if (patrolTimer > 0)
            {
                patrolTimer -= Time.deltaTime;
                if (patrolTimer <= 0)
                {
                    var moveDirection = -lastMoveDirection;
                    var path = GridManager.Instance.FindPathFromWorld(transform.position, new Vector2Int((int)moveDirection.x, (int)moveDirection.y));
                    animator.Play("DogPatrol");
                    MoveAlongPath(path);
                    patrolTimer = patrolInterval;
                }
            }
        }
    }

    private void SnapToGrid()
    {
        // round its position to the nearest grid point based on the gridAnchor
        Vector3 snappedPosition = new Vector3(
            Mathf.Round(transform.position.x - gridAnchor.x) + gridAnchor.x,
            Mathf.Round(transform.position.y - gridAnchor.y) + gridAnchor.y,
            transform.position.z
        );

        transform.position = snappedPosition;
    }

    private Vector2Int? FindPlayer()
    {
        foreach (var direction in lookDirections)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity, targetLayerMask);
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                Debug.Log("[Dog] Player found in direction: " + direction);
                playerDirection = new Vector2Int((int)direction.x, (int)direction.y);
                return playerDirection;
            }
        }

        Debug.Log("[Dog] Player not found in any direction.");
        playerDirection = null;
        return null;
    }

    private void UpdateIdleState()
    {
        // Implement idle behavior
    }

    private void MoveAlongPath(Path path)
    {
        if (path.directions.Count == 0) return;
        isMoving = true;
        moveSequence = DOTween.Sequence();
        var currentPos = transform.position;
        var currentSpeed = currentState == DogState.Patrol ? patrolSpeed : speed;
        foreach (var dir in path.directions)
        {
            moveSequence.AppendCallback(() =>
            {
                var localScale = transform.localScale;
                if (dir.direction.x != 0)
                {
                    localScale.x = dir.direction.x > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                    transform.localScale = localScale;
                }
            });
            moveSequence.Append(transform.DOMove(currentPos + new Vector3(dir.direction.x, dir.direction.y, 0) * path.stepLength, 0.1f / currentSpeed)
                .SetEase(Ease.Linear));
            currentPos += new Vector3(dir.direction.x, dir.direction.y, 0) * path.stepLength;
            lastMoveDirection = new Vector3(dir.direction.x, dir.direction.y, 0);
        }
        moveSequence.OnComplete(() =>
        {
            isMoving = false;
            animator.Play("DogIdle");
        });
    }

    private void CancelMovement()
    {
        if (moveSequence != null && moveSequence.IsActive())
        {
            moveSequence.Kill();
            isMoving = false;
        }
    }

    private void SwitchState(DogState newState)
    {
        if (currentState != newState)
        {
            Debug.Log($"[Dog] Switching state from {currentState} to {newState}");
            currentState = newState;

            if (newState == DogState.Patrol)
            {
                patrolTimer = patrolInterval;
            }

            if (newState == DogState.Win)
            {
                VibrationController.Instance.PlayHeavy();
                transform.DOShakePosition(1f, new Vector3(0.5f, 0.5f, 0)).OnComplete(() =>
                {
                    transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), transform.position.z);
                    PlayerController.OnLoseGame?.Invoke();
                });
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (currentState == DogState.Chase)
            {
                SwitchState(DogState.Win);
                
                return;
            }
            AudioManager.Instance.PlaySfx("dog", transform.position);
            SwitchState(DogState.Chase);
        }
    }

    private string currentAnimState = "";

    private void PlayAnimState(string stateName)
    {
        if (currentAnimState != stateName)
        {
            animator.Play(stateName);
            currentAnimState = stateName;
        }
    }

    private void UpdateAnimation()
    {
        if (currentState == DogState.Chase)
        {
            PlayAnimState("DogChasing");
        }
        else if (currentState == DogState.Idle || !isMoving)
        {
            PlayAnimState("DogIdle");
        }
        else if (currentState == DogState.Patrol)
        {
            PlayAnimState("DogPatrol");
        }
    }
}

public enum DogState
{
    Idle,
    Chase,
    Patrol,
    Win
}
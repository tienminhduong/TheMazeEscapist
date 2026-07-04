using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OneWayDoor : SpecialTile
{
    private const string FROM_TOP_IDLE_ANIMATION = "OneWayDoorTopIdle";
    private const string FROM_BOTTOM_IDLE_ANIMATION = "OneWayDoorBottomIdle";
    private const string FROM_LEFT_IDLE_ANIMATION = "OneWayDoorLeftIdle";
    private const string FROM_RIGHT_IDLE_ANIMATION = "OneWayDoorRightIdle";

    private const string FROM_TOP_OPEN_ANIMATION = "OneWayDoorFromTop";
    private const string FROM_BOTTOM_OPEN_ANIMATION = "OneWayDoorFromBottom";
    private const string FROM_LEFT_OPEN_ANIMATION = "OneWayDoorFromLeft";
    private const string FROM_RIGHT_OPEN_ANIMATION = "OneWayDoorFromRight";

    private const string FROM_TOP_CLOSE_ANIMATION = "OneWayDoorCloseUp";
    private const string FROM_BOTTOM_CLOSE_ANIMATION = "OneWayDoorCloseBottom";
    private const string FROM_LEFT_CLOSE_ANIMATION = "OneWayDoorCloseLeft";
    private const string FROM_RIGHT_CLOSE_ANIMATION = "OneWayDoorCloseRight";


    [Tooltip("The direction from which the door can be passed through")]
    [SerializeField] Vector2 direction; // The direction from which the door can be passed through

    private Animator animator;

    public float StopTime = 0.5f; // Time to stop the player when waiting the door open

    public override TileType Type => TileType.OneWayDoor;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.Play(ChooseIdleAnimation());
    }

    void Start()
    {
        OnInstantiated();
    }

    public bool CanGoThrough(Vector2 moveDirection)
    {
        return moveDirection == direction;
    }

    public void Open()
    {
        StartCoroutine(OpenCoroutine());
    }

    private IEnumerator OpenCoroutine()
    {
        string openAnimation = ChooseOpenAnimation();

        // 1. Open
        animator.Play(openAnimation);
        yield return new WaitUntil(() => IsAnimationFinished(openAnimation));

        // Dừng lại một lúc
        yield return new WaitForSeconds(StopTime);

        // 2. Close
        string closeAnimation = ChooseCloseAnimation();
        animator.Play(closeAnimation);
        yield return new WaitUntil(() => IsAnimationFinished(closeAnimation));

        //chuyển sang Idle
        animator.Play(ChooseIdleAnimation());
    }

    private bool IsAnimationFinished(string animationName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(animationName) &&
               stateInfo.normalizedTime >= 1f;
    }

    private string ChooseIdleAnimation()
    {
        switch(direction)
        {
            case Vector2 up when direction == Vector2.up:
                return FROM_BOTTOM_IDLE_ANIMATION;
            case Vector2 down when direction == Vector2.down:
                return FROM_TOP_IDLE_ANIMATION;
            case Vector2 left when direction == Vector2.left:
                return FROM_RIGHT_IDLE_ANIMATION;
            case Vector2 right when direction == Vector2.right:
                return FROM_LEFT_IDLE_ANIMATION;
        }
        return null;
    }

    private string ChooseOpenAnimation()
    {
        switch (direction)
        {
            case Vector2 up when direction == Vector2.up:
                return FROM_BOTTOM_OPEN_ANIMATION;
            case Vector2 down when direction == Vector2.down:
                return FROM_TOP_OPEN_ANIMATION;
            case Vector2 left when direction == Vector2.left:
                return FROM_RIGHT_OPEN_ANIMATION;
            case Vector2 right when direction == Vector2.right:
                return FROM_LEFT_OPEN_ANIMATION;
        }
        return null;
    }

    private string ChooseCloseAnimation()
    {
        switch (direction)
        {
            case Vector2 up when direction == Vector2.up:
                return FROM_BOTTOM_CLOSE_ANIMATION;
            case Vector2 down when direction == Vector2.down:
                return FROM_TOP_CLOSE_ANIMATION;
            case Vector2 left when direction == Vector2.left:
                return FROM_RIGHT_CLOSE_ANIMATION;
            case Vector2 right when direction == Vector2.right:
                return FROM_LEFT_CLOSE_ANIMATION;
        }
        return null;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Events;


public class BossController : MonoBehaviour
{
    StateMachine stateMachine;
    public Animator animator;

    public float fadeDuration = 1f;
    [SerializeField] private Image InkEffectImage;
    [SerializeField] public GameObject HealPotionPrefab;
    [SerializeField] public GameObject InkPrefab;
    [SerializeField] public GameObject SwordPrefab;
    [SerializeField] public GameObject PencilAttackPrefab;
    [SerializeField] public GameObject WarningTilePrefab;
    [SerializeField] public GameObject SlimePrefab;
    private GameObject grid;

    public GameObject playerObject;

    public static readonly Vector3Int originCell = new Vector3Int(-4, -4, 0);
    public static readonly int size = 7;
    private Tween inkEffectTween;
    public SpriteBlink spriteBlink;

    void OnEnable()
    {
        Ink.OnInkEffectTriggered += TriggerInkEffect;
    }

    void OnDisable()
    {
        Ink.OnInkEffectTriggered -= TriggerInkEffect;
        stateMachine.Reset();
    }

    void Awake()
    {
        stateMachine = new StateMachine();
        // Initialize states and transitions here if needed

        animator = GetComponent<Animator>();

        // Modify attack commands here
        List<BossCommand> phase1Commands = new List<BossCommand>
        {
            new RandomFourDirectionAttack(this),
            new ThreeByThreeAttack(this),
            new LongPlusAttack(this),
        };
        List<BossCommand> phase2Commands = new List<BossCommand>
        {
            new SnakeChaseAttack(this),
            new ShortCheckerAttack(this),
            new Phase2PencilAttack(this),
        };
        List<BossCommand> phase2CombinedCommands = new List<BossCommand>
        {
            new SnakeChaseAttackWithPencil(this),
        };

        List<BossCommand> phase3Commands = new List<BossCommand>
        {
            new FastDownPencilAttack(this),
        };

        var phase1 = new BossPhase(this, animator, phase1Commands, 5, new DoNothing(this), null, new Vector3Int(-1, -10, 0));
        var phase2 = new BossPhase(this, animator, phase2Commands, 6, new RaisePhase2Walls(this), null, new Vector3Int(-2, -7, 0), phase2CombinedCommands);
        var phase3 = new BossPhase(this, animator, phase3Commands, 5, new RaisePhase3Walls(this));
        var hurtState = new BossHurtState(this, animator);
        var winState = new BossWinState(this, animator);
        var loseState = new BossLoseState(this, animator);

        At(phase1, phase2, new FuncPredicate(() => phase1.IsPhaseEnded()));
        At(phase2, phase3, new FuncPredicate(() => phase2.IsPhaseEnded()));
        At(phase3, winState, new FuncPredicate(() => phase3.IsTheEnd()));
        playerObject = GameObject.Find("Player");
        grid = GameObject.Find("Grid");

        stateMachine.SetState(phase1);
    }

    void Start()
    {
        InkEffectImage.gameObject.SetActive(false);
        spriteBlink = GetComponent<SpriteBlink>();
        AudioManager.Instance.PlayBGM("boss_bgm");
    }

    void Update()
    {
        stateMachine.Update();
    }

    void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }


    public void TriggerInkEffect()
    {
        Debug.Log("Boss triggered ink effect!");
        inkEffectTween?.Kill();
        InkEffectImage.gameObject.SetActive(true);
        InkEffectImage.color = new Color(InkEffectImage.color.r, InkEffectImage.color.g, InkEffectImage.color.b, 1f);
        inkEffectTween = InkEffectImage.DOFade(0f, fadeDuration);
        inkEffectTween.OnComplete(() => InkEffectImage.gameObject.SetActive(false));
    }

    public void TriggerPencilAttack(float lockDuration, float speed, Vector3Int direction, Vector3Int initialCellPosition)
    {
        //Debug.Log("Boss triggered pencil attack!");
        GameObject pencil = Instantiate(PencilAttackPrefab);
        pencil.transform.SetParent(grid.transform, false);
        PencilAttack pencilAttack = pencil.GetComponent<PencilAttack>();
        pencilAttack.Initialise(lockDuration, speed, direction, initialCellPosition);
    }

    public void TriggerRaisingWall(Vector3Int cellPosition)
    {
        //Debug.Log("Boss triggered raising wall!");
        GridManager.Instance.RaiseWall(cellPosition);
    }

    public void TriggerLoweringWall(Vector3Int cellPosition)
    {
        //Debug.Log("Boss triggered lowering wall!");
        GridManager.Instance.LowerWall(cellPosition);
    }

    public void TriggerLowerAllWalls()
    {
        for (int i = originCell.x; i < originCell.x + size; i++)
        {
            for (int j = originCell.y; j > originCell.y - size; j--)
            {
                GridManager.Instance.LowerWall(new Vector3Int(i, j, 0));
            }
        }
    }

    public GameObject TriggerCreateTile(Vector3Int cellPosition, GameObject tilePrefab)
    {
        return GridManager.Instance.CreateSpecialTile(cellPosition, tilePrefab);
    }

    public void TriggerRemoveTile(Vector3Int cellPosition)
    {
        GridManager.Instance.RemoveSpecialTile(cellPosition);
    }

    public void TriggerInvoke(string methodName, float delay)
    {
        Invoke(methodName, delay);
    }

    public void TriggerCreateRandomItem(GameObject itemPrefab)
    {
        // Get random walkable cell position within the grid bounds
        Vector3Int? cellPosition = RandomWalkableCell();
        if (cellPosition.HasValue)
        {
            TriggerCreateTile(cellPosition.Value, itemPrefab);
        }
    }

    public Vector3Int? RandomWalkableCell()
    {
        // Gather all walkable cells within the grid bounds
        List<Vector3Int> walkableCells = new List<Vector3Int>();
        for (int i = originCell.x; i < originCell.x + size; i++)
        {
            for (int j = originCell.y; j > originCell.y - size; j--)
            {
                Vector3Int cellPos = new Vector3Int(i, j, 0);
                if (GridManager.Instance.IsWalkable(cellPos) && !GridManager.Instance.IsItem(cellPos) && GridManager.Instance.WorldToCell(playerObject.transform.position) != cellPos)
                {
                    walkableCells.Add(cellPos);
                }
            }
        }
        return walkableCells.Count > 0 ? (Vector3Int?)walkableCells[Random.Range(0, walkableCells.Count)] : null;
    }

    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

}

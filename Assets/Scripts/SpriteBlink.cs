using UnityEngine;

public class SpriteBlink : MonoBehaviour
{
    [SerializeField] private float blinkDecaySpeed = 5f; // Speed at which the blink effect decays

    private SpriteRenderer[] _spriteRenderers;
    private MaterialPropertyBlock _propertyBlock;
    private float _blinkFactor;

    [SerializeField] private float blinkLoopInterval = 0.15f; // time between pulses while duration-blinking
    private bool _isDurationBlinking;
    private float _durationTimer;
    private float _loopTimer;

    public bool IsBlinking => _blinkFactor > 0f || _isDurationBlinking;

    // Event function
    private void Start()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _propertyBlock = new MaterialPropertyBlock();
        _propertyBlock.SetColor("_BlinkColor", Color.red);
    }

    private void Update()
    {
        if (_isDurationBlinking)
        {
            _durationTimer -= Time.deltaTime;
            if (_durationTimer <= 0f)
            {
                _isDurationBlinking = false;
            }
            else
            {
                _loopTimer -= Time.deltaTime;
                if (_loopTimer <= 0f)
                {
                    _blinkFactor = 1f;
                    _loopTimer = blinkLoopInterval;
                }
            }
        }

        if (_blinkFactor <= 0f)
        {
            return;
        }

        _blinkFactor = Mathf.Lerp(_blinkFactor, 0f, Time.deltaTime * blinkDecaySpeed);
        if (_blinkFactor < 0.01f && !_isDurationBlinking)
        {
            _blinkFactor = 0f;
        }
        ApplyBlinkFactor();
    }
    public void Blink()
    {
        _blinkFactor = 1f;
    }

    public void Blink(float duration)
    {
        _isDurationBlinking = true;
        _durationTimer = duration;
        _loopTimer = 0f; // trigger first pulse immediately on next Update
    }

    private void ApplyBlinkFactor()
    {
        foreach (var renderer in _spriteRenderers)
        {
            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat("_BlinkFactor", _blinkFactor);
            renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}

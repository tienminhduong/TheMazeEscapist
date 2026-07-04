using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class RainbowTilemapColor : MonoBehaviour
{
    [SerializeField] private Tilemap[] targetTilemaps;
    [SerializeField] private bool searchChildTilemapsIfNeeded = true;
    [SerializeField, FormerlySerializedAs("cycleSpeed"), Min(0f)] private float pulseSpeed = 0.8f;
    [SerializeField] private Color pulseTargetColor = Color.white;
    [SerializeField] private bool restoreOriginalColorsOnDisable = true;

    private readonly List<TilemapState> tilemapStates = new();

    private void Reset()
    {
        targetTilemaps = FindTargetTilemaps();
    }

    private void Awake()
    {
        if (targetTilemaps == null || targetTilemaps.Length == 0)
            targetTilemaps = FindTargetTilemaps();
    }

    private void OnEnable()
    {
        CacheTilemaps();
        ApplyPulseColors();
    }

    private void OnDisable()
    {
        if (restoreOriginalColorsOnDisable)
            RestoreOriginalColors();
    }

    private void Update()
    {
        ApplyPulseColors();
    }

    private Tilemap[] FindTargetTilemaps()
    {
        var localTilemaps = GetComponents<Tilemap>();
        if (localTilemaps.Length > 0 || !searchChildTilemapsIfNeeded)
            return localTilemaps;

        return GetComponentsInChildren<Tilemap>();
    }

    private void CacheTilemaps()
    {
        tilemapStates.Clear();

        if (targetTilemaps == null || targetTilemaps.Length == 0)
            targetTilemaps = FindTargetTilemaps();

        foreach (var tilemap in targetTilemaps)
        {
            if (tilemap == null)
                continue;

            tilemapStates.Add(new TilemapState(tilemap));
        }
    }

    private void ApplyPulseColors()
    {
        if (tilemapStates.Count == 0)
            CacheTilemaps();

        var pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        pulse = pulse * pulse * (3f - 2f * pulse);

        foreach (var state in tilemapStates)
        {
            if (state.Tilemap == null)
                continue;

            var color = GetPulsedColor(state.OriginalTilemapColor, pulse);
            color.a = state.OriginalTilemapColor.a;
            state.Tilemap.color = color;
        }
    }

    private Color GetPulsedColor(Color baseColor, float pulse)
    {
        var targetColor = pulseTargetColor;
        targetColor.a = baseColor.a;

        return Color.Lerp(baseColor, targetColor, pulse);
    }

    private void RestoreOriginalColors()
    {
        foreach (var state in tilemapStates)
        {
            if (state.Tilemap == null)
                continue;

            state.Tilemap.color = state.OriginalTilemapColor;
        }
    }

    private sealed class TilemapState
    {
        public TilemapState(Tilemap tilemap)
        {
            Tilemap = tilemap;
            OriginalTilemapColor = tilemap.color;
        }

        public Tilemap Tilemap { get; }
        public Color OriginalTilemapColor { get; }
    }
}

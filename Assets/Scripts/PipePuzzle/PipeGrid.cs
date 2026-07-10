using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeGrid : MonoBehaviour
{
    [Header("Layout")]
    public int width = 4;
    public int height = 4;
    public float cellSize = 1f;

    [Header("Tiles")]
    public TileDefinition[] tiles;
    public GameObject tilePrefab;

    [Header("Special Events")]
    [Tooltip("Chance per second that a random rotatable tile becomes blocked")]
    public float blockChancePerSecond = 0.15f;
    [Tooltip("Minimum rotate presses needed to unblock a blocked tile")]
    public int blockBreakHitsMin = 3;
    [Tooltip("Maximum rotate presses needed to unblock a blocked tile")]
    public int blockBreakHitsMax = 8;
    [Tooltip("Chance (0–1) a connected pipe spins by itself when player is hit")]
    [Range(0f, 1f)] public float spinChanceOnHit = 0.5f;
    [Tooltip("Chance (0–1) the whole grid rotates 90 degrees when player is hit")]
    [Range(0f, 1f)] public float gridRotateChanceOnHit = 0.3f;

    // Fires with every system currently reached by a complete pipe path from the source
    public event Action<SystemType[]> OnPoweredSystemsChanged;

    // Fires with the new highlighted grid position whenever the highlight moves
    public event Action<Vector2Int> OnHighlightChanged;

    // Row 0 = top of grid. Row increases downward (south).
    // N=0 E=1 S=2 W=3
    static readonly Vector2Int[] DirOffsets =
    {
        new Vector2Int(0, -1), // N
        new Vector2Int(1,  0), // E
        new Vector2Int(0,  1), // S
        new Vector2Int(-1, 0), // W
    };

    PipeTile[] _runtimeTiles;
    Vector2Int _highlight;
    HashSet<Vector2Int> _connectedTiles = new();
    bool _gridRotating;
    int _gridRotationSteps;

    void Start()
    {
        EnsureTileArraySize();
        BuildRuntime();
        GetTile(_highlight)?.SetHighlighted(true);
        Solve();
    }

    void Update()
    {
        // Randomly block a tile over time
        if (UnityEngine.Random.value < blockChancePerSecond * Time.deltaTime)
            TryBlockRandomTile();
    }

    void BuildRuntime()
    {
        _runtimeTiles = new PipeTile[width * height];

        RectTransform rt = GetComponent<RectTransform>();
        float cellW, cellH;
        Vector2 origin;

        if (rt != null)
        {
            cellW = rt.rect.width / width;
            cellH = rt.rect.height / height;
            origin = new Vector2(-rt.rect.width / 2f + cellW / 2f,
                                  rt.rect.height / 2f - cellH / 2f);
        }
        else
        {
            cellW = cellSize;
            cellH = cellSize;
            origin = Vector2.zero;
        }

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int idx = Idx(col, row);
                GameObject go = tilePrefab != null
                    ? Instantiate(tilePrefab, transform)
                    : new GameObject($"Tile_{col}_{row}");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(origin.x + col * cellW,
                                                         origin.y - row * cellH, 0f);

                RectTransform tileRt = go.GetComponent<RectTransform>();
                if (tileRt != null)
                    tileRt.sizeDelta = new Vector2(cellW, cellH);

                PipeTile tile = go.GetComponent<PipeTile>() ?? go.AddComponent<PipeTile>();
                tile.Init(tiles[idx], this);
                _runtimeTiles[idx] = tile;
            }
        }
    }

    public void Solve()
    {
        _connectedTiles.Clear();
        var powered = new List<SystemType>();
        Vector2Int? src = FindSource();
        if (src == null)
        {
            OnPoweredSystemsChanged?.Invoke(powered.ToArray());
            return;
        }

        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        visited.Add(src.Value);
        queue.Enqueue(src.Value);

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();
            PipeTile tile = GetTile(pos);
            if (tile == null) continue;

            foreach (int dir in tile.GetOpenSides())
            {
                Vector2Int nPos = pos + DirOffsets[dir];
                if (!InBounds(nPos) || visited.Contains(nPos)) continue;

                PipeTile neighbor = GetTile(nPos);
                int opposite = (dir + 2) % 4;
                if (neighbor == null || !neighbor.HasOpenSide(opposite)) continue;

                visited.Add(nPos);

                if (neighbor.data.type == TileType.Endpoint && neighbor.data.endpointSystem != SystemType.None)
                    powered.Add(neighbor.data.endpointSystem);
                else
                    queue.Enqueue(nPos);
            }
        }

        _connectedTiles = new HashSet<Vector2Int>(visited);
        OnPoweredSystemsChanged?.Invoke(powered.ToArray());
    }

    // Call from your input handler with dx/dy from D-pad or left stick
    public void MoveHighlight(int dx, int dy)
    {
        // Counter-rotate input to match each 90° clockwise grid rotation
        for (int i = 0; i < _gridRotationSteps; i++)
        {
            int tmp = dx;
            dx = dy;
            dy = -tmp;
        }

        Vector2Int next = new Vector2Int(
            Mathf.Clamp(_highlight.x + dx, 0, width - 1),
            Mathf.Clamp(_highlight.y + dy, 0, height - 1));

        if (next == _highlight) return;
        GetTile(_highlight)?.SetHighlighted(false);
        _highlight = next;
        GetTile(_highlight)?.SetHighlighted(true);
        OnHighlightChanged?.Invoke(_highlight);
    }

    // Call from your input handler when the rotate button is pressed
    public void RotateHighlighted() => GetTile(_highlight)?.Rotate();

    public PipeTile GetHighlightedTile() => GetTile(_highlight);
    public Vector2Int GetHighlightPosition() => _highlight;

    // Call this from your player health/hit system whenever the player takes a hit
    public void OnPlayerHit(bool shieldActive = false)
    {
        if (UnityEngine.Random.value < gridRotateChanceOnHit)
            CoroutineRunner.Run(AnimateGridRotation());

        float chance = shieldActive ? 0.5f : spinChanceOnHit;
        if (UnityEngine.Random.value < chance)
            TrySpinRandomConnectedPipe();
    }

    void TryBlockRandomTile()
    {
        var candidates = new List<int>();
        for (int i = 0; i < _runtimeTiles.Length; i++)
        {
            PipeTile t = _runtimeTiles[i];
            if (t == null) continue;
            if (t.data.type == TileType.Dead ||
                t.data.type == TileType.Source ||
                t.data.type == TileType.Endpoint) continue;
            if (t.IsBlocked) continue;
            candidates.Add(i);
        }
        if (candidates.Count == 0) return;

        int chosen = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        int hits = UnityEngine.Random.Range(blockBreakHitsMin, blockBreakHitsMax + 1);
        _runtimeTiles[chosen].SetBlocked(hits);
    }

    void TrySpinRandomConnectedPipe()
    {
        var candidates = new List<Vector2Int>();
        foreach (Vector2Int pos in _connectedTiles)
        {
            PipeTile t = GetTile(pos);
            if (t == null) continue;
            if (t.data.type == TileType.Dead ||
                t.data.type == TileType.Source ||
                t.data.type == TileType.Endpoint) continue;
            if (t.IsBlocked) continue;
            candidates.Add(pos);
        }
        if (candidates.Count == 0) return;

        Vector2Int chosenPos = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        PipeTile chosen = GetTile(chosenPos);
        int targetRotation = (chosen.data.rotation + UnityEngine.Random.Range(1, 4)) % 4;
        chosen.ForceRotate(targetRotation);
        Solve();
    }

    IEnumerator AnimateGridRotation()
    {
        if (_gridRotating) yield break;
        _gridRotating = true;

        float startAngle = transform.localEulerAngles.z;
        float endAngle = startAngle - 90f;
        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(startAngle, endAngle, t));
            yield return null;
        }

        transform.localEulerAngles = new Vector3(0f, 0f, endAngle);
        _gridRotationSteps = (_gridRotationSteps + 1) % 4;
        _gridRotating = false;
    }

    Vector2Int? FindSource()
    {
        for (int row = 0; row < height; row++)
            for (int col = 0; col < width; col++)
                if (_runtimeTiles[Idx(col, row)]?.data.type == TileType.Source)
                    return new Vector2Int(col, row);
        return null;
    }

    PipeTile GetTile(Vector2Int pos) =>
        InBounds(pos) ? _runtimeTiles[Idx(pos.x, pos.y)] : null;

    bool InBounds(Vector2Int pos) =>
        pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;

    int Idx(int col, int row) => row * width + col;

    public void EnsureTileArraySize()
    {
        int needed = width * height;
        if (tiles != null && tiles.Length == needed) return;
        var next = new TileDefinition[needed];
        for (int i = 0; i < needed; i++)
            next[i] = (tiles != null && i < tiles.Length) ? tiles[i] : new TileDefinition();
        tiles = next;
    }
}

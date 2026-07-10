using UnityEngine;
using UnityEngine.UI;

public class PipeTile : MonoBehaviour
{
    // Directions: N=0, E=1, S=2, W=3
    // Base connections defined at rotation=0, rotated clockwise at runtime
    static readonly int[][] BaseConnections =
    {
        new int[] { },            // Dead      – no connections
        new[] { 0, 2 },          // Straight  – N, S
        new[] { 0, 1 },          // Elbow     – N, E
        new[] { 0, 1, 2 },       // T         – N, E, S
        new[] { 0, 1, 2, 3 },    // Cross     – all
        new[] { 0, 1, 2, 3 },    // Source    – all
        new[] { 2 },             // Endpoint  – S (rotate so inward side faces the grid)
    };

    // One sprite per tile type in TileType enum order:
    // 0=Dead, 1=Straight, 2=Elbow, 3=T, 4=Cross, 5=Source, 6=Endpoint
    [SerializeField] Sprite[] typeSprites;
    // One sprite per SystemType for endpoints, in SystemType enum order:
    // 0=None, 1=ShieldBuster, 2=Gun, 3=Shield, 4=Healing
    [SerializeField] Sprite[] endpointSystemSprites;
    [SerializeField] GameObject highlightOverlay;
    [SerializeField] GameObject blockedOverlay;

    public TileDefinition data;
    PipeGrid _grid;
    Image _image;

    bool _isBlocked;
    int _blockHitsRemaining;

    public bool IsBlocked => _isBlocked;

    public void Init(TileDefinition def, PipeGrid grid)
    {
        _image = GetComponent<Image>();
        highlightOverlay?.SetActive(false);
        blockedOverlay?.SetActive(false);
        data = new TileDefinition
        {
            type = def.type,
            rotation = def.rotation,
            endpointSystem = def.endpointSystem
        };
        _grid = grid;
        ApplySprite();
        ApplyRotation();
    }

    void ApplySprite()
    {
        if (_image == null) return;
        if (data.type == TileType.Endpoint && endpointSystemSprites != null)
        {
            int sysIdx = (int)data.endpointSystem;
            if (sysIdx < endpointSystemSprites.Length && endpointSystemSprites[sysIdx] != null)
            {
                _image.sprite = endpointSystemSprites[sysIdx];
                return;
            }
        }
        int idx = (int)data.type;
        if (typeSprites != null && idx < typeSprites.Length)
            _image.sprite = typeSprites[idx];
    }

    public bool HasOpenSide(int dir)
    {
        foreach (int d in BaseConnections[(int)data.type])
            if ((d + data.rotation) % 4 == dir) return true;
        return false;
    }

    public int[] GetOpenSides()
    {
        int[] base_ = BaseConnections[(int)data.type];
        int[] result = new int[base_.Length];
        for (int i = 0; i < base_.Length; i++)
            result[i] = (base_[i] + data.rotation) % 4;
        return result;
    }

    public void SetHighlighted(bool on) => highlightOverlay?.SetActive(on);

    public void SetBlocked(int hitsToBreak)
    {
        _isBlocked = true;
        _blockHitsRemaining = hitsToBreak;
        blockedOverlay?.SetActive(true);
    }

    public void Rotate()
    {
        if (data.type == TileType.Dead ||
            data.type == TileType.Source ||
            data.type == TileType.Endpoint) return;

        if (_isBlocked)
        {
            _blockHitsRemaining--;
            if (_blockHitsRemaining <= 0)
            {
                _isBlocked = false;
                blockedOverlay?.SetActive(false);
            }
            return;
        }

        data.rotation = (data.rotation + 1) % 4;
        ApplyRotation();
        _grid.Solve();
    }

    public void ForceRotate(int targetRotation)
    {
        data.rotation = targetRotation;
        ApplyRotation();
    }

    void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, -data.rotation * 90f);
    }
}

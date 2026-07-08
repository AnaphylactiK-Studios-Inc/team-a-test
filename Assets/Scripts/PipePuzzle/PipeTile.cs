using UnityEngine;

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

    public TileDefinition data;
    PipeGrid _grid;

    public void Init(TileDefinition def, PipeGrid grid)
    {
        data = new TileDefinition
        {
            type = def.type,
            rotation = def.rotation,
            endpointSystem = def.endpointSystem
        };
        _grid = grid;
        ApplyRotation();
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

    // Called by player input — Dead, Source, and Endpoint tiles cannot be rotated
    public void Rotate()
    {
        if (data.type == TileType.Dead ||
            data.type == TileType.Source ||
            data.type == TileType.Endpoint) return;

        data.rotation = (data.rotation + 1) % 4;
        ApplyRotation();
        _grid.Solve();
    }

    void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, data.rotation * 90f, 0f);
    }
}

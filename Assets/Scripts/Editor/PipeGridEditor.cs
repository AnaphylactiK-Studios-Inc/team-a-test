using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PipeGrid))]
public class PipeGridEditor : Editor
{
    static readonly string[] TileTypeLabels = { "Dead", "Straight", "Elbow", "T", "Cross", "Source", "Endpoint" };
    static readonly string[] RotLabels = { "0°", "90°", "180°", "270°" };
    static readonly string[] SystemLabels = { "None", "ShieldBuster", "Gun", "Shield", "Healing" };

    static readonly Color[] TileColors =
    {
        new Color(0.25f, 0.25f, 0.25f), // Dead
        new Color(0.35f, 0.55f, 1f),    // Straight
        new Color(0.35f, 0.75f, 0.35f), // Elbow
        new Color(0.85f, 0.6f, 0.15f),  // T
        new Color(0.75f, 0.35f, 0.85f), // Cross
        new Color(1f, 0.9f, 0.15f),     // Source
        new Color(1f, 0.35f, 0.35f),    // Endpoint
    };

    Vector2 _scrollPos;

    public override void OnInspectorGUI()
    {
        var grid = (PipeGrid)target;

        // ── Settings ─────────────────────────────────────────
        EditorGUI.BeginChangeCheck();
        int newW = Mathf.Max(1, EditorGUILayout.IntField("Width", grid.width));
        int newH = Mathf.Max(1, EditorGUILayout.IntField("Height", grid.height));
        float newCell = EditorGUILayout.FloatField("Cell Size", grid.cellSize);
        var newPrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", grid.tilePrefab, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(grid, "Edit Grid Settings");
            grid.width = newW;
            grid.height = newH;
            grid.cellSize = newCell;
            grid.tilePrefab = newPrefab;
            grid.EnsureTileArraySize();
            EditorUtility.SetDirty(grid);
        }

        if (GUILayout.Button("Reset All Tiles to Dead"))
        {
            Undo.RecordObject(grid, "Reset Grid");
            grid.tiles = new TileDefinition[grid.width * grid.height];
            for (int i = 0; i < grid.tiles.Length; i++)
                grid.tiles[i] = new TileDefinition();
            EditorUtility.SetDirty(grid);
        }

        // ── Grid ─────────────────────────────────────────────
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Puzzle Layout  (row 0 = top)", EditorStyles.boldLabel);

        grid.EnsureTileArraySize();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, alwaysShowHorizontal: true, alwaysShowVertical: false, GUILayout.Height(Mathf.Min(grid.height * 105 + 10, 500)));

        for (int row = 0; row < grid.height; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < grid.width; col++)
            {
                TileDefinition tile = grid.tiles[row * grid.width + col];

                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = TileColors[(int)tile.type];
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(108));
                GUI.backgroundColor = prev;

                EditorGUILayout.LabelField($"({col},{row})", EditorStyles.centeredGreyMiniLabel);

                // Type
                int newType = EditorGUILayout.Popup((int)tile.type, TileTypeLabels);
                if (newType != (int)tile.type)
                {
                    Undo.RecordObject(grid, "Change Tile Type");
                    tile.type = (TileType)newType;
                    EditorUtility.SetDirty(grid);
                }

                // Rotation — Dead and Source are fixed (Dead has no pipes, Source connects all sides)
                bool canRotate = tile.type != TileType.Dead && tile.type != TileType.Source;
                if (canRotate)
                {
                    int newRot = EditorGUILayout.Popup(tile.rotation, RotLabels);
                    if (newRot != tile.rotation)
                    {
                        Undo.RecordObject(grid, "Change Tile Rotation");
                        tile.rotation = newRot;
                        EditorUtility.SetDirty(grid);
                    }
                }

                // Endpoint system picker
                if (tile.type == TileType.Endpoint)
                {
                    int newSys = EditorGUILayout.Popup((int)tile.endpointSystem, SystemLabels);
                    if (newSys != (int)tile.endpointSystem)
                    {
                        Undo.RecordObject(grid, "Change Endpoint System");
                        tile.endpointSystem = (SystemType)newSys;
                        EditorUtility.SetDirty(grid);
                    }
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}

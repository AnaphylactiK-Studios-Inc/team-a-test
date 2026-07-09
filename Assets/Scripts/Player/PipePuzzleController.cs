using UnityEngine;
using UnityEngine.InputSystem;

public class PipePuzzleController : MonoBehaviour
{
    [Header("Grids")]
    [SerializeField] PipeGrid gridA;
    [SerializeField] PipeGrid gridB;

    [Header("Resource Selection Visuals")]
    [SerializeField] GameObject resourceAHighlight;
    [SerializeField] GameObject resourceBHighlight;

    [Header("Puzzle UI (shown when puzzle is open)")]
    [SerializeField] GameObject gridAUI;
    [SerializeField] GameObject gridBUI;

    [Header("References")]
    [SerializeField] MouseMove mouseMove;

    [SerializeField] float moveRepeatDelay = 0.15f;

    InputSystem_Actions _input;
    int  _selected   = 0;
    bool _puzzleOpen = false;
    float _nextMove;

    PipeGrid ActiveGrid => _selected == 0 ? gridA : gridB;
    GameObject ActiveUI  => _selected == 0 ? gridAUI : gridBUI;

    void Awake()
    {
        _input = new InputSystem_Actions();

        _input.Game.SelectA.performed  += _ => { if (!_puzzleOpen) SelectResource(0); };
        _input.Game.SelectB.performed  += _ => { if (!_puzzleOpen) SelectResource(1); };
        _input.Game.Confirm.performed  += _ => OnConfirm();
        _input.Game.Cancel.performed   += _ => OnCancel();
    }

    void OnEnable()  => _input.Enable();
    void OnDisable() => _input.Disable();

    void Start()
    {
        SelectResource(0);
        gridAUI?.SetActive(false);
        gridBUI?.SetActive(false);
    }

    void Update()
    {
        if (!_puzzleOpen || Time.unscaledTime < _nextMove) return;

        Vector2 nav = _input.Game.Navigate.ReadValue<Vector2>();
        if (nav.sqrMagnitude < 0.25f) return;

        int dx = nav.x > 0.5f ? 1 : nav.x < -0.5f ? -1 : 0;
        int dy = nav.y > 0.5f ? -1 : nav.y < -0.5f ? 1 : 0; // y flipped: stick up = row up = row index decreases
        if (dx == 0 && dy == 0) return;

        ActiveGrid.MoveHighlight(dx, dy);
        _nextMove = Time.unscaledTime + moveRepeatDelay;
    }

    void OnConfirm()
    {
        if (!_puzzleOpen) OpenPuzzle();
        else ActiveGrid.RotateHighlighted();
    }

    void OnCancel()
    {
        if (_puzzleOpen) ClosePuzzle();
    }

    void SelectResource(int index)
    {
        _selected = index;
        resourceAHighlight?.SetActive(index == 0);
        resourceBHighlight?.SetActive(index == 1);
    }

    void OpenPuzzle()
    {
        _puzzleOpen = true;
        if (mouseMove) mouseMove.reticleEnabled = false;
        ActiveUI?.SetActive(true);
    }

    void ClosePuzzle()
    {
        _puzzleOpen = false;
        if (mouseMove) mouseMove.reticleEnabled = true;
        gridAUI?.SetActive(false);
        gridBUI?.SetActive(false);
    }
}

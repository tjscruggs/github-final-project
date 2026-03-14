using System;
using System.Collections.Generic;
using TrebuchetDuel.Core;
using TrebuchetDuel.Rules;
using UnityEngine;
using UnityEngine.UI;

namespace TrebuchetDuel.Board
{
    /// <summary>
    /// Builds and highlights the board using simple UI tiles for mobile readability.
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("References")]
        public RectTransform boardRoot;
        public GameObject tilePrefab;

        [Header("Colors")]
        public Color normalA = new(0.18f, 0.22f, 0.30f);
        public Color normalB = new(0.22f, 0.28f, 0.38f);
        public Color highlightMove = new(0.66f, 0.82f, 0.44f);
        public Color highlightSelected = new(0.95f, 0.75f, 0.3f);
        public Color castleP1 = new(0.70f, 0.28f, 0.28f);
        public Color castleP2 = new(0.28f, 0.44f, 0.74f);

        private readonly Dictionary<Vector2Int, TileView> _tiles = new();

        public Action<Vector2Int> OnTilePressed;

        public void BuildBoard(GameState state)
        {
            foreach (Transform child in boardRoot) Destroy(child.gameObject);
            _tiles.Clear();

            GridLayoutGroup layout = boardRoot.GetComponent<GridLayoutGroup>();
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = GameState.BoardSize;
            layout.cellSize = new Vector2(130, 130);
            layout.spacing = new Vector2(8, 8);

            for (int y = GameState.BoardSize - 1; y >= 0; y--)
            {
                for (int x = 0; x < GameState.BoardSize; x++)
                {
                    var go = Instantiate(tilePrefab, boardRoot);
                    var tv = go.GetComponent<TileView>();
                    tv.Grid = new Vector2Int(x, y);
                    tv.Button.onClick.AddListener(() => OnTilePressed?.Invoke(tv.Grid));
                    _tiles[tv.Grid] = tv;
                }
            }

            PaintBase(state);
        }

        public void PaintBase(GameState state)
        {
            foreach (var kv in _tiles)
            {
                bool parity = (kv.Key.x + kv.Key.y) % 2 == 0;
                kv.Value.SetHighlight(parity ? normalA : normalB);
            }

            _tiles[state.CastleByPlayer[PlayerId.Player1]].SetHighlight(castleP1);
            _tiles[state.CastleByPlayer[PlayerId.Player2]].SetHighlight(castleP2);
        }

        public void HighlightState(GameState state, RulesEngine rules)
        {
            PaintBase(state);
            var selected = rules.GetSelected(state);
            if (selected == null) return;

            _tiles[selected.Grid].SetHighlight(highlightSelected);
            for (int x = 0; x < GameState.BoardSize; x++)
            for (int y = 0; y < GameState.BoardSize; y++)
            {
                var p = new Vector2Int(x, y);
                if (rules.CanMoveTo(state, selected, p)) _tiles[p].SetHighlight(highlightMove);
            }
        }
    }
}

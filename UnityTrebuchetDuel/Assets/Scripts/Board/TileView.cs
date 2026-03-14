using UnityEngine;
using UnityEngine.UI;

namespace TrebuchetDuel.Board
{
    public class TileView : MonoBehaviour
    {
        public Vector2Int Grid;
        public Image Background;
        public Button Button;

        public void SetHighlight(Color c) => Background.color = c;
    }
}

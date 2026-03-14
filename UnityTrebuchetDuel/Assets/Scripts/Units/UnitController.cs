using TrebuchetDuel.Core;
using UnityEngine;

namespace TrebuchetDuel.Units
{
    /// <summary>
    /// View/controller component for one engineer piece.
    /// </summary>
    public class UnitController : MonoBehaviour
    {
        public UnitState State;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Init(UnitState state, Color color)
        {
            State = state;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.color = color;
            name = state.Id;
        }

        public void SyncPosition(float tileSize, Vector2 origin)
        {
            transform.position = origin + new Vector2(State.Grid.x * tileSize, State.Grid.y * tileSize);
        }
    }
}

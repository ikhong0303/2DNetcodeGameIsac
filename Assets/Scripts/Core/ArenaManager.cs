using UnityEngine;
using TopDownShooter.Data;

namespace TopDownShooter.Core
{
    /// <summary>
    /// Manages the arena boundaries and visual representation.
    /// Creates walls and background at runtime.
    /// </summary>
    public class ArenaManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameSettings gameSettings;

        [Header("Visual")]
        [SerializeField] private Color wallColor = new Color(0.3f, 0.3f, 0.4f);
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        [SerializeField] private float wallThickness = 1f;

        private GameObject _background;
        private GameObject[] _walls;

        private void Start()
        {
            if (gameSettings != null)
            {
                CreateArena();
            }
        }

        /// <summary>
        /// Initializes the arena with the given settings.
        /// </summary>
        public void Initialize(GameSettings settings)
        {
            gameSettings = settings;
            CreateArena();
        }

        private void CreateArena()
        {
            float boundsX = gameSettings.ArenaBoundsX;
            float boundsY = gameSettings.ArenaBoundsY;

            // Create background
            CreateBackground(boundsX, boundsY);

            // Create walls
            CreateWalls(boundsX, boundsY);
        }

        private void CreateBackground(float boundsX, float boundsY)
        {
            _background = new GameObject("Background");
            _background.transform.SetParent(transform);
            _background.transform.position = Vector3.zero;

            var sr = _background.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = backgroundColor;
            sr.sortingOrder = -100;

            _background.transform.localScale = new Vector3(boundsX * 2, boundsY * 2, 1);
        }

        private void CreateWalls(float boundsX, float boundsY)
        {
            _walls = new GameObject[4];

            // Top wall
            _walls[0] = CreateWall("Wall_Top",
                new Vector3(0, boundsY + wallThickness / 2, 0),
                new Vector3(boundsX * 2 + wallThickness * 2, wallThickness, 1));

            // Bottom wall
            _walls[1] = CreateWall("Wall_Bottom",
                new Vector3(0, -boundsY - wallThickness / 2, 0),
                new Vector3(boundsX * 2 + wallThickness * 2, wallThickness, 1));

            // Left wall
            _walls[2] = CreateWall("Wall_Left",
                new Vector3(-boundsX - wallThickness / 2, 0, 0),
                new Vector3(wallThickness, boundsY * 2, 1));

            // Right wall
            _walls[3] = CreateWall("Wall_Right",
                new Vector3(boundsX + wallThickness / 2, 0, 0),
                new Vector3(wallThickness, boundsY * 2, 1));
        }

        private GameObject CreateWall(string name, Vector3 position, Vector3 scale)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(transform);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.tag = "Wall";
            wall.layer = LayerMask.NameToLayer("Default");

            // Add visual
            var sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = wallColor;
            sr.sortingOrder = -50;

            // Add collider
            var collider = wall.AddComponent<BoxCollider2D>();

            return wall;
        }

        private Sprite CreateSquareSprite()
        {
            int size = 4;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }

            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (gameSettings == null) return;

            Gizmos.color = new Color(wallColor.r, wallColor.g, wallColor.b, 0.3f);

            float boundsX = gameSettings.ArenaBoundsX;
            float boundsY = gameSettings.ArenaBoundsY;

            // Draw walls
            Gizmos.DrawCube(new Vector3(0, boundsY + wallThickness / 2, 0),
                new Vector3(boundsX * 2 + wallThickness * 2, wallThickness, 1));
            Gizmos.DrawCube(new Vector3(0, -boundsY - wallThickness / 2, 0),
                new Vector3(boundsX * 2 + wallThickness * 2, wallThickness, 1));
            Gizmos.DrawCube(new Vector3(-boundsX - wallThickness / 2, 0, 0),
                new Vector3(wallThickness, boundsY * 2, 1));
            Gizmos.DrawCube(new Vector3(boundsX + wallThickness / 2, 0, 0),
                new Vector3(wallThickness, boundsY * 2, 1));
        }
#endif
    }
}

using UnityEngine;

namespace DinoGame.Environment
{
    /// <summary>
    /// Альтернативный подход для Dino Chrome:
    /// Персонаж стоит на месте, а фон и земля перемещаются влево с заданной скоростью.
    /// Также скрипт плавно меняет фон (День -> Закат -> Ночь).
    /// </summary>
    public class BackgroundScroller : MonoBehaviour
    {
        [Header("Скорость перемещения фона")]
        [Tooltip("Базовая скорость движения мира влево")]
        [SerializeField] private float scrollSpeed = 5f;

        [Tooltip("Ускорение мира со временем (увеличение сложности)")]
        [SerializeField] private float speedIncreaseRate = 0.05f;

        [Header("Меняющиеся фоны (PascalCase)")]
        [SerializeField] private SpriteRenderer skyRenderer;
        [SerializeField] private Sprite backgroundDay;
        [SerializeField] private Sprite backgroundDusk;
        [SerializeField] private Sprite backgroundNight;

        [Tooltip("Интервал смены фона в секундах")]
        [SerializeField] private float timeBetweenBackgroundChange = 20f;

        [Header("Бесшовное повторение (Tiling)")]
        [SerializeField] private Transform groundTransform1;
        [SerializeField] private Transform groundTransform2;
        [SerializeField] private float groundTileWidth = 10f;

        private float _currentSpeed;
        private float _bgTimer;
        private int _currentBgIndex;

        public float CurrentSpeed => _currentSpeed;

        private void Start()
        {
            _currentSpeed = scrollSpeed;
            if (skyRenderer != null && backgroundDay != null)
            {
                skyRenderer.sprite = backgroundDay;
            }
        }

        private void Update()
        {
            // Постепенное увеличение скорости
            _currentSpeed += speedIncreaseRate * Time.deltaTime;

            // Перемещение тайлов земли влево
            MoveGroundTile(groundTransform1);
            MoveGroundTile(groundTransform2);

            // Смена фона со временем
            UpdateBackgroundCycle();
        }

        private void MoveGroundTile(Transform ground)
        {
            if (ground == null) return;

            ground.position += Vector3.left * (_currentSpeed * Time.deltaTime);

            // Если тайл ушел влево за пределы экрана — переставляем его вправо
            if (ground.position.x <= -groundTileWidth)
            {
                ground.position += Vector3.right * (groundTileWidth * 2f);
            }
        }

        private void UpdateBackgroundCycle()
        {
            if (skyRenderer == null) return;

            _bgTimer += Time.deltaTime;
            if (_bgTimer >= timeBetweenBackgroundChange)
            {
                _bgTimer = 0f;
                _currentBgIndex = (_currentBgIndex + 1) % 3;

                switch (_currentBgIndex)
                {
                    case 0:
                        if (backgroundDay != null) skyRenderer.sprite = backgroundDay;
                        break;
                    case 1:
                        if (backgroundDusk != null) skyRenderer.sprite = backgroundDusk;
                        break;
                    case 2:
                        if (backgroundNight != null) skyRenderer.sprite = backgroundNight;
                        break;
                }
            }
        }
    }
}

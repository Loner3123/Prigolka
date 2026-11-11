using UnityEngine;

namespace DinoGame.Player
{
    /// <summary>
    /// Режимы горизонтального перемещения персонажа:
    /// 1. ManualControl — управление стрелками влево/вправо (A/D).
    /// 2. AutoRunner — персонаж автоматически бежит вправо (классический раннер).
    /// 3. StationaryRunner — персонаж зафиксирован по X (классический Chrome Dino),
    ///    а движение создается перемещением мира/фона.
    /// </summary>
    public enum HorizontalMovementMode
    {
        ManualControl,
        AutoRunner,
        StationaryRunner
    }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Настройки перемещения")]
        [Tooltip("Режим горизонтального перемещения")]
        [SerializeField] private HorizontalMovementMode movementMode = HorizontalMovementMode.ManualControl;

        [Tooltip("Скорость горизонтального перемещения (влево/вправо)")]
        [SerializeField] private float moveSpeed = 8f;

        [Header("Настройки прыжка")]
        [Tooltip("Сила импульса прыжка")]
        [SerializeField] private float jumpForce = 14f;

        [Tooltip("Точка проверки касания земли")]
        [SerializeField] private Transform groundCheckPoint;

        [Tooltip("Радиус проверки земли")]
        [SerializeField] private float groundCheckRadius = 0.2f;

        [Tooltip("Слой, считающийся землей")]
        [SerializeField] private LayerMask groundLayer;

        [Header("Визуал и спрайты (PascalCase)")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite spriteIdle;
        [SerializeField] private Sprite spriteRun1;
        [SerializeField] private Sprite spriteRun2;
        [SerializeField] private Sprite spriteJump;
        [SerializeField] private Sprite spriteDuck;

        [Header("Анимация бега")]
        [Tooltip("Частота смены кадров бега (кадров в секунду)")]
        [SerializeField] private float runAnimationFps = 8f;

        private Rigidbody2D _rb;
        private BoxCollider2D _collider;
        private Vector2 _originalColliderSize;
        private Vector2 _originalColliderOffset;

        private float _horizontalInput;
        private bool _jumpRequested;
        private bool _isDucking;
        private bool _isGrounded;
        private float _animationTimer;
        private int _runFrameIndex;

        public bool IsGrounded => _isGrounded;
        public bool IsDucking => _isDucking;
        public HorizontalMovementMode MovementMode => movementMode;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            _originalColliderSize = _collider.size;
            _originalColliderOffset = _collider.offset;
        }

        private void Update()
        {
            // 1. Считывание ввода перемещения в сторону (влево/вправо)
            if (movementMode == HorizontalMovementMode.ManualControl)
            {
                _horizontalInput = Input.GetAxisRaw("Horizontal");
            }
            else if (movementMode == HorizontalMovementMode.AutoRunner)
            {
                _horizontalInput = 1f; // Бег только вправо
            }
            else
            {
                _horizontalInput = 0f; // Фиксированная позиция
            }

            // 2. Считывание прыжка (Space, стрелка вверх или W)
            if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && _isGrounded && !_isDucking)
            {
                _jumpRequested = true;
            }

            // 3. Считывание приседания (стрелка вниз или S)
            _isDucking = (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && _isGrounded;

            // 4. Корректировка коллайдера при приседании
            UpdateColliderForDuck();

            // 5. Обновление спрайтов персонажа
            UpdateSpriteAnimation();
        }

        private void FixedUpdate()
        {
            // Проверка касания земли
            CheckGroundedStatus();

            // Перемещение по горизонтали
            MoveHorizontally();

            // Выполнение прыжка
            if (_jumpRequested)
            {
                PerformJump();
                _jumpRequested = false;
            }
        }

        /// <summary>
        /// Осуществляет перемещение персонажа в сторону
        /// </summary>
        private void MoveHorizontally()
        {
            float targetVelocityX = _horizontalInput * moveSpeed;
            _rb.velocity = new Vector2(targetVelocityX, _rb.velocity.y);

            // Поворот спрайта в сторону движения (если ручное управление)
            if (movementMode == HorizontalMovementMode.ManualControl && Mathf.Abs(_horizontalInput) > 0.01f)
            {
                transform.localScale = new Vector3(Mathf.Sign(_horizontalInput), 1f, 1f);
            }
        }

        /// <summary>
        /// Выполняет прыжок через физику Rigidbody2D
        /// </summary>
        private void PerformJump()
        {
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
        }

        /// <summary>
        /// Проверяет, стоит ли персонаж на земле
        /// </summary>
        private void CheckGroundedStatus()
        {
            Vector2 checkPosition = groundCheckPoint != null
                ? (Vector2)groundCheckPoint.position
                : (Vector2)transform.position + Vector2.down * (_collider.size.y * 0.5f);

            _isGrounded = Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer);
        }

        /// <summary>
        /// Сжимает коллайдер при приседании (как в Chrome Dino для пролета под птеродактилями)
        /// </summary>
        private void UpdateColliderForDuck()
        {
            if (_isDucking)
            {
                _collider.size = new Vector2(_originalColliderSize.x, _originalColliderSize.y * 0.5f);
                _collider.offset = new Vector2(_originalColliderOffset.x, _originalColliderOffset.y - _originalColliderSize.y * 0.25f);
            }
            else
            {
                _collider.size = _originalColliderSize;
                _collider.offset = _originalColliderOffset;
            }
        }

        /// <summary>
        /// Переключает кадры спрайта в зависимости от состояния
        /// </summary>
        private void UpdateSpriteAnimation()
        {
            if (spriteRenderer == null) return;

            if (!_isGrounded)
            {
                if (spriteJump != null) spriteRenderer.sprite = spriteJump;
            }
            else if (_isDucking)
            {
                if (spriteDuck != null) spriteRenderer.sprite = spriteDuck;
            }
            else if (Mathf.Abs(_rb.velocity.x) > 0.1f || movementMode == HorizontalMovementMode.StationaryRunner)
            {
                // Анимация бега: переключение Run1 и Run2
                _animationTimer += Time.deltaTime;
                if (_animationTimer >= 1f / runAnimationFps)
                {
                    _animationTimer = 0f;
                    _runFrameIndex = (_runFrameIndex + 1) % 2;
                }

                spriteRenderer.sprite = (_runFrameIndex == 0 && spriteRun1 != null) ? spriteRun1 : spriteRun2;
            }
            else
            {
                if (spriteIdle != null) spriteRenderer.sprite = spriteIdle;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector2 checkPosition = groundCheckPoint != null
                ? (Vector2)groundCheckPoint.position
                : (Vector2)transform.position + Vector2.down * 0.5f;
            Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
        }
    }
}

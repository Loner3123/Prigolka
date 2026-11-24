using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки прыжка")]
    // Сила прыжка
    public float jumpForce = 12f;

    // Компонент 2D физики
    private Rigidbody2D rb;

    // Стоит ли игрок на земле (чтобы не прыгать бесконечно в воздухе)
    private bool isGrounded = false;

    [Header("Анимация бега (Dark Fantasy)")]
    // Спрайты для анимации бега и прыжка
    public Sprite runSprite1;
    public Sprite runSprite2;
    public Sprite jumpSprite;

    private SpriteRenderer spriteRenderer;
    private float animationTimer;
    public float animationSpeed = 0.12f; // Скорость переключения кадров
    private bool isFirstFrame = true;

    void Awake()
    {
        // Назначаем стандартный тег игрока
        gameObject.tag = "Player";

        // Находим или автоматически добавляем физику Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Замораживаем вращение, чтобы рыцарь не падал на спину при приземлении
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Находим или автоматически добавляем точный честный коллайдер
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null && GetComponent<Collider2D>() == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.42f, 1.05f);
            col.offset = new Vector2(-0.04f, -0.02f);
            col.edgeRadius = 0.02f;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // --- 1. КНОПКА ПРЫЖКА ---
        // Прыгает по кнопке Пробел, стрелке вверх, W или клику мышки / тапу
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space) 
                        || Input.GetKeyDown(KeyCode.UpArrow) 
                        || Input.GetKeyDown(KeyCode.W) 
                        || Input.GetMouseButtonDown(0);

        if (jumpPressed && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
        }

        // --- 2. АНИМАЦИЯ БЕГА (САМ БЕЖИТ) ---
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (spriteRenderer == null) return;

        if (!isGrounded)
        {
            // В воздухе показываем спрайт прыжка
            if (jumpSprite != null) spriteRenderer.sprite = jumpSprite;
        }
        else
        {
            // На земле циклично переключаем два кадра бега
            animationTimer += Time.deltaTime;
            if (animationTimer >= animationSpeed)
            {
                animationTimer = 0f;
                isFirstFrame = !isFirstFrame;

                if (isFirstFrame && runSprite1 != null)
                {
                    spriteRenderer.sprite = runSprite1;
                }
                else if (!isFirstFrame && runSprite2 != null)
                {
                    spriteRenderer.sprite = runSprite2;
                }
            }
        }
    }

    // --- 3. СТОЛКНОВЕНИЕ С ПРЕПЯТСТВИЕМ -> РЕСЕТ ИГРЫ ---
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, врезался ли игрок в препятствие
        if (IsObstacle(collision.gameObject))
        {
            ResetGame();
            return;
        }

        // Если коснулись земли (любой другой твердой поверхности под ногами)
        isGrounded = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsObstacle(other.gameObject))
        {
            ResetGame();
        }
    }

    // Проверка, является ли объект препятствием
    private bool IsObstacle(GameObject obj)
    {
        if (obj == null) return false;

        string name = obj.name.ToLower();
        return obj.GetComponent<Obstacle>() != null 
            || obj.CompareTag("Obstacle") 
            || name.Contains("obstacle")
            || name.Contains("spike")
            || name.Contains("cactus")
            || name.Contains("tombstone")
            || name.Contains("gargoyle")
            || name.Contains("bat");
    }

    // ПЕРЕЗАПУСК ИГРЫ
    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Управление игроком (Black Souls Hunter) БЕЗ использования Rigidbody.
/// Реализует равноускоренное движение (прыжок и гравитацию) вручную через метод Update.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Прыжок без использования Rigidbody")]
    [Tooltip("Начальная вертикальная скорость импульса при прыжке вверх (м/с)")]
    public float jumpVelocity = 14f;

    [Tooltip("Ускорение свободного падения, тянущее персонажа вниз (м/с^2)")]
    public float gravity = 32f;

    [Tooltip("Координата Y уровня земли, на которой стоит персонаж")]
    public float groundY = -2.25f;

    // Текущая вертикальная скорость персонажа
    private float verticalSpeed = 0f;

    // Находится ли персонаж на земле
    private bool isGrounded = true;

    [Header("Анимация бега (Black Souls Hunter)")]
    public Sprite runSprite1;
    public Sprite runSprite2;
    public Sprite jumpSprite;

    private SpriteRenderer spriteRenderer;
    private float animationTimer;
    public float animationSpeed = 0.11f;
    private bool isFirstFrame = true;

    [Header("Честный хитбокс столкновения (без Rigidbody)")]
    // Хитбокс охватывает только тело, исключая меч и развевающийся плащ
    public Vector2 hitboxSize = new Vector2(0.42f, 1.05f);
    public Vector2 hitboxOffset = new Vector2(-0.04f, -0.02f);

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Проверяем: по заданию Rigidbody использоваться НЕ ДОЛЖЕН.
        // Если он случайно остался на объекте — удаляем его.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Destroy(rb);
        }

        // Фиксируем персонажа точно на уровне земли
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
    }

    /// <summary>
    /// Метод Update() вызывается движком Unity автоматически каждый кадр.
    /// Частота вызова зависит от FPS (например, 60 кадров/сек = 60 вызовов в секунду).
    /// Используется для считывания ввода, расчетного перемещения и обновления таймеров.
    /// Time.deltaTime — время в секундах, прошедшее с момента предыдущего кадра.
    /// </summary>
    void Update()
    {
        // -------------------------------------------------------------
        // 1. СЧИТЫВАНИЕ КНОПКИ ПРЫЖКА
        // -------------------------------------------------------------
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space) 
                        || Input.GetKeyDown(KeyCode.UpArrow) 
                        || Input.GetKeyDown(KeyCode.W) 
                        || Input.GetMouseButtonDown(0);

        // Если нажали прыжок и персонаж на земле — даем импульс скорости вверх
        if (jumpPressed && isGrounded)
        {
            verticalSpeed = jumpVelocity;
            isGrounded = false;
        }

        // -------------------------------------------------------------
        // 2. РАСЧЕТ ПРЫЖКА И ГРАВИТАЦИИ БЕЗ RIGIDBODY
        // Формулы классической кинематики:
        // v = v0 - g * dt  (скорость уменьшается под действием гравитации)
        // y = y0 + v * dt  (координата меняется в соответствии со скоростью)
        // -------------------------------------------------------------
        if (!isGrounded)
        {
            // Уменьшаем вертикальную скорость под действием гравитации
            verticalSpeed -= gravity * Time.deltaTime;

            // Перемещаем персонажа по оси Y
            transform.position += Vector3.up * (verticalSpeed * Time.deltaTime);

            // Проверяем приземление: если опустились ниже уровня земли
            if (transform.position.y <= groundY)
            {
                // Возвращаем строго на уровень земли и сбрасываем скорость
                transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
                verticalSpeed = 0f;
                isGrounded = true;
            }
        }

        // -------------------------------------------------------------
        // 3. АНИМАЦИЯ БЕГА И ПРЫЖКА
        // -------------------------------------------------------------
        UpdateAnimation();

        // -------------------------------------------------------------
        // 4. ПРОВЕРКА СТОЛКНОВЕНИЯ С ПРЕПЯТСТВИЯМИ (ХИТБОКС)
        // Проверяем пересечение хитбокса прямо в Update без зависимости от Rigidbody
        // -------------------------------------------------------------
        CheckCollisions();
    }

    void UpdateAnimation()
    {
        if (spriteRenderer == null) return;

        if (!isGrounded)
        {
            // В воздухе показываем позу прыжка с мечом
            if (jumpSprite != null) spriteRenderer.sprite = jumpSprite;
        }
        else
        {
            // На земле циклично переключаем кадры бега
            animationTimer += Time.deltaTime;
            if (animationTimer >= animationSpeed)
            {
                animationTimer = 0f;
                isFirstFrame = !isFirstFrame;
                spriteRenderer.sprite = isFirstFrame ? runSprite1 : runSprite2;
            }
        }
    }

    // Проверка столкновений через Physics2D.OverlapBox (работает без Rigidbody)
    void CheckCollisions()
    {
        Vector2 center = (Vector2)transform.position + hitboxOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, hitboxSize, 0f);

        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && IsObstacle(hit.gameObject))
            {
                ResetGame();
                break;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsObstacle(other.gameObject))
        {
            ResetGame();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsObstacle(collision.gameObject))
        {
            ResetGame();
        }
    }

    private bool IsObstacle(GameObject obj)
    {
        if (obj == null) return false;
        string name = obj.name.ToLower();
        return obj.GetComponent<Obstacle>() != null 
            || obj.CompareTag("Obstacle") 
            || name.Contains("obstacle")
            || name.Contains("spike")
            || name.Contains("tombstone")
            || name.Contains("gargoyle")
            || name.Contains("bat")
            || name.Contains("crow");
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Отображение точного хитбокса в редакторе Unity (зеленая рамка)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 center = (Vector2)transform.position + hitboxOffset;
        Gizmos.DrawWireCube(center, hitboxSize);
    }
}

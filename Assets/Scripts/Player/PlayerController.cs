using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки движения")]
    // Скорость движения персонажа влево и вправо
    public float speed = 5f;

    // Сила прыжка вверх
    public float jumpForce = 10f;

    // Компонент физики 2D
    private Rigidbody2D rb;

    // Стоит ли персонаж на земле (чтобы не прыгать бесконечно в воздухе)
    private bool isGrounded = false;

    void Start()
    {
        // Получаем компонент Rigidbody2D с этого же объекта
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // --- 1. ПЕРЕМЕЩЕНИЕ В СТОРОНУ ---
        // Input.GetAxis("Horizontal") считывает кнопки A/D или стрелки влево/вправо:
        // A / Стрелка влево = -1
        // D / Стрелка вправо = +1
        // Ничего не нажато = 0
        float moveX = Input.GetAxis("Horizontal");

        // Применяем скорость по оси X (влево/вправо), а скорость по Y (падение/прыжок) оставляем как есть
        rb.velocity = new Vector2(moveX * speed, rb.velocity.y);


        // --- 2. ПРЫЖОК ---
        // Если нажали Пробел И персонаж сейчас на земле
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // Толкаем персонажа вверх с силой jumpForce
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false; // Оторвался от земли
        }
    }

    // Срабатывает в момент касания с любым коллайдером (землей)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Персонаж коснулся поверхности — значит, снова можно прыгать
        isGrounded = true;
    }
}

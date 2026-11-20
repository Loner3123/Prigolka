using UnityEngine;
using UnityEngine.SceneManagement;

public class Obstacle : MonoBehaviour
{
    // Скорость движения препятствия навстречу игроку (влево)
    public float speed = 7f;

    // Граница слева, за которой препятствие удаляется из памяти
    public float leftDestroyX = -15f;

    void Awake()
    {
        // Автоматически добавляем коллайдер, если его забыли повесить
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    void Update()
    {
        // Двигаем препятствие влево каждый кадр
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Если уехало далеко за экран слева — удаляем объект
        if (transform.position.x < leftDestroyX)
        {
            Destroy(gameObject);
        }
    }

    // Если персонаж врезался в препятствие -> ресет игры
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null || collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null || other.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

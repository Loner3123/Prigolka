using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    // Скорость движения влево
    public float speed = 4f;

    // Точка слева, при достижении которой фон возвращается обратно направо
    public float leftLimit = -15f;
    public float rightRespawnX = 15f;

    void Update()
    {
        // Сдвигаем объект влево каждый кадр
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Если объект уехал за левый край экрана — переносим его обратно вправо
        if (transform.position.x < leftLimit)
        {
            transform.position = new Vector3(rightRespawnX, transform.position.y, transform.position.z);
        }
    }
}

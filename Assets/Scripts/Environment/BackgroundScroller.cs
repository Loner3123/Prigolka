using UnityEngine;

/// <summary>
/// Скрипт бесконечного перемещения фона / тайлов земли.
/// Создает иллюзию бега персонажа за счет непрерывного движения окружения влево в методе Update().
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Tooltip("Скорость перемещения фона влево (метров в секунду)")]
    public float speed = 4f;

    [Tooltip("Крайняя левая координата X, при достижении которой объект телепортируется вправо")]
    public float leftLimit = -15f;

    [Tooltip("Координата X справа, куда возвращается объект для бесшовного зацикливания")]
    public float rightRespawnX = 15f;

    /// <summary>
    /// Метод Update() вызывается каждый кадр перед рендерингом.
    /// Перемещение рассчитывается как: speed * Time.deltaTime.
    /// Time.deltaTime гарантирует одинаковую плавную скорость на любых мониторах и при любом FPS.
    /// </summary>
    void Update()
    {
        // 1. Сдвигаем объект влево на расстояние (speed * Time.deltaTime)
        transform.position += Vector3.left * (speed * Time.deltaTime);

        // 2. Если объект уехал за левый край экрана — переносим его обратно направо
        if (transform.position.x < leftLimit)
        {
            transform.position = new Vector3(rightRespawnX, transform.position.y, transform.position.z);
        }
    }
}

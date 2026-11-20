using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Текущий счёт (время выживания в секундах)
    public static float score;

    void Start()
    {
        score = 0f;
    }

    void Update()
    {
        // Увеличиваем счёт каждую секунду выживания
        score += Time.deltaTime * 10f;
    }

    // Перезагрузка всей игры
    public static void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

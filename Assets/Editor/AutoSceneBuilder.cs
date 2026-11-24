#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class AutoSceneBuilder
{
    [MenuItem("Tools/🎮 Собрать сцену (Сразу играть в Unity)")]
    public static void BuildSceneOnly()
    {
        Debug.Log("--- Автоматическая сборка сцены Dark Fantasy Dino Runner ---");

        // 1. Создаем чистую сцену
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. Настраиваем Главную Камеру
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        camObj.transform.position = new Vector3(0, 0, -10);
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.05f, 0.12f, 1f); // Тёмный готический фон
        camObj.AddComponent<AudioListener>();

        // 3. Загружаем все спрайты
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ArtAssets/Backgrounds/DarkFantasyCastle.png");
        Sprite groundSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ArtAssets/Backgrounds/CursedGround.png");

        Sprite knightRun1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ArtAssets/Player/DarkKnightRun1.png");
        Sprite knightRun2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ArtAssets/Player/DarkKnightRun2.png");
        Sprite knightJump = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ArtAssets/Player/DarkKnightJump.png");

        Sprite spikesSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ArtAssets/Obstacles/Ground/CursedSpikes.png");
        Sprite tombstoneSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ArtAssets/Obstacles/Ground/GothicTombstone.png");
        Sprite gargoyleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ArtAssets/Obstacles/Flying/ShadowGargoyleFly1.png");
        Sprite batSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ArtAssets/Obstacles/Flying/BloodBatFly.png");

        // 4. Задний фон (Замок и Кровавая луна)
        GameObject bgObj = new GameObject("DarkFantasyCastle_Background");
        bgObj.transform.position = new Vector3(0, 0.8f, 5f);
        bgObj.transform.localScale = new Vector3(3.6f, 3.6f, 1f);
        SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;

        // Плавный параллакс заднего плана
        BackgroundScroller bgScroller = bgObj.AddComponent<BackgroundScroller>();
        bgScroller.speed = 1.0f;
        bgScroller.leftLimit = -18f;
        bgScroller.rightRespawnX = 18f;

        // 5. Физическая земля (Твердый коллайдер под ногами)
        GameObject groundColliderObj = new GameObject("Ground_Collider");
        groundColliderObj.tag = "Ground";
        groundColliderObj.transform.position = new Vector3(0, -3.8f, 0);
        BoxCollider2D groundBox = groundColliderObj.AddComponent<BoxCollider2D>();
        groundBox.size = new Vector2(50f, 1f);

        // Визуальные бегущие тайлы земли (CursedGround)
        for (int i = 0; i < 3; i++)
        {
            GameObject groundTile = new GameObject("GroundTile_" + i);
            groundTile.transform.position = new Vector3(-9f + (i * 9f), -3.3f, 0);
            groundTile.transform.localScale = new Vector3(3.6f, 3.6f, 1f);
            SpriteRenderer gSr = groundTile.AddComponent<SpriteRenderer>();
            gSr.sprite = groundSprite;
            gSr.sortingOrder = 1;

            BackgroundScroller gScroller = groundTile.AddComponent<BackgroundScroller>();
            gScroller.speed = 7f; // совпадает со скоростью препятствий
            gScroller.leftLimit = -13.5f;
            gScroller.rightRespawnX = 13.5f;
        }

        // 6. Игрок (Тёмный рыцарь / Hunter в стиле Black Souls)
        GameObject playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerObj.transform.position = new Vector3(-5.5f, -2.25f, 0);
        playerObj.transform.localScale = new Vector3(2.0f, 2.0f, 1f);

        SpriteRenderer playerSr = playerObj.AddComponent<SpriteRenderer>();
        playerSr.sprite = knightRun1;
        playerSr.sortingOrder = 10;

        // ПРЫЖОК БЕЗ RIGIDBODY: Rigidbody2D не добавляется!
        // ТОЧНЫЙ ХИТБОКС ИГРОКА: закрывает только тело и ноги, не выступая за меч и плащ
        BoxCollider2D playerCol = playerObj.AddComponent<BoxCollider2D>();
        playerCol.size = new Vector2(0.42f, 1.05f);
        playerCol.offset = new Vector2(-0.04f, -0.02f);
        playerCol.edgeRadius = 0.02f;

        PlayerController playerCtrl = playerObj.AddComponent<PlayerController>();
        playerCtrl.jumpVelocity = 14f;
        playerCtrl.gravity = 32f;
        playerCtrl.groundY = -2.25f;
        playerCtrl.hitboxSize = new Vector2(0.42f, 1.05f);
        playerCtrl.hitboxOffset = new Vector2(-0.04f, -0.02f);
        playerCtrl.runSprite1 = knightRun1;
        playerCtrl.runSprite2 = knightRun2;
        playerCtrl.jumpSprite = knightJump;
        playerCtrl.animationSpeed = 0.11f;

        // 7. Создаем префабы препятствий в папке Assets/Prefabs с ЧЕСТНЫМИ ХИТБОКСАМИ
        string prefabsPath = "Assets/Prefabs";
        if (!Directory.Exists(prefabsPath))
        {
            Directory.CreateDirectory(prefabsPath);
            AssetDatabase.Refresh();
        }

        GameObject[] createdPrefabs = new GameObject[4];

        // 7.1. CursedSpikes Prefab (Гильотинные шипы)
        GameObject spikesObj = new GameObject("CursedSpikes");
        spikesObj.transform.localScale = new Vector3(2.0f, 2.0f, 1f);
        SpriteRenderer spikesSr = spikesObj.AddComponent<SpriteRenderer>();
        spikesSr.sprite = spikesSprite;
        spikesSr.sortingOrder = 5;
        // Хитбокс только на тело шипов, воздух по краям не убивает
        BoxCollider2D spikesCol = spikesObj.AddComponent<BoxCollider2D>();
        spikesCol.size = new Vector2(0.45f, 0.65f);
        spikesCol.offset = new Vector2(0f, -0.15f);
        Obstacle spikesObs = spikesObj.AddComponent<Obstacle>();
        spikesObs.speed = 7f;
        createdPrefabs[0] = PrefabUtility.SaveAsPrefabAsset(spikesObj, prefabsPath + "/CursedSpikes.prefab");
        Object.DestroyImmediate(spikesObj);

        // 7.2. GothicTombstone Prefab (Надгробие безумия)
        GameObject tombObj = new GameObject("GothicTombstone");
        tombObj.transform.localScale = new Vector3(2.0f, 2.0f, 1f);
        SpriteRenderer tombSr = tombObj.AddComponent<SpriteRenderer>();
        tombSr.sprite = tombstoneSprite;
        tombSr.sortingOrder = 5;
        BoxCollider2D tombCol = tombObj.AddComponent<BoxCollider2D>();
        tombCol.size = new Vector2(0.45f, 0.70f);
        tombCol.offset = new Vector2(0f, -0.10f);
        Obstacle tombObs = tombObj.AddComponent<Obstacle>();
        tombObs.speed = 7f;
        createdPrefabs[1] = PrefabUtility.SaveAsPrefabAsset(tombObj, prefabsPath + "/GothicTombstone.prefab");
        Object.DestroyImmediate(tombObj);

        // 7.3. ShadowGargoyle Prefab (Чумной ворон)
        GameObject gargObj = new GameObject("ShadowGargoyle");
        gargObj.transform.localScale = new Vector3(2.0f, 2.0f, 1f);
        SpriteRenderer gargSr = gargObj.AddComponent<SpriteRenderer>();
        gargSr.sprite = gargoyleSprite;
        gargSr.sortingOrder = 5;
        // Хитбокс только на туловище птицы, кончики перьев не цепляют
        BoxCollider2D gargCol = gargObj.AddComponent<BoxCollider2D>();
        gargCol.size = new Vector2(0.50f, 0.40f);
        gargCol.offset = new Vector2(-0.05f, 0f);
        Obstacle gargObs = gargObj.AddComponent<Obstacle>();
        gargObs.speed = 7.5f;
        createdPrefabs[2] = PrefabUtility.SaveAsPrefabAsset(gargObj, prefabsPath + "/ShadowGargoyle.prefab");
        Object.DestroyImmediate(gargObj);

        // 7.4. BloodBat Prefab (Призрак банши)
        GameObject batObj = new GameObject("BloodBat");
        batObj.transform.localScale = new Vector3(2.0f, 2.0f, 1f);
        SpriteRenderer batSr = batObj.AddComponent<SpriteRenderer>();
        batSr.sprite = batSprite;
        batSr.sortingOrder = 5;
        BoxCollider2D batCol = batObj.AddComponent<BoxCollider2D>();
        batCol.size = new Vector2(0.45f, 0.40f);
        batCol.offset = new Vector2(0f, 0f);
        Obstacle batObs = batObj.AddComponent<Obstacle>();
        batObs.speed = 8.0f;
        createdPrefabs[3] = PrefabUtility.SaveAsPrefabAsset(batObj, prefabsPath + "/BloodBat.prefab");
        Object.DestroyImmediate(batObj);

        // 8. Спавнер препятствий
        GameObject spawnerObj = new GameObject("ObstacleSpawner");
        spawnerObj.transform.position = new Vector3(12f, 0f, 0f);
        ObstacleSpawner spawner = spawnerObj.AddComponent<ObstacleSpawner>();
        spawner.obstaclePrefabs = createdPrefabs;
        spawner.spawnX = 12f;
        spawner.groundSpawnY = -2.3f;
        spawner.minFlyingY = -1.4f;
        spawner.maxFlyingY = -0.5f;
        spawner.minSpawnTime = 1.6f;
        spawner.maxSpawnTime = 2.8f;

        // 9. GameManager
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // 10. Сохраняем готовую сцену
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/SampleScene.unity");
        Debug.Log("--- Сцена успешно сохранена в Assets/Scenes/SampleScene.unity! ---");

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true)
        };
        
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Готово!", "Сцена игры успешно собрана!\nНажмите кнопку Play ▶️ вверху экрана, чтобы начать игру.", "Понял");
        }
    }

    [MenuItem("Tools/📦 Собрать готовую игру в .EXE")]
    public static void BuildWindowsExecutable()
    {
        BuildSceneOnly();

        string buildDir = "Z:/unity/Build";
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
        }

        string exePath = buildDir + "/DarkFantasyDino.exe";
        Debug.Log("--- Начинается сборка Windows EXE: " + exePath + " ---");

        BuildPlayerOptions buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = new string[] { "Assets/Scenes/SampleScene.unity" };
        buildOptions.locationPathName = exePath;
        buildOptions.target = BuildTarget.StandaloneWindows64;
        buildOptions.options = BuildOptions.None;

        var report = BuildPipeline.BuildPlayer(buildOptions);
        Debug.Log("--- Сборка EXE завершена с результатом: " + report.summary.result + " ---");

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Сборка готова!", "Игра успешно собрана в EXE!\nФайл находится в Z:/unity/Build/DarkFantasyDino.exe", "Круто");
            }
        }
    }
}
#endif

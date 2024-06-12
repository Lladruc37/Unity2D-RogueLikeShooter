using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneNow { MainMenu, Game }

[RequireComponent(typeof(LoadManager))]
[RequireComponent(typeof(FragsManager))]
public class GameManager : MonoBehaviour
{
	public static GameManager instance = null;
	public OverallUI overallUI;

	public int roomsAmountStart = 8;
	public int roomsAmountMenu = 5;

	public delegate void endGame();
	public event endGame OnEndGame;
	public void RaiseEndGame()
	{
		OnEndGame();
	}

	public GeneratorManager generator;
	[HideInInspector]
	public AudioManager audioManager;
	[HideInInspector]
	public LoadManager loadManager;
	[HideInInspector]
	public FragsManager fragmentsManager;

	public SceneNow sceneNow;
	public int randomSeed;

	public Text loadingText;
	public Text infoText;


	private float _gameTime;
	public float gameTime
	{
		get
		{
			return _gameTime;
		}
		set
		{
			_gameTime = value;
			Time.timeScale = value;
		}
	}

	public bool Loading
	{
		get
		{
			return _loading;
		}
		set
		{
			if (value == true)
			{
				gameTime = 0;
				_loading = true;
			}
			else
			{
				gameTime = 1;
				_loading = false;
			}
		}
	}
	private bool _loading;

	public bool generateLevel;
	public bool dontFadeScreen;

	public GameObject tutorial;

	public int mapTypeIdNow;

	public int level;
	public int score;

	public void SaveGame()
	{
		PlayerData saveData = new PlayerData();
		saveData.score = score;
		saveData.mapTypeIdNow = mapTypeIdNow;
		GameData.SaveData(saveData);
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			Destroy(gameObject);
		}

		OnEndGame += overallUI.EndGame;
		audioManager = GetComponentInChildren<AudioManager>();

		ItemManager.Initialize();
		EnemiesManager.Initialize();
		ExplosionManager.Initialize();

		fragmentsManager = GetComponent<FragsManager>();
		loadManager = GetComponent<LoadManager>();

		level = 1;
		Scene scene = SceneManager.GetActiveScene();
		if (scene.name == "Main Menu")
		{
			sceneNow = SceneNow.MainMenu;
		}
		else
		{
			sceneNow = SceneNow.Game;
		}

		GameData.LoadData();
		mapTypeIdNow = GameData.data.mapTypeIdNow;
		score = GameData.data.score;
		Map.tiles = generator.mapTypes[mapTypeIdNow].tileSet;
	}

	public void SetGenerationSeedRand()
	{
		instance.randomSeed = UnityEngine.Random.Range(0, 999999999);
	}

	public void SetGenerationSeed(int seed)
	{
		if (seed <= 0)
		{
			SetGenerationSeedRand();
		}
		else
		{
			instance.randomSeed = seed;
		}
	}

	private void Start()
	{
		GenerateLevel();
	}

	public void NextLevel()
	{
		loadManager.LoadGame();
	}

	public void GenerateLevel()
	{
		StopAll();
		if (randomSeed <= 0)
		{
			SetGenerationSeedRand();
		}
		StartCoroutine(CreateLevel());
	}

	private IEnumerator CreateLevel()
	{
		if (!Loading)
		{
			// Update map according to the plugin
			// Careful: It calls itself!
			Loading = true;
			SetMapType();
			yield return 0;
		}

		Loading = true;
		if (generateLevel)
		{
			if (sceneNow == SceneNow.MainMenu)
			{
				audioManager.PlayMusic(MusicType.main);
				if (!dontFadeScreen)
					StartCoroutine(Camera.main.GetComponent<Camera_Bounds>().FadeScreen(true, 0));

				loadingText.text = "";
				loadingText.enabled = false;
				infoText.text = "";
				infoText.enabled = false;

				generator.RoomsNow = roomsAmountMenu;
				yield return StartCoroutine(generator.GenerateLevel(false, null));

				// Call move to re-initialize the camera for the new map
				StartCoroutine(Camera.main.GetComponent<Camera_Bounds>().Move(4));
				StartCoroutine(Camera.main.GetComponent<Camera_Bounds>().FadeScreen(false, 3));
				Camera.main.GetComponent<Camera_Bounds>().movement = CameraMovement.MoveAround;
			}
			else if (sceneNow == SceneNow.Game)
			{
				GameData.LoadData();

				if (!dontFadeScreen)
					StartCoroutine(Camera.main.GetComponent<Camera_Bounds>().FadeScreen(true, 0));

				loadingText.enabled = true;
				infoText.enabled = true;

				audioManager.PlayMusic(MusicType.ambient);
				yield return StartCoroutine(generator.GenerateLevel(true, infoText));

				Camera.main.GetComponent<Camera_Bounds>().movement = CameraMovement.ChasePlayer;
				StartCoroutine(Camera.main.GetComponent<Camera_Bounds>().FadeScreen(false, 5));
			}
		}
		Loading = false;

		yield return 0;
	}

	private void SetMapType()
	{
		var coef = 0.25f * PluginController.Instance.GetFeatureCoefficient("CHOICES")
			+ 0.4f * PluginController.Instance.GetFeatureCoefficient("LV&PROG")
			+ 0.35f * PluginController.Instance.GetFeatureCoefficient("WRLDBLD");
		var amount = MathF.Ceiling(coef);
		Debug.Log($"set map type: {amount}");
		amount = 1;
		overallUI.mainMenuUI.ChangeMapType((int)amount);
	}

	public void InstatiatePlayer(Placement place)
	{
		Vector2 spawnPoint = place.GetCenterPosition();

		if (level == 1)
		{
			GameObject t = Instantiate(tutorial, spawnPoint, Quaternion.identity);
			t.transform.parent = place.transform;
		}

		Player.instance.transform.position = spawnPoint;
		Camera.main.transform.position = Player.instance.transform.position;
		place.TurnOnLight();
	}

	public void TeleportPlayer(Placement place)
	{
		if (Player.instance != null)
		{
			Vector2 spawnPoint = place.GetCenterPosition();
			TeleportPlayer(spawnPoint);
		}
	}

	public void TeleportPlayer(Vector2 pos)
	{
		Player.instance.transform.position = pos;
		Camera.main.transform.position = pos;
	}

	public void StopAll()
	{
		instance.generator.map_Generator.StopAllCoroutines();
		instance.generator.StopAllCoroutines();
		StopAllCoroutines();
	}
}

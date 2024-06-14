using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
	public void NextLevel()
	{
		GameManager.instance.level++;
		GameManager.instance.SetGenerationSeedRand();
		if (GameManager.instance.generator.RoomsNow < GameManager.instance.generator.maxRooms)
		{
			if (!PluginController.Instance.pluginEnabled)
			{
				GameManager.instance.generator.RoomsNow = (int)MathF.Min(GameManager.instance.generator.RoomsNow + MathF.Max(0, MathF.Ceiling(UnityEngine.Random.Range(-GameManager.instance.generator.roomIncreaseRange, GameManager.instance.generator.roomIncreaseRange))), GameManager.instance.generator.maxRooms);
				GameManager.instance.NextLevel();
				return;
			}

			var coef = 0.05f * PluginController.Instance.GetFeatureCoefficient("CHOICES")
				+ 0.15f * PluginController.Instance.GetFeatureCoefficient("CHLGS")
				+ 0.4f * PluginController.Instance.GetFeatureCoefficient("EXPLR")
				+ 0.35f * PluginController.Instance.GetFeatureCoefficient("LV&PROG")
				+ 0.05f * PluginController.Instance.GetFeatureCoefficient("WRLDBLD");
			var amount = MathF.Ceiling(GameManager.instance.generator.roomIncreaseRange * coef);
			amount = MathF.Max(0, amount);
			Debug.Log($"room count increase: {amount} and total {GameManager.instance.generator.RoomsNow + amount}");
			GameManager.instance.generator.RoomsNow = (int)MathF.Min(GameManager.instance.generator.RoomsNow + amount, GameManager.instance.generator.maxRooms);
		}
		Debug.Log("ITEM PHASE: " + ItemManager.instance.phaseNow);
		GameManager.instance.NextLevel();
	}

	public void LoadMainMenu()
	{
		GameManager.instance.sceneNow = SceneNow.MainMenu;
		Camera.main.GetComponent<Camera_Bounds>().fadeImage.color = new Color(0, 0, 0, 1f);

		DestroyPlayScene();
		StartCoroutine(LoadScene("Main Menu"));
	}

	public void RestartGame()
	{
		DestroyPlayScene();
		StartCoroutine(LoadScene("Play_Scene"));
	}
	public void LoadGame()
	{
		StartCoroutine(LoadScene("Play_Scene"));
	}

	private IEnumerator LoadScene(string sceneName)
	{
		GameManager.instance.StopAll();

		AsyncOperation wait = SceneManager.LoadSceneAsync(sceneName);
		yield return wait;

		if (sceneName == "Play_Scene")
		{
			GameManager.instance.sceneNow = SceneNow.Game;
		}
		else
		{
			GameManager.instance.sceneNow = SceneNow.MainMenu;
			GameManager.instance.SetGenerationSeed(0);
		}
		GameManager.instance.GenerateLevel();
	}

	private void DestroyPlayScene()
	{
		if (Player.instance != null)
		{
			GameObject.Destroy(Player.instance.gameObject);
		}
	}
}
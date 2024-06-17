using UnityEngine;

public class OverallUI : MonoBehaviour
{
	public Canvas settingsUI;

	public MainMenu mainMenuUI;
	public Pause_Menu pauseUI;
	public EndGame_UI endGameUI;
	public Survey_UI Survey_UI;

	void Start()
	{
		TurnOffMenu();
		if (GameManager.instance.sceneNow != SceneNow.Game)
		{
			TurnOnMenu();
		}
	}

	private void Update()
	{
		if (GameManager.instance.Loading != true)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (Player.instance != null)
				{
					if (Player.instance.mapCamera != null)
					{
						if (Player.instance.mapCamera.bigMap) // Open MiniMap
						{
							Player.instance.mapCamera.TurnOffBigMap(true);
							return;
						}
					}
				}
				if (settingsUI.enabled == false)
				{
					if (Player.instance != null)
					{
						if (Player.instance.isAlive)
						{
							pauseUI.PauseOrResume();
						}
					}
				}
				else
				{
					TurnOnMenu();
				}
			}
			if (Input.GetKeyDown(KeyCode.M))
			{
				if (Player.instance != null)
				{
					if (Player.instance.mapCamera != null)
					{
						if (Player.instance.mapCamera.bigMap)
						{
							Player.instance.mapCamera.TurnOffBigMap(true);
							return;
						}
						if (settingsUI.enabled == false && pauseUI.PauseCanvas.enabled == false)    // Open BigMap
						{
							Player.instance.mapCamera.TurnOnBigMap(null);
						}
					}
				}
			}
		}
	}

	public void GoToSettingsMenu()
	{
		TurnOffMenu();
		settingsUI.enabled = true;
	}

	public void TurnOffMenu()
	{
		mainMenuUI.Show(false);
		pauseUI.Show(false);
		Survey_UI.gameObject.SetActive(false);
	}

	public void TurnOnMenu()
	{
		switch (GameManager.instance.sceneNow)
		{
			case SceneNow.Game:
				{
					settingsUI.enabled = false;
					mainMenuUI.Show(false);
					pauseUI.Show(true);
					pauseUI.Pause();
					break;
				}
			case SceneNow.MainMenu:
				{
					settingsUI.enabled = false;
					mainMenuUI.Show(true);
					break;
				}
		}
	}

	public void NewGame()
	{
		mainMenuUI.field.text = "";
		mainMenuUI.gameSettings.SetActive(false);
		TurnOffMenu();
		GameManager.instance.generator.RoomsNow = GameManager.instance.roomsAmountStart;
		GameManager.instance.loadManager.LoadGame();
	}

	public void GoToMainMenu()
	{
		TurnOffMenu();
		GameManager.instance.loadManager.LoadMainMenu();
		TurnOnMenu();
	}

	public void EndGame()
	{
		endGameUI.Show(SurveyController.Instance.takingSurvey);
	}

	public void StartSurveyMenuOption()
	{
		GameManager.instance.SetGenerationSeedRand();
		SurveyController.Instance.SetupRun(1);
		NewGame();
	}

	public void OpenSurvey()
	{
		endGameUI.Hide();
		Survey_UI.gameObject.SetActive(true);
		Survey_UI.OpenSurvey();
	}
}

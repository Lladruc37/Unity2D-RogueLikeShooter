using UnityEngine;
using UnityEngine.UI;

public class EndGame_UI : MonoBehaviour
{
	public Image background;
	public Canvas canvas;
	public Text enemiesDefeated;
	public Text roomsCount;
	public Text shotsFired;
	public Text timePlayed;

	public Button restartButton;
	public Button mainMenuButton;
	public Button exitButton;
	public Button takeSurveyButton;

	public void Show(bool takingSurvey = false)
	{
		restartButton.gameObject.SetActive(!takingSurvey);
		mainMenuButton.gameObject.SetActive(!takingSurvey);
		exitButton.gameObject.SetActive(!takingSurvey);
		takeSurveyButton.gameObject.SetActive(takingSurvey);

		enemiesDefeated.text = "Enemies Defeated: " + Player.instance.gameStatistic.EnemiesDefeatedCount;
		roomsCount.text = "Rooms: " + Player.instance.gameStatistic.RoomsCount;
		shotsFired.text = "Shots Fired: " + Player.instance.gameStatistic.ShotsBeenFired;

		int minutes = 0;
		float overalltime = Player.instance.gameStatistic.timePlayed;
		while (overalltime > 59.9)
		{
			overalltime -= 60;
			minutes += 1;
		}
		float seconds = overalltime;

		timePlayed.text = "Play Time: " + minutes + ":" + System.Math.Round(seconds, 2);
		background.color = new Color(0, 0, 0, 0.5f);
		canvas.enabled = true;
	}

	public void MainMenu()
	{
		background.color = new Color(0, 0, 0, 0);
		canvas.enabled = false;
		GameManager.instance.overallUI.GoToMainMenu();
	}

	public void Restart()
	{
		background.color = new Color(0, 0, 0, 0);
		canvas.enabled = false;
		GameManager.instance.loadManager.RestartGame();
	}

	public void Quit()
	{
		background.color = new Color(0, 0, 0, 0);
		canvas.enabled = false;
		GameManager.instance.overallUI.mainMenuUI.Quit();
	}
}

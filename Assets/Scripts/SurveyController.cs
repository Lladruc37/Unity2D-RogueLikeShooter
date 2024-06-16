using UnityEngine;

public class SurveyController : MonoBehaviour
{
	public int RunNumber { get; private set; }
	public bool profileQuestion = false;
	public bool takingSurvey = false;
	public char testGroup;
	public SurveyData surveyData;

	public static SurveyController Instance { get; private set; }
	public static bool Initialized = false;

	private void Awake()
	{
		Initialized = true;
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	public void SetupRun(int number)
	{
		takingSurvey = true;
		RunNumber = number;
		switch (RunNumber)
		{
			case 1: //base game
				profileQuestion = true;
				PluginController.Instance.ResetProfile();
				//PluginController.Instance.pluginEnabled = false;
				surveyData = new SurveyData();
				var runData = new SurveyDataRun
				{
					initialProfile = new PlayerProfileData(PluginController.Instance.PlayerProfileData)
				};
				surveyData.runs.Add(runData);
				surveyData.testGroup = Random.Range(0, 2) switch
				{
					0 => 'A',
					1 => 'B',
					2 => 'C',
					_ => '-',
				};
				break;
			case 2: //using player's answers / no changes, base game
				profileQuestion = false;
				break;
			case 3: //different profile
				break;
			default:
				break;
		}
	}

	public void SetupNextRun()
	{
		RunNumber++;
		SetupRun(RunNumber);
	}
}

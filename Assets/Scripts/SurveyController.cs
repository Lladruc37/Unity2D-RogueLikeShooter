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
				PluginController.Instance.pluginEnabled = false;
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
			case 2: //A: no changes, base game / B: using player's answers / C: different profile
				profileQuestion = false;
				PluginController.Instance.pluginEnabled = true;
				switch (surveyData.testGroup)
				{
					case 'A':
						PluginController.Instance.ResetProfile();
						PluginController.Instance.pluginEnabled = false;
						break;
					case 'B':
						foreach (var type in surveyData.runs[0].question0.Profile)
							PluginController.Instance.PlayerProfileData.Profile[type.Key] = type.Value;

						PluginController.Instance.UpdateFeaturesCoefficient();
						break;
					case 'C':
						foreach (var type in surveyData.runs[0].finalProfile.Profile)
							PluginController.Instance.PlayerProfileData.Profile[type.Key] = (10 - type.Value);

						PluginController.Instance.UpdateFeaturesCoefficient();
						break;
					default:
						break;
				}
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

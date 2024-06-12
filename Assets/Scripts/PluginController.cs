using System;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteAlways]
public class PluginController : MonoBehaviour, IDataPersistence
{
	// singleton instance
	public static PluginController Instance { get; private set; }
	public static bool Initialized = false;

	// player profile data
	public PlayerProfileData PlayerProfileData;
	public float ProfileMinCellValue = 0.0f;
	public float ProfileMaxCellValue = 10.0f;

	// plugin data
	[SerializeField] private PluginData pluginData;

	//current gameplay feature coefficient
	public Dictionary<GameplayFeature, float> FeaturesCoefficient = new Dictionary<GameplayFeature, float>();

	// event
	public event Action OnFeaturesCoefficientUpdated;

	private void Awake()
	{
		Initialized = true;
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	// triggers
	public void OnRoomClear()
	{
		var changes = new Dictionary<PlayerTypes, float>
		{
			{ PlayerTypes.ACHIEVER, 0.4f },
			{ PlayerTypes.FREE_SPIRIT, 0.2f }
		};
		PlayerTypeTrigger(changes);
	}

	public void OnRoomClearNoDamage()
	{
		var changes = new Dictionary<PlayerTypes, float>
		{
			{ PlayerTypes.PLAYER, 0.5f },
			{ PlayerTypes.FREE_SPIRIT, -0.2f }
		};
		PlayerTypeTrigger(changes);
	}

	public void OnEnemyDeath()
	{
		var changes = new Dictionary<PlayerTypes, float>
		{
			{ PlayerTypes.PLAYER, 0.15f }
		};
		PlayerTypeTrigger(changes);
	}

	public void OnSecretRoomEnter()
	{
		var changes = new Dictionary<PlayerTypes, float>
		{
			{ PlayerTypes.FREE_SPIRIT, 0.2f }
		};
		PlayerTypeTrigger(changes);
	}

	public void OnShopEnter()
	{
		var changes = new Dictionary<PlayerTypes, float>
		{
			{ PlayerTypes.PHILANTHROPIST, 0.5f },
			{ PlayerTypes.PLAYER, -0.2f }
		};
		PlayerTypeTrigger(changes);
	}

	public void OnShopSkip()
	{
		var changes = new Dictionary<PlayerTypes, float>
		{
			{ PlayerTypes.PHILANTHROPIST, -1f },
			{ PlayerTypes.PLAYER, 0.2f }
		};
		PlayerTypeTrigger(changes);
	}

	public void OnWeaponAcquire()
	{
		var changes = new Dictionary<PlayerTypes, float>
		{
			{ PlayerTypes.ACHIEVER, 0.3f },
			{ PlayerTypes.PLAYER, -0.1f }
		};
		PlayerTypeTrigger(changes);
	}

	public void OnEnvironmentBreak()
	{
		var changes = new Dictionary<PlayerTypes, float>
		{
			{ PlayerTypes.PLAYER, -0.1f },
			{ PlayerTypes.FREE_SPIRIT, 0.05f }
		};
		PlayerTypeTrigger(changes);
	}

	public void OnMoneyUsed(int amount = 1)
	{
		var changes = new Dictionary<PlayerTypes, float>
		{
			{ PlayerTypes.PHILANTHROPIST, 0.05f*amount },
			{ PlayerTypes.ACHIEVER, -0.05f*amount }
		};
		PlayerTypeTrigger(changes);
	}

	public void OnMoneyEarned(int amount = 1)
	{
		var changes = new Dictionary<PlayerTypes, float>
		{
			{ PlayerTypes.ACHIEVER, 0.05f*amount },
			{ PlayerTypes.PHILANTHROPIST, 0.05f*amount }
		};
		PlayerTypeTrigger(changes);
	}

	public void PlayerTypeTrigger(Dictionary<PlayerTypes, float> changesToUpdate)
	{
		// TODO: Update the profile & act accordingly
		foreach (var profileChange in changesToUpdate)
			PlayerProfileData.Profile[profileChange.Key] += profileChange.Value;

		UpdateFeaturesCoefficient();
	}

	public void UpdateFeaturesCoefficient()
	{
		if (pluginData == null)
		{
			Debug.LogWarning("Trying to update features coefficient but plugin data was null.\nAborting...");
			return;
		}

		FeaturesCoefficient.Clear();

		for (int i = 0; i < pluginData.GameplayFeatures.Length; i++)
		{
			GameplayFeature feature = pluginData.GameplayFeatures[i];
			if (feature.FeatureEnabled)
			{
				float newCoef = 0.0f;

				foreach (var profilePlayerType in PlayerProfileData.Profile)
				{
					var varFeature = pluginData.TableOfFeatures[i + (int)profilePlayerType.Key * pluginData.GameplayFeaturesCount] / (pluginData.MaxCellValue - pluginData.MinCellValue);
					var varPlayerType = (2.0f * profilePlayerType.Value / (ProfileMaxCellValue - ProfileMinCellValue)) - 1.0f;
					newCoef += varFeature * varPlayerType;
				}
				newCoef /= (float)PlayerProfileData.Profile.Count;
				FeaturesCoefficient.Add(feature, newCoef);
			}
		}

		OnFeaturesCoefficientUpdated?.Invoke(); // segons el player type fes x o y ???
	}

	public float GetFeatureCoefficient(string featureName)
	{
		foreach (var feature in FeaturesCoefficient)
		{
			if (feature.Key.FeatureName == featureName)
				return feature.Value;
		}

		return -1.0f;
	}

	// reset profile & send update
	public void ResetProfile()
	{
		PlayerProfileData = new PlayerProfileData();
		UpdateFeaturesCoefficient();
		DataPersistenceManager.Instance.SaveProfile();
	}

	public void LoadProfile(PlayerProfileData data)
	{
		PlayerProfileData = data;
		UpdateFeaturesCoefficient();
	}

	public void SaveProfile(ref PlayerProfileData data)
	{
		data = PlayerProfileData;
	}
}

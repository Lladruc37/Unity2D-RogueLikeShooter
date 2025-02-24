using System;

[Serializable]
public class PlayerProfileData
{
	public SerializableDictionary<PlayerTypes, float> Profile;

	public PlayerProfileData()
	{
		Profile = new SerializableDictionary<PlayerTypes, float>();

		for (int i = 0; i < Enum.GetNames(typeof(PlayerTypes)).Length; i++)
			Profile.Add((PlayerTypes)i, 5f);
	}
	public PlayerProfileData(PlayerProfileData data)
	{
		Profile = new SerializableDictionary<PlayerTypes, float>();

		for (int i = 0; i < Enum.GetNames(typeof(PlayerTypes)).Length; i++)
			Profile.Add((PlayerTypes)i, data != null ? data.Profile[(PlayerTypes)i] : 5f);
	}
}

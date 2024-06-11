using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
public class GeneratorManager : MonoBehaviour
{
	public Map_Generation map_Generator;

	private int roomsNow;
	public int RoomsNow
	{
		get
		{
			return roomsNow;
		}
		set
		{
			if (value > 2 && value < maxRooms)
			{
				roomsNow = value;
			}
			else if (value < 3)
			{
				roomsNow = 3;
			}
			else if (value >= maxRooms)
			{
				roomsNow = maxRooms;
			}
		}
	}

	public bool spawnEnemies;

	public int roomIncreaseRange;
	public int maxRooms;

	public List<MapType> mapTypes;

	private int playerSpawnRoom;

	private void Awake()
	{
		RoomsNow = 7;
	}

	public IEnumerator GenerateLevel(bool spawnEnemies, UnityEngine.UI.Text text)
	{
		yield return new WaitForSecondsRealtime(3f);
		this.spawnEnemies = spawnEnemies;
		// TODO: THIS SPAWNS THE ROOMS
		yield return map_Generator.Begin(roomsNow, text, true);

		Room playerRoom = null;
		foreach (Room room in Map.roomObjs)
		{
			if (room.roomType == roomType.playerStart)
			{
				playerRoom = room;
				break;
			}
		}

		playerSpawnRoom = playerRoom.number;
		GameManager.instance.TeleportPlayer(playerRoom);

		if (spawnEnemies)
		{
			EnemiesManager.SpawnInit(playerSpawnRoom);
		}

		if (text != null)
		{
			text.text = "";
		}
	}
}

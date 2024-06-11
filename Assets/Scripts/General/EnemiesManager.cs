using System;
using System.Collections;
using UnityEngine;


[CreateAssetMenu]
public class EnemiesManager : ScriptableObject
{
	[SerializeField]
	public Enemies BossEnemies;
	[SerializeField]
	public Enemies RangeEnemies;
	[SerializeField]
	public Enemies CQCEnemies;
	[SerializeField]
	public Enemies OtherEnemies;

	public static int enemyBaseAmount = 3;
	public static float enemyAmountRange = 2;

	public static EnemiesManager instance;
	public static void Initialize()
	{
		instance = Resources.Load<EnemiesManager>("Enemies_Manager");
	}

	public enum EnemyType
	{
		Boss,
		Range,
		CQC,
		Etc
	}

	public static GameObject GetEnemy(EnemyType type)
	{
		switch (type)
		{
			case EnemyType.Boss:
				return instance.BossEnemies.GetRandomEnemy(GameManager.instance.level);

			case EnemyType.Range:
				return instance.RangeEnemies.GetRandomEnemy(GameManager.instance.level);

			case EnemyType.CQC:
				return instance.CQCEnemies.GetRandomEnemy(GameManager.instance.level);

			case EnemyType.Etc:
				return instance.OtherEnemies.GetRandomEnemy(GameManager.instance.level);

			default:
				return instance.RangeEnemies.GetRandomEnemy(GameManager.instance.level);
		}
	}

	// Use this for initialization

	public static void SpawnInit(int playerSpawnRoom)
	{
		foreach (Room room in Map.roomObjs)
		{
			if (room.number != playerSpawnRoom)
			{
				if (room.roomType == roomType.common)
				{
					// TODO: THIS SPAWNS ENEMIES
					for (int i = 0; i < GetEnemiesAmount(); i++)
					{
						GameObject enemy = GetRandEnemy();
						if (enemy != null)
						{
							SpawnEnemy(room, enemy);
						}
					}
				}

				if (room.roomType == roomType.last)
				{
					// TODO: THIS SPAWNS BOSS
					GameObject enemy = GetEnemy(EnemyType.Boss);
					if (enemy != null)
					{
						SpawnEnemy(room, enemy, new Vector2(room.width / 2, room.height / 2), true);
					}
				}
			}
		}
	}

	private static int GetEnemiesAmount()
	{
		var coef = PluginController.Instance.GetFeatureCoefficient("SCRCTY");
		var amount = enemyBaseAmount + MathF.Ceiling(enemyAmountRange * coef);
		amount = MathF.Max(0, amount);
		Debug.Log($"spawn enemies: {amount}");
		return (int)amount;
	}

	public static GameObject GetRandEnemy()
	{
		EnemyType type = (EnemyType)UnityEngine.Random.Range(1, 3);
		GameObject enemy = GetEnemy(type);
		return enemy;
	}


	public static IEnumerator SpawnEnemy(Room room, GameObject enemy, GameObject spawnAnimator, float spawnWaitTime)
	{
		Vector3 randomPos = room.smartGrid.floorTilesInside[UnityEngine.Random.Range(0, room.smartGrid.floorTilesInside.Count)].transform.position;
		randomPos.z = 9;
		var animation = Instantiate(spawnAnimator, randomPos, Quaternion.identity);
		yield return new WaitForSeconds(spawnWaitTime);
		Destroy(animation);

		SpawnEnemy(room, enemy, randomPos, false);
		yield return 0;
	}

	public static GameObject SpawnEnemy(Room room, GameObject enemy)
	{
		Vector3 randomPos = room.smartGrid.floorTilesInside[UnityEngine.Random.Range(0, room.smartGrid.floorTilesInside.Count)].transform.position;
		randomPos.z = 9;
		GameObject newEnemy = Instantiate(enemy, randomPos, Quaternion.identity, room.enemiesGrid.transform);

		Enemy en = newEnemy.GetComponentInChildren<Enemy>();
		en.room = room;

		room.Enemies.Add(en);

		return newEnemy;
	}

	public static GameObject SpawnEnemy(Room room, GameObject enemy, Vector2 position, bool localPosition)
	{
		if (localPosition)
		{
			position = new Vector2(position.x + room.start.x, position.y + room.start.y);
		}
		if (room.smartGrid.CheckFloorCollision(position))
		{
			GameObject newEnemy = Instantiate(enemy, position, Quaternion.identity, room.enemiesGrid.transform);

			Enemy en = newEnemy.GetComponentInChildren<Enemy>();
			en.room = room;

			room.Enemies.Add(en);

			return newEnemy;
		}

		return null;
	}
}

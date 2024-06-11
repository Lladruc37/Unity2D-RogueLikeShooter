using UnityEngine;

public static class Environment
{
	public static void CreateShop(Room room, GameObject shopToLeft, GameObject shopToLeftLong, GameObject shopToRight, GameObject shopToRightLong)
	{
		if (!Environment.CreateShopToLeft(room, shopToLeft, shopToLeftLong))
			Environment.CreateShopToRight(room, shopToRight, shopToRightLong);
	}

	private static bool CreateShopToLeft(Room room, GameObject shop, GameObject longShop)
	{
		var position = new Vector3(room.start.x + 1, room.end.y - 4, 0);
		var endposition = new Vector3(position.x + 8, position.y + 4);
		if (room.width < (endposition.x - position.x) || !CheckForCollisions(room, longShop, position, endposition, out var ls))
		{
			endposition.x -= 2;
			if (room.width < (endposition.x - position.x) || !CheckForCollisions(room, shop, position, endposition, out var s))
				return false;

			foreach (Transform t in s.GetComponentsInChildren<Transform>())
				room.EnvironmentTiles.Add(new EnvironmentTile(t.gameObject, room));

			return true;
		}

		foreach (Transform t in ls.GetComponentsInChildren<Transform>())
			room.EnvironmentTiles.Add(new EnvironmentTile(t.gameObject, room));

		return true;
	}


	private static bool CreateShopToRight(Room room, GameObject shop, GameObject longShop)
	{
		Vector3 position = new Vector3(room.end.x - 5, room.end.y - 4, 0);
		Vector3 endposition = new Vector3(position.x + 4, position.y + 4);

		if (room.width < (endposition.x - position.x) || !CheckForCollisions(room, longShop, position, endposition, out var ls))
		{
			position.x -= 2;
			if (room.width < (endposition.x - position.x) || !CheckForCollisions(room, shop, position, endposition, out var s))
				return false;

			foreach (Transform t in s.GetComponentsInChildren<Transform>())
				room.EnvironmentTiles.Add(new EnvironmentTile(t.gameObject, room));

			return true;
		}

		foreach (Transform t in ls.GetComponentsInChildren<Transform>())
			room.EnvironmentTiles.Add(new EnvironmentTile(t.gameObject, room));

		return true;
	}

	private static bool CheckForCollisions(Room room, GameObject shop, Vector3 position, Vector3 endposition, out GameObject s)
	{
		s = room.CreateEnvironmentObject(shop, position);
		foreach (GameObject d in room.Doors)
		{
			if (SimpleFunctions.Check_Superimpose(position, endposition, d.transform.position, d.transform.position, 2))
			{
				GameObject.Destroy(s);
				return false;
			}
		}
		return true;
	}

	public static void PaintRoomOutSide(Room room, Color color)
	{
		foreach (GameObject tile in room.smartGrid.floorTilesOutSide)
		{
			tile.GetComponent<SpriteRenderer>().color = color;
		}
	}

	public static void PaintLine(Room room, bool axisX, Color color)
	{
		if (axisX == false)
		{
			int Y = System.Convert.ToInt32(Random.Range(room.start.y, room.start.y + room.height));

			foreach (GameObject tile in room.smartGrid.floorTilesInside)
			{
				if (tile.transform.position.y == Y)
				{
					tile.GetComponent<SpriteRenderer>().color = color;
				}
			}
		}
		else
		{
			int X = System.Convert.ToInt32(Random.Range(room.start.x, room.start.x + room.width));

			foreach (GameObject tile in room.smartGrid.floorTilesInside)
			{
				if (tile.transform.position.x == X)
				{
					tile.GetComponent<SpriteRenderer>().color = color;
				}
			}
		}
	}

	//TODO: THIS SPAWNS CHESTS
	public static void SpawnChest(Room room, GameObject chest, GameObject itemInside, Vector2 position)
	{
		chest.GetComponent<Chest>().itemInside = itemInside;

		GameObject c = Object.Instantiate(chest, position, Quaternion.identity) as GameObject;

		c.transform.parent = room.grid.transform;
	}

	public static void SpawnChest(Room room, GameObject chest, Vector2 position)
	{
		GameObject c = Object.Instantiate(chest, position, Quaternion.identity) as GameObject;

		c.transform.parent = room.grid.transform;
	}
}

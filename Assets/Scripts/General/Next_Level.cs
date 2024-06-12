using UnityEngine;

public class Next_Level : MonoBehaviour
{
	public Room room;
	public Interactable interact;

	private bool pressed = false;

	void Start()
	{
		if (room == null)
		{
			room = GetComponentInParent<Room>();
		}
		interact.interactWithObject = nextLevel;
	}

	private void nextLevel()
	{
		if (room.Enemies.Count == 0)
		{
			if (Player.instance.isAlive)
			{
				if (pressed == false)
				{
					foreach (Room r in Map.roomObjs)
					{
						if (r.roomType == roomType.shop && !r.hereWasPlayer)
							PluginController.Instance.OnShopSkip();
					}
					GameManager.instance.loadManager.NextLevel();
				}
			}
		}
	}

}

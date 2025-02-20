using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Тип NPC
/// </summary>
public enum NPCType { Player, Enemy }

/// <summary>
/// Основной класс от которого наследуются "живые" существа
/// </summary>
public class NPC : MonoBehaviour
{
	/// <summary>
	/// Тип NPC
	/// </summary>
	public NPCType npcType;
	/// <summary>
	/// Может ли стрелять
	/// </summary>
	public bool canShoot = false;
	/// <summary>
	/// Спрайт тела
	/// </summary>
	public SpriteRenderer bodySprite;
	/// <summary>
	/// Аниматор NPC
	/// </summary>
	public Animator animator;

	public Movement_Agent movement;

	public Vector2 lookAt;

	public StatsManager stats;

	public Collider2D hitBox;

	public bool FacingRight = true;

	public List<GameObject> weaponsToInitialize;
	public List<WeaponItem> weapons;
	protected Weapon weapon;

	public NPCMods[] modificators = new NPCMods[Enum.GetValues(typeof(Modificator)).Length];
	public BulletMods[] bulletModificators = new BulletMods[Enum.GetValues(typeof(WeaponModification)).Length];

	public GameObject loot;
	public int lootBaseAmount;
	public float lootAmountRange;

	public bool rollCoolDown = false;

	public bool isAlive = true;

	public Room room;

	public event EventHandler WakeUpEvent;
	public event EventHandler SleepEvent;

	protected virtual void Start()
	{
		InitializeWeapon();
	}

	protected virtual void InitializeWeapon()
	{
		weapons = new List<WeaponItem>();
		if (weaponsToInitialize.Count != 0)
		{
			int i = 0;
			foreach (GameObject w in weaponsToInitialize)
			{
				if (w != null)
				{
					GameObject weap = Instantiate(weaponsToInitialize[i], transform);
					weap.GetComponent<WeaponItem>().Give(this);
				}
				i++;
			}
		}
	}

	// Вызывает WakeUpEvent
	public virtual void WakeUp()
	{
		WakeUpEvent?.Invoke(this, null);
	}

	// Вызывает SleepEvent
	public virtual void Sleep()
	{
		SleepEvent?.Invoke(this, null);
	}

	public virtual void IncreaseMaxHealth(int hp)
	{
		stats.IncreaseMaxHealth(hp);
	}

	public int GetHp()
	{
		return stats.curHealth;
	}

	public void MoveByImpulse(Vector2 normalized)
	{
		movement.rb.AddForce(normalized * 10 * Time.timeScale, ForceMode2D.Impulse);
	}

	public virtual void TakeDamage(int damage)
	{
		Player.instance.AddChargeToItem(damage);
		if (stats.curArmor <= 0)
		{
			stats.curHealth -= damage;
			if (stats.curHealth <= 0)
			{
				if (isAlive)
				{
					isAlive = false;
					StartCoroutine(Die(false));
				}
			}
		}
		else
		{
			stats.curArmor -= damage;
		}
	}

	public void CancelReload()
	{
		Weapon weap = GetWeapon();
		if (weap != null)
		{
			weap.CancelReload();
		}
	}

	public virtual bool Heal(int hp)
	{
		return stats.Heal(hp);
	}

	public virtual IEnumerator Die(bool destroy)
	{
		isAlive = false;

		if (npcType == NPCType.Enemy)
			PluginTriggerController.Instance.OnPluginTrigger(PluginTriggerType.ENEMY_DEATH);

		yield return 0;
	}

	protected virtual IEnumerator PlayDeathAnimation()
	{
		isAlive = false;
		animator.SetBool("Death", true);
		yield return new WaitForSeconds(0.1f);

		yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length / animator.GetCurrentAnimatorStateInfo(0).speed);
	}

	public virtual bool TakeItem(Item item)
	{
		if (item.itemType == ItemTypes.WeaponItem)
		{
			return TakeWeapon(item.GetComponent<WeaponItem>());
		}
		return false;
	}

	virtual protected bool TakeWeapon(WeaponItem weaponItem)
	{
		if (weapons.Count == 0)
		{
			if (FacingRight == false)
			{
				weaponItem.Flip();
			}
			weapons.Add(weaponItem);
			weapon = weaponItem.weapon;
			return true;
		}
		return false;
	}
	virtual public void AddModification(Modificator addMod, float power)
	{
		for (int i = 0; i < modificators.Length; i++)
		{
			if (modificators[i].mod == addMod)
			{
				modificators[i].power += power;
				return;
			}
			if (modificators[i].power == 0)
			{
				modificators[i].mod = addMod;
				modificators[i].power += power;
				return;
			}
		}
	}

	virtual public void AddBulletModification(WeaponModification addMod, float power)
	{
		for (int i = 0; i < bulletModificators.Length; i++)
		{
			if (bulletModificators[i].mod == addMod)
			{
				bulletModificators[i].power += power;
				return;
			}
			if (bulletModificators[i].power == 0)
			{
				bulletModificators[i].mod = addMod;
				bulletModificators[i].power += power;
				return;
			}
		}
	}

	virtual public void DropLoot()
	{
		if (!PluginController.Instance.pluginEnabled)
		{
			Drop(loot, lootBaseAmount, transform.position);
			return;
		}
		var coef = 0.1f * PluginController.Instance.GetFeatureCoefficient("CHLGS")
			+ 0.2f * PluginController.Instance.GetFeatureCoefficient("TRADE")
			+ 0.05f * PluginController.Instance.GetFeatureCoefficient("LV&PROG")
			+ 0.5f * PluginController.Instance.GetFeatureCoefficient("CHANCE")
			+ 0.05f * PluginController.Instance.GetFeatureCoefficient("RESRC M")
			- 0.15f * PluginController.Instance.GetFeatureCoefficient("SCRCTY");
		var amount = lootBaseAmount + MathF.Ceiling(lootAmountRange * coef);
		amount = MathF.Max(amount, 0);
		Debug.Log($"drop loot: {amount}");
		Drop(loot, (int)amount, transform.position);
	}

	public void Drop(GameObject loot, int amount, Vector2 position)
	{
		var initialPos = position;
		for (int i = 0; i < amount; i++)
		{
			GameObject instance = Instantiate(loot, position, Quaternion.identity);
			if (instance.GetComponent<Rigidbody2D>() != null)
				instance.GetComponent<Rigidbody2D>().AddForce(new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)) * 3, ForceMode2D.Impulse);
			else
			{
				Destroy(instance);
				position = new Vector2(initialPos.x + 1 + i * 2, initialPos.y + 2);
				Instantiate(loot, position, Quaternion.identity);
			}
		}
	}

	protected void DisableNPC()
	{
		Collider2D[] colliders = movement.GetComponentsInChildren<Collider2D>();
		foreach (Collider2D coll in colliders)
		{
			coll.enabled = false;
		}

		MonoBehaviour[] scripts = movement.GetComponentsInChildren<MonoBehaviour>();
		foreach (MonoBehaviour script in scripts)
		{
			script.enabled = false;
		}

		SpriteRenderer[] sprites = movement.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer sprite in sprites)
		{
			sprite.enabled = false;
		}

		bodySprite.enabled = true;
		bodySprite.color = new Color(255, 255, 255, 0.6f);

		if (weapon != null)
		{
			weapon.weaponItem.gameObject.SetActive(false);
		}
	}

	protected void DisableMovement()
	{
		Destroy(movement.navAgent);
		Destroy(movement.rb);
	}

	public void Destroy()
	{
		Destroy(this.gameObject);
	}

	public Weapon GetWeapon()
	{
		if (weapon != null)
		{
			return weapon;
		}
		else
		{
			return null;
		}
	}

}

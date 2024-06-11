using System;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : EnvironmentObjectWithHealth
{
	private Collider2D coll;

	public bool destructible;

	public GameObject loot;
	public int lootBaseAmount;
	public float lootAmountRange;
	private bool lootDropped = false;

	[HideInInspector]
	public GameObject destroyOnBrake;
	[HideInInspector]
	public bool breakWithAnimation;

	private int hits;


	public List<Vector2> coordinates;

	protected Explodable expl;
	protected MyFragmentsAddon addon;

	protected override void Awake()
	{
		base.Awake();
		curHealth = maxHealth;
		coll = GetComponent<Collider2D>();
		expl = GetComponent<Explodable>();
		if (expl != null)
		{
			expl.deleteFragments();
			addon = GetComponent<MyFragmentsAddon>();
		}
	}

	protected virtual void Brake()
	{
		coll.enabled = false;
		Destroy(destroyOnBrake);

		if (!lootDropped)
			DropLoot();

		if (breakWithAnimation == true)
			BreakWithAnimation();
		else
			BreakWithFragments();
	}

	private void BreakWithAnimation()
	{
		animator.SetFloat("Hits", 1);
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		if (renderer != null)
		{
			renderer.sortingLayerName = "Floor";
			renderer.sortingOrder = 0;
		}
	}

	private void BreakWithFragments()
	{
		expl.explode();
	}

	public override void TakeDamage(int dmg)
	{
		hits += dmg;
		curHealth -= dmg;
		if (curHealth > 0)
		{
			if (animator != null)
			{
				animator.SetFloat("Hits", (float)(hits / System.Convert.ToDecimal(maxHealth)));
			}
		}
		if (curHealth <= 0)
		{
			Brake();
		}
	}

	public override void PushBack(Vector2 normalized, float power)
	{
		if (addon != null)
			addon.velocity = normalized * power;
	}

	virtual public void DropLoot()
	{
		if (loot == null) return;

		lootDropped = true;
		var coef = PluginController.Instance.GetFeatureCoefficient("SCRCTY");
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
				instance.GetComponent<Rigidbody2D>().AddForce(new Vector2(UnityEngine.Random.Range(0.1f, 1), UnityEngine.Random.Range(0.1f, 1)) * 5, ForceMode2D.Impulse);
			else
			{
				Destroy(instance);
				position = new Vector2(initialPos.x + 1 + i * 2, initialPos.y + 2);
				Instantiate(loot, position, Quaternion.identity);
			}
		}
	}
}
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerryNpc;

namespace TerryNpc;

public partial class Terry : AnimatedEntity
{
	public ClothingContainer Outfit { get; private set; }
	public override void Spawn()
	{
		base.Spawn();

		Tags.Add("npc", "solid");

		SetModel("models/citizen/citizen.vmdl");
		SetupPhysicsFromModel(PhysicsMotionType.Keyframed);

		EnableHitboxes = true;

		Health = 100;

		Outfit = RandomOutfit.Generate();
		Outfit.DressEntity(this);
	}

	private DamageInfo lastDamage;

	public override void TakeDamage(DamageInfo info)
	{
		if (LifeState == LifeState.Dead)
			return;

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;
		if (Game.IsServer && Health > 0f && LifeState == LifeState.Alive)
		{
			Health -= info.Damage;
			if (Health <= 0f)
			{
				Health = 0f;
				OnKilled();
			}
		}

		this.ProceduralHitReaction(info);

		if (info.Hitbox.HasTag("head"))
		{
			info.Damage *= 10.0f;
		}

		lastDamage = info;
	}

	public override void OnKilled()
	{
		if (LifeState == LifeState.Alive)
		{
			LifeState = LifeState.Dead;
		}

		BecomeRagdoll(Velocity, lastDamage.Position, lastDamage.Force, lastDamage.BoneIndex, lastDamage.HasTag("bullet"), lastDamage.HasTag("blast"));

		LifeState = LifeState.Dead;

		Delete();
	}
}
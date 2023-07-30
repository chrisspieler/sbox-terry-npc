using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace TerryNpc;

public partial class Terry : AnimatedEntity
{
	[BindComponent]
	public Mover Mover { get; }
	[BindComponent]
	public Navigator Navigator { get; }

	public ClothingContainer Outfit { get; private set; }
	protected static List<string> SpawnSounds { get; set; } = new()
	{

	};
	protected static int CurrentVoiceLineIndex { get; set; } = 0;

    public override void Spawn()
	{
		base.Spawn();

		Tags.Add("npc", "solid");

		SetModel("models/citizen/citizen.vmdl");
		SetupPhysicsFromModel(PhysicsMotionType.Keyframed);

        Components.Add(new Gravity());
        var target = Game.Clients.First(c => c.Pawn != null).Pawn as Entity;
        Components.Add(new Navigator() { Target = target });
        Components.Add(new Unstucker());

        EnableHitboxes = true;

		Health = 15;

		Outfit = RandomOutfit.Generate();
		Outfit.DressEntity(this);

		if (SpawnSounds.Any())
		{
            var voiceLine = SpawnSounds[CurrentVoiceLineIndex];
            CurrentVoiceLineIndex++;
			// Cycle through the list of spawn sounds.
            CurrentVoiceLineIndex %= SpawnSounds.Count;

            var snd = PlaySound(voiceLine);
            snd.SetVolume(0.6f);
        }

		_ = new ToolsRefresher();
	}

    [GameEvent.Tick.Server]
	protected void OnServerTick()
	{
		Animate();

		if (Navigator.Target != null)
		{
			LookAtPoint = Navigator.Target.Position;
			return;
		}

		var nearestPlayer = FindNearestPerson(100f, true);
		if (nearestPlayer.IsValid())
		{
			LookAtPoint = nearestPlayer.Position;
		}
		else
		{
			LookRandomly();
		}
	}

    private DamageInfo lastDamage;

	public override void TakeDamage(DamageInfo info)
	{
		if (LifeState == LifeState.Dead)
			return;

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;
		// Headshots are fatal
        if (info.Hitbox.HasAnyTags("head"))
        {
            info.Damage *= 10.0f;
        }
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

    protected Entity FindNearestPerson(float radius, bool withEyeContact)
    {
        var people = Entity
			.FindInSphere(Position, radius)
			.Where(e => e.Tags.Has("player") || e.Tags.Has("npc"));
		IEnumerable<Entity> nearbyPeople = people
			.Where(p => p.Position.Distance(Position) <= radius)
			.OrderBy(p => p.Position.Distance(Position));
		if (withEyeContact)
		{
			nearbyPeople = nearbyPeople
				.Where(p => p.AimRay.Forward.Dot(AimRay.Forward) < 0f);
        }
		return nearbyPeople.FirstOrDefault();
    }

    private TimeUntil StopLookTime { get; set; }

    private void LookRandomly()
    {
        if (!StopLookTime)
        {
            return;
        }

        StopLookTime = Game.Random.Float(0.25f, 15f);
        var frontPos = Position + Rotation.Forward * 400f + Vector3.Zero.WithZ(64f);
        var frontBox = BBox.FromPositionAndSize(frontPos, 180f);
        LookAtPoint = frontBox.RandomPointInside;
    }
}
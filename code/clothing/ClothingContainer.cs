﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Sandbox;

namespace TerryNpc;

/// <summary>
/// Holds a collection of clothing items. Won't let you add items that aren't compatible.
/// </summary>
public partial class ClothingContainer : INetworkSerializer
{
    public List<Clothing> Clothing { get; set; } = new();

    /// <summary>
    /// Add a clothing item if we don't already contain it, else remove it
    /// </summary>
    public void Toggle(Clothing clothing)
    {
        if (Has(clothing)) Remove(clothing);
        else Add(clothing);
    }

    /// <summary>
    /// Add clothing item
    /// </summary>
    public void Add(Clothing clothing)
    {
        if (clothing == null)
            return;
        Clothing.RemoveAll(x => !x.CanBeWornWith(clothing));
        Clothing.Add(clothing);
    }

    /// <summary>
    /// Remove clothing item
    /// </summary>
    public void Remove(Clothing clothing)
    {
        Clothing.Remove(clothing);
    }

    /// <summary>
    /// Returns true if we have this clothing item
    /// </summary>
    public bool Has(Clothing clothing) => Clothing.Contains(clothing);

    /// <summary>
    /// Return a list of bodygroups and what their value should be
    /// </summary>
    public IEnumerable<(string name, int value)> GetBodyGroups()
    {
        var mask = Clothing.Select(x => x.HideBody).DefaultIfEmpty().Aggregate((a, b) => a | b);

        yield return ("head", (mask & Sandbox.Clothing.BodyGroups.Head) != 0 ? 1 : 0);
        yield return ("Chest", (mask & Sandbox.Clothing.BodyGroups.Chest) != 0 ? 1 : 0);
        yield return ("Legs", (mask & Sandbox.Clothing.BodyGroups.Legs) != 0 ? 1 : 0);
        yield return ("Hands", (mask & Sandbox.Clothing.BodyGroups.Hands) != 0 ? 1 : 0);
        yield return ("Feet", (mask & Sandbox.Clothing.BodyGroups.Feet) != 0 ? 1 : 0);
    }

    /// <summary>
    /// Serialize to Json
    /// </summary>
    public string Serialize()
    {
        return System.Text.Json.JsonSerializer.Serialize(Clothing.Select(x => new Entry { Id = x.ResourceId }));
    }

    /// <summary>
    /// Deserialize from Json
    /// </summary>
    public void Deserialize(string json)
    {
        Clothing.Clear();

        if (string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            var entries = System.Text.Json.JsonSerializer.Deserialize<Entry[]>(json);

            foreach (var entry in entries)
            {
                var item = ResourceLibrary.Get<Clothing>(entry.Id);
                if (item == null) continue;
                Add(item);
            }
        }
        catch (Exception e)
        {
            Log.Warning(e, "Error deserailizing clothing");
        }
    }

    /// <summary>
    /// Used for serialization
    /// </summary>
    public struct Entry
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        // in the future we could allow some
        // configuration (tint etc) of items
        // which is why this is a struct instead
        // of serializing an array of ints
    }

    List<AnimatedEntity> ClothingModels = new();

    public void ClearEntities()
    {
        foreach (var model in ClothingModels)
        {
            model.Delete();
        }
        ClothingModels.Clear();
    }

    /// <summary>
    /// Dress this citizen with clothes defined inside this class. We'll save the created entities in ClothingModels.
    /// All clothing entities are tagged with "clothes".
    /// </summary>
    public void DressEntity(AnimatedEntity citizen, bool hideInFirstPerson = true, bool castShadowsInFirstPerson = true)
    {
        //
        // Start with defaults
        //
        citizen.SetMaterialGroup("default");

        //
        // Remove old models
        //
        ClearEntities();

        var SkinMaterial = Clothing.Select(x => x.SkinMaterial).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => Material.Load(x)).FirstOrDefault();
        var EyesMaterial = Clothing.Select(x => x.EyesMaterial).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => Material.Load(x)).FirstOrDefault();

        if (SkinMaterial != null) citizen.SetMaterialOverride(SkinMaterial, "skin");
        if (EyesMaterial != null) citizen.SetMaterialOverride(EyesMaterial, "eyes");

        //
        // Create clothes models
        //
        foreach (var c in Clothing)
        {
            if (c.Model == "models/citizen/citizen.vmdl")
            {
                citizen.SetMaterialGroup(c.MaterialGroup);
                continue;
            }

            var anim = new AnimatedEntity(c.Model, citizen);

            anim.Tags.Add("clothes");

            if (SkinMaterial != null) anim.SetMaterialOverride(SkinMaterial, "skin");
            if (EyesMaterial != null) anim.SetMaterialOverride(EyesMaterial, "eyes");

            anim.EnableHideInFirstPerson = hideInFirstPerson;
            anim.EnableShadowInFirstPerson = castShadowsInFirstPerson;
            anim.EnableTraceAndQueries = false;

            if (!string.IsNullOrEmpty(c.MaterialGroup))
                anim.SetMaterialGroup(c.MaterialGroup);

            ClothingModels.Add(anim);
        }

        //
        // Set body groups
        //
        foreach (var group in GetBodyGroups())
        {
            citizen.SetBodyGroup(group.name, group.value);
        }
    }

    /// <summary>
    /// Dress this citizen with clothes defined inside this class. We'll save the created entities in ClothingModels.
    /// All clothing entities are tagged with "clothes".
    /// </summary>
    public List<SceneModel> DressSceneObject(SceneModel citizen)
    {
        var created = new List<SceneModel>();
        var world = citizen.World;

        //
        // Start with defaults
        //
        citizen.SetMaterialGroup("default");
        citizen.SetMaterialOverride(null);

        var SkinMaterial = Clothing.Select(x => x.SkinMaterial).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => Material.Load(x)).FirstOrDefault();
        var EyesMaterial = Clothing.Select(x => x.EyesMaterial).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => Material.Load(x)).FirstOrDefault();

        if (SkinMaterial != null) citizen.SetMaterialOverride(SkinMaterial, "skin");
        if (EyesMaterial != null) citizen.SetMaterialOverride(EyesMaterial, "eyes");

        foreach (var c in Clothing)
        {
            if (string.IsNullOrEmpty(c.Model) || !string.IsNullOrEmpty(c.SkinMaterial))
                continue;

            var model = Model.Load(c.Model);
            var anim = new SceneModel(world, model, citizen.Transform);

            created.Add(anim);

            if (!string.IsNullOrEmpty(c.MaterialGroup))
                anim.SetMaterialGroup(c.MaterialGroup);

            citizen.AddChild("clothing", anim);

            if (SkinMaterial != null) anim.SetMaterialOverride(SkinMaterial, "skin");
            if (EyesMaterial != null) anim.SetMaterialOverride(EyesMaterial, "eyes");

            anim.Update(0.1f);
        }

        foreach (var group in GetBodyGroups())
        {
            citizen.SetBodyGroup(group.name, group.value);
        }

        return created;
    }

    public void Write(NetWrite write)
    {
        write.Write(Clothing.Select(c => c.ResourceId).ToArray());
    }

    public void Read(ref NetRead read)
    {
        int[] clothingIds = Array.Empty<int>();
        read.ReadArray(clothingIds);
        for (int i = 0; i < clothingIds.Length; i++)
        {
            Clothing.Add(ResourceLibrary.Get<Clothing>(clothingIds[i]));
        }
    }


}

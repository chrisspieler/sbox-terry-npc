using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TerryNpc;

public static class RandomOutfit
{
	private static IEnumerable<Clothing> _allClothing;

	public static float FacialPercent { get; set; } = 0.4f;
	public static float ShirtlessFactor { get; set; } = 0.2f;
	public static float CoatPercent { get; set; } = 0.4f;

	public static ClothingContainer Generate()
	{
		if (_allClothing == null)
			_allClothing = ResourceLibrary.GetAll<Clothing>();

		var outfit = new ClothingContainer();

		var mayIncludeNull = Game.Random.NextSingle() <= ShirtlessFactor;
		Clothing top = GetRandomClothing(
			c => c.Category == Clothing.ClothingCategory.Tops 
				&& c.SlotsOver.HasFlag(Clothing.Slots.Chest),
			includeNull: mayIncludeNull,
			exclude: new[] { "Binman Polo Shirt" }
			);
		outfit.Add(top);

	    if (Game.Random.NextSingle() <= CoatPercent)
		{
			Clothing otherTop = GetRandomClothing(
				c => c.Category == Clothing.ClothingCategory.Tops 
				&& (top != null && c.CanBeWornWith(top))
				&& c.SubCategory != "Vests"
				);
			if (otherTop != null)
				outfit.Add(otherTop);
		}


		Clothing bottom = GetRandomClothing(
			c => c.Category == Clothing.ClothingCategory.Bottoms,
			includeNull: false,
			exclude: new[] { "Leg Armour", "Cardboard Trousers", "Bin Man Trousers" }
			);
		outfit.Add(bottom);

		Clothing footwear = GetRandomClothing(c => c.Category == Clothing.ClothingCategory.Footwear);
		outfit.Add(footwear);

		Clothing skin = GetRandomClothing(c => c.Category == Clothing.ClothingCategory.Skin);
		outfit.Add(skin);

		if (Game.Random.NextSingle() <= FacialPercent)
		{
			Clothing facial = GetRandomClothing(c => c.Category == Clothing.ClothingCategory.Facial);
			outfit.Add(facial);
		}

		Clothing hair = GetRandomClothing(
			c => c.Category == Clothing.ClothingCategory.Hair 
			&& c.SlotsUnder.HasFlag(Clothing.Slots.HeadTop)
			);
		outfit.Add(hair);

		Clothing eyebrows = GetRandomClothing(
			c => c.Category == Clothing.ClothingCategory.Hair
			&& c.SlotsUnder.HasFlag(Clothing.Slots.EyeBrows)
			);
		outfit.Add(eyebrows);

		return outfit;

		Clothing GetRandomClothing(Func<Clothing, bool> predicate, bool includeNull = true, params string[] exclude)
		{
			var options = _allClothing.Where(predicate);
			if (exclude != null)
			{
				options = options.Where(c => !exclude.Contains(c.Title));
			}
			if (includeNull)
			{
				options = options.Append(null);
			}
			return options.Random();
		}
	}
}

using System.Collections.Generic;

public class ActiveField {
	// For boolean values, 0 is false and anything else is true
	public Dictionary<int, int> effects;

	public ActiveField() {
		effects = new Dictionary<int, int>();
		effects.Add(Effects.ElectricTerrain, 0);
		effects.Add(Effects.FairyLock, 0);
		effects.Add(Effects.FusionBolt, 0);
		effects.Add(Effects.FusionFlare, 0);
		effects.Add(Effects.GrassyTerrain, 0);
		effects.Add(Effects.Gravity, 0);
		effects.Add(Effects.IonDeluge, 0);
		effects.Add(Effects.MagicRoom, 0);
		effects.Add(Effects.MistyTerrain, 0);
		effects.Add(Effects.MudSportField, 0);
		effects.Add(Effects.TrickRoom, 0);
		effects.Add(Effects.WaterSportField, 0);
		effects.Add(Effects.WonderRoom, 0);
	}
}
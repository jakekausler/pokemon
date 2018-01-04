using UnityEngine;
using System.Collections.Generic;

public class ActiveSide : MonoBehaviour {
	// For boolean values, 0 is false and anything else is true
	public Dictionary<int, int> effects;

	public void Start() {
		effects = new Dictionary<int, int>();
		effects.Add(Effects.CraftyShield, 0);
		effects.Add(Effects.EchoedVoiceCounter, 0);
		effects.Add(Effects.EchoedVoiceUsed, 0);
		effects.Add(Effects.LastRoundFainted, -1);
		effects.Add(Effects.LightScreen, 0);
		effects.Add(Effects.LuckyChant, 0);
		effects.Add(Effects.MatBlock, 0);
		effects.Add(Effects.Mist, 0);
		effects.Add(Effects.QuickGuard, 0);
		effects.Add(Effects.Rainbow, 0);
		effects.Add(Effects.Reflect, 0);
		effects.Add(Effects.Round, 0);
		effects.Add(Effects.Safeguard, 0);
		effects.Add(Effects.SeaOfFire, 0);
		effects.Add(Effects.Spikes, 0);
		effects.Add(Effects.StealthRock, 0);
		effects.Add(Effects.StickyWeb, 0);
		effects.Add(Effects.Swamp, 0);
		effects.Add(Effects.Tailwind, 0);
		effects.Add(Effects.ToxicSpikes, 0);
		effects.Add(Effects.WideGuard, 0);
	} 
}
using System;
using System.Collections.Generic;

public class BattleTrainer {
	public string name;
	public int id;
	public int metaID;
	public int trainerType;
	public int outfit;
	public Dictionary<int, bool> badges;
	public int money;
	public Dictionary<int, bool> seen;
	public Dictionary<int, bool> owned;
	public Dictionary<int, List<Dictionary<int, bool>>> formSeen;
	public Dictionary<int, List<int>> formLastSeen;
	public Dictionary<int, bool> shadowCaught;
	public Pokemon[] party;
	public bool pokedex;
	public bool pokegear;
	public int language;

	public string TrainerTypeName() {
		return TrainerTypes.GetName(trainerType);
	}

	public string FullName() {
		return string.Format("{0} {1}", TrainerTypeName(), name);
	}

	public int PublicID(int i=-1) {
		if (i >= 0) {
			return i&0xFFFF;
		} else {
			return id&0xFFFF;
		}
	}

	public int SecretID(int i=-1) {
		if (i >= 0) {
			return i>>16;
		} else {
			return id>>16;
		}
	}

	public int GetForeignID() {
		int fid = 0;
		while (fid != id) {
			fid = (int)Utilities.Rand(256);
			fid |= ((int)Utilities.Rand(256))<<8;
			fid |= ((int)Utilities.Rand(256))<<16;
			fid |= ((int)Utilities.Rand(256))<<24;
		}
		return fid;
	}

	public void SetForeignID(BattleTrainer other) {
		id = other.GetForeignID();
	}

	public int MetaID() {
		if (metaID < 0) {
			metaID = PokemonGlobal.playerID;
		}
		if (metaID < 0) {
			metaID = 0;
		}
		return metaID;
	}

	public int Outfit() {
		if (outfit < 0) {
			outfit = 0;
		}
		return outfit;
	}

	public int Language() {
		if (language == 0) {
			language = Utilities.GetLanguage();
		}
		return language;
	}

	public int Money(int value) {
		money = Math.Max(Math.Min(value, Settings.MAX_MONEY), 0);
		return money;
	}

	public int MoneyEarned() {
		return TrainerTypes.GetTrainerType(trainerType).BaseMoney;
	}

	public int Skill() {
		return TrainerTypes.GetTrainerType(trainerType).SkillLevel;
	}

	public string SkillCode() {
		return TrainerTypes.GetTrainerType(trainerType).SkillCode;
	}

	public bool HasSkillCode(string code) {
		return SkillCode().Contains(code);
	}

	public int NumBadges() {
		int r = 0;
		foreach (KeyValuePair<int, bool> entry in badges) {
			if (entry.Value) {
				r += 1;
			}
		}
		return r;
	}

	public int Gender() {
		if (TrainerTypes.GetTrainerType(trainerType).Gender == "Male") {
			return 0;
		} else if (TrainerTypes.GetTrainerType(trainerType).Gender == "Female") {
			return 1;
		} else {
			return 2;
		}
	}

	public bool IsMale() {
		return Gender() == 0;
	}

	public bool IsFemale() {
		return Gender() == 1;
	}

	public Pokemon[] PokemonParty() {
		List<Pokemon> valid = new List<Pokemon>();
		for (int i=0; i<party.Length; i++) {
			if (party[i] != null && !party[i].Egg()) {
				valid.Add(party[i]);
			}
		}
		return valid.ToArray();
	}

	public Pokemon[] AblePokemonParty() {
		List<Pokemon> valid = new List<Pokemon>();
		for (int i=0; i<party.Length; i++) {
			if (party[i] != null && !party[i].Egg() && party[i].hp > 0) {
				valid.Add(party[i]);
			}
		}
		return valid.ToArray();
	}

	public int PartyCount() {
		return party.Length;
	}

	public int PokemonCount() {
		int r = 0;
		for (int i=0; i<party.Length; i++) {
			if (party[i] != null && !party[i].Egg()) {
				r += 1;
			}
		}
		return r;
	}

	public int AblePokemonCount() {
		int r = 0;
		for (int i=0; i<party.Length; i++) {
			if (party[i] != null && !party[i].Egg() && party[i].hp > 0) {
				r += 1;
			}
		}
		return r;
	}

	public Pokemon FirstPokemon() {
		Pokemon[] p = PokemonParty();
		if (p.Length == 0) {
			return null;
		}
		return p[0];
	}

	public Pokemon FirstAblePokemon() {
		Pokemon[] p = AblePokemonParty();
		if (p.Length == 0) {
			return null;
		}
		return p[0];
	}

	public Pokemon LastParty() {
		if (party.Length == 0) {
			return null;
		}
		return party[party.Length-1];
	}

	public Pokemon LastPokemon() {
		Pokemon[] p = PokemonParty();
		if (p.Length == 0) {
			return null;
		}
		return p[p.Length-1];
	}

	public Pokemon LastAblePokemon() {
		Pokemon[] p = AblePokemonParty();
		if (p.Length == 0) {
			return null;
		}
		return p[p.Length-1];
	}

	public int PokedexSeen(int region=-1) {
		int r = 0;
		if (region == -1) {
			for (int i=0; i<Species.MaxValue(); i++) {
				if (seen[i]) {
					r++;
				}
			}
		} else {
			List<int> regionList = Utilities.AllRegionalSpecies(region);
			for (int i=0; i<regionList.Count; i++) {
				if (seen[regionList[i]]) {
					r++;
				}
			}
		}
		return r;
	}

	public int PokedexOwned(int region=-1) {
		int r = 0;
		if (region == -1) {
			for (int i=0; i<Species.MaxValue(); i++) {
				if (owned[i]) {
					r++;
				}
			}
		} else {
			List<int> regionList = Utilities.AllRegionalSpecies(region);
			for (int i=0; i<regionList.Count; i++) {
				if (owned[regionList[i]]) {
					r++;
				}
			}
		}
		return r;
	}

	public int numFormsSeen(int species) {
		int r = 0;
		List<Dictionary<int, bool>> arr = formSeen[species];
		for (int i=0; i<arr.Count; i++) {
			foreach (KeyValuePair<int, bool> entry in arr[i]) {
				if (entry.Value) {
					r++;
				}
			}
		}
		return r;
	}

	public bool HasSeen(int species) {
		return species > 0 ? seen[species] : false;
	}

	public bool HasOwned(int species) {
		return species > 0 ? owned[species] : false;
	}

	public void SetSeen(int species) {
		if (species > 0) {
			seen[species] = true;
		}
	}

	public void SetOwned(int species) {
		if (species > 0) {
			owned[species] = true;
		}
	}

	public void ClearPokedex() {
		seen = new Dictionary<int, bool>();
		owned = new Dictionary<int, bool>();
		formSeen = new Dictionary<int, List<Dictionary<int, bool>>>();
		formLastSeen = new Dictionary<int, List<int>>();
		for (int i=0; i<Species.MaxValue(); i++) {
			seen[i] = false;
			owned[i] = false;
			formLastSeen[i] = new List<int>();
			formSeen[i] = new List<Dictionary<int, bool>>();
			formSeen[i].Add(new Dictionary<int, bool>());
			formSeen[i].Add(new Dictionary<int, bool>());
		}
	}

	public BattleTrainer(string name, int trainerType) {
		this.name = name;
		this.trainerType = trainerType;
		language = Utilities.GetLanguage();
		id = (int)Utilities.Rand(256);
		id |= ((int)Utilities.Rand(256))<<8;
		id |= ((int)Utilities.Rand(256))<<16;
		id |= ((int)Utilities.Rand(256))<<24;
		metaID = 0;
		outfit = 0;
		pokegear = false;
		pokedex = false;
		ClearPokedex();
		shadowCaught = new Dictionary<int, bool>();
		for (int i=0; i<Species.MaxValue(); i++) {
			shadowCaught[i] = false;
		}
		badges = new Dictionary<int, bool>();
		for (int i=0; i<8; i++) {
			badges[i] = false;
		}
		money = Settings.INITIAL_MONEY;
		party = new Pokemon[0];
	}
}
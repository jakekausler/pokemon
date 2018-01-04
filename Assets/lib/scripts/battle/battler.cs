using UnityEngine;
using System.Collections.Generic;
using System;

public class Battler : MonoBehaviour {
	public Battle battle;
	public Pokemon pokemon;
	private string _name;
	public new string name {
		get {
			if (effects[Effects.Illusion] != 0) {
				return illusionPokemon.name;
			}
			return _name;
		}
		set { _name = value; }
	}
	public int index;
	public int pokemonIndex;
	public int totalHP;
	private int _hp;
	public int hp {
		get { return _hp; }
		set {
			_hp = (int)value;
			if (pokemon != null) {
				pokemon.hp = (int)value;
			}
		}
	}
	public bool fainted;
	public List<int> lastAttacker;
	public int turnCount;
	public Dictionary<int, int> effects; // Value of 0 is false
	public Pokemon illusionPokemon;
	public int species;
	public int type1;
	public int type2;
	public int ability;
	private int _gender;
	public int gender {
		get {
			if (effects[Effects.Illusion] != 0) {
				return illusionPokemon.Gender();
			}
			return _gender;
		}
		set {
			_gender = value;
		}
	}
	public int attack;
	private int _defense;
	public int defense {
		get {
			return battle.field.effects[Effects.WonderRoom] > 0 ? _specialDefense : _defense;
		}
		set { _defense = value; }
	}
	public int specialAttack;
	private int _specialDefense;
	public int specialDefense {
		get {
			return battle.field.effects[Effects.WonderRoom] > 0 ? _defense : _specialDefense;
		}
		set { _specialDefense = value; }
	}
	public int _speed;
	public int speed {
		get {
			float[] stageMul = {10f, 10f, 10f, 10f, 10f, 10f, 10f, 15f, 20f, 25f, 30f, 35f, 40f};
			float[] stageDiv = {40f, 35f, 30f, 25f, 20f, 15f, 10f, 10f, 10f, 10f, 10f, 10f, 10f};
			int spd = speed;
			int stg = stages[Stats.SPEED] + 6;
			spd = (int)(spd * stageMul[stg] / stageDiv[stg]);
			int spdMult = 0x1000;
			switch (battle.GetWeather()) {
				case Weather.RAINDANCE:
				case Weather.HEAVYRAIN:
				if (HasWorkingAbility(Abilities.SWIFTSWIM)) {
					spdMult = spdMult * 2;
				}
				break;
				case Weather.SUNNYDAY:
				case Weather.HARSHSUN:
				if (HasWorkingAbility(Abilities.CHLOROPHYLL)) {
					spdMult = spdMult * 2;
				}
				break;
				case Weather.SANDSTORM:
				if (HasWorkingAbility(Abilities.SANDRUSH)) {
					spdMult = spdMult * 2;
				}
				break;
			}
			if (HasWorkingAbility(Abilities.QUICKFEET) && status > 0) {
				spdMult = (int)(Math.Round(spdMult * 1.5f));
			}
			if (HasWorkingAbility(Abilities.UNBURDEN) && effects[Effects.Unburden] != 0 && item==0) {
				spdMult = spdMult * 2;
			}
			if (HasWorkingAbility(Abilities.SLOWSTART) && turnCount <= 5) {
				spdMult = (int)Math.Round(spdMult / 2f);
			}
			if (HasWorkingItem(Items.MACHOBRACE) || HasWorkingItem(Items.POWERWEIGHT) || HasWorkingItem(Items.POWERBRACER) || HasWorkingItem(Items.POWERBELT) || HasWorkingItem(Items.POWERANKLET) || HasWorkingItem(Items.POWERLENS) || HasWorkingItem(Items.POWERBAND)) {
				spdMult = (int)Math.Round(spdMult / 2f);
			}
			if (HasWorkingItem(Items.CHOICESCARF)) {
				spdMult = (int)(Math.Round(spdMult * 1.5f));
			}
			if (item == Items.IRONBALL) {
				spdMult = (int)Math.Round(spdMult / 2f);
			}
			if (HasWorkingItem(Items.QUICKPOWDER) && species == Species.DITTO && effects[Effects.Transform] != 0) {
				spdMult = spdMult * 2;
			}
			if (OwnSide().effects[Effects.Tailwind] > 0) {
				spdMult = spdMult * 2;
			}
			if (OwnSide().effects[Effects.Swamp] > 0) {
				spdMult = (int)Math.Round(spdMult / 2f);
			}
			if (status == Statuses.PARALYSIS && !HasWorkingAbility(Abilities.QUICKFEET)) {
				spdMult = (int)Math.Round(spdMult / 4f);
			}
			if (battle.internalbattle && battle.OwnedByPlayer(index) && battle.Player().NumBadges() >= Settings.BADGES_BOOST_SPEED) {
				spdMult = (int)Math.Round(spdMult * 1.1f);
			}
			speed = (int)Math.Round(speed * spdMult * 1.0f/0x1000);
			return Math.Max(spd, 1);
		}
		set {
			_speed = value;
		}
	}
	public Dictionary<int, int> stages; // Stat changes
	public int[] iv;
	public BattleMove[] moves;
	public List<int> participants;
	public bool tookDamage;
	public int lastHPLost;
	public int lastMoveUsed;
	public int lastMoveUsedType;
	public int lastMoveUsedSketch;
	public int lastRegularMoveUsed;
	public int lastRoundMoved;
	public List<int> movesUsed;
	public int currentMove;
	public DamageState damageState;
	public bool captured;
	private int _form;
	public int form {
		get { return _form; }
		set {
			_form = value;
			if (pokemon != null) {
				pokemon.SetForm(value);
			}
		}
	}
	public int happiness {
		get {
			return pokemon == null ? 0 : pokemon.happiness;
		}
		private set {}
	}
	public int nature {
		get {
			return pokemon == null ? 0 : pokemon.Nature();
		}
		private set{}
	}
	public int pokerusStage {
		get {
			return pokemon == null ? 0 : pokemon.PokerusStage();
		}
		private set{}
	}
	public bool hasMega {
		get {
			if (effects[Effects.Transform] != 0) {
				return false;
			}
			if (pokemon != null) {
				try {
					return pokemon.HasMegaForm();
				} catch (Exception e) {
					Debug.Log(string.Format("Error getting mega form. Exception: ",e.Message));
					return false;
				}
			}
			return false;
		}
		private set{}
	}
	public bool isMega {
		get {
			if (pokemon != null) {
				try {
					return pokemon.IsMega();
				} catch (Exception e) {
					Debug.Log(string.Format("Error getting mega form. Exception: ",e.Message));
					return false;
				}
			}
			return false;
		}
		private set{}
	}
	public bool hasPrimal {
		get {
			if (effects[Effects.Transform] != 0) {
				return false;
			}
			if (pokemon != null) {
				try {
					return pokemon.HasPrimalForm();
				} catch (Exception e) {
					Debug.Log(string.Format("Error getting primal form. Exception: ",e.Message));
					return false;
				}
			}
			return false;
		}
		private set{}
	}
	public bool isPrimal {
		get {
			if (pokemon != null) {
				try {
					return pokemon.IsPrimal();
				} catch (Exception e) {
					Debug.Log(string.Format("Error getting primal form. Exception: ",e.Message));
					return false;
				}
			}
			return false;
		}
		private set{}
	}
	private int _level;
	public int level {
		get { return _level; }
		set {
			_level = value;
			pokemon.Level(value);
		}
	}
	private int _statusCount;
	public int statusCount {
		get { return _statusCount; }
		set {
			_statusCount = value;
			if (pokemon != null) {
				pokemon.statusCount = 0;
			}
		}
	}
	private int _status;
	public int status {
		get { return _status; }
		set {
			if (_status == Statuses.SLEEP && value == 0) {
				effects[Effects.Truant] = 0;
			}
			_status = value;
			if (pokemon != null) {
				pokemon.status = value;
			}
			if (value != Statuses.POISON) {
				effects[Effects.Toxic] = 0;
			}
			if (value != Statuses.POISON && value != Statuses.SLEEP) {
				statusCount = 0;
			}
		}
	}
	private int _item;
	public int item {
		get { return _item; }
		set {
			_item = value;
			if (pokemon != null) {
				pokemon.item = value;
			}
		}
	}
	public int weight(Battler attacker=null) {
		float w = pokemon != null ? pokemon.Weight() : 500.0f;
		if (attacker == null || !attacker.HasMoldBreaker()) {
			if (HasWorkingAbility(Abilities.HEAVYMETAL)) {
				w *= 2;
			}
			if (HasWorkingAbility(Abilities.LIGHTMETAL)) {
				w /= 2;
			}
		}
		if (HasWorkingItem(Items.FLOATSTONE)) {
			w /= 2;
		}
		w += effects[Effects.WeightChange];
		int wInt = (int)w;
		if (wInt < 1) {
			wInt = 1;
		}
		return wInt;
	}
	private bool _shiny;
	public bool shiny {
		get {
			if (effects[Effects.Illusion] != 0) {
				return illusionPokemon.IsShiny();
			}
			return _shiny;
		}
		set { _shiny = value; }
	}

	/***************
	* Initializers *
	***************/

	public Battler(Battle btl, int idx) {
		battle = btl;
		index = idx;
		hp = 0;
		totalHP = 0;
		fainted = true;
		captured = false;
		stages = new Dictionary<int, int>();
		effects = new Dictionary<int, int>();
		damageState = new DamageState();
		InitBlank();
		InitEffects(false);
		InitPermanantEffects();
	}

	public void InitPokemon(Pokemon pkmn, int idx) {
		if (pkmn.Egg()) {
			throw new Exception("An egg can't be an active PokÃ©mon");
		}
		name = pkmn.name;
		species = pkmn.species;
		level = pkmn.Level();
		hp = pkmn.hp;
		totalHP = pkmn.totalHP;
		gender = pkmn.Gender();
		ability = pkmn.Ability();
		item = pkmn.item;
		type1 = pkmn.Type1();
		type2 = pkmn.Type2();
		form = pkmn.GetForm();
		attack = pkmn.attack;
		defense = pkmn.defense;
		speed = pkmn.speed;
		specialAttack = pkmn.spatk;
		specialDefense = pkmn.spdef;
		status = pkmn.status;
		statusCount = pkmn.statusCount;
		pokemon = pkmn;
		pokemonIndex = idx;
		participants = new List<int>();
		moves = new BattleMove[4] {
			BattleMove.FromBattleMove(battle, pkmn.moves[0]),
			BattleMove.FromBattleMove(battle, pkmn.moves[1]),
			BattleMove.FromBattleMove(battle, pkmn.moves[2]),
			BattleMove.FromBattleMove(battle, pkmn.moves[3])
		};
		iv = new int[6] {
			pkmn.iv[0],
			pkmn.iv[1],
			pkmn.iv[2],
			pkmn.iv[3],
			pkmn.iv[4],
			pkmn.iv[5]
		};
	}

	public void InitDummyPokemon(Pokemon pkmn, int idx) {
		if (pkmn.Egg()) {
			throw new Exception("An egg can't be an active PokÃ©mon");
		}
		name = pkmn.name;
		species = pkmn.species;
		level = pkmn.Level();
		hp = pkmn.hp;
		totalHP = pkmn.totalHP;
		gender = pkmn.Gender();
		ability = pkmn.Ability();
		item = pkmn.item;
		type1 = pkmn.Type1();
		type2 = pkmn.Type2();
		form = pkmn.GetForm();
		attack = pkmn.attack;
		defense = pkmn.defense;
		speed = pkmn.speed;
		specialAttack = pkmn.spatk;
		specialDefense = pkmn.spdef;
		status = pkmn.status;
		statusCount = pkmn.statusCount;
		pokemon = pkmn;
		pokemonIndex = idx;
		participants = new List<int>();
		moves = new BattleMove[4] {null, null, null, null};
		iv = new int[6] {
			pkmn.iv[0],
			pkmn.iv[1],
			pkmn.iv[2],
			pkmn.iv[3],
			pkmn.iv[4],
			pkmn.iv[5]
		};
	}

	public void InitBlank() {
		name = "";
		species = 0;
		level = 0;
		hp = 0;
		totalHP = 0;
		gender = 0;
		ability = 0;
		item = 0;
		type1 = 0;
		type2 = 0;
		form = 0;
		attack = 0;
		defense = 0;
		speed = 0;
		specialAttack = 0;
		specialDefense = 0;
		status = 0;
		statusCount = 0;
		pokemon = null;
		pokemonIndex = -1;
		participants = new List<int>();
		moves = new BattleMove[4] {null, null, null, null};
		iv = new int[6] {0, 0, 0, 0, 0, 0};
	}

	public void InitPermanantEffects() {
		effects[Effects.FutureSight] = 0;
		effects[Effects.FutureSightMove] = 0;
		effects[Effects.FutureSightUser] = -1;
		effects[Effects.FutureSightUserPos] = -1;
		effects[Effects.HealingWish] = 0;
		effects[Effects.LunarDance] = 0;
		effects[Effects.Wish] = 0;
		effects[Effects.WishAmount] = 0;
		effects[Effects.WishMaker] = -1;
	}

	public void InitEffects(bool batonpass) {
		if (!batonpass) {
			// These effects are retained if baton pass is used
			stages[Stats.ATTACK] = 0;
			stages[Stats.DEFENSE] = 0;
			stages[Stats.SPEED] = 0;
			stages[Stats.SPATK] = 0;
			stages[Stats.SPDEF] = 0;
			stages[Stats.EVASION] = 0;
			stages[Stats.ACCURACY] = 0;
			lastMoveUsedSketch = -1;
			effects[Effects.AquaRing] = 0;
			effects[Effects.Confusion] = 0;
			effects[Effects.Curse] = 0;
			effects[Effects.Embargo] = 0;
			effects[Effects.FocusEnergy] = 0;
			effects[Effects.GastroAcid] = 0;
			effects[Effects.HealBlock] = 0;
			effects[Effects.Ingrain] = 0;
			effects[Effects.LeechSeed] = 0;
			effects[Effects.LockOn] = 0;
			effects[Effects.LockOnPos] = 0;
			for (int i=0; i<=4; i++) {
				if (battle.battlers[i] != null && battle.battlers[i].effects[Effects.LockOnPos] == index && battle.battlers[i].effects[Effects.LockOn] > 0) {
					battle.battlers[i].effects[Effects.LockOn] = 0;
					battle.battlers[i].effects[Effects.LockOnPos] = -1;
				}
			}
			effects[Effects.MagnetRise] = 0;
			effects[Effects.PerishSong] = 0;
			effects[Effects.PerishSongUser] = -1;
			effects[Effects.PowerTrick] = 0;
			effects[Effects.Substitute] = 0;
			effects[Effects.Telekinesis] = 0;
		} else {
			if (effects[Effects.LockOn] > 0) {
				effects[Effects.LockOn] = 2;
			} else {
				effects[Effects.LockOn] = 0;
			}
			if (effects[Effects.PowerTrick] != 0) {
				attack = defense;
				defense = attack;
			}
		}
		damageState.Reset();
		fainted = false;
		lastAttacker = new List<int>();
		lastHPLost = 0;
		tookDamage = false;
		lastMoveUsed = -1;
		lastMoveUsedType = -1;
		lastRoundMoved = -1;
		movesUsed = new List<int>();
		turnCount = 0;
		effects[Effects.Attract] = -1;
		for (int i=0; i<=4; i++) {
			if (battle.battlers[i] != null && battle.battlers[i].effects[Effects.Attract] == index) {
				battle.battlers[i].effects[Effects.Attract] = -1;
			}
		}
		effects[Effects.BatonPass] = 0;
		effects[Effects.Bide] = 0;
		effects[Effects.BideDamage] = 0;
		effects[Effects.BideTarget] = -1;
		effects[Effects.Charge] = 0;
		effects[Effects.ChoiceBand] = -1;
		effects[Effects.Counter] = -1;
		effects[Effects.CounterTarget] = -1;
		effects[Effects.DefenseCurl] = 0;
		effects[Effects.DestinyBond] = 0;
		effects[Effects.Disable] = 0;
		effects[Effects.DisableMove] = 0;
		effects[Effects.Electrify] = 0;
		effects[Effects.Encore] = 0;
		effects[Effects.EncoreIndex] = 0;
		effects[Effects.EncoreMove] = 0;
		effects[Effects.Endure] = 0;
		effects[Effects.FirstPledge] = 0;
		effects[Effects.FlashFire] = 0;
		effects[Effects.Flinch] = 0;
		effects[Effects.FollowMe] = 0;
		effects[Effects.Foresight] = 0;
		effects[Effects.FuryCutter] = 0;
		effects[Effects.Grudge] = 0;
		effects[Effects.HelpingHand] = 0;
		effects[Effects.HyperBeam] = 0;
		effects[Effects.Illusion] = 0;
		if (HasWorkingAbility(Abilities.ILLUSION)) {
			int lastPkmn = battle.GetLastPokemonInTeam(index);
			if (lastPkmn != index) {
				effects[Effects.Illusion] = 1;
				illusionPokemon = battle.Party(index)[lastPkmn].pokemon;
			}
		}
		effects[Effects.Imprison] = 0;
		effects[Effects.KingsShield] = 0;
		effects[Effects.LifeOrb] = 0;
		effects[Effects.MagicCoat] = 0;
		effects[Effects.MeanLook] = -1;
		for (int i=0; i<=4; i++) {
			if (battle.battlers[i] != null && battle.battlers[i].effects[Effects.MeanLook] == index) {
				battle.battlers[i].effects[Effects.MeanLook] = -1;
			}
		}
		effects[Effects.MeFirst] = 0;
		effects[Effects.Metronome] = 0;
		effects[Effects.MicleBerry] = 0;
		effects[Effects.Minimize] = 0;
		effects[Effects.MiracleEye] = 0;
		effects[Effects.MirrorCoat] = -1;
		effects[Effects.MirrorCoatTarget] = -1;
		effects[Effects.MoveNext] = 0;
		effects[Effects.MudSport] = 0;
		effects[Effects.MultiTurn] = 0;
		effects[Effects.MultiTurnAttack] = 0;
		effects[Effects.MultiTurnUser] = -1;
		for (int i=0; i<=4; i++) {
			if (battle.battlers[i] != null && battle.battlers[i].effects[Effects.MultiTurnUser] == index) {
				battle.battlers[i].effects[Effects.MultiTurn] = 0;
				battle.battlers[i].effects[Effects.MultiTurnUser] = -1;
			}
		}
		effects[Effects.Nightmare] = 0;
		effects[Effects.Outrage] = 0;
		effects[Effects.ParentalBond] = 0;
		effects[Effects.PickupItem] = 0;
		effects[Effects.PickupUse] = 0;
		effects[Effects.Pinch] = 0;
		effects[Effects.Powder] = 0;
		effects[Effects.Protect] = 1;
		effects[Effects.ProtectNegation] = 0;
		effects[Effects.ProtectRate] = 0;
		effects[Effects.Pursuit] = 0;
		effects[Effects.Quash] = 0;
		effects[Effects.Rage] = 0;
		effects[Effects.Revenge] = 0;
		effects[Effects.Roar] = 0;
		effects[Effects.Rollout] = 0;
		effects[Effects.Roost] = 0;
		effects[Effects.SkipTurn] = 0;
		effects[Effects.SkyDrop] = 0;
		effects[Effects.SmackDown] = 0;
		effects[Effects.Snatch] = 0;
		effects[Effects.SpikyShield] = 0;
		effects[Effects.Stockpile] = 0;
		effects[Effects.StockpileDef] = 0;
		effects[Effects.StockpileSpDef] = 0;
		effects[Effects.Taunt] = 0;
		effects[Effects.Torment] = 0;
		effects[Effects.Toxic] = 0;
		effects[Effects.Transform] = 0;
		effects[Effects.Truant] = 0;
		effects[Effects.TwoTurnAttack] = 0;
		effects[Effects.Type3] = -1;
		effects[Effects.Unburden] = 0;
		effects[Effects.Uproar] = 0;
		effects[Effects.Uturn] = 0;
		effects[Effects.WaterSport] = 0;
		effects[Effects.WeightChange] = 0;
		effects[Effects.Yawn] = 0;
	}

	public void Update(bool fullChange=false) {
		if (pokemon != null) {
			pokemon.CalcStats();
			level = pokemon.Level();
			hp = pokemon.hp;
			totalHP = pokemon.totalHP;
			if (effects[Effects.Transform] != 0) {
				attack = pokemon.attack;
				defense = pokemon.defense;
				speed = pokemon.speed;
				specialAttack = pokemon.spatk;
				specialDefense = pokemon.spdef;
				if (fullChange) {
					ability = pokemon.Ability();
					type1 = pokemon.Type1();
					type2 = pokemon.Type2();
				}
			}
		}
	}

	public void InitBattle(Pokemon pkmn, int idx, bool batonpass) {
		if (HasWorkingAbility(Abilities.NATURALCURE)) {
			status = 0;
		}
		if (HasWorkingAbility(Abilities.REGENERATOR)) {
			RecoverHP((int)(totalHP/3.0f), true);
		}
		InitPokemon(pkmn, idx);
		InitEffects(batonpass);
	}

	public void Reset() {
		pokemon = null;
		pokemonIndex = -1;
		hp = 0;
		InitEffects(false);
		status = 0;
		statusCount = 0;
		fainted = true;
		battle.useMoveChoice[index] = 0;
		battle.indexChoice[index] = 0;
		battle.moveChoice[index] = null;
		battle.targetChoice[index] = -1;
	}

	// Update PokÃ©mon who will gain EXP if this battler is defeated
	public void UpdateParticipants() {
		if (fainted) {
			// Can't update if already fainted
			return;
		}
		if (IsOpposing(index)) {
			bool found1 = false;
			bool found2 = false;
			for (int i=0; i < participants.Count; i++) {
				if (i == Opposing1().pokemonIndex) {
					found1 = true;
				}
				if (i == Opposing2().pokemonIndex) {
					found2 = true;
				}
			}
			if (!found1 && !Opposing1().fainted) {
				participants[participants.Count - 1] = Opposing1().pokemonIndex;
			}
			if (!found2 && !Opposing2().fainted) {
				participants[participants.Count - 1] = Opposing2().pokemonIndex;
			}
		}
	}

	/*********************
	* About this Battler *
	*********************/
	public bool Fainted() {
		return hp <= 0;
	}

	public string String(bool lowercase=false) {
		if (IsOpposing(index)) {
			if (battle.opponent != null) {
				return lowercase ?
				string.Format("the opposing {0}", name) :
				string.Format("The opposing {0}", name);
			} else {
				return lowercase ?
				string.Format("the wild {0}", name) :
				string.Format("The wild {0}", name);
			}
		} else if (battle.OwnedByPlayer(index)) {
			return name;
		} else {
			return lowercase ?
			string.Format("the ally {0}", name) :
			string.Format("The ally {0}", name);
		}
	}

	public bool HasType(int type) {
		if (type < 0) {
			return false;
		}
		bool ret = type1 == type || type2 == type;
		if (effects[Effects.Type3] >= 0) {
			ret |= effects[Effects.Type3] == type;
		}
		return ret;
	}

	public bool HasMove(int idx) {
		if (idx <= 0) {
			return false;
		}
		for (int i=0; i < moves.Length; i++) {
			if (moves[i].id == idx) {
				return true;
			}
		}
		return false;
	}

	public bool HasMoveType(int type) {
		if (type < 0) {
			return false;
		}
		for (int i=0; i < moves.Length; i++) {
			if (moves[i].type == type) {
				return true;
			}
		}
		return false;
	}

	public bool HasMovedThisRound() {
		return lastRoundMoved < 0 || lastRoundMoved == battle.turnCount;
	}

	public bool HasMoldBreaker() {
		return HasWorkingAbility(Abilities.MOLDBREAKER) || HasWorkingAbility(Abilities.TERAVOLT) || HasWorkingAbility(Abilities.TURBOBLAZE);
	}

	public bool HasWorkingAbility(int abl, bool ignoreFainted=false) {
		if ((fainted && !ignoreFainted) || effects[Effects.GastroAcid] != 0) {
			return false;
		}
		return ability == abl;
	}

	public bool HasWorkingItem(int itm, bool ignoreFainted=false) {
		if ((fainted && !ignoreFainted) || effects[Effects.Embargo] > 0 || effects[Effects.MagicRoom] > 0 || HasWorkingAbility(Abilities.KLUTZ, ignoreFainted)) {
			return false;
		}
		return item == itm;
	}

	public bool HasWorkingBerry(bool ignoreFainted=false) {
		if ((fainted && !ignoreFainted) || effects[Effects.Embargo] > 0 || effects[Effects.MagicRoom] > 0 || HasWorkingAbility(Abilities.KLUTZ, ignoreFainted)) {
			return false;
		}
		return Items.IsBerry(item);
	}

	public bool IsAirborne(bool ignoreAbility=false) {
		if (HasWorkingItem(Items.IRONBALL) || effects[Effects.Ingrain] != 0 || effects[Effects.SmackDown] != 0 || battle.field.effects[Effects.Gravity] > 0) {
			return false;
		}
		if ((HasType(Types.FLYING) && effects[Effects.Roost] == 0) || (HasWorkingAbility(Abilities.LEVITATE) && !ignoreAbility) || HasWorkingItem(Items.AIRBALLOON) || effects[Effects.MagnetRise] > 0 || effects[Effects.Telekinesis] > 0) {
			return true;
		}
		return false;
	}

	/************
	* Change HP *
	************/
	public int ReduceHP(int amt, bool anim=false, bool registerDamage=true) {
		if (amt > hp) {
			amt = hp;
		} else if (amt < 1 && !fainted) {
			amt = 1;
		}
		int oldhp = hp;
		hp -= amt;
		if (hp < 0) {
			throw new Exception("HP less than 0");
		}
		if (hp > totalHP) {
			throw new Exception("HP greater than total HP");
		}
		if (amt > 0) {
			battle.scene.HPChanged(this, oldhp, anim);
		}
		if (amt > 0 && registerDamage) {
			tookDamage = true;
		}
		return amt;
	}

	public int RecoverHP(int amt, bool anim) {
		if (hp + amt > totalHP) {
			amt = totalHP - hp;
		} else if (amt < 1 && hp != totalHP) {
			amt = 1;
		}
		int oldhp = hp;
		hp += amt;
		if (hp < 0) {
			throw new Exception("HP less than 0");
		}
		if (hp > totalHP) {
			throw new Exception("HP greater than total HP");
		}
		if (amt > 0) {
			battle.scene.HPChanged(this, oldhp, anim);
		}
		return amt;
	}

	public bool Faint(bool showMessages=true) {
		if (fainted) {
			Debug.Log("!!!***Can't faint if already fainted");
			return true;
		}
		battle.scene.Fainted(this);
		InitEffects(false);
		status = 0;
		statusCount = 0;
		if (pokemon != null && battle.internalbattle) {
			pokemon.ChangeHappiness("faint");
		}
		if (isMega) {
			pokemon.MakeUnmega();
		}
		if (isPrimal) {
			pokemon.MakeUnprimal();
		}
		fainted = true;
		battle.useMoveChoice[index] = 0;
		battle.indexChoice[index] = 0;
		battle.moveChoice[index] = null;
		battle.targetChoice[index] = -1;
		OwnSide().effects[Effects.LastRoundFainted] = battle.turnCount;
		if (showMessages) {
			battle.DisplayPaused(string.Format("{0} fainted!", String()));
		}
		Debug.Log(string.Format("[PokÃ©mon fainted] {0}", String()));
		return true;
	}

	/********************************************************
	* Find other battlers/sides in relation to this battler *
	********************************************************/
	// Returns the data structure for this battler's side
	public ActiveSide OwnSide() {
		return battle.sides[index & 1]; // Player: 0/2, Opponent: 1/3
	}

	// Returns the data structure for this opponent's side
	public ActiveSide OpposingSide() {
		return battle.sides[(index & 1) ^ 1]; // Opponent: 0/2, Player: 1/3
	}

	// Returns whether the position belongs to the opponent's side
	public bool IsOpposing(int i) {
		return (index & 1) == (i & 1);
	}

	// Returns the battler's partner
	public Battler Partner() {
		return battle.battlers[(index & 1) | ((index & 2) ^ 2)];
	}

	// Returns the battler's first opposing PokÃ©mon
	public Battler Opposing1() {
		return battle.battlers[((index & 1) ^ 1)];
	}

	// Returns the battler's second opposing PokÃ©mon
	public Battler Opposing2() {
		return battle.battlers[((index & 1) ^ 1) + 2];
	}

	public Battler OppositeOpposing() {
		return battle.battlers[index ^ 1];
	}

	public Battler OppositeOpposing2() {
		return battle.battlers[(index ^ 1) | ((index & 2) ^ 2)];
	}

	public int NonActivePokemonCount() {
		int count = 0;
		Battler[] party = battle.Party(index);
		for (int i=0; i < party.Length; i++) {
			if ((fainted || i != pokemonIndex) && (Partner().fainted || i != Partner().pokemonIndex) && (party[i] != null && !party[i].pokemon.Egg() && party[i].hp > 0)) {
				count += 1;
			}
		}
		return count;
	}

	/********
	* Forms *
	********/
	public void CheckForm() {
		if (effects[Effects.Transform] != 0) {
			return;
		}
		if (fainted) {
			return;
		}
		bool transformed = false;
		// Forecast
		if (species == Species.CASTFORM) {
			if (HasWorkingAbility(Abilities.FORECAST)) {
				switch (battle.weather) {
					case Weather.SUNNYDAY:
					case Weather.HARSHSUN:
					if (form != 1) {
						form = 1;
						transformed = true;
					}
					break;
					case Weather.RAINDANCE:
					case Weather.HEAVYRAIN:
					if (form != 2) {
						form = 2;
						transformed = true;
					}
					break;
					case Weather.HAIL:
					if (form != 3) {
						form = 3;
						transformed = true;
					}
					break;
					default:
					if (form != 0) {
						form = 0;
						transformed = true;
					}
					break;
				}
			} else {
				if (form != 0) {
					form = 0;
					transformed = true;
				}
			}
		}
		// Cherrim
		if (species == Species.CHERRIM) {
			if (HasWorkingAbility(Abilities.FLOWERGIFT) && (
				battle.weather == Weather.SUNNYDAY || battle.weather == Weather.HARSHSUN)) {
				if (form != 1) {
					form = 1;
					transformed = true;
				}
			} else {
				if (form != 0) {
					form = 0;
					transformed = true;
				}
			}
		}
		// Shaymin, Giratina, Arceus, Keldeo, Genesect
		if (species == Species.SHAYMIN || species == Species.GIRATINA || species == Species.ARCEUS || species == Species.KELDEO || species == Species.GENESECT) {
			if (form != pokemon.GetForm()) {
				form = pokemon.GetForm();
				transformed = true;
			}
		}
		// Zen Mode
		if (species == Species.DARMANITAN) {
			if (HasWorkingAbility(Abilities.ZENMODE) && hp <= ((int)(totalHP/2))) {
				if (form != 1) {
					form = 1;
					transformed = true;
				}
			} else {
				if (form != 0) {
					form = 0;
					transformed = true;
				}
			}
		}
		if (species == Species.KELDEO) {
			if (form != pokemon.GetForm()) {
				form = pokemon.GetForm();
				transformed = true;
			}
		}
		if (species == Species.GENESECT) {
			if (form != pokemon.GetForm()) {
				form = pokemon.GetForm();
				transformed = true;
			}
		}
		if (transformed) {
			Update(true);
			battle.scene.ChangePokemon(this, pokemon);
			battle.Display(string.Format("{0} transformed!",String()));
			Debug.Log(string.Format("[Form changed] {0} changed to form {1}",String(), form));
		}
	}

	public void ResetForm() {
		if (effects[Effects.Transform] != 0) {
			if (species == Species.CASTFORM || species == Species.CHERRIM || species == Species.DARMANITAN || species == Species.MELOETTA || species == Species.AEGISLASH || species == Species.XERNEAS) {
				form = 0;
			}
		}
		Update();
	}

	/******************
	* Ability Effects *
	******************/
	public void AbilitiesOnSwitchIn(bool onActive) {
		if (fainted) {
			return;
		}
		if (onActive) {
			battle.PrimalReversion(index);
		}
		// Weather
		if (onActive) {
			if (HasWorkingAbility(Abilities.PRIMORDIALSEA) && battle.weather != Weather.HEAVYRAIN) {
				battle.weather = Weather.HEAVYRAIN;
				battle.weatherduration = -1;
				battle.CommonAnimation("HeavyRain", null, null);
				battle.Display(string.Format("{0}'s {1} made a heavy rain begin to fall!", String(), Abilities.GetName(ability)));
				Debug.Log(string.Format("[Ability triggered] {0}'s {1} made a heavy rain begin to fall!", String(), Abilities.GetName(ability)));
			}
			if (HasWorkingAbility(Abilities.DESOLATELAND) && battle.weather != Weather.HARSHSUN) {
				battle.weather = Weather.HARSHSUN;
				battle.weatherduration = -1;
				battle.CommonAnimation("HarshSun", null, null);
				battle.Display(string.Format("{0}'s {1} turned the sunlight extremely harsh!", String(), Abilities.GetName(ability)));
				Debug.Log(string.Format("[Ability triggered] {0}'s {1} turned the sunlight extremely harsh!", String(), Abilities.GetName(ability)));
			}
			if (HasWorkingAbility(Abilities.DELTASTREAM) && battle.weather != Weather.STRONGWINDS) {
				battle.weather = Weather.STRONGWINDS;
				battle.weatherduration = -1;
				battle.CommonAnimation("StrongWinds", null, null);
				battle.Display(string.Format("{0}'s {1} caused a mysterious air current that protects Flying-type PokÃ©mon!", String(), Abilities.GetName(ability)));
				Debug.Log(string.Format("[Ability triggered] {0}'s {1} caused a mysterious air current that protects Flying-type PokÃ©mon!", String(), Abilities.GetName(ability)));
			}
			if (battle.weather != Weather.HEAVYRAIN && battle.weather != Weather.HARSHSUN && battle.weather != Weather.STRONGWINDS) {
				if (HasWorkingAbility(Abilities.DRIZZLE) && (
					battle.weather != Weather.RAINDANCE || battle.weatherduration != -1)) {
					battle.weather = Weather.RAINDANCE;
					if (Settings.USE_NEW_BATTLE_MECHANICS) {
						battle.weatherduration = 5;
						if (HasWorkingItem(Items.DAMPROCK)) {
							battle.weatherduration = 8;
						}
					} else {
						battle.weatherduration = -1;
					}
					battle.CommonAnimation("Rain", null, null);
					battle.Display(string.Format("{0}'s {1} made it rain!", String(), Abilities.GetName(ability)));
					Debug.Log(string.Format("[Ability triggered] {0}'s {1} made it rain!", String(), Abilities.GetName(ability)));
				}
				if (HasWorkingAbility(Abilities.DROUGHT) && (
					battle.weather != Weather.SUNNYDAY || battle.weatherduration != -1)) {
					battle.weather = Weather.SUNNYDAY;
					if (Settings.USE_NEW_BATTLE_MECHANICS) {
						battle.weatherduration = 5;
						if (HasWorkingItem(Items.HEATROCK)) {
							battle.weatherduration = 8;
						}
					} else {
						battle.weatherduration = -1;
					}
					battle.CommonAnimation("Sunny", null, null);
					battle.Display(string.Format("{0}'s {1} intensified the sun's rays!", String(), Abilities.GetName(ability)));
					Debug.Log(string.Format("[Ability triggered] {0}'s {1} intensified the sun's rays!", String(), Abilities.GetName(ability)));
				}
				if (HasWorkingAbility(Abilities.SANDSTREAM) && (
					battle.weather != Weather.SANDSTORM || battle.weatherduration != -1)) {
					battle.weather = Weather.SANDSTORM;
					if (Settings.USE_NEW_BATTLE_MECHANICS) {
						battle.weatherduration = 5;
						if (HasWorkingItem(Items.SMOOTHROCK)) {
							battle.weatherduration = 8;
						}
					} else {
						battle.weatherduration = -1;
					}
					battle.CommonAnimation("Sandstorm", null, null);
					battle.Display(string.Format("{0}'s {1} whipped up a sandstorm!", String(), Abilities.GetName(ability)));
					Debug.Log(string.Format("[Ability triggered] {0}'s {1} whipped up a sandstorm!", String(), Abilities.GetName(ability)));
				}
				if (HasWorkingAbility(Abilities.SNOWWARNING) && (
					battle.weather != Weather.HAIL || battle.weatherduration != -1)) {
					battle.weather = Weather.HAIL;
					if (Settings.USE_NEW_BATTLE_MECHANICS) {
						battle.weatherduration = 5;
						if (HasWorkingItem(Items.ICYROCK)) {
							battle.weatherduration = 8;
						}
					} else {
						battle.weatherduration = -1;
					}
					battle.CommonAnimation("Hail", null, null);
					battle.Display(string.Format("{0}'s {1} made it hail!", String(), Abilities.GetName(ability)));
					Debug.Log(string.Format("[Ability triggered] {0}'s {1} made it hail!", String(), Abilities.GetName(ability)));
				}
				if (HasWorkingAbility(Abilities.CLOUDNINE) || HasWorkingAbility(Abilities.AIRLOCK)) {
					battle.Display(string.Format("{0} has {1}!", String(), Abilities.GetName(ability)));
					battle.Display(string.Format("The effects of the weather disappered."));
					Debug.Log(string.Format("[Ability triggered] {0}'s {1} mad the weather disappear!", String(), Abilities.GetName(ability)));
				}
			}
		}
		battle.PrimordialWeather();
		// Trace
		if (HasWorkingAbility(Abilities.TRACE)) {
			List<int> choices = new List<int>();
			for (int i=0; i < 5; i++) {
				Battler foe = battle.battlers[i];
				if (IsOpposing(i) && !foe.fainted) {
					int ability = foe.ability;
					if (ability > 0 && ability != Abilities.TRACE && ability != Abilities.MULTITYPE && ability != Abilities.ILLUSION && ability != Abilities.FLOWERGIFT && ability != Abilities.IMPOSTER && ability != Abilities.STANCECHANGE) {
						choices.Add(i);
					}
				}
			}
			if (choices.Count > 0) {
				int choice = choices[battle.Rand(choices.Count)];
				Battler battler = battle.battlers[choice];
				string battlerName = battler.String(true);
				string abilityName = Abilities.GetName(battler.ability);
				battle.Display(string.Format("{0} traces {1}'s {3}!", String(), battlerName, abilityName));
				Debug.Log(string.Format("[Ability triggered] {0} traces {1}'s {3}!", String(), battlerName, abilityName));
			}
		}
		// Intimidate
		if (HasWorkingAbility(Abilities.INTIMIDATE) && onActive) {
			Debug.Log(string.Format("[Ability triggered] {0}'s Intimidate!", String()));
			for (int i=0; i<4; i++) {
				if (IsOpposing(i) && !battle.battlers[i].fainted) {
					battle.battlers[i].ReduceStatWithIntimidate(this);
				}
			}
		}
		// Download
		if (HasWorkingAbility(Abilities.DOWNLOAD) && onActive) {
			int odef = 0;
			int ospdef = 0;
			if (Opposing1() != null && !Opposing1().fainted) {
				odef += Opposing1().defense;
				ospdef += Opposing1().specialDefense;
			}
			if (Opposing2() != null && !Opposing2().fainted) {
				odef += Opposing2().defense;
				ospdef += Opposing2().specialDefense;
			}
			if (ospdef > odef) {
				if (IncreaseStatWithCause(Stats.ATTACK, 1, this, Abilities.GetName(ability))) {
					Debug.Log(string.Format("[Ability triggered] {0}'s download (Raising attack)!", String()));
				}
			} else {
				if (IncreaseStatWithCause(Stats.SPATK, 1, this, Abilities.GetName(ability))) {
					Debug.Log(string.Format("[Ability triggered] {0}'s download (Raising special attack)!", String()));
				}
			}
		}
		// Frisk
		if (HasWorkingAbility(Abilities.FRISK) && battle.OwnedByPlayer(index) && onActive) {
			List<Battler> foes = new List<Battler>();
			if (Opposing1() != null && Opposing1().item > 0 && !Opposing1().fainted) {
				foes.Add(Opposing1());
			}
			if (Opposing2() != null && Opposing2().item > 0 && !Opposing2().fainted) {
				foes.Add(Opposing2());
			}
			if (Settings.USE_NEW_BATTLE_MECHANICS) {
				Debug.Log(string.Format("[Ability triggered] {0}'s frisk!", String()));
				for (int i=0; i<foes.Count; i++) {
					string itemName = Items.GetName(foes[i].item);
					battle.Display(string.Format("{0} frisked {1} and found one {2}!", String(), foes[i].String(true), itemName));
				}
			} else if (foes.Count > 0) {
				Debug.Log(string.Format("[Ability triggered] {0}'s frisk!", String()));
				Battler foe = foes[battle.Rand(foes.Count)];
				string itemName = Items.GetName(foe.item);
				battle.Display(string.Format("{0} frisked the foe and found one {1}!", String(), itemName));
			}
		}
		// Anticipation
		if (HasWorkingAbility(Abilities.ANTICIPATION) && battle.OwnedByPlayer(index) && onActive) {
			Debug.Log(string.Format("[Ability triggered] {0} has Anticipation!", String()));
			bool found = false;
			Battler[] foes = {Opposing1(), Opposing2()};
			for (int i=0; i<foes.Length; i++) {
				if (foes[i].Fainted()) {
					continue;
				}
				for (int j=0; j < foes[i].moves.Length; j++) {
					BattleMove moveData = BattleMove.FromBattleMove(battle, new Moves.Move(foes[i].moves[j].id));
					int eff = Types.GetCombinedEffectiveness(moveData.type, type1, type2, effects[Effects.Type3]);
					if ((moveData.baseDamage > 0 && eff > 8) || (moveData.function == 0x70 && eff > 0)) {
						found = true;
					}
					if (found) {
						break;
					}
				}
				if (found) {
					break;
				}
			}
			if (found) {
				battle.Display(string.Format("{0} shuddered with anticipation!", String()));
			}
		}
		// Forewarn
		if (HasWorkingAbility(Abilities.FOREWARN) && battle.OwnedByPlayer(index) && onActive) {
			Debug.Log(string.Format("[Ability triggered] {0} has Forewarn!", String()));
			int highPower = 0;
			List<int> fwMoves = new List<int>();
			Battler[] foes = {Opposing1(), Opposing2()};
			for (int i=0; i<foes.Length; i++) {
				if (foes[i].fainted) {
					continue;
				}
				for (int j=0; j<foes[i].moves.Length; j++) {
					BattleMove moveData = BattleMove.FromBattleMove(battle, new Moves.Move(foes[i].moves[j].id));
					int power = moveData.baseDamage;
					if (moveData.function == 0x70) { // OHKO
						power = 160;
					}
					if (moveData.function == 0x8B) { // Eruption
						power = 150;
					}
					if (moveData.function == 0x71 || moveData.function == 0x72 || moveData.function == 0x73) {
							power = 120;
						}
					if (moveData.function == 0x6A || moveData.function == 0x6B || moveData.function == 0x6D || moveData.function == 0x6E || moveData.function == 0x6F || moveData.function == 0x89 || moveData.function == 0x8A || moveData.function == 0x8C || moveData.function == 0x8D || moveData.function == 0x90 || moveData.function == 0x96 || moveData.function == 0x97 || moveData.function == 0x98 || moveData.function == 0x9A) {
							power = 80;
						}
						if (power > highPower) {
							fwMoves = new List<int>();
							fwMoves.Add(foes[i].moves[j].id);
							highPower = power;
						} else if (power == highPower) {
							fwMoves.Add(foes[i].moves[j].id);
						}
					}
				}
				if (fwMoves.Count > 0) {
					int fwMove = fwMoves[battle.Rand(fwMoves.Count)];
					string moveName = Moves.GetName(fwMove);
					battle.Display(string.Format("{0}'s Forewarn alerted it to {1}!", String(), moveName));
				}
			}
		// Pressure
			if (HasWorkingAbility(Abilities.PRESSURE) && onActive) {
				battle.Display(string.Format("{0} is exerting its pressure!", String()));
			}
		// Mold Breaker
			if (HasWorkingAbility(Abilities.MOLDBREAKER) && onActive) {
				battle.Display(string.Format("{0} breaks the mold!", String()));
			}
		// Turboblaze
			if (HasWorkingAbility(Abilities.TURBOBLAZE) && onActive) {
				battle.Display(string.Format("{0} is radiating a blazing aura!", String()));
			}
		// Teravolt
			if (HasWorkingAbility(Abilities.TERAVOLT) && onActive) {
				battle.Display(string.Format("{0} is radiating a bursting aura!", String()));
			}
		// Dark Aura
			if (HasWorkingAbility(Abilities.DARKAURA) && onActive) {
				battle.Display(string.Format("{0} is radiating a dark aura!", String()));
			}
		// Fairy Aura
			if (HasWorkingAbility(Abilities.FAIRYAURA) && onActive) {
				battle.Display(string.Format("{0} is radiating a fairy aura!", String()));
			}
		// Aura Break
			if (HasWorkingAbility(Abilities.AURABREAK) && onActive) {
				battle.Display(string.Format("{0} reversed all other PokÃ©mon's auras!", String()));
			}
		// Slow Start
			if (HasWorkingAbility(Abilities.SLOWSTART) && onActive) {
				battle.Display(string.Format("{0} can't get it going because of its {1}!", String(), Abilities.GetName(ability)));
			}
		// Imposter
			if (HasWorkingAbility(Abilities.IMPOSTER) && effects[Effects.Transform] != 0 && onActive) {
				Battler choice = OppositeOpposing();
				int[] blacklist = {
				0xC9, // Fly
				0xCA, // Dig
				0xCB, // Dive
				0xCC, // Bounce
				0xCD, // Shadow Force
				0xCE, // Sky Drop
				0x14D // Phantom Force
			};
			if (choice.effects[Effects.Transform] != 0 || choice.effects[Effects.Illusion] != 0 || choice.effects[Effects.Substitute] > 0 || choice.effects[Effects.SkyDrop] != 0 || Array.IndexOf(blacklist, BattleMove.FromBattleMove(battle, new Moves.Move(choice.effects[Effects.TwoTurnAttack])).function) > -1) {
				Debug.Log(string.Format("[Ability triggered] {0}'s Imposter couldn't transform", String()));
			} else {
				Debug.Log(string.Format("[Ability triggered] {0}'s Imposter", String()));
				battle.Animation(Moves.TRANSFORM, this, choice);
				effects[Effects.Transform] = 1;
				type1 = choice.type1;
				type2 = choice.type2;
				effects[Effects.Type3] = -1;
				ability = choice.ability;
				attack = choice.attack;
				defense = choice.defense;
				speed = choice.speed;
				specialAttack = choice.specialAttack;
				specialDefense = choice.specialDefense;
				stages[Stats.ATTACK] = choice.stages[Stats.ATTACK];
				stages[Stats.DEFENSE] = choice.stages[Stats.DEFENSE];
				stages[Stats.SPEED] = choice.stages[Stats.SPEED];
				stages[Stats.SPATK] = choice.stages[Stats.SPATK];
				stages[Stats.SPDEF] = choice.stages[Stats.SPDEF];
				for (int i=0; i<4; i++) {
					moves[i] = BattleMove.FromBattleMove(battle, new Moves.Move(choice.moves[i].id));
					moves[i].pp = 5;
					moves[i].totalPP = 5;
				}
				effects[Effects.Disable] = 0;
				effects[Effects.DisableMove] = 0;
				battle.Display(string.Format("{0} transformed into {1}", String(), choice.String(true)));
				Debug.Log(string.Format("[PokÃ©mon transformed] {0} transformed into {1}", String(), choice.String(true)));
			}
		}
		// Air Balloon
		if (HasWorkingItem(Items.AIRBALLOON) && onActive) {
			battle.Display(string.Format("{0} floats in the air with its {1}!", String(), Items.GetName(item)));
		}
	}

	public void EffectsOnDealingDamage(BattleMove move, Battler user, Battler target, int damage) {
		int moveType = move.GetType(move.type, user, target);
		if (damage > 0 && move.IsContactMove()) {
			if (!target.damageState.Substitute) {
				// Sticky Barb
				if (target.HasWorkingItem(Items.STICKYBARB, true) && user.item == 0 && target.effects[Effects.Unburden] != 0) {
					user.item = target.item;
					target.item = 0;
					target.effects[Effects.Unburden] = 1;
					if (battle.opponent == null && !battle.IsOpposing(user.index)) {
						if (user.pokemon.itemInitial == 0 && target.pokemon.itemInitial == user.item) {
							user.pokemon.itemInitial = user.item;
							target.pokemon.itemInitial = 0;
						}
					}
					battle.Display(string.Format("{0}'s {1} was transformed to {3}!", String(), Items.GetName(user.item), user.String(true)));
					Debug.Log(string.Format("[Item triggered] {0}'s {1} was transformed to {3}!", String(), Items.GetName(user.item), user.String(true)));
				}
				// Rocky Helmet
				if (target.HasWorkingAbility(Items.ROCKYHELMET, true) && !user.Fainted()) {
					if (!user.HasWorkingAbility(Abilities.MAGICGUARD)) {
						Debug.Log(string.Format("[Item Triggered] {0}'s Rocky Helmet", String()));
						battle.scene.DamageAnimation(user, 0);
						user.ReduceHP(user.totalHP/6);
						battle.Display(string.Format("{0} was hurt by the {1}!", String(), Items.GetName(target.item)));
					}
				}
				// Aftermath
				if (target.HasWorkingAbility(Abilities.AFTERMATH, true) && target.fainted && !user.Fainted()) {
					if (!battle.CheckGlobalAbility(Abilities.DAMP) && !user.HasMoldBreaker() && !user.HasWorkingAbility(Abilities.MAGICGUARD)) {
						Debug.Log(string.Format("[Ability Triggered] {0}'s Aftermath", target.String()));
						battle.scene.DamageAnimation(user, 0);
						user.ReduceHP(user.totalHP/4);
						battle.Display(string.Format("{0} was caught in the aftermath!", user.String()));
					}
				}
				// Cute Charm
				if (target.HasWorkingAbility(Abilities.CUTECHARM) && battle.Rand(10) < 3) {
					if (!user.Fainted() && user.CanAttract(target, false)) {
						Debug.Log(string.Format("[Ability Triggered] {0}'s Cute Charm", target.String()));
						user.Attract(target, string.Format("{0}'s {2} made {3} fall in love!", target.String(), Abilities.GetName(target.ability), user.String(true)));
					}
				}
				// Effect Spore
				if (target.HasWorkingAbility(Abilities.EFFECTSPORE, true) && battle.Rand(10) < 3) {
					if (!(Settings.USE_NEW_BATTLE_MECHANICS && (user.HasType(Types.GRASS) || user.HasWorkingAbility(Abilities.OVERCOAT) || user.HasWorkingItem(Items.SAFETYGOGGLES)))) {
						Debug.Log(string.Format("[Ability Triggered] {0}'s Effect Spore", target.String()));
						switch (battle.Rand(3)) {
							case 0:
							if (user.CanPoison(null, false)) {
								user.Poison(target, string.Format("{0}'s {2} poisoned {3}!", target.String(), Abilities.GetName(target.ability), user.String(true)));
							}
							break;
							case 1:
							if (user.CanSleep(null, false)) {
								user.Sleep(string.Format("{0}'s {2} made {3} fall asleep!", target.String(), Abilities.GetName(target.ability), user.String(true)));
							}
							break;
							case 2:
							if (user.CanParalyze(null, false)) {
								user.Paralyze(target, string.Format("{0}'s {2} paralyzed {3}! It may be unable to move!", target.String(), Abilities.GetName(target.ability), user.String(true)));
							}
							break;
						}
					}
				}
				// Flame Body
				if (target.HasWorkingAbility(Abilities.FLAMEBODY) && battle.Rand(10) < 3 && user.CanBurn(null, false)) {
					Debug.Log(string.Format("[Ability Triggered] {0}'s Flame Body", target.String()));
					user.Burn(target, string.Format("{0}'s {2} burned {3}!", target.String(), Abilities.GetName(target.ability), user.String(true)));
				}
				// Mummy
				if (target.HasWorkingAbility(Abilities.MUMMY) && !user.Fainted()) {
					if (user.ability != Abilities.MULTITYPE && user.ability != Abilities.STANCECHANGE && user.ability != Abilities.MUMMY) {
						Debug.Log(string.Format("[Ability Triggered] {0}'s Mummy copied onto {1}", target.String(), user.String(true)));
						user.ability = Abilities.MUMMY;
						battle.Display(string.Format("{0} was mummified by {1}!", user.String(), target.String(true)));
					}
				}
				// Poison Point
				// Poison Touch
				if ((
					target.HasWorkingAbility(Abilities.POISONPOINT) || target.HasWorkingAbility(Abilities.POISONTOUCH)) && battle.Rand(10) < 3 && user.CanPoison(null, false)) {
					Debug.Log(string.Format("[Ability Triggered] {0}'s {1}", target.String(), Abilities.GetName(target.ability)));
					user.Poison(target, string.Format("{0}'s {2} poisoned {3}!", target.String(), Abilities.GetName(target.ability), user.String(true)));
				}
				// Rough Skin
				// Iron Barbs
				if ((
					target.HasWorkingAbility(Abilities.ROUGHSKIN, true) || target.HasWorkingAbility(Abilities.IRONBARBS, true)) && ! user.Fainted()) {
					Debug.Log(string.Format("[Ability Triggered] {0}'s {1}", target.String(), Abilities.GetName(target.ability)));
					battle.scene.DamageAnimation(user, 0);
					user.ReduceHP(user.totalHP/8);
					battle.Display(string.Format("{0}'s {2} hurt {3}!", target.String(), Abilities.GetName(target.ability), user.String(true)));
				}
				// Static
				if (target.HasWorkingAbility(Abilities.STATIC) && battle.Rand(10) < 3 && user.CanParalyze(null, false)) {
					Debug.Log(string.Format("[Ability Triggered] {0}'s {1}", target.String(), Abilities.GetName(target.ability)));
					user.Paralyze(target, string.Format("{0}'s {2} paralyzed {3}! It may be unable to move!", target.String(), Abilities.GetName(target.ability), user.String(true)));
				}
				// Gooey
				if (target.HasWorkingAbility(Abilities.GOOEY)) {
					if (user.ReduceStatWithCause(Stats.SPEED, 1, target, Abilities.GetName(target.ability))) {
						Debug.Log(string.Format("[Ability Triggered] {0}'s {1}", target.String(), Abilities.GetName(target.ability)));
					}
				}
			}
		}
		if (damage > 0) {
			if (!target.damageState.Substitute) {
				// Cursed Body
				if (target.HasWorkingAbility(Abilities.CURSEDBODY, true) && battle.Rand(10) < 3 && user.effects[Effects.Disable] <= 0 && move.pp > 0 && !user.Fainted()) {
					user.effects[Effects.Disable] = 3;
					user.effects[Effects.DisableMove] = move.id;
					battle.Display(string.Format("{0}'s {2} disabled {3}! It may be unable to move!", target.String(), Abilities.GetName(target.ability), user.String(true)));
					Debug.Log(string.Format("[Ability Triggered] {0}'s {1}", target.String(), Abilities.GetName(target.ability)));
				}
				// Justified
				if (target.HasWorkingAbility(Abilities.JUSTIFIED) && moveType == Types.DARK) {
					if (target.IncreaseStatWithCause(Stats.ATTACK, 1, target, Abilities.GetName(target.ability))) {
						Debug.Log(string.Format("[Ability Triggered] {0}'s {1}", target.String(), Abilities.GetName(target.ability)));
					}
				}
				// Rattled
				if (target.HasWorkingAbility(Abilities.RATTLED) && (
					moveType == Types.BUG || moveType == Types.DARK || moveType == Types.GHOST)) {
					if (target.IncreaseStatWithCause(Stats.SPEED, 1, target, Abilities.GetName(target.ability))) {
						Debug.Log(string.Format("[Ability Triggered] {0}'s {1}", target.String(), Abilities.GetName(target.ability)));
					}
				}
				// Weak Armor
				if (target.HasWorkingAbility(Abilities.WEAKARMOR) && move.IsPhysical(moveType)) {
					if (target.ReduceStatWithCause(Stats.DEFENSE, 1, target, Abilities.GetName(target.ability))) {
						Debug.Log(string.Format("[Ability Triggered] {0}'s {1} (Lower defense)", target.String(), Abilities.GetName(target.ability)));
					}
					if (target.IncreaseStatWithCause(Stats.SPEED, 1, target, Abilities.GetName(target.ability))) {
						Debug.Log(string.Format("[Ability Triggered] {0}'s {1} (Increase Speed)", target.String(), Abilities.GetName(target.ability)));
					}
				}
				// Air Balloon
				if (target.HasWorkingItem(Items.AIRBALLOON, true)) {
					Debug.Log(string.Format("[Item Triggered] {0}'s {1} popped!", target.String(), Items.GetName(target.item)));
					battle.Display(string.Format("{0}'s {1} popped!", target.String(), Items.GetName(target.item)));
					target.ConsumeItem();
				}
				// Absorb Bulb
				else if (target.HasWorkingItem(Items.ABSORBBULB) && moveType == Types.WATER) {
					if (target.IncreaseStatWithCause(Stats.SPATK, 1, target, Items.GetName(target.item))) {
						Debug.Log(string.Format("[Item Triggered] {0}'s {1}", target.String(), Items.GetName(target.item)));
						target.ConsumeItem();
					}
				}
				// Luminous Moss
				else if (target.HasWorkingItem(Items.LUMINOUSMOSS) && moveType == Types.WATER) {
					if (target.IncreaseStatWithCause(Stats.SPDEF, 1, target, Items.GetName(target.item))) {
						Debug.Log(string.Format("[Item Triggered] {0}'s {1}", target.String(), Items.GetName(target.item)));
						target.ConsumeItem();
					}
				}
				// Cell Battery
				else if (target.HasWorkingItem(Items.CELLBATTERY) && moveType == Types.ELECTRIC) {
					if (target.IncreaseStatWithCause(Stats.ATTACK, 1, target, Items.GetName(target.item))) {
						Debug.Log(string.Format("[Item Triggered] {0}'s {1}", target.String(), Items.GetName(target.item)));
						target.ConsumeItem();
					}
				}
				// Snow Ball
				else if (target.HasWorkingItem(Items.SNOWBALL) && moveType == Types.ICE) {
					if (target.IncreaseStatWithCause(Stats.ATTACK, 1, target, Items.GetName(target.item))) {
						Debug.Log(string.Format("[Item Triggered] {0}'s {1}", target.String(), Items.GetName(target.item)));
						target.ConsumeItem();
					}
				}
				// Weakness Policy
				else if (target.HasWorkingItem(Items.WEAKNESSPOLICY) && target.damageState.TypeModifier > 8) {
					bool showanim = true;
					if (target.IncreaseStatWithCause(Stats.ATTACK, 1, target, Items.GetName(target.item))) {
						Debug.Log(string.Format("[Item Triggered] {0}'s {1} (Attack)", target.String(), Items.GetName(target.item)));
						showanim = false;
					}
					if (target.IncreaseStatWithCause(Stats.SPATK, 1, target, Items.GetName(target.item))) {
						Debug.Log(string.Format("[Item Triggered] {0}'s {1} (Special Attack)", target.String(), Items.GetName(target.item)));
						showanim = false;
					}
					if (!showanim) {
						target.ConsumeItem();
					}
				}
				// Enigma Berry
				else if (target.HasWorkingItem(Items.ENIGMABERRY) && target.damageState.TypeModifier > 8) {
					target.ActivateBerryEffect();
				}
				//Jaboca Berry
				//Rowap Berry
				else if ((target.HasWorkingItem(Items.JABOCABERRY) && move.IsPhysical(moveType)) || (target.HasWorkingItem(Items.ROWAPBERRY) && move.IsSpecial(moveType))) {
					if (!user.HasWorkingAbility(Abilities.MAGICGUARD) && !user.Fainted()) {
						Debug.Log(string.Format("[Item Triggered] {0}'s {1}", target.String(), Items.GetName(target.item)));
						battle.scene.DamageAnimation(user, 0);
						user.ReduceHP(user.totalHP/8);
						battle.Display(string.Format("{0} consumed its {1} and hurt {2}!", target.String(), Items.GetName(target.item), user.String(true)));
						target.ConsumeItem();
					}
				}
				// Kee Berry
				else if (target.HasWorkingItem(Items.KEEBERRY) && move.IsPhysical(moveType)) {
					target.ActivateBerryEffect();
				}
				// Maranga Berry
				else if (target.HasWorkingItem(Items.MARANGABERRY) && move.IsSpecial(moveType)) {
					target.ActivateBerryEffect();
				}
			}
			// Anger Point
			if (target.HasWorkingAbility(Abilities.ANGERPOINT)) {
				if (target.damageState.Critical && !target.damageState.Substitute && target.CanIncreaseStatStage(Stats.ATTACK, target)) {
					Debug.Log(string.Format("[Ability Triggered] {0}'s {1}", target.String(), Abilities.GetName(target.ability)));
					target.stages[Stats.ATTACK] = 6;
					battle.CommonAnimation("StatUp", target, null);
					battle.Display(string.Format("{0}'s {1} maxed its {2}!", target.String(), Abilities.GetName(target.ability), Stats.GetName(Stats.ATTACK)));
				}
			}
		}
		user.AbilityCureCheck();
		target.AbilityCureCheck();
	}

	public void EffectAfterHit(Battler user, Battler target, BattleMove thisMove, Dictionary<int, int> turnEffects) {
		if (turnEffects[Effects.TotalDamage] == 0) {
			return;
		}
		if (!(user.HasWorkingAbility(Abilities.SHEERFORCE) && thisMove.add1Effect > 0)) {
			// Target Held Items
			// Red Card
			if (target.HasWorkingItem(Items.REDCARD) && battle.CanSwitch(user.index, -1, false)) {
				user.effects[Effects.Roar] = 1;
				battle.Display(string.Format("{0} held up its {1} against the {3}!", target.String(), Items.GetName(target.item), user.String(this)));
				target.ConsumeItem();
			}
			// Eject Button
			else if (target.HasWorkingItem(Items.EJECTBUTTON) && battle.CanChooseNonActive(target.index)) {
				target.effects[Effects.Uturn] = 1;
				battle.Display(string.Format("{0} is switched out with the {1}!", target.String(), Items.GetName(target.item)));
				target.ConsumeItem();
			}
			// User's Held Items
			// Shell Bell
			if (user.HasWorkingItem(Items.SHELLBELL) && user.effects[Effects.HealBlock] == 0) {
				Debug.Log(string.Format("[Item Triggered] {0}'s {1} (Total Damage: {2})", user.String(), Items.GetName(target.item), turnEffects[Effects.TotalDamage]));
				int hpGain = user.RecoverHP(turnEffects[Effects.TotalDamage]/8, true);
				if (hpGain > 0) {
					battle.Display(string.Format("{0} restored a little HP using its {1}!", user.String(), Items.GetName(target.item)));
				}
			}
			// Life Orb
			if (user.effects[Effects.LifeOrb] != 0 && !user.HasWorkingAbility(Abilities.MAGICGUARD)) {
				Debug.Log(string.Format("[Item Triggered] {0}'s {1} (Recoil)", user.String(), Items.GetName(target.item)));
				int hpLoss = user.ReduceHP(turnEffects[Effects.TotalDamage]/10, true);
				if (hpLoss > 0) {
					battle.Display(string.Format("{0} lost some of its HP!", user.String()));
				}
			}
			if (user.Fainted()) {
				user.Faint();
			}
			// Abilities
			// Color Change
			int moveType = thisMove.GetType(thisMove.type, user, target);
			if (target.HasWorkingAbility(Abilities.COLORCHANGE) && Types.IsPseudoType(moveType) && !target.HasType(moveType)) {
				Debug.Log(string.Format("[Ability Triggered] {0}'s {1} made it the {2} type!", target.String(), Abilities.GetName(target.ability), Types.GetName(moveType)));
				target.type1 = moveType;
				target.type2 = moveType;
				target.effects[Effects.Type3] = -1;
				battle.Display(string.Format("{0}'s {1} made it the {2} type!", target.String(), Abilities.GetName(target.ability), Types.GetName(moveType)));
			}
		}
		// Moxie
		if (user.HasWorkingAbility(Abilities.MOXIE) && target.fainted) {
			if (user.IncreaseStatWithCause(Stats.ATTACK, 1, user, Abilities.GetName(user.ability))) {
				Debug.Log(string.Format("[Ability Triggered] {0}'s {1}!", target.String(), Abilities.GetName(target.ability)));
			}
		}
		// Magician
		if (user.HasWorkingAbility(Abilities.PICKPOCKET)) {
			if (target.item > 0 && user.item == 0 && user.effects[Effects.Substitute] == 0 && target.effects[Effects.Substitute] == 0 && !target.HasWorkingAbility(Abilities.STICKYHOLD) && !battle.IsUnlosableItem(target, target.item) && !battle.IsUnlosableItem(user, target.item) && (battle.opponent != null || !battle.IsOpposing(target.index))) {
				target.item = user.item;
				user.item = 0;
				user.effects[Effects.Unburden] = 1;
				if (battle.opponent == null && // In a wild battle
					target.pokemon.itemInitial == 0 && user.pokemon.itemInitial == target.item) {
					target.pokemon.itemInitial = target.item;
					user.pokemon.itemInitial = 0;
				}
				battle.Display(string.Format("{0} stole {1}'s {2} with {3}!", user.String(), target.String(), Items.GetName(user.item), Abilities.GetName(user.ability)));
				Debug.Log(string.Format("[Ability Triggered] {0} stole {1}'s {2} with {3}!", user.String(), target.String(), Items.GetName(user.item), Abilities.GetName(user.ability)));
			}
		}
		// Pickpocket
		if (user.HasWorkingAbility(Abilities.PICKPOCKET)) {
			if (target.item > 0 && user.item == 0 && user.effects[Effects.Substitute] == 0 && target.effects[Effects.Substitute] == 0 && !target.HasWorkingAbility(Abilities.STICKYHOLD) && !battle.IsUnlosableItem(target, target.item) && !battle.IsUnlosableItem(user, target.item) && (battle.opponent != null || !battle.IsOpposing(target.index))) {
				target.item = user.item;
				user.item = 0;
				user.effects[Effects.Unburden] = 1;
				if (battle.opponent == null && // In a wild battle
					target.pokemon.itemInitial == 0 && user.pokemon.itemInitial == target.item) {
					target.pokemon.itemInitial = target.item;
					user.pokemon.itemInitial = 0;
				}
				battle.Display(string.Format("{0} pickpocketed {1}'s {2}!", user.String(), target.String(), Items.GetName(user.item)));
				Debug.Log(string.Format("[Ability Triggered] {0} pickpocketed {1}'s {2}!", user.String(), target.String(), Items.GetName(user.item)));
			}
		}
	}

	public void AbilityCureCheck() {
		if (fainted) {
			return;
		}
		switch (status) {
			case Statuses.SLEEP:
			if (HasWorkingAbility(Abilities.VITALSPIRIT) || HasWorkingAbility(Abilities.INSOMNIA)) {
				Debug.Log(string.Format("[Ability Triggered] {0}'s {1} woke it up!", String(), Abilities.GetName(ability)));
				CureStatus(false);
				battle.Display(string.Format("{0}'s {1} woke it up!", String(), Abilities.GetName(ability)));
			}
			break;
			case Statuses.POISON:
			if (HasWorkingAbility(Abilities.IMMUNITY)) {
				Debug.Log(string.Format("[Ability Triggered] {0}'s {1} cured its poisoning!", String(), Abilities.GetName(ability)));
				CureStatus(false);
				battle.Display(string.Format("{0}'s {1} cured its poisoning!", String(), Abilities.GetName(ability)));
			}
			break;
			case Statuses.BURN:
			if (HasWorkingAbility(Abilities.WATERVEIL)) {
				Debug.Log(string.Format("[Ability Triggered] {0}'s {1} healed its burn!", String(), Abilities.GetName(ability)));
				CureStatus(false);
				battle.Display(string.Format("{0}'s {1} healed its burn!", String(), Abilities.GetName(ability)));
			}
			break;
			case Statuses.PARALYSIS:
			if (HasWorkingAbility(Abilities.LIMBER)) {
				Debug.Log(string.Format("[Ability Triggered] {0}'s {1} cured its paralysis!", String(), Abilities.GetName(ability)));
				CureStatus(false);
				battle.Display(string.Format("{0}'s {1} cured its paralysis!", String(), Abilities.GetName(ability)));
			}
			break;
			case Statuses.FROZEN:
			if (HasWorkingAbility(Abilities.MAGMAARMOR)) {
				Debug.Log(string.Format("[Ability Triggered] {0}'s {1} defrosted it!", String(), Abilities.GetName(ability)));
				CureStatus(false);
				battle.Display(string.Format("{0}'s {1} defrosted it!", String(), Abilities.GetName(ability)));
			}
			break;
		}
		if (effects[Effects.Confusion] > 0 && HasWorkingAbility(Abilities.OWNTEMPO)) {
			Debug.Log(string.Format("[Ability Triggered] {0}'s {1} snapped it out of its confusion!", String(), Abilities.GetName(ability)));
			CureConfusion(false);
			battle.Display(string.Format("{0}'s {1} snapped it out of its confusion!", String(), Abilities.GetName(ability)));
		}
		if (effects[Effects.Attract] > 0 && HasWorkingAbility(Abilities.OBLIVIOUS)) {
			Debug.Log(string.Format("[Ability Triggered] {0}'s {1} cured its infatuation status!", String(), Abilities.GetName(ability)));
			CureConfusion(false);
			battle.Display(string.Format("{0}'s {1} cured its infatuation status!", String(), Abilities.GetName(ability)));
		}
		if (Settings.USE_NEW_BATTLE_MECHANICS && effects[Effects.Taunt] > 0 && HasWorkingAbility(Abilities.OBLIVIOUS)) {
			Debug.Log(string.Format("[Ability Triggered] {0}'s {1} made its taunt wear off!", String(), Abilities.GetName(ability)));
			CureConfusion(false);
			battle.Display(string.Format("{0}'s {1} made its taunt wear off!", String(), Abilities.GetName(ability)));
		}
	}

	/********************
	* Held Item Effects *
	********************/
	public void ConsumeItem(bool recycle=true, bool pickup=true) {
		if (recycle) {
			pokemon.itemRecycle = item;
		}
		if (pokemon.itemInitial == item) {
			pokemon.itemInitial = 0;
		}
		if (pickup) {
			effects[Effects.PickupItem] = item;
			effects[Effects.PickupUse] = battle.nextPickupUse;
		}
		item = 0;
		effects[Effects.Unburden] = 1;
		// Symbiosis
		if (Partner() != null && Partner().HasWorkingAbility(Abilities.SYMBIOSIS) && recycle) {
			if (Partner().item > 0 && !battle.IsUnlosableItem(Partner(), Partner().item) && !battle.IsUnlosableItem(this, Partner().item)) {
				battle.Display(string.Format("{0}'s {1} let it share its {2} with {3}!", Partner().String(), Abilities.GetName(ability), Items.GetName(Partner().item), String(true)));
				item = Partner().item;
				Partner().item = 0;
				Partner().effects[Effects.Unburden] = 1;
				BerryCureCheck();
			}
		}
	}

	public bool ConfusionBerry(int flavor, string message1, string message2) {
		if (effects[Effects.HealBlock] > 0) {
			int amt = RecoverHP(totalHP/8, true);
			if (amt > 0) {
				battle.Display(message1);
				if (nature%5 == flavor && nature/5 != nature%5) {
					battle.Display(message2);
					ConfuseSelf();
				}
				return true;
			}
		}
		return false;
	}

	public bool StatIncreasingBerry(int stat, string berryName) {
		return IncreaseStatWithCause(stat, 1, this, berryName);
	}

	public void ActivateBerryEffect(int berry=0, bool consume=true) {
		if (berry == 0) {
			berry = item;
		}
		string berryname = berry==0 ? "" : Items.GetName(berry);
		Debug.Log(string.Format("[Item Triggered] {0}'s {1}", String(), berryname));
		bool consumed = false;
		if (berry == Items.ORANBERRY) {
			if (effects[Effects.HealBlock] == 0) {
				int amt = RecoverHP(10, true);
				if (amt > 0) {
					battle.Display(string.Format("{0} restored its health using its {1}!", String(), Items.GetName(item)));
					consumed = true;
				}
			}
		} else if (berry == Items.SITRUSBERRY || berry == Items.ENIGMABERRY) {
			if (effects[Effects.HealBlock] == 0) {
				int amt = RecoverHP(totalHP/4, true);
				if (amt > 0) {
					battle.Display(string.Format("{0} restored its health using its {1}!", String(), Items.GetName(item)));
					consumed = true;
				}
			}
		} else if (berry == Items.CHESTOBERRY) {
			if (status == Statuses.SLEEP) {
				CureStatus(false);
				battle.Display(string.Format("{0}'s {1} woke it up!", String(), Items.GetName(item)));
				consumed = true;
			}
		} else if (berry == Items.PECHABERRY) {
			if (status == Statuses.POISON) {
				CureStatus(false);
				battle.Display(string.Format("{0}'s {1} cured its poisoning.", String(), Items.GetName(item)));
				consumed = true;
			}
		} else if (berry == Items.RAWSTBERRY) {
			if (status == Statuses.BURN) {
				CureStatus(false);
				battle.Display(string.Format("{0}'s {1} cured its burn.", String(), Items.GetName(item)));
				consumed = true;
			}
		} else if (berry == Items.CHERIBERRY) {
			if (status == Statuses.PARALYSIS) {
				CureStatus(false);
				battle.Display(string.Format("{0}'s {1} cured its paralysis.", String(), Items.GetName(item)));
				consumed = true;
			}
		} else if (berry == Items.ASPEARBERRY) {
			if (status == Statuses.FROZEN) {
				CureStatus(false);
				battle.Display(string.Format("{0}'s {1} thawed it out.", String(), Items.GetName(item)));
				consumed = true;
			}
		} else if (berry == Items.LEPPABERRY) {
			List<int> found = new List<int>();
			for (int i=0; i<pokemon.moves.Length; i++) {
				if (pokemon.moves[i].Id != 0) {
					if ((consume && pokemon.moves[i].pp == 0) || (!consume && pokemon.moves[i].pp < pokemon.moves[i].TotalPP())) {
						found.Add(i);
					}
				}
			}
			if (found.Count > 0) {
				int choice = consume ? found[0] : found[battle.Rand(found.Count)];
				BattleMove pokemove = BattleMove.FromBattleMove(battle, pokemon.moves[choice]);
				pokemove.pp += 10;
				if (pokemove.pp > pokemove.totalPP) {
					pokemove.pp = pokemove.totalPP;
				}
				moves[choice].pp = pokemove.pp;
				string movename = Moves.GetName(pokemove.id);
				battle.Display(string.Format("{0}'s {1} restored {3}'s PP.", String(), Items.GetName(item), movename));
				consumed = true;
			}
		} else if (berry == Items.PERSIMBERRY) {
			if (effects[Effects.Confusion] > 0) {
				CureConfusion(false);
				battle.Display(string.Format("{0}'s {1} snapped out of its confusion.", String(), Items.GetName(item)));
				consumed = true;
			}
		} else if (berry == Items.LUMBERRY) {
			if (status > 0 || effects[Effects.Confusion] > 0) {
				int st = status;
				bool conf = effects[Effects.Confusion] > 0;
				CureStatus(false);
				CureConfusion(false);
				switch (st) {
					case Statuses.SLEEP:
					battle.Display(string.Format("{0}'s {1} woke it up!", String(), Items.GetName(item)));
					break;
					case Statuses.POISON:
					battle.Display(string.Format("{0}'s {1} cured its poisoning.", String(), Items.GetName(item)));
					break;
					case Statuses.BURN:
					battle.Display(string.Format("{0}'s {1} cured its burn.", String(), Items.GetName(item)));
					break;
					case Statuses.PARALYSIS:
					battle.Display(string.Format("{0}'s {1} cured its paralysis.", String(), Items.GetName(item)));
					break;
					case Statuses.FROZEN:
					battle.Display(string.Format("{0}'s {1} thawed it out.", String(), Items.GetName(item)));
					break;
				}
				if (conf) {
					battle.Display(string.Format("{0}'s {1} snapped out of its confusion.", String(), Items.GetName(item)));
				}
				consumed = true;
			}
		} else if (berry == Items.FIGYBERRY) {
			consumed = ConfusionBerry(0,
				string.Format("{1}'s {2} restored health!", String(), berryname),
				string.Format("For {1}, the {2} was too spicy!", String(true), berryname));
		} else if (berry == Items.WIKIBERRY) {
			consumed = ConfusionBerry(0,
				string.Format("{1}'s {2} restored health!", String(), berryname),
				string.Format("For {1}, the {2} was too dry!", String(true), berryname));
		} else if (berry == Items.MAGOBERRY) {
			consumed = ConfusionBerry(0,
				string.Format("{1}'s {2} restored health!", String(), berryname),
				string.Format("For {1}, the {2} was too sweet!", String(true), berryname));
		} else if (berry == Items.AGUAVBERRY) {
			consumed = ConfusionBerry(0,
				string.Format("{1}'s {2} restored health!", String(), berryname),
				string.Format("For {1}, the {2} was too bitter!", String(true), berryname));
		} else if (berry == Items.IAPAPABERRY) {
			consumed = ConfusionBerry(0,
				string.Format("{1}'s {2} restored health!", String(), berryname),
				string.Format("For {1}, the {2} was too sour!", String(true), berryname));
		} else if (berry == Items.LIECHIBERRY) {
			consumed = StatIncreasingBerry(Stats.ATTACK, berryname);
		} else if (berry == Items.GANLONBERRY || berry == Items.KEEBERRY) {
			consumed = StatIncreasingBerry(Stats.DEFENSE, berryname);
		} else if (berry == Items.SALACBERRY) {
			consumed = StatIncreasingBerry(Stats.SPEED, berryname);
		} else if (berry == Items.PETAYABERRY) {
			consumed = StatIncreasingBerry(Stats.SPATK, berryname);
		} else if (berry == Items.APICOTBERRY || berry == Items.MARANGABERRY) {
			consumed = StatIncreasingBerry(Stats.SPDEF, berryname);
		} else if (berry == Items.LANSATBERRY) {
			if (effects[Effects.FocusEnergy] < 2) {
				effects[Effects.FocusEnergy] = 2;
				battle.Display(string.Format("{0} used its {1} to get pumped!", String(), berryname));
				consumed = true;
			}
		} else if (berry == Items.MICLEBERRY) {
			if (effects[Effects.MicleBerry] == 0) {
				effects[Effects.MicleBerry] = 1;
				battle.Display(string.Format("{0} boosted the accuracy of its next move using its {1}", String(), berryname));
				consumed = true;
			}
		} else if (berry == Items.STARFBERRY) {
			List<int> stats = new List<int>();
			int[] s = {Stats.ATTACK, Stats.DEFENSE, Stats.SPATK, Stats.SPDEF, Stats.SPEED};
			for (int i = 0; i < s.Length; i++) {
				if (CanIncreaseStatStage(s[i], this)) {
					stats.Add(s[i]);
				}
			}
			if (stats.Count > 0) {
				int stat = stats[battle.Rand(stats.Count)];
				consumed = IncreaseStatWithCause(stat, 2, this, berryname);
			}
		}
		if (consumed) {
			if (HasWorkingAbility(Abilities.CHEEKPOUCH) && effects[Effects.HealBlock] == 0) {
				int amt = RecoverHP(totalHP/3, true);
				if (amt > 0) {
					battle.Display(string.Format("{1}'s {2} restored its health", String(), Abilities.GetName(ability)));
				}
			}
			if (consume) {
				ConsumeItem();
			}
			if (pokemon != null) {
				pokemon.belch = true;
			}
		}
	}

	public void BerryCureCheck(bool hpCure=false) {
		if (fainted) {
			return;
		}
		bool unnerver = (Opposing1().HasWorkingAbility(Abilities.UNNERVE) || Opposing2().HasWorkingAbility(Abilities.UNNERVE));
		string itemName = item==0 ? "" : Items.GetName(item);
		if (hpCure) {
			if (HasWorkingItem(Items.BERRYJUICE) && hp <= totalHP/2) {
				int amt = RecoverHP(20, true);
				if (amt > 0) {
					battle.CommonAnimation("UseItem", this, null);
					battle.Display(string.Format("{1} restored its health using its {2}!", String(), itemName));
					ConsumeItem();
					return;
				}
			}
		}
		if (!unnerver) {
			if (hpCure) {
				if (hp <= totalHP/2) {
					if (HasWorkingItem(Items.ORANBERRY) || HasWorkingItem(Items.SITRUSBERRY)) {
						ActivateBerryEffect();
						return;
					}
					if (HasWorkingItem(Items.FIGYBERRY) || HasWorkingItem(Items.WIKIBERRY) || HasWorkingItem(Items.MAGOBERRY) || HasWorkingItem(Items.AGUAVBERRY) || HasWorkingItem(Items.IAPAPABERRY)) {
						ActivateBerryEffect();
						return;
					}
				}
			}
			if ((HasWorkingAbility(Abilities.GLUTTONY) && hp <= totalHP/2) || hp <= totalHP/4) {
				if (HasWorkingItem(Items.LIECHIBERRY) || HasWorkingItem(Items.GANLONBERRY) || HasWorkingItem(Items.SALACBERRY) || HasWorkingItem(Items.PETAYABERRY) || HasWorkingItem(Items.APICOTBERRY)) {
					ActivateBerryEffect();
					return;
				}
				if (HasWorkingItem(Items.LANSATBERRY) || HasWorkingItem(Items.STARFBERRY)) {
					ActivateBerryEffect();
					return;
				}
				if (HasWorkingItem(Items.MICLEBERRY)) {
					ActivateBerryEffect();
					return;
				}
			}
			if (HasWorkingItem(Items.LEPPABERRY)) {
				ActivateBerryEffect();
				return;
			}
			if (HasWorkingItem(Items.CHESTOBERRY) || HasWorkingItem(Items.PECHABERRY) || HasWorkingItem(Items.RAWSTBERRY) || HasWorkingItem(Items.CHERIBERRY) || HasWorkingItem(Items.PERSIMBERRY) || HasWorkingItem(Items.LUMBERRY)) {
				ActivateBerryEffect();
				return;
			}
		}
		if (HasWorkingItem(Items.WHITEHERB)) {
			bool reducedstats = false;
			int[] s = {Stats.ATTACK, Stats.DEFENSE, Stats.SPATK, Stats.SPDEF, Stats.SPEED};
			for (int i = 0; i < s.Length; i++) {
				if (stages[i] < 0) {
					stages[i] = 0;
					reducedstats = true;
				}
			}
			if (reducedstats) {
				Debug.Log(string.Format("[Item triggered] {0}'s {1}", String(), itemName));
				battle.CommonAnimation("UserItem", this, null);
				battle.Display(string.Format("{0} restored its stats using its {1}!", String(), itemName));
				ConsumeItem();
				return;
			}
		}
		if (HasWorkingItem(Items.MENTALHERB) && (
			effects[Effects.Attract] >= 0 || effects[Effects.Taunt] > 0 || effects[Effects.Encore] > 0 || effects[Effects.Torment] != 0 || effects[Effects.Taunt] > 0 || effects[Effects.HealBlock] > 0)) {
			Debug.Log(string.Format("[Item triggered] {0}'s {1}", String(), itemName));
			battle.CommonAnimation("UserItem", this, null);
			if (effects[Effects.Attract] >= 0) {
				battle.Display(string.Format("{0} cured its infatuation using its {1}!", String(), itemName));
			}
			if (effects[Effects.Taunt] > 0) {
				battle.Display(string.Format("{0}'s taunt wore off!", String()));
			}
			if (effects[Effects.Encore] > 0) {
				battle.Display(string.Format("{0}'s encore ended!", String()));
			}
			if (effects[Effects.Torment] != 0) {
				battle.Display(string.Format("{0}'s torment wore off!", String()));
			}
			if (effects[Effects.Disable] > 0) {
				battle.Display(string.Format("{0} is no longer disabled!", String()));
			}
			if (effects[Effects.HealBlock] > 0) {
				battle.Display(string.Format("{0}'s Heal Block wore off!", String()));
			}
			CureAttract();
			effects[Effects.Taunt] = 0;
			effects[Effects.Encore] = 0;
			effects[Effects.EncoreMove] = 0;
			effects[Effects.EncoreIndex] = 0;
			effects[Effects.Torment] = 0;
			effects[Effects.Disable] = 0;
			effects[Effects.HealBlock] = 0;
			ConsumeItem();
			return;
		}
		if (hpCure && HasWorkingItem(Items.LEFTOVERS) && hp != totalHP && effects[Effects.HealBlock] == 0) {
			Debug.Log(string.Format("[Item triggered] {0}'s {1}", String(), itemName));
			battle.CommonAnimation("UseItem", this, null);
			RecoverHP(totalHP/16, true);
			battle.Display(string.Format("{0}'s restored a little HP using its {2}!", String(), itemName));
		}
		if (hpCure && HasWorkingItem(Items.BLACKSLUDGE)) {
			if (HasType(Types.POISON)) {
				if (hp != totalHP && effects[Effects.HealBlock] == 0) {
					Debug.Log(string.Format("[Item triggered] {0}'s {1} (heal)", String(), itemName));
					battle.CommonAnimation("UseItem", this, null);
					RecoverHP(totalHP/16, true);
					battle.Display(string.Format("{0}'s restored a little HP using its {2}!", String(), itemName));
				}
			} else if (!HasWorkingAbility(Abilities.MAGICGUARD)) {
				Debug.Log(string.Format("[Item triggered] {0}'s {1} (damage)", String(), itemName));
				battle.CommonAnimation("UseItem", this, null);
				ReduceHP(totalHP/8, true);
				battle.Display(string.Format("{0}'s was hurt by its {2}!", String(), itemName));
			}
			if (fainted) {
				Faint();
			}
		}
	}

	/************************
	* Move user and targets *
	************************/
	public Battler FindUser(int useMoveChoice, int indexChoice, BattleMove moveChoice, int targetChoice, List<Battler> targets) {
		Battler user = this;
		Battler targetBattler = battle.battlers[targetChoice];
		switch (Target(moveChoice)) {
			case Targets.SingleNonUser:
			if (targetChoice >= 0) {
				if (!IsOpposing(targetBattler.index)) {
					if (!AddTarget(targets, targetBattler)) {
						if (!AddTarget(targets, Opposing1())) {
							AddTarget(targets, Opposing2());
						}
					}
				} else {
					AddTarget(targets, targetBattler.Partner());
				}
			} else {
				RandTarget(targets);
			}
			break;
			case Targets.SingleOpposing:
			if (targetChoice >= 0) {
				targetBattler = battle.battlers[targetChoice];
				if (!IsOpposing(targetBattler.index)) {
					if (!AddTarget(targets, targetBattler)) {
						if (!AddTarget(targets, Opposing1())) {
							AddTarget(targets, Opposing2());
						}
					}
				} else {
					AddTarget(targets, targetBattler.Partner());
				}
			} else {
				RandTarget(targets);
			}
			break;
			case Targets.OppositeOpposing:
			if (!AddTarget(targets, OppositeOpposing2())) {
				AddTarget(targets, OppositeOpposing());
			}
			break;
			case Targets.RandomOpposing:
			RandTarget(targets);
			break;
			case Targets.AllOpposing:
			if (!AddTarget(targets, Opposing1())) {
				AddTarget(targets, Opposing2());
			}
			break;
			case Targets.AllNonUsers:
			for (int i=0; i<5; i++) {
				if (i != index) {
					AddTarget(targets, battle.battlers[i]);
				}
			}
			break;
			case Targets.UserOrPartner:
			if (targetChoice >= 0) {
				if (AddTarget(targets, targetBattler)) {
					AddTarget(targets, targetBattler.Partner());
				} else {
					AddTarget(targets, this);
				}
			}
			break;
			case Targets.Partner:
			AddTarget(targets, Partner());
			break;
			default:
			moveChoice.AddTarget(targets, this);
			break;
		}
		return user;
	}

	public Battler ChangeUser(BattleMove thisMove, Battler user) {
		if (thisMove.CanSnatch()) {
			Battler[] priority = battle.Priority();
			for (int i = 0; i < priority.Length; i++) {
				if (priority[i].effects[Effects.Snatch] != 0) {
					battle.Display(string.Format("{0} snatched {1}'s move!", priority[i].String(), user.String(true)));
					Debug.Log(string.Format("[Lingering effect triggered] {0}'s {1}", String(true), thisMove.name));
					priority[i].effects[Effects.Snatch] = 0;
					Battler target = user;
					user = priority[i];
					int userchoice = battle.indexChoice[user.index];
					if (target.HasWorkingAbility(Abilities.PRESSURE) && user.IsOpposing(target.index) && userchoice >= 0) {
						BattleMove pressureMove = user.moves[userchoice];
						if (pressureMove.pp > 0) {
							SetPP(pressureMove, pressureMove.pp - 1);
						}
					}
					if (Settings.USE_NEW_BATTLE_MECHANICS) {
						break;
					}
				}
			}
		}
		return user;
	}

	public int Target(BattleMove move) {
		int target = move.target;
		if (move.function == 0x10D && HasType(Types.GHOST))	{
			target = Targets.OppositeOpposing;
		}
		return target;
	}

	public bool AddTarget(List<Battler> targets, Battler target) {
		if (!target.Fainted()) {
			targets.Add(target);
			return true;
		}
		return false;
	}

	public void RandTarget(List<Battler> targets) {
		List<Battler> choices = new List<Battler>();
		AddTarget(choices, Opposing1());
		AddTarget(choices, Opposing2());
		if (choices.Count > 0) {
			AddTarget(targets, choices[battle.Rand(choices.Count)]);
		}
	}

	public bool ChangeTarget(BattleMove thisMove, Battler[] userAndTarget, List<Battler> targets) {
		Battler[] priority = battle.Priority();
		int changeEffect = 0;
		Battler user = userAndTarget[0];
		Battler target = userAndTarget[1];
		// Lightningrod
		if (targets.Count == 1 && thisMove.GetType(thisMove.type, user, target) == Types.ELECTRIC && !target.HasWorkingAbility(Abilities.LIGHTNINGROD)) {
			for (int i=0; i < priority.Length; i++) {
				if (user.index == priority[i].index || target.index == priority[i].index) {
					continue;
				}
				if (priority[i].HasWorkingAbility(Abilities.LIGHTNINGROD)) {
					Debug.Log(string.Format("[Ability Triggered] {0}'s {1} (change target)", priority[i].String(), Abilities.GetName(target.ability)));
					target = priority[i];
					changeEffect = 1;
					break;
				}
			}
		}
		// Storm Drain
		if (targets.Count == 1 && thisMove.GetType(thisMove.type, user, target) == Types.ELECTRIC && !target.HasWorkingAbility(Abilities.STORMDRAIN)) {
			for (int i=0; i < priority.Length; i++) {
				if (user.index == priority[i].index || target.index == priority[i].index) {
					continue;
				}
				if (priority[i].HasWorkingAbility(Abilities.STORMDRAIN)) {
					Debug.Log(string.Format("[Ability Triggered] {0}'s {1} (change target)", priority[i].String(), Abilities.GetName(target.ability)));
					target = priority[i];
					changeEffect = 1;
					break;
				}
			}
		}
		// Change target to user of Follow Me (Overrides Magic Coat
		// because check for Magic Coat below uses this target)
		if (Targets.TargetsOneOpponent(thisMove)) {
			Battler newTarget = null;
			int strength = 100;
			for (int i=0; i < priority.Length; i++) {
				if (!user.IsOpposing(priority[i].index)) {
					continue;
				}
				if (!priority[i].Fainted() && !battle.switching && priority[i].effects[Effects.SkyDrop] == 0 && priority[i].effects[Effects.FollowMe] > 0 && priority[i].effects[Effects.FollowMe] < strength) {
					Debug.Log(string.Format("Lingering effect triggered] {0}'s Follow Me", priority[i].String()));
					newTarget = priority[i];
					strength = priority[i].effects[Effects.FollowMe];
					changeEffect = 0;
				}
			}
			if (newTarget != null) {
				target = newTarget;
			}
		}
		// TODO: Pressure here is incorrect if Magic Coat redirects target
		if (user.IsOpposing(target.index) && target.HasWorkingAbility(Abilities.PRESSURE)) {
			Debug.Log(string.Format("[Ability Triggered] {0}'s {1}", target.String(), Abilities.GetName(target.ability)));
			user.ReducePP(thisMove);
		}
		// Change user to user of Snatch
		if (thisMove.CanSnatch()) {
			for (int i=0; i < priority.Length; i++) {
				if (priority[i].effects[Effects.Snatch] != 0) {
					battle.Display(string.Format("{0} Snatched {1}'s move!", priority[i].String(), user.String()));
					Debug.Log(string.Format("Lingering effect triggered] {0}'s Snatch", priority[i].String()));
					priority[i].effects[Effects.Snatch] = 0;
					target = user;
					user = priority[i];
					int userchoice = battle.indexChoice[user.index];
					if (target.HasWorkingAbility(Abilities.PRESSURE) && user.IsOpposing(target.index) && userchoice >= 0) {
						Debug.Log(string.Format("[Ability Triggered] {0}'s {1} (part of Snatch)", target.String(), Abilities.GetName(target.ability)));
						BattleMove pressureMove = user.moves[userchoice];
						if (pressureMove.pp > 0) {
							SetPP(pressureMove, pressureMove.pp - 1);
						}
					}
				}
			}
		}
		if (thisMove.CanMagicCoat()) {
			if (target.effects[Effects.MagicCoat] != 0) {
				Debug.Log(string.Format("[Lingering Effect Triggered] {0}'s {1} made it use {2}'s {3}", target.String(), Abilities.GetName(target.ability), user.String(true), thisMove.name));
				changeEffect = 3;
				Battler tmp = user;
				user = target;
				target = tmp;
				int userchoice = battle.indexChoice[user.index];
				if (target.HasWorkingAbility(Abilities.PRESSURE) && user.IsOpposing(target.index) && userchoice >= 0) {
					Debug.Log(string.Format("[Ability Triggered] {0}'s {1} (part of Snatch)", target.String(), Abilities.GetName(target.ability)));
					BattleMove pressureMove = user.moves[userchoice];
					if (pressureMove.pp > 0) {
						SetPP(pressureMove, pressureMove.pp - 1);
					}
				}
			} else if (!user.HasMoldBreaker() && target.HasWorkingAbility(Abilities.MAGICBOUNCE)) {
				Debug.Log(string.Format("[Ability Triggered] {0}'s {1} made it use {2}", target.String(), Abilities.GetName(target.ability), thisMove.name));
				changeEffect = 3;
				Battler tmp = user;
				user = target;
				target = tmp;
			}
		}
		if (changeEffect == 1) {
			battle.Display(string.Format("{1}'s {2} took the move!", target.String(), Abilities.GetName(target.ability)));
		} else if (changeEffect == 3) {
			battle.Display(string.Format("{1} bounced the {2} back!", target.String(), thisMove.name));
		}
		userAndTarget[0] = user;
		userAndTarget[1] = target;
		if (!user.HasMoldBreaker() && target.HasWorkingAbility(Abilities.SOUNDPROOF) && thisMove.IsSoundbased() && thisMove.function != 0xE5 && thisMove.function != 0x151) {
			Debug.Log(string.Format("[Ability Triggered] {0}'s {1} blocked {2}'s {3}", target.String(), Abilities.GetName(target.ability), user.String(), thisMove.name));
			battle.Display(string.Format("{1}'s {2} blocks {3}!", target.String(), Abilities.GetName(target.ability), thisMove.name));
			return false;
		}
		return true;
	}

	/**********
	* Move PP *
	**********/
	public void SetPP(BattleMove move, int pp) {
		move.pp = pp;
		if (move.thisMove != null && move.id == move.thisMove.Id && effects[Effects.Transform] == 0) {
			move.pp = pp;
		}
	}

	public bool ReducePP(BattleMove move) {
		if (effects[Effects.TwoTurnAttack] > 0 || effects[Effects.Bide] > 0 || effects[Effects.Outrage] > 0 || effects[Effects.Rollout] > 0 || effects[Effects.HyperBeam] > 0 || effects[Effects.Uproar] > 0) {
			return true;
		}
		if (move.pp < 0) {
			return true;
		}
		if (move.totalPP == 0) {
			return true;
		}
		if (move.pp == 0) {
			return false;
		}
		if (move.pp > 0) {
			SetPP(move, move.pp - 1);
		}
		return true;
	}

	public void ReducePPOther(BattleMove move) {
		if (move.pp > 0) {
			SetPP(move, move.pp-1);
		}
	}

	/***************
	* Using a move *
	****************/
	public bool ObedienceCheck(int useMoveChoice, int indexChoice, BattleMove moveChoice, int targetChoice) {
		if (useMoveChoice != 0) {
			return true;
		}
		if (!battle.internalbattle) {
			return true;
		}
		if (!battle.OwnedByPlayer(index)) {
			return true;
		}
		bool disobedient = false;
		int badgelevel = 10*(battle.Player().NumBadges() + 1);
		if (battle.Player().NumBadges() > 8) {
			badgelevel = Experience.MAXLEVEL;
		}
		BattleMove move = moveChoice;
		if (pokemon.IsForeign(battle.Player()) && level > badgelevel) {
			int a = (int)((level + badgelevel) * battle.Rand(256)/256);
			disobedient |= a >= badgelevel;
		}
		if (!disobedient) {
			return true;
		}
		Debug.Log(string.Format("[Disobedience] {0} disobeyed"));
		effects[Effects.Rage] = 0;
		if (status == Statuses.SLEEP && move.CanUseWhileAsleep()) {
			battle.Display(string.Format("{1} ignored orders and kept sleeping!"));
			return false;
		}
		return true;
	}

	public bool SuccessCheck(BattleMove thisMove, Battler user, Battler target, Dictionary<int, int> turnEffects, bool accuracy=true) {
		if (user.effects[Effects.TwoTurnAttack] > 0) {
			return true;
		}
		// TODO: "Before Protect" applies to Counter/Mirror Coat
		if (thisMove.function == 0xDE && target.status != Statuses.SLEEP) { // Dream Eater
			battle.Display(string.Format("{0} wasn't effected!", target.String()));
			Debug.Log(string.Format("[Move Failed] {0}'s {1} didn't affect {2} because it wasn't asleep!", user.String(), thisMove.name, target.String(true)));
			return false;
		}
		if (thisMove.function == 0x113 && user.effects[Effects.Stockpile] == 0) { // Spit up
			battle.Display(string.Format("But it failed to spit up a thing!", target.String()));
			Debug.Log(string.Format("[Move Failed] {0}'s {1} did nothing as Stockpile's count is 0!", user.String(), thisMove.name, target.String(true)));
			return false;
		}
		if (target.effects[Effects.Protect] != 0 && thisMove.CanProtectAgainst() && target.effects[Effects.ProtectNegation] == 0) {
			battle.Display(string.Format("{0} protected itself!", target.String()));
			Debug.Log(string.Format("[Move Failed] {0}'s Protect stopped the attack!", user.String()));
			return false;
		}
		int p = thisMove.priority;
		if (Settings.USE_NEW_BATTLE_MECHANICS) {
			if (user.HasWorkingAbility(Abilities.PRANKSTER) && thisMove.IsStatus()) {
				p += 1;
			}
			if (user.HasWorkingAbility(Abilities.GALEWINGS) && thisMove.type == Types.FLYING) {
				p += 1;
			}
		}
		if (target.OwnSide().effects[Effects.QuickGuard] != 0 && thisMove.CanProtectAgainst() && p > 0 && target.effects[Effects.ProtectNegation] == 0) {
			battle.Display(string.Format("{0} was protected by Quick Guard!", target.String()));
			Debug.Log(string.Format("[Move Failed] The opposing side's Quick Guard stopped the attack!"));
			return false;
		}
		if (target.OwnSide().effects[Effects.WideGuard] != 0 && Targets.HasMultipleTargets(thisMove) && thisMove.IsStatus() && target.effects[Effects.ProtectNegation] == 0) {
			battle.Display(string.Format("{0} was protected by Wide Guard!", target.String()));
			Debug.Log(string.Format("[Move Failed] The opposing side's Wide Guard stopped the attack!"));
			return false;
		}
		if (target.OwnSide().effects[Effects.CraftyShield] != 0 && thisMove.IsStatus() && thisMove.function != 0xE5) {
			battle.Display(string.Format("Crafty Shield protected {0}!", target.String()));
			Debug.Log(string.Format("[Move Failed] The opposing side's Crafty Shield stopped the attack!"));
			return false;
		}
		if (target.OwnSide().effects[Effects.MatBlock] != 0 && !thisMove.IsStatus() && thisMove.CanProtectAgainst() && target.effects[Effects.ProtectNegation] == 0) {
			battle.Display(string.Format("{0} was protected by the kicked-up mat!", thisMove.name));
			Debug.Log(string.Format("[Move Failed] The opposing side's Mat Block stopped the attack!"));
			return false;
		}
		// TODO: Mind Reader/Lock-On
		// --Sketch/FutureSight/PsychUp work even on Fly/Bounce/Dive/Dig
		if (thisMove.MoveFailed(user, target)) { // TODO: Applies to Snore/Fake Out
			battle.Display(string.Format("But it failed!"));
			Debug.Log(string.Format("[Move Failed] Failed MoveFailed() (function code {0})", thisMove.function));
			return false;
		}
		// King's shield (purposely after MoveFailed())
		if (target.effects[Effects.KingsShield] != 0 && !thisMove.IsStatus() && thisMove.CanProtectAgainst() && target.effects[Effects.ProtectNegation] == 0) {
			battle.Display(string.Format("{0} protected itself!", target.String()));
			battle.successStates[user.index].Protected = true;
			Debug.Log(string.Format("[Move Failed] {0}'s King's Shield stopped the attack", target.String()));
			if (thisMove.IsContactMove()) {
				user.ReduceStat(Stats.ATTACK, 2, null, false);
			}
			return false;
		}
		// Spiky Shield
		if (target.effects[Effects.SpikyShield] != 0 && thisMove.CanProtectAgainst() && target.effects[Effects.ProtectNegation] == 0) {
			battle.Display(string.Format("{0} protected itself!", target.String()));
			battle.successStates[user.index].Protected = true;
			Debug.Log(string.Format("[Move Failed] {0}'s Spiky Shield stopped the attack", target.String()));
			if (thisMove.IsContactMove() && !user.Fainted()) {
				battle.scene.DamageAnimation(user, 0);
				int amount = user.ReduceHP(user.totalHP/8);
				if (amount > 0) {
					battle.Display(string.Format("{0} was hurt!", user.String()));
				}
			}
			return false;
		}
		// Immunity to powder-based moves
		if (Settings.USE_NEW_BATTLE_MECHANICS && thisMove.IsPowderMove() && (
			target.HasType(Types.GRASS) || (
				!user.HasMoldBreaker() && target.HasWorkingItem(Abilities.OVERCOAT)) || target.HasWorkingItem(Items.SAFETYGOGGLES))) {
			battle.Display(string.Format("It doesn't affect {0}...!", target.String()));
			Debug.Log(string.Format("[Move Failed] {0} is immune to powder-based moves somehow"));
			return false;
		}
		if (thisMove.baseDamage > 0 && thisMove.function != 0x02 && thisMove.function != 0x111) {
			int type = thisMove.GetType(thisMove.type, user, target);
			int TypeModifier = thisMove.TypeModifier(type, user, target);
			// Airborne-based immunity to ground
			if (type == Types.GROUND && target.IsAirborne(user.HasMoldBreaker()) && !target.HasWorkingItem(Items.RINGTARGET) && thisMove.function != 0x11C) {
				if (!user.HasMoldBreaker() && target.HasWorkingAbility(Abilities.LEVITATE)) {
					battle.Display(string.Format("{0} makes Ground moves miss with Levitate!", target.String()));
					Debug.Log(string.Format("[Ability Triggered] {0}'s Levitate made the Ground-type move miss"));
					return false;
				}
				if (target.HasWorkingItem(Items.AIRBALLOON)) {
					battle.Display(string.Format("{0}'s Air Balloon makes Ground moves miss!", target.String()));
					Debug.Log(string.Format("[Item Triggered] {0}'s Air Balloon made the Ground-type move miss"));
					return false;
				}
				if (target.effects[Effects.MagnetRise] > 0) {
					battle.Display(string.Format("{0} makes Ground moves miss with Magnet Rise!", target.String()));
					Debug.Log(string.Format("[Lingering Effect Triggered] {0}'s Magnet Rise made the Ground-type move miss"));
					return false;
				}
				if (target.effects[Effects.Telekinesis] > 0) {
					battle.Display(string.Format("{0} makes Ground moves miss with Telekinesis!", target.String()));
					Debug.Log(string.Format("[Lingering Effect Triggered] {0}'s Telekinesis made the Ground-type move miss"));
					return false;
				}
			}
			if (!user.HasMoldBreaker() && target.HasWorkingAbility(Abilities.WONDERGUARD) && type >= 0 && TypeModifier <= 8) {
				battle.Display(string.Format("{0} avoided damage with Wonder Guard!", target.String()));
				Debug.Log(string.Format("[Ability Triggered] {0}'s Wonder Guard", target.String()));
				return false;
			}
			if (TypeModifier == 0) {
				battle.Display(string.Format("It doesn't effect {0}...", target.String()));
				Debug.Log(string.Format("[Move Failed] Type Immunity"));
				return false;
			}
		}
		if (accuracy) {
			if (target.effects[Effects.LockOn] > 0 && target.effects[Effects.LockOnPos] == user.index) {
				Debug.Log(string.Format("[Lingering effect triggered] {0}'s Lock-On", target.String()));
				return true;
			}
			bool miss = false;
			bool Override = false;
			int invulMove = Convert.ToInt32(Moves.GetMove(target.effects[Effects.TwoTurnAttack]).Function, 16);
			switch (invulMove) {
				case 0xC9:
				case 0xCC:
				if (!(
					thisMove.function == 0x08 || thisMove.function == 0x15 || thisMove.function == 0x77 || thisMove.function == 0x78 || thisMove.function == 0x11B || thisMove.function == 0x11C || thisMove.id == Moves.WHIRLWIND)) {
					miss = true;
				}
				break;
				case 0xCA:
				if (!(
					thisMove.function == 0x76 || thisMove.function == 0x95)) {
					miss = true;
				}
				break;
				case 0xCB:
				if (!(
					thisMove.function == 0x75 || thisMove.function == 0xD0)) {
					miss = true;
				}
				break;
				case 0xCD:
				miss = true;
				break;
				case 0xCE:
				if (!(
					thisMove.function == 0x08 || thisMove.function == 0x15 || thisMove.function == 0x77 || thisMove.function == 0x78 || thisMove.function == 0x11B || thisMove.function == 0x11C)) {
					miss = true;
				}
				break;
				case 0x14D:
				miss = true;
				break;
			}
			if (target.effects[Effects.SkyDrop] != 0) {
				if (!(
					thisMove.function == 0x08 || thisMove.function == 0x15 || thisMove.function == 0x77 || thisMove.function == 0x78 || thisMove.function == 0x11B || thisMove.function == 0x11C)) {
					miss = true;
				}
			}
			if (user.HasWorkingAbility(Abilities.NOGUARD) || target.HasWorkingAbility(Abilities.NOGUARD) || battle.futuresight) {
				miss = false;
			}
			if (Settings.USE_NEW_BATTLE_MECHANICS && thisMove.function == 0x06 && thisMove.baseDamage == 0 && user.HasType(Types.POISON)) {
				Override = true;
			}
			if (!miss && turnEffects[Effects.SkipAccuracyCheck] != 0) {
				Override = true;
			}
			if (!Override && (miss || !thisMove.AccuracyCheck(user, target))) {
				Debug.Log(string.Format("[Move Failed] Failed AccuracyCheck() (function code {0}", thisMove.function));
				if (thisMove.target == Targets.AllOpposing && (!user.Opposing1().Fainted() ? 1 : 0) + (!user.Opposing2().Fainted() ? 1 : 0) > 1) {
					battle.Display(string.Format("{0} avoided the attack!", target.String()));
				} else if (thisMove.target == Targets.AllNonUsers && (!user.Opposing1().Fainted() ? 1 : 0) + (!user.Opposing2().Fainted() ? 1 : 0) + (!user.Partner().Fainted() ? 1 : 0) > 1) {
					battle.Display(string.Format("{0} avoided the attack!", target.String()));
				} else if (target.effects[Effects.TwoTurnAttack] > 0) {
					battle.Display(string.Format("{0} avoided the attack!", target.String()));
				} else if (thisMove.function == 0xDC) {
					battle.Display(string.Format("{0} evaded the attack!", target.String()));
				} else {
					battle.Display(string.Format("{1}'s attack missed!", user.String()));
				}
				return false;
			}
		}
		return true;
	}

	public bool TryUseMove(int useMoveChoice, int indexChoice, BattleMove moveChoice, int targetChoice, BattleMove thisMove, Dictionary<int, int> turnEffects) {
		if (turnEffects[Effects.PassedTrying] != 0) {
			return true;
		}
		// TODO: Return true if attack has been Mirror Coated once already
		if (effects[Effects.SkyDrop] != 0) {
			Debug.Log(string.Format("[Move Failed] {0} can't use {1} because of being Sky Dropped", String(), thisMove.name));
			return false;
		}
		if (battle.field.effects[Effects.Gravity] > 0 && thisMove.UnusableInGravity()) {
			Debug.Log(string.Format("[Move Failed] {0} can't use {1} because of Gravity", String(), thisMove.name));
			battle.Display(string.Format("{0} can't use {1} because of Gravity!", String(), thisMove.name));
			return false;
		}
		if (battle.field.effects[Effects.Taunt] > 0 && thisMove.baseDamage == 0) {
			Debug.Log(string.Format("[Move Failed] {0} can't use {1} because of Taunt", String(), thisMove.name));
			battle.Display(string.Format("{0} can't use {1} after the taunt!", String(), thisMove.name));
			return false;
		}
		if (battle.field.effects[Effects.HealBlock] > 0 && thisMove.IsHealingMove()) {
			Debug.Log(string.Format("[Move Failed] {0} can't use {1} because of Heal Block", String(), thisMove.name));
			battle.Display(string.Format("{0} can't use {1} because of Heal Block!", String(), thisMove.name));
			return false;
		}
		if (battle.field.effects[Effects.Torment] != 0 && thisMove.id == lastMoveUsed && thisMove.id != battle.struggle.id && effects[Effects.TwoTurnAttack] == 0 ) {
			Debug.Log(string.Format("[Move Failed] {0} can't use {1} because of Torment", String(), thisMove.name));
			battle.Display(string.Format("{0} can't use the same move in a row due to the torment!", String()));
			return false;
		}
		if (Opposing1().effects[Effects.Imprison] != 0 && !Opposing1().Fainted()) {
			if (thisMove.id ==Opposing1().moves[0].id || thisMove.id ==Opposing1().moves[1].id || thisMove.id ==Opposing1().moves[2].id || thisMove.id ==Opposing1().moves[3].id) {
				Debug.Log(string.Format("[Move Failed] {0} can't use {1} because of {2}'s Imprison", String(), thisMove.name, Opposing1().String()));
				battle.Display(string.Format("{0} can't use the sealed {1}!", String(), thisMove.name));
				return false;
			}
		}
		if (Opposing2().effects[Effects.Imprison] != 0 && !Opposing2().Fainted()) {
			if (thisMove.id ==Opposing2().moves[0].id || thisMove.id ==Opposing2().moves[1].id || thisMove.id ==Opposing2().moves[2].id || thisMove.id ==Opposing2().moves[3].id) {
				Debug.Log(string.Format("[Move Failed] {0} can't use {1} because of {2}'s Imprison", String(), thisMove.name, Opposing2().String()));
				battle.Display(string.Format("{0} can't use the sealed {1}!", String(), thisMove.name));
				return false;
			}
		}
		if (effects[Effects.Disable] > 0 && thisMove.id == effects[Effects.DisableMove] && !battle.switching) {
			battle.DisplayPaused(string.Format("{0}'s {1} is disabled!", String(), thisMove.name));
			Debug.Log(string.Format("[Move Failed] {0}'s {1} is disabled", String(), thisMove.name));
			return false;
		}
		if (indexChoice == -2) {
			battle.Display(string.Format("{0} appears incapable of using its power!", String()));
			Debug.Log(string.Format("[Move Failed] Battle Palace: {0} is incapable of using its power.", String()));
			return false;
		}
		if (effects[Effects.HyperBeam] > 0) {
			battle.Display(string.Format("{0} must recharge!", String()));
			Debug.Log(string.Format("[Move Failed] {0} must recharge after using {1}.", String(), Moves.GetName(currentMove)));
			return false;
		}
		if (HasWorkingAbility(Abilities.TRUANT) && effects[Effects.Truant] != 0) {
			battle.Display(string.Format("{0} is loafing around!", String()));
			Debug.Log(string.Format("[Ability Triggered] {0}'s Truant.", String()));
			return false;
		}
		if (turnEffects[Effects.SkipAccuracyCheck] == 0) {
			if (status == Statuses.SLEEP) {
				statusCount -= 1;
				if (statusCount <= 0) {
					CureStatus();
				} else {
					ContinueStatus();
					Debug.Log(string.Format("[Status] {0} remained asleep (count: {1})", String(), statusCount));
					if (!thisMove.CanUseWhileAsleep()) {
						Debug.Log(string.Format("[Move Failed] {0} couldn't use {1} while asleep", String(), thisMove.name));
						return false;
					}
				}
			}
		}
		if (status == Statuses.FROZEN) {
			if (thisMove.CanThawUser()) {
				Debug.Log(string.Format("[Move effect triggered] {0} was defrosted by using {1}", String(), thisMove.name));
				battle.Display(string.Format("{0} melted the ice!", String()));
				CheckForm();
			} else if (battle.Rand(10) < 2 && turnEffects[Effects.SkipAccuracyCheck] == 0) {
				CureStatus();
				CheckForm();
			} else if (!thisMove.CanThawUser()) {
				ContinueStatus();
				Debug.Log(string.Format("[Status] {0} remained frozen and couldn't move!", String()));
				return false;
			}
		}
		if (turnEffects[Effects.SkipAccuracyCheck] == 0) {
			if (effects[Effects.Confusion] > 0) {
				effects[Effects.Confusion] = effects[Effects.Confusion] - 1;
				if (effects[Effects.Confusion] <= 0) {
					CureConfusion();
				} else {
					ContinueConfusion();
					Debug.Log(string.Format("[Status] {0} remained confused (count: {1}", String(), effects[Effects.Confusion]));
					if (battle.Rand(2) == 0) {
						ConfusionDamage();
						battle.Display(string.Format("It hurt itself in confusion!"));
						Debug.Log(string.Format("[Status] {0} hurt itself in its confusion and couldn't move", String()));
						return false;
					}
				}
			}
		}
		if (effects[Effects.Flinch] != 0) {
			effects[Effects.Flinch] = 0;
			battle.Display(string.Format("{0} flinched and couldn't move!", String()));
			Debug.Log(string.Format("[Lingering effect triggered] {0} flinched", String()));
			if (HasWorkingAbility(Abilities.STEADFAST)) {
				if (IncreaseStatWithCause(Stats.SPEED, 1, this, Abilities.GetName(ability))) {
					Debug.Log(string.Format("[Ability triggered] {0}'s Steadfast", String()));
				}
			}
			return false;
		}
		if (turnEffects[Effects.SkipAccuracyCheck] == 0) {
			if (effects[Effects.Attract] >= 0) {
				AnnounceAttract(battle.battlers[effects[Effects.Attract]]);
				if (battle.Rand(2) == 0) {
					ContinueAttract();
					Debug.Log(string.Format("[Lingering effect triggered] {0} was infatuated and couldn't move", String()));
				}
			}
			if (status == Statuses.PARALYSIS) {
				if (battle.Rand(4) == 0) {
					ContinueStatus();
					Debug.Log(string.Format("[Status] {0} was fully paralyzed and couldn't move", String()));
				}
			}
		}
		if (turnEffects[Effects.SkipAccuracyCheck] == 0) {
			if (!ObedienceCheck(useMoveChoice, indexChoice, moveChoice, targetChoice)) {
				return false;
			}
		}
		turnEffects[Effects.PassedTrying] = 1;
		return true;
	}

	public void ConfusionDamage() {
		damageState.Reset();
		MoveConfusion confMove = new MoveConfusion(battle, null);
		confMove.Effect(this, this);
		if (Fainted()) {
			Faint();
		}
	}

	public void UpdateTargetedMove(BattleMove thisMove, Battler user) {
		// TODO: Snatch, moves that use other moves
		// TODO: All targeting cases
		// Two-turn attacks, Magic Coat, Future Sight, Counter/MirrorCoat/Bide handled
	}

	public void ProcessMoveAgainstTarget(BattleMove thisMove, Battler user, Battler target, int numHits, Dictionary<int, int> turnEffects, bool noCheck=false, List<Battler> allTargets=null, bool showAnimation=true) {
		int realNumHits = 0;
		int totalDamage = 0;
		bool destinyBond = false;
		int damage;
		for (int i=0; i<numHits; i++) {
			target.damageState.Reset();
			if (!noCheck && !SuccessCheck(thisMove, user, target, turnEffects, i==0 || thisMove.SuccessCheckPerHit())) {
				if (thisMove.function == 0xBF && realNumHits > 0) {
					break;
				} else if (thisMove.function == 0x10B) {
					if (!user.HasWorkingAbility(Abilities.MAGICGUARD)) {
						Debug.Log(string.Format("[Move effect triggered] {0} took crash damage", String()));
						// TODO: Not shown if message is "It doesn't effect XXX..."
						battle.Display(string.Format("{0} kept going and crashed!", String()));
						damage = user.totalHP/2;
						if (damage > 0) {
							battle.scene.DamageAnimation(user, 0);
							user.ReduceHP(damage);
						}
						if (user.Fainted()) {
							user.Faint();
						}
					}
				}
				if (thisMove.function == 0xD2) {
					user.effects[Effects.Outrage] = 0;
				}
				if (thisMove.function == 0xD3) {
					user.effects[Effects.Rollout] = 0;
				}
				if (thisMove.function == 0x91) {
					user.effects[Effects.FuryCutter] = 0;
				}
				if (thisMove.function == 0x113) {
					user.effects[Effects.Stockpile] = 0;
				}
			}
			if (thisMove.function == 0x91) {
				if (user.effects[Effects.FuryCutter] < 4) {
					user.effects[Effects.FuryCutter] = user.effects[Effects.FuryCutter] + 1;
				}
			} else {
				user.effects[Effects.FuryCutter] = 0;
			}
			if (thisMove.function == 0x92) {
				if (user.OwnSide().effects[Effects.EchoedVoiceUsed] == 0 && user.OwnSide().effects[Effects.EchoedVoiceCounter] < 5) {
					user.OwnSide().effects[Effects.EchoedVoiceCounter] = user.OwnSide().effects[Effects.EchoedVoiceCounter] + 1;
				}
				user.OwnSide().effects[Effects.EchoedVoiceUsed] = 1;
			}
			if (user.effects[Effects.ParentalBond] > 0) {
				user.effects[Effects.ParentalBond] = user.effects[Effects.ParentalBond] - 1;
			}
			realNumHits += 1;
			damage = thisMove.Effect(user, target, i, allTargets, showAnimation);
			if (damage > 0) {
				totalDamage += damage;
			}
			if (target.damageState.BerryWeakened) {
				battle.Display(string.Format("The {0} weakened the damage to {1}", Items.GetName(target.item), target.String(true)));
				target.ConsumeItem();
			}
			if (target.effects[Effects.Illusion] != 0 && target.HasWorkingAbility(Abilities.ILLUSION) && damage > 0 && !target.damageState.Substitute) {
				Debug.Log(string.Format("[Ability triggered] {0}'s Illusion ended", target.String()));
				target.effects[Effects.Illusion] = 0;
				battle.scene.ChangePokemon(target, target.pokemon);
				battle.Display(string.Format("{0}'s {1} wore off!", target.String(), Abilities.GetName(target.ability)));
			}
			if (user.Fainted()) {
				user.Faint();
			}
			if (numHits > 1 && target.damageState.CalculatedDamage <= 0) {
				return;
			}
			battle.JudgeCheckpoint(user, thisMove.id);
			if (target.damageState.CalculatedDamage > 0 && !user.HasWorkingAbility(Abilities.SHEERFORCE) && (user.HasMoldBreaker() || target.HasWorkingAbility(Abilities.SHIELDDUST))) {
				int add1Effect = thisMove.add1Effect;
				if ((user.HasWorkingAbility(Abilities.SERENEGRACE) || user.OwnSide().effects[Effects.Rainbow] > 0) && thisMove.function != 0xA4) {
					add1Effect *= 2;
				}
				if (Settings.DEBUG && Input.GetKey("ctrl")) {
					add1Effect = 100;
				}
				if (battle.Rand(100) < add1Effect) {
					Debug.Log(string.Format("[Move effect triggered] {0}'s added effect", thisMove.name));
					thisMove.AdditionalEffect(user, target);
				}
			}
			EffectsOnDealingDamage(thisMove, user, target, damage);
			if (!user.Fainted() && target.Fainted()) {
				if (target.effects[Effects.Grudge] != 0 && target.IsOpposing(user.index)) {
					thisMove.pp = 0;
					battle.Display(string.Format("{0}'s {1} lost all its PP due to the grudge!", user.String(), thisMove.name));
					Debug.Log(string.Format("[Lingering effect triggered] {0}'s Grudge made {1} lose all its PP", user.String(), thisMove.name));
				}
			}
			if (target.Fainted()) {
				destinyBond = destinyBond || target.effects[Effects.DestinyBond] != 0;
			}
			if (user.Fainted()) {
				user.Faint();
			}
			if (user.Fainted()) {
				break;
			}
			if (target.Fainted()) {
				break;
			}
			if (target.damageState.CalculatedDamage > 0 && !target.damageState.Substitute) {
				if (user.HasMoldBreaker() || !target.HasWorkingAbility(Abilities.SHIELDDUST)) {
					bool canFlinch = false;
					if ((user.HasWorkingItem(Items.KINGSROCK) || user.HasWorkingItem(Items.RAZORFANG)) && thisMove.CanKingsRock()) {
						canFlinch = true;
					}
					if (user.HasWorkingAbility(Abilities.STENCH) && thisMove.function != 0x09 && thisMove.function != 0x0B && thisMove.function != 0x0E && thisMove.function != 0x0F && thisMove.function != 0x10 && thisMove.function != 0x11 && thisMove.function != 0x12 && thisMove.function != 0x78 && thisMove.function != 0xC7) {
						canFlinch = true;
					}
					if (canFlinch && battle.Rand(10) == 0) {
						Debug.Log(string.Format("[Item/Ability triggered] {0}'s King's Rock/Razor Fang or Stench", user.String()));
						target.Flinch(user);
					}
				}
			}
			if (target.damageState.CalculatedDamage > 0 && !target.Fainted()) {
				if (target.status == Statuses.FROZEN && (
					thisMove.type == Types.FIRE || (Settings.USE_NEW_BATTLE_MECHANICS && thisMove.id == Moves.SCALD))) {
					target.CureStatus();
				}
				if (target.effects[Effects.Rage] != 0 && target.IsOpposing(user.index)) {
					// TODO: Apparantly triggers if opposing Pokémon uses Future Sight after a Future Sight attack
					if (target.IncreaseStatWithCause(Stats.ATTACK, 1, target, "", true, false)) {
						Debug.Log(string.Format("[Lingering effect triggered] {0}'s Rage", target.String()));
						battle.Display(string.Format("{0}'s rage is building!", target.String()));
					}
				}
			}
			if (target.Fainted()) {
				target.Faint();
			}
			if (user.Fainted()) {
				user.Faint();
			}
			if (user.Fainted() || target.Fainted()) {
				break;
			}
			for (int j=0; j < 4; j++) {
				battle.battlers[j].BerryCureCheck();
			}
			if (user.Fainted() || target.Fainted()) {
				break;
			}
			target.UpdateTargetedMove(thisMove, user);
			if (target.damageState.CalculatedDamage <= 0) {
				break;
			}
		}
		if (totalDamage > 0) {
			turnEffects[Effects.TotalDamage] = turnEffects[Effects.TotalDamage] + totalDamage;
		}
		battle.successStates[user.index].UseState = 2;
		battle.successStates[user.index].TypeModifier = target.damageState.TypeModifier;
		if (numHits > 1) {
			if (target.damageState.TypeModifier > 8) {
				if (allTargets.Count > 1) {
					battle.Display(string.Format("It's super effective on {0}!", target.String(true)));
				} else {
					battle.Display("It's super effective!");
				}
			} else if (target.damageState.TypeModifier >= 1 && target.damageState.TypeModifier < 8) {
				if (allTargets.Count > 1) {
					battle.Display(string.Format("It's not very effectve on {0}...", target.String(true)));
				} else {
					battle.Display("It's not very effective...");
				}
			}
			if (realNumHits == 1) {
				battle.Display(string.Format("Hit {0} time!", realNumHits));
			} else {
				battle.Display(string.Format("Hit {0} times!", realNumHits));
			}
		}
		Debug.Log(string.Format("Move did {0} hit(s), total damage={1}", numHits, turnEffects[Effects.TotalDamage]));
		if (target.Fainted()) {
			target.Faint();
		}
		if (user.Fainted()) {
			user.Faint();
		}
		thisMove.EffectAfterHit(user, target, turnEffects);
		if (target.Fainted()) {
			target.Faint();
		}
		if (user.Fainted()) {
			user.Faint();
		}
		if (!user.Fainted() && target.Fainted()) {
			if (destinyBond && target.IsOpposing(user.index)) {
				Debug.Log(string.Format("[Lingering effect triggered] {0}'s Destiny Bond", target.String()));
				battle.Display(string.Format("{0} took its attacker down with it!", target.String()));
				user.ReduceHP(user.hp);
				user.Faint();
				battle.JudgeCheckpoint(user);
			}
		}
		EffectAfterHit(user, target, thisMove, turnEffects);
		for (int j=0; j < 4; j++) {
			battle.battlers[j].BerryCureCheck();
		}
		target.UpdateTargetedMove(thisMove, user);
	}

	public void UseMoveSimple(int moveid, int index=-1, int target=-1) {
		int useMoveChoice = 1;
		int indexChoice = index;
		BattleMove moveChoice = BattleMove.FromBattleMove(battle, new Moves.Move(moveid));
		moveChoice.pp = -1;
		int targetChoice = target;
		Debug.Log(string.Format("{0} used simple move {1}", String(), moveChoice.name));
		UseMove(useMoveChoice, indexChoice, moveChoice, targetChoice, true);
		return;
	}

	public void UseMove(int useMoveChoice, int indexChoice, BattleMove moveChoice, int targetChoice, bool specialUsage=false) {
		// TODO: lastMoveUsed is not to be updated on nested calls
		// Note: user.lastMoveUsedType IS to be updated on nested calls; is used for Conversion 2
		Dictionary<int, int> turnEffects = new Dictionary<int, int>();
		turnEffects[Effects.SpecialUsage] = specialUsage ? 1 : 0;
		turnEffects[Effects.SkipAccuracyCheck] = (specialUsage && moveChoice != battle.struggle) ? 1 : 0;
		turnEffects[Effects.PassedTrying] = 0;
		turnEffects[Effects.TotalDamage] = 0;
		BeginTurn(useMoveChoice, indexChoice, moveChoice, targetChoice);
		if (effects[Effects.TwoTurnAttack] > 0 || effects[Effects.HyperBeam] > 0 || effects[Effects.Outrage] > 0 || effects[Effects.Rollout] > 0 || effects[Effects.Uproar] > 0 || effects[Effects.Bide] > 0) {
			moveChoice = BattleMove.FromBattleMove(battle, new Moves.Move(currentMove));
			turnEffects[Effects.SpecialUsage] = 1;
			Debug.Log(string.Format("Continuing multi-turn move {0}", moveChoice.name));
		} else if (effects[Effects.Encore] > 0 && indexChoice >= 0) {
			if (battle.CanShowCommands(index) && battle.CanChooseMove(index, effects[Effects.EncoreIndex], false)) {
				if (indexChoice != effects[Effects.EncoreIndex]) {
					indexChoice = effects[Effects.EncoreIndex];
					moveChoice = moves[effects[Effects.EncoreIndex]];
					targetChoice = -1;
				}
				Debug.Log(string.Format("Using Encored move {0}", moveChoice.name));
			}
		}
		BattleMove thisMove = moveChoice;
		if (thisMove == null || thisMove.id == 0) {
			return;
		}
		if (turnEffects[Effects.SpecialUsage] == 0) {
			// TODO: Quick Claw Message
		}
		if (HasWorkingAbility(Abilities.STANCECHANGE) && species == Species.AEGISLASH && effects[Effects.Transform] == 0) {
			if (thisMove.IsDamaging() && form != 1) {
				form = 1;
				Update(true);
				battle.scene.ChangePokemon(this, pokemon);
				battle.Display(string.Format("{0} changed to Blade Forme!", String()));
				Debug.Log(string.Format("[Form Changed] {0} changed to Blade Forme!", String()));
			} else if (thisMove.id == Moves.KINGSSHIELD && form != 0) {
				form = 0;
				Update(true);
				battle.scene.ChangePokemon(this, pokemon);
				battle.Display(string.Format("{0} changed to Shield Forme!", String()));
				Debug.Log(string.Format("[Form Changed] {0} changed to Shield Forme!", String()));
			}
		}
		lastRoundMoved = battle.turnCount;
		if (!TryUseMove(useMoveChoice, indexChoice, moveChoice, targetChoice, thisMove, turnEffects)) {
			lastMoveUsed = -1;
			lastMoveUsedType = -1;
			if (turnEffects[Effects.SpecialUsage] == 0) {
				if (effects[Effects.TwoTurnAttack] == 0) {
					lastMoveUsedSketch = -1;
					lastRegularMoveUsed = -1;
				}
				CancelMoves();
				battle.GainEXP();
				EndTurn(useMoveChoice, indexChoice, moveChoice, targetChoice);
				battle.Judge();
				return;
			}
		}
		if (thisMove == null || thisMove.id == 0) {
			return;
		}
		if (turnEffects[Effects.SpecialUsage] != 0) {
			if (!ReducePP(thisMove)) {
				battle.Display(string.Format("{0} used {2}!", String(), thisMove.name));
				battle.Display(string.Format("But there was no PP left for the move!"));
				lastMoveUsed = -1;
				lastMoveUsedType = -1;
				if (effects[Effects.TwoTurnAttack] == 0) {
					lastMoveUsedSketch = -1;
				}
				lastRegularMoveUsed = -1;
				EndTurn(useMoveChoice, indexChoice, moveChoice, targetChoice);
				battle.Judge();
				Debug.Log(string.Format("[Move Failed] {0} has no PP left", thisMove.name));
				return;
			}
		}
		if (thisMove.TwoTurnAttack(this)) {
			effects[Effects.TwoTurnAttack] = thisMove.id;
			currentMove = thisMove.id;
		} else {
			effects[Effects.TwoTurnAttack] = 0;
		}
		if (lastMoveUsed == thisMove.id) {
			effects[Effects.Metronome] = effects[Effects.Metronome] + 1;
		} else {
			effects[Effects.Metronome] = 0;
		}
		switch (thisMove.DisplayUseMessage(this)) {
			case 2:
			return;
			case 1:
			lastMoveUsed = thisMove.id;
			lastMoveUsedType = thisMove.GetType(thisMove.type, this, null);
			if (turnEffects[Effects.SpecialUsage] == 0) {
				if (effects[Effects.TwoTurnAttack] == 0) {
					lastMoveUsedSketch = thisMove.id;
				}
				lastRegularMoveUsed = thisMove.id;
			}
			battle.lastMoveUsed = thisMove.id;
			battle.lastMoveUser = index;
			battle.successStates[index].UseState = 2;
			battle.successStates[index].TypeModifier = 8;
			return;
			case -1:
			lastMoveUsed = thisMove.id;
			lastMoveUsedType = thisMove.GetType(thisMove.type, this, null);
			if (turnEffects[Effects.SpecialUsage] == 0) {
				if (effects[Effects.TwoTurnAttack] == 0) {
					lastMoveUsedSketch = thisMove.id;
				}
				lastRegularMoveUsed = thisMove.id;
			}
			battle.lastMoveUsed = thisMove.id;
			battle.lastMoveUser = index;
			battle.successStates[index].UseState = 2;
			battle.successStates[index].TypeModifier = 8;
			Debug.Log(string.Format("[Move Failed] {0} was hurt while readying Focus Punch", String()));
			return;
		}
		List<Battler> targets = new List<Battler>();
		Battler user = FindUser(useMoveChoice, indexChoice, moveChoice, targetChoice, targets);
		battle.successStates[user.index].UseState = 1;
		battle.successStates[user.index].TypeModifier = 8;
		if (!thisMove.OnStartUse(user)) {
			Debug.Log(string.Format("[Move Failed] Failed OnStartUse (Function Code {0})", thisMove.function));
			user.lastMoveUsed = thisMove.id;
			lastMoveUsedType = thisMove.GetType(thisMove.type, this, null);
			if (turnEffects[Effects.SpecialUsage] == 0) {
				if (effects[Effects.TwoTurnAttack] == 0) {
					lastMoveUsedSketch = thisMove.id;
				}
				lastRegularMoveUsed = thisMove.id;
			}
			battle.lastMoveUsed = thisMove.id;
			battle.lastMoveUser = user.index;
		}
		if (thisMove.IsDamaging()) {
			switch (battle.GetWeather()) {
				case Weather.HEAVYRAIN:
				if (thisMove.GetType(thisMove.type, user, null) == Types.FIRE) {
					Debug.Log(string.Format("[Move Failed] Primoridal Sea's rain cancelled the Fire-type {0}", thisMove.name));
					battle.Display("The Fire-type attack fizzled out in the heavy rain!");
					user.lastMoveUsed = thisMove.id;
					lastMoveUsedType = thisMove.GetType(thisMove.type, this, null);
					if (turnEffects[Effects.SpecialUsage] == 0) {
						if (effects[Effects.TwoTurnAttack] == 0) {
							lastMoveUsedSketch = thisMove.id;
						}
						lastRegularMoveUsed = thisMove.id;
					}
					battle.lastMoveUsed = thisMove.id;
					battle.lastMoveUser = user.index;
				}
				return;
				case Weather.HARSHSUN:
				if (thisMove.GetType(thisMove.type, user, null) == Types.FIRE) {
					Debug.Log(string.Format("[Move Failed] Desolate Land's sun cancelled the Water-type {0}", thisMove.name));
					battle.Display("The Water-type attack evaporated in the harsh sunlight!");
					user.lastMoveUsed = thisMove.id;
					lastMoveUsedType = thisMove.GetType(thisMove.type, this, null);
					if (turnEffects[Effects.SpecialUsage] == 0) {
						if (effects[Effects.TwoTurnAttack] == 0) {
							lastMoveUsedSketch = thisMove.id;
						}
						lastRegularMoveUsed = thisMove.id;
					}
					battle.lastMoveUsed = thisMove.id;
					battle.lastMoveUser = user.index;
				}
				return;
			}
		}
		if (user.effects[Effects.Powder] != 0 && thisMove.GetType(thisMove.type, user, null) == Types.FIRE) {
			Debug.Log(string.Format("[Lingering effect triggered] {0}'s Powder cancelled the Fire move", String()));
			battle.CommonAnimation("Powder", user, null);
			battle.Display(string.Format("When the flame touched the powder on the Pokémon, it exploded!"));
			if (!user.HasWorkingAbility(Abilities.MAGICGUARD)) {
				user.ReduceHP(1 + user.totalHP/4);
			}
			user.lastMoveUsed = thisMove.id;
			lastMoveUsedType = thisMove.GetType(thisMove.type, this, null);
			if (turnEffects[Effects.SpecialUsage] == 0) {
				if (effects[Effects.TwoTurnAttack] == 0) {
					lastMoveUsedSketch = thisMove.id;
				}
				lastRegularMoveUsed = thisMove.id;
			}
			battle.lastMoveUsed = thisMove.id;
			battle.lastMoveUser = user.index;
			if (user.Fainted()) {
				user.Faint();
			}
			EndTurn(useMoveChoice, indexChoice, moveChoice, targetChoice);
			return;
		}
		if (user.HasWorkingAbility(Abilities.PROTEAN) && thisMove.function != 0xAE && thisMove.function != 0xAF && thisMove.function != 0xB0 && thisMove.function != 0xB3 && thisMove.function != 0xB4 && thisMove.function != 0xB5 && thisMove.function != 0xB6) {
			int moveType = thisMove.GetType(thisMove.type, user, null);
			if (!user.HasType(moveType)) {
				string typeName = Types.GetName(moveType);
				Debug.Log(string.Format("[Ability Triggered] {0}'s Protean made it {1}-type", String(), typeName));
				user.type1 = moveType;
				user.type2 = moveType;
				user.effects[Effects.Type3] = -1;
				battle.Display(string.Format("{0} transformed into the {1} type", String(), typeName));
			}
		}
		if (targets.Count == 0) {
			user = ChangeUser(thisMove, user);
			if (thisMove.target == Targets.SingleNonUser || thisMove.target == Targets.RandomOpposing || thisMove.target == Targets.AllOpposing || thisMove.target == Targets.AllNonUsers || thisMove.target == Targets.Partner || thisMove.target == Targets.UserOrPartner || thisMove.target == Targets.SingleOpposing || thisMove.target == Targets.OppositeOpposing) {
				battle.Display(string.Format("But there was no target..."));
			}
		} else {
			bool showAnimation = true;
			List<Battler> allTargets = new List<Battler>();
			for (int i=0; i<targets.Count; i++) {
				if (!targets.Contains(battle.battlers[targets[i].index])) {
					allTargets.Add(targets[i]);
				}
			}
			int j = 0;
			while (j<targets.Count) {
				Battler[] userAndTarget = new Battler[2]{user, battle.battlers[targets[j].index]};
				bool success = ChangeTarget(thisMove, userAndTarget, targets);
				user = userAndTarget[0];
				Battler target = userAndTarget[1];
				if (j == 0 && thisMove.target == Targets.AllOpposing) {
					AddTarget(targets, target.Partner());
				}
				if (!success) {
					j += 1;
					continue;
				}
				int numHits = thisMove.NumHits(user);
				target.damageState.Reset();
				ProcessMoveAgainstTarget(thisMove, user, target, numHits, turnEffects, false, allTargets, showAnimation);
				showAnimation = false;
				j += 1;
			}
		}
		List<int> switched = new List<int>();
		if (!user.Fainted()) {
			for (int i=0; i<4; i++) {
				if (battle.battlers[i].effects[Effects.Roar] != 0) {
					battle.battlers[i].effects[Effects.Roar] = 0;
					battle.battlers[i].effects[Effects.Uturn] = 0;
					if (battle.battlers[i].Fainted()) {
						continue;
					}
					if (!battle.CanSwitch(i, -1, false)) {
						continue;
					}
					List<int> choices = new List<int>();
					Battler[] party = battle.Party(i);
					for (int j=0; j<party.Length; j++) {
						if (battle.CanSwitchLax(i, j, false)) {
							choices.Add(j);
						}
					}
					if (choices.Count > 0) {
						int newPoke = choices[battle.Rand(choices.Count)];
						int newPokeName = newPoke;
						if (party[newPoke].pokemon.Ability() == Abilities.ILLUSION) {
							newPokeName = battle.GetLastPokemonInTeam(i);
						}
						switched.Add(i);
						battle.battlers[i].ResetForm();
						battle.RecallAndReplace(i, newPoke, newPokeName, false, user.HasMoldBreaker());
						battle.Display(string.Format("{0} was dragged out!", battle.battlers[i].String()));
						battle.useMoveChoice[i] = 0;
						battle.indexChoice[i] = 0;
						battle.moveChoice[i] = null;
						battle.targetChoice[i] = -1;
					}
				}
			}
			for (int i=0; i < battle.Priority().Length; i++) {
				if (!switched.Contains(battle.Priority()[i].index)) {
					continue;
				}
				battle.Priority()[i].AbilitiesOnSwitchIn(true);
			}
		}
		switched = new List<int>();
		for (int i=0; i<4; i++) {
			if (battle.battlers[i].effects[Effects.Uturn] != 0) {
				battle.battlers[i].effects[Effects.Uturn] = 0;
				battle.battlers[i].effects[Effects.Roar] = 0;
				if (!battle.battlers[i].Fainted() && battle.CanChooseNonActive(i) && !battle.AllFainted(battle.OpposingParty(i))) {
					battle.Display(string.Format("{0} went back to {1}", battle.battlers[i].String(), battle.GetOwner(i).name));
					int newPoke = 0;
					newPoke = battle.SwitchInBetween(i, true, false);
					int newPokeName = newPoke;
					if (battle.Party(i)[newPoke].pokemon.Ability() == Abilities.ILLUSION) {
						newPokeName = battle.GetLastPokemonInTeam(i);
					}
					switched.Add(i);
					battle.RecallAndReplace(i, newPoke, newPokeName, battle.battlers[i].effects[Effects.BatonPass] != 0);
					battle.useMoveChoice[i] = 0;
					battle.indexChoice[i] = 0;
					battle.moveChoice[i] = null;
					battle.targetChoice[i] = -1;
				}
			}
		}
		for (int i=0; i < battle.Priority().Length; i++) {
			if (!switched.Contains(battle.Priority()[i].index)) {
				continue;
			}
			battle.Priority()[i].AbilitiesOnSwitchIn(true);
		}
		if (user.effects[Effects.BatonPass] != 0) {
			user.effects[Effects.BatonPass] = 0;
			if (!user.Fainted() && battle.CanChooseNonActive(user.index) && !battle.AllFainted(battle.Party(user.index))) {
				int newPoke = 0;
				newPoke = battle.SwitchInBetween(user.index, true, false);
				int newPokeName = newPoke;
				if (battle.Party(user.index)[newPoke].pokemon.Ability() == Abilities.ILLUSION) {
					newPokeName = battle.GetLastPokemonInTeam(user.index);
				}
				user.ResetForm();
				battle.RecallAndReplace(user.index, newPoke, newPokeName, true);
				battle.useMoveChoice[user.index] = 0;
				battle.indexChoice[user.index] = 0;
				battle.moveChoice[user.index] = null;
				battle.targetChoice[user.index] = -1;
				user.AbilitiesOnSwitchIn(true);
			}
		}
		user.lastMoveUsed = thisMove.id;
		user.lastMoveUsedType = thisMove.GetType(thisMove.type, user, null);
		if (turnEffects[Effects.SpecialUsage] == 0) {
			if (user.effects[Effects.TwoTurnAttack] == 0) {
				user.lastMoveUsedSketch = thisMove.id;
			}
			user.lastRegularMoveUsed = thisMove.id;
			if (!user.movesUsed.Contains(thisMove.id)) {
				user.movesUsed.Add(thisMove.id);
			}
		}
		battle.lastMoveUsed = thisMove.id;
		battle.lastMoveUser = user.index;
		battle.GainEXP();
		for (int i=0; i<4; i++) {
			battle.successStates[i].UpdateSkill();
		}
		EndTurn(useMoveChoice, indexChoice, moveChoice, targetChoice);
		battle.Judge();
		return;
	}

	public void CancelMoves() {
		if (effects[Effects.TwoTurnAttack] > 0) {
			effects[Effects.TwoTurnAttack] = 0;
		}
		effects[Effects.Outrage] = 0;
		effects[Effects.Rollout] = 0;
		effects[Effects.Uproar] = 0;
		effects[Effects.Bide] = 0;
		currentMove = 0;
		effects[Effects.FuryCutter] = 0;
		Debug.Log("Cancelled using the move");
	}

	/******************
	* Turn Processing *
	******************/
	public void BeginTurn(int useMoveChoice, int indexChoice, BattleMove moveChoice, int targetChoice) {
		effects[Effects.DestinyBond] = 0;
		effects[Effects.Grudge] = 0;
		effects[Effects.ParentalBond] = 0;
		if (effects[Effects.Encore] > 0 && moves[effects[Effects.EncoreIndex]].id != effects[Effects.EncoreMove]) {
			Debug.Log("Resetting Encore effect");
			effects[Effects.Encore] = 0;
			effects[Effects.EncoreIndex] = 0;
			effects[Effects.EncoreMove] = 0;
		}
		if (status == Statuses.SLEEP && !HasWorkingAbility(Abilities.SOUNDPROOF)) {
			for (int i=0; i<4; i++) {
				if (battle.battlers[i].effects[Effects.Uproar] > 0) {
					CureStatus(false);
					battle.Display(string.Format("{0} woke up in the uproar!", String()));
				}
			}
		}
	}

	public void EndTurn(int useMoveChoice, int indexChoice, BattleMove moveChoice, int targetChoice) {
		if (effects[Effects.ChoiceBand] < 0 && lastMoveUsed >= 0 && !Fainted() && (
			HasWorkingItem(Items.CHOICEBAND) || HasWorkingItem(Items.CHOICESPECS) || HasWorkingItem(Items.CHOICESCARF))) {
			effects[Effects.ChoiceBand] = lastMoveUsed;
		}
		battle.PrimordialWeather();
		for (int i=0; i<4; i++) {
			battle.battlers[i].BerryCureCheck();
		}
		for (int i=0; i<4; i++) {
			battle.battlers[i].AbilityCureCheck();
		}
		for (int i=0; i<4; i++) {
			battle.battlers[i].AbilitiesOnSwitchIn(false);
		}
		for (int i=0; i<4; i++) {
			battle.battlers[i].CheckForm();
		}
	}

	public bool ProcessTurn(int useMoveChoice, int indexChoice, BattleMove moveChoice, int targetChoice) {
		if (Fainted()) {
			return false;
		}
		if (battle.opponent == null && battle.IsOpposing(index) && battle.rules["alwaysflee"] != 0 && battle.CanRun(index)) {
			BeginTurn(useMoveChoice, indexChoice, moveChoice, targetChoice);
			battle.Display(string.Format("{0} fled!", String()));
			battle.decision = 3;
			EndTurn(useMoveChoice, indexChoice, moveChoice, targetChoice);
			Debug.Log(string.Format("[Escape] {0} fled", String()));
			return true;
		}
		if (useMoveChoice != 1) {
			BeginTurn(useMoveChoice, indexChoice, moveChoice, targetChoice);
			EndTurn(useMoveChoice, indexChoice, moveChoice, targetChoice);
			return false;
		}
		if (effects[Effects.Pursuit] != 0) {
			effects[Effects.Pursuit] = 0;
			CancelMoves();
			EndTurn(useMoveChoice, indexChoice, moveChoice, targetChoice);
			battle.Judge();
			return false;
		}
		Debug.Log(string.Format("{0} used {1}", String(), moveChoice.name));
		return true;
	}

	/********
	* Sleep *
	********/
	public bool CanSleep(Battler attacker, bool showMessages, BattleMove move=null, bool ignoreStatus=false) {
		if (Fainted()) {
			return false;
		}
		bool selfSleep = (attacker != null && attacker.index == index);
		if (!ignoreStatus && status == Statuses.SLEEP) {
			if (showMessages) {
				battle.Display(string.Format("{0} is already asleep!"));
			}
			return false;
		}
		if (!selfSleep) {
			if (status != 0 || (
				effects[Effects.Substitute] > 0 && (
					move == null || !move.IgnoresSubstitute(attacker)))) {
				if (showMessages) {
					battle.Display("But it failed!");
				}
			}
		}
		if (!IsAirborne(attacker != null && attacker.HasMoldBreaker())) {
			if (battle.field.effects[Effects.ElectricTerrain] > 0) {
				if (showMessages) {
					battle.Display(string.Format("The Electric Terrain prevented {0} from falling asleep!", String(true)));
				}
			} else if (battle.field.effects[Effects.MistyTerrain] > 0) {
				if (showMessages) {
					battle.Display(string.Format("The Misty Terrain prevented {0} from falling asleep!", String(true)));
				}
			}
		}
		if ((attacker != null && attacker.HasMoldBreaker()) || !HasWorkingAbility(Abilities.SOUNDPROOF)) {
			for (int i=0; i<4; i++) {
				if (battle.battlers[i].effects[Effects.Uproar] > 0) {
					if (showMessages) {
						battle.Display(string.Format("But the uproar kept {0} awake", String(true)));
					}
					return false;
				}
			}
		}
		if (attacker == null || attacker.index == index || !attacker.HasMoldBreaker()) {
			if (HasWorkingAbility(Abilities.VITALSPIRIT) || HasWorkingAbility(Abilities.INSOMNIA) || HasWorkingAbility(Abilities.SWEETVEIL) || (HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) || (HasWorkingAbility(Abilities.LEAFGUARD) && (
				battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN))) {
				string abilityName = Abilities.GetName(ability);
				if (showMessages) {
					battle.Display(string.Format("{0} stayed awake using its {1}", String(), abilityName));
				}
				return false;
			}
		}
		if (Partner().HasWorkingAbility(Abilities.SWEETVEIL) || (Partner().HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS))) {
			string abilityName = Abilities.GetName(Partner().ability);
			if (showMessages) {
				battle.Display(string.Format("{0} stayed awake using its partner's {1}", String(), abilityName));
			}
			return false;
		}
		if (!selfSleep) {
			if (OwnSide().effects[Effects.Safeguard] > 0 && (attacker == null || !attacker.HasWorkingAbility(Abilities.INFILTRATOR))) {
				if (showMessages) {
					battle.Display(string.Format("{0}'s team is protected by Safeguard!", String()));
				}
				return false;
			}
		}
		return true;
	}

	public bool CanSleepYawn() {
		if (status != 0) {
			return false;
		}
		if (!HasWorkingAbility(Abilities.SOUNDPROOF)) {
			for (int i=0; i<4; i++) {
				if (battle.battlers[i].effects[Effects.Uproar] > 0) {
					return false;
				}
			}
		}
		if (!IsAirborne()) {
			if (battle.field.effects[Effects.ElectricTerrain] > 0) {
				return false;
			}
			if (battle.field.effects[Effects.MistyTerrain] > 0) {
				return false;
			}
		}
		if (HasWorkingAbility(Abilities.VITALSPIRIT) || HasWorkingAbility(Abilities.INSOMNIA) || HasWorkingAbility(Abilities.SWEETVEIL) || (HasWorkingAbility(Abilities.LEAFGUARD) && (
			battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN))) {
			return false;
		}
		if (Partner().HasWorkingAbility(Abilities.SWEETVEIL)) {
			return false;
		}
		return true;
	}

	public void Sleep(string msg="") {
		status = Statuses.SLEEP;
		statusCount = 2 + battle.Rand(3);
		if (HasWorkingAbility(Abilities.EARLYBIRD)) {
			statusCount = statusCount/2;
		}
		CancelMoves();
		battle.CommonAnimation("Sleep", this, null);
		if (msg != "") {
			battle.Display(msg);
		} else {
			battle.Display(string.Format("{0} fell asleep!", String()));
		}
		Debug.Log(string.Format("[Status change] {0} fell asleep ({1} turns)", String(), statusCount));
	}

	public void SleepSelf(int duration=-1) {
		status = Statuses.SLEEP;
		if (duration > 0) {
			statusCount = duration;
		} else {
			statusCount = 2 + battle.Rand(3);
		}
		if (HasWorkingAbility(Abilities.EARLYBIRD)) {
			statusCount = statusCount/2;
		}
		CancelMoves();
		battle.CommonAnimation("Sleep", this, null);
		Debug.Log(string.Format("[Status change] {0} fell asleep ({1} turns)", String(), statusCount));
	}

	/*********
	* Poison *
	*********/
	public bool CanPoison(Battler attacker, bool showMessages, BattleMove move=null) {
		if (Fainted()) {
			return false;
		}
		if (status == Statuses.POISON) {
			if (showMessages) {
				battle.Display(string.Format("{0} is already poisoned.", String()));
			}
			return false;
		}
		if (status != 0 || (
			effects[Effects.Substitute] > 0 && (
				move == null || !move.IgnoresSubstitute(attacker)))) {
			if (showMessages) {
				battle.Display("But it failed!");
			}
			return false;
		}
		if ((HasType(Types.POISON) || HasType(Types.STEEL)) && !HasWorkingItem(Items.RINGTARGET)) {
			if (showMessages) {
				battle.Display(string.Format("It doesn't affect {0}...", String(true)));
			}
			return false;
		}
		if (battle.field.effects[Effects.MistyTerrain] > 0 && !IsAirborne(attacker && attacker.HasMoldBreaker())) {
			if (showMessages) {
				battle.Display(string.Format("The Misty Terrain prevented {0} from being poisoned!", String(true)));
			}
			return false;
		}
		if (attacker == null || !attacker.HasMoldBreaker()) {
			if (HasWorkingAbility(Abilities.IMMUNITY) || (HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) || (HasWorkingAbility(Abilities.LEAFGUARD) && (
				battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN))) {
				if (showMessages) {
					battle.Display(string.Format("{0}'s {1} prevents poisoning!", String(), Abilities.GetName(ability)));
				}
				return false;
			}
			if (Partner().HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) {
				string abilityName = Abilities.GetName(Partner().ability);
				if (showMessages) {
					battle.Display(string.Format("{0}'s partner's {1} prevents poisoning!", String(), abilityName));
				}
				return false;
			}
		}
		if (OwnSide().effects[Effects.Safeguard] > 0 && (attacker == null || !attacker.HasWorkingAbility(Abilities.INFILTRATOR))) {
			if (showMessages) {
				battle.Display(string.Format("{0}'s team is protected by Safeguard!", String()));
			}
			return false;
		}
		return true;
	}

	public bool CanPoisonSynchronize(Battler opponent) {
		if (Fainted()) {
			return false;
		}
		if ((HasType(Types.POISON) || HasType(Types.STEEL)) && !HasWorkingItem(Items.RINGTARGET)) {
			battle.Display(string.Format("{0}'s {1} has not effect on {2}!", opponent.String(), Abilities.GetName(opponent.ability), String(true)));
			return false;
		}
		if (status != 0) {
			return false;
		}
		if (HasWorkingAbility(Abilities.IMMUNITY) || (HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) || (HasWorkingAbility(Abilities.LEAFGUARD) && (
			battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN))) {
			battle.Display(string.Format("{0}'s {1} prevents poisoning!", String(), Abilities.GetName(ability)));
			return false;
		}
		if (Partner().HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) {
			string abilityName = Abilities.GetName(Partner().ability);
			battle.Display(string.Format("{0}'s partner's {1} prevents poisoning!", String(), abilityName));
			return false;
		}
		return true;
	}

	public bool CanPoisonSpikes(bool moldbreaker=false) {
		if (Fainted()) {
			return false;
		}
		if (status != 0) {
			return false;
		}
		if (HasType(Types.POISON) || HasType(Types.STEEL)) {
			return false;
		}
		if (!moldbreaker) {
			if (HasWorkingAbility(Abilities.IMMUNITY) || (HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) || (Partner().HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS))) {
				return false;
			}
			if (HasWorkingAbility(Abilities.LEAFGUARD) && (
				battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN)) {
				return false;
			}
		}
		if (OwnSide().effects[Effects.Safeguard] > 0) {
			return false;
		}
		return true;
	}

	public void Poison(Battler attacker=null, string msg="", bool toxic=false) {
		status = Statuses.POISON;
		statusCount = toxic ? 1 : 0;
		effects[Effects.Toxic] = 0;
		battle.CommonAnimation("Poison", this, null);
		if (msg != "") {
			battle.Display(msg);
		} else {
			if (toxic) {
				battle.Display(string.Format("{0} was badly poisoned!", String()));
			} else {
				battle.Display(string.Format("{0} was poisoned!", String()));
			}
		}
		if (toxic) {
			Debug.Log(string.Format("[Status Change] {0} was badly poisoned!", String()));
		} else {
			Debug.Log(string.Format("[Status Change] {0} was poisoned!", String()));
		}
		if (attacker != null && index != attacker.index && HasWorkingAbility(Abilities.SYNCHRONIZE)) {
			if (attacker.CanPoisonSynchronize(this)) {
				Debug.Log(string.Format("[Ability Triggered] {0}'s Synchronize", String()));
				attacker.Poison(null, string.Format("{0}'s {1} poisoned {2}", String(), Abilities.GetName(ability), attacker.String(true)), toxic);
			}
		}
	}

	/*******
	* Burn *
	*******/
	public bool CanBurn(Battler attacker, bool showMessages, BattleMove move=null) {
		if (Fainted()) {
			return false;
		}
		if (status != 0 || (
			effects[Effects.Substitute] > 0 && (move == null || !move.IgnoresSubstitute(attacker)))) {
			if (showMessages) {
				battle.Display(string.Format("But it failed!"));
			}
			return false;
		}
		if (battle.field.effects[Effects.MistyTerrain] > 0 && !IsAirborne(attacker != null && attacker.HasMoldBreaker())) {
			if (showMessages) {
				battle.Display(string.Format("The Misty Terrain prevented {0} from being burned!", String(true)));
			}
			return false;
		}
		if (HasType(Types.FIRE) && !HasWorkingItem(Items.RINGTARGET)) {
			if (showMessages) {
				battle.Display(string.Format("It doesn't affect {0}...", String(true)));
			}
			return false;
		}
		if (attacker == null || !attacker.HasMoldBreaker()) {
			if (HasWorkingAbility(Abilities.WATERVEIL) || (HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) || (
				HasWorkingAbility(Abilities.LEAFGUARD) && 
				(
					battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN))) {
				if (showMessages) {
					battle.Display(string.Format("{0}'s {1} prevents burns!", String(), Abilities.GetName(ability)));
				}
				return false;
			}
			if (Partner().HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) {
				string abilityName = Abilities.GetName(Partner().ability);
				if (showMessages) {
					battle.Display(string.Format("{0}'s partner's {1} prevents burns!", String(), abilityName));
				}
				return false;
			}
		}
		if (OwnSide().effects[Effects.Safeguard] > 0 && (attacker == null || attacker.HasWorkingAbility(Abilities.INFILTRATOR))) {
			if (showMessages) {
				battle.Display(string.Format("{0}'s team is protected by Safeguard!", String()));
			}
			return false;
		}
		return true;
	}

	public bool CanBurnSynchronize(Battler opponent) {
		if (Fainted()) {
			return false;
		}
		if (status != 0) {
			return false;
		}
		if (HasType(Types.FIRE) && !HasWorkingItem(Items.RINGTARGET)) {
			battle.Display(string.Format("{0}'s {1} had no effect of {2}!", opponent.String(), Abilities.GetName(opponent.ability), String(true)));
			return false;
		}
		if (HasWorkingAbility(Abilities.WATERVEIL) || (HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) || (HasWorkingAbility(Abilities.LEAFGUARD) && 
			(
				battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN))) {
			battle.Display(string.Format("{0}'s {1} prevents {2}'s {3} from working!", String(), Abilities.GetName(ability), opponent.String(true), Abilities.GetName(opponent.ability)));
			return false;
		}
		if (Partner().HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) {
			battle.Display(string.Format("{0}'s {1} prevents {2}'s {3} from working!", Partner().String(), Abilities.GetName(Partner().ability), opponent.String(true), Abilities.GetName(opponent.ability)));
			return false;
		}
		return true;
	}

	public void Burn(Battler attacker=null, string msg="") {
		status = Statuses.BURN;
		statusCount = 0;
		battle.CommonAnimation("Burn", this, null);
		if (msg != "") {
			battle.Display(string.Format(msg));
		} else {
			battle.Display(string.Format("{0} was burned!", String()));
		}
		Debug.Log(string.Format("[Status Changed] {0} was burned", String()));
		if (attacker != null && index != attacker.index && HasWorkingAbility(Abilities.SYNCHRONIZE)) {
			Debug.Log(string.Format("[Ability Triggered] {0}'s Synchronize", String()));
			attacker.Burn(null, string.Format("{0}'s {1} burned {2}!", String(), Abilities.GetName(ability), attacker.String(true)));
		}
	}

	/***********
	* Paralyze *
	***********/
	public bool CanParalyze(Battler attacker, bool showMessages, BattleMove move=null) {
		if (Fainted()) {
			return false;
		}
		if (status == Statuses.PARALYSIS) {
			if (showMessages) {
				battle.Display(string.Format("{0} is already paralyzed!", String()));
			}
			return false;
		}
		if (status != 0 || (
			effects[Effects.Substitute] > 0 && (
				move == null || move.IgnoresSubstitute(attacker)))) {
			if (showMessages) {
				battle.Display(string.Format("But it failed!"));
			}
			return false;
		}
		if (battle.field.effects[Effects.MistyTerrain] > 0 && !IsAirborne(attacker != null && attacker.HasMoldBreaker())) {
			if (showMessages) {
				battle.Display(string.Format("The Misty Terrain prevented {0} from being paralyzed!", String(true)));
			}
			return false;
		}
		if (HasType(Types.ELECTRIC) && !HasWorkingItem(Items.RINGTARGET) && Settings.USE_NEW_BATTLE_MECHANICS) {
			if (showMessages) {
				battle.Display(string.Format("It doesn't effect {0}...", String(true)));
			}
			return false;
		}
		if (attacker == null || attacker.HasMoldBreaker()) {
			if (HasWorkingAbility(Abilities.LIMBER) || (HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) || (HasWorkingAbility(Abilities.LEAFGUARD) && (
				battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN))) {
				if (showMessages) {
					battle.Display(string.Format("{0}'s {1} prevents paralysis!", String(), Abilities.GetName(ability)));
				}
				return false;
			}
			if (Partner().HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) {
				string abilityName = Abilities.GetName(Partner().ability);
				if (showMessages) {
					battle.Display(string.Format("{0}'s partner's {1} prevents paralysis!", String(), abilityName));
				}
				return false;
			}
		}
		if (OwnSide().effects[Effects.Safeguard] > 0 && (attacker == null || !attacker.HasWorkingAbility(Abilities.INFILTRATOR))) {
			if (showMessages) {
				battle.Display(string.Format("{0}'s team is protected by Safeguard!", String()));
			}
			return false;
		}
		return true;
	}

	public bool CanParalyzeSynchronize(Battler opponent) {
		if (status != 0) {
			return false;
		}
		if (battle.field.effects[Effects.MistyTerrain] > 0 && !IsAirborne()) {
			return false;
		}
		if (HasType(Types.ELECTRIC) && !HasWorkingItem(Items.RINGTARGET) && Settings.USE_NEW_BATTLE_MECHANICS) {
			return false;
		}
		if (HasWorkingAbility(Abilities.LIMBER) || (HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) || (HasWorkingAbility(Abilities.LEAFGUARD) && (
			battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN))) {
			battle.Display(string.Format("{0}'s {1} prevents {2}'s {3} from working!", String(), Abilities.GetName(ability), opponent.String(true), Abilities.GetName(opponent.ability)));
			return false;
		}
		if (Partner().HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) {
			battle.Display(string.Format("{0}'s {1} prevents {2}'s {3} from working!", String(), Abilities.GetName(Partner().ability), opponent.String(true), Abilities.GetName(opponent.ability)));
			return false;
		}
		return true;
	}

	public void Paralyze(Battler attacker=null, string msg="") {
		status = Statuses.PARALYSIS;
		statusCount = 0;
		battle.CommonAnimation("Paralysis", this, null);
		if (msg != "") {
			battle.Display(string.Format(msg));
		} else {
			battle.Display(string.Format("{0} is paralyzed! It may be unable to move!", String()));
		}
		Debug.Log(string.Format("[Status Change] {0} was paralyzed", String()));
		if (attacker != null && index != attacker.index && HasWorkingAbility(Abilities.SYNCHRONIZE)) {
			if (attacker.CanParalyzeSynchronize(this)) {
				Debug.Log(string.Format("[Ability Triggered] {0}'s Synchronize", String()));
				attacker.Paralyze(null, string.Format("{0}'s {1} paralyzed {2}! It may be unable to move!", String(), Abilities.GetName(ability), attacker.String(true)));
			}
		}
	}

	/*********
	* Freeze *
	*********/
	public bool CanFreeze(Battler attacker, bool showMessages, BattleMove move=null) {
		if (Fainted()) {
			return false;
		}
		if (status == Statuses.FROZEN) {
			if (showMessages) {
				battle.Display(string.Format("{0} is already frozen solid!", String()));
			}
			return false;
		}
		if (status != 0 || (
			effects[Effects.Substitute] > 0 && (
				move == null || !move.IgnoresSubstitute(attacker))) || battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN) {
			if (showMessages) {
				battle.Display(string.Format("But it failed!"));
			}
			return false;
		}
		if (HasType(Types.ICE) && !HasWorkingItem(Items.RINGTARGET)) {
			if (showMessages) {
				battle.Display(string.Format("It doesn't affect {0}...", String(true)));
			}
			return false;
		}
		if (battle.field.effects[Effects.MistyTerrain] > 0 && !IsAirborne(attacker != null && attacker.HasMoldBreaker())) {
			if (showMessages) {
				battle.Display(string.Format("The Misty Terrain prevented {0} form being frozen!", String(true)));
			}
			return false;
		}
		if (attacker == null || attacker.HasMoldBreaker()) {
			if (HasWorkingAbility(Abilities.MAGMAARMOR) || (HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) || (HasWorkingAbility(Abilities.LEAFGUARD) && (
				battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN))) {
				if (showMessages) {
					battle.Display(string.Format("{0}'s {1} prevents freezing!", String(), Abilities.GetName(ability)));
				}
				return false;
			}
			if (Partner().HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) {
				string abilityName = Abilities.GetName(Partner().ability);
				if (showMessages) {
					battle.Display(string.Format("{0}'s partner's {1} prevents freezing!", String(), abilityName));
				}
				return false;
			}
		}
		if (OwnSide().effects[Effects.Safeguard] > 0 && (attacker == null || !attacker.HasWorkingAbility(Abilities.INFILTRATOR))) {
			if (showMessages) {
				battle.Display(string.Format("{0}'s team is protected by Safeguard!", String()));
			}
			return false;
		}
		return true;
	}

	public void Freeze(string msg="") {
		status = Statuses.FROZEN;
		statusCount = 0;
		CancelMoves();
		battle.CommonAnimation("Frozen", this, null);
		if (msg != "") {
			battle.Display(string.Format(msg));
		} else {
			battle.Display(string.Format("{0} was frozen solid!", String()));
		}
		Debug.Log(string.Format("[Status Change] {0} was frozen", String()));
	}

	/******************************
	* Generalized Status Displays *
	******************************/
	public void ContinueStatus(bool showAnimation=true) {
		switch (status) {
			case Statuses.SLEEP:
			battle.CommonAnimation("Sleep", this, null);
			battle.Display(string.Format("{0} is fast asleep.", String()));
			break;
			case Statuses.POISON:
			battle.CommonAnimation("Poison", this, null);
			battle.Display(string.Format("{0} was hurt by poison!", String()));
			break;
			case Statuses.BURN:
			battle.CommonAnimation("Burn", this, null);
			battle.Display(string.Format("{0} was hurt by its burn!", String()));
			break;
			case Statuses.PARALYSIS:
			battle.CommonAnimation("Paralysis", this, null);
			battle.Display(string.Format("{0} is paralyzed! It can't move!", String()));
			break;
			case Statuses.FROZEN:
			battle.CommonAnimation("Frozen", this, null);
			battle.Display(string.Format("{0} is frozen solid!", String()));
			break;
		}
	}

	public void CureStatus(bool showMessages=true) {
		int oldStatus = status;
		status = 0;
		statusCount = 0;
		if (showMessages) {
			switch (oldStatus) {
				case Statuses.SLEEP:
				battle.Display(string.Format("{0} woke up!", String()));
				break;
				case Statuses.POISON:
				case Statuses.BURN:
				case Statuses.PARALYSIS:
				battle.Display(string.Format("{0} was cured!", String()));
				break;
				case Statuses.FROZEN:
				battle.Display(string.Format("{0} thawed out!", String()));
				break;
			}
		}
		Debug.Log(string.Format("[Status Change] {0}'s status was cured", String()));
	}

	/************
	* Confusion *
	************/
	public bool CanConfuse(Battler attacker, bool showMessages, BattleMove move=null) {
		if (Fainted()) {
			return false;
		}
		if (effects[Effects.Confusion] > 0) {
			if (showMessages) {
				battle.Display(string.Format("{0} is already confused!", String()));
			}
			return false;
		}
		if (effects[Effects.Substitute] > 0 && (move == null || !move.IgnoresSubstitute(attacker))) {
			if (showMessages) {
				battle.Display(string.Format("But it failed!"));
			}
			return false;
		}
		if (attacker == null || !attacker.HasMoldBreaker()) {
			if (HasWorkingAbility(Abilities.OWNTEMPO)) {
				if (showMessages) {
					battle.Display(string.Format("{0}'s {1} prevents confusion!", String(), Abilities.GetName(ability)));
				}
				return false;
			}
		}
		if (OwnSide().effects[Effects.Safeguard] > 0 && (attacker == null || !attacker.HasWorkingAbility(Abilities.INFILTRATOR))) {
			if (showMessages) {
				battle.Display(string.Format("{0}'s team is protected by Safeguard!", String()));
			}
			return false;
		}
		return true;
	}

	public bool CanConfuseSelf(bool showMessages) {
		if (Fainted()) {
			return false;
		}
		if (effects[Effects.Confusion] > 0) {
			if (showMessages) {
				battle.Display(string.Format("{0} is already confused!", String()));
			}
			return false;
		}
		if (HasWorkingAbility(Abilities.OWNTEMPO)) {
			if (showMessages) {
				battle.Display(string.Format("{0}'s {1} prevents confusion!", String(), Abilities.GetName(ability)));
			}
			return false;
		}
		return true;
	}

	public void Confuse() {
		effects[Effects.Confusion] = 2 + battle.Rand(4);
		battle.CommonAnimation("Confusion", this, null);
		Debug.Log(string.Format("[Lingering effect triggered] {0} became confused ({1} turns)", String(), effects[Effects.Confusion]));
	}

	public void ConfuseSelf() {
		if (CanConfuseSelf(false)) {
			effects[Effects.Confusion] = 2 + battle.Rand(4);
			battle.CommonAnimation("Confusion", this, null);
			battle.Display(string.Format("{0} became confused!", String()));
			Debug.Log(string.Format("[Lingering effect triggered] {0} became confused ({1} turns)", String(), effects[Effects.Confusion]));
		}
	}

	public void ContinueConfusion() {
		battle.CommonAnimation("Confusion", this, null);
		battle.DisplayBrief(string.Format("{0} is confused!", String()));
	}

	public void CureConfusion(bool showMessages=true) {
		effects[Effects.Confusion] = 0;
		if (showMessages) {
			battle.Display(string.Format("{0} snapped out of confusion!", String()));
		}
		Debug.Log(string.Format("[End of effect] {0} was cured of confusion", String()));
	}

	/*************
	* Attraction *
	*************/
	public bool CanAttract(Battler attacker, bool showMessages=true) {
		if (Fainted()) {
			return false;
		}
		if (attacker == null || attacker.Fainted()) {
			return false;
		}
		int agender = attacker.gender;
		int ogender = gender;
		if (agender == 2 || ogender == 2 || agender == ogender) {
			if (showMessages) {
				battle.Display(string.Format("But it failed!"));
			}
			return false;
		}
		if ((attacker == null || !attacker.HasMoldBreaker()) && HasWorkingAbility(Abilities.OBLIVIOUS)) {
			if (showMessages) {
				battle.Display(string.Format("{0}'s {1} prevents romance!", String(), Abilities.GetName(ability)));
			}
			return false;
		}
		return true;
	}

	public void Attract(Battler attacker, string msg="") {
		effects[Effects.Attract] = attacker.index;
		battle.CommonAnimation("Attract", this, null);
		if (msg != "") {
			battle.Display(string.Format(msg));
		} else {
			battle.Display(string.Format("{0} fell in love!", String()));
		}
		Debug.Log(string.Format("[Lingering effect triggered] {0} became infatuation (with {1})", String(), attacker.String(true)));
		if (HasWorkingItem(Items.DESTINYKNOT) && attacker.CanAttract(this, false)) {
			Debug.Log(string.Format("[Item triggered] {0}'s Destiny Knot", String()));
			attacker.Attract(this, string.Format("{0}'s {1} made {2} fall in love!", String(), Items.GetName(item), attacker.String(true)));
		}
	}

	public void AnnounceAttract(Battler seducer) {
		battle.CommonAnimation("Attract", this, null);
		battle.DisplayBrief(string.Format("{0} is in love with {1}!", String(), seducer.String()));
	}

	public void ContinueAttract() {
		battle.Display(string.Format("{0} is immobilized by love!", String()));
	}

	public void CureAttract() {
		effects[Effects.Attract] = -1;
		Debug.Log(string.Format("[End of Effect] {0} was cured of infatuation", String()));
	}

	/************
	* Flinching *
	************/
	public bool Flinch(Battler attacker=null) {
		if ((attacker == null || attacker.HasMoldBreaker()) && HasWorkingAbility(Abilities.INNERFOCUS)) {
			return false;
		}
		effects[Effects.Flinch] = 1;
		return true;
	}

	/***********************
	* Increase stat stages *
	***********************/
	public bool TooHigh(int stat) {
		return stages[stat] >= 6;
	}

	public bool CanIncreaseStatStage(int stat, Battler attacker=null, bool showMessages=false, BattleMove move=null, bool moldbreaker=false, bool ignoreContrary=false) {
		if (!moldbreaker) {
			if (attacker == null || attacker.index == index || !attacker.HasMoldBreaker()) {
				if (HasWorkingAbility(Abilities.CONTRARY) && !ignoreContrary) {
					return CanReduceStatStage(stat, attacker, showMessages, move, moldbreaker, true);
				}
			}
		}
		if (Fainted()) {
			return false;
		}
		if (TooHigh(stat)) {
			if (showMessages) {
				battle.Display(string.Format("{0}'s {1} won't go any higher!", String(), Stats.GetName(stat)));
			}
			return false;
		}
		return true;
	}

	public int IncreaseStatBasic(int stat, int increment, Battler attacker=null, bool moldbreaker=false, bool ignoreContrary=false) {
		if (!moldbreaker) {
			if (attacker == null || attacker.index == index || !attacker.HasMoldBreaker()) {
				if (HasWorkingAbility(Abilities.CONTRARY) && !ignoreContrary) {
					return ReduceStatBasic(stat, increment, attacker, moldbreaker, true);
				}
				if (HasWorkingAbility(Abilities.SIMPLE)) {
					increment *= 2;
				}
			}
		}
		increment = Math.Min(increment, 6-stages[stat]);
		Debug.Log(string.Format("[Stat change] {0}'s {1} rose by {2} (was {3}, now {4})", String(), Stats.GetName(stat), increment, stages[stat], stages[stat] + increment));
		stages[stat] += increment;
		return increment;
	}

	public bool IncreaseStat(int stat, int increment, Battler attacker, bool showMessages, BattleMove move=null, bool upAnim=true, bool moldbreaker=false, bool ignoreContrary=false) {
		if (!moldbreaker) {
			if (attacker == null || attacker.index == index || !attacker.HasMoldBreaker()) {
				if (HasWorkingAbility(Abilities.CONTRARY) && !ignoreContrary) {
					return ReduceStat(stat, increment, attacker, showMessages, move, upAnim, moldbreaker, true);
				}
			}
		}
		if (stat != Stats.ATTACK && stat != Stats.DEFENSE && stat != Stats.SPATK && stat != Stats.SPDEF && stat != Stats.SPEED && stat != Stats.EVASION && stat != Stats.ACCURACY) {
			return false;
		}
		if (CanIncreaseStatStage(stat, attacker, showMessages, move, moldbreaker, ignoreContrary)) {
			increment = IncreaseStatBasic(stat, increment, attacker, moldbreaker, ignoreContrary);
			if (increment > 0) {
				if (ignoreContrary) {
					if (upAnim) {
						battle.Display(string.Format("{0}'s {1} activated!", String(), Abilities.GetName(ability)));
					}
				}
				if (upAnim) {
					battle.CommonAnimation("StatUp", this, null);
				}
				string[] arrStatTexts = new string[3] {
					string.Format("{0}'s {1} rose!", String(), Stats.GetName(stat)),
					string.Format("{0}'s {1} rose sharply!", String(), Stats.GetName(stat)),
					string.Format("{0}'s {1} rose drastically!", String(), Stats.GetName(stat))
				};
				battle.Display(string.Format(arrStatTexts[Math.Min(2, increment-1)]));
				return true;
			}
		}
		return false;
	}

	public bool IncreaseStatWithCause(int stat, int increment, Battler attacker, string cause, bool showAnimation=true, bool showMessages=true, bool moldbreaker=false, bool ignoreContrary=false) {
		if (!moldbreaker) {
			if (attacker == null || attacker.index == index || !attacker.HasMoldBreaker()) {
				if (HasWorkingAbility(Abilities.CONTRARY) && !ignoreContrary) {
					return ReduceStatWithCause(stat, increment, attacker, cause, showAnimation, showMessages, moldbreaker, true);
				}
			}
		}
		if (stat != Stats.ATTACK && stat != Stats.DEFENSE && stat != Stats.SPATK && stat != Stats.SPDEF && stat != Stats.SPEED && stat != Stats.EVASION && stat != Stats.ACCURACY) {
			return false;
		}
		if (CanIncreaseStatStage(stat, attacker, false, null, moldbreaker, ignoreContrary)) {
			increment = IncreaseStatBasic(stat, increment, attacker, moldbreaker, ignoreContrary);
			if (increment > 0) {
				if (ignoreContrary) {
					if (showMessages) {
						battle.Display(string.Format("{0}'s {1} activated!", String(), Abilities.GetName(ability)));
					}
				}
				if (showAnimation) {
					battle.CommonAnimation("StatUp", this, null);
				}
				string[] arrStatTexts;
				if (attacker.index == index) {
					arrStatTexts = new string[3] {
						string.Format("{0}'s {1} raised its {2}!", String(), cause, Stats.GetName(stat)),
						string.Format("{0}'s {1} sharply raised its {2}!", String(), cause, Stats.GetName(stat)),
						string.Format("{0}'s {1} went way up!", String(), Stats.GetName(stat))
					};
				} else {
					arrStatTexts = new string[3] {
						string.Format("{0}'s {1} raised {2}'s {3}!", attacker.String(), cause, String(true), Stats.GetName(stat)),
						string.Format("{0}'s {1} sharply raised {2}'s {3}!", attacker.String(), cause, String(true), Stats.GetName(stat)),
						string.Format("{0}'s {1} drastically raised {2}'s {3}!", attacker.String(), cause, String(true), Stats.GetName(stat))
					};
				}
				if (showMessages) {
					battle.Display(string.Format(arrStatTexts[Math.Min(2, increment-1)]));
				}
				return true;
			}
		}
		return false;
	}

	/***********************
	* Reduce stat stages *
	***********************/
	public bool TooLow(int stat) {
		return stages[stat] <= -6;
	}

	public bool CanReduceStatStage(int stat, Battler attacker=null, bool showMessages=false, BattleMove move=null, bool moldbreaker=false, bool ignoreContrary=false) {
		if (!moldbreaker) {
			if (attacker == null || attacker.index == index || !attacker.HasMoldBreaker()) {
				if (HasWorkingAbility(Abilities.CONTRARY) && !ignoreContrary) {
					return CanIncreaseStatStage(stat, attacker, showMessages, move, moldbreaker, true);
				}
			}
		}
		if (Fainted()) {
			return true;
		}
		bool selfReduce = (attacker != null && attacker.index == index);
		if (!selfReduce) {
			if (effects[Effects.Substitute] > 0 && (move == null || move.IgnoresSubstitute(attacker))) {
				if (showMessages) {
					battle.Display(string.Format("But it Failed!"));
				}
				return false;
			}
			if (OwnSide().effects[Effects.Mist] > 0 && (attacker == null || !attacker.HasWorkingAbility(Abilities.INFILTRATOR))) {
				if (showMessages) {
					battle.Display(string.Format("{0} is protected by Mist!"));
				}
				return false;
			}
			if (!moldbreaker && (attacker == null || !attacker.HasMoldBreaker())) {
				if (HasWorkingAbility(Abilities.CLEARBODY) || HasWorkingAbility(Abilities.WHITESMOKE)) {
					string abilityName = Abilities.GetName(ability);
					if (showMessages) {
						battle.Display(string.Format("{0}'s {1} prevents stat loss!", String(), abilityName));
					}
					return false;
				}
				if (HasType(Types.GRASS)) {
					if (HasWorkingAbility(Abilities.FLOWERVEIL)) {
						string abilityName = Abilities.GetName(ability);
						if (showMessages) {
							battle.Display(string.Format("{0}'s {1} prevents stat loss!", String(), abilityName));
						}
						return false;
					} else if (Partner().HasWorkingAbility(Abilities.FLOWERVEIL)) {
						string abilityName = Abilities.GetName(Partner().ability);
						if (showMessages) {
							battle.Display(string.Format("{0}'s {1} prevents {2}'s stat loss!", Partner().String(), abilityName, String(true)));
						}
						return false;
					}
				}
				if (stat == Stats.ATTACK && HasWorkingAbility(Abilities.HYPERCUTTER)) {
					string abilityName = Abilities.GetName(ability);
					if (showMessages) {
						battle.Display(string.Format("{0}'s {1} prevents attack loss!", String(), abilityName));
					}
					return false;
				}
				if (stat == Stats.DEFENSE && HasWorkingAbility(Abilities.BIGPECKS)) {
					string abilityName = Abilities.GetName(ability);
					if (showMessages) {
						battle.Display(string.Format("{0}'s {1} prevents defense loss!", String(), abilityName));
					}
					return false;
				}
				if (stat == Stats.ACCURACY && HasWorkingAbility(Abilities.KEENEYE)) {
					string abilityName = Abilities.GetName(ability);
					if (showMessages) {
						battle.Display(string.Format("{0}'s {1} prevents accuracy loss!", String(), abilityName));
					}
					return false;
				}
			}
		}
		if (TooLow(stat)) {
			if (showMessages) {
				battle.Display(string.Format("{0}'s {1} won't go any lower!", String(), Stats.GetName(stat)));
			}
		}
		return true;
	}

	public int ReduceStatBasic(int stat, int increment, Battler attacker=null, bool moldbreaker=false, bool ignoreContrary=false) {
		if (!moldbreaker) {
			if (attacker == null || attacker.index == index || !attacker.HasMoldBreaker()) {
				if (HasWorkingAbility(Abilities.CONTRARY) && !ignoreContrary) {
					return IncreaseStatBasic(stat, increment, attacker, moldbreaker, true);
				}
				if (HasWorkingAbility(Abilities.SIMPLE)) {
					increment *= 2;
				}
			}
		}
		increment = Math.Min(increment, 6+stages[stat]);
		Debug.Log(string.Format("[Stat change] {0}'s {1} fell by {2} stage(s) (was {3}, now {4})", String(), Stats.GetName(stat), increment, stages[stat], stages[stat]-increment));
		stages[stat] -= increment;
		return increment;
	}

	public bool ReduceStat(int stat, int increment, Battler attacker, bool showMessages, BattleMove move=null, bool downAnim=true, bool moldbreaker=false, bool ignoreContrary=false) {
		if (!moldbreaker) {
			if (attacker == null || attacker.index == index || !attacker.HasMoldBreaker()) {
				if (HasWorkingAbility(Abilities.CONTRARY) && !ignoreContrary) {
					return IncreaseStat(stat, increment, attacker, showMessages, move, downAnim, moldbreaker, true);
				}
			}
		}
		if (stat != Stats.ATTACK && stat != Stats.DEFENSE && stat != Stats.SPATK && stat != Stats.SPDEF && stat != Stats.SPEED && stat != Stats.EVASION && stat != Stats.ACCURACY) {
			return false;
		}
		if (CanReduceStatStage(stat, attacker, showMessages, move, moldbreaker, ignoreContrary)) {
			increment = ReduceStatBasic(stat, increment, attacker, moldbreaker, ignoreContrary);
			if (increment > 0) {
				if (ignoreContrary) {
					if (downAnim) {
						battle.Display(string.Format("{0}'s {1} activated!", String(), Abilities.GetName(ability)));
					}
				}
				if (downAnim) {
					battle.CommonAnimation("StatDown", this, null);
				}
				string[] arrStatTexts = new string[3] {
					string.Format("{0}'s {1} fell!", String(), Stats.GetName(stat)),
					string.Format("{0}'s {1} harshly fell!", String(), Stats.GetName(stat)),
					string.Format("{0}'s {1} severely fell!", String(), Stats.GetName(stat))
				};
				battle.Display(string.Format(arrStatTexts[Math.Min(2, increment-1)]));
				if (HasWorkingAbility(Abilities.DEFIANT) && (attacker == null || attacker.IsOpposing(index))) {
					IncreaseStatWithCause(Stats.ATTACK, 2, this, Abilities.GetName(ability));
				}
				if (HasWorkingAbility(Abilities.COMPETITIVE) && (attacker == null || attacker.IsOpposing(index))) {
					IncreaseStatWithCause(Stats.SPATK, 2, this, Abilities.GetName(ability));
				}
				return true;
			}
		}
		return false;
	}

	public bool ReduceStatWithCause(int stat, int increment, Battler attacker, string cause, bool showAnimation=true, bool showMessages=true, bool moldbreaker=false, bool ignoreContrary=false) {
		if (!moldbreaker) {
			if (attacker == null || attacker.index == index || !attacker.HasMoldBreaker()) {
				if (HasWorkingAbility(Abilities.CONTRARY) && !ignoreContrary) {
					return IncreaseStatWithCause(stat, increment, attacker, cause, showAnimation, showMessages, moldbreaker, true);
				}
			}
		}
		if (stat != Stats.ATTACK && stat != Stats.DEFENSE && stat != Stats.SPATK && stat != Stats.SPDEF && stat != Stats.SPEED && stat != Stats.EVASION && stat != Stats.ACCURACY) {
			return false;
		}
		if (CanReduceStatStage(stat, attacker, false, null, moldbreaker, ignoreContrary)) {
			increment = ReduceStatBasic(stat, increment, attacker, moldbreaker, ignoreContrary);
			if (increment > 0) {
				if (ignoreContrary) {
					if (showMessages) {
						battle.Display(string.Format("{0}'s {1} activated!", String(), Abilities.GetName(ability)));
					}
				}
				if (showAnimation) {
					battle.CommonAnimation("StatUp", this, null);
				}
				string[] arrStatTexts;
				if (attacker.index == index) {
					arrStatTexts = new string[3] {
						string.Format("{0}'s {1} lowered its {2}!", String(), cause, Stats.GetName(stat)),
						string.Format("{0}'s {1} harshly lowered its {2}!", String(), cause, Stats.GetName(stat)),
						string.Format("{0}'s {1} severely lowered its {2}!", String(), cause, Stats.GetName(stat))
					};
				} else {
					arrStatTexts = new string[3] {
						string.Format("{0}'s {1} lowered {2}'s {3}!", attacker.String(), cause, String(true), Stats.GetName(stat)),
						string.Format("{0}'s {1} harshly lowered {2}'s {3}!", attacker.String(), cause, String(true), Stats.GetName(stat)),
						string.Format("{0}'s {1} severely lowered {2}'s {3}!", attacker.String(), cause, String(true), Stats.GetName(stat))
					};
				}
				if (showMessages) {
					battle.Display(string.Format(arrStatTexts[Math.Min(2, increment-1)]));
				}
				if (HasWorkingAbility(Abilities.DEFIANT) && (attacker == null || attacker.IsOpposing(index))) {
					IncreaseStatWithCause(Stats.ATTACK, 2, this, Abilities.GetName(ability));
				}
				if (HasWorkingAbility(Abilities.COMPETITIVE) && (attacker == null || attacker.IsOpposing(index))) {
					IncreaseStatWithCause(Stats.SPATK, 2, this, Abilities.GetName(ability));
				}
				return true;
			}
		}
		return false;
	}

	public bool ReduceStatWithIntimidate(Battler opponent) {
		if (Fainted()) {
			return false;
		}
		if (effects[Effects.Substitute] > 0) {
			battle.Display(string.Format("{0}'s substitute protected it from {1}'s {2}", String(), opponent.String(true), Abilities.GetName(opponent.ability)));
			return false;
		}
		if (!opponent.HasWorkingAbility(Abilities.CONTRARY)) {
			if (OwnSide().effects[Effects.Mist] > 0) {
				battle.Display(string.Format("{0} is protected from {1}'s {2} by Mist!", String(), opponent.String(true), Abilities.GetName(opponent.ability)));
				return false;
			}
			if (HasWorkingAbility(Abilities.CLEARBODY) || HasWorkingAbility(Abilities.WHITESMOKE) || HasWorkingAbility(Abilities.HYPERCUTTER) || (HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS))) {
				string abilityName = Abilities.GetName(ability);
				string oAbilityName = Abilities.GetName(opponent.ability);
				battle.Display(string.Format("{0}'s {1} prevented {2}'s {3} from working!", String(), abilityName, opponent.String(), oAbilityName));
				return false;
			}
			if (Partner().HasWorkingAbility(Abilities.FLOWERVEIL) && HasType(Types.GRASS)) {
				string abilityName = Abilities.GetName(Partner().ability);
				string oAbilityName = Abilities.GetName(opponent.ability);
				battle.Display(string.Format("{0}'s {1} prevented {2}'s {3} from working!", Partner().String(), abilityName, opponent.String(), oAbilityName));
				return false;
			}
		}
		return ReduceStatWithCause(Stats.ATTACK, 1, opponent, Abilities.GetName(opponent.ability));
	}
}
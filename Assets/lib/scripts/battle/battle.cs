using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

// Results of battle:
// 0 - Undecided or aborted
// 1 - Player won
// 2 - Player lost
// 3 - Player or wild PokÃ©mon ran from battle, or player forfeited the match
// 4 - Wild PokÃ©mon was caught
// 5 - Draw

/*********************
* Main battle class. *
*********************/
public class Battle {

	/*********************************
	* Catching and storing PokÃ©mon. *
	*********************************/
	public BattleScene scene; // Scene object for this battle
	public int decision; // Decision: 0=undecided; 1=win; 2=loss; 3=escaped; 4=caught
	public bool internalbattle; // Internal battle flag
	public bool doublebattle; // Double battle flag
	public bool cantescape; // True if player can't escape
	public bool shiftStyle; // Shift/Set "battle style" option
	public bool battlescene; // "Battle scene" option
	public bool debug; // Debug flag
	public int debugUpdate;
	public BattleTrainer[] player; // Player trainer
	public BattleTrainer[] opponent; // Opponent trainer
	public Battler[] party1; // Player's PokÃ©mon party
	public Battler[] party2; // Foe's PokÃ©mon party
	public List<int> party1order; // Order of PokÃ©mon in the player's party
	public List<int> party2order; // Order of PokÃ©mon in the opponent's party
	public bool fullparty1; // True if player's party's max size is 6 instead of 3
	public bool fullparty2; // True if opponent's party's max size is 6 instead of 3
	public List<Battler> battlers; // Currently active PokÃ©mon
	public int[] items; // Items held by opponents
	public ActiveSide[] sides; // Effects common to each side of a battle
	public ActiveField field; // Effects common to the whole of a battle
	public int environment; // Battle surroundings
	public int weather; // Current weather, custom methods should use pbWeather instead
	public int weatherduration; // Duration of current weather, or -1 if indefinite
	public bool switching; // True if during the switching phase of the round
	public bool futuresight; // True if Future Sight is hitting
	public BattleMove struggle; // The Struggle move
	public int[] useMoveChoice; // Use move choices made this round
	public int[] indexChoice; // Move index choices made this round
	public BattleMove[] moveChoice; // Move choices made this round
	public int[] itemChoice; // Move choices made this round
	public int[] targetChoice; // Target choices made this round
	public List<SuccessState> successStates; // Success states
	public int lastMoveUsed; // Last move used
	public int lastMoveUser; // Last move user
	public int[][] megaEvolution; // Battle index of each trainer's PokÃ©mon to Mega Evolve
	public bool amuletcoin; // Whether Amulet Coin's effect applies
	public int extramoney; // Money gained in battle by using Pay Day
	public bool doublemoney; // Whether Happy Hour's effect applies
	public string endspeech; // Speech by opponent when player wins
	public string endspeech2; // Speech by opponent when player wins
	public string endspeechwin; // Speech by opponent when opponent wins
	public string endspeechwin2; // Speech by opponent when opponent wins
	public Dictionary<string, int> rules;
	public int turnCount;
	public BattlePeer peer;
	public bool controlPlayer;
	public int runCommand;
	public List<int> priority;
	public bool usePriority;
	private int _nextPickupUse;
	public int nextPickupUse {
		get {
			_nextPickupUse += 1;
			return _nextPickupUse - 1;
		}
		set {
			_nextPickupUse = value;
		}
	}

	public const int MAXPARTYSIZE = 6;

	public void StorePokemon(Pokemon pokemon) {
		if (DisplayConfirm(string.Format("Would you like to give a nickname to {0}?", pokemon.name))) {
			string species = Species.GetName(pokemon.species);
			string nickname = scene.NameEntry(string.Format("{0}'s nickname?", species), pokemon);
			if (nickname != "") {
				pokemon.name = nickname;
			}
		}
		int oldcurbox = peer.CurrentBox();
		int storedbox = peer.StorePokemon(Player(), pokemon);
		string creator = peer.GetStorageCreator();
		if (storedbox < 0) {
			return;
		}
		string curboxname = peer.BoxName(oldcurbox);
		string boxname = peer.BoxName(storedbox);
		if (storedbox != oldcurbox) {
			if (creator != "") {
				DisplayPaused(string.Format("Box {0} on {1}'s PC was full.", curboxname, creator));
			} else {
				DisplayPaused(string.Format("Box {0} on someone's PC was full.", curboxname));
			}
			DisplayPaused(string.Format("{0} was transferred to box \"{1}\".", pokemon.name, boxname));
		} else {
			if (creator != "") {
				DisplayPaused(string.Format("{0} was transferred to {1}'s PC.", pokemon.name, creator));
			} else {
				DisplayPaused(string.Format("{0} was transferred to someone's PC.", pokemon.name));
			}
			DisplayPaused(string.Format("It was stored in box \"{0}\".", boxname));
		}
	}

	public void ThrowPokeBall(int idxPokemon, int ball, int rareness=-1, bool showPlayer=false) {
		string itemName = Items.GetName(ball);
		Battler battler = null;
		if (IsOpposing(idxPokemon)) {
			battler = battlers[idxPokemon];
		} else {
			battler = battlers[idxPokemon].OppositeOpposing();
		}
		if (battler.Fainted()) {
			battler = battler.Partner();
		}
		DisplayBrief(string.Format("{0} threw one {1}", Player().name, itemName));
		if (battler.Fainted()) {
			Display(string.Format("But there was no target..."));
			return;
		}
		if (opponent.Length > 0) {
			scene.ThrowAndDeflect(ball, 1);
			Display(string.Format("The Trainer blocked the Ball!\nDon't be a thief!"));
		} else {
			Pokemon pokemon = battler.pokemon;
			int species = pokemon.species;
			int shakes;
			bool critical = false;
			if (Settings.DEBUG && Input.GetKey("ctrl")) {
				shakes = 4;
			} else {
				if (rareness != -1) {
					rareness = Species.GetSpecies(pokemon.species).Rareness;
				}
				int a = battler.totalHP;
				int b = battler.hp;
				rareness =  Items.ModifyCatchRateHandler(ball, rareness, this, battler);
				int x = (int)(((a*3.0f-b*2.0f)*rareness)/(a*3.0f));
				if (battler.status == Statuses.SLEEP || battler.status == Statuses.FROZEN) {
					x = (int)(x*2.5f);
				} else if (battler.status != 0) {
					x = (int)(x*1.5f);
				}
				int c = 0;
				if (PokemonGlobal.Trainer.PokedexOwned() > 600) {
					c = (int)(x*2.5f/6);
				} else if (PokemonGlobal.Trainer.PokedexOwned() > 450) {
					c = (int)(x*2.0f/6);
				} else if (PokemonGlobal.Trainer.PokedexOwned() > 300) {
					c = (int)(x*1.5f/6);
				} else if (PokemonGlobal.Trainer.PokedexOwned() > 150) {
					c = (int)(x*1.0f/6);
				} else if (PokemonGlobal.Trainer.PokedexOwned() > 30) {
					c = (int)(x*0.5f/6);
				}
				shakes = 0;
				if (x > 255 || Items.IsUnconditionalHandler(ball, this, battler)) {
					shakes = 4;
				} else {
					if (x < 1) {
						x = 1;
					}
					int y = (int)(65536f/(Math.Pow(255.0f/x, 0.1875f)));
					if (Settings.USE_CRITICAL_CAPTURE && Rand(256) < c) {
						critical = true;
						if (Rand(65536) < y) {
							shakes = 4;
						}
					} else {
						if (Rand(65536) < y) {
							shakes += 1;
						}
						if (Rand(65536) < y && shakes == 1) {
							shakes += 1;
						}
						if (Rand(65536) < y && shakes == 2) {
							shakes += 1;
						}
						if (Rand(65536) < y && shakes == 3) {
							shakes += 1;
						}
					}
				}
			}
			Debug.Log(string.Format("[Threw Poke Ball] {0}, {1} shakes (4=Capture)", itemName, shakes));
			scene.Throw(ball, shakes, critical, battler, showPlayer);
			switch (shakes) {
				case 0:
					Display(string.Format("Oh no! The Pokémon broke free!"));
					 Items.OnFailCatchHandler(ball, this, battler);
					break;
				case 1:
					Display(string.Format("Aww... It appeared to be caught!"));
					 Items.OnFailCatchHandler(ball, this, battler);
					break;
				case 2:
					Display(string.Format("Aargh! Almost had it!"));
					 Items.OnFailCatchHandler(ball, this, battler);
					break;
				case 3:
					Display(string.Format("Gah! It was so close, too!"));
					 Items.OnFailCatchHandler(ball, this, battler);
					break;
				case 4:
					DisplayBrief(string.Format("Gotcha! {0} was caught!", pokemon.name));
					decision = 4;
					Items.OnCatchHandler(ball, this, battler);
					pokemon.ballUsed = Items.GetBallType(ball);
					if (pokemon.IsMega()) {
						pokemon.MakeUnmega();
					}
					if (pokemon.IsPrimal()) {
						pokemon.MakeUnprimal();
					}
					pokemon.RecordFirstMoves();
					if (Settings.GAIN_EXP_FOR_CAPTURE) {
						battler.captured = true;
						GainEXP();
						battler.captured = false;
					}
					if (!Player().HasOwned(species)) {
						Player().SetOwned(species);
						if (PokemonGlobal.Trainer.pokedex) {
							DisplayPaused(string.Format("{0}'s data was added to the Pokédex", pokemon.name));
							scene.ShowPokedex(species);
						}
					}
					if (MultipleForms.HasFunction(pokemon.species, "getForm")) {
						pokemon.forcedForm = false;
					}
					scene.HideCaptureBall();
					StorePokemon(pokemon);
					break;
			}
		}
	}

	/***************
	* Misc Methods *
	***************/
	public void Abort() {
		throw new Exception("Battle Aborted");
	}

	public void DebugUpdate() {
	}

	public int Rand(int x) {
		return Utilities.Rand(x);
	}

	public int AIRand(int x) {
		return Rand(x);
	}

	public Battle(BattleScene scene, Battler[] p1, Battler[] p2, BattleTrainer[] player, BattleTrainer[] opponent) {
		if (p1.Length == 0) {
			throw new Exception("Party 1 has no Pokémon.");
		}
		if (p2.Length == 0) {
			throw new Exception("Party 2 has no Pokémon.");
		}
		if (p2.Length > 2 && (opponent == null || opponent.Length == 0)) {
			throw new Exception("Wild battles with more than two Pokémon are not allowed.");
		}
		this.scene = scene;
		decision = 0;
		internalbattle = true;
		doublebattle = false;
		cantescape = false;
		shiftStyle = true;
		battlescene = true;
		debug = false;
		debugUpdate = 0;
		this.player = player;
		this.opponent = opponent;
		party1 = p1;
		party2 = p2;
		party1order = new List<int>();
		for (int i=0; i<12; i++) {
			party1order.Add(i);
		}
		party2order = new List<int>();
		for (int i=0; i<12; i++) {
			party2order.Add(i);
		}
		fullparty1 = false;
		fullparty2 = false;
		battlers = new List<Battler>();
		items = null;
		sides = new ActiveSide[2]{new ActiveSide(), new ActiveSide()};
		field = new ActiveField();
		environment = Environment.None;
		weather = 0;
		weatherduration = 0;
		switching = false;
		futuresight = false;
		useMoveChoice = new int[4]{0, 0, 0, 0};
		indexChoice = new int[4]{0, 0, 0, 0};
		moveChoice = new BattleMove[4]{null, null, null, null};
		targetChoice = new int[4]{-1, -1, -1, -1};
		successStates = new List<SuccessState>();
		for (int i=0; i<4; i++) {
			successStates.Add(new SuccessState());
		}
		lastMoveUsed = -1;
		lastMoveUser = -1;
		nextPickupUse = 0;
		megaEvolution = new int[2][]{new int[2]{-1, -1}, new int[2]{-1, -1}};
		amuletcoin = false;
		extramoney = 0;
		doublemoney = false;
		endspeech = "";
		endspeech2 = "";
		endspeechwin = "";
		endspeechwin2 = "";
		rules = new Dictionary<string, int>();
		turnCount = 0;
		peer = new RealBattlePeer();
		priority = new List<int>();
		usePriority = false;
		runCommand = 0;
		struggle = new MoveStruggle(this, null);
		struggle.pp = -1;
		for (int i=0; i<4; i++) {
			battlers.Add(new Battler(this, i));
		}
		for (int i=0; i<this.party1.Length; i++) {
			if (this.party1[i] == null) {
				continue;
			}
			this.party1[i].pokemon.itemRecycle = 0;
			this.party1[i].pokemon.itemInitial = this.party1[i].item;
			this.party1[i].pokemon.belch = false;
		}
		for (int i=0; i<this.party2.Length; i++) {
			if (this.party2[i] == null) {
				continue;
			}
			this.party2[i].pokemon.itemRecycle = 0;
			this.party2[i].pokemon.itemInitial = this.party2[i].item;
			this.party2[i].pokemon.belch = false;
		}
	}

	/**************
	* Battle Info *
	**************/
	public bool DoubleBattleAllowed() {
		if (!fullparty1 && party1.Length > MAXPARTYSIZE) {
			return false;
		}
		if (!fullparty2 && party2.Length > MAXPARTYSIZE) {
			return false;
		}
		int sendOut1;
		int sendOut2;
		// Wild Battle
		if ((opponent == null || opponent.Length == 0)) {
			if (party2.Length == 1) {
				return false;
			} else if (party2.Length == 2) {
				return true;
			} else {
				return false;
			}
		}
		// Trainer Battle
		else {
			if (opponent.Length > 2 || opponent.Length == 0) {
				return false;
			}
			if (player.Length > 2 || player.Length == 0) {
				return false;
			}
			if (opponent.Length > 1) {
				sendOut1 = FindNextUnfainted(party2, 0, SecondPartyBegin(1));
				sendOut2 = FindNextUnfainted(party2, SecondPartyBegin(1));
			} else {
				sendOut1 = FindNextUnfainted(party2, 0);
				sendOut2 = FindNextUnfainted(party2, sendOut1+1);
			}
			if (sendOut1 < 0 || sendOut2 < 0) {
				return false;
			}
		}
		if (player.Length > 1) {
			sendOut1 = FindNextUnfainted(party1, 0, SecondPartyBegin(0));
			sendOut2 = FindNextUnfainted(party1, SecondPartyBegin(0));
		} else {
			sendOut1 = FindNextUnfainted(party1, 0);
			sendOut2 = FindNextUnfainted(party1, sendOut1+1);
		}
		if (sendOut1 < 0 || sendOut2 < 0) {
			return false;
		}
		return true;
	}

	public int GetWeather() {
		for (int i=0; i<4; i++) {
			if (battlers[i].HasWorkingAbility(Abilities.CLOUDNINE) || battlers[i].HasWorkingAbility(Abilities.AIRLOCK)) {
				return 0;
			}
		}
		return weather;
	}

	/***************
	* Battler Info *
	***************/
	public bool IsOpposing(int index) {
		return index%2 == 1;
	}

	public bool OwnedByPlayer(int index) {
		if (IsOpposing(index)) {
			return false;
		}
		return true;
	}

	public bool IsDoubleBattler(int index) {
		return index >= 2;
	}

	// Only used for Wish
	public string StringEx(int battlerIndex, int pokemonIndex) {
		Battler[] party = Party(battlerIndex);
		if (IsOpposing(battlerIndex)) {
			if (opponent.Length > 0) {
				return string.Format("The foe {0}", party[pokemonIndex].name);
			} else {
				return string.Format("The wild {0}", party[pokemonIndex].name);
			}
		} else {
			return party[pokemonIndex].name;
		}
	}

	//Checks whether an item can be removed from a PokÃ©mon
	public bool IsUnlosableItem(Battler pkmn, int item) {
		if (Items.IsMail(item)) {
			return true;
		}
		if (pkmn.effects[Effects.Transform] != 0) {
			return false;
		}
		if (pkmn.pokemon != null && pkmn.pokemon.GetMegaForm(true) != 0) {
			return true;
		}
		if (pkmn.ability == Abilities.MULTITYPE) {
			int[] plates = new int[17]{Items.FISTPLATE, Items.SKYPLATE, Items.TOXICPLATE, Items.EARTHPLATE, Items.STONEPLATE, Items.INSECTPLATE, Items.SPOOKYPLATE, Items.IRONPLATE, Items.FLAMEPLATE, Items.SPLASHPLATE, Items.MEADOWPLATE, Items.ZAPPLATE, Items.MINDPLATE, Items.ICICLEPLATE, Items.DRACOPLATE, Items.DREADPLATE, Items.PIXIEPLATE};
			for (int i=0; i<plates.Length; i++) {
				if (item == plates[i]) {
					return true;
				}
			}
		}
		if ((pkmn.species == Species.GIRATINA && item == Items.GRISEOUSORB) || (pkmn.species == Species.GENESECT && item == Items.BURNDRIVE) || (pkmn.species == Species.GENESECT && item == Items.CHILLDRIVE) || (pkmn.species == Species.GENESECT && item == Items.DOUSEDRIVE) || (pkmn.species == Species.GENESECT && item == Items.SHOCKDRIVE) || (pkmn.species == Species.KYOGRE && item == Items.BLUEORB) || (pkmn.species == Species.GROUDON && item == Items.REDORB)) {
			return true;
		}
		return false;
	}

	public Battler CheckGlobalAbility(int ability) {
		for (int i=0; i<4; i++) {
			if (battlers[i].HasWorkingAbility(ability)) {
				return battlers[i];
			}
		}
		return null;
	}

	/***********************
	* Player-related info. *
	***********************/
	public BattleTrainer Player() {
		return player[0];
	}

	public int[] GetOwnerItems(int battlerIndex) {
		if (items == null) {
			return new int[0];
		}
		if (IsOpposing(battlerIndex)) {
			return items;
		}
		return new int[0];
	}

	public void SetSeen(Pokemon pokemon) {
		if (pokemon != null && internalbattle) {
			Player().seen[pokemon.species] = true;
			Utilities.SeenForm(pokemon);
		}
	}

	public string GetMegaRingName(int battlerIndex) {
		if (BelongsToPlayer(battlerIndex)) {
			for (int i=0; i<Settings.MEGA_RINGS.Length; i++) {
				if (PokemonGlobal.Bag.HasItem(Settings.MEGA_RINGS[i])) {
					return Items.GetName(Settings.MEGA_RINGS[i]);
				}
			}
		}
		return string.Format("Mega Ring");
	}

	public bool HasMegaRing(int battlerIndex) {
		if (!BelongsToPlayer(battlerIndex)) {
			return true;
		}
		for (int i=0; i<Settings.MEGA_RINGS.Length; i++) {
			if (PokemonGlobal.Bag.HasItem(Settings.MEGA_RINGS[i])) {
				return true;
			}
		}
		return false;
	}

	/*******************************
	* Party Info and Manipulation. *
	*******************************/
	public int PokemonCount(Battler[] party) {
		int count = 0;
		for (int i=0; i<party.Length; i++) {
			if (party[i] == null) {
				continue;
			}
			if (party[i].hp > 0 && !party[i].pokemon.Egg()) {
				count++;
			}
		}
		return count;
	}

	public bool AllFainted(Battler[] party) {
		return PokemonCount(party) == 0;
	}

	public int MaxLevel(Battler[] party) {
		int lv = 0;
		for (int i=0; i<party.Length; i++) {
			if (party[i] == null) {
				continue;
			}
			if (party[i].level > lv) {
				lv = party[i].level;
			}
		}
		return lv;
	}

	public int MaxLevelFromIndex(int index) {
		Battler[] party = Party(index);
		BattleTrainer[] owner = IsOpposing(index) ? opponent : player;
		int maxLevel = 0;
		if (owner.Length > 1) {
			int start = 0;
			int limit = SecondPartyBegin(index);
			if (IsDoubleBattler(index)) {
				start = limit;
			}
			for (int i=start; i<start+limit; i++) {
				if (party[1] == null) {
					continue;
				}
				if (maxLevel < party[i].level) {
					maxLevel = party[i].level;
				}
			}
		} else {
			for (int i=0; i<party.Length; i++) {
				if (party[i] == null) {
					continue;
				}
				if (maxLevel < party[i].level) {
					maxLevel = party[i].level;
				}
			}
		}
		return maxLevel;
	}

	public Battler[] Party(int index) {
		return IsOpposing(index) ? party2 : party1;
	}

	public Battler[] OpposingParty(int index) {
		return IsOpposing(index) ? party1 : party2;
	}

	public int SecondPartyBegin(int battlerIndex) {
		if (IsOpposing(battlerIndex)) {
			return fullparty2 ? 6 : 3;
		} else {
			return fullparty1 ? 6 : 3;
		}
	}

	public int PartyLength(int battlerIndex) {
		if (IsOpposing(battlerIndex)) {
			return opponent.Length > 1 ? SecondPartyBegin(battlerIndex) : MAXPARTYSIZE;
		} else {
			return player.Length > 1 ? SecondPartyBegin(battlerIndex) : MAXPARTYSIZE;
		}
	}

	public int FindNextUnfainted(Battler[] party, int start, int finish=-1) {
		if (finish < 0) {
			finish = party.Length;
		}
		for (int i=start; i<finish; i++) {
			if (party[i] == null) {
				continue;
			}
			if (party[i].hp > 0 && !party[i].pokemon.Egg()) {
				return i;
			}
		}
		return -1;
	}

	public int GetLastPokemonInTeam(int index) {
		Battler[] party = Party(index);
		List<int> partyorder = !IsOpposing(index) ? party1order : party2order;
		int plength = PartyLength(index);
		int pstart = GetOwnerIndex(index)*plength;
		int lastpoke = -1;
		for (int i=pstart; i<pstart+plength; i++) {
			Battler p = party[partyorder[i]];
			if (p == null || p.pokemon.Egg() || p.hp <= 0) {
				continue;
			}
			lastpoke = partyorder[i];
		}
		return lastpoke;
	}

	public Battler FindPlayerBattler(int pokemonIndex) {
		Battler battler = null;
		for (int i=0; i<4; i++) {
			if (!IsOpposing(i) && battlers[i].pokemonIndex == pokemonIndex) {
				battler = battlers[i];
				break;
			}
		}
		return battler;
	}

	public bool IsOwner(int battlerIndex, int partyIndex) {
		int secondParty = SecondPartyBegin(battlerIndex);
		if (!IsOpposing(battlerIndex)) {
			if (player == null || player.Length == 1) {
				return true;
			}
			return battlerIndex == 0 ? partyIndex < secondParty : partyIndex >= secondParty;
		} else {
			if ((opponent == null || opponent.Length == 0) || opponent.Length == 1) {
				return true;
			}
			return battlerIndex == 1 ? partyIndex < secondParty : partyIndex >= secondParty;
		}
	}

	public BattleTrainer GetOwner(int battlerIndex) {
		if (IsOpposing(battlerIndex)) {
			if (opponent.Length > 1) {
				return battlerIndex == 1 ? opponent[0] : opponent[1];
			} else {
				return opponent[0];
			}
		} else {
			if (player.Length > 1) {
				return battlerIndex == 0 ? player[0] : player[1];
			} else {
				return player[0];
			}
		}
	}

	public BattleTrainer GetOwnerPartner(int battlerIndex) {
		if (IsOpposing(battlerIndex)) {
			if (opponent.Length > 1) {
				return battlerIndex == 1 ? opponent[1] : opponent[0];
			} else {
				return opponent[0];
			}
		} else {
			if (player.Length > 1) {
				return battlerIndex == 0 ? player[1] : player[0];
			} else {
				return player[0];
			}
		}
	}

	public int GetOwnerIndex(int battlerIndex) {
		if (IsOpposing(battlerIndex)) {
			return opponent.Length > 1 ? (battlerIndex == 1 ? 0 : 1) : 0;
		} else {
			return player.Length > 1 ? (battlerIndex == 0 ? 0 : 1) : 0;
		}
	}

	public bool BelongsToPlayer(int battlerIndex) {
		if (player.Length > 1) {
			return battlerIndex == 0;
		} else {
			return battlerIndex%2 == 0;
		}
	}

	public BattleTrainer PartyGetOwner(int battlerIndex, int partyIndex) {
		int secondParty = SecondPartyBegin(battlerIndex);
		if (!IsOpposing(battlerIndex)) {
			if (player.Length == 1) {
				return player[0];
			}
			return (partyIndex < secondParty) ? player[0] : player[1];
		} else {
			if (opponent.Length == 1) {
				return opponent[0];
			}
			return (partyIndex < secondParty) ? opponent[0] : opponent[1];
		}
	}

	public void AddToPlayerParty(Battler pokemon) {
		Battler[] party = Party(0);
		for (int i=0; i<party.Length; i++) {
			if (IsOwner(0, i) && party[i] == null) {
				party[i] = pokemon;
			}
		}
	}

	public void RemoveFromParty(int battlerIndex, int partyIndex) {
		Battler[] party = Party(battlerIndex);
		BattleTrainer[] side = IsOpposing(battlerIndex) ? opponent : player;
		BattleTrainer owner = GetOwner(battlerIndex);
		List<int> order = IsOpposing(battlerIndex) ? party2order : party1order;
		int secondpartybegin = SecondPartyBegin(battlerIndex);
		party[partyIndex] = null;
		if (side == null || side.Length == 1) {
			List<Battler> np = new List<Battler>();
			for (int i=0; i<party.Length; i++) {
				if (party[i] != null) {
					np.Add(party[i]);
				}
			}
			party = np.ToArray();
			for (int i=partyIndex; i<party.Length+1; i++) {
				for (int j=0; j<4; j++) {
					if (battlers[j] == null) {
						continue;
					}
					if (GetOwner(j) == owner && battlers[j].pokemonIndex == i) {
						battlers[j].pokemonIndex -= 1;
						break;
					}
				}
			}
			for (int i=0; i<order.Count; i++) {
				order[i] = i==partyIndex ? order.Count - 1 : order[i] - 1;
			}
		} else {
			if (partyIndex < secondpartybegin-1) {
				for (int i=partyIndex; i<secondpartybegin; i++) {
					if (i >= secondpartybegin-1) {
						party[i] = null;
					} else {
						party[i] = party[i+1];
					}
				}
				for (int i=0; i<order.Count; i++) {
					if (order[i] >= secondpartybegin) {
						continue;
					}
					order[i] = i == partyIndex ? secondpartybegin-1 : order[i]-1;
				}
			} else {
				for (int i=partyIndex; i<secondpartybegin+PartyLength(battlerIndex); i++) {
					if (i >= party.Length-1) {
						party[i] = null;
					} else {
						party[i] = party[i+1];
					}
				}
				for (int i=0; i<order.Count; i++) {
					if (order[i] < secondpartybegin) {
						order[i] = i==partyIndex ? secondpartybegin+PartyLength(battlerIndex)-1 : order[i]-1;
					}
				}
			}
		}
	}

	/***************************************
	* Check whether actions can be taken. *
	***************************************/
	public bool CanShowCommands(int idxPokemon) {
		Battler thisPokemon = battlers[idxPokemon];
		if (thisPokemon.Fainted() || thisPokemon.effects[Effects.TwoTurnAttack] > 0 || thisPokemon.effects[Effects.HyperBeam] > 0 || thisPokemon.effects[Effects.Rollout] > 0 || thisPokemon.effects[Effects.Outrage] > 0 || thisPokemon.effects[Effects.Uproar] > 0 || thisPokemon.effects[Effects.Bide] > 0) {
			return false;
		}
		return true;
	}

	/*************
	* Attacking. *
	*************/
	public bool CanShowFightMenu(int idxPokemon) {
		Battler thisPokemon = battlers[idxPokemon];
		if (!CanShowCommands(idxPokemon)) {
			return false;
		}
		if (!CanChooseMove(idxPokemon, 0, false) && !CanChooseMove(idxPokemon, 1, false) && !CanChooseMove(idxPokemon, 2, false) && !CanChooseMove(idxPokemon, 3, false)) {
			return false;
		}
		if (thisPokemon.effects[Effects.Encore] > 0) {
			return false;
		}
		return true;
	}

	public bool CanChooseMove(int idxPokemon, int idxMove, bool showMessages, bool sleeptalk=false) {
		Battler thisPokemon = battlers[idxPokemon];
		BattleMove thisMove = thisPokemon.moves[idxMove];
		Battler opp1 = thisPokemon.Opposing1();
		Battler opp2 = thisPokemon.Opposing2();
		if (thisMove == null || thisMove.id == 0) {
			return false;
		}
		if (thisMove.pp <= 0 && thisMove.totalPP > 0 && !sleeptalk) {
			if (showMessages) {
				DisplayPaused(string.Format("There's no PP left for this move!"));
			}
			return false;
		}
		if (thisPokemon.HasWorkingItem(Items.ASSAULTVEST) && thisMove.IsStatus()) {
			if (showMessages) {
				DisplayPaused(string.Format("The effects of the {0} prevent status moves from being used!", Items.GetName(thisPokemon.item)));
			}
			return false;
		}
		if (thisPokemon.effects[Effects.ChoiceBand] >= 0 && (thisPokemon.HasWorkingItem(Items.CHOICEBAND) || thisPokemon.HasWorkingItem(Items.CHOICESPECS) || thisPokemon.HasWorkingItem(Items.CHOICESCARF))) {
			bool hasMove = false;
			for (int i=0; i<4; i++) {
				if (thisPokemon.moves[i].id == thisPokemon.effects[Effects.ChoiceBand]) {
					hasMove = true;
					break;
				}
			}
			if (hasMove && thisMove.id != thisPokemon.effects[Effects.ChoiceBand]) {
				if (showMessages) {
					DisplayPaused(string.Format("{0} allows only the use of {1}!", Items.GetName(thisPokemon.item), Moves.GetName(thisPokemon.effects[Effects.ChoiceBand])));
				}
				return false;
			}
		}
		if (opp1.effects[Effects.Imprison] != 0) {
			if (thisMove.id == opp1.moves[0].id || thisMove.id == opp1.moves[1].id || thisMove.id == opp1.moves[2].id || thisMove.id == opp1.moves[3].id) {
				if (showMessages) {
					DisplayPaused(string.Format("{0} can't use the sealed {1}!", thisPokemon.String(), thisMove.name));
				}
				return false;
			}
		}
		if (opp2.effects[Effects.Imprison] != 0) {
			if (thisMove.id == opp2.moves[0].id || thisMove.id == opp2.moves[1].id || thisMove.id == opp2.moves[2].id || thisMove.id == opp2.moves[3].id) {
				if (showMessages) {
					DisplayPaused(string.Format("{0} can't use the sealed {1}!", thisPokemon.String(), thisMove.name));
				}
				return false;
			}
		}
		if (thisPokemon.effects[Effects.Taunt] > 0 && thisMove.baseDamage == 0) {
			if (showMessages) {
				DisplayPaused(string.Format("{0} can't use {1} after the taunt!", thisPokemon.String(), thisMove.name));
			}
			return false;
		}
		if (thisPokemon.effects[Effects.Torment] != 0) {
			if (thisMove.id == thisPokemon.lastMoveUsed) {
				if (showMessages) {
					DisplayPaused(string.Format("{0} can't use the same move twice in a row due to the torment!", thisPokemon.String()));
				}
				return false;
			}
		}
		if (thisMove.id == thisPokemon.effects[Effects.DisableMove] && !sleeptalk) {
			if (showMessages) {
				DisplayPaused(string.Format("{0}'s {1} is disabled!", thisPokemon.String(), thisMove.name));
			}
			return false;
		}
		if (thisMove.function == 0x158 && (thisPokemon.pokemon != null || !thisPokemon.pokemon.belch)) {
			if (showMessages) {
				DisplayPaused(string.Format("{0} hasn't eaten any held berry, so it can't possibly belch!", thisPokemon.String()));
			}
		}
		if (thisPokemon.effects[Effects.Encore] > 0 && idxMove != thisPokemon.effects[Effects.EncoreIndex]) {
			return false;
		}
		return true;
	}

	public void AutoChooseMove(int idxPokemon, bool showMessages=true) {
		Battler thisPokemon = battlers[idxPokemon];
		if (thisPokemon.Fainted()) {
			useMoveChoice[idxPokemon] = 0;
			indexChoice[idxPokemon] = 0;
			moveChoice = null;
			return;
		}
		if (thisPokemon.effects[Effects.Encore] > 0 && CanChooseMove(idxPokemon, thisPokemon.effects[Effects.EncoreIndex], false)) {
			Debug.Log(string.Format("[Auto choosing encore move] {0}", thisPokemon.moves[thisPokemon.effects[Effects.EncoreIndex]].name));
			useMoveChoice[idxPokemon] = 1;
			indexChoice[idxPokemon] = thisPokemon.effects[Effects.EncoreIndex];
			moveChoice[idxPokemon] = thisPokemon.moves[thisPokemon.effects[Effects.EncoreIndex]];
			targetChoice[idxPokemon] = -1;
			if (doublebattle) {
				BattleMove thisMove = thisPokemon.moves[thisPokemon.effects[Effects.EncoreIndex]];
				int target = thisPokemon.Target(thisMove);
				if (target == Targets.SingleNonUser) {
					target = scene.ChooseTarget(idxPokemon, target);
					if (target >= 0) {
						RegisterTarget(idxPokemon, target);
					}
				} else if (target == Targets.UserOrPartner) {
					target = scene.ChooseTarget(idxPokemon, target);
					if (target >= 0 && (target&1)==(idxPokemon&1)) {
						RegisterTarget(idxPokemon, target);
					}
				}
			} else {
				if (!IsOpposing(idxPokemon)) {
					if (showMessages) {
						DisplayPaused(string.Format("{0} has no moves left!", thisPokemon.name));
					}
				}
				useMoveChoice[idxPokemon] = 1;
				indexChoice[idxPokemon] = -1;
				moveChoice[idxPokemon] = struggle;
				targetChoice[idxPokemon] = -1;
			}
		}
	}

	public bool RegisterMove(int idxPokemon, int idxMove, bool showMessages=true) {
		Battler thisPokemon = battlers[idxPokemon];
		BattleMove thisMove = thisPokemon.moves[idxMove];
		if (!CanChooseMove(idxPokemon, idxMove, showMessages)) {
			return false;
		}
		useMoveChoice[idxPokemon] = 1;
		indexChoice[idxPokemon] = idxMove;
		moveChoice[idxPokemon] = thisMove;
		targetChoice[idxPokemon] = -1;
		return true;
	}

	public bool ChoseMove(int i, int move) {
		if (battlers[i].Fainted()) {
			return false;
		}
		if (useMoveChoice[i] == 1 && indexChoice[i] >= 0) {
			int idxChoice = indexChoice[i];
			return battlers[i].moves[idxChoice].id == new Moves.Move(move).Id;
		}
		return false;
	}

	public bool ChoseMoveFunctionCode(int i, int code) {
		if (battlers[i].Fainted()) {
			return false;
		}
		if (useMoveChoice[i] == 1 && indexChoice[i] >= 0) {
			int idxChoice = indexChoice[i];
			return battlers[i].moves[idxChoice].function == code;
		}
		return false;
	}

	public bool RegisterTarget(int idxPokemon, int idxTarget) {
		targetChoice[idxPokemon] = idxTarget;
		return true;
	}

	public Battler[] Priority(bool ignoreQuickclaw=false, bool log=false) {
		List<Battler> actualPriorities = new List<Battler>(); //Add based on battler array before returming
		if (usePriority) {
			for (int i=0; i<priority.Count; i++) {
				actualPriorities.Add(battlers[priority[i]]);
			}
			return actualPriorities.ToArray();
		}
		priority = new List<int>();
		List<int> speeds = new List<int>();
		List<int> priorities = new List<int>();
		List<bool> quickclaw = new List<bool>();
		List<bool> lagging = new List<bool>();
		int minpri = 0;
		int maxpri = 0;
		List<int> temp = new List<int>();
		for (int i=0; i<4; i++) {
			speeds.Add(battlers[i].speed);
			quickclaw.Add(false);
			lagging.Add(false);
			if (!ignoreQuickclaw && useMoveChoice[i] == 1) {
				if (!quickclaw[i] && battlers[i].HasWorkingItem(Items.CUSTAPBERRY) && !battlers[i].Opposing1().HasWorkingAbility(Abilities.UNNERVE) && !battlers[i].Opposing2().HasWorkingAbility(Abilities.UNNERVE)) {
					if ((battlers[i].HasWorkingAbility(Abilities.GLUTTONY) && battlers[i].hp <= battlers[i].totalHP/2) || battlers[i].hp <= battlers[i].totalHP/4) {
						CommonAnimation("UseItem", battlers[i], null);
						quickclaw[i] = true;
						DisplayBrief(string.Format("{0}'s {1} let it move first!", battlers[i].String(), Items.GetName(battlers[i].item)));
						battlers[i].ConsumeItem();
					}
				}
				if (!quickclaw[i] && battlers[i].HasWorkingItem(Items.QUICKCLAW)) {
					if (Rand(10) < 2) {
						CommonAnimation("UseItem", battlers[i], null);
						quickclaw[i] = true;
						DisplayBrief(string.Format("{0}'s {1} let it move first!", battlers[i].String(), Items.GetName(battlers[i].item)));
					}
				}
				if (!quickclaw[i] && (battlers[i].HasWorkingAbility(Abilities.STALL) || battlers[i].HasWorkingItem(Items.LAGGINGTAIL) || battlers[i].HasWorkingItem(Items.FULLINCENSE))) {
					lagging[i] = true;
				}
			}
		}
		for (int i=0; i<4; i++) {
			int pri = 0;
			if (useMoveChoice[i] == 1) {
				pri = moveChoice[i].priority;
				if (battlers[i].HasWorkingAbility(Abilities.PRANKSTER) && moveChoice[i].IsStatus()) {
					pri += 1;
				}
				if (battlers[i].HasWorkingAbility(Abilities.GALEWINGS) && moveChoice[i].type == Types.FLYING) {
					pri += 1;
				}
			}
			priorities[i] = pri;
			if (i==0) {
				minpri = pri;
				maxpri = pri;
			} else {
				if (minpri > pri) {
					minpri = pri;
				}
				if (maxpri < pri) {
					maxpri = pri;
				}
			}
		}
		int curpri = maxpri;
		while (curpri >= minpri) {
			temp = new List<int>();
			for (int j=0; j<4; j++) {
				if (priorities[j] == curpri) {
					temp.Add(j);
				}
			}
			if (temp.Count == 1) {
				priority.Add(temp[0]);
			} else if (temp.Count > 1) {
				int n = temp.Count;
				for (int m=0; m < temp.Count-1; m++) {
					for (int i=1; i<temp.Count; i++) {
						// For each pair of battlers, rank the second compared to the first
						// -1 means rank higher, 0 means rank equal, 1 means rank lower
						int cmp = 0;
						if (quickclaw[temp[i]]) {
							cmp = -1;
							if (quickclaw[temp[i-1]]) {
								if (speeds[temp[i]] == speeds[temp[i-1]]) {
									cmp = 0;
								} else {
									cmp = (speeds[temp[i]] > speeds[temp[i-1]]) ? -1 : 1;
								}
							}
						} else if (quickclaw[temp[i-1]]) {
							cmp = 1;
						} else if (lagging[temp[i]]) {
							cmp = 1;
							if (lagging[temp[i-1]]) {
								if (speeds[temp[i]] == speeds[temp[i-1]]) {
									cmp = 0;
								} else {
									cmp = (speeds[temp[i]] > speeds[temp[i-1]]) ? 1 : -1;
								}
							}
						} else if (lagging[temp[i-1]]) {
							cmp = -1;
						} else if (speeds[temp[i]] != speeds[temp[i-1]]) {
							if (field.effects[Effects.TrickRoom] > 0) {
								cmp = (speeds[temp[i]] > speeds[temp[i-1]]) ? 1 : -1;
							} else {
								cmp = (speeds[temp[i]] > speeds[temp[i-1]]) ? -1 : 1;
							}
						}
						if (cmp < 0 || (cmp == 0 && Rand(2) == 0)) {
							int swaptmp = temp[i];
							temp[i] = temp[i-1];
							temp[i-1] = swaptmp;
						}
					}
				}
				for (int i=0; i<temp.Count; i++) {
					priority.Add(temp[i]);
				}
			}
			curpri--;
		} 
		if (log) {
			string s = "[Priority] ";
			bool comma = false;
			for (int i=0; i<4; i++) {
				if (actualPriorities[i] != null && !actualPriorities[i].Fainted()) {
					if (comma) {
						s += ", ";
					}
					s += string.Format("{0} ({1})", actualPriorities[i].String(comma), actualPriorities[i].index);
					comma = true;
				}
			}
			Debug.Log(string.Format(s));
		}
		usePriority = true;
		for (int i=0; i<priority.Count; i++) {
			actualPriorities.Add(battlers[priority[i]]);
		}
		return actualPriorities.ToArray();
	}

	/*********************
	* Switching Pokemon. *
	*********************/
	public bool CanSwitchLax(int idxPokemon, int pkmnIdxTo, bool showMessages) {
		if (pkmnIdxTo >= 0) {
			Battler[] party = Party(idxPokemon);
			if (pkmnIdxTo >= party.Length) {
				return false;
			}
			if (party[pkmnIdxTo] == null) {
				return false;
			}
			if (party[pkmnIdxTo].pokemon.Egg()) {
				if (showMessages) {
					Display(string.Format("An Egg can't battle!"));
				}
				return false;
			}
			if (!IsOwner(idxPokemon, pkmnIdxTo)) {
				BattleTrainer owner = PartyGetOwner(idxPokemon, pkmnIdxTo);
				if (showMessages) {
					DisplayPaused(string.Format("You can't switch {0}'s Pokémon with one of yours!", owner.name));
				}return false;
			}
			if (party[pkmnIdxTo].hp <= 0) {
				if (showMessages) {
					DisplayPaused(string.Format("{0} has no energy left to battle!", party[pkmnIdxTo].name));
				}return false;
			}
			if (battlers[idxPokemon].pokemonIndex == pkmnIdxTo || battlers[idxPokemon].Partner().pokemonIndex == pkmnIdxTo) {
				if (showMessages) {
					DisplayPaused(string.Format("{0} is already in battle!", party[pkmnIdxTo].name));
				}
				return false;
			}
		}
		return true;
	}

	public bool CanSwitch(int idxPokemon, int pkmnIdxTo, bool showMessages, bool ignoreMeanlook=false) {
		Battler thisPokemon = battlers[idxPokemon];
		if (!CanSwitchLax(idxPokemon, pkmnIdxTo, showMessages)) {
			return false;
		}
		bool isOpposing = IsOpposing(idxPokemon);
		Battler[] party = Party(idxPokemon);
		for (int i=0; i<4; i++) {
			if (isOpposing != IsOpposing(i)) {
				continue;
			}
			if (useMoveChoice[i] == 2 && indexChoice[i] == pkmnIdxTo) {
				if (showMessages) {
					DisplayPaused(string.Format("{0} has already been selected.", party[pkmnIdxTo].name));
				}
				return false;
			}
		}
		if (thisPokemon.HasWorkingItem(Items.SHEDSHELL)) {
			return true;
		}
		if (Settings.USE_NEW_BATTLE_MECHANICS && thisPokemon.HasType(Types.GHOST)) {
			return true;
		}
		if (thisPokemon.effects[Effects.MultiTurn] > 0 || (!ignoreMeanlook && thisPokemon.effects[Effects.MeanLook] >= 0)) {
			if (showMessages) {
				DisplayPaused(string.Format("{0} can't be switched out!", thisPokemon.String()));
			}
			return false;
		}
		if (thisPokemon.effects[Effects.FairyLock] > 0) {
			if (showMessages) {
				DisplayPaused(string.Format("{0} can't be switched out!", thisPokemon.String()));
			}
			return false;
		}
		if (thisPokemon.effects[Effects.Ingrain] != 0) {
			if (showMessages) {
				DisplayPaused(string.Format("{0} can't be switched out!", thisPokemon.String()));
			}
			return false;
		}
		Battler opp1 = thisPokemon.Opposing1();
		Battler opp2 = thisPokemon.Opposing2();
		Battler opp = null;
		if (thisPokemon.HasType(Types.STEEL)) {
			if (opp1.HasWorkingAbility(Abilities.MAGNETPULL)) {
				opp = opp1;
			}
			if (opp2.HasWorkingAbility(Abilities.MAGNETPULL)) {
				opp = opp2;
			}
		}
		if (!thisPokemon.IsAirborne()) {
			if (opp1.HasWorkingAbility(Abilities.ARENATRAP)) {
				opp = opp1;
			}
			if (opp2.HasWorkingAbility(Abilities.ARENATRAP)) {
				opp = opp2;
			}
		}
		if (!thisPokemon.HasWorkingAbility(Abilities.SHADOWTAG)) {
			if (opp1.HasWorkingAbility(Abilities.SHADOWTAG)) {
				opp = opp1;
			}
			if (opp2.HasWorkingAbility(Abilities.SHADOWTAG)) {
				opp = opp2;
			}
		}
		if (opp != null) {
			string abilityname = Abilities.GetName(opp.ability);
			if (showMessages) {
				DisplayPaused(string.Format("{0}'s {1} prevents switching!", opp.String(), abilityname));
			}
			return false;
		}
		return true;
	}

	public bool RegisterSwitch(int idxPokemon, int idxOther) {
		if (!CanSwitch(idxPokemon, idxOther, false)) {
			return false;
		}
		useMoveChoice[idxPokemon] = 2;
		indexChoice[idxPokemon] = idxOther;
		moveChoice[idxPokemon] = null;
		int side = IsOpposing(idxPokemon) ? 1 : 0;
		int owner = GetOwnerIndex(idxPokemon);
		if (megaEvolution[side][owner] == idxPokemon) {
			megaEvolution[side][owner] = -1;
		}
		return true;
	}

	public bool CanChooseNonActive(int index) {
		Battler[] party = Party(index);
		for (int i=0; i<party.Length; i++) {
			if (CanSwitchLax(index, i, false)) {
				return true;
			};
		}
		return false;
	}

	public void Switch(bool favorDraws=false) {
		if (!favorDraws) {
			if (decision > 0) {
				return;
			}
		} else {
			if (decision == 5) {
				return;
			}
		}
		Judge();
		if (decision > 0) {
			return;
		}
		int firstBattlerHP = battlers[0].hp;
		List<int> switched = new List<int>();
		for (int i=0; i<4; i++) {
			if (!doublebattle && IsDoubleBattler(i)) {
				continue;
			}
			if (battlers[i] != null && !battlers[i].Fainted()) {
				continue;
			}
			if (!CanChooseNonActive(i)) {
				continue;
			}
			if (!OwnedByPlayer(i)) {
				if (!IsOpposing(i) || (opponent.Length > 0 && IsOpposing(i))) {
					int newEnemy = SwitchInBetween(i, false, false);
					int newEnemyName = newEnemy;
					if (newEnemy >= 0 && Party(i)[newEnemy].pokemon.Ability() == Abilities.ILLUSION) {
						newEnemyName = GetLastPokemonInTeam(i);
					}
					BattleTrainer opp = GetOwner(i);
					if (!doublebattle && firstBattlerHP > 0 && shiftStyle && opp != null && internalbattle && CanChooseNonActive(0) && IsOpposing(i) && battlers[0].effects[Effects.Outrage] == 0) {
						DisplayPaused(string.Format("{0} is about to send in {1}.", opp.FullName(), Party(i)[newEnemyName].name));
						if (DisplayConfirm(string.Format("Will {0} change Pokémon?", Player().name))) {
							int newPoke = SwitchPlayer(0, true, true);
							if (newPoke >= 0) {
								int newPokeName = newPoke;
								if (party1[newPoke].pokemon.Ability() == Abilities.ILLUSION) {
									newPokeName = GetLastPokemonInTeam(0);
								}
								DisplayBrief(string.Format("{0}, that's enough! Come back!", battlers[0].name));
								RecallAndReplace(0, newPoke, newPokeName);
								switched.Add(0);
							}
						}
					}
					RecallAndReplace(i, newEnemy, newEnemyName, false, false);
					switched.Add(i);
				}
			} else if (opponent.Length > 0) {
				int newPoke = SwitchInBetween(i, true, false);
				int newPokeName = newPoke;
				if (party1[newPoke].pokemon.Ability() == Abilities.ILLUSION) {
					newPokeName = GetLastPokemonInTeam(0);
				}
				DisplayBrief(string.Format("{0}, that's enough! Come back!", battlers[0].name));
				RecallAndReplace(0, newPoke, newPokeName);
				switched.Add(0);
			} else {
				bool swt = false;
				if (!DisplayConfirm("Use next Pokémon?")) {
					swt = Run(i, true) <= 0;
				} else {
					swt = true;
				}
				if (swt) {
					int newPoke = SwitchInBetween(i, true, false);
					int newPokeName = newPoke;
					if (party1[newPoke].pokemon.Ability() == Abilities.ILLUSION) {
						newPokeName = GetLastPokemonInTeam(0);
					}
					DisplayBrief(string.Format("{0}, that's enough! Come back!", battlers[0].name));
					RecallAndReplace(0, newPoke, newPokeName);
					switched.Add(0);
				}
			}
		}
		if (switched.Count > 0) {
			Battler[] p = Priority();
			for (int i=0; i<p.Length; i++) {
				if (switched.Contains(p[i].index)) {
					p[i].AbilitiesOnSwitchIn(true);
				}
			}
		}
	}

	public void SendOut(int index, Pokemon pokemon) {
		SetSeen(pokemon);
		peer.OnEnteringBattle(this, pokemon);
		if (IsOpposing(index)) {
			scene.SendOut(index, pokemon);
		} else {
			scene.SendOut(index, pokemon);
		}
		scene.ResetMoveIndex(index);
	}

	public void Replace(int index, int newPoke, bool batonpass=false) {
		Battler[] party = Party(index);
		int oldPoke = battlers[index].pokemonIndex;
		battlers[index].InitBattle(party[newPoke].pokemon, newPoke, batonpass);
		List<int> partyorder = !IsOpposing(index) ? party1order : party2order;
		int bpo = -1;
		int bpn = -1;
		for (int i=0; i<partyorder.Count; i++) {
			if (partyorder[i] == oldPoke) {
				bpo = i;
			}
			if (partyorder[i] == newPoke) {
				bpn = i;
			}
		}
		int p = partyorder[bpo];
		partyorder[bpo] = partyorder[bpn];
		partyorder[bpn] = p;
		SendOut(index, party[newPoke].pokemon);
		SetSeen(party[newPoke].pokemon);
	}

	public bool RecallAndReplace(int index, int newPoke, int newPokeName, bool batonpass=false, bool moldbreaker=false) {
		battlers[index].ResetForm();
		if (!battlers[index].Fainted()) {
			scene.Recall(index);
		}
		MessagesOnReplace(index, newPoke, newPokeName);
		Replace(index, newPoke, batonpass);
		return OnActiveOne(battlers[index], false, moldbreaker);
	}

	public void MessagesOnReplace(int index, int newPoke, int newPokeName) {
		if (newPokeName < 0) {
			newPokeName = newPoke;
		}
		Battler[] party = Party(index);
		if (OwnedByPlayer(index)) {
			Battler opposing = battlers[index].OppositeOpposing();
			if (opposing.Fainted() || opposing.hp == opposing.totalHP) {
				DisplayBrief(string.Format("Go! {0}!", party[newPokeName].name));
			} else if (opposing.Fainted() || opposing.hp == opposing.totalHP) {
				DisplayBrief(string.Format("Do it! {0}!", party[newPokeName].name));
			} else if (opposing.Fainted() || opposing.hp == opposing.totalHP) {
				DisplayBrief(string.Format("Go for it, {0}!", party[newPokeName].name));
			} else {
				DisplayBrief(string.Format("Your opponent's weak!\nGet 'em, {0}!", party[newPokeName].name));
			}
			Debug.Log(string.Format("[Send out Pokemon] Player sent out {0} in position {1}", party[newPokeName].name, index));
		}
		BattleTrainer owner = GetOwner(index);
		DisplayBrief(string.Format("{0} sent out {1}!", owner.FullName(), party[newPokeName].name));
		Debug.Log(string.Format("[Send out Pokemon] Opponent sent out {0} in position {1}", party[newPokeName].name, index));
	}

	public int SwitchInBetween(int index, bool lax, bool canCancel) {
		if (OwnedByPlayer(index)) {
			return scene.ChooseNewEnemy(index, Party(index));
		} else {
			return SwitchPlayer(index, lax, canCancel);
		}
	}

	public int SwitchPlayer(int index, bool lax, bool canCancel) {
		if (debug) {
			return scene.ChooseNewEnemy(index, Party(index));
		} else {
			return scene.Switch(index, lax, canCancel);
		}
	}

	/*****************
	* Using an Item. *
	*****************/
	// Uses an item on a PokÃ©mon in the player's party.
	public bool UseItemOnPokemon(int item, int pkmnIndex, Battler userPkmn, BattleScene scene) {
		Pokemon pokemon = party1[pkmnIndex].pokemon;
		Battler battler = null;
		string name = GetOwner(userPkmn.index).FullName();
		if (BelongsToPlayer(userPkmn.index)) {
			name = GetOwner(userPkmn.index).name;
		}
		DisplayBrief(string.Format("{0} used the {1}.",name, Items.GetName(item)));
		Debug.Log(string.Format("[Use item] Player used {0} on {1}",Items.GetName(item), pokemon.name));
		bool ret = false;
		if (pokemon.Egg()) {
			Display(string.Format("But it had no effect!"));
		} else {
			for (int i=0; i<4; i++) {
				if (IsOpposing(i) && battlers[i].pokemonIndex==pkmnIndex) {
					battler = battlers[i];
				}
			}
			ret = Items.TriggerBattleUseOnPokemon(item, pokemon, battler, scene);
		}
		if (!ret && BelongsToPlayer(userPkmn.index)) {
			if (PokemonGlobal.Bag.CanStore(item)) {
				PokemonGlobal.Bag.StoreItem(item);
			} else {
				throw new Exception("Couldn't return unused item to Bag somehow.");
			}
		}
		return ret;
	}

	// Uses an item on an active PokÃ©mon.
	public bool UseItemOnBattler(int item, int index, Battler userPkmn, BattleScene scene) {
		Debug.Log(string.Format("[Use item] Player used {0} on {1}",Items.GetName(item), battlers[index].String(true)));
		bool ret = Items.TriggerBattleUseOnBattler(item, battlers[index], scene);
		if (!ret && BelongsToPlayer(userPkmn.index)) {
			if (PokemonGlobal.Bag.CanStore(item)) {
				PokemonGlobal.Bag.StoreItem(item);
			} else {
				throw new Exception("Couldn't return unused item to Bag somehow.");
			}
		}
		return ret;
	}

	public bool RegisterItem(int idxPokemon, int idxItem, int idxTarget=-1) {
		if (idxTarget >= 0) {
			for (int i=0; i<4; i++) {
				if (!battlers[i].IsOpposing(idxPokemon) && battlers[i].pokemonIndex == idxTarget && battlers[i].effects[Effects.Embargo] > 0) {
					Display(string.Format("Embargo's effect prevents the item's use on {0}!",battlers[i].String(true)));
					if (BelongsToPlayer(battlers[i].index)) {
						if (PokemonGlobal.Bag.CanStore(idxItem)) {
							PokemonGlobal.Bag.StoreItem(idxItem);
						} else {
							throw new Exception("Couldn't return unused item to Bag somehow.");
						}
					}
					return false;
				}
			}
		}
		if (Items.HasUseInBattle(idxItem)) {
			if (idxPokemon == 0) {
				if (Items.TriggerBattleUseOnBattler(idxItem, battlers[idxPokemon], this)) {
					Items.TriggerUseInBattle(idxItem, battlers[idxPokemon], this);
					if (doublebattle) {
						battlers[idxPokemon].Partner().effects[Effects.SkipTurn] = 1;
					}
				} else {
					if (PokemonGlobal.Bag.CanStore(idxItem)) {
						PokemonGlobal.Bag.StoreItem(idxItem);
					} else {
						throw new Exception("Couldn't return unused item to Bag somehow.");
					}
					return false;
				}
			} else {
				if (Items.TriggerBattleUseOnBattler(idxItem, battlers[idxPokemon], this)) {
					Display(string.Format("It's impossible to aim without being focused!"));
				}
			}
			return false;
		}
		useMoveChoice[idxPokemon] = 3;
		indexChoice[idxPokemon] = idxItem;
		targetChoice[idxPokemon] = idxTarget;
		int side = IsOpposing(idxPokemon) ? 1 : 0;
		int owner = GetOwnerIndex(idxPokemon);
		if (megaEvolution[side][owner] == idxPokemon) {
			megaEvolution[side][owner] = -1;
		}
		return true;
	}

	public void EnemyUseItem(int item, Battler battler) {
		if (!internalbattle) {
			return;
		}
		List<int> items = new List<int>(GetOwnerItems(battler.index));
		if (items.Count == 0) {
			return;
		}
		for (int i=0; i<items.Count; i++) {
			if (items[i] == item) {
				items.Remove(i);
				break;
			}
		}
		string itemName = Items.GetName(item);
		DisplayBrief(string.Format("{0} used the {1}!",opponent[0].FullName(), itemName));
		Debug.Log(string.Format("[Use Item] Opponent used {0} on {1}",itemName, battler.String(true)));
		if (item == Items.POTION) {
			battler.RecoverHP(20, true);
			Display(string.Format("{0}'s HP was restored.",battler.String()));
		} else if (item == Items.SUPERPOTION) {
			battler.RecoverHP(50, true);
			Display(string.Format("{0}'s HP was restored.",battler.String()));
		} else if (item == Items.HYPERPOTION) {
			battler.RecoverHP(200, true);
			Display(string.Format("{0}'s HP was restored.",battler.String()));
		} else if (item == Items.MAXPOTION) {
			battler.RecoverHP(battler.totalHP-battler.hp, true);
			Display(string.Format("{0}'s HP was restored.",battler.String()));
		} else if (item == Items.FULLRESTORE) {
			bool fullhp = battler.hp == battler.totalHP;
			battler.RecoverHP(battler.totalHP-battler.hp, true);
			battler.status = 0;
			battler.statusCount = 0;
			battler.effects[Effects.Confusion] = 0;
			if (fullhp) {
				Display(string.Format("{0}'s became healthy.",battler.String()));
			} else {
				Display(string.Format("{0}'s HP was restored.",battler.String()));
			}
		} else if (item == Items.FULLRESTORE) {
			battler.status = 0;
			battler.statusCount = 0;
			battler.effects[Effects.Confusion] = 0;
			Display(string.Format("{0}'s became healthy.",battler.String()));
		} else if (item == Items.XATTACK) {
			if (battler.CanIncreaseStatStage(Stats.ATTACK, battler)) {
				battler.IncreaseStat(Stats.ATTACK, 1, battler, true);
			}
		} else if (item == Items.XDEFENSE) {
			if (battler.CanIncreaseStatStage(Stats.DEFENSE, battler)) {
				battler.IncreaseStat(Stats.DEFENSE, 1, battler, true);
			}
		} else if (item == Items.XSPEED) {
			if (battler.CanIncreaseStatStage(Stats.SPEED, battler)) {
				battler.IncreaseStat(Stats.SPEED, 1, battler, true);
			}
		} else if (item == Items.XSPATK) {
			if (battler.CanIncreaseStatStage(Stats.SPATK, battler)) {
				battler.IncreaseStat(Stats.SPATK, 1, battler, true);
			}
		} else if (item == Items.XSPDEF) {
			if (battler.CanIncreaseStatStage(Stats.SPDEF, battler)) {
				battler.IncreaseStat(Stats.SPDEF, 1, battler, true);
			}
		} else if (item == Items.XACCURACY) {
			if (battler.CanIncreaseStatStage(Stats.ACCURACY, battler)) {
				battler.IncreaseStat(Stats.ACCURACY, 1, battler, true);
			}
		}
	}

	/***********************
	* Fleeing from battle. *
	***********************/
	public bool CanRun(int idxPokemon) {
		if (opponent.Length > 0) {
			return false;
		}
		if (cantescape && !IsOpposing(idxPokemon)) {
			return false;
		}
		Battler thisPokemon = battlers[idxPokemon];
		if (thisPokemon.HasType(Types.GHOST) && Settings.USE_NEW_BATTLE_MECHANICS) {
			return true;
		}
		if (thisPokemon.HasWorkingItem(Items.SMOKEBALL)) {
			return true;
		}
		if (thisPokemon.HasWorkingAbility(Abilities.RUNAWAY)) {
			return true;
		}
		return CanSwitch(idxPokemon, -1, false);
	}

	public int Run(int idxPokemon, bool duringBattle=false) {
		Battler thisPokemon = battlers[idxPokemon];
		if (IsOpposing(idxPokemon)) {
			if (opponent.Length > 0) {
				return 0;
			}
			useMoveChoice[idxPokemon] = 5;
			indexChoice[idxPokemon] = 0;
			moveChoice[idxPokemon] = null;
			return -1;
		}
		if (opponent.Length > 0) {
			if (Settings.DEBUG && Input.GetKey("ctrl")) {
				if (DisplayConfirm(string.Format("Treat this battle as a win?"))) {
					decision = 1;
					return 1;
				} else if (DisplayConfirm(string.Format("Treat this battle as a win?"))) {
					decision = 2;
					return 1;
				}
			} else if (internalbattle) {
				DisplayPaused(string.Format("No! There's no running from a Trainer battle!"));
			} else if (DisplayConfirm(string.Format("Would you like to forfeit the match and quit now?"))) {
				Music.SEPlay("Battle flee");
				Display(string.Format("{0} forfeited the match!",Player().name));
				decision = 3;
				return 1;
			}
			return 0;
		}
		if (Settings.DEBUG && Input.GetKey("ctrl")) {
			Music.SEPlay("Battle flee");
			DisplayPaused(string.Format("Got away safely!"));
			decision = 3;
			return 1;
		}
		if (cantescape) {
			DisplayPaused(string.Format("Can't escape!"));
			return 0;
		}
		if (thisPokemon.HasType(Types.GHOST) && Settings.USE_NEW_BATTLE_MECHANICS) {
			Music.SEPlay("Battle flee");
			DisplayPaused(string.Format("Got away safely!"));
			decision = 3;
			return 1;
		}
		if (thisPokemon.HasWorkingAbility(Abilities.RUNAWAY)) {
			Music.SEPlay("Battle flee");
			if (duringBattle) {
				DisplayPaused(string.Format("Got away safely!"));
			} else {
				DisplayPaused(string.Format("{0} escaped using Run Away!",thisPokemon.String()));
			}
			decision = 3;
			return 1;
		}
		if (thisPokemon.HasWorkingItem(Items.SMOKEBALL)) {
			Music.SEPlay("Battle flee");
			if (duringBattle) {
				DisplayPaused(string.Format("Got away safely!"));
			} else {
				DisplayPaused(string.Format("{0} escaped using Run Away!",thisPokemon.String()));
			}
			decision = 3;
			return 1;
		}
		if (!duringBattle && !CanSwitch(idxPokemon, -1, false)) {
			DisplayPaused(string.Format("Can't escape!"));
			return 0;
		}
		int speedPlayer = battlers[idxPokemon]._speed;
		Battler opposing = battlers[idxPokemon].OppositeOpposing();
		if (opposing.Fainted()) {
			opposing = opposing.Partner();
		}
		int rate;
		if (!opposing.Fainted()) {
			int speedEnemy = opposing.speed;
			if (speedPlayer > speedEnemy) {
				rate = 256;
			} else {
				if (speedEnemy <= 0) {
					speedEnemy = 1;
				}
				rate = speedPlayer*128/speedEnemy;
				rate += runCommand*30;
				rate &= 0xFF;
			}
		} else {
			rate = 256;
		}
		int ret = 1;
		if (AIRand(256) < rate) {
			Music.SEPlay("Battle flee");
			DisplayPaused(string.Format("Got away safely!"));
			decision = 3;
		} else {
			DisplayPaused(string.Format("Can't escape!"));
			ret -= 1;
		}
		if (!duringBattle) {
			runCommand += 1;
		}
		return ret;
	}

	/***********************
	* Mega Evolve Battler. *
	***********************/
	public bool CanMegaEvolve(int index) {
		if (Globals.switches[Settings.NO_MEGA_EVOLUTION]) {
			return false;
		}
		if (!battlers[index].hasMega) {
			return false;
		}
		if (IsOpposing(index) && (opponent == null || opponent.Length == 0)) {
			return false;
		}
		if (Settings.DEBUG && Input.GetKey("ctrl")) {
			return true;
		}
		if (!HasMegaRing(index)) {
			return false;
		}
		int side = IsOpposing(index) ? 1 : 0;
		int owner = GetOwnerIndex(index);
		if (megaEvolution[side][owner] != -1) {
			return false;
		}
		if (battlers[index].effects[Effects.SkyDrop] != 0) {
			return false;
		}
		return true;
	}

	public void RegisterMegaEvolution(int index) {
		int side = IsOpposing(index) ? 1 : 0;
		int owner = GetOwnerIndex(index);
		megaEvolution[side][owner] = index;
	}

	public void MegaEvolve(int index) {
		if (battlers[index] == null || battlers[index].pokemon == null) {
			return;
		}
		if (!battlers[index].hasMega) {
			return;
		}
		if (battlers[index].isMega) {
			return;
		}
		string ownerName = GetOwner(index).FullName();
		if (BelongsToPlayer(index)) {
			ownerName = GetOwner(index).name;
		}
		switch (battlers[index].pokemon.MegaMessage()) {
			case 1:
				Display(string.Format("{0}'s fervent wish has reached {1}!",ownerName, battlers[index].String()));
				break;
			default:
				Display(string.Format("{0}'s {1} is reacting to {2}'s {3}",battlers[index].String(), Items.GetName(battlers[index].item), ownerName, GetMegaRingName(index)));
				break;
		}
		CommonAnimation("MegaEvolution", battlers[index], null);
		battlers[index].pokemon.MakeMega();
		battlers[index].form = battlers[index].pokemon.GetForm();
		battlers[index].Update(true);
		scene.ChangePokemon(battlers[index], battlers[index].pokemon);
		CommonAnimation("MegaEvolution2", battlers[index], null);
		string megaName = battlers[index].pokemon.MegaName();
		if (megaName != "") {
			megaName = string.Format("Mega {0}", Species.GetName(battlers[index].pokemon.species));
		}
		Display(string.Format("{0} has Mega Evolved int {1}!",battlers[index].String(), megaName));
		Debug.Log(string.Format("[Mega Evolution] {0} became {1}",battlers[index].String(), megaName));
		int side = IsOpposing(index) ? 1 : 0;
		int owner = GetOwnerIndex(index);
		megaEvolution[side][owner] = -1;
	}

	/*************************
	* Primal Revert Battler. *
	*************************/
	public void PrimalReversion(int index) {
		if (battlers[index] == null || battlers[index].pokemon == null) {
			return;
		}
		if (!battlers[index].hasPrimal) {
			return;
		}
		if (battlers[index].isPrimal) {
			return;
		}
		if (battlers[index].pokemon.species == Species.KYOGRE) {
			CommonAnimation("PrimalKyogre", battlers[index], null);
		}
		if (battlers[index].pokemon.species == Species.GROUDON) {
			CommonAnimation("PrimalGroudon", battlers[index], null);
		}
		battlers[index].pokemon.MakePrimal();
		battlers[index].form = battlers[index].pokemon.GetForm();
		battlers[index].Update(true);
		scene.ChangePokemon(battlers[index], battlers[index].pokemon);
		if (battlers[index].pokemon.species == Species.KYOGRE) {
			CommonAnimation("PrimalKyogre2", battlers[index], null);
		}
		if (battlers[index].pokemon.species == Species.GROUDON) {
			CommonAnimation("PrimalGroudon2", battlers[index], null);
		}
		Display(string.Format("{0}'s Primal Reversion! It reverted to its primal form!",battlers[index].String()));
		Debug.Log(string.Format("[Primal Reversion] {0} Primal Reverted",battlers[index].String()));
	}

	/****************
	* Call Battler. *
	****************/
	public void Call(int index) {
		BattleTrainer owner = GetOwner(index);
		Display(string.Format("{0} called {1}!",owner.name, battlers[index].name));
		Display(string.Format("{0}!",battlers[index].name));
		Debug.Log(string.Format("[Call to Pokemon] {0} called to {1}",owner.name, battlers[index].String(true)));
		if (battlers[index].status != Statuses.SLEEP && battlers[index].CanIncreaseStatStage(Stats.ACCURACY, battlers[index])) {
			battlers[index].IncreaseStat(Stats.ACCURACY, 1, battlers[index], true);
		} else {
			Display(string.Format("But nothing happened!"));
		}
	}

	/**********************
	* Gaining Experience. *
	**********************/
	public void GainEXP() {
		if (!internalbattle) {
			return;
		}
		bool successbegin = true;
		for (int i=0; i<4; i++) {
			if (!doublebattle && IsDoubleBattler(i)) {
				battlers[i].participants = new List<int>();
				continue;
			}
			if (IsOpposing(i) && battlers[i].participants.Count > 0 && (battlers[i].Fainted() || battlers[i].captured)) {
				bool haveExpAll = PokemonGlobal.Bag.HasItem(Items.EXPALL);
				int partic = 0;
				int expShare = 0;
				for (int j=0; j<battlers[i].participants.Count; j++) {
					if (party1[battlers[i].participants[j]] == null || !IsOwner(0, battlers[i].participants[j])) {
						continue;
					}
					if (party1[battlers[i].participants[j]].hp > 0 && !party1[battlers[i].participants[j]].pokemon.Egg()) {
						partic += 1;
					}
				}
				if (!haveExpAll) {
					for (int j=0; j<party1.Length; i++) {
						if (party1[j] == null || !IsOwner(0, j)) {
							continue;
						}
						if (party1[j].hp > 0 && !party1[j].pokemon.Egg() && (party1[j].item == Items.EXPSHARE || party1[j].pokemon.itemInitial == Items.EXPSHARE)) {
							expShare += 1;
						}
					}
				}
				if (partic > 0 || expShare > 0 || haveExpAll) {
					if ((opponent == null || opponent.Length == 0) && successbegin && AllFainted(party2)) {
						scene.WildBattleSuccess();
						successbegin = false;
					}
					for (int j=0; j<party1.Length; j++) {
						if (party1[j] == null || !IsOwner(0, j)) {
							continue;
						}
						if (party1[j].hp <= 0 || party1[j].pokemon.Egg()) {
							continue;
						}
						bool haveExpShare = party1[j].item == Items.EXPSHARE || party1[j].pokemon.itemInitial == Items.EXPSHARE;
						if (!haveExpShare && !battlers[i].participants.Contains(j)) {
							continue;
						}
						GainEXPOne(j, battlers[i], partic, expShare, haveExpAll);
					}
					if (haveExpAll) {
						bool showMessage = true;
						for (int j=0; j<party1.Length; j++) {
							if (party1[j] == null || !IsOwner(0, j)) {
								continue;
							}
							if (party1[j].hp <= 0 || party1[j].pokemon.Egg()) {
								continue;
							}
							if (party1[j].item == Items.EXPSHARE || party1[j].pokemon.itemInitial == Items.EXPSHARE) {
								continue;
							}
							if (battlers[i].participants.Contains(j)) {
								continue;
							}
							if (showMessage) {
								DisplayPaused(string.Format("The rest of your team gained Exp. Points thanks to the {0}!",Items.GetName(Items.EXPALL)));
							}
							showMessage = false;
							GainEXPOne(j, battlers[i], partic, expShare, haveExpAll, false);
						}
					}
				}
				battlers[i].participants = new List<int>();
			}
		}
	}

	public void GainEXPOne(int index, Battler defeated, int participants, int expShare, bool haveExpAll, bool showMessages=true) {
		Pokemon thisPokemon = party1[index].pokemon;
		int level = defeated.level;
		int baseExp = defeated.pokemon.BaseExp();
		int[] evYield = defeated.pokemon.EvYield();
		int totalEv = 0;
		for (int k=0; k<6; k++) {
			totalEv += thisPokemon.ev[k];
		}
		for (int k=0; k<6; k++) {
			int evGain = evYield[k];
			if (thisPokemon.item == Items.MACHOBRACE || thisPokemon.itemInitial == Items.MACHOBRACE) {
				evGain *= 2;
			}
			switch (k) {
				case Stats.HP:
					if (thisPokemon.item == Items.POWERWEIGHT || thisPokemon.itemInitial == Items.POWERWEIGHT) {
						evGain += 4;
					}
					break;
				case Stats.ATTACK:
					if (thisPokemon.item == Items.POWERBRACER || thisPokemon.itemInitial == Items.POWERBRACER) {
						evGain += 4;
					}
					break;
				case Stats.DEFENSE:
					if (thisPokemon.item == Items.POWERBELT || thisPokemon.itemInitial == Items.POWERBELT) {
						evGain += 4;
					}
					break;
				case Stats.SPATK:
					if (thisPokemon.item == Items.POWERLENS || thisPokemon.itemInitial == Items.POWERLENS) {
						evGain += 4;
					}
					break;
				case Stats.SPDEF:
					if (thisPokemon.item == Items.POWERBAND || thisPokemon.itemInitial == Items.POWERBAND) {
						evGain += 4;
					}
					break;
				case Stats.SPEED:
					if (thisPokemon.item == Items.POWERANKLET || thisPokemon.itemInitial == Items.POWERANKLET) {
						evGain += 4;
					}
					break;
			}
			if (thisPokemon.PokerusStage() >= 1) {
				if (evGain > 0) {
					if (totalEv + evGain > Pokemon.EV_LIMIT) {
						evGain -= totalEv + evGain - Pokemon.EV_LIMIT;
					}
					if (thisPokemon.ev[k] + evGain > Pokemon.EV_STAT_LIMIT) {
						evGain -= thisPokemon.ev[k] + evGain - Pokemon.EV_STAT_LIMIT;
					}
					thisPokemon.ev[k] += evGain;
					if (thisPokemon.ev[k] > Pokemon.EV_STAT_LIMIT) {
						Debug.Log(string.Format("Single-stat EV limit {0} exceeded. Stat: {1}, EV Gain: {2}, EVs: {3}",Pokemon.EV_STAT_LIMIT, k, evGain, thisPokemon.ev));
						thisPokemon.ev[k] = Pokemon.EV_STAT_LIMIT;
					}
					totalEv += evGain;
					if (thisPokemon.ev[k] > Pokemon.EV_LIMIT) {
						Debug.Log(string.Format("Single-stat EV limit {0} exceeded. Total EVs: {1}, EV Gain: {2}, EVs: {3}",Pokemon.EV_LIMIT, totalEv, evGain, thisPokemon.ev));
					}
				}
			}
		}
		int isPartic = 0;
		if (defeated.participants.Contains(index)) {
			isPartic = 1;
		}
		int haveExpShare = (thisPokemon.item == Items.EXPSHARE || thisPokemon.itemInitial == Items.EXPSHARE) ? 1 : 0;
		int exp = 0;
		if (expShare > 0) {
			if (participants == 0) {
				exp = level*baseExp;
				exp = (exp/(Settings.NO_SPLIT_EXP ? 1 : expShare))*haveExpShare;
			} else {
				if (Settings.NO_SPLIT_EXP) {
					exp = (level*baseExp)*isPartic;
					if (isPartic == 0) {
						exp = (level*baseExp/2) * haveExpShare;
					}
				} else {
					exp = level*baseExp;
					exp = (exp/participants)*isPartic + (exp/expShare)*haveExpShare;
				}
			}
		} else if (isPartic == 1) {
			exp = level * baseExp/(Settings.NO_SPLIT_EXP ? 1 : participants);
		} else if (haveExpAll) {
			exp = level*baseExp/2;
		}
		if (exp <= 0) {
			return;
		}
		if (opponent.Length > 0) {
			exp = exp*3/2;
		}
		if (Settings.USE_SCALED_EXP_FORMULA) {
			exp = exp/5;
			double levelAdjust = (2*level+10.0)/(level+thisPokemon.Level()+10.0);
			levelAdjust = Math.Pow(levelAdjust, 5);
			levelAdjust = Math.Sqrt(levelAdjust);
			exp = (int)(exp*levelAdjust);
			if (isPartic > 0 || haveExpShare > 0) {
				exp += 1;
			}
		} else {
			exp = exp/7;
		}
		bool isOutsider = thisPokemon.trainerID != Player().id || (thisPokemon.language != 0 && thisPokemon.language != Player().language);
		if (isOutsider) {
			if (thisPokemon.language != 0 && thisPokemon.language != Player().language) {
				exp = (int)(exp*1.7f);
			} else {
				exp = exp*3/2;
			}
		}
		if (thisPokemon.item == Items.LUCKYEGG || thisPokemon.itemInitial == Items.LUCKYEGG) {
			exp = exp*3/2;
		}
		int growthRate = thisPokemon.GrowthRate();
		int newExp = Experience.AddExperience(thisPokemon.exp, exp, growthRate);
		exp = newExp - thisPokemon.exp;
		if (exp > 0) {
			if (showMessages) {
				if (isOutsider) {
					DisplayPaused(string.Format("{0} gained a boosted {1} Exp. Points!",thisPokemon.name, exp));
				} else {
					DisplayPaused(string.Format("{0} gained {1} Exp. Points!",thisPokemon.name, exp));
				}
			}
			int newLevel = Experience.GetLevelFromExperience(newExp, growthRate);
			int curLevel = thisPokemon.Level();
			if (newLevel < curLevel) {
				string debugInfo = string.Format("{0}: {1}/{2} | {3}/{4} | gain: {5}", thisPokemon.name, thisPokemon.Level(), newLevel, thisPokemon.exp, newExp, exp);
				throw new Exception(string.Format("The new level ({0}) is less than the Pokemon's current level ({1}), which shouldn't happen. {2}", newLevel, curLevel, debugInfo));
			}
			int tempExp1 = thisPokemon.exp;
			int tempExp2 = 0;
			Battler battler = FindPlayerBattler(index);
			while (true) {
				int startExp = Experience.GetStartExperience(curLevel, growthRate);
				int endExp = Experience.GetStartExperience(curLevel+1, growthRate);
				tempExp2 = (endExp < newExp) ? endExp : newExp;
				thisPokemon.exp = tempExp2;
				scene.EXPBar(thisPokemon, battler, startExp, endExp, tempExp1, tempExp2);
				tempExp1 = tempExp2;
				curLevel++;
				if (curLevel > newLevel) {
					thisPokemon.CalcStats();
					if (battler != null) {
						battler.Update(false);
					}
					scene.Refresh();
					break;
				}
				int oldtotalHP = thisPokemon.totalHP;
				int oldAttack = thisPokemon.attack;
				int oldDefense = thisPokemon.defense;
				int oldSpeed = thisPokemon.speed;
				int oldSpAtk = thisPokemon.spatk;
				int oldSpDef = thisPokemon.spdef;
				if (battler != null && battler.pokemon != null && internalbattle) {
					battler.pokemon.ChangeHappiness("levelup");
				}
				thisPokemon.CalcStats();
				if (battler != null) {
					battler.Update();
				}
				scene.Refresh();
				DisplayPaused(string.Format("{0} grew to Level {1}!",thisPokemon.name, curLevel));
				scene.LevelUp(thisPokemon, battler, oldtotalHP, oldAttack, oldDefense, oldSpeed, oldSpAtk, oldSpDef);
				List<int[]> moveList = thisPokemon.GetMoveList();
				for (int k=0; k<moveList.Count; k++) {
					if (moveList[k][0] == thisPokemon.Level()) {
						LearnMove(index, moveList[k][1]);
					}
				}
			}
		}
	}

	/*******************
	* Learning a move. *
	*******************/
	public void LearnMove(int pkmnIndex, int move) {
		Pokemon pokemon = party1[pkmnIndex].pokemon;
		if (pokemon == null) {
			return;
		}
		string pokemonName = pokemon.name;
		Battler battler = FindPlayerBattler(pkmnIndex);
		string moveName = Moves.GetName(move);
		for (int i=0; i<4; i++) {
			if (pokemon.moves[i].Id == move) {
				return;
			}
			if (pokemon.moves[i].Id == 0) {
				pokemon.moves[i] = new Moves.Move(move);
				if (battler != null) {
					battler.moves[i] = BattleMove.FromBattleMove(this, pokemon.moves[i]);
				}
				DisplayPaused(string.Format("{0} learned {1}!", pokemonName,moveName));
				Debug.Log(string.Format("[Learn Move] {0} learned {1}", pokemonName, moveName));
			}
		}
		while (true) {
			DisplayPaused(string.Format("{0} is trying to learn {1}.", pokemonName, moveName));
			DisplayPaused(string.Format("But {0} can't learn more than four moves.", pokemonName));
			if (DisplayConfirm(String.Format("Delete a move to make room for {0}?", moveName))) {
				DisplayPaused(string.Format("Which move should be forgotten?"));
				int forgetMove = scene.ForgetMove(pokemon, move);
				if (forgetMove >= 0) {
					string oldMoveName = Moves.GetName(pokemon.moves[forgetMove].Id);
					pokemon.moves[forgetMove] = new Moves.Move(move);
					if (battler != null) {
						battler.moves[forgetMove] = BattleMove.FromBattleMove(this, pokemon.moves[forgetMove]);
						DisplayPaused(string.Format("1, 2, and... ... ..."));
						DisplayPaused(string.Format("Poof!"));
						DisplayPaused(string.Format("{0} forgot {1}.", pokemonName, oldMoveName));
						DisplayPaused(string.Format("And..."));
						DisplayPaused(string.Format("{0} learned {1}!", pokemonName, moveName));
						Debug.Log(string.Format("[Learn Move] {0} learned {1}", pokemonName, moveName));
					}
				} else if (DisplayConfirm(string.Format("Should {0} stop learning {1}?", pokemonName, moveName))) {
					DisplayPaused(string.Format("{0} did not learn {1}.", pokemonName, moveName));
					return;
				}
			} else if (DisplayConfirm(string.Format("Should {0} stop learning {1}?", pokemonName, moveName))) {
				DisplayPaused(string.Format("{0} did not learn {1}.", pokemonName, moveName));
				return;
			}
		}
	}

	/*************
	* Abilities. *
	*************/
	public void OnActiveAll() {
		for (int i=0; i<4; i++) {
			if (IsOpposing(i)) {
				battlers[i].UpdateParticipants();
				if (!IsOpposing(i) && (battlers[i].item == Items.AMULETCOIN || battlers[i].item == Items.LUCKINCENSE)) {
					amuletcoin = true;
				}
			}
		}
		usePriority = false;
		Battler[] pri = Priority();
		for (int i=0; i<pri.Length; i++) {
			pri[i].AbilitiesOnSwitchIn(true);
		}
		for (int i=0; i<4; i++) {
			if (battlers[i].Fainted()) {
				continue;
			}
			battlers[i].CheckForm();
		}
	}

	public bool OnActiveOne(Battler pkmn, bool onlyAbilities=false, bool moldbreaker=false) {
		if (pkmn.Fainted()) {
			return false;
		}
		if (!onlyAbilities) {
			for (int i=0; i<4; i++) {
				if (IsOpposing(i)) {
					battlers[i].UpdateParticipants();
				}
				if (!IsOpposing(i) && (battlers[i].item == Items.AMULETCOIN || battlers[i].item == Items.LUCKINCENSE)) {
					amuletcoin = true;
				}
				if (pkmn.effects[Effects.HealingWish] != 0) {
					Debug.Log(string.Format("[Lingering effect triggered] {0}'s Healing Wish"));
					CommonAnimation("HealingWish", pkmn, null);
					DisplayPaused(string.Format("The healing wish came true for {0}!", pkmn.String(true)));
					pkmn.RecoverHP(pkmn.totalHP, true);
					pkmn.CureStatus(false);
					pkmn.effects[Effects.HealingWish] = 0;
				}
				if (pkmn.effects[Effects.LunarDance] != 0) {
					Debug.Log(string.Format("[Lingering effect triggered] {0}'s Lunar Dance"));
					CommonAnimation("LunarDance", pkmn, null);
					DisplayPaused(string.Format("{0} became cloaked in mystical moonlight!", pkmn.String(true)));
					pkmn.RecoverHP(pkmn.totalHP, true);
					pkmn.CureStatus(false);
					for (int j=0; j<4; j++) {
						pkmn.moves[i].pp = pkmn.moves[i].totalPP;
					}
					pkmn.effects[Effects.LunarDance] = 0;
				}
				if (pkmn.effects[Effects.Spikes] > 0 && !pkmn.IsAirborne(moldbreaker)) {
					if (!pkmn.HasWorkingAbility(Abilities.MAGICGUARD)) {
						Debug.Log(string.Format("[Entry hazard] {0} triggered Spikes", pkmn.String()));
						int spikesDiv;
						if (pkmn.OwnSide().effects[Effects.Spikes] == 1) {
							spikesDiv = 8;
						} else if (pkmn.OwnSide().effects[Effects.Spikes] == 2) {
							spikesDiv = 6;
						} else {
							spikesDiv = 4;
						}
						scene.DamageAnimation(pkmn, 0);
						pkmn.ReduceHP(pkmn.totalHP/spikesDiv);
						DisplayPaused(string.Format("{0} is hurt by the spikes!", pkmn.String()));
					}
				}
				if (pkmn.Fainted()) {
					pkmn.Faint();
				}
				if (pkmn.effects[Effects.StealthRock] > 0 && !pkmn.Fainted()) {
					if (!pkmn.HasWorkingAbility(Abilities.MAGICGUARD)) {
						int atype = Types.ROCK;
						int eff = Types.GetCombinedEffectiveness(atype, pkmn.type1, pkmn.type2, pkmn.effects[Effects.Type3]);
						if (eff > 0) {
							Debug.Log(string.Format("[Entry hazard] {0} triggered Stealth Rock", pkmn.String()));
							scene.DamageAnimation(pkmn, 0);
							pkmn.ReduceHP(pkmn.totalHP*eff/64);
							DisplayPaused(string.Format("Pointed stones dug into {0}!", pkmn.String()));
						}
					}
				}
				if (pkmn.Fainted()) {
					pkmn.Faint();
				}
				if (pkmn.OwnSide().effects[Effects.ToxicSpikes] > 0 && !pkmn.Fainted()) {
					if (!pkmn.IsAirborne(moldbreaker)) {
						if (pkmn.HasType(Types.POISON)) {
							Debug.Log(string.Format("[Entry Hazard] {0} absorbed Toxic Spikes", pkmn.String()));
							pkmn.OwnSide().effects[Effects.ToxicSpikes] = 0;
							DisplayPaused(string.Format("{0} absorbed the poison spikes!", pkmn.String()));
						} else if (pkmn.CanPoisonSpikes(moldbreaker)) {
							Debug.Log(string.Format("[Entry Hazard] {0} triggered Toxis Spikes", pkmn.String()));
							if (pkmn.OwnSide().effects[Effects.ToxicSpikes] == 2) {
								pkmn.Poison(null, string.Format("{0} was badly poisoned by the poison spikes!", pkmn.String()), true);
							} else {
								pkmn.Poison(null, string.Format("{0} was poisoned by the poison spikes!", pkmn.String()));
							}
						}
					}
				}
				if (pkmn.OwnSide().effects[Effects.StickyWeb] != 0 && !pkmn.Fainted() && !pkmn.IsAirborne(moldbreaker)) {
					if (pkmn.CanReduceStatStage(Stats.SPEED, null, false, null, moldbreaker)) {
						Debug.Log(string.Format("[Entry Hazard] {0} triggered Sticky Web", pkmn.String()));
						pkmn.ReduceStat(Stats.SPEED, 1, null, false, null, true, moldbreaker);
						DisplayPaused(string.Format("{0} was caught in a sticky web!", pkmn.String()));
					}
				}
			}
		}
		pkmn.AbilityCureCheck();
		if (pkmn.Fainted()) {
			GainEXP();
			Judge();
			return false;
		}
		if (!onlyAbilities) {
			pkmn.CheckForm();
			pkmn.BerryCureCheck();
		}
		return true;
	}

	public void PrimordialWeather() {
		bool hasAbil = false;
		switch (weather) {
			case Weather.HEAVYRAIN:
				for (int i=0; i<4; i++) {
					if (battlers[i].ability == Abilities.PRIMORDIALSEA && !battlers[i].Fainted()) {
						hasAbil = true;
						break;
					}
				}
				if (!hasAbil) {
					weather = 0;
					DisplayBrief(string.Format("The heavy rain has lifted!"));
				}
				break;
			case Weather.HARSHSUN:
				for (int i=0; i<4; i++) {
					if (battlers[i].ability == Abilities.DESOLATELAND && !battlers[i].Fainted()) {
						hasAbil = true;
						break;
					}
				}
				if (!hasAbil) {
					weather = 0;
					DisplayBrief(string.Format("The harsh sunlight faded!"));
				}
				break;
			case Weather.STRONGWINDS:
				for (int i=0; i<4; i++) {
					if (battlers[i].ability == Abilities.DELTASTREAM && !battlers[i].Fainted()) {
						hasAbil = true;
						break;
					}
				}
				if (!hasAbil) {
					weather = 0;
					DisplayBrief(string.Format("The mysterious air current has dissipated!"));
				}
				break;
		}
	}

	/***********
	* Judging. *
	***********/
	public void JudgeCheckpoint(Battler attacker, int move=0) {
		return;
	}

	public int DecisionOnTime() {
		int count1 = 0;
		int count2 = 0;
		int hptotal1 = 0;
		int hptotal2 = 0;
		for (int i=0; i<party1.Length; i++) {
			if (party1[i] == null) {
				continue;
			}
			if (party1[i].hp > 0 && !party1[i].pokemon.Egg()) {
				count1 += 1;
				hptotal1 += party1[i].hp;
			}
		}
		for (int i=0; i<party2.Length; i++) {
			if (party2[i] == null) {
				continue;
			}
			if (party2[i].hp > 0 && !party2[i].pokemon.Egg()) {
				count2 += 1;
				hptotal2 += party2[i].hp;
			}
		}
		if (count1 > count2) {
			return 1;
		}
		if (count1 < count2) {
			return 2;
		}
		if (hptotal1 > hptotal2) {
			return 1;
		}
		if (hptotal1 < hptotal2) {
			return 2;
		}
		return 5;
	}

	public int DecisionOnTime2() {
		int count1 = 0;
		int count2 = 0;
		int hptotal1 = 0;
		int hptotal2 = 0;
		for (int i=0; i<party1.Length; i++) {
			if (party1[i] == null) {
				continue;
			}
			if (party1[i].hp > 0 && !party1[i].pokemon.Egg()) {
				count1 += 1;
				hptotal1 += party1[i].hp*100/party1[i].totalHP;
			}
		}
		for (int i=0; i<party2.Length; i++) {
			if (party2[i] == null) {
				continue;
			}
			if (party2[i].hp > 0 && !party2[i].pokemon.Egg()) {
				count2 += 1;
				hptotal2 += party2[i].hp*100/party2[i].totalHP;
			}
		}
		if (count1 > count2) {
			return 1;
		}
		if (count1 < count2) {
			return 2;
		}
		if (hptotal1 > hptotal2) {
			return 1;
		}
		if (hptotal1 < hptotal2) {
			return 2;
		}
		return 5;
	}

	public int DecisionOnDraw() {
		return 5;
	}

	public void Judge() {
		if (AllFainted(party1) && AllFainted(party2)) {
			decision = DecisionOnDraw();
			return;
		}
		if (AllFainted(party1)) {
			decision = 2;
			return;
		}
		if (AllFainted(party2)) {
			decision = 1;
			return;
		}
	}

	/*****************************
	* Messaging and Animaations. *
	*****************************/
	public void Display(string msg) {
		scene.DisplayMessage(msg);
	}

	public void DisplayPaused(string msg) {
		scene.DisplayPausedMessage(msg);
	}

	public void DisplayBrief(string msg) {
		scene.DisplayMessage(msg, true);
	}

	public bool DisplayConfirm(string msg) {
		return scene.DisplayConfirmMessage(msg);
	}

	public void ShowCommands(string msg, string[] commands, bool canCancel=true) {
		scene.ShowCommands(msg, commands, canCancel ? 1:-1);
	}

	public void Animation(int move, Battler attacker, Battler opponent, int hitNum=0) {
		if (battlescene) {
			scene.Animation(move, attacker, opponent, hitNum);
		}
	}

	public void CommonAnimation(string name, Battler attacker, Battler opponent, int hitNum=0) {
		if (battlescene) {
			scene.CommonAnimation(name, attacker, opponent, hitNum);
		}
	}

	/***************
	* Battle core. *
	***************/
	public int StartBattle(bool canLose=false) {
		Debug.Log(string.Format(""));
		Debug.Log(string.Format("*******************************"));
		try {
			StartBattleCore(canLose);
		} catch (Exception e) {
			Debug.Log(string.Format("Ending with decision 0 because of exception: {0}",e.Message));
			decision = 0;
			scene.EndBattle(decision);
		}
		return decision;
	}

	public int StartBattleCore(bool canLose) {
		if (!fullparty1 && party1.Length > MAXPARTYSIZE) {
			throw new Exception(string.Format("Party 1 has more than {0} Pokemon.", MAXPARTYSIZE));
		}
		if (!fullparty2 && party2.Length > MAXPARTYSIZE) {
			throw new Exception(string.Format("Party 2 has more than {0} Pokemon.", MAXPARTYSIZE));
		}
		// Initialize Wild Pokemon
		if ((opponent == null || opponent.Length == 0) || opponent.Length == 0) {
			if (party2.Length == 1) {
				if (doublebattle) {
					throw new Exception("Only two wild Pokemon are allowed in double battles");
				}
				Pokemon wildPoke = party2[0].pokemon;
				battlers[1].InitBattle(wildPoke, 0, false);
				peer.OnEnteringBattle(this, wildPoke);
				SetSeen(wildPoke);
				scene.StartBattle(this);
				DisplayPaused(string.Format("Wild {0} appeared!", wildPoke.name));
			} else if (party2.Length == 2) {
				if (!doublebattle) {
					throw new Exception("Only one wild Pokemon is allowed in single battles");
				}
				battlers[1].InitBattle(party2[0].pokemon, 0, false);
				battlers[3].InitBattle(party2[1].pokemon, 1, false);
				peer.OnEnteringBattle(this, party2[0].pokemon);
				peer.OnEnteringBattle(this, party2[1].pokemon);
				SetSeen(party2[0].pokemon);
				SetSeen(party2[1].pokemon);
				scene.StartBattle(this);
				DisplayPaused(string.Format("Wild {0} and {1} appeared!", party2[0].name, party2[1].name));
			} else {
				throw new Exception("Only one or two wild Pokemon are allowed");
			}
		// Initialize opponents in double battles
		} else if (doublebattle) {
			if (opponent.Length == 0 || opponent.Length > 2) {
				throw new Exception("Opponents with zero or more than two people are not allowed");
			}
			if (player.Length == 0 || player.Length > 2) {
				throw new Exception("Player trainers with zero or more than two people are not allowed");
			}
			scene.StartBattle(this);
			if (opponent.Length > 1) {
				DisplayPaused(string.Format("{0} and {1} want to battle!", opponent[0].FullName(), opponent[1].FullName()));
				int sendOut1 = FindNextUnfainted(party2, 0, SecondPartyBegin(1));
				if (sendOut1 < 0) {
					throw new Exception("Opponent 1 has no unfainted Pokemon");
				}
				int sendOut2 = FindNextUnfainted(party2, SecondPartyBegin(1));
				if (sendOut2 < 0) {
					throw new Exception("Opponent 2 has no unfainted Pokemon");
				}
				battlers[1].InitBattle(party2[sendOut1].pokemon, sendOut1, false);
				DisplayBrief(string.Format("{0} sent out {1}!", opponent[0].FullName(), battlers[1].name));
				SendOut(1, party2[sendOut1].pokemon);
				battlers[3].InitBattle(party2[sendOut2].pokemon, sendOut2, false);
				DisplayBrief(string.Format("{0} sent out {1}!", opponent[1].FullName(), battlers[3].name));
				SendOut(3, party2[sendOut2].pokemon);
			} else {
				DisplayPaused(string.Format("{0} wants to battle!", opponent[0].FullName()));
				int sendOut1 = FindNextUnfainted(party2, 0);
				int sendOut2 = FindNextUnfainted(party2, sendOut1+1);
				if (sendOut1 < 0 || sendOut2 < 0) {
					throw new Exception("Opponent doesn't have two unfainted Pokemon");
				}
				battlers[1].InitBattle(party2[sendOut1].pokemon, sendOut1, false);
				battlers[3].InitBattle(party2[sendOut2].pokemon, sendOut2, false);
				DisplayBrief(string.Format("{0} sent out {2} and {3}", opponent[0].FullName(), battlers[1].name, battlers[3].name));
				SendOut(1, party2[sendOut1].pokemon);
				SendOut(3, party2[sendOut2].pokemon);
			}
		// Initialize opponent in single battles
		} else {
			int sendOut = FindNextUnfainted(party2, 0);
			if (sendOut < 0) {
				throw new Exception("Trainer has no unfainted Pokemon");
			}
			if (opponent.Length != 1) {
				throw new Exception("Opponent trainer must be only one person in single battle");
			}
			if (player.Length != 1) {
				throw new Exception("Player trainer must be only one person in single battle");
			}
			Pokemon trainerPoke = party2[sendOut].pokemon;
			scene.StartBattle(this);
			DisplayPaused(string.Format("{0} would like to battle!", opponent[0].FullName()));
			battlers[1].InitBattle(trainerPoke, sendOut, false);
			DisplayBrief(string.Format("{0} send out {1}!", opponent[0].FullName(), battlers[1].name));
			SendOut(1, trainerPoke);
		}
		// Initialize players in double battles
		if (doublebattle) {
			int sendOut1;
			int sendOut2;
			if (player.Length > 1) {
				sendOut1 = FindNextUnfainted(party1, 0, SecondPartyBegin(0));
				if (sendOut1 < 0) {
					throw new Exception("Player 1 has no unfainted Pokemon");
				}
				sendOut2 = FindNextUnfainted(party1, 0, SecondPartyBegin(0));
				if (sendOut2 < 0) {
					throw new Exception("Player 2 has no unfainted Pokemon");
				}
				battlers[0].InitBattle(party1[sendOut1].pokemon, sendOut1, false);
				battlers[2].InitBattle(party1[sendOut2].pokemon, sendOut2, false);
				DisplayBrief(string.Format("{0} send out {1}! Go {2}!", player[1].FullName(), battlers[2].name, battlers[0].name));
				SetSeen(party1[sendOut1].pokemon);
				SetSeen(party1[sendOut2].pokemon);
			} else {
				sendOut1 = FindNextUnfainted(party1, 0);
				if (sendOut1 < 0) {
					throw new Exception("Player doesn't have two unfainted Pokemon");
				}
				sendOut2 = FindNextUnfainted(party1, sendOut1+1);
				if (sendOut2 < 0) {
					throw new Exception("Player doesn't have two unfainted Pokemon");
				}
				battlers[0].InitBattle(party1[sendOut1].pokemon, sendOut1, false);
				battlers[2].InitBattle(party1[sendOut2].pokemon, sendOut2, false);
				DisplayBrief(string.Format("Go {0} and {1}!", battlers[0].name, battlers[2].name));
			}
			SendOut(0, party1[sendOut1].pokemon);
			SendOut(2, party1[sendOut2].pokemon);
		// Initialize player in single battles
		} else {
			int sendOut = FindNextUnfainted(party1, 0);
			if (sendOut < 0) {
				throw new Exception("Player has no unfainted Pokemon!");
			}
			battlers[0].InitBattle(party1[sendOut].pokemon, sendOut, false);
			DisplayBrief(string.Format("Go! {0}!", battlers[0].name));
			SendOut(0, party1[sendOut].pokemon);
		}
		// Initialize battle
		switch (weather) {
			case Weather.SUNNYDAY:
				CommonAnimation("Sunny", null, null);
				Display(string.Format("The sunlight is strong."));
				break;
			case Weather.RAINDANCE:
				CommonAnimation("Rain", null, null);
				Display(string.Format("It is raining."));
				break;
			case Weather.SANDSTORM:
				CommonAnimation("Sandstorm", null, null);
				Display(string.Format("A sandstorm is raging."));
				break;
			case Weather.HAIL:
				CommonAnimation("Hail", null, null);
				Display(string.Format("Hail is falling."));
				break;
			case Weather.HEAVYRAIN:
				CommonAnimation("HeavyRain", null, null);
				Display(string.Format("It is raining heavily."));
				break;
			case Weather.HARSHSUN:
				CommonAnimation("HarshSun", null, null);
				Display(string.Format("The sunlight is extremely harsh."));
				break;
			case Weather.STRONGWINDS:
				CommonAnimation("StrongWinds", null, null);
				Display(string.Format("The wind is stong."));
				break;
		}
		OnActiveAll();
		turnCount = 0;
		while (true) {
			Debug.Log(string.Format(""));
			Debug.Log(string.Format("***Round {0}***", turnCount+1));
			if (debug && turnCount >= 100) {
				decision = DecisionOnTime();
				Debug.Log(string.Format("***Undecided after 100 rounds, aborting***"));
				Abort();
				break;
			}
			CommandPhase();
			AttackPhase();
			EndOfRoundPhase();
			if (decision > 0) {
				break;
			}
			turnCount++;
		}
		return EndOfBattle(canLose);
	}

	/*****************
	* Command Phase. *
	*****************/
	public int CommandMenu(int i) {
		return scene.CommandMenu(i);
	}

	public int[] ItemMenu(int i) {
		return scene.ItemMenu(i);
	}

	public bool AutoFightMenu(int i) {
		return false;
	}

	public void CommandPhase() {
		scene.BeginCommandPhase();
		for (int i=0; i<4; i++) {
			battlers[i].effects[Effects.SkipTurn] = 0;
			if (CanShowCommands(i) || battlers[i].Fainted()) {
				useMoveChoice[i] = 0;
				indexChoice[i] = 0;
				moveChoice[i] = null;
				targetChoice[i] = -1;
			} else {
				if (doublebattle || !IsDoubleBattler(i)) {
					Debug.Log(string.Format("[Reuing commands] {0}", battlers[i].String(true)));
				}
			}
		}
		for (int i=0; i<2; i++) {
			for (int j=0; j<megaEvolution[i].Length; j++) {
				if (megaEvolution[i][j] >= 0) {
					megaEvolution[i][j] = -1;
				}
			}
		}
		for (int i=0; i<4; i++) {
			if (decision != 0) {
				break;
			}
			if (useMoveChoice[i] != 0) {
				continue;
			}
			if (!OwnedByPlayer(i) || controlPlayer) {
				if (!battlers[i].Fainted() && CanShowCommands(i)) {
					scene.ChooseEnemyCommand(i);
				}
			} else {
				bool commandDone = false;
				if (CanShowCommands(i)) {
					while (true) {
						int cmd = CommandMenu(i);
						if (cmd == 0) {
							if (CanShowFightMenu(i)) {
								if (AutoFightMenu(i)) {
									commandDone = true;
								}
								while (!commandDone) {
									int index = scene.FightMenu(i);
									if (index < 0) {
										int side = IsOpposing(i) ? 1 : 0;
										int owner = GetOwnerIndex(i);
										if (megaEvolution[side][owner] == i) {
											megaEvolution[side][owner] = -1;
										}
										break;
									}
									if (!RegisterMove(i, index)) {
										continue;
									}
									if (doublebattle) {
										BattleMove thisMove = battlers[i].moves[index];
										int target = battlers[i].Target(thisMove);
										if (target == Targets.SingleNonUser) {
											target = scene.ChooseTarget(i, target);
											if (target < 0) {
												continue;
											}
											RegisterTarget(i, target);
										} else if (target == Targets.UserOrPartner) {
											target = scene.ChooseTarget(i, target);
											if (target < 0 || (target&1) == 1) {
												continue;
											}
											RegisterTarget(i, target);
										}
									}
									commandDone = true;
								}
							} else {
								AutoChooseMove(i);
								commandDone = true;
							}
						} else if (cmd != 0 && battlers[i].effects[Effects.SkyDrop] != 0) {
							Display(string.Format("Sky Drop won't let {0} go!", battlers[i].String(true)));
						} else if (cmd == 1) {
							if (!internalbattle) {
								if (OwnedByPlayer(i)) {
									Display(string.Format("Items can't be used here."));
								}
							} else {
								int[] item = ItemMenu(i);
								if (item[0] > 0) {
									if (RegisterItem(i, item[0], item[1])) {
										commandDone = true;
									}
								}
							}
						} else if (cmd == 2) {
							int pkmn = SwitchPlayer(i, false, true);
							if (pkmn >= 0) {
								if (RegisterSwitch(i, pkmn)) {
									commandDone = true;
								}
							}
						} else if (cmd == 3) {
							int run = Run(i);
							if (run > 0) {
								commandDone = true;
								return;
							} else if (run < 0) {
								commandDone = true;
								int side = IsOpposing(i) ? 1 : 0;
								int owner = GetOwnerIndex(i);
								if (megaEvolution[side][owner] == i) {
									megaEvolution[side][owner] = -1;
								}
							}
						} else if (cmd == 4) {
							Battler thisPokemon = battlers[i];
							useMoveChoice[i] = 4;
							indexChoice[i] = 0;
							moveChoice[i] = null;
							int side = IsOpposing(i) ? 1 : 0;
							int owner = GetOwnerIndex(i);
							if (megaEvolution[side][owner] == i) {
								megaEvolution[side][owner] = -1;
							}
							commandDone = true;
						} else if (cmd == -1) {
							if (megaEvolution[0][0] >= 0) {
								megaEvolution[0][0] = -1;
							}
							if (megaEvolution[1][0] >= 0) {
								megaEvolution[1][0] = -1;
							}
							if (useMoveChoice[0] == 3 && PokemonGlobal.Bag.CanStore(indexChoice[0])) {
								PokemonGlobal.Bag.StoreItem(indexChoice[0]);
							}
							CommandPhase();
							return;
						}
						if (commandDone) {
							break;
						}
					}
				}
			}
		}
	}

	/****************
	* Attack Phase. *
	****************/
	public void AttackPhase() {
		scene.BeginAttackPhase();
		for (int i=0; i<4; i++) {
			successStates[i].Clear();
			if (useMoveChoice[i] != 1 && useMoveChoice[i] != 2) {
				battlers[i].effects[Effects.DestinyBond] = 0;
				battlers[i].effects[Effects.Grudge] = 0;
			}
			if (!battlers[i].Fainted()) {
				battlers[i].turnCount++;
			}
			if (!ChoseMove(i, Moves.RAGE)) {
				battlers[i].effects[Effects.Rage] = 0;
			}
		}
		usePriority = false;
		Battler[] priority = Priority(false, true);
		List<int> megaEvolved = new List<int>();
		for (int i=0; i<megaEvolved.Count; i++) {
			if (useMoveChoice[priority[i].index] == 1 && priority[i].effects[Effects.SkipTurn] == 0) {
				int side = IsOpposing(priority[i].index) ? 1 : 0;
				int owner = GetOwnerIndex(priority[i].index);
				if (megaEvolution[side][owner] == priority[i].index) {
					MegaEvolve(priority[i].index);
					megaEvolved.Add(priority[i].index);
				}
			}
		}
		if (megaEvolved.Count > 0) {
			for (int i=0; i<priority.Length; i++) {
				if (megaEvolved.Contains(priority[i].index)) {
					priority[i].AbilitiesOnSwitchIn(true);
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (useMoveChoice[priority[i].index] == 4 && priority[i].effects[Effects.SkipTurn] == 0) {
				Call(priority[i].index);
			}
		}
		switching = true;
		List<int> switched = new List<int>();
		for (int i=0; i<priority.Length; i++) {
			if (useMoveChoice[priority[i].index] == 2 && priority[i].effects[Effects.SkipTurn] == 0) {
				int index = indexChoice[priority[i].index];
				int newPokeName = index;
				if (Party(priority[i].index)[index].pokemon.Ability() == Abilities.ILLUSION) {
					newPokeName = GetLastPokemonInTeam(priority[i].index);
				}
				lastMoveUser = priority[i].index;
				if (OwnedByPlayer(priority[i].index)) {
					BattleTrainer owner = GetOwner(priority[i].index);
					DisplayBrief(string.Format("{0} withdrew {1}!",owner.FullName(), priority[i].name));
					Debug.Log(string.Format("[Withdrew Pokémon] Opponent withdrew {0}",priority[i].String(true)));
				} else {
					DisplayBrief(string.Format("{0}, that's enough! Come back!",priority[i].name));
					Debug.Log(string.Format("[Withdrew Pokémon] Player withdrew {0}",priority[i].String(true)));
				}
				for (int j=0; j<priority.Length; j++) {
					if (!priority[i].IsOpposing(priority[j].index)) {
						continue;
					}
					if (ChoseMoveFunctionCode(priority[j].index, 0x88) && !priority[j].HasMovedThisRound()) {
						if (priority[j].status != Statuses.SLEEP && priority[j].status != Statuses.FROZEN && priority[j].effects[Effects.SkyDrop] == 0 && (!priority[j].HasWorkingAbility(Abilities.TRUANT) || priority[j].effects[Effects.Truant] == 0)) {
							targetChoice[priority[j].index] = priority[i].index;
							priority[j].UseMove(useMoveChoice[priority[j].index], indexChoice[priority[j].index], moveChoice[priority[j].index], targetChoice[priority[j].index]);
							priority[j].effects[Effects.Pursuit] = 1;
							switching = false;
							if (decision > 0) {
								return;
							}
						}
					}
					if (priority[i].Fainted()) {
						break;
					}
				}
				if (!RecallAndReplace(priority[i].index, index, newPokeName)) {
					if (!doublebattle) {
						switching = false;
						return;
					}
				} else {
					switched.Add(priority[i].index);
				}
			}
		}
		if (switched.Count > 0) {
			for (int i=0; i<priority.Length; i++) {
				if (switched.Contains(priority[i].index)) {
					priority[i].AbilitiesOnSwitchIn(true);
				}
			}
		}
		switching = false;
		for (int i=0; i<priority.Length; i++) {
			if (useMoveChoice[priority[i].index] == 3 && priority[i].effects[Effects.SkipTurn] == 0) {
				if (IsOpposing(priority[i].index)) {
					EnemyUseItem(indexChoice[priority[i].index], priority[i]);
				} else {
					int item = indexChoice[priority[i].index];
					if (item > 0) {
						int usetype = Items.ItemData[item][Items.ITEMBATTLEUSE];
						if (usetype == 1 || usetype == 3) {
							if (itemChoice[priority[i].index] >= 0) {
								UseItemOnPokemon(item, itemChoice[priority[i].index], priority[i], scene);
							}
						} else if (usetype == 2 || usetype == 4) {
							if (!Items.HasUseInBattle(item)) {
								UseItemOnBattler(item, itemChoice[priority[i].index], priority[i], scene);
							}
						}
					}
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].effects[Effects.SkipTurn] != 0) {
				continue;
			}
			if (ChoseMoveFunctionCode(priority[i].index, 0x115)) {
				CommonAnimation("FocusPunch", priority[i], null);
				Display(string.Format("{0} is tightening its focus!",priority[i].String()));
			}
		}
		for (int c=0; c<10; c++) {
			bool advance = false;
			for (int i=0; i<priority.Length; i++) {
				if (priority[i].effects[Effects.MoveNext] == 0) {
					continue;
				}
				if (priority[i].HasMovedThisRound() || priority[i].effects[Effects.SkipTurn] != 0) {
					continue;
				}
				advance = priority[i].ProcessTurn(useMoveChoice[priority[i].index], indexChoice[priority[i].index], moveChoice[priority[i].index], targetChoice[priority[i].index]);
				if (advance) {
					break;
				}
			}
			if (decision > 0) {
				return;
			}
			if (advance) {
				continue;
			}
			for (int i=0; i<priority.Length; i++) {
				if (priority[i].effects[Effects.Quash] != 0) {
					continue;
				}
				if (priority[i].HasMovedThisRound() || priority[i].effects[Effects.SkipTurn] != 0) {
					continue;
				}
				advance = priority[i].ProcessTurn(useMoveChoice[priority[i].index], indexChoice[priority[i].index], moveChoice[priority[i].index], targetChoice[priority[i].index]);
				if (advance) {
					break;
				}
			}
			if (decision > 0) {
				return;
			}
			if (advance) {
				continue;
			}
			for (int i=0; i<priority.Length; i++) {
				if (priority[i].effects[Effects.Quash] == 0) {
					continue;
				}
				if (priority[i].HasMovedThisRound() || priority[i].effects[Effects.SkipTurn] != 0) {
					continue;
				}
				advance = priority[i].ProcessTurn(useMoveChoice[priority[i].index], indexChoice[priority[i].index], moveChoice[priority[i].index], targetChoice[priority[i].index]);
				if (advance) {
					break;
				}
			}
			if (decision > 0) {
				return;
			}
			if (advance) {
				continue;
			}
			for (int i=0; i<priority.Length; i++) {
				if (useMoveChoice[priority[i].index] == 1 && !priority[i].HasMovedThisRound() && priority[i].effects[Effects.SkipTurn] == 0) {
					advance = true;
				}
				if (advance) {
					break;
				}
			}
			if (advance) {
				continue;
			}
			break;
		}
		for (int i=0; i<10; i++) {
			scene.GraphicsUpdate();
			scene.InputUpdate();
			scene.FrameUpdate();
		}
	}

	/****************
	* End of Round. *
	****************/
	public void EndOfRoundPhase() {
		Debug.Log(string.Format("[End of Round]"));
		for (int i=0; i<4; i++) {
			battlers[i].effects[Effects.Electrify] = 0;
			battlers[i].effects[Effects.Endure] = 0;
			battlers[i].effects[Effects.FirstPledge] = 0;
			if (battlers[i].effects[Effects.HyperBeam] > 0) {
				battlers[i].effects[Effects.HyperBeam] = 0;
			}
			battlers[i].effects[Effects.KingsShield] = 0;
			battlers[i].effects[Effects.LifeOrb] = 0;
			battlers[i].effects[Effects.MoveNext] = 0;
			battlers[i].effects[Effects.Powder] = 0;
			battlers[i].effects[Effects.Protect] = 0;
			battlers[i].effects[Effects.ProtectNegation] = 0;
			battlers[i].effects[Effects.Quash] = 0;
			battlers[i].effects[Effects.Roost] = 0;
			battlers[i].effects[Effects.SpikyShield] = 0;
		}
		usePriority = false;
		Battler[] priority = Priority(true);
		bool hasAbil = false;
		switch (weather) {
			case Weather.SUNNYDAY:
				if (weatherduration > 0) {
					weatherduration--;
				}
				if (weatherduration == 0) {
					Display(string.Format("The sunlight faded."));
					weather = 0;
					Debug.Log(string.Format("[End of effect] Sunlight weather ended"));
				} else {
					CommonAnimation("Sunny", null, null);
					if (GetWeather() == Weather.SUNNYDAY) {
						for (int i=0; i<priority.Length; i++) {
							if (priority[i].HasWorkingAbility(Abilities.SOLARPOWER)) {
								Debug.Log(string.Format("[Ability triggered] {0}'s Solar Power",priority[i].String()));
								scene.DamageAnimation(priority[i], 0);
								priority[i].ReduceHP(priority[i].totalHP/8);
								Display(string.Format("{0} was hurt by the sunlight!",priority[i].String()));
								if (priority[i].Fainted()) {
									if (!priority[i].Faint()) {
										return;
									}
								}
							}
						}
					}
				}
				break;
			case Weather.RAINDANCE:
				if (weatherduration > 0) {
					weatherduration--;
				}
				if (weatherduration == 0) {
					Display(string.Format("The rain stopped."));
					weather = 0;
					Debug.Log(string.Format("[End of effect] Rain weather ended"));
				} else {
					CommonAnimation("Rain", null, null);
				}
				break;
			case Weather.SANDSTORM:
				if (weatherduration > 0) {
					weatherduration--;
				}
				if (weatherduration == 0) {
					Display(string.Format("The sandstorm subsided."));
					weather = 0;
					Debug.Log(string.Format("[End of effect] Sandstorm weather ended"));
				} else {
					CommonAnimation("Sandstorm", null, null);
					if (GetWeather() == Weather.SANDSTORM) {
						Debug.Log(string.Format("[Lingering effect triggered] Sandstorm weather damage"));
						for (int i=0; i<priority.Length; i++) {
							if (priority[i].Fainted()) {
								continue;
							}
							if (!priority[i].HasType(Types.GROUND) && !priority[i].HasType(Types.ROCK) && !priority[i].HasType(Types.STEEL) && !priority[i].HasWorkingAbility(Abilities.SANDVEIL) && !priority[i].HasWorkingAbility(Abilities.SANDRUSH) && !priority[i].HasWorkingAbility(Abilities.SANDFORCE) && !priority[i].HasWorkingAbility(Abilities.MAGICGUARD) && !priority[i].HasWorkingAbility(Abilities.OVERCOAT) && !priority[i].HasWorkingItem(Items.SAFETYGOGGLES) && (new Moves.Move(priority[i].effects[Effects.TwoTurnAttack]).Function()) != 0xCA && (new Moves.Move(priority[i].effects[Effects.TwoTurnAttack]).Function()) != 0xCB) {				
								scene.DamageAnimation(priority[i], 0);
								priority[i].ReduceHP(priority[i].totalHP/16);
								Display(string.Format("{0} is buffeted by the sandstorm!",priority[i].String()));
								if (priority[i].Fainted()) {
									if (!priority[i].Faint()) {
										return;
									}
								}
							}
						}
					}
				}
				break;
			case Weather.HAIL:
				if (weatherduration > 0) {
					weatherduration--;
				}
				if (weatherduration == 0) {
					Display(string.Format("The hail stopped."));
					weather = 0;
					Debug.Log(string.Format("[End of effect] Hail weather ended"));
				} else {
					CommonAnimation("Hail", null, null);
					if (GetWeather() == Weather.HAIL) {
						Debug.Log(string.Format("[Lingering effect triggered] Hail weather damage"));
						for (int i=0; i<priority.Length; i++) {
							if (priority[i].Fainted()) {
								continue;
							}
							if (!priority[i].HasType(Types.ICE) && !priority[i].HasWorkingAbility(Abilities.ICEBODY) && !priority[i].HasWorkingAbility(Abilities.SNOWCLOAK) && !priority[i].HasWorkingAbility(Abilities.MAGICGUARD) && !priority[i].HasWorkingAbility(Abilities.OVERCOAT) && !priority[i].HasWorkingItem(Items.SAFETYGOGGLES) && (new Moves.Move(priority[i].effects[Effects.TwoTurnAttack]).Function()) != 0xCA && (new Moves.Move(priority[i].effects[Effects.TwoTurnAttack]).Function()) != 0xCB) {				
								scene.DamageAnimation(priority[i], 0);
								priority[i].ReduceHP(priority[i].totalHP/16);
								Display(string.Format("{0} is buffeted by the hail!",priority[i].String()));
								if (priority[i].Fainted()) {
									if (!priority[i].Faint()) {
										return;
									}
								}
							}
						}
					}
				}
				break;
			case Weather.HEAVYRAIN:
				for (int i=0; i<4; i++) {
					if (battlers[i].ability == Abilities.PRIMORDIALSEA && !battlers[i].Fainted()) {
						hasAbil = true;
						break;
					}
				}
				if (!hasAbil) {
					weatherduration = 0;
				}
				if (weatherduration == 0) {
					Display(string.Format("The heavy rain stopped."));
					weather = 0;
					Debug.Log(string.Format("[End of effect] Primordial Sea's rain weather ended"));
				} else {
					CommonAnimation("HeavyRain", null, null);
				}
				break;
			case Weather.HARSHSUN:
				for (int i=0; i<4; i++) {
					if (battlers[i].ability == Abilities.DESOLATELAND && !battlers[i].Fainted()) {
						hasAbil = true;
						break;
					}
				}
				if (!hasAbil) {
					weatherduration = 0;
				}
				if (weatherduration == 0) {
					Display(string.Format("The harsh sunlight faded."));
					weather = 0;
					Debug.Log(string.Format("[End of effect] Desolate Land's sunlight weather ended"));
				} else {
					CommonAnimation("HarshSun", null, null);
					if (GetWeather() == Weather.HARSHSUN) {
						for (int i=0; i<priority.Length; i++) {
							if (priority[i].HasWorkingAbility(Abilities.SOLARPOWER)) {
								Debug.Log(string.Format("[Ability triggered] {0}'s Solar Power",priority[i].String()));
								scene.DamageAnimation(priority[i], 0);
								priority[i].ReduceHP(priority[i].totalHP/8);
								Display(string.Format("{0} was hurt by the sunlight!",priority[i].String()));
								if (priority[i].Fainted()) {
									if (!priority[i].Faint()) {
										return;
									}
								}
							}
						}
					}
				}
				break;
			case Weather.STRONGWINDS:
				for (int i=0; i<4; i++) {
					if (battlers[i].ability == Abilities.DELTASTREAM && !battlers[i].Fainted()) {
						hasAbil = true;
						break;
					}
				}
				if (!hasAbil) {
					weatherduration = 0;
				}
				if (weatherduration == 0) {
					Display(string.Format("The air current subsided."));
					weather = 0;
					Debug.Log(string.Format("[End of effect] Delta Stream's wind weather ended"));
				} else {
					CommonAnimation("StrongWinds", null, null);
				}
				break;
		}
		for (int i=0; i<battlers.Count; i++) {
			if (battlers[i].Fainted()) {
				continue;
			}
			if (battlers[i].effects[Effects.FutureSight] > 0) {
				battlers[i].effects[Effects.FutureSight]--;
				if (battlers[i].effects[Effects.FutureSight] == 0) {
					int move = battlers[i].effects[Effects.FutureSightMove];
					Debug.Log(string.Format("[Lingering effect triggered] {0} struck {1}",Moves.GetName(move), battlers[i].String(true)));
					Display(string.Format("{0} took the {1} attack!",battlers[i].String(), Moves.GetName(move)));
					Battler moveUser = null;
					for (int j=0; j<battlers.Count; j++) {
						if (battlers[j].IsOpposing(battlers[i].effects[Effects.FutureSightUserPos])) {
							continue;
						}
						if (battlers[j].pokemonIndex == battlers[i].effects[Effects.FutureSightUser] && !battlers[j].Fainted()) {
							moveUser = battlers[j];
							break;
						}
					}
					if (moveUser == null) {
						Battler[] party = Party(battlers[i].effects[Effects.FutureSightUserPos]);
						if (party[battlers[i].effects[Effects.FutureSightUser]].hp > 0) {
							moveUser = new Battler(this, battlers[i].effects[Effects.FutureSightUserPos]);
							moveUser.InitDummyPokemon(party[battlers[i].effects[Effects.FutureSightUser]].pokemon, battlers[i].effects[Effects.FutureSightUser]);
						}
					}
					if (moveUser == null) {
						Display(string.Format("But it failed!"));
					} else {
						futuresight = true;
						moveUser.UseMoveSimple(move, -1, battlers[i].index);
						futuresight = false;
					}
					battlers[i].effects[Effects.FutureSight] = 0;
					battlers[i].effects[Effects.FutureSightMove] = 0;
					battlers[i].effects[Effects.FutureSightUser] = -1;
					battlers[i].effects[Effects.FutureSightUserPos] = -1;
					if (battlers[i].Fainted()) {
						if (!battlers[i].Faint()) {
							return;
						}
						continue;
					}
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (battlers[i].HasWorkingAbility(Abilities.RAINDISH) && battlers[i].effects[Effects.HealBlock] == 0 && (GetWeather() == Weather.RAINDANCE || GetWeather() == Weather.HEAVYRAIN)) {
				Debug.Log(string.Format("[Ability triggered] {0}'s Rain Dish",battlers[i].String()));
				int hpgain = battlers[i].RecoverHP(battlers[i].totalHP/16, true);
				if (hpgain > 0) {
					Display(string.Format("{0}'s {1} restored its HP a little!",battlers[i].String(), Abilities.GetName(battlers[i].ability)));
				}
			}
			if (battlers[i].HasWorkingAbility(Abilities.DRYSKIN)) {
				if ((GetWeather() == Weather.RAINDANCE || GetWeather() == Weather.HEAVYRAIN) && battlers[i].effects[Effects.HealBlock] == 0) {
					Debug.Log(string.Format("[Ability triggered] {0}'s Dry Skin (in rain)",battlers[i].String()));
					int hpgain = battlers[i].RecoverHP(battlers[i].totalHP/8, true);
					if (hpgain > 0) {
						Display(string.Format("{0}'s {1} was healed by the rain!",battlers[i].String(), Abilities.GetName(battlers[i].ability)));
					}
				} else if (GetWeather() == Weather.SUNNYDAY || GetWeather() == Weather.HARSHSUN) {
					Debug.Log(string.Format("[Ability triggered] {0}'s Dry Skin (in sun)",battlers[i].String()));
					int hploss = battlers[i].ReduceHP(battlers[i].totalHP/8);
					if (hploss > 0) {
						Display(string.Format("{0}'s {1} was hurt by the sunlight!",battlers[i].String(), Abilities.GetName(battlers[i].ability)));
					}
				}
			}
			if (battlers[i].HasWorkingAbility(Abilities.ICEBODY) && battlers[i].effects[Effects.HealBlock] == 0 && GetWeather() == Weather.HAIL) {
				Debug.Log(string.Format("[Ability triggered] {0}'s Ice Body",battlers[i].String()));
				int hpgain = battlers[i].RecoverHP(battlers[i].totalHP/16, true);
				if (hpgain > 0) {
					Display(string.Format("{0}'s {1} restored its HP a little!",battlers[i].String(), Abilities.GetName(battlers[i].ability)));
				}
			}
			if (battlers[i].Fainted()) {
				if (!battlers[i].Faint()) {
					return;
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.Wish] > 0) {
				priority[i].effects[Effects.Wish]--;
				if (priority[i].effects[Effects.Wish] == 0) {
					Debug.Log(string.Format("[Lingering effect triggered] {0}'s Wish",priority[i].String()));
					int hpgain = priority[i].RecoverHP(priority[i].effects[Effects.WishAmount], true);
					if (hpgain > 0) {
						string wishmaker = StringEx(priority[i].index, priority[i].effects[Effects.WishMaker]);
						Display(string.Format("{0}'s wish came true!",wishmaker));
					}
				}
			}
		}
		for (int i=0; i<2; i++) {
			if (sides[i].effects[Effects.SeaOfFire] > 0 && GetWeather() != Weather.RAINDANCE && GetWeather() != Weather.HEAVYRAIN) {
				if (i == 0) {
					CommonAnimation("SeaOfFire", null, null);
				}
				if (i == 1) {
					CommonAnimation("SeaOfFireOpp", null, null);
				}
				for (int j=0; j<priority.Length; i++) {
					if ((priority[j].index&1) != i) {
						continue;
					}
					if (priority[j].HasType(Types.FIRE) || priority[j].HasWorkingAbility(Abilities.MAGICGUARD)) {
						scene.DamageAnimation(priority[j], 0);
						int hploss = priority[j].ReduceHP(priority[j].totalHP/8);
						if (hploss > 0) {
							Display(string.Format("{0} is hurt by the sea of fire!",priority[j].String()));
						}
						if (priority[j].Fainted()) {
							if (!priority[j].Faint()) {
								return;
							}
						}
					}
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if ((priority[i].HasWorkingAbility(Abilities.SHEDSKIN) && Rand(10) < 3) || (priority[i].HasWorkingAbility(Abilities.HYDRATION) && (GetWeather()==Weather.RAINDANCE || GetWeather()==Weather.HEAVYRAIN))) {
				if (priority[i].status > 0) {
					Debug.Log(string.Format("[Ability triggered] {0}'s {1}", priority[i].String(), Abilities.GetName(priority[i].ability)));
					int s = priority[i].status;
					priority[i].CureStatus(false);
					switch (s) {
						case Statuses.SLEEP:
							Display(string.Format("{0}'s {1} cured its sleep problem!",priority[i].String(), Abilities.GetName(priority[i].ability)));
							break;
						case Statuses.POISON:
							Display(string.Format("{0}'s {1} cured its poison problem!",priority[i].String(), Abilities.GetName(priority[i].ability)));
							break;
						case Statuses.BURN:
							Display(string.Format("{0}'s {1} healed its burn!",priority[i].String(), Abilities.GetName(priority[i].ability)));
							break;
						case Statuses.PARALYSIS:
							Display(string.Format("{0}'s {1} cured its paralysis!",priority[i].String(), Abilities.GetName(priority[i].ability)));
							break;
						case Statuses.FROZEN:
							Display(string.Format("{0}'s {1} thawed it out!",priority[i].String(), Abilities.GetName(priority[i].ability)));
							break;
					}
				}
			}
			if (priority[i].HasWorkingAbility(Abilities.HEALER) && Rand(10) < 3) {
				Battler partner = priority[i].Partner();
				if (partner != null && partner.status > 0) {
					Debug.Log(string.Format("[Ability triggered] {0}'s {1}",priority[i].String(), Abilities.GetName(priority[i].ability)));
					int s = partner.status;
					partner.CureStatus(false);
					switch (s) {
						case Statuses.SLEEP:
							Display(string.Format("{0}'s {1} cured its partner's sleep problem!",priority[i].String(), Abilities.GetName(priority[i].ability)));
							break;
						case Statuses.POISON:
							Display(string.Format("{0}'s {1} cured its partner's poison problem!",priority[i].String(), Abilities.GetName(priority[i].ability)));
							break;
						case Statuses.BURN:
							Display(string.Format("{0}'s {1} healed its partner's burn!",priority[i].String(), Abilities.GetName(priority[i].ability)));
							break;
						case Statuses.PARALYSIS:
							Display(string.Format("{0}'s {1} cured its partner's paralysis!",priority[i].String(), Abilities.GetName(priority[i].ability)));
							break;
						case Statuses.FROZEN:
							Display(string.Format("{0}'s {1} thawed its partner out!",priority[i].String(), Abilities.GetName(priority[i].ability)));
							break;
					}
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (field.effects[Effects.GrassyTerrain] > 0 && !priority[i].IsAirborne()) {
				if (priority[i].effects[Effects.HealBlock] == 0) {
					int hpgain = priority[i].RecoverHP(priority[i].totalHP/16, true);
					if (hpgain > 0) {
						Display(string.Format("{0}'s HP was restored.",priority[i].String()));
					}
				}
				priority[i].BerryCureCheck(true);
				if (priority[i].Fainted()) {
					if (!priority[i].Faint()) {
						return;
					}
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.AquaRing] != 0) {
				Debug.Log(string.Format("[Lingering effect triggered] {0}'s Aqua Ring",priority[i].String()));
				int hpgain = priority[i].totalHP/16;
				if (priority[i].HasWorkingItem(Items.BIGROOT)) {
					hpgain = (int)(hpgain*1.3);
				}
				hpgain = priority[i].RecoverHP(hpgain, true);
				if (hpgain > 0) {
					Display(string.Format("Aqua Ring resored {0}'s HP!",priority[i].String(true)));
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.Ingrain] != 0) {
				Debug.Log(string.Format("[Lingering effect triggered] {0}'s Ingrain",priority[i].String()));
				int hpgain = priority[i].totalHP/16;
				if (priority[i].HasWorkingItem(Items.BIGROOT)) {
					hpgain = (int)(hpgain*1.3);
				}
				hpgain = priority[i].RecoverHP(hpgain, true);
				if (hpgain > 0) {
					Display(string.Format("{0} absorbed nutrients with its roots!",priority[i].String()));
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.LeechSeed] != 0) {
				Battler recipient = battlers[priority[i].effects[Effects.LeechSeed]];
				if (recipient != null && !recipient.Fainted()) {
					Debug.Log(string.Format("[Lingering effect triggered] {0}'s Leech Seed",priority[i].String()));
					CommonAnimation("LeechSeed", recipient, priority[i]);
					int hploss = priority[i].ReduceHP(priority[i].totalHP/8, true);
					if (priority[i].HasWorkingAbility(Abilities.LIQUIDOOZE)) {
						recipient.ReduceHP(hploss, true);
						Display(string.Format("{0} sucked up the liquid ooze!",recipient.String()));
					} else {
						if (recipient.effects[Effects.HealBlock] == 0) {
							if (priority[i].HasWorkingItem(Items.BIGROOT)) {
								hploss = (int)(hploss*1.3);
							}
							hploss = recipient.RecoverHP(hploss, true);
						}
						Display(string.Format("{0}'s health was sapped by the Leech Seed!",priority[i].String()));
					}
				}
				if (priority[i].Fainted()) {
					if (!priority[i].Faint()) {
						return;
					}
				}
				if (recipient.Fainted()) {
					if (recipient.Faint()) {
						return;
					}
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].status == Statuses.POISON) {
				if (priority[i].statusCount > 0) {
					priority[i].effects[Effects.Toxic] += 1;
					priority[i].effects[Effects.Toxic] = Math.Min(priority[i].effects[Effects.Toxic], 15);
				}
				if (priority[i].HasWorkingAbility(Abilities.POISONHEAL)) {
					CommonAnimation("Poison", priority[i], null);
					if (priority[i].effects[Effects.HealBlock] == 0 && priority[i].hp < priority[i].totalHP) {
						Debug.Log(string.Format("[Ability triggered] {0}'s Poison Heal",priority[i].String()));
						priority[i].RecoverHP(priority[i].totalHP/8, true);
						Display(string.Format("{0} is healed by poison!",priority[i].String()));
					}
				} else {
					if (!priority[i].HasWorkingAbility(Abilities.MAGICGUARD)) {
						Debug.Log(string.Format("[Status damage] {0} took damage from poison/toxic",priority[i].String()));
						if (priority[i].statusCount == 0) {
							priority[i].ReduceHP(priority[i].totalHP/8);
						} else {
							priority[i].ReduceHP(priority[i].totalHP*priority[i].effects[Effects.Toxic]/16);
						}
						priority[i].ContinueStatus();
					}
				}
			}
			if (priority[i].status == Statuses.BURN) {
				if (!priority[i].HasWorkingAbility(Abilities.MAGICGUARD)) {
					Debug.Log(string.Format("[Status damage] {0} took damage from poison/toxic",priority[i].String()));
					if (priority[i].HasWorkingAbility(Abilities.HEATPROOF)) {
						Debug.Log(string.Format("[Ability Triggered] {0}'s Heatproof",priority[i].String()));
						priority[i].ReduceHP(priority[i].totalHP/16);
					} else {
						priority[i].ReduceHP(priority[i].totalHP/8);
					}
				}
				priority[i].ContinueStatus();
			}
			if (priority[i].effects[Effects.Nightmare] != 0) {
				if (priority[i].status == Statuses.SLEEP) {
					if (!priority[i].HasWorkingAbility(Abilities.MAGICGUARD)) {
						Debug.Log(string.Format("[Lingering effect triggered] {0}'s nightmare",priority[i].String()));
						priority[i].ReduceHP(priority[i].totalHP/4, true);
						Display(string.Format("{0} is locked in a nightmare!",priority[i].String()));
					}
				} else {
					priority[i].effects[Effects.Nightmare] = 0;
				}
			}
			if (priority[i].Fainted()) {
				if (!priority[i].Faint()) {
					return;
				}
				continue;
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.Curse] != 0 && !priority[i].HasWorkingAbility(Abilities.MAGICGUARD)) {
				Debug.Log(string.Format("[Lingering effect triggered] {0}'s curse",priority[i].String()));
				priority[i].ReduceHP(priority[i].totalHP/4, true);
				Display(string.Format("{0} is afflicted by the curse!",priority[i].String()));
			}
			if (priority[i].Fainted()) {
				if (!priority[i].Faint()) {
					return;
				}
				continue;
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.MultiTurn] > 0) {
				priority[i].effects[Effects.MultiTurn]--;
				string moveName = Moves.GetName(priority[i].effects[Effects.MultiTurnAttack]);
				if (priority[i].effects[Effects.MultiTurn] == 0) {
					Debug.Log(string.Format("[End of effect] Trapping move {0} affecting {1} ended",moveName, priority[i].String()));
					Display(string.Format("{0} was freed from {1}!",priority[i].String(), moveName));
				} else {
					if (priority[i].effects[Effects.MultiTurnAttack] == Moves.BIND) {
						CommonAnimation("Bind", priority[i], null);
					} else if (priority[i].effects[Effects.MultiTurnAttack] == Moves.CLAMP) {
						CommonAnimation("Clamp", priority[i], null);
					} else if (priority[i].effects[Effects.MultiTurnAttack] == Moves.FIRESPIN) {
						CommonAnimation("FireSpin", priority[i], null);
					} else if (priority[i].effects[Effects.MultiTurnAttack] == Moves.MAGMASTORM) {
						CommonAnimation("MagmaStorm", priority[i], null);
					} else if (priority[i].effects[Effects.MultiTurnAttack] == Moves.SANDTOMB) {
						CommonAnimation("SandTomb", priority[i], null);
					} else if (priority[i].effects[Effects.MultiTurnAttack] == Moves.WRAP) {
						CommonAnimation("Wrap", priority[i], null);
					} else if (priority[i].effects[Effects.MultiTurnAttack] == Moves.INFESTATION) {
						CommonAnimation("Infestation", priority[i], null);
					} else {
						CommonAnimation("Wrap", priority[i], null);
					}
					if (!priority[i].HasWorkingAbility(Abilities.MAGICGUARD)) {
						Debug.Log(string.Format("[Lingering effect triggered] {0} took damage from trapping move {1}",priority[i].String(), moveName));
						scene.DamageAnimation(priority[i], 0);
						int amt = Settings.USE_NEW_BATTLE_MECHANICS ? (priority[i].totalHP/8) : (priority[i].totalHP/16);
						if (battlers[priority[i].effects[Effects.MultiTurnUser]].HasWorkingItem(Items.BINDINGBAND)) {
							amt = Settings.USE_NEW_BATTLE_MECHANICS ? (priority[i].totalHP/6) : (priority[i].totalHP/8);
						}
						priority[i].ReduceHP(amt);
						Display(string.Format("{0} is hurt by {1}!",priority[i].String(), moveName));
					}
				}
			}
			if (priority[i].Fainted()) {
				if (!priority[i].Faint()) {
					return;
				}
				continue;
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.Taunt] > 0) {
				priority[i].effects[Effects.Taunt]--;
				if (priority[i].effects[Effects.Taunt] == 0) {
					Display(string.Format("{0}'s taunt wore off!",priority[i].String()));
					Debug.Log(string.Format("[End of effect] {0} is no longer taunted",priority[i].String()));
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.Encore] > 0) {
				if (priority[i].moves[priority[i].effects[Effects.EncoreIndex]].id != priority[i].effects[Effects.EncoreMove]) {
					priority[i].effects[Effects.Encore] = 0;
					priority[i].effects[Effects.EncoreIndex] = 0;
					priority[i].effects[Effects.EncoreMove] = 0;
					Debug.Log(string.Format("[End of effect] {0} is no longer encored (encored move was lost)",priority[i].String()));
				} else {
					priority[i].effects[Effects.Encore]--;
					if (priority[i].effects[Effects.Encore] == 0 || priority[i].moves[priority[i].effects[Effects.EncoreIndex]].pp == 0) {
						priority[i].effects[Effects.Encore] = 0;
						Display(string.Format("{0}'s encore ended!",priority[i].String()));
						Debug.Log(string.Format("[End of effect] {0} is no longer disabled",priority[i].String()));
					}
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.Disable] > 0) {
				priority[i].effects[Effects.Disable]--;
				if (priority[i].effects[Effects.Disable] == 0) {
					priority[i].effects[Effects.DisableMove] = 0;
					Display(string.Format("{0}'s is no longer disabled!",priority[i].String()));
					Debug.Log(string.Format("[End of effect] {0} is no longer disabled",priority[i].String()));
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.MagnetRise] > 0) {
				priority[i].effects[Effects.MagnetRise]--;
				if (priority[i].effects[Effects.MagnetRise] == 0) {
					Display(string.Format("{0}'s stopped levitating!",priority[i].String()));
					Debug.Log(string.Format("[End of effect] {0} is no longer levitating by magnet rise",priority[i].String()));
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.Telekinesis] > 0) {
				priority[i].effects[Effects.Telekinesis]--;
				if (priority[i].effects[Effects.Telekinesis] == 0) {
					Display(string.Format("{0}'s stopped levitating!",priority[i].String()));
					Debug.Log(string.Format("[End of effect] {0} is no longer levitating by Telekinesis",priority[i].String()));
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.HealBlock] > 0) {
				priority[i].effects[Effects.HealBlock]--;
				if (priority[i].effects[Effects.HealBlock] == 0) {
					Display(string.Format("{0}'s Heal Block wore off!",priority[i].String()));
					Debug.Log(string.Format("[End of effect] {0} is no longer Heal Blocked",priority[i].String()));
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.Embargo] > 0) {
				priority[i].effects[Effects.Embargo]--;
				if (priority[i].effects[Effects.Embargo] == 0) {
					Display(string.Format("{0}'s can use items again!",priority[i].String()));
					Debug.Log(string.Format("[End of effect] {0} is no longer affected by an embargo",priority[i].String()));
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.Yawn] > 0) {
				priority[i].effects[Effects.Yawn]--;
				if (priority[i].effects[Effects.Yawn] == 0 && priority[i].CanSleepYawn()) {
					Debug.Log(string.Format("[Lingering effect triggered] {0}'s Yawn",priority[i].String()));
					priority[i].Sleep();
					priority[i].BerryCureCheck();
				}
			}
		}
		List<int> perishSongUsers = new List<int>();
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.PerishSong] > 0) {
				priority[i].effects[Effects.PerishSong]--;
				Display(string.Format("{0}'s perish count fell to {1}!",priority[i].String(), priority[i].effects[Effects.PerishSong]));
				if (priority[i].effects[Effects.PerishSong] == 0) {
					perishSongUsers.Add(priority[i].effects[Effects.PerishSongUser]);
					priority[i].ReduceHP(priority[i].hp, true);
				}
			}
			if (priority[i].Fainted()) {
				if (!priority[i].Faint()) {
					return;
				}
			}

		}
		if (perishSongUsers.Count > 0) {
			List<int> opposingPerish = new List<int>();
			List<int> nonOpposingPerish = new List<int>();
			for (int i=0; i<perishSongUsers.Count; i++) {
				if (IsOpposing(perishSongUsers[i])) {
					opposingPerish.Add(perishSongUsers[i]);
				}
				if (!IsOpposing(perishSongUsers[i])) {
					nonOpposingPerish.Add(perishSongUsers[i]);
				}
			}
			if (opposingPerish.Count == perishSongUsers.Count || nonOpposingPerish.Count == perishSongUsers.Count) {
				JudgeCheckpoint(battlers[perishSongUsers[0]]);
			}
		}
		if (decision > 0) {
			GainEXP();
			return;
		}
		for (int i=0; i<2; i++) {
			if (sides[i].effects[Effects.Reflect] > 0) {
				sides[i].effects[Effects.Reflect]--;
				if (sides[i].effects[Effects.Reflect] == 0) {
					if (i==0) {
						Display(string.Format("Your team's Reflect faded!"));
						Debug.Log(string.Format("[End of effect] Reflect ended on the player's side"));
					}
					if (i==1) {
						Display(string.Format("The opposing team's Reflect faded!"));
						Debug.Log(string.Format("[End of effect] Reflect ended on the opponent's side"));
					}
				}
			}
		}
		for (int i=0; i<2; i++) {
			if (sides[i].effects[Effects.LightScreen] > 0) {
				sides[i].effects[Effects.LightScreen]--;
				if (sides[i].effects[Effects.LightScreen] == 0) {
					if (i==0) {
						Display(string.Format("Your team's Light Screen faded!"));
						Debug.Log(string.Format("[End of effect] Light Screen ended on the player's side"));
					}
					if (i==1) {
						Display(string.Format("The opposing team's Light Screen faded!"));
						Debug.Log(string.Format("[End of effect] Light Screen ended on the opponent's side"));
					}
				}
			}
		}
		for (int i=0; i<2; i++) {
			if (sides[i].effects[Effects.Tailwind] > 0) {
				sides[i].effects[Effects.Tailwind]--;
				if (sides[i].effects[Effects.Tailwind] == 0) {
					if (i==0) {
						Display(string.Format("Your team's Tailwind faded!"));
						Debug.Log(string.Format("[End of effect] Tailwind ended on the player's side"));
					}
					if (i==1) {
						Display(string.Format("The opposing team's Tailwind faded!"));
						Debug.Log(string.Format("[End of effect] Tailwind ended on the opponent's side"));
					}
				}
			}
		}
		for (int i=0; i<2; i++) {
			if (sides[i].effects[Effects.Mist] > 0) {
				sides[i].effects[Effects.Mist]--;
				if (sides[i].effects[Effects.Mist] == 0) {
					if (i==0) {
						Display(string.Format("Your team's Mist faded!"));
						Debug.Log(string.Format("[End of effect] Mist ended on the player's side"));
					}
					if (i==1) {
						Display(string.Format("The opposing team's Mist faded!"));
						Debug.Log(string.Format("[End of effect] Mist ended on the opponent's side"));
					}
				}
			}
		}
		for (int i=0; i<2; i++) {
			if (sides[i].effects[Effects.Tailwind] > 0) {
				sides[i].effects[Effects.Tailwind]--;
				if (sides[i].effects[Effects.Tailwind] == 0) {
					if (i==0) {
						Display(string.Format("Your team's Tailwind petered out!"));
						Debug.Log(string.Format("[End of effect] Tailwind ended on the player's side"));
					}
					if (i==1) {
						Display(string.Format("The opposing team's Tailwind petered out!"));
						Debug.Log(string.Format("[End of effect] Tailwind ended on the opponent's side"));
					}
				}
			}
		}
		for (int i=0; i<2; i++) {
			if (sides[i].effects[Effects.LuckyChant] > 0) {
				sides[i].effects[Effects.LuckyChant]--;
				if (sides[i].effects[Effects.LuckyChant] == 0) {
					if (i==0) {
						Display(string.Format("Your team's Lucky Chant faded!"));
						Debug.Log(string.Format("[End of effect] Lucky Chant ended on the player's side"));
					}
					if (i==1) {
						Display(string.Format("The opposing team's Lucky Chant faded!"));
						Debug.Log(string.Format("[End of effect] Lucky Chant ended on the opponent's side"));
					}
				}
			}
		}
		for (int i=0; i<2; i++) {
			if (sides[i].effects[Effects.Swamp] > 0) {
				sides[i].effects[Effects.Swamp]--;
				if (sides[i].effects[Effects.Swamp] == 0) {
					if (i==0) {
						Display(string.Format("The swamp around your team disappeared!"));
						Debug.Log(string.Format("[End of effect] Grass Pledge's Swamp ended on the player's side"));
					}
					if (i==1) {
						Display(string.Format("The swamp around the opposing team disappeared!"));
						Debug.Log(string.Format("[End of effect] Grass Pledge's Swamp ended on the opponent's side"));
					}
				}
			}
			if (sides[i].effects[Effects.SeaOfFire] > 0) {
				sides[i].effects[Effects.SeaOfFire]--;
				if (sides[i].effects[Effects.SeaOfFire] == 0) {
					if (i==0) {
						Display(string.Format("The sea of fire around your team disappeared!"));
						Debug.Log(string.Format("[End of effect] Fire Pledge's sea of fire ended on the player's side"));
					}
					if (i==1) {
						Display(string.Format("The sea of fire around the opposing team disappeared!"));
						Debug.Log(string.Format("[End of effect] Fire Pledge's sea of fire ended on the opponent's side"));
					}
				}
			}
			if (sides[i].effects[Effects.Rainbow] > 0) {
				sides[i].effects[Effects.Rainbow]--;
				if (sides[i].effects[Effects.Rainbow] == 0) {
					if (i==0) {
						Display(string.Format("The rainbow around your team disappeared!"));
						Debug.Log(string.Format("[End of effect] Water Pledge's rainbow ended on the player's side"));
					}
					if (i==1) {
						Display(string.Format("The rainbow around the opposing team disappeared!"));
						Debug.Log(string.Format("[End of effect] Water Pledge's rainbow ended on the opponent's side"));
					}
				}
			}
		}
		if (field.effects[Effects.Gravity] > 0) {
			field.effects[Effects.Gravity]--;
			if (field.effects[Effects.Gravity] == 0) {
				Display(string.Format("Gravity returned to normal."));
				Debug.Log(string.Format("[End of effect] Strong gravity ended"));
			}
		}
		if (field.effects[Effects.TrickRoom] > 0) {
			field.effects[Effects.TrickRoom]--;
			if (field.effects[Effects.TrickRoom] == 0) {
				Display(string.Format("The twisted dimensions returned to normal."));
				Debug.Log(string.Format("[End of effect] Trick Room ended"));
			}
		}
		if (field.effects[Effects.WonderRoom] > 0) {
			field.effects[Effects.WonderRoom]--;
			if (field.effects[Effects.WonderRoom] == 0) {
				Display(string.Format("Wonder Room wore off, and the Defense and Sp. Def stats returned to normal."));
				Debug.Log(string.Format("[End of effect] Wonder Room ended"));
			}
		}
		if (field.effects[Effects.MagicRoom] > 0) {
			field.effects[Effects.MagicRoom]--;
			if (field.effects[Effects.MagicRoom] == 0) {
				Display(string.Format("The area returned to normal."));
				Debug.Log(string.Format("[End of effect] Magic Room ended"));
			}
		}
		if (field.effects[Effects.MudSportField] > 0) {
			field.effects[Effects.MudSportField]--;
			if (field.effects[Effects.MudSportField] == 0) {
				Display(string.Format("The effects of Mud Sport have faded."));
				Debug.Log(string.Format("[End of effect] Mud Sport ended"));
			}
		}
		if (field.effects[Effects.WaterSportField] > 0) {
			field.effects[Effects.WaterSportField]--;
			if (field.effects[Effects.WaterSportField] == 0) {
				Display(string.Format("The effects of Water Sport have faded."));
				Debug.Log(string.Format("[End of effect] Water Sport ended"));
			}
		}
		if (field.effects[Effects.ElectricTerrain] > 0) {
			field.effects[Effects.ElectricTerrain]--;
			if (field.effects[Effects.ElectricTerrain] == 0) {
				Display(string.Format("The electric current disappeared from the battlefield."));
				Debug.Log(string.Format("[End of effect] Electric Terrain ended"));
			}
		}
		if (field.effects[Effects.GrassyTerrain] > 0) {
			field.effects[Effects.GrassyTerrain]--;
			if (field.effects[Effects.GrassyTerrain] == 0) {
				Display(string.Format("The grass disappeared from the battlefield."));
				Debug.Log(string.Format("[End of effect] Grassy Terrain ended"));
			}
		}
		if (field.effects[Effects.MistyTerrain] > 0) {
			field.effects[Effects.MistyTerrain]--;
			if (field.effects[Effects.MistyTerrain] == 0) {
				Display(string.Format("The mist disappeared from the battlefield."));
				Debug.Log(string.Format("[End of effect] Misty Terrain ended"));
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].effects[Effects.Uproar] > 0) {
				for (int j=0; j<priority.Length; j++) {
					if (!priority[j].Fainted() && priority[j].status == Statuses.SLEEP && !priority[j].HasWorkingAbility(Abilities.SOUNDPROOF)) {
						Debug.Log(string.Format("[Lingering effect triggered] Uproar woke up {0}",priority[j].String(true)));
						priority[j].CureStatus(false);
						Display(string.Format("{0} woke up in the uproar!",priority[j].String()));
					}
				}
				priority[i].effects[Effects.Uproar]--;
				if (priority[i].effects[Effects.Uproar]==0) {
					Display(string.Format("{0} calmed down.",priority[i].String()));
					Debug.Log(string.Format("[End of effect] {0} is no longer uproaring",priority[i].String()));
				} else {
					Display(string.Format("{0} is making an uproar!",priority[i].String()));
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].turnCount > 0 && priority[i].HasWorkingAbility(Abilities.SPEEDBOOST)) {
				if (priority[i].IncreaseStatWithCause(Stats.SPEED, 1, priority[i], Abilities.GetName(priority[i].ability))) {
					Debug.Log(string.Format("[Ability Triggered] {0}'s {1}",priority[i].String(), Abilities.GetName(priority[i].ability)));
				}
			}
			if (priority[i].status == Statuses.SLEEP && !priority[i].HasWorkingAbility(Abilities.MAGICGUARD)) {
				if (priority[i].Opposing1().HasWorkingAbility(Abilities.BADDREAMS) || priority[i].Opposing2().HasWorkingAbility(Abilities.BADDREAMS)) {
					Debug.Log(string.Format("[Ability triggered] {0}'s opponent's Bad Dreams",priority[i].String()));
					int hploss = priority[i].ReduceHP(priority[i].totalHP/8, true);
					if (hploss > 0) {
						Display(string.Format("{0} is having a bad dream!",priority[i].String()));
					}
				}
			}
			if (priority[i].Fainted()) {
				if (!priority[i].Faint()) {
					return;
				}
				continue;
			}
			if (priority[i].HasWorkingAbility(Abilities.PICKUP) && priority[i].item <= 0) {
				int item = 0;
				int index = -1;
				int use = 0;
				for (int j=0; j<4; j++) {
					if (j==priority[i].index) {
						continue;
					}
					if (battlers[j].effects[Effects.PickupUse] > use) {
						item = battlers[j].effects[Effects.PickupItem];
						index = j;
						use = battlers[j].effects[Effects.PickupUse];
					}
				}
				if (item > 0) {
					priority[i].item = item;
					battlers[index].effects[Effects.PickupItem] = 0;
					battlers[index].effects[Effects.PickupUse] = 0;
					if (battlers[index].pokemon.itemRecycle == item) {
						battlers[index].pokemon.itemRecycle = 0;
					}
					if (opponent.Length == 0 && priority[i].pokemon.itemInitial == 0 && battlers[index].pokemon.itemInitial == item) {
						priority[i].pokemon.itemInitial = item;
						battlers[index].pokemon.itemInitial = 0;
					}
				}
				Display(string.Format("{0} found one {1}!",priority[i].String(), Items.GetName(item)));
				priority[i].BerryCureCheck(true);
			}
			if (priority[i].HasWorkingAbility(Abilities.HARVEST) && priority[i].item <= 0 && priority[i].pokemon.itemRecycle > 0) {
				if (Items.IsBerry(priority[i].pokemon.itemRecycle) && (GetWeather() == Weather.SUNNYDAY || GetWeather() == Weather.HARSHSUN || Rand(10) < 5)) {
					priority[i].item = priority[i].pokemon.itemRecycle;
					priority[i].pokemon.itemRecycle = 0;
					if (priority[i].pokemon.itemInitial == 0) {
						priority[i].pokemon.itemInitial = priority[i].item;
					}
					Display(string.Format("{0} harvested one {1}!",priority[i].String(), Items.GetName(priority[i].item)));
					priority[i].BerryCureCheck(true);
				}
			}
			if (priority[i].HasWorkingAbility(Abilities.MOODY)) {
				List<int> randomup = new List<int>();
				List<int> randomdown = new List<int>();
				int[] stats = new int[7]{Stats.ATTACK, Stats.DEFENSE, Stats.SPEED, Stats.SPATK, Stats.SPDEF, Stats.ACCURACY, Stats.EVASION};
				for (int j=0; j<stats.Length; j++) {
					if (priority[i].CanIncreaseStatStage(stats[j], priority[i])) {
						randomup.Add(stats[j]);
					}
					if (priority[i].CanReduceStatStage(stats[j], priority[i])) {
						randomdown.Add(stats[j]);
					}
				}
				if (randomup.Count > 0) {
					Debug.Log(string.Format("[Ability triggered] {0}'s Moody (raise stat)",priority[i].String()));
					int r = Rand(randomup.Count);
					priority[i].IncreaseStatWithCause(randomup[r], 2, priority[i], Abilities.GetName(priority[i].ability));
					for (int j=0; j < randomdown.Count; j++) {
						if (randomdown[j] == randomup[r]) {
							randomdown.Remove(j);
							break;
						}
					}
				}
				if (randomdown.Count > 0) {
					Debug.Log(string.Format("[Ability triggered] {0}'s Moody (lower stat)",priority[i].String()));
					int r = Rand(randomdown.Count);
					priority[i].ReduceStatWithCause(randomdown[r], 1, priority[i], Abilities.GetName(priority[i].ability));
				}
			}
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			if (priority[i].HasWorkingItem(Items.TOXICORB) && priority[i].status == 0 && priority[i].CanPoison(null, false)) {
				Debug.Log(string.Format("[Item triggered] {0}'s Toxic Orb",priority[i].String()));
				priority[i].Poison(null, string.Format("{0} was badly poisoned by its {1}", priority[i].String(), Items.GetName(priority[i].item)), true);
			}
			if (priority[i].HasWorkingItem(Items.FLAMEORB) && priority[i].status == 0 && priority[i].CanBurn(null, false)) {
				Debug.Log(string.Format("[Item triggered] {0}'s Flame Orb",priority[i].String()));
				priority[i].Burn(null, string.Format("{0} was burned by its {1}", priority[i].String(), Items.GetName(priority[i].item)));
			}
			if (priority[i].HasWorkingItem(Items.STICKYBARB) && !priority[i].HasWorkingAbility(Abilities.MAGICGUARD)) {
				Debug.Log(string.Format("[Item triggered] {0}'s Sticky Barb",priority[i].String()));
				scene.DamageAnimation(priority[i], 0);
				priority[i].ReduceHP(priority[i].totalHP/8);
				Display(string.Format("{0} is hurt by its {1}!",priority[i].String(), Items.GetName(priority[i].item)));
			}
			if (priority[i].Fainted()) {
				if (!priority[i].Faint()) {
					return;
				}
			}
			if (priority[i].HasWorkingAbility(Abilities.SLOWSTART) && priority[i].turnCount==6) {
				Display(string.Format("{0} finally got its act together!",priority[i].String()));
			}
		}
		for (int i=0; i<4; i++) {
			if (battlers[i].Fainted()) {
				continue;
			}
			battlers[i].CheckForm();
		}
		GainEXP();
		Switch();
		if (decision > 0) {
			return;
		}
		for (int i=0; i<priority.Length; i++) {
			if (priority[i].Fainted()) {
				continue;
			}
			priority[i].AbilitiesOnSwitchIn(false);
		}
		for (int i=0; i<4; i++) {
			if (battlers[i].turnCount > 0 && battlers[i].HasWorkingAbility(Abilities.TRUANT)) {
				battlers[i].effects[Effects.Truant] = battlers[i].effects[Effects.Truant] == 0 ? 1 : 0;
			}
			if (battlers[i].effects[Effects.LockOn] > 0) {
				battlers[i].effects[Effects.LockOn]--;
				if (battlers[i].effects[Effects.LockOn] == 0) {
					battlers[i].effects[Effects.LockOnPos]--;
				}
			}
			battlers[i].effects[Effects.Flinch] = 0;
			battlers[i].effects[Effects.FollowMe] = 0;
			battlers[i].effects[Effects.HelpingHand] = 0;
			battlers[i].effects[Effects.MagicCoat] = 0;
			battlers[i].effects[Effects.Snatch] = 0;
			if (battlers[i].effects[Effects.Charge] > 0) {
				battlers[i].effects[Effects.Charge]--;
			}
			battlers[i].lastHPLost = 0;
			battlers[i].tookDamage = false;
			battlers[i].lastAttacker.Clear();
			battlers[i].effects[Effects.Counter] = -1;
			battlers[i].effects[Effects.CounterTarget] = -1;
			battlers[i].effects[Effects.MirrorCoat] = -1;
			battlers[i].effects[Effects.MirrorCoatTarget] = -1;
		}
		for (int i=0; i<2; i++) {
			if (sides[i].effects[Effects.EchoedVoiceUsed] == 0) {
				sides[i].effects[Effects.EchoedVoiceCounter] = 0;
			}
			sides[i].effects[Effects.EchoedVoiceUsed] = 0;
			sides[i].effects[Effects.MatBlock] = 0;
			sides[i].effects[Effects.QuickGuard] = 0;
			sides[i].effects[Effects.WideGuard] = 0;
			sides[i].effects[Effects.CraftyShield] = 0;
			sides[i].effects[Effects.Round] = 0;
		}
		field.effects[Effects.FusionBolt] = 0;
		field.effects[Effects.FusionFlare] = 0;
		field.effects[Effects.IonDeluge] = 0;
		if (field.effects[Effects.FairyLock] > 0) {
			field.effects[Effects.FairyLock] = -1;
		}
		usePriority = false;
	}

	/*****************
	* End of Battle. *
	*****************/
	public int EndOfBattle(bool canLose=false) {
		switch (decision) {
			case 1:
				Debug.Log(string.Format(""));
				Debug.Log(string.Format("***Player Won***"));
				if (opponent.Length > 0) {
					scene.TrainerBattleSuccess();
					if (opponent.Length > 1) {
						DisplayPaused(string.Format("{0} defeated {1} and {2}!",Player().name, opponent[0].FullName(), opponent[1].FullName()));
					} else {
						DisplayPaused(string.Format("{0} defeated {1}!",Player().name, opponent[0].FullName()));
					}
					scene.ShowOpponent(0);
					Regex rgx = new Regex("[Pp][Nn]");
					DisplayPaused(rgx.Replace(endspeech, Player().name));
					if (opponent.Length > 1) {
						scene.HideOpponent();
						scene.ShowOpponent(1);
						DisplayPaused(rgx.Replace(endspeech2, Player().name));
					}
					if (internalbattle) {
						int tmoney = 0;
						if (opponent.Length > 1) {
							int maxlevel1 = 0;
							int maxlevel2 = 0;
							int limit = SecondPartyBegin(1);
							for (int i=0; i<limit; i++) {
								if (party2[i] != null) {
									if (maxlevel1 < party2[i].pokemon.Level()) {
										maxlevel1 = party2[i].pokemon.Level();
									}
									if (maxlevel2 < party2[i+limit].pokemon.Level()) {
										maxlevel2 = party2[i+limit].pokemon.Level();
									}
								}
							}
							tmoney += maxlevel1*opponent[0].MoneyEarned();
							tmoney += maxlevel2*opponent[1].MoneyEarned();
						} else {
							int maxlevel = 0;
							for (int i=0; i<party2.Length; i++) {
								if (party2[i] == null) {
									continue;
								}
								if (maxlevel < party2[i].pokemon.Level()) {
									maxlevel = party2[i].pokemon.Level();
								}
							}
							tmoney += maxlevel*opponent[0].MoneyEarned();
						}
						if (amuletcoin) {
							tmoney *= 2;
						}
						if (doublemoney) {
							tmoney *= 2;
						}
						int oldmoney = Player().money;
						Player().money += extramoney;
						int moneygained = Player().money - oldmoney;
						if (moneygained > 0) {
							DisplayPaused(string.Format("{0} picked up {1}!",Player().name, Utilities.CommaNumber(extramoney)));
						}
					}
				}
				break;
			case 2:
			case 5:
				Debug.Log(string.Format(""));
				if (decision == 2) {
					Debug.Log(string.Format("***Player Lost***"));
				}
				if (decision == 2) {
					Debug.Log(string.Format("***Player Drew with Opponent***"));
				}
				if (internalbattle) {
					int moneylost = MaxLevelFromIndex(0);
					int[] multiplier = new int[9]{8,16,24,36,48,64,80,100,120};
					moneylost *= multiplier[Math.Min(multiplier.Length-1, Player().NumBadges())];
					if (moneylost > Player().money) {
						moneylost = Player().money;
					}
					if (Globals.switches[Globals.NO_MONEY_LOSS]) {
						moneylost = 0;
					}
					int oldmoney = Player().money;
					Player().money -= moneylost;
					int lostmoney = oldmoney - Player().money;
					if (opponent.Length > 0) {
						if (opponent.Length > 1) {
							DisplayPaused(string.Format("You lost against {0} and {1}!",opponent[0].FullName(), opponent[1].FullName()));
						} else {
							DisplayPaused(string.Format("You lost against {0}!",opponent[0].FullName()));
						}
						if (moneylost > 0) {
							DisplayPaused(string.Format("You gave ${0} to the winner...",Utilities.CommaNumber(lostmoney)));
						}
					} else {
						DisplayPaused(string.Format("You have no more Pokémon that can fight!"));
						if (moneylost > 0) {
							DisplayPaused(string.Format("You panicked and dropped ${0}...",Utilities.CommaNumber(lostmoney)));
						}
					}
					if (!canLose) {
						DisplayPaused(string.Format("You blacked out!"));
					}
				} else if (decision == 2) {
					scene.ShowOpponent(0);
					Regex rgx = new Regex("[Pp][Nn]");
					DisplayPaused(rgx.Replace(endspeechwin, Player().name));
					if (opponent.Length > 1) {
						scene.HideOpponent();
						scene.ShowOpponent(1);
						DisplayPaused(rgx.Replace(endspeechwin2, Player().name));
					}
				}
				break;
		}
		List<int> infected = new List<int>();
		for (int i=0; i<PokemonGlobal.Trainer.party.Length; i++) {
			if (PokemonGlobal.Trainer.party[i].PokerusStage() == 1) {
				infected.Add(i);
			}
		}
		if (infected.Count >= 1) {
			for (int i=0; i<infected.Count; i++) {
				int strain = PokemonGlobal.Trainer.party[infected[i]].PokerusStage();
				if (infected[i] > 0 && PokemonGlobal.Trainer.party[i-1].PokerusStage() == 0) {
					if (Rand(3)==0) {
						PokemonGlobal.Trainer.party[i-1].GivePokerus(strain);
					}
				}
				if (infected[i] < PokemonGlobal.Trainer.party.Length-1 && PokemonGlobal.Trainer.party[i+1].PokerusStage() == 0) {
					if (Rand(3)==0) {
						PokemonGlobal.Trainer.party[i+1].GivePokerus(strain);
					}
				}
			}
		}
		scene.EndBattle(decision);
		for (int i=0; i<battlers.Count; i++) {
			battlers[i].ResetForm();
			if (battlers[i].HasWorkingAbility(Abilities.NATURALCURE)) {
				battlers[i].status = 0;
			}
		}
		for (int i=0; i<PokemonGlobal.Trainer.party.Length; i++) {
			PokemonGlobal.Trainer.party[i].SetItem(PokemonGlobal.Trainer.party[i].itemInitial);
			PokemonGlobal.Trainer.party[i].itemInitial = 0;
			PokemonGlobal.Trainer.party[i].itemRecycle = 0;
			PokemonGlobal.Trainer.party[i].belch = false;
		}
		return decision;
	}

	/*****
	* AI *
	*****/
	public int GetMoveScore(BattleMove move, Battler attacker, Battler opponent, int skill=TrainerAI.bestSkill) {
		if (skill < TrainerAI.minimumSkill) {
			skill = TrainerAI.minimumSkill;
		}
		double score = 100;
		if (opponent != null) {
			opponent = attacker.OppositeOpposing();
		}
		if (opponent != null && opponent.Fainted()) {
			opponent = opponent.Partner();
		}
		int avg;
		int count;
		int aspeed;
		int ospeed;
		int aatk;
		int oatk;
		int adef;
		int odef;
		int aspa;
		int ospa;
		int aspd;
		int ospd;
		int[] blacklist;
		switch (move.function) {
			case 0x00:
			case 0x01:
				score -= 95;
				if (skill >= TrainerAI.highSkill) {
					score = 0;
				}
				break;
			case 0x02:
			case 0x03:
				if (opponent.CanSleep(attacker, false)) {
					score += 30;
					if (skill >= TrainerAI.mediumSkill) {
						if (opponent.effects[Effects.Yawn] > 0) {
							score -= 30;
						}
					}
					if (skill >= TrainerAI.highSkill) {
						if (opponent.HasWorkingAbility(Abilities.MARVELSCALE)) {
							score -= 30;
						}
					}
					if (skill >= TrainerAI.bestSkill) {
						for (int i=0; i<opponent.moves.Length; i++) {
							BattleMove moveData = BattleMove.FromBattleMove(this, new Moves.Move(opponent.moves[i].id));
							if (moveData.function == 0xB4 || moveData.function == 0x11) {
								score -= 50;
								break;
							}
						}
					}
				} else {
					if (skill >= TrainerAI.mediumSkill) {
						if (move.baseDamage == 0) {
							score -= 90;
						}
					}
				}
				break;
			case 0x04:
				if (opponent.effects[Effects.Yawn] > 0 || !opponent.CanSleep(attacker, false)) {
					if (skill >= TrainerAI.mediumSkill) {
						score -= 90;
					}
				} else {
					score += 30;
					if (skill >= TrainerAI.highSkill) {
						if (opponent.HasWorkingAbility(Abilities.MARVELSCALE)) {
							score -= 30;
						}
					}
					if (skill >= TrainerAI.bestSkill) {
						for (int i=0; i<opponent.moves.Length; i++) {
							BattleMove moveData = BattleMove.FromBattleMove(this, new Moves.Move(opponent.moves[i].id));
							if (moveData.function == 0xB4 || moveData.function == 0x11) {
								score -= 50;
								break;
							}
						}
					}
				}
				break;
			case 0x05:
			case 0x06:
			case 0xBE:
				if (opponent.CanPoison(attacker, false)) {
					score += 30;
					if (skill >= TrainerAI.mediumSkill) {
						if (opponent.hp <= opponent.totalHP/4) {
							score += 30;
						}
						if (opponent.hp <= opponent.totalHP/8) {
							score += 50;
						}
						if (opponent.effects[Effects.Yawn] > 0) {
							score -= 40;
						}
					}
					if (skill >= TrainerAI.highSkill) {
						if (RoughStat(opponent, Stats.DEFENSE, skill) > 100) {
							score += 10;
						}
						if (RoughStat(opponent, Stats.SPDEF, skill) > 100) {
							score += 10;
						}
						if (opponent.HasWorkingAbility(Abilities.GUTS)) {
							score -= 40;
						}
						if (opponent.HasWorkingAbility(Abilities.MARVELSCALE)) {
							score -= 40;
						}
						if (opponent.HasWorkingAbility(Abilities.TOXICBOOST)) {
							score -= 40;
						}
					}
				} else {
					if (skill >= TrainerAI.mediumSkill) {
						if (move.baseDamage == 0) {
							score -= 90;
						}
					}
				}
				break;
			case 0x07:
			case 0x08:
			case 0x09:
			case 0xC5:
				if (opponent.CanParalyze(attacker, false) && !(skill >= TrainerAI.mediumSkill && move.id == Moves.THUNDERWAVE && TypeModifier(move.type, attacker, opponent) == 0)) {
					score += 30;
					if (skill >= TrainerAI.mediumSkill) {
						aspeed = RoughStat(attacker, Stats.SPEED, skill);
						ospeed = RoughStat(opponent, Stats.SPEED, skill);
						if (aspeed < ospeed) {
							score += 30;
						} else if (aspeed > ospeed) {
							score -= 40;
						}
					}
					if (skill >= TrainerAI.highSkill) {
						if (opponent.HasWorkingAbility(Abilities.GUTS)) {
							score -= 40;
						}
						if (opponent.HasWorkingAbility(Abilities.MARVELSCALE)) {
							score -= 40;
						}
						if (opponent.HasWorkingAbility(Abilities.QUICKFEET)) {
							score -= 40;
						}
					}
				} else {
					if (skill >= TrainerAI.mediumSkill) {
						if (move.baseDamage == 0) {
							score -= 90;
						}
					}
				}
				break;
			case 0x0A:
			case 0x0B:
			case 0xC6:
				if (opponent.CanBurn(attacker, false)) {
					score += 30;
					if (skill >= TrainerAI.highSkill) {
						if (opponent.HasWorkingAbility(Abilities.GUTS)) {
							score -= 40;
						}
						if (opponent.HasWorkingAbility(Abilities.MARVELSCALE)) {
							score -= 40;
						}
						if (opponent.HasWorkingAbility(Abilities.QUICKFEET)) {
							score -= 40;
						}
						if (opponent.HasWorkingAbility(Abilities.FLAREBOOST)) {
							score -= 40;
						}
					}
				} else {
					if (skill >= TrainerAI.mediumSkill) {
						if (move.baseDamage == 0) {
							score -= 90;
						}
					}
				}
				break;
			case 0x0C:
			case 0x0D:
			case 0x0E:
				if (opponent.CanFreeze(attacker, false)) {
					score += 30;
					if (skill >= TrainerAI.highSkill) {
						if (opponent.HasWorkingAbility(Abilities.MARVELSCALE)) {
							score -= 20;
						}
					}
				} else {
					if (skill >= TrainerAI.mediumSkill) {
						if (move.baseDamage == 0) {
							score -= 90;
						}
					}
				}
				break;
			case 0x0F:
				score += 30;
				if (skill >= TrainerAI.highSkill) {
					if (!opponent.HasWorkingAbility(Abilities.INNERFOCUS) && opponent.effects[Effects.Substitute] == 0) {
						score += 30;
					}
				}
				break;
			case 0x10:
				if (skill >= TrainerAI.highSkill) {
					if (!opponent.HasWorkingAbility(Abilities.INNERFOCUS) && opponent.effects[Effects.Substitute] == 0) {
						score += 30;
					}
				}
				if (opponent.effects[Effects.Minimize] != 0) {
					score += 30;
				}
				break;
			case 0x11:
				if (attacker.status == Statuses.SLEEP) {
					score += 100;
					if (skill >= TrainerAI.highSkill) {
						if (skill >= TrainerAI.highSkill) {
							if (!opponent.HasWorkingAbility(Abilities.INNERFOCUS) && opponent.effects[Effects.Substitute] == 0) {
								score += 30;
							}
						}
					}
				} else {
					score -= 90;
					if (skill >= TrainerAI.bestSkill) {
						score = 0;
					}
				}
				break;
			case 0x12:
				if (attacker.turnCount == 0) {
					if (skill >= TrainerAI.highSkill) {
						if (skill >= TrainerAI.highSkill) {
							if (!opponent.HasWorkingAbility(Abilities.INNERFOCUS) && opponent.effects[Effects.Substitute] == 0) {
								score += 30;
							}
						}
					}
				} else {
					score -= 90;
					if (skill >= TrainerAI.bestSkill) {
						score = 0;
					}
				}
				break;
			case 0x13:
			case 0x14:
			case 0x15:
				if (opponent.CanConfuse(attacker, false)) {
					score += 30;
				} else {
					if (skill >= TrainerAI.mediumSkill) {
						if (move.baseDamage == 0) {
							score -= 90;
						}
					}
				}
				break;
			case 0x16:
				bool canAttract = true;
				int agender = attacker.gender;
				int ogender = opponent.gender;
				if (agender == 2 || ogender == 2 || agender == ogender) {
					score -= 90;
					canAttract = false;
				} else if (opponent.effects[Effects.Attract] >= 0) {
					score -= 80;
					canAttract = false;
				} else if (skill >= TrainerAI.bestSkill && opponent.HasWorkingAbility(Abilities.OBLIVIOUS)) {
					score -= 80;
					canAttract = false;
				}
				if (skill >= TrainerAI.highSkill) {
					if (canAttract && opponent.HasWorkingItem(Items.DESTINYKNOT) && attacker.CanAttract(opponent, false)) {
						score -= 30;
					}
				}
				break;
			case 0x17:
				if (opponent.status == 0) {
					score += 30;
				}
				break;
			case 0x18:
				if (attacker.status == Statuses.BURN) {
					score += 40;
				} else if (attacker.status == Statuses.POISON) {
					score += 40;
					if (skill >= TrainerAI.mediumSkill) {
						if (attacker.hp < attacker.totalHP/8) {
							score += 60;
						} else if (skill >= TrainerAI.highSkill && attacker.hp < (attacker.effects[Effects.Toxic] + 1) * attacker.totalHP/16) {
							score += 60;
						}
					}
				} else if (attacker.status == Statuses.PARALYSIS) {
					score += 40;
				} else {
					score -= 90;
				}
				break;
			case 0x19:
				Battler[] party = Party(attacker.index);
				int statuses = 0;
				for (int i=0; i<party.Length; i++) {
					if (party[i] != null && party[i].status != 0) {
						statuses++;
					}
				}
				if (statuses==0) {
					score -= 80;
				} else {
					score += 20*statuses;
				}
				break;
			case 0x1A:
				if (attacker.OwnSide().effects[Effects.Safeguard] > 0) {
					score -= 80;
				} else if (attacker.status != 0) {
					score -= 40;
				} else {
					score += 30;
				}
				break;
			case 0x1B:
				if (attacker.status == 0) {
					score -= 90;
				} else {
					score += 40;
				}
				break;
			case 0x1C:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.ATTACK)) {
						score -= 90;
					} else {
						score -= attacker.stages[Stats.ATTACK]*20;
						if (skill >= TrainerAI.mediumSkill) {
							bool hasPhysicalAttack = false;
							for (int i=0; i<attacker.moves.Length; i++) {
								if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
									hasPhysicalAttack = true;
									break;
								}
							}
							if (hasPhysicalAttack) {
								score += 20;
							} else if (skill >= TrainerAI.highSkill) {
								score -= 90;
							}
						}
					}
				} else {
					if (attacker.stages[Stats.ATTACK] < 0) {
						score += 20;
					}
					if (skill >= TrainerAI.mediumSkill) {
						bool hasPhysicalAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasPhysicalAttack = true;
								break;
							}
						}
						if (hasPhysicalAttack) {
							score += 20;
						}
					}
				}
				break;
			case 0x1D:
			case 0x1E:
			case 0xC8:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.DEFENSE)) {
						score -= 90;
					} else {
						score -= attacker.stages[Stats.DEFENSE]*20;
					}
				} else {
					if (attacker.stages[Stats.DEFENSE] < 0) {
						score += 20;
					}
				}
				break;
			case 0x1F:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.SPEED)) {
						score -= 90;
					} else {
						score -= attacker.stages[Stats.SPEED]*10;
						if (skill >= TrainerAI.highSkill) {
							aspeed = RoughStat(attacker, Stats.SPEED, skill);
							ospeed = RoughStat(opponent, Stats.SPEED, skill);
							if (aspeed < ospeed && aspeed*2 > ospeed) {
								score += 30;
							}
						}
					}
				} else {
					if (attacker.stages[Stats.SPEED] < 0) {
						score += 20;
					}
				}
				break;
			case 0x20:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.SPATK)) {
						score -= 90;
					} else {
						score -= attacker.stages[Stats.SPATK]*20;
						if (skill >= TrainerAI.mediumSkill) {
							bool hasSpecialAttack = false;
							for (int i=0; i<attacker.moves.Length; i++) {
								if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsSpecial(attacker.moves[i].type)) {
									hasSpecialAttack = true;
								}
							}
							if (hasSpecialAttack) {
								score += 20;
							} else if (skill >= TrainerAI.highSkill) {
								score -= 90;
							}
						}
					}
				} else {
					if (attacker.stages[Stats.SPATK] < 0) {
						score += 20;
					}
					if (skill >= TrainerAI.mediumSkill) {
						bool hasSpecialAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasSpecialAttack = true;
								break;
							}
						}
						if (hasSpecialAttack) {
							score += 20;
						}
					}
				}
				break;
			case 0x21:
				bool foundMove = false;
				for (int i=0; i<4; i++) {
					if (attacker.moves[i].type == Types.ELECTRIC && attacker.moves[i].baseDamage > 0) {
						foundMove = true;
						break;
					}
				}
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.SPDEF)) {
						score -= 90;
					} else {
						score -= attacker.stages[Stats.SPDEF]*20;
					}
					if (foundMove) {
						score += 20;
					}
				} else {
					if (attacker.stages[Stats.SPDEF] < 0) {
						score += 20;
					}
					if (foundMove) {
						score += 20;
					}
				}
				break;
			case 0x22:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.EVASION)) {
						score -= 90;
					} else {
						score -= attacker.stages[Stats.EVASION]*10;
					}
				} else {
					if (attacker.stages[Stats.EVASION]<0) {
						score += 20;
					}
				}
				break;
			case 0x23:
				if (move.baseDamage==0) {
					if (attacker.effects[Effects.FocusEnergy] >= 2) {
						score -= 80;
					} else {
						score += 30;
					}
				} else {
					if (attacker.effects[Effects.FocusEnergy]<2)  
					{
						score += 30;
					}
				}
				break;
			case 0x24:
				if (attacker.TooHigh(Stats.ATTACK) && attacker.TooHigh(Stats.DEFENSE)) {
					score -= 90;
				} else {
					score -= attacker.stages[Stats.ATTACK]*10;
					score -= attacker.stages[Stats.DEFENSE]*10;
					if (skill >= TrainerAI.mediumSkill) {
						bool hasPhysicalAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasPhysicalAttack = true;
								break;
							}
						}
						if (hasPhysicalAttack) {
							score += 20;
						} else if (skill >= TrainerAI.highSkill) {
							score -= 90;
						}
					}
				}
				break;
			case 0x25:
				if (attacker.TooHigh(Stats.ATTACK) && attacker.TooHigh(Stats.DEFENSE) && attacker.TooHigh(Stats.ACCURACY)) {
					score -= 90;
				} else {
					score -= attacker.stages[Stats.ATTACK]*10;
					score -= attacker.stages[Stats.DEFENSE]*10;
					score -= attacker.stages[Stats.ACCURACY]*10;
					if (skill >= TrainerAI.mediumSkill) {
						bool hasPhysicalAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasPhysicalAttack = true;
								break;
							}
						}
						if (hasPhysicalAttack) {
							score += 20;
						} else if (skill >= TrainerAI.highSkill) {
							score -= 90;
						}
					}
				}
				break;
			case 0x26:
				if (attacker.TooHigh(Stats.ATTACK) && attacker.TooHigh(Stats.DEFENSE)) {
					score -= 90;
				} else {
					score -= attacker.stages[Stats.ATTACK]*10;
					score -= attacker.stages[Stats.SPEED]*10;
					if (skill >= TrainerAI.mediumSkill) {
						bool hasPhysicalAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasPhysicalAttack = true;
								break;
							}
						}
						if (hasPhysicalAttack) {
							score += 20;
						} else if (skill >= TrainerAI.highSkill) {
							score -= 90;
						}
					}
					if (skill >= TrainerAI.highSkill) {
						aspeed = RoughStat(attacker, Stats.SPEED, skill);
						ospeed = RoughStat(opponent, Stats.SPEED, skill);
						if (aspeed < ospeed && aspeed*2 > ospeed) {
							score += 20;
						}
					}
				}
				break;
			case 0x27:
			case 0x28:
				if (attacker.TooHigh(Stats.ATTACK) && attacker.TooHigh(Stats.SPATK)) {
					score -= 90;
				} else {
					score -= attacker.stages[Stats.ATTACK]*10;
					score -= attacker.stages[Stats.SPATK]*10;
					if (skill >= TrainerAI.mediumSkill) {
						bool hasDamagingAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0) {
								hasDamagingAttack = true;
								break;
							}
						}
						if (hasDamagingAttack) {
							score += 20;
						} else if (skill >= TrainerAI.highSkill) {
							score -= 90;
						}
					}
					if (move.function == 0x28) {
						if (GetWeather() == Weather.SUNNYDAY) {
							score += 20;
						}
					}
				}
				break;
			case 0x29:
				if (attacker.TooHigh(Stats.ATTACK) && attacker.TooHigh(Stats.ACCURACY)) {
					score -= 90;
				} else {
					score -= attacker.stages[Stats.ATTACK]*10;
					score -= attacker.stages[Stats.ACCURACY]*10;
					if (skill >= TrainerAI.mediumSkill) {
						bool hasPhysicalAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasPhysicalAttack = true;
								break;
							}
						}
						if (hasPhysicalAttack) {
							score += 20;
						} else if (skill >= TrainerAI.highSkill) {
							score -= 90;
						}
					}
				}
				break;
			case 0x2A:
				if (attacker.TooHigh(Stats.SPDEF) && attacker.TooHigh(Stats.DEFENSE)) {
					score -= 90;
				} else {
					score -= attacker.stages[Stats.SPDEF]*10;
					score -= attacker.stages[Stats.DEFENSE]*10;
				}
				break;
			case 0x2B:
				if (attacker.TooHigh(Stats.SPEED) && attacker.TooHigh(Stats.SPATK) && attacker.TooHigh(Stats.SPDEF)) {
					score -= 90;
				} else {
					score -= attacker.stages[Stats.SPEED]*10;
					score -= attacker.stages[Stats.SPATK]*10;
					score -= attacker.stages[Stats.SPDEF]*10;
					if (skill >= TrainerAI.mediumSkill) {
						bool hasSpecialAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasSpecialAttack = true;
								break;
							}
						}
						if (hasSpecialAttack) {
							score += 20;
						} else if (skill >= TrainerAI.highSkill) {
							score -= 90;
						}
					}
				}
				break;
			case 0x2C:
				if (attacker.TooHigh(Stats.SPATK) && attacker.TooHigh(Stats.SPDEF)) {
					score -= 90;
				} else {
					score -= attacker.stages[Stats.SPATK]*10;
					score -= attacker.stages[Stats.SPDEF]*10;
					if (skill >= TrainerAI.mediumSkill) {
						bool hasSpecialAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasSpecialAttack = true;
								break;
							}
						}
						if (hasSpecialAttack) {
							score += 20;
						} else if (skill >= TrainerAI.highSkill) {
							score -= 90;
						}
					}
				}
				break;
			case 0x2D:
				if (attacker.stages[Stats.ATTACK] < 0) {
					score += 10;
				}
				if (attacker.stages[Stats.DEFENSE] < 0) {
					score += 10;
				}
				if (attacker.stages[Stats.SPEED] < 0) {
					score += 10;
				}
				if (attacker.stages[Stats.SPATK] < 0) {
					score += 10;
				}
				if (attacker.stages[Stats.SPDEF] < 0) {
					score += 10;
				}
				if (skill >= TrainerAI.mediumSkill) {
					bool hasDamagingAttack = false;
					for (int i=0; i<attacker.moves.Length; i++) {
						if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0) {
							hasDamagingAttack = true;
							break;
						}
					}
					if (hasDamagingAttack) {
						score += 20;
					}
				}
				break;
			case 0x2E:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.ATTACK)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 40;
						}
						score -= attacker.stages[Stats.ATTACK]*20;
						if (skill >= TrainerAI.mediumSkill) {
							bool hasPhysicalAttack = false;
							for (int i=0; i<attacker.moves.Length; i++) {
								if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
									hasPhysicalAttack = true;
									break;
								}
							}
							if (hasPhysicalAttack) {
								score += 20;
							} else if (skill >= TrainerAI.highSkill) {
								score -= 90;
							}
						}
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (attacker.stages[Stats.ATTACK] < 0) {
						score += 20;
					}
					if (skill >= TrainerAI.mediumSkill) {
						bool hasPhysicalAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasPhysicalAttack = true;
								break;
							}
						}
						if (hasPhysicalAttack) {
							score += 20;
						}
					}
				}
				break;
			case 0x2F:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.DEFENSE)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 40;
						}
						score -= attacker.stages[Stats.DEFENSE]*20;
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (attacker.stages[Stats.DEFENSE] < 0) {
						score += 20;
					}
				}
				break;
			case 0x30:
			case 0x31:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.SPEED)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 20;
						}
						score -= attacker.stages[Stats.SPEED]*10;
						if (skill >= TrainerAI.highSkill) {
							aspeed = RoughStat(attacker, Stats.SPEED, skill);
							ospeed = RoughStat(opponent, Stats.SPEED, skill);
							if (aspeed < ospeed && aspeed*2 > ospeed) {
								score += 30;
							}
						}
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (attacker.stages[Stats.SPEED] < 0) {
						score += 20;
					}
				}
				break;
			case 0x32:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.SPATK)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 40;
						}
						score -= attacker.stages[Stats.SPATK]*20;
						if (skill >= TrainerAI.mediumSkill) {
							bool hasSpecialAttack = false;
							for (int i=0; i<attacker.moves.Length; i++) {
								if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsSpecial(attacker.moves[i].type)) {
									hasSpecialAttack = true;
									break;
								}
							}
							if (hasSpecialAttack) {
								score += 20;
							} else if (skill >= TrainerAI.highSkill) {
								score -= 90;
							}
						}
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (attacker.stages[Stats.SPATK] < 0) {
						score += 20;
					}
					if (skill >= TrainerAI.mediumSkill) {
						bool hasSpecialAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasSpecialAttack = true;
								break;
							}
						}
						if (hasSpecialAttack) {
							score += 20;
						}
					}
				}
				break;
			case 0x33:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.SPDEF)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 40;
						}
						score -= attacker.stages[Stats.SPDEF]*20;
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (attacker.stages[Stats.SPDEF] < 0) {
						score += 20;
					}
				}
				break;
			case 0x34:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.EVASION)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 40;
						}
						score -= attacker.stages[Stats.EVASION]*20;
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (attacker.stages[Stats.EVASION] < 0) {
						score += 20;
					}
				}
				break;
			case 0x35:
				score -= attacker.stages[Stats.ATTACK]*20;
				score -= attacker.stages[Stats.SPEED]*20;
				score -= attacker.stages[Stats.SPATK]*20;
				score -= attacker.stages[Stats.DEFENSE]*10;
				score -= attacker.stages[Stats.SPDEF]*10;
				if (skill >= TrainerAI.mediumSkill) {
					bool hasDamagingAttack = false;
					for (int i=0; i<attacker.moves.Length; i++) {
						if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0) {
							hasDamagingAttack = true;
							break;
						}
					}
					if (hasDamagingAttack) {
						score += 20;
					}
				}
				break;
			case 0x36:
				if (attacker.TooHigh(Stats.ATTACK) && attacker.TooHigh(Stats.SPEED)) {
					score -= 90;
				} else {
					score -= attacker.stages[Stats.ATTACK]*20;
					score -= attacker.stages[Stats.SPEED]*20;
					if (skill >= TrainerAI.mediumSkill) {
						bool hasPhysicalAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasPhysicalAttack = true;
								break;
							}
						}
						if (hasPhysicalAttack) {
							score += 20;
						} else if (skill >= TrainerAI.highSkill) {
							score -= 90;
						}
					}
					if (skill >= TrainerAI.highSkill) {
						aspeed = RoughStat(attacker, Stats.SPEED, skill);
						ospeed = RoughStat(opponent, Stats.SPEED, skill);
						if (aspeed < ospeed && aspeed*2 > ospeed) {
							score += 30;
						}
					}
				}
				break;
			case 0x37:
				if (attacker.TooHigh(Stats.ATTACK) && attacker.TooHigh(Stats.DEFENSE) && attacker.TooHigh(Stats.SPEED) && attacker.TooHigh(Stats.SPATK) && attacker.TooHigh(Stats.SPDEF) && attacker.TooHigh(Stats.ACCURACY) && attacker.TooHigh(Stats.EVASION)) {
					score -= 90;
				} else {
					int avstat = 0;
					avstat -= attacker.stages[Stats.ATTACK];
					avstat -= attacker.stages[Stats.ATTACK];
					avstat -= attacker.stages[Stats.ATTACK];
					avstat -= attacker.stages[Stats.ATTACK];
					avstat -= attacker.stages[Stats.ATTACK];
					avstat -= attacker.stages[Stats.ATTACK];
					avstat -= attacker.stages[Stats.ATTACK];
					if (avstat < 0) {
						avstat = (int)(avstat/2);
					}
					score += avstat * 10;
				}
				break;
			case 0x38:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.DEFENSE)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 40;
						}
						score -= attacker.stages[Stats.DEFENSE]*30;
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (attacker.stages[Stats.DEFENSE] < 0) {
						score += 30;
					}
				}
				break;
			case 0x39:
				if (move.baseDamage == 0) {
					if (attacker.TooHigh(Stats.SPATK)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 40;
						}
						score -= attacker.stages[Stats.SPATK]*30;
						if (skill >= TrainerAI.mediumSkill) {
							bool hasSpecialAttack = false;
							for (int i=0; i<attacker.moves.Length; i++) {
								if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsSpecial(attacker.moves[i].type)) {
									hasSpecialAttack = true;
									break;
								}
							}
							if (hasSpecialAttack) {
								score += 20;
							} else if (skill >= TrainerAI.highSkill) {
								score -= 90;
							}
						}
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (attacker.stages[Stats.SPATK] < 0) {
						if (skill >= TrainerAI.mediumSkill) {
							bool hasSpecialAttack = false;
							for (int i=0; i<attacker.moves.Length; i++) {
								if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsSpecial(attacker.moves[i].type)) {
									hasSpecialAttack = true;
									break;
								}
							}
							if (hasSpecialAttack) {
								score += 30;
							}
						}
					}
				}
				break;
			case 0x3A:
				if (attacker.TooHigh(Stats.ATTACK) || attacker.hp <= attacker.totalHP/2) {
					score -= 100;
				} else {
					score += (6-attacker.stages[Stats.ATTACK])*10;
					if (skill >= TrainerAI.mediumSkill) {
						bool hasPhysicalAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasPhysicalAttack = true;
								break;
							}
						}
						if (hasPhysicalAttack) {
							score += 40;
						} else if (skill >= TrainerAI.highSkill) {
							score -= 90;
						}
					}
				}
				break;
			case 0x3B:
				avg = attacker.stages[Stats.ATTACK]*10;
				avg += attacker.stages[Stats.DEFENSE]*10;
				score += avg/2;
				break;
			case 0x3C:
				avg = attacker.stages[Stats.DEFENSE]*10;
				avg += attacker.stages[Stats.SPDEF]*10;
				score += avg/2;
				break;
			case 0x3D:
				avg = attacker.stages[Stats.DEFENSE]*10;
				avg += attacker.stages[Stats.SPDEF]*10;
				avg += attacker.stages[Stats.SPEED]*10;
				score += avg/3;
				break;
			case 0x3E:
				score += attacker.stages[Stats.SPEED]*10;
				break;
			case 0x3F:
				score += attacker.stages[Stats.SPATK]*10;
				break;
			case 0x40:
				if (!opponent.CanConfuse(attacker, false)) {
					score -= 90;
				} else {
					if (opponent.stages[Stats.SPATK] < 0) {
						score += 30;
					}
				}
				break;
			case 0x41:
				if (!opponent.CanConfuse(attacker, false)) {
					score -= 90;
				} else {
					if (opponent.stages[Stats.ATTACK] < 0) {
						score += 30;
					}
				}
				break;
			case 0x42:
				if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.ATTACK, attacker)) {
						score -= 90;
					} else {
						score += opponent.stages[Stats.ATTACK]*20;
						if (skill >= TrainerAI.mediumSkill) {
							bool hasPhysicalAttack = false;
							for (int i=0; i<attacker.moves.Length; i++) {
								if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
									hasPhysicalAttack = true;
									break;
								}
							}
							if (hasPhysicalAttack) {
								score += 20;
							} else if (skill >= TrainerAI.highSkill) {
								score -= 90;
							}
						}
					}
				} else {
					if (opponent.stages[Stats.ATTACK] > 0) {
						score += 20;
					}
					if (skill >= TrainerAI.mediumSkill) {
						bool hasPhysicalAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasPhysicalAttack = true;
								break;
							}
						}
						if (hasPhysicalAttack) {
							score += 20;
						}
					}
				}
				break;
			case 0x43:
				if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.DEFENSE, attacker)) {
						score -= 90;
					} else {
						score += opponent.stages[Stats.DEFENSE]*20;
					}
				} else {
					if (opponent.stages[Stats.DEFENSE] > 0) {
						score += 20;
					}
				}
				break;
			case 0x44:
				if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.DEFENSE, attacker)) {
						score += 90;
					} else {
						score += attacker.stages[Stats.DEFENSE]*10;
						if (skill >= TrainerAI.highSkill) {
							aspeed = RoughStat(attacker, Stats.SPEED, skill);
							ospeed = RoughStat(opponent, Stats.SPEED, skill);
							if (aspeed < ospeed && aspeed*2 > ospeed) {
								score += 30;
							}
						}
					}
				} else {
					if (opponent.stages[Stats.SPEED] > 0) {
						score += 20;
					}
				}
				break;
			case 0x45:
				if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.SPATK, attacker)) {
						score -= 90;
					} else {
						score += attacker.stages[Stats.SPATK]*20;
						if (skill >= TrainerAI.mediumSkill) {
							bool hasSpecialAttack = false;
							for (int i=0; i<attacker.moves.Length; i++) {
								if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsSpecial(attacker.moves[i].type)) {
									hasSpecialAttack = true;
									break;
								}
							}
							if (hasSpecialAttack) {
								score += 20;
							} else if (skill >= TrainerAI.highSkill) {
								score -= 90;
							}
						}
					}
				} else {
					if (opponent.stages[Stats.SPATK] > 0) {
						score += 20;
					}
					if (skill >= TrainerAI.mediumSkill) {
						bool hasSpecialAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsSpecial(attacker.moves[i].type)) {
								hasSpecialAttack = true;
								break;
							}
						}
						if (hasSpecialAttack) {
							score += 20;
						}
					}
				}
				break;
			case 0x46:
				if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.SPDEF, attacker)) {
						score -= 90;
					} else {
						score += attacker.stages[Stats.SPDEF]*20;
					}
				} else {
					if (opponent.stages[Stats.SPDEF] > 0) {
						score += 20;
					}
				}
				break;
			case 0x47:
				if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.ACCURACY, attacker)) {
						score -= 90;
					} else {
						score += attacker.stages[Stats.ACCURACY]*20;
					}
				} else {
					if (opponent.stages[Stats.ACCURACY] > 0) {
						score += 20;
					}
				}
				break;
			case 0x48:
				if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.EVASION, attacker)) {
						score -= 90;
					} else {
						score += attacker.stages[Stats.EVASION]*20;
					}
				} else {
					if (opponent.stages[Stats.EVASION] > 0) {
						score += 20;
					}
				}
				break;
			case 0x49:
				if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.EVASION, attacker)) {
						score -= 90;
					} else {
						score += attacker.stages[Stats.EVASION]*20;
					}
				} else {
					if (opponent.stages[Stats.EVASION] > 0) {
						score += 20;
					}
				}
				if (opponent.OwnSide().effects[Effects.Reflect] > 0 || opponent.OwnSide().effects[Effects.LightScreen] > 0 || opponent.OwnSide().effects[Effects.Mist] > 0 || opponent.OwnSide().effects[Effects.Safeguard] > 0) {
					score += 30;
				}
				if (opponent.OwnSide().effects[Effects.Spikes] > 0 || opponent.OwnSide().effects[Effects.ToxicSpikes] > 0 || opponent.OwnSide().effects[Effects.StealthRock] != 0) {
					score -= 30;
				}
				break;
			case 0x4A:
				avg = attacker.stages[Stats.ATTACK]*10;
				avg += attacker.stages[Stats.DEFENSE]*10;
				score += avg/2;
				break;
			case 0x4B:
				if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.ATTACK, attacker)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 40;
						}
						score += attacker.stages[Stats.ATTACK]*20;
						if (skill >= TrainerAI.mediumSkill) {
							bool hasPhysicalAttack = false;
							for (int i=0; i<attacker.moves.Length; i++) {
								if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
									hasPhysicalAttack = true;
									break;
								}
							}
							if (hasPhysicalAttack) {
								score += 20;
							} else if (skill >= TrainerAI.highSkill) {
								score -= 90;
							}
						}
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (opponent.stages[Stats.ATTACK] > 0) {
						score += 20;
					}
					if (skill >= TrainerAI.mediumSkill) {
						bool hasPhysicalAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasPhysicalAttack = true;
								break;
							}
						}
						if (hasPhysicalAttack) {
							score += 20;
						}
					}
				}
				break;
			case 0x4C:
				if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.DEFENSE, attacker)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 40;
						}
						score += attacker.stages[Stats.DEFENSE]*20;
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (opponent.stages[Stats.DEFENSE] > 0) {
						score += 20;
					}
				}
				break;
			case 0x4D:
				if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.SPEED, attacker)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 20;
						}
						score += attacker.stages[Stats.SPEED]*20;
						if (skill >= TrainerAI.highSkill) {
							aspeed = RoughStat(attacker, Stats.SPEED, skill);
							ospeed = RoughStat(opponent, Stats.SPEED, skill);
							if (aspeed < ospeed && aspeed*2 > ospeed) {
								score += 30;
							}
						}
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (opponent.stages[Stats.SPEED] > 0) {
						score += 30;
					}
				}
				break;
			case 0x4E:
				if (attacker.gender == 2 || opponent.gender == 2 || attacker.gender == opponent.gender || opponent.HasWorkingAbility(Abilities.OBLIVIOUS)) {
					score -= 90;
				} else if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.SPATK, attacker)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 40;
						}
						score += attacker.stages[Stats.SPATK]*20;
						if (skill >= TrainerAI.mediumSkill) {
							bool hasSpecialAttack = false;
							for (int i=0; i<attacker.moves.Length; i++) {
								if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsSpecial(attacker.moves[i].type)) {
									hasSpecialAttack = true;
									break;
								}
							}
							if (hasSpecialAttack) {
								score += 20;
							} else if (skill >= TrainerAI.highSkill) {
								score -= 90;
							}
						}
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (opponent.stages[Stats.SPATK] > 0) {
						score += 20;
					}
					if (skill >= TrainerAI.mediumSkill) {
						bool hasSpecialAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsSpecial(attacker.moves[i].type)) {
								hasSpecialAttack = true;
								break;
							}
						}
						if (hasSpecialAttack) {
							score += 30;
						}
					}
				}
				break;
			case 0x4F:
				if (move.baseDamage == 0) {
					if (!opponent.CanReduceStatStage(Stats.SPDEF, attacker)) {
						score -= 90;
					} else {
						if (attacker.turnCount == 0) {
							score += 40;
						}
						score += attacker.stages[Stats.SPDEF]*20;
					}
				} else {
					if (attacker.turnCount == 0) {
						score += 10;
					}
					if (opponent.stages[Stats.SPDEF] > 0) {
						score += 20;
					}
				}
				break;
			case 0x50:
				if (opponent.effects[Effects.Substitute] > 0) {
					score -= 90;
				} else {
					bool anyChange = false;
					avg = opponent.stages[Stats.ATTACK];
					if (avg != 0) {
						anyChange=true;
					}
					avg += opponent.stages[Stats.DEFENSE];
					if (avg != 0) {
						anyChange=true;
					}
					avg += opponent.stages[Stats.SPEED];
					if (avg != 0) {
						anyChange=true;
					}
					avg += opponent.stages[Stats.SPATK];
					if (avg != 0) {
						anyChange=true;
					}
					avg += opponent.stages[Stats.SPDEF];
					if (avg != 0) {
						anyChange=true;
					}
					avg += opponent.stages[Stats.ACCURACY];
					if (avg != 0) {
						anyChange=true;
					}
					avg += opponent.stages[Stats.EVASION];
					if (avg != 0) {
						anyChange=true;
					}
					if (anyChange) {
						score += avg * 10;
					} else {
						score -= 90;
					}
				}
				break;
			case 0x51:
				if (skill >= TrainerAI.mediumSkill) {
					int stages = 0;
					for (int i = 0; i<4; i++) {
						Battler battler = battlers[i];
						if (attacker.IsOpposing(i)) {
							stages += battler.stages[Stats.ATTACK];
							stages += battler.stages[Stats.DEFENSE];
							stages += battler.stages[Stats.SPEED];
							stages += battler.stages[Stats.SPATK];
							stages += battler.stages[Stats.SPDEF];
							stages += battler.stages[Stats.EVASION];
							stages += battler.stages[Stats.ACCURACY];
						} else {
							stages -= battler.stages[Stats.ATTACK];
							stages -= battler.stages[Stats.DEFENSE];
							stages -= battler.stages[Stats.SPEED];
							stages -= battler.stages[Stats.SPATK];
							stages -= battler.stages[Stats.SPDEF];
							stages -= battler.stages[Stats.EVASION];
							stages -= battler.stages[Stats.ACCURACY];
						}
					}
					score += stages * 10;
				}
				break;
			case 0x52:
				if (skill >= TrainerAI.mediumSkill) {
					aatk = attacker.stages[Stats.ATTACK];
					aspa = attacker.stages[Stats.SPATK];
					oatk = opponent.stages[Stats.ATTACK];
					ospa = opponent.stages[Stats.SPATK];
					if (aatk >= oatk && aspa >= ospa) {
						score -= 80;
					} else {
						score += (oatk-aatk)*10;
						score += (ospa-aspa)*10;
					}
				} else {
					score += 50;
				}
				break;
			case 0x53:
				if (skill >= TrainerAI.mediumSkill) {
					adef = attacker.stages[Stats.DEFENSE];
					aspd = attacker.stages[Stats.SPDEF];
					odef = opponent.stages[Stats.DEFENSE];
					ospd = opponent.stages[Stats.SPDEF];
					if (adef >= odef && aspd >= ospd) {
						score -= 80;
					} else {
						score += (odef-adef)*10;
						score += (ospd-aspd)*10;
					}
				} else {
					score -= 50;
				}
				break;
			case 0x54:
				if (skill >= TrainerAI.mediumSkill) {
					int astages = attacker.stages[Stats.ATTACK];
					astages += attacker.stages[Stats.DEFENSE];
					astages += attacker.stages[Stats.SPEED];
					astages += attacker.stages[Stats.SPATK];
					astages += attacker.stages[Stats.SPDEF];
					astages += attacker.stages[Stats.EVASION];
					astages += attacker.stages[Stats.ACCURACY];
					int ostages = opponent.stages[Stats.ATTACK];
					ostages += opponent.stages[Stats.DEFENSE];
					ostages += opponent.stages[Stats.SPEED];
					ostages += opponent.stages[Stats.SPATK];
					ostages += opponent.stages[Stats.SPDEF];
					ostages += opponent.stages[Stats.EVASION];
					ostages += opponent.stages[Stats.ACCURACY];
					score += (ostages-astages)*10;
				} else {
					score -= 50;
				}
				break;
			case 0x55:
				if (skill >= TrainerAI.mediumSkill) {
					bool equal = true;
					int[] stats = new int[7]{Stats.ATTACK, Stats.DEFENSE, Stats.SPEED, Stats.SPATK, Stats.SPDEF, Stats.ACCURACY, Stats.EVASION};
					for (int i=0; i<stats.Length; i++) {
						int stagediff = opponent.stages[stats[i]] - attacker.stages[stats[i]];
						score += stagediff*10;
						if (stagediff != 0) {
							equal = false;
						}
					}
					if (equal) {
						score -= 80;
					}
				} else {
					score -= 50;
				}
				break;
			case 0x56:
				if (attacker.OwnSide().effects[Effects.Mist] > 0) {
					score -= 80;
				}
				break;
			case 0x57:
				if (skill >= TrainerAI.mediumSkill) {
					aatk = RoughStat(attacker, Stats.ATTACK, skill);
					adef = RoughStat(attacker, Stats.DEFENSE, skill);
					if (aatk == adef || attacker.effects[Effects.PowerTrick] != 0) {
						score -= 90;
					} else if (adef > aatk) {
						score += 30;
					} else {
						score -= 30;
					}
				} else {
					score -= 30;
				}
				break;
			case 0x58:
				if (skill >= TrainerAI.mediumSkill) {
					aatk = RoughStat(attacker, Stats.ATTACK, skill);
					aspa = RoughStat(attacker, Stats.SPATK, skill);
					oatk = RoughStat(opponent, Stats.ATTACK, skill);
					ospa = RoughStat(opponent, Stats.SPATK, skill);
					if (aatk<oatk && aspa<ospa) {
						score += 50;
					} else if ((aatk+aspa)<(oatk+ospa)) {
						score += 30;
					} else {
						score -= 50;
					}
				} else {
					score -= 30;
				}
				break;
			case 0x59:
				if (skill >= TrainerAI.mediumSkill) {
					adef = RoughStat(attacker, Stats.DEFENSE, skill);
					aspd = RoughStat(attacker, Stats.SPDEF, skill);
					odef = RoughStat(opponent, Stats.DEFENSE, skill);
					ospd = RoughStat(opponent, Stats.SPDEF, skill);
					if (adef<odef && aspd<ospd) {
						score += 50;
					} else if ((adef+aspd)<(odef+ospd)) {
						score += 30;
					} else {
						score -= 50;
					}
				} else {
					score -= 30;
				}
				break;
			case 0x5A:
				if (opponent.effects[Effects.Substitute] > 0) {
					score -= 90;
				} else if (attacker.hp >= (attacker.hp+opponent.hp)/2) {
					score -= 90;
				} else {
					score += 40;
				}
				break;
			case 0x5B:
				if (attacker.OwnSide().effects[Effects.Tailwind] > 0) {
					score -= 90;
				}
				break;
			case 0x5C:
				blacklist = new int[5]{0x02,0x14,0x5C,0x5D,0xB6};
				if (attacker.effects[Effects.Transform] != 0 || opponent.lastMoveUsed <= 0 || Array.IndexOf(blacklist, BattleMove.FromBattleMove(this, new Moves.Move(opponent.lastMoveUsed)).function) > -1) {
					score -= 90;
				}
				for (int i=0; i<attacker.moves.Length; i++) {
					if (attacker.moves[i].id == opponent.lastMoveUsed) {
						score -= 90;
					}
				}
				break;
			case 0x5D:
				blacklist = new int[3]{0x02,0x14,0x5D};
				if (attacker.effects[Effects.Transform] != 0 || opponent.lastMoveUsedSketch <= 0 || Array.IndexOf(blacklist, BattleMove.FromBattleMove(this, new Moves.Move(opponent.lastMoveUsedSketch)).function) > -1) {
					score -= 90;
				}
				for (int i=0; i<attacker.moves.Length; i++) {
					if (attacker.moves[i].id == opponent.lastMoveUsedSketch) {
						score -= 90;
					}
				}
				break;
			case 0x5E:
				if (attacker.ability == Abilities.MULTITYPE) {
					score -= 90;
				} else {
					List<int> types = new List<int>();
					for (int i=0; i<attacker.moves.Length; i++) {
						if (attacker.moves[i].id == move.id) {
							continue;
						}
						if (Types.IsPseudoType(attacker.moves[i].type)) {
							continue;
						}
						if (attacker.HasType(attacker.moves[i].type)) {
							continue;
						}
						if (!types.Contains(attacker.moves[i].type)) {
							types.Add(attacker.moves[i].type);
						}
					}
					if (types.Count == 0) {
						score -= 90;
					}
				}
				break;
			case 0x5F:
				if (attacker.ability == Abilities.MULTITYPE) {
					score -= 90;
				} else if (opponent.lastMoveUsed <= 0 || Types.IsPseudoType(BattleMove.FromBattleMove(this, new Moves.Move(opponent.lastMoveUsed)).type)) {
					score -= 90;
				} else {
					int atype = -1;
					for (int i=0; i<opponent.moves.Length; i++) {
						if (opponent.moves[i].id == opponent.lastMoveUsed) {
							atype = opponent.moves[i].GetType(move.type, attacker, opponent);
							break;
						}
					}
					if (atype < 0) {
						score -= 90;
					} else {
						List<int> types = new List<int>();
						for (int i=0; i<Types.MaxValue(); i++) {
							if (attacker.HasType(i)) {
								continue;
							}
							if (Types.GetEffectiveness(atype, i) < 2) {
								types.Add(i);
							}
						}
						if (types.Count == 0) {
							score -= 90;
						}
					}
				}
				break;
			case 0x60:
				if (attacker.ability == Abilities.MULTITYPE) {
					score -= 90;
				} else if (skill >= TrainerAI.mediumSkill) {
					int[] envtypes = new int[9]{Types.NORMAL, Types.GRASS, Types.GRASS, Types.WATER, Types.WATER, Types.WATER, Types.ROCK, Types.ROCK, Types.STEEL};
					int type = envtypes[environment];
					if (attacker.HasType(type)) {
						score -= 90;
					}
				}
				break;
			case 0x61:
				if (opponent.effects[Effects.Substitute] > 0 || opponent.ability == Abilities.MULTITYPE) {
					score -= 90;
				} else if (opponent.HasType(Types.WATER)) {
					score -= 90;
				}
				break;
			case 0x62:
				if (attacker.ability == Abilities.MULTITYPE) {
					score -= 90;
				} else if (attacker.HasType(opponent.type1) && attacker.HasType(opponent.type2) && opponent.HasType(attacker.type1) && opponent.HasType(attacker.type2)) {
					score -= 90;
				}
				break;
			case 0x63:
				if (opponent.effects[Effects.Substitute] > 0) {
					score -= 90;
				} else if (skill >= TrainerAI.mediumSkill) {
					if (opponent.ability == Abilities.MULTITYPE || opponent.ability == Abilities.SIMPLE || opponent.ability == Abilities.TRUANT) {
						score -= 90;
					}
				}
				break;
			case 0x64:
				if (opponent.effects[Effects.Substitute] > 0) {
					score -= 90;
				} else if (skill >= TrainerAI.mediumSkill) {
					if (opponent.ability == Abilities.MULTITYPE || opponent.ability == Abilities.INSOMNIA || opponent.ability == Abilities.TRUANT) {
						score -= 90;
					}
				}
				break;
			case 0x65:
				score -= 40;
				if (skill >= TrainerAI.mediumSkill) {
					if (opponent.ability == 0 || attacker.ability == opponent.ability || attacker.ability == Abilities.MULTITYPE || opponent.ability == Abilities.FLOWERGIFT || opponent.ability == Abilities.FORECAST || opponent.ability == Abilities.ILLUSION || opponent.ability == Abilities.IMPOSTER || opponent.ability == Abilities.MULTITYPE || opponent.ability == Abilities.TRACE || opponent.ability == Abilities.WONDERGUARD || opponent.ability == Abilities.ZENMODE) {
						score -= 90;
					}
				}
				if (skill >= TrainerAI.highSkill) {
					if (opponent.ability == Abilities.TRUANT && attacker.IsOpposing(opponent.index)) {
						score -= 90;
					}
					if (opponent.ability == Abilities.SLOWSTART && attacker.IsOpposing(opponent.index)) {
						score -= 90;
					}
				}
				break;
			case 0x66:
				score -= 40;
				if (skill >= TrainerAI.mediumSkill) {
					if (opponent.ability == 0 || attacker.ability == opponent.ability || opponent.ability == Abilities.MULTITYPE || opponent.ability == Abilities.TRUANT || attacker.ability == Abilities.FLOWERGIFT || attacker.ability == Abilities.FORECAST || attacker.ability == Abilities.ILLUSION || attacker.ability == Abilities.IMPOSTER || attacker.ability == Abilities.MULTITYPE || attacker.ability == Abilities.TRACE || attacker.ability == Abilities.WONDERGUARD || opponent.ability == Abilities.ZENMODE) {
						score -= 90;
					}
				}
				if (skill >= TrainerAI.highSkill) {
					if (opponent.ability == Abilities.TRUANT && attacker.IsOpposing(opponent.index)) {
						score += 90;
					}
					if (opponent.ability == Abilities.SLOWSTART && attacker.IsOpposing(opponent.index)) {
						score += 90;
					}
				}
				break;
			case 0x67:
				score -= 40;
				if (skill >= TrainerAI.mediumSkill) {
					if ((opponent.ability == 0 && attacker.ability == 0) || attacker.ability == opponent.ability || attacker.ability == Abilities.ILLUSION || opponent.ability == Abilities.ILLUSION || attacker.ability == Abilities.MULTITYPE || opponent.ability == Abilities.MULTITYPE || attacker.ability == Abilities.WONDERGUARD || opponent.ability == Abilities.WONDERGUARD) {
						score -= 90;
					}
				}
				if (skill >= TrainerAI.highSkill) {
					if (opponent.ability == Abilities.TRUANT && attacker.IsOpposing(opponent.index)) {
						score -= 90;
					}
					if (opponent.ability == Abilities.SLOWSTART && attacker.IsOpposing(opponent.index)) {
						score -= 90;
					}
				}
				break;
			case 0x68:
				if (opponent.effects[Effects.Substitute] > 0 || opponent.effects[Effects.GastroAcid] != 0) {
					score -= 90;
				} else if (skill >= TrainerAI.highSkill) {
					if (opponent.ability == Abilities.MULTITYPE) {
						score -= 90;
					}
					if (opponent.ability == Abilities.SLOWSTART) {
						score -= 90;
					}
					if (opponent.ability == Abilities.TRUANT) {
						score -= 90;
					}
				}
				break;
			case 0x69:
				score -= 70;
				break;
			case 0x6A:
				if (opponent.hp <= 20) {
					score += 80;
				} else if (opponent.level >= 25) {
					score -= 80;
				}
				break;
			case 0x6B:
				if (opponent.hp <= 40) {
					score += 80;
				}
				break;
			case 0x6C:
				score -= 50;
				score += (opponent.hp*100/opponent.totalHP);
				break;
			case 0x6D:
				if (opponent.hp <= attacker.level) {
					score += 80;
				}
				break;
			case 0x6E:
				if (attacker.hp >= opponent.hp) {
					score -= 90;
				} else if (attacker.hp*2 < opponent.hp) {
					score += 50;
				}
				break;
			case 0x6F:
				if (opponent.hp <= attacker.level) {
					score += 30;
				}
				break;
			case 0x70:
				if (opponent.HasWorkingAbility(Abilities.STURDY)) {
					score -= 90;
				}
				if (opponent.level > attacker.level) {
					score -= 90;
				}
				break;
			case 0x71:
				if (opponent.effects[Effects.HyperBeam]>0) {
					score -= 90;
				} else {
					int attack = RoughStat(attacker, Stats.ATTACK, skill);
					int spatk = RoughStat(attacker, Stats.SPATK, skill);
					if (attack*1.5<spatk) {
						score -= 60;
					} else if (skill >= TrainerAI.mediumSkill && opponent.lastMoveUsed > 0) {
						Moves.InternalMove moveData = new Moves.Move(opponent.lastMoveUsed).moveData;
						if (moveData.Power > 0 && (Settings.USE_MOVE_CATEGORY && moveData.Category ==  "Physical") || (!Settings.USE_MOVE_CATEGORY && Types.IsSpecialType(Types.GetValueFromName(moveData.Type)))) {
							score -= 60;
						}
					}
				}
				break;
			case 0x72:
				if (opponent.effects[Effects.HyperBeam]>0) {
					score -= 90;
				} else {
					int attack = RoughStat(attacker, Stats.ATTACK, skill);
					int spatk = RoughStat(attacker, Stats.SPATK, skill);
					if (attack>spatk*1.5) {
						score -= 60;
					} else if (skill >= TrainerAI.mediumSkill && opponent.lastMoveUsed > 0) {
						Moves.InternalMove moveData = new Moves.Move(opponent.lastMoveUsed).moveData;
						if (moveData.Power > 0 && (Settings.USE_MOVE_CATEGORY && moveData.Category == "Special") || (!Settings.USE_MOVE_CATEGORY && !Types.IsSpecialType(Types.GetValueFromName(moveData.Type)))) {
							score -= 60;
						}
					}
				}
				break;
			case 0x73:
				if (opponent.effects[Effects.HyperBeam] > 0) {
					score -= 90;
				}
				break;
			case 0x74:
				if (!opponent.Partner().Fainted()) {
					score += 10;
				}
				break;
			case 0x75:
			case 0x76:
			case 0x77:
			case 0x78:
				if (skill >= TrainerAI.highSkill) {
					if (!opponent.HasWorkingAbility(Abilities.INNERFOCUS) && opponent.effects[Effects.Substitute] == 0) {
						score += 30;
					}
				}
				break;
			case 0x79:
			case 0x7A:
			case 0x7B:
			case 0x7C:
				if (opponent.status == Statuses.PARALYSIS) {
					score -= 20;
				}
				break;
			case 0x7D:
				if (opponent.status == Statuses.SLEEP && opponent.statusCount > 1) {
					score -= 20;
				}
				break;
			case 0x7E:
			case 0x7F:
			case 0x80:
			case 0x81:
				aspeed = RoughStat(attacker, Stats.SPEED, skill);
				ospeed = RoughStat(opponent, Stats.SPEED, skill);
				if (ospeed > aspeed) {
					score += 30;
				}
				break;
			case 0x82:
				if (doublebattle) {
					score += 20;
				}
				break;
			case 0x83:
				if (skill >= TrainerAI.mediumSkill) {
					if (doublebattle && !attacker.Partner().Fainted() && attacker.Partner().HasMove(move.id)) {
						score += 20;
					}
				}
				break;
			case 0x84:
				aspeed = RoughStat(attacker, Stats.SPEED, skill);
				ospeed = RoughStat(opponent, Stats.SPEED, skill);
				if (ospeed > aspeed) {
					score += 30;
				}
				break;
			case 0x85:
			case 0x86:
			case 0x87:
			case 0x88:
			case 0x89:
			case 0x8A:
			case 0x8B:
			case 0x8C:
			case 0x8D:
			case 0x8E:
			case 0x8F:
			case 0x90:
			case 0x91:
			case 0x92:
			case 0x93:
				if (attacker.effects[Effects.Rage] != 0) {
					score += 25;
				}
				break;
			case 0x94:
			case 0x95:
			case 0x96:
				if (!Items.IsBerry(attacker.item)) {
					score -= 90;
				}
				break;
			case 0x97:
			case 0x98:
			case 0x99:
			case 0x9A:
			case 0x9B:
			case 0x9C:
				if (attacker.Partner().Fainted()) {
					score -= 90;
				}
				break;
			case 0x9D:
				if (attacker.effects[Effects.MudSport] != 0) {
					score -= 90;
				}
				break;
			case 0x9E:
				if (attacker.effects[Effects.WaterSport] != 0) {
					score -= 90;
				}
				break;
			case 0x9F:
			case 0xA0:
			case 0xA1:
				if (attacker.OwnSide().effects[Effects.LuckyChant] > 0) {
					score -= 90;
				}
				break;
			case 0xA2:
				if (attacker.OwnSide().effects[Effects.Reflect] > 0) {
					score -= 90;
				}
				break;
			case 0xA3:
				if (attacker.OwnSide().effects[Effects.LightScreen] > 0) {
					score -= 90;
				}
				break;
			case 0xA4:
			case 0xA5:
			case 0xA6:
				if (opponent.effects[Effects.Substitute] > 0) {
					score -= 90;
				}
				if (opponent.effects[Effects.LockOn] > 0) {
					score -= 90;
				}
				break;
			case 0xA7:
				if (opponent.effects[Effects.Foresight] != 0) {
					score -= 90;
				} else if (opponent.HasType(Types.GHOST)) {
					score += 70;
				} else if (opponent.stages[Stats.EVASION] <= 0) {
					score -= 60;
				}
				break;
			case 0xA8:
				if (opponent.effects[Effects.MiracleEye] != 0) {
					score -= 90;
				} else if (opponent.HasType(Types.DARK)) {
					score += 70;
				} else if (opponent.stages[Stats.EVASION] <= 0) {
					score -= 60;
				}
				break;
			case 0xA9:
			case 0xAA:
				if (attacker.effects[Effects.ProtectRate] > 1 || opponent.effects[Effects.HyperBeam] > 0) {
					score -= 90;
				} else {
					if (skill >= TrainerAI.mediumSkill) {
						score -= (attacker.effects[Effects.ProtectRate]*40);
					}
					if (attacker.turnCount == 0) {
						score += 50;
					}
					if (opponent.effects[Effects.TwoTurnAttack] != 0) {
						score += 30;
					}
				}
				break;
			case 0xAB:
			case 0xAC:
			case 0xAD:
			case 0xAE:
				score -= 40;
				if (skill >= TrainerAI.highSkill) {
					if (opponent.lastMoveUsed <= 0 || ((new Moves.Move(opponent.lastMoveUsed).flags)&0x10) == 0) {
						score -= 100;
					}
				}
				break;
			case 0xAF:
			case 0xB0:
			case 0xB1:
			case 0xB2:
			case 0xB3:
			case 0xB4:
				if (attacker.status == Statuses.SLEEP) {
					score += 200;
				} else {
					score -= 80;
				}
				break;
			case 0xB5:
			case 0xB6:
			case 0xB7:
				if (opponent.effects[Effects.Torment] != 0) {
					score -= 90;
				}
				break;
			case 0xB8:
				if (opponent.effects[Effects.Imprison] != 0) {
					score -= 90;
				}
				break;
			case 0xB9:
				if (opponent.effects[Effects.Disable] > 0) {
					score -= 90;
				}
				break;
			case 0xBA:
				if (opponent.effects[Effects.Taunt] > 0) {
					score -= 90;
				}
				break;
			case 0xBB:
				if (opponent.effects[Effects.HealBlock] > 0) {
					score -= 90;
				}
				break;
			case 0xBC:
				aspeed = RoughStat(attacker, Stats.SPEED, skill);
				ospeed = RoughStat(opponent, Stats.SPEED, skill);
				if (opponent.effects[Effects.Encore] != 0) {
					score -= 90;
				} else if (aspeed > ospeed) {
					if (opponent.lastMoveUsed <= 0) {
						score -= 90;
					} else {
						BattleMove moveData = BattleMove.FromBattleMove(this, new Moves.Move(opponent.lastMoveUsed));
						if (moveData.baseDamage == 0 && (moveData.target == 0x10 || moveData.target == 0x20)) {
							score += 60;
						} else if (moveData.baseDamage != 0 && moveData.target == 0x00 && TypeModifier(moveData.type, opponent, attacker) == 0) {
							score += 60;
						}
					}
				}
				break;
			case 0xBD:
			case 0xBF:
			case 0xC0:
			case 0xC1:
			case 0xC2:
			case 0xC3:
			case 0xC4:
			case 0xC7:
				if (attacker.effects[Effects.FocusEnergy] > 0) {
					score += 20;
				}
				if (skill >= TrainerAI.highSkill) {
					if (!opponent.HasWorkingAbility(Abilities.INNERFOCUS) && opponent.effects[Effects.Substitute]==0) {
						score += 20;
					}
				}
				break;
			case 0xC9:
			case 0xCA:
			case 0xCB:
			case 0xCC:
			case 0xCD:
			case 0xCE:
			case 0xCF:
				if (opponent.effects[Effects.MultiTurn] == 0) {
					score += 40;
				}
				break;
			case 0xD0:
				if (opponent.effects[Effects.MultiTurn] == 0) {
					score += 40;
				}
				break;
			case 0xD1:
			case 0xD2:
			case 0xD3:
			case 0xD4:
				if (attacker.hp <= attacker.totalHP/4) {
					score -= 90;
				} else if (attacker.hp <= attacker.totalHP/2) {
					score -= 50;
				}
				break;
			case 0xD5:
			case 0xD6:
				if (attacker.hp == attacker.totalHP) {
					score -= 90;
				} else {
					score += 50;
					score -= (attacker.hp*100/attacker.totalHP);
				}
				break;
			case 0xD7:
				if (attacker.effects[Effects.Wish]>0) {
					score -= 90;
				}
				break;
			case 0xD8:
				if (attacker.hp == attacker.totalHP) {
					score -= 90;
				} else {
					switch (GetWeather()) {
						case Weather.SUNNYDAY:
							score += 30;
							break;
						case Weather.RAINDANCE:
						case Weather.SANDSTORM:
						case Weather.HAIL:
							score -= 30;
							break;
					}
					score += 50;
					score -= (attacker.hp*100/attacker.totalHP);
				}
				break;
			case 0xD9:
				if (attacker.hp == attacker.totalHP || !attacker.CanSleep(attacker, false, null, true)) {
					score -= 90;
				} else {
					score += 70;
					score -= (attacker.hp*140/attacker.totalHP);
					if (attacker.status != 0) {
						score += 30;
					}
				}
				break;
			case 0xDA:
				if (attacker.effects[Effects.AquaRing] != 0) {
					score -= 90;
				}
				break;
			case 0xDB:
				if (attacker.effects[Effects.Ingrain] != 0) {
					score -= 90;
				}
				break;
			case 0xDC:
				if (opponent.effects[Effects.LeechSeed] >= 0) {
					score -= 90;
				} else if (skill >= TrainerAI.mediumSkill && opponent.HasType(Types.GRASS)) {
					score -= 90;
				} else {
					if (attacker.turnCount == 0) {
						score += 60;
					}
				}
				break;
			case 0xDD:
				if (skill >= TrainerAI.highSkill && opponent.HasWorkingAbility(Abilities.LIQUIDOOZE)) {
					score -= 70;
				} else {
					if (attacker.hp <= (attacker.totalHP/2)) {
						score += 20;
					}
				}
				break;
			case 0xDE:
				if (opponent.status != Statuses.SLEEP) {
					score -= 100;
				} else if (skill >= TrainerAI.highSkill && opponent.HasWorkingAbility(Abilities.LIQUIDOOZE)) {
					score -= 70;
				} else {
					if (attacker.hp <= (attacker.totalHP/2)) {
						score += 20;
					}
				}
				break;
			case 0xDF:
				if (attacker.IsOpposing(opponent.index)) {
					score -= 100;
				} else {
					if (attacker.hp <= (attacker.totalHP/2) && opponent.effects[Effects.Substitute] == 0) {
						score += 20;
					}
				}
				break;
			case 0xE0:
				int reserves = attacker.NonActivePokemonCount();
				int foes = attacker.OppositeOpposing().NonActivePokemonCount();
				if (CheckGlobalAbility(Abilities.DAMP)) {
					score -= 100;
				} else if (skill >= TrainerAI.mediumSkill && reserves == 0 && foes > 0) {
					score -= 100;
				} else if (skill >= TrainerAI.highSkill && reserves == 0 && foes == 0) {
					score -= 100;
				} else {
					score -= (attacker.hp*100/attacker.totalHP);
				}
				break;
			case 0xE1:
			case 0xE2:
				if (!opponent.CanReduceStatStage(Stats.ATTACK, attacker) && !opponent.CanReduceStatStage(Stats.SPATK, attacker)) {
					score -= 100;
				} else if (attacker.NonActivePokemonCount() == 0) {
					score -= 100;
				} else {
					score += attacker.stages[Stats.ATTACK]*10;
					score += attacker.stages[Stats.SPATK]*10;
					score -= (attacker.hp*100/attacker.totalHP);
				}
				break;
			case 0xE3:
			case 0xE4:
				score -= 70;
				break;
			case 0xE5:
				if (attacker.NonActivePokemonCount() == 0) {
					score -= 90;
				} else {
					if (opponent.effects[Effects.PerishSong] > 0) {
						score -= 90;
					}
				}
				break;
			case 0xE6:
				score += 50;
				score -= (attacker.hp*100/attacker.totalHP);
				if (attacker.hp <= (attacker.totalHP/10)) {
					score += 10;
				}
				break;
			case 0xE7:
				score += 50;
				score -= (attacker.hp*100/attacker.totalHP);
				if (attacker.hp <= (attacker.totalHP/10)) {
					score += 10;
				}
				break;
			case 0xE8:
				if (attacker.hp > (attacker.totalHP/2)) {
					score -= 25;
				}
				if (skill >= TrainerAI.mediumSkill) {
					if (attacker.effects[Effects.ProtectRate] > 1) {
						score -= 90;
					}
					if (attacker.effects[Effects.HyperBeam] > 0) {
						score -= 90;
					}
				} else {
					score -= (attacker.effects[Effects.ProtectRate]*40);
				}
				break;
			case 0xE9:
				if (opponent.hp == 1) {
					score -= 90;
				} else if (opponent.hp <= (opponent.totalHP/8)) {
					score -= 60;
				} else if (opponent.hp <= (opponent.totalHP/4)) {
					score -= 30;
				}
				break;
			case 0xEA:
				if (opponent != null) {
					score -= 100;
				}
				break;
			case 0xEB:
				if (opponent.effects[Effects.Ingrain] != 0 || (skill >= TrainerAI.highSkill && opponent.HasWorkingAbility(Abilities.SUCTIONCUPS))) {
					score -= 90;
				} else {
					Battler[] p = Party(opponent.index);
					int ch = 0;
					for (int i=0; i<p.Length; i++) {
						if (CanSwitchLax(opponent.index, i, false)) {
							ch++;
						}
					}
					if (ch == 0) {
						score -= 90;
					}
				}
				if (score > 20) {
					if (opponent.OwnSide().effects[Effects.Spikes] > 0) {
						score += 50;
					}
					if (opponent.OwnSide().effects[Effects.ToxicSpikes] > 0) {
						score += 50;
					}
					if (opponent.OwnSide().effects[Effects.StealthRock] != 0) {
						score += 50;
					}
				}
				break;
			case 0xEC:
				if (opponent.effects[Effects.Ingrain] == 0 && !(skill >= TrainerAI.highSkill && opponent.HasWorkingAbility(Abilities.SUCTIONCUPS))) {
					if (opponent.OwnSide().effects[Effects.Spikes] > 0) {
						score += 40;
					}
					if (opponent.OwnSide().effects[Effects.ToxicSpikes] > 0) {
						score += 40;
					}
					if (opponent.OwnSide().effects[Effects.StealthRock] != 0) {
						score += 40;
					}
				}
				break;
			case 0xED:
				if (!CanChooseNonActive(attacker.index)) {
					score -= 80;
				} else {
					if (attacker.effects[Effects.Confusion] > 0) {
						score -= 40;
						int total = 0;
						total += attacker.stages[Stats.ATTACK]*10;
						total += attacker.stages[Stats.DEFENSE]*10;
						total += attacker.stages[Stats.SPEED]*10;
						total += attacker.stages[Stats.SPATK]*10;
						total += attacker.stages[Stats.SPDEF]*10;
						total += attacker.stages[Stats.EVASION]*10;
						total += attacker.stages[Stats.ACCURACY]*10;
						if (total <= 0 || attacker.turnCount==0) {
							score -= 60;
						} else {
							score += total;
							bool hasDamagingAttack = false;
							for (int i=0; i<attacker.moves.Length; i++) {
								if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0) {
									hasDamagingAttack = true;
									break;
								}
							}
							if (!hasDamagingAttack) {
								score += 75;
							}
						}
					}
				}
				break;
			case 0xEE:
			case 0xEF:
				if (opponent.effects[Effects.MeanLook] >= 0) {
					score -= 90;
				}
				break;
			case 0xF0:
				if (skill >= TrainerAI.highSkill) {
					if (opponent.item != 0) {
						score += 20;
					}
				}
				break;
			case 0xF1:
				if (skill >= TrainerAI.highSkill) {
					if (attacker.item == 0 && opponent.item != 0) {
						score += 40;
					} else {
						score -= 90;
					}
				} else {
					score -= 80;
				}
				break;
			case 0xF2:
				if (attacker.item == 0 && opponent.item == 0) {
					score -= 90;
				} else if (skill >= TrainerAI.highSkill && opponent.HasWorkingAbility(Abilities.STICKYHOLD)) {
					score -= 90;
				} else if (attacker.HasWorkingItem(Items.FLAMEORB) || attacker.HasWorkingItem(Items.TOXICORB) || attacker.HasWorkingItem(Items.STICKYBARB) || attacker.HasWorkingItem(Items.IRONBALL) || attacker.HasWorkingItem(Items.CHOICEBAND) || attacker.HasWorkingItem(Items.CHOICESCARF) || attacker.HasWorkingItem(Items.CHOICESPECS)) {
					score += 50;
				} else if (attacker.item == 0 || opponent.item != 0) {
					if (BattleMove.FromBattleMove(this, new Moves.Move(attacker.lastMoveUsed)).function == 0xF2) {
						score -= 30;
					}
				}
				break;
			case 0xF3:
				if (attacker.item == 0 && opponent.item == 0) {
					score -= 90;
				} else if (attacker.HasWorkingItem(Items.FLAMEORB) || attacker.HasWorkingItem(Items.TOXICORB) || attacker.HasWorkingItem(Items.STICKYBARB) || attacker.HasWorkingItem(Items.IRONBALL) || attacker.HasWorkingItem(Items.CHOICEBAND) || attacker.HasWorkingItem(Items.CHOICESCARF) || attacker.HasWorkingItem(Items.CHOICESPECS)) {
					score += 50;
				} else {
					score -= 80;
				}
				break;
			case 0xF4:
			case 0xF5:
				if (opponent.effects[Effects.Substitute] == 0) {
					if (skill >= TrainerAI.highSkill && Items.IsBerry(opponent.item)) {
						score += 30;
					}
				}
				break;
			case 0xF6:
				if (attacker.pokemon.itemRecycle == 0 || attacker.item != 0) {
					score -= 80;
				} else if (attacker.pokemon.itemRecycle != 0) {
					score += 30;
				}
				break;
			case 0xF7:
				if (attacker.item == 0 || IsUnlosableItem(attacker, attacker.item) || Items.IsPokeball(attacker.item) || attacker.HasWorkingAbility(Abilities.KLUTZ) || attacker.effects[Effects.Embargo] > 0) {
					score -= 90;
				}
				break;
			case 0xF8:
				if (opponent.effects[Effects.Embargo] > 0) {
					score -= 90;
				}
				break;
			case 0xF9:
				if (field.effects[Effects.MagicRoom] > 0) {
					score -= 90;
				} else {
					if (attacker.item == 0 && opponent.item != 0) {
						score += 30;
					}
				}
				break;
			case 0xFA:
				score -= 25;
				break;
			case 0xFB:
				score -= 30;
				break;
			case 0xFC:
				score -= 40;
				break;
			case 0xFD:
				score -= 30;
				if (opponent.CanParalyze(attacker, false)) {
					score += 30;
					if (skill >= TrainerAI.mediumSkill) {
						aspeed = RoughStat(attacker, Stats.SPEED, skill);
						ospeed = RoughStat(opponent, Stats.SPEED, skill);
						if (aspeed < ospeed) {
							score += 30;
						} else if (aspeed > ospeed) {
							score -= 40;
						}
					}
					if (skill >= TrainerAI.highSkill) {
						if (opponent.HasWorkingAbility(Abilities.GUTS)) {
							score -= 40;
						}
						if (opponent.HasWorkingAbility(Abilities.MARVELSCALE)) {
							score -= 40;
						}
						if (opponent.HasWorkingAbility(Abilities.QUICKFEET)) {
							score -= 40;
						}
					}
				}
				break;
			case 0xFE:
				score -= 30;
				if (opponent.CanBurn(attacker, false)) {
					score += 30;
					if (skill >= TrainerAI.highSkill) {
						if (opponent.HasWorkingAbility(Abilities.GUTS)) {
							score -= 40;
						}
						if (opponent.HasWorkingAbility(Abilities.MARVELSCALE)) {
							score -= 40;
						}
						if (opponent.HasWorkingAbility(Abilities.QUICKFEET)) {
							score -= 40;
						}
						if (opponent.HasWorkingAbility(Abilities.FLAREBOOST)) {
							score -= 40;
						}
					}
				}
				break;
			case 0xFF:
				if (CheckGlobalAbility(Abilities.AIRLOCK) || CheckGlobalAbility(Abilities.CLOUDNINE)) {
					score -= 90;
				} else if (GetWeather() == Weather.SUNNYDAY) {
					score -= 90;
				} else {
					for (int i=0; i<attacker.moves.Length; i++) {
						if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].type == Types.FIRE) {
							score += 20;
						}
					}
				}
				break;
			case 0x100:
				if (CheckGlobalAbility(Abilities.AIRLOCK) || CheckGlobalAbility(Abilities.CLOUDNINE)) {
					score -= 90;
				} else if (GetWeather() == Weather.RAINDANCE) {
					score -= 90;
				} else {
					for (int i=0; i<attacker.moves.Length; i++) {
						if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].type == Types.WATER) {
							score += 20;
						}
					}
				}
				break;
			case 0x101:
				if (CheckGlobalAbility(Abilities.AIRLOCK) || CheckGlobalAbility(Abilities.CLOUDNINE)) {
					score -= 90;
				} else if (GetWeather() == Weather.SANDSTORM) {
					score -= 90;
				}
				break;
			case 0x102:
				if (CheckGlobalAbility(Abilities.AIRLOCK) || CheckGlobalAbility(Abilities.CLOUDNINE)) {
					score -= 90;
				} else if (GetWeather() == Weather.HAIL) {
					score -= 90;
				}
				break;
			case 0x103:
				if (attacker.OpposingSide().effects[Effects.Spikes] >= 3) {
					score -= 90;
				} else if (!CanChooseNonActive(attacker.Opposing1().index) && !CanChooseNonActive(attacker.Opposing2().index)) {
					score -= 90;
				} else {
					score += 5*attacker.OppositeOpposing().NonActivePokemonCount();
					if (attacker.OpposingSide().effects[Effects.Spikes] == 0) {
						score += 40;
					} else if (attacker.OpposingSide().effects[Effects.Spikes] == 1) {
						score += 26;
					} else if (attacker.OpposingSide().effects[Effects.Spikes] == 2) {
						score += 13;
					}
				}
				break;
			case 0x104:
				if (attacker.OpposingSide().effects[Effects.ToxicSpikes] >= 2) {
					score -= 90;
				} else if (!CanChooseNonActive(attacker.Opposing1().index) && !CanChooseNonActive(attacker.Opposing2().index)) {
					score -= 90;
				} else {
					score += 4*attacker.OppositeOpposing().NonActivePokemonCount();
					if (attacker.OpposingSide().effects[Effects.ToxicSpikes] == 0) {
						score += 26;
					} else if (attacker.OpposingSide().effects[Effects.ToxicSpikes] == 1) {
						score += 13;
					}
				}
				break;
			case 0x105:
				if (attacker.OpposingSide().effects[Effects.ToxicSpikes] >= 2) {
					score -= 90;
				} else if (!CanChooseNonActive(attacker.Opposing1().index) && !CanChooseNonActive(attacker.Opposing2().index)) {
					score -= 90;
				} else {
					score += 5*attacker.OppositeOpposing().NonActivePokemonCount();
				}
				break;
			case 0x106:
			case 0x107:
			case 0x108:
			case 0x109:
			case 0x10A:
				if (attacker.OpposingSide().effects[Effects.Reflect] > 0) {
					score += 20;
				}
				if (attacker.OpposingSide().effects[Effects.LightScreen] > 0) {
					score += 20;
				}
				break;
			case 0x10B:
				score += 10*(attacker.stages[Stats.ACCURACY] - opponent.stages[Stats.EVASION]);
				break;
			case 0x10C:
				if (attacker.effects[Effects.Substitute] > 0) {
					score -= 90;
				} else if (attacker.hp <= attacker.totalHP/4) {
					score -= 90;
				}
				break;
			case 0x10D:
				if (attacker.HasType(Types.GHOST)) {
					if (opponent.effects[Effects.Curse] != 0) {
						score -= 90;
					} else if (attacker.hp <= (attacker.totalHP/2)) {
						if (attacker.NonActivePokemonCount() == 0) {
							score -= 90;
						} else {
							score -= 50;
							if (shiftStyle) {
								score -= 30;
							}
						}
					}
				} else {
					avg = attacker.stages[Stats.SPEED]*10;
					avg -= attacker.stages[Stats.ATTACK]*10;
					avg -= attacker.stages[Stats.DEFENSE]*10;
					score += avg/3;
				}
				break;
			case 0x10E:
				score -= 40;
				break;
			case 0x10F:
				if (opponent.effects[Effects.Nightmare] != 0 || opponent.effects[Effects.Substitute] > 0) {
					score -= 90;
				} else if (opponent.status != Statuses.SLEEP) {
					score -= 90;
				} else {
					if (opponent.statusCount <= 1) {
						score -= 90;
					}
					if (opponent.statusCount > 3) {
						score += 50;
					}
				}
				break;
			case 0x110:
				if (attacker.effects[Effects.MultiTurn] > 0) {
					score += 30;
				}
				if (attacker.effects[Effects.LeechSeed] > 0) {
					score += 30;
				}
				if (attacker.NonActivePokemonCount() > 0) {
					if (attacker.OwnSide().effects[Effects.Spikes] > 0) {
						score += 80;
					}
					if (attacker.OwnSide().effects[Effects.ToxicSpikes] > 0) {
						score += 80;
					}
					if (attacker.OwnSide().effects[Effects.StealthRock] != 0) {
						score += 80;
					}
				}
				break;
			case 0x111:
				if (opponent.effects[Effects.FutureSight] > 0) {
					score -= 100;
				} else if (attacker.NonActivePokemonCount() == 0) {
					score -= 70;
				}
				break;
			case 0x112:
				avg = 0;
				avg -= attacker.stages[Stats.DEFENSE]*10;
				avg -= attacker.stages[Stats.SPDEF]*10;
				score += avg/2;
				if (attacker.effects[Effects.Stockpile] >= 3) {
					score -= 80;
				} else {
					for (int i=0; i<attacker.moves.Length; i++) {
						if (attacker.moves[i].function == 0x113 || move.function == 0x114) {
							score += 20;
							break;
						}
					}
				}
				break;
			case 0x113:
				if (attacker.effects[Effects.Stockpile] == 0) {
					score -= 100;
				}
				break;
			case 0x114:
				if (attacker.effects[Effects.Stockpile] == 0) {
					score -= 90;
				} else if (attacker.hp == attacker.totalHP) {
					score -= 90;
				} else {
					switch (attacker.effects[Effects.Stockpile]) {
						case 1:
							score += 25;
							score -= (attacker.hp*25*2/attacker.totalHP);
							break;
						case 2:
							score += 50;
							score -= (attacker.hp*50*2/attacker.totalHP);
							break;
						case 3:
							score += 100;
							score -= (attacker.hp*100*2/attacker.totalHP);
							break;
					}
				}
				break;
			case 0x115:
				if (opponent.effects[Effects.HyperBeam] > 0) {
					score += 50;
				}
				if (opponent.hp <= (opponent.totalHP/2)) {
					score -= 35;
				}
				if (opponent.hp <= (opponent.totalHP/4)) {
					score -= 70;
				}
				break;
			case 0x116:
			case 0x117:
				if (!doublebattle) {
					score -= 100;
				} else if (attacker.Partner().Fainted()) {
					score -= 90;
				}
				break;
			case 0x118:
				if (field.effects[Effects.Gravity] > 0) {
					score -= 90;
				} else if (skill >= TrainerAI.mediumSkill) {
					if (skill >= TrainerAI.mediumSkill) {
						if (attacker.effects[Effects.SkyDrop] != 0) {
							score -= 20;
						}
						if (attacker.effects[Effects.MagnetRise] > 0) {
							score -= 20;
						}
						if (attacker.effects[Effects.Telekinesis] > 0) {
							score -= 20;
						}
						if (attacker.HasType(Types.FLYING)) {
							score -= 20;
						}
						if (attacker.HasWorkingAbility(Abilities.LEVITATE)) {
							score -= 20;
						}
						if (attacker.HasWorkingItem(Items.AIRBALLOON)) {
							score -= 20;
						}
						if (opponent.effects[Effects.SkyDrop] != 0) {
							score += 20;
						}
						if (opponent.effects[Effects.MagnetRise] > 0) {
							score += 20;
						}
						if (opponent.effects[Effects.Telekinesis] > 0) {
							score += 20;
						}
						if (BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function == 0xC9 || BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function == 0xCC || BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function == 0xCE) {
							score += 20;
						}
						if (opponent.HasType(Types.FLYING)) {
							score += 20;
						}
						if (opponent.HasWorkingAbility(Abilities.LEVITATE)) {
							score += 20;
						}
						if (opponent.HasWorkingItem(Items.AIRBALLOON)) {
							score += 20;
						}
					}
				}
				break;
			case 0x119:
				if (opponent.effects[Effects.MagnetRise] > 0 || opponent.effects[Effects.Ingrain] != 0 || opponent.effects[Effects.SmackDown] != 0) {
					score -= 90;
				}
				break;
			case 0x11A:
				if (opponent.effects[Effects.Telekinesis] > 0 || opponent.effects[Effects.Ingrain] != 0 || opponent.effects[Effects.SmackDown] != 0) {
					score -= 90;
				}
				break;
			case 0x11B:
			case 0x11C:
				if (skill >= TrainerAI.mediumSkill) {
					if (opponent.effects[Effects.MagnetRise] > 0) {
						score += 20;
					}
					if (opponent.effects[Effects.Telekinesis] > 0) {
						score += 20;
					}
					if (BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function == 0xC9 || BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function == 0xCC) {
						score += 20;
					}
					if (opponent.HasType(Types.FLYING)) {
						score += 20;
					}
					if (opponent.HasWorkingAbility(Abilities.LEVITATE)) {
						score += 20;
					}
					if (opponent.HasWorkingItem(Items.AIRBALLOON)) {
						score += 20;
					}
				}
				break;
			case 0x11D:
			case 0x11E:
			case 0x11F:
			case 0x120:
			case 0x121:
			case 0x122:
			case 0x123:
				if (!opponent.HasType(attacker.type1) && !opponent.HasType(attacker.type2)) {
					score -= 90;
				}
				break;
			case 0x124:
			case 0x125:
				score += 20;
				break;
			case 0x133:
			case 0x134:
				score -= 95;
				if (skill >= TrainerAI.highSkill) {
					score = 0;
				}
				break;
			case 0x135:
				if (opponent.CanFreeze(attacker, false)) {
					score += 30;
					if (skill >= TrainerAI.highSkill) {
						if (opponent.HasWorkingAbility(Abilities.MARVELSCALE)) {
							score -= 20;
						}
					}
				}
				break;
			case 0x136:
				if (opponent.stages[Stats.DEFENSE] > 0) {
					score += 20;
				}
				break;
			case 0x137:
				if (attacker.TooHigh(Stats.DEFENSE) && attacker.TooHigh(Stats.SPDEF) && !attacker.Partner().Fainted() && attacker.TooHigh(Stats.DEFENSE) && attacker.TooHigh(Stats.SPDEF)) {
					score -= 90;
				} else {
					score -= attacker.stages[Stats.DEFENSE]*10;
					score -= attacker.stages[Stats.SPDEF]*10;
					if (!attacker.Partner().Fainted()) {
						score -= attacker.Partner().stages[Stats.DEFENSE]*10;
						score -= attacker.Partner().stages[Stats.SPDEF]*10;
					}
				}
				break;
			case 0x138:
				if (!doublebattle) {
					score -= 100;
				} else if (attacker.Partner().Fainted()) {
					score -= 90;
				} else {
					score -= attacker.stages[Stats.SPDEF]*10;
				}
				break;
			case 0x139:
				if (!opponent.CanReduceStatStage(Stats.ATTACK, attacker)) {
					score -= 90;
				} else {
					score += attacker.stages[Stats.ATTACK]*20;
					if (skill >= TrainerAI.mediumSkill) {
						bool hasPhysicalAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsPhysical(attacker.moves[i].type)) {
								hasPhysicalAttack = true;
								break;
							}
						}
						if (hasPhysicalAttack) {
							score += 20;
						} else if (skill >= TrainerAI.highSkill) {
							score -= 90;
						}
					}
				}
				break;
			case 0x13A:
				avg = attacker.stages[Stats.ATTACK]*10;
				avg += attacker.stages[Stats.SPATK]*10;
				score += avg/2;
				break;
			case 0x13B:
				if (attacker.species != Species.HOOPA || attacker.form != 1) {
					score -= 100;
				} else {
					if (opponent.stages[Stats.DEFENSE] > 0) {
						score += 20;
					}
				}
				break;
			case 0x13C:
				if (opponent.stages[Stats.SPATK] > 0) {
					score += 20;
				}
				break;
			case 0x13D:
				if (!opponent.CanReduceStatStage(Stats.SPATK, attacker)) {
					score -= 90;
				} else {
					if (attacker.turnCount == 0) {
						score += 40;
					}
					score += attacker.stages[Stats.SPATK]*20;
				}
				break;
			case 0x13E:
				count = 0;
				for (int i=0; i<4; i++) {
					Battler battler = battlers[i];
					if (battler.HasType(Types.GRASS) && !battler.IsAirborne() && (!battler.TooHigh(Stats.ATTACK) || !battler.TooHigh(Stats.SPATK))) {
						count++;
						if (attacker.IsOpposing(battler.index)) {
							score -= 20;
						} else {
							score -= attacker.stages[Stats.ATTACK]*10;
							score -= attacker.stages[Stats.SPATK]*10;
						}
					}
				}
				if (count==0) {
					score -= 95;
				}
				break;
			case 0x13F:
				count = 0;
				for (int i=0; i<4; i++) {
					Battler battler = battlers[i];
					if (battler.HasType(Types.GRASS) && !battler.TooHigh(Stats.DEFENSE)) {
						count++;
						if (attacker.IsOpposing(battler.index)) {
							score -= 20;
						} else {
							score -= attacker.stages[Stats.DEFENSE]*10;
						}
					}
				}
				if (count==0) {
					score -= 95;
				}
				break;
			case 0x140:
				count = 0;
				for (int i=0; i<4; i++) {
					Battler battler = battlers[i];
					if (battler.status == Statuses.POISON && (!battler.TooLow(Stats.ATTACK) || !battler.TooLow(Stats.SPATK) || !battler.TooLow(Stats.SPEED))) {
						count++;
						if (attacker.IsOpposing(battler.index)) {
							score += attacker.stages[Stats.ATTACK]*10;
							score += attacker.stages[Stats.SPATK]*10;
							score += attacker.stages[Stats.SPEED]*10;
						} else {
							score -= 20;
						}
					}
				}
				if (count==0) {
					score -= 95;
				}
				break;
			case 0x141:
				if (opponent.effects[Effects.Substitute] > 0) {
					score -= 90;
				} else {
					int numpos = 0;
					int numneg = 0;
					int[] stats = new int[7]{Stats.ATTACK, Stats.DEFENSE, Stats.SPEED, Stats.SPATK, Stats.SPDEF, Stats.ACCURACY, Stats.EVASION};
					for (int i=0; i<stats.Length; i++) {
						int stat = opponent.stages[stats[i]];
						if (stat > 0) {
							numpos += stat;
						} else {
							numneg += stat;
						}
					}
					if (numpos != 0 || numneg != 0) {
						score += (numpos - numneg)*10;
					} else {
						score -= 95;
					}
				}
				break;
			case 0x142:
				if (opponent.HasType(Types.GHOST)) {
					score -= 90;
				}
				break;
			case 0x143:
				if (opponent.HasType(Types.GRASS)) {
					score -= 90;
				}
				break;
			case 0x144:
			case 0x145:
				aspeed = RoughStat(attacker, Stats.SPEED, skill);
				ospeed = RoughStat(opponent, Stats.SPEED, skill);
				if (aspeed > ospeed) {
					score -= 90;
				}
				break;
			case 0x146:
			case 0x147:
			case 0x148:
				aspeed = RoughStat(attacker, Stats.SPEED, skill);
				ospeed = RoughStat(opponent, Stats.SPEED, skill);
				if (aspeed > ospeed) {
					score -= 90;
				} else {
					if (opponent.HasMoveType(Types.FIRE)) {
						score += 30;
					}
				}
				break;
			case 0x149:
				if (attacker.turnCount == 0) {
					score += 30;
				} else {
					score -= 90;
					if (skill >= TrainerAI.bestSkill) {
						score = 0;
					}
				}
				break;
			case 0x14A:
			case 0x14B:
			case 0x14C:
				if (attacker.effects[Effects.ProtectRate] > 1 || opponent.effects[Effects.HyperBeam] > 0) {
					score -= 90;
				} else {
					if (skill >= TrainerAI.mediumSkill) {
						score -= (attacker.effects[Effects.ProtectRate]*40);
					}
					if (attacker.turnCount == 0) {
						score += 50;
					}
					if (opponent.effects[Effects.TwoTurnAttack] != 0) {
						score += 30;
					}
				}
				break;
			case 0x14D:
			case 0x14E:
				if (attacker.TooHigh(Stats.SPATK) && attacker.TooHigh(Stats.SPDEF) && attacker.TooHigh(Stats.SPEED)) {
					score -= 90;
				} else {
					score -= attacker.stages[Stats.SPATK]*10;
					score -= attacker.stages[Stats.SPDEF]*10;
					score -= attacker.stages[Stats.SPEED]*10;
					if (skill >= TrainerAI.mediumSkill) {
						bool hasSpecialAttack = false;
						for (int i=0; i<attacker.moves.Length; i++) {
							if (attacker.moves[i].id != 0 && attacker.moves[i].baseDamage > 0 && attacker.moves[i].IsSpecial(attacker.moves[i].type)) {
								hasSpecialAttack = true;
								break;
							}
						}
						if (hasSpecialAttack) {
							score += 20;
						} else if (skill >= TrainerAI.highSkill) {
							score -= 90;
						}
					}
					if (skill >= TrainerAI.highSkill) {
						aspeed = RoughStat(attacker, Stats.SPEED, skill);
						ospeed = RoughStat(opponent, Stats.SPEED, skill);
						if (aspeed < ospeed && aspeed*2 > ospeed) {
							score += 30;
						}
					}
				}
				break;
			case 0x14F:
				if (skill >= TrainerAI.highSkill && opponent.HasWorkingAbility(Abilities.LIQUIDOOZE)) {
					score -= 80;
				} else {
					if (attacker.hp <= (attacker.totalHP/2)) {
						score += 40;
					}
				}
				break;
			case 0x150:
				if (!attacker.TooHigh(Stats.ATTACK) && opponent.hp <= (opponent.totalHP/4)) {
					score += 20;
				}
				break;
			case 0x151:
				if (skill >= TrainerAI.highSkill && opponent.HasWorkingAbility(Abilities.LIQUIDOOZE)) {
					score -= 80;
				} else {
					if (attacker.hp <= (attacker.totalHP/2)) {
						score += 40;
					}
				}
				break;
			case 0x152:
			case 0x153:
				avg = attacker.stages[Stats.ATTACK]*10;
				avg += attacker.stages[Stats.SPATK]*10;
				score += avg/2;
				break;
			case 0x154:
			case 0x155:
			case 0x156:
			case 0x157:
				if (opponent.OwnSide().effects[Effects.StickyWeb] != 0) {
					score -= 95;
				}
				break;
			case 0x158:
				if (attacker.pokemon == null || !attacker.pokemon.belch) {
					score -= 90;
				}
				break;
		}
		if (score <= 0) {
			return (int)score;
		}
		if (attacker.NonActivePokemonCount() == 0) {
			if (skill >= TrainerAI.mediumSkill && !(skill >= TrainerAI.highSkill && opponent.NonActivePokemonCount() > 0)) {
				if (move.baseDamage == 0) {
					score /= 1.5;
				} else if (opponent.hp <= opponent.totalHP/2) {
					score *= 1.5;
				}
			}
		}
		if (opponent.effects[Effects.TwoTurnAttack] > 0 && skill >= TrainerAI.highSkill) {
			int invulmove = BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function;
			int[] codes = new int[6]{0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE};
			if (move.accuracy > 0 && (Array.IndexOf(codes, invulmove) > -1 || opponent.effects[Effects.SkyDrop] != 0) && attacker.speed > opponent.speed) {
				if (skill >= TrainerAI.bestSkill) {
					bool miss = false;
					switch (invulmove) {
						case 0xC9:
						case 0xCC:
							if (!(move.function == 0x08 || move.function == 0x15 || move.function == 0x77 || move.function == 0x78 || move.function == 0x11B || move.function == 0x11C || move.id == Moves.WHIRLWIND)) {
								miss = true;
							}
							break;
						case 0xCA:
							if (!(move.function == 0x76 || move.function == 0x95)) {
								miss = true;
							}
							break;
						case 0xCB:
							if (!(move.function == 0x75 || move.function == 0xD0)) {
								miss = true;
							}
							break;
						case 0xCD:
							if (!(move.function == 0x08 || move.function == 0x15 || move.function == 0x77 || move.function == 0x78 || move.function == 0x11B || move.function == 0x11C)) {
								miss = true;
							}
							break;
						case 0xCE:
							miss = true;
							break;
						case 0x14D:
							miss = true;
							break;
					}
					if (opponent.effects[Effects.SkyDrop] != 0) {
						if (!(move.function == 0x08 || move.function == 0x15 || move.function == 0x77 || move.function == 0x78 || move.function == 0x11B || move.function == 0x11C)) {
							miss = true;
						}
					}
					if (miss) {
						score -= 80;
					}
				} else {
					score -= 80;
				}
			}
		}
		if (attacker.HasWorkingItem(Items.CHOICEBAND) || attacker.HasWorkingItem(Items.CHOICESPECS) || attacker.HasWorkingItem(Items.CHOICESCARF)) {
			if (skill >= TrainerAI.mediumSkill) {
				if (move.baseDamage >= 60) {
					score += 60;
				} else if (move.baseDamage > 0) {
					score += 30;
				} else if (move.function == 0xF2) {
					score += 70;
				} else {
					score -= 60;
				}
			}
		}
		if (attacker.status == Statuses.SLEEP) {
			if (skill >= TrainerAI.mediumSkill) {
				if (move.function != 0x11 && move.function != 0xB4) {
					bool hasSleepMove = false;
					for (int m=0; m < attacker.moves.Length; m++) {
						if (attacker.moves[m].function == 0x11 || attacker.moves[m].function == 0xB4) {
							hasSleepMove = true;
							break;
						}
					}
					if (hasSleepMove) {
						score -= 60;
					}
				}
			}
		}
		if (attacker.status == Statuses.FROZEN) {
			if (skill >= TrainerAI.mediumSkill) {
				if (move.CanThawUser()) {
					score += 40;
				} else {
					bool hasFreezeMove = false;
					for (int m=0; m < attacker.moves.Length; m++) {
						if (attacker.moves[m].CanThawUser()) {
							hasFreezeMove = true;
							break;
						}
					}
					if (hasFreezeMove) {
						score -= 60;
					}
				}
			}
		}
		if (move.baseDamage > 0) {
			int typeMod = TypeModifier(move.type, attacker, opponent);
			if (typeMod == 0 || score <= 0) {
				score = 0;
			} else if (skill >= TrainerAI.mediumSkill && typeMod <= 8 && opponent.HasWorkingAbility(Abilities.WONDERGUARD)) {
				score = 0;
			} else if (skill >= TrainerAI.mediumSkill && move.type == Types.GROUND && (opponent.HasWorkingAbility(Abilities.LEVITATE) || opponent.effects[Effects.MagnetRise] > 0)) {
				score = 0;
			} else if (skill >= TrainerAI.mediumSkill && move.type == Types.FIRE && opponent.HasWorkingAbility(Abilities.FLASHFIRE)) {
				score = 0;
			} else if (skill >= TrainerAI.mediumSkill && move.type == Types.WATER && (opponent.HasWorkingAbility(Abilities.WATERABSORB) || opponent.HasWorkingAbility(Abilities.STORMDRAIN) || opponent.HasWorkingAbility(Abilities.DRYSKIN))) {
				score = 0;
			} else if (skill >= TrainerAI.mediumSkill && move.type == Types.GRASS && opponent.HasWorkingAbility(Abilities.SAPSIPPER)) {
				score = 0;
			} else if (skill >= TrainerAI.mediumSkill && move.type == Types.ELECTRIC && (opponent.HasWorkingAbility(Abilities.VOLTABSORB) || opponent.HasWorkingAbility(Abilities.LIGHTNINGROD) || opponent.HasWorkingAbility(Abilities.MOTORDRIVE))) {
				score = 0;
			} else {
				int realDamage = move.baseDamage;
				if (move.baseDamage == 1) {
					realDamage = 60;
				}
				if (skill >= TrainerAI.mediumSkill) {
					realDamage = BetterBaseDamage(move, attacker, opponent, skill, realDamage);
				}
				realDamage = RoughDamage(move, attacker, opponent, skill, realDamage);
				double accuracy = RoughAccuracy(move, attacker, opponent, skill);
				double baseDamage = realDamage * accuracy/100.0;
				if (move.TwoTurnAttack(attacker) || move.function == 0xC2) {
					baseDamage = baseDamage*2/3.0;
				}
				if (!opponent.HasWorkingAbility(Abilities.INNERFOCUS) && opponent.effects[Effects.Substitute] == 0) {
					if (attacker.HasWorkingItem(Items.KINGSROCK) || attacker.HasWorkingItem(Items.RAZORFANG) && move.CanKingsRock()) {
						baseDamage *= 1.05;
					} else if (attacker.HasWorkingAbility(Abilities.STENCH) && move.function != 0x09 && move.function != 0x0B && move.function != 0x0E && move.function != 0x0F && move.function != 0x10 && move.function != 0x11 && move.function != 0x12 && move.function != 0x78 && move.function != 0xC7) {
						baseDamage *= 1.05;
					}
				}
				baseDamage = baseDamage*100.0/opponent.hp;
				if (attacker.level-10 > opponent.level) {
					baseDamage *= 1.2;
				}
				baseDamage = Math.Round(baseDamage);
				if (baseDamage > 120) {
					baseDamage = 120;
				}
				if (baseDamage > 100) {
					baseDamage += 100;
				}
				score = Math.Round(score);
				double oldscore = score;
				score += baseDamage;
				Debug.Log(string.Format("[AI] {0} damage calculated ({1} => {2}% of target's {3} HP), score change {4} => {5}",Moves.GetName(move.id), realDamage, baseDamage, opponent.hp, oldscore, score));
			}
		} else {
			score -= 10;
			double accuracy = RoughAccuracy(move, attacker, opponent, skill);
			score *= accuracy/100.0;
			if (score <= 10 && skill >= TrainerAI.highSkill) {
				score = 0;
			}
		}
		if (score < 0) {
			score = 0;
		}
		return (int)score;
	}

	/***********************************************
	* Get type effectiveness and approximate stats *
	***********************************************/
	public int TypeModifier(int type, Battler attacker, Battler opponent) {
		if (type < 0) {
			return 8;
		}
		if (type == Types.GROUND && opponent.HasType(Types.FLYING) && opponent.HasWorkingItem(Items.IRONBALL) && !Settings.USE_NEW_BATTLE_MECHANICS) {
			return 8;
		}
		int atype = type;
		int otype1 = opponent.type1;
		int otype2 = opponent.type2;
		int otype3 = opponent.effects[Effects.Type3];
		if (otype1 == Types.FLYING && opponent.effects[Effects.Roost] != 0) {
			if (otype2 == Types.FLYING && otype3 == Types.FLYING) {
				otype1 = Types.NORMAL;
			} else {
				otype1 = otype2;
			}
		}
		int mod1 = Types.GetEffectiveness(atype, otype1);
		int mod2 = (otype1 == otype2) ? 2 : Types.GetEffectiveness(atype, otype2);
		int mod3 = (otype3 < 0 || otype1 == otype3) ? 2 : Types.GetEffectiveness(atype, otype3);
		if (opponent.HasWorkingItem(Items.RINGTARGET)) {
			if (mod1 == 0) {
				mod1 = 2;
			}
			if (mod2 == 0) {
				mod2 = 2;
			}
			if (mod3 == 0) {
				mod3 = 2;
			}
		}
		if (attacker.HasWorkingAbility(Abilities.SCRAPPY) || opponent.effects[Effects.Foresight] != 0) {
			if (otype1 == Types.GHOST && Types.IsIneffective(atype, otype1)) {
				mod1 = 2;
			}
			if (otype2 == Types.GHOST && Types.IsIneffective(atype, otype2)) {
				mod2 = 2;
			}
			if (otype3 == Types.GHOST && Types.IsIneffective(atype, otype3)) {
				mod3 = 2;
			}
		}
		if (opponent.effects[Effects.MiracleEye] != 0) {
			if (otype1 == Types.DARK && Types.IsIneffective(atype, otype1)) {
				mod1 = 2;
			}
			if (otype2 == Types.DARK && Types.IsIneffective(atype, otype2)) {
				mod2 = 2;
			}
			if (otype3 == Types.DARK && Types.IsIneffective(atype, otype3)) {
				mod3 = 2;
			}
		}
		if (GetWeather() == Weather.STRONGWINDS) {
			if (otype1 == Types.FLYING && Types.IsSuperEffective(atype, otype1)) {
				mod1 = 2;
			}
			if (otype2 == Types.FLYING && Types.IsSuperEffective(atype, otype2)) {
				mod2 = 2;
			}
			if (otype3 == Types.FLYING && Types.IsSuperEffective(atype, otype3)) {
				mod3 = 2;
			}
		}
		if (!opponent.IsAirborne(attacker.HasMoldBreaker()) && atype == Types.GROUND) {
			if (otype1 == Types.FLYING) {
				mod1 = 2;
			}
			if (otype2 == Types.FLYING) {
				mod2 = 2;
			}
			if (otype3 == Types.FLYING) {
				mod3 = 2;
			}
		}
		return mod1*mod2*mod3;
	}

	public int TypeModifier2(Battler thisBattler, Battler otherBattler) {
		if (thisBattler.type1 == thisBattler.type2) {
			return 4*TypeModifier(thisBattler.type1, thisBattler, otherBattler);
		}
		int ret = TypeModifier(thisBattler.type1, thisBattler, otherBattler);
		ret *= TypeModifier(thisBattler.type2, thisBattler, otherBattler);
		return ret * 2;
	}

	public int RoughStat(Battler battler, int stat, int skill) {
		if (skill >= TrainerAI.highSkill && stat == Stats.SPEED) {
			return battler.speed;
		}
		int[] stageMul = {10, 10, 10, 10, 10, 10, 10, 15, 20, 25, 30, 35, 40};
		int[] stageDiv = {40, 35, 30, 25, 20, 15, 10, 10, 10, 10, 10, 10, 10};
		int stage = battler.stages[stat] + 6;
		int value = 0;
		switch (stat) 
		{
			case Stats.ATTACK:
				value = battler.attack;
				break;
			case Stats.DEFENSE:
				value = battler.defense;
				break;
			case Stats.SPEED:
				value = battler.speed;
				break;
			case Stats.SPATK:
				value = battler.specialAttack;
				break;
			case Stats.SPDEF:
				value = battler.specialDefense;
				break;
		}
		return (int)(value*1.0*stageMul[stage]/stageDiv[stage]);
	}

	public int BetterBaseDamage(BattleMove move, Battler attacker, Battler opponent, int skill, int baseDamage) {
		int mult;
		int ospeed;
		int aspeed;
		int n;
		int[] hp;
		int[] stats = new int[7]{Stats.ATTACK, Stats.DEFENSE, Stats.SPEED, Stats.SPATK, Stats.SPDEF, Stats.ACCURACY, Stats.EVASION};
		switch (move.function) 
		{
			case 0x6A:
				baseDamage = 20;
				break;
			case 0x6B:
				baseDamage = 40;
				break;
			case 0x6C:
				baseDamage = opponent.hp/2;
				break;
			case 0x6D:
				baseDamage = attacker.level;
				break;
			case 0x6E:
				baseDamage = opponent.hp-attacker.hp;
				break;
			case 0x6F:
				baseDamage = attacker.level;
				break;
			case 0x70:
				baseDamage = opponent.totalHP;
				break;
			case 0x71:
				baseDamage = 60;
				break;
			case 0x72:
				baseDamage = 60;
				break;
			case 0x73:
				baseDamage = 60;
				break;
			case 0x75:
			case 0x12D:
				if (BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function == 0xCB) {
					baseDamage *= 2;
				}
				break;
			case 0x76:
				if (BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function == 0xCA) {
					baseDamage *= 2;
				}
				break;
			case 0x77:
			case 0x78:
				if (BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function == 0xC9 || BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function == 0xCC || BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function == 0xCE) {
					baseDamage *= 2;
				}
				break;
			case 0x7B:
				if (opponent.status == Statuses.POISON) {
					baseDamage *= 2;
				}
				break;
			case 0x7C:
				if (opponent.status == Statuses.PARALYSIS) {
					baseDamage *= 2;
				}
				break;
			case 0x7D:
				if (opponent.status == Statuses.SLEEP) {
					baseDamage *= 2;
				}
				break;
			case 0x7E:
				if (opponent.status == Statuses.POISON || opponent.status == Statuses.BURN || opponent.status == Statuses.PARALYSIS) {
					baseDamage *= 2;
				}
				break;
			case 0x7F:
				if (opponent.status != 0) {
					baseDamage *= 2;
				}
				break;
			case 0x80:
				if (opponent.hp <= (opponent.totalHP/2)) {
					baseDamage *= 2;
				}
				break;
			case 0x85:
				// TODO
				break;
			case 0x86:
				if (attacker.item == 0 || attacker.HasWorkingItem(Items.FLYINGGEM)) 
				{
					baseDamage *= 2;
				}
				break;
			case 0x87:
				if (GetWeather() != 0) {
					baseDamage *= 2;
				}
				break;
			case 0x89:
				baseDamage = (int)Math.Max(attacker.happiness*2/5, 1);
				break;
			case 0x8A:
				baseDamage = (int)Math.Max(255-attacker.happiness*2/5, 1);
				break;
			case 0x8B:
				baseDamage = (int)Math.Max(150*((double)attacker.hp)/attacker.totalHP, 1);
				break;
			case 0x8C:
				baseDamage = (int)Math.Max(120*((double)attacker.hp)/attacker.totalHP, 1);
				break;
			case 0x8D:
				ospeed = RoughStat(opponent, Stats.SPEED, skill);
				aspeed = RoughStat(attacker, Stats.SPEED, skill);
				baseDamage = (int)Math.Max(Math.Min(25.0*ospeed/aspeed, 150), 1);
				break;
			case 0x8E:
				mult = 0;
				for (int i=0; i<stats.Length; i++) 
				{
					if (attacker.stages[i] > 0) {
						mult += attacker.stages[i];
					}
				}
				baseDamage = 20*(mult+1);
				break;
			case 0x8F:
				mult = 0;
				for (int i=0; i<stats.Length; i++) 
				{
					if (attacker.stages[i] > 0) {
						mult += attacker.stages[i];
					}
				}
				baseDamage = (int)Math.Max(20*(mult+3), 200);
				break;
			case 0x90:
				hp = BattleMove.HiddenPower(attacker.iv);
				baseDamage = hp[1];
				break;
			case 0x91:
				baseDamage = baseDamage << (attacker.effects[Effects.FuryCutter]-1);
				break;
			case 0x92:
				baseDamage *= attacker.OwnSide().effects[Effects.EchoedVoiceCounter];
				break;
			case 0x94:
				baseDamage = 50;
				break;
			case 0x95:
				baseDamage = 71;
				if (BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function == 0xCA) {
					baseDamage *= 2;
				}
				break;
			case 0x96:
				switch (attacker.item) 
				{
					case Items.CHERIBERRY:
					case Items.CHESTOBERRY:
					case Items.PECHABERRY:
					case Items.RAWSTBERRY:
					case Items.ASPEARBERRY:
					case Items.LEPPABERRY:
					case Items.ORANBERRY:
					case Items.PERSIMBERRY:
					case Items.LUMBERRY:
					case Items.SITRUSBERRY:
					case Items.FIGYBERRY:
					case Items.WIKIBERRY:
					case Items.MAGOBERRY:
					case Items.AGUAVBERRY:
					case Items.IAPAPABERRY:
					case Items.RAZZBERRY:
					case Items.OCCABERRY:
					case Items.PASSHOBERRY:
					case Items.WACANBERRY:
					case Items.RINDOBERRY:
					case Items.YACHEBERRY:
					case Items.CHOPLEBERRY:
					case Items.KEBIABERRY:
					case Items.SHUCABERRY:
					case Items.COBABERRY:
					case Items.PAYAPABERRY:
					case Items.TANGABERRY:
					case Items.CHARTIBERRY:
					case Items.KASIBBERRY:
					case Items.HABANBERRY:
					case Items.COLBURBERRY:
					case Items.BABIRIBERRY:
					case Items.CHILANBERRY:
						baseDamage = 60;
						break;
					case Items.BLUKBERRY:
					case Items.NANABBERRY:
					case Items.WEPEARBERRY:
					case Items.PINAPBERRY:
					case Items.POMEGBERRY:
					case Items.KELPSYBERRY:
					case Items.QUALOTBERRY:
					case Items.HONDEWBERRY:
					case Items.GREPABERRY:
					case Items.TAMATOBERRY:
					case Items.CORNNBERRY:
					case Items.MAGOSTBERRY:
					case Items.RABUTABERRY:
					case Items.NOMELBERRY:
					case Items.SPELONBERRY:
					case Items.PAMTREBERRY:
						baseDamage = 70;
						break;
					case Items.WATMELBERRY:
					case Items.DURINBERRY:
					case Items.BELUEBERRY:
					case Items.LIECHIBERRY:
					case Items.GANLONBERRY:
					case Items.SALACBERRY:
					case Items.PETAYABERRY:
					case Items.APICOTBERRY:
					case Items.LANSATBERRY:
					case Items.STARFBERRY:
					case Items.ENIGMABERRY:
					case Items.MICLEBERRY:
					case Items.CUSTAPBERRY:
					case Items.JABOCABERRY:
					case Items.ROWAPBERRY:
						baseDamage = 80;
						break;
				}
				break;
			case 0x97:
				switch ((int)Math.Min(move.pp, 5)) 
				{
					case 1:
						baseDamage = 200;
						break;
					case 2:
						baseDamage = 80;
						break;
					case 3:
						baseDamage = 60;
						break;
					case 4:
						baseDamage = 50;
						break;
					case 5:
						baseDamage = 40;
						break;
				}
				break;
			case 0x98:
				n = (int)(48.0*attacker.hp/attacker.totalHP);
				baseDamage = 20;
				if (n < 33) {
					baseDamage = 40;
				}
				if (n < 17) {
					baseDamage = 80;
				}
				if (n < 10) {
					baseDamage = 100;
				}
				if (n < 5) {
					baseDamage = 150;
				}
				if (n < 2) {
					baseDamage = 200;
				}
				break;
			case 0x99:
				n = (int)(((double)attacker.speed)/opponent.speed);
				baseDamage = 40;
				if (n < 1) {
					baseDamage = 60;
				}
				if (n < 2) {
					baseDamage = 80;
				}
				if (n < 3) {
					baseDamage = 120;
				}
				if (n < 4) {
					baseDamage = 150;
				}
				break;
			case 0x9A:
				n = (int)opponent.weight(attacker);
				baseDamage = 20;
				if (n > 100) {
					baseDamage = 40;
				}
				if (n > 250) {
					baseDamage = 60;
				}
				if (n > 500) {
					baseDamage = 80;
				}
				if (n > 1000) {
					baseDamage = 100;
				}
				if (n > 2000) {
					baseDamage = 120;
				}
				break;
			case 0x9B:
				n = (int)(((double)attacker.weight(attacker))/opponent.weight(attacker));
				baseDamage = 40;
				if (n >= 2) {
					baseDamage = 60;
				}
				if (n >= 3) {
					baseDamage = 80;
				}
				if (n >= 4) {
					baseDamage = 100;
				}
				if (n >= 5) {
					baseDamage = 120;
				}
				break;
			case 0xA0:
				baseDamage *= 2;
				break;
			case 0xBD:
			case 0xBE:
				baseDamage *= 2;
				break;
			case 0xBF:
				baseDamage *= 6;
				break;
			case 0xC0:
				if (attacker.HasWorkingAbility(Abilities.SKILLLINK)) {
					baseDamage *= 5;
				} else {
					baseDamage = baseDamage * 19 / 6;
				}
				break;
			case 0xC1:
				Battler[] party = Party(attacker.index);
				mult = 0;
				for (int i=0; i<party.Length; i++) 
				{
					if (party[i] != null && !party[i].pokemon.Egg() && party[i].hp > 0 && party[i].status == 0) {
						mult += 1;
					}
				}
				baseDamage *= mult;
				break;
			case 0xC4:
				if (GetWeather() != 0 && GetWeather() != Weather.SUNNYDAY) {
					baseDamage = (int)(baseDamage*0.5);
				}
				break;
			case 0xD0:
				if (skill >= TrainerAI.mediumSkill) {
					if (BattleMove.FromBattleMove(this, new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).function == 0xCB) {
						baseDamage *= 2;
					}
				}
				break;
			case 0xD3:
				if (skill >= TrainerAI.mediumSkill) {
					if (attacker.effects[Effects.DefenseCurl] != 0) {
						baseDamage *= 2;
					}
				}
				break;
			case 0xE1:
				baseDamage = attacker.hp;
				break;
			case 0xF7:
				// TODO
				break;
			case 0x113:
				baseDamage *= attacker.effects[Effects.Stockpile];
				break;
			case 0x144:
				mult = Types.GetCombinedEffectiveness(Types.FLYING, opponent.type1, opponent.type2, opponent.effects[Effects.Type3]);
				baseDamage = (int)Math.Round(((double)baseDamage)*mult/8.0);
				break;
		}

		return baseDamage;
	}

	public int RoughDamage(BattleMove move, Battler attacker, Battler opponent, int skill, int baseDamage) {
		if (move.function == 0x6A || move.function == 0x6B || move.function == 0x6C || move.function == 0x6D || move.function == 0x6E || move.function == 0x6F || move.function == 0x70 || move.function == 0x71 || move.function == 0x72 || move.function == 0x73 || move.function == 0xE1) {
			return baseDamage;
		}
		int type = move.type;
		if (skill >= TrainerAI.highSkill) {
			type = move.GetType(type, attacker, opponent);
		}
		if (skill >= TrainerAI.highSkill) {
			if (attacker.HasWorkingAbility(Abilities.TECHNICIAN) && baseDamage <= 60) {
				baseDamage = (int)Math.Round(baseDamage*1.5);
			}
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (attacker.HasWorkingAbility(Abilities.IRONFIST) && move.IsPunchingMove()) {
				baseDamage = (int)Math.Round(baseDamage*1.2);
			}
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (attacker.HasWorkingAbility(Abilities.RECKLESS)) {
				if (move.function == 0xFA || move.function == 0xFB || move.function == 0xFC || move.function == 0xFD || move.function == 0xFE || move.function == 0x10B || move.function == 0x130) {
					baseDamage = (int)Math.Round(baseDamage*1.2);
				}
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (attacker.HasWorkingAbility(Abilities.FLAREBOOST) && attacker.status == Statuses.BURN && move.IsSpecial(type)) {
				baseDamage = (int)Math.Round(baseDamage*1.5);
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (attacker.HasWorkingAbility(Abilities.TOXICBOOST) && attacker.status == Statuses.POISON && move.IsPhysical(type)) {
				baseDamage = (int)Math.Round(baseDamage*1.5);
			}
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (attacker.HasWorkingAbility(Abilities.RIVALRY) && attacker.gender != 2 && opponent.gender != 2) {
				if (attacker.gender == opponent.gender) {
					baseDamage = (int)Math.Round(baseDamage*1.25);
				} else {
					baseDamage = (int)Math.Round(baseDamage*0.75);
				}
			}
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (attacker.HasWorkingAbility(Abilities.SANDFORCE) && GetWeather() == Weather.SANDSTORM && (type == Types.ROCK || type == Types.STEEL || type == Types.GROUND)) {
				baseDamage = (int)Math.Round(baseDamage*1.3);
			}
		}
		if (skill >= TrainerAI.bestSkill) {
			if (opponent.HasWorkingAbility(Abilities.HEATPROOF) && type == Types.FIRE) {
				baseDamage = (int)Math.Round(baseDamage*0.5);
			}
		}
		if (skill >= TrainerAI.bestSkill) {
			if (opponent.HasWorkingAbility(Abilities.DRYSKIN) && type == Types.FIRE) {
				baseDamage = (int)Math.Round(baseDamage*1.25);
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (attacker.HasWorkingAbility(Abilities.SHEERFORCE) && move.add1Effect > 0) {
				baseDamage = (int)Math.Round(baseDamage*1.3);
			}
		}
		if ((attacker.HasWorkingItem(Items.SILKSCARF) && type == Types.NORMAL) || (attacker.HasWorkingItem(Items.BLACKBELT) && type == Types.FIGHTING) || (attacker.HasWorkingItem(Items.SHARPBEAK) && type == Types.FLYING) || (attacker.HasWorkingItem(Items.POISONBARB) && type == Types.POISON) || (attacker.HasWorkingItem(Items.SOFTSAND) && type == Types.GROUND) || (attacker.HasWorkingItem(Items.HARDSTONE) && type == Types.ROCK) || (attacker.HasWorkingItem(Items.SILVERPOWDER) && type == Types.BUG) || (attacker.HasWorkingItem(Items.SPELLTAG) && type == Types.GHOST) || (attacker.HasWorkingItem(Items.METALCOAT) && type == Types.STEEL) || (attacker.HasWorkingItem(Items.CHARCOAL) && type == Types.FIRE) || (attacker.HasWorkingItem(Items.MYSTICWATER) && type == Types.WATER) || (attacker.HasWorkingItem(Items.MIRACLESEED) && type == Types.GRASS) || (attacker.HasWorkingItem(Items.MAGNET) && type == Types.ELECTRIC) || (attacker.HasWorkingItem(Items.TWISTEDSPOON) && type == Types.PSYCHIC) || (attacker.HasWorkingItem(Items.NEVERMELTICE) && type == Types.ICE) || (attacker.HasWorkingItem(Items.DRAGONFANG) && type == Types.DRAGON) || (attacker.HasWorkingItem(Items.BLACKGLASSES) && type == Types.DARK)) {
			baseDamage = (int)Math.Round(baseDamage*1.2);
		}
		if ((attacker.HasWorkingItem(Items.FISTPLATE) && type == Types.FIGHTING) || (attacker.HasWorkingItem(Items.SKYPLATE) && type == Types.FLYING) || (attacker.HasWorkingItem(Items.TOXICPLATE) && type == Types.POISON) || (attacker.HasWorkingItem(Items.EARTHPLATE) && type == Types.GROUND) || (attacker.HasWorkingItem(Items.STONEPLATE) && type == Types.ROCK) || (attacker.HasWorkingItem(Items.INSECTPLATE) && type == Types.BUG) || (attacker.HasWorkingItem(Items.SPOOKYPLATE) && type == Types.GHOST) || (attacker.HasWorkingItem(Items.IRONPLATE) && type == Types.STEEL) || (attacker.HasWorkingItem(Items.FLAMEPLATE) && type == Types.FIRE) || (attacker.HasWorkingItem(Items.SPLASHPLATE) && type == Types.WATER) || (attacker.HasWorkingItem(Items.MEADOWPLATE) && type == Types.GRASS) || (attacker.HasWorkingItem(Items.ZAPPLATE) && type == Types.ELECTRIC) || (attacker.HasWorkingItem(Items.MINDPLATE) && type == Types.PSYCHIC) || (attacker.HasWorkingItem(Items.ICICLEPLATE) && type == Types.ICE) || (attacker.HasWorkingItem(Items.DRACOPLATE) && type == Types.DRAGON) || (attacker.HasWorkingItem(Items.DREADPLATE) && type == Types.DARK) || (attacker.HasWorkingItem(Items.PIXIEPLATE) && type == Types.FAIRY)) {
			baseDamage = (int)Math.Round(baseDamage*1.2);
		}
		if ((attacker.HasWorkingItem(Items.NORMALGEM) && type == Types.NORMAL) || (attacker.HasWorkingItem(Items.FIGHTINGGEM) && type == Types.FIGHTING) || (attacker.HasWorkingItem(Items.FLYINGGEM) && type == Types.FLYING) || (attacker.HasWorkingItem(Items.POISONGEM) && type == Types.POISON) || (attacker.HasWorkingItem(Items.GROUNDGEM) && type == Types.GROUND) || (attacker.HasWorkingItem(Items.ROCKGEM) && type == Types.ROCK) || (attacker.HasWorkingItem(Items.BUGGEM) && type == Types.BUG) || (attacker.HasWorkingItem(Items.GHOSTGEM) && type == Types.GHOST) || (attacker.HasWorkingItem(Items.STEELGEM) && type == Types.STEEL) || (attacker.HasWorkingItem(Items.FIREGEM) && type == Types.FIRE) || (attacker.HasWorkingItem(Items.WATERGEM) && type == Types.WATER) || (attacker.HasWorkingItem(Items.GRASSGEM) && type == Types.GRASS) || (attacker.HasWorkingItem(Items.ELECTRICGEM) && type == Types.ELECTRIC) || (attacker.HasWorkingItem(Items.PSYCHICGEM) && type == Types.PSYCHIC) || (attacker.HasWorkingItem(Items.ICEGEM) && type == Types.ICE) || (attacker.HasWorkingItem(Items.DRAGONGEM) && type == Types.DRAGON) || (attacker.HasWorkingItem(Items.DARKGEM) && type == Types.DARK) || (attacker.HasWorkingItem(Items.FAIRYGEM) && type == Types.FAIRY)) {
			baseDamage = (int)Math.Round(baseDamage*1.5);
		}
		if (attacker.HasWorkingItem(Items.ROCKINCENSE) && type == Types.ROCK) {
			baseDamage = (int)Math.Round(baseDamage*1.2);
		}
		if (attacker.HasWorkingItem(Items.ROSEINCENSE) && type == Types.GRASS) {
			baseDamage = (int)Math.Round(baseDamage*1.2);
		}
		if (attacker.HasWorkingItem(Items.SEAINCENSE) && type == Types.WATER) {
			baseDamage = (int)Math.Round(baseDamage*1.2);
		}
		if (attacker.HasWorkingItem(Items.WAVEINCENSE) && type == Types.WATER) {
			baseDamage = (int)Math.Round(baseDamage*1.2);
		}
		if (attacker.HasWorkingItem(Items.ODDINCENSE) && type == Types.PSYCHIC) {
			baseDamage = (int)Math.Round(baseDamage*1.2);
		}
		if (attacker.HasWorkingItem(Items.MUSCLEBAND) && move.IsPhysical(type)) {
			baseDamage = (int)Math.Round(baseDamage*1.1);
		}
		if (attacker.HasWorkingItem(Items.WISEGLASSES) && move.IsSpecial(type)) {
			baseDamage = (int)Math.Round(baseDamage*1.1);
		}
		if (attacker.species == Species.DIALGA && attacker.HasWorkingItem(Items.ADAMANTORB) && (type == Types.DRAGON || type == Types.STEEL)) {
			baseDamage = (int)Math.Round(baseDamage*1.2);
		}
		if (attacker.species == Species.PALKIA && attacker.HasWorkingItem(Items.LUSTROUSORB) && (type == Types.DRAGON || type == Types.WATER)) {
			baseDamage = (int)Math.Round(baseDamage*1.2);
		}
		if (attacker.species == Species.GIRATINA && attacker.HasWorkingItem(Items.GRISEOUSORB) && (type == Types.DRAGON || type == Types.GHOST)) {
			baseDamage = (int)Math.Round(baseDamage*1.2);
		}
		if (attacker.effects[Effects.Charge] > 0 && type == Types.ELECTRIC) {
			baseDamage *= 2;
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (type == Types.FIRE) {
				for (int i=0; i<4; i++) 
				{
					if (battlers[i].effects[Effects.WaterSport] != 0 && !battlers[i].Fainted()) {
						baseDamage = (int)Math.Round(baseDamage*0.33);
					}
				}
			}
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (type == Types.ELECTRIC) {
				for (int i=0; i<4; i++) 
				{
					if (battlers[i].effects[Effects.MudSport] != 0 && !battlers[i].Fainted()) {
						baseDamage = (int)Math.Round(baseDamage*0.33);
					}
				}
			}
		}
		int atk = RoughStat(attacker, Stats.ATTACK, skill);
		if (move.function == 0x121) {
			atk = RoughStat(opponent, Stats.ATTACK, skill);
		}
		if (type >= 0 && move.IsSpecial(type)) {
			atk = RoughStat(attacker, Stats.SPATK, skill);
			if (move.function == 0x121) {
				atk = RoughStat(opponent, Stats.ATTACK, skill);
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (attacker.HasWorkingAbility(Abilities.HUSTLE) && move.IsPhysical(type)) {
				atk = (int)Math.Round(atk*1.5);
			}
		}
		if (skill >= TrainerAI.bestSkill) {
			if (attacker.HasWorkingAbility(Abilities.THICKFAT) && (type == Types.ICE || type == Types.FIRE)) {
				atk = (int)Math.Round(atk*0.5);
			}
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (attacker.hp <= ((int)(attacker.totalHP/3.0))) {
				if ((attacker.HasWorkingAbility(Abilities.OVERGROW) && type == Types.GRASS) || (attacker.HasWorkingAbility(Abilities.BLAZE) && type == Types.FIRE) || (attacker.HasWorkingAbility(Abilities.TORRENT) && type == Types.WATER) || (attacker.HasWorkingAbility(Abilities.SWARM) && type == Types.BUG)) {
					atk = (int)Math.Round(atk*1.5);
				}
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if ((attacker.HasWorkingAbility(Abilities.PLUS) || attacker.HasWorkingAbility(Abilities.MINUS)) && move.IsSpecial(type)) {
				if (attacker.Partner().HasWorkingAbility(Abilities.PLUS) || attacker.Partner().HasWorkingAbility(Abilities.MINUS)) {
					atk = (int)Math.Round(atk*1.5);
				}
			}
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (attacker.HasWorkingAbility(Abilities.DEFEATIST) && attacker.hp <= ((int)(attacker.totalHP/3.0))) {
				atk = (int)Math.Round(atk*0.5);
			}
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (attacker.HasWorkingAbility(Abilities.PUREPOWER) || attacker.HasWorkingAbility(Abilities.HUGEPOWER)) {
				atk = (int)Math.Round(atk*2.0);
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (attacker.HasWorkingAbility(Abilities.SOLARPOWER) && GetWeather() == Weather.SUNNYDAY && move.IsSpecial(type)) {
				atk = (int)Math.Round(atk*1.5);
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (attacker.HasWorkingAbility(Abilities.FLASHFIRE) && attacker.effects[Effects.FlashFire] != 0 && type == Types.FIRE) {
				atk = (int)Math.Round(atk*1.5);
			}
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (attacker.HasWorkingAbility(Abilities.SLOWSTART) && attacker.turnCount < 5 && move.IsPhysical(type)) {
				atk = (int)Math.Round(atk*0.5);
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (GetWeather() == Weather.SUNNYDAY && move.IsPhysical(type)) {
				if (attacker.HasWorkingAbility(Abilities.FLOWERGIFT) && attacker.species == Species.CHERRIM) {
					atk = (int)Math.Round(atk*1.5);
				}
				if (attacker.Partner().HasWorkingAbility(Abilities.FLOWERGIFT) && attacker.species == Species.CHERRIM) {
					atk = (int)Math.Round(atk*1.5);
				}
			}
		}
		if (attacker.HasWorkingItem(Items.THICKCLUB) && (attacker.species == Species.CUBONE || attacker.species == Species.MAROWAK) && move.IsPhysical(type)) {
			atk = (int)Math.Round(atk*2.0);
		}
		if (attacker.HasWorkingItem(Items.DEEPSEATOOTH) && attacker.species == Species.CLAMPERL && move.IsSpecial(type)) {
			atk = (int)Math.Round(atk*2.0);
		}
		if (attacker.HasWorkingItem(Items.LIGHTBALL) && attacker.species == Species.PIKACHU) {
			atk = (int)Math.Round(atk*2.0);
		}
		if (attacker.HasWorkingItem(Items.SOULDEW) && (attacker.species == Species.LATIOS || attacker.species == Species.LATIAS) && move.IsSpecial(type)) {
			atk = (int)Math.Round(atk*1.5);
		}
		if (attacker.HasWorkingItem(Items.CHOICEBAND) && move.IsPhysical(type)) {
			atk = (int)Math.Round(atk*1.5);
		}
		if (attacker.HasWorkingItem(Items.CHOICESPECS) && move.IsSpecial(type)) {
			atk = (int)Math.Round(atk*1.5);
		}
		int def = RoughStat(opponent, Stats.DEFENSE, skill);
		bool applySandstorm = false;
		if (type >= 0 && move.IsSpecial(type)) {
			if (move.function != 0x122) {
				def = RoughStat(opponent, Stats.SPDEF, skill);
				applySandstorm = true;
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (GetWeather() == Weather.SANDSTORM && opponent.HasType(Types.ROCK) && applySandstorm) {
				def = (int)Math.Round(def*1.5);
			}
		}
		if (skill >= TrainerAI.bestSkill) {
			if (opponent.HasWorkingAbility(Abilities.MARVELSCALE) && opponent.status > 0 && move.IsPhysical(type)) {
				def = (int)Math.Round(def*1.5);
			}
		}
		if (skill >= TrainerAI.bestSkill) {
			if (GetWeather() == Weather.SUNNYDAY && move.IsSpecial(type)) {
				if (attacker.HasWorkingAbility(Abilities.FLOWERGIFT) && attacker.species == Species.CHERRIM) {
					def = (int)Math.Round(def*1.5);
				}
				if (attacker.Partner().HasWorkingAbility(Abilities.FLOWERGIFT) && attacker.species == Species.CHERRIM) {
					def = (int)Math.Round(def*1.5);
				}
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (opponent.HasWorkingItem(Items.EVIOLITE)) {
				int[][] evos = Evolution.GetEvolvedFormData(opponent.pokemon.species);
				if (evos != null && evos.Length > 0) {
					def = (int)Math.Round(def*1.5);
				}
			}
			if (opponent.HasWorkingItem(Items.DEEPSEASCALE) && opponent.species == Species.CLAMPERL && move.IsSpecial(type)) {
				def = (int)Math.Round(def*2.0);
			}
			if (opponent.HasWorkingItem(Items.METALPOWDER) && opponent.species == Species.DITTO && opponent.effects[Effects.Transform] == 0 && move.IsPhysical(type)) {
				def = (int)Math.Round(def*2.0);
			}
			if (opponent.HasWorkingItem(Items.SOULDEW) && (opponent.species == Species.LATIOS || opponent.species == Species.LATIAS) && move.IsSpecial(type)) {
				def = (int)Math.Round(def*1.5);
			}
		}
		def = (int)Math.Max(def, 1);

		int damage = ((int)((int)((int)2.0*attacker.level/5+2)*((double)baseDamage)*atk/def))/50 + 2;
		if (skill >= TrainerAI.highSkill) {
			if (move.TargetsMultiple(attacker)) {
				damage = (int)Math.Round(damage*0.75);
			}
		}
		if (skill >= TrainerAI.mediumSkill) {
			switch (GetWeather()) 
			{
				case Weather.SUNNYDAY:
					if (type == Types.FIRE) {
						damage = (int)Math.Round(damage*1.5);
					} else if (type == Types.WATER) {
						damage = (int)Math.Round(damage*0.5);
					}
					break;
				case Weather.RAINDANCE:
					if (type == Types.WATER) {
						damage = (int)Math.Round(damage*1.5);
					} else if (type == Types.FIRE) {
						damage = (int)Math.Round(damage*0.5);
					}
					break;
			}
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (attacker.HasType(type)) {
				if (attacker.HasWorkingAbility(Abilities.ADAPTABILITY) && skill >= TrainerAI.highSkill) {
					damage = (int)Math.Round(damage*2.0);
				} else {
					damage = (int)Math.Round(damage*1.5);
				}
			}
		}
		int typeMod = TypeModifier(type, attacker, opponent);
		if (skill >= TrainerAI.highSkill) {
			damage = (int)Math.Round(damage * typeMod * 1.0/8);
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (attacker.status == Statuses.BURN && move.IsPhysical(type) && !attacker.HasWorkingAbility(Abilities.GUTS)) {
				damage = (int)Math.Round(damage*0.5);
			}
		}
		if (damage < 1) {
			damage = 1;
		}
		if (skill >= TrainerAI.highSkill) {
			if (opponent.OwnSide().effects[Effects.Reflect] > 0 && move.IsPhysical(type)) {
				if (!opponent.Partner().Fainted()) {
					damage = (int)Math.Round(damage*0.66);
				} else {
					damage = (int)Math.Round(damage*0.5);
				}
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (opponent.OwnSide().effects[Effects.LightScreen] > 0 && move.IsSpecial(type)) {
				if (!opponent.Partner().Fainted()) {
					damage = (int)Math.Round(damage*0.66);
				} else {
					damage = (int)Math.Round(damage*0.5);
				}
			}
		}
		if (skill >= TrainerAI.bestSkill) {
			if (opponent.HasWorkingAbility(Abilities.MULTISCALE) && opponent.hp == opponent.totalHP) {
				damage = (int)Math.Round(damage*0.5);
			}
		}
		if (skill >= TrainerAI.bestSkill) {
			if (opponent.HasWorkingAbility(Abilities.TINTEDLENS) && typeMod < 8) {
				damage = (int)Math.Round(damage*2.0);
			}
		}
		if (skill >= TrainerAI.bestSkill) {
			if (opponent.HasWorkingAbility(Abilities.FRIENDGUARD)) {
				damage = (int)Math.Round(damage*0.75);
			}
		}
		if (skill >= TrainerAI.bestSkill) {
			if ((opponent.HasWorkingAbility(Abilities.SOLIDROCK) || opponent.HasWorkingAbility(Abilities.FILTER)) && typeMod < 8) {
				damage = (int)Math.Round(damage*0.75);
			}
		}
		if (attacker.HasWorkingItem(Items.METRONOME)) {
			if (attacker.effects[Effects.Metronome] > 4) {
				damage = (int)Math.Round(damage*2.0);
			} else {
				double met = 1.0 + attacker.effects[Effects.Metronome] * 0.2;
				damage = (int)Math.Round(damage*met);
			}
		}
		if (attacker.HasWorkingItem(Items.EXPERTBELT) && typeMod > 8) {
			damage = (int)Math.Round(damage*1.2);
		}
		if (attacker.HasWorkingItem(Items.LIFEORB)) {
			damage = (int)Math.Round(damage*1.3);
		}
		if (typeMod > 8 && skill >= TrainerAI.highSkill) {
			if ((opponent.HasWorkingItem(Items.CHOPLEBERRY) && type == Types.FIGHTING) || (opponent.HasWorkingItem(Items.COBABERRY) && type == Types.FLYING) || (opponent.HasWorkingItem(Items.KEBIABERRY) && type == Types.POISON) || (opponent.HasWorkingItem(Items.SHUCABERRY) && type == Types.GROUND) || (opponent.HasWorkingItem(Items.CHARTIBERRY) && type == Types.ROCK) || (opponent.HasWorkingItem(Items.TANGABERRY) && type == Types.BUG) || (opponent.HasWorkingItem(Items.KASIBBERRY) && type == Types.GHOST) || (opponent.HasWorkingItem(Items.BABIRIBERRY) && type == Types.STEEL) || (opponent.HasWorkingItem(Items.OCCABERRY) && type == Types.FIRE) || (opponent.HasWorkingItem(Items.PASSHOBERRY) && type == Types.WATER) || (opponent.HasWorkingItem(Items.RINDOBERRY) && type == Types.GRASS) || (opponent.HasWorkingItem(Items.WACANBERRY) && type == Types.ELECTRIC) || (opponent.HasWorkingItem(Items.PAYAPABERRY) && type == Types.PSYCHIC) || (opponent.HasWorkingItem(Items.YACHEBERRY) && type == Types.ICE) || (opponent.HasWorkingItem(Items.HABANBERRY) && type == Types.DRAGON) || (opponent.HasWorkingItem(Items.COLBURBERRY) && type == Types.DARK)) {
				damage = (int)Math.Round(damage*0.5);
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (opponent.HasWorkingItem(Items.CHILANBERRY) && type == Types.NORMAL) {
				damage = (int)Math.Round(damage*0.5);
			}
		}
		// TODO - ModifyDamage()
		if (skill >= TrainerAI.mediumSkill) {
			int c = 0;
			c += attacker.effects[Effects.FocusEnergy];
			if (move.HasHighCriticalRate()) {
				c += 1;
			}
			if (attacker.species == Species.CHANSEY && attacker.HasWorkingItem(Items.LUCKYPUNCH)) {
				c += 2;
			}
			if (attacker.species == Species.FARFETCHD && attacker.HasWorkingItem(Items.STICK)) {
				c += 2;
			}
			if (attacker.HasWorkingAbility(Abilities.SUPERLUCK)) {
				c += 1;
			}
			if (attacker.HasWorkingItem(Items.SCOPELENS)) {
				c += 1;
			}
			if (attacker.HasWorkingItem(Items.RAZORCLAW)) {
				c += 1;
			}
			if (c > 4) {
				c = 4;
			}
			damage += (int)(damage * 0.1 * c);
		}
		return damage;
	}

	public double RoughAccuracy(BattleMove move, Battler attacker, Battler opponent, int skill) {
		double baseaccuracy = move.accuracy;
		double accuracy;
		if (skill >= TrainerAI.mediumSkill) {
			if (GetWeather() == Weather.SUNNYDAY && (move.function == 0x08 || move.function == 0x15)) {
				baseaccuracy = 50;
			}
		}
		int accstage = attacker.stages[Stats.ACCURACY];
		if (opponent.HasWorkingAbility(Abilities.UNAWARE)) {
			accstage = 0;
		}
		accuracy = (accstage >= 0) ? (accstage+3)*100.0/3 : 300.0/(3-accstage);
		int evastage = opponent.stages[Stats.EVASION];
		if (field.effects[Effects.Gravity] > 0) {
			evastage -= 2;
		}
		if (evastage < -6) {
			evastage = -6;
		}
		if (opponent.effects[Effects.Foresight] != 0 || opponent.effects[Effects.MiracleEye] != 0 || move.function == 0xA9 || attacker.HasWorkingAbility(Abilities.UNAWARE)) {
			evastage = 0;
		}
		double evasion = (evastage >= 0) ? (evastage + 3)*100.0/3 : 300.0/(3-evastage);
		accuracy *= baseaccuracy/evasion;
		if (skill >= TrainerAI.mediumSkill) {
			if (attacker.HasWorkingAbility(Abilities.COMPOUNDEYES)) {
				accuracy *= 1.3;
			}
			if (attacker.HasWorkingAbility(Abilities.VICTORYSTAR)) {
				accuracy *= 1.1;
			}
			if (skill >= TrainerAI.highSkill) {
				Battler partner = attacker.Partner();
				if (partner != null && partner.HasWorkingAbility(Abilities.VICTORYSTAR)) {
					accuracy *= 1.1;
				}
			}
			if (attacker.effects[Effects.MicleBerry] != 0) {
				accuracy *= 1.2;
			}
			if (attacker.HasWorkingItem(Items.WIDELENS)) {
				accuracy *= 1.1;
			}
			if (skill >= TrainerAI.highSkill) {
				if (attacker.HasWorkingAbility(Abilities.HUSTLE) && move.baseDamage > 0 && move.IsPhysical(move.GetType(move.type, attacker, opponent))) {
					accuracy *= 0.8;
				}
			}
			if (skill >= TrainerAI.bestSkill) {
				if (opponent.HasWorkingAbility(Abilities.WONDERSKIN) && move.baseDamage == 0 && attacker.IsOpposing(opponent.index)) {
					accuracy /= 2;
				}
				if (opponent.HasWorkingAbility(Abilities.TANGLEDFEET) && opponent.effects[Effects.Confusion] > 0) {
					accuracy /= 1.2;
				}
				if (GetWeather() == Weather.SANDSTORM && opponent.HasWorkingAbility(Abilities.SANDVEIL)) {
					accuracy /= 1.2;
				}
				if (GetWeather() == Weather.HAIL && opponent.HasWorkingAbility(Abilities.SNOWCLOAK)) {
					accuracy /= 1.2;
				}
				if (skill >= TrainerAI.highSkill) {
					if (opponent.HasWorkingItem(Items.BRIGHTPOWDER)) {
						accuracy /= 1.1;
					}
					if (opponent.HasWorkingItem(Items.LAXINCENSE)) {
						accuracy /= 1.1;
					}
				}
			}
		}
		if (accuracy > 100) {
			accuracy = 100;
		}
		if (baseaccuracy == 0) {
			accuracy = 125;
		}
		if (move.function == 0xA5) {
			accuracy = 125;
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (opponent.effects[Effects.LockOn] > 0 && opponent.effects[Effects.LockOnPos] == attacker.index) {
				accuracy = 125;
			}
			if (skill >= TrainerAI.highSkill) {
				if (attacker.HasWorkingAbility(Abilities.NOGUARD) || opponent.HasWorkingAbility(Abilities.NOGUARD)) {
					accuracy = 125;
				}
			}
			if (opponent.effects[Effects.Telekinesis] > 0) {
				accuracy =  125;
			}
			switch (GetWeather()) 
			{
				case Weather.HAIL:
					if (move.function == 0x0D) {
						accuracy = 125;
					}
					break;
				case Weather.RAINDANCE:
					if (move.function == 0x08 || move.function == 0x15) {
						accuracy = 125;
					}
					break;
			}
			if (move.function == 0x70) {
				accuracy = baseaccuracy + attacker.level - opponent.level;
				if (opponent.HasWorkingAbility(Abilities.STURDY)) {
					accuracy = 0;
				}
				if (opponent.level > attacker.level) {
					accuracy = 0;
				}
			}
		}
		return accuracy;
	}

	/************************
	* Choose a move to use. *
	************************/
	public void ChooseMoves(int index) {
		Battler attacker = battlers[index];
		int[] scores = new int[4]{0,0,0,0};
		List<int> targets = new List<int>();
		List<int> myChoices = new List<int>();
		int totalScore = 0;
		int target = -1;
		int skill = 0;
		bool wildBattle = (opponent == null || opponent.Length == 0) && IsOpposing(index);
		if (wildBattle) {
			for (int i=0; i<4; i++) 
			{
				if (CanChooseMove(index, i, false)) {
					scores[i] = 100;
					myChoices.Add(i);
					totalScore += 100;
				}
			}
		} else {
			skill = GetOwner(attacker.index).Skill();
			Battler opponent = attacker.OppositeOpposing();
			if (doublebattle && !opponent.Fainted() && !opponent.Partner().Fainted()) {
				Battler otherOpp = opponent.Partner();
				List<int[]> scoresAndTargets = new List<int[]>();
				for (int i=0; i<4; i++) 
				{
					targets.Add(-1);
				}
				for (int i=0; i<4; i++) 
				{
					if (CanChooseMove(index, i, false)) {
						double score1 = GetMoveScore(attacker.moves[i], attacker, opponent, skill);
						double score2 = GetMoveScore(attacker.moves[i], attacker, otherOpp, skill);
						if ((attacker.moves[i].target&0x20) != 0) {
							if (attacker.Partner().Fainted()) {
								score1 *= 5.0/3;
								score2 *= 5.0/3;
							} else {
								int s = GetMoveScore(attacker.moves[i], attacker, attacker.Partner(), skill);
								if (s >= 140) {
									score1 *= 1.0/3;
									score2 *= 1.0/3;
								} else if (s >= 100) {
									score1 *= 2.0/3;
									score2 *= 2.0/3;
								} else if (s >= 40) {
									score1 *= 4.0/3;
									score2 *= 4.0/3;
								} else {
									score1 *= 5.0/3;
									score2 *= 5.0/3;
								}
							}
						}
						myChoices.Add(i);
						scoresAndTargets.Add(new int[4]{i*2, i, (int)score1, opponent.index});
						scoresAndTargets.Add(new int[4]{i*2+1, i, (int)score2, otherOpp.index});
					}
				}
				scoresAndTargets.Sort(delegate(int[] l1, int[] l2) {
					if (l1[2] == l2[2]) {
						return l1[0].CompareTo(l2[0]);
					} else {
						return l1[2].CompareTo(l2[2]);
					}
				});
				for (int i=0; i<scoresAndTargets.Count; i++) 
				{
					int idx = scoresAndTargets[i][1];
					int thisScore = scoresAndTargets[i][2];
					if (thisScore > 0) {
						if (scores[idx] == 0 || ((scores[idx] == thisScore) || (AIRand(10) < 5))) {
							scores[idx] = thisScore;
							targets[idx] = scoresAndTargets[i][3];
						}
					}
				}
				for (int i=0; i<4; i++) 
				{
					if (scores[i] < 0) {
						scores[i] = 0;
					}
					totalScore += scores[i];
				}
			} else {
				if (doublebattle && opponent.Fainted()) {
					opponent = opponent.Partner();
				}
				for (int i=0; i<4; i++) 
				{
					if (CanChooseMove(index, i, false)) {
						scores[i] = GetMoveScore(attacker.moves[i], attacker, opponent, skill);
						myChoices.Add(i);
					}
					if (scores[i] < 0) {
						scores[i] = 0;
					}
					totalScore += scores[i];
				}
			}
			int maxScore = 0;
			for (int i=0; i<4; i++) {
				if (scores[i] > maxScore) {
					maxScore = scores[i];
				}
			}
			if (!wildBattle && skill >= TrainerAI.mediumSkill) {
				double threshold = (skill >= TrainerAI.bestSkill) ? 1.5 : (skill >= TrainerAI.highSkill) ? 2 : 3;
				int newScore = (skill >= TrainerAI.bestSkill) ? 5 : (skill >= TrainerAI.highSkill) ? 10 : 15;
				for (int i=0; i<4; i++) {
					if (scores[i] > newScore && scores[i] * threshold < maxScore) {
						totalScore -= (scores[i]-newScore);
						scores[i] = newScore;
					}
				}
				maxScore = 0;
				for (int i=0; i<4; i++) {
					if (scores[i] != 0 && scores[i] > maxScore) {
						maxScore = scores[i];
					}
				}
			}
			if (PokemonGlobal.INTERNAL) {
				string x = string.Format("[AI] {0}'s moves: ", attacker.String());
				int j = 0;
				for (int i=0; i<4; i++) {
					if (attacker.moves[i].id != 0) {
						if (j > 0) {
							x += ", ";
						}
						x += Moves.GetName(attacker.moves[i].id) + "=" + scores[i];
						j++;
					}
				}
				Debug.Log(x);
			}
			if (!wildBattle && maxScore > 100) {
				double stdev = StdDev(scores);
				if (stdev >= 40 && AIRand(10) != 0) {
					List<int> preferredMoves = new List<int>();
					for (int i=0; i<4; i++) {
						if (attacker.moves[i].id != 0 && (scores[i] >= maxScore*0.8 || scores[i] >= 200)) {
							preferredMoves.Add(i);
							if (scores[i] == maxScore) {
								preferredMoves.Add(i);
							}
						}
					}
					if (preferredMoves.Count > 0) {
						int i = preferredMoves[AIRand(preferredMoves.Count)];
						Debug.Log(string.Format("[AI] Prefer {0}",Moves.GetName(attacker.moves[i].id)));
						RegisterMove(index, i, false);
						if (targets != null && targets.Count > 0) {
							target = targets[i];
							if (doublebattle && target >= 0) {
								RegisterTarget(index, target);
							}
							return;
						}
					}
				}
			}
			if (!wildBattle && attacker.turnCount > 0) {
				bool badMoves = false;
				if (((maxScore <= 20 && attacker.turnCount > 2) || (maxScore <= 30 && attacker.turnCount > 5)) && AIRand(10) < 8) {
					badMoves = true;
				}
				if (totalScore < 100 && attacker.turnCount > 1) {
					badMoves = true;
					int moveCount = 0;
					for (int i=0; i<4; i++) {
						if (attacker.moves[i].id != 0) {
							if (scores[i] > 0 && attacker.moves[i].baseDamage > 0) {
								badMoves = false;
							}
							moveCount++;
						}
					}
					badMoves = badMoves && AIRand(10) != 0;
				}
				if (badMoves) {
					if (EnemyShouldWithdrawEx(index, true)) {
						if (PokemonGlobal.INTERNAL) {
							Debug.Log(string.Format("[AI] Switching due to terrible moves"));
							Debug.Log(string.Format("Index: {0}, UseMoveChoice: {1}, IndexChoice: {2}, CanChooseNonActive: {3}, NonActivePokemonCount: {4}",index, useMoveChoice[index], indexChoice[index], CanChooseNonActive(index), battlers[index].NonActivePokemonCount()));
						}
					}
					return;
				}
			}
			if (maxScore <= 0) {
				if (myChoices.Count > 0) {
					RegisterMove(index, myChoices[AIRand(myChoices.Count)], false);
				} else {
					AutoChooseMove(index);
				}
			} else {
				int randnum = AIRand(totalScore);
				int cumtotal = 0;
				for (int i=0; i<4; i++) {
					cumtotal += scores[i];
					if (randnum < cumtotal) {
						RegisterMove(index, i, false);
						if (targets != null && targets.Count > 0) {
							target = targets[i];
						}
					}
				}
			}
			if (moveChoice[index] != null) {
				Debug.Log(string.Format("[AI] Will use {0}", moveChoice[index].name));
			}
			if (doublebattle && target >= 0) {
				RegisterTarget(index, target);
			}
		}
	}

	/*****************************************************************
	* Decide whether the opponent should mega evolve their PokÃ©mon. *
	*****************************************************************/
	public bool EnemyShouldMegaEvolve(int index) {
		return CanMegaEvolve(index);
	}

	/******************************************************************
	* Decide whether the opponent should use an item on the PokÃ©mon. *
	******************************************************************/
	public bool EnemyShouldUseItem(int index) {
		int item = EnemyItemToUse(index);
		if (item > 0) {
			RegisterItem(index, item, -1);
			return true;
		}
		return false;
	}

	public bool EnemyItemAlreadyUsed(int index, int item, int[] items) {
		if (useMoveChoice[1] == 3 && indexChoice[1] == item) {
			int qty = 0;
			for (int i=0; i<items.Length; i++) {
				if (items[i] == item) {
					qty++;
				}
			}
			if (qty <= 1) {
				return true;
			}
		}
		return false;
	}

	public int EnemyItemToUse(int index) {
		if (!internalbattle) {
			return 0;
		}
		int[] items = GetOwnerItems(index);
		if (items == null || items.Length == 0) {
			return 0;
		}
		Battler battler = battlers[index];
		if (battler.Fainted() || battler.effects[Effects.Embargo] > 0) {
			return 0;
		}
		bool hasHpItem = false;
		for (int i=0; i<items.Length; i++) {
			if (EnemyItemAlreadyUsed(index, items[i], items)) {
				continue;
			}
			if (items[i] == Items.POTION || items[i] == Items.SUPERPOTION || items[i] == Items.HYPERPOTION || items[i] == Items.MAXPOTION || items[i] == Items.FULLRESTORE) {
				hasHpItem = true;
			}
		}
		for (int i=0; i<items.Length; i++) {
			if (EnemyItemAlreadyUsed(index, items[i], items)) {
				continue;
			}
			if (items[i] == Items.FULLRESTORE) {
				if (battler.hp <= battler.totalHP/4) {
					return items[i];
				}
				if (battler.hp <= battler.totalHP/2 && AIRand(10) < 3) {
					return items[i];
				}
				if (battler.hp <= battler.totalHP*2/3 && (battler.status > 0 || battler.effects[Effects.Confusion] != 0) && AIRand(10)<3) {
					return items[i];
				}
			} else if (items[i] == Items.POTION || items[i] == Items.SUPERPOTION || items[i] == Items.HYPERPOTION || items[i] == Items.MAXPOTION) {
				if (battler.hp <= battler.totalHP/4) {
					return items[i];
				}
				if (battler.hp <= battler.totalHP/2 && AIRand(10)<3) {
					return items[i];
				}
			} else if (items[i] == Items.FULLHEAL) {
				if (!hasHpItem && (battler.status > 0 || battler.effects[Effects.Confusion] != 0)) {
					return items[i];
				}
			} else if (items[i] == Items.XATTACK || items[i] == Items.XDEFENSE || items[i] == Items.XSPEED || items[i] == Items.XSPATK || items[i] == Items.XSPDEF || items[i] == Items.XACCURACY) {
				int stat = 0;
				if (items[i] == Items.XATTACK) {
					stat = Stats.ATTACK;
				}
				if (items[i] == Items.XDEFENSE) {
					stat = Stats.DEFENSE;
				}
				if (items[i] == Items.XSPEED) {
					stat = Stats.SPEED;
				}
				if (items[i] == Items.XSPATK) {
					stat = Stats.SPATK;
				}
				if (items[i] == Items.XSPDEF) {
					stat = Stats.SPDEF;
				}
				if (items[i] == Items.XACCURACY) {
					stat = Stats.ACCURACY;
				}
				if (stat > 0 && !battler.TooHigh(stat)) {
					if (AIRand(10)<3-battler.stages[stat]) {
						return items[i];
					}
				}
			}
		}
		return 0;
	}

	/******************************************************
	* Decide whether the opponent should switch PokÃ©mon. *
	******************************************************/
	public bool EnemyShouldWithdraw(int index) {
		return EnemyShouldWithdrawEx(index, false);
	}

	public bool EnemyShouldWithdrawEx(int index, bool alwaysSwitch) {
		if (opponent.Length == 0) {
			return false;
		}
		bool shouldSwitch = alwaysSwitch;
		int batonPass = -1;
		int moveType = -1;
		int skill = GetOwner(index).Skill();
		if (opponent.Length > 0 && !shouldSwitch && battlers[index].turnCount > 0) {
			if (skill >= TrainerAI.highSkill) {
				Battler opp = battlers[index].OppositeOpposing();
				if (opp.Fainted()) {
					opp = opp.Partner();
				}
				if (!opp.Fainted() && opp.lastMoveUsed > 0 && Math.Abs(opp.level-battlers[index].level)<=6) {
					Moves.InternalMove move = new Moves.Move(opp.lastMoveUsed).moveData;
					int typemod = TypeModifier(Types.GetValueFromName(move.Type), battlers[index], battlers[index]);
					moveType = Types.GetValueFromName(move.Type);
					if (move.Power > 70 && typemod > 8) {
						shouldSwitch = AIRand(100) < 30;
					} else if (move.Power < 50 && typemod > 8) {
						shouldSwitch = AIRand(100) < 20;
					}
				}
			}
		}
		if (!CanChooseMove(index, 0, false) && !CanChooseMove(index, 1, false) && !CanChooseMove(index, 2, false) && !CanChooseMove(index, 3, false) && battlers[index].turnCount > 5) {
			shouldSwitch = true;
		}
		if (skill >= TrainerAI.highSkill && battlers[index].effects[Effects.PerishSong] != 1) {
			for (int i=0; i<4; i++) {
				BattleMove move = battlers[index].moves[i];
				if (move.id != 0 && CanChooseMove(index, i, false) && move.function == 0xED) {
					batonPass = i;
					break;
				}
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (battlers[index].status == Statuses.POISON && battlers[index].statusCount > 0) {
				int toxicHP = battlers[index].totalHP/16;
				int nextToxicHP = toxicHP*(battlers[index].effects[Effects.Toxic]+1);
				if (nextToxicHP >= battlers[index].hp && toxicHP < battlers[index].hp && AIRand(100) < 80) {
					shouldSwitch = true;
				}
			}
		}
		if (skill >= TrainerAI.mediumSkill) {
			if (battlers[index].effects[Effects.Encore] > 0) {
				int scoreSum = 0;
				int scoreCount = 0;
				Battler attacker = battlers[index];
				int encoreIndex = battlers[index].effects[Effects.EncoreIndex];
				if (!attacker.Opposing1().Fainted()) {
					scoreSum += GetMoveScore(attacker.moves[encoreIndex], attacker, attacker.Opposing1(), skill);
					scoreCount += 1;
				}
				if (!attacker.Opposing2().Fainted()) {
					scoreSum += GetMoveScore(attacker.moves[encoreIndex], attacker, attacker.Opposing2(), skill);
					scoreCount += 1;
				}
				if (scoreCount > 0 && scoreSum/scoreCount <= 20 && AIRand(10) < 8) {
					shouldSwitch = true;
				}
			}
		}
		if (skill >= TrainerAI.highSkill) {
			if (!doublebattle && !battlers[index].OppositeOpposing().Fainted()) {
				Battler opp = battlers[index].OppositeOpposing();
				if ((opp.effects[Effects.HyperBeam] > 0 || (opp.HasWorkingAbility(Abilities.TRUANT) && opp.effects[Effects.Truant] != 0)) && AIRand(100) < 80) {
					shouldSwitch = false;
				}
			}
		}
		if (rules["suddendeath"] != 0) {
			if (battlers[index].hp <= (battlers[index].totalHP/4) && AIRand(10)<3 && battlers[index].turnCount > 0) {
				shouldSwitch = true;
			}
			if (battlers[index].hp <= (battlers[index].totalHP/2) && AIRand(10)<8 && battlers[index].turnCount > 0) {
				shouldSwitch = true;
			}
		}
		if (battlers[index].effects[Effects.PerishSong]==1) {
			shouldSwitch = true;
		}
		if (shouldSwitch) {
			List<int> list = new List<int>();
			Battler[] party = Party(index);
			for (int i=0; i<party.Length; i++) {
				if (CanSwitch(index, i, false)) {
					if (battlers[index].effects[Effects.PerishSong] != 1) {
						int spikes = battlers[index].OwnSide().effects[Effects.Spikes];
						if ((spikes == 1 && party[i].hp <= (party[i].totalHP/8)) || (spikes == 2 && party[i].hp <= (party[i].totalHP/6)) || (spikes == 3 && party[i].hp <= (party[i].totalHP/4))) {
							if (!party[i].HasType(Types.FLYING) && !party[i].HasWorkingAbility(Abilities.LEVITATE)) {
								continue;
							}
						}
					}
					if (moveType >= 0 && TypeModifier(moveType, battlers[index], battlers[index]) == 0) {
						int weight = 65;
						if (TypeModifier2(party[i], battlers[index].OppositeOpposing()) > 8) {
							weight = 85;
						}
						if (AIRand(100) < weight) {
							list.Insert(0, i);
						}
					} else if (moveType >= 0 && TypeModifier(moveType, battlers[index], battlers[index]) < 8) {
						int weight = 40;
						if (TypeModifier2(party[i], battlers[index].OppositeOpposing()) > 8) {
							weight = 60;
						}
						if (AIRand(100) < weight) {
							list.Insert(0, i);
						}
					} else {
						list.Add(i);
					}
				}
			}
			if (list.Count > 0) {
				if (batonPass != -1) {
					if (!RegisterMove(index, batonPass, false)) {
						return RegisterSwitch(index, list[0]);
					}
					return true;
				} else {
					return RegisterSwitch(index, list[0]);
				}
			}
		}
		return false;
	}

	public int DefaultChooseNewEnemy(int index, Battler[] party) {
		List<int> enemies = new List<int>();
		for (int i=0; i<party.Length; i++) {
			if (CanSwitchLax(index, i, false)) {
				enemies.Add(i);
			}
		}
		if (enemies.Count > 0) {
			return ChooseBestNewEnemy(index, party, enemies.ToArray());
		}
		return -1;
	}

	public int ChooseBestNewEnemy(int index, Battler[] party, int[] enemies) {
		if (enemies.Length == 0) {
			return -1;
		}
		if (PokemonGlobal.PkmnTemp == null) {
			PokemonGlobal.PkmnTemp = new PokemonTemp();
		}
		Battler o1 = battlers[index].Opposing1();
		Battler o2 = battlers[index].Opposing2();
		if (o1 != null && o1.Fainted()) {
			o1 = null;
		}
		if (o2 != null && o2.Fainted()) {
			o2 = null;
		}
		int best = -1;
		int bestSum = 0;
		for (int i=0; i<enemies.Length; i++) {
			int e = enemies[i];
			Pokemon pkmn = party[e].pokemon;
			int sum = 0;
			for (int m=0; m<pkmn.moves.Length; m++) {
				if (pkmn.moves[m].Id == 0) {
					continue;
				}
				Moves.InternalMove md = new Moves.Move(pkmn.moves[m].Id).moveData;
				if (md.Power == 0) {
					continue;
				}
				if (o1 != null) {
					sum += Types.GetCombinedEffectiveness(Types.GetValueFromName(md.Type), o1.type1, o1.type2, o1.effects[Effects.Type3]);
				}
				if (o2 != null) {
					sum += Types.GetCombinedEffectiveness(Types.GetValueFromName(md.Type), o2.type1, o2.type2, o2.effects[Effects.Type3]);
				}
			}
			if (best == -1 || sum > bestSum) {
				best = e;
				bestSum = sum;
			}
		}
		return best;
	}

	/********************
	* Choose an action. *
	********************/
	public void DefaultChooseEnemyCommand(int index) {
		if (!CanShowFightMenu(index)) {
			if (EnemyShouldUseItem(index)) {
				return;
			}
			if (EnemyShouldWithdraw(index)) {
				return;
			}
			AutoChooseMove(index);
			return;
		} else {
			if (EnemyShouldUseItem(index)) {
				return;
			}
			if (EnemyShouldWithdraw(index)) {
				return;
			}
			if (AutoFightMenu(index)) {
				return;
			}
			if (EnemyShouldMegaEvolve(index)) {
				RegisterMegaEvolution(index);
			}
			ChooseMoves(index);
		}
	}

	/*******************
	* Other functions. *
	*******************/
	public bool DbgPlayerOnly(int idx) {
		if (!PokemonGlobal.INTERNAL) {
			return true;
		}
		return OwnedByPlayer(idx);
	}

	public double StdDev(int[] scores) {
		int n = 0;
		double sum = 0;
		for (int i=0; i<scores.Length; i++) {
			sum += scores[i];
			n += 1;
		}
		if (n == 0) {
			return 0;
		}
		double mean = ((double)sum)/((double)n);
		double varianceTimesN = 0;
		for (int i=0; i<scores.Length; i++) {
			if (scores[i] > 0) {
				double deviation = scores[i] - mean;
				varianceTimesN += deviation * deviation;
			}
		}
		return Math.Sqrt(varianceTimesN/n);
	}
}

public static class TrainerAI {
	public const int minimumSkill = 1;
	public const int mediumSkill = 32;
	public const int highSkill = 48;
	public const int bestSkill = 100;
}
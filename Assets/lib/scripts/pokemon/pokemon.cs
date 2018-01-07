using System;
using System.Collections.Generic;

public class Pokemon {
	public int totalHP;
	public int attack;
	public int defense;
	public int speed;
	public int spatk;
	public int spdef;
	public int[] iv;
	public int[] ev;
	public int species;
	public int personalID;
	public int trainerID;
	public int hp;
	public int pokerus;
	public int item;
	public int itemRecycle;
	public int itemInitial;
	public bool belch;
	public Mail mail;
	public Pokemon fused;
	public string name;
	public int exp;
	public int happiness;
	public int status;
	public int statusCount;
	public int eggSteps;
	public Moves.Move[] moves;
	public List<int> firstMoves;
	public int ballUsed;
	public int markings;
	public int obtainMode;
	public int obtainMap;
	public string obtainText;
	public int obtainLevel;
	public int hatchedMap;
	public int language;
	public string ot;
	public int otGender;
	public int abilityFlag;
	public int genderFlag;
	public int natureFlag;
	public int shinyFlag; // 0 is false, 1 is true
	public List<int> ribbons;
	public int cool;
	public int beauty;
	public int cute;
	public int smart;
	public int tough;
	public int sheen;
	public DateTime timeRecieved;
	public DateTime timeEggHatched;
	public DateTime formTime;
	public int forcedForm;
	public int form;

	public const int EV_LIMIT = 510;
	public const int EV_STAT_LIMIT = 252;
	public const int NAME_LIMIT = 10;

	public int OTGender() {
		if (otGender < 0 || otGender > 3) {
			otGender = 3;
		}
		return otGender;
	}

	public bool IsForeign(BattleTrainer trainer) {
		return trainerID != trainer.id || ot != trainer.name;
	}

	public int PublicID() {
		return trainerID&0xFFFF;
	}

	public int ObtainLevel() {
		if (obtainLevel < 0) {
			obtainLevel = 0;
		}
		return obtainLevel;
	}

	public DateTime GetTimeRecieved() {
		if (timeRecieved == DateTime.MinValue) {
			timeRecieved = new DateTime(2000, 1, 1);
		}
		return timeRecieved;
	}

	public void SetTimeRecieved(DateTime t) {
		timeRecieved = t;
	}

	public DateTime GetTimeEggHatched() {
		if (timeEggHatched == DateTime.MinValue) {
			timeEggHatched = new DateTime(2000, 1, 1);
		}
		return timeEggHatched;
	}

	public void SetTimeEggHatched(DateTime t) {
		timeEggHatched = t;
	}

	public int Level(int v=-1) {
		if (v > 0 && v <= Experience.MAXLEVEL) {
			exp = Experience.GetStartExperience(v, GrowthRate());
		}
		return Experience.GetLevelFromExperience(exp, GrowthRate());
	}

	public bool Egg() {
		return eggSteps > 0;
	}

	public int GrowthRate() {
		switch (Species.GetSpecies(species).GrowthRate) 
		{
			case "Medium":
			case "MediumFast":
				return 0;
			case "Erratic":
				return 1;
			case "Fluctuating":
				return 2;
			case "Parabolic":
			case "MediumSlow":
				return 3;
			case "Fast":
				return 4;
			case "Slow":
				return 5;
			default:
				return -1;
		}
	}

	public int BaseExp() {
		return Species.GetSpecies(species).BaseEXP;
	}

	public int Gender() {
		if (genderFlag != -1) {
			return genderFlag;
		}
		int lowbyte = personalID&0xFF;
		switch (Species.GetSpecies(species).GenderRate) 
		{
			case "AlwaysMale":
				return 0;
			case "AlwaysFemale":
				return 1;
			case "Genderless":
				return 2;
			case "FemaleOneEighth":
				return isFemale(lowbyte, 0.125) ? 1 : 0;
			case "Female25Percent":
				return isFemale(lowbyte, 0.25) ? 1 : 0;
			case "Female50Percent":
				return isFemale(lowbyte, 0.5) ? 1 : 0;
			case "Female75Percent":
				return isFemale(lowbyte, 0.75) ? 1 : 0;
			case "FemaleSevenEighths":
				return isFemale(lowbyte, 0.875) ? 1 : 0;
		}
		return 2;
	}

	public bool isFemale(int lowbyte, double femalePercentage) {
		return lowbyte <= 255*femalePercentage;
	}

	public bool IsSingleGendered() {
		return Species.GetSpecies(species).GenderRate == "AlwaysMale" || Species.GetSpecies(species).GenderRate == "AlwaysFemale" || Species.GetSpecies(species).GenderRate == "Genderless";
	}

	public bool IsFemale() {
		return Gender()==1;
	}

	public bool IsMale() {
		return Gender()==0;
	}

	public bool IsGenderless() {
		return Gender()==2;
	}

	public void SetGender(int value) {
		if (!IsSingleGendered()) {
			genderFlag = value;
		}
	}

	public void MakeMale() {
		SetGender(0);
	}

	public void MakeFemale() {
		SetGender(1);
	}

	public int AbilityIndex() {
		return abilityFlag == -1 ? personalID&1 : abilityFlag;
	}

	public int Ability() {
		int abil = AbilityIndex();
		List<int> abils = GetAbilityList();
		int ret1 = 0;
		int ret2 = 0;
		for (int i=0; i<abils.Count; i++) 
		{
			if (abils[i] <= 0) {
				continue;
			}
			if (i == abil) {
				return abils[i];
			}
			if (i == 0) {
				ret1 = abils[i];
			}
			if (i == 1) {
				ret2 = abils[i];
			}
		}
		if (abil >= 2) {
			abil = personalID&1;
		}
		if (abil == 1 && ret2 > 0) {
			return ret2;
		}
		return ret1;
	}

	public bool HasAbility(int a=0) {
		if (a == 0) {
			return Ability() > 0;
		} else {
			return Ability() == a;
		}
	}

	public void SetAbility(int a) {
		abilityFlag = a;
	}

	public bool HasHiddenAbility() {
		int abil = AbilityIndex();
		return abil != -1 && abil >= 2;
	}

	public List<int> GetAbilityList() {
		List<int> abils = new List<int>();
		string[] abilities = Species.GetSpecies(species).Abilities;
		for (int i=0; i<abilities.Length; i++) 
		{
			abils.Add(Abilities.GetValueFromName(abilities[i]));
		}
		return abils;
	}

	public int Nature() {
		if (natureFlag > -1) {
			return natureFlag;
		}
		return personalID%25;
	}

	public bool HasNature(int n=-1) {
		if (n < 0) {
			return Nature() >= 0;
		} else {
			return Nature() == n;
		}
	}

	public void SetNature(int n) {
		natureFlag = n;
		CalcStats();
	}

	public bool IsShiny() {
		if (shinyFlag > -1) {
			return shinyFlag == 1;
		}
		int a = personalID^trainerID;
		int b = a&0xFFFF;
		int c = (a>>16)&0xFFFF;
		int d = b^c;
		return d<Settings.SHINY_POKEMON_CHANCE;
	}

	public void MakeShiny() {
		shinyFlag = 1;
	}

	public void MakeNotShiny() {
		shinyFlag = 0;
	}

	public int Pokerus() {
		return pokerus;
	}

	public int PokerusStrain() {
		return pokerus/16;
	}

	public int PokerusStage() {
		if (pokerus == 0) {
			return 0;
		}
		if (pokerus > 0 && (pokerus%16) == 0) {
			return 2;
		}
		return 1;
	}

	public void GivePokerus(int strain=0) {
		if (PokerusStage() == 2) {
			return;
		}
		if (strain <= 0 || strain >= 16) {
			strain = 1 + Battle.Rand(15);
		}
		int time = 1 + (strain%4);
		pokerus = time;
		pokerus |= strain << 4;
	}

	public void ResetPokerusTime() {
		if (pokerus == 0) {
			return;
		}
		int strain = pokerus%16;
		int time = 1 + (strain%4);
		pokerus = time;
		pokerus |= strain << 4;
	}

	public void LowerPokerusCount() {
		if (PokerusStage() != 1) {
			return;
		}
		pokerus--;
	}

	public bool HasType(int t) {
		return Type1() == t || Type2() == t;
	}

	public int Type1() {
		return Types.GetValueFromName(Species.GetSpecies(species).Type1);
	}

	public int Type2() {
		return Types.GetValueFromName(Species.GetSpecies(species).Type2);
	}

	public int NumMoves() {
		int ret = 0;
		for (int i=0; i<4; i++) 
		{
			if (moves[i].Id != 0) {
				ret++;
			}
		}
		return ret;
	}

	public bool HasMove(int id) {
		if (id <= 0) {
			return false;
		}
		for (int i=0; i<4; i++) 
		{
			if (moves[i].Id == 0) {
				return true;
			}
		}
		return false;
	}

	public bool KnowsMove(int id) {
		return HasMove(id);
	}

	public List<int[]> GetMoveList() {
		List<int[]> movelist = new List<int[]>();
		Species.LevMove[] ml = Species.GetSpecies(species).Moves;
		for (int i=0; i<ml.Length; i++) 
		{
			movelist.Add(new int[2]{ml[i].Level, Moves.GetValueFromName(ml[i].Move)});
		}
		return movelist;
	}

	public void ResetMoves() {
		List<int[]> moves = GetMoveList();
		List<int> movelist = new List<int>();
		for (int i=0; i<moves.Count; i++) 
		{
			if (moves[i][0] <= Level() && !movelist.Contains(moves[i][1])) {
				movelist.Add(moves[i][1]);
			}
		}
		int listEnd = movelist.Count-4;
		if (listEnd < 0) {
			listEnd = 0;
		}
		int j = 0;
		for (int i= listEnd; i<listEnd+4; i++) 
		{
			int moveId = (i >= movelist.Count) ? 0 : movelist[i];
			this.moves[j] = new Moves.Move(moveId);
			j++;
		}
	}

	public void LearnMove(int id) {
		if (id <= 0) {
			return;
		}
		for (int i=0; i<4; i++) 
		{
			if (moves[i].Id == id) {
				int j = i + 1;
				while (j < 4) {
					if (moves[j].Id == 0) {
						break;
					}
					Moves.Move tmp = moves[j];
					moves[j] = moves[j-1];
					moves[j-1] = tmp;
					j++;
				}
				return;
			}
		}
		for (int i=0; i<4; i++) 
		{
			if (moves[i].Id == 0) {
				moves[i] = new Moves.Move(id);
				return;
			}
		}
		moves[0] = moves[1];
		moves[1] = moves[2];
		moves[2] = moves[3];
		moves[3] = new Moves.Move(id);
	}

	public void DeleteMove(int id) {
		if (id <= 0) {
			return;
		}
		List<Moves.Move> newMoves = new List<Moves.Move>();
		for (int i=0; i<4; i++) {
			if (moves[i].Id != id) {
				newMoves.Add(moves[i]);
			}
		}
		newMoves.Add(new Moves.Move(0));
		for (int i=0; i<4; i++) {
			moves[i] = newMoves[i];
		}
	}

	public void DeleteMoveAtIndex(int idx) {
		List<Moves.Move> newMoves = new List<Moves.Move>();
		for (int i=0; i<4; i++) {
			if (i != idx) {
				newMoves.Add(moves[i]);
			}
		}
		newMoves.Add(new Moves.Move(0));
		for (int i=0; i<4; i++) {
			moves[i] = newMoves[i];
		}
	}

	public void DeleteAllMoves() {
		for (int i=0; i<4; i++) {
			moves[i] = new Moves.Move(0);
		}
	}

	public void RecordFirstMoves() {
		firstMoves = new List<int>();
		for (int i=0; i<4; i++) {
			if (moves[i].Id > 0) {
				firstMoves.Add(moves[i].Id);
			}
		}
	}

	public void AddFirstMove(int id) {
		if (id > 0 && !firstMoves.Contains(id)) {
			firstMoves.Add(id);
		}
	}

	public void RemoveFirstMove(int id) {
		if (id > 0) {
			for (int i=0; i<4; i++) 
			{
				if (firstMoves[i] == id) {
					firstMoves.RemoveAt(i);
					return;
				}
			}
		}
	}

	public void ClearFirstMoves() {
		firstMoves = new List<int>();
	}

	public bool IsCompatibleWithMove(int id) {
		int[] v = MultipleForms.GetMoveCompatibility(species, this);
		if (v != null) {
			for (int i=0; i<v.Length; i++) {
				if (v[i] == id) {
					return true;
				}
			}
		}
		return MultipleForms.SpeciesCombatible(species, id);
	}

	public int Cool() {
		return cool;
	}

	public int Beauty() {
		return beauty;
	}

	public int Cute() {
		return cute;
	}

	public int Smart() {
		return smart;
	}

	public int Tough() {
		return tough;
	}

	public int Sheen() {
		return sheen;
	}

	public int RibbonCount() {
		if (ribbons == null) {
			ribbons = new List<int>();
		}
		return ribbons.Count;
	}

	public bool HasRibbon(int id) {
		if (ribbons == null) {
			ribbons = new List<int>();
		}
		if (id <= 0) {
			return false;
		}
		return ribbons.Contains(id);
	}

	public void GiveRibbon(int id) {
		if (id <= 0) {
			return;
		}
		if (!HasRibbon(id)) {
			ribbons.Add(id);
		}
	}

	public int UpgradeRibbon(List<int> ids) {
		if (ribbons == null) {
			ribbons = new List<int>();
		}
		for (int i=0; i<ids.Count-1; i++) 
		{
			for (int j=0; j<ribbons.Count; j++) 
			{
				int thisRibbon = ids[i];
				if (ribbons[j] == thisRibbon) {
					int nextRibbon = ids[i+1];
					ribbons[j] = nextRibbon;
					return nextRibbon;
				}
			}
		}
		if (!HasRibbon(ids[ids.Count-1])) {
			int firstRibbon = ids[0];
			GiveRibbon(firstRibbon);
			return firstRibbon;
		}
		return 0;
	}

	public void TakeRibbon(int id) {
		if (ribbons == null || ribbons.Count == 0) {
			return;
		}
		for (int i=0; i<ribbons.Count; i++) 
		{
			if (ribbons[i] == id) {
				ribbons.RemoveAt(i);
				break;
			}
		}
	}

	public void ClearAllRibbons() {
		ribbons = new List<int>();
	}

	public bool HasItem(int id=0) {
		if (id == 0) {
			return item > 0;
		} else {
			return item == id;
		}
	}

	public void SetItem(int id) {
		item = id;
	}

	public int[][] WildHoldItems() {
		string[] common = Species.GetSpecies(species).WildItemCommon;
		string[] uncommon = Species.GetSpecies(species).WildItemUncommon;
		string[] rare = Species.GetSpecies(species).WildItemRare;
		int[][] ret = new int[3][];
		ret[0] = new int[common.Length];
		ret[1] = new int[uncommon.Length];
		ret[2] = new int[rare.Length];
		for (int i=0; i<common.Length; i++) 
		{
			ret[0][i] = Items.GetValueFromName(common[i]);
		}
		for (int i=0; i<uncommon.Length; i++) 
		{
			ret[1][i] = Items.GetValueFromName(uncommon[i]);
		}
		for (int i=0; i<rare.Length; i++) 
		{
			ret[2][i] = Items.GetValueFromName(rare[i]);
		}
		return ret;
	}

	public Mail Mail() {
		if (mail == null) {
			return null;
		}
		if (mail.item == 0 || !HasItem() || mail.item != item) {
			mail = null;
			return null;
		}
		return mail;
	}

	public string SpeciesName() {
		return Species.GetName(species);
	}

	public int Language() {
		return language;
	}

	public int Markings() {
		if (markings < 0) {
			markings = 0;
		}
		return markings;
	}

	public string UnownShape() {
		string[] forms = new string[28]{"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z","?","!"};
		return forms[GetForm()];
	}

	public float Height() {
		return Species.GetSpecies(species).Height;
	}

	public float Weight() {
		return Species.GetSpecies(species).Weight;
	}

	public int[] EvYield() {
		Species.BStats stats = Species.GetSpecies(species).EffortPoints;
		return new int[6]{stats.HP, stats.Attack, stats.Defense, stats.Speed, stats.SpecialAttack, stats.SpecialDefense};
	}

	public void SetHP(int v) {
		if (v < 0) {
			v = 0;
		}
		hp = v;
		if (hp == 0) {
			status = 0;
			statusCount = 0;
		}
	}

	public bool Fainted() {
		return !Egg() && hp <= 0;
	}

	public bool IsFainted() {
		return Fainted();
	}

	public void HealHP() {
		if (Egg()) {
			return;
		}
		hp = totalHP;
	}

	public void HealStatus() {
		if (Egg()) {
			return;
		}
		status = 0;
		statusCount = 0;
	}

	public void HealPP(int index=-1) {
		if (Egg()) {
			return;
		}
		if (index >= 0) {
			moves[index].pp = moves[index].TotalPP();
		} else {
			for (int i=0; i<4; i++) 
			{
				moves[i].pp = moves[i].TotalPP();
			}
		}
	}

	public void Heal() {
		if (Egg()) {
			return;
		}
		HealHP();
		HealStatus();
		HealPP();
	}

	public void ChangeHappiness(string method) {
		int gain = 0;
		bool luxury = false;
		switch (method) 
		{
			case "walking":
				gain = 1;
				if (happiness < 200) {
					gain++;
				}
				if (obtainMap == PokemonGlobal.Map.MapID) {
					gain++;
				}
				luxury = true;
				break;
			case "levelup":
				gain = 2;
				if (happiness < 200) {
					gain = 3;
				}
				if (happiness < 100) {
					gain = 5;
				}
				luxury = true;
				break;
			case "groom":
				gain = 4;
				if (happiness < 200) {
					gain = 10;
				}
				luxury = true;
				break;
			case "faint":
				gain = -1;
				break;
			case "vitamin":
				gain = 2;
				if (happiness < 200) {
					gain = 3;
				}
				if (happiness < 100) {
					gain = 5;
				}
				break;
			case "evberry":
				gain = 2;
				if (happiness < 200) {
					gain = 5;
				}
				if (happiness < 100) {
					gain = 10;
				}
				break;
			case "powder":
				gain = -10;
				if (happiness < 200) {
					gain = -5;
				}
				break;
			case "energyroot":
				gain = -15;
				if (happiness < 200) {
					gain = -10;
				}
				break;
			case "revivalherb":
				gain = -20;
				if (happiness < 200) {
					gain = -15;
				}
				break;
			default:
				Messaging.Message("Unknown happiness-changing method.");
				break;
		}
		if (luxury && ballUsed == Items.GetBallType(Items.LUXURYBALL)) {
			gain++;
		}
		if (item == Items.SOOTHEBELL && gain > 0) {
			gain = (int)(gain*1.5);
		}
		happiness += gain;
		happiness = (int)Math.Max(Math.Min(255, happiness), 0);
	}

	public int[] BaseStats() {
		Species.BStats stats = Species.GetSpecies(species).BaseStats;
		return new int[6]{stats.HP, stats.Attack, stats.Defense, stats.Speed, stats.SpecialAttack, stats.SpecialDefense};
	}

	public int CalcHP(int b, int level, int iv, int ev) {
		if (b == 1) {
			return 1;
		}
		return (int)((b*2.0+iv+(ev>>2))*level/100.0)+level+10;
	}

	public int CalcStat(int b, int level, int iv, int ev, int pv) {
		return (int)((((int)((b*2.0+iv+(ev>>2))*level/100.0))+5)*pv/100.0);
	}

	public void CalcStats() {
		int nature = Nature();
		int[] stats = new int[6]{0,0,0,0,0,0};
		int[] pvalues = new int[5]{100, 100, 100, 100, 100};
		int nd5 = nature/5;
		int nm5 = nature%5;
		if (nd5 != nm5) {
			pvalues[nd5] = 110;
			pvalues[nm5] = 90;
		}
		int level = Level();
		int[] bs = BaseStats();
		for (int i=0; i<6; i++) 
		{
			int b = bs[i];
			if (i == Stats.HP) {
				stats[i] = CalcHP(b, level, iv[i], ev[i]);
			} else {
				stats[i] = CalcStat(b, level, iv[i], ev[i], pvalues[i-1]);
			}
		}
		int diff = totalHP-hp;
		totalHP = stats[0];
		hp = totalHP-diff;
		if (hp <= 0) {
			hp = 0;
		}
		if (hp > totalHP) {
			hp = totalHP;
		}
		attack = stats[1];
		defense = stats[2];
		speed = stats[3];
		spatk = stats[4];
		spdef = stats[5];
	}

	public Pokemon(int species, int level, BattleTrainer trainer, bool withMoves=true) {
		if (species < 1 || species > Species.MaxValue()) {
			throw new Exception(string.Format("The species number (#{0} of{1}) is invalid.", species, Species.MaxValue()));
		}
		this.species = species;
		this.name = Species.GetName(species);
		personalID = Battle.Rand(256);
		personalID |= Battle.Rand(256)<<8;
		personalID |= Battle.Rand(256)<<16;
		personalID |= Battle.Rand(256)<<24;
		hp = 1;
		totalHP = 1;
		ev = new int[6]{0,0,0,0,0,0};
		iv = new int[6]{Battle.Rand(32),Battle.Rand(32),Battle.Rand(32),Battle.Rand(32),Battle.Rand(32),Battle.Rand(32)};
		moves = new Moves.Move[4]{new Moves.Move(0),new Moves.Move(0),new Moves.Move(0),new Moves.Move(0)};
		status = 0;
		statusCount = 0;
		item = 0;
		mail = null;
		fused = null;
		ribbons = new List<int>();
		ballUsed = 0;
		eggSteps = 0;
		if (trainer != null) {
			trainerID = trainer.id;
			ot = trainer.name;
			otGender = trainer.Gender();
			language = trainer.language;
		} else {
			trainerID = 0;
			ot = "";
			otGender = 2;
		}
		if (PokemonGlobal.Map != null) {
			obtainMap = PokemonGlobal.Map.MapID;
		} else {
			obtainMap = 0;
		}
		obtainText = "";
		obtainLevel = level;
		obtainMode = 0;
		if (Globals.getSwitch(Globals.FATEFUL_ENCOUNTER_SWITCH)) {
			obtainMode = 4;
		}
		hatchedMap = 0;
		timeRecieved = DateTime.Now;
		Level(level);
		CalcStats();
		happiness = Species.GetSpecies(species).Happiness;
		if (withMoves) {
			ResetMoves();
		}
		int f = MultipleForms.GetFormOnCreation(species, this);
		if (f >= 0) {
			SetForm(f);
			ResetMoves();
		}
	}

	public static Pokemon GenPokemon(int species, int level, BattleTrainer owner) {
		if (owner == null) {
			owner = PokemonGlobal.Trainer;
		}
		return new Pokemon(species, level, owner);
	}

	public int GetForm() {
		if (forcedForm > 0) {
			return forcedForm;
		}
		int v = MultipleForms.GetForm(species, this);
		if (v >= 0) {
			if (form < 0 || v != form) {
				SetForm(v);
			}
		}
		return form;
	}

	public void SetForm(int v) {
		form = v;
		MultipleForms.OnSetForm(species, this);
		CalcStats();
		Utilities.SeenForm(this);
	}

	public void SetFormNoCall(int v) {
		form = v;
		CalcStats();
	}

	public int FormSpecies() {
		return Utilities.GetFormSpeciesFromForm(species, GetForm());
	}

	public Species.InternalForm GetMegaForm(bool itemOnly=false) {
		Species.InternalForm f = Species.GetForm(Species.GetSpecies(species).InternalName, 1);
		if (f == null) {
			return null;
		}
		if (Items.GetValueFromName(f.MegaStone) > 0 && HasItem(Items.GetValueFromName(f.MegaStone))) {
			return f;
		}
		if (!itemOnly) {
			if (Moves.GetValueFromName(f.MegaMove) > 0 && HasMove(Moves.GetValueFromName(f.MegaMove))) {
				return f;
			}
		}
		return null;
	}

	public Species.InternalSpecies GetUnmegaForm() {
		return Species.GetSpecies(species);
	}

	public bool HasMegaForm() {
		return GetMegaForm() != null;

	}

	public bool IsMega() {
		return GetMegaForm() != null && GetForm() == 1;
	}

	public void MakeMega() {
		Species.InternalForm mf = GetMegaForm();
		if (mf != null) {
			SetForm(1);
		}
	}

	public void MakeUnmega() {
		SetForm(0);
	}

	public string MegaName() {
		Species.InternalForm mf = GetMegaForm();
		string formname = Messaging.GetMessage(MessageTypes.FormNames, mf);
		return (formname != "") ? formname : string.Format("Mega {0}", Species.GetName(species));
	}

	public int MegaMessage() {
		Species.InternalForm mf = GetMegaForm();
		if (mf != null) {
			return mf.MegaMessage;
		}
		return -1;
	}

	public bool HasPrimalForm() {
		int v = (int)MultipleForms.GetPrimalForm(species, this);
		return v > 0;
	}

	public bool IsPrimal() {
		int v = (int)MultipleForms.GetPrimalForm(species, this);
		return v > 0 && v== form;
	}

	public void MakePrimal() {
		int v = (int)MultipleForms.GetPrimalForm(species, this);
		if (v > 0) {
			SetForm(v);
		}
	}

	public void MakeUnprimal() {
		SetForm(0);
	}
}

public static class MultipleForms {

	public static int GetFormOnCreation(int species, Pokemon pokemon) {
		int env;
		int[] maps;
		switch (species) {
			case Species.UNOWN:
				return Battle.Rand(28);
			case Species.BURMY:
			case Species.WORMADAM:
				env = PokemonGlobal.GetEnvironment();
				if (Utilities.GetMetadata(PokemonGlobal.Map.MapID, MiscData.MetadataOutdoor) > 0) {
					return 2;
				} else if (env == Environment.Sand || env == Environment.Rock || env == Environment.Cave) {
					return 1;
				} else {
					return 0;
				}
			case Species.SHELLOS:
			case Species.GASTRODON:
				maps = new int[0]; // TODO Map ids for second form 
				if (PokemonGlobal.Map != null && Array.IndexOf(maps, PokemonGlobal.Map.MapID) > -1) {
					return 1;
				} else {
					return 0;
				}
			case Species.BASCULIN:
				return Battle.Rand(2);
			case Species.SCATTERBUG:
			case Species.SPEWPA:
			case Species.VIVILLON:
				return PokemonGlobal.Trainer.SecretID()%18;
			case Species.FLABEBE:
			case Species.FLOETTE:
			case Species.FLORGES:
				return Battle.Rand(5);
			case Species.PUMPKABOO:
			case Species.GOURGEIST:
				int r = Battle.Rand(20);
				if (r == 0) {
					return 3;
				}
				if (r < 4) {
					return 2;
				}
				if (r < 13) {
					return 1;
				}
				return 0;
		}
		return -1;
	}

	public static void AlterBitmap(int species, Pokemon pokemon, Sprite sprite) {
		switch (species) {
			case Species.SPINDA:
				SpindaSpots(pokemon, sprite);
				break;
		}
	}

	public static int GetFormOnEnteringBattle(int species, Pokemon pokemon) {
		int env;
		switch (species) {
			case Species.BURMY:
				env = PokemonGlobal.GetEnvironment();
				if (Utilities.GetMetadata(PokemonGlobal.Map.MapID, MiscData.MetadataOutdoor) > 0) {
					return 2;
				} else if (env == Environment.Sand || env == Environment.Rock || env == Environment.Cave) {
					return 1;
				} else {
					return 0;
				}
			case Species.XERNEAS:
				return 1;
		}
		return -1;
	}

	public static int[] GetMoveCompatibility(int species, Pokemon pokemon) {
		int[] movelist = new int[0];
		switch (species) {
			case Species.WORMADAM:
				if (pokemon.form == 0) {
					return movelist;
				}
				switch (pokemon.form) 
				{
					case 1:
						movelist = new int[43]{Moves.TOXIC,Moves.VENOSHOCK,Moves.HIDDENPOWER,Moves.SUNNYDAY,Moves.HYPERBEAM,Moves.PROTECT,Moves.RAINDANCE,Moves.SAFEGUARD,Moves.FRUSTRATION,Moves.EARTHQUAKE,Moves.RETURN,Moves.DIG,Moves.PSYCHIC,Moves.SHADOWBALL,Moves.DOUBLETEAM,Moves.SANDSTORM,Moves.ROCKTOMB,Moves.FACADE,Moves.REST,Moves.ATTRACT,Moves.THIEF,Moves.ROUND,Moves.GIGAIMPACT,Moves.FLASH,Moves.STRUGGLEBUG,Moves.PSYCHUP,Moves.BULLDOZE,Moves.DREAMEATER,Moves.SWAGGER,Moves.SUBSTITUTE,Moves.BUGBITE,Moves.EARTHPOWER,Moves.ELECTROWEB,Moves.ENDEAVOR,Moves.MUDSLAP,Moves.SIGNALBEAM,Moves.SKILLSWAP,Moves.SLEEPTALK,Moves.SNORE,Moves.STEALTHROCK,Moves.STRINGSHOT,Moves.SUCKERPUNCH,Moves.UPROAR};
						break;
					case 2:
						movelist = new int[42]{Moves.TOXIC,Moves.VENOSHOCK,Moves.HIDDENPOWER,Moves.SUNNYDAY,Moves.HYPERBEAM,Moves.PROTECT,Moves.RAINDANCE,Moves.SAFEGUARD,Moves.FRUSTRATION,Moves.RETURN,Moves.PSYCHIC,Moves.SHADOWBALL,Moves.DOUBLETEAM,Moves.FACADE,Moves.REST,Moves.ATTRACT,Moves.THIEF,Moves.ROUND,Moves.GIGAIMPACT,Moves.FLASH,Moves.GYROBALL,Moves.STRUGGLEBUG,Moves.PSYCHUP,Moves.DREAMEATER,Moves.SWAGGER,Moves.SUBSTITUTE,Moves.FLASHCANNON,Moves.BUGBITE,Moves.ELECTROWEB,Moves.ENDEAVOR,Moves.GUNKSHOT,Moves.IRONDEFENSE,Moves.IRONHEAD,Moves.MAGNETRISE,Moves.SIGNALBEAM,Moves.SKILLSWAP,Moves.SLEEPTALK,Moves.SNORE,Moves.STEALTHROCK,Moves.STRINGSHOT,Moves.SUCKERPUNCH,Moves.UPROAR};
						break;
				}
				return movelist;
		}
		return movelist;
	}

	public static void OnSetForm(int species, Pokemon pokemon) {
		switch (species) 
		{
			case Species.ROTOM:
				int[] moves = new int[5]{Moves.OVERHEAT, Moves.HYDROPUMP, Moves.BLIZZARD, Moves.AIRSLASH, Moves.LEAFSTORM};
				int hasOldMove = -1;
				for (int i=0; i<4; i++) 
				{
					for (int j=0; j<moves.Length; j++) 
					{
						if (pokemon.moves[i].Id == moves[j]) {
							hasOldMove = i;
							break;
						}
					}
					if (hasOldMove >= 0) {
						break;
					}
				}
				if (pokemon.GetForm() > 0) {
					int newMove = -1;
					if (pokemon.GetForm()-1 >= 0) {
						newMove = moves[pokemon.GetForm()-1];
					}
					if (newMove > 0) {
						if (hasOldMove >= 0) {
							string oldMoveName = Moves.GetName(pokemon.moves[hasOldMove].Id);
							string newMoveName = Moves.GetName(newMove);
							pokemon.moves[hasOldMove] = new Moves.Move(newMove);
							Messaging.Message(string.Format("1,\\wt[16] 2, and\\wt[16]...\\wt[16] ...\\wt[16] ... Ta-da!\\se[Battle ball drop]\\1"));
							Messaging.Message(string.Format("{0} forgot how to use {1}. And...", pokemon.name, oldMoveName));
							Messaging.Message(string.Format("\\se[]{0} learned {1}!\\se[Pkmn move learnt]", pokemon.name, newMoveName));
						} else {
							MultipleForms.LearnMove(pokemon, newMove, true);
						}
					}
				} else {
					if (hasOldMove >= 0) {
						string oldMoveName = Moves.GetName(pokemon.moves[hasOldMove].Id);
						pokemon.DeleteMoveAtIndex(hasOldMove);
						Messaging.Message(string.Format("{0} forgot {1}...", pokemon.name, oldMoveName));
						int found = 0;
						for (int i=0; i<pokemon.moves.Length; i++) 
						{
							if (pokemon.moves[i].Id != 0) {
								found++;
							}
						}
						if (found == 0) {
							MultipleForms.LearnMove(pokemon, Moves.THUNDERSHOCK);
						}
					}
				}
				break;
			case Species.FURFROU:
				pokemon.formTime = ((pokemon.GetForm() > 0) ? Utilities.GetTimeNow() : new DateTime());
				break;
			case Species.HOOPA:
				pokemon.formTime = ((pokemon.GetForm() > 0) ? Utilities.GetTimeNow() : new DateTime());
				break;
		}
	}

	public static int GetForm(int species, Pokemon pokemon) {
		int[] maps;
		switch (species) 
		{
			case Species.GIRATINA:
				maps = new int[0]; // TODO Map ids for second form 
				if (PokemonGlobal.Map != null && Array.IndexOf(maps, PokemonGlobal.Map.MapID) > -1) {
					return 1;
				} else {
					return 0;
				}
			case Species.SHAYMIN:
				if (pokemon.hp <= 0 || pokemon.status == Statuses.FROZEN || DayNight.IsNight()) {
					return 0;
				}
				return -1;
			case Species.ARCEUS:
				switch (pokemon.item) 
				{
					case Items.FISTPLATE:
						return 1;
					case Items.SKYPLATE:
						return 2;
					case Items.TOXICPLATE:
						return 3;
					case Items.EARTHPLATE:
						return 4;
					case Items.STONEPLATE:
						return 5;
					case Items.INSECTPLATE:
						return 6;
					case Items.SPOOKYPLATE:
						return 7;
					case Items.IRONPLATE:
						return 8;
					case Items.FLAMEPLATE:
						return 10;
					case Items.SPLASHPLATE:
						return 11;
					case Items.MEADOWPLATE:
						return 12;
					case Items.ZAPPLATE:
						return 13;
					case Items.MINDPLATE:
						return 14;
					case Items.ICICLEPLATE:
						return 15;
					case Items.DRACOPLATE:
						return 16;
					case Items.DREADPLATE:
						return 17;
					case Items.PIXIEPLATE:
						return 18;
				}
				return 0;
			case Species.DEERLING:
			case Species.SAWSBUCK:
				return Seasons.GetSeason();
			case Species.KELDEO:
				if (pokemon.HasMove(Moves.SECRETSWORD)) {
					return 1;
				}
				return 0;
			case Species.GENESECT:
				switch (pokemon.item)
				{
					case Items.SHOCKDRIVE:
						return 1;
					case Items.BURNDRIVE:
						return 2;
					case Items.CHILLDRIVE:
						return 3;
					case Items.DOUSEDRIVE:
						return 4;
				}
				return 0;
			case Species.FURFROU:
				if (pokemon.formTime == new DateTime() || Utilities.GetTimeNow() > pokemon.formTime.Add(new TimeSpan(5,0,0,0))) {
					return 0;
				}
				return -1;
			case Species.HOOPA:
				if (pokemon.formTime == new DateTime() || Utilities.GetTimeNow() > pokemon.formTime.Add(new TimeSpan(3,0,0,0))) {
					return 0;
				}
				return -1;
		}
		return -2;
	}

	public static int GetPrimalForm(int species, Pokemon pokemon) {
		switch (species) 
		{
			case Species.GROUDON:
				return pokemon.item == Items.BLUEORB ? 1 : -1;
			case Species.KYOGRE:
				return pokemon.item == Items.REDORB ? 1 : -1;
		}
		return -1;
	}

	public static void DrawSpot(Sprite bitmap, int[][] spotpattern, int x, int y, int r, int g, int b) {
		//TODO
	}

	public static void SpindaSpots(Pokemon pokemon, Sprite bitmap) {
		int[][] spot1 = new int[9][]{
			new int[8]{0,0,1,1,1,1,0,0},
			new int[8]{0,1,1,1,1,1,1,0},
			new int[8]{1,1,1,1,1,1,1,1},
			new int[8]{1,1,1,1,1,1,1,1},
			new int[8]{1,1,1,1,1,1,1,1},
			new int[8]{1,1,1,1,1,1,1,1},
			new int[8]{1,1,1,1,1,1,1,1},
			new int[8]{0,1,1,1,1,1,1,0},
			new int[8]{0,0,1,1,1,1,0,0}
		};
		int[][] spot2 = new int[9][]{
			new int[7]{0,0,1,1,1,0,0},
			new int[7]{0,1,1,1,1,1,0},
			new int[7]{1,1,1,1,1,1,1},
			new int[7]{1,1,1,1,1,1,1},
			new int[7]{1,1,1,1,1,1,1},
			new int[7]{1,1,1,1,1,1,1},
			new int[7]{1,1,1,1,1,1,1},
			new int[7]{0,1,1,1,1,1,0},
			new int[7]{0,0,1,1,1,0,0}
		};
		int[][] spot3 = new int[13][]{
			new int[13]{0,0,0,0,0,1,1,1,1,0,0,0,0},
			new int[13]{0,0,0,1,1,1,1,1,1,1,0,0,0},
			new int[13]{0,0,1,1,1,1,1,1,1,1,1,0,0},
			new int[13]{0,1,1,1,1,1,1,1,1,1,1,1,0},
			new int[13]{0,1,1,1,1,1,1,1,1,1,1,1,0},
			new int[13]{1,1,1,1,1,1,1,1,1,1,1,1,1},
			new int[13]{1,1,1,1,1,1,1,1,1,1,1,1,1},
			new int[13]{1,1,1,1,1,1,1,1,1,1,1,1,1},
			new int[13]{0,1,1,1,1,1,1,1,1,1,1,1,0},
			new int[13]{0,1,1,1,1,1,1,1,1,1,1,1,0},
			new int[13]{0,0,1,1,1,1,1,1,1,1,1,0,0},
			new int[13]{0,0,0,1,1,1,1,1,1,1,0,0,0},
			new int[13]{0,0,0,0,0,1,1,1,0,0,0,0,0}
		};
		int[][] spot4 = new int[12][]{
			new int[12]{0,0,0,0,1,1,1,0,0,0,0,0},
			new int[12]{0,0,1,1,1,1,1,1,1,0,0,0},
			new int[12]{0,1,1,1,1,1,1,1,1,1,0,0},
			new int[12]{0,1,1,1,1,1,1,1,1,1,1,0},
			new int[12]{1,1,1,1,1,1,1,1,1,1,1,0},
			new int[12]{1,1,1,1,1,1,1,1,1,1,1,1},
			new int[12]{1,1,1,1,1,1,1,1,1,1,1,1},
			new int[12]{1,1,1,1,1,1,1,1,1,1,1,1},
			new int[12]{1,1,1,1,1,1,1,1,1,1,1,0},
			new int[12]{0,1,1,1,1,1,1,1,1,1,1,0},
			new int[12]{0,0,1,1,1,1,1,1,1,1,0,0},
			new int[12]{0,0,0,0,1,1,1,1,1,0,0,0},
		};
		int id = pokemon.personalID;
		int h = (id>>28)&15;
		int g = (id>>24)&15;
		int f = (id>>20)&15;
		int e = (id>>16)&15;
		int d = (id>>12)&15;
		int c = (id>>8)&15;
		int b = (id>>4)&15;
		int a = (id)&15;
		if (pokemon.IsShiny()) {
			DrawSpot(bitmap, spot1, b+33, a+25, -75, -10, -150);
			DrawSpot(bitmap, spot2, d+21, c+24, -75, -10, -150);
			DrawSpot(bitmap, spot3, f+39, e+7, -75, -10, -150);
			DrawSpot(bitmap, spot4, h+15, g+6, -75, -10, -150);
		} else {
			DrawSpot(bitmap, spot1, b+33, a+25, 0, -115, -75);
			DrawSpot(bitmap, spot2, d+21, c+24, 0, -115, -75);
			DrawSpot(bitmap, spot3, f+39, e+7, 0, -115, -75);
			DrawSpot(bitmap, spot4, h+15, g+6, 0, -115, -75);
		}
	}

	public static void ChangeLevel(Pokemon pokemon, int newLevel, BattleScene scene) {
		if (newLevel < 1) {
			newLevel = 1;
		}
		if (newLevel > Experience.MAXLEVEL) {
			newLevel = Experience.MAXLEVEL;
		}
		if (pokemon.Level() > newLevel) {
			int attackDiff = pokemon.attack;
			int defenseDiff = pokemon.defense;
			int speedDiff = pokemon.speed;
			int spatkDiff = pokemon.spatk;
			int spdefDiff = pokemon.spdef;
			int totalHPDiff = pokemon.totalHP;
			pokemon.Level(newLevel);
			pokemon.CalcStats();
			scene.Refresh();
			Messaging.Message(string.Format("{0} was downgraded to Level {1}!", pokemon.name, pokemon.Level()));
			attackDiff = pokemon.attack - attackDiff;
			defenseDiff = pokemon.defense - defenseDiff;
			speedDiff = pokemon.speed - speedDiff;
			spatkDiff = pokemon.spatk - spatkDiff;
			spdefDiff = pokemon.spdef - spdefDiff;
			totalHPDiff = pokemon.totalHP - totalHPDiff;
			Messaging.TopRightWindow(string.Format("Max. HP<r>{0}\r\nAttack<r>{1}\r\nDefense<r>{2}\r\nSp. Atk<r>{3}\r\nSp. Def<r>{4}\r\nSpeed<r>{5}", totalHPDiff, attackDiff, defenseDiff, spatkDiff, spdefDiff, speedDiff));
			Messaging.TopRightWindow(string.Format("Max. HP<r>{0}\r\nAttack<r>{1}\r\nDefense<r>{2}\r\nSp. Atk<r>{3}\r\nSp. Def<r>{4}\r\nSpeed<r>{5}", pokemon.totalHP, pokemon.attack, pokemon.defense, pokemon.spatk, pokemon.spdef, pokemon.speed));
		} else if (pokemon.Level() == newLevel) {
			Messaging.Message(string.Format("{0}'s level remained unchanged.", pokemon.name));
		} else {
			int attackDiff = pokemon.attack;
			int defenseDiff = pokemon.defense;
			int speedDiff = pokemon.speed;
			int spatkDiff = pokemon.spatk;
			int spdefDiff = pokemon.spdef;
			int totalHPDiff = pokemon.totalHP;
			pokemon.Level(newLevel);
			pokemon.ChangeHappiness("levelup");
			pokemon.CalcStats();
			scene.Refresh();
			Messaging.Message(string.Format("{0} was elevated to Level {1}!", pokemon.name, pokemon.Level()));
			attackDiff = pokemon.attack - attackDiff;
			defenseDiff = pokemon.defense - defenseDiff;
			speedDiff = pokemon.speed - speedDiff;
			spatkDiff = pokemon.spatk - spatkDiff;
			spdefDiff = pokemon.spdef - spdefDiff;
			totalHPDiff = pokemon.totalHP - totalHPDiff;
			Messaging.TopRightWindow(string.Format("Max. HP<r>+{0}\r\nAttack<r>+{1}\r\nDefense<r>+{2}\r\nSp. Atk<r>+{3}\r\nSp. Def<r>+{4}\r\nSpeed<r>+{5}", totalHPDiff, attackDiff, defenseDiff, spatkDiff, spdefDiff, speedDiff));
			Messaging.TopRightWindow(string.Format("Max. HP<r>{0}\r\nAttack<r>{1}\r\nDefense<r>{2}\r\nSp. Atk<r>{3}\r\nSp. Def<r>{4}\r\nSpeed<r>{5}", pokemon.totalHP, pokemon.attack, pokemon.defense, pokemon.spatk, pokemon.spdef, pokemon.speed));
			List<int[]> movelist = pokemon.GetMoveList();
			for (int i=0; i<movelist.Count; i++) 
			{
				if (movelist[i][0] == pokemon.Level()) {
					MultipleForms.LearnMove(pokemon, movelist[i][1], true);
				}
			}
			int newspecies = Evolution.CheckEvolution(pokemon);
			if (newspecies > 0) {
				Graphics.FadeOutInWithMusic(99999, delegate() {
					EvolutionScene evo = new EvolutionScene();
					evo.StartScreen(pokemon, newspecies);
					evo.Evolution();
					evo.EndScreen();
				});
			}
		}
	}

	public static bool LearnMove(Pokemon pokemon, int move, bool ignoreIfKnown=false, bool byMachine=false) {
		if (pokemon == null) {
			return false;
		}
		string moveName = Moves.GetName(move);
		if (pokemon.Egg() && !Settings.DEBUG) {
			Messaging.Message("Eggs can't be taught any moves.");
			return false;
		}
		string pokemonName = pokemon.name;
		for (int i=0; i < 4; i++) 
		{
			if (pokemon.moves[i].Id == move) {
				if (!ignoreIfKnown) {
					Messaging.Message(string.Format("{0} already knows {1}.", pokemonName, moveName));
				}
				return false;
			}
			if (pokemon.moves[i].Id == 0) {
				Messaging.Message(string.Format("\\se[]{0} learned {1}!\\se[Pkmn move learnt]", pokemonName, moveName));
				return true;
			}
		}
		while (true) {
			Messaging.Message(string.Format("{0} wants to learn {1}, but it already knows four moves.\\1", pokemonName, moveName));
			Messaging.Message(string.Format("Please choose a move that will be replaced with {0}.", moveName));
			int forgetMove = MultipleForms.ForgetMove(pokemon, move);
			if (forgetMove >= 0) {
				string oldMoveName = Moves.GetName(pokemon.moves[forgetMove].Id);
				int oldMovePP = pokemon.moves[forgetMove].pp;
				pokemon.moves[forgetMove] = new Moves.Move(move);
				if (byMachine) {
					pokemon.moves[forgetMove].pp = Math.Min(oldMovePP, pokemon.moves[forgetMove].TotalPP());
				}
				Messaging.Message(string.Format("1,\\wt[16] 2, and\\wt[16]...\\wt[16] ...\\wt[16] ... Ta-da!\\se[Battle ball drop]\\1"));
				Messaging.Message(string.Format("{0} forgot how to use {1}.\\nAnd...\\1", pokemonName, oldMoveName));
				Messaging.Message(string.Format("\\se[]{0} learned {1}!\\se[Pkmn move learnt]", pokemonName, oldMoveName));
				return true;
			} else if (Messaging.ConfirmMessage(string.Format("Give up on learning {0}?", moveName))) {
				Messaging.Message(string.Format("{0} did not learn {1}.", pokemonName, moveName));
				return false;
			}
		}
	}

	public static int ForgetMove(Pokemon pokemon, int moveToLearn) {
		int ret = -1;
		Graphics.FadeOutScene(99999, delegate() {
			PokemonSummaryScene scene = new PokemonSummaryScene();
			PokemonSummaryScene.SummaryScreen screen = new PokemonSummaryScene.SummaryScreen(scene);
			ret = screen.StartForgetScreen(new Pokemon[]{pokemon}, 0, moveToLearn);
		});
		return ret;
	}

	public static bool SpeciesCombatible(int species, int move) {
		if (species <= 0) {
			return false;
		}
		return Moves.CanLearnMove(species, move);
	}
}
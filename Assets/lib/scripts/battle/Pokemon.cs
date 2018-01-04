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
	public bool shinyFlag;
	public int[] ribbons;
	public int cool;
	public int beauty;
	public int cute;
	public int smart;
	public int tough;
	public int sheen;
	public DateTime timeRecieved;
	public DateTime timeEggHatched;
	public DateTime formTime;
	public bool forcedForm;

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
			case :
				return ;
			default:
			  return -1;
		}
	}

	public int BaseExp() {
		return Species.GetSpecies(species).BaseEXP;
	}

	public int Gender() {
		return 0;
	}

	public bool isFemale() {
		return true;
	}

	public bool IsSingleGendered() {
		return true;
	}

	public bool IsFemale() {
		return true;
	}

	public bool IsMale() {
		return true;
	}

	public bool IsGenderless() {
		return true;
	}

	public void SetGender(int value) {

	}

	public void MakeMale() {

	}

	public void MakeFemale() {

	}

	public int AbilityIndex() {
		return 0;
	}

	public int Ability() {
		return 0;
	}

	public bool HasAbility(int a) {
		return true;
	}

	public void SetAbility(int a) {

	}

	public bool HasHiddenAbility() {
		return true;
	}

	public List<int> GetAbilityList() {
		return null;
	}

	public int Nature() {
		return 0;
	}

	public bool HasNature(int n) {
		return true;
	}

	public void SetNature(int n) {

	}

	public bool IsShiny() {
		return true;
	}

	public void MakeShiny() {

	}

	public void MakeNotShiny() {

	}

	public int Pokerus() {
		return 0;
	}

	public int PokerusStrain() {
		return 0;
	}

	public int PokerusStage() {
		return 0;
	}

	public void GivePokerus(int strain=0) {

	}

	public void ResetPokerusTime() {

	}

	public void LowerPokerusCount() {

	}

	public bool HasType(int t) {
		return true;
	}

	public int Type1() {
		return 0;
	}

	public int Type2() {
		return 0;
	}

	public int NumMoves() {
		return 0;
	}

	public bool HasMove(int id) {
		return true;
	}

	public bool KnowsMove(int id) {
		return true;
	}

	public List<int[]> GetMoveList() {
		return null;
	}

	public void ResetMoves() {

	}

	public void LearnMove(int id) {

	}

	public void DeleteMove(int id) {

	}

	public void DeleteMoveAtIndex(int i) {

	}

	public void DeleteAllMoves() {

	}

	public void RecordFirstMoves() {

	}

	public void AddFirstMove(int id) {

	}

	public void ClearFirstMoves() {

	}

	public bool IsCompatibleWithMove(int id) {
		return true;
	}

	public int Cool() {
		return 0;
	}

	public int Beauty() {
		return 0;
	}

	public int Cute() {
		return 0;
	}

	public int Smart() {
		return 0;
	}

	public int Tough() {
		return 0;
	}

	public int Sheen() {
		return 0;
	}

	public int RibbonCount() {
		return 0;
	}

	public bool HasRibbon(int id) {
		return true;
	}

	public void GiveRibbon(int id) {

	}

	public void UpgradeRibbon(List<int> ids) {

	}

	public void TakeRibbon(int id) {

	}

	public void ClearAllRibbons() {

	}

	public bool HasItem(int id) {
		return true;
	}

	public void SetItem(int id) {

	}

	public int[] WildHoldItems() {
		return null;
	}

	public Mail Mail() {
		return null;
	}

	public string SpeciesName() {
		return "";
	}

	public int Language() {
		return 0;
	}

	public int Markings() {
		return 0;
	}

	public string UnownShape() {
		return "";
	}

	public float Height() {
		return 0f;
	}

	public float Weight() {
		return 0f;
	}

	public int[] EvYield() {
		return null;
	}

	public void SetHP(int v) {

	}

	public bool Fainted() {
		return true;
	}

	public void HealHP() {

	}

	public void HealStatus() {

	}

	public void HealPP(int index=-1) {

	}

	public void Heal() {

	}

	public void ChangeHappiness(string method) {

	}

	public int[] BaseStats() {
		return null;
	}

	public int CalcHP(int b, int level, int iv, int ev) {
		return 0;
	}

	public int CalcStat(int b, int level, int iv, int ev, int pv) {
		return 0;
	}

	public void CalcStats() {

	}

	public Pokemon(int species, int level, BattleTrainer trainer, bool withMoves=true) {

	}

	public static Pokemon GenPokemon(int species, int level, BattleTrainer owner) {
		return null;
	}

	public int GetForm() {
		return 0;
	}

	public void SetForm(int v) {

	}

	public void SetFormNoCall(int v) {

	}

	public int FormSpecies() {
		return 0;
	}

	public int GetMegaForm(bool itemOnly=false) {
		return 0;
	}

	public int GetUnmegaForm() {
		return 0;
	}

	public bool HasMegaForm() {
		return true;
	}

	public bool IsMega() {
		return true;
	}

	public void MakeMega() {
		
	}

	public void MakeUnmega() {
		
	}

	public string MegaName() {
		return "";
	}

	public int MegaMessage() {
		return 0;
	}

	public bool HasPrimalForm() {
		return true;
	}

	public bool IsPrimal() {
		return true;
	}

	public void MakePrimal() {
		
	}

	public void MakeUnprimal() {
		
	}
}

public static class MultipleForms {
	public static Dictionary<int, Dictionary<string, Func<object[], int>>> formSpecies;

	public static void Copy(int s1, int s2) {

	}

	public static void Register(int species, string fname, Func<object[], int> function) {

	}

	public static bool HasFunction(int species, string fname) {
		return true;
	}

	public static Func<object[], int> GetFunction(int species, string fname) {
		return null;
	}

	public static int Call(int species, string fname, params object[] args) {
		return 0;
	}

	public static void DrawSpot(Pokemon pokemon, Sprite bitmap) {

	}

	public static void ChangeLevel(Pokemon pokemon, int newLevel, BattleScene scene) {

	}

	public static bool LearnMove(Pokemon pokemon, int move, bool ignoreIfKnown=false, bool byMachine=false) {
		return true;
	}

	public static int ForgetMove(Pokemon pokemon, int moveToLearn) {
		return 0;
	}

	public static bool SpeciesCombatible(int species, int move) {
		return true;
	}
}
using UnityEngine;
using System;
using System.Collections.Generic;

public class BattleMove {
	public int id;
	public Battle battle;
	public string name;
	public int function;
	public int baseDamage;
	public int type;
	public int accuracy;
	private int _add1Effect;
	public int add1Effect {
		get {
			return _add1Effect;
		}
		set {
			_add1Effect = value;
		}
	}
	public int category;
	public int target;
	public int priority;
	public int flags;
	public Moves.Move thisMove;
	public int pp;
	public int _totalPP;
	public int totalPP {
		get {
			if (_totalPP > 0) {
				return _totalPP;
			}
			if (thisMove != null) {
				return thisMove.TotalPP();
			}
			return 0;
		}
		set {
			_totalPP = value;
		}
	}
	public bool powerboost;

	public const int NOTYPE = 0x01;
	public const int IGNOREPKMNTYPES = 0x02;
	public const int NOWEIGHTING = 0x04;
	public const int NOCRITICAL = 0x08;
	public const int NOREFLECT = 0x10;
	public const int SELFCONFUSE = 0x20;

	/***************
	* Initializers *
	***************/

	public BattleMove(Battle battle, Moves.Move move) {
		id = move.Id;
		this.battle = battle;
		name = Moves.GetName(id);
		Moves.InternalMove moveData = move.moveData;
		function = Convert.ToInt32(moveData.Function, 16);
		baseDamage = moveData.Power;
		type = move.Type();
		accuracy = moveData.Accuracy;
		add1Effect = moveData.EffectChance;
		target = moveData.Target;
		priority = moveData.Priority;
		flags = move.flags;
		category = moveData.Category == "Physical" ? 0 : moveData.Category == "Special" ? 1 : 2;
		thisMove = move;
		pp = move.pp;
		powerboost = false;
	}

	public static BattleMove FromBattleMove(Battle battle, int moveId) {
		Moves.Move move = new Moves.Move(moveId);
		return FromBattleMove(battle, move);
	}

	public static BattleMove FromBattleMove(Battle battle, Moves.Move move) {
		if (move == null) {
			move = new Moves.Move(0);
		}
		string function = move.moveData.Function;
		string className = string.Format("Move{0}", function);
		Type type = Type.GetType(className);
		if (type != null) {
			return (BattleMove)Activator.CreateInstance(type, battle, move);
		}
		return new UnimplementedMove(battle, move);
	}

	/******************
	* About This Move *
	******************/
	public int ModifyType(int type, Battler attacker, Battler opponent) {
		if (type >= 0) {
			if (attacker.HasWorkingAbility(Abilities.NORMALIZE)) {
				type = Types.NORMAL;
			} else if (type == Types.NORMAL) {
				if (attacker.HasWorkingAbility(Abilities.AERILATE)) {
					type = Types.FLYING;
					powerboost = true;
				} else if (attacker.HasWorkingAbility(Abilities.REFRIGERATE)) {
					type = Types.ICE;
					powerboost = true;
				} else if (attacker.HasWorkingAbility(Abilities.PIXILATE)) {
					type = Types.FAIRY;
					powerboost = true;
				}
			}
		}
		return type;
	}

	public int GetType(int type, Battler attacker, Battler opponent) {
		powerboost = false;
		type = ModifyType(type, attacker, opponent);
		if (type >= 0) {
			if (battle.field.effects[Effects.IonDeluge] != 0 && type == Types.NORMAL) {
				type = Types.ELECTRIC;
				powerboost = false;
			}
			if (attacker.effects[Effects.Electrify] != 0) {
				type = Types.ELECTRIC;
				powerboost = false;
			}
		}
		return type;
	}

	public bool IsPhysical(int type) {
		if (Settings.USE_MOVE_CATEGORY) {
			return category == 0;
		} else {
			return !Types.IsSpecialType(type);
		}
	}

	public bool IsSpecial(int type) {
		if (Settings.USE_MOVE_CATEGORY) {
			return category == 1;
		} else {
			return Types.IsSpecialType(type);
		}
	}
 
	public bool IsStatus() {
		return category == 2;
	}

	public bool IsDamaging() {
		return !IsStatus();
	}

	public bool TargetsMultiple(Battler attacker) {
		int numTargets = 0;
		if (target == Targets.AllOpposing) 
		{
			if (!attacker.Opposing1().Fainted()) {
				numTargets++;
			}
			if (!attacker.Opposing2().Fainted()) {
				numTargets++;
			}
		} else if (target == Targets.AllNonUsers) {
			if (!attacker.Opposing1().Fainted()) {
				numTargets++;
			}
			if (!attacker.Opposing2().Fainted()) {
				numTargets++;
			}
			if (!attacker.Partner().Fainted()) {
				numTargets++;
			}
		}
		return numTargets > 1;
	}

	public int Priority(Battler attacker) {
		return priority;
	}

	public int NumHits(Battler attacker) {
		if (attacker.HasWorkingAbility(Abilities.PARENTALBOND)) {
			if (IsDamaging() && !TargetsMultiple(attacker) && !IsMultiHit() && TwoTurnAttack(attacker)) {
				if (function != 0x6E && function != 0xE0 && function != 0xE1 && function != 0xF7) {
					attacker.effects[Effects.ParentalBond] = 3;
					return 2;
				}
			}
		}
		return 1;
	}

	public bool IsMultiHit() {
		return false;
	}

	public bool TwoTurnAttack(Battler attacker) {
		return false;
	}

	public void AdditionalEffect(Battler attacker, Battler opponent) {}

	public bool CanUseWhileAsleep() {
		return false;
	}

	public bool IsHealingMove() {
		return false;
	}

	public bool IsRecoilMove() {
		return false;
	}

	public bool UnusableInGravity() {
		return false;
	}

	public bool IsContactMove() {
		return (flags&0x01) != 0;		
	}

	public bool CanProtectAgainst() {
		return (flags&0x02) != 0;
	}

	public bool CanMagicCoat() {
		return (flags&0x04) != 0;;		
	}

	public bool CanSnatch() {
		return (flags&0x08) != 0;;		
	}

	public bool CanMirrorMove() {
		return (flags&0x10) != 0;;		
	}

	public bool CanKingsRock() {
		return (flags&0x20) != 0;;		
	}

	public bool CanThawUser() {
		return (flags&0x40) != 0;;		
	}

	public bool HasHighCriticalRate() {
		return (flags&0x80) != 0;;		
	}

	public bool IsBitingMove() {
		return (flags&0x100) != 0;;		
	}

	public bool IsPunchingMove() {
		return (flags&0x200) != 0;;		
	}

	public bool IsSoundbased() {
		return (flags&0x400) != 0;;		
	}

	public bool IsPowderMove() {
		return (flags&0x800) != 0;;		
	}

	public bool IsPulseMove() {
		return (flags&0x1000) != 0;;		
	}

	public bool IsBombMove() {
		return (flags&0x2000) != 0;;		
	}

	public bool TramplesMinimize(int param=1) {
		if (!Settings.USE_NEW_BATTLE_MECHANICS) {
			return false;
		}
		return id == Moves.BODYSLAM || id == Moves.FLYINGPRESS || id == Moves.PHANTOMFORCE;
	}

	public bool SuccessCheckPerHit() {
		return false;
	}

	public bool IgnoresSubstitute(Battler attacker) {
		if (Settings.USE_NEW_BATTLE_MECHANICS) {
			if (IsSoundbased()) {
				return true;
			}
			if (attacker != null && attacker.HasWorkingAbility(Abilities.INFILTRATOR)) {
				return true;
			}
		}
		return false;
	}

	/*********************************
	* This move's type effectiveness *
	*********************************/
	public bool TypeImmunityByAbility(int type, Battler attacker, Battler opponent) {
		if (attacker.index == opponent.index) {
			return false;
		}
		if (attacker.HasMoldBreaker()) {
			return false;
		}
		if (opponent.HasWorkingAbility(Abilities.SAPSIPPER) && type == Types.GRASS) {
			Debug.Log(string.Format("[Ability triggered] {0}'s Sap Sipper (made {1} ineffective)",opponent.String(), name));
			if (opponent.CanIncreaseStatStage(Stats.ATTACK, opponent)) {
				opponent.IncreaseStatWithCause(Stats.ATTACK, 1, opponent, Abilities.GetName(opponent.ability));
			} else {
				battle.Display(string.Format("{0}'s {1} made {3} ineffective!",opponent.String(), Abilities.GetName(opponent.ability), type == Types.GRASS));
			}
			return true;
		}
		if ((opponent.HasWorkingAbility(Abilities.STORMDRAIN) && type == Types.WATER) || (opponent.HasWorkingAbility(Abilities.LIGHTNINGROD) && type == Types.ELECTRIC)) {
			Debug.Log(string.Format("[Ability triggered] {0}'s {1} (made {2} ineffective)",opponent.String(), Abilities.GetName(opponent.ability), name));
			if (opponent.CanIncreaseStatStage(Stats.SPATK, opponent)) {
				opponent.IncreaseStatWithCause(Stats.SPATK, 1, opponent, Abilities.GetName(opponent.ability));
			} else {
				battle.Display(string.Format("{0}'s {1} made {2} ineffective!",opponent.String(), Abilities.GetName(opponent.ability), name));
			}
			return true;
		}
		if (opponent.HasWorkingAbility(Abilities.MOTORDRIVE) && type == Types.ELECTRIC) {
			Debug.Log(string.Format("[Ability triggered] {0}'s Motor Drive (made {2} ineffective)",opponent.String(), name));
			if (opponent.CanIncreaseStatStage(Stats.SPEED, opponent)) {
				opponent.IncreaseStatWithCause(Stats.SPEED, 1, opponent, Abilities.GetName(opponent.ability));
			} else {
				battle.Display(string.Format("{0}'s {1} made {2} ineffective!",opponent.String(), Abilities.GetName(opponent.ability), name));
			}
			return true;
		}
		if ((opponent.HasWorkingAbility(Abilities.DRYSKIN) && type == Types.WATER) || (opponent.HasWorkingAbility(Abilities.VOLTABSORB) && type == Types.ELECTRIC) || (opponent.HasWorkingAbility(Abilities.WATERABSORB) && type == Types.WATER)) {
			Debug.Log(string.Format("[Ability triggered] {0}'s {1} (made {2} ineffective)",opponent.String(), Abilities.GetName(opponent.ability), name));
			bool healed = false;
			if (opponent.effects[Effects.HealBlock] == 0) {
				healed = opponent.RecoverHP(opponent.totalHP/4, true) > 0;
				if (healed) {
					battle.Display(string.Format("{0}'s {1} restored its HP!",opponent.String(), Abilities.GetName(opponent.ability)));
				}
			}
			if (!healed) {
				battle.Display(string.Format("{0}'s {1} made {2} useless!",opponent.String(), Abilities.GetName(opponent.ability), name));
			}
			return true;
		}
		if (opponent.HasWorkingAbility(Abilities.FLASHFIRE) && type == Types.FIRE) {
			Debug.Log(string.Format("[Ability triggered] {0}'s Flash Fire (made {1} ineffective)",opponent.String(), name));
			if (opponent.effects[Effects.FlashFire] == 0) {
				opponent.effects[Effects.FlashFire] = 1;
				battle.Display(string.Format("{0}'s {1} raised its Fire power!",opponent.String(), Abilities.GetName(opponent.ability)));
			} else {
				battle.Display(string.Format("{0}'s {1} made {3} ineffective!",opponent.String(), Abilities.GetName(opponent.ability), name));
			}
			return true;
		}
		if (opponent.HasWorkingAbility(Abilities.TELEPATHY) && IsDamaging() && !opponent.IsOpposing(attacker.index)) {
			Debug.Log(string.Format("[Ability triggered] {0}'s Telepathy (made {1} ineffective)",opponent.String(), name));
			battle.Display(string.Format("{0} avoids attacks by its ally Pokémon!",opponent.String()));
			return true;
		}
		if (opponent.HasWorkingAbility(Abilities.BULLETPROOF) && IsBombMove()) {
			Debug.Log(string.Format("[Ability triggered] {0}'s Telepathy (made {1} ineffective)",opponent.String(), name));
			battle.Display(string.Format("{0}'s {1} made {3} ineffective!",opponent.String(), Abilities.GetName(opponent.ability), name));
			return true;
		}
		return false;
	}

	public int TypeModifier(int type, Battler attacker, Battler opponent) {
		if (type < 0) {
			return 8;
		}
		if (type == Types.GROUND && opponent.HasType(Types.FLYING) && opponent.HasWorkingItem(Items.IRONBALL) && !Settings.USE_NEW_BATTLE_MECHANICS) {
			return 8;
		}
		int atype = type;
		int otype1 = opponent.type1;
		int otype2 = opponent.type1;
		int otype3 = opponent.effects[Effects.Type3];
		if (otype1 == Types.FLYING && opponent.effects[Effects.Roost] != 0) {
			if (otype2 == Types.FLYING && otype3 == Types.FLYING) {
				otype1 = Types.NORMAL;
			} else {
				otype1 = otype2;
			}
		}
		if (otype2 == Types.FLYING && opponent.effects[Effects.Roost] != 0) {
			otype2 = otype1;
		}
		int mod1 = Types.GetEffectiveness(atype, otype1);
		int mod2 = (otype1 == otype2) ? 2 : Types.GetEffectiveness(atype, otype2);
		int mod3 = (otype3 < 0 || otype1 == otype3 || otype2 == otype3) ? 2 : Types.GetEffectiveness(atype, otype3);
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
		if (battle.GetWeather() == Weather.STRONGWINDS) {
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
		if ((!opponent.IsAirborne(attacker.HasMoldBreaker()) || function == 0x11C) && atype == Types.GROUND) {
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
		if (function == 0x135 && attacker.effects[Effects.Electrify] == 0) {
			if (otype1 == Types.WATER) {
				mod1 = 4;
			}
			if (otype2 == Types.WATER) {
				mod2 = (otype1 == otype2) ? 2 : 4;
			}
			if (otype3 == Types.WATER) {
				mod2 = (otype1 == otype3 || otype2 == otype3) ? 2 : 4;
			}
		}
		return mod1*mod2*mod3;
	}

	public int TypeModMessages(int type, Battler attacker, Battler opponent) {
		if (type < 0) {
			return 8;
		}
		int typeMod = TypeModifier(type, attacker, opponent);
		if (typeMod == 0) {
			battle.Display(string.Format("It doesn't affect {0}...", opponent.String(true)));
		} else {
			if (TypeImmunityByAbility(type, attacker, opponent)) {
				return 0;
			}
		}
		return typeMod;
	}

	/*****************************
	* This move's accuracy check *
	*****************************/
	public int ModifybaseAccuracy(int baseAccuracy, Battler attacker, Battler opponent) {
		return baseAccuracy;
	}

	public bool AccuracyCheck(Battler attacker, Battler opponent) {
		int baseAccuracy = this.accuracy;
		baseAccuracy = ModifybaseAccuracy(baseAccuracy, attacker, opponent);
		if (opponent.effects[Effects.Minimize] != 0 && TramplesMinimize(1)) {
			baseAccuracy = 0;
		}
		if (baseAccuracy == 0) {
			return true;
		}
		if (attacker.HasWorkingAbility(Abilities.NOGUARD) || opponent.HasWorkingAbility(Abilities.NOGUARD)) {
			return true;
		}
		if (opponent.HasWorkingAbility(Abilities.STORMDRAIN) && GetType(type, attacker, opponent) == Types.WATER) {
			return true;
		}
		if (opponent.HasWorkingAbility(Abilities.LIGHTNINGROD) && GetType(type, attacker, opponent) == Types.ELECTRIC) {
			return true;
		}
		if (opponent.effects[Effects.Telekinesis] > 0) {
			return true;
		}
		int accstage = attacker.stages[Stats.ACCURACY];
		if (!attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.UNAWARE)) {
			accstage = 0;
		}
		double accuracy = (accstage >= 0) ? (accstage+3)*100.0/3 : 300.0/(3-accstage);
		int evastage = opponent.stages[Stats.EVASION];
		if (battle.field.effects[Effects.Gravity] > 0) {
			evastage -= 2;
		}
		if (evastage < -6) {
			evastage = -6;
		}
		if (evastage > 0 && Settings.USE_NEW_BATTLE_MECHANICS && attacker.HasWorkingAbility(Abilities.KEENEYE)) {
			evastage = 0;
		}
		if (opponent.effects[Effects.Foresight] != 0 || opponent.effects[Effects.MiracleEye] != 0 || function == 0xA9 || attacker.HasWorkingAbility(Abilities.UNAWARE)) {
			evastage = 0;
		}
		double evasion = (evastage >= 0) ? (evastage+3)*100.0/3 : 300.0/(3-evastage);
		if (attacker.HasWorkingAbility(Abilities.COMPOUNDEYES)) {
			accuracy *= 1.3;
		}
		if (attacker.HasWorkingAbility(Abilities.HUSTLE) && IsDamaging() && IsPhysical(GetType(type, attacker, opponent))) {
			accuracy *= 0.8;
		}
		if (attacker.HasWorkingAbility(Abilities.VICTORYSTAR)) {
			accuracy *= 1.1;
		}
		Battler partner = attacker.Partner();
		if (partner != null && partner.HasWorkingAbility(Abilities.VICTORYSTAR)) {
			accuracy *= 1.1;
		}
		if (attacker.effects[Effects.MicleBerry] != 0) {
			attacker.effects[Effects.MicleBerry] = 0;
			accuracy *= 1.2;
		}
		if (attacker.HasWorkingItem(Items.WIDELENS)) {
			accuracy *= 1.1;
		}
		if (attacker.HasWorkingItem(Items.ZOOMLENS) && (battle.useMoveChoice[opponent.index] != -1 || opponent.HasMovedThisRound())) {
			accuracy *= 1.2;
		}
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.WONDERSKIN) && IsStatus() && attacker.IsOpposing(opponent.index)) {
				if (accuracy > 50) {
					accuracy = 50;
				}
			}
			if (opponent.HasWorkingAbility(Abilities.TANGLEDFEET) && opponent.effects[Effects.Confusion] > 0) {
				evasion *= 1.2;
			}
			if (opponent.HasWorkingAbility(Abilities.SANDVEIL) && battle.GetWeather() == Weather.SANDSTORM) {
				evasion *= 1.25;	
			}
			if (opponent.HasWorkingAbility(Abilities.SNOWCLOAK) && battle.GetWeather() == Weather.HAIL) {
				evasion *= 1.25;
			}
		}
		if (opponent.HasWorkingItem(Items.BRIGHTPOWDER)) {
			evasion *= 1.1;
		}
		if (opponent.HasWorkingItem(Items.LAXINCENSE)) {
			evasion *= 1.1;
		}
		return battle.Rand(100) < (baseAccuracy*accuracy/evasion);
	}

	/***********************************
	* Damage calculation and modifiers *
	***********************************/
	public bool CriticalOverride(Battler attacker, Battler opponent) {
		return false;
	}

	public bool IsCritical(Battler attacker, Battler opponent) {
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.BATTLEARMOR) || opponent.HasWorkingAbility(Abilities.SHELLARMOR)) {
				return false;
			}
		}
		if (opponent.OwnSide().effects[Effects.LuckyChant] > 0) {
			return false;
		}
		if (CriticalOverride(attacker, opponent)) {
			return false;
		}
		int c = 0;
		int[] ratios = (Settings.USE_NEW_BATTLE_MECHANICS) ? new int[5]{16,8,2,1,1} : new int[5]{16,8,4,3,2};
		c += attacker.effects[Effects.FocusEnergy];
		if (HasHighCriticalRate()) {
			c++;
		}
		if (attacker.HasWorkingAbility(Abilities.SUPERLUCK)) {
			c++;
		}
		if (attacker.HasWorkingItem(Items.STICK) && attacker.species == Species.FARFETCHD) {
			c += 2;
		}
		if (attacker.HasWorkingItem(Items.LUCKYPUNCH) && attacker.species == Species.CHANSEY) {
			c += 2;
		}
		if (attacker.HasWorkingItem(Items.RAZORCLAW)) {
			c++;
		}
		if (attacker.HasWorkingItem(Items.SCOPELENS)) {
			c++;
		}
		if (c > 4) {
			c = 4;
		}
		return battle.Rand(ratios[c]) == 0;
	}

	public int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		return baseDamage;
	}

	public int BaseDamageMultiplier(int damageMult, Battler attacker, Battler opponent) {
		return damageMult;
	}

	public int ModifyDamage(int damageMult, Battler attacker, Battler opponent) {
		return damageMult;
	}

	public int CalcDamage(Battler attacker, Battler opponent, int options=0) {
		opponent.damageState.Critical = false;
		opponent.damageState.TypeModifier = 0;
		opponent.damageState.CalculatedDamage = 0;
		opponent.damageState.HPLost = 0;
		if (baseDamage == 0) {
			return 0;
		}
		float[] stageMul = {10f, 10f, 10f, 10f, 10f, 10f, 10f, 15f, 20f, 25f, 30f, 35f, 40f};
		float[] stageDiv = {40f, 35f, 30f, 25f, 20f, 15f, 10f, 10f, 10f, 10f, 10f, 10f, 10f};
		int t;
		if ((options&NOTYPE) == 0) {
			t = GetType(type, attacker, opponent);
		} else {
			t = -1;
		}
		if ((options&NOCRITICAL) == 0) {
			opponent.damageState.Critical = IsCritical(attacker, opponent);
		}
		int baseDmg = baseDamage;
		baseDmg = BaseDamage(baseDmg, attacker, opponent);
		int damageMult = 0x1000;
		if (attacker.HasWorkingAbility(Abilities.TECHNICIAN) && baseDmg<=60 && id > 0) {
			damageMult = (int)Math.Round(damageMult*1.5);
		}
		if (attacker.HasWorkingAbility(Abilities.IRONFIST) && IsPunchingMove()) {
			damageMult = (int)Math.Round(damageMult*1.2);
		}
		if (attacker.HasWorkingAbility(Abilities.STRONGJAW) && IsBitingMove()) {
			damageMult = (int)Math.Round(damageMult*1.5);
		}
		if (attacker.HasWorkingAbility(Abilities.MEGALAUNCHER) && IsPulseMove()) {
			damageMult = (int)Math.Round(damageMult*1.5);
		}
		if (attacker.HasWorkingAbility(Abilities.RECKLESS) && IsRecoilMove()) {
			damageMult = (int)Math.Round(damageMult*1.2);
		}
		if (attacker.HasWorkingAbility(Abilities.FLAREBOOST) && attacker.status == Statuses.BURN && IsSpecial(t)) {
			damageMult = (int)Math.Round(damageMult*1.5);
		}
		if (attacker.HasWorkingAbility(Abilities.TOXICBOOST) && attacker.status == Statuses.POISON && IsPhysical(t)) {
			damageMult = (int)Math.Round(damageMult*1.5);
		}
		if (attacker.HasWorkingAbility(Abilities.ANALYTIC) && (battle.useMoveChoice[opponent.index] != -1 || opponent.HasMovedThisRound())) {
			damageMult = (int)Math.Round(damageMult*1.3);
		}
		if (attacker.HasWorkingAbility(Abilities.RIVALRY) && attacker.gender != 2 && opponent.gender != 2) {
			if (attacker.gender == opponent.gender) {
				damageMult = (int)Math.Round(damageMult*1.25);
			} else {
				damageMult = (int)Math.Round(damageMult*0.75);
			}
		}
		if (attacker.HasWorkingAbility(Abilities.SANDFORCE) && battle.GetWeather() == Weather.SANDSTORM && (t == Types.ROCK || t == Types.STEEL || t == Types.GROUND)) {
			damageMult = (int)Math.Round(damageMult*1.3);
		}
		if (attacker.HasWorkingAbility(Abilities.SHEERFORCE) && add1Effect > 0) {
			damageMult = (int)Math.Round(damageMult*1.3);
		}
		if (attacker.HasWorkingAbility(Abilities.TOUGHCLAWS) && IsContactMove()) {
			damageMult = (int)Math.Round(damageMult*4.0/3);
		}
		if ((attacker.HasWorkingAbility(Abilities.AERILATE) || attacker.HasWorkingAbility(Abilities.REFRIGERATE) || attacker.HasWorkingAbility(Abilities.PIXILATE)) && powerboost) {
			damageMult = (int)Math.Round(damageMult*1.3);
		}
		if ((battle.CheckGlobalAbility(Abilities.DARKAURA) && t == Types.DARK) || (battle.CheckGlobalAbility(Abilities.FAIRYAURA) && t == Types.FAIRY)) {
			if (battle.CheckGlobalAbility(Abilities.AURABREAK)) {
				damageMult = (int)Math.Round(damageMult*2.0/3);
			} else {
				damageMult = (int)Math.Round(damageMult*4.0/3);
			}
		}
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.HEATPROOF) && t == Types.FIRE) {
				damageMult = (int)Math.Round(damageMult*0.5);
			}
			if (opponent.HasWorkingAbility(Abilities.THICKFAT) && (t == Types.ICE || t == Types.FIRE)) {
				damageMult = (int)Math.Round(damageMult*0.5);
			}
			if (opponent.HasWorkingAbility(Abilities.FURCOAT) && (IsPhysical(t) || function == 0x122)) {
				damageMult = (int)Math.Round(damageMult*0.5);
			}
			if (opponent.HasWorkingAbility(Abilities.DRYSKIN) && t == Types.FIRE) {
				damageMult = (int)Math.Round(damageMult*1.25);
			}
		}
		if (function != 0x106 && function != 0x107 && function != 0x108) {
			if ((attacker.HasWorkingItem(Items.NORMALGEM) && t == Types.NORMAL) || (attacker.HasWorkingItem(Items.FIGHTINGGEM) && t == Types.FIGHTING) || (attacker.HasWorkingItem(Items.FLYINGGEM) && t == Types.FLYING) || (attacker.HasWorkingItem(Items.POISONGEM) && t == Types.POISON) || (attacker.HasWorkingItem(Items.GROUNDGEM) && t == Types.GROUND) || (attacker.HasWorkingItem(Items.ROCKGEM) && t == Types.ROCK) || (attacker.HasWorkingItem(Items.BUGGEM) && t == Types.BUG) || (attacker.HasWorkingItem(Items.GHOSTGEM) && t == Types.GHOST) || (attacker.HasWorkingItem(Items.STEELGEM) && t == Types.STEEL) || (attacker.HasWorkingItem(Items.FIREGEM) && t == Types.FIRE) || (attacker.HasWorkingItem(Items.WATERGEM) && t == Types.WATER) || (attacker.HasWorkingItem(Items.GRASSGEM) && t == Types.GRASS) || (attacker.HasWorkingItem(Items.ELECTRICGEM) && t == Types.ELECTRIC) || (attacker.HasWorkingItem(Items.PSYCHICGEM) && t == Types.PSYCHIC) || (attacker.HasWorkingItem(Items.ICEGEM) && t == Types.ICE) || (attacker.HasWorkingItem(Items.DRAGONGEM) && t == Types.DRAGON) || (attacker.HasWorkingItem(Items.DARKGEM) && t == Types.DARK) || (attacker.HasWorkingItem(Items.FAIRYGEM) && t == Types.FAIRY)) {
				damageMult = (int)(Settings.USE_NEW_BATTLE_MECHANICS ? Math.Round(damageMult * 1.3) : Math.Round(damageMult * 1.5));
				battle.CommonAnimation("UseItem", attacker, null);
				battle.DisplayBrief(string.Format("The {0} strengthened {1}'s power!",Items.GetName(attacker.item), name));
				attacker.ConsumeItem();
			}
		}
		if ((attacker.HasWorkingItem(Items.SILKSCARF) && t == Types.NORMAL) || (attacker.HasWorkingItem(Items.BLACKBELT) && t == Types.FIGHTING) || (attacker.HasWorkingItem(Items.SHARPBEAK) && t == Types.FLYING) || (attacker.HasWorkingItem(Items.POISONBARB) && t == Types.POISON) || (attacker.HasWorkingItem(Items.SOFTSAND) && t == Types.GROUND) || (attacker.HasWorkingItem(Items.HARDSTONE) && t == Types.ROCK) || (attacker.HasWorkingItem(Items.SILVERPOWDER) && t == Types.BUG) || (attacker.HasWorkingItem(Items.SPELLTAG) && t == Types.GHOST) || (attacker.HasWorkingItem(Items.METALCOAT) && t == Types.STEEL) || (attacker.HasWorkingItem(Items.CHARCOAL) && t == Types.FIRE) || (attacker.HasWorkingItem(Items.MYSTICWATER) && t == Types.WATER) || (attacker.HasWorkingItem(Items.MIRACLESEED) && t == Types.GRASS) || (attacker.HasWorkingItem(Items.MAGNET) && t == Types.ELECTRIC) || (attacker.HasWorkingItem(Items.TWISTEDSPOON) && t == Types.PSYCHIC) || (attacker.HasWorkingItem(Items.NEVERMELTICE) && t == Types.ICE) || (attacker.HasWorkingItem(Items.DRAGONFANG) && t == Types.DRAGON) || (attacker.HasWorkingItem(Items.BLACKGLASSES) && t == Types.DARK)) {
			damageMult = (int)Math.Round(damageMult*1.2);
		}
		if ((attacker.HasWorkingItem(Items.FISTPLATE) && t == Types.FIGHTING) || (attacker.HasWorkingItem(Items.SKYPLATE) && t == Types.FLYING) || (attacker.HasWorkingItem(Items.TOXICPLATE) && t == Types.POISON) || (attacker.HasWorkingItem(Items.EARTHPLATE) && t == Types.GROUND) || (attacker.HasWorkingItem(Items.STONEPLATE) && t == Types.ROCK) || (attacker.HasWorkingItem(Items.INSECTPLATE) && t == Types.BUG) || (attacker.HasWorkingItem(Items.SPOOKYPLATE) && t == Types.GHOST) || (attacker.HasWorkingItem(Items.IRONPLATE) && t == Types.STEEL) || (attacker.HasWorkingItem(Items.FLAMEPLATE) && t == Types.FIRE) || (attacker.HasWorkingItem(Items.SPLASHPLATE) && t == Types.WATER) || (attacker.HasWorkingItem(Items.MEADOWPLATE) && t == Types.GRASS) || (attacker.HasWorkingItem(Items.ZAPPLATE) && t == Types.ELECTRIC) || (attacker.HasWorkingItem(Items.MINDPLATE) && t == Types.PSYCHIC) || (attacker.HasWorkingItem(Items.ICICLEPLATE) && t == Types.ICE) || (attacker.HasWorkingItem(Items.DRACOPLATE) && t == Types.DRAGON) || (attacker.HasWorkingItem(Items.DREADPLATE) && t == Types.DARK) || (attacker.HasWorkingItem(Items.PIXIEPLATE) && t == Types.FAIRY)) {
			damageMult = (int)Math.Round(damageMult*1.2);
		}
		if (attacker.HasWorkingItem(Items.ROCKINCENSE) && t == Types.ROCK) {
			damageMult = (int)Math.Round(damageMult*1.2);
		}
		if (attacker.HasWorkingItem(Items.ROSEINCENSE) && t == Types.GRASS) {
			damageMult = (int)Math.Round(damageMult*1.2);
		}
		if (attacker.HasWorkingItem(Items.SEAINCENSE) && t == Types.WATER) {
			damageMult = (int)Math.Round(damageMult*1.2);
		}
		if (attacker.HasWorkingItem(Items.WAVEINCENSE) && t == Types.WATER) {
			damageMult = (int)Math.Round(damageMult*1.2);
		}
		if (attacker.HasWorkingItem(Items.ODDINCENSE) && t == Types.PSYCHIC) {
			damageMult = (int)Math.Round(damageMult*1.2);
		}
		if (attacker.HasWorkingItem(Items.MUSCLEBAND) && IsPhysical(t)) {
			damageMult = (int)Math.Round(damageMult*1.1);
		}
		if (attacker.HasWorkingItem(Items.WISEGLASSES) && IsSpecial(t)) {
			damageMult = (int)Math.Round(damageMult*1.1);
		}
		if (attacker.HasWorkingItem(Items.LUSTROUSORB) && attacker.species == Species.PALKIA && (t == Types.DRAGON || t == Types.WATER)) {
			damageMult = (int)Math.Round(damageMult*1.2);
		}
		if (attacker.HasWorkingItem(Items.ADAMANTORB) && attacker.species == Species.DIALGA && (t == Types.DRAGON || t == Types.STEEL)) {
			damageMult = (int)Math.Round(damageMult*1.2);
		}
		if (attacker.HasWorkingItem(Items.GRISEOUSORB) && attacker.species == Species.GIRATINA && (t == Types.DRAGON || t == Types.GHOST)) {
			damageMult = (int)Math.Round(damageMult*1.2);
		}
		damageMult = BaseDamageMultiplier(damageMult, attacker, opponent);
		if (attacker.effects[Effects.MeFirst] != 0) {
			damageMult = (int)Math.Round(damageMult*1.5);
		}
		if (attacker.effects[Effects.HelpingHand] != 0 && (options&SELFCONFUSE)==0) {
			damageMult = (int)Math.Round(damageMult*1.5);
		}
		if (attacker.effects[Effects.Charge] > 0 && t == Types.ELECTRIC) {
			damageMult = (int)Math.Round(damageMult*2.0);
		}
		if (t == Types.FIRE) {
			for (int i=0; i<4; i++) 
			{
				if (battle.battlers[i].effects[Effects.WaterSport] != 0 && !battle.battlers[i].Fainted()) {
					damageMult = (int)Math.Round(damageMult*0.33);
					break;
				}
			}
			if (battle.field.effects[Effects.WaterSportField] > 0) {
				damageMult = (int)Math.Round(damageMult*0.33);
			}
		}
		if (t == Types.ELECTRIC) {
			for (int i=0; i<4; i++) 
			{
				if (battle.battlers[i].effects[Effects.MudSport] != 0 && !battle.battlers[i].Fainted()) {
					damageMult = (int)Math.Round(damageMult*0.33);
					break;
				}
			}
			if (battle.field.effects[Effects.MudSportField] > 0) {
				damageMult = (int)Math.Round(damageMult*0.33);
			}
		}
		if (opponent.effects[Effects.ElectricTerrain] > 0 && !opponent.IsAirborne() && t == Types.ELECTRIC) {
			damageMult = (int)Math.Round(damageMult*1.5);
		}
		if (opponent.effects[Effects.GrassyTerrain] > 0 && !opponent.IsAirborne() && t == Types.GRASS) {
			damageMult = (int)Math.Round(damageMult*1.5);
		}
		if (opponent.effects[Effects.MistyTerrain] > 0 && !opponent.IsAirborne(attacker.HasMoldBreaker()) && t == Types.DRAGON) {
			damageMult = (int)Math.Round(damageMult*0.5);
		}
		if (opponent.effects[Effects.Minimize] != 0 && TramplesMinimize(2)) {
			damageMult = (int)Math.Round(damageMult*2.0);
		}
		baseDmg = (int)Math.Round(baseDmg*damageMult*1.0/0x1000);
		int atk = attacker.attack;
		int atkstage = attacker.stages[Stats.ATTACK] + 6;
		if (function == 0x121) {
			atk = opponent.attack;
			atkstage = attacker.stages[Stats.ATTACK] + 6;
		}
		if (t >= 0 && IsSpecial(t)) {
			atk = opponent.specialAttack;
			atkstage = attacker.stages[Stats.SPATK] + 6;
			if (function == 0x121) {
				atk = opponent.specialAttack;
				atkstage = attacker.stages[Stats.SPATK] + 6;
			}
		}
		if (attacker.HasMoldBreaker() || !opponent.HasWorkingAbility(Abilities.UNAWARE)) {
			if (opponent.damageState.Critical && atkstage < 6) {
				atkstage = 6;
			}
			atk = (int)(atk*1.0*stageMul[atkstage]/stageDiv[atkstage]);
		}
		if (attacker.HasWorkingAbility(Abilities.HUSTLE) && IsPhysical(t)) {
			atk = (int)Math.Round(atk*1.5);
		}
		int atkMult = 0x1000;
		if (battle.internalbattle) {
			if (battle.OwnedByPlayer(attacker.index) && IsPhysical(t) && battle.Player().NumBadges() >= Settings.BADGES_BOOST_ATTACK) {
				atkMult = (int)Math.Round(atkMult*1.1);
			}
			if (battle.OwnedByPlayer(attacker.index) && IsSpecial(t) && battle.Player().NumBadges() >= Settings.BADGES_BOOST_SPATK) {
				atkMult = (int)Math.Round(atkMult*1.1);
			}
		}
		if (attacker.hp <= attacker.totalHP/3) {
			if ((attacker.HasWorkingAbility(Abilities.OVERGROW) && t == Types.GRASS) || (attacker.HasWorkingAbility(Abilities.BLAZE) && t == Types.FIRE) || (attacker.HasWorkingAbility(Abilities.TORRENT) && t == Types.WATER) || (attacker.HasWorkingAbility(Abilities.SWARM) && t == Types.BUG)) {
				atkMult = (int)Math.Round(atkMult*1.5);
			}
		}
		if (attacker.HasWorkingAbility(Abilities.GUTS) && attacker.status != 0 && IsPhysical(t)) {
			atkMult = (int)Math.Round(atkMult*1.5);
		}
		if ((attacker.HasWorkingAbility(Abilities.PLUS) || attacker.HasWorkingAbility(Abilities.MINUS)) && IsSpecial(t)) {
			Battler partner = attacker.Partner();
			if (partner.HasWorkingAbility(Abilities.PLUS) || partner.HasWorkingAbility(Abilities.MINUS)) {
				atkMult = (int)Math.Round(atkMult*1.5);
			}
		}
		if (attacker.HasWorkingAbility(Abilities.DEFEATIST) && attacker.hp <= attacker.totalHP/2) {
			atkMult = (int)Math.Round(atkMult*0.5);
		}
		if ((attacker.HasWorkingAbility(Abilities.PUREPOWER) || attacker.HasWorkingAbility(Abilities.HUGEPOWER)) && IsPhysical(t)) {
			atkMult = (int)Math.Round(atkMult*2.0);
		}
		if (attacker.HasWorkingAbility(Abilities.SOLARPOWER) && IsSpecial(t) && (battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN)) {
			atkMult = (int)Math.Round(atkMult*1.5);
		}
		if (attacker.HasWorkingAbility(Abilities.FLASHFIRE) && attacker.effects[Effects.FlashFire] != 0 && t == Types.FIRE) {
			atkMult = (int)Math.Round(atkMult*1.5);
		}
		if (attacker.HasWorkingAbility(Abilities.SLOWSTART) && attacker.turnCount < 5 && IsPhysical(t)) {
			atkMult = (int)Math.Round(atkMult*0.5);
		}
		if ((battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN) && IsPhysical(t)) {
			if (opponent.HasWorkingAbility(Abilities.FLOWERGIFT) || opponent.Partner().HasWorkingAbility(Abilities.FLOWERGIFT)) {
				atkMult = (int)Math.Round(atkMult*1.5);
			}
		}
		if (opponent.HasWorkingItem(Items.THICKCLUB) && (opponent.species == Species.CUBONE || opponent.species == Species.MAROWAK) && IsPhysical(t)) {
			atkMult = (int)Math.Round(atkMult*2.0);
		}
		if (opponent.HasWorkingItem(Items.DEEPSEASCALE) && opponent.species == Species.CLAMPERL && IsSpecial(t)) {
			atkMult = (int)Math.Round(atkMult*2.0);
		}
		if (opponent.HasWorkingItem(Items.LIGHTBALL) && opponent.species == Species.PIKACHU) {
			atkMult = (int)Math.Round(atkMult*2.0);
		}
		if (opponent.HasWorkingItem(Items.SOULDEW) && (opponent.species == Species.LATIOS || opponent.species == Species.LATIAS) && IsSpecial(t) && battle.rules["souldewclause"] == 0) {
			atkMult = (int)Math.Round(atkMult*1.5);
		}
		if (attacker.HasWorkingItem(Items.CHOICEBAND) && IsPhysical(t)) {
			atkMult = (int)Math.Round(atkMult*1.5);
		}
		if (attacker.HasWorkingItem(Items.CHOICESPECS) && IsSpecial(t)) {
			atkMult = (int)Math.Round(atkMult*1.5);
		}
		atk = (int)Math.Round(atk*atkMult*1.0/0x1000);
		int def = opponent.defense;
		int defstage = opponent.stages[Stats.DEFENSE] + 6;
		bool applysandstorm = false;
		if (t >= 0 && IsSpecial(t) && function != 0x122) {
			def = opponent.specialDefense;
			defstage = opponent.stages[Stats.SPDEF] + 6;
			applysandstorm = true;
		}
		if (!attacker.HasWorkingAbility(Abilities.UNAWARE)) {
			if (function == 0xA9) {
				defstage = 6;
			}
			if (opponent.damageState.Critical && defstage > 6) {
				defstage = 6;
			}
			def = (int)(def*1.0*stageMul[defstage]/stageDiv[defstage]);
		}
		if (battle.GetWeather() == Weather.SANDSTORM && opponent.HasType(Types.ROCK) && applysandstorm) {
			def = (int)Math.Round(def*1.5);
		}
		int defMult = 0x1000;
		if (battle.internalbattle) {
			if (battle.OwnedByPlayer(opponent.index) && IsPhysical(t) && battle.Player().NumBadges() >= Settings.BADGES_BOOST_DEFENSE) {
				def = (int)Math.Round(def*1.1);
			}
			if (battle.OwnedByPlayer(opponent.index) && IsSpecial(t) && battle.Player().NumBadges() >= Settings.BADGES_BOOST_SPDEF) {
				def = (int)Math.Round(def*1.1);
			}
		}
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.MARVELSCALE) && opponent.status > 0 && IsPhysical(t)) {
				def = (int)Math.Round(def*1.5);
			}
			if ((battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN) && IsSpecial(t)) {
				if (opponent.HasWorkingAbility(Abilities.FLOWERGIFT) || opponent.Partner().HasWorkingAbility(Abilities.FLOWERGIFT)) {
					def = (int)Math.Round(def*1.5);
				}
			}
		}
		if (opponent.HasWorkingItem(Items.ASSAULTVEST) && IsSpecial(t)) {
			def = (int)Math.Round(def*1.5);
		}
		if (opponent.HasWorkingItem(Items.EVIOLITE)) {
			int[][] evos = Evolution.GetEvolvedFormData(opponent.pokemon.species);
			if (evos != null && evos.Length > 0) {
				def = (int)Math.Round(def*1.5);
			}
		}
		if (opponent.HasWorkingItem(Items.DEEPSEASCALE) && opponent.species == Species.CLAMPERL && IsSpecial(t)) {
			def = (int)Math.Round(def*2.0);
		}
		if (opponent.HasWorkingItem(Items.METALPOWDER) && opponent.species == Species.DITTO && opponent.effects[Effects.Transform] == 0) {
			def = (int)Math.Round(def*1.5);
		}
		if (opponent.HasWorkingItem(Items.SOULDEW) && (opponent.species == Species.LATIOS || opponent.species == Species.LATIAS) && IsSpecial(t) && battle.rules["souldewclause"] == 0) {
			def = (int)Math.Round(def*1.5);
		}
		def = (int)Math.Max(1, Math.Round(def*defMult*1.0*0x1000));
		int damage = ((int)(((int)(2.0*attacker.level/5+2))*(double)baseDmg*atk/def)/50 + 2);
		if (TargetsMultiple(attacker)) {
			damage = (int)Math.Round(damage*0.75);
		}
		switch (battle.GetWeather()) 
		{
			case Weather.SUNNYDAY:
			case Weather.HARSHSUN:
				if (t == Types.FIRE) {
					damage = (int)Math.Round(damage*1.5);
				} else if (t == Types.WATER) {
					damage = (int)Math.Round(damage*0.5);
				}
				break;
			case Weather.RAINDANCE:
			case Weather.HEAVYRAIN:
				if (t == Types.FIRE) {
					damage = (int)Math.Round(damage*0.5);
				} else if (t == Types.WATER) {
					damage = (int)Math.Round(damage*1.5);
				}
				break;
		}
		if (opponent.damageState.Critical) {
			damage = (int)((Settings.USE_NEW_BATTLE_MECHANICS) ? Math.Round(damage*1.5) : Math.Round(damage*2.0));
		}
		int random = 0;
		if ((options&NOWEIGHTING)==0) {
			random = 85 + battle.Rand(16);
			damage = (int)(damage*random/100.0);
		}
		if (attacker.HasType(t) && (options&IGNOREPKMNTYPES)==0) {
			if (attacker.HasWorkingAbility(Abilities.ADAPTABILITY)) {
				damage = (int)Math.Round(damage*2.0);
			} else {
				damage = (int)Math.Round(damage*1.5);
			}
		}
		if ((options&IGNOREPKMNTYPES)==0) {
			int tMod = TypeModMessages(t, attacker, opponent);
			damage = (int)Math.Round(damage*tMod/8.0);
			opponent.damageState.TypeModifier = tMod;
			if (tMod == 0) {
				opponent.damageState.CalculatedDamage = 0;
				opponent.damageState.Critical = false;
				return 0;
			}
		} else {
			opponent.damageState.TypeModifier = 8;
		}
		if (attacker.status == Statuses.BURN && IsPhysical(t) && !attacker.HasWorkingAbility(Abilities.GUTS) && !(Settings.USE_NEW_BATTLE_MECHANICS && function == 0x7E)) {
			damage = (int)Math.Round(damage*0.5);
		}
		if (damage < 1) {
			damage = 1;
		}
		int finaldamagemult = 0x1000;
		if (!opponent.damageState.Critical && (options&NOREFLECT)==0 && !attacker.HasWorkingAbility(Abilities.INFILTRATOR)) {
			if (opponent.OwnSide().effects[Effects.Reflect]>0 && IsPhysical(t)) {
				if (battle.doublebattle) {
					finaldamagemult = (int)Math.Round(finaldamagemult*0.66);
				} else {
					finaldamagemult = (int)Math.Round(finaldamagemult*0.5);
				}
			}
			if (opponent.OwnSide().effects[Effects.LightScreen]>0 && IsSpecial(t)) {
				if (battle.doublebattle) {
					finaldamagemult = (int)Math.Round(finaldamagemult*0.66);
				} else {
					finaldamagemult = (int)Math.Round(finaldamagemult*0.5);
				}
			}
		}
		if (attacker.effects[Effects.ParentalBond]==1) {
			finaldamagemult = (int)Math.Round(finaldamagemult*0.5);
		}
		if (attacker.HasWorkingAbility(Abilities.TINTEDLENS) && opponent.damageState.TypeModifier < 8) {
			finaldamagemult = (int)Math.Round(finaldamagemult*2.0);
		}
		if (attacker.HasWorkingAbility(Abilities.SNIPER) && opponent.damageState.Critical) {
			finaldamagemult = (int)Math.Round(finaldamagemult*1.5);
		}
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.TINTEDLENS) && opponent.hp == opponent.totalHP) {
				finaldamagemult = (int)Math.Round(finaldamagemult*0.5);
			}
			if ((opponent.HasWorkingAbility(Abilities.SOLIDROCK) || opponent.HasWorkingAbility(Abilities.FILTER)) && opponent.damageState.TypeModifier > 8) {
				finaldamagemult = (int)Math.Round(finaldamagemult*0.75);
			}
			if (opponent.Partner().HasWorkingAbility(Abilities.FRIENDGUARD)) {
				finaldamagemult = (int)Math.Round(finaldamagemult*0.75);
			}
		}
		if (attacker.HasWorkingItem(Items.METRONOME)) {
			double met = 1 + 0.2*Math.Min(attacker.effects[Effects.Metronome], 5);
			finaldamagemult = (int)Math.Round(finaldamagemult*met);
		}
		if (attacker.HasWorkingItem(Items.EXPERTBELT) && opponent.damageState.TypeModifier > 8) {
			finaldamagemult = (int)Math.Round(finaldamagemult*1.2);
		}
		if (attacker.HasWorkingItem(Items.LIFEORB) && (options&SELFCONFUSE)==0) {
			finaldamagemult = (int)Math.Round(finaldamagemult*1.3);
		}
		if (opponent.damageState.TypeModifier > 8 && (options&IGNOREPKMNTYPES) == 0) {
			if ((opponent.HasWorkingItem(Items.CHOPLEBERRY) && t == Types.FIGHTING) || (opponent.HasWorkingItem(Items.COBABERRY) && t == Types.FLYING) || (opponent.HasWorkingItem(Items.KEBIABERRY) && t == Types.POISON) || (opponent.HasWorkingItem(Items.SHUCABERRY) && t == Types.GROUND) || (opponent.HasWorkingItem(Items.CHARTIBERRY) && t == Types.ROCK) || (opponent.HasWorkingItem(Items.TANGABERRY) && t == Types.BUG) || (opponent.HasWorkingItem(Items.KASIBBERRY) && t == Types.GHOST) || (opponent.HasWorkingItem(Items.BABIRIBERRY) && t == Types.STEEL) || (opponent.HasWorkingItem(Items.OCCABERRY) && t == Types.FIRE) || (opponent.HasWorkingItem(Items.PASSHOBERRY) && t == Types.WATER) || (opponent.HasWorkingItem(Items.RINDOBERRY) && t == Types.GRASS) || (opponent.HasWorkingItem(Items.WACANBERRY) && t == Types.ELECTRIC) || (opponent.HasWorkingItem(Items.PAYAPABERRY) && t == Types.PSYCHIC) || (opponent.HasWorkingItem(Items.YACHEBERRY) && t == Types.ICE) || (opponent.HasWorkingItem(Items.HABANBERRY) && t == Types.DRAGON) || (opponent.HasWorkingItem(Items.COLBURBERRY) && t == Types.DARK)) {
				finaldamagemult = (int)Math.Round(finaldamagemult*0.5);
				opponent.damageState.BerryWeakened = true;
				battle.CommonAnimation("UseItem", opponent, null);
			}
		}
		if (opponent.HasWorkingItem(Items.CHILANBERRY) && t == Types.NORMAL && (options&IGNOREPKMNTYPES) == 0) {
			finaldamagemult = (int)Math.Round(finaldamagemult*0.5);
			opponent.damageState.BerryWeakened = true;
			battle.CommonAnimation("UseItem", opponent, null);
		}
		finaldamagemult = ModifyDamage(finaldamagemult, attacker, opponent);
		damage = (int)Math.Round(damage*finaldamagemult*1.0/0x1000);
		opponent.damageState.CalculatedDamage = damage;
		return damage;
	}

	public int ReduceHPDamage(int damage, Battler attacker, Battler opponent) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker) && (attacker == null || attacker.index != opponent.index)) {
			Debug.Log(string.Format("[Lingering effect triggered] {0}'s Substitute took the damage",opponent.String()));
			if (damage > opponent.effects[Effects.Substitute]) {
				damage = opponent.effects[Effects.Substitute];
			}
			opponent.effects[Effects.Substitute] -= damage;
			opponent.damageState.Substitute = true;
			battle.scene.DamageAnimation(opponent, 0);
			battle.DisplayPaused(string.Format("The substitute took damage for {0}!",opponent.name));
			if (opponent.effects[Effects.Substitute] <= 0) {
				opponent.effects[Effects.Substitute] = 0;
				battle.DisplayPaused(string.Format("{0}'s substitute faded!",opponent.name));
				Debug.Log(string.Format("[End of effect] {0}'s substitute faded",opponent.String()));
			}
			opponent.damageState.HPLost = damage;
			damage = 0;
		} else {
			opponent.damageState.Substitute = false;
			if (damage >= opponent.hp) {
				damage = opponent.hp;
				if (function == 0xE9) {
					damage = damage - 1;
				} else if (opponent.effects[Effects.Endure] != 0) {
					damage = damage - 1;
					opponent.damageState.Endured = true;
					Debug.Log(string.Format("[Lingering effect triggered] {0}'s endure",opponent.String()));
				} else if (damage == opponent.totalHP) {
					if (opponent.HasWorkingAbility(Abilities.STURDY) && !attacker.HasMoldBreaker()) {
						opponent.damageState.Endured = true;
						damage = damage - 1;
						Debug.Log(string.Format("[Item triggered] {0}'s Endure",opponent.String()));
					} else if (opponent.HasWorkingItem(Items.FOCUSSASH) && opponent.hp == opponent.totalHP) {
						opponent.damageState.FocusSash = true;
						damage = damage - 1;
						Debug.Log(string.Format("[Item triggered] {0}'s Focus Sash",opponent.String()));
					} else if (opponent.HasWorkingItem(Items.FOCUSBAND) && battle.Rand(10) == 0) {
						opponent.damageState.FocusBand = true;
						damage = damage - 1;
						Debug.Log(string.Format("[Item triggered] {0}'s Focus Band",opponent.String()));
					}
				}
				if (damage < 0) {
					damage = 0;
				}
			}
			int oldHP = opponent.hp;
			opponent.hp -= damage;
			int effectiveness = 0;
			if (opponent.damageState.TypeModifier < 8) {
				effectiveness = 1;
			} else if (opponent.damageState.TypeModifier > 8) {
				effectiveness = 2;
			}
			if (opponent.damageState.TypeModifier != 0) {
				battle.scene.DamageAnimation(opponent, effectiveness);
			}
			battle.scene.HPChanged(opponent, oldHP);
			opponent.damageState.HPLost = damage;
		}
		return damage;
	}

	/**********
	* Effects *
	**********/
	public void EffectMessages(Battler attacker, Battler opponent, bool ignoreType=false, List<Battler> allTargets=null) {
		if (opponent.damageState.Critical) {
			if (allTargets != null && allTargets.Count > 1) {
				battle.Display(string.Format("A critical hit on {0}!",opponent.String(true)));
			} else {
				battle.Display(string.Format("A critical hit!"));
			}
		}
		if (!IsMultiHit() && attacker.effects[Effects.ParentalBond] == 0) {
			if (opponent.damageState.TypeModifier > 8) {
				if (allTargets != null && allTargets.Count > 1) {
					battle.Display(string.Format("It's super effective on {0}!",opponent.String(true)));
				} else {
					battle.Display(string.Format("It's super effective!"));
				}
			} else if (opponent.damageState.TypeModifier >=1 && opponent.damageState.TypeModifier < 8) {
				if (allTargets != null && allTargets.Count > 1) {
					battle.Display(string.Format("It's not very effective on {0}...",opponent.String(true)));
				} else {
					battle.Display(string.Format("It's not very effective..."));
				}
			}
		}
		if (opponent.damageState.Endured) {
			battle.Display(string.Format("{0} endured the hit!",opponent.String()));
		} else if (opponent.damageState.Sturdy) {
			battle.Display(string.Format("{0} hung on with Sturdy!",opponent.String()));
		} else if (opponent.damageState.FocusSash) {
			battle.Display(string.Format("{0} hung on using its Focus Sash!",opponent.String()));
		} else if (opponent.damageState.FocusBand) {
			battle.Display(string.Format("{0} hung on using its Focus Band!",opponent.String()));
		}
	}

	public int EffectFixedDamage(int damage, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int t = GetType(type, attacker, opponent);
		int typeMod = TypeModMessages(t, attacker, opponent);
		opponent.damageState.Critical = false;
		opponent.damageState.TypeModifier = 0;
		opponent.damageState.CalculatedDamage = 0;
		opponent.damageState.HPLost = 0;
		if (typeMod != 0) {
			opponent.damageState.CalculatedDamage = damage;
			opponent.damageState.TypeModifier = 8;
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			if (damage < 1) {
				damage = 1;
			}
			damage = ReduceHPDamage(damage, attacker, opponent);
			EffectMessages(attacker, opponent, false, allTargets);
			OnDamageLost(damage, attacker, opponent);
			return damage;
		}
		return 0;
	}

	public int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent == null) {
			return 0;
		}
		int damage = CalcDamage(attacker, opponent);
		if (opponent.damageState.TypeModifier != 0) {
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		}
		damage = ReduceHPDamage(damage, attacker, opponent);
		EffectMessages(attacker, opponent);
		OnDamageLost(damage, attacker, opponent);
		return damage;
	}

	public void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turnEffects) {}

	/*****************
	* Using the Move *
	*****************/
	public bool OnStartUse(Battler attacker) {
		return true;
	}

	public void AddTarget(List<Battler> targets, Battler attacker) {}

	public int DisplayUseMessage(Battler attacker) {
		battle.DisplayBrief(string.Format("{0} used {1}!",attacker.String(), name));
		return 0;
	}

	public void ShowAnimation(int id, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!showAnimation) {
			return;
		}
		if (attacker.effects[Effects.ParentalBond]==1) {
			battle.CommonAnimation("ParentalBond", attacker, opponent);
			return;
		}
		battle.Animation(id, attacker, opponent, hitNum);
	}

	public void OnDamageLost(int damage, Battler attacker, Battler opponent) {
		int t = type;
		t = GetType(t, attacker, opponent);
		if (opponent.effects[Effects.Bide] > 0) {
			opponent.effects[Effects.BideDamage] += damage;
			opponent.effects[Effects.BideTarget] = attacker.index;
		}
		if (function == 0x90) {
			t = Types.NORMAL;
		}
		if (IsPhysical(t)) {
			opponent.effects[Effects.Counter] = damage;
			opponent.effects[Effects.CounterTarget] = attacker.index;
		} else if (IsSpecial(t)) {
			opponent.effects[Effects.MirrorCoat] = damage;
			opponent.effects[Effects.MirrorCoatTarget] = attacker.index;
		}
		opponent.lastHPLost = damage;
		if (damage > 0) {
			opponent.tookDamage = true;
		}
		opponent.lastAttacker.Add(attacker.index);
	}

	public bool MoveFailed(Battler attacker, Battler opponent) {
		return false;
	}

	/***************
	* Hidden Power *
	***************/
	public static int[] HiddenPower(int[] iv) {
		int powermin=30;
		int powermax=70;
		int type=0;
		int bas=0;
		List<int> types = new List<int>();
		for (int i=0; i<Types.MaxValue(); i++) {
			if (Types.IsPseudoType(i) && i != Types.NORMAL) {
				types.Add(i);
			}
		}
		type |= (iv[Stats.HP] & 1);
		type |= (iv[Stats.ATTACK] & 1) << 1;
		type |= (iv[Stats.DEFENSE] & 1) << 2;
		type |= (iv[Stats.SPEED] & 1) << 3;
		type |= (iv[Stats.SPATK] & 1) << 4;
		type |= (iv[Stats.SPDEF] & 1) << 5;
		type = (int)(type*(types.Count - 1) / 63.0f);
		int hptype = types[type];
		bas |= (iv[Stats.HP] & 2) >> 1;
		bas |= (iv[Stats.ATTACK] & 2);
		bas |= (iv[Stats.DEFENSE] & 2) << 1;
		bas |= (iv[Stats.SPEED] & 2) << 2;
		bas |= (iv[Stats.SPATK] & 2) << 3;
		bas |= (iv[Stats.SPDEF] & 2) << 4;
		bas = (int)(bas * (powermax - powermin) / 63.0f) + powermin;
		return new int[2]{hptype, bas};
	}
}

/*****************************************************************
* Superclass that handles moves using a non-existent function code. *
* Damaging moves just do damage with no additional effect.          *
* Non-damaging moves always fail.                                   *
********************************************************************/
public class UnimplementedMove : BattleMove {
	public UnimplementedMove(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		battle.Display(string.Format("But it failed!"));
		return -1;
	}
}

/*******************************************
* Superclass for a failed move. Always fails. *
* This class is unused.                       *
**********************************************/
public class FailedMove : BattleMove {
	public FailedMove(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		battle.Display(string.Format("But it failed!"));
		return -1;
	}
}

/********************************
* Pseudomove for confusion damage. *
***********************************/
public class MoveConfusion : BattleMove {
	public MoveConfusion(Battle battle, Moves.Move move) : base(battle, move) {
		this.battle = battle;
		baseDamage = 40;
		type = -1;
		accuracy = 100;
		pp = -1;
		add1Effect = 0;
		target = 0;
		priority = 0;
		flags = 0;
		thisMove = move;
		name = "";
		id = 0;
	}

	public new bool IsPhysical(int type) {
		return true;
	}

	public new bool IsSpecial(int type) {
		return false;
	}

	public int CalcDamage(Battler attacker, Battler opponent) {
		return base.CalcDamage(attacker, opponent, NOCRITICAL|SELFCONFUSE|NOTYPE|NOWEIGHTING);
	}

	public void EffectMessages(Battler attacker, Battler opponent, bool ignoreType=false) {
		base.EffectMessages(attacker, opponent, true);
	}
}

/************************************************************
* Implements the move Struggle.                                *
* For cases where the real move named Struggle is not defined. *                       *
***************************************************************/
public class MoveStruggle : BattleMove {
	public MoveStruggle(Battle battle, Moves.Move move) : base(battle, move) {
		id = -1;
		this.battle = battle;
		name = "Struggle";
		baseDamage = 50;
		type = -1;
		accuracy = 0;
		add1Effect = 0;
		target = 0;
		priority = 0;
		flags = 0;
		thisMove = null;
		pp = -1;
		totalPP = 0;
		if (move != null) {
			id = move.Id;
			name = Moves.GetName(id);
		}
	}

	public new bool IsPhysical(int type) {
		return true;
	}

	public new bool IsSpecial(int type) {
		return false;
	}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turnEffects) {
		if (!attacker.Fainted() && turnEffects[Effects.TotalDamage] > 0) {
			attacker.ReduceHP((int)Math.Round(attacker.totalHP/4.0));
			battle.Display(string.Format("{0} is damaged by recoil!",attacker.String()));
		}
	}

	public int CalcDamage(Battler attacker, Battler opponent) {
		return base.CalcDamage(attacker, opponent, IGNOREPKMNTYPES);
	}
}

/*********************
* No additional Effect. *
************************/
class Move000 : BattleMove {
	public Move000(Battle battle, Moves.Move move) : base(battle, move) {}
}

/*********************************
* Does absolutely nothing. (Splash) *
************************************/
class Move001 : BattleMove {
	public Move001(Battle battle, Moves.Move move) : base(battle, move) {}

	
	public new bool UnusableInGravity() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.Display("But nothing happened!");
		return 0;
	}
}

/******************************************************
* Struggle. Overrides the default struggle effect above. *
*********************************************************/
public class Move002 : MoveStruggle {
	public Move002(Battle battle, Moves.Move move) : base(battle, move) {}
}

/*************************
* Puts the target to sleep. *
****************************/
public class Move003 : BattleMove{
	public Move003(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (opponent.CanSleep(attacker, true, this)) {
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			opponent.Sleep();
			return 0;
		}
		return -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanSleep(attacker, false, this)) {
			opponent.Sleep();
		}
	}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turnEffects) {
		if (id == Moves.RELICSONG) {
			if (attacker.species == Species.MELOETTA && attacker.effects[Effects.Transform] != 0 && !(attacker.HasWorkingAbility(Abilities.SHEERFORCE) && add1Effect > 0) && !attacker.Fainted()) {
				attacker.form = (attacker.form+1)%2;
				attacker.Update(true);
				battle.scene.ChangePokemon(attacker, attacker.pokemon);
				battle.Display(string.Format("{0} transformed!", attacker.String()));
				Debug.Log(string.Format("[Form Changed] {0} changed to form {1}",attacker.String(), attacker.form));
			}
		}
	}
}

/********************************************************************************
* Makes the target drowsy; it will fall asleep at the end of the next turn. (Yawn) *
***********************************************************************************/
public class Move004 : BattleMove {
	public Move004(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!opponent.CanSleep(attacker, true, this)) {
			return -1;
		}
		if (opponent.effects[Effects.Yawn] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.Yawn] = 2;
		battle.Display(string.Format("{0} made {1} drowsy!",attacker.String(), opponent.String(true)));
		return 0;
	}
}

/*******************
* Poisons the target. *
**********************/
public class Move005 : BattleMove {
	public Move005(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!opponent.CanPoison(attacker, true, this)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.Poison(attacker);
		return 0;
	}
	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanPoison(attacker, false, this)) {
			opponent.Poison(attacker);
		}
	}
}

/*****************************************************************************
* Badly poisons the target. (Poison Fang, Toxic)                                *
* (Handled in Battler's pbSuccessCheck): Hits semi-invulnerable targets if user *
* is Poison-type and move is status move.                                       *
********************************************************************************/
public class Move006 : BattleMove {
	public Move006(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!opponent.CanPoison(attacker, true, this)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.Poison(attacker, null, true);
		return 0;
	}
	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanPoison(attacker, false, this)) {
			opponent.Poison(attacker, null, true);
		}
	}
}

/***********************************************************************
* Paralyzes the target.                                                   *
* Thunder Wave: Doesn't affect target if move's type has no effect on it. *
* Bolt Strike: Powers up the next Fusion Flare used this round.           *
**************************************************************************/
public class Move007 : BattleMove {
	public Move007(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
			if (opponent.damageState.CalculatedDamage > 0 && id == Moves.BOLTSTRIKE) {
				battle.field.effects[Effects.FusionFlare] = 1;
			}
			return ret;
		} else {
			if (id == Moves.THUNDERWAVE) {
				if (TypeModifier(type, attacker, opponent) == 0) {
					battle.Display(string.Format("It doesn't affect {0}...",opponent.String(true)));
					return -1;
				}
			}
			if (TypeImmunityByAbility(GetType(type, attacker, opponent), attacker, opponent)) {
				return -1;
			}
			if (!opponent.CanParalyze(attacker, true, this)) {
				return -1;
			}
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			opponent.Paralyze(attacker);
			return 0;
		}
	}
	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanParalyze(attacker, false, this)) {
			opponent.Paralyze(attacker);
		}
	}
}

/***************************************************************************
* Paralyzes the target. Accuracy perfect in rain, 50% in sunshine. (Thunder)  *
* (Handled in Battler's pbSuccessCheck): Hits some semi-invulnerable targets. *
******************************************************************************/
public class Move008 : BattleMove {
	public Move008(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanParalyze(attacker, false, this)) {
			opponent.Paralyze(attacker);
		}
	}

	public new int ModifybaseAccuracy(int baseAccuracy, Battler attacker, Battler opponent) {
		switch (battle.GetWeather()) 
		{
			case Weather.RAINDANCE:
			case Weather.HEAVYRAIN:
				return 0;
			case Weather.SUNNYDAY:
			case Weather.HARSHSUN:
				return 50;
		}
		return baseAccuracy;
	}
}

/************************************************************************
*  Paralyzes the target. May cause the target to flinch. (Thunder Fang) *
************************************************************************/
public class Move009 : BattleMove {
	public Move009(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (battle.Rand(10) == 0) {
			if (opponent.CanParalyze(attacker, false, this)) {
				opponent.Paralyze(attacker);
			}
		}
		if (battle.Rand(10) == 0) {
			opponent.Flinch(attacker);
		}
	}
}

/***************************************************************
*  Burns the target.                                           *
*  Blue Flare: Powers up the next Fusion Bolt used this round. *
***************************************************************/
public class Move00A : BattleMove {
	public Move00A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
			if (opponent.damageState.CalculatedDamage > 0 && id == Moves.BLUEFLARE) {
				battle.field.effects[Effects.FusionBolt] = 1;
			}
			return ret;
		} else {
			if (TypeImmunityByAbility(GetType(type, attacker, opponent), attacker, opponent)) {
				return -1;
			}
			if (!opponent.CanBurn(attacker, true, this)) {
				return -1;
			}
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			opponent.Burn(attacker);
			return 0;
		}
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanBurn(attacker, false, this)) {
			opponent.Burn(attacker);
		}
	}
}

/*****************************************************************
*  Burns the target. May cause the target to flinch. (Fire Fang) *
*****************************************************************/
public class Move00B : BattleMove {
	public Move00B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (battle.Rand(10) == 0) {
			if (opponent.CanBurn(attacker, false, this)) {
				opponent.Burn(attacker);
			}
		}
		if (battle.Rand(10) == 0) {
			opponent.Flinch(attacker);
		}
	}
}

/***********************
*  Freezes the target. *
***********************/
public class Move00C : BattleMove {
	public Move00C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!opponent.CanFreeze(attacker, true, this)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.Freeze();
		return 0;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanFreeze(attacker, false, this)) {
			opponent.Freeze();
		}
	}
}

/************************************************************
*  Freezes the target. Accuracy perfect in hail. (Blizzard) *
************************************************************/
public class Move00D : BattleMove {
	public Move00D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!opponent.CanFreeze(attacker, true, this)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.Freeze();
		return 0;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanFreeze(attacker, false, this)) {
			opponent.Freeze();
		}
	}

	public new int ModifybaseAccuracy(int baseAccuracy, Battler attacker, Battler opponent) {
		if (battle.GetWeather() == Weather.HAIL) {
			return 0;
		}
		return baseAccuracy;
	}
}

/******************************************************************
*  Freezes the target. May cause the target to flinch. (Ice Fang) *
******************************************************************/
public class Move00E : BattleMove {
	public Move00E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (battle.Rand(10) == 0) {
			if (opponent.CanFreeze(attacker, false, this)) {
				opponent.Freeze();
			}
		}
		if (battle.Rand(10) == 0) {
			opponent.Flinch(attacker);
		}
	}
}

/********************************
*  Causes the target to flinch. *
********************************/
public class Move00F : BattleMove {
	public Move00F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		opponent.Flinch(attacker);
	}
}

/*******************************************************************************
*  Causes the target to flinch. Does double damage and has perfect accuracy if *
*  the target is Minimized.                                                    *
*******************************************************************************/
public class Move010 : BattleMove {
	public Move010(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		opponent.Flinch(attacker);
	}

	public new bool TramplesMinimize(int param=1) {
		if (id == Moves.DRAGONRUSH && !Settings.USE_NEW_BATTLE_MECHANICS) {
			return false;
		}
		if (param == 1 && Settings.USE_NEW_BATTLE_MECHANICS) {
			return true;
		}
		if (param == 2) {
			return true;
		}
		return false;
	}
}

/*************************************************************************
*  Causes the target to flinch. Fails if the user is not asleep. (Snore) *
*************************************************************************/
public class Move011 : BattleMove {
	public Move011(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool CanUseWhileAsleep() {
		return true;
	}

	public new bool MoveFailed(Battler attacker, Battler opponent) {
		return attacker.status != Statuses.SLEEP;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		opponent.Flinch(attacker);
	}
}

/**************************************************************************************
*  Causes the target to flinch. Fails if this isn't the user's first turn. (Fake Out) *
**************************************************************************************/
public class Move012 : BattleMove {
	public Move012(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool MoveFailed(Battler attacker, Battler opponent) {
		return (attacker.turnCount > 1);
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		opponent.Flinch(attacker);
	}
}

/************************
*  Confuses the target. *
************************/
public class Move013 : BattleMove {
	public Move013(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (opponent.CanConfuse(attacker, true, this)) {
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			opponent.Confuse();
			battle.Display(string.Format("{0} became confused!", opponent.String()));
		}
		return -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanConfuse(attacker, false, this)) {
			opponent.Confuse();
			battle.Display(string.Format("{0} became confused!",opponent.String()));
		}
	}
}

/*******************************************************************************
*  Confuses the target. Chance of causing confusion dep}s on the cry's volume. *
*  Confusion chance is 0% if user doesn't have a recorded cry. (Chatter)       *
*  TODO: Play the actual chatter cry as part of the move animation             *
*                                                                              *
*******************************************************************************/
public class Move014 : BattleMove {
	public Move014(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int add1Effect {
		get {
			if (Settings.USE_NEW_BATTLE_MECHANICS) {
				return 100;
			}
			return 0;
		}
		set {
			add1Effect = value;
		}
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanConfuse(attacker, false, this)) {
			opponent.Confuse();
			battle.Display(string.Format("{0} became confused!",opponent.String()));
		}
	}
}

/*******************************************************************************
*  Confuses the target. Accuracy perfect in rain, 50% in sunshine. (Hurricane) *
*  (Handled in Battler's pbSuccessCheck): Hits some semi-invulnerable targets. *
*******************************************************************************/
public class Move015 : BattleMove {
	public Move015(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (opponent.CanConfuse(attacker, true, this)) {
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			opponent.Confuse();
			battle.Display(string.Format("{0} became confused!", opponent.String()));
		}
		return -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanConfuse(attacker, false, this)) {
			opponent.Confuse();
			battle.Display(string.Format("{0} became confused!",opponent.String()));
		}
	}

	public new int ModifybaseAccuracy(int baseAccuracy, Battler attacker, Battler opponent) {
		switch (battle.GetWeather()) 
		{
			case Weather.RAINDANCE:
			case Weather.HEAVYRAIN:
				return 0;
			case Weather.SUNNYDAY:
			case Weather.HARSHSUN:
				return 50;
		}
		return baseAccuracy;
	}
}

/**********************************
*  Attracts the target. (Attract) *
**********************************/
public class Move016 : BattleMove {
	public Move016(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!opponent.CanAttract(attacker)) {
			return -1;
		}
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.AROMAVEIL)) {
				battle.Display(string.Format("But it failed because of {0}'s {1}!",opponent.String(), Abilities.GetName(opponent.ability)));
				return -1;
			} else if (opponent.Partner().HasWorkingAbility(Abilities.AROMAVEIL)) {
				battle.Display(string.Format("But it failed because of {0}'s {1}!",opponent.Partner().String(), Abilities.GetName(opponent.Partner().ability)));
				return -1;
			}
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.Attract(attacker);
		return 0;
	}
}

/********************************************************
*  Burns, freezes or paralyzes the target. (Tri Attack) *
********************************************************/
public class Move017 : BattleMove {
	public Move017(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		switch (battle.Rand(3)) 
		{
			case 0:
				if (opponent.CanBurn(attacker, false, this)) {
					opponent.Burn(attacker);
				}
				break;
			case 1:
				if (opponent.CanFreeze(attacker, false, this)) {
					opponent.Freeze();
				}
				break;
			case 2:
				if (opponent.CanParalyze(attacker, false, this)) {
					opponent.Paralyze(attacker);
				}
				break;
		}
	}
}

/*******************************************************
*  Cures user of burn, poison and paralysis. (Refresh) *
*******************************************************/
public class Move018 : BattleMove {
	public Move018(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.status != Statuses.BURN && attacker.status != Statuses.POISON && attacker.status != Statuses.PARALYSIS) {
			battle.Display("But it failed!");
			return -1;
		} else {
			int t = attacker.status;
			attacker.CureStatus(false);
			ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
			if (t == Statuses.BURN) {
				battle.Display(string.Format("{0} healed its burn!",attacker.String()));
			} else if (t == Statuses.POISON) {
				battle.Display(string.Format("{0} cured its poisoning!",attacker.String()));
			} else if (t == Statuses.PARALYSIS) {
				battle.Display(string.Format("{0} cured its paralysis!",attacker.String()));
			}
			return 0;
		}
	}
}

/**************************************************************************************
*  Cures all party PokÃ©mon of permanent status problems. (Aromatherapy, Heal Bell) *
**************************************************************************************/
public class Move019 : BattleMove {
	public Move019(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		if (id == Moves.AROMATHERAPY) {
			battle.Display(string.Format("A soothing aroma wafted through the area!"));
		} else {
			battle.Display(string.Format("A bell chimed!"));
		}
		List<int> activePokemon = new List<int>();
		for (int i=0; i<battle.battlers.Count; i++) 
		{
			if (attacker.IsOpposing(battle.battlers[i].index) || battle.battlers[i].Fainted()) {
				continue;
			}
			activePokemon.Add(battle.battlers[i].pokemonIndex);
			if (Settings.USE_NEW_BATTLE_MECHANICS && battle.battlers[i].index != attacker.index && TypeImmunityByAbility(GetType(type, attacker, battle.battlers[i]), attacker, battle.battlers[i])) {
				continue;
			}
			switch (battle.battlers[i].status) 
			{
				case Statuses.PARALYSIS:
					battle.Display(string.Format("{0} was cured of paralysis.",battle.battlers[i].String()));
					break;
				case Statuses.SLEEP:
					battle.Display(string.Format("{0}'s sleep was woken.",battle.battlers[i].String()));
					break;
				case Statuses.POISON:
					battle.Display(string.Format("{0} was cured of its poisoning.",battle.battlers[i].String()));
					break;
				case Statuses.BURN:
					battle.Display(string.Format("{0}'s burn was healed.",battle.battlers[i].String()));
					break;
				case Statuses.FROZEN:
					battle.Display(string.Format("{0} was thawed out.",battle.battlers[i].String()));
					break;
			}
			battle.battlers[i].CureStatus(false);
		}
		Battler[] party = battle.Party(attacker.index);
		for (int i=0; i<party.Length; i++) 
		{
			if (activePokemon.Contains(i)) {
				continue;
			}
			if (party[i] == null || party[i].pokemon.Egg() || party[i].hp <= 0) {
				continue;
			}
			switch (party[i].status) 
			{
				case Statuses.PARALYSIS:
					battle.Display(string.Format("{0} was cured of paralysis.",party[i].String()));
					break;
				case Statuses.SLEEP:
					battle.Display(string.Format("{0} was woken from its sleep.",party[i].String()));
					break;
				case Statuses.POISON:
					battle.Display(string.Format("{0} was cured of its poisoning.",party[i].String()));
					break;
				case Statuses.BURN:
					battle.Display(string.Format("{0}'s burn was healed.",party[i].String()));
					break;
				case Statuses.FROZEN:
					battle.Display(string.Format("{0} was thawed out.",party[i].String()));
					break;
			}
			party[i].status = 0;
			party[i].statusCount = 0;
		}
		return 0;
	}
}

/*************************************************************************************
*  Safeguards the user's side from being inflicted with status problems. (Safeguard) *
*************************************************************************************/
public class Move01A : BattleMove {
	public Move01A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OwnSide().effects[Effects.Safeguard] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		attacker.OwnSide().effects[Effects.Safeguard] = 5;
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display(string.Format("Your team became cloaked in a mystical veil!"));
		} else {
			battle.Display(string.Format("The opposing became cloaked in a mystical veil!"));
		}
		return 0;
	}
}

/****************************************************************
*  User passes its status problem to the target. (Psycho Shift) *
****************************************************************/
public class Move01B : BattleMove {
	public Move01B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.status == 0 || (attacker.status == Statuses.PARALYSIS && !opponent.CanParalyze(attacker, false, this)) || (attacker.status == Statuses.SLEEP && !opponent.CanSleep(attacker, false, this)) || (attacker.status == Statuses.POISON && !opponent.CanPoison(attacker, false, this)) || (attacker.status == Statuses.BURN && !opponent.CanBurn(attacker, false, this)) || (attacker.status == Statuses.FROZEN && !opponent.CanFreeze(attacker, false, this))) {
			battle.Display("But it failed!");
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		switch (attacker.status) 
		{
			case Statuses.PARALYSIS:
				opponent.Paralyze(attacker);
				opponent.AbilityCureCheck();
				attacker.CureStatus(false);
				battle.Display(string.Format("{0} was cured of paralysis.",attacker.String()));
				break;
			case Statuses.SLEEP:
				opponent.Sleep();
				opponent.AbilityCureCheck();
				attacker.CureStatus(false);
				battle.Display(string.Format("{0} woke up.",attacker.String()));
				break;
			case Statuses.POISON:
				opponent.Poison(attacker, null, attacker.statusCount != 0);
				opponent.AbilityCureCheck();
				attacker.CureStatus(false);
				battle.Display(string.Format("{0} was cured of its poisoning.",attacker.String()));
				break;
			case Statuses.BURN:
				opponent.Burn(attacker);
				opponent.AbilityCureCheck();
				attacker.CureStatus(false);
				battle.Display(string.Format("{0} was cured of paralysis.",attacker.String()));
				break;
			case Statuses.FROZEN:
				opponent.Freeze();
				opponent.AbilityCureCheck();
				attacker.CureStatus(false);
				battle.Display(string.Format("{0} was thawed out.",attacker.String()));
				break;
		}
		return 0;
	}
}

/*******************************************
*  Increases the user's Attack by 1 stage. *
*******************************************/
public class Move01C : BattleMove {
	public Move01C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.ATTACK, 1, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ATTACK, 1, attacker, false, this);
		}
	}
}

/********************************************
*  Increases the user's Defense by 1 stage. *
********************************************/
public class Move01D : BattleMove {
	public Move01D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
			attacker.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this);
		}
	}
}

/**************************************************************************
*  Increases the user's Defense by 1 stage. User curls up. (Defense Curl) *
**************************************************************************/
public class Move01E : BattleMove {
	public Move01E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		attacker.effects[Effects.DefenseCurl] = 1;
		if (!attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this) ? 0 : -1;
	}
}

/******************************************
*  Increases the user's Speed by 1 stage. *
******************************************/
public class Move01F : BattleMove {
	public Move01F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.SPEED, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.SPEED, 1, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPEED, 1, attacker, false, this);
		}
	}
}

/***************************************************
*  Increases the user's Special Attack by 1 stage. *
***************************************************/
public class Move020 : BattleMove {
	public Move020(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.SPATK, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.SPATK, 1, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPATK, 1, attacker, false, this);
		}
	}
}

/******************************************************************
*  Increases the user's Special Defense by 1 stage.               *
*  Charges up user's next attack if it is Electric-type. (Charge) *
******************************************************************/
public class Move021 : BattleMove {
	public Move021(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		attacker.effects[Effects.Charge] = 2;
		battle.Display(string.Format("{0} began charging power!",attacker.String()));
		if (attacker.CanIncreaseStatStage(Stats.SPDEF, attacker, true, this)) {
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		}
		return 0;
	}
}

/********************************************
*  Increases the user's evasion by 1 stage. *
********************************************/
public class Move022 : BattleMove {
	public Move022(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.EVASION, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.EVASION, 1, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.CanIncreaseStatStage(Stats.EVASION, attacker, false, this)) {
			attacker.IncreaseStat(Stats.EVASION, 1, attacker, false, this);
		}
	}
}

/**********************************************************
*  Increases the user's critical hit rate. (Focus Energy) *
**********************************************************/
public class Move023 : BattleMove {
	public Move023(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (attacker.effects[Effects.FocusEnergy] >= 2) {
			battle.Display("But it failed!");
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.FocusEnergy] = 2;
		battle.Display(string.Format("{0} is getting pumped!",attacker.String()));
		return 0;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.effects[Effects.FocusEnergy] < 2) {
			attacker.effects[Effects.FocusEnergy] = 2;
			battle.Display(string.Format("{0} is getting pumped!",attacker.String()));
		}
	}
}

/**********************************************************************
*  Increases the user's Attack and Defense by 1 stage each. (Bulk Up) *
**********************************************************************/
public class Move024 : BattleMove {
	public Move024(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ATTACK, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
			attacker.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		return 0;
	}
}

/*****************************************************************************
*  Increases the user's Attack, Defense and accuracy by 1 stage each. (Coil) *
*****************************************************************************/
public class Move025 : BattleMove {
	public Move025(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.ACCURACY, attacker, false, this)) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ATTACK, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
			attacker.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.ACCURACY, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ACCURACY, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		return 0;
	}
}

/*************************************************************************
*  Increases the user's Attack and Speed by 1 stage each. (Dragon Dance) *
*************************************************************************/
public class Move026 : BattleMove {
	public Move026(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ATTACK, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPEED, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		return 0;
	}
}

/*****************************************************************************
*  Increases the user's Attack and Special Attack by 1 stage each. (Work Up) *
*****************************************************************************/
public class Move027 : BattleMove {
	public Move027(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ATTACK, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPATK, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		return 0;
	}
}

/*****************************************************************
*  Increases the user's Attack and Sp. Attack by 1 stage each.   *
*  In sunny weather, increase is 2 stages each instead. (Growth) *
*****************************************************************/
public class Move028 : BattleMove {
	public Move028(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool showAnim = true;
		int increment = 1;
		if (battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN) {
			increment = 2;
		}
		if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ATTACK, increment, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
			attacker.IncreaseStat(Stats.DEFENSE, increment, attacker, false, this, showAnim);
			showAnim = false;
		}
		return 0;
	}
}

/**************************************************************************
*  Increases the user's Attack and accuracy by 1 stage each. (Hone Claws) *
**************************************************************************/
public class Move029 : BattleMove {
	public Move029(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.ACCURACY, attacker, false, this)) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ATTACK, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.ACCURACY, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ACCURACY, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		return 0;
	}
}

/************************************************************************************
*  Increases the user's Defense and Special Defense by 1 stage each. (Cosmic Power) *
************************************************************************************/
public class Move02A : BattleMove {
	public Move02A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this)) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
			attacker.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPDEF, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		return 0;
	}
}

/******************************************************************************************
*  Increases the user's Sp. Attack, Sp. Defense and Speed by 1 stage each. (Quiver Dance) *
******************************************************************************************/
public class Move02B : BattleMove {
	public Move02B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPATK, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPDEF, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPEED, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		return 0;
	}
}

/********************************************************************************
*  Increases the user's Sp. Attack and Sp. Defense by 1 stage each. (Calm Mind) *
********************************************************************************/
public class Move02C : BattleMove {
	public Move02C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this)) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPATK, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPDEF, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		return 0;
	}
}

/***********************************************************************************
*  Increases the user's Attack, Defense, Speed, Special Attack and Special Defense *
*  by 1 stage each. (AncientPower, Ominous Wind, Silver Wind)                      *
***********************************************************************************/
public class Move02D : BattleMove {
	public Move02D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ATTACK, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
			attacker.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPATK, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPDEF, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPEED, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
	}
}

/********************************************
*  Increases the user's Attack by 2 stages. *
********************************************/
public class Move02E : BattleMove {
	public Move02E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.ATTACK, 2, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ATTACK, 2, attacker, false, this);
		}
	}
}

/*********************************************
*  Increases the user's Defense by 2 stages. *
*********************************************/
public class Move02F : BattleMove {
	public Move02F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.DEFENSE, 2, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
			attacker.IncreaseStat(Stats.DEFENSE, 2, attacker, false, this);
		}
	}
}

/*******************************************
*  Increases the user's Speed by 2 stages. *
*******************************************/
public class Move030 : BattleMove {
	public Move030(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.SPEED, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.SPEED, 2, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPEED, 2, attacker, false, this);
		}
	}
}

/***************************************************************************************
*  Increases the user's Speed by 2 stages. Lowers user's weight by 100kg. (Autotomize) *
***************************************************************************************/
public class Move031 : BattleMove {
	public Move031(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.SPEED, attacker, true, this)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool ret = attacker.IncreaseStat(Stats.SPEED, 2, attacker, false, this);
		if (ret) {
			attacker.effects[Effects.WeightChange] -= 1000;
			battle.Display(string.Format("{0} became nimble!",attacker.String()));
		}
		return ret ? 0 : -1;
	}
}

/****************************************************
*  Increases the user's Special Attack by 2 stages. *
****************************************************/
public class Move032 : BattleMove {
	public Move032(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.SPATK, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.SPATK, 2, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPATK, 2, attacker, false, this);
		}
	}
}

/*****************************************************
*  Increases the user's Special Defense by 2 stages. *
*****************************************************/
public class Move033 : BattleMove {
	public Move033(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.SPEED, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.SPEED, 2, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPEED, 2, attacker, false, this);
		}
	}
}

/****************************************************************************
*  Increases the user's evasion by 2 stages. Minimizes the user. (Minimize) *
****************************************************************************/
public class Move034 : BattleMove {
	public Move034(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.EVASION, attacker, true, this)) {
			return -1;
		}
		attacker.effects[Effects.Minimize] = 1;
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool ret = attacker.IncreaseStat(Stats.EVASION, 2, attacker, false, this);
		return ret ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		attacker.effects[Effects.Minimize] = 1;
		if (attacker.CanIncreaseStatStage(Stats.EVASION, attacker, false, this)) {
			attacker.IncreaseStat(Stats.EVASION, 2, attacker, false, this);
		}
	}
}

/***********************************************************************************
*  Decreases the user's Defense and Special Defense by 1 stage each. (Shell Smash) *
*  Increases the user's Attack, Speed and Special Attack by 2 stages each.         *
***********************************************************************************/
public class Move035 : BattleMove {
	public Move035(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
			attacker.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPDEF, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ATTACK, 2, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPATK, 2, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPEED, 2, attacker, false, this, showAnim);
			showAnim = false;
		}
		return 0;
	}
}

/***********************************************************************************
*  Increases the user's Speed by 2 stages, and its Attack by 1 stage. (Shift Gear) *
***********************************************************************************/
public class Move036 : BattleMove {
	public Move036(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPEED, 2, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ATTACK, 1, attacker, false, this, showAnim);
			showAnim = false;
		}
		return 0;
	}
}

/********************************************************************************
*  Increases one random stat of the user by 2 stages (except HP). (Acupressure) *
********************************************************************************/
public class Move037 : BattleMove {
	public Move037(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.index != opponent.index) {
			if ((opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) || opponent.OwnSide().effects[Effects.CraftyShield] != 0) {
				battle.Display(string.Format("But it failed!"));
				return -1;
			}
		}
		List<int> arr = new List<int>();
		int[] stats = new int[7]{Stats.ATTACK, Stats.DEFENSE, Stats.SPEED, Stats.SPATK, Stats.SPDEF, Stats.ACCURACY, Stats.EVASION};
		for (int i=0; i<stats.Length; i++) 
		{
			if (opponent.CanIncreaseStatStage(stats[i], attacker, false, this)) {
				arr.Add(stats[i]);
			}
		}
		if (arr.Count == 0) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",opponent.String()));
			return -1;
		}
		int stat = arr[battle.Rand(arr.Count)];
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.IncreaseStat(stat, 2, attacker, false, this);
		return 0;
	}
}

/*********************************************
*  Increases the user's Defense by 3 stages. *
*********************************************/
public class Move038 : BattleMove {
	public Move038(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.DEFENSE, 3, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
			attacker.IncreaseStat(Stats.DEFENSE, 3, attacker, false, this);
		}
	}
}

/****************************************************
*  Increases the user's Special Attack by 3 stages. *
****************************************************/
public class Move039 : BattleMove {
	public Move039(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.SPATK, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.SPATK, 3, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPATK, 3, attacker, false, this);
		}
	}
}

/**************************************************************************************
*  Reduces the user's HP by half of max, and sets its Attack to maximum. (Belly Drum) *
**************************************************************************************/
public class Move03A : BattleMove {
	public Move03A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.hp <= (attacker.totalHP/2) || !attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.ReduceHP(attacker.totalHP/2);
		if (attacker.HasWorkingAbility(Abilities.CONTRARY)) {
			attacker.stages[Stats.ATTACK] = -6;
			battle.CommonAnimation("StatDown", attacker, null);
			battle.Display(string.Format("{0} cut its own HP and minimized its Attack!",attacker.String()));
		} else {
			attacker.stages[Stats.ATTACK] = 6;
			battle.CommonAnimation("StatUp", attacker, null);
			battle.Display(string.Format("{0} cut its own HP and maximized its Attack!",attacker.String()));
		}
		return 0;
	}
}

/*************************************************************************
*  Decreases the user's Attack and Defense by 1 stage each. (Superpower) *
*************************************************************************/
public class Move03B : BattleMove {
	public Move03B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			bool showAnim = true;
			if (attacker.CanReduceStatStage(Stats.ATTACK, attacker, false, this)) {
				attacker.ReduceStat(Stats.ATTACK, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
			if (attacker.CanReduceStatStage(Stats.DEFENSE, attacker, false, this)) {
				attacker.ReduceStat(Stats.DEFENSE, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
		}
		return ret;
	}
}

/************************************************************************************
*  Decreases the user's Defense and Special Defense by 1 stage each. (Close Combat) *
************************************************************************************/
public class Move03C : BattleMove {
	public Move03C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			bool showAnim = true;
			if (attacker.CanReduceStatStage(Stats.DEFENSE, attacker, false, this)) {
				attacker.ReduceStat(Stats.DEFENSE, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
			if (attacker.CanReduceStatStage(Stats.SPDEF, attacker, false, this)) {
				attacker.ReduceStat(Stats.SPDEF, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
		}
		return ret;
	}
}

/****************************************************************************
*  Decreases the user's Defense, Special Defense and Speed by 1 stage each. *
*  User's ally loses 1/16 of its total HP. (V-create)                       *
****************************************************************************/
public class Move03D : BattleMove {
	public Move03D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			if (attacker.Partner() != null && !attacker.Partner().Fainted()) {
				attacker.Partner().ReduceHP(attacker.Partner().totalHP/16, true);
			}
			bool showAnim = true;
			if (attacker.CanReduceStatStage(Stats.SPEED, attacker, false, this)) {
				attacker.ReduceStat(Stats.SPEED, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
			if (attacker.CanReduceStatStage(Stats.DEFENSE, attacker, false, this)) {
				attacker.ReduceStat(Stats.DEFENSE, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
			if (attacker.CanReduceStatStage(Stats.SPDEF, attacker, false, this)) {
				attacker.ReduceStat(Stats.SPDEF, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
		}
		return ret;
	}
}

/******************************************
*  Decreases the user's Speed by 1 stage. *
******************************************/
public class Move03E : BattleMove {
	public Move03E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			if (attacker.CanReduceStatStage(Stats.SPEED, attacker, false, this)) {
				attacker.ReduceStat(Stats.SPEED, 1, attacker, false, this);
			}
		}
		return ret;
	}
}

/****************************************************
*  Decreases the user's Special Attack by 2 stages. *
****************************************************/
public class Move03F : BattleMove {
	public Move03F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			if (attacker.CanReduceStatStage(Stats.SPATK, attacker, false, this)) {
				attacker.ReduceStat(Stats.SPATK, 2, attacker, false, this);
			}
		}
		return ret;
	}
}

/************************************************************************************
*  Increases the target's Special Attack by 1 stage. Confuses the target. (Flatter) *
************************************************************************************/
public class Move040 : BattleMove {
	public Move040(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("{0}'s attack missed!",attacker.String()));
			return -1;
		}
		int ret = -1;
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
			opponent.IncreaseStat(Stats.SPATK, 1, attacker, false, this);
			ret = 0;
		}
		if (opponent.CanConfuse(attacker, true, this)) {
			opponent.Confuse();
			battle.Display(string.Format("{0} became confused!",opponent.String()));
			ret = 0;
		}
		return ret;
	}
}

/*****************************************************************************
*  Increases the target's Attack by 2 stages. Confuses the target. (Swagger) *
*****************************************************************************/
public class Move041 : BattleMove {
	public Move041(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("{0}'s attack missed!",attacker.String()));
			return -1;
		}
		int ret = -1;
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			opponent.IncreaseStat(Stats.ATTACK, 1, attacker, false, this);
			ret = 0;
		}
		if (opponent.CanConfuse(attacker, true, this)) {
			opponent.Confuse();
			battle.Display(string.Format("{0} became confused!",opponent.String()));
			ret = 0;
		}
		return ret;
	}
}

/*********************************************
*  Decreases the target's Attack by 1 stage. *
*********************************************/
public class Move042 : BattleMove {
	public Move042(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.ATTACK, 1, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.ATTACK, 1, attacker, false, this);
		}
	}
}

/**********************************************
*  Decreases the target's Defense by 1 stage. *
**********************************************/
public class Move043 : BattleMove {
	public Move043(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanReduceStatStage(Stats.DEFENSE, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.ReduceStat(Stats.DEFENSE, 1, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (attacker.CanReduceStatStage(Stats.DEFENSE, attacker, false, this)) {
			attacker.ReduceStat(Stats.DEFENSE, 1, attacker, false, this);
		}
	}
}

/********************************************
*  Decreases the target's Speed by 1 stage. *
********************************************/
public class Move044 : BattleMove {
	public Move044(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanReduceStatStage(Stats.SPEED, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.ReduceStat(Stats.SPEED, 1, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (attacker.CanReduceStatStage(Stats.SPEED, attacker, false, this)) {
			attacker.ReduceStat(Stats.SPEED, 1, attacker, false, this);
		}
	}

	public new int ModifyDamage(int damageMult, Battler attacker, Battler opponent) {
		if (id == Moves.BULLDOZE && battle.field.effects[Effects.GrassyTerrain] > 0) {
			return (int)Math.Round(damageMult/2.0);
		}
		return damageMult;
	}
}

/*****************************************************
*  Decreases the target's Special Attack by 1 stage. *
*****************************************************/
public class Move045 : BattleMove {
	public Move045(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanReduceStatStage(Stats.SPATK, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.ReduceStat(Stats.SPATK, 1, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (attacker.CanReduceStatStage(Stats.SPATK, attacker, false, this)) {
			attacker.ReduceStat(Stats.SPATK, 1, attacker, false, this);
		}
	}
}

/******************************************************
*  Decreases the target's Special Defense by 1 stage. *
******************************************************/
public class Move046 : BattleMove {
	public Move046(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanReduceStatStage(Stats.SPDEF, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.ReduceStat(Stats.SPDEF, 1, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (attacker.CanReduceStatStage(Stats.SPDEF, attacker, false, this)) {
			attacker.ReduceStat(Stats.SPDEF, 1, attacker, false, this);
		}
	}
}

/***********************************************
*  Decreases the target's accuracy by 1 stage. *
***********************************************/
public class Move047 : BattleMove {
	public Move047(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanReduceStatStage(Stats.ACCURACY, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.ReduceStat(Stats.ACCURACY, 1, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (attacker.CanReduceStatStage(Stats.ACCURACY, attacker, false, this)) {
			attacker.ReduceStat(Stats.ACCURACY, 1, attacker, false, this);
		}
	}
}

/************************************************************************
*  Decreases the target's evasion by 1 stage OR 2 stages. (Sweet Scent) *
************************************************************************/
public class Move048 : BattleMove {
	public Move048(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanReduceStatStage(Stats.EVASION, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int increment = (Settings.USE_NEW_BATTLE_MECHANICS ? 2 : 1);
		return attacker.ReduceStat(Stats.EVASION, increment, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (attacker.CanReduceStatStage(Stats.EVASION, attacker, false, this)) {
			int increment = (Settings.USE_NEW_BATTLE_MECHANICS ? 2 : 1);
			attacker.ReduceStat(Stats.EVASION, increment, attacker, false, this);
		}
	}
}

/************************************************************************
*  Decreases the target's evasion by 1 stage. }s all barriers and entry *
*  hazards for the target's side OR on both sides. (Defog)              *
************************************************************************/
public class Move049 : BattleMove {
	public Move049(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.ReduceStat(Stats.EVASION, 1, attacker, false, this);
		opponent.OwnSide().effects[Effects.Reflect] = 0;
		opponent.OwnSide().effects[Effects.LightScreen] = 0;
		opponent.OwnSide().effects[Effects.Mist] = 0;
		opponent.OwnSide().effects[Effects.Safeguard] = 0;
		opponent.OwnSide().effects[Effects.Spikes] = 0;
		opponent.OwnSide().effects[Effects.StealthRock] = 0;
		opponent.OwnSide().effects[Effects.StickyWeb] = 0;
		opponent.OwnSide().effects[Effects.ToxicSpikes] = 0;
		if (Settings.USE_NEW_BATTLE_MECHANICS) {
			opponent.OpposingSide().effects[Effects.Reflect] = 0;
			opponent.OpposingSide().effects[Effects.LightScreen] = 0;
			opponent.OpposingSide().effects[Effects.Mist] = 0;
			opponent.OpposingSide().effects[Effects.Safeguard] = 0;
			opponent.OpposingSide().effects[Effects.Spikes] = 0;
			opponent.OpposingSide().effects[Effects.StealthRock] = 0;
			opponent.OpposingSide().effects[Effects.StickyWeb] = 0;
			opponent.OpposingSide().effects[Effects.ToxicSpikes] = 0;
		}
		return 0;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (!opponent.damageState.Substitute) {
			if (opponent.CanReduceStatStage(Stats.EVASION, attacker, false, this)) {
				opponent.ReduceStat(Stats.EVASION, 1, attacker, false, this);
			}
		}
		opponent.ReduceStat(Stats.EVASION, 1, attacker, false, this);
		opponent.OwnSide().effects[Effects.Reflect] = 0;
		opponent.OwnSide().effects[Effects.LightScreen] = 0;
		opponent.OwnSide().effects[Effects.Mist] = 0;
		opponent.OwnSide().effects[Effects.Safeguard] = 0;
		opponent.OwnSide().effects[Effects.Spikes] = 0;
		opponent.OwnSide().effects[Effects.StealthRock] = 0;
		opponent.OwnSide().effects[Effects.StickyWeb] = 0;
		opponent.OwnSide().effects[Effects.ToxicSpikes] = 0;
		if (Settings.USE_NEW_BATTLE_MECHANICS) {
			opponent.OpposingSide().effects[Effects.Reflect] = 0;
			opponent.OpposingSide().effects[Effects.LightScreen] = 0;
			opponent.OpposingSide().effects[Effects.Mist] = 0;
			opponent.OpposingSide().effects[Effects.Safeguard] = 0;
			opponent.OpposingSide().effects[Effects.Spikes] = 0;
			opponent.OpposingSide().effects[Effects.StealthRock] = 0;
			opponent.OpposingSide().effects[Effects.StickyWeb] = 0;
			opponent.OpposingSide().effects[Effects.ToxicSpikes] = 0;
		}
	}
}

/***********************************************************************
*  Decreases the target's Attack and Defense by 1 stage each. (Tickle) *
***********************************************************************/
public class Move04A : BattleMove {
	public Move04A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("{0}'s attack missed!",attacker.String()));
			return -1;
		}
		if (opponent.TooLow(Stats.ATTACK) && opponent.TooLow(Stats.DEFENSE)) {
			battle.Display(string.Format("{0}'s stats won't go any lower!",opponent.String()));
			return -1;
		}
		if (opponent.OwnSide().effects[Effects.Mist] > 0) {
			battle.Display(string.Format("{0} is protected by Mist!",opponent.String()));
			return -1;
		}
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.CLEARBODY) || opponent.HasWorkingAbility(Abilities.WHITESMOKE)) {
				battle.Display(string.Format("{0}'s {1} prevents stat loss!",opponent.String(), Abilities.GetName(opponent.ability)));
				return -1;
			}
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int ret = -1;
		bool showAnim = true;
		if (!attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.HYPERCUTTER) && !opponent.TooLow(Stats.ATTACK)) {
			string abilityName = Abilities.GetName(opponent.ability);
			battle.Display(string.Format("{0}'s {1} prevents Attack loss!",opponent.String(), abilityName));
		} else if (opponent.ReduceStat(Stats.ATTACK, 1, attacker, false, this, showAnim)) {
			ret = 0;
			showAnim = false;
		}
		if (!attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.BIGPECKS) && !opponent.TooLow(Stats.DEFENSE)) {
			string abilityName = Abilities.GetName(opponent.ability);
			battle.Display(string.Format("{0}'s {1} prevents Defense loss!",opponent.String(), abilityName));
		} else if (opponent.ReduceStat(Stats.DEFENSE, 1, attacker, false, this, showAnim)) {
			ret = 0;
			showAnim = false;
		}
		return ret;
	}
}

/**********************************************
*  Decreases the target's Attack by 2 stages. *
**********************************************/
public class Move04B : BattleMove {
	public Move04B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanReduceStatStage(Stats.ATTACK, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.ReduceStat(Stats.ATTACK, 2, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (attacker.CanReduceStatStage(Stats.ATTACK, attacker, false, this)) {
			attacker.ReduceStat(Stats.ATTACK, 2, attacker, false, this);
		}
	}
}

/*********************************************************
*  Decreases the target's Defense by 2 stages. (Screech) *
*********************************************************/
public class Move04C : BattleMove {
	public Move04C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!attacker.CanReduceStatStage(Stats.DEFENSE, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.ReduceStat(Stats.DEFENSE, 2, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (attacker.CanReduceStatStage(Stats.DEFENSE, attacker, false, this)) {
			attacker.ReduceStat(Stats.DEFENSE, 2, attacker, false, this);
		}
	}
}

/*************************************************************************************
*  Decreases the target's Speed by 2 stages. (Cotton Spore, Scary Face, String Shot) *
*************************************************************************************/
public class Move04D : BattleMove {
	public Move04D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (TypeImmunityByAbility(GetType(type, attacker, opponent), attacker, opponent)) {
			return -1;
		}
		if (!attacker.CanReduceStatStage(Stats.SPEED, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int increment = (id == Moves.STRINGSHOT && !Settings.USE_NEW_BATTLE_MECHANICS) ? 1 : 2;
		return attacker.ReduceStat(Stats.SPEED, increment, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (attacker.CanReduceStatStage(Stats.SPEED, attacker, false, this)) {
			int increment = (id == Moves.STRINGSHOT && !Settings.USE_NEW_BATTLE_MECHANICS) ? 1 : 2;
			attacker.ReduceStat(Stats.SPEED, increment, attacker, false, this);
		}
	}
}

/*********************************************************************************
*  Decreases the target's Special Attack by 2 stages. Only works on the opposite *
*  g}er. (Captivate)                                                             *
*********************************************************************************/
public class Move04E : BattleMove {
	public Move04E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (!opponent.CanReduceStatStage(Stats.SPATK, attacker, true, null)) {
			return -1;
		}
		if (attacker.gender == 2 || opponent.gender == 2 || attacker.gender == opponent.gender) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (!attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.OBLIVIOUS)) {
			battle.Display(string.Format("{0}'s {1} prevents romance!",opponent.String(), Abilities.GetName(opponent.ability)));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return opponent.ReduceStat(Stats.SPATK, 2, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (attacker.gender == 2 || opponent.gender == 2 || attacker.gender == opponent.gender) {
			if (!attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.OBLIVIOUS)) {
				if (!opponent.CanReduceStatStage(Stats.SPATK, attacker, true, null)) {
					opponent.ReduceStat(Stats.SPATK, 2, attacker, false, this);
				}
			}
		}
	}
}

/*******************************************************
*  Decreases the target's Special Defense by 2 stages. *
*******************************************************/
public class Move04F : BattleMove {
	public Move04F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (TypeImmunityByAbility(GetType(type, attacker, opponent), attacker, opponent)) {
			return -1;
		}
		if (!attacker.CanReduceStatStage(Stats.SPDEF, attacker, true, null)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int increment = (id == Moves.STRINGSHOT && !Settings.USE_NEW_BATTLE_MECHANICS) ? 1 : 2;
		return attacker.ReduceStat(Stats.SPDEF, increment, attacker, false, this) ? 0 : -1;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (attacker.CanReduceStatStage(Stats.SPDEF, attacker, false, this)) {
			int increment = (id == Moves.STRINGSHOT && !Settings.USE_NEW_BATTLE_MECHANICS) ? 1 : 2;
			attacker.ReduceStat(Stats.SPDEF, increment, attacker, false, this);
		}
	}
}

/******************************************************
*  Resets all target's stat stages to 0. (Clear Smog) *
******************************************************/
public class Move050 : BattleMove {
	public Move050(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0 && !opponent.damageState.Substitute) {
			opponent.stages[Stats.ATTACK] = 0;
			opponent.stages[Stats.DEFENSE] = 0;
			opponent.stages[Stats.SPEED] = 0;
			opponent.stages[Stats.SPATK] = 0;
			opponent.stages[Stats.SPDEF] = 0;
			opponent.stages[Stats.ACCURACY] = 0;
			opponent.stages[Stats.EVASION] = 0;
			battle.Display(string.Format("{0}'s stat changes were removed!",opponent.String()));
		}
		return ret;
	}
}

/********************************************************
*  Resets all stat stages for all battlers to 0. (Haze) *
********************************************************/
public class Move051 : BattleMove {
	public Move051(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		for (int i=0; i<4; i++) 
		{
			battle.battlers[i].stages[Stats.ATTACK] = 0;
			battle.battlers[i].stages[Stats.DEFENSE] = 0;
			battle.battlers[i].stages[Stats.SPEED] = 0;
			battle.battlers[i].stages[Stats.SPATK] = 0;
			battle.battlers[i].stages[Stats.SPDEF] = 0;
			battle.battlers[i].stages[Stats.ACCURACY] = 0;
			battle.battlers[i].stages[Stats.EVASION] = 0;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		battle.Display(string.Format("All stat changes were eliminated!"));
		return 0;
	}
}

/**********************************************************************************
*  User and target swap their Attack and Special Attack stat stages. (Power Swap) *
**********************************************************************************/
public class Move052 : BattleMove {
	public Move052(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int temp = attacker.stages[Stats.ATTACK];
		attacker.stages[Stats.ATTACK] = opponent.stages[Stats.ATTACK];
		opponent.stages[Stats.ATTACK] = temp;
		temp = attacker.stages[Stats.SPATK];
		attacker.stages[Stats.SPATK] = opponent.stages[Stats.SPATK];
		opponent.stages[Stats.SPATK] = temp;
		battle.Display(string.Format("{0} switched all changes to its Attack and Sp. Atk with the targets!",attacker.String()));
		return 0;
	}
}

/************************************************************************************
*  User and target swap their Defense and Special Defense stat stages. (Guard Swap) *
************************************************************************************/
public class Move053 : BattleMove {
	public Move053(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int temp = attacker.stages[Stats.DEFENSE];
		attacker.stages[Stats.DEFENSE] = opponent.stages[Stats.DEFENSE];
		opponent.stages[Stats.DEFENSE] = temp;
		temp = attacker.stages[Stats.SPDEF];
		attacker.stages[Stats.SPDEF] = opponent.stages[Stats.SPDEF];
		opponent.stages[Stats.SPDEF] = temp;
		battle.Display(string.Format("{0} switched all changes to its Defense and Sp. Def with the targets!",attacker.String()));
		return 0;
	}
}

/************************************************************
*  User and target swap all their stat stages. (Heart Swap) *
************************************************************/
public class Move054 : BattleMove {
	public Move054(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int[] stats = new int[7]{Stats.ATTACK, Stats.DEFENSE, Stats.SPEED, Stats.SPATK, Stats.SPDEF, Stats.ACCURACY, Stats.EVASION};
		for (int i=0; i<stats.Length; i++) 
		{
			int temp = attacker.stages[stats[i]];
			attacker.stages[stats[i]] = opponent.stages[stats[i]];
			opponent.stages[stats[i]] = temp;
		}
		battle.Display(string.Format("{0} switched stat changes with the targets!",attacker.String()));
		return 0;
	}
}

/****************************************************
*  User copies the target's stat stages. (Psych Up) *
****************************************************/
public class Move055 : BattleMove {
	public Move055(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.OwnSide().effects[Effects.CraftyShield] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int[] stats = new int[7]{Stats.ATTACK, Stats.DEFENSE, Stats.SPEED, Stats.SPATK, Stats.SPDEF, Stats.ACCURACY, Stats.EVASION};
		for (int i=0; i<stats.Length; i++) 
		{
			attacker.stages[stats[i]] = opponent.stages[stats[i]];
		}
		battle.Display(string.Format("{0} copied {1}'s stat changed!",attacker.String(), opponent.String(true)));
		return 0;
	}
}

/*********************************************************************************
*  For 5 rounds, user's and ally's stat stages cannot be lowered by foes. (Mist) *
*********************************************************************************/
public class Move056 : BattleMove {
	public Move056(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OwnSide().effects[Effects.Mist] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.OwnSide().effects[Effects.Mist] = 5;
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display(string.Format("Your team became shrouded in mist!"));
		} else {
			battle.Display(string.Format("The opposing team became shrouded in mist!"));
		}
		return 0;
	}
}

/************************************************************
*  Swaps the user's Attack and Defense stats. (Power Trick) *
************************************************************/
public class Move057 : BattleMove {
	public Move057(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int temp = attacker.attack;
		attacker.attack = attacker.defense;
		attacker.defense = temp;
		attacker.effects[Effects.PowerTrick] = (attacker.effects[Effects.PowerTrick] == 0) ? 1 : 0;
		battle.Display(string.Format("{0} switched its Attack and Defense!", attacker.String())); 
		return 0;
	}
}

/******************************************************************
*  Averages the user's and target's Attack.                       *
*  Averages the user's and target's Special Attack. (Power Split) *
******************************************************************/
public class Move058 : BattleMove {
	public Move058(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int avatk = (attacker.attack+opponent.attack)/2;
		int avspatk = (attacker.specialAttack+opponent.specialAttack)/2;
		attacker.attack = avatk;
		opponent.attack = avatk;
		attacker.specialAttack = avspatk;
		opponent.specialAttack = avspatk;
		battle.Display(string.Format("{0} shared its power with the target!",attacker.String()));
		return 0;
	}
}

/*******************************************************************
*  Averages the user's and target's Defense.                       *
*  Averages the user's and target's Special Defense. (Guard Split) *
*******************************************************************/
public class Move059 : BattleMove {
	public Move059(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int avdef = (attacker.defense+opponent.defense)/2;
		int avspdef = (attacker.specialDefense+opponent.specialDefense)/2;
		attacker.defense = avdef;
		opponent.defense = avdef;
		attacker.specialDefense = avspdef;
		opponent.specialDefense = avspdef;
		battle.Display(string.Format("{0} shared its power with the target!",attacker.String()));
		return 0;
	}
}

/*************************************************************
*  Averages the user's and target's current HP. (Pain Split) *
*************************************************************/
public class Move05A : BattleMove {
	public Move05A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int olda = attacker.hp;
		int oldo = opponent.hp;
		int avhp = (attacker.hp+opponent.hp)/2;
		attacker.hp = (int)Math.Min(avhp, attacker.totalHP);
		opponent.hp = (int)Math.Min(avhp, opponent.totalHP);
		battle.scene.HPChanged(attacker, olda);
		battle.scene.HPChanged(opponent, oldo);
		battle.Display(string.Format("The battlers shared their pain!"));
		return 0;
	}
}

/**********************************************************************************
*  For 4 rounds, doubles the Speed of all battlers on the user's side. (Tailwind) *
**********************************************************************************/
public class Move05B : BattleMove {
	public Move05B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.OwnSide().effects[Effects.Tailwind] = 4;
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display(string.Format("The tailwind blew from behind your team!"));
		} else {
			battle.Display(string.Format("The tailwind blew from behind the opposing team!"));
		}
		return 0;
	}
}

/******************************************************************************
*  This move turns into the last move used by the target, until user switches *
*  out. (Mimic)                                                               *
******************************************************************************/
public class Move05C : BattleMove {
	public Move05C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.Transform] != 0 || opponent.lastMoveUsed <= 0 || (new Moves.Move(opponent.lastMoveUsed).Function() == 0x02) || (new Moves.Move(opponent.lastMoveUsed).Function() == 0x14) || (new Moves.Move(opponent.lastMoveUsed).Function() == 0x5C) || (new Moves.Move(opponent.lastMoveUsed).Function() == 0x5D) || (new Moves.Move(opponent.lastMoveUsed).Function() == 0xB6)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		for (int i=0; i<attacker.moves.Length; i++) 
		{
			if (attacker.moves[i].id == opponent.lastMoveUsed) {
				battle.Display(string.Format("But it failed!"));
				return -1;
			}
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		for (int i=0; i<attacker.moves.Length; i++) 
		{
			if (attacker.moves[i].id == id) {
				Moves.Move newMove = new Moves.Move(opponent.lastMoveUsed);
				attacker.moves[i] = BattleMove.FromBattleMove(battle, newMove);
				string moveName = Moves.GetName(opponent.lastMoveUsed);
				battle.Display(string.Format("{0} learned {1}!",attacker.String(), moveName));
				return 0;
			}
		}
		battle.Display(string.Format("But it failed!"));
		return -1;
	}
}

/*******************************************************************************
*  This move permanently turns into the last move used by the target. (Sketch) *
*******************************************************************************/
public class Move05D : BattleMove {
	public Move05D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.Transform] != 0 || opponent.lastMoveUsedSketch <= 0 || (new Moves.Move(opponent.lastMoveUsedSketch).Function() == 0x02) || (new Moves.Move(opponent.lastMoveUsedSketch).Function() == 0x14) || (new Moves.Move(opponent.lastMoveUsedSketch).Function() == 0x5D)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		for (int i=0; i<attacker.moves.Length; i++) 
		{
			if (attacker.moves[i].id == opponent.lastMoveUsedSketch) {
				battle.Display(string.Format("But it failed!"));
				return -1;
			}
		}
		if (opponent.OwnSide().effects[Effects.CraftyShield] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		for (int i=0; i<attacker.moves.Length; i++) 
		{
			if (attacker.moves[i].id == id) {
				Moves.Move newMove = new Moves.Move(opponent.lastMoveUsedSketch);
				attacker.moves[i] = BattleMove.FromBattleMove(battle, newMove);
				Battler[] party = battle.Party(attacker.index);
				party[attacker.pokemonIndex].pokemon.moves[i] = newMove;
				string moveName = Moves.GetName(opponent.lastMoveUsed);
				battle.Display(string.Format("{0} learned {1}!",attacker.String(), moveName));
				return 0;
			}
		}
		battle.Display(string.Format("But it failed!"));
		return -1;
	}
}

/********************************************************************************
*  Changes user's type to that of a random user's move, except this one, OR the *
*  user's first move's type. (Conversion)                                       *
********************************************************************************/
public class Move05E : BattleMove {
	public Move05E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.ability == Abilities.MULTITYPE) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		List<int> types = new List<int>();
		for (int i=1; i<attacker.moves.Length; i++) 
		{
			if (attacker.moves[i].id == id) {
				continue;
			}
			if (Types.IsPseudoType(i)) {
				continue;
			}
			if (attacker.HasType(attacker.moves[i].type)) {
				continue;
			}
			if (!types.Contains(attacker.moves[i].type)) {
				types.Add(attacker.moves[i].type);
				if (Settings.USE_NEW_BATTLE_MECHANICS) {
					break;
				}
			}
			if (types.Count == 0) {
				battle.Display(string.Format("But it failed!"));
				return -1;
			}
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int newtype = types[battle.Rand(types.Count)];
		attacker.type1 = newtype;
		attacker.type2 = newtype;
		attacker.effects[Effects.Type3] = newtype;
		string typename = Types.GetName(newtype);
		battle.Display(string.Format("{0} transformed into the {1} type!",attacker.String(), typename));
		return 0;
	}
}

/*******************************************************************************
*  Changes user's type to a random one that resists/is immune to the last move *
*  used by the target. (Conversion 2)                                          *
*******************************************************************************/
public class Move05F : BattleMove {
	public Move05F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.ability == Abilities.MULTITYPE) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (opponent.lastMoveUsed <= 0 || Types.IsPseudoType(new Moves.Move(opponent.lastMoveUsed).Type())) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (opponent.OwnSide().effects[Effects.CraftyShield] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		List<int> types = new List<int>();
		int atype = opponent.lastMoveUsedType;
		if (atype < 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		for (int i=0; i<Types.MaxValue(); i++) 
		{
			if (Types.IsPseudoType(i)) {
				continue;
			}
			if (attacker.HasType(i)) {
				continue;
			}
			if (Types.GetEffectiveness(atype, i) < 2) {
				types.Add(i);
			}
		}
		if (types.Count == 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int newtype = types[battle.Rand(types.Count)];
		attacker.type1 = newtype;
		attacker.type2 = newtype;
		attacker.effects[Effects.Type3] = newtype;
		string typename = Types.GetName(newtype);
		battle.Display(string.Format("{0} transformed into the {1} type!",attacker.String(), typename));
		return 0;
	}
}

/****************************************************************
*  Changes user's type dep}ing on the environment. (Camouflage) *
****************************************************************/
public class Move060 : BattleMove {
	public Move060(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.ability == Abilities.MULTITYPE) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		int t = Types.NORMAL;
		switch (battle.environment) 
		{
			case Environment.None:
				t = Types.NORMAL;
				break;
			case Environment.Grass:
				t = Types.GRASS;
				break;
			case Environment.TallGrass:
				t = Types.GRASS;
				break;
			case Environment.MovingWater:
				t = Types.WATER;
				break;
			case Environment.StillWater:
				t = Types.WATER;
				break;
			case Environment.Underwater:
				t = Types.WATER;
				break;
			case Environment.Cave:
				t = Types.ROCK;
				break;
			case Environment.Rock:
				t = Types.GROUND;
				break;
			case Environment.Sand:
				t = Types.GROUND;
				break;
			case Environment.Forest:
				t = Types.BUG;
				break;
			case Environment.Snow:
				t = Types.ICE;
				break;
			case Environment.Volcano:
				t = Types.FIRE;
				break;
			case Environment.Graveyard:
				t = Types.GHOST;
				break;
			case Environment.Sky:
				t = Types.FLYING;
				break;
			case Environment.Space:
				t = Types.DRAGON;
				break;
		}
		if (battle.field.effects[Effects.ElectricTerrain] > 0) {
			t = Types.ELECTRIC;
		}
		if (battle.field.effects[Effects.GrassyTerrain] > 0) {
			t = Types.GRASS;
		}
		if (battle.field.effects[Effects.MistyTerrain] > 0) {
			t = Types.FAIRY;
		}
		if (attacker.HasType(t)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.type1 = t;
		attacker.type2 = t;
		attacker.effects[Effects.Type3] = t;
		string typename = Types.GetName(t);
		battle.Display(string.Format("{0} transformed into the {1} type!",attacker.String(), typename));
		return 0;
	}
}

/*************************************
*  Target becomes Water type. (Soak) *
*************************************/
public class Move061 : BattleMove {
	public Move061(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (TypeImmunityByAbility(GetType(type, attacker, opponent), attacker, opponent)) {
			return -1;
		}
		if (opponent.ability == Abilities.MULTITYPE) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.type1 == Types.WATER && opponent.type2 == Types.WATER && (opponent.effects[Effects.Type3] < 0 || opponent.effects[Effects.Type3] == Types.WATER)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		opponent.type1 = Types.WATER;
		opponent.type2 = Types.WATER;
		opponent.effects[Effects.Type3] = Types.WATER;
		string typename = Types.GetName(Types.WATER);
		battle.Display(string.Format("{0} transformed into the {1} type!",attacker.String(), typename));
		return 0;
	}
}

/*********************************************
*  User copes target's types. (Reflect Type) *
*********************************************/
public class Move062 : BattleMove {
	public Move062(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.ability == Abilities.MULTITYPE) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (attacker.HasType(opponent.type1) && attacker.HasType(opponent.type2) && attacker.HasType(opponent.effects[Effects.Type3]) && opponent.HasType(attacker.type1) && opponent.HasType(attacker.type2) && opponent.HasType(attacker.effects[Effects.Type3])) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.type1 = opponent.type1;
		attacker.type2 = opponent.type2;
		attacker.effects[Effects.Type3] = -1;
		battle.Display(string.Format("{0}'s type changed to match {1}'s!",attacker.String(), opponent.String(true)));
		return 0;
	}
}

/**************************************************
*  Target's ability becomes Simple. (Simple Beam) *
**************************************************/
public class Move063 : BattleMove {
	public Move063(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (opponent.ability == Abilities.MULTITYPE || opponent.ability == Abilities.SIMPLE || opponent.ability == Abilities.STANCECHANGE || opponent.ability == Abilities.TRUANT) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int oldabil = opponent.ability;
		opponent.ability = Abilities.SIMPLE;
		string abilityName = Abilities.GetName(Abilities.SIMPLE);
		battle.Display(string.Format("{0} acquired {1}!",opponent.String(), abilityName));
		if (opponent.effects[Effects.Illusion] != 0 && oldabil == Abilities.ILLUSION) {
			Debug.Log(string.Format("[Ability triggered] {0}'s Illusion ended",opponent.String()));
			opponent.effects[Effects.Illusion] = 0;
			battle.scene.ChangePokemon(opponent, opponent.pokemon);
			battle.Display(string.Format("{0}'s {1} wore off!",opponent.String(), Abilities.GetName(oldabil)));
		}
		return 0;
	}
}

/***************************************************
*  Target's ability becomes Insomnia. (Worry Seed) *
***************************************************/
public class Move064 : BattleMove {
	public Move064(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (opponent.ability == Abilities.MULTITYPE || opponent.ability == Abilities.SIMPLE || opponent.ability == Abilities.STANCECHANGE || opponent.ability == Abilities.TRUANT) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int oldabil = opponent.ability;
		opponent.ability = Abilities.INSOMNIA;
		string abilityName = Abilities.GetName(Abilities.INSOMNIA);
		battle.Display(string.Format("{0} acquired {1}!",opponent.String(), abilityName));
		if (opponent.effects[Effects.Illusion] != 0 && oldabil == Abilities.ILLUSION) {
			Debug.Log(string.Format("[Ability triggered] {0}'s Illusion ended",opponent.String()));
			opponent.effects[Effects.Illusion] = 0;
			battle.scene.ChangePokemon(opponent, opponent.pokemon);
			battle.Display(string.Format("{0}'s {1} wore off!",opponent.String(), Abilities.GetName(oldabil)));
		}
		return 0;
	}
}

/*********************************************
*  User copies target's ability. (Role Play) *
*********************************************/
public class Move065 : BattleMove {
	public Move065(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.OwnSide().effects[Effects.CraftyShield] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int oldabil = attacker.ability;
		attacker.ability = opponent.ability;
		string abilityName = Abilities.GetName(opponent.ability);
		battle.Display(string.Format("{0} copied {1}'s {2}!",attacker.String(), opponent.String(true), abilityName));
		if (opponent.effects[Effects.Illusion] != 0 && oldabil == Abilities.ILLUSION) {
			Debug.Log(string.Format("[Ability triggered] {0}'s Illusion ended",opponent.String()));
			opponent.effects[Effects.Illusion] = 0;
			battle.scene.ChangePokemon(attacker, attacker.pokemon);
			battle.Display(string.Format("{0}'s {1} wore off!",attacker.String(), Abilities.GetName(oldabil)));
		}
		return 0;
	}
}

/***********************************************
*  Target copies user's ability. (Entrainment) *
***********************************************/
public class Move066 : BattleMove {
	public Move066(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (opponent.OwnSide().effects[Effects.CraftyShield] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (attacker.ability == 0 || attacker.ability == opponent.ability || opponent.ability == Abilities.FLOWERGIFT || opponent.ability == Abilities.IMPOSTER || opponent.ability == Abilities.MULTITYPE || opponent.ability == Abilities.STANCECHANGE || opponent.ability == Abilities.TRACE || opponent.ability == Abilities.TRUANT || opponent.ability == Abilities.ZENMODE || attacker.ability == Abilities.FLOWERGIFT || attacker.ability == Abilities.IMPOSTER || attacker.ability == Abilities.MULTITYPE || attacker.ability == Abilities.STANCECHANGE || attacker.ability == Abilities.TRACE || attacker.ability == Abilities.ZENMODE) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int oldabil = opponent.ability;
		opponent.ability = attacker.ability;
		string abilityName = Abilities.GetName(attacker.ability);
		battle.Display(string.Format("{0} aquired {1}!",opponent.String(), abilityName));
		if (opponent.effects[Effects.Illusion] != 0 && oldabil == Abilities.ILLUSION) {
			Debug.Log(string.Format("[Ability triggered] {0}'s Illusion ended",opponent.String()));
			opponent.effects[Effects.Illusion] = 0;
			battle.scene.ChangePokemon(opponent, opponent.pokemon);
			battle.Display(string.Format("{0}'s {1} wore off!",opponent.String(), Abilities.GetName(oldabil)));
		}
		return 0;
	}
}

/************************************************
*  User and target swap abilities. (Skill Swap) *
************************************************/
public class Move067 : BattleMove {
	public Move067(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if ((attacker.ability == 0 && opponent.ability == 0) || (attacker.ability == opponent.ability && !Settings.USE_NEW_BATTLE_MECHANICS) || attacker.ability == Abilities.ILLUSION || opponent.ability == Abilities.ILLUSION || attacker.ability == Abilities.MULTITYPE || opponent.ability == Abilities.MULTITYPE || attacker.ability == Abilities.STANCECHANGE || opponent.ability == Abilities.STANCECHANGE || attacker.ability == Abilities.WONDERGUARD || opponent.ability == Abilities.WONDERGUARD) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int oldabil = opponent.ability;
		attacker.ability = opponent.ability;
		opponent.ability = oldabil;
		battle.Display(string.Format("{0} swapped its {1} Ability with its target's {2} Ability!",attacker.String(), Abilities.GetName(opponent.ability), Abilities.GetName(attacker.ability)));
		attacker.AbilitiesOnSwitchIn(true);
		opponent.AbilitiesOnSwitchIn(true);
		return 0;
	}
}

/**********************************************
*  Target's ability is negated. (Gastro Acid) *
**********************************************/
public class Move068 : BattleMove {
	public Move068(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (opponent.ability == Abilities.MULTITYPE || opponent.ability == Abilities.STANCECHANGE) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int oldabil = opponent.ability;
		opponent.effects[Effects.GastroAcid] = 1;
		opponent.effects[Effects.Truant] = 0;
		battle.Display(string.Format("{0}'s Ability was suppressed!",opponent.String()));
		if (opponent.effects[Effects.Illusion] != 0 && oldabil == Abilities.ILLUSION) {
			Debug.Log(string.Format("[Ability triggered] {0}'s Illusion ended",opponent.String()));
			opponent.effects[Effects.Illusion] = 0;
			battle.scene.ChangePokemon(opponent, opponent.pokemon);
			battle.Display(string.Format("{0}'s {1} wore off!",opponent.String(), Abilities.GetName(oldabil)));
		}
		return 0;
	}
}

/************************************************
*  User transforms into the target. (Transform) *
************************************************/
public class Move069 : BattleMove {
	public Move069(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.Transform] != 0 || opponent.effects[Effects.Transform] != 0 || attacker.effects[Effects.Illusion] != 0 || (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) || opponent.effects[Effects.SkyDrop] != 0 || (new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() != 0xC9 || (new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() != 0xCA || (new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() != 0xCB || (new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() != 0xCC || (new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() != 0xCD || (new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() != 0xCE || (new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() != 0x14D) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (opponent.OwnSide().effects[Effects.CraftyShield] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.Transform] = 1;
		attacker.type1 = opponent.type1;
		attacker.type2 = opponent.type2;
		attacker.effects[Effects.Type3] = -1;
		attacker.ability = opponent.ability;
		attacker.ability = opponent.ability;
		attacker.ability = opponent.ability;
		attacker.ability = opponent.ability;
		attacker.ability = opponent.ability;
		attacker.ability = opponent.ability;
		int[] stats = new int[7]{Stats.ATTACK, Stats.DEFENSE, Stats.SPEED, Stats.SPATK, Stats.SPDEF, Stats.ACCURACY, Stats.EVASION};
		for (int i=0; i<stats.Length; i++) 
		{
			attacker.stages[stats[i]] = opponent.stages[stats[i]];
		}
		for (int i=0; i<4; i++) 
		{
			attacker.moves[i] = BattleMove.FromBattleMove(battle, new Moves.Move(opponent.moves[i].id));
			attacker.moves[i].pp = 5;
			attacker.moves[i].totalPP = 5;
		}
		attacker.effects[Effects.Disable] = 0;
		attacker.effects[Effects.DisableMove] = 0;
		battle.Display(string.Format("{0} transformed into {1}!",attacker.String(), opponent.String(true)));
		return 0;
	}
}

/*********************************************
*  Inflicts a fixed 20HP damage. (SonicBoom) *
*********************************************/
public class Move06A : BattleMove {
	public Move06A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		return EffectFixedDamage(20, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/***********************************************
*  Inflicts a fixed 40HP damage. (Dragon Rage) *
***********************************************/
public class Move06B : BattleMove {
	public Move06B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		return EffectFixedDamage(40, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/************************************************
*  Halves the target's current HP. (Super Fang) *
************************************************/
public class Move06C : BattleMove {
	public Move06C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		return EffectFixedDamage((int)Math.Max(opponent.hp/2, 1), attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/**************************************************************************
*  Inflicts damage equal to the user's level. (Night Shade, Seismic Toss) *
**************************************************************************/
public class Move06D : BattleMove {
	public Move06D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		return EffectFixedDamage(attacker.level, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/**********************************************************************************
*  Inflicts damage to bring the target's HP down to equal the user's HP. (}eavor) *
**********************************************************************************/
public class Move06E : BattleMove {
	public Move06E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		return EffectFixedDamage(opponent.hp-attacker.hp, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/*************************************************************************
*  Inflicts damage between 0.5 and 1.5 times the user's level. (Psywave) *
*************************************************************************/
public class Move06F : BattleMove {
	public Move06F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int dmg = (int)Math.Max(attacker.level*(battle.Rand(101)+50.0)/100, 1);
		return EffectFixedDamage(dmg, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/*****************************************************************************
*  OHKO. Accuracy increases by difference between levels of user and target. *
*****************************************************************************/
public class Move070 : BattleMove {
	public Move070(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool AccuracyCheck(Battler attacker, Battler opponent) {
		if (!attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.STURDY)) {
			battle.Display(string.Format("{0} was protected by {1}!",opponent.String(), Abilities.GetName(opponent.ability)));
			return false;
		}
		if (opponent.level > attacker.level) {
			battle.Display(string.Format("{0} is unaffected!",opponent.String()));
			return false;
		}
		return battle.Rand(100) < (accuracy + attacker.level-opponent.level);
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int damage = EffectFixedDamage(opponent.totalHP, attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.Fainted()) {
			battle.Display("It's a one-hit KO!");
		}
		return damage;
	}
}

/*******************************************************************************************
*  Counters a physical move used against the user this round, with 2x the power. (Counter) *
*******************************************************************************************/
public class Move071 : BattleMove {
	public Move071(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AddTarget(List<Battler> targets, Battler attacker) {
		if (attacker.effects[Effects.CounterTarget] >= 0 && attacker.IsOpposing(attacker.effects[Effects.CounterTarget])) {
			if (!attacker.AddTarget(targets, battle.battlers[attacker.effects[Effects.CounterTarget]])) {
				attacker.RandTarget(targets);
			}
		}
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.Counter] < 0 || opponent == null) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		return EffectFixedDamage((int)Math.Max(attacker.effects[Effects.Counter]*2, 1), attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/***********************************************************************************************
*  Counters a specical move used against the user this round, with 2x the power. (Mirror Coat) *
***********************************************************************************************/
public class Move072 : BattleMove {
	public Move072(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AddTarget(List<Battler> targets, Battler attacker) {
		if (attacker.effects[Effects.MirrorCoatTarget] >= 0 && attacker.IsOpposing(attacker.effects[Effects.MirrorCoatTarget])) {
			if (!attacker.AddTarget(targets, battle.battlers[attacker.effects[Effects.MirrorCoatTarget]])) {
				attacker.RandTarget(targets);
			}
		}
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.MirrorCoat] < 0 || opponent == null) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		return EffectFixedDamage((int)Math.Max(attacker.effects[Effects.MirrorCoat]*2, 1), attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/*******************************************************************************
*  Counters the last damaging move used against the user this round, with 1.5x *
*  the power. (Metal Burst)                                                    *
*******************************************************************************/
public class Move073 : BattleMove {
	public Move073(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AddTarget(List<Battler> targets, Battler attacker) {
		if (attacker.lastAttacker.Count > 0) {
			int lastAttacker = attacker.lastAttacker[attacker.lastAttacker.Count-1];
			if (lastAttacker >= 0 && attacker.IsOpposing(lastAttacker)) {
				if (!attacker.AddTarget(targets, battle.battlers[lastAttacker])) {
					attacker.RandTarget(targets);
				}
			}
		}
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.lastHPLost == 0 || opponent != null) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		return EffectFixedDamage((int)Math.Max(attacker.lastHPLost*1.5, 1), attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/*************************************************************
*  The target's ally loses 1/16 of its max HP. (Flame Burst) *
*************************************************************/
public class Move074 : BattleMove {
	public Move074(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			if (opponent.Partner() != null && !opponent.Partner().Fainted() && !opponent.Partner().HasWorkingAbility(Abilities.MAGICGUARD)) {
				opponent.Partner().ReduceHP(opponent.Partner().totalHP/16);
				battle.Display(string.Format("The bursting flame hit {0}!",opponent.Partner().String(true)));
			}
		}
		return ret;
	}
}

/*******************************************************************************
*  Power is doubled if the target is using Dive. (Surf)                        *
*  (Handled in Battler's pbSuccessCheck): Hits some semi-invulnerable targets. *
*******************************************************************************/
public class Move075 : BattleMove {
	public Move075(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int ModifyDamage(int damageMult, Battler attacker, Battler opponent) {
		if ((new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() == 0xCB) {
			return (int)Math.Round(damageMult*2.0);
		}
		return damageMult;
	}
}

/**********************************************************************************
*  Power is doubled if the target is using Dig. Power is halved if Grassy Terrain *
*  is in effect. (Earthquake)                                                     *
*  (Handled in Battler's pbSuccessCheck): Hits some semi-invulnerable targets.    *
**********************************************************************************/
public class Move076 : BattleMove {
	public Move076(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int ModifyDamage(int damageMult, Battler attacker, Battler opponent) {
		int ret = damageMult;
		if ((new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() == 0xCA) {
			ret = (int)Math.Round(damageMult*2.0);
		}
		if (battle.field.effects[Effects.GrassyTerrain] > 0) {
			ret = (int)Math.Round(damageMult/2.0);
		}
		return ret;
	}
}

/*******************************************************************************
*  Power is doubled if the target is using Bounce, Fly or Sky Drop. (Gust)     *
*  (Handled in Battler's pbSuccessCheck): Hits some semi-invulnerable targets. *
*******************************************************************************/
public class Move077 : BattleMove {
	public Move077(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int Damage, Battler attacker, Battler opponent) {
		if ((new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() == 0xC9 || (new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() == 0xCC || (new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() == 0xCE || opponent.effects[Effects.SkyDrop] != 0) {
			return baseDamage * 2;
		}
		return baseDamage;
	}
}

/*******************************************************************************
*  Power is doubled if the target is using Bounce, Fly or Sky Drop. (Twister)  *
*  May make the target flinch.                                                 *
*  (Handled in Battler's pbSuccessCheck): Hits some semi-invulnerable targets. *
*******************************************************************************/
public class Move078 : BattleMove {
	public Move078(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if ((new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() == 0xC9 || (new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() == 0xCC || (new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() == 0xCE || opponent.effects[Effects.SkyDrop] != 0) {
			return baseDamage * 2;
		}
		return baseDamage;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		opponent.Flinch(attacker);
	}
}

/************************************************************************************
*  Power is doubled if Fusion Flare has already been used this round. (Fusion Bolt) *
************************************************************************************/
public class Move079 : BattleMove {
	bool doubled;
	public Move079(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamageMultiplier(int damageMult, Battler attacker, Battler opponent) {
		if (battle.field.effects[Effects.FusionBolt] != 0) {
			battle.field.effects[Effects.FusionBolt] = 0;
			doubled = true;
			return damageMult*2;
		}
		return damageMult;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		doubled = false;
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			battle.field.effects[Effects.FusionFlare] = 1;
		}
		return ret;
	}

	public new void ShowAnimation(int id, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.damageState.Critical || doubled) {
			base.ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
		}
		base.ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/************************************************************************************
*  Power is doubled if Fusion Bolt has already been used this round. (Fusion Flare) *
************************************************************************************/
public class Move07A : BattleMove {
	bool doubled;
	public Move07A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamageMultiplier(int damageMult, Battler attacker, Battler opponent) {
		if (battle.field.effects[Effects.FusionFlare] != 0) {
			battle.field.effects[Effects.FusionFlare] = 0;
			doubled = true;
			return damageMult*2;
		}
		return damageMult;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		doubled = false;
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			battle.field.effects[Effects.FusionBolt] = 1;
		}
		return ret;
	}

	public new void ShowAnimation(int id, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.damageState.Critical || doubled) {
			base.ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
		}
		base.ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/***********************************************************
*  Power is doubled if the target is poisoned. (Venoshock) *
***********************************************************/
public class Move07B : BattleMove {
	public Move07B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (opponent.status == Statuses.POISON && (opponent.effects[Effects.Substitute] == 0 || IgnoresSubstitute(attacker))) {
			return baseDamage*2;
		}
		return baseDamage;
	}
}

/*******************************************************************************
*  Power is doubled if the target is paralyzed. Cures the target of paralysis. *
*  (SmellingSalt)                                                              *
*******************************************************************************/
public class Move07C : BattleMove {
	public Move07C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (opponent.status == Statuses.PARALYSIS && (opponent.effects[Effects.Substitute] == 0 || IgnoresSubstitute(attacker))) {
			return baseDamage*2;
		}
		return baseDamage;
	}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!opponent.Fainted() && opponent.damageState.CalculatedDamage > 0 && !opponent.damageState.Substitute && opponent.status == Statuses.PARALYSIS) {
			opponent.CureStatus();
		}
	}
}

/*********************************************************************************
*  Power is doubled if the target is asleep. Wakes the target up. (Wake-Up Slap) *
*********************************************************************************/
public class Move07D : BattleMove {
	public Move07D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (opponent.status == Statuses.PARALYSIS && (opponent.effects[Effects.Substitute] == 0 || IgnoresSubstitute(attacker))) {
			return baseDamage*2;
		}
		return baseDamage;
	}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!opponent.Fainted() && opponent.damageState.CalculatedDamage > 0 && !opponent.damageState.Substitute && opponent.status == Statuses.SLEEP) {
			opponent.CureStatus();
		}
	}
}

/***************************************************************************
*  Power is doubled if the user is burned, poisoned or paralyzed. (Facade) *
***************************************************************************/
public class Move07E : BattleMove {
	public Move07E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (opponent.status == Statuses.PARALYSIS || attacker.status == Statuses.BURN || attacker.status == Statuses.POISON) {
			return baseDamage*2;
		}
		return baseDamage;
	}
}

/**************************************************************
*  Power is doubled if the target has a status problem. (Hex) *
**************************************************************/
public class Move07F : BattleMove {
	public Move07F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (opponent.status > 0 && (opponent.effects[Effects.Substitute] == 0 || IgnoresSubstitute(attacker))) {
			return baseDamage*2;
		}
		return baseDamage;
	}
}

/***********************************************************************
*  Power is doubled if the target's HP is down to 1/2 or less. (Brine) *
***********************************************************************/
public class Move080 : BattleMove {
	public Move080(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (opponent.hp <= opponent.totalHP/2) {
			return baseDamage*2;
		}
		return baseDamage;
	}
}

/*********************************************************************************
*  Power is doubled if the user has lost HP due to the target's move this round. *
*  (Revenge, Avalanche)                                                          *
*********************************************************************************/
public class Move081 : BattleMove {
	public Move081(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (attacker.lastHPLost > 0 && attacker.lastAttacker.Contains(opponent.index)) {
			return baseDamage*2;
		}
		return baseDamage;
	}
}

/******************************************************************************
*  Power is doubled if the target has already lost HP this round. (Assurance) *
******************************************************************************/
public class Move082 : BattleMove {
	public Move082(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (opponent.tookDamage) {
			return baseDamage*2;
		}
		return baseDamage;
	}
}

/************************************************************************************
*  Power is doubled if a user's ally has already used this move this round. (Round) *
*  If an ally is about to use the same move, make it go next, ignoring priority.    *
************************************************************************************/
public class Move083 : BattleMove {
	public Move083(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		return baseDamage * (int)Math.Pow(2, attacker.OwnSide().effects[Effects.Round]);
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			attacker.OwnSide().effects[Effects.Round]++;
			if (attacker.Partner() != null && !attacker.Partner().HasMovedThisRound()) {
				if (battle.useMoveChoice[attacker.Partner().index] == 1) {
					BattleMove partnerMove = battle.moveChoice[attacker.Partner().index];
					if (partnerMove.function == function) {
						attacker.Partner().effects[Effects.MoveNext] = 1;
						attacker.Partner().effects[Effects.Quash] = 0;
					}
				}
			}
		}
		return ret;
	}
}

/**************************************************************************
*  Power is doubled if the target has already moved this round. (Payback) *
**************************************************************************/
public class Move084 : BattleMove {
	public Move084(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (battle.useMoveChoice[opponent.index] != 1 || opponent.HasMovedThisRound()) {
			return baseDamage*2;
		}
		return baseDamage;
	}
}

/*************************************************************************
*  Power is doubled if a user's teammate fainted last round. (Retaliate) *
*************************************************************************/
public class Move085 : BattleMove {
	public Move085(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (attacker.OwnSide().effects[Effects.LastRoundFainted] >= 0 && attacker.OwnSide().effects[Effects.LastRoundFainted] == battle.turnCount-1) {
			return baseDamage*2;
		}
		return baseDamage;
	}
}

/***************************************************************
*  Power is doubled if the user has no held item. (Acrobatics) *
***************************************************************/
public class Move086 : BattleMove {
	public Move086(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamageMultiplier(int damageMult, Battler attacker, Battler opponent) {
		if (attacker.item == 0) {
			return baseDamage*2;
		}
		return baseDamage;
	}
}

/************************************************************************************
*  Power is doubled in weather. Type changes dep}ing on the weather. (Weather Ball) *
************************************************************************************/
public class Move087 : BattleMove {
	public Move087(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (battle.GetWeather() != 0) {
			return baseDamage*2;
		}
		return baseDamage;
	}

	public new int ModifyType(int type, Battler attacker, Battler opponent) {
		int t = Types.NORMAL;
		switch (battle.GetWeather()) 
		{
			case Weather.SUNNYDAY:
			case Weather.HARSHSUN:
				t = Types.FIRE;
				break;
			case Weather.RAINDANCE:
			case Weather.HEAVYRAIN:
				t = Types.WATER;
				break;
			case Weather.SANDSTORM:
				t = Types.ROCK;
				break;
			case Weather.HAIL:
				t = Types.ICE;
				break;
		}
		return t;
	}
}

/***********************************************************************************
*  Power is doubled if a foe tries to switch out or use U-turn/Volt Switch/        *
*  Parting Shot. (Pursuit)                                                         *
*  (Handled in Battle's pbAttackPhase): Makes this attack happen before switching. *
***********************************************************************************/
public class Move088 : BattleMove {
	public Move088(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (battle.switching) {
			return baseDamage*2;
		}
		return baseDamage;
	}

	public new bool AccuracyCheck(Battler attacker, Battler opponent) {
		if (battle.switching) {
			return true;
		}
		return base.AccuracyCheck(attacker, opponent);
	}
}

/************************************************
*  Power increases with the user's happiness. ( *
************************************************/
public class Move089 : BattleMove {
	public Move089(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		return (int)Math.Max(attacker.happiness*2.0/5, 1);
	}
}

/************************************************************
*  Power decreases with the user's happiness. (Frustration) *
************************************************************/
public class Move08A : BattleMove {
	public Move08A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		return (int)Math.Max((255.0-attacker.happiness)*2.0/5, 1);
	}
}

/***************************************************************
*  Power increases with the user's HP. (Eruption, Water Spout) *
***************************************************************/
public class Move08B : BattleMove {
	public Move08B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		return (int)Math.Max(150.0*attacker.hp/attacker.totalHP, 1);
	}
}

/*****************************************************************
*  Power increases with the target's HP. (Crush Grip, Wring Out) *
*****************************************************************/
public class Move08C : BattleMove {
	public Move08C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		return (int)Math.Max(120.0*attacker.hp/attacker.totalHP, 1);
	}
}

/************************************************************************
*  Power increases the quicker the target is than the user. (Gyro Ball) *
************************************************************************/
public class Move08D : BattleMove {
	public Move08D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		return (int)Math.Max(Math.Min(25.0*opponent.speed/attacker.speed, 150), 1);
	}
}

/**********************************************************************************
*  Power increases with the user's positive stat changes (ignores negative ones). *
*  (Stored Power)                                                                 *
**********************************************************************************/
public class Move08E : BattleMove {
	public Move08E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		int mult = 1;
		int[] stats = new int[7]{Stats.ATTACK, Stats.DEFENSE, Stats.SPEED, Stats.SPATK, Stats.SPDEF, Stats.ACCURACY, Stats.EVASION};
		for (int i=0; i<stats.Length; i++) 
		{
			if (attacker.stages[i] > 0) {
				mult += attacker.stages[i];
			}
		}
		return 20*mult;
	}
}

/************************************************************************************
*  Power increases with the target's positive stat changes (ignores negative ones). *
*  (Punishment)                                                                     *
************************************************************************************/
public class Move08F : BattleMove {
	public Move08F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		int mult = 3;
		int[] stats = new int[7]{Stats.ATTACK, Stats.DEFENSE, Stats.SPEED, Stats.SPATK, Stats.SPDEF, Stats.ACCURACY, Stats.EVASION};
		for (int i=0; i<stats.Length; i++) 
		{
			if (opponent.stages[i] > 0) {
				mult += opponent.stages[i];
			}
		}
		return (int)Math.Min(20*mult, 200);
	}
}

/**********************************************************
*  Power and type dep}s on the user's IVs. (Hidden Power) *
**********************************************************/
public class Move090 : BattleMove {
	public Move090(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int ModifyType(int type, Battler attacker, Battler opponent) {
		return HiddenPower(attacker.iv)[0];
	}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (Settings.USE_NEW_BATTLE_MECHANICS) {
			return 60;
		}
		return HiddenPower(attacker.iv)[1];
	}
}

/*********************************************************
*  Power doubles for each consecutive use. (Fury Cutter) *
*********************************************************/
public class Move091 : BattleMove {
	public Move091(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		return baseDamage << (attacker.effects[Effects.FuryCutter]-1);
	}
}

/**********************************************************************************
*  Power is multiplied by the number of consecutive rounds in which this move was *
*  used by any PokÃ©mon on the user's side. (Echoed Voice)                      *
**********************************************************************************/
public class Move092 : BattleMove {
	public Move092(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		return  baseDamage * attacker.OwnSide().effects[Effects.EchoedVoiceCounter];
	}
}

/***********************************************************************************
*  User rages until the start of a round in which they don't use this move. (Rage) *
*  (Handled in Battler's pbProcessMoveAgainstTarget): Ups rager's Attack by 1      *
*  stage each time it loses HP due to a move.                                      *
***********************************************************************************/
public class Move093 : BattleMove {
	public Move093(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (ret > 0) {
			attacker.effects[Effects.Rage] = 1;
		}
		return ret;
	}
}

/***************************************************
*  Randomly damages or heals the target. (Present) *
***************************************************/
public class Move094 : BattleMove {
	bool forceDamage;
	int calcBaseDamage;
	public Move094(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool OnStartUse(Battler attacker) {
		forceDamage = false;
		return true;
	}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		forceDamage = true;
		return calcBaseDamage;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		calcBaseDamage = 1;
		int r = battle.Rand(forceDamage ? 8 : 10);
		if (r < 4) {
			calcBaseDamage = 40;
		} else if (r < 7) {
			calcBaseDamage = 80;
		} else if (r < 8) {
			calcBaseDamage = 120;
		} else {
			if (TypeModifier(GetType(type, attacker, opponent), attacker, opponent) == 0) {
				battle.Display(string.Format("{0} It doesn't affect {1}...", opponent.String(true)));
				return -1;
			}
			if (opponent.hp == opponent.totalHP) {
				battle.Display(string.Format("But it failed!"));
				return -1;
			}
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			opponent.RecoverHP(opponent.totalHP/4, true);
			battle.Display(string.Format("{1} had its HP restored.", opponent.String()));
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/***************************************************************************************
*  Power is chosen at random. Power is doubled if the target is using Dig. (Magnitude) *
*  (Handled in Battler's pbSuccessCheck): Hits some semi-invulnerable targets.         *
***************************************************************************************/
public class Move095 : BattleMove {
	int calcBaseDamage;
	public Move095(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool OnStartUse(Battler attacker) {
		int[] baseDamage = new int[7]{10, 30, 50, 70, 90, 110, 150};
		int[] magnitude = new int[20]{4,5,5,6,6,6,6,7,7,7,7,7,7,8,8,8,8,9,9,10};
		int magni = magnitude[battle.Rand(magnitude.Length)];
		calcBaseDamage = baseDamage[magni-4];
		battle.Display(string.Format("Magnitude {0}!", magni));
		return false;
	}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		int ret = calcBaseDamage;
		if ((new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() == 0xCA) {
			ret *= 2;
		}
		if (battle.field.effects[Effects.GrassyTerrain] > 0) {
			ret = (int)Math.Round(ret/2.0);
		}
		return ret;
	}
}

/************************************************************************************
*  Power and type dep} on the user's held berry. Destroys the berry. (Natural Gift) *
************************************************************************************/
public class Move096 : BattleMove {
	int berry;
	public Move096(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool OnStartUse(Battler attacker) {
		if (!Items.IsBerry(attacker.item) || attacker.effects[Effects.Embargo] > 0 || battle.field.effects[Effects.MagicRoom] > 0 || attacker.HasWorkingAbility(Abilities.KLUTZ) || attacker.Opposing1().HasWorkingAbility(Abilities.UNNERVE) || attacker.HasWorkingAbility(Abilities.UNNERVE)) {
			battle.Display(string.Format("But it failed!"));
			return false;
		}
		berry = attacker.item;
		return true;
	}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		int ret = 1;
		switch (berry) {
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
			case Items.ROSELIBERRY:
					ret = 60;
					if (Settings.USE_NEW_BATTLE_MECHANICS) {
						ret += 20;
					}
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
					ret = 70;
					if (Settings.USE_NEW_BATTLE_MECHANICS) {
						ret += 20;
					}
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
			case Items.KEEBERRY:
			case Items.MARANGABERRY:
					ret = 80;
					if (Settings.USE_NEW_BATTLE_MECHANICS) {
						ret += 20;
					}
					break;
		}
		return ret;
	}

	public new int ModifyType(int type, Battler attacker, Battler opponent) {
		int t = Types.NORMAL;
		switch (berry) 
		{
			case Items.CHILANBERRY:
				t = Types.NORMAL;
				break;
			case Items.CHERIBERRY:
			case Items.BLUKBERRY:
			case Items.WATMELBERRY:
			case Items.OCCABERRY:
				t = Types.FIRE;
				break;
			case Items.CHESTOBERRY:
			case Items.NANABBERRY:
			case Items.DURINBERRY:
			case Items.PASSHOBERRY:
				t = Types.WATER;
				break;
			case Items.PECHABERRY:
			case Items.WEPEARBERRY:
			case Items.BELUEBERRY:
			case Items.WACANBERRY:
				t = Types.ELECTRIC;
				break;
			case Items.RAWSTBERRY:
			case Items.PINAPBERRY:
			case Items.RINDOBERRY:
			case Items.LIECHIBERRY:
				t = Types.GRASS;
				break;
			case Items.ASPEARBERRY:
			case Items.POMEGBERRY:
			case Items.YACHEBERRY:
			case Items.GANLONBERRY:
				t = Types.ICE;
				break;
			case Items.LEPPABERRY:
			case Items.KELPSYBERRY:
			case Items.CHOPLEBERRY:
			case Items.SALACBERRY:
				t = Types.FIGHTING;
				break;
			case Items.ORANBERRY:
			case Items.QUALOTBERRY:
			case Items.KEBIABERRY:
			case Items.PETAYABERRY:
				t = Types.POISON;
				break;
			case Items.PERSIMBERRY:
			case Items.HONDEWBERRY:
			case Items.SHUCABERRY:
			case Items.APICOTBERRY:
				t = Types.GROUND;
				break;
			case Items.LUMBERRY:
			case Items.GREPABERRY:
			case Items.COBABERRY:
			case Items.LANSATBERRY:
				t = Types.FLYING;
				break;
			case Items.SITRUSBERRY:
			case Items.TAMATOBERRY:
			case Items.PAYAPABERRY:
			case Items.STARFBERRY:
				t = Types.PSYCHIC;
				break;
			case Items.FIGYBERRY:
			case Items.CORNNBERRY:
			case Items.TANGABERRY:
			case Items.ENIGMABERRY:
				t = Types.BUG;
				break;
			case Items.WIKIBERRY:
			case Items.MAGOSTBERRY:
			case Items.CHARTIBERRY:
			case Items.MICLEBERRY:
				t = Types.ROCK;
				break;
			case Items.MAGOBERRY:
			case Items.RABUTABERRY:
			case Items.KASIBBERRY:
			case Items.CUSTAPBERRY:
				t = Types.GHOST;
				break;
			case Items.AGUAVBERRY:
			case Items.NOMELBERRY:
			case Items.HABANBERRY:
			case Items.JABOCABERRY:
				t = Types.DRAGON;
				break;
			case Items.IAPAPABERRY:
			case Items.SPELONBERRY:
			case Items.COLBURBERRY:
			case Items.ROWAPBERRY:
			case Items.MARANGABERRY:
				t = Types.DARK;
				break;
			case Items.RAZZBERRY:
			case Items.PAMTREBERRY:
			case Items.BABIRIBERRY:
				t = Types.STEEL;
				break;
			case Items.ROSELIBERRY:
			case Items.KEEBERRY:
				t = Types.FAIRY;
				break;
		}
		return t;
	}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (turneffects[Effects.TotalDamage] > 0) {
			attacker.ConsumeItem();
		}
	}
}

/***********************************************************
*  Power increases the less PP this move has. (Trump Card) *
***********************************************************/
public class Move097 : BattleMove {
	public Move097(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		int ppleft = Math.Min(pp, 4);
		switch (ppleft) {
			case 0:
				baseDamage = 200;
				break;
			case 1:
				baseDamage = 80;
				break;
			case 2:
				baseDamage = 60;
				break;
			case 3:
				baseDamage = 50;
				break;
			case 4:
				baseDamage = 40;
				break;
		}
		return baseDamage;
	}
}

/***************************************************************
*  Power increases the less HP the user has. (Flail, Reversal) *
***************************************************************/
public class Move098 : BattleMove {
	public Move098(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		int n = (int)(48.0*attacker.hp/attacker.totalHP);
		int ret = 20;
		if (n < 33) {
			ret = 40;
		}
		if (n < 17) {
			ret = 80;
		}
		if (n < 10) {
			ret = 100;
		}
		if (n < 5) {
			ret = 150;
		}
		if (n < 2) {
			ret = 200;
		}
		return ret;
	}
}

/***************************************************************************
*  Power increases the quicker the user is than the target. (Electro Ball) *
***************************************************************************/
public class Move099 : BattleMove {
	public Move099(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		int n = (int)(Math.Max(attacker.speed, 1)/Math.Max(opponent.speed, 1));
		int ret = 20;
		if (n >= 100) {
			ret = 40;
		}
		if (n >= 250) {
			ret = 60;
		}
		if (n >= 500) {
			ret = 80;
		}
		if (n >= 1000) {
			ret = 100;
		}
		if (n >= 2000) {
			ret = 120;
		}
		return ret;
	}
}

/*********************************************************************
*  Power increases the heavier the target is. (Grass Knot, Low Kick) *
*********************************************************************/
public class Move09A : BattleMove {
	public Move09A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		int n = opponent.weight(attacker);
		int ret = 20;
		if (n >= 100) {
			ret = 40;
		}
		if (n >= 250) {
			ret = 60;
		}
		if (n >= 500) {
			ret = 80;
		}
		if (n >= 1000) {
			ret = 100;
		}
		if (n >= 2000) {
			ret = 120;
		}
		return ret;
	}
}

/*************************************************************************************
*  Power increases the heavier the user is than the target. (Heat Crash, Heavy Slam) *
*************************************************************************************/
public class Move09B : BattleMove {
	public Move09B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		int n = (int)((double)attacker.weight()/opponent.weight(attacker));
		int ret = 40;
		if (n >= 2) {
			ret = 60;
		}
		if (n >= 3) {
			ret = 80;
		}
		if (n >= 4) {
			ret = 100;
		}
		if (n >= 5) {
			ret = 120;
		}
		return ret;
	}
}

/*****************************************************************
*  Powers up the ally's attack this round by 1.5. (Helping Hand) *
*****************************************************************/
public class Move09C : BattleMove {
	public Move09C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!battle.doublebattle || opponent.Fainted() || battle.useMoveChoice[opponent.index] != 1 || opponent.HasMovedThisRound() || opponent.effects[Effects.HelpingHand] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.HelpingHand] = 1;
		battle.Display(string.Format("{0} is ready to help {1}!",attacker.String(), opponent.String(true)));
		return 0;
	}
}

/*****************************************
*  Weakens Electric attacks. (Mud Sport) *
*****************************************/
public class Move09D : BattleMove {
	public Move09D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (Settings.USE_NEW_BATTLE_MECHANICS) {
			if (battle.field.effects[Effects.MudSportField] > 0) {
				battle.Display(string.Format("But it failed!"));
				return -1;
			}
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			battle.field.effects[Effects.MudSportField] = 5;
			battle.Display(string.Format("Electricity's power was weakened!"));
			return 0;
		} else {
			for (int i=0; i<4; i++) 
			{
				if (attacker.battle.battlers[i].effects[Effects.MudSport] != 0) {
					battle.Display(string.Format("But it failed!"));
					return -1;
				}
			}
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			attacker.effects[Effects.MudSport] = 1;
			battle.Display(string.Format("Electricity's power was weakened!"));
			return 0;
		}
	}
}

/***************************************
*  Weakens Fire attacks. (Water Sport) *
***************************************/
public class Move09E : BattleMove {
	public Move09E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (Settings.USE_NEW_BATTLE_MECHANICS) {
			if (battle.field.effects[Effects.WaterSportField] > 0) {
				battle.Display(string.Format("But it failed!"));
				return -1;
			}
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			battle.field.effects[Effects.WaterSportField] = 5;
			battle.Display(string.Format("Fire's power was weakened!"));
			return 0;
		} else {
			for (int i=0; i<4; i++) 
			{
				if (attacker.battle.battlers[i].effects[Effects.WaterSport] != 0) {
					battle.Display(string.Format("But it failed!"));
					return -1;
				}
			}
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			attacker.effects[Effects.WaterSport] = 1;
			battle.Display(string.Format("Fire's power was weakened!"));
			return 0;
		}
	}
}

/****************************************************************
*  Type dep}s on the user's held item. (Judgment, Techno Blast) *
****************************************************************/
public class Move09F : BattleMove {
	public Move09F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int ModifyType(int type, Battler attacker, Battler opponent) {
		if (id == Moves.JUDGMENT) {
			switch(attacker.item) {
				case Items.FISTPLATE:
					return Types.FIGHTING;
				case Items.SKYPLATE:
					return Types.FLYING;
				case Items.TOXICPLATE:
					return Types.POISON;
				case Items.EARTHPLATE:
					return Types.GROUND;
				case Items.STONEPLATE:
					return Types.ROCK;
				case Items.INSECTPLATE:
					return Types.BUG;
				case Items.SPOOKYPLATE:
					return Types.GHOST;
				case Items.IRONPLATE:
					return Types.STEEL;
				case Items.FLAMEPLATE:
					return Types.FIRE;
				case Items.SPLASHPLATE:
					return Types.WATER;
				case Items.MEADOWPLATE:
					return Types.GRASS;
				case Items.ZAPPLATE:
					return Types.ELECTRIC;
				case Items.MINDPLATE:
					return Types.PSYCHIC;
				case Items.ICICLEPLATE:
					return Types.ICE;
				case Items.DRACOPLATE:
					return Types.DRAGON;
				case Items.DREADPLATE:
					return Types.DARK;
				case Items.PIXIEPLATE:
					return Types.FAIRY;
			}
		} else if (id == Moves.TECHNOBLAST) {
			switch(attacker.item) {
				case Items.SHOCKDRIVE:
					return Types.ELECTRIC;
				case Items.BURNDRIVE:
					return Types.FIRE;
				case Items.CHILLDRIVE:
					return Types.ICE;
				case Items.DOUSEDRIVE:
					return Types.WATER;
			}
		}
		return Types.NORMAL;
	}

	public new void ShowAnimation(int id, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (id == Moves.TECHNOBLAST) {
			int anim = 0;
			switch (GetType(type, attacker, opponent)) {
				case Types.ELECTRIC:
					anim = 1;
					break;
				case Types.FIRE:
					anim = 2;
					break;
				case Types.ICE:
					anim = 3;
					break;
				case Types.WATER:
					anim = 4;
					break;
			}
			base.ShowAnimation(id, attacker, opponent, anim, allTargets, showAnimation);
		}
		base.ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/*********************************************************************
*  This attack is always a critical hit. (Frost Breath, Storm Throw) *
*********************************************************************/
public class Move0A0 : BattleMove {
	public Move0A0(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool CriticalOverride(Battler attacker, Battler opponent) {
		return true;
	}
}

/**************************************************************************
*  For 5 rounds, foes' attacks cannot become critical hits. (Lucky Chant) *
**************************************************************************/
public class Move0A1 : BattleMove {
	public Move0A1(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OwnSide().effects[Effects.LuckyChant] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.OwnSide().effects[Effects.LuckyChant] = 5;
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display("The Lucky Chant shielded your team from critical hits!");
		} else {
			battle.Display("The Lucky Chant shielded the opposing team from critical hits!");
		}
		return 0;
	}
}

/*************************************************************************************
*  For 5 rounds, lowers power of physical attacks against the user's side. (Reflect) *
*************************************************************************************/
public class Move0A2 : BattleMove {
	public Move0A2(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OwnSide().effects[Effects.Reflect] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.OwnSide().effects[Effects.Reflect] = 5;
		if (attacker.HasWorkingItem(Items.LIGHTCLAY)) {
			attacker.OwnSide().effects[Effects.Reflect] = 8;
		}
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display("Reflect raised your team's Defense");
		} else {
			battle.Display("Reflect raised the opposing team's Defense");
		}
		return 0;
	}
}

/*****************************************************************************************
*  For 5 rounds, lowers power of special attacks against the user's side. (Light Screen) *
*****************************************************************************************/
public class Move0A3 : BattleMove {
	public Move0A3(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OwnSide().effects[Effects.LightScreen] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.OwnSide().effects[Effects.LightScreen] = 5;
		if (attacker.HasWorkingItem(Items.LIGHTCLAY)) {
			attacker.OwnSide().effects[Effects.LightScreen] = 8;
		}
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display("Light Screen raised your team's Special Defense");
		} else {
			battle.Display("Light Screen raised the opposing team's Special Defense");
		}
		return 0;
	}
}

/***************************************************
*  Effect dep}s on the environment. (Secret Power) *
***************************************************/
public class Move0A4 : BattleMove {
	public Move0A4(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (battle.field.effects[Effects.ElectricTerrain] > 0) {
			if (opponent.CanParalyze(attacker, false, this)) {
				opponent.Paralyze(attacker);
			}
			return;
		} else if (battle.field.effects[Effects.GrassyTerrain] > 0) {
			if (opponent.CanSleep(attacker, false, this)) {
				opponent.Sleep();
			}
			return;
		} else if (battle.field.effects[Effects.MistyTerrain] > 0) {
			if (opponent.CanReduceStatStage(Stats.SPATK, attacker, false, this)) {
				opponent.ReduceStat(Stats.SPATK, 1, attacker, false, this);
			}
			return;
		}
		switch (battle.environment) 
		{
			case Environment.Grass:
			case Environment.TallGrass:
			case Environment.Forest:
				if (opponent.CanSleep(attacker, false, this)) {
					opponent.Sleep();
				}
				break;
			case Environment.MovingWater:
			case Environment.Underwater:
				if (opponent.CanReduceStatStage(Stats.ATTACK, attacker, false, this)) {
					opponent.ReduceStat(Stats.ATTACK, 1, attacker, false, this);
				}
				break;
			case Environment.StillWater:
			case Environment.Sky:
				if (opponent.CanReduceStatStage(Stats.SPEED, attacker, false, this)) {
					opponent.ReduceStat(Stats.SPEED, 1, attacker, false, this);
				}
				break;
			case Environment.Sand:
				if (opponent.CanReduceStatStage(Stats.ACCURACY, attacker, false, this)) {
					opponent.ReduceStat(Stats.ACCURACY, 1, attacker, false, this);
				}
				break;
			case Environment.Rock:
				if (Settings.USE_NEW_BATTLE_MECHANICS) {
					if (opponent.CanReduceStatStage(Stats.ACCURACY, attacker, false, this)) {
						opponent.ReduceStat(Stats.ACCURACY, 1, attacker, false, this);
					}
				} else {
					if (opponent.effects[Effects.Substitute] == 0 || IgnoresSubstitute(attacker)) {
						opponent.Flinch(attacker);
					}
				}
				break;
			case Environment.Cave:
			case Environment.Graveyard:
			case Environment.Space:
				if (opponent.effects[Effects.Substitute] == 0 || IgnoresSubstitute(attacker)) {
					opponent.Flinch(attacker);
				}
				break;
			case Environment.Snow:
				if (opponent.CanFreeze(attacker, false, this)) {
					opponent.Freeze();
				}
				break;

			case Environment.Volcano:
				if (opponent.CanBurn(attacker, false, this)) {
					opponent.Burn(attacker);
				}
				break;
			default:
				if (opponent.CanParalyze(attacker, false, this)) {
					opponent.Paralyze(attacker);
				}
				break;
		}
	}

	public new void ShowAnimation(int id, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int i = Moves.BODYSLAM;
		if (battle.field.effects[Effects.ElectricTerrain] > 0) {
			i = Moves.THUNDERSHOCK;
		} else if (battle.field.effects[Effects.GrassyTerrain] > 0) {
			i = Moves.VINEWHIP;
		} else if (battle.field.effects[Effects.MistyTerrain] > 0) {
			i = Moves.FAIRYWIND;
		} else {
			switch (battle.environment) 
			{
				case Environment.Grass:
				case Environment.TallGrass:
					i = Settings.USE_NEW_BATTLE_MECHANICS ? Moves.VINEWHIP : Moves.NEEDLEARM;
					break;
				case Environment.MovingWater:
					i = Moves.WATERPULSE;
					break;
				case Environment.StillWater:
					i = Moves.MUDSHOT;
					break;
				case Environment.Underwater:
					i = Moves.WATERPULSE;
					break;
				case Environment.Cave:
					i = Moves.ROCKTHROW;
					break;
				case Environment.Rock:
					i = Moves.MUDSLAP;
					break;
				case Environment.Sand:
					i = Moves.MUDSLAP;
					break;
				case Environment.Forest:
					i = Moves.RAZORLEAF;
					break;
				case Environment.Snow:
					i = Moves.AVALANCHE;
					break;
				case Environment.Volcano:
					i = Moves.INCINERATE;
					break;
				case Environment.Graveyard:
					i = Moves.SHADOWSNEAK;
					break;
				case Environment.Sky:
					i = Moves.GUST;
					break;
				case Environment.Space:
					i = Moves.SWIFT;
					break;
			}
		}
		base.ShowAnimation(i, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/****************
*  Always hits. *
****************/
public class Move0A5 : BattleMove {
	public Move0A5(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool AccuracyCheck(Battler attacker, Battler opponent) {
		return true;
	}
}

/*******************************************************************************************
*  User's attack next round against the target will definitely hit. (Lock-On, Mind Reader) *
*******************************************************************************************/
public class Move0A6 : BattleMove {
	public Move0A6(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.LockOn] = 2;
		opponent.effects[Effects.LockOnPos] = attacker.index;
		battle.Display(string.Format("{0} took aim at {1}!",attacker.String(), opponent.String(true)));
		return 0;
	}
}

/**************************************************************************************
*  Target's evasion stat changes are ignored from now on. (Foresight, Odor Sleuth)    *
*  Normal and Fighting moves have normal effectiveness against the Ghost-type target. *
**************************************************************************************/
public class Move0A7 : BattleMove {
	public Move0A7(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.OwnSide().effects[Effects.CraftyShield] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.Foresight] = 1;
		battle.Display(string.Format("{0} was identified!",opponent.String()));
		return 0;
	}
}

/*************************************************************************
*  Target's evasion stat changes are ignored from now on. (Miracle Eye)  *
*  Psychic moves have normal effectiveness against the Dark-type target. *
*************************************************************************/
public class Move0A8 : BattleMove {
	public Move0A8(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.OwnSide().effects[Effects.CraftyShield] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.Foresight] = 1;
		battle.Display(string.Format("{0} was identified!",opponent.String()));
		return 0;
	}
}

/*********************************************************************************
*  This move ignores target's Defense, Special Defense and evasion stat changes. *
*  (Chip Away, Sacred Sword)                                                     *
*********************************************************************************/
public class Move0A9 : BattleMove {
	public Move0A9(Battle battle, Moves.Move move) : base(battle, move) {}
// Handled in superclass public new bool AccuracyCheck and public bool CalcDamage, do not edit!
}

/***********************************************************************************
*  User is protected against moves with the "B" flag this round. (Detect, Protect) *
***********************************************************************************/
public class Move0AA : BattleMove {
	public Move0AA(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int[] ratesharers = new int[6]{0xAA,0xAB,0xAC,0xE8,0x14B,0x14C};
		if (Array.IndexOf(ratesharers, (new Moves.Move(attacker.lastMoveUsed)).Function()) > -1) {
			attacker.effects[Effects.ProtectRate] = 1;
		}
		bool unmoved = false;
		for (int i=0; i<battle.battlers.Count; i++) 
		{
			Battler poke = battle.battlers[i];
			if (battle.useMoveChoice[poke.index] == 1 && !poke.HasMovedThisRound()) {
				unmoved = true;
				break;
			}
		}
		if (!unmoved || battle.Rand(65536) >= (65536/attacker.effects[Effects.ProtectRate])) {
			attacker.effects[Effects.ProtectRate] = 1;
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.Protect] = 1;
		attacker.effects[Effects.ProtectRate] *= 2;
		battle.Display(string.Format("{0} protected itself!",attacker.String()));
		return 0;
	}
}

/***********************************************************************************
*  User's side is protected against moves with priority greater than 0 this round. *
*  (Quick Guard)                                                                   *
***********************************************************************************/
public class Move0AB : BattleMove {
	public Move0AB(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OwnSide().effects[Effects.QuickGuard] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		int[] ratesharers = new int[6]{0xAA,0xAB,0xAC,0xE8,0x14B,0x14C};
		if (Array.IndexOf(ratesharers, (new Moves.Move(attacker.lastMoveUsed)).Function()) > -1) {
			attacker.effects[Effects.ProtectRate] = 1;
		}
		bool unmoved = false;
		for (int i=0; i<battle.battlers.Count; i++) 
		{
			Battler poke = battle.battlers[i];
			if (battle.useMoveChoice[poke.index] == 1 && !poke.HasMovedThisRound()) {
				unmoved = true;
				break;
			}
		}
		if (!unmoved || battle.Rand(65536) >= (65536/attacker.effects[Effects.ProtectRate])) {
			attacker.effects[Effects.ProtectRate] = 1;
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.OwnSide().effects[Effects.QuickGuard] = 1;
		attacker.effects[Effects.ProtectRate] *= 2;
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display(string.Format("Quick Guard protected your team!"));
		} else {
			battle.Display(string.Format("Quick Guard protected the opposing team!"));
		}
		return 0;
	}
}

/************************************************************************************
*  User's side is protected against moves that target multiple battlers this round. *
*  (Wide Guard)                                                                     *
************************************************************************************/
public class Move0AC : BattleMove {
	public Move0AC(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OwnSide().effects[Effects.WideGuard] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		int[] ratesharers = new int[6]{0xAA,0xAB,0xAC,0xE8,0x14B,0x14C};
		if (Array.IndexOf(ratesharers, (new Moves.Move(attacker.lastMoveUsed)).Function()) > -1) {
			attacker.effects[Effects.ProtectRate] = 1;
		}
		bool unmoved = false;
		for (int i=0; i<battle.battlers.Count; i++) 
		{
			Battler poke = battle.battlers[i];
			if (battle.useMoveChoice[poke.index] == 1 && !poke.HasMovedThisRound()) {
				unmoved = true;
				break;
			}
		}
		if (!unmoved || battle.Rand(65536) >= (65536/attacker.effects[Effects.ProtectRate])) {
			attacker.effects[Effects.ProtectRate] = 1;
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.OwnSide().effects[Effects.WideGuard] = 1;
		attacker.effects[Effects.ProtectRate] *= 2;
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display(string.Format("Wide Guard protected your team!"));
		} else {
			battle.Display(string.Format("Wide Guard protected the opposing team!"));
		}
		return 0;
	}
}

/***************************************************************************
*  Ignores target's protections. If successful, all other moves this round *
*  ignore them too. (Feint)                                                *
***************************************************************************/
public class Move0AD : BattleMove {
	public Move0AD(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (ret > 0) {
			opponent.effects[Effects.ProtectNegation] = 1;
			opponent.OwnSide().effects[Effects.CraftyShield] = 0;
		}
		return ret;
	}
}

/**********************************************************
*  Uses the last move that the target used. (Mirror Move) *
**********************************************************/
public class Move0AE : BattleMove {
	public Move0AE(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.lastMoveUsed <= 0 || ((new Moves.Move(opponent.lastMoveUsed)).flags&0x10) == 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		attacker.UseMoveSimple(opponent.lastMoveUsed, -1, opponent.index);
		return 0;
	}
}

/***********************************************
*  Uses the last move that was used. (Copycat) *
***********************************************/
public class Move0AF : BattleMove {
	public Move0AF(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int[] blacklist = new int[20]{0x02,0x69,0x71,0x72,0x73,0x9C,0xAA,0xAD,0xAE,0xAF,0xB2,0xE7,0xE8,0xEC,0xF1,0xF2,0xF3,0x115,0x117,0x158};
		if (Settings.USE_NEW_BATTLE_MECHANICS) {
			blacklist = new int [14]{0xC3,0xC4,0xC5,0xC6,0xC7,0xC8,0xC9,0xCA,0xCB,0xCC,0xCD,0xCE,0x14D,0x14E};
		}
		if (battle.lastMoveUsed <= 0 || Array.IndexOf(blacklist, (new Moves.Move(battle.lastMoveUsed)).Function()) > -1) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		attacker.UseMoveSimple(battle.lastMoveUsed);
		return 0;
	}
}

/*************************************************************************************
*  Uses the move the target was about to use this round, with 1.5x power. (Me First) *
*************************************************************************************/
public class Move0B0 : BattleMove {
	public Move0B0(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int[] blacklist = new int[9]{0x02,0x14,0x71,0x72,0x73,0xB0,0xF1,0x115,0x158};
		BattleMove oppmove = battle.moveChoice[opponent.index];
		if (battle.useMoveChoice[opponent.index] != 1 || opponent.HasMovedThisRound() || oppmove == null || oppmove.id <= 0 || oppmove.IsStatus() || Array.IndexOf(blacklist, oppmove.function) > -1) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		attacker.effects[Effects.MeFirst] = 1;
		attacker.UseMoveSimple(oppmove.id);
		attacker.effects[Effects.MeFirst] = 0;
		return 0;
	}
}

/*******************************************************************************
*  This round, reflects all moves with the "C" flag targeting the user back at *
*  their origin. (Magic Coat)                                                  *
*******************************************************************************/
public class Move0B1 : BattleMove {
	public Move0B1(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.MagicCoat] = 1;
		battle.Display(string.Format("{0} shrouded itself with Magic Coat!",attacker.String()));
		return 0;
	}
}

/*******************************************************************
*  This round, snatches all used moves with the "D" flag. (Snatch) *
*******************************************************************/
public class Move0B2 : BattleMove {
	public Move0B2(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.Snatch] = 1;
		battle.Display(string.Format("{0} waits for a target to make a move!",attacker.String()));
		return 0;
	}
}

/********************************************************************
*  Uses a different move dep}ing on the environment. (Nature Power) *
********************************************************************/
public class Move0B3 : BattleMove {
	public Move0B3(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int m = Moves.TRIATTACK;
		switch (battle.environment) 
		{
			case Environment.Grass:
			case Environment.TallGrass:
			case Environment.Forest:
				m = Settings.USE_NEW_BATTLE_MECHANICS ? Moves.ENERGYBALL : Moves.SEEDBOMB;
				break;
			case Environment.MovingWater:
				m = Moves.HYDROPUMP;
				break;
			case Environment.StillWater:
				m = Moves.MUDBOMB;
				break;
			case Environment.Underwater:
				m = Moves.HYDROPUMP;
				break;
			case Environment.Cave:
				m = Settings.USE_NEW_BATTLE_MECHANICS ? Moves.POWERGEM : Moves.ROCKSLIDE;
				break;
			case Environment.Rock:
				m = Settings.USE_NEW_BATTLE_MECHANICS ? Moves.EARTHPOWER : Moves.ROCKSLIDE;
				break;
			case Environment.Sand:
				m = Settings.USE_NEW_BATTLE_MECHANICS ? Moves.EARTHPOWER : Moves.EARTHQUAKE;
				break;
			case Environment.Snow:
				m = Settings.USE_NEW_BATTLE_MECHANICS ? Moves.FROSTBREATH : Moves.ICEBEAM;
				break;
			case Environment.Volcano:
				m = Moves.LAVAPLUME;
				break;
			case Environment.Graveyard:
				m = Moves.SHADOWBALL;
				break;
			case Environment.Sky:
				m = Moves.AIRSLASH;
				break;
			case Environment.Space:
				m = Moves.DRACOMETEOR;
				break;
		}
		if (battle.field.effects[Effects.ElectricTerrain] > 0) {
			m = Moves.THUNDERBOLT;
		} else if (battle.field.effects[Effects.GrassyTerrain] > 0) {
			m = Moves.ENERGYBALL;
		} else if (battle.field.effects[Effects.MistyTerrain] > 0) {
			m = Moves.MOONBLAST;
		}
		if (m == 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		string thisMoveName = Moves.GetName(id);
		string moveName = Moves.GetName(m);
		battle.Display(string.Format("{0} turned into {1}!",thisMoveName, moveName));
		int t = (Settings.USE_NEW_BATTLE_MECHANICS && opponent != null) ? opponent.index : -1;
		attacker.UseMoveSimple(m, -1, t);
		return 0;
	}
}

/********************************************************************************
*  Uses a random move the user knows. Fails if user is not asleep. (Sleep Talk) *
********************************************************************************/
public class Move0B4 : BattleMove {
	public Move0B4(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool CanUseWhileAsleep() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.status != Statuses.SLEEP) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		int[] blacklist = new int[28]{0x02,0x14,0x5C,0x5D,0xAE,0xAF,0xB0,0xB3,0xB4,0xB5,0xB6,0xD1,0xD4,0x115,0xC3,0xC4,0xC5,0xC6,0xC7,0xC8,0xC9,0xCA,0xCB,0xCC,0xCD,0xCE,0x14D,0x14E};
		List<int> choices = new List<int>();
		for (int i=0; i<4; i++) 
		{
			bool found = false;
			if (attacker.moves[i].id == 0) {
				continue;
			}
			if (Array.IndexOf(blacklist, attacker.moves[i].function) > -1) {
				found = true;
			}
			if (found) {
				continue;
			}
			if (battle.CanChooseMove(attacker.index, i, false, true)) {
				choices.Add(i);
			}
		}
		if (choices.Count == 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int choice = choices[battle.Rand(choices.Count)];
		attacker.UseMoveSimple(attacker.moves[choice].id, -1, attacker.OppositeOpposing().index);
		return 0;
	}
}

/*************************************************************************************
*  Uses a random move known by any non-user PokÃ©mon in the user's party. (Assist) *
*************************************************************************************/
public class Move0B5 : BattleMove {
	public Move0B5(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int[] blacklist = new int[34]{0x02,0x14,0x5C,0x5D,0x69,0x71,0x72,0x73,0x9C,0xAA,0xAD,0xAE,0xAF,0xB0,0xB2,0xB3,0xB4,0xB5,0xB6,0xCD,0xE7,0xE8,0xEB,0xEC,0xF1,0xF2,0xF3,0x115,0x117,0x149,0x14B,0x14C,0x14D,0x158};
		if (Settings.USE_NEW_BATTLE_MECHANICS) {
			blacklist = new int[14]{0xC3,0xC4,0xC5,0xC6,0xC7,0xC8,0xC9,0xCA,0xCB,0xCC,0xCD,0xCE,0x14D,0x14E};
		}
		List<int> moves = new List<int>();
		Battler[] party = battle.Party(attacker.index);
		for (int i=0; i<party.Length; i++) 
		{
			if (1 != attacker.pokemonIndex && party[i] != null && !(Settings.USE_NEW_BATTLE_MECHANICS && party[i].pokemon.Egg())) {
				for (int j=0; j < party[i].moves.Length; j++) 
				{
					if (party[i].moves[j].id == 0) {
						continue;
					}
					if (Array.IndexOf(blacklist, (new Moves.Move(party[i].moves[j].id)).Function()) > -1) {
						moves.Add(party[i].moves[j].id);
					}
				}
			}
		}
		if (moves.Count == 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int m = moves[battle.Rand(moves.Count)];
		attacker.UseMoveSimple(m);
		return 0;
	}
}

/***********************************************
*  Uses a random move that exists. (Metronome) *
***********************************************/
public class Move0B6 : BattleMove {
	public Move0B6(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int[] blacklist = new int[31]{0x02,0x11,0x14,0x5C,0x5D,0x69,0x71,0x72,0x73,0x9C,0xAA,0xAB,0xAC,0xAD,0xAE,0xAF,0xB0,0xB2,0xB3,0xB4,0xB5,0xB6,0xE7,0xE8,0xF1,0xF2,0xF3,0x115,0x117,0x11D,0x11E};
		int[] blacklistMoves = new int[8]{Moves.FREEZESHOCK,Moves.ICEBURN,Moves.RELICSONG,Moves.SECRETSWORD,Moves.SNARL,Moves.TECHNOBLAST,Moves.VCREATE,Moves.GEOMANCY};
		for (int i=0; i<1000; i++) 
		{
			int m = battle.Rand(Moves.MaxValue()+1);
			bool found = false;
			if (Array.IndexOf(blacklist, (new Moves.Move(m)).Function()) > -1) {
				found = true;
			} else {
				for (int j=0; j<blacklistMoves.Length; j++) 
				{
					if (m == blacklistMoves[j]) {
						found = true;
						break;
					}
				}
			}
			if (!found) {
				ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
				attacker.UseMoveSimple(m);
				return 0;
			}
		}
		battle.Display(string.Format("But it failed!"));
		return -1;
	}
}

/************************************************************************
*  The target can no longer use the same move twice in a row. (Torment) *
************************************************************************/
public class Move0B7 : BattleMove {
	public Move0B7(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Torment] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.AROMAVEIL)) {
				battle.Display(string.Format("But it failed because of {0}'s {1}!",opponent.String(), Abilities.GetName(opponent.ability)));
				return -1;
			} else if (opponent.Partner().HasWorkingAbility(Abilities.AROMAVEIL)) {
				battle.Display(string.Format("But it failed because of {0}'s {1}!",opponent.Partner().String(), Abilities.GetName(opponent.ability)));
				return -1;
			}
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.Torment] = 1;
		battle.Display(string.Format("{0} was subjected to torment!",opponent.String()));
		return 0;
	}
}

/********************************************************************
*  Disables all target's moves that the user also knows. (Imprison) *
********************************************************************/
public class Move0B8 : BattleMove {
	public Move0B8(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.Imprison] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.Imprison] = 1;
		battle.Display(string.Format("{0} sealed the opponent's move(s)!",attacker.String()));
		return 0;
	}
}

/*******************************************************************
*  For 5 rounds, disables the last move the target used. (Disable) *
*******************************************************************/
public class Move0B9 : BattleMove {
	public Move0B9(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Disable] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.AROMAVEIL)) {
				battle.Display(string.Format("But it failed because of {0}'s {1}!",opponent.String(), Abilities.GetName(opponent.ability)));
				return -1;
			} else if (opponent.Partner().HasWorkingAbility(Abilities.AROMAVEIL)) {
				battle.Display(string.Format("But it failed because of {0}'s {1}!",opponent.Partner().String(), Abilities.GetName(opponent.ability)));
				return -1;
			}
		}
		for (int i=0; i<opponent.moves.Length; i++) 
		{
			if (opponent.moves[i].id > 0 && opponent.moves[i].id == opponent.lastMoveUsed && (opponent.moves[i].pp > 0 || opponent.moves[i].totalPP == 0)) {
				ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
				opponent.effects[Effects.Disable] = 5;
				opponent.effects[Effects.DisableMove] = opponent.lastMoveUsed;
				battle.Display(string.Format("{0}'s {1} was disabled!",opponent.String(), opponent.moves[i].name));
				return 0;
			}
		}
		battle.Display(string.Format("But it failed!"));
		return -1;
	}
}

/*******************************************************************
*  For 4 rounds, disables the target's non-damaging moves. (Taunt) *
*******************************************************************/
public class Move0BA : BattleMove {
	public Move0BA(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Taunt] > 0 || (Settings.USE_NEW_BATTLE_MECHANICS && !attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.OBLIVIOUS))) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.AROMAVEIL)) {
				battle.Display(string.Format("But it failed because of {0}'s {1}!",opponent.String(), Abilities.GetName(opponent.ability)));
				return -1;
			} else if (opponent.Partner().HasWorkingAbility(Abilities.AROMAVEIL)) {
				battle.Display(string.Format("But it failed because of {0}'s {1}!",opponent.Partner().String(), Abilities.GetName(opponent.ability)));
				return -1;
			}
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.Taunt] = 4;
		battle.Display(string.Format("{0} fell for the taunt!",opponent.String()));
		return 0;
	}
}

/*******************************************************************
*  For 5 rounds, disables the target's healing moves. (Heal Block) *
*******************************************************************/
public class Move0BB : BattleMove {
	public Move0BB(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.HealBlock] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.AROMAVEIL)) {
				battle.Display(string.Format("But it failed because of {0}'s {1}!",opponent.String(), Abilities.GetName(opponent.ability)));
				return -1;
			} else if (opponent.Partner().HasWorkingAbility(Abilities.AROMAVEIL)) {
				battle.Display(string.Format("But it failed because of {0}'s {1}!",opponent.Partner().String(), Abilities.GetName(opponent.ability)));
				return -1;
			}
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.HealBlock] = 5;
		battle.Display(string.Format("{0} was prevented from healing!",opponent.String()));
		return 0;
	}
}

/************************************************************************
*  For 4 rounds, the target must use the same move each round. (Encore) *
************************************************************************/
public class Move0BC : BattleMove {
	public Move0BC(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int[] blacklist = new int[6]{0x02,0x5C,0x5D,0x69,0xAE,0xBC};
		if (opponent.effects[Effects.Encore] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (opponent.lastMoveUsed <= 0 || Array.IndexOf(blacklist, (new Moves.Move(opponent.lastMoveUsed)).Function()) > -1) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.AROMAVEIL)) {
				battle.Display(string.Format("But it failed because of {0}'s {1}!",opponent.String(), Abilities.GetName(opponent.ability)));
				return -1;
			} else if (opponent.Partner().HasWorkingAbility(Abilities.AROMAVEIL)) {
				battle.Display(string.Format("But it failed because of {0}'s {1}!",opponent.Partner().String(), Abilities.GetName(opponent.ability)));
				return -1;
			}
		}
		for (int i=0; i<4; i++) 
		{
			if (opponent.lastMoveUsed == opponent.moves[i].id && (opponent.moves[i].pp > 0 || opponent.moves[i].totalPP == 0)) {
				ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
				opponent.effects[Effects.Encore] = 4;
				opponent.effects[Effects.EncoreIndex] = i;
				opponent.effects[Effects.EncoreMove] = opponent.moves[i].id;
				battle.Display(string.Format("{0} received an encore!",opponent.String()));
				return 0;
			}
		}
		battle.Display(string.Format("But it failed!"));
		return -1;
	}
}

/***************
*  Hits twice. *
***************/
public class Move0BD : BattleMove {
	public Move0BD(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsMultiHit() {
		return true;
	}

	public new int NumHits(Battler attacker) {
		return 2;
	}
}

/**************************************************************
*  Hits twice. May poison the target on each hit. (Twineedle) *
**************************************************************/
public class Move0BE : BattleMove {
	public Move0BE(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsMultiHit() {
		return true;
	}

	public new int NumHits(Battler attacker) {
		return 2;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanPoison(attacker, false, this)) {
			opponent.Poison(attacker);
		}
	}
}

/**********************************************************************
*  Hits 3 times. Power is multiplied by the hit number. (Triple Kick) *
*  An accuracy check is performed for each hit.                       *
**********************************************************************/
public class Move0BF : BattleMove {
	bool checks;
	int calcBaseDamage;
	public Move0BF(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsMultiHit() {
		return true;
	}

	public new int NumHits(Battler attacker) {
		return 3;
	}

	public new bool SuccessCheckPerHit() {
		return checks;
	}

	public new bool OnStartUse(Battler attacker) {
		calcBaseDamage = baseDamage;
		checks = !attacker.HasWorkingAbility(Abilities.SKILLLINK);
		return true;
	}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		int ret = calcBaseDamage;
		calcBaseDamage += baseDamage;
		return ret;
	}
}

/*******************
*  Hits 2-5 times. *
*******************/
public class Move0C0 : BattleMove {
	public Move0C0(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsMultiHit() {
		return true;
	}

	public new int NumHits(Battler attacker) {
		int[] hitchances = new int[6]{2,2,3,3,4,5};
		int ret = hitchances[battle.Rand(hitchances.Length)];
		if (attacker.HasWorkingAbility(Abilities.SKILLLINK)) {
			ret = 5;
		}
		return ret;
	}
}

/***********************************************************************************
*  Hits X times, where X is 1 (the user) plus the number of non-user unfainted     *
*  status-free PokÃ©mon in the user's party (the participants). Fails if X is 0. *
*  base power of each hit dep}s on the base Attack stat for the species of that    *
*  hit's participant. (Beat Up)                                                    *
***********************************************************************************/
public class Move0C1 : BattleMove {
	List<int> participants;
	public Move0C1(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsMultiHit() {
		return true;
	}

	public new int NumHits(Battler attacker) {
		return participants.Count;
	}

	public new bool OnStartUse(Battler attacker) {
		Battler[] party = battle.Party(attacker.index);
		participants = new List<int>();
		for (int i=0; i<party.Length; i++) 
		{
			if (attacker.pokemonIndex == i) {
				participants.Add(i);
			} else if (party[i] != null && !party[i].pokemon.Egg() && party[i].hp > 0 && party[i].status == 0) {
				participants.Add(i);
			}
		}
		if (participants.Count == 0) {
			battle.Display(string.Format("But it failed!"));
			return false;
		}
		return true;
	}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		Battler[] party = battle.Party(attacker.index);
		int atk = party[participants[0]].pokemon.BaseStats()[Stats.ATTACK];
		participants.Remove(0);
		return 5 + atk/10;
	}
}

/***************************************************************************
*  Two turn attack. Attacks first turn, skips second turn (if successful). *
***************************************************************************/
public class Move0C2 : BattleMove {
	public Move0C2(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			attacker.effects[Effects.HyperBeam] = 2;
			attacker.currentMove = id;
		}
		return ret;
	}
}

/************************************************************************
*  Two turn attack. Skips first turn, attacks second turn. (Razor Wind) *
************************************************************************/
public class Move0C3 : BattleMove {
	bool immediate;
	public Move0C3(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} whipped up a whirlwind!",attacker.String()));
		}
		if (immediate) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/***********************************************************************************
*  Two turn attack. Skips first turn, attacks second turn. (SolarBeam)             *
*  Power halved in all weather except sunshine. In sunshine, takes 1 turn instead. *
***********************************************************************************/
public class Move0C4 : BattleMove {
	bool immediate;
	bool sunny;
	public Move0C4(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		sunny = true;
		if (attacker.effects[Effects.TwoTurnAttack] == 0) {
			if (battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN) {
				immediate = true;
				sunny = true;
			}
		}
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int BaseDamageMultiplier(int damageMult, Battler attacker, Battler opponent) {
		if (battle.GetWeather() != 0 && battle.GetWeather() != Weather.SUNNYDAY && battle.GetWeather() != Weather.HARSHSUN) {
			return (int)Math.Round(damageMult*0.5);
		}
		return damageMult;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} took in sunlight!",attacker.String()));
		}
		if (immediate && !sunny) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/*************************************************************************
* Two turn attack. Skips first turn, attacks second turn. (Freeze Shock) *
* May paralyze the target.                                               *
*************************************************************************/
public class Move0C5 : BattleMove {
	bool immediate;
	public Move0C5(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} became cloaked in a freezing light!",attacker.String()));
		}
		if (immediate) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}
	
	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanParalyze(attacker, false, this)) {
			opponent.Paralyze(attacker);
		}
	}
}

/**********************************************************************
*  Two turn attack. Skips first turn, attacks second turn. (Ice Burn) *
*  May burn the target.                                               *
**********************************************************************/
public class Move0C6 : BattleMove {
	bool immediate;
	public Move0C6(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} became cloaked in freezing air!",attacker.String()));
		}
		if (immediate) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}
	
	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanBurn(attacker, false, this)) {
			opponent.Burn(attacker);
		}
	}
}

/************************************************************************
*  Two turn attack. Skips first turn, attacks second turn. (Sky Attack) *
*  May make the target flinch.                                          *
************************************************************************/
public class Move0C7 : BattleMove {
	bool immediate;
	public Move0C7(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} became cloaked in a harsh light!",attacker.String()));
		}
		if (immediate) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		opponent.Flinch(attacker);
	}
}

/***********************************************************************************
*  Two turn attack. Ups user's Defense by 1 stage first turn, attacks second turn. *
*  (Skull Bash)                                                                    *
***********************************************************************************/
public class Move0C8 : BattleMove {
	bool immediate;
	public Move0C8(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} tucked in its head!",attacker.String()));
			if (attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
				attacker.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this);
			}
		}
		if (immediate) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/***************************************************************************
*  Two turn attack. Skips first turn, attacks second turn. (Fly)           *
*  (Handled in Battler's pbSuccessCheck): Is semi-invulnerable during use. *
***************************************************************************/
public class Move0C9 : BattleMove {
	bool immediate;
	public Move0C9(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool UnusableInGravity() {
		return true;
	}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} flew up high!",attacker.String()));
		}
		if (immediate) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/***************************************************************************
*  Two turn attack. Skips first turn, attacks second turn. (Dig)           *
*  (Handled in Battler's pbSuccessCheck): Is semi-invulnerable during use. *
***************************************************************************/
public class Move0CA : BattleMove {
	bool immediate;
	public Move0CA(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} burrowed its way under the ground!",attacker.String()));
		}
		if (immediate) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/***************************************************************************
*  Two turn attack. Skips first turn, attacks second turn. (Dive)          *
*  (Handled in Battler's pbSuccessCheck): Is semi-invulnerable during use. *
***************************************************************************/
public class Move0CB : BattleMove {
	bool immediate;
	public Move0CB(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} hid underwater!",attacker.String()));
		}
		if (immediate) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/***************************************************************************
*  Two turn attack. Skips first turn, attacks second turn. (Bounce)        *
*  May paralyze the target.                                                *
*  (Handled in Battler's pbSuccessCheck): Is semi-invulnerable during use. *
***************************************************************************/
public class Move0CC : BattleMove {
	bool immediate;
	public Move0CC(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool UnusableInGravity() {
		return true;
	}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} sprang up!",attacker.String()));
		}
		if (immediate) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanParalyze(attacker, false, this)) {
			opponent.Paralyze(attacker);
		}
	}
}

/*******************************************************************************
*  Two turn attack. Skips first turn, attacks second turn. (Shadow Force)      *
*  Is invulnerable during use.                                                 *
*  Ignores target's Detect, King's Shield, Mat Block, Protect and Spiky Shield *
*  this round. If successful, negates them this round.                         *
*******************************************************************************/
public class Move0CD : BattleMove {
	bool immediate;
	public Move0CD(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} vanished immediately!",attacker.String()));
		}
		if (immediate) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			return 0;
		}
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (ret > 0) {
			opponent.effects[Effects.ProtectNegation] = 1;
			opponent.OwnSide().effects[Effects.CraftyShield] = 0;
		}
		return ret;
	}
}

/************************************************************************************
*  Two turn attack. Skips first turn, attacks second turn. (Sky Drop)               *
*  (Handled in Battler's pbSuccessCheck):	Is semi-invulnerable during use.          *
*  Target is also semi-invulnerable during use, and can't take any action.          *
*  Doesn't damage airborne PokÃ©mon (but still makes them unable to move during). *
************************************************************************************/
public class Move0CE : BattleMove {
	public Move0CE(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool UnusableInGravity() {
		return true;
	}

	public new bool MoveFailed(Battler attacker, Battler opponent) {
		bool ret = false;
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			ret = true;
		}
		if (opponent.effects[Effects.TwoTurnAttack] > 0) {
			ret = true;
		}
		if (opponent.effects[Effects.SkyDrop] != 0 && attacker.effects[Effects.TwoTurnAttack] > 0) {
			ret = true;
		}
		if (!opponent.IsOpposing(attacker.index)) {
			ret = true;
		}
		if (Settings.USE_NEW_BATTLE_MECHANICS && opponent.weight(attacker) >= 2000) {
			ret = true;
		}
		return ret;
	}

	public new bool TwoTurnAttack(Battler attacker) {
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} took {1} into the sky!",attacker.String(), opponent.String(true)));
			opponent.effects[Effects.SkyDrop] = 1;
		}
		if (attacker.effects[Effects.TwoTurnAttack] > 0) {
			return 0;
		}
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		battle.Display(string.Format("{0} was freed from the Sky Drop!",opponent.String()));
		opponent.effects[Effects.SkyDrop] = 0;
		return ret;
	}

	public new int TypeModifier(int type, Battler attacker, Battler opponent) {
		if (opponent.HasType(Types.FLYING)) {
			return 0;
		}
		if (!attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.LEVITATE) && opponent.effects[Effects.SmackDown] == 0) {
			return 0;
		}
		return base.TypeModifier(type, attacker, opponent);
	}
}

/**********************************************************************************
*  Trapping move. Traps for 5 or 6 rounds. Trapped PokÃ©mon lose 1/16 of max HP *
*  at } of each round.                                                            *
**********************************************************************************/
public class Move0CF : BattleMove {
	public Move0CF(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0 && !opponent.Fainted() && !opponent.damageState.Substitute) {
			if (opponent.effects[Effects.MultiTurn] == 0) {
				opponent.effects[Effects.MultiTurn] = 5 + battle.Rand(2);
				if (attacker.HasWorkingItem(Items.GRIPCLAW)) {
					opponent.effects[Effects.MultiTurn] = Settings.USE_NEW_BATTLE_MECHANICS ? 8 : 6;
				}
				opponent.effects[Effects.MultiTurnAttack] = id;
				opponent.effects[Effects.MultiTurnUser] = attacker.index;
				if (id == Moves.BIND) {
					battle.Display(string.Format("{0} was squeezed by {1}!",opponent.String(), attacker.String(true)));
				} else if (id == Moves.CLAMP) {
					battle.Display(string.Format("{0} clamped {1}!",opponent.String(), attacker.String(true)));
				} else if (id == Moves.FIRESPIN) {
					battle.Display(string.Format("{0} was trapped in the fiery vortex!",opponent.String()));
				} else if (id == Moves.MAGMASTORM) {
					battle.Display(string.Format("{0} was trapped by Magma Storm!",opponent.String()));
				} else if (id == Moves.SANDTOMB) {
					battle.Display(string.Format("{0} was trapped Sand Tomb!",opponent.String()));
				} else if (id == Moves.WRAP) {
					battle.Display(string.Format("{0} was wrapped by {1}!",opponent.String(), attacker.String(true)));
				} else if (id == Moves.INFESTATION) {
					battle.Display(string.Format("{0} has been afflicted with an infestation by {1}!",opponent.String(), attacker.String(true)));
				} else {
					battle.Display(string.Format("{0} was trapped in the vortex!",opponent.String()));
				}
			}
		}
		return ret;
	}
}

/**********************************************************************************
*  Trapping move. Traps for 5 or 6 rounds. Trapped PokÃ©mon lose 1/16 of max HP *
*  at } of each round. (Whirlpool)                                                *
*  Power is doubled if target is using Dive.                                      *
*  (Handled in Battler's pbSuccessCheck): Hits some semi-invulnerable targets.    *
**********************************************************************************/
public class Move0D0 : BattleMove {
	public Move0D0(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0 && !opponent.Fainted() && !opponent.damageState.Substitute) {
			if (opponent.effects[Effects.MultiTurn] == 0) {
				opponent.effects[Effects.MultiTurn] = 5 + battle.Rand(2);
				if (attacker.HasWorkingItem(Items.GRIPCLAW)) {
					opponent.effects[Effects.MultiTurn] = Settings.USE_NEW_BATTLE_MECHANICS ? 8 : 6;
				}
				opponent.effects[Effects.MultiTurnAttack] = id;
				opponent.effects[Effects.MultiTurnUser] = attacker.index;
				battle.Display(string.Format("{0} became trapped in the vortex!",opponent.String()));
			}
		}
		return ret;
	}

	public new int ModifyDamage(int damageMult, Battler attacker, Battler opponent) {
		if ((new Moves.Move(opponent.effects[Effects.TwoTurnAttack])).Function() == 0x2B) {
			return (int)Math.Round(damageMult*2.0);
		}
		return damageMult;
	}
}

/******************************************************************************
*  User must use this move for 2 more rounds. No battlers can sleep. (Uproar) *
******************************************************************************/
public class Move0D1 : BattleMove {
	public Move0D1(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			if (attacker.effects[Effects.Uproar] == 0) {
				attacker.effects[Effects.Uproar] = 3;
				battle.Display(string.Format("{0} caused an uproar!",attacker.String()));
				attacker.currentMove = id;
			}
		}
		return ret;
	}
}

/********************************************************************************
*  User must use this move for 1 or 2 more rounds. At }, user becomes confused. *
*  (Outrage, Petal Dange, Thrash)                                               *
********************************************************************************/
public class Move0D2 : BattleMove {
	public Move0D2(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0 && attacker.effects[Effects.Outrage] == 0 && attacker.status != Statuses.SLEEP) {
			attacker.effects[Effects.Outrage] = 2 + battle.Rand(2);
			attacker.currentMove = id;
		} else if (TypeModifier(type, attacker, opponent) == 0) {
			attacker.effects[Effects.Outrage] = 0;
		}
		if (attacker.effects[Effects.Outrage] > 0) {
			attacker.effects[Effects.Outrage]--;
			if (attacker.effects[Effects.Outrage] == 0 && attacker.CanConfuseSelf(false)) {
				attacker.Confuse();
				battle.Display(string.Format("{0} became confused due to fatigue!",attacker.String()));
			}
		}
		return ret;
	}
}

/************************************************************************
*  User must use this move for 4 more rounds. Power doubles each round. *
*  Power is also doubled if user has curled up. (Ice Ball, Rollout)     *
************************************************************************/
public class Move0D3 : BattleMove {
	public Move0D3(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		int shift = 4 - attacker.effects[Effects.Rollout];
		if (attacker.effects[Effects.DefenseCurl] != 0) {
			shift++;
		}
		baseDamage = baseDamage << shift;
		return baseDamage;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.Rollout] == 0) {
			attacker.effects[Effects.Rollout] = 5;
		}
		attacker.effects[Effects.Rollout]--;
		attacker.currentMove = id;
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage == 0 || TypeModifier(type, attacker, opponent) == 0 || attacker.status == Statuses.SLEEP) {
			attacker.effects[Effects.Rollout] = 0;
		}
		return ret;
	}
}

/*********************************************************************************
*  User bides its time this round and next round. The round after, deals 2x the  *
*  total damage it took while biding to the last battler that damaged it. (Bide) *
*********************************************************************************/
public class Move0D4 : BattleMove {
	public Move0D4(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int DisplayUseMessage(Battler attacker) {
		if (attacker.effects[Effects.Bide] == 0) {
			battle.DisplayBrief(string.Format("{0} used {1}!", attacker.String(), name));
			attacker.effects[Effects.Bide] = 2;
			attacker.effects[Effects.BideDamage] = 0;
			attacker.effects[Effects.BideTarget] = -1;
			attacker.currentMove = id;
			ShowAnimation(id, attacker, null);
			return 1;
		} else {
			attacker.effects[Effects.Bide]--;
			if (attacker.effects[Effects.Bide] == 0) {
				battle.DisplayBrief(string.Format("{0} unleashed energy!",attacker.String()));
				return 0;
			} else {
				battle.DisplayBrief(string.Format("{0} is storing energy!",attacker.String()));
				return 2;
			}
		}
	}

	public new void AddTarget(List<Battler> targets, Battler attacker) {
		if (attacker.effects[Effects.BideTarget] >= 0) {
			if (!attacker.AddTarget(targets, battle.battlers[attacker.effects[Effects.BideTarget]])) {
				attacker.RandTarget(targets);
			}
		} else {
			attacker.RandTarget(targets);
		}
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.BideDamage] == 0 || opponent != null) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (Settings.USE_NEW_BATTLE_MECHANICS) {
			int typeMod = TypeModifier(GetType(type, attacker, opponent), attacker, opponent);
			if (typeMod == 0) {
				battle.Display(string.Format("It doesn't affect {0}...",opponent.String(true)));
				return -1;
			}
		}
		return EffectFixedDamage(attacker.effects[Effects.BideDamage]*2, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/************************************
*  Heals user by 1/2 of its max HP. *
************************************/
public class Move0D5 : BattleMove {
	public Move0D5(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.hp == attacker.totalHP) {
			battle.Display(string.Format("{0}'s HP is full!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.RecoverHP((attacker.totalHP+1)/2, true);
		battle.Display(string.Format("{0}'s HP was restored.",attacker.String()));
		return 0;
	}
}

/****************************************************************************
*  Heals user by 1/2 of its max HP. (Roost)                                 *
*  User roosts, and its Flying type is ignored for attacks used against it. *
****************************************************************************/
public class Move0D6 : BattleMove {
	public Move0D6(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.hp == attacker.totalHP) {
			battle.Display(string.Format("{0}'s HP is full!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.RecoverHP((attacker.totalHP+1)/2, true);
		attacker.effects[Effects.Roost] = 1;
		battle.Display(string.Format("{0}'s HP was restored.",attacker.String()));
		return 0;
	}
}

/******************************************************************************
*  Battler in user's position is healed by 1/2 of its max HP, at the } of the *
*  next round. (Wish)                                                         *
******************************************************************************/
public class Move0D7 : BattleMove {
	public Move0D7(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.Wish]>0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.Wish] = 2;
		attacker.effects[Effects.WishAmount] = (attacker.totalHP+1)/2;
		attacker.effects[Effects.WishMaker] = attacker.pokemonIndex;
		return 0;
	}
}

/****************************************************************************
*  Heals user by an amount dep}ing on the weather. (Moonlight, Morning Sun, *
*  Synthesis)                                                               *
****************************************************************************/
public class Move0D8 : BattleMove {
	public Move0D8(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.hp == attacker.totalHP) {
			battle.Display(string.Format("{0}'s HP is full!",attacker.String()));
			return -1;
		}
		int hpGain = 0;
		if (battle.GetWeather() == Weather.SUNNYDAY || battle.GetWeather() == Weather.HARSHSUN) {
			hpGain = (int)Math.Floor(attacker.totalHP*2.0/3);
		} else if (battle.GetWeather() != 0) {
			hpGain = (int)Math.Floor(attacker.totalHP/4.0);
		} else {
			hpGain = (int)Math.Floor(attacker.totalHP/2.0);
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.RecoverHP(hpGain, true);
		battle.Display(string.Format("{0}'s HP was restored.",attacker.String()));
		return 0;
	}
}

/**********************************************************************
*  Heals user to full HP. User falls asleep for 2 more rounds. (Rest) *
**********************************************************************/
public class Move0D9 : BattleMove {
	public Move0D9(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.CanSleep(attacker, true, this, true)) {
			return -1;
		}
		if (attacker.status == Statuses.SLEEP) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (attacker.hp == attacker.totalHP) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.SleepSelf(3);
		battle.Display(string.Format("{0} slept and became healthy!",attacker.String()));
		int hp = attacker.RecoverHP(attacker.totalHP-attacker.hp, true);
		if (hp > 0) {
			battle.Display(string.Format("{0}'s HP was restored.",attacker.String()));
		}
		return 0;
	}
}

/*********************************************************************************
*  Rings the user. Ringed PokÃ©mon gain 1/16 of max HP at the } of each round. *
*  (Aqua Ring)                                                                   *
*********************************************************************************/
public class Move0DA : BattleMove {
	public Move0DA(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.AquaRing] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.AquaRing] = 1;
		battle.Display(string.Format("{0} surrounded itself with a veil of water!",attacker.String()));
		return 0;
	}
}

/********************************************************************************
*  Ingrains the user. Ingrained PokÃ©mon gain 1/16 of max HP at the } of each *
*  round, and cannot flee or switch out. (Ingrain)                              *
********************************************************************************/
public class Move0DB : BattleMove {
	public Move0DB(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.Ingrain] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.Ingrain] = 1;
		battle.Display(string.Format("{0} planted its roots!",attacker.String()));
		return 0;
	}
}

/**********************************************************************************
*  Seeds the target. Seeded PokÃ©mon lose 1/8 of max HP at the } of each round, *
*  and the PokÃ©mon in the user's position gains the same amount. (Leech Seed)  *
**********************************************************************************/
public class Move0DC : BattleMove {
	public Move0DC(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (TypeImmunityByAbility(GetType(type, attacker, opponent), attacker, opponent)) {
			return -1;
		}
		if (opponent.effects[Effects.LeechSeed] >= 0) {
			battle.Display(string.Format("{0} evaded the attack!",opponent.String()));
			return -1;
		}
		if (opponent.HasType(Types.GRASS)) {
			battle.Display(string.Format("It doesn't affect {0}...",opponent.String(true)));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.LeechSeed] = attacker.index;
		battle.Display(string.Format("{0} was seeded!",opponent.String()));
		return 0;
	}
}

/*************************************************
*  User gains half the HP it inflicts as damage. *
*************************************************/
public class Move0DD : BattleMove {
	public Move0DD(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return Settings.USE_NEW_BATTLE_MECHANICS;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			int hpGain = (int)Math.Round(opponent.damageState.HPLost/2.0);
			if (opponent.HasWorkingAbility(Abilities.LIQUIDOOZE)) {
				attacker.ReduceHP(hpGain, true);
				battle.Display(string.Format("{0} sucked up the liquid ooze!",attacker.String()));
			} else if (attacker.effects[Effects.HealBlock] == 0) {
				if (attacker.HasWorkingItem(Items.BIGROOT)) {
					hpGain = (int)(hpGain*1.3);
				}
				attacker.RecoverHP(hpGain, true);
				battle.Display(string.Format("{0} had its energy drained!",opponent.String()));
			}
		}
		return ret;
	}
}

/*************************************************************************
*  User gains half the HP it inflicts as damage. (Dream Eater)           *
*  (Handled in Battler's pbSuccessCheck): Fails if target is not asleep. *
*************************************************************************/
public class Move0DE : BattleMove {
	public Move0DE(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			int hpGain = (int)Math.Round(opponent.damageState.HPLost/2.0);
			if (opponent.HasWorkingAbility(Abilities.LIQUIDOOZE)) {
				attacker.ReduceHP(hpGain, true);
				battle.Display(string.Format("{0} sucked up the liquid ooze!",attacker.String()));
			} else if (attacker.effects[Effects.HealBlock] == 0) {
				if (attacker.HasWorkingItem(Items.BIGROOT)) {
					hpGain = (int)(hpGain*1.3);
				}
				attacker.RecoverHP(hpGain, true);
				battle.Display(string.Format("{0} had its energy drained!",opponent.String()));
			}
		}
		return ret;
	}
}

/***************************************************
*  Heals target by 1/2 of its max HP. (Heal Pulse) *
***************************************************/
public class Move0DF : BattleMove {
	public Move0DF(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (opponent.hp == opponent.totalHP) {
			battle.Display(string.Format("{0}'s HP is full!",opponent.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int hpGain = (int)((opponent.totalHP+1)/2.0);
		if (attacker.HasWorkingAbility(Abilities.MEGALAUNCHER)) {
			hpGain = (int)Math.Round(opponent.totalHP*3/4.0);
		}
		opponent.RecoverHP(hpGain, true);
		battle.Display(string.Format("{0}'s HP was restored.",opponent.String()));
		return 0;
	}
}

/******************************************
*  User faints. (Explosion, Selfdestruct) *
******************************************/
public class Move0E0 : BattleMove {
	public Move0E0(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool OnStartUse(Battler attacker) {
		if (!attacker.HasMoldBreaker()) {
			Battler bearer = battle.CheckGlobalAbility(Abilities.DAMP);
			if (bearer != null) {
				battle.Display(string.Format("{0}'s {1} prevents {2} from using {3}!",bearer.String(), Abilities.GetName(bearer.ability), attacker.String(true), name));
				return false;
			}
		}
		return true;
	}

	public new void ShowAnimation(int id, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		base.ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		if (!attacker.Fainted()) {
			attacker.ReduceHP(attacker.hp);
			if (attacker.Fainted()) {
				attacker.Faint();
			}
		}
	}
}

/********************************************************************
*  Inflicts fixed damage equal to user's current HP. (Final Gambit) *
*  User faints (if successful).                                     *
********************************************************************/
public class Move0E1 : BattleMove {
	public Move0E1(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int typeMod = TypeModifier(GetType(type, attacker, opponent), attacker, opponent);
		if (typeMod == 0) {
			battle.Display(string.Format("It doesn't affect {0}...",opponent.String(true)));
			return -1;
		}
		int ret = EffectFixedDamage(attacker.hp, attacker, opponent, hitNum, allTargets, showAnimation);
		return ret;
	}

	public new void ShowAnimation(int id, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		base.ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		if (!attacker.Fainted()) {
			attacker.ReduceHP(attacker.hp);
			if (attacker.Fainted()) {
				attacker.Faint();
			}
		}
	}
}

/********************************************************************************
*  Decreases the target's Attack and Special Attack by 2 stages each. (Memento) *
*  User faints (even if effect does nothing).                                   *
********************************************************************************/
public class Move0E2 : BattleMove {
	public Move0E2(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute]>0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int ret = -1;
		bool showAnim = true;
		if (opponent.ReduceStat(Stats.ATTACK, 2, attacker, false, this, showAnim)) {
			ret = 0;
			showAnim = false;
		}
		if (opponent.ReduceStat(Stats.SPATK, 2, attacker, false, this, showAnim)) {
			ret = 0;
			showAnim = false;
		}
		attacker.ReduceHP(attacker.hp);
		return ret;
	}
}

/******************************************************************************
*  User faints. The PokÃ©mon that replaces the user is fully healed (HP and *
*  status). Fails if user won't be replaced. (Healing Wish)                   *
******************************************************************************/
public class Move0E3 : BattleMove {
	public Move0E3(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!battle.CanChooseNonActive(attacker.index)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.ReduceHP(attacker.hp);
		attacker.effects[Effects.HealingWish] = 1;
		return 0;
	}
}

/**********************************************************************************
*  User faints. The PokÃ©mon that replaces the user is fully healed (HP, PP and *
*  status). Fails if user won't be replaced. (Lunar Dance)                        *
**********************************************************************************/
public class Move0E4 : BattleMove {
	public Move0E4(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!battle.CanChooseNonActive(attacker.index)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.ReduceHP(attacker.hp);
		attacker.effects[Effects.LunarDance] = 1;
		return 0;
	}
}

/***********************************************************************
*  All current battlers will perish after 3 more rounds. (Perish Song) *
***********************************************************************/
public class Move0E5 : BattleMove {
	public Move0E5(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		bool failed = true;
		for (int i=0; i<4; i++) 
		{
			if (battle.battlers[i].effects[Effects.PerishSong]==0 && (attacker.HasMoldBreaker() || !battle.battlers[i].HasWorkingAbility(Abilities.SOUNDPROOF))) {
				failed = false;
				break;
			}
		}
		if (failed) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.Display(string.Format("All Pokémon that hear the song will faint in three turns!"));
		for (int i=0; i<4; i++) 
		{
			if (battle.battlers[i].effects[Effects.PerishSong]==0) {
				if (!attacker.HasMoldBreaker() && battle.battlers[i].HasWorkingAbility(Abilities.SOUNDPROOF)) {
					battle.Display(string.Format("{0}'s {1} blocks {2}!",battle.battlers[i].String(), Abilities.GetName(battle.battlers[i].ability), name));
				} else {
					battle.battlers[i].effects[Effects.PerishSong] = 4;
					battle.battlers[i].effects[Effects.PerishSongUser] = attacker.index;
				}
			}
		}
		return 0;
	}
}

/*********************************************************************************
*  If user is KO'd before it next moves, the attack that caused it loses all PP. *
*  (Grudge)                                                                      *
*********************************************************************************/
public class Move0E6 : BattleMove {
	public Move0E6(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.Grudge] = 1;
		battle.Display(string.Format("{0} wants to bear a grudge!",attacker.String()));
		return 0;
	}
}

/*********************************************************************************
*  If user is KO'd before it next moves, the battler that caused it also faints. *
*  (Destiny Bond)                                                                *
*********************************************************************************/
public class Move0E7 : BattleMove {
	public Move0E7(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.DestinyBond] = 1;
		battle.Display(string.Format("{0} is trying to take its foe down with it!",attacker.String()));
		return 0;
	}
}

/**************************************************************************
*  If user would be KO'd this round, it survives with 1HP instead. (Endure) *
**************************************************************************/
public class Move0E8 : BattleMove {
	public Move0E8(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int[] ratesharers = new int[6]{0xAA,0xAB,0xAC,0xE8,0x14B,0x14C};
		if (Array.IndexOf(ratesharers, (new Moves.Move(attacker.lastMoveUsed)).Function()) == -1) {
			attacker.effects[Effects.ProtectRate] = 1;
		}
		bool unmoved = false;
		for (int i=0; i<battle.battlers.Count; i++) 
		{
			Battler poke = battle.battlers[i];
			if (poke.index == attacker.index) {
				continue;
			}
			if (battle.useMoveChoice[poke.index] == 1 && !poke.HasMovedThisRound()) {
				unmoved = true;
				break;
			}
		}
		if (!unmoved || battle.Rand(65536) > (65536/attacker.effects[Effects.ProtectRate])) {
			attacker.effects[Effects.ProtectRate] = 1;
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.Endure] = 1;
		attacker.effects[Effects.ProtectRate] *= 2;
		battle.Display(string.Format("{0} braced itself!",attacker.String()));
		return 0;
	}
}

/***************************************************************************************
*  If target would be KO'd by this attack, it survives with 1HP instead. (False Swipe) *
***************************************************************************************/
public class Move0E9 : BattleMove {
	public Move0E9(Battle battle, Moves.Move move) : base(battle, move) {}

	// Handled in superclass public new void ReduceHPDamage, do not edit!
}

/****************************************************************
*  User flees from battle. Fails in trainer battles. (Teleport) *
****************************************************************/
public class Move0EA : BattleMove {
	public Move0EA(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if ((battle.opponent != null && battle.opponent.Length > 0) || !battle.CanRun(attacker.index)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		battle.Display(string.Format("{0} fled from battle!",attacker.String()));
		battle.decision = 3;
		return 0;
	}
}

/**********************************************************************************
*  In wild battles, makes target flee. Fails if target is a higher level than the *
*  user.                                                                          *
*  In trainer battles, target switches out.                                       *
*  For status moves. (Roar, Whirlwind)                                            *
**********************************************************************************/
public class Move0EB : BattleMove {
	public Move0EB(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.SUCTIONCUPS)) {
			battle.Display(string.Format("{0} anchored itself with {1}!",opponent.String(), Abilities.GetName(opponent.ability)));
			return -1;
		}
		if (opponent.effects[Effects.Ingrain] != 0) {
			battle.Display(string.Format("{0} anchored itself roots!",opponent.String()));
			return -1;
		}
		if (battle.opponent == null || battle.opponent.Length == 0) {
			if (opponent.level > attacker.level) {
				battle.Display(string.Format("But it failed!"));
				return -1;
			}
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			battle.decision = 3;
			return 0;
		} else {
			bool choices = false;
			Battler[] party = battle.Party(opponent.index);
			for (int i=0; i<party.Length; i++) 
			{
				if (battle.CanSwitch(opponent.index, i, false, true)) {
					choices = true;
					break;
				}
			}
			if (!choices) {
				battle.Display(string.Format("But it failed!"));
				return -1;
			}
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			opponent.effects[Effects.Roar] = 1;
			return 0;
		}
	}
}

/**********************************************************************************
*  In wild battles, makes target flee. Fails if target is a higher level than the *
*  user.                                                                          *
*  In trainer battles, target switches out.                                       *
*  For damaging moves. (Circle Throw, Dragon Tail)                                *
**********************************************************************************/
public class Move0EC : BattleMove {
	public Move0EC(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!attacker.Fainted() && !opponent.Fainted() && opponent.damageState.CalculatedDamage > 0 && !opponent.damageState.Substitute && (attacker.HasMoldBreaker() || !opponent.HasWorkingAbility(Abilities.SUCTIONCUPS)) && opponent.effects[Effects.Ingrain] != 0) {
			if (battle.opponent == null || battle.opponent.Length == 0) {
				if (opponent.level <= attacker.level) {
					battle.decision = 3;
				}
			} else {
				Battler[] party = battle.Party(opponent.index);
				for (int i=0; i<party.Length; i++) 
				{
					if (battle.CanSwitch(opponent.index, i, false, true)) {
						opponent.effects[Effects.Roar] = 1;
						break;
					}
				}
			}
		}
	}
}

/***************************************************************************
*  User switches out. Various effects affecting the user are passed to the *
*  replacement. (Baton Pass)                                               *
***************************************************************************/
public class Move0ED : BattleMove {
	public Move0ED(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!battle.CanChooseNonActive(attacker.index)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.BatonPass] = 1;
		return 0;
	}
}

/***********************************************************************
*  After inflicting damage, user switches out. Ignores trapping moves. *
*  (U-turn, Volt Switch)                                               *
*  TODO: Pursuit should interrupt this move.                           *
***********************************************************************/
public class Move0EE : BattleMove {
	public Move0EE(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (!attacker.Fainted() && opponent.damageState.CalculatedDamage > 0 && battle.CanChooseNonActive(attacker.index) && !battle.AllFainted(battle.Party(opponent.index))) {
			attacker.effects[Effects.Uturn] = 1;
		}
		return ret;
	}
}

/********************************************************************************
*  Target can no longer switch out or flee, as long as the user remains active. *
*  (Block, Mean Look, Spider Web, Thousand Waves)                               *
********************************************************************************/
public class Move0EF : BattleMove {
	public Move0EF(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
			if (opponent.damageState.CalculatedDamage > 0 && !opponent.damageState.Substitute && !opponent.Fainted()) {
				if (opponent.effects[Effects.MeanLook] < 0 && (!Settings.USE_NEW_BATTLE_MECHANICS || !opponent.HasType(Types.GHOST))) {
					opponent.effects[Effects.MeanLook] = attacker.index;
					battle.Display(string.Format("{0} can no longer escape!",opponent.String()));
				}
			}
			return ret;
		}
		if (opponent.effects[Effects.MeanLook] >= 0 || (opponent.effects[Effects.Substitute]>0 && !IgnoresSubstitute(attacker))) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (Settings.USE_NEW_BATTLE_MECHANICS && opponent.HasType(Types.GHOST)) {
			battle.Display(string.Format("It doesn't affect {0}...", opponent.String(true)));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.MeanLook] = attacker.index;
		battle.Display(string.Format("{0} can no longer escape!",opponent.String()));
		return 0;
	}
}

/**********************************************************************************
*  Target drops its item. It regains the item at the } of the battle. (Knock Off) *
*  If target has a losable item, damage is multiplied by 1.5.                     *
**********************************************************************************/
public class Move0F0 : BattleMove {
	public Move0F0(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!attacker.Fainted() && !opponent.Fainted() && opponent.item != 0 && opponent.damageState.CalculatedDamage > 0 && !opponent.damageState.Substitute) {
			if (!attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.STICKYHOLD)) {
				string abilityName = Abilities.GetName(opponent.ability);
				battle.Display(string.Format("{0}'s {1} made {2} ineffective!",opponent.String(), abilityName, name));
			} else if (!battle.IsUnlosableItem(opponent, opponent.item)) {
				string itemname = Items.GetName(opponent.item);
				opponent.item = 0;
				opponent.effects[Effects.ChoiceBand] = -1;
				opponent.effects[Effects.Unburden] = 1;
				battle.Display(string.Format("{0} dropped its {1}!",opponent.String(), itemname));
			}
		}
	}

	public new int ModifyDamage(int damageMult, Battler attacker, Battler opponent) {
		if (Settings.USE_NEW_BATTLE_MECHANICS && !battle.IsUnlosableItem(opponent, opponent.item)) {
			return (int)Math.Round(damageMult*1.5);
		}
		return damageMult;
	}
}

/******************************************************************************
*  User steals the target's item, if the user has none itself. (Covet, Thief) *
*  Items stolen from wild PokÃ©mon are kept after the battle.               *
******************************************************************************/
public class Move0F1 : BattleMove {
	public Move0F1(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!attacker.Fainted() && !opponent.Fainted() && opponent.item != 0 && opponent.damageState.CalculatedDamage > 0 && !opponent.damageState.Substitute) {
			if (!attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.STICKYHOLD)) {
				string abilityName = Abilities.GetName(opponent.ability);
				battle.Display(string.Format("{0}'s {1} made {2} ineffective!",opponent.String(), abilityName, name));
			} else if (!battle.IsUnlosableItem(opponent, opponent.item) && !battle.IsUnlosableItem(attacker, attacker.item) && attacker.item == 0 && (battle.opponent != null || battle.opponent.Length > 0 || !battle.IsOpposing(attacker.index))) {
				string itemName = Items.GetName(opponent.item);
				attacker.item = opponent.item;
				opponent.item = 0;
				opponent.effects[Effects.ChoiceBand] = -1;
				opponent.effects[Effects.Unburden] = 1;
				if ((battle.opponent == null || battle.opponent.Length == 0) && attacker.pokemon.itemInitial == 0 && opponent.pokemon.itemInitial == attacker.item) {
					attacker.pokemon.itemInitial = attacker.item;
					opponent.pokemon.itemInitial = 0;
				}
				battle.Display(string.Format("{0} stole {1}'s {2}!",attacker.String(), opponent.String(true), itemName));
			}
		}
	}
}

/***********************************************************************
*  User and target swap items. They remain swapped after wild battles. *
*  (Switcheroo, Trick)                                                 *
***********************************************************************/
public class Move0F2 : BattleMove {
	public Move0F2(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if ((opponent.effects[Effects.Substitute]>0 && !IgnoresSubstitute(attacker)) || (attacker.item==0 && opponent.item==0) || ((battle.opponent == null || battle.opponent.Length == 0) && battle.IsOpposing(attacker.index))) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (battle.IsUnlosableItem(opponent, opponent.item) || battle.IsUnlosableItem(attacker, opponent.item) || battle.IsUnlosableItem(opponent, attacker.item) || battle.IsUnlosableItem(attacker, attacker.item)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (!attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.STICKYHOLD)) {
			string abilityName = Abilities.GetName(opponent.ability);
			battle.Display(string.Format("{0}'s {1} made {2} ineffective!",opponent.String(), abilityName, name));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int oldattitem = attacker.item;
		int oldoppitem = opponent.item;
		string oldattitemname = Items.GetName(oldattitem);
		string oldoppitemname = Items.GetName(oldoppitem);
		int tempitem = attacker.item;
		attacker.item = opponent.item;
		opponent.item = tempitem;
		if ((battle.opponent == null || battle.opponent.Length == 0) && attacker.pokemon.itemInitial == oldattitem && opponent.pokemon.itemInitial == oldoppitem) {
			attacker.pokemon.itemInitial = oldoppitem;
			opponent.pokemon.itemInitial = oldattitem;
		}
		battle.Display(string.Format("{0} switched items with its opponent!",attacker.String()));
		if (oldoppitem>0 && oldattitem>0) {
			battle.DisplayPaused(string.Format("{0} obtained {1}.",attacker.String(), oldoppitemname));
			battle.Display(string.Format("{0} obtained {1}.",opponent.String(), oldattitemname));
		} else {
			if (oldoppitem > 0) {
				battle.Display(string.Format("{0} obtained {1}.",attacker.String(), oldoppitemname));
			}
			if (oldattitem > 0) {
				battle.Display(string.Format("{0} obtained {1}.",opponent.String(), oldattitemname));
			}
		}
		attacker.effects[Effects.ChoiceBand] = -1;
		opponent.effects[Effects.ChoiceBand] = -1;
		return 0;
	}
}

/*********************************************************************************
*  User gives its item to the target. The item remains given after wild battles. *
*  (Bestow)                                                                      *
*********************************************************************************/
public class Move0F3 : BattleMove {
	public Move0F3(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if ((opponent.effects[Effects.Substitute]>0 && !IgnoresSubstitute(attacker)) || attacker.item == 0 || opponent.item != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (battle.IsUnlosableItem(attacker, attacker.item) || battle.IsUnlosableItem(opponent, attacker.item) || battle.IsUnlosableItem(opponent, attacker.item) || battle.IsUnlosableItem(attacker, attacker.item)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		string itemname = Items.GetName(attacker.item);
		opponent.item = attacker.item;
		attacker.item = 0;
		attacker.effects[Effects.ChoiceBand] = -1;
		attacker.effects[Effects.Unburden] = 1;
		if ((battle.opponent == null || battle.opponent.Length == 0) && attacker.pokemon.itemInitial == opponent.item) {
			opponent.pokemon.itemInitial = opponent.item;
			attacker.pokemon.itemInitial = 0;
		}
		battle.Display(string.Format("{0} received {1} from {2}!",opponent.String(), itemname, attacker.String(true)));
		return 0;
	}
}

/************************************************************************
*  User consumes target's berry and gains its effect. (Bug Bite, Pluck) *
************************************************************************/
public class Move0F4 : BattleMove {
	public Move0F4(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!attacker.Fainted() && !opponent.Fainted() && Items.IsBerry(opponent.item) && opponent.damageState.CalculatedDamage>0 && !opponent.damageState.Substitute) {
			if (attacker.HasMoldBreaker() || !opponent.HasWorkingAbility(Abilities.STICKYHOLD)) {
				int item = opponent.item;
				string itemname = Items.GetName(item);
				opponent.ConsumeItem(false, false);
				battle.Display(string.Format("{0} stole and ate its target's {1}!",attacker.String(), itemname));
				if (!attacker.HasWorkingAbility(Abilities.KLUTZ) && attacker.effects[Effects.Embargo]==0) {
					attacker.ActivateBerryEffect(item, false);
				}
				if (attacker.item == 0 && attacker.Partner() != null && attacker.Partner().HasWorkingAbility(Abilities.SYMBIOSIS)) {
					Battler partner = attacker.Partner();
					if (partner.item > 0 && !battle.IsUnlosableItem(partner, partner.item) && !battle.IsUnlosableItem(attacker, partner.item)) {
						battle.Display(string.Format("{0}'s {1} let it share its {2} with {3}!",partner.String(), Abilities.GetName(partner.ability), Items.GetName(partner.item), attacker.String(true)));
						attacker.item = partner.item;
						partner.item = 0;
						partner.effects[Effects.Unburden] = 1;
						attacker.BerryCureCheck();
					}
				}
			}
		}
	}
}

/*********************************************
*  Target's berry is destroyed. (Incinerate) *
*********************************************/
public class Move0F5 : BattleMove {
	public Move0F5(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (!attacker.Fainted() && opponent.damageState.CalculatedDamage>0 && !opponent.damageState.Substitute && (Items.IsBerry(opponent.item) || (Settings.USE_NEW_BATTLE_MECHANICS && Items.IsGem(opponent.item)))) {
			string itemname = Items.GetName(opponent.item);
			opponent.ConsumeItem(false, false);
			battle.Display(string.Format("{0}'s {1} was incinerated",opponent.String(), itemname));
		}
		return ret;
	}
}

/***************************************************************
*  User recovers the last item it held and consumed. (Recycle) *
***************************************************************/
public class Move0F6 : BattleMove {
	public Move0F6(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.pokemon == null || attacker.pokemon.itemRecycle == 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		int item = attacker.pokemon.itemRecycle;
		string itemname = Items.GetName(item);
		attacker.item = item;
		if (battle.opponent == null || battle.opponent.Length == 0) {
			if (attacker.pokemon.itemInitial == 0) {
				attacker.pokemon.itemInitial = item;
			}
		}
		attacker.pokemon.itemRecycle = 0;
		attacker.effects[Effects.PickupItem] = 0;
		attacker.effects[Effects.PickupUse] = 0;
		battle.Display(string.Format("{0} found one {1}!",attacker.String(), itemname));
		return 0;
	}
}

/**********************************************************************************
*  User flings its item at the target. Power and effect dep} on the item. (Fling) *
**********************************************************************************/
public class Move0F7 : BattleMove {
	public Move0F7(Battle battle, Moves.Move move) : base(battle, move) {}

	public int[] FlingArray(int i) {
		int[] ret = new int[0];
		if (i == 130) {
			ret = new int[1]{Items.IRONBALL};
		}
		if (i == 100) {
			ret = new int[13]{Items.ARMORFOSSIL,Items.CLAWFOSSIL,Items.COVERFOSSIL,Items.DOMEFOSSIL,Items.HARDSTONE,Items.HELIXFOSSIL,Items.JAWFOSSIL,Items.OLDAMBER,Items.PLUMEFOSSIL,Items.RAREBONE,Items.ROOTFOSSIL,Items.SAILFOSSIL,Items.SKULLFOSSIL};
		}
		if (i == 90) {
			ret = new int[20]{Items.DEEPSEATOOTH,Items.DRACOPLATE,Items.DREADPLATE,Items.EARTHPLATE,Items.FISTPLATE,Items.FLAMEPLATE,Items.GRIPCLAW,Items.ICICLEPLATE,Items.INSECTPLATE,Items.IRONPLATE,Items.MEADOWPLATE,Items.MINDPLATE,Items.PIXIEPLATE,Items.SKYPLATE,Items.SPLASHPLATE,Items.SPOOKYPLATE,Items.STONEPLATE,Items.THICKCLUB,Items.TOXICPLATE,Items.ZAPPLATE};
		}
		if (i == 80) {
			ret = new int[14]{Items.ASSAULTVEST,Items.DAWNSTONE,Items.DUSKSTONE,Items.ELECTIRIZER,Items.MAGMARIZER,Items.ODDKEYSTONE,Items.OVALSTONE,Items.PROTECTOR,Items.QUICKCLAW,Items.RAZORCLAW,Items.SAFETYGOGGLES,Items.SHINYSTONE,Items.STICKYBARB,Items.WEAKNESSPOLICY};
		}
		if (i == 70) {
			ret = new int[12]{Items.BURNDRIVE,Items.CHILLDRIVE,Items.DOUSEDRIVE,Items.DRAGONFANG,Items.POISONBARB,Items.POWERANKLET,Items.POWERBAND,Items.POWERBELT,Items.POWERBRACER,Items.POWERLENS,Items.POWERWEIGHT,Items.SHOCKDRIVE};
		}
		if (i == 60) {
			ret = new int[8]{Items.ADAMANTORB,Items.DAMPROCK,Items.GRISEOUSORB,Items.HEATROCK,Items.LUSTROUSORB,Items.MACHOBRACE,Items.ROCKYHELMET,Items.STICK};
		}
		if (i == 50) {
			ret = new int[2]{Items.DUBIOUSDISC,Items.SHARPBEAK};
		}
		if (i == 40) {
			ret = new int[3]{Items.EVIOLITE,Items.ICYROCK,Items.LUCKYPUNCH};
		}
		if (i == 30) {
			ret = new int[170]{Items.ABILITYCAPSULE,Items.ABILITYURGE,Items.ABSORBBULB,Items.AMAZEMULCH,Items.AMULETCOIN,Items.ANTIDOTE,Items.AWAKENING,Items.BALMMUSHROOM,Items.BERRYJUICE,Items.BIGMUSHROOM,Items.BIGNUGGET,Items.BIGPEARL,Items.BINDINGBAND,Items.BLACKBELT,Items.BLACKFLUTE,Items.BLACKGLASSES,Items.BLACKSLUDGE,Items.BLUEFLUTE,Items.BLUESHARD,Items.BOOSTMULCH,Items.BURNHEAL,Items.CALCIUM,Items.CARBOS,Items.CASTELIACONE,Items.CELLBATTERY,Items.CHARCOAL,Items.CLEANSETAG,Items.COMETSHARD,Items.DAMPMULCH,Items.DEEPSEASCALE,Items.DIREHIT,Items.DIREHIT2,Items.DIREHIT3,Items.DRAGONSCALE,Items.EJECTBUTTON,Items.ELIXIR,Items.ENERGYPOWDER,Items.ENERGYROOT,Items.ESCAPEROPE,Items.ETHER,Items.EVERSTONE,Items.EXPSHARE,Items.FIRESTONE,Items.FLAMEORB,Items.FLOATSTONE,Items.FLUFFYTAIL,Items.FRESHWATER,Items.FULLHEAL,Items.FULLRESTORE,Items.GOOEYMULCH,Items.GREENSHARD,Items.GROWTHMULCH,Items.GUARDSPEC,Items.HEALPOWDER,Items.HEARTSCALE,Items.HONEY,Items.HPUP,Items.HYPERPOTION,Items.ICEHEAL,Items.IRON,Items.ITEMDROP,Items.ITEMURGE,Items.KINGSROCK,Items.LAVACOOKIE,Items.LEAFSTONE,Items.LEMONADE,Items.LIFEORB,Items.LIGHTBALL,Items.LIGHTCLAY,Items.LUCKYEGG,Items.LUMINOUSMOSS,Items.LUMIOSEGALETTE,Items.MAGNET,Items.MAXELIXIR,Items.MAXETHER,Items.MAXPOTION,Items.MAXREPEL,Items.MAXREVIVE,Items.METALCOAT,Items.METRONOME,Items.MIRACLESEED,Items.MOOMOOMILK,Items.MOONSTONE,Items.MYSTICWATER,Items.NEVERMELTICE,Items.NUGGET,Items.OLDGATEAU,Items.PARALYZEHEAL,Items.PASSORB,Items.PEARL,Items.PEARLSTRING,Items.POKEDOLL,Items.POKETOY,Items.POTION,Items.PPMAX,Items.PPUP,Items.PRISMSCALE,Items.PROTEIN,Items.RAGECANDYBAR,Items.RARECANDY,Items.RAZORFANG,Items.REDFLUTE,Items.REDSHARD,Items.RELICBAND,Items.RELICCOPPER,Items.RELICCROWN,Items.RELICGOLD,Items.RELICSILVER,Items.RELICSTATUE,Items.RELICVASE,Items.REPEL,Items.RESETURGE,Items.REVIVALHERB,Items.REVIVE,Items.RICHMULCH,Items.SACHET,Items.SACREDASH,Items.SCOPELENS,Items.SHALOURSABLE,Items.SHELLBELL,Items.SHOALSALT,Items.SHOALSHELL,Items.SMOKEBALL,Items.SNOWBALL,Items.SODAPOP,Items.SOULDEW,Items.SPELLTAG,Items.STABLEMULCH,Items.STARDUST,Items.STARPIECE,Items.SUNSTONE,Items.SUPERPOTION,Items.SUPERREPEL,Items.SURPRISEMULCH,Items.SWEETHEART,Items.THUNDERSTONE,Items.TINYMUSHROOM,Items.TOXICORB,Items.TWISTEDSPOON,Items.UPGRADE,Items.WATERSTONE,Items.WHIPPEDDREAM,Items.WHITEFLUTE,Items.XACCURACY,Items.XACCURACY2,Items.XACCURACY3,Items.XACCURACY6,Items.XATTACK,Items.XATTACK2,Items.XATTACK3,Items.XATTACK6,Items.XDEFENSE,Items.XDEFENSE2,Items.XDEFENSE3,Items.XDEFENSE6,Items.XSPDEF,Items.XSPDEF2,Items.XSPDEF3,Items.XSPDEF6,Items.XSPATK,Items.XSPATK2,Items.XSPATK3,Items.XSPATK6,Items.XSPEED,Items.XSPEED2,Items.XSPEED3,Items.XSPEED6,Items.YELLOWFLUTE,Items.YELLOWSHARD,Items.ZINC};
		}
		if (i == 20) {
			ret = new int[7]{Items.CLEVERWING,Items.GENIUSWING,Items.HEALTHWING,Items.MUSCLEWING,Items.PRETTYWING,Items.RESISTWING,Items.SWIFTWING};
		}
		if (i == 10) {
			ret = new int[44]{Items.AIRBALLOON,Items.BIGROOT,Items.BLUESCARF,Items.BRIGHTPOWDER,Items.CHOICEBAND,Items.CHOICESCARF,Items.CHOICESPECS,Items.DESTINYKNOT,Items.EXPERTBELT,Items.FOCUSBAND,Items.FOCUSSASH,Items.FULLINCENSE,Items.GREENSCARF,Items.LAGGINGTAIL,Items.LAXINCENSE,Items.LEFTOVERS,Items.LUCKINCENSE,Items.MENTALHERB,Items.METALPOWDER,Items.MUSCLEBAND,Items.ODDINCENSE,Items.PINKSCARF,Items.POWERHERB,Items.PUREINCENSE,Items.QUICKPOWDER,Items.REAPERCLOTH,Items.REDCARD,Items.REDSCARF,Items.RINGTARGET,Items.ROCKINCENSE,Items.ROSEINCENSE,Items.SEAINCENSE,Items.SHEDSHELL,Items.SILKSCARF,Items.SILVERPOWDER,Items.SMOOTHROCK,Items.SOFTSAND,Items.SOOTHEBELL,Items.WAVEINCENSE,Items.WHITEHERB,Items.WIDELENS,Items.WISEGLASSES,Items.YELLOWSCARF,Items.ZOOMLENS};
		}
		return ret;
	}

	public new bool MoveFailed(Battler attacker, Battler opponent) {
		if (attacker.item == 0 || battle.IsUnlosableItem(attacker, attacker.item) || Items.IsPokeball(attacker.item) || battle.field.effects[Effects.MagicRoom]>0 || attacker.HasWorkingAbility(Abilities.KLUTZ) || attacker.effects[Effects.Embargo] > 0) {
			return true;
		}
		int[] flingKeys = new int[11]{10,20,30,40,50,60,70,80,90,100,130};
		for (int i=0; i<flingKeys.Length; i++) 
		{
			for (int j=0; j<FlingArray(flingKeys[i]).Length; j++) {
				if (attacker.item == FlingArray(flingKeys[i])[j]) {
					return false;
				}
			}
		}
		if (Items.IsBerry(attacker.item) && !attacker.Opposing1().HasWorkingAbility(Abilities.UNNERVE) && !attacker.Opposing2().HasWorkingAbility(Abilities.UNNERVE)) {
			return false;
		}
		return true;
	}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (Items.IsBerry(attacker.item)) {
			return 10;
		}
		if (Items.IsMegaStone(attacker.item)) {
			return 80;
		}
		int[] flingKeys = new int[11]{10,20,30,40,50,60,70,80,90,100,130};
		for (int i=0; i<flingKeys.Length; i++) 
		{
			for (int j=0; j<FlingArray(flingKeys[i]).Length; j++) {
				if (attacker.item == FlingArray(flingKeys[i])[j]) {
					return flingKeys[i];
				}
			}
		}
		return 1;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.item == 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		attacker.effects[Effects.Unburden] = 1;
		battle.Display(string.Format("{0} flung its {1}!",attacker.String(), Items.GetName(attacker.item)));
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0 && !opponent.damageState.Substitute && (attacker.HasMoldBreaker() || !opponent.HasWorkingAbility(Abilities.SHIELDDUST))) {
			if (attacker.HasWorkingBerry()) {
				opponent.ActivateBerryEffect(attacker.item, false);
			} else if (attacker.HasWorkingItem(Items.FLAMEORB)) {
				if (opponent.CanBurn(attacker, false, this)) {
					opponent.Burn(attacker);
				}
			} else if (attacker.HasWorkingItem(Items.KINGSROCK) || attacker.HasWorkingItem(Items.RAZORFANG)) {
				opponent.Flinch(attacker);
			} else if (attacker.HasWorkingItem(Items.LIGHTBALL)) {
				if (opponent.CanParalyze(attacker, false, this)) {
					opponent.Paralyze(attacker);
				}
			} else if (attacker.HasWorkingItem(Items.MENTALHERB)) {
				if (opponent.effects[Effects.Attract]>=0) 
				{
					opponent.CureAttract();
					battle.Display(string.Format("{0} got over its infatuation.",opponent.String()));
				}
				if (opponent.effects[Effects.Taunt] > 0) 
				{
					opponent.effects[Effects.Taunt] = 0;
					battle.Display(string.Format("{0}'s taunt wore off!",opponent.String()));
				}
				if (opponent.effects[Effects.Encore] > 0) 
				{
					opponent.effects[Effects.Encore] = 0;
					opponent.effects[Effects.EncoreMove] = 0;
					opponent.effects[Effects.EncoreIndex] = 0;
					battle.Display(string.Format("{0}'s encore ended!",opponent.String()));
				}
				if (opponent.effects[Effects.Torment] > 0) 
				{
					opponent.effects[Effects.Torment] = 0;
					battle.Display(string.Format("{0}'s torment wore off!",opponent.String()));
				}
				if (opponent.effects[Effects.Disable] > 0) 
				{
					opponent.effects[Effects.Disable] = 0;
					battle.Display(string.Format("{0} is no longer disabled!",opponent.String()));
				}
				if (opponent.effects[Effects.HealBlock] > 0) 
				{
					opponent.effects[Effects.HealBlock] = 0;
					battle.Display(string.Format("{0}'s Heal Block wore off!",opponent.String()));
				}
			} else if (attacker.HasWorkingItem(Items.POISONBARB)) {
				if (opponent.CanPoison(attacker, false, this)) {
					opponent.Poison(attacker, null, true);
				}
			} else if (attacker.HasWorkingItem(Items.WHITEHERB)) {
				while (true) {
					bool reducedStats = false;
					int[] stats = new int[7]{Stats.ATTACK, Stats.DEFENSE, Stats.SPEED, Stats.SPATK, Stats.SPDEF, Stats.ACCURACY, Stats.EVASION};
					for (int i=0; i<stats.Length; i++) 
					{
						if (opponent.stages[stats[i]]<0) {
							opponent.stages[stats[i]] = 0;
							reducedStats = true;
						}
					}
					if (!reducedStats) {
						break;
					}
					battle.Display(string.Format("{0}'s status is returned to normal!",opponent.String()));
				}
			}
		}
		attacker.ConsumeItem();
		return ret;
	}
}

/****************************************************************************
*  For 5 rounds, the target cannnot use its held item, its held item has no *
*  effect, and no items can be used on it. (Embargo)                        *
****************************************************************************/
public class Move0F8 : BattleMove {
	public Move0F8(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Embargo] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.Embargo] = 5;
		battle.Display(string.Format("{0} can't use items anymore!",opponent.String()));
		return 0;
	}
}

/******************************************************************************
*  For 5 rounds, all held items cannot be used in any way and have no effect. *
*  Held items can still change hands, but can't be thrown. (Magic Room)       *
******************************************************************************/
public class Move0F9 : BattleMove {
	public Move0F9(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (battle.field.effects[Effects.MagicRoom] > 0) {
			battle.field.effects[Effects.MagicRoom] = 0;
			battle.Display(string.Format("The area returned to normal!"));
		} else {
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			battle.field.effects[Effects.MagicRoom] = 5;
			battle.Display(string.Format("It created a bizarre area in which Pokémon's held items lose their effects!"));
		}
		return 0;
	}
}

/************************************************************************
*  User takes recoil damage equal to 1/4 of the damage this move dealt. *
************************************************************************/
public class Move0FA : BattleMove {
	public Move0FA(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsRecoilMove() {
		return true;
	}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!attacker.Fainted() && turneffects[Effects.TotalDamage]>0) {
			if (!attacker.HasWorkingAbility(Abilities.ROCKHEAD) && !attacker.HasWorkingAbility(Abilities.MAGICGUARD)) {
				attacker.ReduceHP((int)Math.Round(turneffects[Effects.TotalDamage]/4.0));
				battle.Display(string.Format("{0} is damaged by recoil!",attacker.String()));
			}
		}
	}
}

/************************************************************************
*  User takes recoil damage equal to 1/3 of the damage this move dealt. *
************************************************************************/
public class Move0FB : BattleMove {
	public Move0FB(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsRecoilMove() {
		return true;
	}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!attacker.Fainted() && turneffects[Effects.TotalDamage]>0) {
			if (!attacker.HasWorkingAbility(Abilities.ROCKHEAD) && !attacker.HasWorkingAbility(Abilities.MAGICGUARD)) {
				attacker.ReduceHP((int)Math.Round(turneffects[Effects.TotalDamage]/3.0));
				battle.Display(string.Format("{0} is damaged by recoil!",attacker.String()));
			}
		}
	}
}

/************************************************************************
*  User takes recoil damage equal to 1/2 of the damage this move dealt. *
*  (Head Smash)                                                         *
************************************************************************/
public class Move0FC : BattleMove {
	public Move0FC(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsRecoilMove() {
		return true;
	}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!attacker.Fainted() && turneffects[Effects.TotalDamage]>0) {
			if (!attacker.HasWorkingAbility(Abilities.ROCKHEAD) && !attacker.HasWorkingAbility(Abilities.MAGICGUARD)) {
				attacker.ReduceHP((int)Math.Round(turneffects[Effects.TotalDamage]/2.0));
				battle.Display(string.Format("{0} is damaged by recoil!",attacker.String()));
			}
		}
	}
}

/************************************************************************
*  User takes recoil damage equal to 1/3 of the damage this move dealt. *
*  May paralyze the target. (Volt Tackle)                               *
************************************************************************/
public class Move0FD : BattleMove {
	public Move0FD(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsRecoilMove() {
		return true;
	}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!attacker.Fainted() && turneffects[Effects.TotalDamage]>0) {
			if (!attacker.HasWorkingAbility(Abilities.ROCKHEAD) && !attacker.HasWorkingAbility(Abilities.MAGICGUARD)) {
				attacker.ReduceHP((int)Math.Round(turneffects[Effects.TotalDamage]/3.0));
				battle.Display(string.Format("{0} is damaged by recoil!",attacker.String()));
			}
		}
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanParalyze(attacker, false, this)) {
			opponent.Paralyze(attacker);
		}
	}
}

/************************************************************************
*  User takes recoil damage equal to 1/3 of the damage this move dealt. *
*  May burn the target. (Flare Blitz)                                   *
************************************************************************/
public class Move0FE : BattleMove {
	public Move0FE(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsRecoilMove() {
		return true;
	}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!attacker.Fainted() && turneffects[Effects.TotalDamage]>0) {
			if (!attacker.HasWorkingAbility(Abilities.ROCKHEAD) && !attacker.HasWorkingAbility(Abilities.MAGICGUARD)) {
				attacker.ReduceHP((int)Math.Round(turneffects[Effects.TotalDamage]/3.0));
				battle.Display(string.Format("{0} is damaged by recoil!",attacker.String()));
			}
		}
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanBurn(attacker, false, this)) {
			opponent.Burn(attacker);
		}
	}
}

/*************************************
*  Starts sunny weather. (Sunny Day) *
*************************************/
public class Move0FF : BattleMove {
	public Move0FF(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		switch (battle.weather) 
		{
			case Weather.HEAVYRAIN:
				battle.Display(string.Format("There is no relief from this heavy rain!"));
				return -1;
			case Weather.HARSHSUN:
				battle.Display(string.Format("The extremely harsh sunlight was not lessened at all!"));
				return -1;
			case Weather.STRONGWINDS:
				battle.Display(string.Format("The mysterious air current blows on regardless!"));
				return -1;
			case Weather.SUNNYDAY:
				battle.Display(string.Format("But it failed!"));
				return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.weather = Weather.SUNNYDAY;
		battle.weatherduration = 5;
		if (attacker.HasWorkingItem(Items.HEATROCK)) {
			battle.weatherduration = 8;
		}
		battle.CommonAnimation("Sunny", null, null);
		battle.Display(string.Format("The sunlight turned harsh!"));
		return 0;
	}
}

/**************************************
*  Starts rainy weather. (Rain Dance) *
**************************************/
public class Move100 : BattleMove {
	public Move100(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		switch (battle.weather) 
		{
			case Weather.HEAVYRAIN:
				battle.Display(string.Format("There is no relief from this heavy rain!"));
				return -1;
			case Weather.HARSHSUN:
				battle.Display(string.Format("The extremely harsh sunlight was not lessened at all!"));
				return -1;
			case Weather.STRONGWINDS:
				battle.Display(string.Format("The mysterious air current blows on regardless!"));
				return -1;
			case Weather.SUNNYDAY:
				battle.Display(string.Format("But it failed!"));
				return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.weather = Weather.RAINDANCE;
		battle.weatherduration = 5;
		if (attacker.HasWorkingItem(Items.DAMPROCK)) {
			battle.weatherduration = 8;
		}
		battle.CommonAnimation("Rain", null, null);
		battle.Display(string.Format("It started to rain!"));
		return 0;
	}
}

/*****************************************
*  Starts sandstorm weather. (Sandstorm) *
*****************************************/
public class Move101 : BattleMove {
	public Move101(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		switch (battle.weather) 
		{
			case Weather.HEAVYRAIN:
				battle.Display(string.Format("There is no relief from this heavy rain!"));
				return -1;
			case Weather.HARSHSUN:
				battle.Display(string.Format("The extremely harsh sunlight was not lessened at all!"));
				return -1;
			case Weather.STRONGWINDS:
				battle.Display(string.Format("The mysterious air current blows on regardless!"));
				return -1;
			case Weather.SUNNYDAY:
				battle.Display(string.Format("But it failed!"));
				return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.weather = Weather.SANDSTORM;
		battle.weatherduration = 5;
		if (attacker.HasWorkingItem(Items.SMOOTHROCK)) {
			battle.weatherduration = 8;
		}
		battle.CommonAnimation("Sandstorm", null, null);
		battle.Display(string.Format("A sandstorm brewed!"));
		return 0;
	}
}

/*******************************
*  Starts hail weather. (Hail) *
*******************************/
public class Move102 : BattleMove {
	public Move102(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		switch (battle.weather) 
		{
			case Weather.HEAVYRAIN:
				battle.Display(string.Format("There is no relief from this heavy rain!"));
				return -1;
			case Weather.HARSHSUN:
				battle.Display(string.Format("The extremely harsh sunlight was not lessened at all!"));
				return -1;
			case Weather.STRONGWINDS:
				battle.Display(string.Format("The mysterious air current blows on regardless!"));
				return -1;
			case Weather.SUNNYDAY:
				battle.Display(string.Format("But it failed!"));
				return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.weather = Weather.HAIL;
		battle.weatherduration = 5;
		if (attacker.HasWorkingItem(Items.ICYROCK)) {
			battle.weatherduration = 8;
		}
		battle.CommonAnimation("Hail", null, null);
		battle.Display(string.Format("It started to hail!"));
		return 0;
	}
}

/****************************************************************************
*  Entry hazard. Lays spikes on the opposing side (max. 3 layers). (Spikes) *
****************************************************************************/
public class Move103 : BattleMove {
	public Move103(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OpposingSide().effects[Effects.Spikes]>=3) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.OpposingSide().effects[Effects.Spikes]++;
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display(string.Format("Spikes were scattered all around the opposing team's feet!"));
		} else {
			battle.Display(string.Format("Spikes were scattered all around your team's feet!"));
		}
		return 0;
	}
}

/**************************************************************************
*  Entry hazard. Lays poison spikes on the opposing side (max. 2 layers). *
*  (Toxic Spikes)                                                         *
**************************************************************************/
public class Move104 : BattleMove {
	public Move104(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OpposingSide().effects[Effects.ToxicSpikes]>=2) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.OpposingSide().effects[Effects.ToxicSpikes]++;
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display(string.Format("Poison spikes were scattered all around the opposing team's feet!"));
		} else {
			battle.Display(string.Format("Poison spikes were scattered all around your team's feet!"));
		}
		return 0;
	}
}

/*************************************************************************
*  Entry hazard. Lays stealth rocks on the opposing side. (Stealth Rock) *
*************************************************************************/
public class Move105 : BattleMove {
	public Move105(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OpposingSide().effects[Effects.Spikes] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.OpposingSide().effects[Effects.StealthRock] = 1;
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display(string.Format("Pointed stones float in the air around the opposing team!"));
		} else {
			battle.Display(string.Format("Pointed stones float in the air around your team!"));
		}
		return 0;
	}
}

/***********************************************************************************
*  Forces ally's Pledge move to be used next, if it hasn't already. (Grass Pledge) *
*  Combo's with ally's Pledge move if it was just used. Power is doubled, and      *
*  causes either a sea of fire or a swamp on the opposing side.                    *
***********************************************************************************/
public class Move106 : BattleMove {
	bool doubleDamage;
	bool overrideType;
	public Move106(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool OnStartUse(Battler attacker) {
		doubleDamage = false;
		overrideType = false;
		if (attacker.effects[Effects.FirstPledge] == 0x107 || attacker.effects[Effects.FirstPledge] == 0x108) {
			battle.Display(string.Format("The two moves have become one! It's a combined move!"));
			doubleDamage = true;
			if (attacker.effects[Effects.FirstPledge] == 0x107) {
				overrideType = true;
			}
		}
		return true;
	}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (doubleDamage) {
			return baseDamage * 2;
		}
		return baseDamage;
	}

	public new int ModifyType(int type, Battler attacker, Battler opponent) {
		if (overrideType) {
			type = Types.FIRE;
		}
		return base.ModifyType(type, attacker, opponent);
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!battle.doubleBattle || attacker.Partner() == null || attacker.Partner().Fainted()) {
			attacker.effects[Effects.FirstPledge] = 0;
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (attacker.effects[Effects.FirstPledge] == 0x107) {
			int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
			if (opponent.damageState.CalculatedDamage > 0) {
				attacker.OpposingSide().effects[Effects.SeaOfFire] = 4;
				if (!battle.IsOpposing(attacker.index)) {
					battle.Display(string.Format("A sea of fire enveloped the opposing team!"));
					battle.CommonAnimation("SeaOfFireOpp", null, null);
				} else {
					battle.Display(string.Format("A sea of fire enveloped your team!"));
					battle.CommonAnimation("SeaOfFire", null, null);
				}
			}
			attacker.effects[Effects.FirstPledge] = 0;
			return 0;
		} else if (attacker.effects[Effects.FirstPledge]==0x108) {
			int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
			if (opponent.damageState.CalculatedDamage > 0) {
				attacker.OpposingSide().effects[Effects.Swamp] = 4;
				if (!battle.IsOpposing(attacker.index)) {
					battle.Display(string.Format("A swamp enveloped the opposing team!"));
					battle.CommonAnimation("SwampOpp", null, null);
				} else {
					battle.Display(string.Format("A swamp enveloped your team!"));
					battle.CommonAnimation("Swamp", null, null);
				}
			}
			attacker.effects[Effects.FirstPledge] = 0;
			return 0;
		}
		attacker.effects[Effects.FirstPledge] = 0;
		int partnerMove = -1;
		if (battle.useMoveChoice[attacker.Partner().index] == 1) {
			if (!attacker.Partner().HasMovedThisRound()) {
				BattleMove m = battle.moveChoice[attacker.Partner().index];
				if (m != null && m.id > 0) {
					partnerMove = battle.moveChoice[attacker.Partner().index].function;
				}
			}
		}
		if (partnerMove == 0x107 || partnerMove == 0x108) {
			battle.Display(string.Format("{0} is waiting for {1}'s move...",attacker.String(), attacker.Partner().String()));
			attacker.Partner().effects[Effects.FirstPledge] = function;
			attacker.Partner().effects[Effects.MoveNext] = 1;
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}

	public new void ShowAnimation(int id, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (overrideType) {
			return base.ShowAnimation(Moves.FIREPLEDGE, attacker, opponent, hitNum, allTargets, showAnimation);
		}
		return base.ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/*************************************************************************************
*  Forces ally's Pledge move to be used next, if it hasn't already. (Fire Pledge)    *
*  Combo's with ally's Pledge move if it was just used. Power is doubled, and        *
*  causes either a sea of fire on the opposing side or a rainbow on the user's side. *
*************************************************************************************/
public class Move107 : BattleMove {
	bool doubleDamage;
	bool overrideType;
	public Move107(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool OnStartUse(Battler attacker) {
		doubleDamage = false;
		overrideType = false;
		if (attacker.effects[Effects.FirstPledge] == 0x106 || attacker.effects[Effects.FirstPledge] == 0x108) {
			battle.Display(string.Format("The two moves have become one! It's a combined move!"));
			doubleDamage = true;
			if (attacker.effects[Effects.FirstPledge] == 0x108) {
				overrideType = true;
			}
		}
		return true;
	}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (doubleDamage) {
			return baseDamage * 2;
		}
		return baseDamage;
	}

	public new int ModifyType(int type, Battler attacker, Battler opponent) {
		if (overrideType) {
			type = Types.WATER;
		}
		return base.ModifyType(type, attacker, opponent);
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!battle.doubleBattle || attacker.Partner() == null || attacker.Partner().Fainted()) {
			attacker.effects[Effects.FirstPledge] = 0;
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (attacker.effects[Effects.FirstPledge] == 0x106) {
			int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
			if (opponent.damageState.CalculatedDamage > 0) {
				attacker.OpposingSide().effects[Effects.SeaOfFire] = 4;
				if (!battle.IsOpposing(attacker.index)) {
					battle.Display(string.Format("A sea of fire enveloped the opposing team!"));
					battle.CommonAnimation("SeaOfFireOpp", null, null);
				} else {
					battle.Display(string.Format("A sea of fire enveloped your team!"));
					battle.CommonAnimation("SeaOfFire", null, null);
				}
			}
			attacker.effects[Effects.FirstPledge] = 0;
			return 0;
		} else if (attacker.effects[Effects.FirstPledge]==0x108) {
			int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
			if (opponent.damageState.CalculatedDamage > 0) {
				attacker.OpposingSide().effects[Effects.Swamp] = 4;
				if (!battle.IsOpposing(attacker.index)) {
					battle.Display(string.Format("A rainbow appeared in the sky on your team's side!"));
					battle.CommonAnimation("Rainbow", null, null);
				} else {
					battle.Display(string.Format("A rainbow appeared in the sky on the opposing team's side!"));
					battle.CommonAnimation("RainbowOpp", null, null);
				}
			}
			attacker.effects[Effects.FirstPledge] = 0;
			return 0;
		}
		attacker.effects[Effects.FirstPledge] = 0;
		int partnerMove = -1;
		if (battle.useMoveChoice[attacker.Partner().index] == 1) {
			if (!attacker.Partner().HasMovedThisRound()) {
				BattleMove m = battle.moveChoice[attacker.Partner().index];
				if (m != null && m.id > 0) {
					partnerMove = battle.moveChoice[attacker.Partner().index].function;
				}
			}
		}
		if (partnerMove == 0x106 || partnerMove == 0x108) {
			battle.Display(string.Format("{0} is waiting for {1}'s move...",attacker.String(), attacker.Partner().String()));
			attacker.Partner().effects[Effects.FirstPledge] = function;
			attacker.Partner().effects[Effects.MoveNext] = 1;
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}

	public new void ShowAnimation(int id, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (overrideType) {
			return base.ShowAnimation(Moves.WATERPLEDGE, attacker, opponent, hitNum, allTargets, showAnimation);
		}
		return base.ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/***********************************************************************************
*  Forces ally's Pledge move to be used next, if it hasn't already. (Water Pledge) *
*  Combo's with ally's Pledge move if it was just used. Power is doubled, and      *
*  causes either a swamp on the opposing side or a rainbow on the user's side.     *
***********************************************************************************/
public class Move108 : BattleMove {
	bool doubleDamage;
	bool overrideType;
	public Move108(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool OnStartUse(Battler attacker) {
		doubleDamage = false;
		overrideType = false;
		if (attacker.effects[Effects.FirstPledge] == 0x106 || attacker.effects[Effects.FirstPledge] == 0x108) {
			battle.Display(string.Format("The two moves have become one! It's a combined move!"));
			doubleDamage = true;
			if (attacker.effects[Effects.FirstPledge] == 0x108) {
				overrideType = true;
			}
		}
		return true;
	}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if (doubleDamage) {
			return baseDamage * 2;
		}
		return baseDamage;
	}

	public new int ModifyType(int type, Battler attacker, Battler opponent) {
		if (overrideType) {
			type = Types.GRASS;
		}
		return base.ModifyType(type, attacker, opponent);
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!battle.doubleBattle || attacker.Partner() == null || attacker.Partner().Fainted()) {
			attacker.effects[Effects.FirstPledge] = 0;
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (attacker.effects[Effects.FirstPledge] == 0x106) {
			int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
			if (opponent.damageState.CalculatedDamage > 0) {
				attacker.OpposingSide().effects[Effects.Swamp] = 4;
				if (!battle.IsOpposing(attacker.index)) {
					battle.Display(string.Format("A swamp enveloped the opposing team!"));
					battle.CommonAnimation("SwampOpp", null, null);
				} else {
					battle.Display(string.Format("A swamp enveloped your team!"));
					battle.CommonAnimation("Swamp", null, null);
				}
			}
			attacker.effects[Effects.FirstPledge] = 0;
			return 0;
		} else if (attacker.effects[Effects.FirstPledge]==0x107) {
			int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
			if (opponent.damageState.CalculatedDamage > 0) {
				attacker.OpposingSide().effects[Effects.Swamp] = 4;
				if (!battle.IsOpposing(attacker.index)) {
					battle.Display(string.Format("A rainbow appeared in the sky on your team's side!"));
					battle.CommonAnimation("Rainbow", null, null);
				} else {
					battle.Display(string.Format("A rainbow appeared in the sky on the opposing team's side!"));
					battle.CommonAnimation("RainbowOpp", null, null);
				}
			}
			attacker.effects[Effects.FirstPledge] = 0;
			return 0;
		}
		attacker.effects[Effects.FirstPledge] = 0;
		int partnerMove = -1;
		if (battle.useMoveChoice[attacker.Partner().index] == 1) {
			if (!attacker.Partner().HasMovedThisRound()) {
				BattleMove m = battle.moveChoice[attacker.Partner().index];
				if (m != null && m.id > 0) {
					partnerMove = battle.moveChoice[attacker.Partner().index].function;
				}
			}
		}
		if (partnerMove == 0x106 || partnerMove == 0x107) {
			battle.Display(string.Format("{0} is waiting for {1}'s move...",attacker.String(), attacker.Partner().String()));
			attacker.Partner().effects[Effects.FirstPledge] = function;
			attacker.Partner().effects[Effects.MoveNext] = 1;
			return 0;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}

	public new void ShowAnimation(int id, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (overrideType) {
			return base.ShowAnimation(Moves.GRASSPLEDGE, attacker, opponent, hitNum, allTargets, showAnimation);
		}
		return base.ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/*******************************************************************************
*  Scatters coins that the player picks up after winning the battle. (Pay Day) *
*******************************************************************************/
public class Move109 : BattleMove {
	public Move109(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			if (battle.OwnedByPlayer(attacker.index)) {
				battle.extraMoney += 5*attacker.level;
				if (battle.extraMoney > Settings.MAX_MONEY) {
					battle.extraMoney = Settings.MAX_MONEY;
				}
			}
			battle.Display(string.Format("Coins were scattered everywhere!"));
		}
		return ret;
	}
}

/******************************************************************
*  }s the opposing side's Light Screen and Reflect. (Brick Break) *
******************************************************************/
public class Move10A : BattleMove {
	public Move10A(Battle battle, Moves.Move move) : base(battle, move) {}

	public bool CalcDamage(Battler attacker, Battler opponent) {
		return base.CalcDamage(attacker, opponent, NOREFLECT);
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (attacker.OpposingSide().effects[Effects.Reflect] > 0) {
			attacker.OpposingSide().effects[Effects.Reflect] = 0;
			if (!battle.IsOpposing(attacker.index)) {
				battle.Display(string.Format("The opposing team's Reflect wore off!"));
			} else {
				battle.Display(string.Format("Your team's Reflect wore off!"));
			}
		}
		if (attacker.OpposingSide().effects[Effects.LightScreen] > 0) {
			attacker.OpposingSide().effects[Effects.LightScreen] = 0;
			if (!battle.IsOpposing(attacker.index)) {
				battle.Display(string.Format("The opposing team's Light Screen wore off!"));
			} else {
				battle.Display(string.Format("Your team's Light Screen wore off!"));
			}
		}
		return ret;
	}

	public new void ShowAnimation(int id, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OpposingSide().effects[Effects.Reflect]>0 || attacker.OpposingSide().effects[Effects.LightScreen]>0) {
			return base.ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
		}
		return base.ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/***************************************************************
*  If attack misses, user takes crash damage of 1/2 of max HP. *
*  (Hi Jump Kick, Jump Kick)                                   *
***************************************************************/
public class Move10B : BattleMove {
	public Move10B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsRecoilMove() {
		return true;
	}

	public new bool UnusableInGravity() {
		return true;
	}
}

/************************************************************
*  User turns 1/4 of max HP into a substitute. (Substitute) *
************************************************************/
public class Move10C : BattleMove {
	public Move10C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.Substitute]>0) {
			battle.Display(string.Format("{0} already has a substitute!",attacker.String()));
			return -1;
		}
		int sublife = (int)Math.Max(attacker.totalHP/4, 1);
		if (attacker.hp <= sublife) {
			battle.Display(string.Format("It was too weak to make a substitute!"));
			return -1;
		}
		attacker.ReduceHP(sublife, false, false);
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.MultiTurn] = 0;
		attacker.effects[Effects.MultiTurnAttack] = 0;
		attacker.effects[Effects.Substitute] = sublife;
		battle.Display(string.Format("{0} put in a substitute!",attacker.String()));
		return 0;
	}
}

/********************************************************************************
*  User is not Ghost: Decreases the user's Speed, increases the user's Attack & *
*  Defense by 1 stage each.                                                     *
*  User is Ghost: User loses 1/2 of max HP, and curses the target.              *
*  Cursed PokÃ©mon lose 1/4 of their max HP at the } of each round.           *
*  (Curse)                                                                      *
********************************************************************************/
public class Move10D : BattleMove {
	public Move10D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		bool failed = false;
		if (attacker.HasType(Types.GHOST)) {
			if (opponent.effects[Effects.Curse] != 0 || opponent.OwnSide().effects[Effects.CraftyShield] != 0) {
				failed = true;
			} else {
				ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
				battle.Display(string.Format("{0} cut its own HP and laid a curse on {1}!",attacker.String(), opponent.String(true)));
				opponent.effects[Effects.Curse] = 1;
				attacker.ReduceHP(attacker.totalHP/2);
			}
		} else {
			bool lowerSpeed = attacker.CanReduceStatStage(Stats.SPEED, attacker, false, this);
			bool raiseAtk = attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this);
			bool raiseDef = attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this);
			if (!lowerSpeed && !raiseAtk && !raiseDef) {
				failed = true;
			} else {
				ShowAnimation(id, attacker, null, 1, allTargets, showAnimation);
				if (lowerSpeed) {
					attacker.ReduceStat(Stats.SPEED, 1, attacker, false, this);
				}
				bool showAnim = true;
				if (raiseAtk) {
					attacker.IncreaseStat(Stats.ATTACK, 1, attacker, false, this, showAnim);
					showAnim = false;
				}
				if (raiseAtk) {
					attacker.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this, showAnim);
					showAnim = false;
				}
			}
		}
		if (failed) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		return failed ? -1 : 0;
	}
}

/***********************************************
*  Target's last move used loses 4 PP. (Spite) *
***********************************************/
public class Move10E : BattleMove {
	public Move10E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		for (int i=0; i<opponent.moves.Length; i++) 
		{
			BattleMove m = opponent.moves[i];
			if (m.id == opponent.lastMoveUsed && i.id > 0 && i.pp > 0) {
				showAnim
				int reduction = (int)Math.Min(4, m.pp);
				opponent.SetPP(m, m.pp-reduction);
				battle.Display(string.Format("It reduced the PP of {0}'s {1} by {2}!",opponent.String(true), m.name, reduction));
				return 0;
			}
		}
		battle.Display(string.Format("But it failed!"));
		return -1;
	}
}

/********************************************************************************
*  Target will lose 1/4 of max HP at } of each round, while asleep. (Nightmare) *
********************************************************************************/
public class Move10F : BattleMove {
	public Move10F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.status != Statuses.SLEEP || opponent.effects[Effects.Nightmare] != 0 || (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker))) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.Nightmare] = 1;
		battle.Display(string.Format("{0} began having a nightmare!",opponent.String()));
		return 0;
	}
}

/*****************************************************************************
*  Removes trapping moves, entry hazards and Leech Seed on user/user's side. *
*  (Rapid Spin)                                                              *
*****************************************************************************/
public class Move110 : BattleMove {
	public Move110(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!attacker.Fainted() && turneffects[Effects.TotalDamage]>0) {
			if (attacker.effects[Effects.MultiTurn] > 0) {
				string mtattack = Moves.GetName(attacker.effects[Effects.MultiTurnAttack]);
				Battler mtuser = battle.battlers[attacker.effects[Effects.MultiTurnUser]];
				battle.Display(string.Format("{0} got free of {1}'s {2}!",attacker.String(), mtuser.String(true), mtattack));
				attacker.effects[Effects.MultiTurn] = 0;
				attacker.effects[Effects.MultiTurnAttack] = 0;
				attacker.effects[Effects.MultiTurnUser] = -1;
			}
			if (attacker.effects[Effects.LeechSeed] >= 0) {
				attacker.effects[Effects.LeechSeed] = -1;
				battle.Display(string.Format("{0} shed Leech Seed!",attacker.String()));
			}
			if (attacker.effects[Effects.StealthRock] != 0) {
				attacker.effects[Effects.StealthRock] = 0;
				battle.Display(string.Format("{0} blew away stealth rocks!",attacker.String()));
			}
			if (attacker.effects[Effects.Spikes] > 0) {
				attacker.effects[Effects.Spikes] = 0;
				battle.Display(string.Format("{0} blew away spikes!",attacker.String()));
			}
			if (attacker.effects[Effects.ToxicSpikes] > 0) {
				attacker.effects[Effects.ToxicSpikes] = 0;
				battle.Display(string.Format("{0} blew away poison spikes!",attacker.String()));
			}
			if (attacker.effects[Effects.StickyWeb] != 0) {
				attacker.effects[Effects.StickyWeb] = 0;
				battle.Display(string.Format("{0} blew away sticky webs!",attacker.String()));
			}
		}
	}
}

/***************************************************************
*  Attacks 2 rounds in the future. (Doom Desire, Future Sight) *
***************************************************************/
public class Move111 : BattleMove {
	public Move111(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int DisplayUseMessage(Battler attacker) {
		if (battle.futureSight) {
			return 0;
		}
		return base.DisplayUseMessage(attacker);
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.FutureSight]>0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (battle.futureSight) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.FutureSight] = 3;
		opponent.effects[Effects.FutureSightMove] = id;
		opponent.effects[Effects.FutureSightUser] = attacker.pokemonIndex;
		opponent.effects[Effects.FutureSightUserPos] = attacker.index;
		if (id == Moves.FUTURESIGHT) {
			battle.Display(string.Format("{0} foresaw an attack!",attacker.String()));
		} else {
			battle.Display(string.Format("{0} chose Doom Desire as its destiny!",attacker.String()));
		}
		return 0;
	}

	public new void ShowAnimation(int id, Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (battle.futureSight) {
			return base.ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
		}
		return base.ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/*****************************************************************************
*  Increases the user's Defense and Special Defense by 1 stage each. Ups the *
*  user's stockpile by 1 (max. 3). (Stockpile)                               *
*****************************************************************************/
public class Move112 : BattleMove {
	public Move112(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.Stockpile]>=3) {
			battle.Display(string.Format("{0} can't stockpile any more!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.Stockpile]++;
		battle.Display(string.Format("{0} stockpiled {1}!",attacker.String(), attacker.effects[Effects.Stockpile]));
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
			attacker.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this, showAnim);
			attacker.effects[Effects.StockpileDef]++;
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPDEF, 1, attacker, false, this, showAnim);
			attacker.effects[Effects.StockpileSpDef]++;
			showAnim = false;
		}
		return 0;
	}
}

/***********************************************************************************
*  Power is 100 multiplied by the user's stockpile (X). Resets the stockpile to    *
*  0. Decreases the user's Defense and Special Defense by X stages each. (Spit Up) *
***********************************************************************************/
public class Move113 : BattleMove {
	public Move113(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool MoveFailed(Battler attacker, Battler opponent) {
		return attacker.effects[Effects.Stockpile]==0;
	}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		return 100*attacker.effects[Effects.Stockpile];
	}

	public new void EffectAfterHit(Battler attacker, Battler opponent, Dictionary<int, int> turneffects) {
		if (!attacker.Fainted() && turneffects[Effects.TotalDamage] > 0) {
			bool showAnim;
			if (attacker.effects[Effects.StockpileDef]>0) {
				if (attacker.CanReduceStatStage(Stats.DEFENSE, attacker, false, this)) {
					attacker.ReduceStat(Stats.DEFENSE, attacker.effects[Effects.StockpileDef], attacker, false, this, showAnim);
					showAnim = false;
				}
			}
			if (attacker.effects[Effects.StockpileSpDef]>0) {
				if (attacker.CanReduceStatStage(Stats.SPDEF, attacker, false, this)) {
					attacker.ReduceStat(Stats.SPDEF, attacker.effects[Effects.StockpileSpDef], attacker, false, this, showAnim);
					showAnim = false;
				}
			}
			attacker.effects[Effects.Stockpile] = 0;
			attacker.effects[Effects.StockpileDef] = 0;
			attacker.effects[Effects.StockpileSpDef] = 0;
			battle.Display(string.Format("{0}'s stockpiled effect wore off!",attacker.String()));
		}
	}
}

/********************************************************************************
*  Heals user dep}ing on the user's stockpile (X). Resets the stockpile to 0.   *
*  Decreases the user's Defense and Special Defense by X stages each. (Swallow) *
********************************************************************************/
public class Move114 : BattleMove {
	public Move114(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int hpGain = 0;
		switch (attacker.effects[Effects.Stockpile]) 
		{
			case 0:
				battle.Display(string.Format("But it failed to swallow a thing!"));
				return -1;
			case 1:
				hpGain = attacker.totalHP/4;
				break;
			case 2:
				hpGain = attacker.totalHP/2;
				break;
			case 3:
				hpGain = attacker.totalHP;
				break;
		}
		if (attacker.hp == attacker.totalHP && attacker.effects[Effects.StockpileDef] == 0 && attacker.effects[Effects.StockpileSpDef] == 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		if (attacker.ReduceHP(hpGain, true) > 0) {
			battle.Display(string.Format("{0}'s HP was restored.",attacker.String()));
		}
		bool showAnim = true;
		if (attacker.effects[Effects.StockpileDef]>0) {
			if (attacker.CanReduceStatStage(Stats.DEFENSE, attacker, false, this)) {
				attacker.ReduceStat(Stats.DEFENSE, attacker.effects[Effects.StockpileDef], attacker, false, this, showAnim);
				showAnim = false;
			}
		}
		if (attacker.effects[Effects.StockpileSpDef]>0) {
			if (attacker.CanReduceStatStage(Stats.SPDEF, attacker, false, this)) {
				attacker.ReduceStat(Stats.SPDEF, attacker.effects[Effects.StockpileSpDef], attacker, false, this, showAnim);
				showAnim = false;
			}
		}
		attacker.effects[Effects.Stockpile] = 0;
		attacker.effects[Effects.StockpileDef] = 0;
		attacker.effects[Effects.StockpileSpDef] = 0;
		battle.Display(string.Format("{0}'s stockpiled effect wore off!",attacker.String()));
		return 0;
	}
}

/**********************************************************************
*  Fails if user was hit by a damaging move this round. (Focus Punch) *
**********************************************************************/
public class Move115 : BattleMove {
	public Move115(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int DisplayUseMessage(Battler attacker) {
		if (attacker.lastHPLost > 0) {
			battle.DisplayBrief(string.Format("{0} lost its focus and couldn't move!",attacker.String()));
			return -1;
		}
		return base.DisplayUseMessage(attacker);
	}
}

/******************************************************************************
*  Fails if the target didn't chose a damaging move to use this round, or has *
*  already moved. (Sucker Punch)                                              *
******************************************************************************/
public class Move116 : BattleMove {
	public Move116(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool MoveFailed(Battler attacker, Battler opponent) {
		if (battle.useMoveChoice[opponent.index] != 1) {
			return true;
		}
		BattlerMove oppMove = battle.moveChoice[opponent.index];
		if (oppMove == null || oppMove.id <= 0 || oppMove.IsStatus()) {
			return true;
		}
		if (opponent.HasMovedThisRound() && oppMove.function != 0xB0) {
			return true;
		}
		return false;
	}
}

/****************************************************************************
*  This round, user becomes the target of attacks that have single targets. *
*  (Follow Me, Rage Powder)                                                 *
****************************************************************************/
public class Move117 : BattleMove {
	public Move117(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!battle.doubleBattle) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.FollowMe] = 1;
		if (!attacker.Partner().Fainted() && attacker.Partner().effects[Effects.FollowMe]>0) {
			attacker.effects[Effects.FollowMe] = attacker.Partner().effects[Effects.FollowMe]+1;
		}
		battle.Display(string.Format("{0} became the center of attention!",attacker.String()));
	}
}

/************************************************************************************
*  For 5 rounds, increases gravity on the field. PokÃ©mon cannot become airborne. *
*  (Gravity)                                                                        *
************************************************************************************/
public class Move118 : BattleMove {
	public Move118(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (battle.field.effects[Effects.Gravity]>0) 
		{
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		battle.field.effects[Effects.Gravity] = 5;
		for (int i=0; i<4; i++) 
		{
			Battler poke = battle.battlers[i];
			if (poke == null) {
				continue;
			}
			if ((new Moves.Move(poke.effects[Effects.TwoTurnAttack])).Function() == 0xC9 || (new Moves.Move(poke.effects[Effects.TwoTurnAttack])).Function() == 0xCC || (new Moves.Move(poke.effects[Effects.TwoTurnAttack])).Function() == 0xCE) {
				poke.effects[Effects.TwoTurnAttack] = 0;
			}
			if (poke.effects[Effects.SkyDrop] != 0) {
				poke.effects[Effects.SkyDrop] = 0;
			}
			if (poke.effects[Effects.MagnetRise] > 0) {
				poke.effects[Effects.MagnetRise] = 0;
			}
			if (poke.effects[Effects.Telekinesis] > 0) {
				poke.effects[Effects.Telekinesis] = 0;
			}
		}
		battle.Display(string.Format("Gravity intensified!"));
		return 0;
	}
}

/******************************************************
*  For 5 rounds, user becomes airborne. (Magnet Rise) *
******************************************************/
public class Move119 : BattleMove {
	public Move119(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool UnusableInGravity() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.Ingrain] != 0 || attacker.effects[Effects.SmackDown] != 0 || attacker.effects[Effects.MagnetRise] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.MagnetRise] = 5;
		battle.Display(string.Format("{0} levitated with electromagnetism!",attacker.String()));
		return 0;
	}
}

/******************************************************************************
*  For 3 rounds, target becomes airborne and can always be hit. (Telekinesis) *
******************************************************************************/
public class Move11A : BattleMove {
	public Move11A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool UnusableInGravity() {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.Ingrain] != 0 || attacker.effects[Effects.SmackDown] != 0 || attacker.effects[Effects.MagnetRise] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.MagnetRise] = 5;
		battle.Display(string.Format("{0} was hurled into the air!",attacker.String()));
		return 0;
	}
}/****
* Hits airborne semi-invulnerable targets. (Sky Uppercut)
*/
public class Move11B : BattleMove {
	public Move11B(Battle battle, Moves.Move move) : base(battle, move) {}
// Handled in Battler's pbSuccessCheck, do not edit!
}

/*******************************************************************************
*  Grounds the target while it remains active. (Smack Down, Thousand Arrows)   *
*  (Handled in Battler's pbSuccessCheck): Hits some semi-invulnerable targets. *
*******************************************************************************/
public class Move11C : BattleMove {
	public Move11C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int BaseDamage(int baseDamage, Battler attacker, Battler opponent) {
		if ((new Moves.Move(poke.effects[Effects.TwoTurnAttack])).Function() == 0xC9 || (new Moves.Move(poke.effects[Effects.TwoTurnAttack])).Function() == 0xCC || (new Moves.Move(poke.effects[Effects.TwoTurnAttack])).Function() == 0xCE || opponent.effects[Effects.SkyDrop] != 0) {
			return baseDamage*2;
		}
		return baseDamage;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage>0 && !opponent.damageState.Substitute && !opponent.effects[Effects.Roost] != 0) {
			opponent.effects[Effects.SmackDown] = 1;
			bool showMsg = (opponent.HasType(Types.FLYING) || opponent.HasWorkingAbility(Abilities.LEVITATE));
			if ((new Moves.Move(poke.effects[Effects.TwoTurnAttack])).Function() == 0xC9 || (new Moves.Move(poke.effects[Effects.TwoTurnAttack])).Function() == 0xCC) {
				opponent.effects[Effects.TwoTurnAttack] = 0;
				showMsg = true;
			}
			if (opponent.effects[Effects.MagnetRise] > 0) {
				opponent.effects[Effects.MagnetRise] = 0;
				showMsg = true;
			}
			if (opponent.effects[Effects.Telekinesis] > 0) {
				opponent.effects[Effects.Telekinesis] = 0;
				showMsg = true;
			}
			if (showMsg) {
				battle.Display(string.Format("{0} fell straight down!",opponent.String()));
			}
		}
		return ret;
	}
}

/*********************************************************************************
*  Target moves immediately after the user, ignoring priority/speed. (After You) *
*********************************************************************************/
public class Move11D : BattleMove {
	public Move11D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool MoveFailed(Battler attacker, Battler opponent) {
		if (opponent.effects[Effects.MoveNext] != 0) {
			return true;
		}
		if (battle.useMoveChoice[opponent.index] != 1) {
			return true;
		}
		BattleMove oppMove = battle.moveChoice[opponent.index];
		if (oppMove == null || oppMove.id <= 0) {
			return true;
		}
		if (opponent.HasMovedThisRound()) {
			return true;
		}
		return false;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.MoveNext] = 1;
		opponent.effects[Effects.Quash] = 0;
		battle.Display(string.Format("{0} took the kind offer!",opponent.String()));
		return 0;
	}
}

/******************************************************************
*  Target moves last this round, ignoring priority/speed. (Quash) *
******************************************************************/
public class Move11E : BattleMove {
	public Move11E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool MoveFailed(Battler attacker, Battler opponent) {
		if (opponent.effects[Effects.Quash] != 0) {
			return true;
		}
		if (battle.useMoveChoice[opponent.index] != 1) {
			return true;
		}
		BattleMove oppMove = battle.moveChoice[opponent.index];
		if (oppMove == null || oppMove.id <= 0) {
			return true;
		}
		if (opponent.HasMovedThisRound()) {
			return true;
		}
		return false;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.Quash] = 1;
		opponent.effects[Effects.MoveNext] = 0;
		battle.Display(string.Format("{0}'s move was postponed!",opponent.String()));
		return 0;
	}
}

/***********************************************************************************
*  For 5 rounds, for each priority bracket, slow PokÃ©mon move before fast ones. *
*  (Trick Room)                                                                    *
***********************************************************************************/
public class Move11F : BattleMove {
	public Move11F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (battle.field.effects[Effects.TrickRoom] > 0) {
			battle.field.effects[Effects.TrickRoom] = 0;
			battle.Display(string.Format("{0} reverted the dimensions!",attacker.String()));
		} else {
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			battle.field.effects[Effects.TrickRoom] = 5;
			battle.Display(string.Format("{0} twisted the dimensions!",attacker.String()));
		}
		return 0;
	}
}

/*****************************************************
*  User switches places with its ally. (Ally Switch) *
*****************************************************/
public class Move120 : BattleMove {
	public Move120(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!battle.doubleBattle || attacker.Partner() == null || attacker.Partner().Fainted()) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		Battler temp = battle.battlers[attacker.index];
		battle.battlers[attacker.index] = battle.battlers[attacker.Partner().index];
		battle.battlers[attacker.Partner().index] = temp;
		int[] effectsToSwap = new int[6]{Effects.BideTarget, Effects.CounterTarget, Effects.LeechSeed, Effects.LockOnPos, Effects.MeanLook, Effects.MirrorCoatTarget};
		for (int i=0; i<effectsToSwap.Length; i++) 
		{
			int temp = a.effects[effectsToSwap[i]];
			battle.battlers[attacker.index].effects[effectsToSwap[i]] = battle.battlers[attacker.Partner().index].effects[effectsToSwap[i]];
			battle.battlers[attacker.Partner().index].effects[effectsToSwap[i]] = temp;
		}
		attacker.Update(true);
		opponent.Update(true);
		battle.Display(string.Format("{0} and {1} switched places!",opponent.String(), attacker.String(true)));
	}
}

/**********************************************************************************
*  Target's Attack is used instead of user's Attack for this move's calculations. *
*  (Foul Play)                                                                    *
**********************************************************************************/
public class Move121 : BattleMove {
	public Move121(Battle battle, Moves.Move move) : base(battle, move) {}
// Handled in superclass public bool CalcDamage, do not edit!
}

/***************************************************************************
*  Target's Defense is used instead of its Special Defense for this move's *
*  calculations. (Psyshock, Psystrike, Secret Sword)                       *
***************************************************************************/
public class Move122 : BattleMove {
	public Move122(Battle battle, Moves.Move move) : base(battle, move) {}
// Handled in superclass public bool CalcDamage, do not edit!
}

/***************************************************************************
*  Only damages PokÃ©mon that share a type with the user. (Synchronoise) *
***************************************************************************/
public class Move123 : BattleMove {
	public Move123(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!opponent.HasType(attacker.type1) && !opponent.HasType(attacker.type2) && !opponent.HasType(attacker.effects[Effects.Type3])) {
			battle.Display(string.Format("{0} was unaffected!",opponent.String()));
			return -1;
		}
		return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
	}
}

/*****************************************************************************
*  For 5 rounds, swaps all battlers' base Defense with base Special Defense. *
*  (Wonder Room)                                                             *
*****************************************************************************/
public class Move124 : BattleMove {
	public Move124(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (battle.field.effects[Effects.WonderRoom] > 0) {
			battle.field.effects[Effects.WonderRoom] = 0;
			battle.Display(string.Format("Wonder Room wore off, and the Defense and Sp. Def stats returned to normal!"));
		} else {
			ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
			battle.field.effects[Effects.WonderRoom] = 5;
			battle.Display(string.Format("It created a bizarre area in which the Defense and Sp. Def stats are swapped!"));
		}
		return 0;
	}
}

/******************************************************************************
*  Fails unless user has already used all other moves it knows. (Last Resort) *
******************************************************************************/
public class Move125 : BattleMove {
	public Move125(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool MoveFailed(Battler attacker, Battler opponent) {
		int counter = 0;
		int numMoves = 0;
		for (int i=0; i<attacker.moves.Length; i++) 
		{
			BattleMove m = attacker.moves[i];
			if (m.id <= 0) {
				continue;
			}
			if (m.id != id && !attacker.movesUsed.Contains(m.id)) {
				counter++;
			}
			numMoves++;
		}
		return counter != 0 || numMoves == 1;
	}
}

//===============================================================================
// NOTE: Shadow moves use function codes 126-132 inclusive.
//===============================================================================
/*****************************************
*  Does absolutely nothing. (Hold Hands) *
*****************************************/
public class Move133 : BattleMove {
	public Move133(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!battle.doublebattle || attacker.Partner() == null || attacker.Partner().Fainted()) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return 0;
	}
}

/*****************************************************************
*  Does absolutely nothing. Shows a special message. (Celebrate) *
*****************************************************************/
public class Move134 : BattleMove {
	public Move134(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.Display(string.Format("Congratulations, {0}!",battle.GetOwner(attacker.index).name));
		return 0;
	}
}

/**************************************************************************
*  Freezes the target. (Freeze-Dry)                                       *
*  (Superclass's pbTypeModifier): Effectiveness against Water-type is 2x. *
**************************************************************************/
public class Move135 : BattleMove {
	public Move135(Battle battle, Moves.Move move) : base(battle, move) {}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanFreeze(attacker, false, this)) {
			opponent.Freeze();
		}
	}
}

/********************************************************************************
*  Increases the user's Defense by 1 stage for each target hit. (Diamond Storm) *
********************************************************************************/
public class Move136 : Move01D {
// No difference to function code 01D. It may need to be separate in future.
	public Move136(Battle battle, Moves.Move move) : base(battle, move) {}
}

/******************************************************************************
*  Increases the user's and its ally's Defense and Special Defense by 1 stage *
*  each, if they have Plus or Minus. (Magnetic Flux)                          *
******************************************************************************/
public class Move137 : BattleMove {
	public Move137(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		bool didSomething = false;
		Battler[] bt = new int[2]{attacker, attacker.Partner()};
		for (int i=0; i<b.Length; i++) 
		{
			Battler b = bt[i];
			if (b == null || b.Fainted()) {
				continue;
			}
			if (!b.HasWorkingAbility(Abilities.PLUS) && !b.HasWorkingAbility(Abilities.MINUS)) {
				continue;
			}
			if (!b.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this) && !b.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this)) {
				ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
				didSomething = true;
				bool showAnim = true;
			}
			if (b.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
				b.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
			if (b.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this)) {
				b.IncreaseStat(Stats.SPDEF, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
		}
		if (!didSomething) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		return 0;
	}
}

/****************************************************************
*  Increases ally's Special Defense by 1 stage. (Aromatic Mist) *
****************************************************************/
public class Move138 : BattleMove {
	public Move138(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!battle.doubleBattle || opponent == null || !opponent.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.IncreaseStat(Stats.SPDEF, 1, attacker, false, this) ? 0 : -1;
	}
}

/**********************************************************************
*  Decreases the target's Attack by 1 stage. Always hits. (Play Nice) *
**********************************************************************/
public class Move139 : BattleMove {
	public Move139(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool AccuracyCheck(Battler attacker, Battler opponent) {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (!opponent.CanReduceStatStage(Stats.ATTACK, attacker, true, this)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return attacker.ReduceStat(Stats.ATTACK, 1, attacker, false, this) ? 0 : -1;
	}
}

/**********************************************************************************
*  Decreases the target's Attack and Special Attack by 1 stage each. (Noble Roar) *
**********************************************************************************/
public class Move13A : BattleMove {
	public Move13A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("{0}'s attack missed!",attacker.String()));
			return -1;
		}
		if (opponent.TooLow(Stats.ATTACK) && opponent.TooLow(Stats.SPATK)) {
			battle.Display(string.Format("{0}'s stats won't go any lower!",opponent.String()));
			return -1;
		}
		if (opponent.OwnSide().effects[Effects.Mist]>0) {
			battle.Display(string.Format("{0} is protected by Mist!",opponent.String()));
			return -1;
		}
		if (!attacker.HasMoldBreaker()) {
			if (opponent.HasWorkingAbility(Abilities.CLEARBODY) || opponent.HasWorkingAbility(Abilities.WHITESMOKE)) {
				battle.Display(string.Format("{0}'s {1} prevents stat loss!",opponent.String(), Abilities.GetName(opponent.ability)));
				return -1;
			}
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		int ret = -1;
		bool showAnim = true;
		if (!attacker.HasMoldBreaker() && opponent.HasWorkingAbility(Abilities.HYPERCUTTER)) {
			string abilityName = Abilities.GetName(opponent.ability);
			battle.Display(string.Format("{0}'s {1} prevents Attack loss!",opponent.String(), abilityName));
		} else if (opponent.ReduceStat(Stats.ATTACK,1,attacker,false,this,showAnim)) {
			ret = 0;
			showAnim = false;
		}
		if (opponent.ReduceStat(Stats.SPATK,1,attacker,false,this,showAnim)) {
			ret = 0;
			showAnim = false;
		}
		return ret;
	}
}

/*****************************************************************************
*  Decreases the target's Defense by 1 stage. Always hits. (Hyperspace Fury) *
*****************************************************************************/
public class Move13B : BattleMove {
	public Move13B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool MoveFailed(Battler attacker, Battler opponent) {
		if (attacker.species != Species.HOOPA) {
			return true;
		}
		if (attacker.form != 1) {
			return true;
		}
		return false;
	}

	public new bool AccuracyCheck(Battler attacker, Battler opponent) {
		return true;
	}

	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanReduceStatStage(Stats.DEFENSE, attacker, false, this)) {
			opponent.ReduceStat(Stats.DEFENSE, 1, attacker, false, this);
		}
	}
}

/****************************************************************************
*  Decreases the target's Special Attack by 1 stage. Always hits. (Confide) *
****************************************************************************/
public class Move13C : BattleMove {
	public Move13C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool AccuracyCheck(Battler attacker, Battler opponent) {
		return true;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.CanReduceStatStage(Stats.DEFENSE, attacker, false, this)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return opponent.ReduceStat(Stats.SPATK, 1, attacker, false, this) ? 0 : -1;
	}
}

/**********************************************************************
*  Decreases the target's Special Attack by 2 stages. (Eerie Impulse) *
**********************************************************************/
public class Move13D : BattleMove {
	public Move13D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (IsDamaging()) {
			return base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		}
		if (TypeImmunityByAbility(GetType(type, attacker, opponent), attacker, opponent)) {
			return -1;
		}
		if (!opponent.CanReduceStatStage(Stats.SPATK, attacker, true, this)) {
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		return opponent.ReduceStat(Stats.SPATK, 2, attacker, false, this) ? 0 : -1;
	}


	public new void AdditionalEffect(Battler attacker, Battler opponent) {
		if (opponent.damageState.Substitute) {
			return;
		}
		if (opponent.CanReduceStatStage(Stats.SPATK, attacker, false, this)) {
			opponent.ReduceStat(Stats.SPATK, 1, attacker, false, this);
		}
	}
}

/*************************************************************************************
*  Increases the Attack and Special Attack of all Grass-type PokÃ©mon on the field *
*  by 1 stage each. Doesn't affect airborne PokÃ©mon. (Rototiller)                 *
*************************************************************************************/
public class Move13E : BattleMove {
	public Move13E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		bool didSomething = false;
		Battler[] bt = new Battler[4]{attacker, attacker.Partner(), attacker.Opposing1(), attacker.Opposing2()};
		for (int i=0; i<bt.Length; i++) 
		{
			Battler b = bt[i];
			if (b == null || b.Fainted()) {
				continue;
			}
			if (!b.HasType(Types.GRASS)) {
				continue;
			}
			if (b.IsAirborne(attacker.HasMoldBreaker())) {
				continue;
			}
			if (!b.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this) && !b.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
				continue;
			}
			ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
			didSomething = true;
			bool showAnim = true;
			if (b.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
				b.IncreaseStat(Stats.ATTACK, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
			if (b.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
				b.IncreaseStat(Stats.SPATK, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
		}
		if (!didSomething) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		return 0;
	}
}

/**********************************************************************************
*  Increases the Defense of all Grass-type PokÃ©mon on the field by 1 stage each. *
*  (Flower Shield)                                                                *
**********************************************************************************/
public class Move13F : BattleMove {
	public Move13F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		bool didSomething = false;
		Battler[] bt = new Battler[4]{attacker, attacker.Partner(), attacker.Opposing1(), attacker.Opposing2()};
		for (int i=0; i<bt.Length; i++) 
		{
			Battler b = bt[i];
			if (b == null || b.Fainted()) {
				continue;
			}
			if (!b.HasType(Types.GRASS)) {
				continue;
			}
			if (!b.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this) && !b.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
				continue;
			}
			ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
			didSomething = true;
			bool showAnim = true;
			if (b.CanIncreaseStatStage(Stats.DEFENSE, attacker, false, this)) {
				b.IncreaseStat(Stats.DEFENSE, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
		}
		if (!didSomething) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		return 0;
	}
}

/*****************************************************************************************
*  Decreases the Attack, Special Attack and Speed of all poisoned Battler opponents by 1 *
*  stage each. (Venom Drench)                                                            *
*****************************************************************************************/
public class Move140 : BattleMove {
	public Move140(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		bool didSomething = false;
		Battler[] bt = new int[2]{attacker.Opposing1(), attacker.Opposing2()};
		for (int i=0; i<bt.Length; i++) 
		{
			Battler b = bt[i];
			if (b == null || b.Fainted()) {
				continue;
			}
			if (!b.status == Statuses.POISON) {
				continue;
			}
			if (!b.CanReduceStatStage(Stats.ATTACK, attacker, false, this) && !b.CanReduceStatStage(Stats.SPATK, attacker, false, this) && !b.CanReduceStatStage(Stats.SPEED, attacker, false, this)) {
				continue;
			}
			ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
			didSomething = true;
			bool showAnim = true;
			if (b.CanReduceStatStage(Stats.ATTACK, attacker, false, this)) {
				b.ReduceStat(Stats.ATTACK, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
			if (b.CanReduceStatStage(Stats.SPATK, attacker, false, this)) {
				b.ReduceStat(Stats.SPATK, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
			if (b.CanReduceStatStage(Stats.SPEED, attacker, false, this)) {
				b.ReduceStat(Stats.SPEED, 1, attacker, false, this, showAnim);
				showAnim = false;
			}
		}
		if (!didSomething) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		return 0;
	}
}

/**********************************************************
*  Reverses all stat changes of the target. (Topsy-Turvy) *
**********************************************************/
public class Move141 : BattleMove {
	public Move141(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		bool nonzero = false;
		int[] stats = new int[7]{Stats.ATTACK, Stats.DEFENSE, Stats.SPEED, Stats.SPATK, Stats.SPDEF, Stats.ACCURACY, Stats.EVASION};
		for (int i=0; i<stats.Length; i++) 
		{
			if (opponent.stages[stats[i]] != 0) {
				nonzero = true;
				break;
			}
		}
		if (!nonzero) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		for (int i=0; i<stats.Length; i++) {
			opponent.stages[stats[i]] *= -1;
		}
		battle.Display(string.Format("{0}'s stats were reversed!",opponent.String()));
		return 0;
	}
}

/*************************************************
*  Gives target the Ghost type. (Trick-or-Treat) *
*************************************************/
public class Move142 : BattleMove {
	public Move142(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if ((opponent.effects[Effects.Substitute] > 0 && !IgnoresSubstitute(attacker)) || opponent.HasType(Types.GHOST) || opponent.ability == Abilities.MULTITYPE) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.Type3] = Types.GHOST;
		string typeName = Types.GetName(Types.GHOST);
		battle.Display(string.Format("{0} transformed into the {1} type!",opponent.String(), typeName));
		return 0;
	}
}

/*************************************************
*  Gives target the Grass type. (Forest's Curse) *
*************************************************/
public class Move143 : BattleMove {
	public Move143(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Substitute]>0 && !IgnoresSubstitute(attacker)) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (TypeImmunityByAbility(GetType(type, attacker, opponent), attacker, opponent)) {
			return -1;
		}
		if (opponent.effects[Effects.LeechSeed]>=0) {
			battle.Display(string.Format("{0} evaded the attack!",opponent.String()));
			return -1;
		}
		if (opponent.HasType(Types.GRASS) || opponent.ability == Abilities.MULTITYPE) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.Type3] = Types.GRASS;
		string typeName = Types.GetName(Types.GRASS);
		battle.Display(string.Format("{0} transformed into the {1} type!",opponent.String(), typeName));
		return 0;
	}
}

/**********************************************************************************
*  Damage is multiplied by Flying's effectiveness against the target. Does double *
*  damage and has perfect accuracy if the target is Minimized. (Flying Press)     *
**********************************************************************************/
public class Move144 : BattleMove {
	public Move144(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int ModifyDamage(int damageMult, Battler attacker, Battler opponent) {
		int t = Types.FLYING;
		if (t >= 0) {
			int mult = Types.GetCombinedEffectiveness(t, opponent.type1, opponent.type2, opponent.effects[Effects.Type3]);
			return (int)Math.Round((damageMult*mult)/8.0);
		}
		return damageMult;
	}

	public new bool TramplesMinimize(int param=1) {
		if (param==1 && Settings.USE_NEW_BATTLE_MECHANICS) {
			return true;
		}
		if (param == 2) {
			return true;
		}
		return false;
	}
}

/******************************************************************************
*  Target's moves become Electric-type for the rest of the round. (Electrify) *
******************************************************************************/
public class Move145 : BattleMove {
	public Move145(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (TypeImmunityByAbility(GetType(type, attacker, opponent), attacker, opponent)) {
			return -1;
		}
		if (opponent.effects[Effects.Electrify] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		if (battle.useMoveChoice[opponent.index]!=1 || battle.moveChoice[opponent.index] == null || battle.moveChoice[opponent.index].id <= 0 || opponent.HasMovedThisRound()) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		effects[Effects.Electrify] = 1;
		battle.Display(string.Format("{0} was electrified!",opponent.String()));
	}
}

/*************************************************************************
*  All Normal-type moves become Electric-type for the rest of the round. *
*  (Ion Deluge)                                                          *
*************************************************************************/
public class Move146 : BattleMove {
	public Move146(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		bool unmoved = false;
		for (int i=0; i<battle.battlers.Length; i++) 
		{
			Battler poke = battle.battlers[i];
			if (poke.index == attacker.index) {
				continue;
			}
			if (battle.useMoveChoice[poke.index] == 1 && !poke.HasMovedThisRound()) {
				unmoved = true;
				break;
			}
		}
		if (!unmoved || battle.field.effects[Effects.IonDeluge] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.field.effects[Effects.IonDeluge] = 1;
		battle.Display(string.Format("The Ion Deluge started!"));
		return 0;
	}
}

/***************************************
*  Always hits. (Hyperspace Hole)      *
*  TODO: Hits through various shields. *
***************************************/
public class Move147 : BattleMove {
	public Move147(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool AccuracyCheck(Battler attacker, Battler opponent) {
		return true;
	}
}

/********************************************************************************
*  Powders the foe. This round, if it uses a Fire move, it loses 1/4 of its max *
*  HP instead. (Powder)                                                         *
********************************************************************************/
public class Move148 : BattleMove {
	public Move148(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (opponent.effects[Effects.Powder] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		opponent.effects[Effects.Powder] = 1;
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		battle.Display(string.Format("{0} is covered in powder!",attacker.String()));
		return 0;
	}	
}

/****************************************************************************
*  This round, the user's side is unaffected by damaging moves. (Mat Block) *
****************************************************************************/
public class Move149 : BattleMove {
	public Move149(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool MoveFailed(Battler attacker, Battler opponent) {
		return attacker.turnCount > 1;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		attacker.OwnSide().effects[Effects.MatBlock] = 1;
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.Display(string.Format("{0} intends to flip up a mat and block incoming attacks!",attacker.String()));
		return 0;
	}
}

/*****************************************************************************
*  User's side is protected against status moves this round. (Crafty Shield) *
*****************************************************************************/
public class Move14A : BattleMove {
	public Move14A(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OwnSide().effects[Effects.CraftyShield]) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		bool unmoved = false;
		for (int i=0; i<battle.battlers.Length; i++) 
		{
			Battler poke = battle.battlers[i];
			if (poke.index == attacker.index) {
				continue;
			}
			if (battle.useMoveChoice[poke.index] == 1 && !poke.HasMovedThisRound()) {
				unmoved = true;
				break;
			}
		}
		if (!unmoved) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.OwnSide().effects[Effects.CraftyShield] = 1;
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display(string.Format("Crafty Shield protected your team!"));
		} else {
			battle.Display(string.Format("Crafty Shield protected the opposing team!"));
		}
		return 0;
	}
}

/********************************************************************************
*  User is protected against damaging moves this round. Decreases the Attack of *
*  the user of a stopped contact move by 2 stages. (King's Shield)              *
********************************************************************************/
public class Move14B : BattleMove {
	public Move14B(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.KingsShield] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		int[] ratesharers = new int[6]{0xAA,0xAB,0xAC,0xE8,0x14B,0x14C};
		if (Array.IndexOf(ratesharers, (new Moves.Move(attacker.lastMoveUsed)).Function()) > -1) {
			attacker.effects[Effects.ProtectRate] = 1;
		}
		bool unmoved = false;
		for (int i=0; i<battle.battlers.Length; i++) 
		{
			Battler poke = battle.battlers[i];
			if (poke.index == attacker.index) {
				continue;
			}
			if (battle.useMoveChoice[poke.index] == 1 && !poke.HasMovedThisRound()) {
				unmoved = true;
				break;
			}
		}
		if (!unmoved || (!Settings.USE_NEW_BATTLE_MECHANICS && battle.Rand(65536) >= (65536/attacker.effects[Effects.ProtectRate]))) {
			attacker.effects[Effects.ProtectRate] = 1;
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.KingsShield] = 1;
		attacker.effects[Effects.ProtectRate] *= 2;
		battle.Display(string.Format("{0} protected itself!",attacker.String()));
		return 0;
	}
}

/**********************************************************************************
*  User is protected against moves that target it this round. Damages the user of *
*  a stopped contact move by 1/8 of its max HP. (Spiky Shield)                    *
**********************************************************************************/
public class Move14C : BattleMove {
	public Move14C(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.effects[Effects.SpikyShield] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		int[] ratesharers = new int[6]{0xAA,0xAB,0xAC,0xE8,0x14B,0x14C};
		if (Array.IndexOf(ratesharers, (new Moves.Move(attacker.lastMoveUsed)).Function()) > -1) {
			attacker.effects[Effects.ProtectRate] = 1;
		}
		bool unmoved = false;
		for (int i=0; i<battle.battlers.Length; i++) 
		{
			Battler poke = battle.battlers[i];
			if (poke.index == attacker.index) {
				continue;
			}
			if (battle.useMoveChoice[poke.index] == 1 && !poke.HasMovedThisRound()) {
				unmoved = true;
				break;
			}
		}
		if (!unmoved || battle.Rand(65536) >= (65536/attacker.effects[Effects.ProtectRate])) {
			attacker.effects[Effects.ProtectRate] = 1;
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.effects[Effects.SpikyShield] = 1;
		attacker.effects[Effects.ProtectRate] *= 2;
		battle.Display(string.Format("{0} protected itself!",attacker.String()));
		return 0;
	}
}

/*******************************************************************************
*  Two turn attack. Skips first turn, attacks second turn. (Phantom Force)     *
*  Is invulnerable during use.                                                 *
*  Ignores target's Detect, King's Shield, Mat Block, Protect and Spiky Shield *
*  this round. If successful, negates them this round.                         *
*  Does double damage and has perfect accuracy if the target is Minimized.     *
*******************************************************************************/
public class Move14D : BattleMove {
	bool immediate;
	public Move14D(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} vanished immediately!",attacker.String()));
		}
		if (immediate) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack]>0) {
			return 0;
		}
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		opponent.effects[Effects.ProtectNegation] = 1;
		opponent.OwnSide().effects[Effects.CraftyShield] = 0;
		return ret;
	}

	public new bool TramplesMinimize(int param=1) {
		if (param==1 && Settings.USE_NEW_BATTLE_MECHANICS) {
			return true;
		}
		if (param == 2) {
			return true;
		}
		return false;
	}
}

/***************************************************************************
*  Two turn attack. Skips first turn, increases the user's Special Attack, *
*  Special Defense and Speed by 2 stages each second turn. (Geomancy)      *
***************************************************************************/
public class Move14E : BattleMove {
	bool immediate;
	public Move14E(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool TwoTurnAttack(Battler attacker) {
		immediate = false;
		if (!immediate && attacker.HasWorkingItem(Items.POWERHERB)) {
			immediate = true;
		}
		if (immediate) {
			return false;
		}
		return attacker.effects[Effects.TwoTurnAttack] == 0;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (immediate || attacker.effects[Effects.TwoTurnAttack] > 0) {
			ShowAnimation(id, attacker, opponent, 1, allTargets, showAnimation);
			battle.Display(string.Format("{0} is absorbing power!",attacker.String()));
		}
		if (immediate) {
			battle.CommonAnimation("UseItem", attacker, null);
			battle.Display(string.Format("{0} became fully charged due to its Power Herb!",attacker.String()));
			attacker.ConsumeItem();
		}
		if (attacker.effects[Effects.TwoTurnAttack]>0) {
			return 0;
		}
		if (!attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this) && !attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			battle.Display(string.Format("{0}'s stats won't go any higher!",attacker.String()));
			return -1;
		}
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		bool showAnim = true;
		if (attacker.CanIncreaseStatStage(Stats.SPATK, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPATK, 2, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPDEF, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPDEF, 2, attacker, false, this, showAnim);
			showAnim = false;
		}
		if (attacker.CanIncreaseStatStage(Stats.SPEED, attacker, false, this)) {
			attacker.IncreaseStat(Stats.SPEED, 2, attacker, false, this, showAnim);
			showAnim = false;
		}
		return 0;
	}
}

/*******************************************************************************
*  User gains 3/4 the HP it inflicts as damage. (Draining Kiss, Oblivion Wing) *
*******************************************************************************/
public class Move14F : BattleMove {
	public Move14F(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool IsHealingMove() {
		return Settings.USE_NEW_BATTLE_MECHANICS;
	}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0) {
			int hpGain = (int)Math.Round(opponent.damageState.HPLost*3.0/4);
			if (opponent.HasWorkingAbility(Abilities.LIQUIDOOZE)) {
				attacker.ReduceHP(hpGain, true);
				battle.Display(string.Format("{0} sucked up the liquid ooze!",attacker.String()));
			} else if (attacker.effects[Effects.HealBlock] == 0) {
				if (attacker.HasWorkingItem(Items.BIGROOT)) {
					hpGain = (int)(hpGain*1.3);
					attacker.ReduceHP(hpGain, true);
					battle.Display(string.Format("{0} had its energy drained!",opponent.String()));
				}
			}
		}
		return ret;
	}
}

/**************************************************************************
*  If this move KO's the target, increases the user's Attack by 2 stages. *
*  (Fell Stinger)                                                         *
**************************************************************************/
public class Move150 : BattleMove {
	public Move150(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = base.Effect(attacker, opponent, hitNum, allTargets, showAnimation);
		if (opponent.damageState.CalculatedDamage > 0 && opponent.Fainted()) {
			if (attacker.CanIncreaseStatStage(Stats.ATTACK, attacker, false, this)) {
				attacker.IncreaseStat(Stats.ATTACK, 2, attacker, false, this);
			}
		}
		return ret;
	}
}

/********************************************************************************
*  Decreases the target's Attack and Special Attack by 1 stage each. Then, user *
*  switches out. Ignores trapping moves. (Parting Shot)                         *
*  TODO: Pursuit should interrupt this move.                                    *
********************************************************************************/
public class Move151 : BattleMove {
	public Move151(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		int ret = -1;
		ShowAnimation(id, attacker, opponent, hitNum, allTargets, showAnimation);
		if (!IsSoundbased() || attacker.HasMoldBreaker() || !opponent.HasWorkingAbility(Abilities.SOUNDPROOF)) {
			bool showAnim = true;
			if (opponent.ReduceStat(Stats.ATTACK, 1, attacker, false, this, showAnim)) {
				showAnim = false;
				ret = 0;
			}
			if (opponent.ReduceStat(Stats.SPATK, 1, attacker, false, this, showAnim)) {
				showAnim = false;
				ret = 0;
			}
		}
		if (!attacker.Fainted() && battle.CanChooseNonActive(attacker.index) && !battle.AllFainted(battle.Party(opponent.index))) {
			attacker.effects[Effects.Uturn] = true;
			ret = 0;
		}
		return ret;
	}
}

/********************************************************************************
*  No PokÃ©mon can switch out or flee until the } of the next round, as long as *
*  the user remains active. (Fairy Lock)                                        *
********************************************************************************/
public class Move152 : BattleMove {
	public Move152(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (battle.field.effects[Effects.FairyLock] > 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.field.effects[Effects.FairyLock] = 2;
		battle.Display(string.Format("No one will be able to run away during the next turn!"));
		return 0;
	}
}

/***********************************************************************
*  Entry hazard. Lays stealth rocks on the opposing side. (Sticky Web) *
***********************************************************************/
public class Move153 : BattleMove {
	public Move153(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (attacker.OpposingSide().effects[Effects.StickyWeb] != 0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		attacker.OpposingSide().effects[Effects.StickyWeb] = 1;
		if (!battle.IsOpposing(attacker.index)) {
			battle.Display(string.Format("A sticky web has been laid out beneath the opposing team's feet!"));
		} else {
			battle.Display(string.Format("A sticky web has been laid out beneath your team's feet!"));
		}
		return 0;
	}
}

/**********************************************************************************
*  For 5 rounds, creates an electric terrain which boosts Electric-type moves and *
*  prevents PokÃ©mon from falling asleep. Affects non-airborne PokÃ©mon only.     *
*  (Electric Terrain)                                                             *
**********************************************************************************/
public class Move154 : BattleMove {
	public Move154(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (battle.field.effects[Effects.ElectricTerrain]>0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.field.effects[Effects.GrassyTerrain] = 0;
		battle.field.effects[Effects.MistyTerrain] = 0;
		battle.field.effects[Effects.ElectricTerrain] = 5;
		battle.Display(string.Format("An electric current runs across the battlefield!"));
		return 0;
	}
}

/**********************************************************************************
*  For 5 rounds, creates a grassy terrain which boosts Grass-type moves and heals *
*  PokÃ©mon at the } of each round. Affects non-airborne PokÃ©mon only.           *
*  (Grassy Terrain)                                                               *
**********************************************************************************/
public class Move155 : BattleMove {
	public Move155(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (battle.field.effects[Effects.GrassyTerrain]>0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.field.effects[Effects.GrassyTerrain] = 5;
		battle.field.effects[Effects.MistyTerrain] = 0;
		battle.field.effects[Effects.ElectricTerrain] = 0;
		battle.Display(string.Format("Grass grew to cover the battlefield!"));
		return 0;
	}
}

/*******************************************************************************
*  For 5 rounds, creates a misty terrain which weakens Dragon-type moves and   *
*  protects PokÃ©mon from status problems. Affects non-airborne PokÃ©mon only. *
*  (Misty Terrain)                                                             *
*******************************************************************************/
public class Move156 : BattleMove {
	public Move156(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (battle.field.effects[Effects.MistyTerrain]>0) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.field.effects[Effects.GrassyTerrain] = 0;
		battle.field.effects[Effects.MistyTerrain] = 5;
		battle.field.effects[Effects.ElectricTerrain] = 0;
		battle.Display(string.Format("Mist swirled about the battlefield!"));
		return 0;
	}
}

/**********************************************************************************
*  Doubles the prize money the player gets after winning the battle. (Happy Hour) *
**********************************************************************************/
public class Move157 : BattleMove {
	public Move157(Battle battle, Moves.Move move) : base(battle, move) {}

	public new int Effect(Battler attacker, Battler opponent, int hitNum=0, List<Battler> allTargets=null, bool showAnimation=true) {
		if (battle.IsOpposing(attacker.index) || battle.doubleMoney) {
			battle.Display(string.Format("But it failed!"));
			return -1;
		}
		ShowAnimation(id, attacker, null, hitNum, allTargets, showAnimation);
		battle.doubleMoney = true;
		battle.Display(string.Format("Everyone is caught up in the happy atmosphere!"));
		return 0;
	}
}

/*****************************************************************
*  Fails unless user has consumed a berry at some point. (Belch) *
*****************************************************************/
public class Move158 : BattleMove {
	public Move158(Battle battle, Moves.Move move) : base(battle, move) {}

	public new bool MoveFailed(Battler attacker, Battler opponent) {
		return attacker.pokemon == null || !attacker.pokemon.belch;
	}
}
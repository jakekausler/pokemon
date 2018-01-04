public class NullBattlePeer : BattlePeer {
	public NullBattlePeer() {

	}

	public override void OnEnteringBattle(Battle battle, Pokemon pokemon) {

	}

	public override int StorePokemon(BattleTrainer player, Pokemon pokemon) {
		return 0;
	}

	public override string GetStorageCreator() {
		return "";
	}

	public override int CurrentBox() {
		return 0;
	}

	public override string BoxName(int box) {
		return "";
	}
}

public class RealBattlePeer : BattlePeer {
	public RealBattlePeer() {

	}

	public override void OnEnteringBattle(Battle battle, Pokemon pokemon) {

	}

	public override int StorePokemon(BattleTrainer player, Pokemon pokemon) {
		return 0;
	}

	public override string GetStorageCreator() {
		return "";
	}

	public override int CurrentBox() {
		return 0;
	}

	public override string BoxName(int box) {
		return "";
	}
}

public abstract class BattlePeer {
	public abstract void OnEnteringBattle(Battle battle, Pokemon pokemon);
	public abstract int StorePokemon(BattleTrainer player, Pokemon pokemon);
	public abstract string GetStorageCreator();
	public abstract int CurrentBox();
	public abstract string BoxName(int box);
}
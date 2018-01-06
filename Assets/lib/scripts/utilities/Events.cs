public class Events {
	public static Event OnMapCreate = new Event();
	public static Event OnMapUpdate = new Event();
	public static Event OnMapChange = new Event();
	public static Event OnMapChanging = new Event();
	public static Event OnMapSceneChange = new Event();
	public static Event OnSpritesetCreate = new Event();
	public static Event OnAction = new Event();
	public static Event OnStepTaken = new Event();
	public static Event OnLeaveTile = new Event();
	public static Event OnStepTakenFieldMovement = new Event();
	public static Event OnStepTakenTransferPossible = new Event();
	public static Event OnStartBattle = new Event();
	public static Event OnEndBattle = new Event();
	public static Event OnWildPokemonCreate = new Event();
	public static Event OnWildBattleOverride = new Event();
	public static Event OnWildBattleEnd = new Event();
	public static Event OnTrainerPartyLoad = new Event();

	public static void Add(Event e, Action a) {
		// TODO
	}
}

public class Event {
	public Action action;
	public Event() {
		// TODO
	}
}
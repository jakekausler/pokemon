using System;
using System.Collections.Generic;

public class PokemonPartyScene {
	
	public PokemonPartyScene() {
		// TODO
	}

	public void StartScene(Pokemon[] party, string startHelpText, string[] annotations=null, bool multiSelect=false) {
		// TODO
	}

	public void EndScene() {
		// TODO
	}

	public void Display(string msg) {
		// TODO
	}

	public int DisplayConfirm(string msg) {
		// TODO
		return 0;
	}

	public int ShowCommands(string helpText, string[] commands, int index=0) {
		// TODO
		return 0;
	}

	public string MessageFreeText(string text, string startMsg, int maxLength) {
		// TODO
		return "";
	}

	public void SetHelpText(string helpText) {
		// TODO
	}

	public void Annotate(string[] annot) {
		// TODO
	}

	public void Select(int item) {
		// TODO
	}

	public void PreSelect(int item) {
		// TODO
	}

	public void SwitchBegin(int oldid, int newid) {
		// TODO
	}

	public void SwitchEnd(int oldid, int newid) {
		// TODO
	}

	public void ClearSwitching() {
		// TODO
	}

	public void Summary(int pkmnId) {
		// TODO
	}

	public int ChooseItem(PokemonBag bag) {
		// TODO
		return 0;
	}

	public int UseItem(PokemonBag bag, Pokemon pokemon) {
		// TODO
		return 0;
	}

	public int ChoosePokemon(bool switching=false, int initialSel=-1, int canSwitch=0) {
		// TODO
		return 0;
	}

	public int ChangeSelection(int key, int currentSel) {
		// TODO
		return 0;
	}

	public void HardRefresh() {
		// TODO
	}

	public void Refresh() {
		// TODO
	}

	public void RefreshSingle(int i) {
		// TODO
	}

	public void Update() {
		// TODO
	}

	public class PartyScreen {

		public PartyScreen(PokemonPartyScene scene, Pokemon[] party) {
			// TODO
		}

		public void StartScene(string helpText, bool doublebattle, string[] annotations=null) {
			// TODO
		}

		public int ChoosePokemon(string helpText = "") {
			// TODO
			return 0;
		}

		public int PokemonGiveScreen(int item) {
			// TODO
			return 0;
		}

		public void PokemonGiveMailScreen(int mailIndex) {
			// TODO
		}

		public void EndScene() {
			// TODO
		}

		public void Update() {
			// TODO
		}

		public void HardRefresh() {
			// TODO
		}

		public void Refresh() {
			// TODO
		}

		public void RefreshSingle(int i) {
			// TODO
		}

		public void Display(string msg) {
			// TODO
		}

		public bool Confirm(string msg) {
			// TODO
			return false;
		}

		public void ShowCommands(string helpText, string[] commands, int index=0) {
			// TODO
		}

		public bool CheckSpecies(Pokemon[] arr) {
			// TODO
			return false;
		}

		public bool CheckItems(Pokemon[] arr) {
			// TODO
			return false;
		}

		public void Switch(int oldid, int newid) {
			// TODO
		}

		public int ChooseMove(Pokemon pokemon, string helpText, int index=0) {
			// TODO
			return 0;
		}

		public void RefreshAnnotations(Func<bool> ableProc) {
			// TODO
		}

		public void ClearAnnotations() {
			// TODO
		}

		public List<Pokemon> PokemonMultipleEntryScreenEx(Dictionary<int, int> ruleSet) {
			// TODO
			return null;
		}

		public int ChooseAblePokemon(Func<bool> ableProc, bool allowIneligible=false) {
			// TODO
			return 0;
		}

		public int ChooseTradablePokemon(Func<bool> ableProc, bool allowIneligible=false) {
			// TODO
			return 0;
		}

		public Tuple<Pokemon, int> PokemonScreen() {
			// TODO
			return null;
		}

	}
}
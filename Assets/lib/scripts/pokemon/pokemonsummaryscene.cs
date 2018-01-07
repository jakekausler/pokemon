public class PokemonSummaryScene {

	public PokemonSummaryScene() {
		// TODO
	}

	public void Update() {
		// TODO
	}

	public void StartScreen(Pokemon[] party, int partyIndex) {
		// TODO
	}

	public void StartForgetScreen(Pokemon[] party, int partyIndex, int moveToLearn) {
		// TODO
	}

	public void EndScene() {
		// TODO
	}

	public void Display(string msg) {
		// TODO
	}

	public int Confirm(string msg) {
		// TODO
		return 0;
	}

	public int ShowCommands(string[] commands, int index=0) {
		// TODO
		return 0;
	}

	public void DrawMarkings(Sprite bitmap, int x, int y) {
		// TODO
	}

	public void DrawPage(int page) {
		// TODO
	}

	public void DrawPageOne() {
		// TODO
	}

	public void DrawPageOneEgg() {
		// TODO
	}

	public void DrawPageTwo() {
		// TODO
	}

	public void DrawPageThree() {
		// TODO
	}

	public void DrawPageFour() {
		// TODO
	}

	public void DrawSelectedMove(int moveToLearn, int moveId) {
		// TODO
	}

	public void DrawMoveSelection(int moveToLearn) {
		// TODO
	}

	public void DrawPageFive() {
		// TODO
	}

	public void DrawSelectedRibbon(int ribbonId) {
		// TODO
	}

	public void GoToPrevious() {
		// TODO
	}

	public void GoToNext() {
		// TODO
	}

	public void MoveSelection() {
		// TODO
	}

	public void RibbonSelection() {
		// TODO
	}

	public void Marking(Pokemon pokemon) {
		// TODO
	}

	public void Options() {
		// TODO
	}

	public int ChooseMoveToForget(int moveToLearn) {
		// TODO
		return 0;
	}

	public void Scene() {
		// TODO
	}

	public class SummaryScreen {

		public SummaryScreen(PokemonSummaryScene scene) {
			// TODO
		}

		public int StartScreen(Pokemon[] party, int partyIndex) {
			// TODO
			return 0;
		}

		public int StartForgetScreen(Pokemon[] party, int partyIndex, int moveToLearn) {
			// TODO
			return 0;
		}

		public int StartChooseMoveScreen(Pokemon[] party, int partyIndex, string message) {
			// TODO
			return 0;
		}
	}
}
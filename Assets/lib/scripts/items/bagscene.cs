using System;
using System.Collections.Generic;

public class BagScene {

	public static Color ITEMLISTBASECOLOR = new Color(88,88,80);
	public static Color ITEMLISTSHADOWCOLOR = new Color(168,184,184);
	public static Color ITEMTEXTBASECOLOR = new Color(248,248,248);
	public static Color ITEMTEXTSHADOWCOLOR = new Color(0,0,0);
	public static Color POCKETNAMEBASECOLOR = new Color(88,88,80);
	public static Color POCKETNAMESHADOWCOLOR = new Color(168,184,184);
	public static int ITEMSVISIBLE = 7;
	
	public BagScene(PokemonBag bag) {
		// TODO
	}

	public void Update() {
		// TODO
	}

	public void StartScene(PokemonBag bag, bool choosing, Func<int, bool> filterProc=null, bool resetPocket=true) {
		// TODO
	}

	public void EndScene() {
		// TODO
	}

	public void Display(string msg, bool brief=false) {
		// TODO
	}

	public void Confirm(string msg) {
		// TODO
	}

	public int ChooseNumber(string helpText, int maximum, int initNum=1) {
		// TODO
		return 0;
	}

	public int ChooseNumber(string helpText, string[] commands, int initNum=1) {
		// TODO
		return 0;
	}

	public void Refresh() {
		// TODO
	}

	public void RefreshIndexChanged() {
		// TODO
	}

	public void RefreshFilter() {
		// TODO
	}

	public int ChooseItem() {
		// TODO
		return 0;
	}

	public class BagScreen {

		public BagScreen(BagScene scene, PokemonBag bag) {
			// TODO
		}

		public void StartScreen() {
			// TODO
		}

		public void Display(string msg) {
			// TODO
		}

		public bool Confirm(string msg) {
			// TODO
			return true;
		}

		public int ChooseItemScreen(Func<int, bool> proc=null) {
			// TODO
			return 0;
		}

		public void WithdrawItemScreen() {
			// TODO
		}

		public void DepositItemScreen() {
			// TODO
		}

		public void TossItemScreen() {
			// TODO
		}

	}
}
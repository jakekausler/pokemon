public class Window {
	public int width;
	public int height;
	public Color baseColor;
	public string text;
	public bool visible;
	public Color shadowColor;
	public int opacity;
	public int x;
	public int y;
	public Viewport viewport;
	public Window() {
		// TODO
	}

	public void Update() {
		// TODO
	}

	public bool Pausing() {
		// TODO
		return false;
	}

	public bool Busy() {
		// TODO
		return false;
	}

	public void Resume() {
		// TODO
	}

	public void Dispose() {
		// TODO
	}

	public class CommandPokemon : Window {
		public int index;
		public CommandPokemon(string[] commands) {
			// TODO
		}
	}
}
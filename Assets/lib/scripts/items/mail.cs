public class Mail {
	public int item;
	public string message;
	public string sender;
	public MailPokemon poke1;
	public MailPokemon poke2;
	public MailPokemon poke3;
	public Mail(int item, string msg, string sender, Pokemon poke1, Pokemon poke2, Pokemon poke3) {
		this.item = item;
		this.message = msg;
		this.sender = sender;
		this.poke1 = new MailPokemon(poke1.species, poke1.Gender(), poke1.IsShiny(), poke1.GetForm(), poke1.IsEgg());
		this.poke2 = new MailPokemon(poke2.species, poke2.Gender(), poke2.IsShiny(), poke2.GetForm(), poke2.IsEgg());
		this.poke3 = new MailPokemon(poke3.species, poke3.Gender(), poke3.IsShiny(), poke3.GetForm(), poke3.IsEgg());
	}

	public static bool MoveToMailbox(Pokemon pokemon) {
		if (PokemonGlobal.Mailbox == null) {
			PokemonGlobal.Mailbox = new List<Mail>();
		}
		if (PokemonGlobal.Mailbox.Count >= Settings.MAILBOX_SIZE) {
			return false;
		}
		if (pokemon.mail == null) {
			return false;
		}
		PokemonGlobal.Mailbox.Add(pokemon.mail);
		pokemon.mail = null;
		return true;
	}

	public static void StoreMail(Pokemon pkmn, int item, string message, Pokemon poke1, Pokemon poke2, Pokemon poke3) {
		if (pkmn.mail != null) {
			throw new Exception("Pokémon already has mail");
		}
		pkmn.mail = new Mail(item, message, PokemonGlobal.Trainer.name, poke1, poke2, poke3);
	}

	public static void DisplayMail(Mail mail) {
		// TODO
	}

	public static bool WriteMail(int item, Pokemon pkmn, int pkmnId, BattleScene scene) {
		string message = "";
		while (true) {
			message = Messaging.MessageFreeText("Please enter a message (max. 256 characters).", false, 256, Graphics.width, new Action() {
				scene.Update();
			});
			if (message != "") {
				Pokemon poke1 = null;
				Pokemon poke2 = null;
				Pokemon poke3 = null;
				if (pkmnId+2 < 6 && PokemonGlobal.Trainer.party[pkmnId+2] != null) {
					poke1 = PokemonGlobal.Trainer.party[pkmnId+2];
				}
				if (pkmnID + 1 < 6 && PokemonGlobal.Trainer.party[pkmnId+1] != null) {
					poke2 = PokemonGlobal.Trainer.party[pkmnId+1];
				}
				poke3 = PokemonGlobal.Trainer.party[pkmnId];
				StoreMail(pkmn, item, message, poke1, poke2, poke3);
				return true;
			} else {
				if (scene.Confirm("Stop giving the Pokémon Mail?")) {
					return false;
				}
			}
		}
	}

	public class MailPokemon {
		public int species;
		public int gender;
		public bool shininess;
		public int form;
		public bool egg;
		public MailPokemon(int species, int gender, bool shininess, int form, bool egg) {
			this.species = species;
			this.gender = gender;
			this.shininess = shininess;
			this.form = form;
			this.egg = egg;
		}
	}
}
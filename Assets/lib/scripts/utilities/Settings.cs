public static class Settings {
	public static bool DEBUG = true;
	/**
	 - Default screen width
	 - Default screen height
	**/
	public static int DEFAULT_SCREEN_WIDTH = 512;
	public static int DEFAULT_SCREEN_HEIGHT = 384;

	/**
	 - Maximum level a pokemon can reach
	 - The level of a newly hatched Pokemon
	 - The odds (out of 65536) of a newly generated Pokemon being shiny
	 - The odds (out of  65536) of a wild Pokemon or bred egg having Pokerus
	**/
	public static int MAXIMUM_LEVEL = 100;
	public static int EGG_INITIAL_LEVEL = 1;
	public static int SHINY_POKEMON_CHANCE = 8;
	public static int POKERUS_CHANCE = 3;

	/**
	 - Whether a poisoned pokemon will lose hp while walking in the field
	 - Whether a poisoned pokemon will faint while walking in the field
	 - Whether fishing automatically hooks the pokemon (there is a reaction
	   test on false)
	 - Whether the player can surface from anywhere while diving (true) or only
	   in spots where they could dive down from above (false)
	 - Whether planted berries grow according to Gen 4 (true) or Gen 3 (false)
	   mechanics
	 - Whether TMs can be used infinitely
	**/
	public static bool POISON_IN_FIELD = true;
	public static bool POISON_FAINT_IN_FIELD = false;
	public static bool FISHING_AUTO_HOOK = false;
	public static bool DIVING_SURFACE_ANYWHERE = false;
	public static bool NEW_BERRY_PLANTS = true;
	public static bool INFINITE_TMS = true;

	/**
	 - Pairs of map IDs where the location signpost is not shown when moving
	   from one to the other. Useful for long routes spread over multiple maps.
	   For example [4,5,16,17] will be pairs 4,5 and 16,17.
	   Map pairs with the same names will not show the signpost anyway, so
	   listing them is unnecessary
	**/
	public static int[] NO_SIGN_POSTS = {};

	/**
	 - Whether a move's physical/special category depends on the move itself as
       in newer Gens (true), or on its type as in older Gens (false).
     - Whether the battle mechanics mimic Gen 6 (true) or Gen 5 (false).
     - Whether the Exp gained from beating a PokÃ©mon should be scaled
       depending on the gainer's level as in Gen 5 (true), or not as in other
       Gens (false).
	 - Whether the Exp gained from beating a PokÃ©mon should be divided equally
       between each participant (false), or whether each participant should
	   gain that much Exp. This also applies to Exp gained via the Exp Share
	   (held item version) being distributed to all Exp Share holders. This is
	   true in Gen 6 and false otherwise.
	 - Whether the critical capture mechanic applies (true) or not (false).
	   Note that it is based on a total of 600+ species (i.e. that many species
	   need to be caught to provide the greatest critical capture chance of
	   2.5x), and there may be fewer species in your game.
	 - Whether PokÃ©mon gain Exp for capturing a PokÃ©mon (true) or not (false)
	 - An array of item IDs which act as Mega Rings for the player (NPCs don't
	   need a Mega Ring item, just a Mega Stone).
	**/
	public static bool USE_MOVE_CATEGORY = true;
	public static bool USE_NEW_BATTLE_MECHANICS = true;
	public static bool USE_SCALED_EXP_FORMULA = true;
	public static bool NO_SPLIT_EXP = true;
	public static bool USE_CRITICAL_CAPTURE = true;
	public static bool GAIN_EXP_FOR_CAPTURE = true;
	public static int[] MEGA_RINGS = {};

	/**
	 - The minimum number of badges required to boost each stat of a player's
	   Pokemon by 1.1x, while using moves in battle only.
	 - Whether the badge restriction on using certain hidden moves is either
	   owning at least a certain number of badges (true), or owning a
	   particular badge (false).
	 - Depending on HIDDEN_MOVES_COUNT_BADGES, either the number of badges
	   required to use each hidden move, or the specific badge number required
	   to use each move. Remember that badge 0 is the first badge, badge 1 is
	   the second badge, etc.
	   e.g. To require the second badge, put false and 1.
	   e.g. To require at least 2 badges, put true and 2.
	**/
	public static int BADGES_BOOST_ATTACK      = 1;
	public static int BADGES_BOOST_DEFENSE     = 5;
	public static int BADGES_BOOST_SPEED       = 3;
	public static int BADGES_BOOST_SPATK       = 7;
	public static int BADGES_BOOST_SPDEF       = 7;
	public static bool HIDDEN_MOVES_COUNT_BADGES = true;
	public static int BADGE_FOR_CUT            = 1;
	public static int BADGE_FOR_FLASH          = 2;
	public static int BADGE_FOR_ROCKSMASH      = 3;
	public static int BADGE_FOR_SURF           = 4;
	public static int BADGE_FOR_FLY            = 5;
	public static int BADGE_FOR_STRENGTH       = 6;
	public static int BADGE_FOR_DIVE           = 7;
	public static int BADGE_FOR_WATERFALL      = 8;

	/**
	 - The names of each pocket of the Bag. Leave the first entry blank.
	 - The maximum number of slots per pocket (-1 means infinite number).
	   Ignore the first number (0).
	 - The maximum number of items each slot in the Bag can hold.
	 - Whether each pocket in turn auto-sorts itself by item ID number. Ignore
	   the first entry (the 0).
	**/
	public static string[] POCKET_NAMES = {
			"",
			"Items",
			"Medicine",
			"PokÃ© Balls",
			"TMs",
			"Berries",
			"Mail",
			"Battle Items",
			"Key Items"
		};
	public static int[] MAX_POCKET_SIZE = {
		0,
		-1,
		-1,
		-1,
		-1,
		-1,
		-1,
		-1,
		-1
	};
	public static int BAG_MAX_PER_SLOT = 999;
	public static bool[] POCKET_AUTO_SORT = {
		false,
		false,
		false,
		false,
		true,
		true,
		false,
		false,
		false
	};

	/**
	 - The name of the person who created the Pokemon storage system
	 - The number of boxes in the storage system
	**/
	public static string STORAGE_CREATOR = "Bill";
	public const int STORAGE_BOXES = 100;

	/**
	 - Whether the PokÃ©dex list shown is the one for the player's current
	   region (true), or whether a menu pops up for the player to manually
	   choose which Dex list to view when appropriate (false).
	 - The names of each Dex list in the game, in order and with National Dex
	   at the end. This is also the order that $PokemonGlobal.pokedexUnlocked
	   is in, which records which Dexes have been unlocked (first is unlocked
	   by default).
	   You can define which region a particular Dex list is linked to. This
	   means the area map shown while viewing that Dex list will ALWAYS be that
	   of the defined region, rather than whichever region the player is
	   currently in. To define this, put the Dex name and the region number in
	   an array, like the Kanto and Johto Dexes are. The National Dex isn't in
	   an array with a region number, therefore its area map is whichever
	   region the player is currently in.
	 - Whether all forms of a given species will be immediately available to
	   view in the PokÃ©dex so long as that species has been seen at all (true)
	   or whether each form needs to be seen specifically before that form
	   appears in the PokÃ©dex (false).
	 - An array of numbers, where each number is that of a Dex list (National
	   Dex is -1). All Dex lists included here have the species numbers in them
	   reduced by 1, thus making the first listed species have a species number
	   of 0 (e.g. Victini in Unova's Dex).
	**/
	public static bool DEX_DEPENDS_ON_LOCATION = false;
	public static string[] DEX_NAMES = {
		"Kanto PokÃ©dex",
		"Johto PokÃ©dex",
		"National PokÃ©dex"
	};
	public static bool ALWAYS_SHOW_ALL_FORMS = false;
	public static int[] DEX_INDEX_OFFSETS = {};

	/**
	 - The amount of money the player starts the game with.
	 - The maximum amount of money the player can have.
	 - The maximum number of Game Corner coins the player can have.
	 - The maximum length, in characters, that the player's name can be.
	**/
	public static int INITIAL_MONEY = 3000;
	public static int MAX_MONEY = 999999999;
	public static int MAX_COINS = 99999;
	public static int PLAYER_NAME_LIMIT = 10;

	/**
	 - A set of arrays, each containing a trainer type id and a global variable
	   number. If the variable is not set to "", then all trainers with the
	   associated trainer type id will be named whatever is in the variable.
	   For example [[24, 10],[12,30]] will name all trainers of type 24 to the
	   value stored in global variable 10, and all trainers of type 12 to the
	   value stored in global variable 30.
	**/
	public static int[][] RIVAL_NAMES = {};

	/**
	 - A list of map ids used by roaming Pokemon. Each map can lead to a number of
	   other maps. The first value in each array can lead to any of the other
	   values in the array.
	   For example, [[1,2,3],[2,1,3]] means that a Pokemon on map 1 can
	   go to map 2 or 3, and one on map 2 can go to map 1 or 3.
	 - An array of arrays of roaming types. The first value is the species
	   number, the second is the level, the third is the global switch (the
	   encounter is active when this is on), the fourth is the encounter type
	   (see Field_RoamingPokemon for the list), and the fifth is the id of the
	   background music to play (-1 means no specific).
	   For example, [[151,30,53,1,0]] represents one roaming Mew at level 30,
	   active when switch 53 is active, found while walking in grass or caves,
	   with background music 0 playing.
	 - A list of map ids used by specific roaming Pokemon. Each map can lead to
	   a number of other maps. The first value in each array is the global
	   switch that represents this encounter being active. The second value in
	   each array can lead to any of the following values in the array.
	   For example, using the above example, [[53,4,5,6],[53,5,4,6]] means that
	   the mew encounter will not use the ROAMING_AREAS array, but instead will
	   use a different set of roaming areas. In this case, 4 can lead to 5 or
	   6, and 5 can lead to 4 or 6.
	**/
	public static int[][] ROAMING_AREAS = {};
	public static int[][] ROAMING_POKEMON = {};
	public static int[][] ROAMING_SPECIFIC_AREAS = {};

	/**
	 - A set of arrays containing details of a wild encounter that can only
	   occur using the Poke Radar. The first value is the map id on which the
	   encounter can occur, the second value is the probability that this
	   encounter will occur (out of 100), the third is the species number of
	   the Pokemon, the fourth is the minimum possible level, and the fifth is
	   the maximum possible level.
	**/
	public static int[][] POKE_RADAR_EXCLUSIVES = {};

	/**
	 - A set of arrays containing details of a graphic to be shown on the town
	   map if appropriate. The first value is the region id of the town map to
	   use, the second is the global switch (on=graphic will show), the third
	   is the x coordinate to show the graphic (in squares), the fourth is the
	   y coordinate to show the graphic (in squares), and the fifth is the
	   graphic id.
	**/
	public static int[][] REGION_MAP_EXTRAS = {};

	/**
	 - The number of steps allowed before a Safari Zone game ends (0 = infinte)
	 - The number of seconds a Bug Catching Contest lasts for (0 = infinte)
	**/
	public static int SAFARI_STEPS = 600;
	public static int BUG_CONTEST_TIME = 1200;

	/**
	 - The Global Switch that is set to ON when the player whites out.
	 - The Global Switch that is set to ON when the player has seen PokÃ©rus in
	   the PokÃ© Center, and doesn't need to be told about it again.
	 - The Global Switch which, while ON, makes all wild PokÃ©mon created be
	   shiny.
	 - The Global Switch which, while ON, makes all PokÃ©mon created considered
	   to be met via a fateful encounter.
	 - The Global Switch which determines whether the player will lose money if
	   they lose a battle (they can still gain money from trainers for winning)
	 - The Global Switch which, while ON, prevents all PokÃ©mon in battle from
	   Mega Evolving even if they otherwise could.
	**/
	public static int STARTING_OVER_SWITCH = 1;
	public static int SEEN_POKERUS_SWITCH = 2;
	public static int SHINY_WILD_POKEMON_SWITCH = 31;
	public static int FATEFUL_ENCOUNTER_SWITCH = 32;
	public static int NO_MONEY_LOSS = 33;
	public static int NO_MEGA_EVOLUTION = 34;

	/**
	 - The ID of the animation played when the player steps on grass (shows
	   grass rustling).
	 - The ID of the animation played when the player lands on the ground after
	   hopping over a ledge (shows a dust impact).
	 - The ID of the animation played when a trainer notices the player (an
	   exclamation bubble).
	 - The ID of the animation played when a patch of grass rustles due to
	   using the PokÃ© Radar.
	 - The ID of the animation played when a patch of grass rustles vigorously
	   due to using the PokÃ© Radar. (Rarer species)
	 - The ID of the animation played when a patch of grass rustles and shines
	   due to using the PokÃ© Radar. (Shiny encounter)
	 - The ID of the animation played when a berry tree grows a stage while the
	   player is on the map (for new plant growth mechanics only).
	**/
	public static int GRASS_ANIMATION_ID = 1;
	public static int DUST_ANIMATION_ID = 2;
	public static int EXCLAMATION_ANIMATION_ID = 3;
	public static int RUSTLE_NORMAL_ANIMATION_ID = 1;
	public static int RUSTLE_VIGOROUS_ANIMATION_ID = 5;
	public static int RUSTLE_SHINY_ANIMATION_ID = 6;
	public static int PLANT_SPARKLE_ANIMATION_ID = 7;
}

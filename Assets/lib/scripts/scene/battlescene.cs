/***********************************************************************************
* -  def pbChooseNewEnemy(index,party)
* Use this method to choose a new PokÃ©mon for the enemy
* The enemy's party is guaranteed to have at least one 
* choosable member.
* index - Index to the battler to be replaced (use e.g. @battle.battlers[index] to 
* access the battler)
* party - Enemy's party
* 
* - def pbWildBattleSuccess
* This method is called when the player wins a wild PokÃ©mon battle.
* This method can change the battle's music for example.
* 
* - def pbTrainerBattleSuccess
* This method is called when the player wins a Trainer battle.
* This method can change the battle's music for example.
* 
* - def pbFainted(pkmn)
* This method is called whenever a PokÃ©mon faints.
* pkmn - PokeBattle_Battler object indicating the PokÃ©mon that fainted
* 
* - def pbChooseEnemyCommand(index)
* Use this method to choose a command for the enemy.
* index - Index of enemy battler (use e.g. @battle.battlers[index] to 
* access the battler)
* 
* - def pbCommandMenu(index)
* Use this method to display the list of commands and choose
* a command for the player.
* index - Index of battler (use e.g. @battle.battlers[index] to 
* access the battler)
* Return values:
* 0 - Fight
* 1 - PokÃ©mon
* 2 - Bag
* 3 - Run
***********************************************************************************/

/****************************************
* Command menu (Fight/PokÃ©mon/Bag/Run) *
****************************************/
public static class BattleSceneConstraints {

}

public class CommandMenuDisplay {

}

public class CommandMenuButtons {

}

/*****************************
* Fight Menu (Choose a Move) *
*****************************/
public class FightMenuDisplay {

}

public class FightMenuButtons {

}

/******************************
* Data box for safari battles *
******************************/
public class SafariDataBox {

}

/*****************************************************
* Data box for regular battles (singles and doubles) *
*****************************************************/
public class PokemonDataBox {
	
}



/*********************************************************************************
* Shows the enemy trainer(s)'s PokÃ©mon being thrown out.  It appears at coords  *
* (@spritex,@spritey), and moves in y to @endspritey where it stays for the rest *
* of the battle, i.e. the latter is the more important value.                    *
* Doesn't show the ball itself being thrown.                                     *
*********************************************************************************/
public class PokeballSendOutAnimation {

}

/*********************************************************************************
* Shows the player's (or partner's) PokÃ©mon being thrown out.  It appears at    *
* (@spritex,@spritey), and moves in y to @endspritey where it stays for the rest *
* of the battle, i.e. the latter is the more important value.                    *
* Doesn't show the ball itself being thrown.                                     *
*********************************************************************************/
public class PokeballPlayerSendOutAnimation {

}

/****************************************************************************
* Shows the enemy trainer(s) and the enemy party lineup sliding off screen. *
* Doesn't show the ball thrown or the PokÃ©mon.                             *
****************************************************************************/
public class TrainerFadeAnimation {

}

/*********************************************************************************
* Shows the player (and partner) and the player party lineup sliding off screen. *
* Shows the player's/partner's throwing animation (if they have one).            *
# Doesn't show the ball thrown or the PokÃ©mon.                                  *
*********************************************************************************/
public class PlayerFadeAnimation {

}

/*******************************************
* Battle scene (the visuals of the battle) *
*******************************************/
public class BattleScene {
	public bool abortable;
	public const int BLANK = 0;
	public const int MESSAGEBOX = 1;
	public const int COMMANDBOX = 2;
	public const int FIGHTBOX = 3;

	public BattleScene() {

	}

	public void Update() {

	}

	public void GraphicsUpdate() {

	}

	public void InputUpdate() {

	}

	public void ShowWindow(int windowType) {

	}

	public void SetMessageMode(int mode) {

	}

	public void WaitMessage() {

	}

	public void Display(string msg, bool brief=false) {

	}

	public void DisplayMessage(string msg, bool brief=false) {

	}

	public void DisplayPausedMessage(string msg) {

	}

	public bool DisplayConfirmMessage(string msg) {
		return true;
	}

	public int ShowCommands(string msg, string[] commands, int defaultValue) {
		return 0;
	}

	public void FrameUpdate(CommandWindow cw=null) {

	}

	public void Refresh() {

	}

	public Sprite AddSprite(int id, float x, float y, string filename) {
		return null;
	}

	public Sprite AddPlane(int id, string filename) {
		return null;
	}

	public void DisposeSprites() {

	}

	public void BeginCommandPhase() {

	}

	public void ShowOpponent(int index) {

	}

	public void HideOpponent() {

	}

	public void ShowHelp(string text) {

	}

	public void HideHelp() {

	}

	public void Backdrop() {

	}

	public bool InPartyAnimation() {
		return true;
	}

	public void PartyAnimationUpdate() {

	}

	public void StartBattle(Battle battle) {

	}

	public void EndBattle(int result) {

	}

	public void Recall(int battlerIndex) {

	}

	public void TrainerSendOut(int battlerIndex, Pokemon pkmn) {

	}

	public void SendOut(int battlerIndex, Pokemon pkmn) {

	}

	public void TrainerWithdraw(Battle battle, Pokemon pkmn) {

	}

	public void Withdraw(Battle battle, Pokemon pkmn) {

	}

	public string MoveString(BattleMove move) {
		return "";
	}

	public void BeginAttackPhase() {

	}

	public void SafariStart() {

	}

	public void ResetCommandIndices() {

	}

	public void ResetMoveIndex(int index) {

	}

	public void SafariCommandMenu(int index) {

	}

	public int CommandMenu(int index) {
		return 0;
	}

	public int CommandMenuEx(int index) {
		return 0;
	}

	public int FightMenu(int index) {
		return 0;
	}

	public int[] ItemMenu(int index) {
		return null;
	}

	public int ForgetMove(Pokemon pokemon, int moveToLearn) {
		return 0;
	}

	public int ChooseMove(Pokemon pokemon, string message) {
		return 0;
	}

	public string NameEntry(string helpText, Pokemon pokemon) {
		return "";
	}

	public void SelectBattler(int index, int selectMode=1) {

	}

	public int FirstTarget(int index, int targetType) {
		return 0;
	}

	public int UpdateSelected(int index) {
		return 0;
	}

	public int ChooseTarget(int index, int targetType) {
		return 0;
	}

	public int Switch(int index, bool lax, bool canCancel) {
		return 0;
	}

	public void DamageAnimation(Battler pkmn, int effectiveness) {

	}

	public void HPChanged(Battler pkmn, int oldHP, bool anim=false) {

	}

	public void Fainted(Battler pkmn) {

	}

	public void ChooseEnemyCommand(int index) {

	}

	public int ChooseNewEnemy(int index, Battler[] party) {
		return 0;
	}

	public void WildBattleSuccess() {

	}

	public void TrainerBattleSuccess() {

	}

	public void EXPBar(Pokemon pokemon, Battler battler, int startExp, int endExp, int tmpExp1, int tmpExp2) {

	}

	public void ShowPokedex(int species) {

	}

	public string ChangeSpecies(Battler attacker, int species) {
		return "";
	}

	public void ChangePokemon(Battler attacker, Pokemon pokemon) {

	}

	public void SaveShadows() {

	}

	public void FindAnimation(int moveId, int userIndex, int hitNum) {

	}

	public void CommonAnimation(string animName, Battler user, Battler target, int hitNum=0) {

	}

	public void Animation(int moveId, Battler user, Battler target, int hitNum=0) {

	}

	public void AnimationCore(Anim animation, Battler user, Battler target, bool oppMove=false) {

	}

	public void LevelUp(Pokemon pokemon, Battler battler, int oldTotalHP, int oldAttack, int oldDefense, int oldSpeed, int oldSpAtk, int oldSpDef) {

	}

	public void ThrowAndDeflect(int ball, int targetBattler) {

	}

	public void Throw(int ball, int shakes, bool critical, Battler targetBattler, bool showPlayer=false) {

	}

	public void ThrowSuccess() {

	}

	public void HideCaptureBall() {

	}

	public void ThrowBait() {

	}

	public void ThrowRock() {
		
	}
}
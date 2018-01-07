using System;
using System.Collections.Generic;
using UnityEngine;

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
	public static bool USECOMMANDBOX = true; // If true, expects the file Graphics/Pictures/battleCommand.png
	public static bool USEFIGHTBOX = true; // If true, expects the file Graphics/Pictures/battleFight.png

	// Text colors
	public static Color MESSAGEBASECOLOR = new Color(80,80,88);
	public static Color MESSAGESHADOWCOLOR = new Color(160,160,168);
	public static Color MENUBASECOLOR = MESSAGEBASECOLOR;
	public static Color MENUSHADOWCOLOR = MESSAGESHADOWCOLOR;
	public static Color PPTEXTBASECOLOR = MESSAGEBASECOLOR; // More than 1/2 of total PP
	public static Color PPTEXTSHADOWCOLOR = MESSAGESHADOWCOLOR;
	public static Color PPTEXTBASECOLORYELLOW = new Color(248,192,0); // 1/2 of total PP or less
	public static Color PPTEXTSHADOWCOLORYELLOW = new Color(144,104,0);
	public static Color PPTEXTBASECOLORORANGE = new Color(248,136,32); // 1/4 of total PP or less
	public static Color PPTEXTSHADOWCOLORORANGE = new Color(144,72,24);
	public static Color PPTEXTBASECOLORRED = new Color(248,72,72); // Zero PP
	public static Color PPTEXTSHADOWCOLORRED = new Color(136,48,48);

	// Coordinates of the top left of the player's data boxes
	public static int PLAYERBOX_X = Graphics.width - 244;
	public static int PLAYERBOX_Y = Graphics.height - 192;
	public static int PLAYERBOXD1_X = PLAYERBOX_X - 12;
	public static int PLAYERBOXD1_Y = PLAYERBOX_Y - 20;
	public static int PLAYERBOXD2_X = PLAYERBOX_X;
	public static int PLAYERBOXD2_Y = PLAYERBOX_Y + 34;

	// Coordinates of the top left of the foe's data boxes
	public static int FOEBOX_X = -16;
	public static int FOEBOX_Y = 36;
	public static int FOEBOXD1_X = FOEBOX_X + 12;
	public static int FOEBOXD1_Y = FOEBOX_Y - 34;
	public static int FOEBOXD2_X = FOEBOX_X;
	public static int FOEBOXD2_Y = FOEBOX_Y + 20;

	// Coordinates of the top left of the player's Safari game data box
	public static int SAFARIBOX_X = Graphics.width - 232;
	public static int SAFARIBOX_Y = Graphics.height - 184;

	// Coordinates of the party bars and balls of both sides
	// Coordinates are the top left of the graphics except where specified
	public static int PLAYERPARTYBAR_X = Graphics.width - 248;
	public static int PLAYERPARTYBAR_Y = Graphics.height - 142;
	public static int PLAYERPARTYBALL1_X = PLAYERPARTYBAR_X + 44;
	public static int PLAYERPARTYBALL1_Y = PLAYERPARTYBAR_Y - 30;
	public static int PLAYERPARTYBALL_GAP = 32;
	public static int FOEPARTYBAR_X = 248; // Coordinates of end of bar nearest screen middle
	public static int FOEPARTYBAR_Y = 114;
	public static int FOEPARTYBALL1_X = FOEPARTYBAR_X - 44 - 30; // 30 is width of ball icon
	public static int FOEPARTYBALL1_Y = FOEPARTYBAR_Y - 30;
	public static int FOEPARTYBALL_GAP = 32; // Distance between centres of two adjacent balls

	// Coordinates of the centre bottom of the player's battler's sprite
	// Is also the centre middle of its shadow
	public static int PLAYERBATTLER_X = 128;
	public static int PLAYERBATTLER_Y = Graphics.height - 80;
	public static int PLAYERBATTLERD1_X = PLAYERBATTLER_X - 48;
	public static int PLAYERBATTLERD1_Y = PLAYERBATTLER_Y;
	public static int PLAYERBATTLERD2_X = PLAYERBATTLER_X + 32;
	public static int PLAYERBATTLERD2_Y = PLAYERBATTLER_Y + 16;

	// Coordinates of the centre bottom of the foe's battler's sprite
	// Is also the centre middle of its shadow
	public static int FOEBATTLER_X = Graphics.width - 128;
	public static int FOEBATTLER_Y = (Graphics.height * 3/4) - 112;
	public static int FOEBATTLERD1_X = FOEBATTLER_X + 48;
	public static int FOEBATTLERD1_Y = FOEBATTLER_Y;
	public static int FOEBATTLERD2_X = FOEBATTLER_X - 32;
	public static int FOEBATTLERD2_Y = FOEBATTLER_Y - 16;

	// Centre bottom of the player's side base graphic
	public static int PLAYERBASEX = PLAYERBATTLER_X;
	public static int PLAYERBASEY = PLAYERBATTLER_Y;

	// Centre middle of the foe's side base graphic
	public static int FOEBASEX = FOEBATTLER_X;
	public static int FOEBASEY = FOEBATTLER_Y;

	// Coordinates of the centre bottom of the player's sprite
	public static int PLAYERTRAINER_X = PLAYERBATTLER_X;
	public static int PLAYERTRAINER_Y = PLAYERBATTLER_Y - 16;
	public static int PLAYERTRAINERD1_X = PLAYERBATTLERD1_X;
	public static int PLAYERTRAINERD1_Y = PLAYERTRAINER_Y;
	public static int PLAYERTRAINERD2_X = PLAYERBATTLERD2_X;
	public static int PLAYERTRAINERD2_Y = PLAYERTRAINER_Y;

	// Coordinates of the centre bottom of the foe trainer's sprite
	public static int FOETRAINER_X = FOEBATTLER_X;
	public static int FOETRAINER_Y = FOEBATTLER_Y + 6;
	public static int FOETRAINERD1_X = FOEBATTLERD1_X;
	public static int FOETRAINERD1_Y = FOEBATTLERD1_Y + 6;
	public static int FOETRAINERD2_X = FOEBATTLERD2_X;
	public static int FOETRAINERD2_Y = FOEBATTLERD2_Y + 6;

	// Default focal points of user and target in animations - do not change!
	public static int FOCUSUSER_X = 128; // 144
	public static int FOCUSUSER_Y = 224; // 188
	public static int FOCUSTARGET_X = 384; // 352
	public static int FOCUSTARGET_Y = 96; // 108, 98
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
	public Battle battle;
	public int[] lastcmd;
	public int[] lastmove;
	public Dictionary<string, Window> sprites;
	public bool battleStart;
	public int messageMode;
	public bool abortable;
	public bool aborted;
	public bool briefmessage;
	public PokemonPartyScene.PartyScreen switchscreen;
	public Viewport viewport;

	public const int BLANK = 0;
	public const int MESSAGEBOX = 1;
	public const int COMMANDBOX = 2;
	public const int FIGHTBOX = 3;

	public BattleScene() {
		battle = null;
		lastcmd = new int[4]{0,0,0,0};
		lastmove = new int[4]{0,0,0,0};
		sprites = new Dictionary<string, Window>();
		battleStart = true;
		messageMode = 0;
		abortable = false;
		aborted = false;
	}

	public void Update() {
		PartyAnimationUpdate();
		sprites["battlebg"].Update();
	}

	public void GraphicsUpdate() {
		PartyAnimationUpdate();
		sprites["battlebg"].Update();
		Graphics.Update();
	}

	public void InputUpdate() {
		if (Input.GetKey("b") && abortable && !aborted) {
			aborted = true;
			battle.Abort();
		}
	}

	public void ShowWindow(int windowType) {
		sprites["messagebox"].visible = (windowType == MESSAGEBOX || windowType == COMMANDBOX || windowType == FIGHTBOX || windowType == BLANK);
		sprites["messagewindow"].visible = (windowType == MESSAGEBOX);
		sprites["commandwindow"].visible = (windowType == COMMANDBOX);
		sprites["fightwindow"].visible = (windowType == FIGHTBOX);
	}

	public void SetMessageMode(int mode) {
		messageMode = mode;
		Window msgwindow = sprites["messagewindow"];
		if (mode != 0) {
			msgwindow.baseColor = BattleSceneConstraints.MENUBASECOLOR;
			msgwindow.shadowColor = BattleSceneConstraints.MENUSHADOWCOLOR;
			msgwindow.opacity = 255;
			msgwindow.x = 16;
			msgwindow.width = Graphics.width;
			msgwindow.height = 96;
			msgwindow.y = Graphics.height-msgwindow.height+2;
		} else {
			msgwindow.baseColor = BattleSceneConstraints.MESSAGEBASECOLOR;
			msgwindow.shadowColor = BattleSceneConstraints.MESSAGESHADOWCOLOR;
			msgwindow.opacity = 0;
			msgwindow.x = 16;
			msgwindow.width = Graphics.width-32;
			msgwindow.height = 96;
			msgwindow.y = Graphics.height-msgwindow.height+2;
		}
	}

	public void WaitMessage() {
		if (briefmessage) {
			ShowWindow(MESSAGEBOX);
			Window cw = sprites["messagewindow"];
			for (int i=0; i<40; i++) 
			{
				GraphicsUpdate();
				InputUpdate();
				FrameUpdate(cw);
			}
			cw.text = "";
			cw.visible = false;
			briefmessage = false;
		}
	}

	public void Display(string msg, bool brief=false) {
		DisplayMessage(msg, brief);
	}

	public void DisplayMessage(string msg, bool brief=false) {
		WaitMessage();
		Refresh();
		ShowWindow(MESSAGEBOX);
		Window cw = sprites["messagewindow"];
		cw.text = msg;
		for (int i=0; i<40; i++) 
		{
			GraphicsUpdate();
			InputUpdate();
			FrameUpdate(cw);
			if (Input.GetKey("c") || abortable) {
				if (cw.Pausing()) {
					if (!abortable) {
						Music.PlayDecisionSE();
					}
					cw.Resume();
				}
			}
			if (!cw.Busy()) {
				if (brief) {
					briefmessage = true;
					return;
				}
			}
		}
		cw.text = "";
		cw.visible = false;
	}

	public void DisplayPausedMessage(string msg) {
		WaitMessage();
		Refresh();
		ShowWindow(MESSAGEBOX);
		if (messageMode != 0) {
			switchscreen.Display(msg);
			return;
		}
		Window cw = sprites["messagewindow"];
		cw.text = string.Format("{0}\\1", msg);
		while (true) {
			GraphicsUpdate();
			InputUpdate();
			FrameUpdate(cw);
			if (Input.GetKey("c") || abortable) {
				if (cw.Busy()) {
					if (cw.Pausing() && !abortable) {
						Music.PlayDecisionSE();
					}
					cw.Resume();
				} else if (!InPartyAnimation()) {
					cw.text = "";
					Music.PlayDecisionSE();
					if (messageMode != 0) {
						cw.visible = false;
					}
					return;
				}
			}
			cw.Update();
		}
	}

	public bool DisplayConfirmMessage(string msg) {
		return ShowCommands(msg, new string[2]{"Yes", "No"}, 1) == 0;
	}

	public int ShowCommands(string msg, string[] commands, int defaultValue) {
		WaitMessage();
		Refresh();
		ShowWindow(MESSAGEBOX);
		Window dw = sprites["messagewindow"];
		dw.text = msg;
		Window.CommandPokemon cw = new Window.CommandPokemon(commands);
		cw.x = Graphics.width - cw.width;
		cw.y = Graphics.height - cw.height - dw.height;
		cw.index = 0;
		cw.viewport = viewport;
		Refresh();
		while (true) {
			cw.visible = !dw.Busy();
			GraphicsUpdate();
			InputUpdate();
			FrameUpdate(cw);
			dw.Update();
			if (Input.GetKey("b") && defaultValue >= 0) {
				if (dw.Busy()) {
					if (dw.Pausing()) {
						Music.PlayDecisionSE();
					}
					dw.Resume();
				} else {
					cw.Dispose();
					dw.text = "";
					return defaultValue;
				}
			}
			if (Input.GetKey("c")) {
				if (dw.Busy()) {
					if (dw.Pausing()) {
						Music.PlayDecisionSE();
					}
					dw.Resume();
				} else {
					cw.Dispose();
					dw.text = "";
					return cw.index;
				}
			}
		}
	}

	public void FrameUpdate(Window cw=null) {

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
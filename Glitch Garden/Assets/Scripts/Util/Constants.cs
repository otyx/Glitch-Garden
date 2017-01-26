using UnityEngine;
using System.Collections;

public static class Constants {

	// Level constants
	public const string SCN_SPLASH				= "00 Splash"	;
	public const string SCN_STARTMENU			= "01a Start"	;
	public const string SCN_OPTIONS				= "01b Options"	;
	public const string SCN_LEVEL_PREFIX		= "02 Level_"	;
	public const string SCN_WIN					= "03a Win"		;
	public const string SCN_LOSE				= "03b Lose"	;
	
	// Audio System
	public const string MUSIC_AUDIOSRC_NAME		= "Music";
	public const string EFFECTS_AUDIOSRC_NAME	= "Effects";
	
	// Enemies
	public const string ATTACKER				= "attacker";
	public const string FOX						= "Fox";
	public const string LIZARD					= "Lizard";
	
	// Defenders
	public const string DEFENDER				= "defender";
	public const string GRAVESTONE				= "Gravestone";
	public const string GNOME					= "Gnome";
	public const string STAR_TROPHY				= "Star Trophy";
	public const string CACTUS					= "Cactus";
	
	// other GameObjects
	public const string OBJ_PROJECTILES			= "Projectiles";
	public const string OBJ_LAUNCHER			= "Launcher";
	public const string OBJ_DEFENDERS			= "Defenders";
	public const int	LYR_DEFENDERS			= 8;
	public const int	LYR_PROJECTILES			= 10;
	
	public const string OBJ_ATTACKERS			= "Attackers";
	public const int	LYR_ATTACKERS			= 9;
	
	public const string	OBJ_SELECTOR_PANEL		= "SelectorPanel";
	
	// Animation Constants
	public const string FOX_JUMP_TRIGGER		= "Jump Trigger";
	
	// Is the object attacking?
	public const string BOOL_IS_ATTACKING		= "isAttacking";
	// Is the object being attacked?
	public const string BOOL_IS_ATTACKED		= "isAttacked";
	
}

using UnityEngine;
using System.Collections;

public static class Constants {

	// Level constants
	public const string LVL_SPLASH				= "00 Splash"	;
	public const string LVL_STARTMENU			= "01a Start"	;

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
	
	// Animation Constants
	public const string FOX_JUMP_TRIGGER		= "Jump Trigger";
	
	// Is the object attacking?
	public const string BOOL_IS_ATTACKING		= "isAttacking";
	// Is the object being attacked?
	public const string BOOL_IS_ATTACKED		= "isAttacked";
	

}

using UnityEngine;
using System.Collections;

namespace MyFightGame {
public class GUIScript : MonoBehaviour {
	public GUISkin customSkin;

	//You can copy or make a reference to any of the live variables in the system

	// Player 1 Variables
	private CharacterInfo player1;
	private Vector2 player1NameLocation;
	private Vector2 player1AlertLocation;
	private TextAnchor player1Anchor;
	private float player1TargetLife;
	private float player1TotalLife;
	private GameObject player1AlertGO;
	private GameObject player1NameGO;

	// Player 2 Variables
	private CharacterInfo player2;
	private Vector3 player2NameLocation;
	private Vector3 player2AlertLocation;
	private TextAnchor player2Anchor;
	private float player2TargetLife;
	private float player2TotalLife;
	private GameObject player2AlertGO;
	private GameObject player2NameGO;
	private GameObject infoGO;

	// Main Alert Variables
	private GameObject mainAlertGO;
	private Vector2 mainAlertLocation;
	private bool showEndMenu;
	private bool showControls;
	private bool showSpecials;

	private bool isRunning; // Checks if the game is running

	void Awake () {
		// Subscribe to the events from UFE
		/* Possible Events:
		 * OnLifePointsChange(float newLifePoints, CharacterInfo player)
		 * OnNewAlert(string alertMessage, CharacterInfo player)
		 * OnHit(MoveInfo move, CharacterInfo hitter)
		 * OnMove(MoveInfo move, CharacterInfo player)
		 * OnRoundEnds(CharacterInfo winner, CharacterInfo loser)
		 * OnRoundBegins(int roundNumber)
		 * OnGameEnds(CharacterInfo winner, CharacterInfo loser)
		 * OnGameBegins(CharacterInfo player1, CharacterInfo player2, StageOptions stage)
		 * 
		 * usage:
		 * UFE.OnMove += YourFunctionHere;
		 * .
		 * .
		 * void YourFunctionHere(T param1, T param2){...}
		 * 
		 * The following code bellow show more usage examples
		 */
		//UFE.OnGameBegin += OnGameBegins;
		//UFE.OnGameEnds += OnGameEnds;
		//UFE.OnRoundBegins += OnRoundBegins;
		//UFE.OnRoundEnds += OnRoundEnds;
		//UFE.OnLifePointsChange += OnLifePointsChange;
		//UFE.OnNewAlert += OnNewAlert;
		//UFE.OnHit += OnHit;
		UFE.OnMove += OnMove;
	}
        
	void OnGameEnds(CharacterInfo winner, CharacterInfo loser){
		showEndMenu = true;
		isRunning = false;
		Destroy(player1NameGO);
		Destroy(player2NameGO);
		Destroy(infoGO);
	}

	void OnRoundBegins(int round){
		// Fires when a round begins
		if (player1AlertGO != null) Destroy(player1AlertGO);
		if (player2AlertGO != null) Destroy(player2AlertGO);
	}
	
	void OnRoundEnds(CharacterInfo winner, CharacterInfo loser){
		// Fires when a round ends
		// TODO add round counter to show how many rounds a player has won
	}

	void OnMove(MoveInfo move, CharacterInfo player){
		// Fires when a player successfully executes a move
	}
        
	void OnLifePointsChange(float newLife, CharacterInfo player){
		// You can use this to have your own custom events when a player's life points changes
	}

	void OnHit(HitBox strokeHitBox, MoveInfo move, CharacterInfo hitter){
		// You can use this to have your own custom events when a character gets hit
	}
        

	
	
	void GOTween(GameObject gameObject, Vector3 destination, float speed){
		// Lerp effect to move the alert text
		gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, destination, Time.deltaTime * speed);
	}

	void DrawBar(GUIBarOptions guiBar, Side side, float currentValue, float totalValue, bool topGUI){
		// A custom method to create the draining bar
		Rect backRect = SetResolution(guiBar.backgroundRect);
		Rect fillRect = SetResolution(guiBar.fillRect);

		Rect remainFill;
		float newWidth = (currentValue/totalValue) * fillRect.width;
		float leftAdjustment = fillRect.width - newWidth;
		if (side == Side.Right){
			backRect.x = Screen.width - backRect.width - backRect.x;
			remainFill = new Rect(fillRect.x, fillRect.y, newWidth, fillRect.height);
		}else{
			float newXPos = fillRect.x + (fillRect.width - newWidth);
			remainFill = new Rect(newXPos, fillRect.y, (currentValue/totalValue) * fillRect.width, fillRect.height);
		}

		if (!topGUI) backRect.y = Screen.height - backRect.height - backRect.y;

		GUI.DrawTexture(backRect, guiBar.backgroundImage);
		GUI.BeginGroup(backRect);{
			GUI.BeginGroup(remainFill);{
				if (side == Side.Right){
					GUI.DrawTexture(new Rect(0,0, fillRect.width, fillRect.height), guiBar.fillImage, ScaleMode.ScaleAndCrop);
				}else{
					GUI.DrawTexture(new Rect(-leftAdjustment,0, fillRect.width, fillRect.height), guiBar.fillImage, ScaleMode.ScaleAndCrop);
				}
			}GUI.EndGroup();
		}GUI.EndGroup();
	}
	
	Rect SetResolution(Rect rect){
		// Adjusts the texture's size and position according to the size of the window
		rect.x *= ((float)Screen.width/1280);
		rect.y *= ((float)Screen.height/720);
		rect.width *= ((float)Screen.width/1280);
		rect.height *= ((float)Screen.height/720);
		return rect;
	}


	
    }
}

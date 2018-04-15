using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyFightGame { 
public class UFE : MonoBehaviour {
        public InputConfig mInputConfig;

        public delegate void MoveHandler(MoveInfo move, CharacterInfo player);
        public static event MoveHandler OnMove;

        public GlobalInfo mGlobalInfo;
        public static InputConfig inputConfig;
        public static GlobalInfo config;

        



        private void Awake()
        {
            inputConfig = mInputConfig;
            config = mGlobalInfo;
        }

        // Use this for initialization
        void Start () {
            GameObject p1 = new GameObject("Player1");
            p1.AddComponent<ControlsScript>();
            p1.AddComponent<PhysicsScript>();

            GameObject p2 = new GameObject("Player2");
            p2.AddComponent<ControlsScript>();
            p2.AddComponent<PhysicsScript>();
        }
	
	    // Update is called once per frame
	    void Update () {
		
	    }

        public static void FireMove(MoveInfo move, CharacterInfo player)
        {
            if (UFE.OnMove != null) OnMove(move, player);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        GetComponent<Animation>().Play("idle_standing");
        Invoke("PlayMoveAnim",2f);
        
    }

    public void PlayMoveAnim() {
        GetComponent<Animation>().CrossFade("move_forward", 0.1f);
    }

	// Update is called once per frame
	void Update () {
		
	}
}

using UnityEngine;
using System.Collections;

namespace MyFightGame
{

    public class PhysicsScript : MonoBehaviour
    {
        [HideInInspector]
        public bool freeze;
        [HideInInspector]
        public float airTime = 0;

        private float faceDirection = 0; // ÈËÎï³¯Ïò
        //private float verticalForce = 0;
        private float xForce = 0;
        private float yForce = 0;
        private float zForce = 0;
        private float verticalTotalForce = 0;
        private int groundLayer;
        private int groundMask;
        private int currentAirJumps;
        private int bounceTimes;
        private bool isBouncing;
        private float walkSpeed;
        private float runSpeed;
        private float appliedGravity;

        private ControlsScript myControlsScript;
        private HitBoxesScript myHitBoxesScript;
        private MoveSetScript myMoveSetScript;
        private GameObject character;

        public void Start()
        {
            /*Plane groundPlane = (Plane) GameObject.FindObjectOfType(typeof(Plane));
            if (groundPlane == null) Debug.LogError("Plane not found. Please add a plane mesh to your stage prefab!");*/

            groundLayer = LayerMask.NameToLayer("Ground");
            groundMask = 1 << groundLayer;
            myControlsScript = GetComponent<ControlsScript>();
            character = myControlsScript.character;
            myHitBoxesScript = character.GetComponent<HitBoxesScript>();
            myMoveSetScript = character.GetComponent<MoveSetScript>();
            appliedGravity = myControlsScript.myInfo.physics.weight * UFE.config.gravity;

            walkSpeed = myControlsScript.myInfo.physics.walkSpeed;
            runSpeed = myControlsScript.myInfo.physics.runSpeed;
        }

        public void SetMoveFoce(Vector2 direction) {
            xForce = direction.x;
            zForce = direction.y;
        }

        public void move(Vector2 direction)
        {
            if (!isGrounded()) return;
            if (direction.x != 0) {
                faceDirection = direction.x;
                xForce = myControlsScript.myInfo.physics.walkSpeed * direction.x;
            }
            if (direction.y != 0) {
                zForce = myControlsScript.myInfo.physics.moveUpSpeed * direction.y;
            }
        }

        public void AddXForce(float force)
        {
            if (isGrounded()) {
                faceDirection = force;
            }
            xForce = force;
        }
        public void AddZForce(float force)
        {
            //if (!isGrounded()) return;
            zForce = force;
        }

        public void xMove(float direction)
        {
            if (!isGrounded()) return;
            faceDirection = direction;

            xForce = myControlsScript.myInfo.physics.walkSpeed * direction;
        }

        public void zMove(float direction)
        {
            if (!isGrounded()) return;
            //moveDirection = direction;

            zForce = myControlsScript.myInfo.physics.moveUpSpeed * direction;
        }

        public void jump()
        {
            if (currentAirJumps >= myControlsScript.myInfo.physics.multiJumps) return;
            currentAirJumps++;
            xForce = myControlsScript.myInfo.physics.jumpDistance * faceDirection;
            yForce = myControlsScript.myInfo.physics.jumpForce;
            setVerticalData(myControlsScript.myInfo.physics.jumpForce);
        }

        public void resetForces(bool resetX, bool resetY)
        {
            if (resetX) xForce = 0;
            if (resetY) zForce = 0;
        }

        public void addForce(Vector2 push, int mirror)
        {
            push.x *= mirror;
            isBouncing = false;
            if (!myControlsScript.myInfo.physics.cumulativeForce)
            {
                xForce = 0;
                zForce = 0;
            }
            if (zForce < 0 && push.y > 0) zForce = 0;
            xForce += push.x;
            zForce += push.y;
            setVerticalData(zForce);
        }


        void setVerticalData(float appliedForce)
        {
            float maxHeight = Mathf.Pow(appliedForce, 2) / (appliedGravity * 2);
            maxHeight += transform.position.y;
            airTime = Mathf.Sqrt(maxHeight * 2 / appliedGravity);
            verticalTotalForce = appliedGravity * airTime;
        }

        public void applyForces(MoveInfo move)
        {
            if (xForce != 0)
                transform.Translate(xForce * Time.deltaTime, 0, 0);
            if (zForce != 0)
                transform.Translate(0, 0, zForce * Time.deltaTime);
            if (faceDirection > 0)
                character.transform.localScale = new Vector3(1, 1, 1f);
            else if (faceDirection < 0) 
                character.transform.localScale = new Vector3(1, 1, -1f);
            
            if (!myControlsScript.stunned && move == null)
            {
                if(xForce == walkSpeed || xForce == -walkSpeed || zForce == walkSpeed || zForce == -walkSpeed)
                { 
                    myMoveSetScript.playBasicMove(myMoveSetScript.basicMoves.moveForward);
                }
                else if(xForce == runSpeed|| xForce == -runSpeed || zForce == runSpeed || zForce == -runSpeed)
                {
                    myMoveSetScript.playBasicMove(myMoveSetScript.basicMoves.run);
                }
            }

            faceDirection = 0;
        }

        public bool isGrounded()
        {
            Vector3 p = transform.position + Vector3.up + new Vector3(0, 0f, 0);

            if (Physics.RaycastAll(p, Vector3.down, 2.1f, groundMask).Length > 0)
            {
                //if (transform.position.y != 0) transform.Translate(new Vector3(0, -transform.position.y, 0));
                return true;
            }
            return false;
        }
    }
}
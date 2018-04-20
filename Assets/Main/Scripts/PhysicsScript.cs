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
        public float xForce = 0;
        public float yForce = 0;
        public float zForce = 0;
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
                faceDirection = force>0?1:-1;
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
            //xForce = myControlsScript.myInfo.physics.jumpForwardSpeed * faceDirection;
            yForce = myControlsScript.myInfo.physics.jumpForce;
            setVerticalData(myControlsScript.myInfo.physics.jumpForce);
        }

        public void resetForces(bool resetX,bool resetY,bool resetZ)
        {
            if (resetX) xForce = 0;
            if (resetY) yForce = 0;
            if (resetZ) zForce = 0;
        }

        public void addForce(Vector2 push, int mirror)
        {
            push.x *= mirror;
            isBouncing = false;
            if (!myControlsScript.myInfo.physics.cumulativeForce)
            {
                xForce = 0;
                yForce = 0;
            }
            if (yForce < 0 && push.y > 0) yForce = 0;
            xForce += push.x;
            yForce += push.y;
            setVerticalData(yForce);
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

            if (move == null || (move != null && !move.ignoreGravity))
            {
                if ((yForce < 0 && !isGrounded()) || yForce > 0)
                {
                    yForce -= appliedGravity * Time.deltaTime;
                    transform.Translate(faceDirection * myControlsScript.myInfo.physics.jumpForwardSpeed * Time.deltaTime, yForce * Time.deltaTime, 0);
                }
                else if (yForce < 0 && isGrounded())
                {
                    currentAirJumps = 0;
                    yForce = 0;
                }
            }

            if (isGrounded())
            {
                if (verticalTotalForce != 0)
                {
                    if (bounceTimes < UFE.config.bounceOptions.maximumBounces && myControlsScript.stunned &&
                    UFE.config.bounceOptions.bounceForce != Sizes.None &&
                    yForce <= -UFE.config.bounceOptions.minimumBounceForce)
                    {
                        if (!UFE.config.bounceOptions.bounceHitBoxes) myHitBoxesScript.hideHitBoxes();
                        if (UFE.config.bounceOptions.bounceForce == Sizes.Small)
                        {
                            addForce(new Vector2(0, -yForce / 2.4f), 1);
                        }
                        else if (UFE.config.bounceOptions.bounceForce == Sizes.Medium)
                        {
                            addForce(new Vector2(0, -yForce / 1.8f), 1);
                        }
                        else if (UFE.config.bounceOptions.bounceForce == Sizes.High)
                        {
                            addForce(new Vector2(0, -yForce / 1.2f), 1);
                        }
                        bounceTimes++;
                        if (!isBouncing)
                        {
                            myControlsScript.stunTime += airTime + UFE.config.knockDownOptions.knockedOutTime;
                            myMoveSetScript.playBasicMove(myMoveSetScript.basicMoves.bounce);
                            if (UFE.config.bounceOptions.bouncePrefab != null)
                            {
                                GameObject pTemp = (GameObject)Instantiate(UFE.config.bounceOptions.bouncePrefab);
                                pTemp.transform.parent = transform;
                                pTemp.transform.localPosition = Vector3.zero;
                                Destroy(pTemp, 3);
                            }
                            isBouncing = true;
                        }
                        return;
                    }

                    verticalTotalForce = 0;
                    airTime = 0;
                    myMoveSetScript.totalAirMoves = 0;
                    BasicMoveInfo airAnimation = null;
                    if (myControlsScript.stunned)
                    {
                        myControlsScript.stunTime = UFE.config.knockDownOptions.knockedOutTime + UFE.config.knockDownOptions.getUpTime;
                        airAnimation = myMoveSetScript.basicMoves.fallDown;
                        myControlsScript.currentState = PossibleStates.FallDown;
                        if (!UFE.config.knockDownOptions.knockedOutHitBoxes) myHitBoxesScript.hideHitBoxes();
                    }
                    else
                    {
                        if ((myControlsScript.currentMove != null && myControlsScript.currentMove.cancelMoveWheLanding) ||
                            myControlsScript.currentMove == null)
                        {
                            airAnimation = myMoveSetScript.basicMoves.landing;
                            myControlsScript.KillCurrentMove();
                        }
                        if (myControlsScript.isRun) 
                            myControlsScript.currentState = PossibleStates.Run;
                        else
                            myControlsScript.currentState = PossibleStates.Stand;
                    }
                    isBouncing = false;
                    bounceTimes = 0;
                    if (airAnimation != null && !character.GetComponent<Animation>().IsPlaying(airAnimation.name))
                    {
                        myMoveSetScript.playBasicMove(airAnimation);
                    }
                }

                if (!myControlsScript.stunned && move == null)
                {
                    if (xForce == walkSpeed || xForce == -walkSpeed || zForce == walkSpeed || zForce == -walkSpeed)
                    {
                        myMoveSetScript.playBasicMove(myMoveSetScript.basicMoves.moveForward);
                    }
                    else if (xForce == runSpeed || xForce == -runSpeed || zForce == runSpeed || zForce == -runSpeed)
                    {
                        myMoveSetScript.playBasicMove(myMoveSetScript.basicMoves.run);
                    }
                }
            }
            else if (yForce > 0 || !isGrounded())
            {
                if (move != null && myControlsScript.currentState == PossibleStates.Stand)
                    myControlsScript.currentState = PossibleStates.Jump;
                if (move == null && yForce / verticalTotalForce > 0 && yForce / verticalTotalForce <= 1)
                {
                    if (isBouncing) return;
                    BasicMoveInfo airAnimation = myControlsScript.stunned ?
                        myMoveSetScript.basicMoves.getHitAir : myMoveSetScript.basicMoves.jumping;

                    if (xForce == 0)
                    {
                        myControlsScript.currentState = PossibleStates.Jump;
                    }
                    else
                    {
                        if (xForce > 0)
                            myControlsScript.currentState = PossibleStates.Jump;

                        if (xForce < 0)
                            myControlsScript.currentState = PossibleStates.Jump;
                    }

                    if (!character.GetComponent<Animation>().IsPlaying(airAnimation.name))
                    {
                        //character.animation[airAnimation].speed = character.animation[airAnimation].length * (appliedGravity/verticalTotalForce);
                        character.GetComponent<Animation>()[airAnimation.name].speed = character.GetComponent<Animation>()[airAnimation.name].length / airTime;
                        myMoveSetScript.playBasicMove(airAnimation);

                    }
                }
                else if (move == null && yForce / verticalTotalForce <= 0)
                {
                    BasicMoveInfo airAnimation;
                    if (isBouncing)
                    {
                        airAnimation = myMoveSetScript.basicMoves.fallingFromBounce;
                    }
                    else
                    {
                        airAnimation = myControlsScript.stunned ?
                            myMoveSetScript.basicMoves.getHitAir : myMoveSetScript.basicMoves.falling;
                    }

                    if (!character.GetComponent<Animation>().IsPlaying(airAnimation.name))
                    {
                        //character.animation[airAnimation].speed = character.animation[airAnimation].length * (appliedGravity/verticalTotalForce);
                        //character.animation.CrossFade(airAnimation, GlobalScript.getCurrentMoveSet(myControlsScript.myInfo).interpolationSpeed);
                        character.GetComponent<Animation>()[airAnimation.name].speed = character.GetComponent<Animation>()[airAnimation.name].length / airTime;
                        myMoveSetScript.playBasicMove(airAnimation);
                    }
                }
            }
           // if (xForce == 0 && yForce == 0 && zForce == 0)
                faceDirection = 0;
            
        }

        public bool isGrounded()
        {
            Vector3 p = transform.position + Vector3.up + new Vector3(0, 0f, 0);

            if (Physics.RaycastAll(p, Vector3.down, 1f, groundMask).Length > 0)
            {
                if (transform.position.y != 0) transform.Translate(new Vector3(0, -transform.position.y, 0));
                return true;
            }
            return false;
        }
    }
}
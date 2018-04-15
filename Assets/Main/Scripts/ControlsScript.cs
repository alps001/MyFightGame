using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyFightGame
{

    [System.Serializable]
    public class InputReferences
    {
        public InputType inputType;
        public string inputButtonName;
        public ButtonPress engineRelatedButton;

        [HideInInspector]
        public float heldDown;
    }

    public enum PreRunDirection
    {
        Left,
        Right,
        Up,
        Down,
        None
    };

    public class ControlsScript : MonoBehaviour
    {
        public GameObject character;
        public GameObject opponent;
        public CharacterInfo myInfo;

        public PossibleStates currentState;
        public MoveInfo currentMove;
        public MoveInfo storedMove;
        
        public Vector2 moveDirection;
        public bool isRun;
        public float leftHeldTime;
        public float rightHeldTime;
        public float upHeldTime;
        public float downHeldTime;

        public bool stunned;
        public float stunTime;
        public bool isBlocking;

        private InputReferences[] inputReferences;

        private PhysicsScript myPhysicsScript;
        private MoveSetScript myMoveSetScript;
        private HitBoxesScript myHitBoxesScript;

        private PhysicsScript opPhysicsScript;
        private ControlsScript opControlsScript;
        private HitBoxesScript opHitBoxesScript;
        private CharacterInfo opInfo;
        private float storedMoveTime;
        private bool animationPaused;
        private string currentHitAnimation;
        private Vector3 pullInLocation;
        private int pullInSpeed;
        private int comboHits;
        public bool isDead;
        private float hitStunDeceleration = 0;

        private Shader[] normalShaders;
        private Color[] normalColors;

        // Use this for initialization
        void Start()
        {
            //inputReferences = UFE.inputConfig.inputReferences;

            myPhysicsScript = GetComponent<PhysicsScript>();
            if (gameObject.name == "Player1")
            {
                myInfo = (CharacterInfo)Instantiate(UFE.config.player1Character);
                inputReferences = UFE.config.player1_Inputs;
                opponent = GameObject.Find("Player2");
            }
            else
            {
                myInfo = (CharacterInfo)Instantiate(UFE.config.player2Character);
                inputReferences = UFE.config.player2_Inputs;
                opponent = GameObject.Find("Player1");
            }

            character = (GameObject)Instantiate(myInfo.characterPrefab);
            character.transform.parent = transform;
            character.AddComponent<MoveSetScript>();
            myHitBoxesScript = character.GetComponent<HitBoxesScript>();
            myMoveSetScript = character.GetComponent<MoveSetScript>();
        }

        // Update is called once per frame
        void Update()
        {

            // 存放动作时间减少
            if (!myPhysicsScript.freeze && storedMoveTime > 0) storedMoveTime -= Time.deltaTime;
            if (storedMoveTime < 0)
            {
                storedMoveTime = 0;
                storedMove = null;
            }
            // 执行存放动作
            if ((currentMove == null || currentMove.cancelable) && storedMove != null && !myPhysicsScript.freeze)
            {
                if (currentMove != null) KillCurrentMove();
                //if (System.Array.IndexOf(storedMove.possibleStates, currentState) != -1) currentMove = storedMove;
                storedMove = null;
                return;
            }

            foreach (AnimationState animState in character.GetComponent<Animation>())
            {
                if (character.GetComponent<Animation>().IsPlaying(animState.name))
                {

                    Debug.Log("IsPlaying: " + animState.name);
                }
            }

            // 执行存放动作
            if ((currentMove == null || currentMove.cancelable) && storedMove != null && !myPhysicsScript.freeze)
            {
                if (currentMove != null) KillCurrentMove();
                if (System.Array.IndexOf(storedMove.possibleStates, currentState) != -1) currentMove = storedMove;
                storedMove = null;
                return;
            }
            // 执行默认idle动作
            if (!myPhysicsScript.freeze && myPhysicsScript.isGrounded() && isAxisRested() && !character.GetComponent<Animation>().IsPlaying("idle"))
            {
                Debug.Log(character.GetComponent<Animation>().GetClipCount());

                bool playIdle = true;
                foreach (AnimationState animState in character.GetComponent<Animation>())
                {
                    if (animState.name != "idle" &&
                        animState.name != "moveForward" &&
                        animState.name != "moveBack" &&
                        animState.name != "run" &&
                        animState.name != "crouching" &&
                        animState.name != "blockingLowPose" &&
                        character.GetComponent<Animation>().IsPlaying(animState.name))
                    {
                        playIdle = false;
                    }
                }
                if (playIdle)
                {
                    myMoveSetScript.playBasicMove(myMoveSetScript.basicMoves.idle);
                    currentState = PossibleStates.Stand;
                    //if (GlobalScript.prefs.blockOptions.blockType == BlockType.AutoBlock) potentialBlock = true;
                }
            }
            
            bool hasAxisKeyDown = false;
            foreach (InputReferences inputRef in inputReferences)
            {
                // 清空方向键 按下时间heldDown
                if (inputRef.inputType != InputType.Button && inputRef.heldDown > 0 && Input.GetAxisRaw(inputRef.inputButtonName) == 0)
                {
                    if (inputRef.heldDown >= myInfo.chargeTiming) {
                        storedMove = myMoveSetScript.getMove(new ButtonPress[] { inputRef.engineRelatedButton }, inputRef.heldDown, currentMove, true);
                        
                    }
                    inputRef.heldDown = 0;
                    if (inputRef.inputType == InputType.Left)
                    {
                        leftHeldTime = 0;
                    }
                    else if (inputRef.inputType == InputType.Right)
                    {
                        rightHeldTime = 0;
                    }
                    else if (inputRef.inputType == InputType.Up)
                    {
                        upHeldTime = 0;
                    }
                    else if (inputRef.inputType == InputType.Down)
                    {
                        downHeldTime = 0;
                    }

                    if ((currentMove == null || currentMove.cancelable) && storedMove != null)
                    {
                        currentMove = storedMove;
                        storedMove = null;
                        return;
                    }
                    else if (storedMove != null)
                    {
                        storedMoveTime = UFE.config.storedExecutionDelay;
                        return;
                    }
                }

                if (Input.GetButtonUp(inputRef.inputButtonName))
                {
                    if (inputRef.heldDown >= myInfo.chargeTiming)
                        storedMove = myMoveSetScript.getMove(new ButtonPress[] { inputRef.engineRelatedButton }, inputRef.heldDown, currentMove, true);
                    inputRef.heldDown = 0;
                    if ((currentMove == null || currentMove.cancelable) && storedMove != null)
                    {
                        currentMove = storedMove;
                        storedMove = null;
                        return;
                    }
                    else if (storedMove != null)
                    {
                        storedMoveTime = UFE.config.storedExecutionDelay;
                        return;
                    }
                }
                
                // 方向键按下
                if (inputRef.inputType != InputType.Button && Input.GetAxisRaw(inputRef.inputButtonName) != 0)
                {

                    hasAxisKeyDown = true;
                    bool axisPressed = false;
                    moveDirection = Vector2.zero;

                    
                    if (inputRef.inputType == InputType.Left)
                    {
                        inputRef.engineRelatedButton = ButtonPress.Left;
                        moveDirection.x = -1;
                        leftHeldTime = inputRef.heldDown;
                    }
                    else if (inputRef.inputType == InputType.Right)
                    {
                        inputRef.engineRelatedButton = ButtonPress.Right;
                        moveDirection.x = 1;
                        rightHeldTime = inputRef.heldDown;
                    }
                    else if (inputRef.inputType == InputType.Up)
                    {
                        inputRef.engineRelatedButton = ButtonPress.Up;
                        moveDirection.y = 1;
                        upHeldTime = inputRef.heldDown;
                    }
                    else if (inputRef.inputType == InputType.Down)
                    {
                        inputRef.engineRelatedButton = ButtonPress.Down;
                        moveDirection.y = -1;
                        downHeldTime = inputRef.heldDown;
                    }

                    if (inputRef.heldDown == 0) axisPressed = true;
                    inputRef.heldDown += Time.deltaTime;

                    // 第一次（或执行动作之后）按下方向键
                    if (axisPressed)
                    {
                        storedMove = myMoveSetScript.getMove(new ButtonPress[] { inputRef.engineRelatedButton }, 0, currentMove, false);
                        if ((currentMove == null || currentMove.cancelable) && storedMove != null)
                        {
                            currentMove = storedMove;
                            storedMove = null;
                            return;
                        }
                        else if (storedMove != null)
                        {
                            storedMoveTime = UFE.config.storedExecutionDelay;
                            return;
                        }
                    }
                }// END 方向键

                // 按钮判断
                if (inputRef.inputType == InputType.Button && !UFE.config.lockInputs)
                {
                    if (Input.GetButton(inputRef.inputButtonName))
                    {
                        // 多个按钮同时按下
                    }

                    // 单个按钮按下时可能执行的动作
                    if (Input.GetButtonDown(inputRef.inputButtonName))
                    {
                        storedMove = myMoveSetScript.getMove(new ButtonPress[] { inputRef.engineRelatedButton }, 0, currentMove, false);
                        if ((currentMove == null || currentMove.cancelable) && storedMove != null)
                        {
                            currentMove = storedMove;
                            storedMove = null;
                            return;
                        }
                        else if (storedMove != null)
                        {
                            storedMoveTime = UFE.config.storedExecutionDelay;
                            return;
                        }
                        // 跳跃键按下
                        if (inputRef.engineRelatedButton == ButtonPress.Jump) {
                            if (currentMove == null)
                            {
                                if (myPhysicsScript.isGrounded()) myPhysicsScript.jump();
                                if (inputRef.heldDown == 0)
                                {
                                    if (!myPhysicsScript.isGrounded() && myInfo.physics.multiJumps > 1)
                                        myPhysicsScript.jump();
                                }
                            }
                        }
                    }
                    // 执行只有当按钮弹起才执行的动作
                    if (Input.GetButtonUp(inputRef.inputButtonName))
                    {
                        storedMove = myMoveSetScript.getMove(new ButtonPress[] { inputRef.engineRelatedButton }, 0, currentMove, true);
                        if ((currentMove == null || currentMove.cancelable) && storedMove != null)
                        {
                            currentMove = storedMove;
                            storedMove = null;
                        }
                        else if (storedMove != null)
                        {
                            storedMoveTime = UFE.config.storedExecutionDelay;
                            return;
                        }
                    }
                }
            }// END 按键列表循环


            SetMovementForce();

            if (!hasAxisKeyDown) {
                //readyToRun = PreRunDirection.None;
                isRun = false;
            }

        }

        
        private bool isAxisRested()
        {
            foreach (InputReferences inputRef in inputReferences)
            {
                if (inputRef.inputType == InputType.Button) continue;
                if (Input.GetAxisRaw(inputRef.inputButtonName) != 0) return false;
            }
            return true;
        }

        private void SetMovementForce()
        {
            if ((currentMove != null && currentState == PossibleStates.Jump) ||
                currentMove == null) {
                float force = isRun ? myInfo.physics.runSpeed : myInfo.physics.walkSpeed;
                if (leftHeldTime != 0 && rightHeldTime != 0)
                {
                    float dir = leftHeldTime < rightHeldTime ? -1 : 1;
                    myPhysicsScript.AddXForce(force * dir);
                }
                else if (leftHeldTime != 0 && rightHeldTime == 0)
                {
                    myPhysicsScript.AddXForce(-force);
                }
                else if (leftHeldTime == 0 && rightHeldTime != 0)
                {
                    myPhysicsScript.AddXForce(force);
                }

                if (downHeldTime != 0 && upHeldTime != 0)
                {
                    float dir = downHeldTime < upHeldTime ? -1 : 1;
                    myPhysicsScript.AddZForce(force * dir);
                }
                else if (downHeldTime != 0 && upHeldTime == 0)
                {
                    myPhysicsScript.AddZForce(-force);
                }
                else if (downHeldTime == 0 && upHeldTime != 0)
                {
                    myPhysicsScript.AddZForce(force);
                }
            }
            
        }

        void FixedUpdate()
        {
            if (opHitBoxesScript == null)
            {
                if (opControlsScript == null) opControlsScript = opponent.GetComponent<ControlsScript>();
                opPhysicsScript = opponent.GetComponent<PhysicsScript>();
                opHitBoxesScript = opponent.GetComponentInChildren<HitBoxesScript>();
                opInfo = opControlsScript.myInfo;
                if (gameObject.name == "Player2" && character.name == opControlsScript.character.name)
                {  
                   // Alternative Costume
                   //Renderer charRender = character.GetComponentInChildren<Renderer>();
                   //charRender.material.color = myInfo.alternativeColor;

                    Renderer[] charRenders = character.GetComponentsInChildren<Renderer>();
                    foreach (Renderer charRender in charRenders)
                    {
                        //charRender.material.shader = Shader.Find("VertexLit");
                        charRender.material.color = myInfo.alternativeColor;
                        //charRender.material.SetColor("_Emission", myInfo.alternativeColor);
                    }
                }

                Renderer[] charRenderers = character.GetComponentsInChildren<Renderer>();
                List<Shader> shaderList = new List<Shader>();
                List<Color> colorList = new List<Color>();
                foreach (Renderer char_rend in charRenderers)
                {
                    shaderList.Add(char_rend.material.shader);
                    colorList.Add(char_rend.material.color);
                }
                normalShaders = shaderList.ToArray();
                normalColors = colorList.ToArray();
            }

            // 两个hitbox碰撞的越多，退的越远（攻击碰撞？）
            if (Vector3.Distance(transform.position, opponent.transform.position) < 10)
            {
                float totalHits = myHitBoxesScript.testCollision(opHitBoxesScript.hitBoxes);
                if (totalHits > 0)
                {
                    if (transform.position.x < opponent.transform.position.x)
                    {
                        transform.Translate(new Vector3(-.05f * totalHits, 0, 0));
                    }
                    else
                    {
                        transform.Translate(new Vector3(.05f * totalHits, 0, 0));
                    }
                }
            }

            if (currentMove != null)
            {
                
                /*debugger.text = "";
                if (storedMove != null) debugger.text += storedMove.name + "\n";
                debugger.text += currentMove.name +": "+ character.animation.IsPlaying(currentMove.name) + "\n";
                debugger.text += "frames:"+ currentMove.currentFrame + "/" + currentMove.totalFrames + "\n";
                debugger.text += "animationPaused:"+ animationPaused + "\n";
                if (character.animation.IsPlaying(currentMove.name)){
                    debugger.text += "normalizedTime: "+ character.animation[currentMove.name].normalizedTime + "\n";
                    debugger.text += "time: "+ character.animation[currentMove.name].time + "\n";
                }*/

                // 动作还没开始执行时，赋值动画参数
                if (currentMove.currentFrame == 0)
                {
                    if (character.GetComponent<Animation>()[currentMove.name] == null) Debug.LogError("Animation for move '" + currentMove.moveName + "' not found!");
                    character.GetComponent<Animation>()[currentMove.name].time = 0;
                    character.GetComponent<Animation>().CrossFade(currentMove.name, currentMove.interpolationSpeed);
                    character.GetComponent<Animation>()[currentMove.name].speed = currentMove.animationSpeed;
                }

                // ANIMATION FRAME DATA
                if (!animationPaused) currentMove.currentFrame++;
                //if (currentMove.currentFrame == 1) AddGauge(currentMove.gaugeGainOnMiss);
                // 根据配置的动画类型 设置当前动画的时间点
                //if (UFE.config.animationFlow == AnimationFlow.MorePrecision)
                //{
                //    character.GetComponent<Animation>()[currentMove.name].speed = 0;
                //    AnimationState animState = character.GetComponent<Animation>()[currentMove.name];
                //    animState.time = GetAnimationTime(currentMove.currentFrame);
                //    //animState.time = ((float)currentMove.currentFrame / (float)GlobalScript.prefs.framesPerSeconds) / (1/currentMove.animationSpeed);
                //}

                // 生成该动作的发射物
                foreach (Projectile projectile in currentMove.projectiles)
                {
                    if (!projectile.casted && currentMove.currentFrame >= projectile.castingFrame)
                    {
                        if (projectile.projectilePrefab == null) continue;
                        projectile.casted = true;

                        if (projectile.projectilePrefab == null)
                            Debug.LogError("Projectile prefab for move " + currentMove.moveName + " not found. Make sure you have set the prefab correctly in the Move Editor");
                        GameObject pTemp = (GameObject)Instantiate(projectile.projectilePrefab,
                                                                       projectile.position.position,
                                                                       Quaternion.Euler(0, 0, projectile.directionAngle));
                        pTemp.AddComponent<ProjectileMoveScript>();
                        ProjectileMoveScript pTempScript = pTemp.GetComponent<ProjectileMoveScript>();
                        pTempScript.data = projectile;
                        pTempScript.opHitBoxesScript = opHitBoxesScript;
                        pTempScript.opControlsScript = opControlsScript;
                        //pTempScript.mirror = mirror;
                    }
                }
                // 播放动作的特效
                //foreach (MoveParticleEffect particleEffect in currentMove.particleEffects)
                //{
                //    if (!particleEffect.casted && currentMove.currentFrame >= particleEffect.castingFrame)
                //    {
                //        if (particleEffect.particleEffect.prefab == null)
                //            Debug.LogError("Particle effect for move " + currentMove.moveName + " not found. Make sure you have set the prefab for this particle correctly in the Move Editor");
                //        particleEffect.casted = true;
                //        GameObject pTemp = (GameObject)Instantiate(particleEffect.particleEffect.prefab);
                //        pTemp.transform.parent = transform;
                //        pTemp.transform.localPosition = particleEffect.particleEffect.position;
                //        Destroy(pTemp, particleEffect.particleEffect.duration);
                //    }
                //}

                // 应用动作施加的力
                foreach (AppliedForce addedForce in currentMove.appliedForces)
                {
                    if (!addedForce.casted && currentMove.currentFrame >= addedForce.castingFrame)
                    {
                        myPhysicsScript.resetForces(addedForce.resetPreviousHorizontal, addedForce.resetPreviousVertical,true);
                        myPhysicsScript.addForce(addedForce.force, 1);
                        addedForce.casted = true;
                    }
                }

                // 播放动作的音效
                //foreach (SoundEffect soundEffect in currentMove.soundEffects)
                //{
                //    if (!soundEffect.casted && currentMove.currentFrame >= soundEffect.castingFrame)
                //    {
                //        if (UFE.config.soundfx) Camera.main.GetComponent<AudioSource>().PlayOneShot(soundEffect.sound);
                //        soundEffect.casted = true;
                //    }
                //}

                // 播放摄像机的移动
                //foreach (CameraMovement cameraMovement in currentMove.cameraMovements)
                //{
                //    if (currentMove.currentFrame >= cameraMovement.castingFrame)
                //    {
                //        cameraMovement.time += Time.deltaTime;
                //        if (!cameraMovement.casted)
                //        {
                //            myPhysicsScript.freeze = cameraMovement.freezeGame;
                //            opPhysicsScript.freeze = cameraMovement.freezeGame;
                //            LockCam(cameraMovement.freezeAnimation);
                //            cameraMovement.casted = true;
                //            Vector3 targetPosition = character.transform.TransformPoint(cameraMovement.position);
                //            Vector3 targetRotation = cameraMovement.rotation;
                //            targetRotation.y *= mirror;
                //            targetRotation.z *= mirror;
                //            cameraScript.moveCameraToLocation(targetPosition,
                //                                              targetRotation,
                //                                              cameraMovement.fieldOfView,
                //                                              cameraMovement.camSpeed);
                //        }
                //    }
                //    if (cameraMovement.casted && UFE.freeCamera && cameraMovement.time >= cameraMovement.duration)
                //    {
                //        ReleaseCam();
                //    }
                //}

                // 隐藏动作无敌部分的hitbox
                if (currentMove.invincibleBodyParts.Length > 0)
                {
                    foreach (InvincibleBodyParts invBodyPart in currentMove.invincibleBodyParts)
                    {
                        if (currentMove.currentFrame >= invBodyPart.activeFramesBegin &&
                            currentMove.currentFrame < invBodyPart.activeFramesEnds)
                        {
                            if (invBodyPart.completelyInvincible)
                            {
                                myHitBoxesScript.hideHitBoxes();
                            }
                            else
                            {
                                myHitBoxesScript.hideHitBoxes(invBodyPart.hitBoxes);
                            }
                        }
                        if (currentMove.currentFrame >= invBodyPart.activeFramesEnds)
                        {
                            if (invBodyPart.completelyInvincible)
                            {
                                myHitBoxesScript.showHitBoxes();
                            }
                            else
                            {
                                myHitBoxesScript.showHitBoxes(invBodyPart.hitBoxes);
                            }
                        }
                    }
                }
                // 防御区域判断
                //if (currentMove.blockableArea.bodyPart != BodyPart.none)
                //{
                //    if (currentMove.currentFrame >= currentMove.blockableArea.activeFramesBegin &&
                //        currentMove.currentFrame < currentMove.blockableArea.activeFramesEnds)
                //    {
                //        myHitBoxesScript.blockableArea = currentMove.blockableArea;
                //        Vector3 collisionVector_block = opHitBoxesScript.testCollision(myHitBoxesScript.blockableArea);
                //        if (collisionVector_block != Vector3.zero) opControlsScript.CheckBlocking(true);
                //    }
                //    else if (currentMove.currentFrame >= currentMove.blockableArea.activeFramesEnds)
                //    {
                //        opControlsScript.CheckBlocking(false);
                //    }
                //}

                // 动作的多段攻击？在动作文件中activeframe 中可以设置 hit数组
                foreach (Hit hit in currentMove.hits)
                {
                    //if (comboHits >= UFE.config.comboOptions.maxCombo) continue;
                    // 取消技，当播放到可以取消的帧的时候执行下个动作
                    if ((hit.hasHit && currentMove.frameLink.onlyOnHit) || !currentMove.frameLink.onlyOnHit)
                    {
                        if (currentMove.currentFrame >= currentMove.frameLink.activeFramesBegins) currentMove.cancelable = true;
                        if (currentMove.currentFrame >= currentMove.frameLink.activeFramesEnds) currentMove.cancelable = false;
                    }
                    if (hit.hasHit) continue;

                    if (currentMove.currentFrame >= hit.activeFramesBegin &&
                        currentMove.currentFrame < hit.activeFramesEnds)
                    {
                        if (hit.hurtBoxes.Length > 0)
                        {
                            myHitBoxesScript.activeHurtBoxes = hit.hurtBoxes;
                            // hurtbox判断，攻击中敌方
                            Vector3 collisionVector_hit = opHitBoxesScript.testCollision(myHitBoxesScript.activeHurtBoxes);
                            if (collisionVector_hit != Vector3.zero)
                            { // HURTBOX TEST
                              // 对手成功防御
                                //if (!opControlsScript.stunned && opControlsScript.currentMove == null && opControlsScript.isBlocking && opControlsScript.TestBlockStances(hit.hitType))
                                //{
                                //    opControlsScript.GetHitBlocking(hit, currentMove.totalFrames - currentMove.currentFrame, collisionVector_hit);
                                //    AddGauge(currentMove.gaugeGainOnBlock);
                                //    opControlsScript.AddGauge(currentMove.opGaugeGainOnBlock);
                                //    // 对手成功避开
                                //}
                                //else if (opControlsScript.potentialParry > 0 && opControlsScript.currentMove == null && opControlsScript.TestParryStances(hit.hitType))
                                //{
                                //    opControlsScript.GetHitParry(hit, collisionVector_hit);
                                //    opControlsScript.AddGauge(currentMove.opGaugeGainOnParry);
                                //}
                                //else
                                //{
                                    // 成功攻击到对手
                                    opControlsScript.GetHit(hit, currentMove.totalFrames - currentMove.currentFrame, collisionVector_hit);
                                    //AddGauge(currentMove.gaugeGainOnHit);
                                    // 攻击拉近？
                                    //if (hit.pullSelfIn.enemyBodyPart != BodyPart.none && hit.pullSelfIn.characterBodyPart != BodyPart.none)
                                    //{
                                    //    Vector3 newPos = opHitBoxesScript.getPosition(hit.pullSelfIn.enemyBodyPart);
                                    //    if (newPos != Vector3.zero)
                                    //    {
                                    //        pullInLocation = transform.position + (newPos - hit.pullSelfIn.position.position);
                                    //        pullInSpeed = hit.pullSelfIn.speed;
                                    //    }
                                    //}
                                //}

                                // 施加力
                                myPhysicsScript.resetForces(hit.resetPreviousHorizontal, hit.resetPreviousVertical,true);
                                myPhysicsScript.addForce(hit.appliedForce, 1);

                                // 碰到屏幕两边施加力
                                //if ((opponent.transform.position.x >= UFE.config.selectedStage.rightBoundary - 2 ||
                                //    opponent.transform.position.x <= UFE.config.selectedStage.leftBoundary + 2) &&
                                //    myPhysicsScript.isGrounded())
                                //{

                                //    myPhysicsScript.addForce(
                                //        new Vector2(hit.pushForce.x + (opPhysicsScript.airTime * opInfo.physics.friction), 0),
                                //        mirror * -1);
                                //}
                                // 场景抖动效果
                                //HitPause();
                                //Invoke("HitUnpause", GetFreezingTime(hit.hitStrengh));
                                if (!hit.continuousHit) hit.hasHit = true;
                            };
                        }
                    }
                }
                // 当前动作的帧播完
                if (currentMove.currentFrame >= currentMove.totalFrames)
                {
                    //if (currentMove == myMoveSetScript.getIntro()) introPlayed = true;
                    KillCurrentMove();
                }
                
            }
            myPhysicsScript.applyForces(currentMove);

            myPhysicsScript.resetForces(true,false, true);
        }

        // Imediately cancels any move being executed
        public void KillCurrentMove()
        {
            if (currentMove == null) return;

            currentMove.currentFrame = 0;
            myHitBoxesScript.activeHurtBoxes = null;
            //myHitBoxesScript.blockableArea = null;
            myHitBoxesScript.showHitBoxes();
            //opControlsScript.CheckBlocking(false);
            character.GetComponent<Animation>()[currentMove.name].speed = currentMove.animationSpeed;

            currentMove = null;
            //ReleaseCam();

        }

        /// <summary>
        /// 根据当前角色姿势判断要播放的动画
        /// 根据攻击类型播放特定特效
        /// 播放文字特效，音效
        /// 扣血计算
        /// 摄像机拉近效果
        /// 空中的连击重力判定
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="remainingFrames"></param>
        /// <param name="location"></param>
        public void GetHit(Hit hit, int remainingFrames, Vector3 location)
        {
            // 根据角色状态获取需要播放的动画
            // Get what animation should be played depending on the character's state
            if (myPhysicsScript.isGrounded())
            {
                //if (currentState == PossibleStates.Crouch)
                //{
                //    currentHitAnimation = "getHitLow";
                //}
                //else 
                if (hit.hitType == HitType.Launcher)
                {
                    currentHitAnimation = "getHitAir";
                }
                else
                {
                    currentHitAnimation = "getHitHigh";
                }
            }
            else
            {
                currentHitAnimation = "getHitAir";
            }

            // 拉扯技能？
            // Set position in case of pull enemy in
            if (hit.pullEnemyIn.enemyBodyPart != BodyPart.none && hit.pullEnemyIn.characterBodyPart != BodyPart.none)
            {
                Vector3 newPos = myHitBoxesScript.getPosition(hit.pullEnemyIn.enemyBodyPart);
                if (newPos != Vector3.zero)
                {
                    pullInLocation = transform.position + (hit.pullEnemyIn.position.position - newPos);
                    pullInSpeed = hit.pullEnemyIn.speed;
                }
            }
            // 不同攻击类型，不同特效？
            // Differenciate hit types
            GameObject hitEffect = null;
            float effectKillTime = 0;
            if (hit.hitStrengh == HitStrengh.Weak) hitEffect = GetHitData(UFE.config.hitOptions.weakHit, ref effectKillTime);
            if (hit.hitStrengh == HitStrengh.Medium) hitEffect = GetHitData(UFE.config.hitOptions.mediumHit, ref effectKillTime);
            if (hit.hitStrengh == HitStrengh.Heavy) hitEffect = GetHitData(UFE.config.hitOptions.heavyHit, ref effectKillTime);
            if (hit.hitStrengh == HitStrengh.Crumple) hitEffect = GetHitData(UFE.config.hitOptions.crumpleHit, ref effectKillTime);
            if (hit.hitStrengh == HitStrengh.Custom1) hitEffect = GetHitData(UFE.config.hitOptions.customHit1, ref effectKillTime);
            if (hit.hitStrengh == HitStrengh.Custom2) hitEffect = GetHitData(UFE.config.hitOptions.customHit2, ref effectKillTime);
            if (hit.hitStrengh == HitStrengh.Custom3) hitEffect = GetHitData(UFE.config.hitOptions.customHit3, ref effectKillTime);

            // Cancel current move if any
            if (!hit.armorBreaker && currentMove != null && currentMove.armor > 0)
            {
                currentMove.armor--;
            }
            else
            {
                storedMove = null;
                KillCurrentMove();
            }

            // Create hit effect
            if (location != Vector3.zero && hitEffect != null)
            {
                GameObject pTemp = (GameObject)Instantiate(hitEffect, location, Quaternion.identity);
                Destroy(pTemp, effectKillTime);
            }

            // 打击时文字特效
            // Cast First Hit if true
            //if (!firstHit && !opControlsScript.firstHit)
            //{
            //    opControlsScript.firstHit = true;
            //    UFE.FireAlert(SetStringValues(UFE.config.selectedLanguage.firstHit, opInfo), opInfo);
            //}
            //UFE.FireHit(myHitBoxesScript.getStrokeHitBox(), opControlsScript.currentMove, opInfo);

            //打击扣血计算
            // Convert Percentage
            if (hit.damageType == DamageType.Percentage) hit.damageOnHit = myInfo.lifePoints * (hit.damageOnHit / 100);

            // Damage deterioration
            float damage = 0;
            if (!hit.damageScaling || UFE.config.comboOptions.damageDeterioration == Sizes.None)
            {
                damage = hit.damageOnHit;
            }
            else if (UFE.config.comboOptions.damageDeterioration == Sizes.Small)
            {
                damage = hit.damageOnHit - (hit.damageOnHit * (float)comboHits * .1f);
            }
            else if (UFE.config.comboOptions.damageDeterioration == Sizes.Medium)
            {
                damage = hit.damageOnHit - (hit.damageOnHit * (float)comboHits * .2f);
            }
            else if (UFE.config.comboOptions.damageDeterioration == Sizes.High)
            {
                damage = hit.damageOnHit - (hit.damageOnHit * (float)comboHits * .4f);
            }
            if (damage < UFE.config.comboOptions.minDamage) damage = UFE.config.comboOptions.minDamage;

            // Lose life
            //isDead = DamageMe(damage);


            // Stun
            // Hit stun deterioration (the longer the combo gets, the harder it is to combo)
            stunned = true;

            int stunFrames = 0;
            if (hit.hitStunType == HitStunType.FrameAdvantage)
            {
                stunFrames = hit.frameAdvantageOnHit + remainingFrames;
            }
            else
            {
                stunFrames = hit.hitStunOnHit;
            }

            if (stunFrames < 1) stunFrames = 1;
            if (stunFrames < UFE.config.comboOptions.minHitStun) stunTime = UFE.config.comboOptions.minHitStun;
            stunTime = (float)stunFrames / (float)UFE.config.fps;
            if (!hit.resetPreviousHitStun)
            {
                if (UFE.config.comboOptions.hitStunDeterioration == Sizes.Small)
                {
                    stunTime -= (float)comboHits * .01f;
                }
                else if (UFE.config.comboOptions.hitStunDeterioration == Sizes.Medium)
                {
                    stunTime -= (float)comboHits * .02f;
                }
                else if (UFE.config.comboOptions.hitStunDeterioration == Sizes.High)
                {
                    stunTime -= (float)comboHits * .04f;
                }
            }
            comboHits++;
            if (isDead) stunTime = 999;

            // 致眩晕时播放动画减速
            // Set deceleration of hit stun animation so it can look more natural
            hitStunDeceleration = character.GetComponent<Animation>()[currentHitAnimation].length / Mathf.Pow(stunTime, 2);

            // Stop any previous hit stun and play animation at hit animation speed
            character.GetComponent<Animation>().Stop(currentHitAnimation);
            character.GetComponent<Animation>()[currentHitAnimation].speed = (character.GetComponent<Animation>()[currentHitAnimation].length / stunTime) * 1.5f;
            character.GetComponent<Animation>().Play(currentHitAnimation);

            // 锁屏，拉近摄像机
            // Freeze screen depending on how strong the hit was
            //HitPause();
            //Invoke("HitUnpause", GetFreezingTime(hit.hitStrengh));

            // hit state 1 -> 0
            // Reset hit to allow for another hit while the character is still stunned
            float freezingTime = GetFreezingTime(hit.hitStrengh) * 1.2f;
            myHitBoxesScript.Invoke("resetHit", freezingTime);

            // 给动作添加力
            // Add force to the move		
            // Air juggle deterioration (the longer the combo, the harder it is to push the opponent higher)
            float verticalPush = hit.pushForce.y;
            if (verticalPush > 0 || isDead ||
                hit.hitType == HitType.HardKnockdown ||
                hit.hitType == HitType.Knockdown)
            {
                if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.None)
                {
                    verticalPush = hit.pushForce.y;
                }
                else if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.Small)
                {
                    verticalPush = hit.pushForce.y - (hit.pushForce.y * (float)comboHits * .1f);
                }
                else if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.Medium)
                {
                    verticalPush = hit.pushForce.y - (hit.pushForce.y * (float)comboHits * .2f);
                }
                else if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.High)
                {
                    verticalPush = hit.pushForce.y - (hit.pushForce.y * (float)comboHits * .4f);
                }
                if (verticalPush < UFE.config.comboOptions.minPushForce) verticalPush = UFE.config.comboOptions.minPushForce;
            }

            if (hit.hitType == HitType.Knockdown || hit.hitType == HitType.HardKnockdown) myPhysicsScript.resetForces(true, true,true);

            myPhysicsScript.resetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush,true);
            myPhysicsScript.addForce(new Vector2(hit.pushForce.x, verticalPush), 1);
        }

        private GameObject GetHitData(HitTypeOptions hitTypeOptions, ref float killTime)
        {
            //shakeCamera = hitTypeOptions.shakeCameraOnHit;
            //shakeCharacter = hitTypeOptions.shakeCharacterOnHit;
            //shakeDensity = hitTypeOptions.shakeDensity;

            if (hitTypeOptions.hitSound != null)
                if (UFE.config.soundfx) Camera.main.GetComponent<AudioSource>().PlayOneShot(hitTypeOptions.hitSound);

            killTime = hitTypeOptions.killTime;
            return hitTypeOptions.hitParticle;

        }

        // 暂停动画和物理系统展示一个场景震动
        // Pause animations and physics to create a sense of impact
        void HitPause()
        {
            Camera.main.transform.position += Vector3.forward / 2;
            myPhysicsScript.freeze = true;
            PausePlayAnimation(true);
        }
        // Method to pause animations and return them to their prior speed accordly
        private void PausePlayAnimation(bool pause)
        {
            if (pause)
            {
                int i = 0;
                foreach (AnimationState animState in character.GetComponent<Animation>())
                {
                    if (animState.speed != 0.005f) myMoveSetScript.animSpeedStorage[i] = animState.speed;
                    animState.speed = 0.005f;
                    i++;
                }
                animationPaused = true;
            }
            else if (animationPaused)
            {
                int i = 0;
                foreach (AnimationState animState in character.GetComponent<Animation>())
                {
                    if (animState.speed == 0.005f) animState.speed = myMoveSetScript.animSpeedStorage[i];
                    i++;
                }
                animationPaused = false;
            }
        }
        // Unpauses the pause
        void HitUnpause()
        {
            PausePlayAnimation(false);
            myPhysicsScript.freeze = false;
        }

        private bool DamageMe(float damage)
        {
            if (UFE.config.trainingMode) return false;
            if (myInfo.currentLifePoints <= 0 || opInfo.currentLifePoints <= 0) return true;
            myInfo.currentLifePoints -= damage;
            //UFE.SetLifePoints(myInfo.currentLifePoints, myInfo);
            if (myInfo.currentLifePoints <= 0 && UFE.config.roundOptions.slowMotionKO)
            {
                //opControlsScript.roundsWon++;
                if (UFE.config.soundfx) Camera.main.GetComponent<AudioSource>().PlayOneShot(myInfo.deathSound);

                //storedAnimationFlow = UFE.config.animationFlow;
                //UFE.config.animationFlow = AnimationFlow.Smoother;

                Time.timeScale = Time.timeScale * .2f;
                Invoke("ReturnTimeScale", .4f); // Low timer to account for the slowmotion

                return true;
            }
            else if (myInfo.currentLifePoints <= 0)
            {
                //opControlsScript.roundsWon++;
                Invoke("EndRound", 3);
                return true;
            }
            return false;
        }

        // Get amount of freezing time depending on the strenght of the move
        public float GetFreezingTime(HitStrengh hitStrengh)
        {
            if (hitStrengh == HitStrengh.Weak)
            {
                return UFE.config.hitOptions.weakHit.freezingTime;
            }
            else if (hitStrengh == HitStrengh.Medium)
            {
                return UFE.config.hitOptions.mediumHit.freezingTime;
            }
            else if (hitStrengh == HitStrengh.Heavy)
            {
                return UFE.config.hitOptions.heavyHit.freezingTime;
            }
            else if (hitStrengh == HitStrengh.Crumple)
            {
                return UFE.config.hitOptions.crumpleHit.freezingTime;
            }
            return 0;
        }

        public float GetAnimationTime(int animFrame)
        {
            if (currentMove == null) return 0;
            if (currentMove.animationSpeed < 0)
            {
                return (((float)animFrame / (float)UFE.config.fps) * currentMove.animationSpeed) + currentMove.animationClip.length;
            }
            else
            {
                return ((float)animFrame / (float)UFE.config.fps) * currentMove.animationSpeed;
            }
        }
    }

}

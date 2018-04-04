using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyFightGame {

    [System.Serializable]
    public class InputReferences
    {
        public InputType inputType;
        public string inputButtonName;
        public ButtonPress engineRelatedButton;

        [HideInInspector]
        public float heldDown;
    }
    public class ControlsScript : MonoBehaviour
    {
        public GameObject character;
        public CharacterInfo myInfo;

        public PossibleStates currentState;
        public MoveInfo currentMove;
        public MoveInfo storedMove;

        public bool stunned;
        public float stunTime;
        public bool isBlocking;

        private InputReferences[] inputReferences;

        private PhysicsScript myPhysicsScript;
        private MoveSetScript myMoveSetScript;

        private float storedMoveTime;
        private bool animationPaused;

        // Use this for initialization
        void Start()
        {
            inputReferences = UFE.inputConfig.inputReferences;

            myPhysicsScript = GetComponent<PhysicsScript>();

            myInfo = (CharacterInfo)Instantiate(UFE.config.player1Character);

            character = (GameObject)Instantiate(myInfo.characterPrefab);
            character.transform.parent = transform;
            character.AddComponent<MoveSetScript>();
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
		if ((currentMove == null || currentMove.cancelable) && storedMove != null && !myPhysicsScript.freeze) {
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

            foreach (InputReferences inputRef in inputReferences)
            {
                // 执行方向键动作
                if (inputRef.inputType != InputType.Button && inputRef.heldDown > 0 && Input.GetAxisRaw(inputRef.inputButtonName) == 0)
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

                if (inputRef.inputType != InputType.Button && Input.GetAxisRaw(inputRef.inputButtonName) != 0)
                {
                    bool axisPressed = false;
                    if (inputRef.inputType == InputType.HorizontalAxis)
                    {
                        float direction = Input.GetAxisRaw(inputRef.inputButtonName);
                        if (direction > 0)
                        {
                            inputRef.engineRelatedButton = ButtonPress.Right;
                        }
                        else
                        {
                            inputRef.engineRelatedButton = ButtonPress.Left;
                        }

                        if (inputRef.heldDown == 0) axisPressed = true;
                        inputRef.heldDown += Time.deltaTime;
                        myPhysicsScript.xMove(direction);
                        inputRef.heldDown = 0;
                    }
                    else
                    {
                        float direction = Input.GetAxisRaw(inputRef.inputButtonName);
                        if (direction > 0)
                        {
                            inputRef.engineRelatedButton = ButtonPress.Up;
                        }
                        else
                        {
                            inputRef.engineRelatedButton = ButtonPress.Down;
                        }

                        if (inputRef.heldDown == 0) axisPressed = true;
                        inputRef.heldDown += Time.deltaTime;
                        myPhysicsScript.zMove(direction);
                        inputRef.heldDown = 0;
                    }

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
                }
            }
            myPhysicsScript.applyForces(currentMove);
            myPhysicsScript.resetForces(true, true);
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
        // Imediately cancels any move being executed
        public void KillCurrentMove()
        {
            if (currentMove == null) return;

            currentMove.currentFrame = 0;
            //myHitBoxesScript.activeHurtBoxes = null;
            //myHitBoxesScript.blockableArea = null;
            //myHitBoxesScript.showHitBoxes();
            //opControlsScript.CheckBlocking(false);
            character.GetComponent<Animation>()[currentMove.name].speed = currentMove.animationSpeed;

            currentMove = null;
            //ReleaseCam();

        }

        void FixedUpdate()
        {
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
                //foreach (Projectile projectile in currentMove.projectiles)
                //{
                //    if (!projectile.casted && currentMove.currentFrame >= projectile.castingFrame)
                //    {
                //        if (projectile.projectilePrefab == null) continue;
                //        projectile.casted = true;

                //        if (projectile.projectilePrefab == null)
                //            Debug.LogError("Projectile prefab for move " + currentMove.moveName + " not found. Make sure you have set the prefab correctly in the Move Editor");
                //        GameObject pTemp = (GameObject)Instantiate(projectile.projectilePrefab,
                //                                                       projectile.position.position,
                //                                                       Quaternion.Euler(0, 0, projectile.directionAngle));
                //        pTemp.AddComponent<ProjectileMoveScript>();
                //        ProjectileMoveScript pTempScript = pTemp.GetComponent<ProjectileMoveScript>();
                //        pTempScript.data = projectile;
                //        pTempScript.opHitBoxesScript = opHitBoxesScript;
                //        pTempScript.opControlsScript = opControlsScript;
                //        pTempScript.mirror = mirror;
                //    }
                //}
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
                //foreach (AppliedForce addedForce in currentMove.appliedForces)
                //{
                //    if (!addedForce.casted && currentMove.currentFrame >= addedForce.castingFrame)
                //    {
                //        myPhysicsScript.resetForces(addedForce.resetPreviousHorizontal, addedForce.resetPreviousVertical);
                //        myPhysicsScript.addForce(addedForce.force, 1);
                //        addedForce.casted = true;
                //    }
                //}

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
                //if (currentMove.invincibleBodyParts.Length > 0)
                //{
                //    foreach (InvincibleBodyParts invBodyPart in currentMove.invincibleBodyParts)
                //    {
                //        if (currentMove.currentFrame >= invBodyPart.activeFramesBegin &&
                //            currentMove.currentFrame < invBodyPart.activeFramesEnds)
                //        {
                //            if (invBodyPart.completelyInvincible)
                //            {
                //                myHitBoxesScript.hideHitBoxes();
                //            }
                //            else
                //            {
                //                myHitBoxesScript.hideHitBoxes(invBodyPart.hitBoxes);
                //            }
                //        }
                //        if (currentMove.currentFrame >= invBodyPart.activeFramesEnds)
                //        {
                //            if (invBodyPart.completelyInvincible)
                //            {
                //                myHitBoxesScript.showHitBoxes();
                //            }
                //            else
                //            {
                //                myHitBoxesScript.showHitBoxes(invBodyPart.hitBoxes);
                //            }
                //        }
                //    }
                //}
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
                //foreach (Hit hit in currentMove.hits)
                //{
                //    if (comboHits >= UFE.config.comboOptions.maxCombo) continue;
                //    // 取消技，当播放到可以取消的帧的时候执行下个动作
                //    if ((hit.hasHit && currentMove.frameLink.onlyOnHit) || !currentMove.frameLink.onlyOnHit)
                //    {
                //        if (currentMove.currentFrame >= currentMove.frameLink.activeFramesBegins) currentMove.cancelable = true;
                //        if (currentMove.currentFrame >= currentMove.frameLink.activeFramesEnds) currentMove.cancelable = false;
                //    }
                //    if (hit.hasHit) continue;

                //    if (currentMove.currentFrame >= hit.activeFramesBegin &&
                //        currentMove.currentFrame < hit.activeFramesEnds)
                //    {
                //        if (hit.hurtBoxes.Length > 0)
                //        {
                //            myHitBoxesScript.activeHurtBoxes = hit.hurtBoxes;
                //            // hurtbox判断，攻击中敌方
                //            Vector3 collisionVector_hit = opHitBoxesScript.testCollision(myHitBoxesScript.activeHurtBoxes);
                //            if (collisionVector_hit != Vector3.zero)
                //            { // HURTBOX TEST
                //              // 对手成功防御
                //                if (!opControlsScript.stunned && opControlsScript.currentMove == null && opControlsScript.isBlocking && opControlsScript.TestBlockStances(hit.hitType))
                //                {
                //                    opControlsScript.GetHitBlocking(hit, currentMove.totalFrames - currentMove.currentFrame, collisionVector_hit);
                //                    AddGauge(currentMove.gaugeGainOnBlock);
                //                    opControlsScript.AddGauge(currentMove.opGaugeGainOnBlock);
                //                    // 对手成功避开
                //                }
                //                else if (opControlsScript.potentialParry > 0 && opControlsScript.currentMove == null && opControlsScript.TestParryStances(hit.hitType))
                //                {
                //                    opControlsScript.GetHitParry(hit, collisionVector_hit);
                //                    opControlsScript.AddGauge(currentMove.opGaugeGainOnParry);
                //                }
                //                else
                //                {
                //                    // 成功攻击到对手
                //                    opControlsScript.GetHit(hit, currentMove.totalFrames - currentMove.currentFrame, collisionVector_hit);
                //                    AddGauge(currentMove.gaugeGainOnHit);
                //                    // 攻击拉近？
                //                    if (hit.pullSelfIn.enemyBodyPart != BodyPart.none && hit.pullSelfIn.characterBodyPart != BodyPart.none)
                //                    {
                //                        Vector3 newPos = opHitBoxesScript.getPosition(hit.pullSelfIn.enemyBodyPart);
                //                        if (newPos != Vector3.zero)
                //                        {
                //                            pullInLocation = transform.position + (newPos - hit.pullSelfIn.position.position);
                //                            pullInSpeed = hit.pullSelfIn.speed;
                //                        }
                //                    }
                //                }

                //                // 施加力
                //                myPhysicsScript.resetForces(hit.resetPreviousHorizontal, hit.resetPreviousVertical);
                //                myPhysicsScript.addForce(hit.appliedForce, mirror);

                //                // 碰到屏幕两边施加力
                //                if ((opponent.transform.position.x >= UFE.config.selectedStage.rightBoundary - 2 ||
                //                    opponent.transform.position.x <= UFE.config.selectedStage.leftBoundary + 2) &&
                //                    myPhysicsScript.isGrounded())
                //                {

                //                    myPhysicsScript.addForce(
                //                        new Vector2(hit.pushForce.x + (opPhysicsScript.airTime * opInfo.physics.friction), 0),
                //                        mirror * -1);
                //                }
                //                // 场景抖动效果
                //                HitPause();
                //                Invoke("HitUnpause", GetFreezingTime(hit.hitStrengh));
                //                if (!hit.continuousHit) hit.hasHit = true;
                //            };
                //        }
                //    }
                //}
                // 当前动作的帧播完
                if (currentMove.currentFrame >= currentMove.totalFrames)
                {
                    //if (currentMove == myMoveSetScript.getIntro()) introPlayed = true;
                    KillCurrentMove();
                }
            }

            myPhysicsScript.applyForces(currentMove);
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

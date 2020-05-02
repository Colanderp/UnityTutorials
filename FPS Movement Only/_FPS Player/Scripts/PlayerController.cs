using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum Status { idle, walking, crouching, sprinting, sliding, climbingLadder, wallRunning, vaulting, grabbedLedge, climbingLedge, surfaceSwimming, underwaterSwimming }
public class StatusEvent : UnityEvent<Status, Func<IKData>> { }
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
    public Status status;
    [SerializeField]
    private LayerMask collisionLayer; //Default
    [SerializeField]
    private LayerMask vaultLayer;
    [SerializeField]
    private LayerMask ledgeLayer;
    [SerializeField]
    private LayerMask ladderLayer;
    [SerializeField]
    private LayerMask wallrunLayer;
    [SerializeField]
    private LayerMask topWaterLayer;
    [SerializeField]
    private float crouchHeight = 1f;
    [SerializeField]
    private float sprintTime = 6f;
    [SerializeField]
    private float sprintReserve = 4f;
    [SerializeField]
    private float sprintMinimum = 2f;
    [SerializeField]
    private float wallrunMinimum = 0.2f;

    GameObject vaultHelper;

    Vector3 wallNormal = Vector3.zero;
    Vector3 ladderNormal = Vector3.zero;
    Vector3 pushFrom;
    Vector3 slideDir;
    Vector3 vaultOver;
    Vector3 vaultDir;

    new CameraMovement camera;
    PlayerMovement movement;
    PlayerInput playerInput;
    AnimateLean animateLean;
    AnimateCameraLevel animateCamLevel;

    bool canInteract;
    bool canGrabLedge;
    bool controlledSlide;
    bool isInWater;
    bool canJumpOutOfWater;
    bool forceSprintReserve = false;

    float rayDistance;
    float slideLimit;
    float slideTime;
    float radius;
    float height;
    float halfradius;
    float halfheight;
    float crouchCamAdjust;
    float treadTime;
    float stamina;
    float slideBlendTime = 0.222f;
    float slideDownward = 0f;
    float wallrunTime;

    int wallDir = 1;
    public StatusEvent onStatusChange;

    void ChangeStatus(Status s)
    {
        if (status == s) return;
        status = s;
        if (onStatusChange != null)
            onStatusChange.Invoke(status, null);
    }
    void ChangeStatus(Status s, Func<IKData> call)
    {
        if (status == s) return;
        status = s;
        if (onStatusChange != null)
            onStatusChange.Invoke(status, call);
    }

    public void AddToStatusChange(UnityAction<Status, Func<IKData>> action)
    {
        if(onStatusChange == null)
            onStatusChange = new StatusEvent();

        onStatusChange.AddListener(action);
    }

    private void Start()
    {
        CreateVaultHelper();
        playerInput = GetComponent<PlayerInput>();

        movement = GetComponent<PlayerMovement>();
        movement.AddToReset(() => { status = Status.walking; });

        camera = GetComponentInChildren<CameraMovement>();

        if (GetComponentInChildren<AnimateLean>())
            animateLean = GetComponentInChildren<AnimateLean>();
        if (GetComponentInChildren<AnimateCameraLevel>())
            animateCamLevel = GetComponentInChildren<AnimateCameraLevel>();

        slideLimit = movement.controller.slopeLimit - .1f;
        radius = movement.controller.radius;
        height = movement.controller.height;
        halfradius = radius / 2f;
        halfheight = height / 2f;
        rayDistance = halfheight + radius + .175f;
        crouchCamAdjust = (crouchHeight - height) / 2f;
        stamina = sprintTime;
    }

    /******************************* UPDATE ******************************/
    void Update()
    {
        //Updates
        UpdateInteraction();
        UpdateMovingStatus();

        if((int)status < 9) //If we are not swimming
        {
            //Check for movement updates
            CheckSliding();
            CheckCrouching();
            CheckForWallrun();
            CheckLadderClimbing();
            UpdateLedgeGrabbing();
            CheckForVault();
            //Add new check to change status right here
        }

        //Misc
        UpdateLean();
        UpdateCamLevel();
    }

    void UpdateInteraction()
    {
        if ((int)status >= 5)
            canInteract = false;
        else if (!canInteract)
        {
            if (movement.grounded || movement.moveDirection.y < 0)
                canInteract = true;
        }
    }

    void UpdateMovingStatus()
    {
        if (status == Status.sprinting && stamina > 0)
            stamina -= Time.deltaTime;
        else if (stamina < sprintTime)
            stamina += Time.deltaTime;

        if ((int)status <= 1 || isSprinting())
        {
            if (playerInput.input.magnitude > 0.02f)
                ChangeStatus((shouldSprint()) ? Status.sprinting : Status.walking);
            else
                ChangeStatus(Status.idle);
        }
    }

    bool shouldSprint()
    {
        bool sprint = false;
        sprint = (playerInput.run && playerInput.input.y > 0);
        if (status != Status.sliding)
        {
            if (!isSprinting()) //If we want to sprint
            {
                if (forceSprintReserve && stamina < sprintReserve)
                    return false;
                else if (!forceSprintReserve && stamina < sprintMinimum)
                    return false;
            }
            if (stamina <= 0)
            {
                forceSprintReserve = true;
                return false;
            }
        }
        if (sprint)
            forceSprintReserve = false;
        return sprint;
    }

    void UpdateLean()
    {
        if (animateLean == null) return;
        Vector2 lean = Vector2.zero;
        if (status == Status.wallRunning)
            lean.x = wallDir;
        if (status == Status.sliding)
            lean.y = -1;
        else if (status == Status.grabbedLedge || status == Status.vaulting)
            lean.y = 1;
        animateLean.SetLean(lean);
    }

    void UpdateCamLevel()
    {
        if (animateCamLevel == null) return;

        float level = 0f;
        if(status == Status.crouching || status == Status.sliding || status == Status.vaulting || status == Status.climbingLedge)
            level = crouchCamAdjust;
        animateCamLevel.UpdateLevel(level);
    }
    /*********************************************************************/


    /******************************** MOVE *******************************/
    void FixedUpdate()
    {
        switch (status)
        {
            case Status.surfaceSwimming:
                SurfaceSwimmingMovement();
                break;
            case Status.underwaterSwimming:
                UnderwaterSwimmingMovement();
                break;
            case Status.sliding:
                SlideMovement();
                break;
            case Status.climbingLadder:
                LadderMovement();
                break;
            case Status.grabbedLedge:
                GrabbedLedgeMovement();
                break;
            case Status.climbingLedge:
                ClimbLedgeMovement();
                break;
            case Status.wallRunning:
                WallrunningMovement();
                break;
            case Status.vaulting:
                VaultMovement();
                break;
            default:
                DefaultMovement();
                break;
        }
    }

    void DefaultMovement()
    {
        if (isSprinting() && isCrouching())
            Uncrouch();

        movement.Move(playerInput.input, isSprinting(), isCrouching());
        if (movement.grounded && playerInput.Jump())
        {
            if (status == Status.crouching)
                Uncrouch();

            movement.Jump(Vector3.up, 1f);
            playerInput.ResetJump();
        }
    }

    public bool isSprinting()
    {
        return (status == Status.sprinting && movement.grounded);
    }

    public bool isWalking()
    {
        if (status == Status.walking || status == Status.crouching)
            return (movement.controller.velocity.magnitude > 0f && movement.grounded);
        else
            return false;
    }
    public bool isCrouching()
    {
        return (status == Status.crouching);
    }
    /*********************************************************************/

    /****************************** SLIDING ******************************/
    void SlideMovement()
    {
        if (movement.grounded && playerInput.Jump())
        {
            if (controlledSlide)
                slideDir = transform.forward;
            movement.Jump(slideDir + Vector3.up, 1f);
            playerInput.ResetJump();
            slideTime = 0;
        }

        float blend = Mathf.Clamp(slideTime, 0f, slideBlendTime) / slideBlendTime;
        float slideSpeed = Mathf.Lerp(movement.slideSpeed.min, movement.slideSpeed.max, slideDownward);
        movement.Move(slideDir, slideSpeed * blend, 1f, slideDir.y);
    }

    void CheckSliding()
    {
        Vector3 slideOnGround = transform.forward;
        if (Physics.Raycast(transform.position, -Vector3.up, out var hit, rayDistance, collisionLayer)) //Don't hit the player
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            Vector3 hitNormal = hit.normal;

            Vector3 slopeDir = Vector3.ClampMagnitude(new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z), 1f);
            Vector3.OrthoNormalize(ref hitNormal, ref slopeDir);
            Vector3.OrthoNormalize(ref hitNormal, ref slideOnGround);

            Debug.DrawRay(transform.position - Vector3.up * halfheight, slideOnGround, Color.red);
            Debug.DrawRay(transform.position - Vector3.up * halfheight, slopeDir, Color.blue);

            if (angle > 0 && status == Status.sliding) //Adjust to slope direction
            {
                Debug.DrawRay(transform.position - Vector3.up * halfheight, slideDir, Color.green);
                slideDir = Vector3.RotateTowards(slideDir, slopeDir, movement.slideSpeed.min * Time.deltaTime / 2f, 0.0f);
            }
            else
                slideDir.y = 0;

            if (angle > slideLimit && status != Status.sliding)
            {
                Crouch();
                slideDir = slopeDir;
                controlledSlide = false;
                slideTime = slideBlendTime;
                ChangeStatus(Status.sliding, SlideIK);
            }
        }
        else if (status == Status.sliding)
        {
            slideDir.y = 0;
            slideDir = slideDir.normalized;
            Debug.DrawRay(transform.position - Vector3.up * halfheight, slideDir, Color.black);
            slideDownward = 0f;
        }

        //Check to slide when running
        if (playerInput.crouch && canSlide())
        {
            ChangeStatus(Status.sliding, SlideIK);
            slideDir = slideOnGround;
            movement.controller.height = crouchHeight;
            controlledSlide = true;
            slideDownward = 0f;
            slideTime = 1f;
        }

        //Lower slidetime
        if (slideTime > 0)
        {
            if (slideDir.y < 0)
            {
                slideDownward = Mathf.Clamp(slideDownward + Time.deltaTime * Mathf.Sqrt(Mathf.Abs(slideDir.y)), 0f, 1f);
                if (slideTime <= slideBlendTime)
                    slideTime += Time.deltaTime;
            }
            else
            {
                slideDownward = Mathf.Clamp(slideDownward - Time.deltaTime, 0f, 1f);
                slideTime -= Time.deltaTime;
            }

            if (controlledSlide && slideTime <= slideBlendTime)
            {
                if (shouldSprint() && Uncrouch())
                    ChangeStatus(Status.sprinting);
            }
        }
        else if (status == Status.sliding)
        {
            if (playerInput.crouching)
                Crouch();
            else if (!Uncrouch()) //Try to uncrouch, if this is false then we cannot uncrouch
                Crouch(); //So just keep crouched
        }
    }

    bool canSlide()
    {
        if (!isSprinting()) return false;
        if (slideTime > 0 || status == Status.sliding) return false;
        return true;
    }

    Vector3 groundPos;
    IKData SlideIK()
    {
        IKData data = new IKData();
        Vector3 dir = Vector3.Cross(slideDir, Vector3.up);
        if (Physics.Raycast(transform.position + ((slideDir + dir) * radius), -Vector3.up, out var hit, 1f))
            groundPos = hit.point;
        data.handPos = groundPos;
        data.handEulerAngles = Quaternion.LookRotation(dir, Vector3.up).eulerAngles;

        data.armElbowPos = transform.position - ((transform.right - Vector3.up) * radius);
        data.armLocalPos.x = -0.35f;
        return data;
    }
    /*********************************************************************/

    /***************************** CROUCHING *****************************/
    void CheckCrouching()
    {
        if (!movement.grounded || (int)status > 2) return;

        if (playerInput.run)
        {
            Uncrouch();
            return;
        }

        if (playerInput.crouch)
        {
            if (status != Status.crouching)
                Crouch();
            else
                Uncrouch();
        }
    }

    void Crouch()
    {
        movement.controller.height = crouchHeight;
        ChangeStatus(Status.crouching);
    }

    bool Uncrouch()
    {
        Vector3 bottom = transform.position - (Vector3.up * ((crouchHeight / 2) - radius));
        bool isBlocked = Physics.SphereCast(bottom, radius, Vector3.up, out var hit, height - radius);
        if (isBlocked) return false; //If we have something above us, do nothing and return
        movement.controller.height = height;
        ChangeStatus(Status.walking);
        return true;
    }
    /*********************************************************************/

    /************************** LADDER CLIMBING **************************/
    void LadderMovement()
    {
        Vector3 input = playerInput.input;
        Vector3 move = Vector3.Cross(Vector3.up, ladderNormal).normalized;
        move *= input.x;
        move.y = input.y * movement.walkSpeed;

        bool goToGround = false;
        goToGround = (move.y < -0.02f && movement.grounded);

        if (playerInput.Jump())
        {
            movement.Jump((-ladderNormal + Vector3.up * 2f).normalized, 1f);
            playerInput.ResetJump();
            ChangeStatus(Status.walking);
        }

        if (!hasObjectInfront(0.05f, ladderLayer) || goToGround)
        {
            ChangeStatus(Status.walking);
            Vector3 pushUp = ladderNormal;
            pushUp.y = 0.25f;

            movement.ForceMove(pushUp, movement.walkSpeed, 0.25f, true);
        }
        else
            movement.Move(move, 1f, 0f);
    }

    void CheckLadderClimbing()
    {
        if (!canInteract)
            return;
        //Check for ladder all across player (so they cannot use the side)
        bool right = Physics.Raycast(transform.position + (transform.right * halfradius), transform.forward, radius + 0.125f, ladderLayer);
        bool left = Physics.Raycast(transform.position - (transform.right * halfradius), transform.forward, radius + 0.125f, ladderLayer);

        if (Physics.Raycast(transform.position, transform.forward, out var hit, radius + 0.125f, ladderLayer) && right && left)
        {
            if (hit.normal != hit.transform.forward) return;

            ladderNormal = -hit.normal;
            if (hasObjectInfront(0.05f, ladderLayer) && playerInput.input.y > 0.02f)
                ChangeStatus(Status.climbingLadder, LadderIK);
        }
    }

    Vector3 lastTouch = Vector3.zero;
    IKData LadderIK()
    {
        IKData data = new IKData();
        Vector3 upOffset = Vector3.up * radius * 2f;
        Vector3 handUp = Vector3.Cross(ladderNormal, Vector3.up);
        if (Physics.SphereCast(transform.position + upOffset, radius, ladderNormal, out var hit, 0.125f, ladderLayer))
        {
            if (Physics.SphereCast(hit.point + handUp, 0.125f, -handUp, out var hit2, 1.125f, ladderLayer))
               lastTouch = hit2.point - (ladderNormal * 0.125f);
        }
        lastTouch.y = (int)(lastTouch.y * 2f) / 2f;
        data.handPos = lastTouch;

        data.handEulerAngles = Quaternion.LookRotation(ladderNormal, handUp).eulerAngles;
        data.armElbowPos = transform.position + handUp * radius;
        data.armLocalPos.x = -0.35f;
        return data;
    }
    /*********************************************************************/

    /**************************** WALLRUNNING ****************************/
    void WallrunningMovement()
    {
        Vector3 input = playerInput.input;
        float s = (input.y > 0) ? input.y : 0;

        Vector3 move = wallNormal * s;

        if (playerInput.Jump() && wallrunTime > wallrunMinimum)
        {
            wallrunTime = 0;
            Vector3 forward = wallNormal.normalized;
            Vector3 right = Vector3.Cross(forward, Vector3.up) * wallDir;
            Vector3 wallJump = (Vector3.up * (s + 0.5f) + forward * s * 1.5f + right * (s + 0.5f)).normalized;
            movement.Jump(wallJump, (s + 1f));
            playerInput.ResetJump();
            ChangeStatus(Status.walking);
        }

        if (!hasWallToSide(wallDir) || movement.grounded)
            ChangeStatus(Status.walking);

        float inputGravity = (1f - s) + (s / 4f); //More input, less gravity
        float timeGravity = Mathf.Lerp(0f, 1f, wallrunTime / wallrunMinimum);
        movement.Move(move, movement.runSpeed, inputGravity * timeGravity);
        wallrunTime += Time.deltaTime;
    }

    void CheckForWallrun()
    {
        if (!canInteract || movement.grounded || movement.moveDirection.y >= 0)
            return;

        int wall = 0;
        if (hasWallToSide(1))
            wall = 1;
        else if (hasWallToSide(-1))
            wall = -1;

        if (wall == 0) return;

        if(Physics.Raycast(transform.position + (transform.right * wall * radius), transform.right * wall, out var hit, halfradius, wallrunLayer))
        {
            wallDir = wall;
            wallrunTime = 0;
            wallNormal = Vector3.Cross(hit.normal, Vector3.up) * -wallDir;
            ChangeStatus(Status.wallRunning, WallrunIK);
        }
    }

    bool hasWallToSide(int dir)
    {
        //Check for ladder in front of player
        Vector3 top = transform.position + (transform.right * 0.25f * dir);
        Vector3 bottom = top - (transform.up * radius);
        top += (transform.up * radius);

        return (Physics.CapsuleCastAll(top, bottom, 0.25f, transform.right * dir, 0.05f, wallrunLayer).Length >= 1);
    }

    IKData WallrunIK()
    {
        IKData data = new IKData();
        bool left = (wallDir == -1);
        if (Physics.Raycast(transform.position + (transform.right * wallDir * radius), transform.right * wallDir, out var hit, halfradius, wallrunLayer))
            data.handPos = hit.point;
        if (left)
        {
            data.armLocalPos.x = -0.35f;
            data.armLocalPos.z = -0.55f;
            data.handPos += (Vector3.up + wallNormal) * radius * 2f;
            data.handEulerAngles = Quaternion.LookRotation(wallNormal, wallDir * Vector3.Cross(wallNormal, Vector3.up)).eulerAngles;
            data.armElbowPos = data.handPos - wallNormal;
        }
        else
        {
            data.armElbowPos = data.handPos;
            data.armLocalPos.x = 0;
            data.armLocalPos.z = -0.325f;
            data.handPos += (2 * Vector3.up + wallNormal) * radius;
            data.handEulerAngles = Quaternion.LookRotation(Vector3.up, wallDir * Vector3.Cross(wallNormal, Vector3.up)).eulerAngles;
        }

        data.armLocalPos.y = 0; 
        return data;
    }
    /*********************************************************************/

    /******************** LEDGE GRABBING AND CLIMBING ********************/
    void GrabbedLedgeMovement()
    {
        if (playerInput.Jump())
        {
            movement.Jump((Vector3.up - transform.forward).normalized, 1f);
            playerInput.ResetJump();
            ChangeStatus(Status.walking);
        }

        movement.Move(Vector3.zero, 0f, 0f); //Stay in place
    }

    void ClimbLedgeMovement()
    {
        Vector3 dir = pushFrom - transform.position;
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
        Vector3 move = Vector3.Cross(dir, right).normalized;

        playerInput.ResetJump();
        movement.Move(move, movement.runSpeed, 0f);
        if (new Vector2(dir.x, dir.z).magnitude < 0.125f)
            ChangeStatus(Status.idle);
    }

    void CheckLedgeGrab()
    {
        if (!canInteract)
            return;
        //Check for ledge to grab onto 
        Vector3 dir = transform.TransformDirection(new Vector3(0, -0.5f, 1).normalized);
        Vector3 pos = transform.position + (Vector3.up * height / 3f) + (transform.forward * radius / 2f);
        bool right = Physics.Raycast(pos + (transform.right * radius / 2f), dir, radius + 0.125f, ledgeLayer);
        bool left = Physics.Raycast(pos - (transform.right * radius / 2f), dir, radius + 0.125f, ledgeLayer);

        if (Physics.Raycast(pos, dir, out var hit, radius + 0.125f, ledgeLayer) && right && left)
        {
            Vector3 rotatePos = transform.InverseTransformPoint(hit.point);
            rotatePos.x = 0; rotatePos.z = 1;
            pushFrom = transform.position + transform.TransformDirection(rotatePos); //grab the position with local z = 1
            rotatePos.z = radius * 2f;

            Vector3 checkCollisions = transform.position + transform.TransformDirection(rotatePos); //grab it closer now

            //Check if you would be able to stand on the ledge
            if (!Physics.SphereCast(checkCollisions, radius, Vector3.up, out hit, height - radius))
                ChangeStatus(Status.grabbedLedge, GrabbedLedgeIK);
        }
    }

    void UpdateLedgeGrabbing()
    {
        if (movement.grounded || movement.moveDirection.y > 0)
            canGrabLedge = true;

        if (status != Status.climbingLedge)
        {
            if (canGrabLedge && !movement.grounded)
            {
                if (movement.moveDirection.y < 0)
                    CheckLedgeGrab();
            }

            if (status == Status.grabbedLedge)
            {
                canGrabLedge = false;
                Vector2 down = playerInput.down;
                if (down.y == -1)
                    ChangeStatus(Status.walking);
                else if (down.y == 1)
                    ChangeStatus(Status.climbingLedge, ClimbingLedgeIK);
            }
        }
    }

    IKData GrabbedLedgeIK()
    {
        IKData data = new IKData();
        Vector3 dir = (pushFrom - transform.position); dir.y = 0;
        dir = dir.normalized;

        float handRadius = 0.125f;
        data.handPos = transform.position;
        data.handPos.y = pushFrom.y;
        data.handPos += dir * (radius + handRadius);

        Vector3 handDir = -Vector3.Cross(dir, Vector3.up);
        data.armElbowPos = (data.handPos - (handDir * radius));
        data.handEulerAngles = Quaternion.LookRotation(handDir).eulerAngles;
        data.armLocalPos.y = 0.075f; data.armLocalPos.z = -0.5f;
        return data;
    }

    IKData ClimbingLedgeIK()
    {
        IKData data = new IKData();
        Vector3 dir = (pushFrom - transform.position).normalized;

        data.handPos = pushFrom;
        data.handEulerAngles = Quaternion.LookRotation(dir).eulerAngles;
        data.armElbowPos = transform.position;
        data.armElbowPos += Vector3.Cross(dir, Vector3.up) * radius;
        data.armElbowPos.z = transform.position.z;
        return data;
    }
    /*********************************************************************/

    /***************************** VAULTING ******************************/
    void VaultMovement()
    {
        Vector3 dir = vaultOver - transform.position;
        Vector3 localPos = vaultHelper.transform.InverseTransformPoint(transform.position);
        Vector3 move = (vaultDir + (Vector3.up * -(localPos.z - radius) * height)).normalized;

        if (localPos.z < -(radius * 2f))
            move = dir.normalized;
        else if (localPos.z > halfheight)
        {
            movement.controller.height = height;
            ChangeStatus(Status.walking);
        }

        movement.Move(move, movement.runSpeed, 0f);
    }

    void CheckForVault()
    {
        if (status == Status.vaulting) return;

        float movementAdjust = (Vector3.ClampMagnitude(movement.controller.velocity, 16f).magnitude / 16f);
        float checkDis = radius + movementAdjust;

        if(hasObjectInfront(checkDis, vaultLayer) && playerInput.Jump())
        {
            if (Physics.SphereCast(transform.position + (transform.forward * (radius - 0.25f)), 0.25f, transform.forward, out var sphereHit, checkDis, vaultLayer))
            {
                if (Physics.SphereCast(sphereHit.point + (Vector3.up * halfheight), radius, Vector3.down, out var hit, halfheight - radius, vaultLayer))
                {
                    Debug.DrawRay(hit.point + (Vector3.up * radius), Vector3.up * halfheight);
                    //Check above the point to make sure the player can fit
                    if (Physics.SphereCast(hit.point + (Vector3.up * radius), radius, Vector3.up, out var trash, halfheight))
                        return; //If cannot fit the player then do not vault

                    //Check in-front of the vault to see if something is blocking
                    Vector3 fromPlayer = transform.position;
                    Vector3 toVault = hit.point + (Vector3.up * radius);
                    fromPlayer.y = toVault.y;

                    Vector3 dir = (toVault - fromPlayer);
                    if (Physics.SphereCast(fromPlayer, radius / 2f, dir.normalized, out var trash2, dir.magnitude + radius))
                        return; //If we hit something blocking the vault, then do nothing

                    vaultOver = hit.point;
                    vaultDir = transform.forward;
                    SetVaultHelper();

                    movement.controller.height = radius;
                    ChangeStatus(Status.vaulting, VaultIK);
                }
            }
        }
    }

    void CreateVaultHelper()
    {
        vaultHelper = new GameObject();
        vaultHelper.transform.name = "(IGNORE) Vault Helper";
    }

    void SetVaultHelper()
    {
        vaultHelper.transform.position = vaultOver;
        vaultHelper.transform.rotation = Quaternion.LookRotation(vaultDir);
    }

    IKData VaultIK()
    {
        IKData data = new IKData();
        data.handPos = vaultOver + (Vector3.up * radius);
        data.handEulerAngles = Quaternion.LookRotation(vaultDir - Vector3.up).eulerAngles;
        data.armElbowPos = vaultOver;
        data.armElbowPos.y = transform.position.y;
        data.armElbowPos += Vector3.Cross(vaultDir, Vector3.up) * radius;
        return data;
    }
    /*********************************************************************/

    /***************************** SWIMMING ******************************/
    void SurfaceSwimmingMovement()
    {
        float wantedYPos = getWaterLevel();
        float dif = transform.position.y - wantedYPos;
        float swimAdjust = Mathf.Sin(dif);
        Vector3 move = new Vector3(playerInput.input.x, 0, playerInput.input.y);
        move = transform.TransformDirection(move) * 2f;

        bool isTreading = (move.sqrMagnitude < 0.02f);
        treadTime = Mathf.PingPong(treadTime + Time.deltaTime, isTreading ? 0.5f : 0.25f);

        if (dif < halfheight / 4f)
            canJumpOutOfWater = true;

        if (playerInput.elevate >= 0.02f && canJumpOutOfWater)
        {
            if (dif >= halfheight / 4)
            {
                movement.Jump(Vector3.up, 1f);
                EndSwim();
            }
            else
                move.y = 1f;
        }
        else if (isInWater)
        {
            if (playerInput.elevate <= -0.02f)
                move.y = -1;
            else
            {
                float downWithOffset = camera.transform.forward.y + 0.333f;
                float swimDown = Mathf.Clamp(downWithOffset * playerInput.input.y, -Mathf.Infinity, 0f);
                move.y = (swimDown <= -0.02f) ? swimDown : treadTime;
            }

                     
            if (dif < -halfheight / 4f)
                ChangeStatus(Status.underwaterSwimming);
        }

        movement.Move(move, 1f, Mathf.Clamp(swimAdjust * 0.5f, 0f, Mathf.Infinity));
    }

    void UnderwaterSwimmingMovement()
    {
        Vector3 swim = camera.transform.TransformDirection(new Vector3(playerInput.input.x, 0, playerInput.input.y)) * 2f;
        swim += Vector3.up * playerInput.elevate;
        swim = Vector3.ClampMagnitude(swim, 2f);
        movement.Move(swim, 1f, 0f);
    }

    //Call when you enter the top of the water trigger
    public void WithinWaterTop()
    {
        float wantedYPos = getWaterLevel();
        float dif = transform.position.y - wantedYPos;
        if ((int)status < 9) //If we are not swimming
        {
            if (dif <= 0f && movement.controller.velocity.y <= 0)
                StartSwim();
        }
        else
        {
            //If we are already in the water swimming, then only go back to surface swimming when heading upwards
            if (dif >= -0.1f)
            {
                if (movement.grounded)
                    EndSwim();
                else if (status == Status.underwaterSwimming)
                    StartSwim();
            }
        }
    }

    float getWaterLevel()
    {
        float waterLevel = transform.position.y; //This is just a default y value
        Vector3 pos = transform.position; pos.y += 100f; //Add 100 to make it above the player and the water (NOTE: just don't have 100 units deep water, if you do, increase this)
        if (Physics.Raycast(pos, Vector3.down, out var hit, Mathf.Infinity, topWaterLayer))
            waterLevel = hit.point.y - (halfheight / 2f);
        return waterLevel;
    }

    void StartSwim()
    {
        StartCoroutine(startSwimming());
        IEnumerator startSwimming()
        {
            slideTime = 0;
            movement.controller.height = halfheight;
            yield return new WaitForEndOfFrame();
            ChangeStatus(Status.surfaceSwimming, SurfaceSwimIK);
            canJumpOutOfWater = false;
            treadTime = 0;
        }
    }

    void EndSwim()
    {
        movement.controller.height = height;
        ChangeStatus(Status.walking);
    }

    public void CurrentlyInWater(bool inWater)
    {
        isInWater = inWater;
    }

    IKData SurfaceSwimIK()
    {
        IKData data = new IKData();
        float onWater = getWaterLevel() + (halfheight / 2f);
        Vector3 adjust = (transform.forward - transform.right) * radius;
        data.handPos = transform.position + adjust;
        data.handPos.y = onWater;

        float time = Mathf.Repeat(Time.time * Mathf.PI, Mathf.PI * 2);
        Vector3 animate = (new Vector3(Mathf.Sin(time), 0, -Mathf.Cos(time)) * radius) / Mathf.PI;
        data.handPos += animate;

        data.handEulerAngles = transform.eulerAngles;

        adjust = (-transform.forward - transform.right) * radius;
        data.armElbowPos = transform.position + adjust * 2f;
        data.armElbowPos.y = onWater - radius;

        data.armLocalPos.x -= radius;
        return data;
    }

    /*********************************************************************/

    bool hasObjectInfront(float dis, LayerMask layer)
    {
        Vector3 top = transform.position + (transform.forward * 0.25f);
        Vector3 bottom = top - (transform.up * halfheight);

        return (Physics.CapsuleCastAll(top, bottom, 0.25f, transform.forward, dis, layer).Length >= 1);
    }
}
public class IKData
{
    public Vector3 handPos;
    public Vector3 handEulerAngles;

    public Vector3 armElbowPos;
    public Vector3 armLocalPos;

    public IKData()
    {
        handPos = Vector3.zero;
        handEulerAngles = Vector3.zero;
        armElbowPos = Vector3.zero;
        armLocalPos = Vector3.zero;

        //armLocalPos = ArmIKController.defaultArmPos;
    }

    public TransformData HandData()
    {
        TransformData data = new TransformData();
        data.position = handPos;
        data.eulerAngles = handEulerAngles;
        return data;
    }
}
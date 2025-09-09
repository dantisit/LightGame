using System;
using Light_and_controller.Scripts.Components.PushableObject;
using UnityEngine;

public class PushableObject : PlayerMain
{
    private void Awake()
    {
        // Declaration of necessary components:
        // Animator for controlling character animations,
        // Rigidbody2D for physics simulation,
        // InputManager for handling player input,
        // CapsuleCollider2D for slope detection and ground checking.
        Animator = GetComponent<Animator>();
        Rigidbody2D = GetComponent<Rigidbody2D>();
        InputManager = GetComponent<ManualInputManager>();
        CapsuleCollider2D = GetComponent<CapsuleCollider2D>();

        // In this section, we assign all states
        _stateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, _stateMachine, AnimName.Idle, PlayerData);
        WalkState = new PlayerWalkState(this, _stateMachine, AnimName.Walk, PlayerData);
        JumpState = new PlayerJumpState(this, _stateMachine, AnimName.Jump, PlayerData);
        LandState = new PlayerLandState(this, _stateMachine, AnimName.Land, PlayerData);
        DashState = new PlayerDashState(this, _stateMachine, AnimName.Dash, PlayerData);
        CrouchIdleState = new PlayerCrouchIdleState(this, _stateMachine, AnimName.CrouchIdle, PlayerData);
        CrouchWalkState = new PlayerCrouchWalkState(this, _stateMachine, AnimName.CrouchWalk, PlayerData);
        WallGrabState = new PlayerWallGrabState(this, _stateMachine, AnimName.WallGrab, PlayerData);
        WallClimbState = new PlayerWallClimbState(this, _stateMachine, AnimName.WallClimb, PlayerData);
        WallJumpState = new PlayerWallJumpState(this, _stateMachine, AnimName.WallJump, PlayerData);
        WallSlideState = new PlayerWallSlideState(this, _stateMachine, AnimName.WallSlide, PlayerData);

        baseMaxSpeed = PlayerData.Walk.MaxSpeed;
    }

    private PlayerMain _lastPlayer;
    private float baseMaxSpeed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.TryGetComponent(out PlayerMain newEntry)) return;
        _lastPlayer = newEntry;
    }

    private void LateUpdate()
    {
        if(!_lastPlayer) return;
        if(!_lastPlayer.PlayerData.Physics.IsGrounded) return;
        var playerDir = Mathf.Sign(_lastPlayer.InputManager.Input_Walk);
        var dir = Mathf.Sign((transform.position - _lastPlayer.transform.position).x);
        var isSameDir = Mathf.Approximately(playerDir, dir);
        var isStopping = Mathf.Approximately(_lastPlayer.InputManager.Input_Walk, 0);
        var isCrouching = _lastPlayer.CurrentState == AnimName.CrouchWalk;
        if(!isSameDir && !isCrouching && !isStopping) return;
        if (isCrouching) PlayerData.Walk.MaxSpeed = _lastPlayer.PlayerData.Walk.MaxSpeed;
        else PlayerData.Walk.MaxSpeed = baseMaxSpeed;
        InputManager.input_Walk = _lastPlayer.InputManager.Input_Walk;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(!_lastPlayer || other.gameObject != _lastPlayer.gameObject) return;
        InputManager.input_Walk = 0;
        _lastPlayer = null;
    }

    private void OnDisable()
    {
        InputManager.input_Walk = 0;
        _stateMachine.ChangeState(IdleState);
    }
}
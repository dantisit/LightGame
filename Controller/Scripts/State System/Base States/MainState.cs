using UnityEngine;

public class MainState
{
    // Required variables to create logic of states
    protected PlayerMain player;
    protected PlayerStateMachine stateMachine;
    protected Rigidbody rigidbody;
    protected readonly PlayerMain.AnimName _animEnum;
    protected PlayerData playerData;
    protected PlayerInputManager inputManager;

    protected float localTime; // Variable to work at states local timeline that resets with every state change

    public MainState(PlayerMain player, PlayerStateMachine stateMachine, PlayerMain.AnimName animEnum, PlayerData playerData)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        _animEnum = animEnum;
        this.playerData = playerData;
        this.inputManager = player.InputManager;
    }

    // Enter function runs at every state change after old state's exit
    public virtual void Enter()
    {
        player.CurrentState = _animEnum;
        rigidbody = player.Rigidbody;
        player.Animator.SetBool(_animEnum.ToString(), true);
        localTime = 0f;
    }

    // Update function runs at every frame
    public virtual void Update()
    {
        localTime += Time.deltaTime;

        SwitchStateLogic();
    }

    // FixedUpdate function runs at every fixed time frame (per 0.02 second default)
    public virtual void FixedUpdate()
    {
        PhysicsCheck();
    }

    // Exit function runs at every state change before new state's enter
    public virtual void Exit()
    {
        player.Animator.SetBool(_animEnum.ToString(), false);
        localTime = 0f;
    }

    // The PhysicsCheck function runs in FixedUpdate and its purpose is to reduce the complexity of FixedUpdate and separate out the algorithms for readability and maintainability.
    // This function includes essential physics checks that are required for the player state logic to function correctly.
    public virtual void PhysicsCheck()
    {
        EssentialPhysics.SetPlayerFacingDirection(inputManager, player, playerData);

        // ��� 3D ����� ������������ 3D ������ ��������� ���������
        // �������� �� ��������������� 3D �����
        // player.CapsuleCollider.GetContacts(playerData.Physics.Contacts);

        EssentialPhysics.GroundCheck(player, playerData);
        EssentialPhysics.WallCheck(player, playerData);

        if (playerData.Physics.UseCustomZRotations)
        {
            rigidbody.freezeRotation = false;
            EssentialPhysics.ApplyRotationOnSlope(player, playerData);
        }
        else
        {
            rigidbody.freezeRotation = true;
        }

        EssentialPhysics.HeadBumpCheck(player, playerData);

        // ��� 3D ����� ������������ �������� �����
        // playerData.Physics.CanSlideCorner = EssentialPhysics.CornerSlideCheck(playerData.Physics.Contacts, player, playerData);

        playerData.Physics.CanJump = playerData.Jump.CoyoteTimeTimer > 0 && playerData.Jump.JumpBufferTimer > 0
                                        && (!playerData.Physics.IsOnNotWalkableSlope || playerData.Physics.IsMultipleContactWithNonWalkableSlope || playerData.Physics.Slope.StayStill);
        playerData.Physics.CanWallJump = playerData.Walls.WallJump.CoyoteTimeTimer > 0 && playerData.Walls.WallJump.JumpBufferTimer > 0;

        EssentialPhysics.GetPlatformVelocity(playerData.Physics.CollidedMovingRigidbody, playerData);

        // �����������: ����������� Vector2 � Vector3 ��� 3D ������
        Vector3 platformVelocity3D = new Vector3(
            playerData.Physics.Platform.DampedVelocity.x,
            playerData.Physics.Platform.DampedVelocity.y,
            0f
        );
        playerData.Physics.LocalVelocity = rigidbody.linearVelocity - platformVelocity3D;

        if (player.CurrentState == PlayerMain.AnimName.Jump
            || player.CurrentState == PlayerMain.AnimName.Land
            || player.CurrentState == PlayerMain.AnimName.Dash
            || player.CurrentState == PlayerMain.AnimName.WallJump)
        {
            if (localTime > playerData.Jump.CoyoteTimeTimer && playerData.Jump.NextJumpInt == 1)
            {
                playerData.Jump.NextJumpInt++;
            }
            if (playerData.Jump.NewJump && playerData.Jump.NextJumpInt <= playerData.Jump.Jumps.Count && !playerData.Physics.CanWallJump)
            {
                stateMachine.ChangeState(player.JumpState);
            }
        }
        else
        {
            playerData.Jump.NextJumpInt = 1;
        }
    }

    public virtual void SwitchStateLogic() { }
}
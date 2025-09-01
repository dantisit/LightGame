using UnityEngine;

public class PlayerCrouchIdleState : MainState
{
    public PlayerCrouchIdleState(PlayerMain player, PlayerStateMachine stateMachine, PlayerMain.AnimName animEnum, PlayerData playerData) : base(player, stateMachine, animEnum, playerData)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // Для управления гравитацией в 3D нужно использовать другие подходы
        // gravityScale доступен только в Rigidbody2D
    }

    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // Используем linearVelocity вместо устаревшего velocity
        if (playerData.Physics.Contacts.Count == 0)
        {
            rigidbody.linearVelocity = new Vector3(0f, -1f, 0f);
        }
        else if (playerData.Physics.IsMultipleContactWithNonWalkableSlope)
        {
            rigidbody.linearVelocity = new Vector3(0f, (playerData.Physics.Slope.CurrentSlopeAngle / 90) - 1, 0f);
        }
        else if (playerData.Physics.CanSlideCorner)
        {
            rigidbody.linearVelocity = new Vector3(0f, -playerData.Physics.SlideSpeedOnCorner, 0f);
        }
        else
        {
            rigidbody.linearVelocity = Vector3.zero;
        }

        // Добавляем платформенную скорость
        rigidbody.linearVelocity += new Vector3(
            playerData.Physics.Platform.DampedVelocity.x,
            playerData.Physics.Platform.DampedVelocity.y,
            0f
        );

        playerData.Walls.CurrentStamina = Mathf.Clamp(playerData.Walls.CurrentStamina + (Time.fixedDeltaTime * playerData.Walls.StaminaRegenPerSec), 0, playerData.Walls.MaxStamina);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void PhysicsCheck()
    {
        base.PhysicsCheck();
    }

    public override void SwitchStateLogic()
    {
        base.SwitchStateLogic();

        if (!inputManager.Input_Crouch)
        {
            if (inputManager.Input_Walk == 0f)
            {
                stateMachine.ChangeState(player.IdleState);
            }
            else if (inputManager.Input_Walk != 0)
            {
                stateMachine.ChangeState(player.WalkState);
            }
        }
        else if (inputManager.Input_Walk != 0 && !playerData.Physics.Slope.StayStill)
        {
            stateMachine.ChangeState(player.CrouchWalkState);
        }
        else if (!playerData.Physics.IsGrounded || (playerData.Physics.IsOnNotWalkableSlope && !playerData.Physics.Slope.StayStill && !playerData.Physics.IsMultipleContactWithNonWalkableSlope))
        {
            stateMachine.ChangeState(player.LandState);
        }
        else if (inputManager.Input_Dash && playerData.Dash.DashCooldownTimer <= 0f)
        {
            stateMachine.ChangeState(player.DashState);
        }
        else if (playerData.Physics.IsNextToWall && inputManager.Input_WallGrab && playerData.Walls.CurrentStamina > 0)
        {
            stateMachine.ChangeState(player.WallGrabState);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlayerMain;

public static class EssentialPhysics
{
    // Updates player's facing direction variable at PlayerData.Physics
    public static void SetPlayerFacingDirection(PlayerInputManager inputManager, PlayerMain player, PlayerData playerData)
    {
        if (player.CurrentState == AnimName.WallSlide)
        {
            playerData.Physics.FacingDirection = playerData.Physics.WallDirection * playerData.Walls.WallSlide.FacingDirectionWhenSliding;
        }
        else if (player.CurrentState == AnimName.WallGrab || player.CurrentState == AnimName.WallClimb)
        {
            playerData.Physics.FacingDirection = playerData.Physics.WallDirection;
        }
        else if (player.CurrentState == AnimName.WallJump)
        {
            playerData.Physics.FacingDirection = playerData.Physics.IsNextToWall ? -playerData.Physics.WallDirection : (int)Mathf.Sign(player.Rigidbody.linearVelocity.x);
        }
        else if (inputManager.Input_Walk != 0 && Mathf.Sign(inputManager.Input_Walk) != Mathf.Sign(playerData.Physics.FacingDirection))
        {
            playerData.Physics.FacingDirection *= -1;
        }
        else if (playerData.Physics.FacingDirection == 0)
        {
            playerData.Physics.FacingDirection = 1;
        }

        SetLocalScale(player, playerData);
    }

    // Updates localScale of player based on facing direction
    public static void SetLocalScale(PlayerMain player, PlayerData playerData)
    {
        if (Mathf.Sign(playerData.Physics.FacingDirection) != Mathf.Sign(player.transform.localScale.x))
        {
            player.transform.localScale = Vector3.Scale(player.transform.localScale, new Vector3(-1, 1, 1));
        }
    }

    // This function returns the time value on an AnimationCurve where a given value is first reached at that specified time.
    // The 'greater' parameter is used to specify whether the function should search for the first time the curve value is greater than or equal to the given value.
    public static float SetCurveTimeByValue(AnimationCurve curve, float value, float maxTime, bool greaterValues = true)
    {
        float _curveTime = 0f;
        while ((greaterValues && curve.Evaluate(_curveTime) <= value) || (!greaterValues && curve.Evaluate(_curveTime) >= value))
        {
            _curveTime += Time.fixedDeltaTime;
            if (_curveTime >= maxTime)
            {
                break;
            }
        }
        return _curveTime;
    }

    public static void GroundCheck(PlayerMain player, PlayerData playerData)
    {
        playerData.Physics.GroundCheckPosition = SetGroundCheckPosition(player, playerData);
        float _offset = -0.01f;

        // ��� 3D ���������� Physics.SphereCast ������ Physics2D.CircleCast
        RaycastHit _hit;
        bool isHit = Physics.SphereCast(playerData.Physics.GroundCheckPosition,
                                       player.CapsuleCollider.radius * Mathf.Abs(player.transform.localScale.x) + _offset,
                                       -player.transform.up,
                                       out _hit,
                                       0.2f,
                                       playerData.Physics.GroundLayerMask);

        Debug.DrawRay(_hit.point, _hit.normal, Color.red);
        if (isHit)
        {
            playerData.Physics.IsGrounded = true;

            // ��� 3D ��������� ����������� �������� �� ������
            Vector3 slopeRight = Vector3.Cross(_hit.normal, Vector3.up).normalized;
            playerData.Physics.WalkSpeedDirection = new Vector2(slopeRight.x, slopeRight.z);

            playerData.Physics.ContactPosition = _hit.point;
            playerData.Physics.Slope.CurrentSlopeAngle = Vector3.Angle(_hit.normal, Vector3.up);
            playerData.Physics.IsOnNotWalkableSlope = playerData.Physics.Slope.CurrentSlopeAngle > playerData.Physics.Slope.MaxSlopeAngle;
            SlopeAngleChangeCheck(_hit, player, playerData);

            if (_hit.collider.gameObject.layer == 12 && _hit.rigidbody)
            {
                playerData.Physics.CollidedMovingRigidbody = _hit.rigidbody;
            }
        }
        else
        {
            playerData.Physics.IsGrounded = false;
            playerData.Physics.WalkSpeedDirection = new Vector2(-1, 0);
            playerData.Physics.ContactPosition = Vector3.zero;
            playerData.Physics.Slope.CurrentSlopeAngle = 0f;
            playerData.Physics.IsOnNotWalkableSlope = false;
            playerData.Physics.Slope.CurrentSlopeAngleChange = 0f;
            playerData.Physics.IsOnCorner = false;
            playerData.Physics.CollidedMovingRigidbody = null;
        }

        playerData.Physics.IsMultipleContactWithWalkableSlope = MultipleGroundContactCheck(player, playerData);
    }

    public static Vector3 SetGroundCheckPosition(PlayerMain player, PlayerData playerData)
    {
        Vector3 _groundCheckPosition = player.transform.position;
        Vector3 _size = new Vector3(player.CapsuleCollider.radius * 2 * Mathf.Abs(player.transform.localScale.x),
                                   player.CapsuleCollider.height * player.transform.localScale.y,
                                   player.CapsuleCollider.radius * 2 * Mathf.Abs(player.transform.localScale.z));

        Vector3 _offset = new Vector3(player.CapsuleCollider.center.x * Mathf.Abs(player.transform.localScale.x),
                                     player.CapsuleCollider.center.y * player.transform.localScale.y,
                                     player.CapsuleCollider.center.z * Mathf.Abs(player.transform.localScale.z));

        _groundCheckPosition.y -= Mathf.Abs(_size.x - _size.y) / 2;
        _groundCheckPosition += new Vector3(_offset.x * playerData.Physics.FacingDirection, _offset.y, _offset.z);

        return _groundCheckPosition;
    }

    public static Vector3 SetHeadCheckPosition(PlayerMain player, PlayerData playerData)
    {
        Vector3 _headCheckPosition = player.transform.position;
        Vector3 _size = new Vector3(player.CapsuleCollider.radius * 2 * Mathf.Abs(player.transform.localScale.x),
                                   player.CapsuleCollider.height * player.transform.localScale.y,
                                   player.CapsuleCollider.radius * 2 * Mathf.Abs(player.transform.localScale.z));

        Vector3 _offset = new Vector3(player.CapsuleCollider.center.x * Mathf.Abs(player.transform.localScale.x),
                                     player.CapsuleCollider.center.y * player.transform.localScale.y,
                                     player.CapsuleCollider.center.z * Mathf.Abs(player.transform.localScale.z));

        _headCheckPosition.y += Mathf.Abs(_size.x - _size.y) / 2;
        _headCheckPosition += new Vector3(_offset.x * playerData.Physics.FacingDirection, _offset.y, _offset.z);

        return _headCheckPosition;
    }

    public static void SlopeAngleChangeCheck(RaycastHit hit, PlayerMain player, PlayerData playerData)
    {
        RaycastHit hitFront;
        bool frontHit = Physics.Raycast(hit.point + new Vector3(0.05f * playerData.Physics.FacingDirection, 0.2f, 0), Vector3.down, out hitFront, 1f, playerData.Physics.GroundLayerMask);

        RaycastHit hitBack;
        bool backHit = Physics.Raycast(hit.point + new Vector3(-0.05f * playerData.Physics.FacingDirection, 0.2f, 0), Vector3.down, out hitBack, 1f, playerData.Physics.GroundLayerMask);

        Debug.DrawRay(hitFront.point, hitFront.normal, Color.magenta);
        playerData.Physics.Slope.CurrentSlopeAngleChange = Vector3.Angle(hitFront.normal, hit.normal);

        if (playerData.Physics.Slope.CurrentSlopeAngleChange > 0 && frontHit
            && Vector3.Angle(hitFront.normal, Vector3.up) <= playerData.Physics.Slope.MaxSlopeAngle
            && playerData.Physics.Slope.CurrentSlopeAngleChange <= playerData.Physics.Slope.MaxEasedSlopeAngleChange)
        {
            // ��� 3D ������� ����������� ��������
            Quaternion rotation = Quaternion.Euler(0, 0, 20 * -playerData.Physics.FacingDirection);
            Vector3 rotatedDirection = rotation * new Vector3(playerData.Physics.WalkSpeedDirection.x, 0, playerData.Physics.WalkSpeedDirection.y);
            playerData.Physics.WalkSpeedDirection = new Vector2(rotatedDirection.x, rotatedDirection.z);

            playerData.Physics.IsOnCorner = !backHit;
        }
        else if (!frontHit)
        {
            playerData.Physics.WalkSpeedDirection = new Vector2(-1, 0);
            playerData.Physics.Slope.CurrentSlopeAngleChange = 0f;
            playerData.Physics.IsOnCorner = true;
        }
        else if (!backHit)
        {
            playerData.Physics.IsOnCorner = true;
        }
        else
        {
            playerData.Physics.IsOnCorner = false;
        }
    }

    public static bool MultipleGroundContactCheck(PlayerMain player, PlayerData playerData)
    {
        float _offset = -0.01f;
        RaycastHit _frontHit;
        bool frontHit = Physics.SphereCast(playerData.Physics.GroundCheckPosition,
                                          player.CapsuleCollider.radius * Mathf.Abs(player.transform.localScale.x) + _offset,
                                          new Vector3(playerData.Physics.FacingDirection, 0, 0),
                                          out _frontHit,
                                          0.1f,
                                          playerData.Physics.GroundLayerMask);

        if (playerData.Physics.IsGrounded && !playerData.Physics.IsOnNotWalkableSlope && !playerData.Physics.IsOnCorner && frontHit && (_frontHit.point - playerData.Physics.ContactPosition).magnitude > 0.1f)
        {
            if (Vector3.Angle(playerData.Physics.GroundCheckPosition - _frontHit.point, Vector3.up) > playerData.Physics.Slope.MaxSlopeAngle)
            {
                playerData.Physics.IsMultipleContactWithNonWalkableSlope = true;
                playerData.Physics.Slope.StayStill = Mathf.Sign(_frontHit.point.x - playerData.Physics.GroundCheckPosition.x) == Mathf.Sign(playerData.Physics.FacingDirection);
                return false;
            }
            else
            {
                playerData.Physics.WalkSpeedDirection = new Vector2(-1, 0);
                playerData.Physics.Slope.StayStill = false;
                playerData.Physics.IsMultipleContactWithNonWalkableSlope = false;
                return true;
            }
        }
        else
        {
            playerData.Physics.Slope.StayStill = false;
            playerData.Physics.IsMultipleContactWithNonWalkableSlope = false;
        }
        return false;
    }

    public static void WallCheck(PlayerMain player, PlayerData playerData)
    {
        playerData.Physics.HeadCheckPosition = SetHeadCheckPosition(player, playerData);
        float _offset = 0.1f;

        RaycastHit _rightHit;
        bool rightHit = Physics.Raycast(playerData.Physics.HeadCheckPosition, Vector3.right, out _rightHit,
                                       player.CapsuleCollider.radius * Mathf.Abs(player.transform.localScale.x) + _offset,
                                       playerData.Physics.WallLayerMask);

        RaycastHit _leftHit;
        bool leftHit = Physics.Raycast(playerData.Physics.HeadCheckPosition, Vector3.left, out _leftHit,
                                      player.CapsuleCollider.radius * Mathf.Abs(player.transform.localScale.x) + _offset,
                                      playerData.Physics.WallLayerMask);

        if (rightHit || leftHit)
        {
            playerData.Physics.WallDirection = rightHit ? 1 : -1;
            RaycastHit _hit = rightHit ? _rightHit : _leftHit;
            playerData.Physics.CollidedMovingRigidbody = _hit.rigidbody;
            playerData.Physics.IsNextToWall = true;
        }
        else
        {
            playerData.Physics.WallDirection = 0;
            if (playerData.Physics.CollidedMovingRigidbody != null && playerData.Physics.CollidedMovingRigidbody.gameObject.layer == 13)
            {
                playerData.Physics.CollidedMovingRigidbody = null;
            }
            playerData.Physics.IsNextToWall = false;
        }
    }

    public static void HeadBumpCheck(PlayerMain player, PlayerData playerData)
    {
        playerData.Physics.HeadCheckPosition = SetHeadCheckPosition(player, playerData);
        RaycastHit _hit;
        float _offset = 0.01f;

        bool isHit = Physics.SphereCast(playerData.Physics.HeadCheckPosition,
                                       player.CapsuleCollider.radius * Mathf.Abs(player.transform.localScale.x) + _offset,
                                       player.transform.up,
                                       out _hit,
                                       0.2f,
                                       playerData.Physics.HeadBumpLayerMask);

        if (isHit)
        {
            playerData.Physics.IsOnHeadBump = true;
            playerData.Physics.CanBumpHead = Vector3.Angle(_hit.normal, Vector3.down) < playerData.Physics.HeadBumpMinAngle;
        }
        else
        {
            playerData.Physics.IsOnHeadBump = false;
            playerData.Physics.CanBumpHead = false;
        }
    }

    public static bool CornerSlideCheck(List<ContactPoint> contacts, PlayerMain player, PlayerData playerData)
    {
        contacts = contacts.Where(x => x.point.y <= playerData.Physics.GroundCheckPosition.y).GroupBy(x => x.point).Select(x => x.First()).ToList();
        foreach (ContactPoint contact in contacts)
        {
            if (Vector3.Angle(playerData.Physics.GroundCheckPosition - contact.point, Vector3.up) >= playerData.Physics.SlideOnCornerMinAngle)
            {
                return playerData.Physics.IsOnCorner;
            }
        }
        return false;
    }

    public static void ApplyRotationOnSlope(PlayerMain player, PlayerData playerData)
    {
        float _currentSlopeAngle, _rotationDirection, _finalRotation, _rotationDifference;
        _currentSlopeAngle = playerData.Physics.Slope.CurrentSlopeAngle;
        _rotationDirection = playerData.Physics.ContactPosition.x > playerData.Physics.GroundCheckPosition.x ? 1 : -1;
        _finalRotation = 0f;

        if (playerData.Physics.IsGrounded && !(playerData.Physics.IsOnNotWalkableSlope || playerData.Physics.IsMultipleContactWithNonWalkableSlope || playerData.Physics.IsMultipleContactWithWalkableSlope))
        {
            _finalRotation = _currentSlopeAngle * _rotationDirection * playerData.Physics.Slope.RotationMultiplierOnSlope;
        }

        _rotationDifference = _finalRotation - player.Rigidbody.rotation.eulerAngles.z;

        if (Mathf.Abs(_rotationDifference) > 2f)
        {
            Vector3 _pivot = playerData.Physics.GroundCheckPosition;
            Vector3 _offset = player.Rigidbody.position - _pivot;
            float angleInRadians = playerData.Physics.Slope.RotationSpeed * _rotationDifference * Time.fixedDeltaTime * Mathf.Deg2Rad;
            float cosAngle = Mathf.Cos(angleInRadians);
            float sinAngle = Mathf.Sin(angleInRadians);

            Vector3 rotatedOffset = new Vector3(
                _offset.x * cosAngle - _offset.y * sinAngle,
                _offset.x * sinAngle + _offset.y * cosAngle,
                _offset.z
            );

            player.Rigidbody.position += _pivot + rotatedOffset - player.Rigidbody.position;
            player.Rigidbody.angularVelocity = new Vector3(0, 0, playerData.Physics.Slope.RotationSpeed * _rotationDifference);
        }
        else
        {
            player.Rigidbody.angularVelocity = Vector3.zero;
            player.Rigidbody.rotation = Quaternion.Euler(0, 0, _finalRotation);
        }
    }

    public static void GetPlatformVelocity(Rigidbody platformRigidbody, PlayerData playerData)
    {
        if (platformRigidbody)
        {
            Vector3 _platformCenterToContact = playerData.Physics.ContactPosition - platformRigidbody.position;
            Vector3 _angularVelocity = platformRigidbody.angularVelocity * Mathf.Deg2Rad;
            Vector3 _rotationalLinearVelocity = Vector3.Cross(_angularVelocity, _platformCenterToContact);
            Vector3 _movingLinearVelocity = platformRigidbody.linearVelocity;
            Vector3 _linearVelocity = _rotationalLinearVelocity + _movingLinearVelocity;

            playerData.Physics.Platform.MaxPlatformVelocity = _linearVelocity;
            playerData.Physics.Platform.DampedVelocity = _linearVelocity;
        }
        else
        {
            float magnitudeRatio = playerData.Physics.Platform.DampedVelocity.magnitude / playerData.Physics.Platform.MaxPlatformVelocity.magnitude;
            float _slowDownTime = SetCurveTimeByValue(playerData.Physics.Platform.DampingCurve, magnitudeRatio, 1, false);
            _slowDownTime += Time.fixedDeltaTime;
            playerData.Physics.Platform.DampedVelocity = playerData.Physics.Platform.DampingCurve.Evaluate(_slowDownTime / playerData.Physics.Platform.DampingTime) * playerData.Physics.Platform.MaxPlatformVelocity;
        }
    }
}
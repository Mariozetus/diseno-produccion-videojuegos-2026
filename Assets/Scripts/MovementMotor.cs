using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class MovementMotor : MonoBehaviour
{
    struct CharacterRaycastOrigins
    {
        public Vector3 topLeft;
        public Vector3 bottomRight;
        public Vector3 bottomLeft;
    }
    public class CharacterCollisionState2D
    {
        public bool right;
        public bool left;
        public bool above;
        public bool below;
        public bool becameGroundedThisFrame;
        public bool wasGroundedLastFrame;
        public bool movingDownSlope;
        public float slopeAngle;
        public GameObject myGround;

        public bool hasCollision()
        {
            return below || right || left || above;
        }

        public void reset()
        {
            right = left = above = below = becameGroundedThisFrame = movingDownSlope = false;
            slopeAngle = 0f;
        }

        public override string ToString()
        {
            return string.Format("[CharacterCollisionState2D] r: {0}, l: {1}, a: {2}, b: {3}, movingDownSlope: {4}, angle: {5}, wasGroundedLastFrame: {6}, becameGroundedThisFrame: {7}",
                                 right, left, above, below, movingDownSlope, slopeAngle, wasGroundedLastFrame, becameGroundedThisFrame);
        }
    }

    public event Action<RaycastHit2D> onControllerCollidedEvent;
    public float gravity = -25f;
    public float runSpeed = 8f;
    public float groundDamping = 20f;
    public float inAirDamping = 5f;
    public float jumpHeight = 3f;


    [SerializeField]
    [Range(0.001f, 0.3f)]
    float _skinWidth = 0.02f;
    public float SkinWidth
    {
        get { return _skinWidth; }
        set
        {
            _skinWidth = value;
            recalculateDistanceBetweenRays();
        }
    }

    public LayerMask platformMask = 0;

    public LayerMask triggerMask = 0;

    [Range(0f, 90f)]
    public float slopeLimit = 30f;

    public float jumpingThreshold = 0.07f;

    public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));

    [Range(2, 20)]
    public int totalHorizontalRays = 8;
    [Range(2, 20)]
    public int totalVerticalRays = 4;

    readonly float _slopeLimitTangent = Mathf.Tan(75f * Mathf.Deg2Rad);

    public BoxCollider2D boxCollider;

    [HideInInspector]
    [NonSerialized]
    public CharacterCollisionState2D collisionState = new CharacterCollisionState2D();
    [HideInInspector]
    [NonSerialized]
    public Vector3 velocity;

    public bool IsGrounded { get { return collisionState.below; } }

    const float kSkinWidthFloatFudgeFactor = 0.001f;

    CharacterRaycastOrigins _raycastOrigins;

    RaycastHit2D _raycastHit;

    readonly List<RaycastHit2D> _raycastHitsThisFrame = new List<RaycastHit2D>(2);

    float _verticalDistanceBetweenRays;
    float _horizontalDistanceBetweenRays;
    bool _isGoingUpSlope = false;

    void Awake()
    {
        SkinWidth = _skinWidth;
        for (var i = 0; i < 32; i++)
        {
            if ((triggerMask.value & 1 << i) == 0)
                Physics2D.IgnoreLayerCollision(gameObject.layer, i);
        }
    }

    void DrawRay(Vector3 start, Vector3 dir, Color color)
    {
        Debug.DrawRay(start, dir, color);
    }

    public void Move(Vector3 deltaMovement)
    {
        collisionState.wasGroundedLastFrame = collisionState.below;

        collisionState.reset();
        _raycastHitsThisFrame.Clear();
        _isGoingUpSlope = false;

        primeRaycastOrigins();

        if (deltaMovement.y < 0f && collisionState.wasGroundedLastFrame)
            HandleVerticalSlope(ref deltaMovement);

        if (deltaMovement.x != 0f)
            MoveHorizontally(ref deltaMovement);

        if (deltaMovement.y != 0f)
            MoveVertically(ref deltaMovement);

        deltaMovement.z = 0;
        transform.Translate(deltaMovement, Space.World);

        if (collisionState.wasGroundedLastFrame)
        {
            Platform myPlatform = collisionState.myGround.GetComponent<Platform>();
            if (myPlatform != null)
            {
                transform.Translate(myPlatform.velocity * Time.fixedDeltaTime, Space.World);
            }
        }

        if (_raycastHit = Physics2D.CircleCast(transform.position, _skinWidth, deltaMovement.normalized, _skinWidth, triggerMask))
        {
            _raycastHitsThisFrame.Add(_raycastHit);
        }

        if (Time.fixedDeltaTime > 0f)
            velocity = deltaMovement / Time.fixedDeltaTime;

        if (!collisionState.wasGroundedLastFrame && collisionState.below)
            collisionState.becameGroundedThisFrame = true;

        if (_isGoingUpSlope)
            velocity.y = 0;

        if (onControllerCollidedEvent != null)
        {
            for (var i = 0; i < _raycastHitsThisFrame.Count; i++)
                onControllerCollidedEvent(_raycastHitsThisFrame[i]);
        }
    }

    public void warpToGrounded()
    {
        do
        {
            Move(new Vector3(0, -1f, 0));
        } while (!IsGrounded);
    }


    public void recalculateDistanceBetweenRays()
    {
        var colliderUseableHeight = boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2f * _skinWidth);
        _verticalDistanceBetweenRays = colliderUseableHeight / (totalHorizontalRays - 1);

        var colliderUseableWidth = boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2f * _skinWidth);
        _horizontalDistanceBetweenRays = colliderUseableWidth / (totalVerticalRays - 1);
    }

    void primeRaycastOrigins()
    {
        var modifiedBounds = boxCollider.bounds;
        modifiedBounds.Expand(-2f * _skinWidth);

        _raycastOrigins.topLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y);
        _raycastOrigins.bottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y);
        _raycastOrigins.bottomLeft = modifiedBounds.min;
    }

    void MoveHorizontally(ref Vector3 deltaMovement)
    {
        var isGoingRight = deltaMovement.x > 0;
        var rayDistance = Mathf.Abs(deltaMovement.x) + _skinWidth;
        var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
        var initialRayOrigin = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;

        for (var i = 0; i < totalHorizontalRays; i++)
        {
            var ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * _verticalDistanceBetweenRays);

            DrawRay(ray, rayDirection * rayDistance, Color.red);
            _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);

            if (_raycastHit)
            {
                if (i == 0 && HandleHorizontalSlope(ref deltaMovement, Vector2.Angle(_raycastHit.normal, Vector2.up)))
                {
                    _raycastHitsThisFrame.Add(_raycastHit);
                    if (!collisionState.wasGroundedLastFrame)
                    {
                        float flushDistance = Mathf.Sign(deltaMovement.x) * (_raycastHit.distance - SkinWidth);
                        transform.Translate(new Vector2(flushDistance, 0));
                    }
                    break;
                }
                deltaMovement.x = _raycastHit.point.x - ray.x;
                rayDistance = Mathf.Abs(deltaMovement.x);
                if (isGoingRight)
                {
                    deltaMovement.x -= _skinWidth;
                    collisionState.right = true;
                }
                else
                {
                    deltaMovement.x += _skinWidth;
                    collisionState.left = true;
                }

                _raycastHitsThisFrame.Add(_raycastHit);
                if (rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
                    break;
            }
        }
    }

    bool HandleHorizontalSlope(ref Vector3 deltaMovement, float angle)
    {
        if (Mathf.RoundToInt(angle) == 90)
            return false;
        if (angle < slopeLimit)
        {
            if (deltaMovement.y < jumpingThreshold)
            {
                var slopeModifier = slopeSpeedMultiplier.Evaluate(angle);
                deltaMovement.x *= slopeModifier;
                deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * deltaMovement.x);
                var isGoingRight = deltaMovement.x > 0;
                var ray = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
                RaycastHit2D raycastHit;
                if (collisionState.wasGroundedLastFrame)
                    raycastHit = Physics2D.Raycast(ray, deltaMovement.normalized, deltaMovement.magnitude, platformMask);
                else
                    raycastHit = Physics2D.Raycast(ray, deltaMovement.normalized, deltaMovement.magnitude, platformMask);

                if (raycastHit)
                {
                    deltaMovement = (Vector3)raycastHit.point - ray;
                    if (isGoingRight)
                        deltaMovement.x -= _skinWidth;
                    else
                        deltaMovement.x += _skinWidth;
                }

                _isGoingUpSlope = true;
                collisionState.below = true;
                if (raycastHit.collider)
                {
                    collisionState.myGround = raycastHit.collider.gameObject;
                }
                collisionState.slopeAngle = -angle;
            }
        }
        else
        {
            deltaMovement.x = 0;
        }

        return true;
    }

    void MoveVertically(ref Vector3 deltaMovement)
    {
        var isGoingUp = deltaMovement.y > 0;
        var rayDistance = Mathf.Abs(deltaMovement.y) + _skinWidth;
        var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
        var initialRayOrigin = isGoingUp ? _raycastOrigins.topLeft : _raycastOrigins.bottomLeft;

        initialRayOrigin.x += deltaMovement.x;

        var mask = platformMask;

        for (var i = 0; i < totalVerticalRays; i++)
        {
            var ray = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);

            DrawRay(ray, rayDirection * rayDistance, Color.red);
            _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);
            if (_raycastHit)
            {
                deltaMovement.y = _raycastHit.point.y - ray.y;
                rayDistance = Mathf.Abs(deltaMovement.y);
                if (isGoingUp)
                {
                    deltaMovement.y -= _skinWidth;
                    collisionState.above = true;
                }
                else
                {
                    deltaMovement.y += _skinWidth;
                    collisionState.below = true;
                    collisionState.myGround = _raycastHit.collider.gameObject;
                }

                _raycastHitsThisFrame.Add(_raycastHit);
                if (!isGoingUp && deltaMovement.y > 0.00001f)
                    _isGoingUpSlope = true;
                if (rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
                    break;
            }
        }
    }

    private void HandleVerticalSlope(ref Vector3 deltaMovement)
    {
        var centerOfCollider = (_raycastOrigins.bottomLeft.x + _raycastOrigins.bottomRight.x) * 0.5f;
        var rayDirection = -Vector2.up;
        var slopeCheckRayDistance = _slopeLimitTangent * (_raycastOrigins.bottomRight.x - centerOfCollider);

        var slopeRay = new Vector2(centerOfCollider, _raycastOrigins.bottomLeft.y);
        DrawRay(slopeRay, rayDirection * slopeCheckRayDistance, Color.yellow);
        _raycastHit = Physics2D.Raycast(slopeRay, rayDirection, slopeCheckRayDistance, platformMask);
        if (_raycastHit)
        {
            var angle = Vector2.Angle(_raycastHit.normal, Vector2.up);
            if (angle == 0)
                return;
            var isMovingDownSlope = Mathf.Sign(_raycastHit.normal.x) == Mathf.Sign(deltaMovement.x);
            if (isMovingDownSlope)
            {
                var slopeModifier = slopeSpeedMultiplier.Evaluate(-angle);
                deltaMovement.y += _raycastHit.point.y - slopeRay.y - SkinWidth;
                deltaMovement = new Vector3(0, deltaMovement.y, 0) +
                                (Quaternion.AngleAxis(-angle, Vector3.forward) * new Vector3(deltaMovement.x * slopeModifier, 0, 0));
                collisionState.movingDownSlope = true;
                collisionState.slopeAngle = angle;
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }
#endif
}
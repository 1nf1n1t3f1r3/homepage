using UnityEngine;
using System.Collections.Generic;

public class EntityPhysics : MonoBehaviour
{
    [SerializeField]
    private float dampSmallVelocitiesThreshold = 0.0125f;


    [Header("Internal Forces Settings")]
    [SerializeField] private float gravity = -10.0f;
    [SerializeField] private float maxGravityImpact = -10.0f; // Max Speed achieved by only Gravitational pull
    [SerializeField] private float drag = 1.5f; // Airborne
    [SerializeField] private float friction = 3.0f; // Grounded

    [SerializeField] private float wallSlidingGravityMultiplier = 1.00f;
    //[SerializeField] private float bounceFactor = 0.1f;

    [SerializeField] private Vector2 velocity;
    [SerializeField] private Vector2 position;

    public Vector2 Position { get => position; set { position = value; transform.position = new Vector3(value.x, value.y, transform.position.z); } }
    public Vector2 Velocity { get => velocity; set => velocity = value; }

    // Public Functions to Set Position/Velocity
    public void SetPosition(Vector2 newPosition) => Position = newPosition;
    public void SetVelocity(Vector2 newVelocity) => Velocity = newVelocity;

    // Public Functions to Add Position/Velocity
    public void AddPosition(Vector2 addPosition) => Position += addPosition;
    public void AddVelocity(Vector2 addVelocity) => Velocity += addVelocity;

    // Groundedness
    [SerializeField] private bool groundedUp;
    [SerializeField] private bool groundedDown;
    [SerializeField] private bool groundedLeft;
    [SerializeField] private bool groundedRight;

    public bool GroundedUp { get => groundedUp; set => groundedUp = value; }
    public bool GroundedDown { get => groundedDown; set => groundedDown = value; }
    public bool GroundedLeft { get => groundedLeft; set => groundedLeft = value; }
    public bool GroundedRight { get => groundedRight; set => groundedRight = value; }

    // Groundedness Slopes
    [SerializeField] private bool groundedSlopeLeft = false;
    [SerializeField] private bool groundedSlopeRight = false;

    public bool GroundedSlopeLeft { get => groundedSlopeLeft; set => groundedSlopeLeft = value; }
    public bool GroundedSlopeRight { get => groundedSlopeRight; set => groundedSlopeRight = value; }

    // Slopes that are too steep
    [SerializeField] private bool tooSteepSlopeLeft = false;
    [SerializeField] private bool tooSteepSlopeRight = false;

    public bool TooSteepSlopeLeft { get => tooSteepSlopeLeft; set => tooSteepSlopeLeft = value; }
    public bool TooSteepSlopeRight { get => tooSteepSlopeRight; set => tooSteepSlopeRight = value; }

    // Slopes Angles
    [SerializeField] private Vector2 slopeNormalLeft = Vector2.zero;
    [SerializeField] private float slopeAngleLeft = 0f;
    [SerializeField] private float slopeDotLeft = 0f;

    [SerializeField] private float maxSlopeAngleConversion = 89f; // 90 With Offset




    // Convert Velocities
    [SerializeField] private float convertVelocitiesCooldownTimer;
    [SerializeField] private float convertVelocitiesCooldownInterval = 2.5f; // 2.5 Second Cooldown

    // HandleImpact
    [SerializeField] private float steepBounceCooldownTime = 5.0f;
    [SerializeField] private float steepBounceTimer = 0f;
    [SerializeField] float minConvertedY = 5f; // 5 seems to cover everything, though I'm a bit foggy on the actual reasons why

    [SerializeField] private float angle; // Visualize Angle in Inspector
    [SerializeField] private float cooldownFactor; // Visualize cooldownFactor in Inspector

    // HandleImpact Timers
    private float positiveYTime;
    private float negativeYTime;
    private float positiveXTime;
    private float negativeXTime;
    private float airTimeDecayMultiplier;
    private float fallDamageSpeedThreshold;
    private float fallDamageTimerThreshold;

    private float falldamageSpeedFactor = 1.5f;
    private float fallDamageTimerFactor = 1.2f;
    private float fallDamageDamageFactor = 1.0f;

    // TickInterval
    private float tickInterval;

    // Other
    private Vector2 previousPosition;
    private Vector2 previousVelocity;

    private Vector2 postSnapPosition;

    // Get Script Components
    private EntityRaycastSystem entityRaycastSystem;
    private EntityStateMachine entityStateMachine;
    private SpriteRenderer spriteRenderer;

    // External Forces Enum/Struct/List
    public enum ExternalForceMode
    {
        AddInstant,
        AddOverTime,
        Override
    }

    private struct ExternalForce
    {
        public Vector2 force;
        public ExternalForceMode mode;
        public float duration;      // How long should this force last?
        public float timeElapsed;  // How much time has passed so far?
        public float maxVelocity; // The maximum Velocity the object can reach from this External Force

        public ExternalForce(Vector2 force, ExternalForceMode mode, float duration = 0f, float maxVelocity = float.MaxValue)
        {
            this.force = force;
            this.mode = mode;
            this.duration = duration;
            this.timeElapsed = 0f;  // Starts from 0, will increment each frame  
            this.maxVelocity = maxVelocity;

        }
    }

    private List<ExternalForce> externalForces = new List<ExternalForce>();
    private List<ExternalForce> activeForces = new List<ExternalForce>();

    private void Awake()
    {
        tickInterval = SimulationConstants.TickRate > 0 ? 1f / SimulationConstants.TickRate : 0.016f; // 60 FPS 

        entityRaycastSystem = GetComponent<EntityRaycastSystem>();
        entityStateMachine = GetComponent<EntityStateMachine>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }







    // Main Updates for Position, Velocity, Groundedness. Called by its Owner in its own Tick Loop
    public void HandlePhysics()
    {
        // Store Previous Information
        previousPosition = Position;
        previousVelocity = Velocity;

        // Update Velocities
        ProcessInternalForces(); // Gravity, Drag, Friction  
        ProcessExternalForces(); // Player Inputs, and being moved by External Forces

        // Update Fall Damage Air Timer before checking if we're Grounded this Tick. That way, we won't reset it before Impact
        UpdateImpactAirTimers();

        // Check Slopes before Snapping
        entityRaycastSystem.RaycastSlopeCheck(Position, Velocity);

        groundedSlopeLeft = entityRaycastSystem.GroundedSlopeLeft;
        groundedSlopeRight = entityRaycastSystem.GroundedSlopeRight;

        tooSteepSlopeLeft = entityRaycastSystem.TooSteepSlopeLeft;
        tooSteepSlopeRight = entityRaycastSystem.TooSteepSlopeRight;

        groundedDown = entityRaycastSystem.GroundedDown;

        slopeAngleLeft = entityRaycastSystem.SlopeAngleLeft;
        slopeNormalLeft = entityRaycastSystem.SlopeNormalLeft;
        slopeDotLeft = entityRaycastSystem.SlopeAngleLeft;

        // Update grounded state flags based on raycast result
        entityRaycastSystem.UpdateRaycasts(Position, Velocity);

        groundedDown = entityRaycastSystem.GroundedDown;
        groundedUp = entityRaycastSystem.GroundedUp;
        groundedLeft = entityRaycastSystem.GroundedLeft;
        groundedRight = entityRaycastSystem.GroundedRight;

        // Snap and Clear/Reduce Velocities if about to Collide
        entityRaycastSystem.RaycastSnap(Position, Velocity);

        // Do Normal Position Updates based on Velocity. (Also converts Velocities based on Slopes we're on, if applicable)
        UpdatePosition();

        // Unstuck us
        if (entityRaycastSystem.WouldOverlapAtPosition(Position, out var count, out var overlaps))
        {
            Vector2 preTunnelingPosition = Position;
            Vector2 nextPos = position + velocity * Time.fixedDeltaTime;
            Position = entityRaycastSystem.RepelFromOverlap(Position, overlaps);
            Debug.Log($"Prevented tunnelling in Final Tunnelling Prevention Check. Starting Position before Operation: {preTunnelingPosition}. Position we would have moved to: {nextPos}. Final Corrected Position: {position}");
        }

        // Correct Final Velocities to Zero
        DampSmallVelocities(dampSmallVelocitiesThreshold);

        // Run Timer
        HandleImpactSteepImpactTimer();
    }



    // Set Small Velocities to 0
    private void DampSmallVelocities(float dampSmallVelocitiesThreshold)
    {
        if (Mathf.Abs(velocity.x) < dampSmallVelocitiesThreshold) velocity.x = 0f;
        if (Mathf.Abs(velocity.y) < dampSmallVelocitiesThreshold) velocity.y = 0f;
    }

    // Gravity, Drag, Friction, Buoyancy. Take 'Internal' with a grain of salt. 
    private void ProcessInternalForces()
    {
        //Gravity based on Groundedness, WallSliding and MaxSpeed
        if (!entityStateMachine.IsInState(CharacterState.Grounded))
        {
            // Wall Sliding
            if (entityStateMachine.IsInState(CharacterState.WallSliding))
            {
                float targetFallSpeed = maxGravityImpact * wallSlidingGravityMultiplier;

                // First apply gravity as normal until we're no longer rising
                if (velocity.y >= 0)
                {
                    velocity.y += gravity * Time.fixedDeltaTime;
                    velocity.y = Mathf.Max(velocity.y, targetFallSpeed); // Clamp to not exceed fall speed
                }

                // Then apply reduced gravity until we're at the targetFallSpeed
                else if (velocity.y > targetFallSpeed)
                {
                    velocity.y += gravity * wallSlidingGravityMultiplier * Time.fixedDeltaTime;
                    velocity.y = Mathf.Max(velocity.y, targetFallSpeed); // Clamp to not exceed fall speed
                }

                // Clamp when descending faster than the targetFallSpeed lerp towards targetFallSpeed 
                else
                {
                    velocity.y = Mathf.Lerp(velocity.y, targetFallSpeed, 0.2f);
                }
            }

            // Normal Gravity
            else
            {
                if (velocity.y > maxGravityImpact)
                {
                    velocity.y += gravity * Time.fixedDeltaTime;
                }
            }
        }

        // Drag and Friction when not SelfMoving
        if ((velocity.x != 0 && !entityStateMachine.IsInState(CharacterState.SelfMoving)) || // Velocity without SelfMoving
            (entityStateMachine.IsInState(CharacterState.SelfMoving) && entityStateMachine.IntentDirectionHorizontal == Direction.None)) //SelfMoving in Different Direction
        {
            if (!entityStateMachine.IsInState(CharacterState.Grounded))
            {
                velocity.x *= 1 - (drag * Time.fixedDeltaTime);
            }

            else if (entityStateMachine.IsInState(CharacterState.Grounded))
            {
                velocity.x *= 1 - (friction * Time.fixedDeltaTime);

                if (groundedSlopeLeft || groundedSlopeRight)
                {
                    velocity.y *= 1 - (friction * Time.fixedDeltaTime);      // Slope Handling
                }

            }
        }

        DampSmallVelocities(dampSmallVelocitiesThreshold);         // Damping
    }

    public void AddExternalForce(Vector2 force, ExternalForceMode mode, float duration = 0f, float maxVelocity = float.MaxValue)
    {
        externalForces.Add(new ExternalForce(force, mode, duration, maxVelocity));
    }

    // Process External Forces Locally. One type at a time to prevent interference
    private void ProcessExternalForces()
    {
        foreach (var externalForce in externalForces)
        {
            var updatedForce = externalForce;
            updatedForce.timeElapsed += Time.fixedDeltaTime;

            switch (externalForce.mode)
            {
                case ExternalForceMode.AddInstant:
                    velocity += externalForce.force;
                    break;

                case ExternalForceMode.AddOverTime:
                    velocity += externalForce.force * Time.fixedDeltaTime;
                    break;

                case ExternalForceMode.Override:
                    velocity = externalForce.force;
                    break;
            }

            // Clamp velocity according to maxVelocity in the external force
            velocity.x = Mathf.Clamp(velocity.x, -externalForce.maxVelocity, externalForce.maxVelocity);
            velocity.y = Mathf.Clamp(velocity.y, -externalForce.maxVelocity, externalForce.maxVelocity);

            // Update timeElapsed and keep if still active
            if (updatedForce.mode == ExternalForceMode.AddOverTime && updatedForce.timeElapsed < updatedForce.duration)
            {
                activeForces.Add(updatedForce);
            }
        }

        // Reset ExternalForces, aside from ActiveForces. 
        externalForces.Clear();
        externalForces.AddRange(activeForces);
        activeForces.Clear();
        DampSmallVelocities(dampSmallVelocitiesThreshold); // Damping
    }

    //// Not converting upwards gets us stuck? 
    //private void ConvertVelocities()
    //{
    //    if (slopeAngle > 0 && slopeAngle <= maxSlopeAngleConversion) 
    //    {
    //        // Get slope tangent and roject velocity onto the slope tangent
    //        Vector2 previousVelocity = velocity;
    //        Vector2 slopeTangent = new Vector2(slopeNormal.y, -slopeNormal.x);
    //        float slopeSpeed = Vector2.Dot(velocity, slopeTangent);
    //        Vector2 slopeVelocity = slopeTangent * slopeSpeed;

    //        // Normal Behaviour. Apply at Will
    //        if (slopeAngle <= entityRaycastSystem.SlopeMaxTraversableAngle)
    //        {
    //            // Slope-aligned X. Preserve Original Y for Upwards Movement (Jumping). 
    //            if (velocity.y > 0f)
    //            {
    //                velocity.x = slopeVelocity.x;
    //            }
    //            // Slope-aligned X and Y. Y for Downwards Slope-Aligned Movement (Sliding). 
    //            else if (velocity.y <= 0f)
    //            {
    //                velocity = slopeVelocity;
    //            }
    //        }

    //        // Slope too Steep for repeat application
    //        else // (slopeAngle > entityPhysics.SlopeMaxTraversableAngle)
    //        {
    //            if (convertVelocitiesCooldownTimer <= 0f)
    //            {
    //                // Slope - aligned X.Preserve Original Y for Upwards Movement (Jumping). 
    //                if (velocity.y > 0f)
    //                {
    //                    velocity.x = slopeVelocity.x;
    //                }
    //                // Slope-aligned X and Y. Y for Downwards Slope-Aligned Movement (Sliding). 
    //                else if (velocity.y <= 0f)
    //                {
    //                    velocity = slopeVelocity;
    //                }

    //                // Start cooldown
    //                convertVelocitiesCooldownTimer = convertVelocitiesCooldownInterval;
    //                Debug.Log($"Too Steep Slope Hit. convertVelocitiesCooldownTimer is <= 0 at: {convertVelocitiesCooldownTimer}. Converting Velocity.");
    //            }

    //            //else
    //            //{
    //            //    Debug.Log($"Too Steep Slope Hit. convertVelocitiesCooldownTimer is > 0 at: {convertVelocitiesCooldownTimer}. Skipping.");
    //            //}

    //        }

    //        float xChange = previousVelocity.x - velocity.x;
    //        float yChange = previousVelocity.y - velocity.y;

    //        Debug.Log($"slopeTangent: {slopeTangent}. slopeSpeed: {slopeSpeed}. slopeVelocity: {slopeVelocity}");
    //        Debug.Log($"SlopeAngle: {slopeAngle}. Converted previousVelocity: {previousVelocity} into velocity: {velocity}. Change: {xChange} {yChange}.");
    //    }
    //}

    //// Increment Tick based on the Timer
    //private void UpdateConvertVelocitiesCooldownTimer()
    //{
    //    if (convertVelocitiesCooldownTimer > 0f)
    //    {
    //        convertVelocitiesCooldownTimer -= Time.deltaTime;
    //        if (convertVelocitiesCooldownTimer < 0f)
    //        {
    //            convertVelocitiesCooldownTimer = 0f;
    //        }

    //    }
    //}



    // Update Position with Velocity * Time
    private void UpdatePosition()
    {
        // Store the Position we expect to be in in nextPos 
        Vector2 nextPos = position + velocity * Time.fixedDeltaTime;
        Vector2 startingPos = position;

        if (entityRaycastSystem.WouldOverlapAtPosition(nextPos, out var count, out var overlaps))
        {
            nextPos = entityRaycastSystem.RepelFromOverlap(nextPos, overlaps);
            Debug.Log($"Prevented tunnelling in UpdatePosition(). Starting Position: {startingPos}. Position we would have moved to: {nextPos}. Final Corrected Position: {position}");


        }

        // Execute
        position = nextPos;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    private void HandleImpactSteepImpactTimer()
    {
        // Cooldown ticking
        if (steepBounceTimer > 0f)
        {
            steepBounceTimer -= Time.deltaTime;
        }

        cooldownFactor = Mathf.InverseLerp(0f, steepBounceCooldownTime, steepBounceTimer); // New
    }

    // Update the Fall Damage Timers when in the Air and beyond FallDamageSpeedThreshold
    private void UpdateImpactAirTimers()
    {
        if (groundedDown)
        {
            positiveYTime = 0f;
            negativeYTime = 0f;
            positiveXTime = 0f;
            negativeXTime = 0f;
            return;
        }

        // If Upward Movement is beyond Threshold
        if (velocity.y > fallDamageSpeedThreshold)
        {
            positiveYTime += Time.deltaTime;
        }

        else
        {
            positiveYTime = 0f;
        }

        // If Downward Movement is beyond Threshold
        if (velocity.y < -fallDamageSpeedThreshold)
        {
            negativeYTime += Time.deltaTime;
        }

        else
        {
            negativeYTime = 0f;
        }

        // If Rightward Movement is beyond Threshold
        if (velocity.x > fallDamageSpeedThreshold)
        {
            positiveXTime += Time.deltaTime;
        }

        else
        {
            positiveXTime = 0f;
        }

        // If Leftward Movement is beyond Threshold
        if (velocity.x < -fallDamageSpeedThreshold)
        {
            negativeXTime += Time.deltaTime;
        }

        else
        {
            negativeXTime = 0f;
        }
    }

    // Use the Angle to decide how much of the Timer's we're using for our Impact. 
    float GetImpactTimeFromNormal(Vector2 impactSpeed)
    {
        // Ensure the normal is normalized just in case
        Vector2 impactSpeedNormalized = impactSpeed.normalized;

        float time = 0f;

        // Weigh each directional timer by how much the normal points in that direction
        if (impactSpeedNormalized.y > 0f) time += positiveYTime * impactSpeedNormalized.y;
        if (impactSpeedNormalized.y < 0f) time += negativeYTime * -impactSpeedNormalized.y;
        if (impactSpeedNormalized.x > 0f) time += positiveXTime * impactSpeedNormalized.x;
        if (impactSpeedNormalized.x < 0f) time += negativeXTime * -impactSpeedNormalized.x;

        return time;
    }



    // Calculate Fall Damage based on ImpactSpeed & ImpactTime
    float CalculateFallDamage(float impactSpeed, float impactTime)
    {
        // Scales damage by both speed and air time — tuned via curves
        float speedFactor = Mathf.Pow(impactSpeed - fallDamageSpeedThreshold, falldamageSpeedFactor);
        float timeFactor = Mathf.Pow(impactTime - fallDamageTimerThreshold, fallDamageTimerFactor);

        return speedFactor * timeFactor * fallDamageDamageFactor;
    }


    // Change this to .Dots for Consistency
    public void HandleImpact(Vector2 velocity, Vector2 normal)
    {
        Vector2 previousVelocity = velocity;

        normal.Normalize();

        // Project velocity onto normal to get the impact portion (into the surface)
        float impactMagnitude = Vector2.Dot(velocity, -normal);              // Get angle between velocity and normal
        Vector2 impactVelocity = -normal * Mathf.Max(impactMagnitude, 0f);  // Discard Velocity not moving into the Surface

        // Project onto the surface tangent to get the preserved motion
        Vector2 tangent = new Vector2(-normal.y, normal.x); // 90 degree rotation
        float tangentMagnitude = Vector2.Dot(velocity, tangent);
        Vector2 tangentVelocity = tangent * tangentMagnitude;

        // Angle-based scaling: Reduce sliding effect on steep surfaces
        angle = Vector2.Angle(normal, Vector2.up);
        float steepnessFactor = Mathf.InverseLerp(entityRaycastSystem.SlopeMaxTraversableAngle, 90f, angle); // 0 at walkable, 1 at vertical
        float tangentScale = 1f - steepnessFactor; // 1 at flat, 0 at wall

        // Preserve only the sliding motion
        float impactSpeed = impactVelocity.magnitude;

        // Old & New
        Vector2 preImpactVelocity = this.Velocity;
        Vector2 postImpactVelocity = tangentVelocity * tangentScale;
        float convertedY = postImpactVelocity.y - preImpactVelocity.y;

        Debug.Log($"Pre-Impact/Conversion Velocity = {Velocity}");

        // When dealing with Slopes, apply special rules for upward Y conversion
        if (angle > entityRaycastSystem.SlopeMaxTraversableAngle && angle < entityRaycastSystem.SlopeWallThreshold)
        {
            if (convertedY > 0f && steepBounceTimer > 0f)
            {
                // Apply Cooldown Factor
                float scaledConvertedY = convertedY * cooldownFactor;

                // Y Velocity Not Converted on Subsequent Hits
                if (scaledConvertedY <= minConvertedY)
                {
                    // Debug.Log($"[HandleImpact] Partial Conversion: scaledConvertedY {scaledConvertedY} <= minConvertedY {minConvertedY}. Only changing X.");
                    this.velocity.x = postImpactVelocity.x;
                    this.velocity.y = preImpactVelocity.y;
                }

                // Slope Aware Y Velocity Conversion on Probable Secondary Impact. Viz. Neither Continuous, nor enough to fully Reset.
                // However, enough to partially reset the CooldownTimer. Entity likely disengaged and re-engaged Slope
                else
                {
                    this.velocity = postImpactVelocity;
                    // Debug.Log($"[HandleImpact] Full Conversion: cooldownFactor on CD: {steepBounceTimer}. scaledConvertedY > {minConvertedY} convertedY = {convertedY} = ({postImpactVelocity.y} - {preImpactVelocity.y}).");
                }

                //Debug.Log($"cooldownFactor = {cooldownFactor} = mathf.InverseLerp({steepBounceCooldownTime}, 0f, {steepBounceTimer})");
                //Debug.Log($"scaledConvertedY = {scaledConvertedY} = convertedY {convertedY} * cooldownFactor {cooldownFactor}");
                //Debug.Log($"tangentDir = {tangentDir} = (tangentVelocity.normalized)");
                //Debug.Log($"projectedMagnitude = {projectedMagnitude} = finalY {finalY} / tangentDir {tangentDir}");
                //Debug.Log($"postImpactVelocity = {postImpactVelocity} = tangentDir {tangentDir} * projectedMagnitude {projectedMagnitude}");
                //Debug.Log($"postImpactVelocity = {postImpactVelocity}. preImpactVelocity = {preImpactVelocity}. ");

            }

            // Apply Normal Velocity change because we're not dealing with Positive Y or we're not on Cooldown
            else // steepBounceTimer <= 0 && converted Y <= 0f
            {
                this.velocity = postImpactVelocity;

                // Debug.Log($"[HandleImpact] Full Conversion: cooldownFactor not on CD: {steepBounceTimer}. convertedY {convertedY} not > 0. ({postImpactVelocity.y} - {preImpactVelocity.y}).");

            }

            steepBounceTimer = steepBounceCooldownTime;
        }

        // When dealing with Walls, handle only the .X
        else if (angle >= entityRaycastSystem.SlopeWallThreshold)
        {
            // Preserve downward motion, cancel horizontal
            postImpactVelocity = new Vector2(0f, preImpactVelocity.y);
            this.velocity = postImpactVelocity;
        }

        // Just Apply postImpactVelocity if we're not dealing with any difficult terrain 
        else
        {
            this.velocity = postImpactVelocity;
        }

        Debug.Log($"Post-Impact Velocity. Velocity = {Velocity}. preImpactVelocity = {preImpactVelocity}. normal = {normal}. tangentVelocity = {tangentVelocity}. tangentScale = {tangentScale}. Multiplication postImpactVelocity {postImpactVelocity}");

        // TODO: Fall Damage Partial Placeholder
        if (impactSpeed > fallDamageSpeedThreshold) // TBD based on PlayerStats? Fast characters shouldn't damage themselves
        {
            Debug.Log($"{impactSpeed} > {fallDamageSpeedThreshold}?");

            float impactTimer = GetImpactTimeFromNormal(preImpactVelocity);
            Debug.Log($"{impactTimer} > {fallDamageTimerThreshold}?");
            if (impactTimer > fallDamageTimerThreshold)
            {
                float damage = CalculateFallDamage(impactSpeed, impactTimer);
                // ApplyDamage(damage);
                Debug.Log($"Fall Damage = {damage}. Calculated from impactSpeed = {impactSpeed} and impactTimer =  {impactTimer}");
            }
        }

        // Reset Timers for Impacts after resolving 
        positiveYTime = 0f;
        negativeYTime = 0f;
        positiveXTime = 0f;
        negativeXTime = 0f;
    }
}

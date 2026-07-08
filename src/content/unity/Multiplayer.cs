using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;


public class TickClient : NetworkBehaviour
{
    // Entity Registration. Next ID & Dictionary/ReadOnlyDictionary
    private static Dictionary<uint, RemoteEntityClass> remoteEntities = new();
    public static IReadOnlyDictionary<uint, RemoteEntityClass> RemoteEntities => remoteEntities;

    // Awake Debugging
    private static int instanceCounter = 0;
    private int instanceID;

    // Ticks
    public int LocalTick { get; private set; }
    public int EstimatedServerTick { get; private set; }

    // Server and Clients increment Ticks (But Server is Authoritative!)
    private float tickInterval;
    private float tickTimer;
    private float heartbeatTimer;

    public float TickAlpha => tickTimer / tickInterval; // For Remotes

    // Round / Half Trip Times, Prediction Delay & ConnectionState
    private int rttTicks; // = Mathf.CeilToInt(rttSeconds / tickInterval);
    private int httTicks; // = rttTicks / 2;

    private int halfTripTimeTicks = SimulationConstants.MinPredictionDelay;
    public int HalfTripTimeTicks => halfTripTimeTicks;

    private int roundTripTimeTicks = SimulationConstants.MinPredictionDelay;
    public int RoundTripTimeTicks => roundTripTimeTicks;

    public ConnectionState ConnectionState { get; private set; } = new();

    // Get Local Components
    private EntityInputBuffer localInputBuffer;
    public EntityInputBuffer LocalInputBuffer => localInputBuffer;


    private EntityStateBuffer localStateBuffer;
    private EntitySimulator localSimulator; // Optional if not already accessed elsewhere 
    private EntityPhysics entityPhysics;

    // Latest Information Received from Server
    private Queue<(int tick, EntityStateSnapshot snapshot)> pendingSnapshots = new();
    private EntityStateSnapshot latestServerSnapshot;

    Vector2 lastServerSnapshotPosition = Vector2.zero;
    Vector2 lastLocalSnapshotPosition = Vector2.zero;

    // Packet Loss
    private const int PacketLossWindowSize = 150;
    public float rollingPacketLossRatio { get; private set; } = 0f;

    // List of Recent Snapshots and the streak of Mismatches in it, if any
    private List<SnapshotComparisonStruct> recentSnapshots = new List<SnapshotComparisonStruct>();
    private List<SnapshotComparisonStruct> recentMismatchStreak = new List<SnapshotComparisonStruct>();
    private int currentMismatchStreak;   // How many Consecutive Mismatches we have

    bool isMismatch = false;
    bool misMatchApproximatelyFixed = false; // Approximately no Mismatch

    // If True, the Correction Logic activates
    private Vector2 positionDelta;
    private Vector2 velocityDelta;

    // Mismatch Evaluation Data with Non-Shrinking Ticks & Delta Slope 
    private int consecutiveNonShrinkingTicks = 0;
    private float previousSlope;


    // SoftCorrection Data 
    private bool isSoftCorrecting = false;
    private int isSoftCorrectingUntilTick = -1;

    private Vector2 currentSoftCorrectionPosition = Vector2.zero;
    private Vector2 currentSoftCorrectionVelocity = Vector2.zero;
    private int softCorrectionTicksRemaining = 0;

    // Remote Inter- and Extrapolation
    private float currentRemoteBufferSize = SimulationConstants.HighRemoteBufferSize;

    // Redundancy
    private readonly HashSet<int> receivedRedundancySnapshotTicks = new HashSet<int>();
    private Queue<bool> snapshotOnTimeHistory = new Queue<bool>();


    // Awake
    private void Awake()
    {
        tickInterval = SimulationConstants.TickRate > 0 ? 1f / SimulationConstants.TickRate : 0.016f; // 60 FPS

        localInputBuffer = new EntityInputBuffer(); // Create a Local Input Buffer for Prediction
        localStateBuffer = new EntityStateBuffer();

        localSimulator = GetComponent<EntitySimulator>();
        entityPhysics = GetComponent<EntityPhysics>();

        instanceID = instanceCounter++;
        Debug.Log($"[TickClient #{instanceID}] Awake on {gameObject.name}");
    }

    // Update Tick Timer, Heartbeats and PredictionDelay
    void Update()
    {
        if (isClient && isLocalPlayer)
        {
            UpdateClientTickTimer();            // Ticks
            CheckHeartbeatFromClientTimer();    // Heartbeats. Then Evaluated by Server
            UpdatePredictionDelay();            // Update Prediction Delay (Send Orders into the Future, so they don't arrive in the Past) 

        }
    }

    // Interpolate Remote Snapshots in LateUpdate Non-Host Clients Only
    private void LateUpdate()
    {
        if (isServer || !isLocalPlayer) return;
        InterpolateRemoteSnapshots();
        UpdateRemoteGhostPositions(Time.deltaTime);
    }

    // Register Entities that need to be Simulated Locally and return the ID. If they already had one, just return that. 
    public uint RegisterRemoteSimulator(EntitySimulator entitySimulator)
    {
        if (entitySimulator == null)
        {
            Debug.LogWarning($"[TickClient #{instanceID}] Tried to register a null EntitySimulator.");
            return 0;
        }

        var netId = entitySimulator.GetComponent<NetworkIdentity>().netId;

        if (remoteEntities.ContainsKey(netId))
        {
            Debug.LogWarning($"EntitySimulator already registered with ID {netId}");
            return netId;
        }

        remoteEntities[netId] = new RemoteEntityClass(entitySimulator);
        Debug.Log($"[TickClient #{instanceID}] Registered entity ID = {netId}");

        PrintAllRemoteSimulators();

        return netId;
    }


    // Unregister from Local Simulation
    public void UnregisterRemoteSimulator(EntitySimulator entitySimulator)
    {
        uint? keyToRemove = null;

        foreach (var kvp in remoteEntities)
        {
            if (kvp.Value.Simulator == entitySimulator)
            {
                keyToRemove = kvp.Key;
                break;
            }
        }

        if (keyToRemove.HasValue)
        {
            remoteEntities.Remove(keyToRemove.Value);
            Debug.Log($"[TickClient #{instanceID}] Unregistered entity ID = {keyToRemove.Value}");
            PrintAllRemoteSimulators();
        }
        else
        {
            Debug.LogWarning($"[TickClient #{instanceID}] Tried to unregister an entity that wasn't registered.");
        }
    }

    // Print all Registered Simulators for this TickClient
    private void PrintAllRemoteSimulators()
    {
        string clientID = name; // or $"{gameObject.name}" or get connectionId if available
        Debug.Log($"[TickClient #{instanceID}, ClientID: {clientID}] Local entity count: {remoteEntities.Count}");

        if (remoteEntities.Count == 0)
        {
            Debug.Log($"[TickClient #{instanceID}] No registered Local entities.");
            return;
        }

        foreach (var kvp in remoteEntities)
        {
            string simulatorName = kvp.Value.Simulator != null ? kvp.Value.Simulator.name : "null";
            Debug.Log($"[TickClient #{instanceID}, ClientID: {clientID}] Current Local Entities - ID: {kvp.Key}, Simulator: {simulatorName}");
        }
    }




    // Increment Tick based on the Timer
    private void UpdateClientTickTimer()
    {
        tickTimer += Time.deltaTime;

        while (tickTimer >= tickInterval)
        {
            UpdateClientTick();
            tickTimer -= tickInterval;
        }
    }

    // Update Client Tick and do Local Inputs  
    private void UpdateClientTick()
    {
        LocalTick++;

        if (isServer) return; // Disable Host's Double Sim

        UpdateSoftCorrection();

        //Store Local Input in a Local Buffer
        var input = localInputBuffer.GetInputOrDefault(LocalTick);
        localSimulator.Simulate(input, (LocalTick));

        // Save Local State in a Local Buffer
        var snapshot = localSimulator.CreateStateSnapshot(LocalTick);
        localStateBuffer.SaveState(LocalTick, snapshot);

        // Cleanup
        localInputBuffer.ClearOldInputs(LocalTick - SimulationConstants.BufferSize);
        localStateBuffer.ClearOldStates(LocalTick - SimulationConstants.BufferSize);

        // Process any Server Snapshots that weren't handled immediately on Arrival
        ProcessPendingSnapshots();


    }




    // Receive Server Snapshot (Server sends it every Tick)
    [TargetRpc]
    public void ReceiveLocalSnapshot(NetworkConnection target, int tick, EntityStateSnapshot snapshot)
    {
        if (isServer) return;
        ProcessOrEnqueueLocalSnapshot(tick, snapshot);
    }



    // Acknowledge and Process Server Snapshot. Also Calculate Packet Loss here
    public void ProcessOrEnqueueLocalSnapshot(int tick, EntityStateSnapshot serverSnapshot)
    {
        if (receivedRedundancySnapshotTicks.Contains(tick)) return;
        receivedRedundancySnapshotTicks.Add(tick);

        // Packet Loss Tracking Attempt. Don't think it's working too well, though. 
        bool arrivedOnTime = (tick >= LocalTick - 1);
        snapshotOnTimeHistory.Enqueue(arrivedOnTime);
        if (snapshotOnTimeHistory.Count > PacketLossWindowSize)
        {
            snapshotOnTimeHistory.Dequeue();
        }

        rollingPacketLossRatio = 1f - (float)snapshotOnTimeHistory.Count(x => x) / snapshotOnTimeHistory.Count;
        // Extra Debug Log to go with RTT / Jitter, if we want it
        // Debug.Log($"rollingPacketLossRatio over {PacketLossWindowSize} Ticks = {rollingPacketLossRatio}"); 

        if (tick > LocalTick)
        {
            pendingSnapshots.Enqueue((tick, serverSnapshot));
            int delta = tick - LocalTick;

            // Almost everything is going through this queue
            // Debug.Log($"[NetWarn] Queued future snapshot: server tick {tick}, LocalTick={LocalTick} (delta={delta})");
            return;
        }

        else if (tick <= LocalTick && !pendingSnapshots.Any(s => s.tick == tick))
        {
            CheckLocalSnapshot(tick, serverSnapshot);
        }

        // Clear old Ticks 
        int ticksToKeep = Mathf.Max(SimulationConstants.MinRedundancyTicks, (int)(SimulationConstants.RedundancyBufferTicksFactor * roundTripTimeTicks));
        int cutoffTick = LocalTick - ticksToKeep;
        receivedRedundancySnapshotTicks.RemoveWhere(t => t < cutoffTick);
    }

    // Queue
    private void ProcessPendingSnapshots()
    {
        const int MaxSnapshotsPerTick = 10;
        const int SkipThreshold = 30;

        int processed = 0;

        while (pendingSnapshots.Count > 0)
        {
            var (tick, snapshot) = pendingSnapshots.Peek();

            // Drop stale
            if (tick < LocalTick - SimulationConstants.BufferSize)
            {
                Debug.LogWarning($"[NetWarn] Dropping stale snapshot for tick {tick} (LocalTick={LocalTick})");
                pendingSnapshots.Dequeue();
                continue;
            }

            // Too early
            if (tick > LocalTick) break;

            // If we're falling behind too much, skip to latest eligible
            if (pendingSnapshots.Count > SkipThreshold)
            {
                int latestTick = -1;
                EntityStateSnapshot latestSnapshot = default;

                while (pendingSnapshots.Count > 0)
                {
                    var (t, s) = pendingSnapshots.Dequeue();
                    if (t <= LocalTick)
                    {
                        latestTick = t;
                        latestSnapshot = s;
                    }
                }

                if (latestTick != -1)
                {
                    Debug.Log($"[NetWarn] Too far ahead. Skipping to {latestTick}");
                    CheckLocalSnapshot(latestTick, latestSnapshot);
                }

                return;
            }

            // Otherwise, process normally
            pendingSnapshots.Dequeue();
            CheckLocalSnapshot(tick, snapshot);
            processed++;
            if (processed >= MaxSnapshotsPerTick) break;
        }
    }



    // Create a List of Snapshots and see if they're Mismatched. Pass that along for Evaluation
    private void CheckLocalSnapshot(int tick, EntityStateSnapshot serverSnapshot)
    {
        if (halfTripTimeTicks <= 0) return;

        int baseComparisonTick = tick - halfTripTimeTicks;
        int searchOffsetWindow = Mathf.Max(1, (int)(halfTripTimeTicks * SimulationConstants.SnapshotSearchOffsetWindow));

        EntityStateSnapshot bestLocalSnapshot = default;
        int bestLocalTick = -1;
        float bestDeltaMagnitudeSquared = float.MaxValue;

        // Search within ±searchWindow ticks of the server tick
        for (int offset = -searchOffsetWindow; offset <= searchOffsetWindow; offset++)
        {
            int candidateTick = baseComparisonTick + offset;
            if (candidateTick > LocalTick || candidateTick < 0) continue;

            if (localStateBuffer.TryGetState(candidateTick, out var candidateSnapshot))
            {
                Vector2 candidateDelta = serverSnapshot.Position - candidateSnapshot.Position;
                float sqrMagnitude = candidateDelta.sqrMagnitude;
                lastServerSnapshotPosition = serverSnapshot.Position;

                if (sqrMagnitude < bestDeltaMagnitudeSquared)
                {
                    bestDeltaMagnitudeSquared = sqrMagnitude;
                    bestLocalSnapshot = candidateSnapshot;
                    bestLocalTick = candidateTick;
                }
            }
        }

        // Debug Spam
        // Debug.Log($"Server Tick is {tick}. Candidate Local Comparison Tick is {bestLocalTick}. Diff is {tick - bestLocalTick} lastServerSnapshotPosition is {lastServerSnapshotPosition}. candidateSnapshot Position is {bestLocalSnapshot.Position}. rttTicks is {rttTicks}");

        // No usable snapshot found
        if (bestLocalTick == -1)
        {
            Debug.LogWarning($"[NetWarn] No local state found within ±{searchOffsetWindow} ticks of {baseComparisonTick} for server tick {tick} (LocalTick={LocalTick})");
            positionDelta = Vector2.zero;
            velocityDelta = Vector2.zero;
            isMismatch = false;
            return;
        }

        // Else Continue:
        positionDelta = serverSnapshot.Position - bestLocalSnapshot.Position;
        velocityDelta = serverSnapshot.Velocity - bestLocalSnapshot.Velocity;
        lastLocalSnapshotPosition = bestLocalSnapshot.Position;
        isMismatch = positionDelta.sqrMagnitude > SimulationConstants.MinimumDeltaSquared;

        var snapshot = new SnapshotComparisonStruct
        {
            evaluatedTick = bestLocalTick,
            hasMismatch = isMismatch,
            positionDelta = positionDelta,
            velocityDelta = velocityDelta,
        };

        recentSnapshots.Add(snapshot);

        // Maintain Rolling Buffer
        if (recentSnapshots.Count > SimulationConstants.MaxSnapshotStructs) recentSnapshots.RemoveAt(0);

        // Track mismatch streak
        currentMismatchStreak = Mathf.Min(snapshot.hasMismatch ? currentMismatchStreak + 1 : 0, SimulationConstants.MaxSnapshotStructs);

        // Run EvaluateLocalSnapshot and decide what Correction to Perform, if any 
        var action = EvaluateLocalSnapshot(currentMismatchStreak);
        if (action == CorrectionAction.HardCorrect)
        {
            ApplyServerSnapshot(bestLocalTick, bestLocalSnapshot, serverSnapshot);
        }

        else if (action == CorrectionAction.SoftCorrect)
        {
            if (!isSoftCorrecting)
            {
                // TrySoftCorrect(LocalTick - bestLocalTick);
                // TrySoftCorrect(LocalTick - baseComparisonTick + searchOffsetWindow);
                TrySoftCorrect(halfTripTimeTicks);
            }
        }
        //else if (action == CorrectionAction.None)
        //{

        //}
    }

    private enum CorrectionAction
    {
        None,
        SoftCorrect,
        HardCorrect
    }

    // Evaluate the Mismatches in the List and see if we need to do Nothing, HardCorrect or SoftCorrect
    private CorrectionAction EvaluateLocalSnapshot(int currentMismatchStreak)
    {
        // If Count is < minTrendSamples, just Return None <3
        if (currentMismatchStreak < SimulationConstants.MinTrendSamples) return CorrectionAction.None;

        // Set the Start Index based on currentMismatchStreak , with a Null Check
        int startIndex = Mathf.Max(recentSnapshots.Count - currentMismatchStreak, 0);

        // Turn that into a new List
        var mismatchWindow = recentSnapshots.Skip(startIndex).Take(currentMismatchStreak).ToList();

        // Extract the Deltas from the List as Magnitudes. Now we have a list of Deltas
        var positionDeltas = mismatchWindow.Select(s => s.positionDelta.magnitude).ToList();

        // Check if Deltas are Shrinking with a simple y = a*x + b Formula
        float sumX = 0f, sumY = 0f, sumXY = 0f, sumXX = 0f;
        int n = positionDeltas.Count;

        for (int i = 0; i < n; i++)
        {
            float x = i;
            float y = positionDeltas[i];

            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumXX += x * x;
        }

        // Get the Total Change in Deltas and the Average Delta Change
        float deltaChangeSum = 0f;
        for (int i = 1; i < positionDeltas.Count; i++)
        {
            deltaChangeSum += Mathf.Abs(positionDeltas[i] - positionDeltas[i - 1]);
        }
        float averageDeltaChange = deltaChangeSum / (positionDeltas.Count - 1);

        // Shrinking if slope is negative (i.e., downward trend)
        float slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
        bool shrinking = slope < SimulationConstants.SlopeDeadzone;

        // Set Threshold for when we take action if our Deltas refuse to Shrink
        int mismatchThreshold = Mathf.Max(1, Mathf.FloorToInt(SimulationConstants.MaxSnapshotStructs * SimulationConstants.MisMatchThresholdRatio));

        // Get Latest and Average Deltas
        float latestDelta = positionDeltas[^1];
        float averageDelta = positionDeltas.Average();

        // Velocity
        float velocitySqrMag = entityPhysics.Velocity.sqrMagnitude;
        float velocityFactor = Mathf.Clamp01(velocitySqrMag / 10f); // TODO: Replace with Dynamic Max Player Speed from PlayerStats

        // Set Base Max Delta for forcing Hard Corrections, either in General, or because the Deltas refuse to Shrink
        float baseMaxAllowedDelta = halfTripTimeTicks * SimulationConstants.AcceptableThreshold;    // Perhaps base this on Player Speed
        float baseMaxAllowedDeltaNonShrinking = halfTripTimeTicks * (SimulationConstants.AcceptableThreshold * SimulationConstants.AcceptableThresholdSlopesModifier);

        // Velocity Lerp: Idle gets Lower/Tighter Threshold, Fast Movement gets Higher/Tighter Threshold
        float velocityScaledDeltaThreshold = Mathf.Lerp(baseMaxAllowedDelta * SimulationConstants.HardCorrectionIdleFactor, baseMaxAllowedDelta * SimulationConstants.HardCorrectionMovingFactor, velocityFactor);
        float velocityScaledDeltaThresholdNonShrinking = Mathf.Lerp(baseMaxAllowedDeltaNonShrinking * SimulationConstants.HardCorrectionIdleFactor, baseMaxAllowedDeltaNonShrinking * SimulationConstants.HardCorrectionMovingFactor, velocityFactor);

        // Replace uses of maxAllowedDelta and maxAllowedDeltaNonShrinking:
        float maxAllowedDelta = velocityScaledDeltaThreshold;
        float maxAllowedDeltaNonShrinking = velocityScaledDeltaThresholdNonShrinking;

        // Evaluate the Slope
        float slopeDelta = slope - previousSlope;
        previousSlope = slope;
        bool slopeImproving = slopeDelta < 0f; // Slope moving back down

        // If not Shrinking based on the Slope for mismatchThreshold Ticks we look to HardCorrect
        if (!shrinking)
        {
            consecutiveNonShrinkingTicks++;

            if (consecutiveNonShrinkingTicks >= mismatchThreshold)
            {

                // If not Shrinking for a Longer Period of Time. HardCorrect, or give SoftCorrect another chance to fix it
                if (averageDelta > maxAllowedDeltaNonShrinking && averageDeltaChange > SimulationConstants.maxAllowedDeltaChange && !slopeImproving)
                {
                    Debug.Log($"[Evaluate] Δs not shrinking for {consecutiveNonShrinkingTicks} Ticks compared to Max {mismatchThreshold}. Slope at {slope}. Last Δ at {latestDelta}. Avg Δ: {averageDelta}, ΔΔ: {averageDeltaChange}, Max Δ for Shrinking Trend at {maxAllowedDeltaNonShrinking}. Forcing HardCorrect.");
                    consecutiveNonShrinkingTicks = 0; // reset
                    return CorrectionAction.HardCorrect;
                }
                else
                {
                    Debug.Log($"[Evaluate] Δs not shrinking for {consecutiveNonShrinkingTicks} Ticks compared to Max {mismatchThreshold}. Slope at {slope}. Last Δ at {latestDelta}. Avg Δ: {averageDelta}, ΔΔ: {averageDeltaChange}, Max Δ for Shrinking Trend at {maxAllowedDeltaNonShrinking}. Too small to force HardCorrect.");
                    return CorrectionAction.SoftCorrect;
                }
            }
        }

        // Delta Shrinking. Reset Counter if trend improved
        else
        {
            consecutiveNonShrinkingTicks = 0;
        }

        // Debug.Log($"Slope at {slope}. Consecutive Non-Shrinking Ticks at {consecutiveNonShrinkingTicks}. Shrinking is {shrinking}. Latest Delta is {latestDelta}. Max Delta for Shrinking Trend {maxAllowedDeltaNonShrinking}.");

        // Average Delta too big. Correct instantly
        if (averageDelta > maxAllowedDelta)
        {
            Debug.Log($"[Evaluate] Avg Δ too Big at {averageDelta} compared to Max. {maxAllowedDelta}. Forcing HardCorrect.");
            return CorrectionAction.HardCorrect;
        }

        // Baseline Action is to SoftCorrect. If the Delta isn't big enough to be worth Correcting, SoftCorrect will determine that from there
        return CorrectionAction.SoftCorrect;
    }

    private Vector2 totalPositionCorrection;
    private Vector2 appliedPositionCorrectionSoFar;

    private Vector2 totalVelocityCorrection;
    private Vector2 appliedVelocityCorrectionSoFar;

    private int softCorrectionTotalTicks;



    // Enable the SoftCorrections in UpdateSoftCorrection
    private void TrySoftCorrect(int cooldownTicks)
    {
        if (positionDelta == Vector2.zero || !isMismatch || isSoftCorrecting) return;

        totalPositionCorrection = positionDelta;
        appliedPositionCorrectionSoFar = Vector2.zero;

        totalVelocityCorrection = velocityDelta;
        appliedVelocityCorrectionSoFar = Vector2.zero;

        softCorrectionTotalTicks = cooldownTicks - 1;
        softCorrectionTicksRemaining = softCorrectionTotalTicks;

        isSoftCorrecting = true;
        isSoftCorrectingUntilTick = LocalTick + cooldownTicks + 1; // +1 For Safety
    }

    // Soft Correct by adding Position & Velocity of the last Server Snapshot in a RTT-Dynamic Tick Period. Keep in mind, we won't be able to see its Results until that Period is Complete
    private void UpdateSoftCorrection()
    {
        // Return if we're not SoftCorrecting or if the Discrepancy is approximately Fixed for any other Reason
        if (!isSoftCorrecting) return;

        if (softCorrectionTicksRemaining <= 0 || LocalTick >= isSoftCorrectingUntilTick || misMatchApproximatelyFixed)
        {
            isSoftCorrecting = false;
            softCorrectionTicksRemaining = 0;
            isSoftCorrectingUntilTick = -1;

            totalPositionCorrection = Vector2.zero;
            appliedPositionCorrectionSoFar = Vector2.zero;
            totalVelocityCorrection = Vector2.zero;
            appliedVelocityCorrectionSoFar = Vector2.zero;
            return;
        }

        if (softCorrectionTicksRemaining > 0)
        {
            // Ease-out Quad
            float t = 1f - (softCorrectionTicksRemaining / (float)softCorrectionTotalTicks);
            float easedT = 1f - Mathf.Pow(1f - t, 2);

            Vector2 positionTargetOffset = totalPositionCorrection * easedT;
            Vector2 positionTargetCorrectionStep = positionTargetOffset - appliedPositionCorrectionSoFar;
            Vector2 positionSmoothedStep = Vector2.Lerp(Vector2.zero, positionTargetCorrectionStep, 1 - Mathf.Exp(-SimulationConstants.GhostSmoothingSpeed * Time.deltaTime));
            appliedPositionCorrectionSoFar += positionSmoothedStep;

            Vector2 velocityTargetOffset = totalVelocityCorrection * easedT;
            Vector2 velocityTargetCorrectionStep = velocityTargetOffset - appliedVelocityCorrectionSoFar;
            Vector2 velocitySmoothedStep = Vector2.Lerp(Vector2.zero, velocityTargetCorrectionStep, 1 - Mathf.Exp(-SimulationConstants.GhostSmoothingSpeed * Time.deltaTime));
            appliedVelocityCorrectionSoFar += velocitySmoothedStep;


            // Don't Correct Upward if we're GroundedUp
            if (entityPhysics.GroundedUp && positionTargetCorrectionStep.y > 0f)
            {
                positionTargetCorrectionStep.y = 0f;
                velocityTargetCorrectionStep.y = 0f;
            }

            // Don't Correct Downward if we're GroundedDown
            if (entityPhysics.GroundedDown && positionTargetCorrectionStep.y < 0f)
            {
                positionTargetCorrectionStep.y = 0f;
                velocityTargetCorrectionStep.y = 0f;
            }

            // Don't Correct Leftward if we're GroundedLeft
            if (entityPhysics.GroundedLeft && positionTargetCorrectionStep.x < 0f)
            {
                positionTargetCorrectionStep.x = 0f;
                velocityTargetCorrectionStep.x = 0f;
            }

            // Don't Correct Rightward if we're GroundedRight
            if (entityPhysics.GroundedRight && positionTargetCorrectionStep.x > 0f)
            {
                positionTargetCorrectionStep.x = 0f;
                velocityTargetCorrectionStep.x = 0f;
            }

            // Apply X Corrections only if Velocity.x != 0
            if (entityPhysics.Velocity.x != 0f)
            {
                entityPhysics.AddPosition(new Vector2(positionSmoothedStep.x, 0));
                // entityPhysics.AddVelocity(new Vector2(velocityTargetCorrectionStep.x * SimulationConstants.SoftCorrectionVelocityModifier, 0));
            }

            // Apply Y Corrections only if Velocity.y != 0
            if (entityPhysics.Velocity.y != 0f)
            {
                entityPhysics.AddPosition(new Vector2(0, positionSmoothedStep.y));
                // entityPhysics.AddVelocity(new Vector2(0, velocityTargetCorrectionStep.y * SimulationConstants.SoftCorrectionVelocityModifier));
            }

            // Always take off a softCorrectionTick and check if the Mismatch is already fixed
            softCorrectionTicksRemaining--;
            misMatchApproximatelyFixed = (entityPhysics.Position - lastServerSnapshotPosition).sqrMagnitude <= SimulationConstants.MinimumDeltaSquared;
            Debug.Log($"[SoftCorrecting] LocalTick: {LocalTick}. Current positionSmoothedStep: {positionSmoothedStep}. Current velocityTargetCorrectionStep: {velocityTargetCorrectionStep}. Duration: {isSoftCorrectingUntilTick - LocalTick} Ticks. From Current Server Position: {lastServerSnapshotPosition}. To Current Local Position: {lastLocalSnapshotPosition}.");
        }
    }




    // Apply Hard Correction from Tick - HTTT and Resimulate Inputs from there
    private void ApplyServerSnapshot(int bestLocalTick, EntityStateSnapshot bestLocalSnapshot, EntityStateSnapshot serverSnapshot)
    {
        // localSimulator.ApplySnapshot(serverSnapshot); // Most Authoritative, i think
        localSimulator.InterpolateSnapshot(bestLocalSnapshot, serverSnapshot, SimulationConstants.HardCorrectionSmoothingSpeed); // Slightly Smoother

        for (int t = bestLocalTick + 1; t <= LocalTick; t++)
        {
            var input = localInputBuffer.GetInputOrDefault(t);
            localSimulator.Simulate(input, t);
            localStateBuffer.SaveState(t, localSimulator.CreateStateSnapshot(t));
        }

        int correctedTicks = LocalTick - bestLocalTick;
        Debug.Log($"[Correction] Snapshot applied at Tick {bestLocalTick} → Re-simulating {correctedTicks} Ticks to LocalTick {LocalTick}");
    }


    // Remote Side
    [TargetRpc]
    public void ReceiveRemoteSnapshots(NetworkConnection target, uint entityID, int tick, EntityStateSnapshot snapshot)
    {
        if (isServer) return;
        ProcessRemoteSnapshot(entityID, tick, snapshot);

    }

    // Save the Snapshots in a Small Buffer for each remoteEntity
    private void ProcessRemoteSnapshot(uint entityID, int tick, EntityStateSnapshot snapshot)
    {
        // Null Check with Debug
        if (!remoteEntities.TryGetValue(entityID, out var remoteEntity))
        {
            Debug.LogWarning($"[TickClient] No entity found with netID {entityID} for remote snapshot.");
            PrintAllRemoteSimulators();
            return;
        }

        // Store with De-duplication. If we already have something Stored, don't do anything
        if (!remoteEntity.StateBuffer.TryGetState(tick, out _))
        {
            remoteEntity.StateBuffer.SaveState(tick, snapshot);
        }
    }



    // Interpolate all Remote Entities from their last known Snapshot to newer one. Generates Lag, but gives Smoothness
    private void InterpolateRemoteSnapshots()
    {
        if (isServer) return;
        // Interpolation Tick based on Dynamic HTTT
        int remoterenderTick = LocalTick - halfTripTimeTicks;

        // Dynamically determine buffer size, but keep it consistent for this frame
        int remoteBufferTargetSize = ConnectionState.Stability switch
        {
            ConnectionStability.Stable => SimulationConstants.LowRemoteBufferSize,
            ConnectionStability.Unstable => SimulationConstants.MediumRemoteBufferSize,
            ConnectionStability.Critical => SimulationConstants.HighRemoteBufferSize,
            _ => SimulationConstants.MediumRemoteBufferSize // Fallback
        };

        Debug.Log($"remoteBufferTargetSize = {remoteBufferTargetSize} | ConnectionState.Stability = {ConnectionState.Stability}");

        // Smoothly interpolate buffer size over time (lerp factor 0.1f is tweakable)
        currentRemoteBufferSize = Mathf.Lerp(currentRemoteBufferSize, remoteBufferTargetSize, 0.1f);
        int remoteBufferSize = Mathf.RoundToInt(currentRemoteBufferSize);

        if (remoterenderTick < remoteBufferSize) return; // Not Enough to Interpolate yet


        foreach (var kvp in remoteEntities)
        {
            var remoteEntity = kvp.Value;

            if (remoteEntity.Simulator == localSimulator) continue; // Skip Local Player. Just in case. 

            var buffer = remoteEntity.StateBuffer;

            if (buffer.TryGetState(remoterenderTick, out var from) && buffer.TryGetState(remoterenderTick + 1, out var to))
            {
                float alpha = Mathf.Clamp01(TickAlpha);

                if (remoteEntity.WasExtrapolatedLastTick)
                {
                    Vector2 interpolatedPos = Vector2.Lerp(from.Position, to.Position, alpha);
                    Vector2 blendedPos = Vector2.Lerp(remoteEntity.LastExtrapolatedPosition, interpolatedPos, SimulationConstants.ExtrapolationToInterpolationLerp);
                    remoteEntity.Simulator.EntityPhysics.Position = blendedPos;

                    // Still interpolate velocity normally
                    remoteEntity.Simulator.EntityPhysics.Velocity = Vector2.Lerp(from.Velocity, to.Velocity, alpha);
                    remoteEntity.WasExtrapolatedLastTick = false;
                }

                else
                {
                    // Normal interpolation
                    remoteEntity.Simulator.InterpolateSnapshot(from, to, alpha);
                }
            }

            // Extrapolation from Last Known State Fallback
            else
            {
                if (buffer.TryGetLatestSnapshot(remoterenderTick, out var latestAvailable))
                {
                    float ticksSinceLast = LocalTick - from.Tick;
                    // float dt = ticksSinceLast * tickInterval;

                    float dt = tickInterval; // Or Without Dynamic
                    float clampedDt = Mathf.Min(dt, SimulationConstants.MaxExtrapolationTime);

                    remoteEntity.Simulator.ExtrapolateFromSnapshot(latestAvailable, clampedDt);
                    remoteEntity.LastExtrapolatedPosition = remoteEntity.Simulator.EntityPhysics.Position;
                    remoteEntity.WasExtrapolatedLastTick = true;
                }

            }

            // Clear when done. Keep a Minimum of Snapshots
            int clearThreshold = Mathf.Min(remoterenderTick - remoteBufferSize, LocalTick - SimulationConstants.LowRemoteBufferSize);
            remoteEntity.StateBuffer.ClearOldStates(clearThreshold);
        }
    }

    // Ghost Positions
    private void UpdateRemoteGhostPositions(float deltaTime)
    {
        foreach (var kvp in remoteEntities)
        {
            var remoteEntity = kvp.Value;
            if (remoteEntity.Simulator == localSimulator) continue; // Skip local player

            // Initialize
            if (!remoteEntity.GhostPositionInitialized)
            {
                remoteEntity.GhostPosition = remoteEntity.Simulator.EntityPhysics.Position;
                remoteEntity.GhostPositionInitialized = true;
            }

            // Smoothly lerp visual position toward simulation position every frame
            // Consider Storing the Pre-Ghost Smoothed remoteEntity Position for later use
            remoteEntity.GhostPosition = Vector2.Lerp(remoteEntity.GhostPosition, remoteEntity.Simulator.EntityPhysics.Position, 1 - Mathf.Exp(-SimulationConstants.GhostSmoothingSpeed * deltaTime));
            remoteEntity.Simulator.EntityPhysics.Position = new Vector2(remoteEntity.GhostPosition.x, remoteEntity.GhostPosition.y);
        }
    }


    // Synchronize Tick Watches with Server
    [TargetRpc]
    public void SyncTickOnClient(int serverTick, double serverTime)
    {
        //float timeSinceSync = (float)(NetworkTime.time - serverTime);
        //int estimatedTick = serverTick + Mathf.FloorToInt(timeSinceSync / tickInterval);

        int drift = Mathf.Abs(LocalTick - serverTick);
        if (drift >= 1)
        {
            // Debug.Log($"[TickSync] Adjusting Client Tick from {LocalTick} to {serverTick} (drift={drift}).");
            LocalTick = serverTick;
            tickTimer = 0f;
        }
    }



    // Client Timer for Sending Pings
    private void CheckHeartbeatFromClientTimer()
    {
        heartbeatTimer += Time.deltaTime;

        if (heartbeatTimer >= SimulationConstants.HeartbeatInterval)
        {
            heartbeatTimer = 0f;
            CmdRequestHeartbeat(); // <- NEW Command

            // CmdSendHeartbeatToServer(NetworkTime.time); // ← pass the timestamp
        }
    }

    // Send Heartbeat to Server and ask it to report back (Ping Pong)
    [Command]
    private void CmdRequestHeartbeat()
    {
        double serverTimeSent = NetworkTime.time;
        TargetRespondToHeartbeat(connectionToClient, serverTimeSent);
    }

    // Heartbeat Response from Server
    [TargetRpc]
    private void TargetRespondToHeartbeat(NetworkConnection target, double serverTimeSent)
    {
        double clientTimeReceived = NetworkTime.time; // Just in case we want to use it somewhere. Probably not, though. 
        CmdReturnHeartbeatResponse(serverTimeSent, clientTimeReceived);
    }

    // Now calculate the actual Stats with the Information from the Server (Not the Client, as we had it before)
    [Command]
    private void CmdReturnHeartbeatResponse(double serverTimeSent, double clientTimeReceived)
    {
        // New
        double serverTimeReceived = NetworkTime.time;
        double roundTripTime = serverTimeReceived - serverTimeSent;
        float rttF = (float)roundTripTime;

        // RTT smoothing
        if (ConnectionState.smoothedRTT == 0)
            ConnectionState.smoothedRTT = rttF;
        else
            ConnectionState.smoothedRTT = Mathf.Lerp(ConnectionState.smoothedRTT, rttF, 0.1f);

        // Jitter
        float jitter = Mathf.Abs(ConnectionState.smoothedRTT - rttF);
        float transit = rttF;
        float jitterDelta = Mathf.Abs(transit - ConnectionState.lastTransit);
        ConnectionState.lastTransit = transit;

        // Respond Fast when going up, Respond Slow if going down.
        if (ConnectionState.smoothedJitter == 0)
            ConnectionState.smoothedJitter = jitterDelta;
        else
        {
            if (jitterDelta > ConnectionState.smoothedJitter)
            {
                ConnectionState.smoothedJitter += (jitterDelta - ConnectionState.smoothedJitter) * SimulationConstants.JitterSmoothIncrease;
            }
            else
            {
                ConnectionState.smoothedJitter += (jitterDelta - ConnectionState.smoothedJitter) * SimulationConstants.JitterSmoothDecrease;
            }
        }

        //// Packet Loss
        //float rawPacketLossRatio = rollingPacketLossRatio;

        //// Respond Fast when going up, Respond Slow if going down.
        //if (ConnectionState.smoothedPacketLossRatio == 0)
        //    ConnectionState.smoothedPacketLossRatio = rawPacketLossRatio;

        //else
        //{
        //    if (rawPacketLossRatio > ConnectionState.smoothedPacketLossRatio)
        //    {
        //        ConnectionState.smoothedPacketLossRatio += (rawPacketLossRatio - ConnectionState.smoothedPacketLossRatio) * SimulationConstants.PacketLossSmoothIncrease;
        //    }
        //    else
        //    {
        //        ConnectionState.smoothedPacketLossRatio += (rawPacketLossRatio - ConnectionState.smoothedPacketLossRatio) * SimulationConstants.PacketLossSmoothDecrease;
        //    }
        //}


        // Quality based on RTT
        if (rttF < SimulationConstants.RTT_Excellent)
            ConnectionState.Quality = ConnectionQuality.Excellent;
        else if (rttF < SimulationConstants.RTT_Moderate)
            ConnectionState.Quality = ConnectionQuality.Moderate;
        else
            ConnectionState.Quality = ConnectionQuality.Poor;

        // Stability based on both Jitter and Packet Loss
        bool isJitterStable = ConnectionState.smoothedJitter < SimulationConstants.Jitter_Stable;
        bool isJitterUnstable = ConnectionState.smoothedJitter < SimulationConstants.Jitter_Unstable;
        bool isJitterCritical = ConnectionState.smoothedJitter >= SimulationConstants.Jitter_Unstable;

        bool isPacketLossStable = ConnectionState.smoothedPacketLossRatio < SimulationConstants.PacketLoss_Stable;
        bool isPacketLossUnstable = ConnectionState.smoothedPacketLossRatio < SimulationConstants.PacketLoss_Unstable;
        bool isPacketLossCritical = ConnectionState.smoothedPacketLossRatio >= SimulationConstants.PacketLoss_Unstable;

        if (isJitterStable) // Cut out the Packet Loss for now
        {
            ConnectionState.Stability = ConnectionStability.Stable;
        }
        else if (isJitterUnstable)
        {
            ConnectionState.Stability = ConnectionStability.Unstable;
        }
        else if (isJitterCritical)
        {
            ConnectionState.Stability = ConnectionStability.Critical;
        }

        // Update last heartbeat
        ConnectionState.lastHeartbeatTime = serverTimeReceived;

        // Update combined status
        ConnectionState.Status = GetCombinedStatus();

        // Inform the client
        TargetReceiveConnectionStatus(connectionToClient,
            ConnectionState.Status,
            ConnectionState.Quality,
            ConnectionState.Stability,
            rttF,
            ConnectionState.smoothedJitter,
            ConnectionState.smoothedPacketLossRatio
            );
    }

    // Combine Stability & Heartbeat to decide on final Connection Status
    private ConnectionCombinedStatus GetCombinedStatus()
    {
        double now = NetworkTime.time;
        double timeSinceLastHeartbeat = now - ConnectionState.lastHeartbeatTime;

        ConnectionCombinedStatus status;

        if (timeSinceLastHeartbeat > SimulationConstants.HeartbeatTimeout + SimulationConstants.GraceDisconnectDelay)
            status = ConnectionCombinedStatus.Disconnected;
        else if (timeSinceLastHeartbeat > SimulationConstants.HeartbeatTimeout)
            status = ConnectionCombinedStatus.Disconnecting;
        else if (ConnectionState.Stability == ConnectionStability.Critical)
            status = ConnectionCombinedStatus.TimingOut;
        else
            status = ConnectionCombinedStatus.Connected;

        ConnectionState.Status = status;

        return status;
    }

    [TargetRpc]
    private void TargetReceiveConnectionStatus(NetworkConnection target, ConnectionCombinedStatus status, ConnectionQuality quality, ConnectionStability stability, float rttF, float jitter, float packetLossRatio)
    {
        ConnectionState.smoothedRTT = rttF;
        ConnectionState.smoothedJitter = jitter;
        ConnectionState.smoothedPacketLossRatio = packetLossRatio;
        ConnectionState.Status = status;
        ConnectionState.Quality = quality;
        ConnectionState.Stability = stability;

        // Add this to the Debug.Log after Jitter if we fix that: 
        // Packet Loss Ratio: {packetLossRatio:F3}s |

        // Debug Spam
        // Debug.Log($"RTT: {rttF:F3}s | Jitter: {jitter:F3}s | Stability: {stability} | Quality: {quality} | Status: {status}");
        // Some Information on ConnectionState.Stability Here? 
    }


    // Set Prediction Input Delay to Smoothed Half Trip Time, then Clamped. Also store full RTT
    public void UpdatePredictionDelay()
    {
        float rttSeconds = ConnectionState.smoothedRTT;
        float jitterSeconds = ConnectionState.smoothedJitter;

        rttTicks = Mathf.CeilToInt(rttSeconds / tickInterval);
        httTicks = rttTicks / 2;

        int jitterTicks = Mathf.CeilToInt(jitterSeconds / tickInterval);
        int jitterTicksWithMargin = Mathf.CeilToInt(jitterTicks * SimulationConstants.JitterMargin);

        halfTripTimeTicks = Mathf.Clamp(httTicks + jitterTicksWithMargin, SimulationConstants.MinPredictionDelay, SimulationConstants.MaxPredictionDelay); // Use new variable instead of httTicks
        roundTripTimeTicks = Mathf.Clamp(rttTicks + jitterTicksWithMargin, SimulationConstants.MinPredictionDelay, SimulationConstants.MaxPredictionDelay); // Use new variable instead of rttTicks

        // Debug.Log($"[PredictionDelay]: {halfTripTimeTicks} ticks. Base HTT Ticks: {httTicks}. Jitter Ticks: {jitterTicks} * {SimulationConstants.JitterMargin}.");
    }

    // Store Input Locally for Prediction
    public void StoreLocalInput(int tick, EntityInputStruct input)
    {
        localInputBuffer.StoreInput(tick, input);
    }
}
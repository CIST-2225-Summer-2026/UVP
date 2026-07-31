# Underground Robot AR Visualization Specification

## 1. Purpose

This document specifies the augmented reality visualization portion of a system used to display the estimated position and orientation of a robot operating underground.

A separate robot team will design and build the robot and its localization system. The robot will capture a GPS position before entering the underground environment. After GPS is no longer available, the robot system will calculate its movement relative to that initial surface position and report its current local Cartesian position and orientation.

The Unity application covered by this specification will receive that pose data, transform it into the application's calibrated real-world coordinate frame, and display a virtual representation of the robot at the estimated underground location.

## 2. Project Baseline

The current project is based on the Unity Mixed Reality template.

- Unity Editor: `6000.4.7f1`
- Render pipeline: Universal Render Pipeline `17.4.0`
- XR framework: OpenXR `1.16.1`
- Meta XR support: Meta OpenXR `2.5.0`
- AR framework: AR Foundation `6.4.2`
- Interaction framework: XR Interaction Toolkit `3.4.1`
- Input System: `1.19.0`
- Current build scene: `Assets/Scenes/SampleScene.unity`
- Primary target device: Meta Quest 3

The current scene and scripts are template content. No robot-specific pose receiver, calibration system, robot model, or underground visualization system is currently implemented.

## 3. Scope

### 3.1 In Scope

The first version will provide:

1. A data interface for receiving a continuously updated robot pose.
2. A representation of the robot's initial surface GPS anchor.
3. A local Cartesian coordinate system based on Unity conventions.
4. A manual calibration process that aligns the robot coordinate frame with the AR application's world coordinate frame.
5. Conversion of received robot poses into Unity world-space poses.
6. A virtual robot model displayed at the transformed position and orientation.
7. Visual aids that allow the user to understand where the underground robot is located.
8. Connection, calibration, pose age, and data-validity status information.
9. A simulated pose source for development and testing before the robot interface is available.

### 3.2 Out of Scope

The first version will not:

- Control or navigate the robot.
- Calculate the robot's underground position.
- Perform dead reckoning, inertial navigation, or sensor fusion for the robot.
- Correct errors in the robot team's localization estimate.
- Design the robot communication hardware.
- Render an accurate underground tunnel or soil model unless such geometry is provided separately.
- Automatically align the AR coordinate frame using GPS alone.
- Guarantee centimeter-level alignment between the reported robot pose and the AR visualization.

## 4. System Context

The complete system consists of two major subsystems.

### 4.1 Robot Localization Subsystem

Owned by the robot team.

Responsibilities:

- Capture an initial GPS coordinate before the robot goes underground.
- Establish the robot's local Cartesian coordinate frame.
- Estimate the robot's underground position and orientation relative to that frame.
- Continuously publish robot pose updates.
- Include timestamps and validity information with pose updates.

### 4.2 AR Visualization Subsystem

Owned by the visualization team.

Responsibilities:

- Receive robot pose updates.
- Validate and retain the latest usable pose.
- Allow the user to manually align the robot coordinate frame with the physical site.
- Convert the robot pose into Unity world coordinates.
- Display the robot at its calculated location and orientation.
- Clearly indicate stale, invalid, disconnected, or uncalibrated states.

## 5. Coordinate Systems

The implementation must keep the following coordinate systems conceptually separate.

### 5.1 GPS Anchor

The robot captures a GPS coordinate at or before the point where it enters the underground environment.

The GPS anchor should be represented as:

```text
latitudeDegrees
longitudeDegrees
altitudeMeters
horizontalAccuracyMeters
verticalAccuracyMeters
capturedAtUtc
```

The GPS anchor identifies the approximate geographic starting location. It does not by itself define the orientation of Unity's axes or provide sufficiently precise AR alignment.

### 5.2 Robot Local Coordinate Frame

The robot reports its underground pose using Unity-style Cartesian conventions:

- Units: meters
- `+X`: right
- `+Y`: up
- `+Z`: forward
- Position: `Vector3`
- Orientation: Unity-compatible quaternion
- Quaternion component order in serialized data: `x, y, z, w`
- Quaternion handedness and rotation behavior must match Unity's left-handed scene convention

The robot team's origin is expected to correspond to the GPS anchor or to a documented point with a known offset from it.

The exact physical direction of `+Z` at the surface must be established during manual calibration.

### 5.3 Unity Tracking Space

This is the coordinate frame maintained by the Meta Quest and Unity XR runtime for the current AR session.

It may reset or drift between sessions. Robot poses must not be placed directly into this frame without calibration.

### 5.4 Calibrated Site Frame

The calibrated site frame is a Unity `Transform` representing the robot team's coordinate origin and orientation inside the current Unity tracking space.

All robot poses will be children of, or mathematically transformed by, this site-frame transform.

The core transformation is:

```csharp
unityPosition = siteOrigin.TransformPoint(robotLocalPosition);
unityRotation = siteOrigin.rotation * robotLocalRotation;
```

This design isolates AR alignment from the incoming robot data.

## 6. Manual Calibration

### 6.1 Calibration Goal

Manual calibration must determine:

1. The Unity world position corresponding to the robot coordinate origin.
2. The Unity world direction corresponding to the robot coordinate frame's `+Z` axis.
3. The `+Y` direction, normally aligned with Unity world up unless site requirements indicate otherwise.

### 6.2 Recommended Two-Step Calibration

The initial implementation should use a two-step process.

#### Step 1: Set Origin

The user stands at, points to, or places a marker at the physical location corresponding to the robot's local origin and confirms **Set Origin**.

The app stores that Unity world position as the site origin.

#### Step 2: Set Forward Direction

The user points toward or places a second marker along the physical direction corresponding to robot-local `+Z` and confirms **Set Forward**.

The app calculates a horizontal forward direction from the origin to the second point and creates the site-frame rotation.

The minimum distance between the origin and forward calibration points should be configurable. A default minimum of `1.0 meter` is recommended to reduce angular error.

### 6.3 Calibration Controls

The calibration interface must include:

- Begin calibration
- Set origin
- Set forward direction
- Preview coordinate axes
- Confirm calibration
- Cancel calibration
- Reset calibration
- Recalibrate

### 6.4 Calibration Visualization

During and after calibration, the app should display:

- Origin marker
- Red `+X` axis
- Green `+Y` axis
- Blue `+Z` axis
- Text labels for axes
- Distance between calibration points
- Confirmation that calibration is complete

### 6.5 Calibration Persistence

For the first implementation, calibration may be session-only.

The architecture should permit later persistence through an XR anchor or a saved site configuration without changing the pose-conversion components.

The app must not silently reuse a saved calibration unless it can verify that the saved anchor has been restored correctly.

## 7. Robot Pose Data Contract

The transport protocol is not yet selected. The application must therefore define a transport-independent data model and receiver interface.

### 7.1 Pose Message

Recommended JSON representation:

```json
{
  "schemaVersion": "1.0",
  "robotId": "robot-01",
  "sequenceNumber": 1452,
  "timestampUtc": "2026-07-23T22:15:41.235Z",
  "anchor": {
    "latitudeDegrees": 35.0844,
    "longitudeDegrees": -106.6504,
    "altitudeMeters": 1619.2,
    "horizontalAccuracyMeters": 2.5,
    "verticalAccuracyMeters": 4.0,
    "capturedAtUtc": "2026-07-23T22:03:10.000Z"
  },
  "positionMeters": {
    "x": 3.25,
    "y": -4.80,
    "z": 12.40
  },
  "orientation": {
    "x": 0.0,
    "y": 0.3826834,
    "z": 0.0,
    "w": 0.9238795
  },
  "positionAccuracyMeters": 0.35,
  "orientationAccuracyDegrees": 4.0,
  "trackingState": "Tracking"
}
```

### 7.2 Required Fields

Each pose update must contain:

- Schema version
- Robot identifier
- Monotonically increasing sequence number
- Source timestamp in UTC
- Local position in meters
- Local orientation as a quaternion
- Tracking state or validity indicator

The initial GPS anchor may be transmitted with every message or sent once in a separate session/configuration message.

### 7.3 Tracking State

Supported values should include:

```text
Initializing
Tracking
Degraded
Lost
Stopped
Error
```

Only `Tracking` and, if explicitly configured, `Degraded` poses should update the displayed robot transform.

### 7.4 Validation Rules

The application must reject or flag messages when:

- Required fields are missing.
- Numeric fields contain `NaN` or infinity.
- The quaternion magnitude is effectively zero.
- The timestamp is unreasonably far in the future.
- The sequence number is older than the last accepted update.
- The robot identifier does not match the selected robot.
- The reported pose exceeds configurable site bounds.

Valid non-unit quaternions should be normalized before use.

## 8. Data Interface Architecture

### 8.1 Pose Source Interface

```csharp
public interface IRobotPoseSource
{
    bool IsConnected { get; }
    event Action<RobotPoseMessage> PoseReceived;
    event Action<RobotConnectionState> ConnectionStateChanged;

    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync();
}
```

The concrete network transport will implement this interface later.

Possible implementations include:

- `UdpRobotPoseSource`
- `WebSocketRobotPoseSource`
- `HttpPollingRobotPoseSource`
- `SimulatedRobotPoseSource`
- `RecordedRobotPoseSource`

### 8.2 Main Data Types

Recommended application types:

```text
RobotPoseMessage
GpsAnchorData
RobotTrackingState
RobotConnectionState
RobotPoseValidationResult
CalibrationData
```

### 8.3 Threading

Network callbacks may occur outside Unity's main thread.

The receiver must not modify GameObjects or Unity transforms from a background thread. Received messages should be placed in a thread-safe queue or copied into a synchronized latest-pose buffer. Unity scene updates must occur on the main thread.

### 8.4 Update Rate

The application should support continuously moving pose data.

Initial target capabilities:

- Accept at least 10 pose updates per second.
- Render at the headset's normal frame rate independently of network update frequency.
- Avoid creating garbage every frame in the pose-update path.
- Track the age of the most recently accepted pose.

The final expected publishing rate must be agreed upon with the robot team.

## 9. Pose Processing Pipeline

The application should process each update in the following order:

1. Receive serialized data.
2. Deserialize into `RobotPoseMessage`.
3. Validate schema, values, timestamp, sequence, and tracking state.
4. Store the most recent valid target pose.
5. Convert the target from robot-local coordinates to Unity world coordinates using the calibrated site frame.
6. Apply optional interpolation or smoothing.
7. Update the robot visualization on Unity's main thread.
8. Update status and diagnostic displays.

## 10. Visualization Requirements

### 10.1 Robot Model

The app must display a 3D robot prefab at the transformed pose.

Until the final model is supplied, use a clearly directional placeholder model containing:

- A body
- A visible forward indicator
- Distinct top and bottom surfaces
- A transform origin documented relative to the model

The prefab should use Unity scale where one unit equals one meter.

The model pivot should represent the same physical point used by the robot localization system. Any required model offset must be configured explicitly rather than embedded in pose-processing code.

### 10.2 Underground Visibility

The robot may be physically hidden by the ground but must remain visible in AR.

The first version should support an **X-ray visualization mode** that renders the robot through real-world surfaces. The robot material should be visually distinct from normal scene geometry and may use transparency, an outline, or both.

Because passthrough depth and real-world occlusion availability can vary, the core requirement is understandable location—not physically perfect soil occlusion.

### 10.3 Supporting Visuals

The visualization should include configurable aids:

- Vertical line from the surface/origin level to the robot
- Direction arrow showing robot forward
- Text label containing robot ID
- Depth below the calibrated origin plane
- Horizontal distance from the site origin
- Straight-line distance from the user
- Position accuracy radius or uncertainty indicator when provided
- Breadcrumb trail of recent robot positions

Each aid should be independently enabled or disabled.

### 10.4 Pose Smoothing

Pose messages may arrive less frequently than rendered frames or may contain small jumps.

The visualizer should interpolate toward the latest valid target pose. Smoothing must be configurable and must not disguise prolonged data loss.

Recommended initial behavior:

- Position: linear interpolation or exponential smoothing
- Orientation: quaternion spherical interpolation
- Snap immediately when the difference exceeds a configurable teleport threshold
- Stop interpolation toward new targets when data becomes invalid

The raw received pose must remain available in diagnostics so smoothing does not conceal source behavior during testing.

### 10.5 Stale Data Behavior

Recommended initial thresholds:

- Fresh: pose age less than `0.5 seconds`
- Delayed: `0.5–2 seconds`
- Stale: greater than `2 seconds`
- Lost: greater than `5 seconds` or explicit `Lost` state

Thresholds must be configurable.

When data is stale or lost:

- Retain the last known position unless the user disables this behavior.
- Change the robot's appearance to indicate that it is not current.
- Show the age of the last pose.
- Do not extrapolate indefinitely.

## 11. User Interface

The user interface should provide the following information and controls.

### 11.1 Status Panel

Display:

- Robot connection state
- Selected robot ID
- Calibration state
- Robot tracking state
- Last pose timestamp
- Pose age
- Current local position
- Current local orientation
- Current depth
- Position and orientation accuracy, when supplied

### 11.2 Controls

Provide:

- Connect/disconnect
- Select robot, if multiple robot IDs are later supported
- Begin/reset calibration
- Show/hide calibration axes
- Show/hide robot
- Toggle X-ray mode
- Toggle trail
- Toggle distance and depth labels
- Toggle smoothing
- Select live, simulated, or recorded data source in development builds

### 11.3 Error Feedback

Errors must be understandable to a field user. Examples:

- Not calibrated
- Waiting for robot data
- Connection lost
- Robot tracking degraded
- Pose rejected: invalid quaternion
- Pose rejected: outside configured site bounds
- Last update 8.2 seconds ago

## 12. Proposed Unity Architecture

Create application-specific code outside the template folders.

Recommended folder structure:

```text
Assets/
  UVP/
    Art/
      Materials/
      Models/
      Prefabs/
    Scenes/
    Scripts/
      Calibration/
      Configuration/
      Data/
      Diagnostics/
      Networking/
      PoseProcessing/
      UI/
      Visualization/
    Tests/
      EditMode/
      PlayMode/
```

Recommended main components:

### `RobotPoseCoordinator`

Coordinates the selected pose source, validator, calibration provider, and visualizer.

### `IRobotPoseSource`

Abstracts UDP, WebSocket, HTTP, simulation, or recorded playback.

### `RobotPoseValidator`

Checks incoming messages before they affect the scene.

### `ManualSiteCalibrationController`

Runs the two-step origin and forward-direction calibration process.

### `ICoordinateFrameProvider`

Provides the currently valid robot-local-to-Unity transformation.

```csharp
public interface ICoordinateFrameProvider
{
    bool IsCalibrated { get; }
    Transform SiteOrigin { get; }
    bool TryTransformPose(Pose robotLocalPose, out Pose unityWorldPose);
}
```

### `RobotPoseVisualizer`

Updates the robot prefab, interpolation, trail, labels, uncertainty graphics, and stale-state appearance.

### `RobotStatusPanelController`

Displays connection, calibration, and pose status.

### `RobotVisualizationSettings`

A `ScriptableObject` containing configurable thresholds, smoothing values, bounds, model offsets, and display options.

### `SimulatedRobotPoseSource`

Generates predictable paths for development without robot hardware.

## 13. Simulation and Development Mode

The visualization team must not depend on live robot hardware for normal development.

The simulated source should support:

- Fixed pose
- Straight-line motion
- Circular path
- Descent followed by horizontal motion
- Configurable update rate
- Configurable noise
- Tracking-state changes
- Pauses and dropped updates
- Invalid-message test cases

A recorded playback source should later allow timestamped pose messages to be replayed at original speed or a selected speed.

## 14. Testing Requirements

### 14.1 Edit Mode Tests

Tests should verify:

- JSON deserialization
- Required-field validation
- Sequence-number handling
- Timestamp and stale-data classification
- Quaternion normalization and rejection
- Coordinate transformation using known origins and rotations
- Depth and distance calculations

### 14.2 Play Mode Tests

Tests should verify:

- Robot prefab placement after calibration
- Correct application of position and orientation
- Smooth movement between pose updates
- Snap behavior for large pose changes
- Stale/lost visual states
- Calibration reset behavior
- Switching between simulated and live pose sources

### 14.3 Field Tests

Before underground use, test above ground using measured locations.

Recommended procedure:

1. Mark a physical origin.
2. Mark the physical `+Z` direction.
3. Complete manual calibration.
4. Publish several known local Cartesian poses.
5. Compare the rendered object with measured target points.
6. Record position and heading error at multiple distances.
7. Repeat after restarting the headset and application.

## 15. Performance Requirements

The visualization must:

- Maintain a comfortable XR frame rate on Meta Quest 3.
- Avoid blocking network operations on Unity's main thread.
- Avoid per-frame JSON parsing.
- Avoid unbounded breadcrumb-trail growth.
- Allow diagnostic displays to be disabled in operational builds.
- Continue rendering the last valid state when network updates temporarily stop.

## 16. Safety and Interpretation

The displayed robot position is an estimate based on robot localization data and manual AR calibration.

The app must display a persistent notice similar to:

> Estimated robot location. Do not use as the sole basis for excavation or personnel safety decisions.

The interface must make uncertainty and stale data visible. It must not imply that the robot is precisely located when the source data or calibration does not support that conclusion.

## 17. Acceptance Criteria

The first visualization milestone is complete when:

1. The app runs on Meta Quest 3 using the current Unity project.
2. A user can manually set the robot coordinate origin and `+Z` direction.
3. The app can receive pose updates through an `IRobotPoseSource` implementation.
4. A simulated pose source continuously moves a robot through a repeatable underground path.
5. Robot-local Unity-style position and quaternion orientation are transformed correctly into the AR scene.
6. The virtual robot clearly shows its position and forward orientation through the ground.
7. The status panel reports connection, calibration, tracking state, and pose age.
8. Delayed, stale, lost, and invalid data produce visible warnings.
9. The live transport can be added later without modifying calibration or visualization logic.
10. Coordinate-transform and pose-validation tests pass.

## 18. Open Decisions

The following items require agreement with the robot team or project stakeholders:

1. Network transport: UDP, WebSocket, HTTP, or another protocol.
2. Expected pose update rate and maximum acceptable latency.
3. Whether GPS anchor data is sent once or included in each pose message.
4. The exact physical point on the robot represented by its reported position.
5. The physical heading used as robot-local `+Z` at initialization.
6. Expected position and orientation accuracy.
7. Maximum operating range and site bounds.
8. Whether multiple robots must be supported.
9. Whether calibration must persist between sessions.
10. Whether the system must use spatial anchors or shared anchors later.
11. Whether an underground tunnel, pipe, or terrain model will be supplied.
12. Final visual design and model format for the robot.

## 19. Recommended Initial Implementation Sequence

1. Create the `Assets/UVP` folder structure.
2. Define pose, anchor, calibration, and status data classes.
3. Implement `IRobotPoseSource` and `SimulatedRobotPoseSource`.
4. Implement pose validation.
5. Implement manual two-point calibration.
6. Implement coordinate transformation tests.
7. Create a directional placeholder robot prefab.
8. Implement robot pose visualization and smoothing.
9. Add status and calibration UI.
10. Add stale-data and error states.
11. Test measured points above ground.
12. Implement the selected live network transport when its contract is approved.

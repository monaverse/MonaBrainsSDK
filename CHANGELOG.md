# MonaBrainsSDK 0.12.0

**Full Changelog**: https://github.com/monaverse/MonaBrainsSDK/compare/0.11.0...0.12.0

# MonaBrainsSDK 0.11.0

**Full Changelog**: https://github.com/monaverse/MonaBrainsSDK/compare/0.10.0...0.11.0

# MonaBrainsSDK 0.10.0

**Full Changelog**: https://github.com/monaverse/MonaBrainsSDK/compare/0.9.0...0.10.0

# MonaBrainsSDK 0.9.0

**Full Changelog**: https://github.com/monaverse/MonaBrainsSDK/compare/0.8.0...0.9.0

# MonaBrainsSDK 0.8.0

**Full Changelog**: https://github.com/monaverse/MonaBrainsSDK/compare/0.7.0...0.8.0

# MonaBrainsSDK 0.7.0

**Full Changelog**: https://github.com/monaverse/MonaBrainsSDK/compare/0.6.0...0.7.0

# MonaBrainsSDK 0.6.0

**Full Changelog**: https://github.com/monaverse/MonaBrainsSDK/compare/0.5.0...0.6.0

# MonaBrainsSDK 0.5.0

### Bug Fixes
- Fixed Input lag
- fixed Rigidbody movement behaving abnormally. Moved movement into fixedupdate context.
- removed samples

### Improvments
- Move to Position Tile (allows objects to move over time to set global coordinates)
- Added MovementPlaneType to Camera Relative movement tiles that allow for single plane based movement
- Added Tiles to bound the extends of position of objects to a given range (eg. This ship can only move between -5, and 5 on the east/west axis)
- More meaningful and concise labels on tiles and fields

# MonaBrainsSDK 0.4.0

### Bug Fixes
- move local on grounded creature now triggers animation
- null check for monatag
- reset input on tile after processing 
- stubbing out DialogStyle  

# MonaBrainsSDK 0.3.0

40+ New Tiles for controlling Visuals, Pausing Brains, and basic Physics forces

### Bug Fixes
- Fixed When user clicks on instruction area, instruction is selected and current tile is deselected.
- Fixed `ChangeState` tile no longer allows tiles after itself in an instruction for State Pages in a `MonaBrainGraph`
- Fixed Ensure that `Unload()` properly unloads brains before hotswap reloading.
- Fixed Disposed element error in `MonaBrainRunnerEditor`
- Fixed issue when only one tile on `Instruction` was causing issues with brain execution because `Instruction` result state was never switched to running.
- Fixed default categories and naming for all tiles
- Fixed Null check if tileset is missing tile reference

### Improvements
- Allow Attaching `MonaBrainGraph` to LocalPlayer object

** GENERAL **
- Added `AddTag(string tag)` instruction tile
- Added `RemoveTag(string tag)` instruction tile
- Added `EnableByTag(string tag)` instruction tile
- Added `DisableByTag(string tag)` instruction tile
- Added `DisableTarget(string target)` instruction tile
- Added `EnableTarget(string target)` instruction tile
- Added `EnablePart(string part)` instruction tile
- Added `DisablePart(string part)` instruction tile

** PAUSING ** 
- Added `PauseBodyByTag(string tag)` instruction tile
- Added `ResumeBodyByTag(string tag)` instruction tile
- Added `PauseTarget(string target)` instruction tile
- Added `ResumeTarget(string target)` instruction tile
- Added `PauseSelf()` instruction tile

** VISUALS ** 
- Added `ShowByTag(string tag)` instruction tile
- Added `HideByTag(string tag)` instruction tile
- Added `Show()` instruction tile
- Added `Hide()` instruction tile
- Added `ShowByTag(string tag)` instruction tile
- Added `HideByTag(string tag)` instruction tile
- Added `ShowTarget(string target)` instruction tile
- Added `HideTarget(string target)` instruction tile
- Added `ShowPart(string part)` instruction tile
- Added `HidePart(string part)` instruction tile

** PHYSICS **
- Added `AttachToTag(string tag, Vector3 offset, Vector3 scale)` instruction tile
- Added `AttachToTagPart(string tag, string part, Vector3 offset, Vector3 scale)` instruction tile
- Added `AttachToTarget(string target, Vector3 offset, Vector3 scale)` instruction tile
- Added `AttachToTargetPart(string target, string part, Vector3 offset, Vector3 scale)` instruction tile
- Added `AttachToPlayerPart(string part, Vector3 offset, Vector3 scale)` instruction tile
- Added `Deattach()` instruction tile
- Added `ApplyForceForward(float duration, DragType dragType, float drag, float angularDrag, float friction, float bounce)` instruction tile
- Added `ApplyForceBackward(float duration, DragType dragType, float drag, float angularDrag, float friction, float bounce)` instruction tile
- Added `ApplyForceRight(float duration, DragType dragType, float drag, float angularDrag, float friction, float bounce)` instruction tile
- Added `ApplyForceLeft(float duration, DragType dragType, float drag, float angularDrag, float friction, float bounce)` instruction tile
- Added `ApplyForceUp(float duration, DragType dragType, float drag, float angularDrag, float friction, float bounce)` instruction tile
- Added `ApplyForceDown(float duration, DragType dragType, float drag, float angularDrag, float friction, float bounce)` instruction tile
- Added `ApplyForcePush(float duration, DragType dragType, float drag, float angularDrag, float friction, float bounce)` instruction tile
- Added `ApplyForcePull(float duration, DragType dragType, float drag, float angularDrag, float friction, float bounce)` instruction tile
- Added `ApplyForceAway(float duration, DragType dragType, float drag, float angularDrag, float friction, float bounce)` instruction tile
- Added `ApplyForceToward(float duration, DragType dragType, float drag, float angularDrag, float friction, float bounce)` instruction tile
- Added `ApplyForceAlongMoveInput(float duration, DragType dragType, float drag, float angularDrag, float friction, float bounce)` instruction tile


---

# MonaBrainsSDK 0.2.0

### Improvements
 - UI/U Improvements to Brain Editing in Unity which make inserting/removing/moving/updating tiles much more efficient.
 - Instructions no longer need conditionals to execute.

# MonaBrainsSDK 0.1.0

This first release of Mona Brains SDK establishes a foundation for defining and running Mona Brains in the Monaverse.

### Mona Brains Definition
- Core Page
- State Pages
- Default Brain Values
- Mona Tags

### Mona State
- Set Number|String|Bool|Vector2|Vector3|MonaBody|MonaBrain properties on a MonaBrain
- Number|String|Bool|Vector2|Vector3 properties are synced across the network

### Mona Runner
- Run one or more brains on a MonaBody

### Mona Brain Instruction Tiles

**Conditions**
- OnStart
- OnMessage
- OnInput
- OnKey
- OnInteract
- OnSelect
- OnSelectTag
- OnValue
- OnValueChanged
- OnValueEven
- OnValueOdd
- OnNear
- OnFar
- OnCanSee
- OnCanNotSee

**Actions**
- BroadcastMessageToSelf
- BroadcastMessageToSender
- BroadcastMessageToTag
- BroadcastMessageToTarget
- VisualScripting
- ChangeState
- ChangeColor
- ChangeValue
- CopyResult
- MoveFoward
- MoveBackward
- MoveUp
- MoveDown
- MoveRight
- MoveLeft
- SpinRight
- SpinLeft
- SpinUp
- SpinDown
- RollLeft
- RollRight
- Wait
- Log

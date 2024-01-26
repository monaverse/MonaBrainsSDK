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
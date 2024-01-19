using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Unity.VisualScripting;

[UnitTitle("TriggerMonaBrainsThen")]
[UnitSubtitle("Events")]
[UnitCategory("Events\\Mona Brain")]
[TypeIcon(typeof(TriggerEventUnit))]
public class TriggerMonaBrainsThen : Unit
{
    [DoNotSerialize]
    public ControlInput Then { get; private set; }

    [DoNotSerialize]
    public ValueInput Brain { get; private set; }

    [DoNotSerialize]
    public ValueInput EventName { get; private set; }

    [DoNotSerialize]
    public ValueInput Result { get; private set; }

    protected override void Definition()
    {
        Brain = ValueInput<IMonaBrain>(nameof(Brain));
        EventName = ValueInput<string>(nameof(EventName), "Default");
        Result = ValueInput<InstructionTileResult>(nameof(Result), InstructionTileResult.Success);
        Then = ControlInput(nameof(Then), Enter);
        Requirement(Brain, Then);
    }

    private ControlOutput Enter(Flow flow)
    {
        var brain = flow.GetValue<IMonaBrain>(Brain);
        var eventName = flow.GetValue<string>(EventName);
        var result = flow.GetValue<InstructionTileResult>(Result);
        EventBus.Trigger(new EventHook(MonaBrainConstants.MONA_BRAINS_THEN_EVENT, brain), new MonaBrainsThenEvent(eventName, result));
        return null;
    }


}

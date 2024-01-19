using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Core.Body;
using Unity.VisualScripting;
using UnityEngine;

namespace Mona.SDK.Brains.VisualScripting.Units.Events
{
    [UnitTitle("OnMonaBrainsDo")]
    [UnitSubtitle("Events")]
    [UnitCategory("Events\\Mona Brain")]
    [TypeIcon(typeof(CustomEvent))]
    public sealed class OnMonaBrainsDo : EventUnit<MonaBrainsDoEvent>
    {
        protected override bool register => true;

        [DoNotSerialize]
        public ValueInput EventName;

        [DoNotSerialize]
        public ValueOutput Brain;

        [DoNotSerialize]
        public ValueOutput Body;

        [DoNotSerialize]
        public ValueOutput Values;

        [DoNotSerialize]
        public ValueOutput GameObject;

        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(MonaBrainConstants.MONA_BRAINS_DO_EVENT);
        }

        protected override void Definition()
        {
            base.Definition();
            EventName = ValueInput<string>(nameof(EventName), "Default");
            Brain = ValueOutput<IMonaBrain>(nameof(Brain));
            Body = ValueOutput<MonaBody>(nameof(Body));
            Values = ValueOutput<MonaBrainState>(nameof(Values));
            GameObject = ValueOutput<GameObject>(nameof(GameObject));
        }

        protected override bool ShouldTrigger(Flow flow, MonaBrainsDoEvent args)
        {
            return flow.GetValue<string>(EventName) == args.EventName;
        }

        protected override void AssignArguments(Flow flow, MonaBrainsDoEvent args)
        {
            flow.SetValue(Brain, args.Brain);
            flow.SetValue(Body, args.Brain.Body);
            flow.SetValue(Values, args.Brain.State);
            flow.SetValue(GameObject, args.Brain.GameObject);
            base.AssignArguments(flow, args);
        }

    }
}
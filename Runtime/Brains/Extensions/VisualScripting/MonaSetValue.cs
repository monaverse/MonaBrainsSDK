#if MONA_SERVICES
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Utils;
using System.Threading.Tasks;
using UnityEngine.SocialPlatforms;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Brains.Core.Utils.Structs;
using System.Collections.Generic;

namespace Mona.VisualScripting
{
    //Custom node to send the Event
    [UnitTitle("MonaSetValue")]
    [UnitCategory("Mona\\CloudSave")]//Setting the path to find the node in the fuzzy finder as Events > My Events.
    public class MonaSetValue : Unit
    {
        /// <summary>
        /// The moment at which to start the delay.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        [DoNotSerialize]
        public ValueInput Key;

        [DoNotSerialize]
        public ValueInput ValueType;

        [DoNotSerialize]
        public ValueInput BoolValue;

        [DoNotSerialize]
        public ValueInput IntValue;

        [DoNotSerialize]
        public ValueInput LongValue;

        [DoNotSerialize]
        public ValueInput FloatValue;

        [DoNotSerialize]
        public ValueInput DoubleValue;

        [DoNotSerialize]
        public ValueInput StringValue;

        [DoNotSerialize]
        public ValueInput Vector2Value;

        [DoNotSerialize]
        public ValueInput Vector3Value;

        [DoNotSerialize]
        public ValueInput LayoutValue;

        /// <summary>
        /// The action to execute after the delay has elapsed.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInputCoroutine(nameof(enter), Await);

            Key = ValueInput<string>(nameof(Key), "");
            ValueType = ValueInput<MonaGetValueType>(nameof(ValueType), MonaGetValueType.Bool);

            BoolValue = ValueInput<bool>(nameof(BoolValue), false);
            IntValue = ValueInput<int>(nameof(IntValue), 0);
            LongValue = ValueInput<long>(nameof(LongValue), 0);
            FloatValue = ValueInput<float>(nameof(FloatValue), 0f);
            DoubleValue = ValueInput<double>(nameof(DoubleValue), 0);
            StringValue = ValueInput<object>(nameof(StringValue), null);
            Vector2Value = ValueInput<Vector2>(nameof(Vector2Value), Vector2.zero);
            Vector3Value = ValueInput<Vector3>(nameof(Vector3Value), Vector3.zero);
            LayoutValue = ValueInput<LayoutStorageData>(nameof(LayoutValue), new LayoutStorageData());

            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }

        protected IEnumerator Await(Flow flow)
        {
            BrainProcess state = new BrainProcess();
            state.StartProcess();

            while (MonaGlobalBrainRunner.Instance.BrainLeaderboards == null)
                yield return new WaitForSeconds(1f);

            SetValue(flow, state);

            while (state.IsProcessing)
                yield return new WaitForSeconds(.1f);

            yield return exit;
        }

        protected async Task SetValue(Flow flow, BrainProcess state)
        {
            var keyValue = flow.GetValue<string>(Key);
            var valueType = flow.GetValue<MonaGetValueType>(ValueType);

            BrainProcess result;

            if (valueType is MonaGetValueType.Bool)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.SetBool(keyValue, flow.GetValue<bool>(BoolValue));
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueType is MonaGetValueType.Int)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.SetInt(keyValue, flow.GetValue<int>(IntValue));
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueType is MonaGetValueType.Long)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.SetLong(keyValue, flow.GetValue<long>(LongValue));
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueType is MonaGetValueType.Float)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.SetFloat(keyValue, flow.GetValue<float>(FloatValue));
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueType is MonaGetValueType.Double)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.SetDouble(keyValue, flow.GetValue<double>(DoubleValue));
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueType is MonaGetValueType.Vector2)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.SetVector2(keyValue, flow.GetValue<Vector2>(Vector2Value));
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueType is MonaGetValueType.Vector3)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.SetVector3(keyValue, flow.GetValue<Vector3>(Vector3Value));
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueType is MonaGetValueType.Layout)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.SetLayout(flow.GetValue<LayoutStorageData>(LayoutValue));
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueType is MonaGetValueType.String)
            {
                var value = flow.GetValue<object>(StringValue);
                if (value is string)
                    result = await MonaGlobalBrainRunner.Instance.CloudStorage.SetString(keyValue, (string)value);
                else
                    result = await MonaGlobalBrainRunner.Instance.CloudStorage.SetString(keyValue, JsonUtility.ToJson(value));
                state.EndProcess(result.WasSuccessful);
            }
            else
                state.EndProcess(false);
        }

    }
}
#endif
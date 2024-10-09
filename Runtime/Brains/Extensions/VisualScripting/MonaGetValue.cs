#if MONA_CLOUDSTORAGE
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
using Mona.SDK.Core.Body;
using System;

namespace Mona.VisualScripting
{
    public enum MonaGetValueType
    {
        Bool = 0,
        Int,
        Long,
        Float,
        Double,
        String,
        Vector2,
        Vector3,
        Layout
    }
    //Custom node to send the Event
    [UnitTitle("MonaGetValue")]
    [UnitCategory("Mona\\CloudSave")]//Setting the path to find the node in the fuzzy finder as Events > My Events.
    public class MonaGetValue : Unit
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
        public ValueOutput BoolValue;

        [DoNotSerialize]
        public ValueOutput IntValue;

        [DoNotSerialize]
        public ValueOutput LongValue;

        [DoNotSerialize]
        public ValueOutput FloatValue;

        [DoNotSerialize]
        public ValueOutput DoubleValue;

        [DoNotSerialize]
        public ValueOutput StringValue;

        [DoNotSerialize]
        public ValueOutput Vector2Value;

        [DoNotSerialize]
        public ValueOutput Vector3Value;

        [DoNotSerialize]
        public ValueOutput LayoutValue;

        [DoNotSerialize]
        public ValueInput monaBody;

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
            monaBody = ValueInput<MonaBody>(nameof(monaBody), null);

            BoolValue = ValueOutput<bool>(nameof(BoolValue));
            IntValue = ValueOutput<int>(nameof(IntValue));
            LongValue = ValueOutput<long>(nameof(LongValue));
            FloatValue = ValueOutput<float>(nameof(FloatValue));
            DoubleValue = ValueOutput<double>(nameof(DoubleValue));
            StringValue = ValueOutput<object>(nameof(StringValue));
            Vector2Value = ValueOutput<Vector2>(nameof(Vector2Value));
            Vector3Value = ValueOutput<Vector3>(nameof(Vector3Value));
            LayoutValue = ValueOutput<LayoutStorageData>(nameof(LayoutValue));

            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }

        protected IEnumerator Await(Flow flow)
        {
            BrainProcess state = new BrainProcess();
            state.StartProcess();

            while (MonaGlobalBrainRunner.Instance.BrainLeaderboards == null)
                yield return new WaitForSeconds(1f);

            GetValue(flow, state);

            while (state.IsProcessing)
                yield return new WaitForSeconds(.1f);

            yield return exit;
        }

        protected async Task GetValue(Flow flow, BrainProcess state)
        {
            var keyValue = flow.GetValue<string>(Key);
            var valueTypeValue = flow.GetValue<MonaGetValueType>(ValueType);

            BrainProcess result;

            if (valueTypeValue == MonaGetValueType.Bool)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.LoadBool(keyValue);
                flow.SetValue(BoolValue, result.GetBool());
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaGetValueType.Int)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.LoadInt(keyValue);
                flow.SetValue(IntValue, result.GetInt());
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaGetValueType.Long)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.LoadLong(keyValue);
                flow.SetValue(LongValue, result.GetLong());
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaGetValueType.Float)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.LoadFloat(keyValue);
                flow.SetValue(FloatValue, result.GetFloat());
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaGetValueType.Double)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.LoadDouble(keyValue);
                flow.SetValue(DoubleValue, result.GetDouble());
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaGetValueType.Vector2)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.LoadVector2(keyValue);
                flow.SetValue(Vector2Value, result.GetVector2());
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaGetValueType.Vector3)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.LoadVector3(keyValue);
                flow.SetValue(Vector3Value, result.GetVector3());
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaGetValueType.Layout)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.LoadLayout(keyValue, (IMonaBody)flow.GetValue<MonaBody>(monaBody));
                flow.SetValue(LayoutValue, result.GetLayout());
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaGetValueType.String)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.LoadString(keyValue);
                flow.SetValue(StringValue, result.GetString());
                state.EndProcess(result.WasSuccessful);
            }
            else
                state.EndProcess(false);
        }
    }
}
#endif
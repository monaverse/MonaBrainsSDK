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
using Mona.SDK.Core.Body;
using System;

namespace Mona.VisualScripting
{
    public enum MonaDeleteValueType
    {
        Bool = 0,
        Int,
        Long,
        Float,
        Double,
        String,
        Vector2,
        Vector3
    }
    //Custom node to send the Event
    [UnitTitle("MonaDeleteValue")]
    [UnitCategory("Mona\\CloudSave")]//Setting the path to find the node in the fuzzy finder as Events > My Events.
    public class MonaDeleteValue : Unit
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
            ValueType = ValueInput<MonaDeleteValueType>(nameof(ValueType), MonaDeleteValueType.Bool);

            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }

        protected IEnumerator Await(Flow flow)
        {
            BrainProcess state = new BrainProcess();
            state.StartProcess();

            while (MonaGlobalBrainRunner.Instance.BrainLeaderboards == null)
                yield return new WaitForSeconds(1f);

            DeleteValue(flow, state);

            while (state.IsProcessing)
                yield return new WaitForSeconds(.1f);

            yield return exit;
        }

        protected async Task DeleteValue(Flow flow, BrainProcess state)
        {
            var keyValue = flow.GetValue<string>(Key);
            var valueTypeValue = flow.GetValue<MonaDeleteValueType>(ValueType);

            BrainProcess result;

            if (valueTypeValue == MonaDeleteValueType.Bool)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.DeleteBool(keyValue);
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaDeleteValueType.Int)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.DeleteInt(keyValue);
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaDeleteValueType.Long)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.DeleteLong(keyValue);
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaDeleteValueType.Float)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.DeleteFloat(keyValue);
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaDeleteValueType.Double)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.DeleteDouble(keyValue);
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaDeleteValueType.Vector2)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.DeleteVector2(keyValue);
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaDeleteValueType.Vector3)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.DeleteVector3(keyValue);
                state.EndProcess(result.WasSuccessful);
            }
            else if (valueTypeValue == MonaDeleteValueType.String)
            {
                result = await MonaGlobalBrainRunner.Instance.CloudStorage.DeleteString(keyValue);
                state.EndProcess(result.WasSuccessful);
            }
            else
                state.EndProcess(false);
        }
    }
}
#endif
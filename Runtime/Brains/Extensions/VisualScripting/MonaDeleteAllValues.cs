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
    //Custom node to send the Event
    [UnitTitle("MonaDeleteAllValues")]
    [UnitCategory("Mona\\CloudSave")]//Setting the path to find the node in the fuzzy finder as Events > My Events.
    public class MonaDeleteAllValues : Unit
    {
        /// <summary>
        /// The moment at which to start the delay.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The action to execute after the delay has elapsed.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInputCoroutine(nameof(enter), Await);

            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }

        protected IEnumerator Await(Flow flow)
        {
            BrainProcess state = new BrainProcess();
            state.StartProcess();

            while (MonaGlobalBrainRunner.Instance.BrainLeaderboards == null)
                yield return new WaitForSeconds(1f);

            DeleteAllValues(flow, state);

            while (state.IsProcessing)
                yield return new WaitForSeconds(.1f);

            yield return exit;
        }

        protected async Task DeleteAllValues(Flow flow, BrainProcess state)
        {
            var result = await MonaGlobalBrainRunner.Instance.CloudStorage.DeleteAllData();
            state.EndProcess(result.WasSuccessful);           
        }
    }
}
#endif
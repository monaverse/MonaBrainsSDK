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

public enum MonaLoadUserScoreSortingOrder
{
    Lowest = 0,
    Highest = 1
}

//Custom node to send the Event
[UnitTitle("MonaLoadUserScore")]
[UnitCategory("Mona\\Leaderboards")]//Setting the path to find the node in the fuzzy finder as Events > My Events.
public class MonaLoadUserScore : Unit
{
    /// <summary>
    /// The moment at which to start the delay.
    /// </summary>
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput enter { get; private set; }

    [DoNotSerialize]
    public ValueInput topic;

    [DoNotSerialize]
    public ValueInput count;

    [DoNotSerialize]
    public ValueInput order;

    [DoNotSerialize]
    public ValueOutput userScore;

    /// <summary>
    /// The action to execute after the delay has elapsed.
    /// </summary>
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput exit { get; private set; }

    protected override void Definition()
    {
        enter = ControlInputCoroutine(nameof(enter), Await);

        topic = ValueInput<string>(nameof(topic), "");
        count = ValueInput<int>(nameof(count), 10);
        order = ValueInput<MonaLoadUserScoreSortingOrder>(nameof(order), MonaLoadUserScoreSortingOrder.Lowest);

        userScore = ValueOutput<LeaderboardScore>(nameof(userScore));
        
        exit = ControlOutput(nameof(exit));
        Succession(enter, exit);
    }

    protected IEnumerator Await(Flow flow)
    {
        BrainProcess state = new BrainProcess();
        state.StartProcess();

        while (MonaGlobalBrainRunner.Instance.BrainLeaderboards == null)
            yield return new WaitForSeconds(1f);

        LoadScores(flow, state);

        while (state.IsProcessing)
            yield return new WaitForSeconds(.1f);

        yield return exit;
    }

    protected async Task LoadScores(Flow flow, BrainProcess state)
    {
        //LoadScores(string id, TimeScope timeScope, Range range, LeaderboardOrderType order = LeaderboardOrderType.Default)

        var topicValue = flow.GetValue<string>(topic);
        var countValue = flow.GetValue<int>(count);
        var orderValue = flow.GetValue<MonaLoadUserScoreSortingOrder>(order);

        var result = await MonaGlobalBrainRunner.Instance.BrainLeaderboards.LoadClientScore(topicValue, countValue, orderValue.ToString());

        flow.SetValue(userScore, result.GetUserScore());

        state.EndProcess(result.WasSuccessful);
    }

}

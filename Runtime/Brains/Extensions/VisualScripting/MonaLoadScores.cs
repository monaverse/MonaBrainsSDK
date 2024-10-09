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

//Custom node to send the Event
[UnitTitle("MonaLoadScores")]
[UnitCategory("Mona\\Leaderboards")]//Setting the path to find the node in the fuzzy finder as Events > My Events.
public class MonaLoadScores : Unit
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
    public ValueInput timeScope;

    [DoNotSerialize]
    public ValueInput count;

    [DoNotSerialize]
    public ValueInput from;

    [DoNotSerialize]
    public ValueInput order;

    [DoNotSerialize]
    public ValueOutput userScore;

    [DoNotSerialize]
    public ValueOutput scores;

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
        timeScope = ValueInput<TimeScope>(nameof(timeScope), TimeScope.AllTime);
        count = ValueInput<int>(nameof(count), 10);
        from = ValueInput<int>(nameof(from), 0);
        order = ValueInput<LeaderboardOrderType>(nameof(order), LeaderboardOrderType.Default);

        scores = ValueOutput<List<LeaderboardScore>>(nameof(scores));

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
        var timeScopeValue = flow.GetValue<TimeScope>(timeScope);
        var rangeValue = new Range() { count = flow.GetValue<int>(count), from = flow.GetValue<int>(from) };
        var orderValue = flow.GetValue<LeaderboardOrderType>(order);

        var result = await MonaGlobalBrainRunner.Instance.BrainLeaderboards.LoadScores(topicValue, timeScopeValue, rangeValue, orderValue);

        flow.SetValue(scores, result.GetScores());

        state.EndProcess(result.WasSuccessful);
    }

}

using System.Collections;
using UnityEngine;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Utils;
using System.Threading.Tasks;
using Mona.SDK.Brains.Core.Utils.Interfaces;

//Custom node to send the Event
[UnitTitle("MonaLogin")]
[UnitCategory("Mona\\MonaLogin")]//Setting the path to find the node in the fuzzy finder as Events > My Events.
public class MonaLogin : Unit
{

    /// <summary>
    /// The moment at which to start the delay.
    /// </summary>
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput enter { get; private set; }

    [DoNotSerialize]
    public ValueInput environment;

    [DoNotSerialize]
    public ValueInput applicationId;

    [DoNotSerialize]
    public ValueInput secret;

    [DoNotSerialize]
    public ValueOutput username;


    /// <summary>
    /// The action to execute after the delay has elapsed.
    /// </summary>
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput exit { get; private set; }

    protected override void Definition()
    {
        enter = ControlInputCoroutine(nameof(enter), Await);
        environment = ValueInput<string>(nameof(environment), "");
        applicationId = ValueInput<string>(nameof(applicationId), "");
        secret = ValueInput<string>(nameof(secret), "");

        username = ValueOutput<string>(nameof(username));

        exit = ControlOutput(nameof(exit));
        Succession(enter, exit);
    }

    protected IEnumerator Await(Flow flow)
    {
        BrainProcess state = new BrainProcess();
        state.StartProcess();

        while (MonaGlobalBrainRunner.Instance.BrainLeaderboards == null)
            yield return new WaitForSeconds(1f);

        Login(flow, state);

        while (state.IsProcessing)
            yield return new WaitForSeconds(.1f);

        yield return exit;
    }

    protected async Task Login(Flow flow, BrainProcess state)
    {
        if (MonaGlobalBrainRunner.Instance.BrainLeaderboards is IMonaLeaderboardAsync)
        {
            var environmentValue = flow.GetValue<string>(environment);
            var applicationIdValue = flow.GetValue<string>(applicationId);
            var secretValue = flow.GetValue<string>(secret);
            var result = await ((IMonaLeaderboardAsync)MonaGlobalBrainRunner.Instance.BrainLeaderboards).AutoLogin(environmentValue, applicationIdValue, secretValue);

            if(result.WasSuccessful)
            {
                var usernameValue = result.GetString();
                flow.SetValue(username, usernameValue);
            }

            state.EndProcess(result.WasSuccessful);
        }
        else
        {
            var result = await MonaGlobalBrainRunner.Instance.BrainLeaderboards.AutoLogin();

            state.EndProcess(result.WasSuccessful);
        }
    }

}

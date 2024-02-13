using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Assets.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class MonaDefaultAnimationController : MonoBehaviour
{
    private Animator _animator;
    private AnimatorController _controller;
    private AnimatorState _start;
    private AnimatorState _end;

    private const string START_STATE = "__Start";
    private const string END_STATE = "_End";

    private GameObject _root;

    public void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
        if (_animator == null)
            _animator = gameObject.AddComponent<Animator>();

        SetupAnimationController();
    }

    private void SetupAnimationController()
    {
        if (_animator.runtimeAnimatorController == null)
        {
            var controller = new AnimatorController();
                controller.AddLayer("Base");

            var layer = controller.layers[0];

            _start = layer.stateMachine.AddState(START_STATE);
            _end = layer.stateMachine.AddState(END_STATE);

            layer.stateMachine.defaultState = _start;

            _animator.runtimeAnimatorController = controller;
        }
        _controller = (AnimatorController)_animator.runtimeAnimatorController;
    }

    public void AddClip(IMonaAnimationAssetItem clipItem, float speed = 1f)
    {
        var layer = _controller.layers[0];
        
        var clipState = layer.stateMachine.AddState(clipItem.Value.name);

        clipState.motion = clipItem.Value;
        clipState.speed = speed;

        var transition2 = clipState.AddTransition(_end);
            transition2.hasExitTime = true;
    }

    public bool Play(IMonaAnimationAssetItem clipItem, bool canInterrupt)
    {
        if(canInterrupt)
        {
            //Debug.Log($"CrossFade {clipItem.Value.name}");
            _animator.CrossFade(clipItem.Value.name, 0.2f);
            return true;
        }
        else
        {
            var transition = _animator.GetAnimatorTransitionInfo(0);
            var current = _animator.GetCurrentAnimatorStateInfo(0);
            if (current.IsName(_end.name) || current.IsName(_start.name))
            {
                //Debug.Log($"transition time {transition.normalizedTime}");
                if (transition.normalizedTime == 0)
                {
                    //Debug.Log($"play {clipItem.Value.name}");
                    _animator.Play(clipItem.Value.name);
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasEnded()
    {
        var current = _animator.GetCurrentAnimatorStateInfo(0);
        return current.IsName(_end.name);
    }
}

using Mona.SDK.Brains.Core.Brain;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Events
{

    public struct MonaBodyAnimationControllerChangeEvent
    {
        public Animator Animator;
        public MonaBodyAnimationControllerChangeEvent(Animator animator)
        {
            Animator = animator;
        }
    }
}
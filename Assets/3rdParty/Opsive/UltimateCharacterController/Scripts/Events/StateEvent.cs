/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System.Collections;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Events
{
    public class StateEvent : StateMachineBehaviour
    {
        [SerializeField] protected string m_EventName;

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            EventHandler.ExecuteEvent(animator.gameObject, m_EventName);
        }
    }
}
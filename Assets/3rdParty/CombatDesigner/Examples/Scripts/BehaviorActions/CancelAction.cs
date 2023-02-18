using UnityEngine;

namespace CombatDesigner
{
    /// <summary>
    /// ��Ͻڵ�
    /// </summary>
    public class CancelAction : IBehaviorAction
    {
        public float cancelThreshold;

        public override void Execute(ActorModel model)
        {
            model.CanCancel = cancelThreshold > 0f;

        }
    }
}

using UnityEngine;

namespace CombatDesigner
{
    /// <summary>
    /// ´ò¶Ï½Úµã
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

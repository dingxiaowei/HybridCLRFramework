using UnityEngine;

namespace CombatDesigner
{
    /// <summary>
    /// 受击时暂停
    /// 1.人物随机抖动
    /// 2.暂停期间不走update逻辑
    /// </summary>
    public class HitPauseAction : IBehaviorAction
    {
        public int puaseFrames;

        public override void Execute(ActorModel model)
        {
            model.CurrentHitPauseFrames = puaseFrames;
            model.ActorShake(true);
        }
    }
}

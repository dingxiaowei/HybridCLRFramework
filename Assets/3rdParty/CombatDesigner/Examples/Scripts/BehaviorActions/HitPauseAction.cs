using UnityEngine;

namespace CombatDesigner
{
    /// <summary>
    /// �ܻ�ʱ��ͣ
    /// 1.�����������
    /// 2.��ͣ�ڼ䲻��update�߼�
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

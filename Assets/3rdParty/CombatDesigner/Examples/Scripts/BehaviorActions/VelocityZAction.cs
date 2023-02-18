using UnityEngine;

namespace CombatDesigner
{
    public class VelocityZAction : IBehaviorAction
    {
        public float zDirForce;
        public override void Execute(ActorModel model)
        {
            var dir = zDirForce * model.transform.forward;
            model.velocity += dir;
        }
    }
}

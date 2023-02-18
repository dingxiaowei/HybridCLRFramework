using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CombatDesigner
{
    public class UnderAttack : IUnderAttack
    {
        public void OnGetHit(ActorModel attacker, ActorModel victim, AttackBase atk, Transform dmgTransform)
        {
            victim.transform.LookAt(attacker.transform.localPosition);
        }
    }
}

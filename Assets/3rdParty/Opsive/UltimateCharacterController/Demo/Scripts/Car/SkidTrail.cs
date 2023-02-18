

namespace Opsive.UltimateCharacterController.Demo.UnityStandardAssets.Vehicles.Car
{
    using System.Collections;
    using UnityEngine;

    public class SkidTrail : MonoBehaviour
    {
        [SerializeField] protected float m_PersistTime;

        private IEnumerator Start()
        {
			while (true)
            {
                yield return null;

                if (transform.parent.parent == null)
                {
					Destroy(gameObject, m_PersistTime);
                }
            }
        }
    }
}

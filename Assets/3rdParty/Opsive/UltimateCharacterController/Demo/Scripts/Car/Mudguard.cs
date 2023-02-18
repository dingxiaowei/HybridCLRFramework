
namespace Opsive.UltimateCharacterController.Demo.UnityStandardAssets.Vehicles.Car
{
    using UnityEngine;

    // this script is specific to the supplied Sample Assets car, which has mudguards over the front wheels
    // which have to turn with the wheels when steering is applied.

    public class Mudguard : MonoBehaviour
    {
        public GameObject m_Wheel; // The wheel that the script needs to referencing to get the postion for the suspension

        private CarController m_CarController; // car controller to get the steering angle
        private Vector3 m_TargetOriginalPosition;
        private Vector3 m_OriginalPosition;
        private Quaternion m_OriginalRotation;

        private void Start()
        {
            m_CarController = GetComponentInParent<CarController>();
            m_TargetOriginalPosition = m_Wheel.transform.localPosition;
            m_OriginalPosition = transform.localPosition;
            m_OriginalRotation = transform.localRotation;
        }

        private void Update()
        {
            transform.localPosition = m_OriginalPosition + (m_Wheel.transform.localPosition - m_TargetOriginalPosition);
            transform.localRotation = m_OriginalRotation * Quaternion.Euler(0, m_CarController.CurrentSteerAngle, 0);
        }
    }
}

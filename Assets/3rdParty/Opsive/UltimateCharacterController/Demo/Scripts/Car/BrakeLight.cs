
namespace Opsive.UltimateCharacterController.Demo.UnityStandardAssets.Vehicles.Car
{
    using UnityEngine;

    public class BrakeLight : MonoBehaviour
    {
        private CarController m_Car;
        private Renderer m_Renderer;

        private void Start()
        {
            m_Car = GetComponentInParent<CarController>();
            m_Renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            // enable the Renderer when the car is braking, disable it otherwise.
            m_Renderer.enabled = m_Car.BrakeInput > 0f;
        }
    }
}

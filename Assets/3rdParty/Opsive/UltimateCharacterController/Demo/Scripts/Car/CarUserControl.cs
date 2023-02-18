
namespace Opsive.UltimateCharacterController.Demo.UnityStandardAssets.Vehicles.Car
{
    using UnityEngine;

    [RequireComponent(typeof (CarController))]
    public class CarUserControl : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
        }

        private void FixedUpdate()
        {
            // pass the input to the car!
            float h = UnityEngine.Input.GetAxis("Horizontal");
            float v = UnityEngine.Input.GetAxis("Vertical");
            float handbrake = UnityEngine.Input.GetAxis("Jump");
            m_Car.Move(h, v, v, handbrake);
        }
    }
}

using UnityEngine;

namespace CombatDesigner
{
    /// <summary>
    /// �ƶ��ڵ�
    /// </summary>
    public class GroundMovementAction : IBehaviorAction
    {
        public float moveSpeed;
        public float rotationMaxSpeed = 3000f;
        public float rotationSmoothTime = 0.01f;


        float turnSmoothVelocity;
        private float moveSmoothVelocity;

        public override void Execute(ActorModel model)
        {
            JoyStickMovement(moveSpeed, model);
        }

        void JoyStickMovement(float power, ActorModel model)
        {
            if (model.actorType == ActorType.PLAYER)
            {
                model.moveInputDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                model.animMoveSpeed = Mathf.SmoothDamp(model.animMoveSpeed, model.moveInputDir.magnitude, ref moveSmoothVelocity, 0.1f);
            }

            if (model.moveInputDir.sqrMagnitude > 1)
            {
                model.moveInputDir.Normalize();
            }

            Vector3 moveDir = Vector3.zero;
            if (model.moveInputDir != Vector2.zero)
            {
                Vector3 camFwd = Camera.main.transform.forward;
                camFwd.y = 0;
                camFwd.Normalize();
                moveDir = camFwd * model.moveInputDir.y+ Camera.main.transform.right * model.moveInputDir.x;
                moveDir.y = 0;
            }

            model.velocity = new Vector3(moveDir.x * power,model.velocity.y, moveDir.z * power);
            HandleRotation(model.moveInputDir, model);
        }
        void HandleRotation(Vector2 _inputDir, ActorModel model)
        {
            if (_inputDir != Vector2.zero)
            {
                float targetRotation = Mathf.Atan2(_inputDir.x, _inputDir.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                model.character.transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(model.character.transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, rotationSmoothTime, rotationMaxSpeed, Time.fixedDeltaTime);
            }
        }
    }
}



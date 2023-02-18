/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Abstract class which determines if the ground object is a valid object.
    /// </summary>
    public abstract class DetectGroundAbilityBase : Ability
    {
        [Tooltip("The unique ID value of the Object Identifier component. A value of -1 indicates that this ID should not be used.")]
        [SerializeField] protected int m_ObjectID = -1;
        [Tooltip("The layer mask of the ground object.")]
        [SerializeField] protected LayerMask m_LayerMask = -1;
        [Tooltip("The character is no longer over the ground if the dot product between the character's up direction and the ground normal is less than the sensitivity.")]
        [Range(0, 1)] [SerializeField] protected float m_GroundNormalSensitivity = 0.5f;
        [Tooltip("The maximum angle that the character can be relative to the forward direction of the object.")]
        [Range(0, 180)] [SerializeField] protected float m_AngleThreshold = 180;

        public int ObjectID { get { return m_ObjectID; } set { m_ObjectID = value; } }
        public LayerMask LayerMask { get { return m_LayerMask; } set { m_LayerMask = value; } }
        public float NormalSensitivity { get { return m_GroundNormalSensitivity; } set { m_GroundNormalSensitivity = value; } }
        public float AngleThreshold { get { return m_AngleThreshold; } set { m_AngleThreshold = value; } }

        protected Collider m_GroundCollider;
        protected Transform m_GroundTransform;

        /// <summary>
        /// Can the ability be started?
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            return IsOverValidObject();
        }

        /// <summary>
        /// Is the character over a valid ground object?
        /// </summary>
        /// <returns>True if the character is over a valid ground object.</returns>
        protected bool IsOverValidObject()
        {
            if (!m_CharacterLocomotion.Grounded) {
                return false;
            }

            // The ability may require the character to be directly on top of the ground.
            if (Vector3.Dot(m_CharacterLocomotion.Up, m_CharacterLocomotion.GroundRaycastHit.normal) < m_GroundNormalSensitivity) {
                return false;
            }

            var angle = Quaternion.Angle(Quaternion.LookRotation(m_Transform.forward, m_CharacterLocomotion.Up), 
                                Quaternion.LookRotation(m_CharacterLocomotion.GroundRaycastHit.transform.forward, m_CharacterLocomotion.Up));
            var objectFaces = m_CharacterLocomotion.GroundRaycastHit.transform.gameObject.GetCachedParentComponent<ObjectForwardFaces>();
            // If an object has multiple faces then the ability can start from multiple directions.
            if (objectFaces != null) {
                var roundedAngle = 180 / objectFaces.ForwardFaceCount;
                angle = Mathf.Abs(MathUtility.ClampInnerAngle(angle - (roundedAngle * Mathf.RoundToInt(angle / roundedAngle))));
            }
            if (angle > m_AngleThreshold) {
                return false;
            }

            // Determine if the ground object is a valid ground object. This check only needs to be run when the grounded object changes.
            if (m_GroundCollider != m_CharacterLocomotion.GroundRaycastHit.collider) {
                // The ground object can be detected by using the ObjectIdentifier component.
                if (m_ObjectID != -1) {
                    var objIdentifiers = m_CharacterLocomotion.GroundRaycastHit.collider.gameObject.GetCachedComponents<ObjectIdentifier>();
                    if (objIdentifiers == null || objIdentifiers.Length == 0) {
                        return false;
                    }

                    var idMatch = false;
                    for (int i = 0; i < objIdentifiers.Length; ++i) {
                        if (objIdentifiers[i].ID == m_ObjectID) {
                            idMatch = true;
                            break;
                        }
                    }

                    if (!idMatch) {
                        return false;
                    }
                }

                // The ground object can be detected by using the layer mask.
                if (!MathUtility.InLayerMask(m_CharacterLocomotion.GroundRaycastHit.collider.gameObject.layer, m_LayerMask)) {
                    return false;
                }

                // The ground object is valid.
                m_GroundCollider = m_CharacterLocomotion.GroundRaycastHit.collider;
                m_GroundTransform = m_GroundCollider.transform;
            }

            return true;
        }

        /// <summary>
        /// Stops the ability if the character is no longer over a valid object.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (!IsOverValidObject()) {
                StopAbility();
            }
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            m_GroundCollider = null;
            m_GroundTransform = null;
        }
    }
}
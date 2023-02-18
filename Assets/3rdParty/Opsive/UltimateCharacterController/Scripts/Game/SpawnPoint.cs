/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Game
{
    using UnityEngine;

    /// <summary>
    /// Specifies a location that the object can spawn.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        /// <summary>
        /// Specifies the shape in which the spawn point should randomly be determined.
        /// </summary>
        public enum SpawnShape
        {
            Point,  // The spawn point will be determined at the transform position.
            Sphere, // The spawn point will be determined within a random sphere.
            Box     // The spawn point will be determined within a box.
        }

        [Tooltip("An index value used to group multiple sets of spawn points. A value of -1 will ignore the grouping.")]
        [SerializeField] protected int m_Grouping = -1;
        [Tooltip("Specifies the shape in which the spawn point should randomly be determined.")]
        [SerializeField] protected SpawnShape m_Shape;
        [Tooltip("The size of the spawn shape.")]
        [SerializeField] protected float m_Size;
        [Tooltip("Should the object be spawned randomly within the shape?")]
        [SerializeField] protected bool m_RandomShapeSpawn = true;
        [Tooltip("Specifies the height of the ground check.")]
        [SerializeField] protected float m_GroundSnapHeight;
        [Tooltip("Should the character spawn with a random y direction?")]
        [SerializeField] protected bool m_RandomDirection;
        [Tooltip("Should a check be performed to determine if there are any objects obstructing the spawn point?")]
        [SerializeField] protected bool m_CheckForObstruction;
        [Tooltip("The maximum number of collision points which the spawn points should check against.")]
        [SerializeField] protected int m_MaxCollisionCount = 20;
        [Tooltip("The layers which can obstruct the spawn point.")]
        [SerializeField] protected LayerMask m_ObstructionLayers = ~(1 << LayerManager.Default | 1 << LayerManager.IgnoreRaycast | 1 << LayerManager.TransparentFX | 
                                                                     1 << LayerManager.SubCharacter | 1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect);
        [Tooltip("If checking for obstruction, specifies how many times the location should be determined before it is decided that there are no valid spawn locations.")]
        [SerializeField] protected int m_PlacementAttempts = 10;
#if UNITY_EDITOR
        [Tooltip("The color to draw the editor gizmo in (editor only).")]
        [SerializeField] protected Color m_GizmoColor = new Color(1, 0, 0, 0.3f);
#endif

        public int Grouping
        {
            get { return m_Grouping; }
            set
            {
                if (m_Grouping != value) {
                    // The SpawnPointManager needs to be aware of the change so it can update its internal mapping.
                    if (Application.isPlaying) {
                        SpawnPointManager.UpdateSpawnPointGrouping(this, value);
                    }
                    m_Grouping = value;
                }
            }
        }
        public SpawnShape Shape { get { return m_Shape; } set { m_Shape = value; } }
        public float Size { get { return m_Size; } set { m_Size = value; } }
        public float GroundSnapHeight { get { return m_GroundSnapHeight; } set { m_GroundSnapHeight = value; } }
        public bool RandomDirection { get { return m_RandomDirection; } set { m_RandomDirection = value; } }
        public bool CheckForObstruction { get { return m_CheckForObstruction; } set { m_CheckForObstruction = value; } }
        public int PlacementAttempts { get { return m_PlacementAttempts; } set { m_PlacementAttempts = value; } }
#if UNITY_EDITOR
        public Color GizmoColor { get { return m_GizmoColor; } set { m_GizmoColor = value; } }
#endif

        private Transform m_Transform;
        private Collider[] m_ObstructionColliders;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;

            if (m_CheckForObstruction) {
                m_ObstructionColliders = new Collider[m_MaxCollisionCount];
            }
        }

        /// <summary>
        /// Adds the spawn point to the manager.
        /// </summary>
        private void OnEnable()
        {
            SpawnPointManager.AddSpawnPoint(this);
        }

        /// <summary>
        /// Gets the position and rotation of the spawn point. If false is returned then the point wasn't successfully retrieved.
        /// </summary>
        /// <param name="spawningObject">The object that is spawning.</param>
        /// <param name="position">The position of the spawn point.</param>
        /// <param name="rotation">The rotation of the spawn point.</param>
        /// <returns>True if the spawn point was successfully retrieved.</returns>
        public virtual bool GetPlacement(GameObject spawningObject, ref Vector3 position, ref Quaternion rotation)
        {
            position = RandomPosition(0);

            // Ensure the spawn point is clear of any obstructing objects.
            if (m_CheckForObstruction) {
                var attempt = 0;
                var success = false;
                while (attempt < m_PlacementAttempts) {
                    if (m_Shape == SpawnShape.Point) {
                        // A point will always succeed.
                        success = true;
                    } else if (m_Shape == SpawnShape.Sphere) {
                        // Ignore any collisions with itself.
                        var overlapCount = Physics.OverlapSphereNonAlloc(position, m_Size / 2, m_ObstructionColliders, m_ObstructionLayers, QueryTriggerInteraction.Ignore);
                        if (spawningObject != null) {
                            for (int i = overlapCount - 1; i > -1; --i) {
                                if (!m_ObstructionColliders[i].transform.IsChildOf(spawningObject.transform)) {
                                    break;
                                }
                                overlapCount--;
                            }
                        }
                        success = overlapCount == 0;
                        if (success) {
                            break;
                        }
                    } else { // Box.
                        var extents = Vector3.zero;
                        extents.x = extents.z = m_Size / 2;
                        extents.y = m_GroundSnapHeight / 2;
                        var boxPosition = m_Transform.TransformPoint(extents);

                        // Ignore any collisions with itself.
                        var overlapCount = Physics.OverlapBoxNonAlloc(boxPosition, extents, m_ObstructionColliders, m_Transform.rotation, m_ObstructionLayers, QueryTriggerInteraction.Ignore);
                        for (int i = overlapCount - 1; i > -1; --i) {
                            if (!m_ObstructionColliders[i].transform.IsChildOf(spawningObject.transform)) {
                                break;
                            }
                            overlapCount--;
                        }
                        success = overlapCount == 0;
                        if (success) {
                            break;
                        }
                    }

                    ++attempt;
                    position = RandomPosition(attempt);
                }

                // No valid position was found - return false.
                if (!success) {
                    return false;
                }
            }

            // If the ground snap height is positive then the position should be located on the ground.
            if (m_GroundSnapHeight > 0) {
                RaycastHit raycastHit;
                if (Physics.Raycast(position + m_Transform.up * m_GroundSnapHeight, -m_Transform.up, out raycastHit, m_GroundSnapHeight + 0.2f, m_ObstructionLayers, QueryTriggerInteraction.Ignore)) {
                    position = raycastHit.point + m_Transform.up * 0.01f;
                }
            }

            // Optionally rotate a random spawn direction.
            if (m_RandomDirection) {
                rotation = Quaternion.Euler(m_Transform.up * Random.Range(0, 360));
            } else {
                rotation = m_Transform.rotation;
            }

            return true;
        }

        /// <summary>
        /// Retruns a position based on the spawn shape.
        /// </summary>
        /// <param name="attempt">The attempt to position the object.</param>
        /// <returns>A position within the spawn spape.</returns>
        private Vector3 RandomPosition(int attempt)
        {
            // Always first try to position in the center.
            if (attempt == 0 || !m_RandomShapeSpawn) {
                return m_Transform.position;
            }
            var localPosition = Vector3.zero;
            if (m_Shape == SpawnShape.Sphere) {
                localPosition = Random.insideUnitSphere * m_Size;
                localPosition.y = 0;
            } else if (m_Shape == SpawnShape.Box) {
                var halfSize = m_Size / 2;
                localPosition.x = Random.Range(-halfSize, halfSize);
                localPosition.z = Random.Range(-halfSize, halfSize);
            }

            return m_Transform.TransformPoint(localPosition);
        }

        /// <summary>
        /// Removes the spawn point from the manager.
        /// </summary>
        private void OnDisable()
        {
            SpawnPointManager.RemoveSpawnPoint(this);
        }
    }
}
/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A collection of math functions.
    /// </summary>
    public class MathUtility
    {
        // The multiplier when converting from the CharacterLocomotion force to a Rigidbody force.
        public const float RigidbodyForceMultiplier = 50;
        private static Dictionary<Collider, Transform> m_ColliderTransformMap = new Dictionary<Collider, Transform>();

        /// <summary>
        /// Returns the friction value between material1 and material2.
        /// </summary>
        /// <param name="material1">The first material to get the friction value of.</param>
        /// <param name="material2">The second material to get the friction value of.</param>
        /// <returns>The combined friction value.</returns>
        public static float FrictionValue(PhysicMaterial material1, PhysicMaterial material2, bool dynamicFriction)
        {
            if (material1.frictionCombine == PhysicMaterialCombine.Maximum || material2.frictionCombine == PhysicMaterialCombine.Maximum) {
                return dynamicFriction ? Mathf.Max(material1.dynamicFriction, material2.dynamicFriction) : Mathf.Max(material1.staticFriction, material2.staticFriction);
            }
            if (material1.frictionCombine == PhysicMaterialCombine.Minimum || material2.frictionCombine == PhysicMaterialCombine.Minimum) {
                return dynamicFriction ? Mathf.Min(material1.dynamicFriction, material2.dynamicFriction) : Mathf.Min(material1.staticFriction, material2.staticFriction);
            }
            if (material1.frictionCombine == PhysicMaterialCombine.Multiply || material2.frictionCombine == PhysicMaterialCombine.Multiply) {
                return dynamicFriction ? (material1.dynamicFriction * material2.dynamicFriction) : (material1.staticFriction * material2.staticFriction);
            }
            return dynamicFriction ? ((material1.dynamicFriction + material2.dynamicFriction) / 2) : ((material1.staticFriction + material2.staticFriction) / 2); // Average combine.
        }

        /// <summary>
        /// Returns the bounciness value between material1 and material2.
        /// </summary>
        /// <param name="material1">The first material to get the bounciness value of.</param>
        /// <param name="material2">The second material to get the bounciness value of.</param>
        /// <returns>The combined bounciness value.</returns>
        public static float BouncinessValue(PhysicMaterial material1, PhysicMaterial material2)
        {
            if (material1.bounceCombine == PhysicMaterialCombine.Maximum || material2.bounceCombine == PhysicMaterialCombine.Maximum) {
                return Mathf.Max(material1.bounciness, material2.bounciness);
            }
            if (material1.bounceCombine == PhysicMaterialCombine.Minimum || material2.bounceCombine == PhysicMaterialCombine.Minimum) {
                return Mathf.Min(material1.bounciness, material2.bounciness);
            }
            if (material1.bounceCombine == PhysicMaterialCombine.Multiply || material2.bounceCombine == PhysicMaterialCombine.Multiply) {
                return (material1.bounciness * material2.bounciness);
            }
            return (material1.bounciness + material2.bounciness) / 2; // Average combine.
        }

        /// <summary>
        /// Transforms the position from local space to world space. This is similar to Transform.TransformPoint but does not require a Transform.
        /// </summary>
        /// <param name="worldPosition">The world position of the object.</param>
        /// <param name="rotation">The world rotation of the object.</param>
        /// <param name="localPosition">The local position of the object</param>
        /// <returns>The world space position.</returns>
        public static Vector3 TransformPoint(Vector3 worldPosition, Quaternion rotation, Vector3 localPosition)
        {
            return worldPosition + (rotation * localPosition);
        }

        /// <summary>
        /// Transforms the position from world space to local space. This is similar to Transform.InverseTransformPoint but does not require a Transform.
        /// </summary>
        /// <param name="worldPosition">The world position of the object.</param>
        /// <param name="rotation">The world rotation of the object.</param>
        /// <param name="position">The position of the object.</param>
        /// <returns>The local space position.</returns>
        public static Vector3 InverseTransformPoint(Vector3 worldPosition, Quaternion rotation, Vector3 position)
        {
            var diff = position - worldPosition;
            return Quaternion.Inverse(rotation) * diff;
        }

        /// <summary>
        /// Transforms the direction from local space to world space. This is similar to Transform.TransformDirection but does not require a Transform.
        /// </summary>
        /// <param name="direction">The direction to transform from local space to world space.</param>
        /// <param name="rotation">The world rotation of the object.</param>
        /// <returns>The world space direction.</returns>
        public static Vector3 TransformDirection(Vector3 direction, Quaternion rotation)
        {
            return rotation * direction;
        }

        /// <summary>
        /// Transforms the direction from world space to local space. This is similar to Transform.InverseTransformDirection but does not require a Transform.
        /// </summary>
        /// <param name="direction">The direction to transform from world space to local space.</param>
        /// <param name="rotation">The world rotation of the object.</param>
        /// <returns>The local space direction.</returns>
        public static Vector3 InverseTransformDirection(Vector3 direction, Quaternion rotation)
        {
            return Quaternion.Inverse(rotation) * direction;
        }

        /// <summary>
        /// Transforms the rotation from local space to world space.
        /// </summary>
        /// <param name="worldRotation">The world rotation of the object.</param>
        /// <param name="rotation">The rotation to transform from local space to world space.</param>
        /// <returns>The world space rotation.</returns>
        public static Quaternion TransformQuaternion(Quaternion worldRotation, Quaternion rotation)
        {
            return worldRotation * rotation;
        }

        /// <summary>
        /// Transforms the rotation from world space to local space.
        /// </summary>
        /// <param name="worldRotation">The world rotation of the object.</param>
        /// <param name="rotation">The rotation to transform from world space to local space.</param>
        /// <returns>The local space rotation.</returns>
        public static Quaternion InverseTransformQuaternion(Quaternion worldRotation, Quaternion rotation)
        {
            return Quaternion.Inverse(worldRotation) * rotation;
        }

        /// <summary>
        /// Determines the endcaps of a capsule.
        /// </summary>
        /// <param name="capsuleCollider">The CapsuleCollider to determine the endcaps of.</param>
        /// <param name="position">The position of the CapsuleCollider's transform.</param>
        /// <param name="rotation">The rotation of the CapsuleCollider's transform.</param>
        /// <param name="firstEndCap">The first resulting endcap.</param>
        /// <param name="secondEndCap">The second resulting endcap.</param>
        public static void CapsuleColliderEndCaps(CapsuleCollider capsuleCollider, Vector3 position, Quaternion rotation, out Vector3 firstEndCap, out Vector3 secondEndCap)
        {
            var direction = CapsuleColliderDirection(capsuleCollider);
            var heightMultiplier = CapsuleColliderHeightMultiplier(capsuleCollider);
            var radiusMultipler = ColliderRadiusMultiplier(capsuleCollider);
            firstEndCap = TransformPoint(position, rotation, Vector3.Scale(capsuleCollider.center, capsuleCollider.transform.lossyScale) + direction * (-(capsuleCollider.height * heightMultiplier * 0.5f) + capsuleCollider.radius * radiusMultipler));
            secondEndCap = firstEndCap + (rotation * direction) * (capsuleCollider.height * heightMultiplier - capsuleCollider.radius * radiusMultipler * 2);
        }

        /// <summary>
        /// Determines the endcaps of a capsule.
        /// </summary>
        /// <param name="height">The height of the CapsuleCollider.</param>
        /// <param name="radius">The radius of the CapsuleCollider.</param>
        /// <param name="center">The center of the CapsuleCollider.</param>
        /// <param name="direction">The direction of the CapsuleCollider.</param>
        /// <param name="position">The position of the CapsuleCollider's transform.</param>
        /// <param name="rotation">The rotation of the CapsuleCollider's transform.</param>
        /// <param name="firstEndCap">The first resulting endcap.</param>
        /// <param name="secondEndCap">The second resulting endcap.</param>
        public static void CapsuleColliderEndCaps(float height, float radius, Vector3 center, Vector3 direction, Vector3 position, Quaternion rotation, out Vector3 firstEndCap, out Vector3 secondEndCap)
        {
            firstEndCap = TransformPoint(position, rotation, center + direction * (-(height * 0.5f) + radius));
            secondEndCap = firstEndCap + (rotation * direction) * (height - radius * 2);
        }

        /// <summary>
        /// Returns the world direction that the CapsuleCollider is facing.
        /// </summary>
        /// <param name="capsuleCollider">The CapsuleCollider to determine the direction that it is facing.</param>
        /// <returns>The world direction of the CapsuleCollider.</returns>
        public static Vector3 CapsuleColliderDirection(CapsuleCollider capsuleCollider)
        {
            Vector3 direction;
            if (capsuleCollider.direction == 1) { // Y-Axis.
                direction = Vector3.up;
            } else if (capsuleCollider.direction == 2) { // Z-Axis.
                direction = Vector3.forward;
            } else { // X-Axis.
                direction = Vector3.right;
            }
            return direction;
        }

        /// <summary>
        /// Clamp the angle between -180 and 180 degrees.
        /// </summary>
        /// <param name="angle">The angle to clamp.</param>
        /// <returns>An angle between -180 and 180 degrees.</returns>
        public static float ClampInnerAngle(float angle)
        {
            if (angle < -180) {
                angle += 360;
            }
            if (angle > 180) {
                angle -= 360;
            }
            return angle;
        }

        /// <summary>
        /// Clamp the angle between 0 and 360 degrees.
        /// </summary>
        /// <param name="angle">The angle to clamp.</param>
        /// <returns>An angle between 0 and 360 degrees.</returns>
        public static float ClampAngle(float angle)
        {
            if (angle < 0) {
                angle += 360;
            }
            if (angle > 360) {
                angle -= 360;
            }
            return angle;
        }

        /// <summary>
        /// Clamp the angle between min and max degrees.
        /// </summary>
        /// <param name="angle">The angle to clamp.</param>
        /// <param name="min">The minimum angle range.</param>
        /// <param name="max">The maximum angle range.</param>
        /// <returns>An angle between min and max degrees.</returns>
        public static float ClampAngle(float angle, float min, float max)
        {
            var minDiff = ClampInnerAngle(min - angle);
            var maxDiff = ClampInnerAngle(angle - max);
            if (Mathf.Abs(minDiff) < Mathf.Abs(maxDiff)) {
                if (minDiff <= 0) {
                    return angle;
                }
                return min;
            }
            if (maxDiff <= 0) {
                return angle;
            }
            return max;
        }

        /// <summary>
        /// Clamp the angle between min and max degrees.
        /// </summary>
        /// <param name="angle">The original angle to clamp.</param>
        /// <param name="deltaAngle">The angle to add to the original angle.</param>
        /// <param name="min">The minimum angle range.</param>
        /// <param name="max">The maximum angle range.</param>
        /// <returns>An angle between min and max degrees.</returns>
        public static float ClampAngle(float angle, float deltaAngle, float min, float max)
        {
            var minDiff = ClampInnerAngle(min - angle);
            var maxDiff = ClampInnerAngle(angle - max);
            if (Mathf.Abs(minDiff) < Mathf.Abs(maxDiff)) {
                if (ClampInnerAngle(min - (angle + deltaAngle)) <= 0) {
                    return (angle + deltaAngle);
                }
                return min;
            }
            if (ClampInnerAngle((angle + deltaAngle) - max) <= 0) {
                return (angle + deltaAngle);
            }
            return max;
        }

        /// <summary>
        /// Returns the rotation of the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix to get the rotation of.</param>
        /// <returns>The rotation of the specified matrix.</returns>
        public static Quaternion QuaternionFromMatrix(Matrix4x4 matrix)
        {
            return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        }

        /// <summary>
        /// Returns the position of the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix to get the position of.</param>
        /// <returns>The rotation of the specified matrix.</returns>
        public static Vector3 PositionFromMatrix(Matrix4x4 matrix)
        {
            return matrix.GetColumn(3);
        }

        /// <summary>
        /// Returns the matrix of the first Transform with the deltaRotation applied to the root Transform. This is similar to calling Transform.rotation = value on
        /// the root Transform and getting the position/rotation of the child.
        /// </summary>
        /// <param name="current">The current Transform to add to the matrix.</param>
        /// <param name="root">The base Transform that should have the delta rotation applied to.</param>
        /// <param name="deltaRotation">The rotation to apply to the root Transform.</param>
        /// <returns>The matrix of the first Transform with the deltaRotation applied to the root Transform.</returns>
        public static Matrix4x4 ApplyRotationToChildMatrices(Transform current, Transform root, Quaternion deltaRotation)
        {
            // Recursively multiply the matrices as long as the current Transform is not at the root.
            if (current != root) {
                return ApplyRotationToChildMatrices(current.parent, root, deltaRotation) * Matrix4x4.TRS(current.localPosition, current.localRotation, current.localScale);
            }
            // At the root of the tree, apply the delta to the rotation and return the matrix.
            return Matrix4x4.TRS(current.localPosition, current.localRotation * deltaRotation, current.localScale);
        }

        /// <summary>
        /// Returns the closest point on a capsule or sphere collider.
        /// </summary>
        /// <param name="transform">The parent transform of the object which contains the collider.</param>
        /// <param name="collider">The collider to get the closest point of.</param>
        /// <param name="point">The point used to find the closest point on the collider.</param>
        /// <param name="moveDirection">The direction that the character is moving.</param>
        /// <param name="sphereCheck">Should a sphere check be performed? If false the Pythagorean theorem will be used.</param>
        /// <param name="lowerPoint">Should the lower point of the collider be returned? Used by the ground check to always return the lowest point.</param>
        /// <returns>The closest point on the collider.</returns>
        public static Vector3 ClosestPointOnCollider(Transform transform, Collider collider, Vector3 point, Vector3 moveDirection, bool sphereCheck, bool lowerPoint)
        {
            if (collider is CapsuleCollider) {
                return ClosestPointOnCapsule(transform, collider as CapsuleCollider, point, moveDirection, sphereCheck, lowerPoint);
            } else { // SphereCollider.
                var sphereCollider = collider as SphereCollider;
                return ClosestPointOnSphere(transform, point, collider.transform.TransformPoint(sphereCollider.center), sphereCollider.radius * ColliderRadiusMultiplier(collider), sphereCheck, lowerPoint);
            }
        }

        /// <summary>
        /// Returns the closest point on a CapsuleCollider.
        /// </summary>
        /// <param name="transform">The parent transform of the object which contains the collider.</param>
        /// <param name="capsuleCollider">The CapsuleCollider to get the closest point of.</param>
        /// <param name="point">The point used to find the closest point on the collider.</param>
        /// <param name="moveDirection">The direction that the character is moving.</param>
        /// <param name="sphereCheck">Should a sphere check be performed? If false the Pythagorean theorem will be used.</param>
        /// <param name="lowerPoint">Should the lower point of the collider be returned? Used by the ground check to always return the lowest point.</param>
        /// <returns>The closest point on a capsule.</returns>
        private static Vector3 ClosestPointOnCapsule(Transform transform, CapsuleCollider capsuleCollider, Vector3 point, Vector3 moveDirection, bool sphereCheck, bool lowerPoint)
        {
            Vector3 capsuleDirection;
            if (capsuleCollider.direction == 1) { // Y-Axis.
                capsuleDirection = capsuleCollider.transform.up;
            } else if (capsuleCollider.direction == 2) { // Z-Axis.
                capsuleDirection = capsuleCollider.transform.forward;
            } else { // X-Axis.
                capsuleDirection = capsuleCollider.transform.right;
            }
            var heightMultiplier = CapsuleColliderHeightMultiplier(capsuleCollider);
            var radiusMultiplier = ColliderRadiusMultiplier(capsuleCollider);

            // If the hit point is within the spheres of the Capsule Collider then the collider position should be based off of the Capsule Collider length (using the point projected onto
            // a cylinder forumla). If the hit point is on the ends of the Capsule Collider then the Calsule Collider caps should be used (or, based off of a sphere).
            var capsuleCenter = capsuleCollider.transform.TransformPoint(capsuleCollider.center) + moveDirection;
            var capsuleLength = ((capsuleCollider.height * heightMultiplier * 0.5f) - capsuleCollider.radius * radiusMultiplier);
            var start = capsuleCenter - capsuleDirection * capsuleLength;
            var end = capsuleCenter + capsuleDirection * capsuleLength;
            var hitDirection = (point - capsuleCenter).normalized;

            // Use the project point on segment forumla to determine if the closest point is on the segment or the endcap.
            var pointStartDirection = point - start;
            var endStartDirection = end - start;
            var segment = (Vector3.Dot(pointStartDirection, endStartDirection) / Vector3.Dot(endStartDirection, endStartDirection));
            if (segment >= 0 && segment <= 1) { // On cylinder.
                // If the point is on the segment then the collision point is within the collider.
                var closestPoint = start + segment * endStartDirection;
                var pointDirection = (point - closestPoint).normalized * capsuleCollider.radius * radiusMultiplier;
                if (lowerPoint) {
                    // If the direction is above the collider then inverse the direction. This will prevent the closest point being on top of the collider when it should be on the bottom.
                    var localCylinderDirection = transform.InverseTransformDirection(pointDirection);
                    if (localCylinderDirection.y > 0) {
                        localCylinderDirection.y *= -1;
                        pointDirection = transform.TransformDirection(localCylinderDirection);
                    }
                }
                return closestPoint + pointDirection;
            } else { // On sphere.
                if (lowerPoint) {
                    // If the direction is above the collider then inverse the direction. This will prevent the closest point being on top of the collider when it should be on the bottom.
                    var localHitDirection = transform.InverseTransformDirection(hitDirection);
                    if (localHitDirection.y > 0) {
                        localHitDirection.y *= -1;
                        hitDirection = transform.TransformDirection(localHitDirection);
                    }
                }
                var dot = Vector3.Dot(capsuleDirection, hitDirection);
                var sphereCenter = capsuleCenter + (capsuleDirection * capsuleLength * Mathf.Sign(dot));
                return ClosestPointOnSphere(transform, point, sphereCenter, capsuleCollider.radius * radiusMultiplier, sphereCheck, lowerPoint);
            }
        }

        /// <summary>
        /// Returns the closest point on a SphereCollider.
        /// </summary>
        /// <param name="transform">The parent transform of the object which contains the collider.</param>
        /// <param name="point">The point used to find the closest point on the collider.</param>
        /// <param name="sphereCenter">The center of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <param name="sphereCheck">Should a sphere check be performed? If false the Pythagorean theorem will be used.</param>
        /// <returns>The closest point on a sphere.</returns>
        private static Vector3 ClosestPointOnSphere(Transform transform, Vector3 point, Vector3 sphereCenter, float radius, bool sphereCheck, bool lowerPoint)
        {
            var position = Vector3.zero;
            var localDirection = InverseTransformPoint(sphereCenter, transform.rotation, point);
            if (sphereCheck || localDirection.y > radius) {
                // Use the standard closest point on a sphere algorithm.
                var direction = (point - sphereCenter).normalized;
                if (lowerPoint) {
                    // If the direction is above the collider then inverse the direction. This will prevent the closest point being on top of the collider when it should be on the bottom.
                    var localSphereDirection = transform.InverseTransformDirection(direction);
                    if (localSphereDirection.y > 0) {
                        localSphereDirection.y *= -1;
                        direction = transform.TransformDirection(localSphereDirection);
                    }
                }
                position = sphereCenter + (direction * radius);
            } else {
                // Use the Pythagorean theorem to determine the point. This won't return the closest point but it will return the point that the collider should adjust to.
                // Ignore the local y value because the Pythagorean theorem is used to determine the y position.
                localDirection.y = 0;
                var magnitude = localDirection.magnitude;
                if (magnitude < radius) {
                    position = sphereCenter - transform.up * Mathf.Sqrt((radius * radius) - (magnitude * magnitude));
                } else {
                    position = sphereCenter - transform.up * radius;
                }
            }

            return position;
        }

        /// <summary>
        /// Returns the CapsuleCollider height multipler based off of the scale.
        /// </summary>
        /// <param name="capsuleCollider">The CapsuleCollider to determine the height multiplier of.</param>
        /// <returns>The capsule collider height multipler based off of the scale.</returns>
        public static float CapsuleColliderHeightMultiplier(CapsuleCollider capsuleCollider)
        {
            // Use the cached transform for quick lookup.
            Transform transform;
            if (!m_ColliderTransformMap.TryGetValue(capsuleCollider, out transform)) {
                transform = capsuleCollider.transform;
                m_ColliderTransformMap.Add(capsuleCollider, transform);
            }

            if (capsuleCollider.direction == 1) { // Y-axis.
                return transform.lossyScale.y;
            } else if (capsuleCollider.direction == 2) { // Z-axis.
                return transform.lossyScale.z;
            }
            return transform.lossyScale.x;
        }

        /// <summary>
        /// Returns the radius multipler of the collider based off of the scale.
        /// </summary>
        /// <param name="collider">The collider determine the radius multiplier of.</param>
        /// <returns>The radius multipler of the collider based off of the scale.</returns>
        public static float ColliderRadiusMultiplier(Collider collider)
        {
            // Use the cached transform for quick lookup.
            Transform transform;
            if (!m_ColliderTransformMap.TryGetValue(collider, out transform)) {
                transform = collider.transform;
                m_ColliderTransformMap.Add(collider, transform);
            }

            var lossyScale = transform.lossyScale;
            if (collider is CapsuleCollider) {
                var capsuleCollider = collider as CapsuleCollider;
                if (capsuleCollider.direction == 1) { // Y-axis.
                    return Mathf.Max(lossyScale.x, lossyScale.z);
                } else if (capsuleCollider.direction == 2) { // Z-axis.
                    return Mathf.Max(lossyScale.x, lossyScale.y);
                }
                return Mathf.Max(lossyScale.y, lossyScale.z);
            } else { // SphereCollider.
                return Mathf.Max(lossyScale.x, Mathf.Max(lossyScale.y, lossyScale.z));
            }
        }

        /// <summary>
        /// Is the point under the Collider?
        /// </summary>
        /// <param name="transform">The Collider's Transform.</param>
        /// <param name="collider">The interested Collider.</param>
        /// <param name="point">The point to check if under the Collider.</param>
        /// <returns>Returns true if the point is under the Collider.</returns>
        public static bool IsUnderCollider(Transform transform, Collider collider, Vector3 point)
        {
            var center = (collider is SphereCollider ? (collider as SphereCollider).center : (collider as CapsuleCollider).center);
            var direction = transform.InverseTransformDirection(point - collider.transform.TransformPoint(center));
            return direction.y <= 0.001f;
        }

        /// <summary>
        /// Returns the height of the collider.
        /// </summary>
        /// <param name="transform">The transform used to determine the up direction.</param>
        /// <param name="collider">The collider to get the height of.</param>
        /// <returns>The height of the collider.</returns>
        public static float LocalColliderHeight(Transform transform, Collider collider)
        {
            // The height of the collider is determined by the uppermost point on the collider transformed into the local position of the object.
            var maxValue = (collider is CapsuleCollider ? (collider as CapsuleCollider).height : (collider as SphereCollider).radius) * 100;
            var topPosition = ClosestPointOnCollider(transform, collider, transform.TransformPoint(0, maxValue, 0), Vector3.zero, true, false);
            return transform.InverseTransformPoint(topPosition).y;
        }

        /// <summary>
        /// Returns the invese of pow.
        /// </summary>
        /// <param name="b">The base value.</param>
        /// <param name="value">The value computed by pow.</param>
        /// <returns>The inverse of pow.</returns>
        public static float InversePow(float b, float value)
        {
            return Mathf.Log(value) / Mathf.Log(b);
        }

        /// <summary>
        /// Rounds the specified value according to the number of decimals.
        /// </summary>
        /// <param name="value">The value to round.</param>
        /// <param name="factor">The factor to round to.</param>
        /// <returns>The roudned value.</returns>
        public static float Round(float value, int factor)
        {
            return Mathf.Round(value * factor) / factor;
        }

        /// <summary>
        /// Rounds the specified value according to the number of decimals.
        /// </summary>
        /// <param name="value">The value to round.</param>
        /// <param name="factor">The factor to round to.</param>
        /// <returns>The roudned value.</returns>
        public static Quaternion Round(Quaternion value, int factor)
        {
            value.x = Round(value.x, factor);
            value.y = Round(value.y, factor);
            value.z = Round(value.z, factor);
            value.w = Round(value.w, factor);
            return value;
        }

        /// <summary>
        /// Retrusn true if the specified scale is almost uniform. An epsilon value is used so the scale doesn't have to be 
        /// precisely uniform.
        /// </summary>
        /// <param name="scale">The scale to determine if it is uniform.</param>
        /// <returns>True if the specified scale is almost uniform.</returns>
        public static bool IsUniform(Vector3 scale)
        {
            return Mathf.Abs(scale.x - scale.y) < 0.00001f && Mathf.Abs(scale.y - scale.z) < 0.00001f;
        }

        /// <summary>
        /// Returns true if layer is within the layerMask.
        /// </summary>
        /// <param name="layer">The layer to check.</param>
        /// <param name="layerMask">The mask to compare against.</param>
        /// <returns>True if the layer is within the layer mask.</returns>
        public static bool InLayerMask(int layer, int layerMask)
        {
            return ((1 << layer) & layerMask) == (1 << layer);
        }

        /// <summary>
        /// Concatenate three integers into one. The first int will occupy the millions place, the second int will occupy the thousands place, and the third 
        /// int will occupy the hundeds place. For example, a value of 30, 52, 1 will return 30052001.
        /// </summary>
        /// <param name="a">The first integer to concatentate.</param>
        /// <param name="a">The second integer to concatentate.</param>
        /// <param name="a">The third integer to concatentate.</param>
        /// <returns>The concatenated integer.</returns>
        public static int Concatenate(int a, int b, int c)
        {
            return (a * 1000000) + (b * 1000) + c;
        }
    }
}

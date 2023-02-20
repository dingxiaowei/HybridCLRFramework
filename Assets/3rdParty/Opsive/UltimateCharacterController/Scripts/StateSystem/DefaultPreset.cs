/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System.Reflection;
using System.Collections.Generic;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.StateSystem
{
    /// <summary>
    /// Used by the state system for the default preset. The default preset will only set the property value when there has been a change from another preset.
    /// </summary>
    public class DefaultPreset : Preset
    {
        private Dictionary<MethodInfo, int> m_DelegateIndexMap;

        /// <summary>
        /// Creates a default preset.
        /// </summary>
        /// <returns>The created preset.</returns>
        public static DefaultPreset CreateDefaultPreset()
        {
            return CreateInstance<DefaultPreset>();
        }

        /// <summary>
        /// Initializes the preset with the specified visiblity. The preset must be initialized before the preset values are applied so the delegates can be created.
        /// </summary>
        /// <param name="obj">The object to map the delegates to.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        public override void Initialize(object obj, MemberVisibility visibility)
        {
            base.Initialize(obj, visibility);

            m_DelegateIndexMap = new Dictionary<MethodInfo, int>();
            for (int i = 0; i < m_Delegates.Length; ++i) {
                m_DelegateIndexMap.Add(m_Delegates[i].SetMethod, i);
            }
        }

        /// <summary>
        /// Applies the values to the component specified by the delegates.
        /// </summary>
        /// <param name="delegates">The properties that were changed.</param>
        public override void ApplyValues(BaseDelegate[] delegates)
        {
            // Only apply the properties that were changed. This is determined by the delegates array.
            for (int i = 0; i < delegates.Length; ++i) {
                var index = m_DelegateIndexMap[delegates[i].SetMethod];
                m_Delegates[index].ApplyValue();
            }
        }
    }
}
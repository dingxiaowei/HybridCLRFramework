/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Opsive.UltimateCharacterController.Editor.Utility
{
    /// <summary>
    /// Generates the code to create the states and transitions of a particular state machine.
    /// </summary>
    public static class AnimatorBuilder
    {
        private const string c_StartGeneratedCodeComment = "// ------------------------------------------- Start Generated Code -------------------------------------------";

        /// <summary>
        /// Storage struct for specifying if a transition originates from the any or entry state transition.
        /// </summary>
        public struct AnimatorTransition
        {
            private AnimatorTransitionBase m_Transition;
            private bool m_AnyStateTransition;

            public AnimatorTransitionBase Transition { get { return m_Transition; } }
            public bool AnyStateTransition { get { return m_AnyStateTransition; } }

            public AnimatorTransition(AnimatorTransitionBase transition, bool anyStateTransition)
            {
                m_Transition = transition;
                m_AnyStateTransition = anyStateTransition;
            }
        }

        /// <summary>
        /// Generates the code necessary to recreate the states/transitions that are affected by the specified parameter name and value.
        /// </summary>
        /// <param name="animatorController">The animator controller to generate the states/transitions of.</param>
        /// <param name="parameterName">The name of the animator parameter which should have its states/transitions generated.</param>
        /// <param name="parameterValue">The value of the animator parameter which should have its states/transitions generated.</param>
        /// <param name="parentObject">The object which called GenerateAnimator.</param>
        /// <param name="baseDirectory">The directory that the scripts are located.</param>
        /// <returns>The file path of the generated code.</returns>
        public static string GenerateAnimatorCode(AnimatorController animatorController, AnimatorController firstPersonAnimatorController, string parameterName, float parameterValue, object parentObject, string baseDirectory)
        {
            var generatedCode = new StringBuilder();
            var generatedStateMachineCode = GenerateAnimatorCode(animatorController, "animatorController", parameterName, parameterValue, generatedCode);
            generatedStateMachineCode = GenerateAnimatorCode(firstPersonAnimatorController, "firstPersonAnimatorController", parameterName, parameterValue, generatedCode) || generatedStateMachineCode;

            // The code for the animator controller has been generated. Add it to a new file.
            if (generatedStateMachineCode) {
                // Prepare the string for being appended to a file.
                var parentName = parentObject.GetType().Name;
                var path = EditorUtility.SaveFilePanel("Save Item", baseDirectory + "/Abilities", parentName + "InspectorDrawer.cs", "cs");
                if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                    var fileString = new StringBuilder();
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    // If the file already exists then the generated code should be appended to the end.
                    if (File.Exists(path)) {
                        using (var sr = new StreamReader(path)) {
                            var contents = sr.ReadToEnd();
                            var startIndex = -1;
                            if ((startIndex = contents.IndexOf(c_StartGeneratedCodeComment)) > -1) {
                                // Remove the contents after the start generated code comment so the file can start fresh.
                                contents = contents.Remove(startIndex);
                            } else {
                                // Remove the last two curly brackets if the code comment doesn't exist.
                                var count = 0;
                                while (count < 2) {
                                    contents = contents.TrimEnd(new char[] { ' ', '\n', '\r', '\t' });
                                    contents.Remove(contents.Length - 1);
                                    count++;
                                }
                            }

                            contents = contents.TrimEnd(new char[] { ' ', '\t' });
                            fileString.Append(contents);
                        }
                    } else {
                        // Start a new file.
                        fileString.AppendLine("using UnityEngine;");
                        fileString.AppendLine("using UnityEditor;");
                        fileString.AppendLine("using UnityEditor.Animations;");
                        fileString.AppendLine("using Opsive.UltimateCharacterController.Editor.Utility;");
                        fileString.AppendLine();
                        fileString.AppendLine("namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Abilities");
                        fileString.AppendLine("{");
                        fileString.AppendLine("\t/// <summary>");
                        fileString.AppendLine("\t/// Draws a custom inspector for the " + parentName + " Ability.");
                        fileString.AppendLine("\t/// </summary>");
                        fileString.AppendLine("\t[InspectorDrawer(typeof(" + parentObject.GetType().FullName + "))]");
                        fileString.AppendLine("\tpublic class " + fileName + " : AbilityInspectorDrawer");
                        fileString.AppendLine("\t{");
                    }

                    // Add the generated code.
                    fileString.AppendLine("\t\t" + c_StartGeneratedCodeComment);
                    fileString.AppendLine("\t\t// ------- Do NOT make any changes below. Changes will be removed when the animator is generated again. -------");
                    fileString.AppendLine("\t\t// ------------------------------------------------------------------------------------------------------------");
                    fileString.AppendLine();
                    fileString.AppendLine("\t\t/// <summary>");
                    fileString.AppendLine("\t\t/// Returns true if the ability can build to the animator.");
                    fileString.AppendLine("\t\t/// </summary>");
                    fileString.AppendLine("\t\tpublic override bool CanBuildAnimator { get { return true; } }");
                    fileString.AppendLine();
                    fileString.AppendLine("\t\t/// <summary>");
                    fileString.AppendLine("\t\t/// An editor only method which can add the abilities states/transitions to the animator.");
                    fileString.AppendLine("\t\t/// </summary>");
                    fileString.AppendLine("\t\t/// <param name=\"animatorController\">The Animator Controller to add the states to.</param>");
                    fileString.AppendLine("\t\t/// <param name=\"firstPersonAnimatorController\">The first person Animator Controller to add the states to.</param>");
                    fileString.AppendLine("\t\tpublic override void BuildAnimator(AnimatorController animatorController, AnimatorController firstPersonAnimatorController)");
                    fileString.AppendLine("\t\t{");
                    fileString.Append(generatedCode.ToString());
                    fileString.AppendLine("\t\t}");
                    fileString.AppendLine("\t}");
                    fileString.AppendLine("}");

                    // Save the file.
                    var file = new StreamWriter(path, false);
                    file.Write(fileString.ToString());
                    file.Close();
                    AssetDatabase.Refresh();
                    return path;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Generates the code necessary to recreate the states/transitions that are affected by the specified parameter name and value.
        /// </summary>
        /// <param name="animatorController">The animator controller to generate the states/transitions of.</param>
        /// <param name="animatorVariableName">The name of the animator controller variable name. This name is used within the generated code.</param>
        /// <param name="parameterName">The name of the animator parameter which should have its states/transitions generated.</param>
        /// <param name="parameterValue">The value of the animator parameter which should have its states/transitions generated.</param>
        /// <param name="generatedCode">The final generated code.</param>
        /// <returns>Was the animator code generated?</returns>
        private static bool GenerateAnimatorCode(AnimatorController animatorController, string animatorVariableName, string parameterName, float parameterValue, StringBuilder generatedCode)
        {
            var generateAnimatorCode = false;
            var motionSet = new HashSet<UnityEngine.Motion>();
            for (int i = 0; i < animatorController.layers.Length; ++i) {
                var layer = animatorController.layers[i];
                var stateMachine = layer.stateMachine;

                var transitions = new List<AnimatorTransition>();
                FindTransitions(stateMachine.anyStateTransitions, parameterName, parameterValue, transitions, true);
                FindTransitions(stateMachine.entryTransitions, parameterName, parameterValue, transitions, false);

                // The list of transitions have been found which match the given parameters. The transition has a reference to the state
                // but not the state machine so another search needs to be done which finds the parent state machine. If the state machine
                // has the state then that state machine (and any child state machines) should be generated.
                var stateMachines = new List<ChildAnimatorStateMachine>();
                for (int j = 0; j < transitions.Count; ++j) {
                    for (int k = 0; k < stateMachine.stateMachines.Length; ++k) {
                        if (HasState(stateMachine.stateMachines[k].stateMachine, transitions[j].Transition.destinationState)) {
                            stateMachines.Add(stateMachine.stateMachines[k]);
                        }
                    }
                }

                // Create all of the states under the highest level child state machine.
                if (stateMachines.Count > 0) {
                    generateAnimatorCode = true;
                    if (generatedCode.Length > 0) {
                        generatedCode.AppendLine();
                    }

                    // The state machine should start fresh.
                    var baseStateMachine = stateMachines[0];
                    var baseStateMachineName = "baseStateMachine" + i;
                    generatedCode.AppendLine("\t\t\tvar " + baseStateMachineName + " = " + animatorVariableName + ".layers[" + i + "].stateMachine;");
                    generatedCode.AppendLine();
                    generatedCode.AppendLine("\t\t\t// The state machine should start fresh.");
                    generatedCode.AppendLine("\t\t\tfor (int i = 0; i < " + animatorVariableName + ".layers.Length; ++i) {");
                    generatedCode.AppendLine("\t\t\t\tfor (int j = 0; j < " + baseStateMachineName + ".stateMachines.Length; ++j) {");
                    generatedCode.AppendLine("\t\t\t\t\tif (" + baseStateMachineName + ".stateMachines[j].stateMachine.name == \"" + baseStateMachine.stateMachine.name + "\") {");
                    generatedCode.AppendLine("\t\t\t\t\t\t" + baseStateMachineName + ".RemoveStateMachine(" + baseStateMachineName + ".stateMachines[j].stateMachine);");
                    generatedCode.AppendLine("\t\t\t\t\t\tbreak;");
                    generatedCode.AppendLine("\t\t\t\t\t}");
                    generatedCode.AppendLine("\t\t\t\t}");
                    generatedCode.AppendLine("\t\t\t}");
                    generatedCode.AppendLine();

                    // Generate the AnimationClips first so they can be later referenced by the states.
                    generatedCode.AppendLine("\t\t\t// AnimationClip references.");
                    GenerateMotions(baseStateMachine, motionSet, generatedCode);
                    generatedCode.AppendLine();

                    // Generate the states and transition within each substate machine.
                    GenerateStateMachine(baseStateMachineName, baseStateMachine, generatedCode);

                    // Add the any state and entry transitions.
                    generatedCode.AppendLine("\t\t\t// State Machine Transitions.");
                    for (int j = 0; j < transitions.Count; ++j) {
                        GenerateTransition(baseStateMachineName, transitions[j].Transition, false, transitions[j].AnyStateTransition, generatedCode);
                        if (j != transitions.Count - 1) {
                            generatedCode.AppendLine();
                        }
                    }
                }
            }
            return generateAnimatorCode;
        }

        /// <summary>
        /// Finds any transitions that have a condition with the specified parameter name and value.
        /// </summary>
        /// <param name="transitions">The array of transitions to search.</param>
        /// <param name="parameterName">The name of the parameter to search for.</param>
        /// <param name="parameterValue">The value of the paramter to search for.</param>
        /// <param name="validTransitions">A list of transitions that have the matched parameters.</param>
        /// <param name="anyStateTransition">Does the transition originate from the any state?</param>
        public static void FindTransitions(AnimatorTransitionBase[] transitions, string parameterName, float parameterValue, List<AnimatorTransition> validTransitions, bool anyStateTransition)
        {
            if (transitions == null) {
                return;
            }

            for (int i = 0; i < transitions.Length; ++i) {
                if (transitions[i] == null) {
                    continue;
                }
                var conditions = transitions[i].conditions;
                var validTransition = false;
                for (int j = 0; j < conditions.Length; ++j) {
                    // The transition is valid if the parameter name and value matches.
                    if (conditions[j].mode == AnimatorConditionMode.Equals && conditions[j].parameter == parameterName && conditions[j].threshold == parameterValue) {
                        validTransition = true;
                        break;
                    }
                }

                if (validTransition) {
                    validTransitions.Add(new AnimatorTransition(transitions[i], anyStateTransition));
                }
            }
        }

        /// <summary>
        /// Returns true if the state machine (or any child state machines) contains the specified state.
        /// </summary>
        /// <param name="stateMachine">The state machine to determine if it has the specified state.</param>
        /// <param name="state">The state to check against.</param>
        /// <returns>True if the state machine contains the specified state.</returns>
        public static bool HasState(AnimatorStateMachine stateMachine, AnimatorState state)
        {
            if (stateMachine == null) {
                return false;
            }

            // Determine if the current state machine contains the specified state.
            if (stateMachine.states != null) {
                for (int i = 0; i < stateMachine.states.Length; ++i) {
                    if (stateMachine.states[i].state == state) {
                        return true;
                    }
                }
            }

            // The state wasn't found within the current StateMachine. Search deeper.
            for (int i = 0; i < stateMachine.stateMachines.Length; ++i) {
                if (HasState(stateMachine.stateMachines[i].stateMachine, state)) {
                    return true;
                }
            }

            // The state machine and child state machines do not contain the state.
            return false;
        }

        /// <summary>
        /// Generates the code to recreate the motions within the specified state machine.
        /// </summary>
        /// <param name="childStateMachine">The state machine used to search for motions.</param>
        /// <param name="motionSet">The list of motions which have been already generated.</param>
        /// <param name="generatedCode">The final generated code.</param>
        private static void GenerateMotions(ChildAnimatorStateMachine childStateMachine, HashSet<UnityEngine.Motion> motionSet, StringBuilder generatedCode)
        {
            var stateMachine = childStateMachine.stateMachine;
            if (stateMachine.states.Length > 0) {
                for (int i = 0; i < stateMachine.states.Length; ++i) {
                    var motion = stateMachine.states[i].state.motion;
                    if (motion is BlendTree) {
                        // Blend trees can contain other blend trees so recursion is necessary.
                        GenerateBlendTreeMotions(motion as BlendTree, motionSet, generatedCode);
                    } else {
                        if (motion != null) {
                            GenerateMotion(motion, motionSet, generatedCode);
                        }
                    }
                }
            }

            // Search deeper for any motions.
            for (int i = 0; i < stateMachine.stateMachines.Length; ++i) {
                GenerateMotions(stateMachine.stateMachines[i], motionSet, generatedCode);
            }
        }

        /// <summary>
        /// Generates the code to recreate the motions within the specified blend tree.
        /// </summary>
        /// <param name="childStateMachine">The blend tree used to search for motions.</param>
        /// <param name="motionSet">The list of motions which have been already generated.</param>
        /// <param name="generatedCode">The final generated code.</param>
        private static void GenerateBlendTreeMotions(BlendTree blendTree, HashSet<UnityEngine.Motion> motionSet, StringBuilder generatedCode)
        {
            for (int j = 0; j < blendTree.children.Length; ++j) {
                var childMotion = blendTree.children[j];
                if (childMotion.motion != null) {
                    if (childMotion.motion is BlendTree) {
                        // The blend tree contains another blend tree - continue the search.
                        GenerateBlendTreeMotions(childMotion.motion as BlendTree, motionSet, generatedCode);
                    } else {
                        GenerateMotion(childMotion.motion, motionSet, generatedCode);
                    }
                }
            }
        }

        /// <summary>
        /// Generates the code to recreate the specified motion.
        /// </summary>
        /// <param name="motion">The motion to generate the code of.</param>
        /// <param name="motionSet">The list of motions which have been already generated.</param>
        /// <param name="generatedCode">The final generated code.</param>
        private static void GenerateMotion(UnityEngine.Motion motion, HashSet<UnityEngine.Motion> motionSet, StringBuilder generatedCode)
        {
            if (motionSet.Contains(motion)) {
                return;
            }
            motionSet.Add(motion);

            // Store the GUID so the path can change and not affect the generated animator controller.
            var uniqueMotionName = UniqueName(motion);
            var uniqueMotionNamePath = uniqueMotionName + "Path";
            generatedCode.AppendLine("\t\t\tvar " + uniqueMotionNamePath + " = AssetDatabase.GUIDToAssetPath(\"" +
                                                AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(motion as AnimationClip)) + "\"); ");
            generatedCode.AppendLine("\t\t\tvar " + uniqueMotionName + " = AnimatorBuilder.GetAnimationClip(" + uniqueMotionNamePath + ", \"" + motion.name + "\");");
        }

        /// <summary>
        /// Generates the code to recreate the specified state machine.
        /// </summary>
        /// <param name="parentStateMachineName">The name of the parent state machine variable.</param>
        /// <param name="childStateMachine">The state machine to generate the code of.</param>
        /// <param name="generatedCode">The final generated code.</param>
        private static void GenerateStateMachine(string parentStateMachineName, ChildAnimatorStateMachine childStateMachine, StringBuilder generatedCode)
        {
            var stateMachine = childStateMachine.stateMachine;
            generatedCode.AppendLine("\t\t\t// State Machine.");
            var uniqueStateMachineName = UniqueName(stateMachine);
            generatedCode.AppendLine("\t\t\tvar " + uniqueStateMachineName + " = " + parentStateMachineName + ".AddStateMachine(\"" +
                                                stateMachine.name + "\", " + Vector3String(childStateMachine.position) + ");");
            generatedCode.AppendLine();

            // Add all the states.
            if (stateMachine.states.Length > 0) {
                generatedCode.AppendLine("\t\t\t// States.");
                for (int i = 0; i < stateMachine.states.Length; ++i) {
                    var state = stateMachine.states[i].state;
                    var uniqueStateName = UniqueName(state);
                    generatedCode.AppendLine("\t\t\tvar " + uniqueStateName + " = " + uniqueStateMachineName + ".AddState(\"" + state.name + "\", " +
                                                        Vector3String(stateMachine.states[i].position) + ");");
                    if (state.motion is BlendTree) {
                        var blendTree = state.motion as BlendTree;
                        var blendTreeName = uniqueStateName + UniqueName(blendTree);
                        GenerateBlendTree(blendTreeName, blendTree, generatedCode, 3);
                        generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".motion = " + blendTreeName + ";");
                    } else if (state.motion != null) {
                        generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".motion = " + UniqueName(state.motion) + ";");
                    }
                    generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".cycleOffset = " + state.cycleOffset + "f;");
                    if (!string.IsNullOrEmpty(state.cycleOffsetParameter)) {
                        generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".cycleOffsetParameter = \"" + state.cycleOffsetParameter + "\";");
                    }
                    generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".cycleOffsetParameterActive = " + BoolString(state.cycleOffsetParameterActive) + ";");
                    generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".iKOnFeet = " + BoolString(state.iKOnFeet) + ";");
                    generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".mirror = " + BoolString(state.mirror) + ";");
                    if (!string.IsNullOrEmpty(state.mirrorParameter)) {
                        generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".mirrorParameter = \"" + state.mirrorParameter + "\";");
                    }
                    generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".mirrorParameterActive = " + BoolString(state.mirrorParameterActive) + ";");
                    generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".speed = " + state.speed + "f;");
                    if (!string.IsNullOrEmpty(state.speedParameter)) {
                        generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".speedParameter = \"" + state.speedParameter + "\";");
                    }
                    generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".speedParameterActive = " + BoolString(state.speedParameterActive) + ";");
                    if (!string.IsNullOrEmpty(state.tag)) {
                        generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".tag = \"" + state.tag + "\";");
                    }
                    generatedCode.AppendLine("\t\t\t" + uniqueStateName + ".writeDefaultValues = " + BoolString(state.writeDefaultValues) + ";");
                    generatedCode.AppendLine();
                }
            }

            // Add all of the any state transitions.
            if (stateMachine.anyStateTransitions.Length > 0) {
                generatedCode.AppendLine("\t\t\t// Any State Transitions.");
                for (int i = 0; i < stateMachine.anyStateTransitions.Length; ++i) {
                    GenerateTransition(uniqueStateMachineName, stateMachine.anyStateTransitions[i], false, true, generatedCode);
                    generatedCode.AppendLine();
                }
            }

            // Add all of the entry transitions.
            if (stateMachine.entryTransitions.Length > 0) {
                generatedCode.AppendLine("\t\t\t// Entry Transitions.");
                for (int i = 0; i < stateMachine.entryTransitions.Length; ++i) {
                    GenerateTransition(uniqueStateMachineName, stateMachine.entryTransitions[i], false, false, generatedCode);
                    generatedCode.AppendLine();
                }
            }

            // Add all of the state transitions.
            if (stateMachine.states.Length > 0) {
                generatedCode.AppendLine("\t\t\t// State Transitions.");
                for (int i = 0; i < stateMachine.states.Length; ++i) {
                    var state = stateMachine.states[i].state;
                    var uniqueStateName = UniqueName(stateMachine.states[i].state);
                    for (int j = 0; j < state.transitions.Length; ++j) {
                        GenerateTransition(uniqueStateName, state.transitions[j], true, false, generatedCode);
                        generatedCode.AppendLine();
                    }
                }
            }

            // Add all of the state machines.
            for (int i = 0; i < stateMachine.stateMachines.Length; ++i) {
                GenerateStateMachine(uniqueStateMachineName, stateMachine.stateMachines[i], generatedCode);
            }

            // Add the state machine defaults.
            generatedCode.AppendLine("\t\t\t// State Machine Defaults.");
            generatedCode.AppendLine("\t\t\t" + uniqueStateMachineName + ".anyStatePosition = " + Vector3String(stateMachine.anyStatePosition) + ";");
            generatedCode.AppendLine("\t\t\t" + uniqueStateMachineName + ".defaultState = " + UniqueName(stateMachine.defaultState) + ";");
            generatedCode.AppendLine("\t\t\t" + uniqueStateMachineName + ".entryPosition = " + Vector3String(stateMachine.entryPosition) + ";");
            generatedCode.AppendLine("\t\t\t" + uniqueStateMachineName + ".exitPosition = " + Vector3String(stateMachine.exitPosition) + ";");
            generatedCode.AppendLine("\t\t\t" + uniqueStateMachineName + ".parentStateMachinePosition = " + Vector3String(stateMachine.parentStateMachinePosition) + ";");
            generatedCode.AppendLine();
        }

        /// <summary>
        /// Generates the code to recreate the specified blend tree.
        /// </summary>
        /// <param name="blendTreeName">The name of the blend tree variable.</param>
        /// <param name="blendTree">The blend tree to generate the code of.</param>
        /// <param name="generatedCode">The final generated code.</param>
        /// <param name="indentLevel">The number of indentations for the generated code.</param>
        private static void GenerateBlendTree(string blendTreeName, BlendTree blendTree, StringBuilder generatedCode, int indentLevel)
        {
            var indentation = "";
            for (int i = 0; i < indentLevel; ++i) {
                indentation += "\t";
            }
            generatedCode.AppendLine(indentation + "var " + blendTreeName + " = new BlendTree();");
            generatedCode.AppendLine(indentation + "AssetDatabase.AddObjectToAsset(" + blendTreeName + ", animatorController);");
            generatedCode.AppendLine(indentation + blendTreeName + ".hideFlags = HideFlags.HideInHierarchy;");
            generatedCode.AppendLine(indentation + blendTreeName + ".blendParameter = \"" + blendTree.blendParameter + "\";");
            generatedCode.AppendLine(indentation + blendTreeName + ".blendParameterY = \"" + blendTree.blendParameterY + "\";");
            generatedCode.AppendLine(indentation + blendTreeName + ".blendType = BlendTreeType." + blendTree.blendType + ";");
            generatedCode.AppendLine(indentation + blendTreeName + ".maxThreshold = " + blendTree.maxThreshold + "f;");
            generatedCode.AppendLine(indentation + blendTreeName + ".minThreshold = " + blendTree.minThreshold + "f;");
            generatedCode.AppendLine(indentation + blendTreeName + ".name = \"" + blendTree.name + "\";");
            generatedCode.AppendLine(indentation + blendTreeName + ".useAutomaticThresholds = " + BoolString(blendTree.useAutomaticThresholds) + ";");
            for (int j = 0; j < blendTree.children.Length; ++j) {
                var childName = blendTreeName + "Child" + j;
                if (blendTree.children[j].motion == null) {
                    generatedCode.AppendLine(indentation + "var " + childName + " = new ChildMotion();");
                } else {
                    var name = UniqueName(blendTree.children[j].motion);
                    if (blendTree.children[j].motion is BlendTree) {
                        name = blendTreeName + name;
                        GenerateBlendTree(name, blendTree.children[j].motion as BlendTree, generatedCode, indentLevel + 1);
                    }
                    generatedCode.AppendLine(indentation + "var " + childName + "=  new ChildMotion();");
                    generatedCode.AppendLine(indentation + childName + ".motion = " + name + ";");
                }
                generatedCode.AppendLine(indentation + childName + ".cycleOffset = " + blendTree.children[j].cycleOffset + "f;");
                generatedCode.AppendLine(indentation + childName + ".directBlendParameter = \"" + blendTree.children[j].directBlendParameter + "\";");
                generatedCode.AppendLine(indentation + childName + ".mirror = " + BoolString(blendTree.children[j].mirror) + ";");
                generatedCode.AppendLine(indentation + childName + ".position = " + Vector2String(blendTree.children[j].position) + ";");
                generatedCode.AppendLine(indentation + childName + ".threshold = " + blendTree.children[j].threshold + "f;");
                generatedCode.AppendLine(indentation + childName + ".timeScale = " + blendTree.children[j].timeScale + "f;");
            }
            // The children are only updated when assigned initially. Ass a new array so the children references will be correct.
            generatedCode.AppendLine(indentation + blendTreeName + ".children = new ChildMotion[] {");
            for (int j = 0; j < blendTree.children.Length; ++j) {
                var childName = blendTreeName + "Child" + j;
                generatedCode.AppendLine(indentation + "\t" + childName + (j < blendTree.children.Length - 1 ? "," : ""));
            }
            generatedCode.AppendLine(indentation + "};");
        }

        /// <summary>
        /// Generates the code to recreate the specified transition.
        /// </summary>
        /// <param name="stateMachineName">The name of the state machine variable that the transition has been added to.</param>
        /// <param name="transition">The transition to generate the code of.</param>
        /// <param name="isStateTransition">Is the transition a state transition?</param>
        /// <param name="isAnyStateTransition">Is the transition an AnyState transition?</param>
        /// <param name="generatedCode">The final generated code.</param>
        private static void GenerateTransition(string stateMachineName, AnimatorTransitionBase transition, bool isStateTransition, bool isAnyStateTransition, StringBuilder generatedCode)
        {
            if (transition.destinationState == null && !transition.isExit) {
                return;
            }

            var transitionName = UniqueName(transition);
            if (isStateTransition || isAnyStateTransition) {
                if (isAnyStateTransition) {
                    generatedCode.AppendLine("\t\t\tvar " + transitionName + " = " + stateMachineName + ".AddAnyStateTransition(" + UniqueName(transition.destinationState) + ");");
                } else if (transition.isExit) {
                    generatedCode.AppendLine("\t\t\tvar " + transitionName + " = " + stateMachineName + ".AddExitTransition();");
                } else {
                    generatedCode.AppendLine("\t\t\tvar " + transitionName + " = " + stateMachineName + ".AddTransition(" + UniqueName(transition.destinationState) + ");");
                }
                var stateTransition = transition as AnimatorStateTransition;
                generatedCode.AppendLine("\t\t\t" + transitionName + ".canTransitionToSelf = " + BoolString(stateTransition.canTransitionToSelf) + ";");
                generatedCode.AppendLine("\t\t\t" + transitionName + ".duration = " + stateTransition.duration + "f;");
                generatedCode.AppendLine("\t\t\t" + transitionName + ".exitTime = " + stateTransition.exitTime + "f;");
                generatedCode.AppendLine("\t\t\t" + transitionName + ".hasExitTime = " + BoolString(stateTransition.hasExitTime) + ";");
                generatedCode.AppendLine("\t\t\t" + transitionName + ".hasFixedDuration = " + BoolString(stateTransition.hasFixedDuration) + ";");
                generatedCode.AppendLine("\t\t\t" + transitionName + ".interruptionSource = TransitionInterruptionSource." + stateTransition.interruptionSource + ";");
                generatedCode.AppendLine("\t\t\t" + transitionName + ".offset = " + stateTransition.offset + "f;");
                generatedCode.AppendLine("\t\t\t" + transitionName + ".orderedInterruption = " + BoolString(stateTransition.orderedInterruption) + ";");
            } else {
                generatedCode.AppendLine("\t\t\tvar " + transitionName + " = " + stateMachineName + ".AddEntryTransition(" + UniqueName(transition.destinationState) + ");");
            }
            generatedCode.AppendLine("\t\t\t" + transitionName + ".isExit = " + BoolString(transition.isExit) + ";");
            generatedCode.AppendLine("\t\t\t" + transitionName + ".mute = " + BoolString(transition.mute) + ";");
            generatedCode.AppendLine("\t\t\t" + transitionName + ".solo = " + BoolString(transition.solo) + ";");
            for (int i = 0; i < transition.conditions.Length; ++i) {
                generatedCode.AppendLine("\t\t\t" + transitionName + ".AddCondition(AnimatorConditionMode." + transition.conditions[i].mode.ToString() + ", " + 
                                                    transition.conditions[i].threshold + "f, \"" + transition.conditions[i].parameter + "\");");
            }
        }

        /// <summary>
        /// Adds the Ultimate Character Controller parameters to the animator controller.
        /// </summary>
        /// <param name="animatorController">The animator controller to add the parameters to.</param>
        public static void AddParameters(AnimatorController animatorController)
        {
            if (!HasParameter(animatorController, "HorizontalMovement")) {
                animatorController.AddParameter("HorizontalMovement", AnimatorControllerParameterType.Float);
            }
            if (!HasParameter(animatorController, "ForwardMovement")) {
                animatorController.AddParameter("ForwardMovement", AnimatorControllerParameterType.Float);
            }
            if (!HasParameter(animatorController, "Pitch")) {
                animatorController.AddParameter("Pitch", AnimatorControllerParameterType.Float);
            }
            if (!HasParameter(animatorController, "Yaw")) {
                animatorController.AddParameter("Yaw", AnimatorControllerParameterType.Float);
            }
            if (!HasParameter(animatorController, "Speed")) {
                animatorController.AddParameter("Speed", AnimatorControllerParameterType.Float);
            }
            if (!HasParameter(animatorController, "Height")) {
                animatorController.AddParameter("Height", AnimatorControllerParameterType.Float);
            }
            if (!HasParameter(animatorController, "Moving")) {
                animatorController.AddParameter("Moving", AnimatorControllerParameterType.Bool);
            }
            if (!HasParameter(animatorController, "Aiming")) {
                animatorController.AddParameter("Aiming", AnimatorControllerParameterType.Bool);
            }
            if (!HasParameter(animatorController, "MovementSetID")) {
                animatorController.AddParameter("MovementSetID", AnimatorControllerParameterType.Int);
            }
            if (!HasParameter(animatorController, "AbilityIndex")) {
                animatorController.AddParameter("AbilityIndex", AnimatorControllerParameterType.Int);
            }
            if (!HasParameter(animatorController, "AbilityChange")) {
                animatorController.AddParameter("AbilityChange", AnimatorControllerParameterType.Trigger);
            }
            if (!HasParameter(animatorController, "AbilityIntData")) {
                animatorController.AddParameter("AbilityIntData", AnimatorControllerParameterType.Int);
            }
            if (!HasParameter(animatorController, "AbilityFloatData")) {
                animatorController.AddParameter("AbilityFloatData", AnimatorControllerParameterType.Float);
            }
            for (int i = 0; i < 2; ++i) {
                var parameterName = string.Format("Slot{0}ItemID", i);
                if (!HasParameter(animatorController, parameterName)) {
                    animatorController.AddParameter(parameterName, AnimatorControllerParameterType.Int);
                }
                parameterName = string.Format("Slot{0}ItemStateIndex", i);
                if (!HasParameter(animatorController, parameterName)) {
                    animatorController.AddParameter(parameterName, AnimatorControllerParameterType.Int);
                }
                parameterName = string.Format("Slot{0}ItemStateIndexChange", i);
                if (!HasParameter(animatorController, parameterName)) {
                    animatorController.AddParameter(parameterName, AnimatorControllerParameterType.Trigger);
                }
                parameterName = string.Format("Slot{0}ItemSubstateIndex", i);
                if (!HasParameter(animatorController, parameterName)) {
                    animatorController.AddParameter(parameterName, AnimatorControllerParameterType.Int);
                }
            }
            if (!HasParameter(animatorController, "LegIndex")) {
                animatorController.AddParameter("LegIndex", AnimatorControllerParameterType.Float);
            }
        }

        /// <summary>
        /// Does the animator controller have the specified parameter?
        /// </summary>
        /// <param name="animatorController">The animator controller to determine if it has the specified parameter.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>True if the animator controller has the specified parameter?</returns>
        private static bool HasParameter(AnimatorController animatorController, string parameterName)
        {
            for (int i = 0; i < animatorController.parameters.Length; ++i) {
                if (animatorController.parameters[i].name == parameterName) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a unique name for the specified object.
        /// </summary>
        /// <param name="unityObject">The object to generate the unique name of.</param>
        /// <returns>The unique name for the specified object.</returns>
        private static string UniqueName(Object unityObject)
        {
            return "m_" + AlphanumericString(unityObject.name + unityObject.GetType().Name + unityObject.GetHashCode());
        }

        /// <summary>
        /// Returns the string version of a new Vector2.
        /// </summary>
        /// <param name="value">The value of the Vector2.</param>
        /// <returns>The string version of a new Vector2.</returns>
        private static string Vector2String(Vector2 value)
        {
            return "new Vector2(" + value.x + "f, " + value.y + "f)";
        }

        /// <summary>
        /// Returns the string version of a new Vector3.
        /// </summary>
        /// <param name="value">The value of the Vector3.</param>
        /// <returns>The string version of a new Vector3.</returns>
        private static string Vector3String(Vector3 value)
        {
            return "new Vector3(" + value.x + "f, " + value.y + "f, " + value.z + "f)";
        }

        /// <summary>
        /// Returns the string version of a new bool.
        /// </summary>
        /// <param name="value">The value of the bool.</param>
        /// <returns>The string version of a new bool.</returns>
        private static string BoolString(bool value)
        {
            return value ? "true" : "false";
        }

        /// <summary>
        /// Returns an alphanumeric string.
        /// </summary>
        /// <param name="value">The value of the string.</param>
        /// <returns>An alphanumeric string.</returns>
        private static string AlphanumericString(string value)
        {
            return Regex.Replace(value, "[^a-zA-Z0-9]+", "");
        }
        
        /// <summary>
        /// Returns the AnimationClip at the specified path with the specified name.
        /// </summary>
        /// <param name="path">The path of the AnimationClip.</param>
        /// <param name="name">The name of the AnimationClip.</param>
        /// <returns>The AnimationClip at the specified path with the specified name. Can be null.</returns>
        public static AnimationClip GetAnimationClip(string path, string name)
        {
            var animationClips = AssetDatabase.LoadAllAssetsAtPath(path);
            if (animationClips != null) {
                // The path may point to an asset with multiple AnimationClips within. Search through that asset for the AnimationClip with the specified name.
                for (int i = 0; i < animationClips.Length; i++) {
                    if (animationClips[i] is AnimationClip && animationClips[i].name == name) {
                        return animationClips[i] as AnimationClip;
                    }
                }
            }
            // No animation clips with the specified name were found.
            return null;
        }
    }
}
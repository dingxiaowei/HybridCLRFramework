﻿
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace CombatDesigner.EditorTool
{
    public class RootBehaviorNode : ChainBehaviorNode
    {
        /// <summary>
        /// Init the Node
        /// </summary>
        /// <param name="parentGraph"></param>
        public override void InitNode(ChainGraph parentGraph)
        {
            output = new NodeOutput();
            behavior = parentGraph.model.GetBehavior("Default");
            id = 0;
            priority = 0;

            outPortSkin = new GUIStyle("Button");
            outPortSkin.overflow.top = 10;

            nodeType = NodeType.RootBehaviorNode;
            nodeRect = new Rect(10, 10, 100, 100);
        }

#if UNITY_EDITOR

        /// <summary>
        ///  Draw Line GUI (Before Node GUI)
        /// </summary>
        public override void UpdateLineGUI()
        {
        }



        /// <summary>
        ///  Draw Node GUI
        /// </summary>
        /// <param name="e"></param>
        /// <param name="viewRect"></param>
        /// <param name="skin"></param>
        public override void UpdateNodeGUI(Event e, Rect viewRect, GUISkin skin)
        {
            if (parentGraph != null)
            {

                outPortSkin = new GUIStyle("Button");
                outPortSkin.overflow.top = 10;

                // output
                if (GUI.Button(new Rect(nodeRect.x + nodeRect.width / 4, nodeRect.y + nodeRect.height, nodeRect.width / 2, 16), "", outPortSkin))
                {
                    parentGraph.connectionRequest = true;
                    parentGraph.connectedNode = this;
                }

                // the gui difference between selected node and non-selected node
                if (!isSelected)
                {
                    skinName = IsMatchingRuntimeBehavior() ? "Node_Executing" : "RootNode";

                    GUI.Box(nodeRect, "", skin.GetStyle(skinName));
                }
                else
                {
                    skinName = IsMatchingRuntimeBehavior() ? "NodeSelected_Executing" : "RootNodeSelected";
                    GUI.Box(nodeRect, "", skin.GetStyle(skinName));
                }
            }

            // Process the Keyboard and mouse events
            ProcessEvents(e, viewRect);

            // Draw Node GUI
            NodeBodyGUI();

            // Mark Dirty to save the data in editor mode
            EditorUtility.SetDirty(this);
        }
#endif

        /// <summary>
        /// A method to draw root node body gui
        /// </summary>
        void NodeBodyGUI()
        {
            GUILayout.BeginArea(nodeRect);
            EditorGUI.LabelField(new Rect(nodeRect.width / 2 - 25, nodeRect.height / 2 - 40, 100, 40), "Default \n (Root)", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(nodeRect.width / 2 - 16, nodeRect.height / 2, 20, 16), "ID");
            EditorGUI.IntField(new Rect(nodeRect.width / 2, nodeRect.height / 2, 14, 16), id);
            GUILayout.EndArea();
        }

    }
}
#endif
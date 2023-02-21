/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Game
{
    using Dypsloom.DypThePenguin.Scripts.Damage;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// The nice penguin that the character should save.
    /// </summary>
    public class NicePenguin : MonoBehaviour
    {
        [Tooltip("The animator.")]
        [SerializeField] protected Animator m_Animator;
        [Tooltip("The damageables.")]
        [SerializeField] protected Damageable[] m_Damageables;
        [Tooltip("The text box.")]
        [SerializeField] protected GameObject m_TextBox;
        [Tooltip("The dialog text.")]
        [SerializeField] protected TextMeshProUGUI m_DialogText;

        protected int m_BrokenChainCount;
        private static readonly int s_Free = Animator.StringToHash("Free");

        /// <summary>
        /// Initialize components.
        /// </summary>
        private void Awake()
        {
            for (int i = 0; i < m_Damageables.Length; i++) { m_Damageables[i].OnDie += BrokeChain; }
            m_Animator.SetBool(s_Free,false);

            UpdateDialogText();
        }

        /// <summary>
        /// Broke a chain.
        /// </summary>
        private void BrokeChain()
        {
            m_BrokenChainCount++;

            if (m_BrokenChainCount == m_Damageables.Length) {
                UpdateDialogText();
                m_Animator.SetBool(s_Free,true);
                m_TextBox.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Update the dialog text.
        /// </summary>
        public void UpdateDialogText()
        {
            if (m_BrokenChainCount == 0) {

                if (GameManager.Instance.EnemyKillCount == 0) {
                    m_DialogText.text = "The bad penguins chained me here. Please defeat the bad penguins and break my chains";
                    return;
                } 
            
                if (GameManager.Instance.EnemyKillCount == 1) {
                    m_DialogText.text = "You defeated one penguin, please defeat the others!";
                    return;
                }
            
                if (GameManager.Instance.EnemyKillCount == 2) {
                    m_DialogText.text = "You almost defeated all the penguins!";
                    return;
                }
            
                if (GameManager.Instance.EnemyKillCount == 3) {
                    m_DialogText.text = "You defeated all the penguins! Please use the pick axe to destroy these chains";
                    return;
                }
            
            
                return;
            }
        
            if (m_BrokenChainCount == 1) {
                m_DialogText.text = "Almost there, break my other chain";
                return;
            }
        
            if (m_BrokenChainCount == 2) {
                m_DialogText.text = "I'm Free! Thank you! \n\n\nThank you for playing this demo, learn more about us at dysloom.com";
                return;
            }
        
            m_DialogText.text = "¿¿¿¿I don't have any dialog for this state????";
        }
    }
}

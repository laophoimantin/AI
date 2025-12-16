using Spells;
using UnityEngine;

namespace Wizardo
{
    /// <summary>
    /// Defines the AI personality archetype.
    /// Acts as a configuration file for the Agent.
    /// </summary>
    [CreateAssetMenu(menuName = "Personality Profile")]
    public class PersonalitySO : ScriptableObject
    {
        [Header("Profile")]
        public string Name;
        [TextArea(3, 5)] public string Description;
        
        [Header("Roleplay Biases")]
        [Tooltip("Multiplies score of OFFENSE spells.\nHow much the wizard values dealing damage.\n> 1.0 = Aggressive \n< 1.0 = Passive")]
        [Range(0f, 2f)] public float Aggression = 1.0f; 
    
        [Tooltip("Multiplies score of DEFENSE/HEAL spells.\nHow much the wizard values keeping their own HP high.\n> 1.0 = Cautious \n< 1.0 = Brave")]
        [Range(0f, 2f)] public float Caution = 1.0f;    

        [Tooltip("Multiplies score of UTILITY spells.\nHow much the wizard values using tactical spells \n> 1.0 = Tactical \n< 1.0 = Simple-minded")]
        [Range(0f, 2f)] public float Utility = 1.0f;
        
        [Header("Behavioral Traits")] // Makes the AI more unpredictable
        [Tooltip("Delusion level. Affects decisions for low-accuracy spells.\n< 0.3 = Snap back to reality \n~ 0.5 = A wizard who thinks all the time \n> 0.7 = Gambling blood flows in the body")]
        [Range(0f, 1f)] public float RiskTaking = 0.5f;

        [Tooltip("Affects the final spell selection.\n0.0 = Perfect Robot \n0.2 = Smart Human \n0.5 = Drunk Wizard \n>0.5 = The greatest gambler!")]
        [Range(0f, 1f)]  public float Randomness = 0.2f; // The higher the value, the more random the AI is when choosing final spells.
        
       
        // Returns the score multiplier for a specific spell type.
        public float GetModifierForType(SpellType type)
        {
            switch (type)
            {
                case SpellType.Offense: 
                    return Aggression;
                case SpellType.Defense: 
                    return Caution;
                case SpellType.Utility: 
                    return Utility;
                default: return 1.0f;
            }
        }
    }
}
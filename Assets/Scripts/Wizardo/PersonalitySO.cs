using UnityEngine;

namespace Wizardo
{
    [CreateAssetMenu(menuName = "AI/Personality Profile")]
    public class PersonalitySO : ScriptableObject
    {
        [Header("Roleplay Biases")]
        [Tooltip("Multiplies score of OFFENSE spells.\n> 1.0 = Aggressive (Berserker)\n< 1.0 = Passive")]
        public float Aggression = 1.0f; 
    
        [Tooltip("Multiplies score of DEFENSE/HEAL spells.\n> 1.0 = Cautious (Coward)\n< 1.0 = Brave")]
        public float Caution = 1.0f;    

        [Tooltip("Multiplies score of UTILITY/BUFF spells.\n> 1.0 = Tactical\n< 1.0 = Simple-minded")]
        public float Utility = 1.0f;

        [Header("Human Flaws")]
        [Range(0f, 1f)] 
        [Tooltip("How much the AI deviates from the 'Correct' math.\n0.0 = Perfect Robot\n0.2 = Smart Human\n0.5 = Drunk Wizard")]
        public float Randomness = 0.2f; 
    }
}
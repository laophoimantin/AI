using UnityEngine;

namespace Wizardo
{
    [CreateAssetMenu(menuName = "AI/Personality Profile")]
    public class PersonalitySO : ScriptableObject
    {
        [Header("Profile")]
        public string Name;
        public string Description;
        
        [Header("Roleplay Biases")]
        [Tooltip("How much the wizard values dealing damage. \nMultiplies score of OFFENSE spells.\n> 1.0 = Aggressive (Berserker) \n< 1.0 = Passive")]
        [Range(0f, 1f)] public float Aggression = 1.0f; 
    
        [Tooltip("How much the wizard values keeping their own HP high. \nMultiplies score of DEFENSE/HEAL spells.\n> 1.0 = Cautious (Coward) \n< 1.0 = Brave")]
        [Range(0f, 1f)] public float Caution = 1.0f;    

        [Tooltip("Multiplies score of UTILITY/BUFF spells.\n> 1.0 = Tactical \n< 1.0 = Simple-minded")]
        [Range(0f, 1f)] public float Utility = 1.0f;
        
        [Tooltip("DELUSION LEVEL! Affects decisions for low-accuracy spells.\n< 0.3 = Snap back to reality \n~ 0.5 = A wizard who thinks all the time \n> 0.7 = Gambling blood flows in the body")]
        [Range(0f, 1f)] public float RiskTaking = 0.5f;

        [Header("Human Flaws")]
        [Tooltip("Affects the final spell selection.\n0.0 = Perfect Robot \n0.2 = Smart Human \n0.5 = Drunk Wizard \n>0.5 = Da GREATEST GAMBLER IN THE WIZARDING WORLD!")]
        [Range(0f, 1f)]  public float Randomness = 0.2f; 
    }
}
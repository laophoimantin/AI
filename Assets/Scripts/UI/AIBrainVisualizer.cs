using Core;
using System.Text;
using TMPro;
using UnityEngine;
using Wizardo;

public class AIBrainVisualizer : MonoBehaviour
{
    [Header("Connections")]
    [SerializeField] private BattleManager _battleManager;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI _redBrainText;
    [SerializeField] private TextMeshProUGUI _blueBrainText;

    void Update()
    {
        if (_battleManager == null) return;

        UpdateBrainText(_battleManager.RedWizard, _redBrainText);
        UpdateBrainText(_battleManager.BlueWizard, _blueBrainText);
    }

    private void UpdateBrainText(Agent wizard, TextMeshProUGUI textBox)
    {
        if (wizard == null || textBox == null) return;

        StringBuilder sb = new StringBuilder();

        string nameColor = wizard.name.Contains("Red") ? "#FF5555" : "#5555FF";
        sb.AppendLine($"<color={nameColor}><b>=== {wizard.Name.ToUpper()}'s BRAIN ===</b></color>");

        if (wizard.Personality != null)
        {
            sb.AppendLine($"<size=80%><color=#AAAAAA>Personality: {wizard.Personality.Name}</color>");
            sb.AppendLine($"<color=#888888>Agg: {wizard.Personality.Aggression:F1} | Cau: {wizard.Personality.Caution:F1} | Uti: {wizard.Personality.Utility:F1} | Ran: {wizard.Personality.Randomness:F1}</color></size>");
        }
        sb.AppendLine("---------------------------------------");

        if (wizard.LastTurnData == null || wizard.LastTurnData.Count == 0)
        {
            sb.AppendLine("\n<color=grey><i>...Brain is empty / Waiting...</i></color>");
        }
        else
        {
            sb.AppendLine("SPELL         BASE  MOD  FINAL  WIN%");
            sb.AppendLine("---------------------------------------");

            foreach (var data in wizard.LastTurnData)
            {
                string colorTag = data.IsWinner ? "<color=#00FF00><b>" : "<color=#DDDDDD>";
                string endTag = data.IsWinner ? "</b></color>" : "</color>";
                string arrow = data.IsWinner ? "►" : " ";

                string spellName = data.SpellName.Length > 12 ? data.SpellName.Substring(0, 12) : data.SpellName.PadRight(12);

                if (data.IsCulled)
                {
                    spellName = $"<color=grey><s>{spellName}</s></color>";
                    colorTag = "<color=grey>";
                    endTag = "</color>";
                }
                string raw = data.RawScore.ToString("0").PadLeft(4);
                string mod = data.PersonalityMod.ToString("0.0").PadLeft(4);
                string final = data.FinalScore.ToString("0.0").PadLeft(5);
                string chance = (data.WinChance * 100).ToString("0").PadLeft(3) + "%";

                sb.AppendLine($"{colorTag}{arrow} {spellName} {raw} {mod} {final}  {chance}{endTag}");
            }

            sb.AppendLine("---------------------------------------");
            sb.AppendLine($"<size=80%><color=#FFFF00>Total Weight: {wizard.LastTotalWeight:F1}</color>");
            sb.AppendLine($"<color=#FF8800>Needle Landed: {wizard.LastWinningTicket:F1}</color></size>");

            sb.AppendLine("\n<size=70%><color=#888888>--- Explain ---");
            sb.AppendLine("BASE : Objective value of the spell (Math).");
            sb.AppendLine("MOD  : Personality Bias + Survival Instincts.");
            sb.AppendLine("FINAL: Base x Mod x Noise (Random Error).");
            sb.AppendLine("WIN% : Probability to be chosen this turn.</color></size>");
        }

        textBox.text = sb.ToString();
    }
}
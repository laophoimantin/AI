using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEditor;
using UnityEngine;
using Wizardo;
/// <summary>
/// Custom Editor for the BattleManager.
/// Displays the decision of each agent in the scene.
/// Useful for debugging purposes.
/// </summary>
[CustomEditor(typeof(BattleManager))]
public class BattleManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BattleManager manager = (BattleManager)target;
        
        // Assign the two wizards to display
        if (manager.RedWizard == null || manager.BlueWizard == null) return;
        List<Agent> wizardToDisplay = new List<Agent>
        {
            manager.RedWizard,
            manager.BlueWizard
        };
        
        
        GUILayout.Space(30);
        EditorGUILayout.LabelField("BATTLE VISUALIZER", EditorStyles.boldLabel);
        
        foreach (var wizard in wizardToDisplay)
        {
            // Display the wizard's info
            GUILayout.Space(20);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Agent: {wizard.name}", EditorStyles.boldLabel); // Agent name
            GUILayout.Space(5);
            EditorGUILayout.LabelField($"Personality: === {wizard.Personality.Name} ===", EditorStyles.boldLabel); // Personality name
            EditorGUILayout.LabelField($"Aggression:     {wizard.Personality.Aggression}", EditorStyles.label); // Aggression
            EditorGUILayout.LabelField($"Caution:           {wizard.Personality.Caution}", EditorStyles.label); // Caution
            EditorGUILayout.LabelField($"Utility:               {wizard.Personality.Utility}", EditorStyles.label); // Utility
            EditorGUILayout.LabelField($"RiskTaking:     {wizard.Personality.RiskTaking}", EditorStyles.label); // RiskTaking
            EditorGUILayout.LabelField($"Randomness:  {wizard.Personality.Randomness}", EditorStyles.label); // Randomness

            // No decision to display
            if (wizard.LastTurnData == null || wizard.LastTurnData.Count == 0)
            {
                EditorGUILayout.HelpBox("No agent activity detected.", MessageType.Info);
                EditorGUILayout.EndVertical(); 
                continue; // Skip to the next wizard
            }

            GUILayout.Space(5);

            // float maxScoreInList = 1f;
            // if (wizard.LastTurnData.Count > 0)
            // {
            //     maxScoreInList = wizard.LastTurnData.Max(x => x.FinalScore);
            //     if (maxScoreInList <= 0) maxScoreInList = 1f;
            // }

            // Header
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Spell Name", GUILayout.Width(120)); // Spell Name
            GUILayout.Label("Raw Score", GUILayout.Width(80)); // Raw Score (Score without modifiers)
            GUILayout.Label("Personality Mod", GUILayout.Width(40 + 70)); // Personality Mod (aggression, caution, utility)
            GUILayout.Label("Noise", GUILayout.Width(40 + 10)); // Noise (randomness of the agent)
            GUILayout.Label("Final Score", GUILayout.Width(100)); // Final Score
            GUILayout.Label("%", GUILayout.Width(40)); // Win Chance
            EditorGUILayout.EndHorizontal();

            foreach (var data in wizard.LastTurnData)
            {
                DrawSpellRow(data); //, maxScoreInList);
            }
            
            // SUMMARY BOX ======
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label($"Total Score/Weight: {wizard.LastTotalWeight:F1}"); // Total score of all spells
            GUILayout.Label($"Final Ticket: {wizard.LastWinningTicket:F1}"); // Final Ticket (random value that determines the winner)
            EditorGUILayout.EndVertical();
            // END SUMMARY BOX ======
            
            
            EditorGUILayout.EndVertical();

            if (Application.isPlaying) Repaint();
        }
    }

    void DrawSpellRow(SpellDecisionDebug data, float maxScore = 0f)
    {
        GUIStyle rowStyle = new GUIStyle(EditorStyles.helpBox);
        
        // Highlight the winner
        if (data.IsWinner) 
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f); 
        else 
            GUI.backgroundColor = Color.white;

        EditorGUILayout.BeginHorizontal(rowStyle);

        var labelStyle = data.IsWinner ? EditorStyles.boldLabel : EditorStyles.label;
        EditorGUILayout.LabelField(data.SpellName, labelStyle, GUILayout.Width(120 + 30)); // Spell Name
        EditorGUILayout.LabelField(data.RawScore.ToString("0"), GUILayout.Width(80)); // Raw Score
        EditorGUILayout.LabelField(data.PersonalityMod.ToString("0.0"), GUILayout.Width(40 + 50)); // Personality Mod
        EditorGUILayout.LabelField(data.Noise.ToString("0.00"), GUILayout.Width(60)); // Noise
        EditorGUILayout.LabelField(data.FinalScore.ToString("0.00"), GUILayout.Width(80)); // Final Score
        EditorGUILayout.LabelField((data.WinChance * 100).ToString("0") + "%", GUILayout.Width(40)); // Win Chance

        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
    }
}
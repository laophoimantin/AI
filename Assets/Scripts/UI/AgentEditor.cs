using System.Linq;
using UnityEditor;
using UnityEngine;
using Wizardo;

[CustomEditor(typeof(Agent))]
public class AgentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Agent wizard = (Agent)target;

        GUILayout.Space(20);
        EditorGUILayout.LabelField($"Agent: {wizard.name}", EditorStyles.boldLabel);

        if (wizard.LastTurnData == null || wizard.LastTurnData.Count == 0)
        {
            EditorGUILayout.HelpBox("No agent activity detected.", MessageType.Info);
            return;
        }

        // --- SUMMARY BOX ---
        GUIStyle summaryStyle = new GUIStyle(EditorStyles.helpBox);
        summaryStyle.padding = new RectOffset(10, 10, 10, 10);

        EditorGUILayout.BeginVertical(summaryStyle);
        GUILayout.Label($"Total Weight: {wizard.LastTotalWeight:F1}");
        GUILayout.Label($"Winning Ticket: {wizard.LastWinningTicket:F1}");
        EditorGUILayout.EndVertical();
        GUILayout.Space(5);
        // -------------------

        float maxScoreInList = 1f;
        if (wizard.LastTurnData.Count > 0)
        {
            maxScoreInList = wizard.LastTurnData.Max(x => x.FinalScore);
            if (maxScoreInList <= 0) maxScoreInList = 1f;
        }

        // Header
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Spell Name", GUILayout.Width(120));
        GUILayout.Label("Raw", GUILayout.Width(40));
        GUILayout.Label("Pers", GUILayout.Width(40));
        GUILayout.Label("Noise", GUILayout.Width(40));
        //GUILayout.Label("Final Score", GUILayout.ExpandWidth(true));
        GUILayout.Label("Final Score", GUILayout.Width(100));
        GUILayout.Label("%", GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();

        foreach (var data in wizard.LastTurnData)
        {
            DrawSpellRow(data, maxScoreInList);
        }

        if (Application.isPlaying) Repaint();
    }

    void DrawSpellRow(SpellDecisionDebug data, float maxScoreContext)
    {
        GUIStyle rowStyle = new GUIStyle(EditorStyles.helpBox);
        if (data.IsWinner)
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        else
            GUI.backgroundColor = Color.white;

        EditorGUILayout.BeginHorizontal(rowStyle);

        var labelStyle = data.IsWinner ? EditorStyles.boldLabel : EditorStyles.label;
        EditorGUILayout.LabelField(data.SpellName, labelStyle, GUILayout.Width(120));

        EditorGUILayout.LabelField(data.RawScore.ToString("0"), GUILayout.Width(40));
        EditorGUILayout.LabelField(data.PersonalityMod.ToString("0.0"), GUILayout.Width(40));
        EditorGUILayout.LabelField(data.Noise.ToString("0.00"), GUILayout.Width(40));

        // Rect r = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
        // // Calculate fill relative to the highest score in the list
        // float barValue = data.FinalScore / maxScoreContext;
        // EditorGUI.ProgressBar(r, barValue, data.FinalScore.ToString("0.0"));

        EditorGUILayout.LabelField(data.FinalScore.ToString("0.00"), GUILayout.Width(100));
        EditorGUILayout.LabelField((data.WinChance * 100).ToString("0") + "%", GUILayout.Width(40));

        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
    }
}
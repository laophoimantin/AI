#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class FuzzyVisualizer : EditorWindow
{
    [MenuItem("Tools/Wizardo Fuzzy Graph 😎")]
    public static void ShowWindow()
    {
        GetWindow<FuzzyVisualizer>("Fuzzy Graph");
    }

    // Các thông số đầu vào
    private int _graphType = 0;
    private float _min = 0.2f;
    private float _max = 0.6f;
    private float _currentInput = 0.4f;

    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("(FUZZY LOGIC)", EditorStyles.boldLabel);

        _graphType = GUILayout.Toolbar(_graphType, new string[] { "Grade Down", "Grade Up", "Triangle" });

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Info here", MessageType.Info);

        _min = EditorGUILayout.FloatField("(Min)", _min);
        _max = EditorGUILayout.FloatField("(Max)", _max);

        GUILayout.Space(10);
        _currentInput = EditorGUILayout.Slider("Test Input  Real", _currentInput, 0f, 1f);

        GUILayout.Space(20);
        DrawGraph(); 
    }

    private void DrawGraph()
    {
        Rect rect = GUILayoutUtility.GetRect(10, 200, GUILayout.ExpandWidth(true));

        EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));

        Handles.color = new Color(0.3f, 0.3f, 0.3f);
        Handles.DrawLine(new Vector3(rect.x, rect.yMax), new Vector3(rect.xMax, rect.yMax)); 
        Handles.DrawLine(new Vector3(rect.x, rect.yMax - rect.height / 2), new Vector3(rect.xMax, rect.yMax - rect.height / 2)); // Y=0.5 (Giữa)
        Handles.DrawLine(new Vector3(rect.x, rect.yMin), new Vector3(rect.xMax, rect.yMin));

        Handles.color = Color.cyan;
        Vector3[] points = new Vector3[100];
        for (int i = 0; i < 100; i++)
        {
            float xVal = i / 99f;
            float yVal = EvaluateMath(xVal);

            points[i] = new Vector3(rect.x + xVal * rect.width, rect.yMax - yVal * rect.height);
        }
        Handles.DrawAAPolyLine(4f, points);

        float currentY = EvaluateMath(_currentInput);
        Vector2 pointPos = new Vector2(rect.x + _currentInput * rect.width, rect.yMax - currentY * rect.height);

        Handles.color = new Color(1f, 0f, 0f, 0.4f);
        Handles.DrawLine(new Vector2(pointPos.x, rect.yMax), pointPos);
        Handles.DrawLine(new Vector2(rect.x, pointPos.y), pointPos); 

        Handles.color = Color.red;
        Handles.DrawSolidDisc(pointPos, Vector3.forward, 6f);

        GUILayout.Space(10);

        GUIStyle resultStyle = new GUIStyle(EditorStyles.boldLabel);
        resultStyle.normal.textColor = Color.green;
        resultStyle.fontSize = 16;
        EditorGUILayout.LabelField($"(Y): {currentY:F3}", resultStyle);
    }

    private float EvaluateMath(float x)
    {
        if (_graphType == 0) 
        {
            if (x <= _min) return 1f;
            if (x >= _max) return 0f;
            return 1f - ((x - _min) / (_max - _min));
        }
        else if (_graphType == 1) 
        {
            if (x <= _min) return 0f;
            if (x >= _max) return 1f;
            return (x - _min) / (_max - _min);
        }
        else 
        {
            float mid = (_min + _max) / 2f;
            if (x <= _min || x >= _max) return 0f;
            if (x <= mid) return (x - _min) / (mid - _min);
            return 1f - ((x - mid) / (_max - mid));
        }
    }
}
#endif
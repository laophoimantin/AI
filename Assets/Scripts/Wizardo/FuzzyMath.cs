using UnityEngine;

namespace Wizardo.AI
{
    public static class FuzzyMath
    {
        public static float GradeDown(float value, float min, float max)
        {
            if (value <= min) return 1.0f;
            if (value >= max) return 0.0f; 

            return 1.0f - ((value - min) / (max - min));
        }

        public static float GradeUp(float value, float min, float max)
        {
            if (value <= min) return 0.0f; 
            if (value >= max) return 1.0f; 

            return (value - min) / (max - min);
        }

        public static float Triangle(float value, float left, float center, float right)
        {
            if (value <= left || value >= right) return 0.0f;
            if (Mathf.Approximately(value, center)) return 1.0f;

            if (value < center)
                return (value - left) / (center - left);

            return (right - value) / (right - center);
        }


        public static float AND(float a, float b) => Mathf.Min(a, b);
        public static float AND(float a, float b, float c) => Mathf.Min(a, Mathf.Min(b, c));

        public static float OR(float a, float b) => Mathf.Max(a, b);
        public static float OR(float a, float b, float c) => Mathf.Max(a, Mathf.Max(b, c));


        public static float DefuzzifySugeno(float[] weights, float[] crispValues)
        {
            if (weights == null || crispValues == null) return 0f;
            if (weights.Length != crispValues.Length)
            {
                return 0f;
            }

            float numerator = 0f;
            float denominator = 0f;

            for (int i = 0; i < weights.Length; i++)
            {
                numerator += weights[i] * crispValues[i];
                denominator += weights[i];
            }

            if (denominator == 0f) return 0f;
                
            return numerator / denominator;
        }
    }
}
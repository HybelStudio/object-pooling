using System;
using UnityEditor;
using UnityEngine;

using static UnityEditor.EditorGUILayout;
using System.Linq;

namespace Hybel.EditorUtils
{
    public static class EditorGUILayoutExtras
    {
        public static void ScriptField<T>(T target) where T : UnityEngine.Object
        {
            MonoScript _script = null;

            if (target is MonoBehaviour)
                _script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

            if (target is ScriptableObject)
                _script = MonoScript.FromScriptableObject(target as ScriptableObject);

            using (new Scopes(new HorizontalScope(), new DisabledGUIScope()))
                _script = ObjectField("Script", _script, typeof(MonoScript), false) as MonoScript;
        }

        public static void LabeledPropertyField(SerializedProperty property, GUIContent leftLabel, GUIContent rightLabel, bool autoSpace = false, float offset = 11f) =>
            LabeledField(position => EditorGUI.PropertyField(position, property), leftLabel, rightLabel, autoSpace, offset);

        public static void LabeledPropertyField(SerializedProperty property, GUIContent label, GUIContent leftLabel, GUIContent rightLabel, bool autoSpace = false, float offset = 11f) =>
            LabeledField(position => EditorGUI.PropertyField(position, property, label), leftLabel, rightLabel, autoSpace, offset);

        public static void LabeledField(Action<Rect> fieldFunc, GUIContent leftLabel, GUIContent rightLabel, bool autoSpace = false, float offset = 11f)
        {
            var sliderRect = GetControlRect();
            fieldFunc(sliderRect);
            sliderRect.x += EditorGUIUtility.labelWidth + 3;
            sliderRect.width -= EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth + 8;

            var sliderLabels = new SliderLabels();
            sliderLabels.SetLabels(leftLabel, rightLabel);

            Color orgColor = GUI.color;
            GUI.color = GUI.color * new Color(1f, 1f, 1f, 0.5f);
            float labelHeight = Mathf.Max(EditorStyles.miniLabel.CalcHeight(sliderLabels.LeftLabel, 0), EditorStyles.miniLabel.CalcHeight(sliderLabels.RightLabel, 0));

            offset = Mathf.Max(offset, 0f);
            Rect labelRect = new Rect(sliderRect.x, sliderRect.y + (sliderRect.height + offset) / 2, sliderRect.width, labelHeight);
            DoTwoLabels(labelRect, sliderLabels.LeftLabel, sliderLabels.RightLabel, EditorStyles.miniLabel);
            GUI.color = orgColor;
            sliderLabels.SetLabels(null, null);

            if (autoSpace)
                Space(offset - 5);

            static void DoTwoLabels(Rect rect, GUIContent leftLabel, GUIContent rightLabel, GUIStyle labelStyle)
            {
                if (Event.current.type != EventType.Repaint)
                    return;

                TextAnchor oldAlignment = labelStyle.alignment;

                labelStyle.alignment = TextAnchor.UpperLeft;
                GUI.Label(rect, leftLabel, labelStyle);

                labelStyle.alignment = TextAnchor.UpperRight;
                GUI.Label(rect, rightLabel, labelStyle);

                labelStyle.alignment = oldAlignment;
            }
        }

        public static void HeaderLabel(string label, float spaceBefore = 4, float spaceAfter = 0)
        {
            Space(spaceBefore);
            LabelField(label, EditorStyles.boldLabel);
            Space(spaceAfter);
        }

        public static void SliderWithCenterLabel(SerializedProperty property, GUIContent label, GUIContent centerLabel, float leftValue, float rightValue)
        {
            Rect position = GetControlRect();
            EditorGUI.Slider(position, property, leftValue, rightValue, label);

            const float Y_OFFSET = 6f;
            var labelPosition = new Rect(position)
            {
                width = position.width - EditorGUIUtility.fieldWidth,
                y = position.y - Y_OFFSET,
            };

            EditorGUI.LabelField(labelPosition, centerLabel, EditorStyles.centeredGreyMiniLabel);
        }

        public static void IntSliderWithCenterLabel(SerializedProperty property, GUIContent label, GUIContent centerLabel, int leftValue, int rightValue)
        {
            Rect position = GetControlRect();
            EditorGUI.IntSlider(position, property, leftValue, rightValue, label);

            const float Y_OFFSET = 6f;
            var labelPosition = new Rect(position)
            {
                width = position.width - EditorGUIUtility.fieldWidth,
                y = position.y - Y_OFFSET,
            };

            EditorGUI.LabelField(labelPosition, centerLabel, EditorStyles.centeredGreyMiniLabel);
        }

        public delegate Rect LabelPositionFunc(Rect controlPosition);
        public delegate void PropertyFunc(Rect controlPosition, SerializedProperty property);

        public static void EvenlySpacedPropertiesFields(GUIContent label, float padding, params SerializedProperty[] properties) =>
            EvenlySpacedPropertiesFields(label, padding, 0f, (pos, prop) => EditorGUI.PropertyField(pos, prop, GUIContent.none), properties);

        public static void EvenlySpacedPropertiesFields(GUIContent label, float padding, float addedHeight, PropertyFunc propertyFunc, params SerializedProperty[] properties)
        {
            var totalPosition = GetControlRect();
            totalPosition.y += addedHeight / 2f;
            totalPosition = EditorGUI.PrefixLabel(totalPosition, label);
            totalPosition.y += addedHeight / 2f;

            int propCount = properties.Length;
            if (propCount == 0)
                return;

            bool moreThanOneProp = propCount > 1;
            float singlePropertyWidth = totalPosition.width / propCount - (padding * (propCount - 1f) / propCount);

            using (new FieldWidthChangeScope(32, moreThanOneProp))
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    var position = new Rect(totalPosition);
                    position.width = singlePropertyWidth;
                    position.x = totalPosition.x + singlePropertyWidth * i + padding * i;

                    SerializedProperty property = properties[i];
                    propertyFunc(position, property);
                }
            }

            Space(addedHeight);
        }

        public static void EvenlySpacedPropertiesFields(GUIContent label, float padding, bool displayMiniLabels, params SerializedProperty[] properties)
        {
            if (!displayMiniLabels)
            {
                EvenlySpacedPropertiesFields(label, padding, properties);
                return;
            }

            var propertyLabelPairs = properties.Select(property => new PropertyLabelPair(property, property.displayName)).ToArray();

            EvenlySpacedPropertiesFields(label, padding, propertyLabelPairs);
        }

        public static void EvenlySpacedPropertiesFields(GUIContent label, float padding, params PropertyLabelPair[] propertyLabelPairs) =>
            EvenlySpacedPropertiesFields(label, padding, null, 15f, propertyLabelPairs: propertyLabelPairs);

        public static void EvenlySpacedPropertiesFields(GUIContent label, float padding, LabelPositionFunc miniLabelPositionFunc, float labelYOffset = 5f, params PropertyLabelPair[] propertyLabelPairs)
        {
            if (miniLabelPositionFunc == null)
                miniLabelPositionFunc = DefaultLabelPositionFunc;

            EvenlySpacedPropertiesFields(label, padding, 10f, DefaultPropertyWithLabelFunc, propertyLabelPairs.Select(p => p.Property).ToArray());

            void DefaultPropertyWithLabelFunc(Rect position, SerializedProperty property)
            {
                GUIContent label = null;

                foreach (var propertyLabelPair in propertyLabelPairs)
                    if (propertyLabelPair.Property == property)
                        label = propertyLabelPair.Label;

                var labelPosition = miniLabelPositionFunc(position);
                EditorGUI.LabelField(labelPosition, label, EditorStyles.centeredGreyMiniLabel);
                EditorGUI.PropertyField(position, property, GUIContent.none);
            }

            Rect DefaultLabelPositionFunc(Rect position)
            {
                var labelPosition = new Rect(position);
                labelPosition.y -= labelYOffset + 10f;
                return labelPosition;
            }
        }

        public static void ConditionalHelpBox(bool condition, string message, MessageType messageType)
        {
            if (condition) HelpBox(message, messageType);
        }

        public static void ConditionalHelpBox(Func<bool> condition, string message, MessageType messageType)
        {
            if (condition()) HelpBox(message, messageType);
        }

        public static void SearchBar(ref string searchString, string emptySearchText = null)
        {
            using (new HorizontalScope(GUI.skin.FindStyle("Toolbar")))
            {
                var totalPosition = GetControlRect();
                Rect searchBarPosition = new Rect(totalPosition)
                {
                    width = totalPosition.width - 16f,
                };

                Rect cancelButtonPosition = new Rect(totalPosition)
                {
                    x = totalPosition.x + searchBarPosition.width + 4f,
                    width = 16f,
                };

                searchString = GUI.TextField(searchBarPosition, searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));

                if (GUI.Button(cancelButtonPosition, "", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    // Remove focus if cleared
                    searchString = "";
                    GUI.FocusControl(null);
                }
            }
        }

        private struct SliderLabels
        {
            public GUIContent LeftLabel;
            public GUIContent RightLabel;

            public void SetLabels(GUIContent leftLabel, GUIContent rightLabel)
            {
                LeftLabel = leftLabel;
                RightLabel = rightLabel;
            }

            public bool HasLabels() => LeftLabel != null && RightLabel != null;
        }

        public class PropertyLabelPair
        {
            public readonly SerializedProperty Property;
            public readonly GUIContent Label;

            public PropertyLabelPair(SerializedProperty proeprty, GUIContent label)
            {
                Property = proeprty;
                Label = label;
            }

            public PropertyLabelPair(SerializedProperty proeprty, string label) : this(proeprty, new GUIContent(label)) { }
        }

        public class FieldWidthChangeScope : IDisposable
        {
            private float _previousFieldWidth;

            public FieldWidthChangeScope(float fieldWidth) => StartFieldWidth(fieldWidth);

            public FieldWidthChangeScope(float fieldWidth, bool condition)
            {
                if (condition)
                    StartFieldWidth(fieldWidth);
            }

            private void StartFieldWidth(float fieldWidth)
            {
                _previousFieldWidth = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.fieldWidth = fieldWidth;
            }

            public void Dispose() => EditorGUIUtility.fieldWidth = _previousFieldWidth;
        }
    }
}

using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Entry))]
public class EntryPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Begin property
        EditorGUI.BeginProperty(position, label, property);

        // Draw foldout header
        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded, label);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            // Draw each property from the Entry class
            Rect propertyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight,
                position.width, EditorGUIUtility.singleLineHeight);

            // Draw all properties except progressionLevel and slowTypeSpeed first
            SerializedProperty nameProp = property.FindPropertyRelative("name");
            propertyRect.height = EditorGUI.GetPropertyHeight(nameProp);
            EditorGUI.PropertyField(propertyRect, nameProp);
            propertyRect.y += propertyRect.height + EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty outputProp = property.FindPropertyRelative("output");
            propertyRect.height = EditorGUI.GetPropertyHeight(outputProp);
            EditorGUI.PropertyField(propertyRect, outputProp);
            propertyRect.y += propertyRect.height + EditorGUIUtility.standardVerticalSpacing;

            // Draw progressionLevel with custom width and right alignment
            SerializedProperty progressionLevelProp = property.FindPropertyRelative("progressionLevel");
            propertyRect.height = EditorGUIUtility.singleLineHeight;

            float valueWidth = 50f;  // Width for the int field
            float rightMargin = 10f;  // Optional margin from the right edge

            EditorGUI.LabelField(new Rect(propertyRect.x, propertyRect.y, propertyRect.width, propertyRect.height),
                new GUIContent("Progression Level"));

            EditorGUI.BeginChangeCheck();
            int newValue = EditorGUI.IntField(
                new Rect(propertyRect.x + propertyRect.width - valueWidth - rightMargin, propertyRect.y, valueWidth, propertyRect.height),
                progressionLevelProp.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                progressionLevelProp.intValue = newValue;
            }

            propertyRect.y += propertyRect.height + EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty altOutputProp = property.FindPropertyRelative("altOutput");
            propertyRect.height = EditorGUI.GetPropertyHeight(altOutputProp);
            EditorGUI.PropertyField(propertyRect, altOutputProp);
            propertyRect.y += propertyRect.height + EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty entryTypeProp = property.FindPropertyRelative("entryType");
            propertyRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(propertyRect, entryTypeProp);
            propertyRect.y += propertyRect.height + EditorGUIUtility.standardVerticalSpacing;

            // Draw slowTypeSpeed only if entryType is SlowType, with right alignment
            if (entryTypeProp.enumValueIndex == (int)Entry.type.SlowType)
            {
                SerializedProperty slowTypeSpeedProp = property.FindPropertyRelative("slowTypeSpeed");
                propertyRect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(new Rect(propertyRect.x, propertyRect.y, propertyRect.width, propertyRect.height),
                    new GUIContent("Slow Type Speed"));

                EditorGUI.BeginChangeCheck();
                float newFloatValue = EditorGUI.FloatField(
                    new Rect(propertyRect.x + propertyRect.width - valueWidth - rightMargin, propertyRect.y, valueWidth, propertyRect.height),
                    slowTypeSpeedProp.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    slowTypeSpeedProp.floatValue = newFloatValue;
                }

                propertyRect.y += propertyRect.height + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded)
        {
            SerializedProperty nameProp = property.FindPropertyRelative("name");
            SerializedProperty outputProp = property.FindPropertyRelative("output");
            SerializedProperty progressionLevelProp = property.FindPropertyRelative("progressionLevel");
            SerializedProperty altOutputProp = property.FindPropertyRelative("altOutput");
            SerializedProperty entryTypeProp = property.FindPropertyRelative("entryType");
            SerializedProperty slowTypeSpeedProp = property.FindPropertyRelative("slowTypeSpeed");

            totalHeight += EditorGUI.GetPropertyHeight(nameProp) + EditorGUIUtility.standardVerticalSpacing;
            totalHeight += EditorGUI.GetPropertyHeight(outputProp) + EditorGUIUtility.standardVerticalSpacing;
            totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // progressionLevel
            totalHeight += EditorGUI.GetPropertyHeight(altOutputProp) + EditorGUIUtility.standardVerticalSpacing;
            totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // entryType

            if (entryTypeProp.enumValueIndex == (int)Entry.type.SlowType)
            {
                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // slowTypeSpeed
            }
        }

        return totalHeight;
    }
}
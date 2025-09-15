using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.fscigliano.AsyncInitialization.Editor
{
    [CustomPropertyDrawer(typeof(InitializationHandler.InitializationItem))]
    public class InitializationItemDrawer : PropertyDrawer
    {
        private ReorderableList dependenciesList;
        private float dependenciesHeight;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Draw a box around the entire item
            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
            
            position.x += 4;
            position.y += 4;
            position.width -= 8;
            position.height -= 8;
            
            float yOffset = 0;
            
            // Draw Component field with proper spacing
            var componentProperty = property.FindPropertyRelative("component");
            Rect componentRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(componentRect, componentProperty, new GUIContent("Component"));
            yOffset += EditorGUIUtility.singleLineHeight + 4;
            
            // Add space between component field and dependencies list
            yOffset += 6;
            
            // Draw Dependencies list
            var dependenciesProperty = property.FindPropertyRelative("dependencies");
            if (dependenciesList == null || dependenciesList.serializedProperty != dependenciesProperty)
            {
                CreateDependenciesList(dependenciesProperty);
            }
            
            if (dependenciesList != null)
            {
                Rect dependenciesRect = new Rect(position.x, position.y + yOffset, position.width, dependenciesHeight);
                dependenciesList.DoList(dependenciesRect);
                yOffset += dependenciesHeight + 4;
            }
            
            // Add space before isEnabled checkbox
            yOffset += 6;
            
            // Draw IsEnabled field
            var isEnabledProperty = property.FindPropertyRelative("isEnabled");
            Rect isEnabledRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(isEnabledRect, isEnabledProperty, new GUIContent("Is Enabled"));
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var dependenciesProperty = property.FindPropertyRelative("dependencies");
            if (dependenciesList == null || dependenciesList.serializedProperty != dependenciesProperty)
            {
                CreateDependenciesList(dependenciesProperty);
            }
            
            float height = 8; // Top and bottom padding
            height += EditorGUIUtility.singleLineHeight + 4; // Component field + spacing
            height += 6; // Space between component and dependencies
            height += dependenciesHeight + 4; // Dependencies list + spacing
            height += 6; // Space before isEnabled
            height += EditorGUIUtility.singleLineHeight; // IsEnabled field
            
            return height;
        }
        
        private void CreateDependenciesList(SerializedProperty dependenciesProperty)
        {
            if (dependenciesProperty == null) return;
            
            dependenciesList = new ReorderableList(dependenciesProperty.serializedObject, dependenciesProperty, true, true, true, true);
            
            dependenciesList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "Dependencies");
            };
            
            dependenciesList.drawElementCallback = (rect, index, active, focused) =>
            {
                if (index >= 0 && index < dependenciesProperty.arraySize)
                {
                    var element = dependenciesProperty.GetArrayElementAtIndex(index);
                    rect.height = EditorGUIUtility.singleLineHeight;
                    rect.y += 2;
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                }
            };
            
            dependenciesList.elementHeight = EditorGUIUtility.singleLineHeight + 4;
            
            dependenciesList.onAddCallback = (list) =>
            {
                dependenciesProperty.arraySize++;
                dependenciesProperty.serializedObject.ApplyModifiedProperties();
            };
            
            dependenciesList.onRemoveCallback = (list) =>
            {
                if (list.index >= 0 && list.index < dependenciesProperty.arraySize)
                {
                    dependenciesProperty.DeleteArrayElementAtIndex(list.index);
                    dependenciesProperty.serializedObject.ApplyModifiedProperties();
                }
            };
            
            // Calculate height
            dependenciesHeight = dependenciesList.GetHeight();
        }
    }
}
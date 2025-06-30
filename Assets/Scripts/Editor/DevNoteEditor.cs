using UnityEditor;
using UnityEngine;

// Utilise ce script pour expliquer tes systèmes aux prochains devs <3
[CustomEditor(typeof(DevNote))]
[CanEditMultipleObjects]
public class DevNoteEditor : Editor
{
    SerializedProperty messageProp;
    SerializedProperty typeProp;
    SerializedProperty showFieldsOnlyInDebugProp;

    private void OnEnable()
    {
        messageProp = serializedObject.FindProperty("message");
        typeProp = serializedObject.FindProperty("messageType");
        showFieldsOnlyInDebugProp = serializedObject.FindProperty("readOnly");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DevNote note = (DevNote)target;

        if (!string.IsNullOrEmpty(note.Message))
        {
            MessageType unityMessageType = MessageType.None;

            switch (note.Type)
            {
                case DevNote.MessageType.Info:
                    unityMessageType = MessageType.Info;
                    break;
                case DevNote.MessageType.Warning:
                    unityMessageType = MessageType.Warning;
                    break;
                case DevNote.MessageType.Error:
                    unityMessageType = MessageType.Error;
                    break;
            }

            EditorGUILayout.HelpBox(note.Message, unityMessageType);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(showFieldsOnlyInDebugProp);

        if (!note.ReadOnly)
        {
            EditorGUILayout.PropertyField(messageProp);
            EditorGUILayout.PropertyField(typeProp);
        }
        else
        {
            EditorGUILayout.HelpBox("Mode lecture seule.", MessageType.None);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
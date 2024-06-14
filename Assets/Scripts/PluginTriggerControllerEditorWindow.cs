using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PluginTriggerController))]
public class PluginTriggerControllerEditorWindow : Editor
{
	private PluginTriggerController controller;

	private void OnEnable()
	{
		controller = (PluginTriggerController)target;
	}

	public override void OnInspectorGUI()
	{
		if (Application.isPlaying)
			GUI.enabled = false;

		EditorGUILayout.Space();

		GUITitle();

		EditorGUILayout.Space();
		if (PluginController.Initialized == false)
		{
			// play is required at least once to instantiate the instances and load the profile from the path
			GUIModularTextField("Enter play mode once to finish plugin setup!");
		}
		else
			GUITriggers();

		EditorGUILayout.Space();

		var a = new SerializedObject(controller);
		EditorGUILayout.PropertyField(a.FindProperty("triggers"));

		if (GUI.changed)
			a.ApplyModifiedProperties();
	}

	private static void GUITitle()
	{
		var titleLabelStyle = new GUIStyle();
		titleLabelStyle.fontStyle = FontStyle.BoldAndItalic;
		titleLabelStyle.fontSize = 14;
		EditorGUILayout.LabelField("Plugin Trigger Controller", titleLabelStyle);
	}

	private void GUITriggers()
	{
		GUILayoutOption[] options = { GUILayout.MinWidth(28.0f), GUILayout.MaxWidth(140.0f) };
		var TriggerLabelStyle = new GUIStyle();
		TriggerLabelStyle.fontStyle = FontStyle.Bold;
		TriggerLabelStyle.fontSize = 12;

		EditorGUILayout.BeginVertical();
		foreach (var trigger in controller.triggers)
		{
			GUIModularTextField(EnumToTitle(trigger.triggerType.ToString()), TriggerLabelStyle);
			EditorGUILayout.BeginHorizontal();
			for (int i = 0; i < trigger.playerTypes.Count; i++)
			{
				EditorGUILayout.BeginVertical();
				GUIModularTextField(EnumToTitle(trigger.playerTypes[i].ToString()));
				trigger.modifications[i] = EditorGUILayout.FloatField(trigger.modifications[i], options);
				EditorGUILayout.EndVertical();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
	}

	private static void GUIModularTextField(string title, GUIStyle style = null)
	{
		var label = new GUIContent(title);

		if (style != null)
			EditorGUILayout.LabelField(title, style, GUILayout.Width(GUI.skin.label.CalcSize(label).x));
		else
			EditorGUILayout.LabelField(title, GUILayout.Width(GUI.skin.label.CalcSize(label).x));
	}

	private static string EnumToTitle(string name)
	{
		var title = name.ToLower();
		var split = title.Split('_');
		title = "";

		foreach (var i in split)
			title += char.ToUpper(i[0]) + i.Substring(1) + " ";

		return title;
	}
}

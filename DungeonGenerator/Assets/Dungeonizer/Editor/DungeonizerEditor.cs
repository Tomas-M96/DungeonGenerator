using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Dungeonizer))] 	
public class DungeonizerEditor : Editor {
	
	public override void OnInspectorGUI () {
		//Called whenever the inspector is drawn for this object.
		DrawDefaultInspector();
		//This draws the default screen.  You don't need this if you want
		//to start from scratch, but I use this when I'm just adding a button or
		//some small addition and don't feel like recreating the whole inspector.
		Dungeonizer realscript = (Dungeonizer)target;

		if(GUILayout.Button("Create Now")) {
			//add everthing the button would do.
			realscript.ClearOldDungeon(true);
			realscript.Generate();
			
		}
	}
}
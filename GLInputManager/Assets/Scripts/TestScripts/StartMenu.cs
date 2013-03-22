using UnityEngine;
using System.Collections;

public class StartMenu : MonoBehaviour
{
	
	bool displayMenu;
	
	void Update()
	{
		if( Input.GetKeyDown(KeyCode.Escape))
		{
			if(!NewCustomInputManager.self.isOpen()) NewCustomInputManager.self.OpenEditor();
			else NewCustomInputManager.self.CloseEditor();
		}
	}
	void OnGUI()
	{
		if( !displayMenu ) return;
		

		
		GUI.Button(new Rect( Screen.width/2 - 100, Screen.height/2 - 25, 100, 50), "InputManager");
		
		
	}
	
	
}

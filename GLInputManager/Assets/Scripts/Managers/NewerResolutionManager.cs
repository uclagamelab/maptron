using UnityEngine;
using System.Collections;

public class NewerResolutionManager : MonoBehaviour {

	//returns whether or not the window is open
	bool _open = false;
	
	
	
	//returns the standard resolution ratio for the resolution
	Vector2 GetStandardResolution( Resolution r )
	{
		
	}

	//draw the info about the current resolution
	void DrawResolutionInfo(Rect r)
	{
	
		GUILayout.BeginArea( r );
		{
			GUILayout.BeginVertical();
			{
				
				//draw title of the thing
				GUILayout.Label("TEST");
				
				//draw current resolution info
				GUILayout.Label( Screen.currentResolution.ToString() );
				
				
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndArea();
		
	}
	
	//draw the selector for the resolution
	void DrawResolutionSelector(Rect r)
	{
		GUILayout.BeginArea(r);
		{
			GUILayout.BeginScrollView(Vector2.zero);
			{
				
			}
			GUILayout.EndScrollView();
		}
		GUILayout.EndArea();		
	}
	
	//draw graphics info box
	void DrawGraphicsInfo(Rect r)
	{
		GUILayout.BeginArea( r );
		{
			GUILayout.BeginVertical();
			{
				//draw title of the thing
				GUILayout.Label("TEST");
				
				//draw current resolution info
				GUILayout.Label( QualitySettings.names[QualitySettings.GetQualityLevel()] );
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndArea();		
	}
	
	//draw graphics info box
	void DrawGraphicsSelector(Rect r)
	{
		GUILayout.BeginArea(r);
		{
			GUILayout.BeginScrollView(Vector2.zero);
			{
				
			}
			GUILayout.EndScrollView();
		}
		GUILayout.EndArea();		
	}
	
	//draw the full resolution editor
	void DrawResolutionEditor(Rect r )
	{
		GUI.BeginGroup( r );
		{
			Rect tempRect = r;
			r.width /= 2;
			DrawResolutionInfo( r );
			
			r.x += r.width;
			DrawResolutionSelector( r );
		}
		GUI.EndGroup();
	}
	
	//draw the full graphics editor
	void DrawGraphicsEditor(Rect r)
	{
		GUI.BeginGroup( r );
		{
			Rect tempRect = r;
			r.width /= 2;
			DrawGraphicsInfo( r );
			
			r.x += r.width;
			DrawGraphicsSelector( r );
		}
		GUI.EndGroup();
	}
	
	//revert the resolution and graphics state to their original setting
	void Revert()
	{}
	
	//open the editor
	void Open()
	{}
	
	//close the editor
	void Close()
	{}
	
	//save the settings and close the editor
	void SaveAndClose()
	{}
	
	//returns whether or not the editor is open
	bool IsOpen()
	{ return _open; }
	
	//draw the editor
	void OnGUI()
	{
	
		
		
		
	}
}

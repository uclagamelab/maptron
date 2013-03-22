using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(NewCustomInputManager))]

public class NewCustomInputManagerEditor : Editor {
	
	bool _defaultkeysFoldout = false;
	bool _settingsFoldout = false;
	bool _visualsFoldout = false;
	class ConfigFoldoutState
	{
		public bool allOut = false;
		public bool parameters = false;
		public bool mouseOut = false;
		public bool keyboardOut = false;
		public bool joyOut = false;
	}
	
	List<ConfigFoldoutState> _foldoutStates;
	
	void OnEnable()
	{
		_foldoutStates = new List<ConfigFoldoutState>();
	}
	
	//resize the list of foldout states
	void ResizeFoldOutStates( int num )
	{
		while( _foldoutStates.Count != num )
		{
			if(_foldoutStates.Count > num)
			{
				_foldoutStates.RemoveAt( _foldoutStates.Count - 1);
			}
			else if(_foldoutStates.Count < num)
			{
				ConfigFoldoutState cfs = new ConfigFoldoutState();
				_foldoutStates.Add( cfs );
			}
		}
		
		return;
	}
	
	//draw the inspector gui
	public override void OnInspectorGUI ()
	{
		
		NewCustomInputManager ncim = target as NewCustomInputManager;
		
		serializedObject.Update();
		EditorGUIUtility.LookLikeInspector();
		
		
		//create dropdown for default keys
		_defaultkeysFoldout = EditorGUILayout.Foldout( _defaultkeysFoldout, "Default Keys" );
		
		//DRAW DEFAULT KEYS ARRAY
		if( _defaultkeysFoldout )
		{
			EditorGUI.indentLevel ++;
			
			//get the size of the array
			SerializedProperty sp = serializedObject.FindProperty("defaultKeys");
			sp.arraySize = EditorGUILayout.IntField("Size", sp.arraySize); //size fields
			int size = sp.arraySize;
			
			//resize the array of states
			ResizeFoldOutStates( size );
			
			//next parameter
			sp.NextVisible(true);

			for( int i = 0 ; i < size ; i ++ )
			{
				sp.NextVisible(false);
				DrawConfig( sp.Copy() , i );
			}
			
			EditorGUI.indentLevel --;
		}
		
		//DRAW SETTINGS LIST
		_settingsFoldout = EditorGUILayout.Foldout( _settingsFoldout, "Settings" );
		
		if( _settingsFoldout )
		{
			EditorGUI.indentLevel++;
			GUILayout.Label("TEST");
			

			
			EditorGUI.indentLevel--;
		}
		
		//DRAW SKINNING LIST
		_visualsFoldout = EditorGUILayout.Foldout( _visualsFoldout, "Skin" );

		if( _visualsFoldout )
		{
			EditorGUI.indentLevel ++;
				ncim.TitleTexture = EditorGUILayout.ObjectField("Title Image", ncim.TitleTexture, typeof(Texture2D)) as Texture;
			
				EditorGUILayout.Space();
			
				ncim.DescriptionTexture = EditorGUILayout.ObjectField("Description Image", ncim.DescriptionTexture, typeof(Texture2D)) as Texture;
				ncim.MouseTexture = EditorGUILayout.ObjectField("Mouse Image", ncim.MouseTexture, typeof(Texture2D)) as Texture;
				ncim.JoystickTexture = EditorGUILayout.ObjectField("Joystick Image", ncim.JoystickTexture, typeof(Texture2D)) as Texture;
			
				EditorGUILayout.Space();
			
				ncim.OddButtonTexture = EditorGUILayout.ObjectField("Odd Row Button", ncim.OddButtonTexture, typeof(Texture2D)) as Texture;
				ncim.EvenButtonTexture = EditorGUILayout.ObjectField("Even Row Button", ncim.EvenButtonTexture, typeof(Texture2D)) as Texture;
				
				EditorGUILayout.Space();
			
				ncim.SliderTexture = EditorGUILayout.ObjectField( ncim.SliderTexture, typeof(Texture2D)) as Texture;
			
				ncim.BackgroundTexture = EditorGUILayout.ObjectField("Background Image", ncim.BackgroundTexture, typeof(Texture2D)) as Texture;
			EditorGUI.indentLevel --;
		}
		
		//DrawDefaultInspector();
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		// Saves the configuration
		if( GUILayout.Button("Save Config") )
		{
			( target as NewCustomInputManager ).SaveEditorConfig();
		}
		
		// loads the configuration
		if( GUILayout.Button("Load Config") )
		{
			( target as NewCustomInputManager ).LoadEditorConfig();
			EditorUtility.SetDirty( target );
		}
		
		serializedObject.ApplyModifiedProperties();
		
		return;
	}
	
	void DrawConfig( SerializedProperty sp , int num)
	{
		sp.NextVisible( true );
		
		_foldoutStates[ num ].allOut = EditorGUILayout.Foldout( _foldoutStates[ num ].allOut, sp.stringValue );
		
		if( !_foldoutStates[ num ].allOut ) return;
		
		EditorGUI.indentLevel +=1 ;
		
		bool isAxis, isBirDir;
		
		{
			 _foldoutStates[ num ].parameters = EditorGUILayout.Foldout( _foldoutStates[ num ].parameters, "Parameters" );
			EditorGUI.indentLevel ++;
			{				
				//NAME
				if(_foldoutStates[ num ].parameters)sp.stringValue = EditorGUILayout.TextField( sp.name, sp.stringValue );
				
				//DESCRIPTION
				sp.NextVisible( false );
				if(_foldoutStates[ num ].parameters) sp.stringValue = EditorGUILayout.TextField( "Description", sp.stringValue );
				
				//TAB
				sp.NextVisible( false );
				if(_foldoutStates[ num ].parameters) sp.stringValue = EditorGUILayout.TextField( "Tab" , sp.stringValue );
				
				//PLAYER NUMBER
				sp.NextVisible( false );
				if(_foldoutStates[ num ].parameters) sp.intValue = EditorGUILayout.IntField( "Player", sp.intValue );
				
				if(_foldoutStates[ num ].parameters) EditorGUILayout.Space();
				
				//IS AXIS
				sp.NextVisible( false );
				if(_foldoutStates[ num ].parameters) sp.boolValue = EditorGUILayout.Toggle( "Is An Axis", sp.boolValue );
				isAxis = sp.boolValue;
				
				EditorGUI.indentLevel +=1;
				{
					//GRAVITY
					sp.NextVisible( false );
					if(isAxis && _foldoutStates[ num ].parameters) sp.floatValue = EditorGUILayout.FloatField( sp.name, sp.floatValue );
					
					//SENSITIVITY
					sp.NextVisible( false );
					if(isAxis && _foldoutStates[ num ].parameters) sp.floatValue = EditorGUILayout.FloatField( sp.name, sp.floatValue );
				}
				EditorGUI.indentLevel -=1 ;
				
				
				//IS BIDIRECTIONAL
				if( isAxis )
				{		
					if(_foldoutStates[ num ].parameters) EditorGUILayout.Space();
	
					sp.NextVisible( false );
					if(_foldoutStates[ num ].parameters) sp.boolValue = EditorGUILayout.Toggle( "Is A Bidirectional Axis", sp.boolValue );
					isBirDir = sp.boolValue;
					
					EditorGUI.indentLevel +=1;
					{
						//GRAVITY
						sp.NextVisible( false );
						if(isBirDir && _foldoutStates[ num ].parameters) sp.stringValue = EditorGUILayout.TextField( sp.name, sp.stringValue );
						
						//SENSITIVITY
						sp.NextVisible( false );
						if(isBirDir && _foldoutStates[ num ].parameters) sp.enumValueIndex = EditorGUILayout.Popup( "Direction", sp.enumValueIndex, sp.enumNames);
					}
					EditorGUI.indentLevel -=1;
				}
				else
				{
						sp.NextVisible( false );
						sp.NextVisible( false );
						sp.NextVisible( false );
				}
				//if(_foldoutStates[ num ].parameters) EditorGUILayout.Space();
			}
			EditorGUI.indentLevel --;
			
			//KEYBOARD INPUT
			_foldoutStates[ num ].keyboardOut = EditorGUILayout.Foldout(_foldoutStates[ num ].keyboardOut, "Keyboard Input");
			EditorGUI.indentLevel +=1;
			if(_foldoutStates[ num ].keyboardOut){
				//KEYBOARD POS
				sp.NextVisible( false );
				sp.stringValue = EditorGUILayout.TextField("Key", sp.stringValue );
	
				//KEYBOARD NEG
				sp.NextVisible( false );
				if(isAxis) sp.stringValue = EditorGUILayout.TextField("Negative Key", sp.stringValue );
			}
			else
			{
				sp.NextVisible( false );
				sp.NextVisible( false );
			}
			EditorGUI.indentLevel -=1;
			
			//EditorGUILayout.Space();
			
			//MOUSE INPUT
			_foldoutStates[ num ].mouseOut = EditorGUILayout.Foldout(_foldoutStates[ num ].mouseOut, "Mouse Input");
			EditorGUI.indentLevel +=1;
			if(_foldoutStates[ num ].mouseOut ){
				//MOUSE POS
				sp.NextVisible( false );
				sp.stringValue = EditorGUILayout.TextField("Mouse Button", sp.stringValue );
	
				//MOUSE NEG
				sp.NextVisible( false );
				if(isAxis) sp.stringValue = EditorGUILayout.TextField("Negative Mouse Button", sp.stringValue );
			
				//MOUSE AXIS
				sp.NextVisible( false );
				if(isAxis) sp.enumValueIndex = EditorGUILayout.Popup("Mouse Axis", sp.enumValueIndex, sp.enumNames );
			}
			else
			{
				sp.NextVisible( false );
				sp.NextVisible( false );
				sp.NextVisible( false );
			}
			EditorGUI.indentLevel -=1;
			
			//EditorGUILayout.Space();
			
			//JOYSTICK INPUT
			_foldoutStates[ num ].joyOut = EditorGUILayout.Foldout(_foldoutStates[ num ].joyOut, "Joystick Input");
			EditorGUI.indentLevel +=1;
			if(_foldoutStates[ num ].joyOut ){
				//JOYSTICK POS
				sp.NextVisible( false );
				sp.stringValue = EditorGUILayout.TextField("Joystick Button", sp.stringValue );
	
				//JOYSTICK NEG
				sp.NextVisible( false );
				if(isAxis) sp.stringValue = EditorGUILayout.TextField("Negative Joystick Button", sp.stringValue );
			
				//JOYSTICK AXIS
				sp.NextVisible( false );
				if(isAxis) sp.enumValueIndex = EditorGUILayout.Popup("Joystick Axis", sp.enumValueIndex, sp.enumNames );
			}
			else
			{
				sp.NextVisible( false );
				sp.NextVisible( false );
				sp.NextVisible( false );
			}
			EditorGUI.indentLevel -=1;

		}
		EditorGUI.indentLevel -=1;

	}
	
	
}

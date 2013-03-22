using UnityEngine;
using System.Collections;

/***************************************************
 * Game Lab Input Manager
 * Set up the Key config files from the Unity Editor
 * The typed out key names must match what the KeyCode enumerations are called - with any added spacing or capitals being okay
 * any joypad key must be proceeded by "joystick", ie "joystick A"
 * 
 * If the config is an axis, meaning it has a decimal value between -1 and 1 (like a joystick),
 * an analog controller input may be selected along with keys that correspond to negative input (these are only used if "isAxis" is selected)
 * 
 * The Player Number is used for the key binding tab organization and controller association
 * 
 * Interface:
 * 		GetKey(keycode) (along with GetKeyUp and GetKeyDown)
 * 		GetKey(Player, JoyCode)
 * 		GetKey(ConfigName)
 * 		GetAxis(ConfigName)
 * 
 * 		OpenEditor()
 * 		CloseEditor()
 * 		GetReference()
 * 
 ***************************************************/

// allow dupe, display warning
// allow dupe, show popup
// remove dupe

// Allow dupe
// not allow duplicates, display error
// play error noise




//User must check if the window is open and disable background GUI elements themselves

//TODO:
// X allow user to change the check box and scrollbar
// implement errors and warnings
// X implement saving out the keys
// ? make controller manager joysticks allow for output of custom strings per button/key
// implement templates for the controller manager so the user has the ability to make their own
// X allow for analog inputs to be used as binary ones (joy up is a "button") **
// X make a special case for analog triggers -- when both are set opposite eachother on an axis, they are added -- should not be treated as one axis each
// > make menus navigable with controller

//DUPE CHECKING
// implement duplicates functionality
// sound effect if a redundent is found

// red if there is no key assigned
// dont allow people to save if there is a key missing
// warning when something is missing in both

// recommended settings for graphics/computer


//new feedback:
// - confirm dialogue for saving with duplicates
// - removing the allow dupes checkbox
// - bidirectional analog axes should be more clear -- always show four spots (including mouse)
// - dont trap the user in the joystick column while only using the mouse/keyboard
// - add option to colorize the fields
// - make tabs more readable -> add some kind of vertical divider
// X make the inspector window more collapseable and readable
// - add a title bar that says "all controls"

[RequireComponent (typeof(ControllerManager))]

//custom input manager to handle input from a variety of controllers and allow for keybinding
public class NewCustomInputManager : MonoBehaviour {
	
	//singleton variables
	static NewCustomInputManager _selfReference = null;
	public static NewCustomInputManager self
	{
		get{ return _selfReference; }
	}
	
	
	#region Enumerations
	
	/*** ENUMs For Filling Out Configs ***/
	enum MouseAxes {
		None = 0,
		MoveVertical,
		MoveHorizontal,
		ScrollWheel
	}
	
	public enum BiDir{
		Horizontal = 0,
		Vertical
	}
	#endregion
	
	#region KeyConfig
	/*** KeyConfig Class ***/
	//object that represents a key configuration
	[System.Serializable]
	class KeyConfig{
		
		//separate into TWO columns -- keyboard and mouse / joystick controls
		
		//remove alt keys and replace with joystick keys
		//allow for negative versions of both if it's an axis
		
		//player indicates where in the config manager this player is listed
		public string Name = ""; //name of the config - used to retrieve info
		public string Description = "";
		public string Tab = "";
		public int PlayerNum = 1; //player associated with this control (used for controller and config menu
		
		// is axis information
		public bool isAxis = false; //true if the config can have a value between -1 and 1
		public float Gravity = 1.0f;
		public float Sensitivity = 1.0f;
		
		// is bidirectional information
		public bool IsBidirectional = false;
		/* If the config is part of a bidirectional axis */
		public string BiDirectionalLinkName = "";
		public BiDir Direction = BiDir.Horizontal;
		
		/* Positive Buttons */
		public string Key = "";
		public string NegativeKey = "";

		public string MouseButton = "";
		public string NegativeMouseButton = "";
		public MouseAxes MouseAxis;

		public string JoystickButton = "";
		public string NegativeJoystickButton = "";
		public JoyAnalog JoystickAxis;

		

		

		
		/* Converted Values */
		[HideInInspector]
		public int _key = 0, _joyKey = 0, _negKey = 0, _negJoyKey = 0, _mouseKey = -1, _negMouseKey = -1;
		[HideInInspector]
		public KeyConfig _biDirLink = null;
		[HideInInspector]
		public int _joyKeyPlayer = 0, _negJoyKeyPlayer = 0, _joyAxisPlayer = 0;
		[HideInInspector]
		public float _value = 0.0f;
		
		//clones the config - does NOT clone link to the bidirectional axis partner
		public KeyConfig Clone(){
			KeyConfig k = new KeyConfig();
			
			k.PlayerNum = PlayerNum;
			k.Name = Name;
			k.Description = Description;
			k.Key = Key;
			k.isAxis = isAxis;
			k.NegativeKey = NegativeKey;
			k.Gravity = Gravity;
			k.Sensitivity = Sensitivity;
			k._value = _value;
			k._key = _key;
			k._negKey = _negKey;
			
			k._mouseKey = _mouseKey;
			k._negMouseKey = _negMouseKey;
			/* New */
 			k.JoystickButton = JoystickButton;
			
			k.MouseAxis = MouseAxis;
			k.JoystickAxis = JoystickAxis;
			
			k.IsBidirectional = IsBidirectional;
			k.BiDirectionalLinkName = BiDirectionalLinkName;
			k.Direction = Direction;
			
			/* Converted Values */
			k._key = _key;
			k._joyKey = _joyKey;
			k._negKey = _negKey;
			k._negJoyKey = _negJoyKey;
			
			k._joyKeyPlayer = _joyKeyPlayer;
			k._negJoyKeyPlayer = _negJoyKeyPlayer;
			k._joyAxisPlayer = _joyAxisPlayer;
			
			return k;
		}
		
	}
	#endregion
	
	#region Class Variables and HashTables
	/*** Key Config Containers ***/
	[SerializeField]
	KeyConfig[] defaultKeys; //array of default key config
	Hashtable _configs; // table of the current config being used
	Hashtable _changedConfigs; // editted configs while the editor is up
	
	Hashtable _keycodemap; // map of keycodes and joycodes - each code is converted to a string for the key
	
	Hashtable _bidirectionalDrawCheck; //saves whether or not a bi directional axis has already been drawn or not
	
	ArrayList _tabList;
	
	//Total amount of players allowed
	int _totalPlayers = 0;
	
	//references to self and controller manager
	ControllerManager CMRef = null;
	
	//this string is prepended to joystick values when added to the _keycodeMap
	const string joyMapAdd = "joystick";
	
	
	/*** Default Editor Images ***/
	Texture defaultMouseIcon;
	Texture defaultJoystickIcon;
	
	#endregion
	
	#region Unity Functions
	//fill in keycode map and config tables
	void Awake(){
		//print( " : " + DefaultKeys.Length );

		//destroys the manager if one already exists
		if(_selfReference != null){
			Destroy(this);
			return;
		}
		
		//Prepare self reference
		_selfReference = this;
		
		//add the keycodes and joystickcodes to a map for easy lookup
		_keycodemap = new Hashtable();
		foreach (KeyCode kc in KeyCode.GetValues(typeof(KeyCode))){
			//skip any built in joystick buttons
			if(kc >= KeyCode.JoystickButton0 && kc <= (KeyCode.JoystickButton0 + 20*5)-1)
			{
				continue;
			}
			if(!_keycodemap.Contains(kc.ToString().ToLower()))
			{
				_keycodemap.Add( kc.ToString().ToLower(), (int)kc);
			}
		}
		
		foreach (JoyCode jc in JoyCode.GetValues(typeof(JoyCode))){
			if(!_keycodemap.Contains(joyMapAdd+jc.ToString().ToLower()))
			{
				_keycodemap.Add( joyMapAdd+jc.ToString().ToLower(), (int)jc);
			}
		}
		//set the used configs to the default buttons
		ResetToDefault(out _configs);
		//load the configs from preferences
		
		//LoadConfig(ref _configs, SAVE_PREF_NAME);
		
		//get the total amount of players needed
		foreach(KeyConfig kc in defaultKeys){
			if(kc.PlayerNum > _totalPlayers) _totalPlayers = kc.PlayerNum;
		}
	}
	
	//get reference to controllermanager
	void Start(){
		CMRef = ControllerManager.GetReference();	
	}
	
	/*** Axis Updates ***/
	// Update any axis that are dependent on keys and not joysticks
	void Update () {
		
		//update the axis for each config
		foreach(DictionaryEntry de in _configs){
			KeyConfig kc = (KeyConfig)de.Value;
			
			//special case if two triggers are opposite each other -- then the joystick keys should not influence the value
			bool triggerSpecialCase = (kc._joyKey == (int)JoyCode.LeftTrigger || kc._joyKey == (int)JoyCode.RightTrigger) && (kc._negJoyKey == (int)JoyCode.LeftTrigger || kc._negJoyKey == (int)JoyCode.RightTrigger);
			
			//are the positive or negative keys pressed
			bool posKey = (GetKey( (KeyCode) kc._key)) || (GetKey(kc._joyKeyPlayer, kc._joyKey) && !triggerSpecialCase);
			bool negKey = (GetKey( (KeyCode) kc._negKey)) || (GetKey(kc._negJoyKeyPlayer, kc._negJoyKey) && !triggerSpecialCase);
			
			//move in the direction of the keys
			if( posKey) kc._value += kc.Sensitivity * .016f;
			if( negKey) kc._value -= kc.Sensitivity * .016f;
			
			kc._value = Mathf.Clamp(kc._value, -1, 1);
			
			//if no keys are pressed, ease them down to 0.0f and clamp to it
			if( !posKey && !negKey && kc._value != 0f ){	
				bool pos = kc._value > 0;
				kc._value += (pos?-1:1) * kc.Gravity * .016f;
				kc._value = Mathf.Clamp(kc._value, pos?0f:-1f, pos?1f:0f);
			}
		}
				
		DoEditKeyWork();
		DoNavigationWork();
	}
	#endregion
	
	#region Key State checks
	/*** Get Key Functions ***/
	//returns true if the passed key is pressed, false otherwise -- "player" is used for joypad keys
	bool GetKeyUp( int player, int key )
	{
		if(key < 0){
			if(CMRef.GetController(player) != null) return CMRef.GetController(player).GetButtonUp((JoyCode)key);
			else return false;
		}else{
			return Input.GetKeyUp((KeyCode)key);
		}
	}
	bool GetKeyDown( int player, int key )
	{ 
		if(key < 0){
			if(CMRef.GetController(player) != null) return CMRef.GetController(player).GetButtonDown((JoyCode)key);
			else return false;
		}else{
			return Input.GetKeyDown((KeyCode)key);
		}	
	}
	bool GetKey( int player, int key )
	{
		if(key < 0){
			if(CMRef.GetController(player) != null) return CMRef.GetController(player).GetButton((JoyCode)key);
			else return false;
		}else{
			return Input.GetKey((KeyCode)key);
		}
	}
		
	//returns true if the keys in the pass config are pressed, false otherwise
	public bool GetKeyUp( string name ){
		if( !_configs.Contains(name)) throw new UnityException("Axis " + name + " is not set up");
		KeyConfig kc = (KeyConfig)_configs[name];
				
		return GetKeyUp( (KeyCode) kc._key) || GetKeyUp( (KeyCode)kc._negKey) || GetKeyUp(kc._joyKeyPlayer, kc._joyKey) || GetKeyUp(kc._negJoyKeyPlayer, kc._negJoyKey);
	}
	public bool GetKeyDown(string name){
		if( !_configs.Contains(name)) throw new UnityException("Axis " + name + " is not set up");
		KeyConfig kc = (KeyConfig)_configs[name];
						
		return GetKeyDown((KeyCode) kc._key) || GetKeyDown((KeyCode) kc._negKey) || GetKeyDown(kc._joyKeyPlayer, kc._joyKey) || GetKeyDown(kc._negJoyKeyPlayer, kc._negJoyKey);
		
	}
	public bool GetKey( string name ){
		if( !_configs.Contains(name)) throw new UnityException("Axis " + name + " is not set up");
		KeyConfig kc = (KeyConfig)_configs[name];
				
		return GetKey((KeyCode) kc._key) || GetKey((KeyCode) kc._negKey) || GetKey(kc._joyKeyPlayer, kc._joyKey) || GetKey(kc._negJoyKeyPlayer, kc._negJoyKey);
	}
	
	//returns true if the keycode key is down
	public bool GetKeyUp(KeyCode kc){ return Input.GetKeyUp(kc); }
	public bool GetKeyDown(KeyCode kc){ return Input.GetKeyDown(kc); }
	public bool GetKey(KeyCode kc){ return Input.GetKey(kc); }
	
	//returns true if the joycode button is down on the controller
	public bool GetKeyUp(int player, JoyCode jc){ return GetKeyUp(player, (int)jc); }
	public bool GetKeyDown(int player, JoyCode jc){ return GetKeyDown(player, (int)jc); }
	public bool GetKey(int player, JoyCode jc){ return GetKey(player, (int)jc); }
	
	
	
	/*** Analog Axis Values ***/
	//returns the value for the axis name passed
	public float GetAxis( string name ){
		if( !_configs.Contains(name)) throw new UnityException("An axis named " + name + " is not set up");
				
		KeyConfig kc = (KeyConfig)_configs[name];
		if(!kc.isAxis) return (GetKey(name))?1f:0f;
		
		//get the value associated with the mouse axis
		float mouseVal = 0.0f;
		switch(kc.MouseAxis){
		case MouseAxes.MoveHorizontal:
			mouseVal = Input.GetAxis("MouseX");
			break;
		case MouseAxes.MoveVertical:
			Input.GetAxis("MouseY");
			break;
		case MouseAxes.ScrollWheel:
			Input.GetAxis("MouseScrollWheel");
			break;
		}
		
		float controllerVal = (CMRef.GetController(kc._joyAxisPlayer) != null)? CMRef.GetController(kc._joyAxisPlayer).GetAnalogValue(kc.JoystickAxis) : 0;
		
		//if the left and right trigger are opposite of eachother, add them
		if( (kc._negJoyKey == (int)JoyCode.LeftTrigger || kc._negJoyKey == (int)JoyCode.RightTrigger) &&  (kc._joyKey == (int)JoyCode.LeftTrigger || kc._joyKey == (int)JoyCode.RightTrigger) )
		{
			JoyAnalog negKey = (kc._negJoyKey == (int)JoyCode.LeftTrigger) ? JoyAnalog.LeftTrigger : JoyAnalog.RightTrigger;
			JoyAnalog posKey = (kc._joyKey == (int)JoyCode.LeftTrigger) ? JoyAnalog.LeftTrigger : JoyAnalog.RightTrigger;

			controllerVal = 
				Mathf.Abs( 
					CMRef.GetController(kc._joyKeyPlayer).GetAnalogValue(posKey)
				)
				- 
				Mathf.Abs(
					CMRef.GetController(kc._negJoyKeyPlayer).GetAnalogValue(negKey)
				);
				
		}
		//add the various axis values together
		return Mathf.Clamp( kc._value + controllerVal, -1.0f , 1.0f ) + mouseVal;
	}
	public bool IsAxis( string name ){
		return _configs.Contains(name);
	}
	
	
	/*** Mouse Functions ***/
	public bool GetMouseButton(int button){ return Input.GetMouseButton(button); }
	public bool GetMouseButtonDown(int button){ return Input.GetMouseButtonDown(button); }
	public bool GetMouseButtonUp(int button){ return Input.GetMouseButtonUp(button); }
	
	public Vector2 GetMousePosition(){ return Input.mousePosition; }
	#endregion
	
	#region Key Table manipulation
	/*** Reset Keys to Default ***/
	//resets the config to the default configuration and saves it
	//cfg is a hash table for the configuration - it will be overwritten with the default config files
	void ResetToDefault(out Hashtable cfg){
		
		//add the configs to a map for easy lookup
		cfg = new Hashtable(defaultKeys.Length);
		_bidirectionalDrawCheck = new Hashtable();
		_tabList = new ArrayList();
		
		foreach (KeyConfig kc in defaultKeys){
			/*** Convert Keys to ints for easy use ***/
						
			//check if the default key is valid
			string keystring = kc.Key.ToString().Replace(" ","").ToLower(); // strip the key of any spaces
			if(keystring == "") keystring = "none"; // if it is left empty, set it to no key
			if(!_keycodemap.Contains(keystring)) throw new UnityException("Key " + kc.Key + " is not valid"); // throw an error if it can't find the key
			else kc._key = (int)_keycodemap[keystring]; //otherwise, find the code associated with it
			
			//check if the default negative key is valid
			keystring = kc.NegativeKey.ToString().Replace(" ","").ToLower();
			if(keystring == "") keystring = "none";
			if(!_keycodemap.Contains(keystring)) throw new UnityException("Key " + kc.NegativeKey + " is not valid");
			else kc._negKey = (int)_keycodemap[keystring];

			//check if the joystick button is valid
			keystring = kc.JoystickButton.ToString().Replace(" ","").ToLower();
			if(keystring == "") keystring = "none";
			else keystring = joyMapAdd + keystring;
			if(!_keycodemap.Contains(keystring))	throw new UnityException("Key " + kc.JoystickButton + " is not valid");
			else kc._joyKey = (int)_keycodemap[keystring];			
			
			//check if the negative joystick button is valid
			keystring = kc.NegativeJoystickButton.ToString().Replace(" ","").ToLower();
			if(keystring == "") keystring = "none";
			else keystring = joyMapAdd + keystring;
			if(!_keycodemap.Contains(keystring))	throw new UnityException("Key " + kc.NegativeJoystickButton + " is not valid");
			else kc._negJoyKey = (int)_keycodemap[keystring];	
			
			//set the positive mouse button
			keystring = kc.MouseButton.ToString().Replace(" ","").ToLower();
			if(keystring == "left" || keystring == "0") kc._mouseKey = 0;
			else if(keystring == "middle" || keystring == "2") kc._mouseKey = 2;
			else if(keystring == "right" || keystring == "1") kc._mouseKey = 1;
			else kc._mouseKey = -1;
			
			//set the positive mouse button
			keystring = kc.NegativeMouseButton.ToString().Replace(" ","").ToLower();
			if(keystring == "left" || keystring == "0") kc._negMouseKey = 0;
			else if(keystring == "middle" || keystring == "2") kc._negMouseKey = 2;
			else if(keystring == "right" || keystring == "1") kc._negMouseKey = 1;
			else kc._negMouseKey = -1;
			
			//Make sure that the joy keys cannot be in keyboard keys and vice versa (Joycode keys are always negative whereas KeyCode keys are positive)
			kc._key = Mathf.Max(kc._key, 0);
			kc._negKey = Mathf.Max(kc._negKey, 0);
			kc._joyKey = Mathf.Min(kc._joyKey, 0);
			kc._negJoyKey = Mathf.Min(kc._negJoyKey, 0);
			
			//assign the player number to all individual keys
			kc.PlayerNum = Mathf.Clamp( kc.PlayerNum, 1 , 10 );
			kc._joyKeyPlayer = kc.PlayerNum;
			kc._negJoyKeyPlayer = kc.PlayerNum;
			kc._joyAxisPlayer = kc.PlayerNum;
			
			//add to the configs
			if(!cfg.Contains(kc.Name)) cfg.Add( kc.Name , kc );
			if(kc.isAxis && kc.IsBidirectional) _bidirectionalDrawCheck.Add(kc.Name , true);
			
			//Save the tabs
			if( !_tabList.Contains(kc.Tab) ) _tabList.Add(kc.Tab);
		}
		SetupBidirectionalLinks(cfg);	
	}
	
	
	/*** Set Up Bidirectional Links Between Configs ***/
	//relinks the bidirectional input axes for the passed configs
	void SetupBidirectionalLinks(Hashtable cfg){
		//cycle through all keys and look for bidirectional ones to complete the links properly
		foreach(DictionaryEntry e in cfg){
			KeyConfig kc = e.Value as KeyConfig;
			kc._biDirLink = null; //so if the same table is setup twice, no errors are thrown
			
			//check if it is a bidirectional axis
			if(!kc.isAxis || !kc.IsBidirectional) continue;
			//throw error if a link name is not setup
			if(kc.BiDirectionalLinkName.Replace(" ", "") == "") throw new UnityException("Expected BiDirectional Axis Link Name for Config " + kc.Name);
			
			
			//cycle through each other dictionary item to find the matching config
			foreach(DictionaryEntry b in cfg){
				KeyConfig lc = b.Value as KeyConfig;
				if(lc == kc) continue;
				if(!lc.isAxis || !lc.IsBidirectional) continue;
			
				if(kc.BiDirectionalLinkName.Replace(" ", "") != lc.BiDirectionalLinkName.Replace(" ", ""))  continue;
				
				//if a link has already been found for this config
				if(kc._biDirLink != null) throw new UnityException("Found too many configs with BiDirectional Axis Link Name " + kc.BiDirectionalLinkName);
				//or if they found link has the same direction as the one listed
				if(kc.Direction == lc.Direction) throw new UnityException("Linked configs with link name " + kc.BiDirectionalLinkName + " cannot have the same axis direction");
								
				kc._biDirLink = lc;
			}
			
		}
	}
	#endregion
	
	#region Editor open and close functions and variables
	/*** Open and Close the Editor ***/
	bool _editorOpen = false;
	int viewPlayerPage = 1;
	
	//frame that the editor was opened on
	int _openedFrame = 0;
	
	//open the editor window
	public bool OpenEditor(){
		if(_editorOpen) return false;
	
		_editorOpen = true;
		
		//copy over the current config to the displayed one for a temp
		_changedConfigs = new Hashtable();
		foreach(DictionaryEntry de in _configs){
			KeyConfig kc = (KeyConfig)de.Value;
			_changedConfigs.Add(kc.Name, kc.Clone());
		}
		SetupBidirectionalLinks(_changedConfigs);
		
		_errorText = "";
				
		_openedFrame = Time.frameCount;
		
		return true;
	}
	public bool isOpen(){return _editorOpen;}
	
	//closes the Editor
	public bool CloseEditor(){
		if(!_editorOpen) return false;
		
		_editorOpen = false;
		_changedConfigs = null;
		_editting = null;
		_scrollPos = Vector2.zero;
		viewPlayerPage = 1;
		
		return true;
	}
	#endregion
	
	#region Config Saving
	/********* Manage Saved Configs ***********/
	const char CONFIG_SEPERATOR = '|';
	const char KEY_SEPERATOR = '!';
	const string SAVE_PREF_NAME = "GLIM_SaveInput";
	const string SAVE_EDIT_PREF_NAME = "GLIM_SaveInput_Editor";
	
	
	
	//copy the changes to the config hashtable an save keys to player preferences
	bool SaveChanges(){
		if(_changedConfigs == null || !_editorOpen)return false;
		
		//save the changed configs to the configs
		_configs = new Hashtable();
		foreach(DictionaryEntry de in _changedConfigs){
			KeyConfig kc = (KeyConfig)de.Value;
			_configs.Add(kc.Name, kc.Clone());
		}
		SetupBidirectionalLinks(_configs);
		
		return true;
	}
	
	//save out the configs to player preferences 
	bool SaveOutConfig( ref Hashtable cfg, string name )
	{
			
		string s = "";
		
		//save the current configs to load again later
		foreach(DictionaryEntry de in cfg)
		{
			KeyConfig kc = de.Value as KeyConfig;
			
			s += kc.Name + KEY_SEPERATOR; 				// 0 : name
			s += kc._key.ToString() + KEY_SEPERATOR; 				// 1 : key
			s += kc._mouseKey.ToString() + KEY_SEPERATOR; 			// 2 : mouse button
			s += kc._joyKey.ToString() + KEY_SEPERATOR; 			// 3 : joystick button
			s += kc._joyKeyPlayer.ToString() + KEY_SEPERATOR; 		// 4 : joystick button player
			
			s += kc._negKey.ToString() + KEY_SEPERATOR; 			// 5 : negative key
			s += kc._negMouseKey.ToString() + KEY_SEPERATOR; 		// 6 : negative mouse
			s += kc._negJoyKey.ToString() + KEY_SEPERATOR; 		// 7 : negative joystick button
			s += kc._negJoyKeyPlayer.ToString() + KEY_SEPERATOR;	// 8 : negative joystick player
				
			s += ( (int) kc.MouseAxis).ToString() + KEY_SEPERATOR; 		// 9 : mouse axis
			s += ( (int) kc.JoystickAxis ).ToString() + KEY_SEPERATOR; 	// 10: joystick axis
			s += ( (int) kc._joyAxisPlayer).ToString();	// 11: joystick axis player
			
			s += CONFIG_SEPERATOR;
		}
		
		//PlayerPrefs.SetString(name, s);
		
		return true;
	}
	
	//loads the set keys from the last play from player preferences
	bool LoadConfig( ref Hashtable cfg, string name ){	
		//check if preferences have been saved before
		if(!PlayerPrefs.HasKey( name )) return false;
		
		string setupstring = "";
		
		setupstring = PlayerPrefs.GetString(name);
		
		string[] configsetups = setupstring.Split( CONFIG_SEPERATOR );
		
		//cycle through each key config string
		foreach( string s in configsetups )
		{
			if( s == "" ) continue;
			
			// split the keyconfig string and save out each part
			string[] keys = s.Split(KEY_SEPERATOR);
		
			if( keys.Length != 12 )
			{
				Debug.LogWarning( "KeyConfig did not have expected number of partitions, skipping" );
				continue;
			}
			
			KeyConfig kc = cfg[ keys[0] ] as KeyConfig;	// 0 : name
			
			kc._key = int.Parse( keys[1] );				// 1 : key
			kc._mouseKey = int.Parse( keys[2] );		// 2 : mouse button
			kc._joyKey = int.Parse( keys[3] );			// 3 : joystick button
			kc._joyKeyPlayer = int.Parse( keys[4] ); 	// 4 : joystick button player
		
			kc._negKey = int.Parse( keys[5] );			// 5 : negative key
			kc._negMouseKey = int.Parse( keys[6] );		// 6 : negative mosue key
			kc._negJoyKey = int.Parse( keys[7] );		// 7 : negative joystick button
			kc._negJoyKeyPlayer = int.Parse( keys[8] ); // 8 : negative joystick player
			
			kc.MouseAxis = (MouseAxes) int.Parse( keys[9] );		// 9 : mouse axis
			kc.JoystickAxis = (JoyAnalog) int.Parse( keys[10] );	// 10: joystick axis
			kc._joyAxisPlayer = int.Parse( keys[11] );				// 11: joystick axis player
		}
		SetupBidirectionalLinks(cfg);
		
		return true;
	}
	
	//delete all the saved keys for this config
	public void DeleteSavedConfig(string name){
		PlayerPrefs.DeleteKey( name );
	}
	
	//saves out the config for what has been saved in the editor ONLY
	public bool SaveEditorConfig()
	{
		string s = "";
		
		foreach( KeyConfig kc in defaultKeys )
		{
			s += kc.Name + KEY_SEPERATOR;						// 0 : name
			
			s += kc.PlayerNum.ToString() + KEY_SEPERATOR;		// 1 : player number
			s += kc.Tab + KEY_SEPERATOR;						// 2 : tab
			s += kc.Description + KEY_SEPERATOR;				// 3 : description
			
			s += kc.Key + KEY_SEPERATOR;						// 4 : keyboard key
			s += kc.MouseButton + KEY_SEPERATOR;				// 5 : mouse button
			s += kc.JoystickButton + KEY_SEPERATOR;				// 6 : joystick button
			
			s += kc.isAxis.ToString() + KEY_SEPERATOR;			// 7 : is an axis
			
			s += kc.NegativeKey + KEY_SEPERATOR;				// 8 : negative keyboard key
			s += kc.NegativeMouseButton + KEY_SEPERATOR;		// 9 : negative mouse button
			s += kc.NegativeJoystickButton + KEY_SEPERATOR;		// 10 : negative joystick button
			
			s += ( (int) kc.MouseAxis ).ToString() + KEY_SEPERATOR;		// 11 : mouse axis
			s += ( (int) kc.JoystickAxis ).ToString() + KEY_SEPERATOR;	// 12 : joystick axis
			
			s += kc.Gravity.ToString() + KEY_SEPERATOR;					// 13 : gravity
			s += kc.Sensitivity.ToString() + KEY_SEPERATOR;				// 14 : sensitivity
			
			s += kc.IsBidirectional.ToString() + KEY_SEPERATOR;			// 15 : is bidirectional
			s += kc.BiDirectionalLinkName.ToString() + KEY_SEPERATOR;	// 16 : bidirectional link name
			s += ( (int) kc.Direction ).ToString();						// 17 : bidirectional direction
			
			s += CONFIG_SEPERATOR; // one extra config seperator is always added to teh end
		}
		
		Debug.Log ("Saved");
		PlayerPrefs.SetString( SAVE_EDIT_PREF_NAME, s);
		return true;
	}
	
	// load the configs for the editor
	public bool LoadEditorConfig()
	{
		if(!PlayerPrefs.HasKey( SAVE_EDIT_PREF_NAME ))
		{
			Debug.Log("An editor configuration has not been saved before");
			return false;
		}
	
		string setupstring = "";
		
		setupstring = PlayerPrefs.GetString( SAVE_EDIT_PREF_NAME );
		
		string[] configsetups = setupstring.Split( CONFIG_SEPERATOR );
		
		//subtract by one because one extra config seperator is always added to the end
		defaultKeys = new KeyConfig[ configsetups.Length - 1 ];
		int count = 0;
		
		if( configsetups.Length <= 1 ) return false;
		
		foreach( string s in configsetups )
		{
			int i = count;
			count ++;
			
			if( i >= defaultKeys.Length ) continue;
			
			defaultKeys[i] = new KeyConfig();
			
			string[] param = s.Split( KEY_SEPERATOR );
			if( param.Length != 18 )
			{
				Debug.LogWarning( "Config number " + i + " was not stored properly, skipping" );
				continue;
			}

			defaultKeys[i].Name = param[0];						// 0 : name
			
			defaultKeys[i].PlayerNum = int.Parse(param[1]);		// 1 : player number
			defaultKeys[i].Tab = param[2];						// 2 : tab
			defaultKeys[i].Description = param[3];				// 3 : description
			
			defaultKeys[i].Key = param[4];						// 4 : keyboard key
			defaultKeys[i].MouseButton = param[5];				// 5 : mouse button
			defaultKeys[i].JoystickButton = param[6];			// 6 : joystick button
			
			defaultKeys[i].isAxis = bool.Parse(param[7]);						// 7 : is an axis
			
			defaultKeys[i].NegativeKey = param[8];								// 8 : negative keyboard key
			defaultKeys[i].NegativeMouseButton = param[9];						// 9 : negative mouse button
			defaultKeys[i].NegativeJoystickButton = param[10];					// 10 : negative joystick button
			
			defaultKeys[i].MouseAxis = (MouseAxes) int.Parse(param[11]);		// 11 : mouse axis
			defaultKeys[i].JoystickAxis = (JoyAnalog) int.Parse(param[12]);		// 12 : joystick axis
			
			defaultKeys[i].Gravity = int.Parse( param[13] );					// 13 : gravity
			defaultKeys[i].Sensitivity = int.Parse( param[14] );				// 14 : sensitivity
			
			defaultKeys[i].IsBidirectional = bool.Parse( param[15] );			// 15 : is bidirectional
			defaultKeys[i].BiDirectionalLinkName = param[16];					// 16 : bidirectional link name
			defaultKeys[i].Direction = (BiDir) int.Parse(param[17]);			// 17 : bidirectional direction			
		}
				
		return true;
	}
	
	#endregion
	
	#region GUI Functions and Variables
	
	enum EditStates {
		PosKey = 2,
		NegKey = 0,
		VertPosKey = 1,
		VertNegKey = 3
	}
	
	Vector2 _scrollPos = Vector2.zero;
	float _scrollBoxHeight = 0.0f;
	
	KeyConfig _editting = null;
	bool _edittingJoy = false;
	EditStates _edittingState = EditStates.NegKey;
	KeyCode _prevKeyPressed = 0;
	
	[SerializeField]
	bool _allowDupes = false;
		
	/*** GUI EDITOR VISUAL VARIABLES ***/
	//variables in terms of percentages from 0 to 1
	float EditorWidthPercent = 1;
	float EditorHeightPercent = 1;
	
	int TabsHeight = 50;
	int TitleHeight = 50;
	int HeaderHeight = 50;
	int FooterHeight = 50;
	int FooterPadding = 10;
	int ConfigButtonHeight = 65;
	float DescriptionColumnWidthPercent = .33f;

	public GUISkin GeneralSkin;
	
	int _currentTab = 0;
	
	// Textures
	public Texture TitleTexture = null;
	
	public Texture DescriptionTexture = null;
	public Texture MouseTexture = null;
	public Texture JoystickTexture = null;
	public Texture BackgroundTexture = null;
	
	public Texture EvenButtonTexture = null;
	public Texture OddButtonTexture = null;
	
	public Texture SliderTexture = null;
	
	public AudioClip ErrorSound = null;
	
	bool ShowDuplicatesToggle = false;
	bool Tabulate = false;

	public int GUIDepth = 0;
	
	string _errorText = "";
		
	void OnGUI(){
		
		if(!_editorOpen)return;
		
		if(_openedFrame != Time.frameCount && Input.GetKeyDown(KeyCode.Escape) && _editting == null){
			CloseEditor();
			return;
		}
		

		
		
		GUI.depth = GUIDepth;
		if(GeneralSkin != null) GUI.skin = GeneralSkin;
		
		
		//set up the total dimensions and position of the window
		float w = Screen.width * EditorWidthPercent;
		float h = Screen.height * EditorHeightPercent;
		
		float x = (Screen.width - w)/2;
		float y = (Screen.height - h)/2;		
		
		//Draw Background
		if(BackgroundTexture == null)
		{
			GUI.Box(new Rect(x,y,w,h),"");
		}
		else
		{
			GUI.DrawTexture(new Rect(x,y,w,h), BackgroundTexture);
		}
		
		float topBuffer = HeaderHeight + TitleHeight;
		float bottomBuffer = FooterHeight;
		
		
		int currListNum = 0;
		
		
		// TABS
		//disable the tabs if something is being editted
		if(_editting != null) GUI.enabled = false;
		float tabSize = 0;		
		
		GUI.BeginGroup(new Rect(x,y,w,500));
		
			//if tabs are to be shown
			if( Tabulate ){
				tabSize = TabsHeight;
				
				//draw the buttons
				for(int i = 0 ; i < _tabList.Count; i ++){
					string tabname = _tabList[i].ToString();
					if(tabname.Replace(" ", "") == "") tabname = "Other";
				
					//set the gui style
					if( GUI.Button(new Rect( w*i/_tabList.Count, 0, w /_tabList.Count, tabSize), tabname, (i == _currentTab)? "ActiveTab" : "Tab"))
					{
						_currentTab = i;
					}
				
				}
				//count how many configs are in the current tab
				foreach( KeyConfig kc in defaultKeys ){
					if( kc.Tab == _tabList[ _currentTab ].ToString() ) {
						currListNum ++;
					}
				}
			}else{
				currListNum = _changedConfigs.Count;
			}
			
		GUI.EndGroup();
		GUI.enabled = true;
		
		
		
		// TOP icons
		GUI.BeginGroup(new Rect(x,y+tabSize,w,topBuffer));
			
		
			// calculate percent that the key config columns will take up
			float buttonPercent = (1 - DescriptionColumnWidthPercent)/2;
			
			//draw the description header
			Rect r = new Rect(0,0,w,TitleHeight);
	
			GUI.Label(r, "Title");
			r.y += TitleHeight;
			r.height = HeaderHeight;
			r.width *= DescriptionColumnWidthPercent;
		
			if(DescriptionTexture) GUI.DrawTexture(r, DescriptionTexture, ScaleMode.ScaleToFit);
			else GUI.Label( r, "Description");
			
			//draw the keyboard image
			r.width = buttonPercent * w;
			r.x += w*DescriptionColumnWidthPercent;
			if(MouseTexture) GUI.DrawTexture(r, MouseTexture, ScaleMode.ScaleToFit);
			else GUI.Label( r, "Mouse and Keyboard");
			
			//draw the joystick image
			r.x += w * buttonPercent;
			if(JoystickTexture) GUI.DrawTexture(r, JoystickTexture, ScaleMode.ScaleToFit);
			else GUI.Label( r, "Controller" );
		
		GUI.EndGroup();
		
		topBuffer += tabSize;
		
		
		
		// SCROLL VIEW
		_scrollPos = GUI.BeginScrollView(new Rect( x, y + 10 + topBuffer, w, h-20-topBuffer-bottomBuffer), _scrollPos, new Rect(0,0,w-50, currListNum * ConfigButtonHeight));
		
			ArrayList drawnBiDir = new ArrayList();
			
			//draw all config buttons
			int currspot = 0;
			foreach(KeyConfig d in defaultKeys){
				//skip if it does not belong to the current tab
				if( d.Tab != _tabList[ _currentTab ].ToString() && Tabulate) continue;
				//skip if the bidir link associated has already been passed
				if(drawnBiDir.Contains(d.Name)) continue;
				
				KeyConfig kc = _changedConfigs[d.Name] as KeyConfig;
				
				currspot += DrawKeyConfigButton(currspot, kc, w - 5);
				
				if( kc._biDirLink != null) drawnBiDir.Add(kc._biDirLink.Name);
			}
		
		GUI.EndScrollView();
		
		
		//disable the buttons if something is being editted
		if(_editting != null) GUI.enabled = false;
		// BOTTOM BUTTONS
		GUI.BeginGroup(new Rect(x + FooterPadding, y + h-bottomBuffer, w, bottomBuffer - FooterPadding));
			
			//draw cancel, save, and defaults buttons
			if( GUI.Button(new Rect(0,0,150,bottomBuffer - FooterPadding), "Cancel") ) CloseEditor();
			if( GUI.Button(new Rect(150,0,150,bottomBuffer - FooterPadding), "Save") ) { SaveChanges(); SaveOutConfig(ref _configs, SAVE_PREF_NAME); CloseEditor(); }
			if( GUI.Button(new Rect(300,0,150,bottomBuffer - FooterPadding), "Reset To Defaults") ) ResetToDefault(out _changedConfigs);
			
			if(ShowDuplicatesToggle) _allowDupes = GUI.Toggle(new Rect(450,0,200,150), _allowDupes,"Allow Duplicates");
			
		GUI.EndGroup();
		GUI.enabled = false;
		
		//store the current key taht was pressed
		_prevKeyPressed = Event.current.keyCode;
	}
	
	//draw the given keyconfig at the given position. Returns the amount of positions to be jumped
	int DrawKeyConfigButton(int pos, KeyConfig kc, float width){
		
		if(kc._biDirLink != null) return DrawBidirAxisButton(pos,kc,width);
		
		float lineHeight = ConfigButtonHeight;
		
		float buttonPercent = (1 - DescriptionColumnWidthPercent)/2;

		GUIStyle centerLabel = new GUIStyle("KeyConfigLabel");
		centerLabel.alignment = TextAnchor.MiddleCenter;

		GUI.BeginGroup(new Rect(0, lineHeight * pos, width,lineHeight * 2));
		
		//draw description
		GUI.Label(new Rect(0,0,width * DescriptionColumnWidthPercent, lineHeight), kc.Description);
		
		//draw keyboard button
		if( GUI.Button(new Rect(width * DescriptionColumnWidthPercent,0, width * buttonPercent, lineHeight), ""/*,(_editting == kc && !_edittingJoy)?"ActiveKeyConfig":"KeyConfig"*/))
		{
			EditConfig(kc);
		}
		// draw label
		GUI.Label(new Rect(width * DescriptionColumnWidthPercent,0, width * buttonPercent, lineHeight), GetButtonString(kc),centerLabel);
		
		//draw joystick button
		if( GUI.Button(new Rect(width *(1-buttonPercent),0, width * buttonPercent, lineHeight), ""/*,(_editting == kc && _edittingJoy)?"ActiveKeyConfig":"KeyConfig"*/))
		{
			EditConfig(kc, true);
		}
		//draw label 
		GUI.Label(new Rect(width *(1-buttonPercent),0, width * buttonPercent, lineHeight), GetButtonString(kc,true),centerLabel);
		
		GUI.EndGroup();
		
		return 1;
	}
	
	//draw the button as a bidirectional axis button
	int DrawBidirAxisButton(int pos, KeyConfig kc, float width){
		if( kc._biDirLink == null ) return 0;
		
		const int bidiraxisHeight = 1;
		float lineHeight = ConfigButtonHeight;
		
		GUI.BeginGroup(new Rect(0, lineHeight * pos, width,lineHeight * bidiraxisHeight));
		
		lineHeight *= bidiraxisHeight;
		
		float buttonPercent = (1 - DescriptionColumnWidthPercent)/2;
		
		// Draw description
		GUI.Label( new Rect(0,0,width * DescriptionColumnWidthPercent, lineHeight) , kc.Description);
		
		// keyboard button
		Rect r = new Rect(width * DescriptionColumnWidthPercent,0, width * buttonPercent, lineHeight);
		if( GUI.Button(r, ""/*, (_editting == kc && !_edittingJoy)?"ActiveKeyConfig":"KeyConfig"*/))
		{
			EditConfig(kc);
		}
		
		GUI.BeginGroup(r);
		DrawBidirLabels(kc, new Vector2(width *buttonPercent, lineHeight));
		GUI.EndGroup();
		
		//joystick button
		r = new Rect(width * (1 - buttonPercent),0, width * buttonPercent, lineHeight);
		if( GUI.Button(r, ""/*, (_editting == kc && _edittingJoy)?"ActiveKeyConfig":"KeyConfig"*/))
		{
			EditConfig(kc, true);
		}
		
		GUI.BeginGroup(r);
		DrawBidirLabels(kc,new Vector2(width *buttonPercent, lineHeight),true);
		GUI.EndGroup();
		
		GUI.EndGroup();
		
		return bidiraxisHeight;
	}
	
	//get the button string for the given key config
	string GetButtonString(KeyConfig kc, bool forJoy = false){
		string s = "";
		
		string editKey = "_"; //for edit
		
		if(kc.isAxis){
			if( !forJoy ){
				//KEYBOARD
				if( kc.MouseAxis != MouseAxes.None && !( kc == _editting && !_edittingJoy)){ return kc.MouseAxis.ToString(); } //for edit
				
				if( kc._negMouseKey != -1 ) s += "Mouse " + kc._negMouseKey.ToString();
				else s += ((KeyCode) kc._negKey).ToString();
				
				if( kc == _editting && _edittingState == EditStates.NegKey && !_edittingJoy) s = editKey; //for edit
			}else{
				//JOYSTICK
				if(  kc.JoystickAxis != JoyAnalog.None && !( kc ==_editting && _edittingJoy)){ return "P" + kc._joyAxisPlayer + " " + ((JoyAnalog)kc.JoystickAxis).ToString(); } //for edit

				//s += GetControllerKeyName((JoyCode)kc._negJoyKey,kc._negJoyKeyPlayer); //for custom strings
				s += "P" + kc._negJoyKeyPlayer + " " + ((JoyCode) kc._negJoyKey).ToString();
				
				if( kc == _editting && _edittingState == EditStates.NegKey && _edittingJoy) s = editKey; //for edit

			}			
			s += " / ";
		
			if( kc == _editting && _edittingState == EditStates.NegKey && _edittingJoy == forJoy) return s; //for edit

		}
		
		if( !forJoy ){
			//KEYBOARD
			if( kc == _editting && _edittingState == EditStates.PosKey && !_edittingJoy) s += editKey; //for edit
			else if( kc._mouseKey != -1 ) s += "Mouse " + kc._mouseKey.ToString();
			else s += ((KeyCode) kc._key).ToString();
		}else{
			//JOYSTICK
			if( kc == _editting && _edittingState == EditStates.PosKey && _edittingJoy) s += editKey; //for edit
			//else s += GetControllerKeyName((JoyCode)kc._joyKey,kc._joyKeyPlayer); // for custom strings
			
			else s += "P" + kc._joyKeyPlayer + " " + ((JoyCode) kc._joyKey).ToString();
		}
		
		return s;
	}
	
	// draw the layout for the bidir labels
	void DrawBidirLabels(KeyConfig kc, Vector2 dim, bool forJoy = false){
		
		if( kc._biDirLink == null ) return;
		
		string editKey = "_";
		
		KeyConfig HorizConfig;
		KeyConfig VertConfig;
		
		if(kc.Direction == BiDir.Horizontal){
			HorizConfig = kc;
			VertConfig = kc._biDirLink;
		}else{
			VertConfig = kc;
			HorizConfig = kc._biDirLink;
		}
		
		string analogAxis = "";
		string[] keys = new string[4]; //left up right down
		
		if( !forJoy ){
			switch(kc.MouseAxis){
			case MouseAxes.MoveHorizontal:
			case MouseAxes.MoveVertical:
				analogAxis = "Mouse";
				break;
			case MouseAxes.ScrollWheel:
				analogAxis = "Scroll Wheel";
				break;
			}
			
			if(analogAxis == "")
			{
				if( HorizConfig._negMouseKey != -1 ) keys[0] += "Mouse " + HorizConfig._negMouseKey;
				else keys[0] = ((KeyCode) HorizConfig._negKey).ToString();
				
				if( VertConfig._mouseKey != -1 ) keys[1] += "Mouse " + VertConfig._mouseKey;
				else keys[1] = ((KeyCode) VertConfig._key).ToString();
				
				if( HorizConfig._mouseKey != -1 ) keys[2] += "Mouse " + HorizConfig._mouseKey;
				else keys[2] = ((KeyCode) HorizConfig._key).ToString();
				
				if( VertConfig._negMouseKey != -1 ) keys[3] += "Mouse " + VertConfig._negMouseKey;
				keys[3] = ((KeyCode) VertConfig._negKey).ToString();			
			}
			
		}else{
			switch(kc.JoystickAxis){
			case JoyAnalog.DPadHorizontal:
			case JoyAnalog.DPadVertical:
				analogAxis = "D Pad";
				break;
			case JoyAnalog.LeftStickHorizontal:
			case JoyAnalog.LeftStickVertical:
				analogAxis = "Left Stick";
				break;
			case JoyAnalog.RightStickHorizontal:
			case JoyAnalog.RightStickVertical:
				analogAxis = "Right Stick";
				break;	
			}
			
			//analogAxis = GetControllerKeyName(kc.JoystickAxis, kc._joyAxisPlayer); //for custom strings
			
			if(analogAxis == "")
			{
				/*
				//for custom strings
				keys[0] = GetControllerKeyName((JoyCode)HorizConfig._negJoyKey, HorizConfig._negJoyKeyPlayer);
				keys[1] = GetControllerKeyName((JoyCode)VertConfig._joyKey, VertConfig._joyKeyPlayer);
				keys[2] = GetControllerKeyName((JoyCode)VertConfig._negJoyKey, VertConfig._negJoyKeyPlayer);
				keys[3] = GetControllerKeyName((JoyCode)HorizConfig._joyKey, HorizConfig._joyKeyPlayer);
				*/
				
				keys[0] = "P" + HorizConfig._negJoyKeyPlayer + " " + ((JoyCode)HorizConfig._negJoyKey).ToString();
				keys[1] = "P" + VertConfig._joyKeyPlayer + " " + ((JoyCode)VertConfig._joyKey).ToString();
				keys[2] = "P" + HorizConfig._joyKeyPlayer + " " + ((JoyCode)HorizConfig._joyKey).ToString();
				keys[3] = "P" + VertConfig._negJoyKeyPlayer + " " + ((JoyCode)VertConfig._negJoyKey).ToString();
			}
		}
		
		GUIStyle centeredLabel = new GUIStyle("label");
		centeredLabel.alignment = TextAnchor.MiddleCenter;
		
		//change the key to the edit character
		if(_editting == kc && forJoy == _edittingJoy)
		{
			int start = (int) _edittingState;
			for(int i = start ; i < 4 ; i++){
				keys[i] = "";
			}
			keys[start] = editKey;
		}
		
		if(analogAxis != ""){
			GUI.Label(new Rect(0,0,dim.x,dim.y), analogAxis,centeredLabel);
		}else{
			
			int padding = ConfigButtonHeight/5;
			
			centeredLabel.padding = new RectOffset(0,padding,0,0);
			centeredLabel.alignment = TextAnchor.MiddleRight;
			GUI.Label(new Rect(0,0, dim.x/2, dim.y), keys[0],centeredLabel); //left
			
			centeredLabel.padding = new RectOffset(0,0,0,padding);
			centeredLabel.alignment = TextAnchor.LowerCenter;
			GUI.Label(new Rect(0,0, dim.x, dim.y/2), keys[1],centeredLabel); //top
			
			centeredLabel.padding = new RectOffset(padding,0,0,0);
			centeredLabel.alignment = TextAnchor.MiddleLeft;
			GUI.Label(new Rect(dim.x/2,0, dim.x/2, dim.y), keys[2],centeredLabel); //right
			
			centeredLabel.padding = new RectOffset(0,0,padding,0);
			centeredLabel.alignment = TextAnchor.UpperCenter;
			GUI.Label(new Rect(0,dim.y/2, dim.x, dim.y/2), keys[3],centeredLabel); //bottom
			
		}
	}
	
	//returns the appropriate name for the given string
	string GetControllerKeyName(JoyCode jc, int jplayer)
	{
		ControllerManager.JoyInterface ji = CMRef.GetController(jplayer);
		
		if(ji != null)
		{
			return "P" + jplayer + " " + ji.GetButtonName(jc);
		}
		else
		{
			return "P" + jplayer + " " + jc.ToString();
		}
	}
	string GetControllerKeyName(JoyAnalog ja, int jplayer)
	{
		ControllerManager.JoyInterface ji = CMRef.GetController(jplayer);
		
		if(ji != null)
		{
			return "P" + jplayer + " " + ji.GetButtonName(ja);
		}
		else
		{
			return "P" + jplayer + " " + ja.ToString();
		}
	}
	#endregion
	
	#region Dupe Key Checking
	
	//returns whether or not the keyconfig contains the given key
	bool HasKey(KeyConfig config, int k, int jplayer)
	{
		if( k > 0 ) return HasKey( config, (KeyCode) k);
		else if( k < 0 ) return HasKey( config, (JoyCode) k, jplayer);
		else return HasKey( config, (KeyCode) k) || HasKey( config, (JoyCode) k, jplayer);
	}
	
	bool HasKey(KeyConfig config, KeyCode kc)
	{
		/*KeyConfig HorizConfig;
		KeyConfig VertConfig;
		if( config._biDirLink == null || config.Direction == BiDir.Horizontal ){
			HorizConfig = config;
			VertConfig = config._biDirLink;
		}else{
			VertConfig = config;
			HorizConfig = config._biDirLink;
		}
		
		bool val = HorizConfig._key == (int)kc || HorizConfig._negKey == (int)kc;
		if(VertConfig != null) val = val && (VertConfig._key == (int)kc || VertConfig._negKey == (int)kc);
		
		return val;
		*/
		if( config == null ) return false;
		return config._negKey == (int)kc || config._key == (int)kc;
	}
	bool HasKey(KeyConfig config, MouseAxes ma)
	{
		if( config == null ) return false;
		return config.MouseAxis == ma;
	}
	bool HasMouseKey(KeyConfig config, int button)
	{
		if( config == null ) return false;
		return config._mouseKey == button || config._negMouseKey == button;
	}
	bool HasKey(KeyConfig config, JoyCode jc, int jplayer)
	{
		if( config == null ) return false;
		return (config._joyKey == (int)jc && config._joyKeyPlayer == jplayer) || (config._negJoyKey == (int)jc && config._negJoyKeyPlayer == jplayer);
	}
	bool HasKey(KeyConfig config, JoyAnalog ja, int jplayer)
	{
		if( config == null ) return false;
		return config.JoystickAxis == ja && config._joyAxisPlayer == jplayer;
	}
	
	//returns true if it found the given key in a config in the hashtable and files the arraylist with all keyconfigs that contain it
	bool FindKey(Hashtable cfg, KeyCode kc, out ArrayList keyConfigList){
		bool found = false;
		keyConfigList = new ArrayList();
		
		foreach(IDictionaryEnumerator d in cfg){
			KeyConfig config = d.Value as KeyConfig;
			
			if( HasKey(config, kc) ) keyConfigList.Add(config);
		}
		
		return found;
	}
	bool FindKey(Hashtable cfg, MouseAxes ma, out ArrayList keyConfigList){
		bool found = false;
		keyConfigList = new ArrayList();
		
		foreach(IDictionaryEnumerator d in cfg){
			KeyConfig config = d.Value as KeyConfig;
			
			if( HasKey(config, ma) ) keyConfigList.Add(config);
		}
		
		return found;
	}
	bool FindMouseKey(Hashtable cfg, int button, out ArrayList keyConfigList){
		bool found = false;
		keyConfigList = new ArrayList();
		
		foreach(IDictionaryEnumerator d in cfg){
			KeyConfig config = d.Value as KeyConfig;
			
			if( HasMouseKey(config, button) ) keyConfigList.Add(config);
		}
		
		return found;
	}
	bool FindKey(Hashtable cfg, JoyCode jc, int jplayer, out ArrayList keyConfigList){
		bool found = false;
		keyConfigList = new ArrayList();
		
		foreach(IDictionaryEnumerator d in cfg){
			KeyConfig config = d.Value as KeyConfig;
			
			if( HasKey(config, jc, jplayer) ) keyConfigList.Add(config);
		}
		
		return found;
	}
	bool FindKey(Hashtable cfg, JoyAnalog ja, int jplayer, out ArrayList keyConfigList){
		bool found = false;
		keyConfigList = new ArrayList();
		
		foreach(IDictionaryEnumerator d in cfg){
			KeyConfig config = d.Value as KeyConfig;
			
			if( HasKey(config, ja, jplayer) ) keyConfigList.Add(config);
		}
		
		return found;
	}
	
	//removes the key from the table unless it is in "except"
	//TODO: When removing an axis, make sure that it is removed from the bidirlink keyconfig aswell.
	bool RemoveKey(Hashtable cfg, KeyCode kc, KeyConfig except){
				
		ArrayList al = null;
		if( FindKey(cfg, kc, out al) ){
			int ikc = (int)kc;
			
			foreach(KeyConfig e in al){
				if( e == except ) continue;
				
				e._key = (ikc == e._key)? 0 : e._key;
				e._negKey = (ikc == e._negKey)? 0 : e._negKey;
			}
		}
		return al.Count >= 1 && !(al.Count == 1 && al.Contains(except));
	}
	bool RemoveKey(Hashtable cfg, MouseAxes ma, KeyConfig except){
				
		ArrayList al = null;
		if( FindKey(cfg, ma, out al) ){
			
			foreach(KeyConfig e in al){
				if( e == except ) continue;
				
				e.MouseAxis = MouseAxes.None;
			}
		}
		return al.Count >= 1 && !(al.Count == 1 && al.Contains(except));
	}
	bool RemoveMouseKey(Hashtable cfg, int button, KeyConfig except){
				
		ArrayList al = null;
		if( FindMouseKey(cfg, button, out al) ){
			
			foreach(KeyConfig e in al){
				if( e == except ) continue;
				
				e._mouseKey = (e._mouseKey == button)? -1 : e._mouseKey;
				e._negMouseKey = (e._negMouseKey == button)? -1 : e._negMouseKey;
			}
		}
		return al.Count >= 1 && !(al.Count == 1 && al.Contains(except));
	}
	bool RemoveKey(Hashtable cfg, JoyCode jc, int jplayer, KeyConfig except){
				
		ArrayList al = null;
		if( FindKey(cfg, jc, jplayer, out al) ){
			int ijc = (int)jc;
			
			foreach(KeyConfig e in al){
				if( e == except ) continue;
				
				e._joyKey = (ijc == e._joyKey && jplayer == e._joyKeyPlayer)? 0 : e._joyKey;
				e._negJoyKey = (ijc == e._negJoyKey && jplayer == e._negJoyKeyPlayer)? 0 : e._negJoyKey;
			}
		}
		return al.Count >= 1 && !(al.Count == 1 && al.Contains(except));
	}
	bool RemoveKey(Hashtable cfg, JoyAnalog ja, int jplayer, KeyConfig except){
				
		ArrayList al = null;
		if( FindKey(cfg, ja, jplayer, out al) ){
			
			foreach(KeyConfig e in al){
				if( e == except ) continue;
				
				e.JoystickAxis = JoyAnalog.None;
			}
		}
		return al.Count >= 1 && !(al.Count == 1 && al.Contains(except));
	}
	
	#endregion
	
	#region Key Editting
	//set the given keyconfig to be editted
	bool EditConfig(KeyConfig kc, bool forJoy = false){
		if( _editting != null) return false;
		
		_editting = kc;
		_edittingJoy = forJoy;
		
		if( kc.isAxis ) _edittingState = EditStates.NegKey;
		else _edittingState = EditStates.PosKey;
		
		return true;
	}
	
	//reassign the keys to what the player presses if the player is editting a config
	void DoEditKeyWork(){
		if(_editting == null || !_editorOpen)return;	
		
		//stop editting if escape is pressed
		if( _prevKeyPressed == KeyCode.Escape && (( _edittingState == EditStates.NegKey && _editting.isAxis) || (_edittingState == EditStates.PosKey && !_editting.isAxis)) ) {
			_editting = null;
			return;
		}else if(_prevKeyPressed == KeyCode.Escape){
			return;
		}
		
		//get the keys that were pressed
		bool keychanged = false;
		bool axischanged = false;
		
		//if the player is editting the joystick keys
		if(_edittingJoy)
		{
			// Joycode
			int JCPlayer = -1;
			JoyCode JCPressed = ControllerManager.Get().GetKeyPressed(out JCPlayer);
			
			// Joystick Axis
			int JAPlayer = -1;
			JoyAnalog JAChanged = JoyAnalog.None;
			
			//check if an analog button was pressed
			for(int i = 1 ; i <= 4 ; i ++){
				ControllerManager.JoyInterface joy = CMRef.GetController(i);
				if(joy == null) continue;
	
				//check each analog axis
				foreach(JoyAnalog ja in JoyCode.GetValues(typeof(JoyAnalog))){
					if(joy.GetAnalogValue(ja) != 0){
						JAChanged = ja;
						JAPlayer = i;
						break;
					}
				}
			}
			
			//if and analog value was changed
			if( JAChanged != JoyAnalog.None && JAChanged != JoyAnalog.LeftTrigger && JAChanged != JoyAnalog.RightTrigger)
			{
				axischanged = EditKey( _editting, _edittingState, JAChanged, JAPlayer );
			}
			
			//if a button was pressed
			if( JCPressed != JoyCode.None && !axischanged)
			{
				keychanged = EditKey( _editting, _edittingState, JCPressed, JCPlayer);
			}
			else if( !axischanged )
			{
				return;
			}
			
		}
		//otherwise if the player is editting the keyboard bindings
		else
		{
			// Keycode
			KeyCode KCPressed = _prevKeyPressed;
			
			// Mouse Axis
			MouseAxes MAChanged = MouseAxes.None;
			float mX = Mathf.Abs(Input.GetAxis("MouseX"));
			float mY = Mathf.Abs(Input.GetAxis("MouseY"));
			float ms = Mathf.Abs(Input.GetAxis("MouseScrollWheel"));
			
			//scroll wheel and movement change checks
			if(ms > .2f) MAChanged = MouseAxes.ScrollWheel;
			else if(mX > 3 && mX > mY) MAChanged = MouseAxes.MoveHorizontal;
			else if(mY > 3 && mY > mX) MAChanged = MouseAxes.MoveVertical;
			
			// Mouse Buttons
			int MPressed = -1;
			for(int i = 0 ; i < 3 ; i ++){
				if(GetMouseButtonDown(i)) MPressed = i;
			}
			
			// if the mouse axis changed
			if( MAChanged != MouseAxes.None )
			{
				axischanged = EditKey( _editting, _edittingState, MAChanged);
			}
			//if a button was pressed
			else if( MPressed != -1)
			{
				keychanged = EditMouseKey( _editting, _edittingState, MPressed);
			}
			//if a key was pressed
			else if( KCPressed != KeyCode.None )
			{
				keychanged = EditKey( _editting, _edittingState, KCPressed);
			}
			else
			{
				return;
			}
			
			
		}
		
		//if an axis was changed
		if( axischanged ){
			_editting = null;
		//if a key changed
		}else if(keychanged){
			//cycle the editted key forward
			if(_editting.isAxis && _editting._biDirLink != null)
			{
				
				switch(_edittingState){
				case EditStates.NegKey: //left
					_edittingState = EditStates.VertPosKey;
					break;
				case EditStates.VertPosKey: //up
					_edittingState = EditStates.PosKey;
					break;
				case EditStates.PosKey: //right
					_edittingState = EditStates.VertNegKey;
					break;
				case EditStates.VertNegKey: //down
					_editting = null;
					break;
				default:
					_editting = null;
					break;
				}
				
			}
			else if(_editting.isAxis)
			{
				switch(_edittingState){
				case EditStates.NegKey:
					_edittingState = EditStates.PosKey;
					break;
				case EditStates.PosKey:
					_editting = null;
					break;
				default:
					_editting = null;
					break;
				}
			}
			else
			{
				_editting = null;				
			}
		}
	}
	
	//edit the current key if a keycode was pressed
	bool EditKey( KeyConfig config, EditStates es, KeyCode kc){
		if( ( es == EditStates.VertNegKey || es == EditStates.VertPosKey ) && config._biDirLink == null) return false;
		
		KeyConfig HorizConfig;
		KeyConfig VertConfig;
		
		if( config._biDirLink == null || config.Direction == BiDir.Horizontal ){
			HorizConfig = config;
			VertConfig = config._biDirLink;
		}else{
			VertConfig = config;
			HorizConfig = config._biDirLink;
		}
		
		int ikc = (int) kc;
		
		//check for dupes in this keyconfig alone
		if(config.isAxis && config._biDirLink != null)
		{			
			switch(es){
			case EditStates.VertNegKey:
				if(ikc == HorizConfig._key) return false;
				goto case EditStates.PosKey;
			case EditStates.PosKey:
				if(ikc == VertConfig._key) return false;
				goto case EditStates.VertPosKey;
			case EditStates.VertPosKey:
				if(ikc == HorizConfig._negKey) return false;
				goto case EditStates.NegKey;
			case EditStates.NegKey:
				break;
			}
		}
		else if(config.isAxis)
		{	
			switch(es){
			case EditStates.PosKey:
				if(ikc == HorizConfig._negKey) return false;
				goto case EditStates.NegKey;
			case EditStates.NegKey:
				break;
			}
		}
		
		// SET the correct key
		switch(es){
		case EditStates.PosKey:
			ClearKeys(HorizConfig,es);
			HorizConfig._key = ikc;
			break;
		case EditStates.NegKey:
			ClearKeys(HorizConfig,es);
			HorizConfig._negKey = ikc;
			break;
		case EditStates.VertPosKey:
			ClearKeys(VertConfig,es);
			VertConfig._key = ikc;
			break;
		case EditStates.VertNegKey:
			ClearKeys(VertConfig,es);
			VertConfig._negKey = ikc;
			break;
		}
		return true;
	}
	// edit the current key if a joystick key was pressed
	bool EditKey( KeyConfig config, EditStates es, JoyCode jc, int jplayer){
		if( ( es == EditStates.VertNegKey || es == EditStates.VertPosKey ) && config._biDirLink == null) return false;
		
		KeyConfig HorizConfig;
		KeyConfig VertConfig;
		
		if( config._biDirLink == null || config.Direction == BiDir.Horizontal ){
			HorizConfig = config;
			VertConfig = config._biDirLink;
		}else{
			VertConfig = config;
			HorizConfig = config._biDirLink;
		}
		
		int ijc = (int) jc;
		
		//check for dupes in this joyconfig alone
		if(config.isAxis && config._biDirLink != null)
		{			
			switch(es){
			case EditStates.VertNegKey:
				if(ijc == HorizConfig._joyKey && jplayer == HorizConfig._joyKeyPlayer) return false;
				goto case EditStates.PosKey;
			case EditStates.PosKey:
				if(ijc == VertConfig._joyKey && jplayer == VertConfig._joyKeyPlayer) return false;
				goto case EditStates.VertPosKey;
			case EditStates.VertPosKey:
				if(ijc == HorizConfig._negJoyKey && jplayer == HorizConfig._negJoyKeyPlayer) return false;
				goto case EditStates.NegKey;
			case EditStates.NegKey:
				break;
			}
		}
		else if(config.isAxis)
		{	
			switch(es){
			case EditStates.PosKey:
				if(ijc == HorizConfig._negJoyKey && jplayer == HorizConfig._negJoyKeyPlayer) return false;
				goto case EditStates.NegKey;
			case EditStates.NegKey:
				break;
			}
		}
		
		// SET the key

		switch(es){
		case EditStates.PosKey:
			ClearKeys(HorizConfig, es, true);
			HorizConfig._joyKeyPlayer = jplayer;
			HorizConfig._joyKey = ijc;
			break;
		case EditStates.NegKey:
			ClearKeys(HorizConfig, es, true);
			HorizConfig._negJoyKeyPlayer = jplayer;
			HorizConfig._negJoyKey = ijc;
			break;
		case EditStates.VertPosKey:
			ClearKeys(VertConfig, es, true);
			VertConfig._joyKeyPlayer = jplayer;
			VertConfig._joyKey = ijc;
			break;
		case EditStates.VertNegKey:
			ClearKeys(VertConfig, es, true);
			VertConfig._negJoyKeyPlayer = jplayer;
			VertConfig._negJoyKey = ijc;
			break;
		}
		
		return true;
	}
	
	//edit the current config if a mouse axis changed
	bool EditKey( KeyConfig config, EditStates es, MouseAxes ma){
		if( !( (es == EditStates.PosKey && !config.isAxis) || (es == EditStates.NegKey && config.isAxis) ) ) return false;

		if(config.isAxis && config._biDirLink != null){
			
			KeyConfig HorizConfig;
			KeyConfig VertConfig;
			
			if( config.Direction == BiDir.Horizontal ){
				HorizConfig = config;
				VertConfig = config._biDirLink;
			}else{
				VertConfig = config;
				HorizConfig = config._biDirLink;
			}
		
			// SET
			switch(ma){
			case MouseAxes.MoveHorizontal:
			case MouseAxes.MoveVertical:
				ClearKeys(HorizConfig);
				ClearKeys(VertConfig);
				
				HorizConfig.MouseAxis = MouseAxes.MoveHorizontal;
				VertConfig.MouseAxis = MouseAxes.MoveVertical;
				break;
			case MouseAxes.None:
				ClearKeys(HorizConfig);
				ClearKeys(VertConfig);
				
				HorizConfig.MouseAxis = MouseAxes.None;
				VertConfig.MouseAxis = MouseAxes.None;
				break;
			default:
				return false;
			}
		}
		else if(config.isAxis)
		{
			ClearKeys(config);
			config.MouseAxis = ma;
		}
		else
		{
			return false;
		}

		return true;
	}
	
	//edit the current config if a analog joystick input was changed
	bool EditKey( KeyConfig config, EditStates es, JoyAnalog ja, int jplayer){
		if( !( (es == EditStates.PosKey && !config.isAxis) || (es == EditStates.NegKey && config.isAxis) ) || !config.isAxis ) return false;
		
		config._joyAxisPlayer = jplayer;
		config.JoystickAxis = ja;
		
		if(config._biDirLink != null){
			
			KeyConfig HorizConfig;
			KeyConfig VertConfig;
			
			if( config.Direction == BiDir.Horizontal ){
				HorizConfig = config;
				VertConfig = config._biDirLink;
			}else{
				VertConfig = config;
				HorizConfig = config._biDirLink;
			}
		
			// SET
			switch(ja){
			case JoyAnalog.DPadHorizontal:
			case JoyAnalog.DPadVertical:
				ClearKeys(HorizConfig, true);
				ClearKeys(VertConfig, true);
				
				HorizConfig._joyAxisPlayer = jplayer;
				HorizConfig.JoystickAxis = JoyAnalog.DPadHorizontal;
				VertConfig._joyAxisPlayer = jplayer;
				VertConfig.JoystickAxis = JoyAnalog.DPadVertical;
				break;
			case JoyAnalog.LeftStickHorizontal:
			case JoyAnalog.LeftStickVertical:
				ClearKeys(HorizConfig, true);
				ClearKeys(VertConfig, true);
				
				HorizConfig._joyAxisPlayer = jplayer;
				HorizConfig.JoystickAxis = JoyAnalog.LeftStickHorizontal;
				VertConfig._joyAxisPlayer = jplayer;
				VertConfig.JoystickAxis = JoyAnalog.LeftStickVertical;
				break;
			case JoyAnalog.RightStickHorizontal:
			case JoyAnalog.RightStickVertical:
				ClearKeys(HorizConfig, true);
				ClearKeys(VertConfig, true);
				
				HorizConfig._joyAxisPlayer = jplayer;
				HorizConfig.JoystickAxis = JoyAnalog.RightStickHorizontal;
				VertConfig._joyAxisPlayer = jplayer;
				VertConfig.JoystickAxis = JoyAnalog.RightStickVertical;
				break;
			default:
				return false;
			}
		}
		else
		{
			// SET
			ClearKeys(config, true);
			config._joyAxisPlayer = jplayer;
			config.JoystickAxis = ja;
		}
		return true;
	}
	
	// edit the current config if a mouse button was pressed
	bool EditMouseKey( KeyConfig config, EditStates es, int button){
		if( ( es == EditStates.VertNegKey || es == EditStates.VertPosKey ) && config._biDirLink == null) return false;
		
		KeyConfig HorizConfig;
		KeyConfig VertConfig;
		
		if( config._biDirLink == null || config.Direction == BiDir.Horizontal ){
			HorizConfig = config;
			VertConfig = config._biDirLink;
		}else{
			VertConfig = config;
			HorizConfig = config._biDirLink;
		}
				
		//check for dupes in this keyconfig alone
		if(config.isAxis && config._biDirLink != null)
		{			
			switch(es){
			case EditStates.VertNegKey:
				if(button == HorizConfig._mouseKey) return false;
				goto case EditStates.PosKey;
			case EditStates.PosKey:
				if(button == VertConfig._mouseKey) return false;
				goto case EditStates.VertPosKey;
			case EditStates.VertPosKey:
				if(button == HorizConfig._negMouseKey) return false;
				goto case EditStates.NegKey;
			case EditStates.NegKey:
				break;
			}
		}
		else if(config.isAxis)
		{	
			switch(es){
			case EditStates.PosKey:
				if(button == HorizConfig._negMouseKey) return false;
				goto case EditStates.NegKey;
			case EditStates.NegKey:
				break;
			}
		}
		
		// SET the correct key
		switch(es){
		case EditStates.PosKey:
			ClearKeys(HorizConfig, es);
			HorizConfig._mouseKey = button;
			break;
		case EditStates.NegKey:
			ClearKeys(HorizConfig, es);
			HorizConfig._negMouseKey = button;
			break;
		case EditStates.VertPosKey:
			ClearKeys(VertConfig, es);
			VertConfig._mouseKey = button;
			break;
		case EditStates.VertNegKey:
			ClearKeys(VertConfig, es);
			VertConfig._negMouseKey = button;
			break;
		}
		return true;
	}
	
	//clears both positive and negative key assignments
	void ClearKeys(KeyConfig kc, bool forjoy = false){
		
		ClearKeys(kc, EditStates.PosKey, forjoy);
		ClearKeys(kc, EditStates.NegKey, forjoy);
	}
	
	//clears the key assignments for the editstate es, and whether or not it's for a joystick
	void ClearKeys(KeyConfig kc, EditStates es, bool forjoy = false){
		if(kc == null) return;
			
		if( !forjoy )
		{
			switch( es ){
			case EditStates.NegKey:
			case EditStates.VertNegKey:
				kc._negKey = 0;
				kc._negMouseKey = -1;
				break;
			case EditStates.PosKey:
			case EditStates.VertPosKey:
				kc._key = 0;
				kc._mouseKey = -1;
				break;
			}
			kc.MouseAxis = MouseAxes.None;
		}
		else
		{
			switch( es ){
			case EditStates.NegKey:
			case EditStates.VertNegKey:
				kc._negJoyKey = 0;
				kc._negJoyKeyPlayer = 0;
				break;
			case EditStates.PosKey:
			case EditStates.VertPosKey:
				kc._joyKey = 0;
				kc._joyKeyPlayer = 0;
				break;
			}
			kc.JoystickAxis = JoyAnalog.None;
			kc._joyAxisPlayer = 0;
		}
	}
	#endregion
	
	#region Controller / Keyboard navigation
	
	int _navRow = 0;
	int _navCol = 0;
	
	//allows for navigation around the page wit ha controller
	void DoNavigationWork()
	{
		if(_editting!=null || !_editorOpen) return;
		
		int temp = 0;
		JoyCode j = CMRef.GetKeyPressed(out temp);
	
		// Up is pressed
		if( j == JoyCode.DPadUp || j == JoyCode.LeftStickUp || GetKeyDown(KeyCode.UpArrow) )
		{
			NavigateFocus( -1, 0 );
		}
		//Left is pressed
		else if( j == JoyCode.DPadLeft || j == JoyCode.LeftStickLeft || GetKeyDown(KeyCode.LeftArrow) )
		{
			NavigateFocus( 0 , -1 );
		}
		//Right is pressed
		else if( j == JoyCode.DPadRight || j == JoyCode.LeftStickRight || GetKeyDown(KeyCode.RightArrow) )
		{
			NavigateFocus( 0 , 1 );
		}
		//Down is pressed
		else if( j == JoyCode.DPadDown || j == JoyCode.LeftStickDown || GetKeyDown(KeyCode.DownArrow) )
		{
			NavigateFocus( 1 , 0 );
		}
	}
	
	void NavigateFocus( int row, int col ){
		_navRow += row;
		_navCol += col;
			
		//find how many configs will be on the current page
		int configsInPage = 0;
		
		ArrayList al = new ArrayList();
		foreach(KeyConfig kc in defaultKeys){
			if( Tabulate && kc.Tab != _tabList[_currentTab].ToString()) continue;
				if(al.Contains(kc.Name)) continue;
				
				configsInPage++;
				
				if(kc.isAxis && kc._biDirLink != null ) al.Add(kc._biDirLink.Name);
		}		
		
	
		
		//clamp the nav row to the acceptable region
		int maxRow = configsInPage + 1 + (Tabulate ? 1 : 0);
		_navRow %= maxRow;
		if(_navRow < 0) _navRow += maxRow;
		
		//find the max col position
		int maxCol = 1;
		
		int tempRow = _navRow + (Tabulate ? 0 : 1);
		
		//figure maxCol for the current step
		if(tempRow == 0)
		{
			maxCol = _tabList.Count; 
		}
		else if(tempRow >= 1 && tempRow + (Tabulate ? 1 : 0) < maxRow)
		{
			maxCol = 2;
		}
		else if(tempRow + (Tabulate ? 1 : 0) == maxRow)
		{
			maxCol = 3 + (ShowDuplicatesToggle ? 1 : 0);
		}
			
		
		//clamp the nav col to the acceptable region
		_navCol %= maxCol;
		if(_navCol < 0) _navCol += maxCol;
		
		print("row : " + _navRow + " - col : " + _navCol + " - temprow : " + tempRow);
		print("MaxRow : " + maxRow + " - MaxCol : " + maxCol);
		//print(configsInPage + " : " + tempRow);
	}
	#endregion
	
	#region Missing Key Checks
	
	bool isMissingInput( KeyConfig kc )
	{
		return isMissingJoystickInput(kc) && isMissingKeyboardInput(kc);
	}
	
	// checks if the config or its bidirectional link are missing any inputs
	bool isMissingJoystickInput( KeyConfig kc )
	{
		for(int i = 0 ; i < 2 ; i ++)
		{
			if( kc == null ) return false;
			if( kc.JoystickAxis == JoyAnalog.None )
			{
				if(kc._joyKey == (int) JoyCode.None || kc._negJoyKey == (int) JoyCode.None)
				{
					return true;
				}
			}
			kc = kc._biDirLink;
		}
		
		return false;
	}
	
	// checks if the config or its bidirectional link are missing any inputs
	bool isMissingKeyboardInput( KeyConfig kc )
	{
		for(int i = 0 ; i < 2 ; i ++)
		{
			if( kc == null ) return false;
			if( kc.MouseAxis == MouseAxes.None )
			{
				if( (kc._key == (int) KeyCode.None && kc._mouseKey == -1) || (kc._negKey == (int) KeyCode.None && kc._negMouseKey == -1) )
				{
					return true;									
				}
			}
		}
		
		return false;
	}
	
	#endregion
	
	#region Display Error
	
	void DisplayError(string e)
	{
		
		
	}
	
	#endregion
	
	#region Singleton Functions
	//returns a reference to this manager - and creates it if it was never created
	public static NewCustomInputManager GetReference(){
		if(_selfReference == null){
			_selfReference = new GameObject("InputManager").AddComponent<NewCustomInputManager>();
		}
		return _selfReference; 
	}	
	public static NewCustomInputManager Get(){ return GetReference(); }
	#endregion
}

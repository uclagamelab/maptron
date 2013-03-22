using UnityEngine;
using System.Collections;

/****************************************
 * Controller Manager
 * Made to be used with the Custom Input Manager, this allows access
 * to the controllers attached to the computer, automatically assigns a template
 * and allows access to the controller's buttons throgh a generic interface
 ***************************************/

//Issues:
//	- allow passed Joycode inputs that represent analog directions to be used to get a "buttondown" call etc (maybe - causes issues with button down and button up etc)
//  X test with other controllers
//  - allow for dynamic detection of missing controllers
//	* consider removing joyButton and converting over to JoyCode for consistency
//  - consider removing the DUp/Down/Left/Right parameters from the button template array because they are never used (so far)
//  - Create an editor script that fills in the added controller axes
//  ? figure out why the attached controllers are in different orders some times
//	? allow for custom templates - what would the purpose be?
//  ? allow controllers to be added in odd orders when a button is pressed ( ie, press a button to join) and to be removed
//  X remove analog references to specific axes in const and inputmanager axis names

// - notify of conflicts - multiple key
// 		- popup (and error pops up in the bottom right with notifications bout errors - popup cannot lock background gui)
// X get rid of "controller axes toggle" button
//		- bundle two configs together
// - three columns
//		X keyboard
//		- mouse/wheel (mouse and wheel as axis)
//		X joystick
// - add graphics

// - how to set up each kind of controller on a mac vs a pc
// - MAC/PC logitech wired (plug in on pc)
// - MAC/PC logitech wireless
// - MAC/PC PS3 wired ?
// - MAC/PC PS3 wireless (wired not possible on pc)
// - MAC/PC XBOX wired ? (easy on pc)
// - MAC/PC XBOX wireless (easy on pc)

// TODO: Allow the user to input their own templates

//allow for refinding of of controllers 

// Custom keycode alternatives for controllers - functions give back player number, aswell
public enum JoyCode
{
	None = 0,
	
	//Digital
	Start = -1,
	Select = -2,
	A = -3,
	B = -4,
	X = -5,
	Y = -6,
	
	LeftBumper = -7,
	RightBumper = -8,
	
	LeftStickClick = -9,
	RightStickClick = -10,
	
	//Analog used as digital
	DPadUp = -11,
	DPadDown = -12,
	DPadLeft = -13,
	DPadRight = -14,
	
	LeftTrigger = -15,
	RightTrigger = -16,
	
	LeftStickUp = -17,
	LeftStickDown = -18,
	LeftStickLeft = -19,
	LeftStickRight = -20,
	
	RightStickUp = -21,
	RightStickDown = -22,
	RightStickLeft = -23,
	RightStickRight = -24,
}

// Analog joystick type
public enum JoyAnalog
{
	None = 0,
	DPadHorizontal = 15,
	DPadVertical,
	
	LeftTrigger,
	RightTrigger,
	
	LeftStickHorizontal,
	LeftStickVertical,
	
	RightStickHorizontal,
	RightStickVertical	
}

//Class that normalizes controller input across a variety of controllers
public class ControllerManager : MonoBehaviour{
	
	//todo: Rid need for this
	
	//available joypad inputs
	public enum JoyButton {
		//Digital
		Start = 1, Select,
		A, B, X, Y,
		LBumper, RBumper,

		LJoyClick, RJoyClick,
		
		//Analog
		DPadUp, DPadDown, DPadLeft, DPadRight,
		DPadVert, DPadHoriz,

		LTrigger, RTrigger,
		LJoyHoriz, LJoyVert,
		RJoyHoriz, RJoyVert,
	};
	
	//Analog Input values
	const int YAXIS = -1;
	const int XAXIS = -2;
	
	const int AXIS5 = -3;
	const int AXIS4 = -4;
	
	const int AXIS9 = -5;
	const int AXIS10 = -6;
	
	const int AXIS7 = -7;
	const int AXIS6 = -8;
	
	const int AXIS3 = -9;
	const int AXIS8 = -10;
	
	//returns the axis name of the associated constant
	static string GetAxisName(int a){
		switch(a){
		case YAXIS:
			return "_YAxis";
		case XAXIS:
			return "_XAxis";
		case AXIS3:
			return "_Axis3";
		case AXIS4:
			return "_Axis4";
		case AXIS5:
			return "_Axis5";
		case AXIS6:
			return "_Axis6";
		case AXIS7:
			return "_Axis7";
		case AXIS8:
			return "_Axis8";
		case AXIS9:
			return "_Axis9";
		case AXIS10:
			return "_Axis10";
		}
		return "";
	}
	
	
	//set of buttons for controllers
	public class JoyButtonSet{
		string type; //name for the controller
		int[] _keys; //array of key mappings
		//string[] _keynames; //array of the names of keys associated with the given controller
		
		public bool Template; //whether or not this is a template
		
		//values used to invert analog axes
		public int LJoyHorizMult = 1;
		public int LJoyVertMult = 1;
		public int RJoyHorizMult = 1;
		public int RJoyVertMult = 1;
		public int DPadHorizMult = 1;
		public int DPadVertMult = 1;
		
		
		//contstructor to setup key mapping
		public JoyButtonSet(
			string controller_type,
			int Start,int Select,
			int A,int B,int X,int Y,
			int LBumper,int RBumper,
			int LJoyClick, int RJoyClick,
			int LTrigger,int RTrigger,
			int LJoyHoriz,int LJoyVert,
			int RJoyHoriz,int RJoyVert,
			int DPadHoriz,int DPadVert,
			int DPadUp,int DPadDown,int DPadLeft,int DPadRight
		){
			type = controller_type;
			
			//TODO: this has been artificially increased in size so that regular joycodes can be retrieved
			_keys = new int[25];
			//_keynames = new string[25];
			
			
			
			_keys[(int)JoyButton.Start ] = Start;
			_keys[(int)JoyButton.Select ] = Select;
			_keys[(int)JoyButton.A ] = A;
			_keys[(int)JoyButton.B ] = B;
			_keys[(int)JoyButton.X ] = X;
			_keys[(int)JoyButton.Y ] = Y;
			
			_keys[(int)JoyButton.LJoyClick ] = LJoyClick;
			_keys[(int)JoyButton.RJoyClick ] = RJoyClick;
			
			_keys[(int)JoyButton.LBumper ] = LBumper;
			_keys[(int)JoyButton.RBumper ] = RBumper;
			_keys[(int)JoyButton.LTrigger ] = LTrigger;
			_keys[(int)JoyButton.RTrigger ] = RTrigger;
			
			_keys[(int)JoyButton.LJoyHoriz ] = LJoyHoriz;
			_keys[(int)JoyButton.LJoyVert ] = LJoyVert;
			_keys[(int)JoyButton.RJoyHoriz ] = RJoyHoriz;
			_keys[(int)JoyButton.RJoyVert ] = RJoyVert;
			
			_keys[(int)JoyButton.DPadVert ] = DPadVert;
			_keys[(int)JoyButton.DPadHoriz ] = DPadHoriz;
			
			_keys[(int)JoyButton.DPadUp ] = DPadUp;
			_keys[(int)JoyButton.DPadDown ] = DPadDown;
			_keys[(int)JoyButton.DPadLeft ] = DPadLeft;
			_keys[(int)JoyButton.DPadRight ] = DPadRight;
			
			_controllerTemplates.Add(this);
		}
		
		//constructor that allows for inversion of some axes
		public JoyButtonSet(
			string controller_type,
			int Start,int Select,
			int A,int B,int X,int Y,
			int LBumper,int RBumper,
			int LJoyClick, int RJoyClick,
			int LTrigger,int RTrigger,
			int LJoyHoriz,int LJoyVert,
			int RJoyHoriz,int RJoyVert,
			int DPadHoriz,int DPadVert,
			int DPadUp,int DPadDown,int DPadLeft,int DPadRight,
			
			bool InvertLStickHoriz, bool InvertLStickVert,
			bool InvertRStickHoriz, bool InvertRStickVert,
			bool InvertDPadHoriz, bool InvertDPadVert
		):this(
				controller_type,
				Start,Select,
				A,B,X,Y,
				LBumper,RBumper,
				LJoyClick, RJoyClick,
				LTrigger,RTrigger,
				LJoyHoriz,LJoyVert,
				RJoyHoriz,RJoyVert,
				DPadHoriz,DPadVert,
				DPadUp,DPadDown,DPadLeft,DPadRight
			)
		{		
			LJoyHorizMult = InvertLStickHoriz ? -1 : 1;
			LJoyVertMult = InvertLStickVert ? -1 : 1;
			RJoyHorizMult = InvertRStickHoriz ? -1 : 1;
			RJoyVertMult = InvertRStickVert ? -1 : 1;
			DPadHorizMult = InvertDPadHoriz ? -1 : 1;
			DPadVertMult = InvertDPadVert ? -1 : 1;
		}
		
		//copy constructor
		public JoyButtonSet(JoyButtonSet prev){
			//copy type
			type = prev.type;
			//copy key mappings
			_keys = new int[ prev._keys.Length ];
			for(int i = 0 ; i < _keys.Length ; i ++){
				_keys[ i ] = prev._keys[ i ];
			}
			//copy names
			/*_keynames = new string[ prev._keynames.Length ];
			for(int i = 0 ; i < _keynames.Length ; i ++){
				_keynames[ i ] = prev._keynames[ i ];
			}*/
			//copy inversions
			LJoyHorizMult = prev.LJoyHorizMult;
			LJoyVertMult = prev.LJoyVertMult;
			RJoyHorizMult = prev.RJoyHorizMult;
			RJoyVertMult = prev.RJoyVertMult;
			DPadHorizMult = prev.DPadHorizMult;
			DPadVertMult = prev.DPadVertMult;
		}
		
		/*
		//set the custom names for each key
		public void SetNames(
			string Start,string Select,
			string A,string B,string X,string Y,
			string LBumper,string RBumper,
			string LJoyClick, string RJoyClick,
			string LTrigger,string RTrigger,
			string LJoyHoriz,string LJoyVert,
			string RJoyHoriz,string RJoyVert,
			string DPadHoriz,string DPadVert,
			string DPadUp,string DPadDown,string DPadLeft,string DPadRight
		){			
			_keynames[(int)JoyButton.Start ] = Start;
			_keynames[(int)JoyButton.Select ] = Select;
			_keynames[(int)JoyButton.A ] = A;
			_keynames[(int)JoyButton.B ] = B;
			_keynames[(int)JoyButton.X ] = X;
			_keynames[(int)JoyButton.Y ] = Y;
			
			_keynames[(int)JoyButton.LJoyClick ] = LJoyClick;
			_keynames[(int)JoyButton.RJoyClick ] = RJoyClick;
			
			_keynames[(int)JoyButton.LBumper ] = LBumper;
			_keynames[(int)JoyButton.RBumper ] = RBumper;
			_keynames[(int)JoyButton.LTrigger ] = LTrigger;
			_keynames[(int)JoyButton.RTrigger ] = RTrigger;
			
			_keynames[(int)JoyButton.LJoyHoriz ] = LJoyHoriz;
			_keynames[(int)JoyButton.LJoyVert ] = LJoyVert;
			_keynames[(int)JoyButton.RJoyHoriz ] = RJoyHoriz;
			_keynames[(int)JoyButton.RJoyVert ] = RJoyVert;
			
			_keynames[(int)JoyButton.DPadVert ] = DPadVert;
			_keynames[(int)JoyButton.DPadHoriz ] = DPadHoriz;
			
			_keynames[(int)JoyButton.DPadUp ] = DPadUp;
			_keynames[(int)JoyButton.DPadDown ] = DPadDown;
			_keynames[(int)JoyButton.DPadLeft ] = DPadLeft;
			_keynames[(int)JoyButton.DPadRight ] = DPadRight;			
		}*/
		
		//returns the name of the controller template
		public string GetName(){ return type; }
		
		//returns the custom names of the buttons
		public string GetButtonName(int j){ return ""; /*return _keynames[ (int) j ];*/ }
		public string GetButtonName(JoyCode j){ return GetButtonName( -(int) j );}
		public string GetButtonName(JoyButton j){ return GetButtonName( (int) j );}
		
		//returns the offset for the controller button
		public int GetKeyCodeOffset(int j){ return _keys[ (int)j ]; }
		public int GetKeyCodeOffset(JoyCode j){ return GetKeyCodeOffset(-(int)j); }
		public int GetKeyCodeOffset(JoyButton j){ return GetKeyCodeOffset((int)j); }
		
		//looks up the key offset number (0-19) and returns the associated joycode for this controller
		public JoyCode GetJoyCode(int jk){
			for(int i = 1 ; i < _keys.Length ; i ++){
				if(_keys[i] == jk) return (JoyCode)(-i);
			}
			return 0;
		}
	}
	
	/*** ALL REFERENCES TO KEYS HERE ARE HANDLED AS OFFSETS FROM KeyCode.JoystickButton0 ***/
	
	//interface for getting data about a joypad
	public class JoyInterface{
		int _keySet;
		ControllerManager.JoyButtonSet _buttons;
		
		string LTrig="";
		string RTrig="";
		string LJoyV="";
		string LJoyH="";
		string RJoyV="";
		string RJoyH="";
		string DPadV="";
		string DPadH="";
		
		// used to store whether a button is pressed or not so key up and keydown values can be sent -- uses joycodes?
		bool[] _pressed = new bool[25];
		bool[] _prevPressed = new bool[25];
		
		//used to store when the list was previously updated (update will have to be called at the beginning of each keydown call for joysticks)
		int _lastUpdated = 0;
		
		void SetKeySet(int joystick){
			_keySet = Mathf.Clamp(joystick, 1, 4);
		}
		
		//creates an interface object to get data from
		public JoyInterface( int p , JoyButtonSet j){
			_keySet = p;
			_buttons = new JoyButtonSet(j);
			
			LTrig = GetAxisName(_buttons.GetKeyCodeOffset(JoyButton.LTrigger));
			RTrig = GetAxisName(_buttons.GetKeyCodeOffset(JoyButton.RTrigger));
			
			LJoyV = GetAxisName(_buttons.GetKeyCodeOffset(JoyButton.LJoyVert));
			LJoyH = GetAxisName(_buttons.GetKeyCodeOffset(JoyButton.LJoyHoriz));
			
			RJoyV = GetAxisName(_buttons.GetKeyCodeOffset(JoyButton.RJoyVert));
			RJoyH = GetAxisName(_buttons.GetKeyCodeOffset(JoyButton.RJoyHoriz));
			
			DPadV = GetAxisName(_buttons.GetKeyCodeOffset(JoyButton.DPadVert));
			DPadH = GetAxisName(_buttons.GetKeyCodeOffset(JoyButton.DPadHoriz));
		}
		
		//updates the buttons states in the _pressed arrays so that "keydown" and "keyup" etc can be used -- Unity does not provide this functionality
		public void UpdateButtons(){
			if(_lastUpdated == Time.frameCount) return;
			_lastUpdated = Time.frameCount;

			int val = 0;
			foreach(JoyCode jc in JoyCode.GetValues(typeof(JoyCode))){
				if(jc == JoyCode.None) continue;
				//negative here because all joycode values are <= to 0
				val = -(int) jc;
								
				_prevPressed[val] = _pressed[val];				
				//code copied from old GetButton function
				if(_buttons.GetKeyCodeOffset(val) < 0) _pressed[val] = false;
				else _pressed[val] = Input.GetKey( KeyCode.JoystickButton0 + _buttons.GetKeyCodeOffset(val) + _keySet * 20);
			}
			
			//update the analog buttons to be used as triggers
			
			//Triggers
			_pressed[-(int)JoyCode.LeftTrigger] = GetLeftTrigger() > .9f;
			_pressed[-(int)JoyCode.RightTrigger] = GetRightTrigger() > .9f;
			
			//dpad
			_pressed[-(int)JoyCode.DPadUp] = GetDPadVert() > .9f;
			_pressed[-(int)JoyCode.DPadDown] = GetDPadVert() < -.9f;
			
			_pressed[-(int)JoyCode.DPadRight] = GetDPadHoriz() > .9f;
			_pressed[-(int)JoyCode.DPadLeft] = GetDPadHoriz() < -.9f;
			
			//left stick
			_pressed[-(int)JoyCode.LeftStickUp] = GetLeftStickVert() > .9f;
			_pressed[-(int)JoyCode.LeftStickDown] = GetLeftStickVert() < -.9f;
			
			_pressed[-(int)JoyCode.LeftStickRight] = GetLeftStickHoriz() > .9f;
			_pressed[-(int)JoyCode.LeftStickLeft] = GetLeftStickHoriz() < -.9f;
			
			//right stick
			_pressed[-(int)JoyCode.RightStickUp] = GetRightStickVert() > .9f;
			_pressed[-(int)JoyCode.RightStickDown] = GetRightStickVert() < -.9f;
			
			_pressed[-(int)JoyCode.RightStickRight] = GetRightStickHoriz() > .9f;
			_pressed[-(int)JoyCode.RightStickLeft] = GetRightStickHoriz() < -.9f;
		}
		
		/*** DPad ***/
		//returns a vector with info about the vertical and horizontal DPad axis
		//x: horizontal , y: vertical
		public float GetDPadHoriz(){
			return (DPadH != "")? Input.GetAxis(_keySet + DPadH) * _buttons.DPadHorizMult : (- (GetButton(JoyCode.DPadLeft)?1:0) + (GetButton(JoyCode.DPadRight)?1:0));
		}
		public float GetDPadVert(){
			return (DPadV != "")? Input.GetAxis(_keySet + DPadV) * _buttons.DPadVertMult : (- (GetButton(JoyCode.DPadDown)?1:0) + (GetButton(JoyCode.DPadUp)?1:0));
		}
		public Vector2 GetDPad(){
			return new Vector2(GetDPadHoriz(),GetDPadVert());
		}
		
		/*** Left Stick ***/
		//returns a vector with info about the vertical and horizontal Left Joy axis
		public float GetLeftStickHoriz(){
			return (LJoyH != "")? Input.GetAxis(_keySet + LJoyH) * _buttons.LJoyHorizMult : 0.0f; 
		}
		public float GetLeftStickVert(){
			return (LJoyV != "")? Input.GetAxis(_keySet + LJoyV) * _buttons.LJoyVertMult : 0.0f;
		}
		public Vector2 GetLeftStick(){
			return new Vector2( GetLeftStickHoriz(),GetLeftStickVert());
		}
		
		/*** Right Stick ***/
		//returns a vector with info about the vertical and horizontal Right Joy axis
		public float GetRightStickHoriz(){
			return (RJoyH != "")? Input.GetAxis(_keySet + RJoyH) * _buttons.RJoyHorizMult : 0.0f;
		}
		public float GetRightStickVert(){
			return (RJoyV != "")? Input.GetAxis(_keySet + RJoyV) * _buttons.RJoyVertMult : 0.0f;
		}
		public Vector2 GetRightStick(){
			return new Vector2(GetRightStickHoriz(), GetRightStickVert());
		}
		
		/*** Triggers ***/
		//returns a vector with info about the left and right trigger axis
		//x: left , y: right
		public float GetLeftTrigger(){
			return (LTrig != "")? Input.GetAxis(_keySet + LTrig) : GetButton(JoyCode.LeftTrigger)?1:0;
		}
		public float GetRightTrigger(){
			return (RTrig != "")? Input.GetAxis(_keySet + RTrig) : GetButton(JoyCode.RightTrigger)?1:0;
		}
		public Vector2 GetTriggers(){
			return new Vector2( GetLeftTrigger(), GetRightTrigger());
		}
		
		//returns the analog value associated with the analog inputs
		public float GetAnalogValue(JoyAnalog ja){		
			switch(ja){
			case JoyAnalog.DPadHorizontal:
				return GetDPadHoriz();
			case JoyAnalog.DPadVertical:
				return GetDPadVert();
			case JoyAnalog.LeftStickHorizontal:
				return GetLeftStickHoriz();
			case JoyAnalog.LeftStickVertical:
				return GetLeftStickVert();
			case JoyAnalog.RightStickHorizontal:
				return GetRightStickHoriz();
			case JoyAnalog.RightStickVertical:
				return GetRightStickVert();
			case JoyAnalog.LeftTrigger:
				return GetLeftTrigger();
			case JoyAnalog.RightTrigger:
				return GetRightTrigger();
			}
			return 0.0f;
		}
		
		
		
		
		/*** Get Buttons States ***/
		// returns if a button is held, released, or pressed that frame
		public bool GetButton(JoyCode jb){
			UpdateButtons();
			int val = Mathf.Abs((int)jb);
			return _pressed[val];
			
			//if(_buttons.GetKeyCodeOffset(val) < 0) return false;
			//return Input.GetKey( KeyCode.JoystickButton0 + _buttons.GetKeyCodeOffset(val) + _player * 20);
		}
		public bool GetButtonDown(JoyCode jb){ 
			UpdateButtons();
			int val = Mathf.Abs((int)jb);
			return _pressed[val] && !_prevPressed[val];
		}
		public bool GetButtonUp(JoyCode jb){
			UpdateButtons();
			int val = Mathf.Abs((int)jb);
			return !_pressed[val] && _prevPressed[val];
		}
		
		//looks up the key offset number (0-19) and returns the associated joycode for this controller
		public JoyCode ReverseLookup (int jk){
			return _buttons.GetJoyCode(jk);
		}
		
		//returns the custom names of the buttons
		public string GetButtonName (JoyCode j){
			string name = _buttons.GetButtonName((JoyCode)j);
			return (name == "")? j.ToString() : name;
		}
		public string GetButtonName (JoyAnalog j){
			string name = _buttons.GetButtonName((int)j);
			return (name == "")? j.ToString() : name;
		}
 	}
	
	[System.Serializable]
	public class JoystickCreationTemplate{
		
		[System.Serializable]
		public class ButtonInfo{
			public int buttonNumber;
			public bool exists;
		}
		
		[System.Serializable]
		public class AxisInfo{
			public int axisNumber;
			public bool exists;
			public bool invertAxis;
		}
		
		public string name;
		
		public ButtonInfo Start;
		public ButtonInfo Select;
		public ButtonInfo BottomFaceButton;
		public ButtonInfo RightFaceButton;
		public ButtonInfo TopFaceButton;
		public ButtonInfo LeftFaceButton;
		
		public ButtonInfo LeftBumper;
		public ButtonInfo LeftTrigger;
		public ButtonInfo RightBumper;
		public ButtonInfo RightTrigger;
		
		public ButtonInfo DPadUp;
		public ButtonInfo DPadDown;
		public ButtonInfo DPadLeft;
		public ButtonInfo DPadRight;
		
		public ButtonInfo LeftStickClick;
		public ButtonInfo RightStickClick;
		
		public AxisInfo LeftStickHorizontalAxis;
		public AxisInfo LeftStickVerticalAxis;
		
		public AxisInfo RightStickHorizontalAxis;
		public AxisInfo RightStickVerticalAxis;
		
		public AxisInfo DPadHorizontalAxis;
		public AxisInfo DPadVerticalAxis;
		
		public AxisInfo LeftTriggerAxis;
		public AxisInfo RightTriggerAxis;
	}
	
	public JoystickCreationTemplate[] Templates;
	
	
	
	
	
	
	
	
	int _playerCount = 0;
	JoyInterface[] _players = new JoyInterface[4];
	static ArrayList _controllerTemplates = new ArrayList();
	
	/****** TEMPLATES ***********/
	//Templates for a variety of controllers
	
	//controller_type, Start, Select, A, B, X, Y, LBumper, RBumper, LJoyClick, RJoyClick, LTrigger, RTrigger, LJoyHoriz, LJoyVert, RJoyHoriz, RJoyVert, DPadHoriz, DPadVert, DPadUp, DPadDown, DPadLeft, DPadRight, inverLStickH, invertLStickV, invertRStickH, invertRStickV, invertDPadH, invertDPadV
	JoyButtonSet XBOX = new JoyButtonSet("XBOX", 										 7,6,0,1,2,3,4,5,8,9,AXIS9,AXIS10,XAXIS,YAXIS, AXIS4, AXIS5, AXIS6, AXIS7,-1,-1,-1,-1);
	//JoyButtonSet LOGITECH_DUAL_ACTION = new JoyButtonSet("Logitech Dual Action", 		 9,8,1,2,0,3,4,5,10,11,6,7,XAXIS,YAXIS, AXIS3, AXIS4, AXIS5, AXIS6,-1,-1,-1,-1, false, false, false, true, true, false);
	//JoyButtonSet LOGITECH_RUMBLEPAD2 = new JoyButtonSet("Logitech Cordless RumblePad 2", 9,8,1,2,0,3,4,5,10,11,6,7,XAXIS,YAXIS, AXIS3, AXIS4, AXIS5, AXIS6,-1,-1,-1,-1, false,false,false,true,true,false);
	//JoyButtonSet PS3 = new JoyButtonSet("PLAYSTATION(R)3 Controller", 				 9,8,1,2,0,3,4,5,10,11,6,7,XAXIS,YAXIS, -1 ,AXIS4, AXIS5 ,AXIS6, -1,-1,-1,-1);
	//JoyButtonSet COOLING = new JoyButtonSet("PS3/USB Corded Gamepad", 					 9,8,1,2,0,3,4,5,10,11,6,7,XAXIS,YAXIS, AXIS3, AXIS4, AXIS5, AXIS6, -1,-1,-1,-1, false, false, false, true, true, false);
	
	string[] _oldNames;
	int[] _playerAssignment = {1,2,3,4};
	
	/****** CLASS FUNCTIONS ****/
	static ControllerManager _selfReference = null;
	void Awake(){
		_selfReference = this;
	}
	void Start(){
		
		//convert the created templates int buttonsets
		foreach( JoystickCreationTemplate jct in Templates ){
			//set up the button axes and configurations
			JoyButtonSet j = new JoyButtonSet(
				jct.name,
				
				(jct.Start.exists) ? jct.Start.buttonNumber : -1,
				(jct.Select.exists) ? jct.Select.buttonNumber : -1,
				
				(jct.BottomFaceButton.exists) ? jct.BottomFaceButton.buttonNumber : -1,
				(jct.RightFaceButton.exists) ? jct.RightFaceButton.buttonNumber : -1,
				(jct.LeftFaceButton.exists) ? jct.LeftFaceButton.buttonNumber : -1,
				(jct.TopFaceButton.exists) ? jct.TopFaceButton.buttonNumber : -1,
				(jct.LeftBumper.exists) ? jct.LeftBumper.buttonNumber : -1,
				(jct.RightBumper.exists) ? jct.RightBumper.buttonNumber : -1,
				(jct.LeftStickClick.exists) ? jct.LeftStickClick.buttonNumber : -1,
				(jct.RightStickClick.exists) ? jct.RightStickClick.buttonNumber : -1,
				(jct.LeftTrigger.exists) ? jct.LeftTrigger.buttonNumber : -1,
				(jct.RightTrigger.exists) ? jct.RightTrigger.buttonNumber : -1,
				
				(jct.LeftStickHorizontalAxis.exists) ? jct.LeftStickHorizontalAxis.axisNumber : -1,
				(jct.LeftStickVerticalAxis.exists) ? jct.LeftStickVerticalAxis.axisNumber : -1,
				(jct.RightStickHorizontalAxis.exists) ? jct.RightStickHorizontalAxis.axisNumber : -1,
				(jct.RightStickVerticalAxis.exists) ? jct.RightStickVerticalAxis.axisNumber : -1,
				(jct.DPadHorizontalAxis.exists) ? jct.DPadHorizontalAxis.axisNumber : -1,
				(jct.DPadVerticalAxis.exists) ? jct.DPadVerticalAxis.axisNumber : -1,
				(jct.DPadUp.exists) ? jct.DPadUp.buttonNumber : -1,
				(jct.DPadDown.exists) ? jct.DPadDown.buttonNumber : -1,
				(jct.DPadLeft.exists) ? jct.DPadLeft.buttonNumber : -1,
				(jct.DPadRight.exists) ? jct.DPadRight.buttonNumber : -1,
				
				jct.LeftStickHorizontalAxis.invertAxis,
				jct.LeftStickVerticalAxis.invertAxis,
				jct.RightStickHorizontalAxis.invertAxis,
				jct.RightStickVerticalAxis.invertAxis,
				jct.DPadHorizontalAxis.invertAxis,
				jct.DPadVerticalAxis.invertAxis
			);
		}
		
		ReassignControllers();
		_oldNames = (string[])Input.GetJoystickNames().Clone();
	}
	
	/*** Update Buttons ***/
	//update the button presses for each joystick
	void Update(){
		//get which controllers have been removed
		string[] newNames = Input.GetJoystickNames();
		if(newNames.Length != _oldNames.Length){
			//IF ONE IS REMOVED
			if(newNames.Length > _oldNames.Length){
				for(int i = 0 ; i < _oldNames.Length ; i ++){
					//if one of the spots does not match, then that must be the spot that was removed
					if(newNames[i] != _oldNames[i]){
						for(int j = i ; j < _oldNames.Length-1 ; j ++){
							_playerAssignment[j] = _playerAssignment[j+1];
						}
						_playerAssignment[3] = 0;
						
						//REMOVED!
					}
				}
				//otherwise, the last controller must be the one that was removed
			//IF ONE IS ADDED
			}else if(newNames.Length < _oldNames.Length){
				for(int i = 0 ; i < newNames.Length ; i ++){
					//if one of hte spots does not match, then that must be the spot that was added
					if(newNames[i] != _oldNames[i]){
						for(int j = i ; j < _oldNames.Length-1 ; j ++){
							_playerAssignment[j+1] = _playerAssignment[j];
						}
						_playerAssignment[i] = 0;
						
						//ADDED!
					}					
				}
				//otherwise, the last controller must be the one that was added
			}
		}
		//Update the buttons for each controller
		foreach(JoyInterface ji in _players){
			if(ji != null) ji.UpdateButtons();
		}
		
		
		for(int i = (int)KeyCode.Joystick1Button0 ; i < (int)KeyCode.Joystick4Button19 ; i ++){
			//if(Input.GetKey((KeyCode)i)) print((KeyCode) i );
		}
		
	}
	
	// reassign controllers to the right type
	void ReassignControllers(){
		_playerCount = 0;
		
		//cycle through every player
		for(int i = 0 ; i < _players.Length ; i ++){
			_players[i] = null;
			if(Input.GetJoystickNames().Length <= i)continue;
			
			AssignController(i + 1, i + 1, Input.GetJoystickNames()[i]);
		}
	}
	
	/*** Assign a new controller to the player number "player", getting key presses from 'keyset', with nname 'controllerName' ***/
	void AssignController(int player, int keyset, string controllerName){
		player --;
		
		//check if the controller matches the the string of the joystick at all
		for(int j = 0 ; j < _controllerTemplates.Count ; j ++){
			//print(player + " : " + _templates[j].GetName() + " - " + controllerName);
			if(_controllerTemplates[j] == null) break;
			
			//if the joystick name matches (or rather, contains) the type of button set
			if( controllerName.Contains( (_controllerTemplates[j] as JoyButtonSet).GetName()) ){
				_players[player] = new JoyInterface(keyset, _controllerTemplates[j] as JoyButtonSet);
				_playerCount++;
				break;
			}
		}
	}
	
	
	//returns the controller for the player p
	public JoyInterface GetController(int p){
		p-=1;
		if(p < 0 || p >= _players.Length) return null;
		return _players[p];
	}
	
	//returns the amount of attached controllers
	public int GetAttachedControllerCount(){ return _playerCount; }
	
	
	
	
	//gets the most recent key pressed
	//player is set to 0 if no key is pressed
	public JoyCode GetKeyPressed(out int player){
		//cycle through each player
		for( int i = 0; i < _players.Length; i ++ ){
			if(_players[i] == null) continue;
			
			//print(i);
			
			//see if they've pressed any one of the keys 
			foreach(JoyCode jc in JoyCode.GetValues(typeof(JoyCode))){
				if(_players[i].GetButtonDown(jc)){
					player = i + 1;
					return jc;
				}
			}
		}
		
		//otherwise, return that they haven't
		player = 0;
		return JoyCode.None;
	}
	
	//returns a reference to this controllerManager
	public static ControllerManager GetReference(){
		if(_selfReference == null) throw new UnityException("Controller Manager is not included in the scene");
		return _selfReference;
	}
	public static ControllerManager Get(){
		return GetReference();
	}
}

using UnityEngine;
using System.Collections;


//User must check if the window is open and disable background GUI elements themselves

// Opens a window that allows the user to change their resolution
public class ResolutionManager : MonoBehaviour {
	// Percentages of the screen that the window takes up
	public float EditorWidthPercent = 1;
	public float EditorHeightPercent = 1;
	
	//Texture for the background of the editor
	public Texture Background = null;
	
	//current Settings
	Resolution _selected;
	bool _fullScreen = true;
	
	//original settings (saved after verification)
	Resolution _origResolution;
	bool _origFullScreen = true;
	
	//variables for resolution verification
	const float VERIFY_TIME = 15.0f;
	bool _verifying = false;
	float _verifyTimeLeft = 0.0f;
	
	//scroll position
	Vector2 _scrollPos = Vector2.zero;
	
	//frame that the editor was opened on
	int _openedFrame = 0;
	
	//whether or not the editor is open
	bool _open = false;

	static ResolutionManager _selfReference = null;
	public void Awake(){
		_selfReference = this;
	}
	
	//tick down time for resolution verification
	public void Update(){
		if(_verifying){
			_verifyTimeLeft -= Time.deltaTime;
			if(_verifyTimeLeft <= 0.0f){ //if not verified in time, revert
				Revert();
			}
		}
	}
	
	//Open the Resolution Editor
	public void OpenEditor(){
		if(_open)return;
		
		//store the current resolution to cehck against
		_open = true;
		_selected = Screen.currentResolution;
		_fullScreen = Screen.fullScreen;
		
		_origResolution = _selected;
		_origFullScreen = _fullScreen;
		_scrollPos = Vector2.zero;
		
		_openedFrame = Time.frameCount;
	}
	public bool isOpen(){return _open;}

	//Close the Resolution Editor
	public void CloseEditor(){
		_open = false;
		if(_verifying)Revert();
	}
	
	//Save Changes to the resolution and begin verification
	void SaveChanges(){
		if(CompareResolution(_selected, _origResolution) && _fullScreen == _origFullScreen) return;
				
		_verifying = true;
		_verifyTimeLeft = VERIFY_TIME;
		Screen.SetResolution(_selected.width, _selected.height, _fullScreen, _selected.refreshRate);
	}
	
	//Reverts to the "original" variables
	void Revert(){
		if(CompareResolution(_selected, _origResolution) && _origFullScreen == _fullScreen) return;
		
		_verifying = false;
		_selected = _origResolution;
		_fullScreen = _origFullScreen;
		
		Screen.SetResolution(_origResolution.width, _origResolution.height, _origFullScreen, _origResolution.refreshRate);
	}
	
	//returns whether or not the resolutions ar ethe same
	bool CompareResolution(Resolution r1, Resolution r2){
		return
			r1.height == r2.height &&
			r1.width == r2.width &&
			r1.refreshRate == r2.refreshRate;
	}
	
	int GetCommonDenominator(int a, int b){
		if( b == 0 ) return a;
		return GetCommonDenominator(b, a%b);
	}
	
	
	/*** Draw the Editor ***/
	void OnGUI(){
		//TODO: If rewritten -- use GUI Groups
		
		if(!_open)return;
		
		if(_openedFrame != Time.frameCount && Input.GetKeyDown(KeyCode.Escape)){
			CloseEditor();
			return;
		}
		
		// Get the Width, Height, and coordinates for the editor
		float w = Screen.width * EditorWidthPercent;
		float h = Screen.height * EditorHeightPercent;
		
		float x = (Screen.width - w)/2;
		float y = (Screen.height - h)/2;
		
		int butHeight = 50;
		
		GUI.Box(new Rect(x,y,w,h),"");
		GUI.Box(new Rect(x,y,w,h),"");
		GUI.Box(new Rect(x,y,w,h),"");
		GUI.Box(new Rect(x,y,w,h),"");
		GUI.Box(new Rect(x,y,w,h),"");
		
		GUI.Label(new Rect(x+10,y+10, w - 20, h - 20), "Change Resolution");

		y += 50;
		h -= 50;
		x += 10;
		w -= 20;
		
		//disable the gui if hte user must decide on the current resolution
		if(_verifying) GUI.enabled = false;

		
		//update the scroll position of hte resolution frame
		float lineHeight = 25;
		float scrollWidth = w;
		_scrollPos = GUI.BeginScrollView(new Rect(x, y, scrollWidth, h-70), _scrollPos, new Rect(0,0,w - 20,lineHeight * Screen.resolutions.Length));

		//Draw all the possible resolutions
		for(int i = 0 ; i < Screen.resolutions.Length ; i ++){	
			Resolution r = Screen.resolutions[i];

			
			int gcd = GetCommonDenominator(r.width, r.height);
						
			
			if(GUI.Button(
					new Rect(0, i * lineHeight, scrollWidth, lineHeight),
					(((CompareResolution(_selected,r))?">":"") + /*r.width/gcd + " x " + r.height/gcd + " - " +*/ r.width + "x" + r.height + ", " + r.refreshRate + "GHz")
				) 
			){
				_selected = r;
			}
						
			if( CompareResolution(_selected,r) ){
				//GUI.Box(new Rect(0,0,xRat * 100, (r.height/(r.width/xRat)) * 100), "");
			}
			
		}
		GUI.EndScrollView();

		
		//draw the cancel, save and close, and reset buttons
		if(GUI.Button(new Rect(x,y + h -butHeight-10, 100, butHeight), "Close")) CloseEditor();
		
		//TODO: Dont allow it to be saved if the resolution has not changed
		if(GUI.Button(new Rect(x + 100,y + h-butHeight-10, 100, butHeight), "Apply")) SaveChanges();

		//draw the toggle button
		_fullScreen = GUI.Toggle (new Rect (x + 200,y + h-30, 200, 50), _fullScreen, "Fullscreen"); //TODO: If the toggle changed, cycle through and get rid of doubles
		
		//Enable the GUI if it was disabled before due to verification
		GUI.enabled = true;
		
		
		//TODO: make this into a separate box on top of the resolution -- overlaps too much if on small res
		/*** VERIFY IF USER WANTS THE CHANGED RESOLUTION ***/
		if(_verifying){
			if(GUI.Button(new Rect(x + w - 100*2, y+h-butHeight-10, 100, butHeight), "Keep Resolution")){
				_verifying = false;
				_origFullScreen = _fullScreen;
				_origResolution = _selected;
			}
			if(GUI.Button(new Rect(x + w - 100, y+h-butHeight-10, 100, butHeight), "Revert")) Revert();
			
			GUI.Label(new Rect(x + w - 100*3, y+h-butHeight-10, 100, butHeight), "Will revert in " + ((int)_verifyTimeLeft).ToString() + "s");
		}
	}
	
	//returns a reference ot the manager
	public static ResolutionManager GetReference(){
		return _selfReference;
	}
	public static ResolutionManager Get(){
		return GetReference();
	}
	
}

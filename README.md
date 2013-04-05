Game Lab Custom Input Manager
===

Overview
===
The gamelab custom input manager is built to allow developers to create a keybinding window within their game
without too much hassle. It is also built to abstract a controllers input so it is consistant across a variety of joystick
types.

Set Up
===
The controller manager relies on Unity's built in Input Manager to access axis information from the plugged in
controllers. The "input.asset" file must included in the project for the controllermanager to function. Replacing the
existing file will remove any pre existing key presets, so be sure to back up or take note of any needed input
settings before doing this.


ControllerManager Class
===
The Controller Manager abstracts axis and button input between controllers so controls are consistent

ControllerManager API
===
enum JoyCode
---
JoyCode is a custom equivelant of KeyCode that generalizes binary input across controllers

enum JoyAnalog
---
JoyAnalog is a custom equivelant of KeyCode that generalizes analog input across controllers

class JoyInterface
---
An object that stores input information about a controller in a single contained package and exposes functions to
access that information

float JoyInterface.GetDPadHoriz()
---
float JoyInterface.GetDPadVert()
---
Vector2 JoyInterface.GetDPad()
---
returns the values for both the vertical and horizontal DPad inputs. 
Vector2.x holds the horizontal value while Vector2.y holds the vertical one

float JoyInterface.GetLeftStickHoriz()
---
float JoyInterface.GetLeftStickVert()
---
Vector2 JoyInterface.GetLeftStick()
---
returns the input values for the left joystick in a Vector2.
x holds the horizontal input while y holds the vertical input

float JoyInterface.GetRightStickHoriz()
---
float JoyInterface.GetRightStickVert()
---
Vector2 JoyInterface.GetRightStick()
---
returns the input values for the right joystick in a Vector2.
x holds the horizontal input while y holds the vertical input

float JoyInterface.GetLeftTrigger()
---
float JoyInterface.GetRightTrigger()
---
Vector2 JoyInterface.GetTriggers()
---
returns the analog values for the trigger inputs in a Vector2
x holds the left trigger input while y holds the right trigger input

float JoyInterface.GetAnalogValue(JoyAnalog)
---
returns the analog value associated with the JoyAnalog input passed to the function

bool JoyInterface.GetButton(JoyCode)
---
returns whether or not the passed binary input is being pressed

bool JoyInterface.GetButtonDown(JoyCode)
---
returns whether or not the passed binary input was pressed down on the current frame

bool JoyInterface.GetButtonUp(JoyCode)
---
returns whether or not the passed binary input was released on the current frame

JoyCode JoyInterface.ReverseLookup(int)
---
takes the KeyCode joystickbutton number and returns the JoyCode associated with that number for that controller

string JoyInterface.GetButtonName(JoyCode), (JoyAnalog)
---
returns the custom name associated with the given input

JoyInterface GetController(int)
---
returns the joystick interface object associated with the passed player. Returns null if no controller

int GetAttachedControllerCount()
---
returns the amount of controllers attached to the computer

JoyCode GetKeyPressed(out int)
---
returns the most recent binary input pressed and places the player that pressed it in the output parameter

GLInputManager Class
===
The GL Input Manager serves the same role as Unity's built in input manager, but also allows for opening a window
in which the user can bind their own custom keyboard and controller keys

GLInputManager API
===

class KeyConfig
---

bool GetKeyUp(string), (KeyCode), (JoyCode,int)
---
returns whether or not the given key was just released. Takes a KeyConfig name and checks all keys, a KeyCode, or a joystick button and a player

bool GetKeyDown(string), (KeyCode), (JoyCode,int)
---
return whether or not the given key was just pressed. Takes a KeyConfig name and checks all keys, a KeyCode, or a joystick button and a player

bool GetKey(string), (KeyCode), (JoyCode,int)
---
returns whether or not the given key is currently pressed. Takes a KeyConfig name and checks all keys, a KeyCode, or a joystick button and a player

float GetAxis(string)
---
returns the analog value associated with the keyconfig with the given name

bool IsAxis(string)
---
returns whether or not a KeyConfig exists with the given name

bool GetMouseButton(int)
---
returns whether or not the mouse button with the given ID is currently pressed

bool GetMouseButtonDown(int)
---
returns whether or not the mouse button with the given ID was just pressed

bool GetMouseButtonUp(int)
---
returns whether or not the mouse button with the given ID was just released

Vector2 GetMousePosition()
---
returns the mouse's current position

ResolutionManager Class
===
The Resolution Manager class allows the player to change the resolution during run time of the game instead of using
the popup at the beginning of running a unity game.

ResolutionManager API
===




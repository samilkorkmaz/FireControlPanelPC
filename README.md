# Fire Control Panel PC
Windows 8+, C#, .NET 9.0, Visual Studio 2022

[Developer diary](https://docs.google.com/document/d/1mTkX9o0rhzWKpD7OGl0VyvK-jbmuOlqKo6SBZcMANt4/edit?tab=t.0#heading=h.ntkd19l6sz4o)

Features:
1. Detects COM port to which fire control panel is connected by sending a command to all available COM ports. The responding port is the fire control panel port.
2. Periodically sends commands over serial port to fire control panel and visualizes reponses.

To enable emulator for testing without fire control panel hardware:
1. If you haven't already, install null modem emulator [com0com](https://com0com.sourceforge.net/). This will add COM ports:
   
![image](https://github.com/user-attachments/assets/440fd989-0dd7-46f0-82b4-979465a7917b)

2. In FormUser.cs, FormUser_Show(), uncomment **_emulator.Run()**
3. In FireControlPanelEmulator.cs, set **RECEIVE_PORT** to COM3 or COM5, depending on your configuration.

![image](https://github.com/user-attachments/assets/d3de373b-b979-4e48-ac76-e44f0c120fc2)

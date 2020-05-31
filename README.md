# Licence Work 

CURRENT PROGRESS LOG: 

- Single player parameters refactoring
- Added thread locks for the safety of the python server variables
- Generated Unity and python server build
- Added command line args for the python server
- DOCUMENTATION: Python Client code and some Python server classes
- Unity asset material change in order to improve render complexity
- Simple Unity player movement animation and look at target 
- Fixed some map related bugs on the MapGenerator class in unity client + python server
- Added dfs check on the single player map generation
- FIRST GAME PLAYED ON SINGLE PLAYER AGAINS 3 BOTS
- Game manager refactoring + integration of the single player manager in the game flow
- Unity single player screen updated, allows user to choose number of bots, obstacles, pumpkins
- Single player socket manager implemented
- FIRST GAME PLAYED WITH 4 PLAYERS ON LAN (DIFFERENT COMPUTERS)
- FIRST GAME PLAYED WITH COMPLETE FLOW 1 PLAYER VS 1 BOT
- Changed the Game Manager to allow more than one bot when singleplayer
- Added parsing function for the python client and spectate mode
- Fixed the parsing method (Errors due to the '\n')
- Fixed some disconnection issues on the python server + handeled a server crash when players disconnect in late game
- Process commands from the server on the Unity client
- Added a function to safely close the python server when necessary
- Improved messages to be more user friendly on python server + client
- Fixed a bug that didn't allow clients to connect on LAN 
- Helper functions to easily parse, combine and print the data on the python server
- Server map generation + DFS check
- Implemented initial map setup for python server (first data packet containing initial game setup, sent by the server to all unity clients before the game starts)
- Fixed the code to support different types of objects on the same place on the map
- Implemented map debugger in game to see easier the entire map while playing
- Implemented bombs logic in the game + particles effect + detonate script
- Better Unity level design for the entire scene.
- Unity Game manager: implemented helper functions and handled some commands from the server + different player types and obstacles spawning
- Game level generation for the unity game 
- Model loading for the unity game and some simple set up for the scene
- Some minimalist Unity UI to check the connection to the python server + user friendly feedback
- Implemented a game manager that ensures the flow of data from unity -> server -> python and backwards and disconnects players if they do not provide input/output
- Implemented a thread that deals with the disconnected players and safely removes them from the server
- Token sending to the unity clients after all the clients from the lobby are ready to play
- Lobby manager implemented in py server to simulate a room for players
- Handled some errors and some stupid scenarios which led to unity clients not responding or broken sockets
- Socket Read & Write implemented on threads for better communication
- Python Server split in classes to track the things better
- Unity Client async pipeline methods to server to avoid freezing the app
- Unity Client sends and receives data from python server
- Unity Client networking, successfully connecting to the python server
- Python server more stable
- Handled some undesired work flows on the server and client
- User friendly messages in the server and client interfaces
- Handling most of the exceptions and errors on python server and client
- Successfully set an python server and client to test a TCP connection which will further be used to connect to the Unity for sending and receiving info.
- Some Unity code to connect to a C# separate server.

## Idea: 

This is a challenge-based training platform  where you can improve your coding skills with fun turn based Unity games.
The main idea is the same as on Codingame Platform: https://www.codingame.com


#### Basic description:

The player opens the Unity Platform and chooses a game. 
Next, the user is able to play offline or online (LAN multiplayer and Online Multiplayer)

OFFLINE: The player will fight agains a hardcoded bot.
ONLINE: The player will be guided to a lobby where it will wait for other opponents. After that, each user will get a token. Each user will use that token in order to successfully connect to the server from a desired programming language and ide.

#### More detailed description:

- The server is started on the Unity Platform while on multiplayer section. 
- User is allowed to enter a lobby or to create one.
- After the lobby is created and the users press the ready button they will receive a token for security reasons. (just to make sure no desired users connect to the same game while the game is running)
- After receiving the token, the Unity Platform switches to the next state where each player will require to test it's connection with the server from the IDE (eg: Visual Studio C++, run the code in order to connect the IDE with the Unity Platform on the specified token)
- When both players successfuly established a connection from the IDE to the Unity Platform, the game can start, otherwise there will be a timeout and players will be returned to the lobby.
- The game starts and the server will send some initial data about the game to each player. Each player will have some amount of time (eg: 100ms) to answer back with an action (eg: "MOVE X Y"). The server will process the commands from the users and will send them to the UI to get some awesome visual output in the current Unity game.
- The process is repeated until one of the players wins or it's a draw.


# Licence Work 

CURRENT PROGRESS: 

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
Next, the user is able to play offline or online (LAN multiplayer)

OFFLINE: the player will fight agains a hardcoded bot.
ONLINE: the player will be guided to a lobby and will be asked to choose an opponent. After that, each user will get a token. Each user will use that token in order to successfully connect to the server from a desired programming language and ide.

#### More detailed description:

- The server is started on the Unity Platform while on multiplayer section. 
- User is allowed to enter a lobby or to create one.
- After the lobby is created and the users press the ready button they will receive a token for security reasons. (just to make sure no desired users connect to the same game while the game is running)
- After receiving the token, the Unity Platform switches to the next state where each player will require to test it's connection with the server from the IDE (eg: Visual Studio C++, run the code in order to connect the IDE with the Unity Platform on the specified token)
- When both players successfuly established a connection from the IDE to the Unity Platform, the game can start, otherwise there will be a timeout and players will be returned to the lobby.
- The game starts and the server will send some initial data about the game to each player. Each player will have some amount of time (eg: 100ms) to answer back with an action (eg: "MOVE X Y"). The server will process the commands from the users and will send them to the UI to get some awesome visual output in the current Unity game.
- The process is repeated until one of the players wins or it's a draw.


# Connect 5
Connect 5 is a game similar to tic tac toe, the difference is that the board is infinite and you have to connect 5 of your X's or O's in a row.

# Overview
The app is still in development.
I used Unity to create the game. The app allows two people to play on a single device. You can zoom in and out with 2 fingers and move around the board using 1 finger.

# Technical Architecture
The board is composed of tile objects, each one displays an empty tile, or a player tick. Each tile is one Unity unit in with and height and are positioned with the center of each being an integer value (ex. The starting tile is positioned at 0,0). This makes it easy to map the position of the tile to a 2d array. The array stores the current state of each tile, whether it's empty or has a mark. Since the board is supposed to be infinite, the 2d array starts at a fixed size, and as the user scrolls around it increases. To improve performance only the visible parts of the board are instantiated. New tiles are added when they come into view, and tiles outside of the view are removed. 

The game saves all player moves and gives the ability to undo, and redo moves. A move is represented by the coordinates of the tile the player chose to put their tic.


<img src="/docs/Connect5Zoom.png" alt="zoom" width="400px"> <img src="/docs/Connect5GamepPlay1.png" alt="play" width="400px"> <img src="/docs/Connect5GamepPlay2.png" alt="play" width="400px">

# How to Setup this Project Locally
You would need to download the free version of Unity to run the game locally. https://unity3d.com/ To Run the game locally clone the source code, then open it with Unity.

<img src="/docs/UnityOpen.PNG" alt="open" width="400px">

Open the Connect5 scene under Assets, then hit the play button.

<img src="/docs/Connect5Scene.png" alt="scene" width="400px"> <img src="/docs/Connect5Run.png" alt="run" width="400px">

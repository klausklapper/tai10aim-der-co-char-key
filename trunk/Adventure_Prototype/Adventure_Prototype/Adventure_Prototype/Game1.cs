/*
 * POINT AND CLICK ADVENTURE FOR DHBW MANNHEIM SE PROJEKT 8
 * CODENAME: ?
 * AUTHORS : LUCAS HILDEBRANDT; CHRISTIAN LOHR; FELIX OTTO
 * ENGINE  : BRUNNEN-G
 * 
 * Main Class. Leave as is!
 * 
 */



/*######################################################################################
 * Imports and uses
 * #####################################################################################
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


using Classes;
using Classes.Dialogues;
using Classes.Graphics;
using Classes.IO;
using Classes.Dev;
using Classes.Pipeline;






//######################################################################################
//Namespace Definition: Make sure to change respective to build status
//######################################################################################
namespace Adventure_Prototype
{





	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{

		//######################################################################################
		//Global Variable Definition
		//######################################################################################
		GraphicsDeviceManager graphics;		//Global graphics card Interface
		SpriteBatch spriteBatch;			//Global spriteBatch used for Drawing
		Player player1;						//Link to Player1
		Player player2;						//Link to Player2
		public Boolean _EDITOR = false;		//Boot up in Editor mode? [SUPPOSED TO BE FALSE FOR RELEASE]



		/// <summary>
		/// Game Constructor
		/// </summary>
		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);		//Initialize our Graphics Card Interface
			Content.RootDirectory = "Content";				//Setting up our Content-Root Directory
			
			//*le setting screen resolution
			graphics.PreferredBackBufferWidth = 1280;
			graphics.PreferredBackBufferHeight = 720;

			

		}





		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{

			//Set Mouse to invisible in Game Mode
			//Setup our Cursor Class (Load images)
			if (!_EDITOR)
			{
				IsMouseVisible = false;
				Cursor.Initialize(this);
			}
			else
			{
				IsMouseVisible = true;
			}
			


			//Setup our Game Reference for use with Instances
			GameRef.Game = this;
			GameRef._EDITOR = this._EDITOR;
			GameRef.Resolution = new Vector2(1280, 720);
			GameRef.AnimationFrames = new Vector2(6, 3);
			

			//Generic Initializing Procedure
			RoomProcessor.Initialize(this);		//Builds our room from source files [.bmap]
			KeyboardEx.Initialize();			//A more powerful keyboard class
			DialogueManager.Initialize();		//Get all Dialogues
			
			//Load up all our fonts
			GraphicsManager.initializeFonts(Content.Load<SpriteFont>("ui_font"), Content.Load<SpriteFont>("big"), GraphicsDevice);
				
			//Initialize Parent
			base.Initialize();
		}





		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{

			// Create a new SpriteBatch, which can be used to draw textures.
			// Assign it to the Graphics Manager
			spriteBatch = new SpriteBatch(GraphicsDevice);
			GraphicsManager.spriteBatch = spriteBatch;
			
			

			//TEMPORARY CREATION OF OUR PLAYER
			//THIS WILL BE CHANGED LATER ON - SO DONT RELY ON IT !!
			if (!_EDITOR)
			{
				Room testRoom = RoomProcessor.createRoomFromFile("Data/Rooms/test.bmap");
				SceneryManager.CurrentRoom = testRoom;
				Texture2D p1Sprite = Content.Load<Texture2D>("Graphics/Charsets/gb_sheet");
				player1 = new Player(this, testRoom, "player01", "Darksvakthaniel", new Animation(p1Sprite.Width, p1Sprite.Height, 6, 3, 0, 0, false), p1Sprite);
				player1.Position = new Vector2(100, 300);
				GameRef.Player1 = player1;
				GraphicsManager.addChild(player1);
				UpdateManager.addItem(player1);
			}
			else
			{
				Editor.Initialize(this);
			}

		}
		
	
		

		
		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}





		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (KeyboardEx.isKeyHit(Keys.Escape))
				this.Exit();


			//SEND OUR PLAYER WALKING IF IN GAME MODE
			// WILL BE CHANGED TO INPUTMANAGER EVENTUALLY
			if (MouseEx.click() && Cursor.CurrentAction == Cursor.CursorAction.walk && !DialogueManager.busy && !_EDITOR )
			{
				player1.setWalkingTarget(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
			}

			//Update our Managers
			UpdateManager.Update(gameTime);
			InputManager.Update();
			MouseEx.Update();
			KeyboardEx.Update();
			DialogueManager.Update();
			

			//Update the Editor
			if (_EDITOR)
				Editor.Update();
			

			base.Update(gameTime);
		}




		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{

			//Let the GM handle the GFXOutput
			GraphicsManager.Draw(gameTime);

			//Editor takes over if necessary
			if (_EDITOR)
				Editor.Draw(this, gameTime);

			//Draw Dialogue
			if (!_EDITOR)
			{
				DialogueManager.draw(GraphicsManager.font02, gameTime );
			}

			//Draw Cursor at last
			if(!_EDITOR)
				Cursor.Draw();
			
			base.Draw(gameTime);
		}
	}
}

//E O F

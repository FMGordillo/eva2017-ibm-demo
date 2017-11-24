using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.Conversation.v1; 	// for Conversation
using IBM.Watson.DeveloperCloud.Utilities; 					// for Credentials
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Completed
{
	using System.Collections.Generic;       //Allows us to use Lists. 
    using FullSerializer;
    using UnityEngine.Networking;
    using UnityEngine.UI;					//Allows us to use UI.
	
	public class GameManager : MonoBehaviour
	{


		public float levelStartDelay = 2f;						//Time to wait before starting level, in seconds.
		public float turnDelay = 0.1f;							//Delay between each Player turn.
		public int playerFoodPoints = 100;						//Starting value for Player food points.
		public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
		[HideInInspector] public bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.
		
		
		private Text levelText;									//Text to display current level number.
		private GameObject levelImage;							//Image to block out level as levels are being set up, background for levelText.
		private BoardManager boardScript;						//Store a reference to our BoardManager which will set up the level.
		private int level = 1;									//Current level number, expressed in game as "Day 1".
		private List<Enemy> enemies;							//List of all Enemy units, used to issue them move commands.
        //private List<Princess> princess;
		private bool enemiesMoving;								//Boolean to check if enemies are moving.
		private bool doingSetup = true;                         //Boolean to check if we're setting up board, prevent Player from moving during setup.

        //Cloudant
        private static System.DateTime now = System.DateTime.Now;
        private string _id = ""+ now.DayOfYear.ToString() + now.Hour + now.Minute + now.Second + now.Millisecond + "";
        private string _rev = "";
        private int counter = 0;

		//Watson Conversation core
		private Conversation _conversation;
		//private bool _waitingForResponse = true;
		//Watson Conversation controls
		public Text chat;
		public InputField userInput;
		public Canvas dialog;
        private fsSerializer _serializer = new fsSerializer();
        private Dictionary<string, object> _context = null;
		//Watson Conversation credentials
		//Oh, please, don't copy these ones... Seriously, don't.
		private string _username = "xxxx-xxxx-xxxx-xxxx-xxxxxxx";
		private string _password = "xxxxxxxxx";
		private string _url = "https://gateway.watsonplatform.net/conversation/api";
		private string _workspaceId = "xxxxx-xxxx-xxxx-xxxx-xxxxxxx";
		private string _conversationVersionDate = "2017-05-26";

		// List of strings to keep track of the last MAX_CONVERSATION_LOG_ENTRIES messages (2 by default)
		List<string> conversationLog;

		// The maximum number of conversation entries to show in the dialog text
		int MAX_CONVERSATION_LOG_ENTRIES = 2;

		// The greeting message shown when starting the conversation with the enemy
		//string GREETING_MESSAGE = "Troll> Greetings, what's the purpose of your journey?";

		//Awake is always called before any Start functions
		void Awake()
		{
            //Check if instance already exists
            if (instance == null)

                //if not, set instance to this
                instance = this;

            //If instance already exists and it's not this:
            else if (instance != this)

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);	
			
			//Sets this to not be destroyed when reloading scene
			DontDestroyOnLoad(gameObject);
			
			//Assign enemies to a new List of Enemy objects.
			enemies = new List<Enemy>();


			
			//Get a component reference to the attached BoardManager script
			boardScript = GetComponent<BoardManager>();

			/**
    			* Init Watson Conversation with credentials
    			* @author Facundo Martin Gordillo (famargor@ar.ibm.com)
			*/
			Credentials credentials = new Credentials(_username, _password, _url);
			_conversation = new Conversation(credentials);
			_conversation.VersionDate = _conversationVersionDate;
			
			//Call the InitGame function to initialize the first level 
			InitGame();
		}

        //this is called only once, and the paramter tell it to be called only after the scene was loaded
        //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            //register the callback to be called everytime the scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //This is called each time a scene is loaded.
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            instance.level++;
            instance.InitGame();
        }

		
		//Initializes the game for each level.
		void InitGame()
		{
			//While doingSetup is true the player can't move, prevent player from moving while title card is up.
			doingSetup = true;
			
			//Get a reference to our image LevelImage by finding it by name.
			levelImage = GameObject.Find("LevelImage");
			
			//Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
			levelText = GameObject.Find("LevelText").GetComponent<Text>();
			
			//Set the text of levelText to the string "Day" and append the current level number.
			levelText.text = "EVA 2017 Demo";
			
			//Set levelImage to active blocking player's view of the game board during setup.
			levelImage.SetActive(true);
			
			//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
			Invoke("HideLevelImage", levelStartDelay);
			
			//Clear any Enemy objects in our List to prepare for next level.
			enemies.Clear();
			
			//Call the SetupScene function of the BoardManager script, pass it current level number.
			boardScript.SetupScene(level);

			/**
    			* Watson conversation controls
    			* @author Diego Masini (masini@ar.ibm.com)
			*/
			//Get a reference to the Dialog container
			dialog = GameObject.Find("Dialog").GetComponent<Canvas>();
			dialog.enabled = false;

			//Get a reference to the DialogText text component by finding it by name and calling GetComponent.
			chat = GameObject.Find("Conversation").GetComponent<Text>();

			//Get a reference to the UserInput input field component by finding it by name and calling GetComponent.
			userInput = GameObject.Find("UserInput").GetComponent<InputField>();

			//Add an OnEndEdit listener on the userInput inputField to get notified when the player submits text to the game.
			userInput.onEndEdit.AddListener(SubmitUserInput);

			//Create the conversation log to hold a mini history of the dialog
			conversationLog = new List<string> ();

            // HARDCODED WELCOME MESSAGE 
            SubmitWatsonInput("¡Hola! Soy Sirius, el gran viejo sabio, Guardian de los Senderos del Bosque Mágico. ¿Cuál es tu nombre?");
		}

		// Shows the conversation controls and the last logged conversation entries (if any)
		// @author Diego Masini (masini@ar.ibm.com)
		public void ShowConversation()
		{
			dialog.enabled = true;
			chat.text = string.Join ("\n", conversationLog.ToArray ());

			// Set focus on the user input field
			userInput.ActivateInputField();
		}

		// Hides the conversation controls
		// @author Diego Masini (masini@ar.ibm.com)
		public void HideConversation()
		{
			dialog.enabled = false;
		}

		// Handler of the OnEndEdit event triggered by the userInput input field.
		// @author Diego Masini (masini@ar.ibm.com)
		public void SubmitUserInput(string input)
		{
			UpdateConversation("You> ", input);
			userInput.text = "";

            SubmitMessage(input);

			// To keep the focus on the input field
			userInput.ActivateInputField();
		}

		// Updates the conversation with the response provided by Watson 
		// (must be called when a response from the Watson service is received)
		// @author Diego Masini (masini@ar.ibm.com)
		public void SubmitWatsonInput(string input)
		{
			UpdateConversation("Troll>", input);
		}

		// @author Diego Masini (masini@ar.ibm.com)
		void UpdateConversation(string speaker, string input){
			string trimmedInput = input.Trim();

			// Check if the input is empty
			if (trimmedInput.Equals("")) {
				return;
			}

			trimmedInput = speaker + trimmedInput;

			// If the conversation list already has the max number of items allowed, make room for the new input
			if (conversationLog.Count == MAX_CONVERSATION_LOG_ENTRIES) {
				conversationLog.RemoveAt(0);
			}

			conversationLog.Add(trimmedInput);
			chat.text = string.Join ("\n", conversationLog.ToArray ());
		}
		
		//Hides black image used between levels
		void HideLevelImage()
		{
			//Disable the levelImage gameObject.
			levelImage.SetActive(false);
			
			//Set doingSetup to false allowing player to move again.
			doingSetup = false;
		}
		
		//Update is called every frame.
		void Update()
		{
			//Check that playersTurn or enemiesMoving or doingSetup are not currently true.
			if(playersTurn || enemiesMoving || doingSetup)
				
				//If any of these are true, return and do not start MoveEnemies.
				return;
			
			//Start moving enemies.
			StartCoroutine (MoveEnemies ());
		}
		
		//Call this to add the passed in Enemy to the List of Enemy objects.
		public void AddEnemyToList(Enemy script)
		{
			//Add Enemy to List enemies.
			enemies.Add(script);
		}

        //public void AddPrincess(Princess script)
        //{
            //Add Enemy to List enemies.
            //princess.Add(script);
        //}
		
		
		//GameOver is called when the player reaches 0 food points
		public void GameOver()
		{
			//Set levelText to display number of levels passed and game over message
			levelText.text = "After " + level + " days, you starved.";
			
			//Enable black background image gameObject.
			levelImage.SetActive(true);
			
			//Disable this GameManager.
			enabled = false;
		}
		
		//Coroutine to move enemies in sequence.
		IEnumerator MoveEnemies()
		{
			//While enemiesMoving is true player is unable to move.
			enemiesMoving = true;
			
			//Wait for turnDelay seconds, defaults to .1 (100 ms).
			yield return new WaitForSeconds(turnDelay);
			
			//If there are no enemies spawned (IE in first level):
			if (enemies.Count == 0) 
			{
				//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
				yield return new WaitForSeconds(turnDelay);
			}
			
			//Loop through List of Enemy objects.
			for (int i = 0; i < enemies.Count; i++)
			{
				//Call the MoveEnemy function of Enemy at index i in the enemies List.
				// enemies[i].MoveEnemy ();
				
				//Wait for Enemy's moveTime before moving next Enemy, 
				yield return new WaitForSeconds(enemies[i].moveTime);
			}
			//Once Enemies are done moving, set playersTurn to true so player can move.
			playersTurn = true;
			
			//Enemies are done moving, set enemiesMoving to false.
			enemiesMoving = false;
		}

        static string GetOutput(Dictionary<string, object> respDict)
        {
            object outputs;
            respDict.TryGetValue("output", out outputs);

            object output;
            (outputs as Dictionary<string, object>).TryGetValue("text", out output);

            string var = (output as List<object>)[0] as string;

            return var;
        }

        // If it receives a message, deal with it!
        // @author Facundo Martin Gordillo (famargor@ar.ibm.com)
		private void OnMessage(object resp, string data)
		{
            Dictionary<string, object> respDict = resp as Dictionary<string, object>;
            string output = (GetOutput(respDict));

            //  Set context for next round of messaging
            object _tempContext = null;
            (resp as Dictionary<string, object>).TryGetValue("context", out _tempContext);

            if (_tempContext != null)
                _context = _tempContext as Dictionary<string, object>;
            else
                Log.Debug("ExampleConversation", "Failed to get context");

            Debug.Log("Conversation: Message Response: " +  output);

             SubmitWatsonInput(output);

			//_waitingForResponse = false;
            StartCoroutine(SendToCloudant(output));
		}


        private void SubmitMessage(string text)
        {
            MessageRequest messageRequest = new MessageRequest()
            {
                input = new Dictionary<string, object>()
                {
                    { "text", text }

                },
                context = _context
            };

            if (!_conversation.Message(OnMessage, _workspaceId, messageRequest))
                Log.Debug("ExampleConversation", "Failed to message!");
        }

        //TODO: Add DB handler via REST API to store dialog (for future analytics)
        //@author Facundo Martin Gordillo (famargor@ar.ibm.com)
        private IEnumerator SendToCloudant(string input) {
            
            WWWForm body = new WWWForm();

            //Headers
            Dictionary<string, string> headers = body.headers;
            headers["Content-Type"] = "application/json";

            //Fields
            //string body = LitJson.JsonMapper.ToJson();
            body.AddField("_id", _id);
            if (_rev != "")
                body.AddField("_rev", _rev);
            body.AddField(counter + "-message", input);

            WWW api = new WWW("xxxxx:xxxxx@xxxxx-bluemix.cloudant.com/eva-2017", body);
            //api.SetRequestHeader("Content-Type", "application/json");
            yield return api;
            if (api.error != null)
            {
                Debug.Log("Error: " + api.error);
            }
            else
            {
                Debug.Log("All OK");
                Debug.Log("Text: " + api.isDone);
            }
        }
	}

}


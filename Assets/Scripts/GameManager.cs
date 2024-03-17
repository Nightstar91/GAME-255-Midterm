using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace Midterm
{
    public class GameManager : MonoBehaviour
    {
        // The Gameboard
        // NOTE: with the way how I set up the grid layout group (0,0) starts at bottom right, (3,6) is top left IN GAME.
        [SerializeField]
        public int[,] gameBoard = new int[,]
        {
            {0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0}
        };

        public int[] playerSpace = { 2, 0, 0, 0, 0, 0, 0 };

        // If the array is holding the following...
        // 0 = empty
        // 1 = dodgeball
        // 2 = player


        // Timer for the dodge ball to start spawning in
        private float dodgeBallSpawnTimer = 2.5f;
        private float dodgeBallTickRate = 0.8f;


        // Total amount of rows and columns
        private int nRows;
        private int nCols;

        // Current row and column the player is in
        private int row;
        private int column;

        // Scores and timer for the game
        private int score = 0;
        private int timer;

        // A bunch of 
        private bool dodgeballInColumn0 = false;
        private bool dodgeballInColumn1 = false;
        private bool dodgeballInColumn2 = false;
        private bool dodgeballInColumn3 = false;
        private bool dodgeballInColumn4 = false;
        private bool dodgeballInColumn5 = false;
        private bool dodgeballInColumn6 = false;

        // To be use for part of the restart
        private bool hasPlayerBeenHit = false;

        // All audio variable to be use
        [SerializeField] AudioClip playerMovementSound;
        [SerializeField] AudioClip playerHitSound;
        [SerializeField] AudioClip dodgeballSpawnSound;

        private AudioSource audioSystem;
        
        // The parent for all cells prefab
        [SerializeField] Transform gridRoot;

        // Reference to all my gameobject and textmesh that will be used for the dodgeball game
        [SerializeField] GameObject cellPrefab;
        [SerializeField] GameObject restartButton;
        [SerializeField] TextMeshProUGUI restartLabel;
        [SerializeField] TextMeshProUGUI timerLabel;
        [SerializeField] TextMeshProUGUI scoreLabel;
        [SerializeField] GameObject leftButton;
        [SerializeField] GameObject rightButton;
        

        private void Awake()
        {
            audioSystem = GetComponent<AudioSource>();

            // Initialize rows/cols to create our game board
            nRows = gameBoard.GetLength(0);
            nCols = gameBoard.GetLength(1);

            // According to the battleship instruction, i need to create a identical array for the hit object
            // to function. gonna ignore this for now

            // Populating the gameboard with grid using a for loop
            for(int i = 0; i < nRows * nCols; i++)
            {
                // instantiate the cell prefab to fill up our gameboard
                Instantiate(cellPrefab,gridRoot);
            }

            // Start the game with a selected cell
            SelectCurrentCell();

            // Set the score when the game start
            scoreLabel.text = string.Format("Score: {0}", score);

            // Set the timer when the game start
            InvokeRepeating("IncrementTime", 1f, 2f);
        }


        // This method is to determine where the player is at using the player array to be use for collision
        int GetCurrentCellElement()
        {
            // You can figure out the index
            // of the cell that is a part of the grid 
            // by calculating (row*Cols) + col
            int index = (row * nCols) + column;

            // Return the index
            return index;
        }


        Transform GetCurrentCell()
        {
            // You can figure out the child index
            // of the cell that is a part of the grid 
            // by calculating (row*Cols) + col
            int index = (row * nCols) + column ;

            // Return the child by index
            return gridRoot.GetChild(index);
        }


        void SelectCurrentCell()
        {
            // Get the current cell
            Transform cell = GetCurrentCell();

            // Get the current element of said cell
            int cellElement = GetCurrentCellElement();

            // Setting the player object to the appropriate cell 
            playerSpace[cellElement] = 2;


            // Set the "Cursor" image on
            Transform cursor = cell.Find("Player");
            cursor.gameObject.SetActive(true);
        }


        void UnselectCurrentCell()
        {
            // Get the current cell
            Transform cell = GetCurrentCell();

            // Get the current element of said cell
            int cellElement = GetCurrentCellElement();

            // Creating an empty space inside the player array 
            playerSpace[cellElement] = 0;

            // Set the "Cursor" image off
            Transform cursor = cell.Find("Player");
            cursor.gameObject.SetActive(false);
        }


        public void MoveHorizontal(int amt)
        {
            // Unselecting the previous cell to visualize movement
            UnselectCurrentCell();

            // Updating the column
            column += amt;

            // Making sure the column stay within the array and prevent error
            column = Mathf.Clamp(column, 0, nCols - 1);

            // Select the new cell
            SelectCurrentCell();

            audioSystem.PlayOneShot(playerMovementSound);
        }


        void IncrementScore()
        {
            // This will stop the score from being added if the player has been hit once
            if(hasPlayerBeenHit == false)
            {
                // Add 1 to the score
                score++;
            }

        }


        void IncrementTime()
        {
            // Add 1 to the time
            timer++;
        }


        public void DodgeBallMovement()
        {
            // Simple countdown timer for the dodgeball to update
            dodgeBallTickRate -= Time.deltaTime;

            // Once the timer reaches 0
            if(dodgeBallTickRate <= 0)
            {
                // This for loop will go though the entire columnn of the first row (remember its (3,whatever) that is considered the top row )
                // and will do the appropriate movement of the dodgeball based
                // on the value of the gameboard 2d array
                for (int column = 0; column < gameBoard.GetLength(1); column++)
                {
                    // For the first row (which is consider the top row in game)
                    if (gameBoard[3, column] == 1)
                    {
                        gameBoard[3, column] = 0;
                        gameBoard[2, column] = 1;
                    }

                    else if (gameBoard[2, column] == 1)
                    {
                        gameBoard[2, column] = 0;
                        gameBoard[1, column] = 1;
                    }

                    else if (gameBoard[1, column] == 1)
                    {
                        gameBoard[1, column] = 0;
                        gameBoard[0, column] = 1;
                    }

                    else if (gameBoard[0, column] == 1)
                    {
                        gameBoard[0, column] = 0;

                        // Dodgeball despawn from the play field and the player earned a point
                        IncrementScore();
                    }

                }


                dodgeBallTickRate = 0.8f;
            }
        }


        // Randomly choose a column in the first row of the array and then set the element to 1 to represent dodgeball
        public void SpawnDodgeBall()
        {
            int randomNumber;

            // Picking a random number to place spawn a dodgeball on a certain column
            randomNumber = Random.Range(0, 7);


            if (dodgeballInColumn0 == false && randomNumber == 0)
            {
                gameBoard[3, randomNumber] = 1;
                audioSystem.PlayOneShot(dodgeballSpawnSound);
            }

            else if (dodgeballInColumn1 == false && randomNumber == 1)
            {
                gameBoard[3, randomNumber] = 1;
                audioSystem.PlayOneShot(dodgeballSpawnSound);
            }

            else if (dodgeballInColumn2 == false && randomNumber == 2)
            {
                gameBoard[3, randomNumber] = 1;
                audioSystem.PlayOneShot(dodgeballSpawnSound);
            }

            else if (dodgeballInColumn3 == false && randomNumber == 3)
            {
                gameBoard[3, randomNumber] = 1;
                audioSystem.PlayOneShot(dodgeballSpawnSound);
            }

            else if (dodgeballInColumn4 == false && randomNumber == 4)
            {
                gameBoard[3, randomNumber] = 1;
                audioSystem.PlayOneShot(dodgeballSpawnSound);
            }

            else if (dodgeballInColumn5 == false && randomNumber == 5)
            {
                gameBoard[3, randomNumber] = 1;
                audioSystem.PlayOneShot(dodgeballSpawnSound);
            }

            else if (dodgeballInColumn6 == false && randomNumber == 6)
            {
                gameBoard[3, randomNumber] = 1;
                audioSystem.PlayOneShot(dodgeballSpawnSound);
            }


        }


        public void DodgeBallSpawnRate()
        {
            dodgeBallSpawnTimer -= Time.deltaTime;

            // Once the timer reach zero
            if(dodgeBallSpawnTimer <= 0)
            {
                // Using selection in order to determine the diffuculty using the value of score. Difficult is measure by the dodgeballSpawnTimer
                // Easy
                if(score <= 10)
                {
                    SpawnDodgeBall();

                    Debug.Log("Difficulty = Easy");
                    dodgeBallSpawnTimer = 2.5f;
                }
                // Normal
                else if (score <= 25)
                {
                    SpawnDodgeBall();

                    Debug.Log("Difficulty = Normal");
                    dodgeBallSpawnTimer = 1.5f;
                }
                // Hard
                else if (score <= 50)
                {
                    SpawnDodgeBall();

                    Debug.Log("Difficulty = Hard");
                    dodgeBallSpawnTimer = 0.7f;
                }
                // ULTRAHARD
                else
                {
                    SpawnDodgeBall();

                    Debug.Log("Difficulty = ULTRA HARD");
                    dodgeBallSpawnTimer = 0.5f;
                }
            }
        }


        // To check the entire array for any dodgeball and to display it based on the ticks of the game
        public void DisplayDodgeBall()
        {
            // This for loop is to turn on any dodgeball to the grid ingame using the 2d array as reference
            for(int row = 0; row < gameBoard.GetLength(0); row++)
            {
                for(int column = 0; column < gameBoard.GetLength(1); column++)
                {
                    if (gameBoard[row, column] == 1)
                    {
                        int index = (row * gameBoard.GetLength(1)) + column;

                        //Grabbing the cell that holds a dodgeball inside and caching as a variable
                        Transform currentCell = gridRoot.GetChild(index);

                        //Caching the dodgeball cell and setting the object to be active
                        Transform dodgeBallCell = currentCell.Find("DodgeBall");
                        dodgeBallCell.gameObject.SetActive(true);
                    }
                }
            }

            // This for loop is to turn off any dodgeball to the grid ingame using the 2d array as reference
            for (int row = 0; row < gameBoard.GetLength(0); row++)
            {
                for (int column = 0; column < gameBoard.GetLength(1); column++)
                {
                    if (gameBoard[row, column] == 0)
                    {
                        int index = (row * gameBoard.GetLength(1)) + column;

                        //Grabbing the cell that holds a dodgeball inside and caching as a variable
                        Transform currentCell = gridRoot.GetChild(index);

                        //Caching the dodgeball cell and setting the object to be active
                        Transform dodgeBallCell = currentCell.Find("DodgeBall");
                        dodgeBallCell.gameObject.SetActive(false);
                    }
                }
            }

        }


        // Setting the entire gameboard to empty with the value of 0
        public void ClearBoard()
        {
            for (int row = 0; row < gameBoard.GetLength(0); row++)
            {
                for (int column = 0; column < gameBoard.GetLength(1); column++)
                {
                    gameBoard[row, column] = 0;
                }
            }
        }


        // Debug method
        public void CheckCurrentArray()
        {
            for (int row = 0; row < gameBoard.GetLength(0); row++)
            {
                for (int column = 0; column < gameBoard.GetLength(1); column++)
                {
                    if (gameBoard[row, column] == 1)
                    {
                        Debug.Log($"dodgeball found in {row},{column}");
                    }


                }
            }

            for (int i = 0; i < playerSpace.Length; i++)
            {
                if (playerSpace[i] == 2)
                {
                    Debug.Log($"player found in element {i}");
                }
            }
        }


        public void CheckColumn()
        {
            // Checking column 0 for any dodgeball, if found will set a bool to be true otherwise it was 
            if (gameBoard[0, 0] == 1 || gameBoard[1, 0] == 1 || gameBoard[2, 0] == 1 || gameBoard[3, 0] == 1)
            {
                dodgeballInColumn0 = true;
            }
            else
                dodgeballInColumn0 = false;

            // Checking column 1 for any dodgeball
            if (gameBoard[0, 1] == 1 || gameBoard[1, 1] == 1 || gameBoard[2, 1] == 1 || gameBoard[3, 1] == 1)
            {
                dodgeballInColumn1 = true;
            }
            else
                dodgeballInColumn1 = false;

            // Checking column 2 for any dodgeball
            if (gameBoard[0, 2] == 1 || gameBoard[1, 2] == 1 || gameBoard[2, 2] == 1 || gameBoard[3, 2] == 1)
            {
                dodgeballInColumn2 = true;
            }
            else
                dodgeballInColumn2 = false;

            // Checking column 3 for any dodgeball
            if (gameBoard[0, 3] == 1 || gameBoard[1, 3] == 1 || gameBoard[2, 3] == 1 || gameBoard[3, 3] == 1)
            {
                dodgeballInColumn3 = true;
            }
            else
                dodgeballInColumn3 = false;

            // Checking column 4 for any dodgeball
            if (gameBoard[0, 4] == 1 || gameBoard[1, 4] == 1 || gameBoard[2, 4] == 1 || gameBoard[3, 4] == 1)
            {
                dodgeballInColumn4 = true;
            }
            else
                dodgeballInColumn4 = false;

            // Checking column 5 for any dodgeball
            if (gameBoard[0, 5] == 1 || gameBoard[1, 5] == 1 || gameBoard[2, 5] == 1 || gameBoard[3, 5] == 1)
            {
                dodgeballInColumn5 = true;
            }
            else
                dodgeballInColumn5 = false;

            // Checking column 6 for any dodgeball
            if (gameBoard[0, 6] == 1 || gameBoard[1, 6] == 1 || gameBoard[2, 6] == 1 || gameBoard[3, 6] == 1)
            {
                dodgeballInColumn6 = true;
            }
            else
                dodgeballInColumn6 = false;

        }


        // Method to check to see if the player and the dodgeball has collide
        // Since I am working with an array to keep track of object, I am using the 2d array for the dodgeball and the player array
        // As a reference for when the player and the dodgeball has collided
        public void DodgeballCollision()
        {
            // Collision in column 0 and row 0
            if (gameBoard[0, 0] == 1 && playerSpace[0] == 2 && hasPlayerBeenHit == false)
            {
                audioSystem.PlayOneShot(playerHitSound);
                hasPlayerBeenHit = true;
            }

            // Collision in column 1 and row 0
            if (gameBoard[0,1] == 1 && playerSpace[1] == 2 && hasPlayerBeenHit == false)
            {
                audioSystem.PlayOneShot(playerHitSound);
                hasPlayerBeenHit = true;
            }

            // Collision in column 2 and row 0
            if (gameBoard[0, 2] == 1 && playerSpace[2] == 2 && hasPlayerBeenHit == false)
            {
                audioSystem.PlayOneShot(playerHitSound);
                hasPlayerBeenHit = true;
            }

            // Collision in column 3 and row 0
            if (gameBoard[0, 3] == 1 && playerSpace[3] == 2 && hasPlayerBeenHit == false)
            {
                audioSystem.PlayOneShot(playerHitSound);
                hasPlayerBeenHit = true;
            }

            // Collision in column 4 and row 0
            if (gameBoard[0, 4] == 1 && playerSpace[4] == 2 && hasPlayerBeenHit == false)
            {
                audioSystem.PlayOneShot(playerHitSound);
                hasPlayerBeenHit = true;
            }

            // Collision in column 5 and row 0
            if (gameBoard[0, 5] == 1 && playerSpace[5] == 2 && hasPlayerBeenHit == false)
            {
                audioSystem.PlayOneShot(playerHitSound);
                hasPlayerBeenHit = true;
            }

            // Collision in column 6 and row 0
            if (gameBoard[0, 6] == 1 && playerSpace[6] == 2 && hasPlayerBeenHit == false)
            {
                audioSystem.PlayOneShot(playerHitSound);
                hasPlayerBeenHit = true;
            }
        }


        // This will be a constant check to see if the player has collided with the dodgeball to end the game
        private void TryToEndGame()
        {
            DodgeballCollision();

            if(hasPlayerBeenHit == true)
            {
                restartLabel.gameObject.SetActive(true);
                restartButton.gameObject.SetActive(true);

                leftButton.gameObject.SetActive(false);
                rightButton.gameObject.SetActive(false);

                CancelInvoke("IncrementTime");
            }
        }

        public void Restart()
        {
            // Setting the boolean back to false to restart the whole game
            hasPlayerBeenHit = false;

            // Resetting the score and timer back to 0
            score = 0;
            timer = 0;


            // Set the timer when the game restart
            InvokeRepeating("IncrementTime", 1f, 2f);

            // Turning off the restart object and text
            restartLabel.gameObject.SetActive(false);
            restartButton.gameObject.SetActive(false);

            // Turning back on the button for player movement
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);

            // Lastly clearing the board so the player has a chance and resetting the timer for the dodgeball to give the player a fair start
            dodgeBallSpawnTimer = 2.5f;
            dodgeBallTickRate = 0.8f;
            ClearBoard();
        }

        // Update is called once per frame
        void Update()
        {
            //Update the score label with the current score
            scoreLabel.text = string.Format("Score: {0}", score);

            // Update the time label with current time
            // Format it mm:ss where m is the minute and s is the seconds
            // ss should always display 2 digits
            // mm should only display as many digits that are necessary
            timerLabel.text = string.Format("Timer: {0}:{1}", timer / 60, (timer % 60).ToString("00"));

            CheckColumn();

            DodgeBallSpawnRate();

            DodgeBallMovement();

            DisplayDodgeBall();

            TryToEndGame();
        }
    }
}

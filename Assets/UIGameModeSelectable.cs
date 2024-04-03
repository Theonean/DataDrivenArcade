using System.Collections;
using System.Collections.Generic;
using TMPro; // For UI text elements.
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events; // For Unity event management.

// Enum to track the selection state of each player.
public enum GameModeSelectionState
{
    NONE,
    START_SELECTED_NEW_GAME,
    FINISHED_SELECTED_NEW_GAME,
    START_SELECTED_INFO,
    FINISHED_SELECTED_INFO,
}

// Manages UI elements related to game mode selection, allowing players to choose a game mode or view game information.
public class UIGameModeSelectable : MonoBehaviour
{
    // UI elements and variables related to the game mode.
    [Header("Game Mode Name")]
    public TextMeshProUGUI gameModeText; // UI element to display the name of the game mode.
    public string gameModeName; // Name of the game mode.
    public GameObject[] selectedHighlight = new GameObject[2]; // Highlights to indicate selected options.
    public UISelectable[] startGameSelectables = new UISelectable[2]; // Selectable elements for starting a game.
    public UISelectable[] gameInfos = new UISelectable[2]; // Selectable elements for game information.
    private UISelectable[] currSelected = new UISelectable[2]; // Currently selected UI elements for each player.
    private GameModeSelectionState[] playerGameModeSelectionState = new GameModeSelectionState[2]; // Tracks each player's selection state.
    [Multiline]
    public string gameModeDescription; // Description of the game mode.
    private GameManager gm; // Reference to the game manager.
    public UnityEvent bothPlayersSameSelection; // Event triggered when both players make the same selection.

    // Initialize references and set up UI elements.
    private void Start()
    {
        gm = GameManager.instance; // Get the instance of the GameManager.
        gameModeText.text = gameModeName; // Display the name of the game mode.
    }

    // Handles the selection action for a player.
    public void Selected(int playerNum, bool selectNewGame)
    {
        int playerToArrayNum = playerNum - 1; // Convert player number to array index.
        // Set the selection state based on the choice (new game or info).
        playerGameModeSelectionState[playerToArrayNum] = selectNewGame ?
            GameModeSelectionState.START_SELECTED_NEW_GAME :
            GameModeSelectionState.START_SELECTED_INFO;

        selectedHighlight[playerToArrayNum].SetActive(true); // Highlight the selected option.

        // Determine and set the current selection based on the player's choice.
        UISelectable currSelectedByPlayer = selectNewGame ? startGameSelectables[playerToArrayNum] : gameInfos[playerToArrayNum];

        currSelected[playerToArrayNum] = currSelectedByPlayer;

        // Listen for the completion of the selection animation.
        currSelectedByPlayer.FinishedSelectionEvent.AddListener(() => SelectionAnimationFinished(playerNum));

        currSelectedByPlayer.Selected(); // Trigger the selected state for the UI element.
        MoveHighlight(playerNum);
        print("Player " + playerNum + " selected " + gameModeName);
    }

    // Handles the deselection action for a player.
    public void Deselected(int playerNum)
    {
        int playerToArrayNum = playerNum - 1; // Convert player number to array index.
        playerGameModeSelectionState[playerToArrayNum] = GameModeSelectionState.NONE; // Reset the selection state.
        selectedHighlight[playerToArrayNum].SetActive(false); // Remove the selection highlight.

        // Deselect the previously selected UI element and remove the listener for selection animation completion.
        currSelected[playerToArrayNum].Deselected();

        // remove the listener for previous selected option
        currSelected[playerToArrayNum].FinishedSelectionEvent.RemoveAllListeners();
        print("Player " + playerNum + " deselected " + gameModeName);

    }

    // Switches the selection from one option to another for a player.
    public void SwitchSelection(int playerNum)
    {
        int playerToArrayNum = playerNum - 1; // Convert player number to array index.

        // Toggle the selection state between new game and info.
        GameModeSelectionState newGameModeSelectionState = IsPlayerOnNewGameButton(playerNum) ?
            GameModeSelectionState.START_SELECTED_INFO :
            GameModeSelectionState.START_SELECTED_NEW_GAME;
        playerGameModeSelectionState[playerToArrayNum] = newGameModeSelectionState;

        // Deselect the current selection and remove the listeners
        currSelected[playerToArrayNum].Deselected();
        currSelected[playerToArrayNum].FinishedSelectionEvent.RemoveAllListeners();

        // Select the new selection
        UISelectable newSelected = IsPlayerOnNewGameButton(playerNum) ? startGameSelectables[playerToArrayNum] : gameInfos[playerToArrayNum];
        currSelected[playerToArrayNum] = newSelected;

        newSelected.FinishedSelectionEvent.AddListener(() => SelectionAnimationFinished(playerNum)); // Add the new listener

        newSelected.Selected(); // Trigger the selected state for the new UI element.
        MoveHighlight(playerNum);
    }

    // Callback for when the selection animation finishes.
    private void SelectionAnimationFinished(int playerNum)
    {
        print("Selection animation finished with player states being: " + playerGameModeSelectionState[0] + " and " + playerGameModeSelectionState[1]);

        int playerToArrayNum = playerNum - 1; // Convert player number to array index.
        // Update the selection state to indicate the animation has finished.
        playerGameModeSelectionState[playerToArrayNum] = IsPlayerOnNewGameButton(playerNum) ?
            GameModeSelectionState.FINISHED_SELECTED_NEW_GAME :
            GameModeSelectionState.FINISHED_SELECTED_INFO;

        // If both players have made the same selection, invoke the corresponding event.
        if (this.playerGameModeSelectionState[0] == this.playerGameModeSelectionState[1])
        {
            bothPlayersSameSelection.Invoke();
            print("Both players have made the same selection!");
        }
    }

    // Determines if a player's current selection is the New Game button based on their selection state.
    private bool IsPlayerOnNewGameButton(int playerNum)
    {
        int playerToArrayNum = playerNum - 1; // Convert player number to array index.
        return playerGameModeSelectionState[playerToArrayNum] == GameModeSelectionState.START_SELECTED_NEW_GAME || playerGameModeSelectionState[playerToArrayNum] == GameModeSelectionState.FINISHED_SELECTED_NEW_GAME;
    }

    private void MoveHighlight(int playerNum)
    {
        // Move the highlight to the new selection.
        int playerToArrayNum = playerNum - 1; // Convert player number to array index.
        GameObject selectedHighlight = this.selectedHighlight[playerToArrayNum];

        //Set Highlight position and Z-Layer
        selectedHighlight.transform.position = currSelected[playerToArrayNum].transform.position;
        selectedHighlight.transform.position += new Vector3(0, 0, -0.1f); // Move the highlight slightly up.

        //Scale Highlight
        selectedHighlight.transform.localScale = currSelected[playerToArrayNum].transform.localScale / 400f; // Scale the highlight to match the selection.
        selectedHighlight.transform.localScale *= 1.01f; //scale slightly up so it's slightly larger than selected button
    }
}

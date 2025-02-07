using System.Collections.Generic;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DIRTYInputManager : MonoBehaviour
{
    [SerializeField] private Button backButton;
    public InputActionReference joinInputActionReference;
    public InputActionReference cancelInputActionReference;

    public InputDevice player1DeviceType;
    public InputDevice player2DeviceType;
    public bool Player1IsKeyboard;
    public bool Player2IsKeyboard;
    [SerializeField] List<GameObject> playerUIGroups = new List<GameObject>();
    [SerializeField] List<Image> playerReadyImages = new List<Image>();
    private List<InputDevice> playerDevices = new List<InputDevice>();
    private bool[] playersReady = new bool[] { false, false };
    private string[] playersNames = new string[] { "", "" };

    private void Awake()
    {
        if (SteamManager.Initialized)
        {
            playersNames[0] = SteamFriends.GetPersonaName();

            //If using remote play set player 2 name too
            if (SteamRemotePlay.GetSessionCount() > 0)
            {
                playersNames[1] = SteamRemotePlay.GetSessionClientName(SteamRemotePlay.GetSessionID(0));
                Debug.Log("Remote Play Session Name: " + playersNames[1]);
            }
        }
    }

    private void Start()
    {
        playerUIGroups[0].SetActive(false);
        playerUIGroups[1].SetActive(false);
    }

    private void OnEnable()
    {
        joinInputActionReference.action.Enable();
        cancelInputActionReference.action.Enable();
        joinInputActionReference.action.performed += RegisterDeviceToPlayer;
        cancelInputActionReference.action.canceled += UnregisterDeviceFromPlayer;
    }

    private void OnDisable()
    {
        joinInputActionReference.action.Disable();
        cancelInputActionReference.action.Disable();
        joinInputActionReference.action.performed -= RegisterDeviceToPlayer;
        cancelInputActionReference.action.canceled -= UnregisterDeviceFromPlayer;
    }

    private void RegisterDeviceToPlayer(InputAction.CallbackContext context)
    {
        InputDevice device = context.control.device;
        bool isPlayerAlreadyRegistered = playerDevices.Count > 0 ? playerDevices.Contains(device) : false;

        if (isPlayerAlreadyRegistered)
        {
            int playerIndex = playerDevices.IndexOf(device);
            GameObject playerGroup = playerUIGroups[playerIndex];

            string playerName = playerGroup.GetComponentInChildren<NameCreator>().GetName();
            playersReady[playerIndex] = true;

            //Deselect the name creator and lock in the name
            playerGroup.GetComponentInChildren<NameCreator>().ToggleSelected(device);
            playerReadyImages[playerIndex].color = Color.green;

            Debug.Log("Player Ready: " + playerName);

            int numPlayersReady = playersReady.Where(x => x == true).Count();
            if (numPlayersReady == playerDevices.Count)
            {
                Debug.Log("Players ready: " + numPlayersReady + " num devices: " + playerDevices.Count);
                GameManager.instance.SetPlayerCount(numPlayersReady);
                GameManager.instance.playerDevices = playerDevices;

                for (int i = 0; i < numPlayersReady; i++)
                {
                    playerUIGroups[i].GetComponentInChildren<NameCreator>().SaveName();
                }

                CustomUIEvents.VirtualMoveSceneForward(1);
            }
            return;
        }
        else if (playerDevices.Count < 2) //don't allow more than 2 players
        {
            playerDevices.Add(device);
            int playerNum = playerDevices.Count;

            GameObject playerGroup = playerUIGroups[playerNum - 1];

            Debug.Log("Player" + playerNum + " joined with device: " + device.name);

            playerGroup.SetActive(true);

            //Force Steam Name if available onto player 1
            if (playerNum == 1 && SteamManager.Initialized)
            {
                NameCreator nc = playerUIGroups[0].GetComponentInChildren<NameCreator>();
                nc.SetName(playersNames[0]);
            }
            else
            {
                playerGroup.GetComponentInChildren<NameCreator>().ToggleSelected(device);
            }
        }
    }

    private void UnregisterDeviceFromPlayer(InputAction.CallbackContext context)
    {
        InputDevice device = context.control.device;

        //if trying to unregister while no players are registered, go back to main menu
        if (playerDevices.Count == 0)
        {
            backButton.onClick.Invoke();
            return;
        }

        int playerNum = playerDevices.IndexOf(device) + 1;
        GameObject playerGroup = playerUIGroups[playerNum - 1];

        //Unready player if they were ready and set them up for editing name again
        if (playersReady[playerNum - 1])
        {
            playersReady[playerNum - 1] = false;
            playerGroup.GetComponentInChildren<NameCreator>().ToggleSelected(device);
            Debug.Log("Unreadied Player " + playerNum);

            playerReadyImages[playerNum - 1].color = Color.white;
        }
        //Otherwise just remove the player and device from the list
        else
        {
            //If player 1 tries to deregister while player 2 exists, unready them instead
            if (playerNum == 1 && playerDevices.Count > 1)
            {
                playerNum = 2;
                playerGroup = playerUIGroups[playerNum - 1];

                playerGroup.GetComponentInChildren<NameCreator>().ToggleSelected(playerDevices[1]);
                playerDevices.Remove(playerDevices[playerNum - 1]);
                playerGroup.SetActive(false);

                playerReadyImages[playerNum - 1].color = Color.white;
            }
            else
            {
                Debug.Log("Player" + playerNum + " left with device: " + device.name);

                //Don't toggle selected if player 1 has their name set by steamName
                if (playerNum != 1 && !SteamManager.Initialized)
                    playerGroup.GetComponentInChildren<NameCreator>().ToggleSelected(device);

                playerDevices.Remove(device);
                playerGroup.SetActive(false);
            }
        }

    }

    private void GetControlSchemeLastJoinPressed()
    {
        // Get the last control scheme that triggered the join action, either "Gamepad" or "Keyboard"
        var controlScheme = joinInputActionReference.action.activeControl.device.name;

        Debug.Log("Last control scheme that pressed join: " + controlScheme);

        // Check the device that triggered the action
        var device = joinInputActionReference.action.activeControl.device;

        if (device is Keyboard)
        {
            Debug.Log("Player 1 pressed confirm with keyboard");
            player1DeviceType = Keyboard.current;
            Player1IsKeyboard = true;
            Player2IsKeyboard = false;
        }
        else if (device is Gamepad)
        {
            Debug.Log("Player 1 pressed confirm with gamepad");
            player1DeviceType = Gamepad.current;
            Player1IsKeyboard = false;
            Player2IsKeyboard = true;
        }
        else
        {
            Debug.Log("Player 1 pressed confirm with unknown device: " + device.GetType().Name);
        }
    }
}

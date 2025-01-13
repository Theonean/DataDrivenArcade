using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace ARCHIVE
{

    public enum Direction
    {
        Left,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft
    }

    public class InputVisualizer : MonoBehaviour
    {
        public GameObject lines;
        public GameObject[] playerKeyboardBindings;
        public bool showKeyBindings = false;
        public int playernum = 0;

        private Animator joystickAnimator;
        private Animator[] buttonAnimators;
        private const float timeUntilIdle = 2f;

        private float timeUntilIdleJoystick;
        private bool playingIdleAnimation = false;
        private bool active = false;

        GameManager gm;

        private void Start()
        {
            gm = GameManager.instance;

            timeUntilIdleJoystick = timeUntilIdle;

            //Joystick Events
            //gm.JoystickInputEvent.AddListener(OnJoystickInput);
            //gm.JoystickReleasedEvent.AddListener(OnJoystickReleased);

            //Button Events
            //gm.LineInputEvent.AddListener((iData) => OnButtonInput(iData, "Pressed"));
            //gm.LineReleasedEvent.AddListener((iData) => OnButtonInput(iData, "Released"));
            Debug.LogError("Repair Input System");

            //Find the animators in the scene
            joystickAnimator = GetComponentsInChildren<Animator>().Where(a => a.gameObject.name == "Joystick").First();
            buttonAnimators = GetComponentsInChildren<Animator>().Where(a => a.gameObject.name.Contains("Button")).ToArray();
        }

        private void Update()
        {
            if (active)
            {
                if (timeUntilIdleJoystick <= 0 && !playingIdleAnimation)
                {
                    playingIdleAnimation = true;
                    joystickAnimator.Play("JoystickP" + playernum + "UpDownPrompt");
                }
                else
                {
                    timeUntilIdleJoystick -= Time.deltaTime;
                }
            }
        }

        public void ToggleActive(bool showLines)
        {
            gm = GameManager.instance;
            active = !active;
            lines.SetActive(showLines);

            if (!gm.arcadeMode && showKeyBindings)
            {
                if (playernum == 1)
                {
                    playerKeyboardBindings[0].SetActive(active);
                }
                else
                {
                    playerKeyboardBindings[1].SetActive(active);
                }
            }
        }

        public void UIButtonPressed(int lineNum)
        {
            if (playernum == 1)
            {
                print("Player 1 pressed line on UI " + lineNum);
                //gm.LineInputEvent.Invoke(new InputData(lineNum, 1));
                Debug.LogError("Repair Input System");
            }
        }

        /// <summary>
        /// Plays the appropriate joystick animation on input
        /// </summary>
        /// <param name="iData"></param>
        private void OnJoystickInput(InputData iData)
        {
            if (iData.playerNum == playernum)
            {
                Vector2 direction = iData.joystickDirection;
                Direction dir = GetDirectionFromVector(direction);

                string animationName = "JoystickP" + playernum.ToString() + dir.ToString();
                joystickAnimator.Play(animationName);

                timeUntilIdleJoystick = timeUntilIdle;
                playingIdleAnimation = false;
            }
        }

        private void OnJoystickReleased(InputData iData)
        {
            if (iData.playerNum == playernum)
            {
                joystickAnimator.Play("JoystickP" + playernum + "Neutral");
            }
        }


        private Direction GetDirectionFromVector(Vector2 direction)
        {
            float angleOffset = 180f;//22.5f; // Offset to center the sectors
                                     // Convert vector to angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360; // Normalize angle to 0-360 degrees

            // Divide the circle into 8 sectors and return the corresponding direction
            int sector = (int)((angle + angleOffset) % 360) / 45; // Adding 22.5 normalizes the sectors' starting points
            return (Direction)sector;
        }

        private void OnButtonInput(InputData iData, string inputType)
        {
            if (iData.playerNum == playernum)
            {
                string buttonAnimName = iData.lineCode % 2 == 0 ? "ButtonBlue" : "ButtonRed";
                buttonAnimName += inputType;

                buttonAnimators[iData.lineCode].Play(buttonAnimName);

                timeUntilIdleJoystick = timeUntilIdle;
                playingIdleAnimation = false;
            }
        }
    }
}
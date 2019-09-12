using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN;

namespace ArenaConceder
{
    public class Conceder : MonoBehaviour
    {
        /// <summary>
        /// GUID of last used deck.
        /// This is because I have this requiring the user to give a valid deck at the start.
        /// </summary>
        private string _lastDeck = null;

        /// <summary>
        /// Time when the auto condeder began.
        /// </summary>
        private float _startTime = 0f;

        /// <summary>
        /// The number of times we've conceded a game.
        /// </summary>
        private int _concessions = 0;

        /// <summary>
        /// Time when we performed our last action/operation.
        /// </summary>
        private float _lastAction = 0f;

        /// <summary>
        /// Time to wait between actions.
        /// </summary>
        private const float _timeBetweenActions = 1f;

        /// <summary>
        /// Name of the event we want to auto queue.
        /// This should be put into a settings file at some point.
        /// </summary>
        private const string _internalEventName = "Future_Play_20190909";


        /// <summary>
        /// Called on start by Unity
        /// </summary>
        private void Start()
        {
            // Set start time
            _startTime = Time.time;
        }

#if DEBUG
        /// <summary>
        /// Called every frame by Unity
        /// </summary>
        private void Update()
        {
            // Open debug console... somehow
            if ((Time.time - _startTime) < 1)
                SceneLoader.GetSceneLoader().CurrentContentType.GetType();
        }
#endif

        /// <summary>
        /// Called for UI updates by Unity
        /// </summary>
        public void OnGUI()
        {
            // Get scene
            var scene = SceneLoader.GetSceneLoader();

            // Disable logic while user holds right control
            if (Input.GetKey(KeyCode.RightControl))
                return;

            // Cooldown between actions
            if (!scene || (Time.time - _lastAction) <= _timeBetweenActions)
                return;

            // Handle scene content
            switch (scene.CurrentContentType)
            {
                case NavContentType.Home:
                    {
                        HandleHomepage();
                        break;
                    }
                case NavContentType.EventLanding:
                    {
                        HandleEventPage();
                        break;
                    }
                case NavContentType.ConstructedDeckSelect:
                    {
                        HandleDeckSelectPage();
                        break;
                    }
                case NavContentType.None:
                    {
                        // If in duel then handle duel scene
                        if (scene.IsInDuelScene)
                            HandleGameView();
                        break;
                    }
                default:
                    {
                        Debug.Log("Unhandled content type: " + SceneLoader.GetSceneLoader().CurrentContentType);
                        break;
                    }
            }

            // Update last action time
            _lastAction = Time.time;
        }

        /// <summary>
        /// Handles the logic when the user is in a duel.
        /// </summary>
        private void HandleGameView()
        {
            if (DuelSceneContext.Globals != null && DuelSceneContext.Globals.GameManager && DuelSceneContext.Globals.GameManager.GreConnection != null)
            {
                if (DuelSceneContext.Globals.GameManager.CurrentGameState != null)
                {
                    if (DuelSceneContext.Globals.GameManager.GreConnection.MatchDoorState.State == GreClient.Network.MatchDoorConnectionStateEnum.Playing)
                    {
                        ++_concessions;
                        DuelSceneContext.Globals.GameManager.GreConnection.ConcedeMatch();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the logic for when the user is selecting a deck.
        /// </summary>
        private void HandleDeckSelectPage()
        {
            var deckPage = (SceneLoader.GetSceneLoader().GetContent(NavContentType.ConstructedDeckSelect) as ConstructedDeckSelectController);
            if (!deckPage)
                return;
            
            // Get deck selector
            // Only input last accepted deck
            var selector = deckPage.GetType().GetField("_deckSelectorInstance", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(deckPage) as MetaDeckSelector;
            if (selector && _lastDeck != null)
            {
                selector.SelectDeck(_lastDeck);
                deckPage.GetType().GetMethod("OnOk", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(deckPage, null);
            }

            // Otherwise leave user to initialize deck to use
        }

        /// <summary>
        /// Handles the logic for when we are on the event page.
        /// </summary>
        private void HandleEventPage()
        {
            var eventPage = (SceneLoader.GetSceneLoader().GetContent(NavContentType.EventLanding) as EventPageContentController);
            if (!eventPage)
                return;
            
            EventTemplate eventTemplate = eventPage.GetType().GetField("_activeEventTemplate", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(eventPage) as EventTemplate;
            if (eventTemplate != null)
            {
                MainButtonModule mainButton = eventTemplate.GetType().GetField("_mainButtonModule", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(eventTemplate) as MainButtonModule;

                switch (eventTemplate.EventContext.playerCourse.CurrentModule)
                {
                    case "Join":
                        {
                            // Start
                            if (mainButton)
                                mainButton.GetType().GetMethod("StartButtonClicked", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(mainButton, null);
                            break;
                        }
                    case "DeckSelect":
                        {
                            if (eventTemplate.EventContext.playerCourse.CourseDeck == null || eventTemplate.EventContext.playerCourse.CourseDeck.name == null)
                            {
                                // Select deck
                                SceneLoader.GetSceneLoader().GoToConstructedDeckSelect(eventTemplate.EventContext);
                            }
                            break;
                        }
                    case "TransitionToMatches":
                        {
                            // Save valid deck
                            if (eventTemplate.EventContext.playerCourse.CourseDeck != null && eventTemplate.EventContext.playerCourse.CourseDeck.name != null && _lastDeck == null)
                                _lastDeck = eventTemplate.EventContext.playerCourse.CourseDeck.id.ToString();

                            // Start queue
                            if (mainButton)
                                mainButton.GetType().GetMethod("PlayButtonClicked", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(mainButton, null);
                            break;
                        }
                    case "ClaimPrize":
                        {
                            // Go back to home
                            if (mainButton)
                                mainButton.GetType().GetMethod("HomeButtonClicked", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(mainButton, null);
                            break;
                        }
                    default:
                        {
                            Debug.Log("Unhandled event page module: " + eventTemplate.EventContext.playerCourse.CurrentModule);
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Handles the logic for when we are at the homepage.
        /// </summary>
        private void HandleHomepage()
        {
            HomePageContentController homepage = (SceneLoader.GetSceneLoader().GetContent(NavContentType.Home) as HomePageContentController);
            if (!homepage)
                return;

            // 
            switch (homepage.HomePageState)
            {
                // Homepage play
                case HomePageState.ReadyToPlay_BladeClosed:
                    {
                        homepage.OnPlayButton();
                        break;
                    }
                // Homepage blade play 
                case HomePageState.ReadyToPlay_BladeOpen_Invalid:
                case HomePageState.ReadyToPlay_BladeOpen_Valid:
                    {
                        // Select event
                        homepage.ShowBladeAndSelect(_internalEventName);

                        // Play
                        homepage.OnPlayButton();
                        break;
                    }
                default:
                    {
                        Debug.Log("Unhandled homepage state: " + homepage.HomePageState);
                        break;
                    }
            }
        }
    }
}

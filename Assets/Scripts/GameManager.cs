﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;


namespace PhotonLearning
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance;

        void Start()
        {
            Instance = this;
        }

        #region Photon Callbacks

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
            if (PhotonNetwork.IsMasterClient)
            {
                ////////// MASTER CLIENT ONLY //////////
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
                LoadArena();
                ///////////////////////////////////////
            }
        }

        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects
            if (PhotonNetwork.IsMasterClient)
            {
                ////////// MASTER CLIENT ONLY //////////
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
                LoadArena();
                ///////////////////////////////////////
            }
        }

        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        #endregion


        #region Public Methods

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Loads level based on player count. Should only be called by master client.
        /// </summary>
        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                ////////// *NON* MASTER CLIENT ONLY //////////
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
                //////////////////////////////////////////////
            }
            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount); // convention
        }

        #endregion
    }
}
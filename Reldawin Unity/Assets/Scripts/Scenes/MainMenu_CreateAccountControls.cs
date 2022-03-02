﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LowCloud.Reldawin
{
    public class MainMenu_CreateAccountControls : SceneBehaviour
    {
        [SerializeField] private EventSystem system;
        [SerializeField] private GameObject username;
        [SerializeField] private GameObject password;
        [SerializeField] private Text txtErrorLog;
        [SerializeField] private Button btnCreateAccount;
        [SerializeField] private GameObject mainMenuWindow;

        public void OnCreateAccountClicked()
        {
            ClientTCP.SendCreateAccountQuery( username.GetComponent<InputField>().text
                                               , password.GetComponent<InputField>().text
                                               );
        }

        public void OnIpfUsernameCharChanged( InputField ipf )
        {
            ClientTCP.SendUsernameQuery( ipf.text );
        }

        public void OnBtnReturnToMainMenuClicked()
        {
            mainMenuWindow.SetActive( true );
            gameObject.SetActive( false );
        }

        private void OnNetworkQueryUsernameResultReturned( params object[] args )
        {
            bool result = (bool)args[0];
            btnCreateAccount.interactable = !result;
        }

        private void OnNetworkAccountCreatedResult( params object[] args )
        {
            txtErrorLog.text = (string)args[0];
            txtErrorLog.color = UnityEngine.Color.green;
            btnCreateAccount.interactable = false;
        }

        protected override void Awake()
        {
            base.Awake();
            EventProcessor.AddInstructionParams( Packet.DoesUserExist, OnNetworkQueryUsernameResultReturned );
            EventProcessor.AddInstructionParams( Packet.Account_Create_Success, OnNetworkAccountCreatedResult );
        }

        public void Update()
        {
            if ( Input.GetKeyDown( KeyCode.Tab ) )
            {
                if ( system.currentSelectedGameObject == username )
                {
                    system.SetSelectedGameObject( password, new BaseEventData( system ) );
                    return;
                }
                if ( system.currentSelectedGameObject == password )
                {
                    system.SetSelectedGameObject( username, new BaseEventData( system ) );
                    return;
                }
            }
        }

        private void OnDestroy()
        {
            EventProcessor.RemoveInstructionParams( Packet.DoesUserExist );
            EventProcessor.RemoveInstructionParams( Packet.Account_Create_Success );
        }
    }
}
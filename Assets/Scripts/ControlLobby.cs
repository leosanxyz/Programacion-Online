using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
public class ControlLobby : MonoBehaviourPunCallbacks
{
    #region CORE
    void Start()
    {
        Start_PanelInicio();
    }
    #endregion
    
    #region PANEL INICIO
    [Header("PANEL INICIO")]
    [SerializeField] GameObject panelInicio;
    [SerializeField] TMP_InputField inputNombre;
    [SerializeField] Button botonIniciar;
    [SerializeField] TMP_Text notificacion;
    
    private void Awake_PanelInicio()
    {
        panelInicio.SetActive(true);
        panelSeleccion.SetActive(false);
        
        // Desactivar el boton de iniciar hasta que se conecte a Photon
        botonIniciar.interactable = false;
    }
    
    private void Start_PanelInicio()
    {
       notificacion.text = "Conectandose a Photon . . .";
       
       // Conectarse a Photon usando las configuraciones de Photon
       PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
       notificacion.text = "Entrando al Lobby . . .";
       PhotonNetwork.JoinLobby();
    }
    
    public override void OnJoinedLobby()
    {
      Invoke("activarBotonIniciar", 1f);
    }
    
    public void activarBotonIniciar()
    {
        notificacion.text = string.Empty;
        botonIniciar.onClick.AddListener(Iniciar);
        botonIniciar.interactable = true;
    }

    private void Iniciar()
    {
        
    }
    #endregion
    
    
    
    #region PANEL SELECCION
    [Header("PANEL SELECCION")]
    [SerializeField] GameObject panelSeleccion;
    
    #endregion
    
    
   
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
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
        string nombre = inputNombre.text;
        
        if (string.IsNullOrEmpty(nombre))
        {
            notificacion.text = "Por favor, ingrese un nombre";
            return;
        }
        
        if (nombre.Length > 10)
        {
            notificacion.text = "El nombre no debe tener mas de 10 caracteres";
            return;
        }
        
        PhotonNetwork.NickName = nombre;
        
        int salas = PhotonNetwork.CountOfRooms;
        
        if (salas == 0)
        {
            notificacion.text = "Creando sala . . .";
            RoomOptions config = new RoomOptions(){ MaxPlayers = 7 };
            
            bool seCreoSala = PhotonNetwork.CreateRoom("XP", config);
            
            if (!seCreoSala)
            {
                notificacion.text = "Error al crear sala";
            }
        }
        else
        {
            notificacion.text = "Uniendo a sala . . .";
            bool seUnioSala = PhotonNetwork.JoinRoom("XP");

            if (!seUnioSala)
            {
                notificacion.text = "Error al unirse a sala";
            }
            
        }
    }
    #endregion
    
    #region PANEL SELECCION
    [Header("PANEL SELECCION")]
    [SerializeField] private GameObject panelSeleccion;

    [SerializeField] private Transform panelJugadores;
    [SerializeField] private SlotJugador pfSlotJugador;
    
    private Dictionary<Player, SlotJugador> dicSlotJugadores;

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
    }
    
    public override void OnJoinedRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        panelInicio.SetActive(false);
        panelSeleccion.SetActive(true);

        InicializarSlots();
    }
    
    public void onPlayerEnteredRoom(Player newPlayer)
    {
        CrearSlot(newPlayer);
    }
    
    public void onPlayerLeftRoom(Player otherPlayer)
    {
        EliminarSlot(otherPlayer);
    }
    
    public void InicializarSlots()
    {
        dicSlotJugadores = new Dictionary<Player, SlotJugador>();
        
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            CrearSlot(player);
        }
    }
    
    public void CrearSlot(Player player)
    {
        SlotJugador slot = Instantiate(pfSlotJugador, panelJugadores);
        slot.Player = player;
        dicSlotJugadores.Add(player, slot);
    }
    
    public void EliminarSlot(Player player)
    {
        Destroy(dicSlotJugadores[player].gameObject);
    }
    #endregion



}

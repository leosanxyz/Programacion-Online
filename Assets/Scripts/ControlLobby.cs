using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class ControlLobby : MonoBehaviourPunCallbacks
{
    #region CORE

    private void Awake()
    {
        Awake_PanelInicio();
    }
    
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

    #region PANEL SELECCION - Core Photon
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            EnviarMensaje();
        }
    }
    public override void OnCreatedRoom()
    {
        InicializarChat();
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        panelInicio.SetActive(false);
        panelSeleccion.SetActive(true);

        InicializarSlots();
        ActualizarChat();

        StartCoroutine(CrControlSpam());
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CrearSlot(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        EliminarSlot(otherPlayer);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        ActualizarChat();
    }

    #endregion PANEL SELECCION - Core Photon

    #region PANEL SELECCION - Slots
    [Header("Panel Seleccion - Slots")]
    [SerializeField] private Transform panelJugadores;
    [SerializeField] private SlotJugador pfSlotJugador;
    private Dictionary<Player, SlotJugador> dicSlotJugadores;

    public void InicializarSlots()
    {
        dicSlotJugadores = new Dictionary<Player, SlotJugador>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            CrearSlot(player);
        }
    }
    private void CrearSlot(Player player)
    {
        SlotJugador slot = Instantiate(pfSlotJugador, panelJugadores);
        slot.Player = player;
        dicSlotJugadores.Add(player, slot);
    }

    private void EliminarSlot(Player player)
    {
        Destroy(dicSlotJugadores[player].gameObject);
    }
    #endregion PANEL SELECCION - Slots



    #region PANEL SELECCION - Chat
    [Header("Panel Seleccion - Chat")]
    [SerializeField] private RectTransform scrollView;
    [SerializeField] private RectTransform content;
    [SerializeField] private TMP_Text chat;
    [SerializeField] private TMP_InputField inputMensaje;
    [SerializeField] private Button botonEnviar;

    private int mensajesEnviados = 0;

    private void InicializarChat()
    {
        Hashtable propiedades = PhotonNetwork.CurrentRoom.CustomProperties;
        propiedades["Chat"] = "INICIO DE CHAT";

        PhotonNetwork.CurrentRoom.SetCustomProperties(propiedades);
    }

    private void EnviarMensaje()
    {
        if (mensajesEnviados >= 5)
            return;

        string mensaje = inputMensaje.text;

        if (mensaje == string.Empty)
            return;

        if (mensaje.Length> 40)
            return;

        var propiedades = PhotonNetwork.CurrentRoom.CustomProperties;

        string chat = propiedades["Chat"].ToString();

        chat += "\n" + PhotonNetwork.NickName + ": " + mensaje;
        propiedades["Chat"] = chat;
        PhotonNetwork.CurrentRoom.SetCustomProperties(propiedades);
        inputMensaje.text = string.Empty;
        inputMensaje.ActivateInputField();
        mensajesEnviados++;
    }

    private void ActualizarChat()
    {
       var propiedades = PhotonNetwork.CurrentRoom.CustomProperties;

        if(!propiedades.ContainsKey("Chat"))
            return;
        string chatString = propiedades["Chat"].ToString();
        chat.text = chatString;
        int offsetSuperior = 10;
        int alturaLinea = 29;
        int espacios = chat.text.Count(c => c == '\n');
        float altura = offsetSuperior + alturaLinea * espacios;
        content.sizeDelta = new Vector2(content.sizeDelta.x, altura);

        if(content.sizeDelta.y > scrollView.sizeDelta.y)
        {
            Vector3 posicionContent = content.localPosition;
            posicionContent.y = altura - scrollView.sizeDelta.y;
            content.localPosition = posicionContent;
        }
    }

    public IEnumerator CrControlSpam()
    {
        Repetir:
        yield return new WaitForSeconds(1f);

        if (mensajesEnviados >= 1)
            mensajesEnviados--;
        goto Repetir;
        
    }
    #endregion PANEL SELECCION - Chat
    #endregion
}

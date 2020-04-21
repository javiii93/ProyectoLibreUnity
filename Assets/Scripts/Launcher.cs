// --------------------------------------------------------------------------------------------------------------------
// <author>JLM AMS2/author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

using Photon.Realtime;

namespace Photon.Pun.Demo.PunBasics
{
#pragma warning disable 649

	/// <summary>
	/// Nos conéctamos, unimos a una sala aleatoria o creeamos una si estan llenas o no hay ninguna disponible.
	/// </summary>
	public class Launcher : MonoBehaviourPunCallbacks
    {

		#region Private Serializable Fields

		[Tooltip("The Ui Panel to let the user enter name, connect and play")]
		[SerializeField]
		private GameObject controlPanel;

		[Tooltip("The Ui Text to inform the user about the connection progress")]
		[SerializeField]
		private Text feedbackText;

		[Tooltip("The maximum number of players per room")]
		[SerializeField]
		private byte maxPlayersPerRoom = 4;

		[Tooltip("The UI Loader Anime")]
		[SerializeField]
		private LoaderAnime loaderAnime;

		#endregion

		#region Private Fields
		/// <summary>
		/// Mantenemos un registro del proceso actual. Como la conexión es asíncrona y se basa en varias devoluciones de llamada de Photon,
		/// necesitamos hacer un seguimiento de esto para ajustar adecuadamente el comportamiento cuando recibimos un call de Photon.
		/// Normalmente, esto se usa para la devolución de llamada OnConnectedToMaster ().
		/// </summary>
		bool isConnecting;

		/// <summary>
		/// El número de versión de este cliente. GameVersion separa a los usuarios entre sí (lo que te permite hacer cambios importantes).
		/// </summary>
		string gameVersion = "1";

		#endregion

		#region MonoBehaviour CallBacks

		/// <summary>
		/// El método MonoBehaviour llamado en GameObject por Unity durante la fase de inicialización (awake).
		/// </summary>
		void Awake()
		{
			if (loaderAnime==null)
			{
				Debug.LogError("<Color=Red><b>Missing</b></Color> loaderAnime Reference.",this);
			}

			// #Critical
			// esto asegura que podamos usar PhotonNetwork.LoadLevel () en el cliente maestro y todos los clientes en la misma sala sincronizan su nivel automáticamente
			PhotonNetwork.AutomaticallySyncScene = true;

		}

		#endregion


		#region Public Methods

		/// <summary>
		/// Inicia el proceso de conexión.
		/// - Si ya está conectado, intentamos unirnos a una sala aleatoria
		/// - si aún no está conectado, conecte esta instancia de aplicación a Photon Cloud Network
		/// </summary>
		public void Connect()
		{
			// queremos asegurarnos de que el registro esté limpio cada vez que nos conectemos, podríamos tener varios intentos fallidos si falla la conexión.
			feedbackText.text = "";

			// controlamos si podemos unirnos a una sala, porque cuando volvamos del juego recibiremos una devolución de llamada de que estamos conectados, por lo que debemos saber qué hacer.
			isConnecting = true;

			// ocultamos el ControlPanel (que es el que contiene el boton de play y demas)
			controlPanel.SetActive(false);

			// iniciamos el loarderAnimator
			if (loaderAnime!=null)
			{
				loaderAnime.StartLoaderAnimation();
			}


			// comprobamos si estamos conectados o no, nos unimos si lo estamos, de lo contrario iniciamos la conexión con el servidor.
			if (PhotonNetwork.IsConnected)
			{
				LogFeedback("Joining Room...");
				// # Crítical, en este punto necesitamos intentar unirnos a una sala aleatoria. Si falla, seremos notificados en OnJoinRandomFailed () y crearemos uno.
				PhotonNetwork.JoinRandomRoom();
			}else{

				LogFeedback("Connecting...");

				//# Crítical, primero debemos conectarnos a Photon Online Server.
				PhotonNetwork.GameVersion = this.gameVersion;
				PhotonNetwork.ConnectUsingSettings();
			}
		}

		/// <summary>
		/// Registra los comentarios en la vista de la interfaz de usuario para el jugador, a diferencia del editor de Unity para el desarrollador.
		/// </summary>
		/// <param name="message">Message.</param>
		void LogFeedback(string message)
		{
			// comprobamos si hay un feedbackText definido.
			if (feedbackText == null) {
				return;
			}

			// agregue nuevos mensajes como una nueva línea en la parte inferior del registro.
			feedbackText.text += System.Environment.NewLine+message;
		}

		#endregion


		#region MonoBehaviourPunCallbacks CallBacks

		// a continuación, implementamos algunas devoluciones de llamada de PUN
		// puedes encontrar las devoluciones de llamada de PUN en la clase MonoBehaviourPunCallbacks


		/// <summary>
		/// Llamado después de que se establece y autentica la conexión al maestro
		/// </summary>
		public override void OnConnectedToMaster()
		{

			// no queremos hacer nada si no intentamos unirnos a una sala.
			// este caso donde isConnecting es falso es típicamente cuando perdiste o saliste del juego, cuando se cargó este nivel, se llamará a OnConnectedToMaster, en ese caso
			// no queremos hacer nada.
			if (isConnecting)
			{
				LogFeedback("OnConnectedToMaster: Next -> try to Join Random Room");
				Debug.Log("Launcher: OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room.\n Calling: PhotonNetwork.JoinRandomRoom(); Operation will fail if no room found");

				// #Critical: Lo primero que intentamos hacer es unirnos a una sala existente. Si no hay, seremos llamados de nuevo con OnJoinRandomFailed()
				PhotonNetwork.JoinRandomRoom();
			}
		}

		/// <summary>
		/// Se llama cuando falla una llamada a JoinRandom(). Proporciona ErrorCode y mensaje.
		/// </summary>
		/// <remarks>
		/// Lo más probable es que todas las habitaciones estén llenas o no haya habitaciones disponibles. <br/>
		/// </remarks>
		public override void OnJoinRandomFailed(short returnCode, string message)
		{
			LogFeedback("<Color=Red>OnJoinRandomFailed</Color>: Next -> Create a new Room");
			Debug.Log("Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

			// #Critical: no pudimos unirnos a una sala aleatoria, tal vez no existe ninguna o están todas llenas. Creamos una nueva sala.
			PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = this.maxPlayersPerRoom});
		}


		/// <summary>
		/// Se llama despues de desconectarnos de photon server.
		/// </summary>
		public override void OnDisconnected(DisconnectCause cause)
		{
			LogFeedback("<Color=Red>OnDisconnected</Color> "+cause);
			Debug.LogError("Launcher:Disconnected");

			// #Critical: no pudimos conectarnos o nos desconectamos. No hay mucho que podamos hacer. Por lo general, debe haber un sistema de IU para permitir que el usuario intente conectarse nuevamente.
			loaderAnime.StopLoaderAnimation();

			isConnecting = false;
			controlPanel.SetActive(true);

		}

		/// <summary>
		/// Se llama al ingresar a una room (al crearla o unirse a ella). Llamado en todos los clientes (incluido el Master Client).
		/// </summary>
		/// <remarks>
		/// Este método se usa comúnmente para crear instancias de players.
		/// Si una partida debe iniciarse "actively", puede llamar a un [PunRPC] (@ ref PhotonView.RPC) activado por la pulsación de un botón del usuario o un temporizador.
		///
		/// Cuando se llama a esto, generalmente ya puedes acceder a los jugadores existentes en la sala a través de PhotonNetwork.PlayerList.
		/// Además, todas las propiedades personalizadas ya deberían estar disponibles como Room.customProperties. Verifique Room..PlayerCount para averiguar si
		/// hay suficientes jugadores en la sala para comenzar a jugar.
		/// </remarks>
		public override void OnJoinedRoom()
		{
			LogFeedback("<Color=Green>OnJoinedRoom</Color> with "+PhotonNetwork.CurrentRoom.PlayerCount+" Player(s)");
			Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.\nFrom here on, your game would be running.");

			// #Critical: Solo cargamos si somos el primer player, de lo contrario confiamos en PhotonNetwork.AutomaticallySyncScene para sincronizar nuestra escena de instancia.
			if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
			{
				Debug.Log("We load the 'Room for 1' ");

				// #Critical
				// Cargamos el nivel 1 
				PhotonNetwork.LoadLevel("Room for 1");

			}
		}

		#endregion
		
	}
}
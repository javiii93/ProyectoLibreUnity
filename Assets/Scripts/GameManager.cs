// --------------------------------------------------------------------------------------------------------------------
// <author>JLM AMS2/author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Realtime;

namespace Photon.Pun.Demo.PunBasics
{
#pragma warning disable 649

	/// <summary>
	/// Game manager.

	/// Se conecta y mira el estado de Photon
	/// Se ocupa de abandonar la sala y el juego
	/// Se ocupa de la carga de nivel (fuera de la sincronización en la sala)
	/// </summary>
	public class GameManager : MonoBehaviourPunCallbacks
    {

		#region Public Fields

		static public GameManager Instance;

		#endregion

		#region Private Fields

		private GameObject instance;

        [Tooltip("The prefab to use for representing the player")]
        [SerializeField]
        private GameObject playerPrefab;

		#endregion

		#region MonoBehaviour CallBacks

		/// <summary>
		/// Método MonoBehaviour llamado en GameObject por Unity durante la fase de inicialización.
		/// </summary>
		void Start()
		{
			Instance = this;

			// en caso de que empecemos esta demostración con la escena incorrecta, simplemente cargue la escena del menú
			if (!PhotonNetwork.IsConnected)
			{
				SceneManager.LoadScene("Launcher");

				return;
			}

			if (playerPrefab == null) { // #Tip Siempre hay que verificar que las propiedades publicas de los componentes estan correctamente introducidas

				Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
			} else {


				if (PlayerManager.LocalPlayerInstance==null)
				{
				    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);

					// estamos en una room. generamos un personaje para el jugador local. se sincroniza utilizando PhotonNetwork.
					PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f,5f,0f), Quaternion.identity, 0);
				}else{

					Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
				}


			}

		}

		/// <summary>
		/// Método MonoBehaviour llamado en GameObject por Unity en cada frame.
		/// </summary>
		void Update()
		{
			// si puslsamos el boton escape nos cerrara la app
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				QuitApplication();
			}
		}

		#endregion

		#region Photon Callbacks

		/// <summary>
		/// Se llama cuando se conecta un photon player. Necesitamos cargar una escena más grande.
		/// </summary>
		/// <param name="other">Other.</param>
		public override void OnPlayerEnteredRoom( Player other  )
		{
			Debug.Log( "OnPlayerEnteredRoom() " + other.NickName); // no vemos si eres el player que se conecto

			if ( PhotonNetwork.IsMasterClient )
			{
				Debug.LogFormat( "OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient ); // se llama antes que OnPlayerLeftRoom

				LoadArena();
			}
		}

		/// <summary>
		/// Se llama cuando se desconecta un Photon Player. Necesitamos cargar una escena más pequeña.
		/// </summary>
		/// <param name="other">Other.</param>
		public override void OnPlayerLeftRoom( Player other  )
		{
			Debug.Log( "OnPlayerLeftRoom() " + other.NickName ); // vemos cuando otros se desconectan

			if ( PhotonNetwork.IsMasterClient )
			{
				Debug.LogFormat( "OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient ); // se llama antes que OnPlayerLeftRoom

				LoadArena(); 
			}
		}

		/// <summary>
		/// Llamado cuando el jugador local salió de la sala. Necesitamos cargar la escena del launcher.
		/// </summary>
		public override void OnLeftRoom()
		{
			SceneManager.LoadScene("Launcher");
		}

		#endregion

		#region Public Methods

		public void LeaveRoom()
		{
			PhotonNetwork.LeaveRoom();
		}

		public void QuitApplication()
		{
			Application.Quit();
		}

		#endregion

		#region Private Methods

		void LoadArena()
		{
			if ( ! PhotonNetwork.IsMasterClient )
			{
				Debug.LogError( "PhotonNetwork : Trying to Load a level but we are not the master Client" );
			}

			Debug.LogFormat( "PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount );

			PhotonNetwork.LoadLevel("Room for "+PhotonNetwork.CurrentRoom.PlayerCount);
		}

		#endregion

	}

}
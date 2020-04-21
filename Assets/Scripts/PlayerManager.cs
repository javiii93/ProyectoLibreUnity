// --------------------------------------------------------------------------------------------------------------------
// <author>JLM AMS2/author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;

namespace Photon.Pun.Demo.PunBasics
{
	#pragma warning disable 649

    /// <summary>
    /// Player manager.
    /// Dirige los ataques de rayos laser
    /// </summary>
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Public Fields

        [Tooltip("The current Health of our player")]
        public float Health = 1f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #endregion

        #region Private Fields

        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        private GameObject playerUiPrefab;

        [Tooltip("The Beams GameObject to control")]
        [SerializeField]
        private GameObject beams;

        //booleano para saber si el player esta atacando
        bool IsFiring;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// El método MonoBehaviour llamado en GameObject por Unity durante la fase de inicialización (awake).
        /// </summary>
        public void Awake()
        {
            if (this.beams == null)
            {
                Debug.LogError("<Color=Red><b>Missing</b></Color> Beams Reference.", this);
            }
            else
            {
                this.beams.SetActive(false);
            }

            // #Important
            // utilizado en GameManager.cs: hacemos un seguimiento de la instancia localPlayer para evitar la instanciación cuando los niveles están sincronizados
            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
            }

            // #Critical
            // marcamos para no destruir el gameobject mientras lo cargamos, para que esa instancia sobreviva a la sincronización de niveles, lo que brinda una experiencia perfecta cuando los niveles se cargan.
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Método MonoBehaviour llamado en GameObject por Unity durante la fase de inicialización.
        /// </summary>
        public void Start()
        {
            CameraWork _cameraWork = gameObject.GetComponent<CameraWork>();

            if (_cameraWork != null)
            {
                if (photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><b>Missing</b></Color> CameraWork Component on player Prefab.", this);
            }

            // Creamos el UI
            if (this.playerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(this.playerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
            }

            #if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            #endif
        }


		public override void OnDisable()
		{
			// Siempre llamamos a base para eliminar callbacks
			base.OnDisable ();

			#if UNITY_5_4_OR_NEWER
			UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
			#endif
		}


        /// <summary>
        /// Método MonoBehaviour llamado en GameObject por Unity en cada frame.
        /// Procesamos las entradas del jugador local.
        /// Muestra y ocultamos los rayos laser
        /// Si la vida llega a 0, finaliza el juego
        /// </summary>
        public void Update()
        {
            // procesamos entradas y verificamos el estado, si somos el jugador local
            if (photonView.IsMine)
            {
                this.ProcessInputs();

                if (this.Health <= 0f)
                {
                    GameManager.Instance.LeaveRoom();
                }
            }

            if (this.beams != null && this.IsFiring != this.beams.activeInHierarchy)
            {
                this.beams.SetActive(this.IsFiring);
            }
        }

        /// <summary>
        /// Se llama al método MonoBehaviour cuando salta el trigger del Collider 'other'.
        /// Afecta la salud del jugador si el collider es un rayo laser
        /// </summary>
        public void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }


            // verificamos por nombre si la colision la hacen los rayos laser.
            if (!other.name.Contains("Beam"))
            {
                return;
            }

            this.Health -= 0.1f;
        }

        /// <summary>
        /// El método MonoBehaviour se llama una vez por frame cada vez que el trigger 'other' del Collider se activa.
        /// Vamos a modificar la salud mientras los rayos del player esten acticados y tocando al player
        /// </summary>
        /// <param name="other">Other.</param>
        public void OnTriggerStay(Collider other)
        {
            // Si no somos el player local no hacemos nada.
            if (!photonView.IsMine)
            {
                return;
            }

            // Nos centramos en los rayos
            // Simplemente checkeamos el nombre del del objeto para acticar el triger.
            if (!other.name.Contains("Beam"))
            {
                return;
            }

            // Modificamos la salud gradualmente cuando el rayo nos golpea constantemente, por lo que el player tiene que moverse para evitar la muerte.
            this.Health -= 0.1f*Time.deltaTime;
        }


        #if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
#endif


        /// <summary>
        /// Método MonoBehaviour llamado después de cargar un nuevo nivel con el indice 'level'.
        /// Volvvemos a cargar la IU del jugador porque se destruyó cuando cambiamos de nivel.
        /// También reposicionamos al player si está fuera de la arena actual.
        /// </summary>
        /// <param name="level">Level index loaded</param>
        void CalledOnLevelWasLoaded(int level)
        {
            // Verifica si estamos fuera de la Arena y si es el caso, renacemos en el centro de la arena, en una zona segura
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }

            GameObject _uiGo = Instantiate(this.playerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }

        #endregion

        #region Private Methods


#if UNITY_5_4_OR_NEWER
		void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
		{
			this.CalledOnLevelWasLoaded(scene.buildIndex);
		}
#endif

        /// <summary>
        /// Procesa las entradas. Esto solo debe utilizarse cuando el jugador tiene autoridad sobre este GameObject en red (photonView.isMine == true)
        /// </summary>
        void ProcessInputs()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                // no queremos disparar cuando interactuamos con botones de IU, por ejemplo. IsPointerOverGameObject significa IsPointerOver * UI * GameObject
                // observamos que no usamos GetbuttonUp (), porque se puede mover el mouse hacia abajo, moverse sobre un elemento de la IU y soltar, lo que conduciría a no bajar el indicador isFiring.
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    //	return;
                }

                if (!this.IsFiring)
                {
                    this.IsFiring = true;
                }
            }

            if (Input.GetButtonUp("Fire1"))
            {
                if (this.IsFiring)
                {
                    this.IsFiring = false;
                }
            }
        }

        #endregion

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Enviamos a los demas players nuestros datos(si estamos disparando y nuestra vida)
                stream.SendNext(this.IsFiring);
                stream.SendNext(this.Health);
            }
            else
            {
                // Los demas players de red reciven nuestros datos
                this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion
    }
}
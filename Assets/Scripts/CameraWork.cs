// --------------------------------------------------------------------------------------------------------------------
// <author>JLM AMS2/author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
	/// <summary>
	/// Camera work. Siguira al player local
	/// </summary>
	public class CameraWork : MonoBehaviour
	{
        #region Private Fields

	    [Tooltip("The distance in the local x-z plane to the target")]
	    [SerializeField]
	    private float distance = 7.0f;
	    
	    [Tooltip("The height we want the camera to be above the target")]
	    [SerializeField]
	    private float height = 3.0f;
	    
	    [Tooltip("Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground.")]
	    [SerializeField]
	    private Vector3 centerOffset = Vector3.zero;

	    [Tooltip("Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed.")]
	    [SerializeField]
	    private bool followOnStart = false;

	    [Tooltip("The Smoothing for the camera to follow the target")]
	    [SerializeField]
	    private float smoothSpeed = 0.125f;

        // cached transform of the target
        Transform cameraTransform;

		// maintain a flag internally to reconnect if target is lost or camera is switched
		bool isFollowing;
		
		// Cache for camera offset
		Vector3 cameraOffset = Vector3.zero;
		
		
        #endregion

        #region MonoBehaviour Callbacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase
        /// </summary>
        void Start()
		{
			// empieza a seguir a quien queremos
			if (followOnStart)
			{
				OnStartFollowing();
			}
		}


		void LateUpdate()
		{
			// El objetivo de transformación no se puede destruir mientras se carga el nivel,
			// así que debemos cubrir casos de esquina donde la cámara principal es diferente cada vez que cargamos una nueva escena, y volver a conectarnos cuando eso suceda
			if (cameraTransform == null && isFollowing)
			{
				OnStartFollowing();
			}

			// solo lo seguimos si hay algo que seguir
			if (isFollowing) {
				Follow ();
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Lanza el inicio después del evento.
		/// Usamos esto cuando no sabemos qué seguir, generalmente instancias administradas por la red de Photon.
		/// </summary>
		public void OnStartFollowing()
		{	      
			cameraTransform = Camera.main.transform;
			isFollowing = true;
			//no suavizamos nada, vamos directamente a la cámara correcta
			Cut();
		}
		
		#endregion

		#region Private Methods

		/// <summary>
		/// mejoramos la vision de la camara para que no se vea forzado
		/// </summary>
		void Follow()
		{
			cameraOffset.z = -distance;
			cameraOffset.y = height;
			
		    cameraTransform.position = Vector3.Lerp(cameraTransform.position, this.transform.position +this.transform.TransformVector(cameraOffset), smoothSpeed*Time.deltaTime);

		    cameraTransform.LookAt(this.transform.position + centerOffset);
		    
	    }

	   
		void Cut()
		{
			cameraOffset.z = -distance;
			cameraOffset.y = height;

			cameraTransform.position = this.transform.position + this.transform.TransformVector(cameraOffset);

			cameraTransform.LookAt(this.transform.position + centerOffset);
		}
		#endregion
	}
}
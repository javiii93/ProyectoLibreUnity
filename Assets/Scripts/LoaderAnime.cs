// --------------------------------------------------------------------------------------------------------------------
// <author>JLM AMS2/author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
	/// <summary>
	/// Animamos partículas alrededor para crear un "Ajax Loader" típico. esto es realmente muy importante para informar visualmente al usuario que algo está sucediendo
	/// o mejor decir que la aplicación no está congelada, por lo que una animación de algún tipo ayuda a asegurar al usuario que el sistema está inactivo y bien.
	/// TODO: se oculta cuando falla la conexión.
	/// </summary>
	public class LoaderAnime : MonoBehaviour {

		#region Public Variables

		[Tooltip("Angular Speed in degrees per seconds")]
		public float speed = 180f;

		[Tooltip("Radius os the loader")]
		public float radius = 1f;

		public GameObject particles;

		#endregion
		
		#region Private Variables

		Vector3 _offset;

		Transform _transform;

		Transform _particleTransform;

		bool _isAnimating;

		#endregion

		#region MonoBehaviour CallBacks

		/// <summary>
		/// El método MonoBehaviour llamado en GameObject por Unity durante la inicialización awake.
		/// </summary>
		void Awake()
		{
			// cache para hacerlo mas eficiente
			_particleTransform =particles.GetComponent<Transform>();
			_transform = GetComponent<Transform>();
		}


		/// <summary>
		/// Método MonoBehaviour llamado en GameObject por Unity en cada frame.
		/// </summary>
		void Update () {

			// solo activaremos las partículas giratorias si estamos ejecutando la animacion
			if (_isAnimating)
			{
				// rotamos con el tiempo. Time.deltaTime es obligatorio para tener una animación independiente de la velocidad de fotogramas,
				_transform.Rotate(0f,0f,speed*Time.deltaTime);

				// nos movemos desde el centro hasta el radio deseado para evitar que los artefactos visuales de las partículas salten de su lugar actual, no es muy agradable visualmente
				// entonces la partícula se centra en la escena para que cuando comience a girar, no salte y lentamente la animamos a su radio final dando una transición suave.
				_particleTransform.localPosition = Vector3.MoveTowards(_particleTransform.localPosition, _offset, 0.5f*Time.deltaTime);
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Empieza el loader de la animacion. lo ponemos visible
		/// </summary>
		public void StartLoaderAnimation()
		{
			_isAnimating = true;
			_offset = new Vector3(radius,0f,0f);
			particles.SetActive(true);
		}

		/// <summary>
		/// paramos el loader de la animacion. lo ponemos invisible
		/// </summary>
		public void StopLoaderAnimation()
		{
			particles.SetActive(false);
		}

		#endregion
	}
}
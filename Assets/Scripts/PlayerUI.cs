// --------------------------------------------------------------------------------------------------------------------
// <author>JLM AMS2/author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.PunBasics
{
#pragma warning disable 649

	/// <summary>
	/// Player UI.Bloquea la interfaz de usuario para seguir un GameObject PlayerManager en la scene,
	/// Mostrara el nombre y la salud del jugador
	/// </summary>
	public class PlayerUI : MonoBehaviour
    {
        #region Private Fields

	    [Tooltip("Pixel offset from the player target")]
        [SerializeField]
        private Vector3 screenOffset = new Vector3(0f, 30f, 0f);

	    [Tooltip("UI Text to display Player's Name")]
	    [SerializeField]
	    private Text playerNameText;

	    [Tooltip("UI Slider to display Player's Health")]
	    [SerializeField]
	    private Slider playerHealthSlider;

        PlayerManager target;

		float characterControllerHeight;

		Transform targetTransform;

		Renderer targetRenderer;

	    CanvasGroup _canvasGroup;
	    
		Vector3 targetPosition;

		#endregion

		#region MonoBehaviour Messages

		/// <summary>
		/// Método MonoBehaviour llamado en el GameObject por Unity durante la fase de inicialización (Awake)
		/// </summary>
		void Awake()
		{

			_canvasGroup = this.GetComponent<CanvasGroup>();
			
			this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
		}

		/// <summary>
        /// Metodo que se acutaliza en cada frame
		/// actualiza la barra de salud para reflejar la salud del jugador
		/// </summary>
		void Update()
		{
			// Se destruye a sí mismo si el objetivo es nulo, para evitar problemas de red
			if (target == null) {
				Destroy(this.gameObject);
				return;
			}


			// Muestra la vida del player
			if (playerHealthSlider != null) {
				playerHealthSlider.value = target.Health;
			}
		}

		/// <summary>
		/// Se llama al método MonoBehaviour después de que se hayan llamado todas las funciones del Update. Esto es útil para ordenar la ejecución del script.
		/// En nuestro caso, dado que estamos siguiendo un GameObject en movimiento, debemos proceder después de que el jugador se haya movido durante un cuadro en particular.
		/// </summary>
		void LateUpdate () {

			//No muestre la interfaz de usuario si no somos visibles para la cámara, evite posibles errores al ver la interfaz de usuario
			if (targetRenderer!=null)
			{
				this._canvasGroup.alpha = targetRenderer.isVisible ? 1f : 0f;
			}
			
			// #Critical
			// Sigue al GameObject por la pantalla.
			if (targetTransform!=null)
			{
				targetPosition = targetTransform.position;
				targetPosition.y += characterControllerHeight;
				
				this.transform.position = Camera.main.WorldToScreenPoint (targetPosition) + screenOffset;
			}

		}




		#endregion

		#region Public Methods

		/// <summary>
		/// Asigna un objetivo de jugador para seguir y representar
		/// </summary>
		/// <param name="target">Target.</param>
		public void SetTarget(PlayerManager _target){

			if (_target == null) {
				Debug.LogError("<Color=Red><b>Missing</b></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
				return;
			}

			// Referencias de caché para eficiencia porque las vamos a reutilizar.
			this.target = _target;
            targetTransform = this.target.GetComponent<Transform>();
            targetRenderer = this.target.GetComponentInChildren<Renderer>();


            CharacterController _characterController = this.target.GetComponent<CharacterController> ();

			// Obtiene datos del Player que no cambiarán durante el tiempo que este componente este activo
			if (_characterController != null){
				characterControllerHeight = _characterController.height;
			}

			if (playerNameText != null) {
                playerNameText.text = this.target.photonView.Owner.NickName;
			}
		}

		#endregion

	}
}
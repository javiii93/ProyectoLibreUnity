// --------------------------------------------------------------------------------------------------------------------
// <author>JLM AMS2/author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.PunBasics
{
	/// <summary>
	/// Este script hace que el usuario ingrese su nombre y este aparecerá sobre el jugador en el juego.
	/// </summary>
	[RequireComponent(typeof(InputField))]
	public class PlayerNameInputField : MonoBehaviour
	{
		#region Private Constants

		// Almacene la clave PlayerPref para evitar errores tipográficos
		const string playerNamePrefKey = "PlayerName";

		#endregion

		#region MonoBehaviour CallBacks

		/// <summary>
		/// Método MonoBehaviour llamado en GameObject por Unity durante la fase de inicialización.
		/// </summary>
		void Start () {
		
			string defaultName = string.Empty;
			InputField _inputField = this.GetComponent<InputField>();

			if (_inputField!=null)
			{
				if (PlayerPrefs.HasKey(playerNamePrefKey))
				{
					defaultName = PlayerPrefs.GetString(playerNamePrefKey);
					_inputField.text = defaultName;
				}
			}

			PhotonNetwork.NickName =	defaultName;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Establece el nombre del jugador y lo guarda en PlayerPrefs para futuras sesiones.
		/// </summary>
		/// <param name="value">The name of the Player</param>
		public void SetPlayerName(string value)
		{
			// #Important
			//si el nombre esta vacio da un mensaje de error
		    if (string.IsNullOrEmpty(value))
		    {
                Debug.LogError("Player Name is null or empty");
		        return;
		    }
			PhotonNetwork.NickName = value;

			PlayerPrefs.SetString(playerNamePrefKey, value);
		}
		
		#endregion
	}
}

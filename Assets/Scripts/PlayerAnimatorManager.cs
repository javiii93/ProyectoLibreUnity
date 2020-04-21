// --------------------------------------------------------------------------------------------------------------------
// <author>JLM AMS2/author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
	public class PlayerAnimatorManager : MonoBehaviourPun 
	{
        #region Private Fields

        [SerializeField]
	    private float directionDampTime = 0.25f;
        Animator animator;

		#endregion

		#region MonoBehaviour CallBacks

		/// <summary>
		/// M�todo MonoBehaviour llamado en GameObject por Unity durante la inicializaci�n.
		/// </summary>
		void Start () 
	    {
	        animator = GetComponent<Animator>();
	    }

		/// <summary>
		/// M�todo MonoBehaviour llamado en GameObject por Unity en cada frame.
		/// </summary>
		void Update () 
	    {

			// El control de prevenci�n est� conectado a Photon y representa al jugador local

			if ( photonView.IsMine == false && PhotonNetwork.IsConnected == true )
	        {
	            return;
	        }

			// A failSafe le falta el componente Animator en GameObject
			if (!animator)
	        {
				return;
			}

			// administrando el salto
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);			

			//solo permitimos saltar si estamos corriendo
            if (stateInfo.IsName("Base Layer.Run"))
            {
				// Cuando usamos el parametro disparar del trigger
                if (Input.GetButtonDown("Fire2")) animator.SetTrigger("Jump"); 
			}
           
			// administramos el movimiento
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

			// bloqueamos la velocidad negativa
            if( v < 0 )
            {
                v = 0;
            }

			// configuramos los parametros del Animator
            animator.SetFloat( "Speed", h*h+v*v );
            animator.SetFloat( "Direction", h, directionDampTime, Time.deltaTime );
	    }

		#endregion

	}
}
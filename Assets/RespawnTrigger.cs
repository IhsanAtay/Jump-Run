using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    [SerializeField]
    private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        CharacterController cc = other.gameObject.GetComponent<CharacterController>();
        if (cc != null)
        {
            this.Respawn(cc);
        }
    }

    private void Respawn(CharacterController cc)
    {
        cc.enabled = false;
        cc.gameObject.transform.position = this.respawnPoint.position;
        cc.enabled = true;

        // Velocity im Character Script zurücksetzen
        Character character = cc.gameObject.GetComponent<Character>();
        if (character != null)
        {
            character.ResetVelocity();
        }
    }
}
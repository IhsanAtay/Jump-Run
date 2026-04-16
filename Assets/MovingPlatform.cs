using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    private float platformSpeed;
    [SerializeField]
    private Vector3 start;
    [SerializeField]
    private Vector3 end;

    [SerializeField]
    private bool needsLever = false; // false = bewegt sich immer, true = wartet auf Lever
    private bool isActivated = false;

    private Vector3 lastPosition;
    private Vector3 velocity;

    void Start()
    {
        this.lastPosition = this.transform.localPosition;

        // Wenn kein Lever benötigt → sofort aktivieren
        if (!this.needsLever)
        {
            this.isActivated = true;
        }
    }

    void FixedUpdate()
    {
        if (!this.isActivated)
        {
            this.velocity = Vector3.zero;
            this.lastPosition = this.transform.localPosition;
            return;
        }

        float pingPong = Mathf.PingPong(Time.fixedTime * this.platformSpeed, 1.0f);
        var newPosition = Vector3.Lerp(this.start, this.end, pingPong);
        this.transform.localPosition = newPosition;

        this.velocity = (this.transform.localPosition - this.lastPosition) / Time.fixedDeltaTime;
        this.lastPosition = this.transform.localPosition;
    }

    public Vector3 GetVelocity()
    {
        return this.velocity;
    }

    // Wird vom Lever aufgerufen
    public void SetActivated(bool activated)
    {
        this.isActivated = activated;
    }
}
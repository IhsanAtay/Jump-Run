using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Lever : MonoBehaviour
{
    [SerializeField]
    private float switchTime;
    private bool isOn = false;
    private bool playerInRange = false;
    private bool interactPressed = false;
    private InputAction interactAction;

    [SerializeField]
    private LayerMask characterLayer; // ← Im Inspector auswählen!

    [SerializeField]
    private MovingPlatform[] controlledPlatforms; // Im Inspector befüllen

    [SerializeField]
    private Transform onPosition;
    [SerializeField]
    private Transform offPosition;
    [SerializeField]
    private GameObject leverHandle;

    void Start()
    {
        this.interactAction = InputSystem.actions.FindAction("Interact");
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & this.characterLayer) != 0)
        {
            this.playerInRange = true;
            Debug.Log("Player IN range!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & this.characterLayer) != 0)
        {
            this.playerInRange = false;
            Debug.Log("Player OUT of range!");
        }
    }

    IEnumerator InterpolateLeverCoroutine()
    {
        Vector3 startPosition, targetPosition;
        Quaternion startRotation, targetRotation;

        if (this.isOn)
        {
            startPosition = this.offPosition.localPosition;
            targetPosition = this.onPosition.localPosition;
            startRotation = this.offPosition.localRotation;
            targetRotation = this.onPosition.localRotation;
        }
        else
        {
            startPosition = this.onPosition.localPosition;
            targetPosition = this.offPosition.localPosition;
            startRotation = this.onPosition.localRotation;
            targetRotation = this.offPosition.localRotation;
        }

        float currInterpolationTime = 0.0f;
        while (currInterpolationTime < this.switchTime)
        {
            float percentage = currInterpolationTime / this.switchTime;

            this.leverHandle.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, percentage);
            this.leverHandle.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, percentage);

            currInterpolationTime += Time.deltaTime;
            yield return null;
        }

        this.leverHandle.transform.localPosition = targetPosition;
        this.leverHandle.transform.localRotation = targetRotation;
    }

    void ToggleLever()
    {
        this.isOn = !this.isOn;
        this.StartCoroutine(this.InterpolateLeverCoroutine());

        // Alle verbundenen Platforms aktivieren/deaktivieren
        foreach (var platform in this.controlledPlatforms)
        {
            platform.SetActivated(this.isOn);
        }
    }

    void Update()
    {
        if (this.interactAction.WasPressedThisFrame())
        {
            this.interactPressed = true;
        }
    }

    void FixedUpdate()
    {
        if (this.interactPressed)
        {
            if (this.playerInRange)
            {
                this.ToggleLever();
            }
            this.interactPressed = false;
        }
    }
}
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float moveDistance = 4f;
    [SerializeField] private AudioClip hitSound;

    private Vector3 startPosition;
    private bool movingForward = true;
    private Animator animator;
    private AudioSource audioSource;
    private bool isDead = false;

    void Start()
    {
        this.startPosition = this.transform.position;
        this.animator = this.GetComponent<Animator>();
        this.audioSource = this.gameObject.AddComponent<AudioSource>();
        this.audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (this.isDead) return;

        if (this.movingForward)
        {
            this.transform.position += this.transform.forward * this.moveSpeed * Time.deltaTime;

            if (Vector3.Distance(this.startPosition, this.transform.position) >= this.moveDistance)
            {
                this.movingForward = false;
                this.transform.Rotate(0, 180, 0);
                this.startPosition = this.transform.position;
            }
        }
        else
        {
            this.transform.position += this.transform.forward * this.moveSpeed * Time.deltaTime;

            if (Vector3.Distance(this.startPosition, this.transform.position) >= this.moveDistance)
            {
                this.movingForward = true;
                this.transform.Rotate(0, 180, 0);
                this.startPosition = this.transform.position;
            }
        }
    }

    public void GetHit()
    {
        if (this.isDead) return;
        this.isDead = true;

        if (this.hitSound != null)
            this.audioSource.PlayOneShot(this.hitSound);

        this.StartCoroutine(this.SquashAndDie());
    }

    IEnumerator SquashAndDie()
    {
        float elapsed = 0f;
        Vector3 originalScale = this.transform.localScale;
        Vector3 squashedScale = new Vector3(
            originalScale.x * 2f,
            originalScale.y * 0.2f,
            originalScale.z * 2f
        );

        // Squash Animation
        while (elapsed < 0.2f)
        {
            this.transform.localScale = Vector3.Lerp(originalScale, squashedScale, elapsed / 0.2f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        this.transform.localScale = squashedScale;

        yield return new WaitForSeconds(0.5f);

        // Langsam verschwinden
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(this.gameObject);
    }
}
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject impactEffect;

    private Rigidbody rb;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        Destroy(gameObject, lifetime);
    }

    public void Shoot(Vector3 direction)
    {
        rb.linearVelocity = direction * speed;
        transform.forward = direction;
    }

    private void Update()
    {
        if (!hasHit && rb.linearVelocity != Vector3.zero)
        {
            transform.forward = rb.linearVelocity.normalized;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        hasHit = true;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Aplicar da�o si es un enemigo
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        // Efecto de impacto
        if (impactEffect != null)
        {
            Instantiate(impactEffect, collision.contacts[0].point, Quaternion.identity);
        }

        // Pegar la flecha al objeto
        transform.SetParent(collision.transform);

        Destroy(gameObject, 3f);
    }
}
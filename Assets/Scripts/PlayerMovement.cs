using UnityEngine;
using Mirror; // Es vital usar la librería de Mirror

// Debe heredar de NetworkBehaviour, no del MonoBehaviour clásico
public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // REGLA DE ORO DE RED: 
        // Si este personaje no es el mío, detengo el código aquí mismo.
        // Así evitamos que tus teclas muevan la cápsula de tu amigo y viceversa.
        if (!isLocalPlayer) return;

        // Lectura de inputs (WASD o Flechas)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // Aplicamos el movimiento
        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized * speed * Time.deltaTime;
        transform.Translate(movement);
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

// GEÇİCİ TEST ARACI — gerçek PlayerController hazır olunca silinecek.
[RequireComponent(typeof(CharacterController))]
public class DebugMover : MonoBehaviour
{
    public float speed = 6f;
    public float turnSpeed = 120f;
    public float gravity = -20f;

    private CharacterController controller;
    private float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float forward = 0f;
        if (kb.wKey.isPressed) forward += 1f;
        if (kb.sKey.isPressed) forward -= 1f;

        float turn = 0f;
        if (kb.dKey.isPressed) turn += 1f;
        if (kb.aKey.isPressed) turn -= 1f;

        transform.Rotate(Vector3.up * turn * turnSpeed * Time.deltaTime);

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = transform.forward * forward * speed;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }
}

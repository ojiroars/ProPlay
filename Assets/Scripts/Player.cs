using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] protected Rigidbody2D _rb;
    private SpriteRenderer _renderer;
    private float _movementSpeed = 5f;
    private float _jumpingPower = 15f;
    private float _spinPower = 10f;
    private float inputHorizontal = 0f;
    private float inputVertical = 0f;

    private float angleLimit = 30f;
    private float checkUpdate = 0.1f;

    private bool _isGrounded;
    private bool spinInput;
    private bool isSpinning;
    private bool spinned;

    Vector2 _joystickInput = Vector2.zero;
    Vector2 _lastJoyStickInput = Vector2.zero;

    private int jumps = 0;
    private int correctSpins = 0;
    private int correctSpinsRequired = 4;

    public Animator animator; //animator

    public bool IsSpinning {
        get { return isSpinning; }
        private set { }
    }
    private void OnEnable() {
        spinInput = false;
        isSpinning = false;
    }
    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.flipX = true;
    }
    void Update() {
        animator.SetFloat("Speed", Mathf.Abs(inputHorizontal)); //walk animation

        Debug.Log(jumps);
        Movement();
        JoyStickInput();
        CheckJoyStickSpinning();
        //hier checken we nog voor een aantal andere variable een voornaamelijk als de spin check succesvol eruit is gekomen
        if (isSpinning && !_isGrounded && Input.GetKey(KeyCode.Joystick1Button1)) {
            if (jumps == 1 && !spinned) {
                jumps++;
                animator.SetBool("IsJumping", false);
                if (jumps <= 2) {
                    animator.SetBool("Spin", true);
                    _rb.velocity = new Vector2(_rb.velocity.x, _spinPower);
                    spinned = true;
                }
            }
        }
    }

    public void Movement() {
        if (_isGrounded) {
            inputHorizontal = Input.GetAxis("Horizontal") * _movementSpeed;
            inputVertical = Input.GetAxis("Vertical") * _movementSpeed;
        }

        if (inputHorizontal != 0) {
            _rb.velocity = new Vector2(inputHorizontal, _rb.velocity.y); //walk animatie
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton1) && jumps == 0 && _isGrounded || Input.GetKeyDown(KeyCode.Space) && jumps == 0 && _isGrounded) {
            jumps = 1; //jump animatie
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpingPower);
            animator.SetBool("IsJumping", true); //animator jump
        }

        if (inputHorizontal > 0) {
            _renderer.flipX = true;
        }

        else if (inputHorizontal < 0) {
            _renderer.flipX = false;
        }
    }
    private Vector2 JoyStickInput() {
        _joystickInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        return _joystickInput;
    }
    private void CheckJoyStickSpinning() {
        //checken of de huidge controller input anders is dan de vorige input
        if (_joystickInput != _lastJoyStickInput && !spinInput) {
            spinInput = true;
            StartCoroutine(CheckSpinning());//coroutine starten en hier gaan we alls checken.
        }
        //als het aantal progressie van een spin gelijk is aan het benodigden dan true
        if (correctSpins == correctSpinsRequired)
            isSpinning = true;
        else
            isSpinning = false;
    }
    IEnumerator CheckSpinning() {
        _lastJoyStickInput = _joystickInput;
        yield return new WaitForSeconds(checkUpdate);

        if (Vector2.Angle(_lastJoyStickInput, _joystickInput) >= angleLimit)// als de hoek tussen de twee vectors groter is dan angleLimit"30"
        {
            //gaat de progressie van een spin omhoog.
            correctSpins++;

            //clampen omdat we nog het aantal correctspins gaan vergelijken met correctSpinsRequired.
            correctSpins = Mathf.Clamp(correctSpins, 0, correctSpinsRequired);
        }
        else {
            correctSpins = 0;
        }
        spinInput = false;
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            _isGrounded = true;
            _rb.velocity = Vector2.zero;
            jumps = 0;
            spinned = false;
            animator.SetBool("IsJumping", false);
            animator.SetBool("Spin", false);
        }
    }
    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            _isGrounded = false;
        }
    }
}

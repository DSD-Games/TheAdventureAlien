using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController: MonoBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private float jumpForce;
    private bool isGrounded = true;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask whatLayerMask;

    [SerializeField] private float deadlyFlyTime = 3f;
    private float  flyTime = 0.0f;
    private float checkRadius = 0.3f;

    private int coins = 0;
    private int score = 0;

    public delegate void RecountedCoins(int coins);
    public event RecountedCoins OnRecountedCoins;

    public delegate void RecountedScore(int score);
    public event RecountedScore OnRecountedScore;

    [SerializeField] private UIController UIController;
    [SerializeField] private MusicManager musicManager;
    [SerializeField] private HealthModule healthModule;

    private Rigidbody2D rigidbody2D;
    private Animator animator;

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        healthModule.OnHealthChanged += HealthModule_OnHealthChanged;
        healthModule.OnDied += HealthModule_OnDied;

        OnRecountedScore?.Invoke(score);
        OnRecountedCoins?.Invoke(coins);
    }

    private void HealthModule_OnDied()
    {
        UIController.Lose();
    }

    private void HealthModule_OnHealthChanged(float health)
    {
        UIController.healthBar.handle.fillAmount = health / healthModule.MaxHealth;
        UIController.healthBar.amountText.text = health.ToString();
    }

    private void Update()
    {
        Flip();
        Jump();

    
        if (isGrounded) flyTime = 0.0f;
        else flyTime += Time.deltaTime;

        if (flyTime >= deadlyFlyTime && transform.position.y < 0) UIController.Lose();
    }
    private void FixedUpdate()
    {
        CheckGround();
        rigidbody2D.velocity = new Vector2(speed * Input.GetAxis("Horizontal"), rigidbody2D.velocity.y );
        if ((Input.GetAxis("Horizontal") > 0 || Input.GetAxis("Horizontal") < 0) && isGrounded)
        {
            animator.SetInteger("Behavior", 1);
        }
        else if(isGrounded)
        {
            animator.SetInteger("Behavior", 0);
        }
            
    }
    public void ZeroPhysic()
    {
        animator.SetInteger("Behavior", 0);
        rigidbody2D.velocity = Vector2.zero;
    }
    private void Flip()
    {
        if (Input.GetAxis("Horizontal") > 0)
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        else if(Input.GetAxis("Horizontal") < 0)
            transform.localRotation = Quaternion.Euler(0, 180, 0);
    }
    private void Jump()
    {
        if (isGrounded & Input.GetKeyDown(KeyCode.Space))
        {
            rigidbody2D.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        }
    }
    public void RecountScore(int score)
    {
        this.score += score;
        OnRecountedScore?.Invoke(this.score);
    }
    public void RecountCoins(int coins)
    {
        this.coins += coins;
        OnRecountedCoins?.Invoke(this.coins);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {      

        if (collision.gameObject.tag == "Coin")
        {
            musicManager.OnPlayOneShotAndEndLast(1);
            Destroy(collision.gameObject);
            RecountCoins(1);
        }
        if (collision.gameObject.tag == "Star")
        {
            musicManager.OnPlayOneShotAndEndLast(3);
            Destroy(collision.gameObject);
            RecountScore(1);
        }
        if (collision.gameObject.tag == "Lava")
        {
            healthModule.RecountHealth(-100);
        }
    }
    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatLayerMask);
        if (isGrounded == false)
        {
            animator.SetInteger("Behavior", 2);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Spike")
        {
            healthModule.RecountHealth(-100);
        }
    }
}

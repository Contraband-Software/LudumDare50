using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    public enum World {Alive, Dead };

    [Header("Game State")]
    public World currentWorld = World.Alive;

    [Header("Stats")]
    public float attackRadius;
    public float attackCooldownTime = 0.5f;
    public float worldChangeCooldownTime = 0.5f;
    public LayerMask enemyLayers;

    [Header("Variables")]
    public float moveSpeed;
    private Vector2 moveDirection;

    [Header("Important References")]
    public BoxCollider2D BoxColliderComponent;
    public Rigidbody2D RigidBodyComponent;
    public GameObject AnimationControllerChild;
    public GameObject AudioContainer;
    public GameObject AttackWarningEle;
    public Volume GVol;
    public GameObject DeadCamera;

    [System.Serializable]
    public class DirectionColliders
    {
        public BoxCollider2D Left;
        public BoxCollider2D Right;
        public BoxCollider2D Vertical;
    }
    [Header("Colliders")]
    public DirectionColliders directionalColliders = new DirectionColliders();

    [System.Serializable]
    public class AnimationTypes
    {
        public string Attack;
        public string Move;
    };
    [System.Serializable]
    public class AnimationsProperties
    {
        public AnimationTypes Up = new AnimationTypes();
        public AnimationTypes Down = new AnimationTypes();
        public AnimationTypes Left = new AnimationTypes();
        public AnimationTypes Right = new AnimationTypes();
    }
    [Header("Animations")]
    public AnimationsProperties animationStates = new AnimationsProperties();

    //private variables
    private CameraController CameraController;
    private Animator ani;

    private float currentWorldChangeCooldown = 0;
    private float attackCooldown = 0;
    [HideInInspector]
    public float attackAnimationTimeLeft = 0;
    [HideInInspector]
    public bool attacking = false;

    private AnimationTypes CurrentAnimationType;
    private string CurrentAnimationState;

    private AudioSource AttackSuccess;
    private AudioSource AttackFailiure;
    private AudioSource WorldSwitch;
    private AudioSource Damaged;

    private Warning AttackWarning;

    private PlayerHealth playerHealth;

    // Start is called before the first frame update
    void Start()
    {
        Physics2D.IgnoreLayerCollision(9, 12);

        AttackWarning = AttackWarningEle.GetComponent<Warning>();
        CameraController = Camera.main.GetComponent<CameraController>();
        //CameraController.pCon = this;

        //turn off all colliders initially
        ToggleCollider(directionalColliders.Vertical);

        ani = AnimationControllerChild.GetComponent<Animator>();

        CurrentAnimationType = animationStates.Down;
        CurrentAnimationState = CurrentAnimationType.Move;

        AttackSuccess = AudioContainer.transform.Find("AttackSuccess").gameObject.GetComponent<AudioSource>();
        AttackFailiure = AudioContainer.transform.Find("AttackFail").gameObject.GetComponent<AudioSource>();
        WorldSwitch = AudioContainer.transform.Find("WorldSwitch").gameObject.GetComponent<AudioSource>();
        Damaged = AudioContainer.transform.Find("Damaged").gameObject.GetComponent<AudioSource>();

        playerHealth = GetComponent<PlayerHealth>();

        if (currentWorld == World.Dead)
        {
            ChangePPtoDead();
            
        } else
        {
            ChangePPtoAlive();
        }

        CameraController.pCon = this;

        

        //Bloom bloom;
        //GVol.profile.TryGet<Bloom>(out bloom);
        //bloom.intensity.value = 1000.0F;
    }

    void ChangePPtoDead()
    {
        ColorAdjustments ca;
        GVol.profile.TryGet<ColorAdjustments>(out ca);
        ca.saturation.value = -100.0F;
        ca.postExposure.value = 1.5F;

        Vignette vi;
        GVol.profile.TryGet<Vignette>(out vi);
        vi.smoothness.value = 1.0F;

        DeadCamera.SetActive(true);
    }

    void ChangePPtoAlive()
    {
        ColorAdjustments ca;
        GVol.profile.TryGet<ColorAdjustments>(out ca);
        ca.saturation.value = 0.0F;
        ca.postExposure.value = 0.0F;

        Vignette vi;
        GVol.profile.TryGet<Vignette>(out vi);
        vi.smoothness.value = 0.196F;

        DeadCamera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        ProcessMovementInputs();
        ProcessKeyInputs();

        AttackCooldown();


        
    }
    private void FixedUpdate()
    {
        Move();
    }

    public void PlayDamagedSound()
    {
        Damaged.Play();
    }

    private void ProcessKeyInputs()
    {
        //LEFT MOUSE CLICK
        if (Input.GetMouseButtonDown(0))
        {
            //print("LEFT CLICKED");
            Attack();
        }

        //SPACE BAR
        if (Input.GetKeyDown(KeyCode.Space) && currentWorldChangeCooldown == 0)
        {
            if (currentWorld == World.Alive)
            {
                ChangePPtoDead();
            }
            else
            {
                ChangePPtoAlive();
            }

            currentWorldChangeCooldown = worldChangeCooldownTime;
            //print("SPACEBAR PRESSED");
            CameraController.SwitchWorld();


            if (!WorldSwitch.isPlaying) WorldSwitch.Play();
        }
    }

    private void Attack()
    {
        if (currentWorld == World.Alive)
        {
            if (attackCooldown == 0)
            {
                //PlayAttackAnimation();
                attackCooldown = attackCooldownTime;
                attacking = true;

                //moveDirection = new Vector2(0, 0);

                PlayAnimation(true);

                bool hitSomeone = false;

                //see what enemies are in range of the AOE attack
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayers);
                foreach (Collider2D enemyCollider in hitEnemies)
                {
                    enemyCollider.gameObject.GetComponent<EnemyController>().Die();

                    hitSomeone = true;

                    playerHealth.GainHealth();
                }

                if (hitSomeone)
                {
                    if (!AttackSuccess.isPlaying) AttackSuccess.Play();
                }
                else
                {
                    if (!AttackFailiure.isPlaying) AttackFailiure.Play();
                }
            }
        } else
        {
            AttackWarning.WarnPlayer();
        }
    }

    private void AttackCooldown()
    {
        //when can you attack again
        if(attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
            if(attackCooldown < 0)
            {
                attackCooldown = 0;
            }
        }
        
        if (attackAnimationTimeLeft > 0)
        {
            attackAnimationTimeLeft -= Time.deltaTime;
            if (attackAnimationTimeLeft < 0)
            {
                attackAnimationTimeLeft = 0;
                attacking = false;

                print("FINISHED SWIPING");

                PlayAnimation(false);
            }
        }

        if (currentWorldChangeCooldown > 0)
        {
            currentWorldChangeCooldown -= Time.deltaTime;
            if (currentWorldChangeCooldown < 0)
            {
                currentWorldChangeCooldown = 0;
            }
        }
    }

    void ProcessMovementInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;

        if (attackAnimationTimeLeft == 0)
        {
            if (Mathf.Abs(moveX) > Mathf.Abs(moveY))
            {
                if (moveX > 0)
                {
                    SetAnimationType(animationStates.Right);
                    ToggleCollider(directionalColliders.Right);
                }
                else
                {
                    SetAnimationType(animationStates.Left);
                    ToggleCollider(directionalColliders.Left);
                }
            }
            else
            {
                if (moveY > 0)
                {
                    SetAnimationType(animationStates.Up);
                    ToggleCollider(directionalColliders.Vertical);
                }
                else if (moveY < 0)
                {
                    SetAnimationType(animationStates.Down);
                    ToggleCollider(directionalColliders.Vertical);
                }
            }

            PlayAnimation(false);
        }
    }

    private void ToggleCollider(BoxCollider2D toggleCol)
    {
        directionalColliders.Left.enabled = false;
        directionalColliders.Right.enabled = false;
        directionalColliders.Vertical.enabled = false;

        toggleCol.enabled = true;
    }

    private void Move()
    {
        RigidBodyComponent.AddForce(moveDirection * moveSpeed);
    }

    private void SetAnimationType(AnimationTypes name)
    {
        CurrentAnimationType = name;
    }

    private void PlayAnimation(bool attack)
    {
        if (attack)
        {
            if (CurrentAnimationState == CurrentAnimationType.Attack) return;
            CurrentAnimationState = CurrentAnimationType.Attack;

            ani.Play(CurrentAnimationState);

            attackAnimationTimeLeft = ani.GetCurrentAnimatorStateInfo(0).length;

        } else
        {
            if (CurrentAnimationState == CurrentAnimationType.Move) return;
            CurrentAnimationState = CurrentAnimationType.Move;

            ani.Play(CurrentAnimationState);
        }
    }
}






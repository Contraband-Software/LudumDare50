using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public enum MoveState { Calm, Panic};
    public enum Direction { North, East, West};
    public enum Form { Human, Ghost};

    [Header("State")]
    public MoveState moveState;
    public Direction facing;
    public Form form;
    [HideInInspector]
    public bool traveling = false;
    [HideInInspector]
    public bool sawAGhost = false;
    [HideInInspector]
    public bool allowMove = true;
    private bool panicPositionSelected = false;

    [Header("Details")]
    public Color transitioningColour;
    public Color deadColour;
    private float transitionTime = 1.2f;
    public Transform humanTransform;
    public float maxViewRange = 4f;
    public LayerMask terrainObjects;
    public Vector2 panicRunRange = new Vector2(10f, 15f);

    public Vector2 wanderRange = new Vector2();
    public Vector2 wanderCooldownRange = new Vector2();
    public float panicMoveSpeed;
    public float walkMoveSpeed;
    private float wanderCooldown;

    [Header("Ghost Form")]
    public Sprite ghostSprite;
    public float ghostMoveSpeed;
    public float HitRange = 10;
    public float groundOffset = 0.5F;
    public float HitCoolDown = 1.5F;
    public float Damage = 1.5F;

    [Header("Pathfinding")]
    private NavMeshAgent agent;

    [Header("Important References")]
    public SpriteRenderer spriteRend;
    public BoxCollider2D coll;
    public Animator anim;
    public Rigidbody2D rb;
    [HideInInspector]
    public CompassPointer CompassPointerScript;
    [HideInInspector]
    public PlayerController pCon;
    [HideInInspector]
    public GameObject PlayerObject;
    [HideInInspector]
    public PlayerHealth PlayerObjectHealth;

    private AudioSource DamageSound;

    private float CurrentHitCoolDown = 0;

    // Start is called before the first frame update
    void Start()
    {
        PlayerObject = GameObject.Find("Player");
        pCon = PlayerObject.GetComponent<PlayerController>();
        CompassPointerScript = PlayerObject.transform.Find("Compass Pivot").GetComponent<CompassPointer>();

        PlayerObjectHealth = PlayerObject.GetComponent<PlayerHealth>();

        CompassPointerScript.enemyPositions.Add(humanTransform);

        DamageSound = GetComponent<AudioSource>();

        moveState = MoveState.Calm;
        form = Form.Human;
        facing = Direction.East;
        spriteRend.flipX = true;
        traveling = false;
        sawAGhost = false;
        allowMove = true;
        panicPositionSelected = false;
        wanderCooldown = 0f;
        spriteRend.sortingLayerName = "Humans";

        //pathfinding initialization
        agent = gameObject.GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;


        spriteRend.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        //agent.SetDestination(pCon.gameObject.transform.position);
        Transitioning();


        /*
        if (Input.GetMouseButtonDown(0) && allowMove)
        {

            
            print("LEFT CLICK");
            Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target = new Vector3(target.x, target.y, 0f);
            agent.SetDestination(target);
            
        }
        */

        //gets scythed, stop it from being able to move
        if (!allowMove && form == Form.Human)
        {
            agent.enabled = false;
        }

        HasReachedTarget();

        //if it can move, it can check if it can panic
        if (allowMove && form == Form.Human)
        {
            CheckIfShouldPanic();
        }

        //if its dead, it moves towards player without pathfinder
        if(form == Form.Ghost)
        {
            MoveTowardsPlayerAsGhost();
        }

        if (CurrentHitCoolDown > 0)
        {
            CurrentHitCoolDown -= Time.deltaTime;
            if (CurrentHitCoolDown < 0) CurrentHitCoolDown = 0;
        }

        //panicking, higher move speed
        if(moveState == MoveState.Panic)
        {
            agent.speed = panicMoveSpeed;
        }
        else
        {
            agent.speed = walkMoveSpeed;
        }


        //lastly, if its in the alive form, and moving is still allowed, it will wander around
        WanderCycle();

    }

    private void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, groundOffset, 0), HitRange);
    }

    private void FixedUpdate()
    {
        if(allowMove && form == Form.Human)
        {
            AnimationSwitching();
        }
        if (form == Form.Ghost)
        {
            anim.Play("Ghost");
        }
    }
    private void CheckDamage()
    {
        if ((PlayerObject.transform.position - (transform.position + new Vector3(0, groundOffset, 0))).magnitude < HitRange && CurrentHitCoolDown == 0)
        {
            CurrentHitCoolDown = HitCoolDown;
            PlayerObjectHealth.Damage(Damage);
            DamageSound.Play();
        }
    }

    //called the moment that this enemy dies
    public void Die()
    {
        gameObject.layer = LayerMask.NameToLayer("TransitioningEnemy");
        anim.Play("Dying");
        allowMove = false;
        agent.velocity = Vector3.zero;
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("TransitioningEnemy");
        }

        spriteRend.color = transitioningColour;
        CompassPointerScript.enemyPositions.Remove(humanTransform);
    }
    
    //void OnCollisionEnter(Collision2D collision)
    //{

    //    if (collision.gameObject.tag == "Dead ")
    //    {
    //        Physics.IgnoreCollision(collision.collider, collider);
    //    }

    //}

    public void Transitioning()
    {
        if(gameObject.layer == LayerMask.NameToLayer("TransitioningEnemy"))
        {
            if(transitionTime > 0)
            {
                transitionTime -= Time.deltaTime;
            }
            else
            {

                gameObject.layer = LayerMask.NameToLayer("DeadEnemy");

                spriteRend.sprite = ghostSprite;
                form = Form.Ghost;
                coll.isTrigger = false;
                spriteRend.sortingLayerName = "Ghosts";

                foreach (Transform child in gameObject.transform)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
                }
                spriteRend.color = deadColour;
            }
        }
    }

    private void WanderCycle()
    {
        //standing still, cooldown off
        if(wanderCooldown <= 0f && HasReachedTargetWander())
        {
            float radius = Mathf.Floor(Random.Range(wanderRange.x, wanderRange.y));
            Vector3 targetWanderPosition = Random.insideUnitCircle * radius;
            agent.SetDestination(humanTransform.position + targetWanderPosition);
            wanderCooldown = Random.Range(wanderCooldownRange.x, wanderCooldownRange.y);
        }

        //got to position to wander to
        if(wanderCooldown > 0f && HasReachedTargetWander())
        {
            wanderCooldown -= Time.deltaTime;
        }
        

    }

    private void CheckIfShouldPanic()
    {
        if ((PlayerInConeOfVision() || sawAGhost) && pCon.currentWorld == PlayerController.World.Alive)
        {
            moveState = MoveState.Panic;
            sawAGhost = true;
            GoToPanicPosition();
        }
        else
        {
            moveState = MoveState.Calm;
        }
    }

    private void GoToPanicPosition()
    {
        if (!panicPositionSelected)
        {
            float radius = Mathf.Floor(Random.Range(panicRunRange.x, panicRunRange.y));
            Vector3 targetPanicPosition = Random.insideUnitCircle * radius;
            agent.SetDestination(humanTransform.position + targetPanicPosition);

            panicPositionSelected = true;
        }
    }

    private bool PlayerInConeOfVision()
    {
        //angle between 0 and 360
        Vector2 positionDifference = pCon.transform.position - humanTransform.transform.position;
        float angle = Mathf.Atan2(positionDifference.y, positionDifference.x) * Mathf.Rad2Deg - 90f;
        if(angle > 0f)
        {
            angle = -360 + angle;
        }
        angle *= -1f;

        //SECTORS

        //UPPER SECTOR
        if((300f <= angle) || (angle >= 0 && angle <= 60f))
        {
            if(facing == Direction.North && positionDifference.magnitude <= maxViewRange)
            {
                if (NoTerrainBlocking(positionDifference.normalized))
                {
                    return true;
                }
            }
            return false;
        }

        //RIGHT SECTOR
        if(angle > 60 && angle <= 180)
        {
            if(facing == Direction.East && positionDifference.magnitude <= maxViewRange)
            {
                if (NoTerrainBlocking(positionDifference.normalized))
                {
                    return true;
                }
            }
            return false;
        }

        //LEFT
        if(angle > 180 && angle < 300f)
        {
            if(facing == Direction.West && positionDifference.magnitude <= maxViewRange)
            {
                if (NoTerrainBlocking(positionDifference.normalized))
                {
                    return true;
                }
            }
            return false;
        }
        return false;
    }

    private bool NoTerrainBlocking(Vector2 direction)
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(humanTransform.transform.position, direction, (int)maxViewRange, terrainObjects);
        Debug.DrawRay(humanTransform.position, direction*maxViewRange);

        if(hitInfo.collider == null)
        {
            return true;
        }
        else
        {
            if(hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                return true;
            }
        }
        return false;
    }

    private void HasReachedTarget()
    {
        Vector2 targetPosition = new Vector2(agent.destination.x, agent.destination.y);
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 difference = targetPosition - currentPosition;
        if(difference.magnitude < 0.02)
        {
            traveling = false;
            sawAGhost = false;
            panicPositionSelected = false;
        }
        else
        {
            traveling = true;
        }
    }

    private bool HasReachedTargetWander()
    {
        if (moveState != MoveState.Panic)
        {
            Vector2 targetPosition = new Vector2(agent.destination.x, agent.destination.y);
            Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
            Vector2 difference = targetPosition - currentPosition;
            if (difference.magnitude < 0.02)
            {
                return true;
            }
        }
        
        return false;
    }

    public void AnimationSwitching()
    {
        FlipToDirection();

        if (!traveling)
        {
            if(facing == Direction.North)
            {
                anim.Play("IdleUp");
            }
            else
            {
                anim.Play("Idle");
            }
        }

        else
        {
            //moving upwards
            if (agent.velocity.y > 0f && agent.velocity.y > Mathf.Abs(agent.velocity.x))
            {
                facing = Direction.North;
                //walk upwards
                if (moveState == MoveState.Calm)
                {
                    anim.Play("WalkUp");
                }
                //panic upwards
                if (moveState == MoveState.Panic)
                {
                    anim.Play("PanicUp");
                }
            }

            //moving horizontally OR down
            else
            {
                //RIGHT
                if(agent.velocity.x > 0f)
                {
                    facing = Direction.East;
                }
                //LEFT
                if(agent.velocity.x < 0f)
                {
                    facing = Direction.West;
                }

                FlipToDirection();

                if(moveState == MoveState.Calm)
                {
                    anim.Play("Walk");
                }
                if (moveState == MoveState.Panic)
                {
                    anim.Play("Panic");
                }
            }
        }
    }

    private void FlipToDirection()
    {
        //if dead, check rb.velocity and set direction
        if(form == Form.Ghost)
        {
            if (rb.velocity.x > 0f)
            {
                facing = Direction.West;
            }
            else
            {
                facing = Direction.East;
            }
        }

        //Rotate to face orientation
        if (facing == Direction.East)
        {
            spriteRend.flipX = true;
        }
        if (facing == Direction.West)
        {
            spriteRend.flipX = false;
        }
    }

    private void MoveTowardsPlayerAsGhost()
    {
        if (pCon.currentWorld == PlayerController.World.Dead)
        {
            FlipToDirection();

            CheckDamage();
            //find angle
            Vector2 positionDiff = pCon.transform.position - humanTransform.position;
            Vector2 direction = positionDiff.normalized;
            rb.velocity = direction * ghostMoveSpeed * Time.deltaTime;

        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        
    }
}

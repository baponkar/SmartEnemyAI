using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyHealth))]
public class SmartEnemyAI : MonoBehaviour
{
    [Header("Smart Enemy AI")]
    Transform target;
    [SerializeField] public float chaseRange = 5f;
    [SerializeField] public float turnSpeed = 5f;
    [SerializeField] public float patrolRadius = 5f;
    [SerializeField] public float attackRadius = 3f;
    private float tempX,tempY,tempZ,distanceToTempTarget;
    private Vector3 tempTarget;
    float distanceToTarget = Mathf.Infinity;
    NavMeshAgent navMeshAgent; 
    public bool isProvoked = false;
    EnemyHealth health;
    Animator animator;
    private bool walkPointSet = false;

    private EnemyHealth enemyHealth;

   [HideInInspector]
    public bool pathAvailable;
    public NavMeshPath navMeshPath;


    public float timer = 0.0f;
    public float maxTime = 1.0f;
    public float maxDistance = 1.0f;
    public Vector3 velocity;
    public float walkSpeed = 0.3f;
    public float jogSpeed = 0.66f;
    public float sprintSpeed = 1f;

    void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }
    void Start()
    {
        
        target = GameObject.FindGameObjectsWithTag("Player")[0].transform;
        health = GetComponent<EnemyHealth>();
        navMeshPath = new NavMeshPath();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        tempX = transform.position.x;
        tempY = transform.position.y;
        tempZ = transform.position.z;

        navMeshAgent.SetDestination(transform.position + new Vector3(0,patrolRadius,0));
    }

    void Update()
    {
        timer -= Time.deltaTime;
        velocity = navMeshAgent.velocity;
        animator.SetFloat("Velocity", velocity.magnitude);
        if(enemyHealth.hitPoints < 100f)
        {
            isProvoked = true;
        } 
        if(health.IsDead())
        {
            enabled = false;//after dead
            navMeshAgent.enabled = false;
            animator.SetTrigger("dead");
        }
        distanceToTarget = Vector3.Distance(target.position, transform.position);
        if(isProvoked)
        {
            EngageTarget();
        }
        else if(distanceToTarget <= chaseRange )
       {
            isProvoked = true;
           navMeshAgent.SetDestination(target.position); 
           
       }
       else if(distanceToTarget > chaseRange)
       {
            Patrolling();
       }
    }

    private void Patrolling()
    {
        if(!walkPointSet)
        {
            SearchingPoint();
        }
        if(walkPointSet)
        {
            if (timer < 0)
            {
                navMeshAgent.SetDestination(tempTarget);
                timer = maxTime;
            }
        }

        Vector3 distanceToTempTarget = transform.position - tempTarget;
        if(distanceToTempTarget.magnitude <= navMeshAgent.stoppingDistance + 0.5f)
        {
            walkPointSet = false;
        }

        
        
    }



    private void SearchingPoint()
    {
        Vector3 tempPos = Vector3.zero;
        
        tempPos = RandomNavmeshLocation();
        tempTarget = new Vector3(transform.position.x + tempPos.x, transform.position.y, transform.position.z + tempPos.z);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(tempPos, out hit, 0.1f, NavMesh.AllAreas) )
        {
            if(navMeshAgent.CalculatePath(hit.position, navMeshPath)) //check a path available or not
            {
                tempTarget = hit.position;
                FacePatrol();
                walkPointSet = true;
            }
        }
        else
        {
            tempTarget = Vector3.zero;
            walkPointSet = false;
        }
    }

   public Vector3 RandomNavmeshLocation() {
         Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
         randomDirection += transform.position;
         NavMeshHit hit;
         Vector3 finalPosition = Vector3.zero;
         if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1)) {
             finalPosition = hit.position;
            walkPointSet = true;
         }
         
         FacePatrol();
         return finalPosition;
     }

    private void EngageTarget()
    {
        FaceTarget();

        if( distanceToTarget >= navMeshAgent.stoppingDistance)
        {
            ChaseTarget();
            
        }
        if(distanceToTarget <= navMeshAgent.stoppingDistance) 
        {
            AttackTarget();
        }
    }

    private void ChaseTarget()
    {  
        animator.SetBool("isAttacking", false);
        if(!health.IsDead())
        {
            if((transform.position - target.position).magnitude > attackRadius)
            {
                navMeshAgent.SetDestination(target.position);
            }
            else
            {
                navMeshAgent.isStopped = true;
            }
        }
    }

    private void AttackTarget()
    {
        animator.SetBool("isAttacking", true);
        animator.SetLayerWeight(1, 1f);
    }

    private void IdleState()
    {
        animator.SetBool("isAttacking", false);
    }

    private void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3 (direction.x,0,direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation,Time.deltaTime*turnSpeed);
    }
    private void FacePatrol()
    {   
        Vector3 direction = (tempTarget- transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3 (direction.x,0,direction.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation,Time.time*turnSpeed);
    }



    public void OnDamageTaken()
    {
        isProvoked = true;
    }

    void OnDrawGizmosSelected()
    { 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);   
    }
}

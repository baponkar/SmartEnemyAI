
using UnityEngine;
//using UnityEngine.Animations.Rigging;
public class EnemyHealth : MonoBehaviour
{
   [SerializeField] public float hitPoints = 100f;
   //[SerializeField] public GameObject deadEffect;
   bool isDead = false;
   Rigidbody rb;
   
   
   void Awake()
   {
       rb = GetComponent<Rigidbody>();
       rb.useGravity = false;
       //deadEffect.SetActive(false);
   }
  public bool IsDead()
  {
      return isDead;
  }

  void Update()
  {
      if(hitPoints <= 0)
      {
          Die();
      }
  }
 

   
   public void TakeDamage(float damage)
   {
       BroadcastMessage("OnDamageTaken");
        //GetComponent<EnemyAI>().OnDamageTaken();
       if(hitPoints > 0)
       {
           hitPoints -= damage;
       }
       else if (hitPoints <= 0)
       {
           //Destroy(this.gameObject);
           Die();
       }
       
   }

   public void Die()
   {   if(isDead)
        {
            return;
        }
   
       isDead = true;
       //GetComponent<RigBuilder>().enabled = false;
       rb.useGravity = true;
       GetComponent<Animator>().SetTrigger("die");
   }
}

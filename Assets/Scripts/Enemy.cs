﻿using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity {

	public enum State { Idle, Chasing, Attacking };
	State currentState;

	public ParticleSystem deathEffect;
	public static event System.Action OnDeathStatic;

	NavMeshAgent pathfinder;
	Transform target;
	LivingEntity targetEntity;
	Material skinMaterial;
	Material originalMaterial;

	Color originalColor;

	float attackDistanceThreshold = .5f;
	float timeBetweenAttacks = 1f;
	float damage = 1;

	float nextAttackTime;
	float myCollisionRadius;
	float targetCollisionRadius;

	bool hasTarget;

	//lightning damage
	public bool isHit;  //only used for debugging in instor 
	public bool isChaining { get; set; }
	public float lightningDamageReceived;
	

    private void Awake() {
		pathfinder = GetComponent<NavMeshAgent>();
		originalMaterial = skinMaterial;

		if (GameObject.FindGameObjectWithTag("Player") != null) {
			hasTarget = true;

			target = GameObject.FindGameObjectWithTag("Player").transform;
			targetEntity = target.GetComponent<LivingEntity>();

			myCollisionRadius = GetComponent<CapsuleCollider>().radius;
			targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
		}
	}

    protected override void Start() {
		base.Start();

		if (hasTarget) {
			currentState = State.Chasing;
			targetEntity.OnDeath += OnTargetDeath;
			StartCoroutine(UpdatePath());
		}
	}

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float health, Color skinColor) {
		pathfinder.speed = moveSpeed;

		if(hasTarget) {
			damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }

		startingHealth = health;
		ParticleSystem.MainModule _main = deathEffect.main;
		_main.startColor = new Color(skinColor.r, skinColor.g, skinColor.b, 1);
		skinMaterial = GetComponent<Renderer>().material;
		skinMaterial.color = skinColor;
		originalColor = skinMaterial.color;
	}

	public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection) {
		AudioManager.instance.PlaySound("Impact", transform.position);
		if (damage >= health) {
			if(OnDeathStatic != null) {
				OnDeathStatic();
            }
			AudioManager.instance.PlaySound("Enemy Death", transform.position);
			deathEffect.GetComponent<ParticleSystemRenderer>().material = skinMaterial;
			Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)), deathEffect.main.startLifetime.constant);
		}
		base.TakeHit(damage, hitPoint, hitDirection);
	}

	void OnTargetDeath() {
		hasTarget = false;
		currentState = State.Idle;
		isChaining = false;	
	}

	void Update() {
		if (hasTarget) {
			if (Time.time > nextAttackTime) {
				float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
				if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) {
					nextAttackTime = Time.time + timeBetweenAttacks;
					AudioManager.instance.PlaySound("Enemy Attack", transform.position);
					StartCoroutine(Attack());
				}

			}
		}
	}

    private void FixedUpdate() {
        if(isChaining) {
			TakeDamage(lightningDamageReceived);
		}
    }

    IEnumerator Attack() {

		currentState = State.Attacking;
		pathfinder.enabled = false;

		Vector3 originalPosition = transform.position;
		Vector3 dirToTarget = (target.position - transform.position).normalized;
		Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

		float attackSpeed = 3;
		float percent = 0;

		skinMaterial.color = Color.red;
		bool hasAppliedDamage = false;

		while (percent <= 1) {

			if (percent >= .5f && !hasAppliedDamage) {
				hasAppliedDamage = true;
				targetEntity.TakeDamage(damage);
			}

			percent += Time.deltaTime * attackSpeed;
			float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
			transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

			yield return null;
		}

		skinMaterial.color = originalColor;
		currentState = State.Chasing;
		pathfinder.enabled = true;
	}

	IEnumerator UpdatePath() {
		float refreshRate = .25f;

		while (hasTarget) {
			if (currentState == State.Chasing) {
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);
				if (!dead) {
					pathfinder.SetDestination(targetPosition);
				}
			}
			yield return new WaitForSeconds(refreshRate);
		}
	}

	public Enemy FindClosestEnemy(float chainRadius, LayerMask enemyMask) {
		Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, chainRadius, enemyMask);

		float min = 10000f;

		Transform closestEnemy = null;

		foreach (Collider c in nearbyEnemies) {
			if (gameObject.GetInstanceID() != c.gameObject.GetInstanceID() && !c.GetComponent<Enemy>().isChaining) {
				float distance = Vector3.Distance(transform.position, c.transform.position);
				if (distance < min) {
					min = distance;
					closestEnemy = c.transform;
				}
			}
		}
		if (!closestEnemy) return null;

		return closestEnemy.GetComponent<Enemy>();
	}
}
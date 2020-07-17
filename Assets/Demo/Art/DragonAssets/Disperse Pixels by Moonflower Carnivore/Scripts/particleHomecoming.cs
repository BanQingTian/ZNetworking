/*
This script is copyrighted by Moonflower Carnivore and published on Unity Asset Store in 2018.

It stores the Vector3 world position of each particle at the beginning of its emission to an array.
After some delay, it moves all particles which should have already been strayed away back to the starting position.
*/

using System;
using System.Collections;
using UnityEngine;
//[ExecuteInEditMode]
public class particleHomecoming : MonoBehaviour {
	[Tooltip("How fast the particle is guided to the closest target.")]
	public float speed = 10f;
	[Tooltip("Cap the maximum speed to prevent particle from being flung too far from the missed target.")]
	public float maxSpeed = 50f;
	[Tooltip("How long in the projectile begins being guided towards the target. Higher delay and high particle start speed requires greater distance between attacker and target to avoid uncontrolled orbitting around the target.")]
	public float homingDelay = 1f;
	[Tooltip("Does particles are instead offset to new position?")]
	public bool teleport = false;
	[Tooltip("New world position for teleport effect.")]
	public Vector3 newPositionVector;
	public Transform newPositionTransform;
	ParticleSystem m_System;
	ParticleSystem.Particle[] m_Particles;
	Vector3[] startPosition;
	int numParticlesAlive;
	
	void OnEnable() {
		m_System = GetComponent<ParticleSystem>();
		m_Particles = new ParticleSystem.Particle[m_System.main.maxParticles];
		StartCoroutine(getStartPosition());
	}
	
	IEnumerator getStartPosition() {
		yield return new WaitForSeconds(0.05f);
		numParticlesAlive = m_System.GetParticles(m_Particles);
		startPosition = new Vector3[numParticlesAlive];
		for (int i = 0; i < numParticlesAlive; i++) {
			startPosition[i] = m_Particles[i].position;
		}
		//Debug.Log(startPosition[0]);
		yield return new WaitForSeconds(homingDelay);
		float t=0;
		if (teleport) {
			if (newPositionTransform) {
				newPositionVector = newPositionTransform.position;
			}
		} else {
			newPositionVector = this.transform.position;
		}
		
		Vector3 positionDiff = newPositionVector - this.transform.position;
		while (m_System.GetParticles(m_Particles) != 0) {
			t += Time.deltaTime;
			for (int i = 0; i < numParticlesAlive; i++) {
				Vector3 diff = startPosition[i] - m_Particles[i].position + positionDiff;
				m_Particles[i].velocity = Vector3.ClampMagnitude(Vector3.Lerp(m_Particles[i].velocity, diff * speed, t), maxSpeed);
			}
			m_System.SetParticles(m_Particles, numParticlesAlive);
			yield return null;
		}
	}
}
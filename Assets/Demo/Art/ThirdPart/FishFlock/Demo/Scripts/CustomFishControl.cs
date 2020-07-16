using FishFlock;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFishControl : MonoBehaviour
{
    [Tooltip("The Flock Controller to modify the fish instances from.")]
    public FishFlockController controller;
    [Tooltip("Minumum acceleration value to keep it non-zero and non-negative.")]
    public float minAccelValue = 5.0f;
    [Tooltip("Minumum speed value to keep it non-zero and non-negative.")]
    public float minSpeedValue = 0.01f;
    [Tooltip("Minumum turn speed value to keep it non-zero and non-negative.")]
    public float minTurnSpeedValue = 2;
    [Tooltip("The value to sum or subtract from the fish's acceleration, speed and turn speed.")]
    public float valueToAffect = 0.5f;

    void OnEnable()
    {
        // Register the callback update function on the controller.
        controller.OnUpdateFishEvent += OnUpdateFish;
        // Start the coroutine that bounces the value.
        StartCoroutine(InAndOut());
    }

    void OnDisable()
    {
        // Unregister the callback update function from the controller.
        controller.OnUpdateFishEvent -= OnUpdateFish;
    }

    /// <summary>
    /// This function will make the value bounce between positive and negative on an interval of 1.8 seconds.
    /// </summary>
    /// <returns></returns>
    IEnumerator InAndOut()
    {
        yield return new WaitForSeconds(1.8f);

        valueToAffect *= -1.0f;
        StartCoroutine(InAndOut());
    }

    /// <summary>
    /// This function receives the current Fish Behaviour that is being looped and modify it's values.
    /// </summary>
    /// <param name="behaviour">FishBehaviour struct containing the fish's data.</param>
    /// <returns>The modified FishBehaviour.</returns>
    public FishBehaviour OnUpdateFish(FishBehaviour behaviour)
    {
        FishBehaviour modifiedBehaviour = behaviour;

        modifiedBehaviour.acceleration -= Random.Range(valueToAffect - (valueToAffect / 2), valueToAffect);
        if (modifiedBehaviour.acceleration <= minAccelValue)
            modifiedBehaviour.acceleration = minAccelValue;
        else if (modifiedBehaviour.acceleration >= controller.maxAcceleration * 2)
            modifiedBehaviour.acceleration = controller.maxAcceleration;

        modifiedBehaviour.speed -= Random.Range(valueToAffect - (valueToAffect / 2), valueToAffect);
        if (modifiedBehaviour.speed <= minSpeedValue)
            modifiedBehaviour.speed = minSpeedValue;
        else if (modifiedBehaviour.speed >= controller.maxSpeed * 2)
            modifiedBehaviour.speed = controller.maxSpeed;

        modifiedBehaviour.turnSpeed -= Random.Range(valueToAffect - (valueToAffect / 2), valueToAffect);
        if (modifiedBehaviour.turnSpeed <= minTurnSpeedValue)
            modifiedBehaviour.turnSpeed = minTurnSpeedValue;
        else if (modifiedBehaviour.turnSpeed >= controller.maxTurnSpeed * 2)
            modifiedBehaviour.turnSpeed = controller.maxTurnSpeed;

        return modifiedBehaviour;
    }
}

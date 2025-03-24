using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class GoblinAI : MonoBehaviour
{
    [Header("Goblin Behaviour Settings")]
    [SerializeField] private float actionCooldown = 5f;

    [SerializeField] private Transform goblinHands;
    [SerializeField] private NavMeshAgent agent;

    #region Goblin Action Chance Variables

    [Header("Goblin Actions Chances")]
    private List<GoblinActions> _goblinActions;

    [Tooltip("Chance of throwing items on the floor")]
    [SerializeField] private float chanceOfItems = 0.4f;
    private float _throwDistanceThreshold = 0.5f; // Allow throwing if within this distance

    [Tooltip("Chance of throwing items from crates or shelves")]
    [SerializeField] private float chanceOfCrates = 0.3f;

    [Tooltip("Chance of slurping from the cauldron")]
    [SerializeField] private float chanceOfSlurping = 0.2f;

    [Tooltip("Chance of scaring a customer inline")]
    [SerializeField] private float chanceOfScaring = 0.1f;

    private float _totalWeight;

    #endregion

    #region Wandering Settings

    [Header("Goblin Wander Settings")]
    [Tooltip("Chance of wandering. This only happens on non challenge day")]
    [SerializeField] private float wanderingWeight = 0.3f;

    [SerializeField] private float wanderRadius = 5f;

    #endregion

    private bool _isGoblinActive;
    private Vector3 _currentDestination;

    #region Sfx

    [Header("SFX")]
    [SerializeField] private bool enableSfx;

    [SerializeField] private float noiseTimerMin;
    [SerializeField] private float noiseTimerMax;
    [SerializeField] private SFXLibrary goblinIdle;
    [SerializeField] private SFXLibrary goblinCry;
    [SerializeField] private SFXLibrary goblinScream;
    [SerializeField] private SFXLibrary goblinSlurp;
    [SerializeField] private SFXLibrary rummageStings;
    [SerializeField] private AudioClip rummageSound;
    [SerializeField] private AudioClip goblinMovement;

    private CustomTimer _noiseTimer;

    #endregion

    //Idle SFX variables
    private bool _isFree = false;

    // References to the things the goblin can interact with - the only thing that changes is ingredients.
    private CrateHolder[] _crates;
    private List<GameObject> _ingredients;
    private CauldronInteraction[] _cauldrons;
    private QueueManager _queue;
    private GameObject _customerToScare;

    // Reference to item that might be moving
    private GameObject _itemToThrow;

    [Header("Slime Movement")]
    [SerializeField] private LayerMask slime;

    [SerializeField] private float slowMultiplier = 0.5f;
    private bool _isInSlime;
    private float _defaultSpeed;

    // Coroutines to handle the goblin behaviour
    private Coroutine _goblinBehaviour;
    private Coroutine _currentAction;

    private void Start()
    {
        SetActions();

        if (agent != null)
            _defaultSpeed = agent.speed;

        _noiseTimer = new CustomTimer(Random.Range(noiseTimerMin, noiseTimerMax), false);
        _crates = FindObjectsOfType<CrateHolder>();
        _queue = FindObjectOfType<QueueManager>();
        _cauldrons = FindObjectsOfType<CauldronInteraction>();
        _isFree = false;
    }

    //update loop to handle playing of idle sounds
    private void Update()
    {
        // If the game state isn't in gameplay, nothing will happen
        if (GameManager.Instance.gameState != GameState.Gameplay) return;

        // Using the custom timer to time the goblin noises. 
        if (_noiseTimer.UpdateTimer())
        {
            SFXLibrary goblinSounds;
            // if the goblin is free the goblin sounds will be goblin idle else it'll be goblin cry.
            if (_isFree)
            {
                goblinSounds = goblinIdle;
            }
            else
            {
                goblinSounds = goblinCry;
            }

            AudioManager.instance.sfxManager.PlaySFX(SFX_Type.GoblinSounds, goblinSounds.PickAudioClip(),
                true); //play audio clip

            _noiseTimer = new CustomTimer(Random.Range(noiseTimerMin, noiseTimerMax), false);
        }

        // if the goblin isn't active return
        if (!_isGoblinActive) return;

        // Check below the character to see if they've stepped in a slime trail
        if (Physics.Raycast(transform.position, Vector3.down, 1f, slime))
        {
            if (!_isInSlime)
            {
                _isInSlime = true;
                agent.speed *= slowMultiplier;
            }
        }
        else
        {
            if (!_isInSlime) return;

            _isInSlime = false;
            agent.speed = _defaultSpeed;
        }
    }

    #region OnEnable / OnDisable / OnDestroy Events

    private void OnEnable()
    {
        Actions.OnStartGoblin += StartChaos;
        Actions.OnEndGoblin += EndChaos;
    }

    private void OnDisable()
    {
        Actions.OnStartGoblin -= StartChaos;
        Actions.OnEndGoblin -= EndChaos;
    }

    private void OnDestroy()
    {
        Actions.OnStartGoblin -= StartChaos;
        Actions.OnEndGoblin -= EndChaos;
    }

    #endregion

    private void StartChaos(bool isChallengeDay)
    {
        _isGoblinActive = true;
        _isFree = true;

        agent.enabled = true;
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _goblinBehaviour = StartCoroutine(BehaviourLoop(isChallengeDay));
    }

    private void EndChaos()
    {
        _isGoblinActive = false;
        if (_goblinBehaviour != null)
        {
            StopCoroutine(_currentAction);
            StopCoroutine(_goblinBehaviour);
        }
    }

    private void SetActions()
    {
        _goblinActions = new List<GoblinActions>
        {
            new GoblinActions
            {
                actionName = "ThrowItems", weight = chanceOfItems,
                ActionToExecute = () => StartCoroutine(ThrowItemsOffFloor())
            },
            new GoblinActions
            {
                actionName = "ThrowIngredients", weight = chanceOfCrates,
                ActionToExecute = () => StartCoroutine(ThrowItemsFromCrate())
            },
            new GoblinActions
            {
                actionName = "SlurpCauldron", weight = chanceOfSlurping,
                ActionToExecute = () => StartCoroutine(SlurpCauldron())
            },
            new GoblinActions
            {
                actionName = "ScareCustomer", weight = chanceOfScaring,
                ActionToExecute = () => StartCoroutine(ScareCustomer())
            }
        };

        _totalWeight = 0f;
        // Calculate total weight by summing all the individual weights
        foreach (var action in _goblinActions)
        {
            _totalWeight += action.weight;
        }

        if (_totalWeight != 1)
            Debug.LogError("Total weight of goblin actions must be 1");
    }

    private IEnumerator BehaviourLoop(bool isChallengeDay)
    {
        while (true)
        {
            if (_currentAction != null)
                StopCoroutine(_currentAction);

            // if it's a challenge day goblin will be more active. else they need a wandering time
            if (!isChallengeDay)
            {
                var random = Random.value;

                if (random <= wanderingWeight)
                {
                    _currentAction = StartCoroutine(Wandering());
                    yield break;
                }
            }

            // pick random action
            PickRandomAction();
            yield return new WaitForSeconds(actionCooldown);
        }
    }

    private void PickRandomAction()
    {
        var roll = Random.value * _totalWeight;

        // Loop through the actions and accumulate their weights until the roll value is reached
        float accumulatedWeight = 0f;
        foreach (var action in _goblinActions)
        {
            accumulatedWeight += action.weight;

            if (!(roll <= accumulatedWeight)) continue;

            action.ActionToExecute?.Invoke();
            Debug.Log($"{action.actionName} was chosen");
            return;
        }
    }

    #region Goblin Actions

    // Throwing items on the floor
    private IEnumerator ThrowItemsOffFloor()
    {
        // Set the item to null before going into the loop
        _itemToThrow = null;
        
        while (true) // Keep trying until a valid item is found and thrown
        {
            FindFloorItems(); // Refresh the list of available items

            if (_ingredients.Count == 0) yield break; // No items left, exit

            // Pick a random item
            if(_itemToThrow == null)
                _itemToThrow = _ingredients[Random.Range(0, _ingredients.Count)];

            // Validate the item
            if (!IsItemValid(_itemToThrow)) continue;

            // Set goblin destination
            var lastKnownPosition = _itemToThrow.transform.position;
            agent.SetDestination(lastKnownPosition);
            AudioManager.instance.sfxManager.StartConstantSFX(goblinMovement); // Start movement sound

            while (!ReachedThrowingItem())
            {
                // If the item is no longer valid, restart the loop
                if (!IsItemValid(_itemToThrow))
                {
                    AudioManager.instance.sfxManager.StopConstantSFX();
                    yield return null;
                    break;
                }

                // Update destination if the item moved
                if (_itemToThrow != null && _itemToThrow.transform.position != lastKnownPosition)
                {
                    lastKnownPosition = _itemToThrow.transform.position;
                    agent.SetDestination(lastKnownPosition);
                }

                yield return null;
            }

            // If the goblin reached the item, throw it
            if (IsItemValid(_itemToThrow) && ReachedThrowingItem())
            {
                AudioManager.instance.sfxManager.StopConstantSFX();

                var randomDir = new Vector3(Random.Range(-1f, 1f), 1, Random.Range(-1f, 1f)).normalized;
                _itemToThrow.transform.DOJump(randomDir, 1, 1, 0.5f);

                yield return new WaitForSeconds(1f); // Simulated action time
                yield break; // Successfully threw an item, exit
            }
        }
    }


    private IEnumerator ThrowItemsFromCrate()
    {
        var crate = _crates[Random.Range(0, _crates.Length)];

        agent.SetDestination(crate.transform.position);
        var amount = Random.Range(1, 4);

        AudioManager.instance.sfxManager.StartConstantSFX(goblinMovement); //start movement sound
        while (!ReachedCrate())
        {
            yield return null;
        }

        AudioManager.instance.sfxManager.StopConstantSFX(); // stop movement sound

        AudioManager.instance.sfxManager.StartConstantSFX(rummageSound); //playing the sound for rummaging
        for (var i = 0; i < amount; i++)
        {
            crate.GoblinInteraction(goblinHands);
        }

        yield return new WaitForSeconds(1f); // Simulated action time
        AudioManager.instance.sfxManager.StopConstantSFX(); //stop rummage sound
    }


    private IEnumerator SlurpCauldron()
    {
        var cauldron = _cauldrons[Random.Range(0, _cauldrons.Length)];

        agent.SetDestination(cauldron.transform.position);
        AudioManager.instance.sfxManager.StartConstantSFX(goblinMovement); //start movement sound

        while (!ReachedCrate())
        {
            yield return null;
        }

        AudioManager.instance.sfxManager.StopConstantSFX(); // stop movement sound

        cauldron.GetComponent<CauldronInteraction>().GoblinInteraction();
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.GoblinSounds, goblinSlurp.PickAudioClip(), true);

        yield return new WaitForSeconds(2f);
    }

    private IEnumerator ScareCustomer()
    {
        while (true) // Keep trying until a valid customer is found
        {
            _customerToScare = _queue.FindCustomer(); // Find a valid customer

            if (_customerToScare == null) yield break; // No customers left, exit

            Vector3 lastKnownPosition = _customerToScare.transform.position;

            // Set goblin's destination
            agent.SetDestination(lastKnownPosition);
            AudioManager.instance.sfxManager.StartConstantSFX(goblinMovement); // Start movement sound

            while (!ReachedCrate())
            {
                yield return null; // Wait for the next frame

                // Refresh customers list and check if the target is still valid
                if (_customerToScare == null || !_customerToScare.GetComponent<CustomerBehaviour>().HasJoinedQueue)
                {
                    AudioManager.instance.sfxManager.StopConstantSFX(); // Stop movement sound
                    yield return null; // Try finding a new customer next frame
                    break;
                }

                // Update destination if the customer moved
                if (_customerToScare.transform.position != lastKnownPosition)
                {
                    lastKnownPosition = _customerToScare.transform.position;
                    agent.SetDestination(lastKnownPosition);
                }
            }

            // If the goblin successfully reached the target, scare them
            if (_customerToScare != null && ReachedCrate())
            {
                AudioManager.instance.sfxManager.StopConstantSFX();

                _queue.ScareCustomer(_customerToScare);
                AudioManager.instance.sfxManager.PlaySFX(SFX_Type.GoblinSounds, goblinScream.PickAudioClip(), true);

                yield return new WaitForSeconds(2f); // Simulated scare time
                yield break; // Scared the customer, exit
            }
        }
    }

    #endregion

    #region Wandering Action

    private IEnumerator Wandering()
    {
        PickNewDestination();

        while (!ReachedCrate())
        {
            yield return null;
        }
    }


    private void PickNewDestination()
    {
        _currentDestination = RandomNavSphere(transform.position, wanderRadius, -1);
        agent.SetDestination(_currentDestination);
    }

    private Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
    {
        var randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;

        return NavMesh.SamplePosition(randomDirection, out var navHit, distance, layermask)
            ? navHit.position
            : origin; // If no valid point found, stay put
    }

    #endregion

    /// <summary> Used to find all the ingredients on the floor </summary>
    private void FindFloorItems()
    {
        _ingredients = new List<GameObject>();
        var ing = FindObjectsOfType<PickupObject>();
        var potions = FindObjectsOfType<PotionOutput>();

        foreach (var ingredient in ing)
        {
            if (!ingredient.AddedToCauldron() && !ingredient.isHeld)
                _ingredients.Add(ingredient.gameObject);
        }

        foreach (var potion in potions)
        {
            var pickup = potion.GetComponent<PickupObject>();

            if (!potion.givenToCustomer && !pickup.isHeld)
                _ingredients.Add(potion.gameObject);
        }
    }

    private bool IsItemValid(GameObject item)
    {
        if (item == null) return false;

        // Check if item is a PickupObject and has been added to the cauldron or picked up
        var pickup = item.GetComponent<PickupObject>();
        if (pickup != null)
        {
            if (pickup.isHeld || pickup.AddedToCauldron()) return false;
        }

        // Check if item is a PotionOutput and has been given to a customer
        var potion = item.GetComponent<PotionOutput>();
        if (potion != null)
        {
            if (potion.givenToCustomer) return false;
        }

        return true;
    }


    /// <summary> Check if the agent has reached the target </summary>
    private bool ReachedCrate()
    {
        return !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance &&
               (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }

    private bool ReachedThrowingItem()
    {
        var distance = Vector3.Distance(transform.position, _itemToThrow.transform.position);
        return distance <= agent.stoppingDistance || distance <= _throwDistanceThreshold;
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class GoblinAI : MonoBehaviour
{
    #region Behaviour Settings

    [Header("Goblin Behaviour Settings")]
    [SerializeField] private Transform goblinHands;
    [SerializeField] private float pauseAfterAction = 5f;
    private NavMeshAgent _agent;
    private List<GoblinState> _possibleStates = new();
    private GoblinState _wanderingState;
    private GoblinState _currentState;
    private bool _isChallengeDay;
    #endregion

    #region Goblin Action Chance Variables
    [Header("Goblin Actions Chances")]
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

    [Header("Goblin Wander Settings")]
    [Tooltip("Chance of wandering. This only happens on non challenge day")]
    [SerializeField] private float wanderingWeight = 0.3f;

    private bool _isGoblinActive;

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
    private CauldronInteraction[] _cauldrons;
    private QueueManager _queue;

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
        // Initializes States
        SetAllStates();

        _agent = GetComponent<NavMeshAgent>();

        if (_agent != null)
            _defaultSpeed = _agent.speed;

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
            PlayIdleSound();
            // Start new timer
            _noiseTimer = new CustomTimer(Random.Range(noiseTimerMin, noiseTimerMax), false);
        }

        // if the goblin isn't active return
        if (!_isGoblinActive) return;

        if (_currentState == null)
        {
            PickState();
        }

        CheckForSlime();
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
        _isChallengeDay = isChallengeDay;

        _agent.enabled = true;
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private void EndChaos()
    {
        _isGoblinActive = false;
        if (_goblinBehaviour == null) return;

        if (_currentAction != null)
            StopCoroutine(_currentAction);

        if (_goblinBehaviour != null)
            StopCoroutine(_goblinBehaviour);
    }

    private void SetAllStates()
    {
        _wanderingState = new WanderingState(this);

        _possibleStates.Add(new ThrowItemsFromFloorState(this));
        _possibleStates.Add(new ThrowItemsFromCrateState(this));
        _possibleStates.Add(new ScareCustomerState(this));
        _possibleStates.Add(new SlurpCauldronState(this));
    }

    internal void ChangeState()
    {
        // Exit previous state - stops the coroutine
        _currentState.ExitState();

        // Set state to null
        _currentState = null;

        // if its not a challenge day, we need to check if the goblin will wander for this move
        if (!_isChallengeDay)
        {
            var roll = Random.value;

            if (roll <= wanderingWeight)
            {
                _currentState = _wanderingState;
                _currentState.EnterState();
                return;
            }
        }
        
        // if the goblin isn't wandering pick another state
        PickState();
    }

    /// <summary>
    /// Picks a new state based on the total weight of all chances
    /// </summary>
    private void PickState()
    {
        _totalWeight = chanceOfCrates + chanceOfItems + chanceOfScaring + chanceOfSlurping;

        var roll = Random.value * _totalWeight;


        if (roll <= chanceOfItems)
        {
            // throw items off the floor
            _currentState = _possibleStates[0];
        }
        else if (roll <= chanceOfItems + chanceOfCrates)
        {
            // throw items from crate
            _currentState = _possibleStates[1];
        }
        else if (roll <= chanceOfItems + chanceOfCrates + chanceOfSlurping)
        {
            // slurp out of cauldron
            _currentState = _possibleStates[2];
        }
        else
        {
            // scare customers
            _currentState = _possibleStates[3];
        }

        _currentState.EnterState();
    }

    /// <summary>
    /// Checks to see if he's stepping in slime or not
    /// </summary>
    private void CheckForSlime()
    {
        // Check below the character to see if they've stepped in a slime trail
        if (Physics.Raycast(transform.position, Vector3.down, 1f, slime))
        {
            if (!_isInSlime)
            {
                _isInSlime = true;
                _agent.speed *= slowMultiplier;
            }
        }
        else
        {
            if (!_isInSlime) return;

            _isInSlime = false;
            _agent.speed = _defaultSpeed;
        }
    }

    /// <summary>
    /// Plays the idle sound.
    /// </summary>
    private void PlayIdleSound()
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
    }

    #region Getters for Crate State
    /// <summary>  Gets a Random Crate from the array  </summary>
    internal CrateHolder GetCrate()
    {
        return _crates[Random.Range(0, _crates.Length)];
    }

    /// <summary>  Returns the audio clip for rummaging  </summary>
    internal void PlayRummage()
    {
        AudioManager.instance.sfxManager.StartConstantSFX(rummageSound); //playing the sound for rummaging
    }

    /// <summary>  Returns the transform of where the goblins hands should be when rummaging  </summary>
    internal Transform Hands()
    {
        return goblinHands;
    }
    #endregion

    #region Getters for Floor State

    internal float AgentStoppingDistance()
    {
        return _agent.stoppingDistance;
    }

    internal float ThrowingDistance()
    {
        return _throwDistanceThreshold;
    }

    #endregion

    #region Getters For SlurpingCauldron

    internal CauldronInteraction PickACauldron()
    {
        return _cauldrons[Random.Range(0, _cauldrons.Length)];
    }

    internal void PlayGoblinSlurping()
    {
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.GoblinSounds, goblinSlurp.PickAudioClip(), true);
    }

    #endregion

    #region Scare Customer

    internal GameObject GetCustomerToScare()
    {
        return _queue.FindCustomer();
    }

    internal void ScareCustomer(GameObject customerToScare)
    {
        _queue.ScareCustomer(customerToScare);
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.GoblinSounds, goblinScream.PickAudioClip(), true);
    }

    #endregion

    /// <summary>
    /// Gets the float for the pause after an action 
    /// </summary>
    internal float GetPauseTime()
    {
        return pauseAfterAction;
    }
    
    // Sets the destination of the agent from behaviour classes
    internal void SetDestination(Vector3 destination)
    {
        _agent.SetDestination(destination);
        AudioManager.instance.sfxManager.StartConstantSFX(goblinMovement); //start movement sound
    }

    /// <summary> Check if the agent has reached the target </summary>
    internal bool ReachedDestination()
    {
        return !_agent.pathPending &&
               _agent.remainingDistance <= _agent.stoppingDistance &&
               (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f);
    }
}
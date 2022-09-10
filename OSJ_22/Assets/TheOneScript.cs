using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class TheOneScript : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ManyHeadSelection selectionMenu;

    void Awake()
    {
        gameManager.Awake();
        selectionMenu.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager.Start();
        selectionMenu.Start();
    }

    // Update is called once per frame
    void Update()
    {
        gameManager.Update();
        selectionMenu.Update();
    }
}

public abstract class Instance
{
    public virtual void Awake() { }
    public virtual void Start() { }
    public virtual void Update() { }
    public virtual void Initialise() { }
    public virtual void Disable() { }
}

[System.Serializable]
public class GameManager : Instance
{
    [System.Serializable]
    public class RoomManager
    {
        [System.Serializable]
        public class Room
        {
            [SerializeField] private Camera camera;
            [SerializeField] private Transform idlePoint;

            public Transform IdlePoint { get { return idlePoint; } }

            public void Activate()
            {
                camera.gameObject.SetActive(true);
            }

            public void Deactivate()
            {
                camera.gameObject.SetActive(false);
            }
        }

        [SerializeField] private Room[] rooms;

        public int CurrentRoomIndex { get; private set; } = -1;
        public Room CurrentRoom { get { return rooms[CurrentRoomIndex]; } }

        public void Awake()
        {
            ResetEvent += delegate { ActivateRoom(0); };
        }

        public bool ActivateRoom(int index)
        {
            if (index != CurrentRoomIndex && index < rooms.Length)
            {
                for (int i = 0; i < rooms.Length; i++)
                {
                    if (i == index)
                    {
                        rooms[i].Activate();
                    }
                    else
                    {
                        rooms[i].Deactivate();
                    }
                }
                CurrentRoomIndex = index;
                return true;
            }
            return false;
        }
    }

    [System.Serializable]
    public class ManyHead
    {
        public delegate void DeadDelegate();

        [SerializeField] private float feedRatio = 5;
        [SerializeField] private float hungerDepleteRatio = 1;
        [SerializeField] private float engagementRatio = 5;
        [SerializeField] private float engagementDepleteRatio = 1;

        private float currentHunger = 1;
        private float currentEngagement;

        public bool Dead { get; private set; }
        public int Gender { get; private set; } = -1;
        public GameObject Head { get; private set; }

        public event DeadDelegate DeadEvent;

        public ManyHead(ManyHead properties, GameObject selectedHead, int selectedGender)
        {
            hungerDepleteRatio = properties.hungerDepleteRatio;
            engagementDepleteRatio = properties.engagementDepleteRatio;
            Head = selectedHead;
            Gender = selectedGender;
            ResetEvent += delegate { Head.transform.SetParent(null); Dead = false; };
            DeadEvent = delegate { Dead = true; };
        }

        public void Update()
        {
            if (currentHunger > 0)
            {
                currentHunger -= (1 / hungerDepleteRatio) * Time.deltaTime;
                if (currentHunger <= 0)
                {
                    currentHunger = 0;
                    DeadEvent.Invoke();
                }

                if (currentEngagement > 0)
                {
                    currentEngagement -= (1 / engagementDepleteRatio) * Time.deltaTime;
                    if (currentEngagement <= 0)
                    {
                        currentEngagement = 0;
                    }
                }
            }
        }

        public void Feed(bool decrease = false)
        {
            if (decrease == false)
            {
                currentHunger += 1 / feedRatio;
            }
            else
            {
                currentHunger -= 1 / feedRatio;
            }
        }

        public void Engage(bool decrease = false)
        {
            if (decrease == false)
            {
                currentEngagement += 1 / engagementRatio;
            }
            else
            {
                currentEngagement -= 1 / engagementRatio;
            }
        }
    }

    public delegate void ResetDelegate();

    [SerializeField] private ManyHead manyHead;
    [SerializeField] private AIAgent agent;
    [SerializeField] private Material maleMaterial;
    [SerializeField] private Material femaleMaterial;
    [SerializeField] private MeshRenderer bodyMesh;
    [SerializeField] private Transform headObject;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private Camera foodCamera;
    [SerializeField] private Transform feedPoint;
    [SerializeField] private float feedCooldown;

    private Vector3 currentDir = Vector3.right;
    private float timer = -1;

    public static event ResetDelegate ResetEvent;

    public static GameManager Instance { get; private set; }

    public override void Awake()
    {
        Instance = this;
        ResetEvent = delegate 
        {
            bodyMesh.gameObject.SetActive(false);
            foodCamera.gameObject.SetActive(false);
        };
        bodyMesh.gameObject.SetActive(false);
        agent.Awake();
        roomManager.Awake();
    }

    public override void Start()
    {
        roomManager.ActivateRoom(0);
        foodCamera.gameObject.SetActive(false);
    }

    public override void Update()
    {
        manyHead.Update();
        if (manyHead.Dead == true)
        {
            if (Input.GetKeyDown(KeyCode.R) == true)
            {
                ResetEvent.Invoke();
            }
        }
        else if (bodyMesh.gameObject.activeSelf == true)
        {
            agent.Update();
            if (agent.Busy == false)
            {
                if (foodCamera.gameObject.activeSelf == true)
                {
                    Vector3 relativeDir = foodCamera.transform.InverseTransformDirection(currentDir);
                    if(Vector3.Angle(foodCamera.transform.forward, relativeDir) > 45f)
                    {
                        Vector3 step = Vector3.RotateTowards(foodCamera.transform.forward, relativeDir, 0.5f * Time.deltaTime, 0.0f);
                        foodCamera.transform.forward = step;
                    }
                    else
                    {
                        currentDir = -currentDir;
                    }

                    if(Input.GetButtonDown("Fire1") == true)
                    {
                        ShootFood();
                    }
                    else if(Input.GetButtonDown("Cancel") == true)
                    {
                        foodCamera.gameObject.SetActive(false);
                        agent.SetState(new AIAgent.MoveState(agent, roomManager.CurrentRoom.IdlePoint.position));
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1) == true)
                    {
                        if (roomManager.ActivateRoom(0) == true)
                        {
                            agent.SetState(new AIAgent.MoveState(agent, roomManager.CurrentRoom.IdlePoint.position));
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha2) == true)
                    {
                        if (roomManager.ActivateRoom(1) == true)
                        {
                            agent.SetState(new AIAgent.MoveState(agent, roomManager.CurrentRoom.IdlePoint.position));
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha3) == true)
                    {
                        if (roomManager.ActivateRoom(2) == true)
                        {
                            agent.SetState(new AIAgent.MoveState(agent, roomManager.CurrentRoom.IdlePoint.position));
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha4) == true)
                    {
                        if (roomManager.ActivateRoom(3) == true)
                        {
                            agent.SetState(new AIAgent.MoveState(agent, roomManager.CurrentRoom.IdlePoint.position));
                        }
                    }
                    else if(timer == -1 && Input.GetButtonDown("Fire1") == true)
                    {
                        foodCamera.gameObject.SetActive(true);
                        agent.SetState(new AIAgent.MoveState(agent, feedPoint.position));
                    }
                }
            }
        }

        if(timer > -1)
        {
            timer += Time.deltaTime;
            if(timer >= feedCooldown)
            {
                timer = -1;
            }
        }
    }

    public void SpawnManyHead(Transform head, int gender)
    {
        manyHead = new ManyHead(manyHead, head.gameObject, gender);
        manyHead.DeadEvent += delegate { agent.SetState(new AIAgent.DeadState(agent)); };
        if (gender == 1)
        {
            bodyMesh.material = maleMaterial;
        }
        else if(gender == 0)
        {
            bodyMesh.material = femaleMaterial;
        }
        bodyMesh.gameObject.SetActive(true);
        head.SetParent(headObject);
        head.localPosition = Vector3.zero;
        agent.MoveToPosition(roomManager.CurrentRoom.IdlePoint.position);
        agent.SetState(new AIAgent.IdleState(agent));
    }

    public void ShootFood()
    {
        timer = 0;
        foodCamera.gameObject.SetActive(false);
        agent.SetState(new AIAgent.MoveState(agent, roomManager.CurrentRoom.IdlePoint.position));
    }
}

[System.Serializable]
public class ManyHeadSelection : Instance
{
    [System.Serializable]
    private class UIElementGenderEgg
    {
        [SerializeField] private RawImage uiImage;

        private Outline imageOutline;

        public void Initialise()
        {
            uiImage.TryGetComponent(out imageOutline);
        }

        public void Select(bool toggle)
        {
            imageOutline.enabled = toggle;
        }

        public void Enable()
        {
            uiImage.gameObject.SetActive(true);
        }

        public void Disable()
        {
            uiImage.gameObject.SetActive(false);
        }
    }

    [Header("Gender Selection")]
    [SerializeField] private float eggRotateSpeed = 1f;
    [SerializeField] private Transform eggATransform, eggBTransform;
    [SerializeField] private Transform eggACam, eggBCam;
    [SerializeField] private UIElementGenderEgg eggAElement, eggBElement;
    [Header("Head Selection")]
    [SerializeField] private RawImage headUIImage;
    [SerializeField] private GameObject[] heads;
    [SerializeField] private Camera headCamera;
    [SerializeField] private Vector3[] headGlobalPositions;
    [SerializeField] private Vector3 cameraOffset;
    [SerializeField] private float cameraMotionTime = 1f;

    private int selectedGender = -1;
    private int selectedHead = -1;
    private float lerpTimer = -1;

    public override void Awake()
    {
        eggAElement.Initialise();
        eggBElement.Initialise();
    }

    public override void Initialise()
    {
        selectedGender = 0; //reset gender
        selectedHead = -1; //reset head
        //turn on egg elements
        eggAElement.Enable();
        eggBElement.Enable();
        //select first element
        eggAElement.Select(true);
        eggBElement.Select(false);
        //turn on egg cameras
        eggACam.gameObject.SetActive(true);
        eggBCam.gameObject.SetActive(true);
        //turn off head ui element and camera
        headUIImage.gameObject.SetActive(false);
        headCamera.gameObject.SetActive(false);
        //set head scene positions
        for (int i = 0; i < heads.Length; i++)
        {
            if (i < headGlobalPositions.Length)
            {
                heads[i].transform.position = headGlobalPositions[i];
            }
        }
    }

    public override void Start()
    {
        GameManager.ResetEvent += Initialise;
        Initialise();
    }

    public override void Update()
    {
        eggATransform.transform.Rotate(0, eggRotateSpeed, 0);
        eggBTransform.transform.Rotate(0, eggRotateSpeed, 0);

        float axis = Input.GetAxis("Horizontal");
        if (selectedHead == -1)
        {
            if (axis > 0 && selectedGender != 1)
            {
                selectedGender = 1;
                eggAElement.Select(false);
                eggBElement.Select(true);

            }
            else if (axis < 0 && selectedGender != 0)
            {
                selectedGender = 0;
                eggAElement.Select(true);
                eggBElement.Select(false);
            }

            if (Input.GetButtonDown("Submit") == true)
            {
                //disable egg UI elements
                eggAElement.Disable();
                eggBElement.Disable();
                //disable egg cameras
                eggACam.gameObject.SetActive(false);
                eggBCam.gameObject.SetActive(false);
                //toggle on the head selection
                headUIImage.gameObject.SetActive(true);
                headCamera.gameObject.SetActive(true);
                selectedHead = 0;
                lerpTimer = 0;
            }
        }
        else if (headUIImage.gameObject.activeSelf == true)
        {
            if (lerpTimer > -1)
            {
                lerpTimer += Time.deltaTime;
                headCamera.transform.position = Vector3.Lerp(headCamera.transform.position, heads[selectedHead].transform.position + cameraOffset, lerpTimer / cameraMotionTime);
                if (lerpTimer >= cameraMotionTime)
                {
                    lerpTimer = -1;
                }
            }
            else
            {
                if (axis > 0 && selectedHead < heads.Length - 1)
                {
                    selectedHead++;
                    lerpTimer = 0;
                }
                else if (axis < 0 && selectedHead > 0)
                {
                    selectedHead--;
                    lerpTimer = 0;
                }
            }

            if (Input.GetButtonDown("Submit") == true)
            {
                headUIImage.gameObject.SetActive(false);
                headCamera.gameObject.SetActive(false);
                lerpTimer = -1;
                GameManager.Instance.SpawnManyHead(heads[selectedHead].transform, selectedGender);
            }
        }
    }
}

[System.Serializable]
public class AIAgent : Instance
{
    [SerializeField] private NavMeshAgent agent;

    private FiniteStateMachine stateMachine;

    public bool Busy { get; private set; } = false;

    public abstract class AIState : FiniteStateMachine.IState
    {
        public AIAgent Instance { get; private set; }

        public AIState(AIAgent instance)
        {
            Instance = instance;
        }

        public virtual void OnStateEnter()
        {
        }

        public virtual void OnStateExit()
        {
        }

        public virtual void OnStateUpdate()
        {
        }
    }

    public class IdleState : AIState
    {
        public IdleState(AIAgent instance) : base(instance)
        {
        }

        public override void OnStateEnter()
        {
            Instance.Busy = false;
            Instance.agent.isStopped = true;
            Debug.Log("Set state to idle");
        }

        public override void OnStateUpdate()
        {
            Debug.Log("Idling");
        }

        public override void OnStateExit()
        {
            Instance.agent.isStopped = false;
        }
    }

    public class DeadState : AIState
    {
        public DeadState(AIAgent instance) : base(instance)
        {
        }

        public override void OnStateEnter()
        {
            Instance.Busy = false;
            Instance.agent.isStopped = true;
            Debug.Log("Dead");
        }
    }

    public class MoveState : AIState
    {
        Vector3 targetPosition;

        public MoveState(AIAgent instance, Vector3 position) : base(instance)
        {
            targetPosition = position;
        }

        public override void OnStateEnter()
        {
            if(NavMesh.SamplePosition(targetPosition, out NavMeshHit point, Instance.agent.height * 2, NavMesh.AllAreas) == true)
            {
                targetPosition = point.position;
                Instance.agent.SetDestination(targetPosition);
                Instance.Busy = true;
            }
            else
            {
                Instance.stateMachine.SetState(new IdleState(Instance));
            }
        }

        public override void OnStateUpdate()
        {
            Debug.Log("Moving to position");
            if(Instance.agent.remainingDistance <= Instance.agent.stoppingDistance)
            {
                Instance.stateMachine.SetState(new IdleState(Instance));
            }
        }
    }

    public AIAgent(NavMeshAgent agentInstance)
    {
        agent = agentInstance;
    }

    public override void Awake()
    {
        stateMachine = new FiniteStateMachine();
    }

    public override void Update()
    {
        stateMachine.Update();
    }

    public void SetState(AIState state)
    {
        stateMachine.SetState(state);
    }

    public void MoveToPosition(Vector3 pos)
    {
        agent.transform.position = pos;
    }
}

public class FiniteStateMachine
{
    public interface IState
    {
        void OnStateEnter();
        void OnStateUpdate();
        void OnStateExit();
    }

    public IState CurrentState { get; private set; }

    public void SetState(IState state)
    {
        if(CurrentState != null)
        {
            CurrentState.OnStateExit();
        }
        CurrentState = state;
        CurrentState.OnStateEnter();
    }

    public void Update()
    {
        if(CurrentState != null)
        {
            CurrentState.OnStateUpdate();
        }
    }
}
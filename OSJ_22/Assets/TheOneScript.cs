using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class TheOneScript : MonoBehaviour
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
            public ManyHeadTest RoomTest { get; private set; }

            public Room(Room room, ManyHeadTest test)
            {
                camera = room.camera;
                idlePoint = room.idlePoint;
                RoomTest = test;
            }

            public void Activate()
            {
                camera.gameObject.SetActive(true);
            }

            public void Deactivate()
            {
                camera.gameObject.SetActive(false);
            }
        }

        [System.Serializable]
        public abstract class ManyHeadTest
        {
            public delegate void ModifyIntelligence(float amount);

            [SerializeField] protected float cooldownTime = 10f;

            protected float cooldownTimer = -1;

            public bool Active { get; private set; } = false;

            public ManyHeadTest(ManyHeadTest test)
            {
                cooldownTime = test.cooldownTime;
            }

            public virtual void Initialise(ModifyIntelligence modifyMethod) { Active = true; }

            public virtual void Update() 
            {
                if(cooldownTimer >= 0)
                {
                    cooldownTimer += Time.deltaTime;
                    if(cooldownTimer >= cooldownTime)
                    {
                        cooldownTimer = -1;
                    }
                }
            }

            public virtual void Exit() { Active = false; cooldownTimer = 0; }
        }

        [System.Serializable]
        public class LightTest : ManyHeadTest
        {
            [SerializeField] private Light greenLight, redLight;
            [Range(1, 5)] [SerializeField] private float minTime = 1, maxTime = 5;
            [Range(10, 100)] [SerializeField] private float greenChance = 50;

            private float currentTime = 0;
            private float timer = -1;

            public LightTest(LightTest test) : base(test)
            {
                greenLight = test.greenLight;
                redLight = test.redLight;
                minTime = test.minTime;
                maxTime = test.maxTime;
                greenChance = test.greenChance;
            }

            public event ModifyIntelligence ModifyIntelligenceEvent;

            public void Awake()
            {
                greenLight.enabled = false;
                redLight.enabled = false;
            }

            public override void Initialise(ModifyIntelligence modifyMethod)
            {
                if (cooldownTimer < 0)
                {
                    base.Initialise(modifyMethod);
                    greenLight.enabled = false;
                    redLight.enabled = true;
                    timer = 0;
                    currentTime = Random.Range(minTime, maxTime);
                    ModifyIntelligenceEvent = new ModifyIntelligence(modifyMethod);
                }
            }

            public override void Update()
            {
                if (Active == true)
                {
                    timer += Time.deltaTime;
                    if (timer >= currentTime)
                    {
                        if (greenLight.enabled == true)
                        {
                            Debug.Log("Missed target");
                            ModifyIntelligenceEvent.Invoke(-20);
                        }
                        currentTime = Random.Range(minTime, maxTime);
                        timer = 0;
                        ToggleLight();
                    }
                    if (Input.GetButtonDown("Fire1") == true)
                    {
                        if (greenLight.enabled == true)
                        {
                            greenLight.enabled = false;
                            Debug.Log("Successful interaction");
                            ModifyIntelligenceEvent.Invoke(20);
                        }
                        else if(redLight.enabled == true)
                        {
                            redLight.enabled = false;
                            Debug.Log("Unsuccessful interaction");
                            ModifyIntelligenceEvent.Invoke(-20);
                        }
                    }
                }
                else
                {
                    base.Update();
                }
            }

            private void ToggleLight()
            {
                int r = Random.Range(0, 100);
                if (r < greenChance)
                {
                    Debug.Log("Green light");
                    greenLight.enabled = true;
                    redLight.enabled = false;
                }
                else
                {
                    Debug.Log("Red light");
                    greenLight.enabled = false;
                    redLight.enabled = true;
                }
            }

            public override void Exit()
            {
                base.Exit();
                ModifyIntelligenceEvent = null;
                greenLight.enabled = false;
                redLight.enabled = false;
            }
        }

        [System.Serializable]
        public class ShelfTest : ManyHeadTest
        {
            
            [SerializeField] Transform[] objPlaceLocations; //Where the player places the objects
            [SerializeField] GameObject[] moveableObjects; //Where the objects orginally spawn.
            [SerializeField] Transform[] objOriginalPos; //Where the objects orginally spawn.

            GameObject currentSelectedObj;
            GameObject currentTarget;
            [SerializeField] Image[] tvScreenLocations;
            [SerializeField] Sprite[] objSprites;
            [SerializeField] int[] correctOrder; //Set on start
            [SerializeField] int[] currentOrder; //Change with player

            public float viewTime = 2f;
            public float guessTime = 5f;
            public float coolDownTime = 2f;
            [SerializeField] Camera roomCamera; //Change with player


            enum ActivityState {show,guess,correct};
            [SerializeField] ActivityState state = ActivityState.show;
            [SerializeField] private float currentTime = 0;
            [SerializeField] private float timer = -1;
            
            public ShelfTest(ShelfTest test) : base(test)
            {
                objPlaceLocations = test.objPlaceLocations;
                moveableObjects = test.moveableObjects; 
                tvScreenLocations = test.tvScreenLocations;
                objSprites = test.objSprites;
                state = test.state;
                roomCamera = test.roomCamera;
            }
            

            public event ModifyIntelligence ModifyIntelligenceEvent;

            public void Awake()
            {
                
            }

            public override void Initialise(ModifyIntelligence modifyMethod)
            {
                base.Initialise(modifyMethod);
                timer = 0;
                currentTime = viewTime;
                ModifyIntelligenceEvent = new ModifyIntelligence(modifyMethod);
                ChangeActivityState(ActivityState.show);
                ShowOrder();
            }

            public override void Update()
            {
                if (Active == true)
                {
                    timer += Time.deltaTime;
                    if (timer >= currentTime)
                    {
                        timer = 0;
                        if(state == ActivityState.show)
                        {
                            ChangeActivityState(ActivityState.guess);
                        }
                        else if (state == ActivityState.guess)
                        {
                            //Timer ran out and failed
                            ShowOrder();
                            ModifyIntelligenceEvent.Invoke(-20);
                            ChangeActivityState(ActivityState.correct);
                        }
                        else if (state == ActivityState.correct)
                        {
                            ChangeActivityState(ActivityState.show);
                        }
                    }

                    //Raycast
                    
                    if(currentSelectedObj != null)
                    {
                        if (Input.GetButtonUp("Fire1"))
                        {
                            if(currentTarget != null)
                            {

                                if (CheckCurrentObject())
                                {
                                    //Correct
                                    currentSelectedObj.transform.position = currentTarget.transform.position;
                                    currentSelectedObj.GetComponent<Collider>().enabled = false;
                                    currentTarget.GetComponent<Collider>().enabled = false;
                                    currentSelectedObj = null;
                                    ModifyIntelligenceEvent.Invoke(20);
                                }
                                else
                                {
                                    //Incorrect
                                    ModifyIntelligenceEvent.Invoke(-20);
                                    //reset Pos
                                    currentSelectedObj.transform.localPosition = Vector3.zero;
                                    currentSelectedObj = null;
                                }
                            }
                            else
                            {
                                //Deselect
                                currentSelectedObj.transform.localPosition = Vector3.zero;
                                currentSelectedObj = null;
                            }
                            
                        }
                        //Held
                        if (Input.GetButton("Fire1"))
                        {
                            currentSelectedObj.transform.position = roomCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -roomCamera.transform.position.z));

                            //raycast for target
                            RaycastHit hit;
                            Ray ray = roomCamera.ScreenPointToRay(Input.mousePosition);
                            if (Physics.Raycast(ray, out hit, 20f, LayerMask.GetMask("Target")))
                            {
                                //Set target
                                currentTarget = hit.transform.gameObject;
                            }
                            else
                            {
                                //Remove target
                                currentTarget = null;
                            }
                        }

                    }
                    else
                    {
                        RaycastHit hit;
                        Ray ray = roomCamera.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out hit, 20f, LayerMask.GetMask("Selectable")))
                        {
                            Transform objectHit = hit.transform;
                            if (Input.GetButtonDown("Fire1"))
                            {
                                //Select
                                currentSelectedObj = objectHit.gameObject;

                            }
                            // Do something with the object that was hit by the raycast.
                        }
                    }

                    


                }
                else
                {
                    base.Update();
                }
            }



            public void ShowOrder()
            {
                Random.InitState((int)System.DateTime.Now.Ticks);
                correctOrder = new int[objPlaceLocations.Length];
                currentOrder = new int[objPlaceLocations.Length];
                
                List<int> placementIndexs = new List<int>(){};
                
                    //create list 1-length
                for (int i = 0; i < correctOrder.Length; i++)
                {
                    placementIndexs.Add(i);
                    currentOrder[i] = -1;
                    
                }
                //Random Order for in correctOrder
                for (int i = 0; i < correctOrder.Length; i++)
                {
                    
                    int randomInd = Random.Range(0, placementIndexs.Count);
                    int correctInd = placementIndexs[randomInd];
                    placementIndexs.RemoveAt(randomInd);
                    tvScreenLocations[i].sprite = objSprites[correctInd];
                    correctOrder[i] = correctInd;
                }
                //randomize 
            }

            public void HideOrder()
            {
                foreach (Image item in tvScreenLocations)
                {
                    item.sprite = null;
                }
            }

            public bool CheckCurrentObject()
            {

                //Get ind of current object
                //get ind of target obj
                //correctOrder[targetind] == currentObjInd
                int targetInd = -1;
                int selectedObjInd = -1;
                for (int i = 0; i < objPlaceLocations.Length; i++)
                {
                    if (objPlaceLocations[i] == currentTarget.transform)
                        targetInd = i;
                }
                for (int i = 0; i < moveableObjects.Length; i++)
                {
                    if (moveableObjects[i] == currentSelectedObj)
                        selectedObjInd = i;
                }
                if (correctOrder[targetInd] == selectedObjInd)
                {
                    currentOrder[targetInd] = selectedObjInd;
                    return true;
                }
                    
                return false;
            }

            public void ResetActivity()
            {
                //Reset
                currentSelectedObj = null;
                foreach (GameObject item in moveableObjects)
                {
                    item.transform.localPosition = Vector3.zero;
                    item.GetComponent<Collider>().enabled = true;
                }
                foreach (Transform item in objPlaceLocations)
                {
                    item.GetComponent<Collider>().enabled = true;
                }
                HideOrder();

            }

            void ChangeActivityState(ActivityState newState)
            {
                state = newState;
                if (newState == ActivityState.show)
                {
                    currentTime = viewTime;
                    ShowOrder();
                    foreach (GameObject item in moveableObjects)
                    {
                        item.GetComponent<Collider>().enabled = false;
                    }
                }

                if (newState == ActivityState.guess)
                {
                    currentTime = guessTime;
                    foreach (GameObject item in moveableObjects)
                    {
                        item.GetComponent<Collider>().enabled = true;
                    }
                    HideOrder();
                }

                if (newState == ActivityState.correct)
                {
                    currentTime = coolDownTime;
                    foreach (GameObject item in moveableObjects)
                    {
                        item.GetComponent<Collider>().enabled = false;
                    }
                    ResetActivity();
                    HideOrder();
                }
            }

            public override void Exit()
            {
                base.Exit();
                ModifyIntelligenceEvent = null;
                ResetActivity();

                
            }
        }

        [SerializeField] private Room[] rooms;
        [SerializeField] private LightTest lightTest;
        [SerializeField] private ShelfTest shelfTest;

        public int CurrentRoomIndex { get; private set; } = -1;
        public Room CurrentRoom { get { return rooms[CurrentRoomIndex]; } }

        public void Awake()
        {
            ResetEvent += delegate { ActivateRoom(0); };
            lightTest = new LightTest(lightTest);
            lightTest.Awake();
            shelfTest = new ShelfTest(shelfTest);
            shelfTest.Awake();
            for (int i = 0; i < rooms.Length; i++)
            {
                switch (i)
                {
                    case 1:
                        rooms[i] = new Room(rooms[i], lightTest);
                        break;
                    case 2:
                        rooms[i] = new Room(rooms[i], shelfTest);
                        break;
                    case 3:
                        //rooms[i] = new Room(rooms[i], lightTest);
                        break;
                }
            }
        }

        public void Update()
        {
            foreach(Room room in rooms)
            {
                room.RoomTest?.Update();
            }
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

        public void SubscribeToManyHeadDeathEvent(ManyHead manyHead)
        {
            foreach (Room room in rooms)
            {
                manyHead.DeadEvent += delegate 
                { 
                    if (room.RoomTest?.Active == true) 
                    {
                        room.RoomTest.Exit();
                    }
                };
            }
        }
    }

    [System.Serializable]
    public class ManyHead
    {
        public delegate void DeadDelegate();

        [SerializeField] private int ageLimit = 15;
        [SerializeField] private int yearCycle = 60;
        [SerializeField] private Image hungerImage;
        [Range(2, 10)] [SerializeField] private float feedRatio = 5;
        [SerializeField] private float hungerDepleteRatio = 1;
        [Range(2, 10)] [SerializeField] private float engagementRatio = 5;
        [SerializeField] private float engagementDepleteRatio = 1;
        [SerializeField] private Image engagementImage;

        private float currentHunger = 1;
        private float currentEngagement = 1;
        private float currentIntelligence = 0;
        private int age = 0;
        private float ageTimer = 0;

        public bool Dead { get; private set; }
        public int Gender { get; private set; } = -1;
        public GameObject Head { get; private set; }

        public event DeadDelegate DeadEvent;

        public ManyHead(ManyHead properties, GameObject selectedHead, int selectedGender)
        {
            hungerDepleteRatio = properties.hungerDepleteRatio;
            engagementDepleteRatio = properties.engagementDepleteRatio;
            feedRatio = properties.feedRatio;
            engagementRatio = properties.engagementRatio;
            hungerImage = properties.hungerImage;
            engagementImage = properties.engagementImage;
            ageLimit = properties.ageLimit;
            yearCycle = properties.yearCycle;
            Head = selectedHead;
            Gender = selectedGender;
            ResetEvent += delegate { Head.transform.SetParent(null); Dead = false; };
            DeadEvent = delegate { Dead = true; };
        }

        public void Update()
        {
            if (Dead == false && currentHunger > 0)
            {
                if(age < ageLimit)
                {
                    ageTimer += Time.deltaTime;
                    if(ageTimer >= yearCycle)
                    {
                        age++;
                        ageTimer = 0;
                    }
                }
                else
                {
                    Die();
                    return;
                }
                currentHunger -= (1 / hungerDepleteRatio) * Time.deltaTime;
                if (currentHunger <= 0)
                {
                    Die();
                    return;
                }
                hungerImage.fillAmount = currentHunger / 1;
                
                if (currentEngagement > 0)
                {
                    currentEngagement -= (1 / engagementDepleteRatio) * Time.deltaTime;
                    if (currentEngagement <= 0)
                    {
                        currentEngagement = 0;
                    }
                    engagementImage.fillAmount = currentEngagement / 1;
                }
            }
        }

        public void Feed(bool decrease = false)
        {
            if (decrease == false)
            {
                currentHunger += 1 / feedRatio;
                if(currentHunger > 1)
                {
                    currentHunger = 1;
                }
            }
            else
            {
                currentHunger -= 1 / feedRatio;
                if (currentHunger <= 0)
                {
                    Die();
                    return;
                }
            }
            hungerImage.fillAmount = currentHunger / 1;
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
            engagementImage.fillAmount = currentEngagement / 1;
        }

        private void Die()
        {
            currentHunger = 0;
            hungerImage.fillAmount = currentHunger / 1;
            DeadEvent.Invoke();
        }

        public void ModifyIntelligence(float amount)
        {
            currentIntelligence += amount;
            if(currentIntelligence < 0)
            {
                currentIntelligence = 0;
            }
        }
    }

    public delegate void ResetDelegate();

    [SerializeField] private Camera foodCamera;
    [SerializeField] private Transform feedPoint;
    [SerializeField] private float feedCooldown;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private ManyHeadSelection selectionMenu;
    [Header("Many Head/Agent")]
    [SerializeField] private ManyHead manyHead;
    [SerializeField] private AIAgent agent;
    [SerializeField] private Material maleMaterial;
    [SerializeField] private Material femaleMaterial;
    [SerializeField] private SkinnedMeshRenderer bodyMesh;
    [SerializeField] private Transform headObject;


    private Vector3 currentDir = Vector3.right;
    private float timer = -1;

    public static event ResetDelegate ResetEvent;

    public static TheOneScript Instance { get; private set; }



    public void Awake()
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
        selectionMenu.Awake();
    }

    public void Start()
    {
        selectionMenu.Start();
        roomManager.ActivateRoom(0);
        foodCamera.gameObject.SetActive(false);
    }

    public void Update()
    {
        manyHead.Update();
        selectionMenu.Update();
        roomManager.Update();
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
            if (foodCamera.gameObject.activeSelf == true)
            {
                Vector3 relativeDir = foodCamera.transform.InverseTransformDirection(currentDir);
                if (Vector3.Angle(foodCamera.transform.forward, relativeDir) > 45f)
                {
                    Vector3 step = Vector3.RotateTowards(foodCamera.transform.forward, relativeDir, 0.5f * Time.deltaTime, 0.0f);
                    foodCamera.transform.forward = step;
                }
                else
                {
                    currentDir = -currentDir;
                }

                if (Input.GetButtonDown("Fire1") == true)
                {
                    ShootFood();
                }
                else if (Input.GetButtonDown("Cancel") == true)
                {
                    foodCamera.gameObject.SetActive(false);
                    agent.SetState(new AIAgent.MoveState(agent, roomManager.CurrentRoom.IdlePoint.position));
                }
            }
            if (agent.Busy == false)
            {
                if(roomManager.CurrentRoom.RoomTest?.Active == true)
                {
                    if(Input.GetButtonDown("Cancel") == true)
                    {
                        roomManager.CurrentRoom.RoomTest.Exit();
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
                    else if(Input.GetButtonDown("Submit") == true)
                    {
                        //start the puzzle for the room
                        roomManager.CurrentRoom.RoomTest?.Initialise(manyHead.ModifyIntelligence);
                    }
                    else if (timer == -1 && Input.GetButtonDown("Fire1") == true)
                    {
                        foodCamera.gameObject.SetActive(true);
                        agent.SetState(new AIAgent.MoveState(agent, feedPoint.position));
                    }
                }
            }
        }

        if (timer > -1)
        {
            timer += Time.deltaTime;
            if (timer >= feedCooldown)
            {
                timer = -1;
            }
        }
    }

    public void SpawnManyHead(Transform head, int gender)
    {
        manyHead = new ManyHead(manyHead, head.gameObject, gender);
        manyHead.DeadEvent += delegate { agent.SetState(new AIAgent.DeadState(agent)); };
        roomManager.SubscribeToManyHeadDeathEvent(manyHead);
        if (gender == 1)
        {
            bodyMesh.material = maleMaterial;
        }
        else if (gender == 0)
        {
            bodyMesh.material = femaleMaterial;
        }
        bodyMesh.gameObject.SetActive(true);
        head.SetParent(headObject);
        head.localPosition = Vector3.zero;
        head.localEulerAngles = Vector3.zero;
        agent.MoveToPosition(roomManager.CurrentRoom.IdlePoint.position);
        agent.SetState(new AIAgent.IdleState(agent));
    }

    public void ShootFood()
    {
        timer = 0;
        foodCamera.gameObject.SetActive(false);
        agent.SetState(new AIAgent.MoveState(agent, roomManager.CurrentRoom.IdlePoint.position));
        if(Physics.Raycast(foodCamera.transform.position, foodCamera.transform.forward, out RaycastHit hit, 10f) == true)
        {
            if (hit.collider.CompareTag("Player") == true) { manyHead.Feed(); } else { manyHead.Feed(true); }
        }
        else
        {
            manyHead.Feed(true);
        }
    }
}

[System.Serializable]
public class ManyHeadSelection
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

    public void Awake()
    {
        eggAElement.Initialise();
        eggBElement.Initialise();
    }

    public void Initialise()
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
                heads[i].transform.eulerAngles = Vector3.zero;
            }
        }
    }

    public void Start()
    {
        TheOneScript.ResetEvent += Initialise;
        Initialise();
    }

    public void Update()
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
                TheOneScript.Instance.SpawnManyHead(heads[selectedHead].transform, selectedGender);
            }
        }
    }
}

[System.Serializable]
public class AIAgent
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
            //Debug.Log("Set state to idle");
        }

        public override void OnStateUpdate()
        {
            //Debug.Log("Idling");
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
            //Debug.Log("Moving to position");
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

    public void Awake()
    {
        stateMachine = new FiniteStateMachine();
    }

    public void Update()
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
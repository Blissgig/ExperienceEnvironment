using System.Collections.Generic;
using UnityEngine;

public class ExperienceEnvironment : MonoBehaviour {
    
    public Terrain currentTerrain;
    public int WaterLevel = 0; //This is used to determine what objects are not place in the water.  See function: GameObjectPosition

    [System.Serializable]
    public class ObjectDetails
    {
        public Vector3 MinimumPosition = new Vector3(); //Range of where the object is placed
        public Vector3 MaximumPosition = new Vector3();
        public Vector3 MinimumRotation = new Vector3(); //Range of rotation allowable
        public Vector3 MaximumRotation = new Vector3();
        public Vector3 MinimumScale = new Vector3();    //Range of object size
        public Vector3 MaximumScale = new Vector3();
        public bool useTerrainSize = true;          //Be aware, using this could allow objects to float above scene 
        public bool isScaled = true;                //Only resized from prefab 's original size if this is false.
        public bool isEvenlyScaled = true;          //Scales all values on the X
        public bool avoidObjects = true;
        public byte PercentInWater = 0;             //0 to 100.  This is to allow, or not, objects to be a percentage within water.  See function IsInWater
        public int toTheSky = 0;                    //This is used when detecting if there is anything above an object when getting its position.  ie: To avoid placing an object inside a mountain.  See function; GameObjectPosition()
        public int MinimumQuantity = 0;             //The Min/Max allow a range of objects to be populated.
        public int MaximumQuantity = 1;
        public float yVariance = 0;                 //This is used to allow objects to be a bit higher/lower than the default.   Often used to put something part way within the terrain (rocks, trees)
    }

    [System.Serializable]
    public class ObjectDetailsWithFolder: ObjectDetails
    {
        public string FolderName = "";
        public bool useSingleObjectMultipleTimes = false;  //This allows for a single random item to be use one or more times.
    }

    [System.Serializable]
    public class ObjectDetailsWithGameObject : ObjectDetails
    {
        public GameObject gameObject; 
    }

    public List<ObjectDetailsWithFolder> DetailsWithFolders = new List<ObjectDetailsWithFolder>();
    public List<ObjectDetailsWithGameObject> DetailsWithObject = new List<ObjectDetailsWithGameObject>();

    public class VectorObject
    {
        public Vector3 Vector;
        public bool IsFound = true;
    }
    

    private void Start ()
    {
        PopulateExperience();
    }
	
    public void PopulateExperience()
    {
        try
        {
            GameObject gameObject = new GameObject();
            int iObjectSelected = 0;
            int iObjectCount;


            //Check if currentTerrain is null, if so check if there is an available terrain
            if (this.currentTerrain == null)
            {
                var activeTerrain = Terrain.activeTerrain;

                if (activeTerrain != null)
                {
                    this.currentTerrain = activeTerrain;
                }
            }

            //-- ALL THE OBJECTS FROM WITHIN A "RESOURCES" SUBFOLDER
            #region Collection With Folders
            foreach (ObjectDetailsWithFolder DWF in DetailsWithFolders)
            {
                //In case a folder was not added
                if (DWF.FolderName.Trim().Length > 0)
                {
                    //Get a list of the objects in that Resource Folder
                    List<string> objects = LoadPrefabs(DWF.FolderName.Trim());
                    
                    //In case there are no objects in that folder, or the "FolderName" is incorrect, or....
                    if (objects.Count > 0)
                    {
                        //Insure that Max is not smaller than Min (hey, all us devs make mistakes)
                        if (DWF.MaximumQuantity < DWF.MinimumQuantity)
                        {
                            DWF.MinimumQuantity = DWF.MaximumQuantity;
                        }

                        iObjectCount = Random.Range(DWF.MinimumQuantity, DWF.MaximumQuantity);

                        //Option to use only one of the items
                        if (DWF.useSingleObjectMultipleTimes)
                        {
                            iObjectSelected = Random.Range(0, (objects.Count - 1));
                        }

                        for (int i = 0; i < iObjectCount; i++)
                        {
                            //With the option off, use a different object for each item (well, it's random so POTENTIALLY a different one each time)
                            if (DWF.useSingleObjectMultipleTimes == false)
                            {
                                iObjectSelected = Random.Range(0, (objects.Count - 1));
                            }
                            
                            gameObject = Instantiate(Resources.Load(DWF.FolderName + "\\" + objects[iObjectSelected], typeof(GameObject))) as GameObject;

                            AdjustObject(gameObject, DWF);
                        }
                    }
                }
            }
            #endregion

            //-- A SPECIFIC OBJECT
            #region Collection with Game Object
            foreach(ObjectDetailsWithGameObject DWGO in DetailsWithObject)
            {
                //Let's just make sure that the dev put an object in this property.
                if (DWGO.gameObject != null)
                {
                    gameObject = Instantiate(DWGO.gameObject);

                    //Insure that Max is not smaller than Min (hey, all us devs make mistakes)
                    if (DWGO.MaximumQuantity < DWGO.MinimumQuantity)
                    {
                        DWGO.MinimumQuantity = DWGO.MaximumQuantity;
                    }

                    iObjectCount = Random.Range(DWGO.MinimumQuantity, DWGO.MaximumQuantity);

                    for (int i = 0; i < iObjectCount; i++)
                    {
                        gameObject = Instantiate(DWGO.gameObject);

                        AdjustObject(gameObject, DWGO);
                    }
                }
            }
            #endregion

        }
        catch (System.Exception)
        {
            throw;
        }  
    }

    public VectorObject AdjustObject(GameObject gameObject, ObjectDetails objectDetails)
    {
        //The return object allows this function use outside this class 
        //when there is a desire to place Other objects near it.
        VectorObject VectorReturn = new VectorObject();

        try
        {
            float x;
            float y;
            float z;
            VectorObject position;
            Vector3 objectSize = gameObject.transform.localScale;


            //If the terrain is used as the Min/Max of the area that can be used for placing the object
            if (objectDetails.useTerrainSize)
            {
                if (currentTerrain != null)
                {
                    objectDetails.MinimumPosition = currentTerrain.GetPosition();
                    objectDetails.MaximumPosition = currentTerrain.terrainData.size; //TODO: should the Y value be reset to zero or...?
                }
            }

            //Game Object's Size
            if (objectDetails.isScaled)
            {
                x = Random.Range(objectDetails.MinimumScale.x, objectDetails.MaximumScale.x);

                //This option allows for all values to be scaled evenly, I choose X as it was first.  Deal with it.
                if (objectDetails.isEvenlyScaled)
                {
                    y = x;
                    z = x;
                }
                else
                {
                    y = Random.Range(objectDetails.MinimumScale.y, objectDetails.MaximumScale.y);
                    z = Random.Range(objectDetails.MinimumScale.z, objectDetails.MaximumScale.z);
                }
                gameObject.transform.localScale = new Vector3(x, y, z);
            }

            //Game Object's Rotation
            x = Random.Range(objectDetails.MinimumRotation.x, objectDetails.MaximumRotation.x);
            y = Random.Range(objectDetails.MinimumRotation.y, objectDetails.MaximumRotation.y);
            z = Random.Range(objectDetails.MinimumRotation.z, objectDetails.MaximumRotation.z);
            gameObject.transform.rotation = Quaternion.Euler(x, y, z);

            //Some objects do not have a renderer, so in those cases use the original scale
            if (gameObject.GetComponent<Renderer>() != null)
            {
                objectSize = gameObject.GetComponent<Renderer>().bounds.size;
            }

            //Game Object's Position
            position = GameObjectPosition(
                objectSize,
                objectDetails);

            //Only place if the a position was found within GameObjectPosition.   
            //There is a possibility that there is no room or other reason why a position is not found
            if (position.IsFound)
            {
                gameObject.transform.localPosition = position.Vector;
                VectorReturn.Vector = position.Vector;
            }
            else
            {
                Destroy(gameObject);
                VectorReturn.IsFound = false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }

        return VectorReturn;
    }

    public List<string> LoadPrefabs(string Folder)
    {
        List<string> prefabs = new List<string>();

        try
        {
            Object[] objects = Resources.LoadAll(Folder, typeof(GameObject));

            foreach (var t in objects)
            {
                prefabs.Add(t.name);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }

        return prefabs;
    }

    public VectorObject GameObjectPosition(Vector3 ObjectSize, ObjectDetails objectDetails)
    {  
        //Objects with Y set at ZERO should have the Y value updated to the top of terrain at that position
        //Items below or above ZERO are left to that position.  YEA, ZERO denotes on Terrain.  Got a better idea?

        VectorObject position = new VectorObject();

        try
        {
            float x;
            float y;
            float z;
            float y2 = 0;
            bool isFound = false;
            Vector3 ObjectPositionToCheck;
            bool isIntersected = false;
            byte bAttempts = 0;
            

            do
            {
                y2 = 0; //Reset
                x = Random.Range(objectDetails.MinimumPosition.x, objectDetails.MaximumPosition.x);
                y = Random.Range(objectDetails.MinimumPosition.y, objectDetails.MaximumPosition.y);
                z = Random.Range(objectDetails.MinimumPosition.z, objectDetails.MaximumPosition.z);
                ObjectPositionToCheck = new Vector3(x, y, z);
                

                if (objectDetails.avoidObjects)
                {
                    //If y is zero, then it should be placed on the ground, aka top level of the terrain
                    if (y == 0 && this.currentTerrain != null)
                    {
                        //Find the top Y postion for the terrain
                        y = currentTerrain.SampleHeight(ObjectPositionToCheck);
                        y2 = y; 
                    }

                    //While this seems like the same call as above, the terrain location y value could still come back as zero,
                    //if so it is in water, at least in that location.
                    if (y > 0 || objectDetails.PercentInWater > 0)
                    {
                        ObjectSize.y += objectDetails.toTheSky;

                        ObjectPositionToCheck = new Vector3(x, y + (ObjectSize.y / 2) + 0.2f, z);

                        isIntersected = Physics.CheckBox(ObjectPositionToCheck, (ObjectSize / 2));

                        ObjectPositionToCheck.y -= (objectDetails.toTheSky / 2);
                        
                        if (isIntersected == false)
                        {
                            //Remove this value before moving to check if it's in water.  
                            //No need to do it above as it saves running this line if the object was intersected
                            ObjectSize.y -= objectDetails.toTheSky;

                            if (IsInWater(ObjectPositionToCheck, ObjectSize, objectDetails.PercentInWater) == false)
                            {
                                isFound = true;
                            }
                        }
                    }
                }
                else
                {
                    //Since the AvoidObjects flag is false, we only need to check if it is within water
                    if (IsInWater(ObjectPositionToCheck, ObjectSize, objectDetails.PercentInWater) == false)
                    {
                        isFound = true;
                    }
                }

                //To avoid infinite loop
                bAttempts++;
                if (bAttempts > 250)
                {
                    //It is possible that the object was found during the last cycle.  So set the "IsFound" to whatever it is, true or false.
                    position.IsFound = isFound; //This is used in the function PopulateExperience() in case no position is found
                    isFound = true;             //Not necessarily true, but we need to get out of this loop.
                }

            } while (isFound == false);


            if (y2 > 0)
            {
                //Setting the Y value to the top of the terrain.
                ObjectPositionToCheck.y = y2;
            }

            //Adjusting this just a little to insure that if an object is at ground level then it is burried a tiny bit to avoid floating.
            //Yes this is a hack.  It would be better to find the lowest point of the object.  Someday.
            ObjectPositionToCheck.y += objectDetails.yVariance;

            position.Vector = ObjectPositionToCheck;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }

        return position;
    }

    public bool IsInWater(Vector3 ObjectPosition, Vector3 ObjectSize, float PercentAllowed)
    {
        bool bReturn = false;

        try
        {
            //Check if object is in water.  
            //Create 100 objects each JUST above the WATERLINE to the height of the terrain
            //If the number of objects is less than the percentage allowed it is considered within Water. 
            //NOTE: I am not happy with this function, and I FEEL that there is a better solution.  I just don't have one yet.

            //If the Y position is higher than the Water Level, then the object is not in the water
            if (ObjectPosition.y <= this.WaterLevel)
            {
                byte bChunks = 10;
                byte bNumberInWater = 0;
                float x = (ObjectSize.x / bChunks);
                float z = (ObjectSize.z / bChunks);
                float xStart = ObjectPosition.x + (ObjectSize.x / 2) - (x / 2);
                float zStart = ObjectPosition.z + (ObjectSize.z / 2) - (x / 2);
                float terrainHeight = 0; 
                if (currentTerrain != null)
                {
                    //Null Conditional is not available in the version of c# I am using for the Unity VR Experience.  
                    //So I am using this bit of a hack to insure that the data is what is needed if there is no terrain.
                    terrainHeight = currentTerrain.terrainData.size.y;
                }
                Vector3 ChunkSize = new Vector3((ObjectSize.x / bChunks), terrainHeight, (ObjectSize.z / bChunks));
                Vector3 ObjectPositionToCheck;
                bool isIntersected = false;


                for (byte bWidth = 0; bWidth < bChunks; bWidth++)
                {
                    xStart = ObjectPosition.x + (ObjectSize.x / 2) - (x / 2);

                    for (byte bDepth = 0; bDepth < bChunks; bDepth++)
                    {
                        //Add a little bit to insure it is above water.  
                        //The +.1f is to insure that the Object is to insure it does not intersect with the water object
                        ObjectPositionToCheck = new Vector3(xStart, System.Convert.ToSingle(terrainHeight / 2) + .1f + this.WaterLevel, zStart);

                        //Create a cube in that space, check if it is in water, if so add that value to bNumberInWater
                        isIntersected = Physics.CheckBox(ObjectPositionToCheck, (ChunkSize / 2));

                        //If it's not interselected, it is assumed to be in water
                        if (isIntersected == false)
                        {
                            bNumberInWater++;
                        }

                        xStart -= ChunkSize.x;
                    }
                    zStart -= ChunkSize.z;
                }

                //If it is AT or ABOVE that allowed percentage, this object is show as "In Water"
                //GameObjectPosition calls this function, and if the object is in water it does not place in the current position and keeps trying to find a position
                if (bNumberInWater >= PercentAllowed)
                {
                    bReturn = true;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }

        return bReturn;
    }
}


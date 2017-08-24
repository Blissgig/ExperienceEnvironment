# ExperienceEnvironment
This script is to allow the population of a collection of objects randomly thoughout a given area or terrain.

This is designed for Unity3D.


-- PROPERTIES --
There are two class level properties
* Water Level.  This is an int.  Default is zero.
* Current Terrain.  Used to help place the objects, using the terrain height, and possible location.

There are two sections available in the inspector.
1) Gather objects from a subfolder within the "Resources" folder
2) Specific object.

-------------------------------------------

Each of these have the following properties
* Minimum Position.  This is a Vector3 object.
* Maximum Position.  This is a Vector3 object.  These two properties allow the developer to place the object within a range.  To place the object(s) within a specific position place both the Min and Max value to the same value.
* Minimum Rotation.  This is a Vector3 object.
* Maximum Rotation.  This is a Vector3 object.  Same as above, but for the rotation of the object (obviously)
* Minimum Scale.  This is a Vector3 object.
* Maximum Scale.  This is a Vector3 object.  Same as above, but for the size of the object.
* Use Terrain Size. This is a boolean.  This will over write the Min/Max Position values.
* Is Scaled.  This is a boolean.  Resize the object based on Min/Max Scale.  See function "AdjustObject"
* is Evenly Scaled.  This is a boolean.  Resize the object based on the X value of Min/Max Scale
* Avoid Objects.  This is a boolean.  If this is set then the code will make sure the object is not within another object.   See function "GameObjectPosition"
* Percent In Water.  This is a byte.  This allows the object to be that percent in water.  ie: Trees, zero percent.  Mountains; 40 percent.  In this scenario trees would not be place in the water (as stated by the Water Level variable)
* To The Sky.   This is an int.  This variable is used to insure that objects are not above the placed object.  For example when placing trees it is important to insure that they are not under a mountain.
* Minimum Quantity.  This is an int.
* Maximum Quantity.  This is an int.  These allows a random amount amount of the objects to be placed.  Again, a matched value in the Min and Max will then place only that amount of that object.
* Y Variance.  This is an int.   This is used to add/remove a bit of height in the position.  ie: To insure that a tree is not floating above the ground.   A value of -.02 would in this example place the object just within the ground.

-------------------------------------------

Details With Folder
There are a few variables specific to this object
* Folder Name.  This is a string.    ie: There needs to be a folder named "Resources", the value placed within this variable would be "Mountains" to note the subfolder "Mountains" within the "Resources" folder.   Do NOT place "Resources\Mountains" in this property.  Only place the subfolder name.
* Use Single Object Multiple Times.  This is a boolean.   This will choose ONE item out of the folder and use it as many times as the Min/Max quantity allow.

-------------------------------------------

Details With Object
The additional property to this object is:
* Game Object.  This is a GameObject (obviously) and is used as many times as the Min/Max qualtity allow.



-- NOTES --
Be aware that the order you place objects within the inspector is the order that they are placed within the environment.  For example, in a project where I am using this I place Mountains before I place trees and other objects.  This is to insure that trees are not placed under a mountain.  Your needs may vary, just be aware of this design issue.

The "IsInWater" function works, but I am not as happy with it as I could be.  Because I am rather new to Unity I believe there may be a better solution to this issue.  Feel free to let me know.

If no value is placed in the "Current Terrain" property, the currently active terrain is selected.  If there is no terrain...  well, this is going to go badly.  It REALLY needs a terrain to function.   I believe I should simply throw an error if there is no terrain, but I will see if I can think of any better solution.  See function "PopulateExperience"

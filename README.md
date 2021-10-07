## What is it?  
This is my personal package for additions to the networking library. It adds some extra features on to Unitys Netcode for GameObjects. I'm still tweaking it and testing that everything works in all cases and it comes as "use at your own risk".  

- Adds NetworkReferenceXXX types to handle sending references over the network in RPCs
    - NetworkReferenceObject
    - NetworkReferenceComponent<>
    - NetworkReferenceAsset<>
    - NetworkString
- Adds NetworkVariableXXX types to use the NetworkReferenceXXX types in networked variables 
    - NetworkVariableObject
    - NetworkVariableComponent<>
    - NetworkVariableAsset<>
    - NetworkVariableString
- Adds NetworkListXXX types to use the NetworkReferenceXXX types in networked lists
    - NetworkListObject
    - NetworkListComponent<>
    - NetworkListAsset<>
    - NetworkListString
- Adds system for linking up references to assets over the network (i.e: ScriptableObjects)
    - NetworkAssetManager
    - NetworkAssetManifest  
- Adds Property Drawer for NetworkVariable<> 
    - Supports updating the value across the network when changed at runtime
    - Handles all the above referenceVariableXXX types to allow assigning references in inspector
    - Handles all types usable with NetworkVariable<> including custom struct types
- Adds PropertyDrawer for my NetworkListXXX types
    - Properly displays the list and its contents in the editor inspector
    - Can be altered in the editor inspector while running and changes will be networked.
- Adds NetworkVariable<> Equivalents for some unity field attributes
    - RangeNetworkAttribute
    - MinNetworkAttribute
    - MaxNetworkAttribute
    - ColourUsageNetworkAttribute

## How do I use it?

Install Unitys Netcode package  
&emsp; This is made for the develop branch. It doesn't support the main branch  
&emsp; PackageManager -> Add package from git URL  
&emsp; https://github.com/Unity-Technologies/com.unity.netcode.gameobjects.git?path=/com.unity.netcode.gameobjects#develop  

Install the Addon package  
&emsp; PackageManager -> Add package from git URL  
&emsp; https://github.com/SubtleSanity/NetcodeAddons.git?path=/com.netcode.gameobjects.addons  

Modify Unity.Netcode  
&emsp; **There is a function in the unity netcode package that is internal, but we require it to make referencing work**  
&emsp; After adding the Addon package you will see an error that a function is "inaccessable due to its protection level"  
&emsp; You need to change that function to public  
&emsp; The function is NetworkObject.GetNetworkBehaviourAtOrderIndex()  
&emsp; You can find it in the Unity.Netcode.Runtime project  

## NetworkReferenceXXX types

- NetworkReferenceObject for NetworkObject
- NetworkReferenceComponent<> for types inheriting NetworkBehaviour
- NetworkReferenceAsset<> for assets (any asset that inherits UnityEngine.Object, including ScriptableObject, Materials, etc)
- NetworkString for strings

These are structs that store a reference to an object.
When sent over the network they serialize to the appropriate Id numbers for the object that can be restored at the other end

You can use them as parameters in RPCs to send references in RPCs
You can use them as variables using the NetworkVariableXXX types
- NetworkVariableObject
- NetworkVariableComponent<>
- NetworkVariableAsset<>
- NetworkVariableString

## Referencing assets across the network
This system will allow you to send references to assets across the network. Useful for sending a reference to a scriptable object but can be used for any asset that inherits from UnityEngine.Object including meshes, materials, etc.  
It works by creating a list of known assets with id numbers. The server can simply send the id number to a client and the client will look up the local copy of the asset.

To set up the list of assets:  
&emsp; Add a NetworkAssetManager component to your NetworkManager gameobject  
&emsp; Create a NetworkAssetManifest by right clicking in the project explorer and selecting Create/Netcode/Network Asset Manifest  
&emsp; Add the NetworkAssetManifest to the NetworkAssetManager component you created  
&emsp; Add any assets you want to reference to the NetworkAssetManifest  

Assets are added using ScriptableObjects instead of adding them directly to the manager  
This allows for proper organisation of networked assets instead of having one monolithic list on a scene component. 

Use NetworkReferenceAsset<> to send an asset reference in an RPC    
Use NetworkVariableAsset<> to keep an asset reference synced in a variable

## Custom property drawers

I've implemented a PropertyDrawer for NetworkVariable<> so NetworkVariables will show up in the inspector correctly.  
 - It handles making the variables readonly when the project is not running, is not connected or is running as a client (since only server can edit variables)  
 - It handles assigning values in the inspector correctly. The new value will be sent across the network exactly the same as if you had set the value on the NetworkVariable by code.
 - It handles all types supported by NetworkVariable<>. Displays them in the same format as variables are normally displayed in inspector and will respect any custom property drawers those types have.

There are also PropertyDrawer for the NetworkReferenceXXX types, so they display the reference in the inspector instead of the internal Id numbers that they actually contain and you can assign objects to them by dragging like normal. Unfortunately because they depend on the NetworkManager/NetworkAssetManager to look up their values you can't assign references to them when the game isn't running.  

Unity didn't use property drawers for NetworkVariables and instead implemented a custom editor for NetworkBehaviour that uses reflection on the class to find all the NetworkVariables and manually draw them seperately to the rest of the class. I'm not quite sure what the benefit of doing it that way was but it creates a few issues:
 - It stops NetworkVariables from being drawn the normal way, so we can't implement custom PropertyDrawers for any NetworkVariables or types used in NetworkVariables. 
 - It prevents us from being able to implement attributes for NetworkVariables
 - It  means if a user were to implement a custom editor for a class that inherit NetworkBehaviour they would lose the ability to see NetworkVariables in the editor as their editor would overrule the unity one. 
 - Their approach also involves manually reflecting each type that could potentially be drawn, meaning that it's limited to only drawing primitive types that were known to the original programmer and won't support user defined structs etc.

The new PropertyDrawer replaces the functionality of drawing NetworkVariables and addresses the above issues. To prevent the unity custom editor from applying and interfering i've added an empty editor for NetworkBehaviour to overrule it and just draw the standard editor.  

## NetworkVariable attributes

I've added attributes to match some of the basic attributes that are normally available for use on fields.

RangeNetworkAttribute is the equivalent for RangeAttribute
- min: lowest value to allow
- max: heighest value to allow
- slider: whether to show the range as a slider instead of a textbox (defaults to true)    

MinNetworkAttribute is the equivalent for minAttribute
- min: lowest value to allow

MaxNetworkAttribute is the equivalent for maxAttribute
- max: highest value to allow

ColourUsageAttribute is the equivalent for ColorUsage
- showAlpha
- hdr

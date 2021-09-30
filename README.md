## What is it?  
This adds some extra features on to Unitys Netcode for GameObjects  

- Adds NetworkReferenceXXX types to handle sending references over the network  
- Adds NetworkVariableXXX types to simplify using NetworkReferenceXXX types in networked variables  
- Adds system for sending references to assets over the network (i.e: ScriptableObjects)  
- Adds Property Drawer for NetworkVariable<> that properly supports updating the NetworkVariable when value is changed in inspector  
- Adds Property Drawers for the NetworkReferenceXXX types to allow references to be assigned in inspector with proper typing  

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
- NetworkReferenceBehaviour<> for NetworkBehaviour
- NetworkReferenceAsset<> for assets (anything inheriting UnityEngine.Object, including ScriptableObject)

These work by storing the global network id number for the object/component/asset  
The client can then look up the local copy of the object.  

To get the reference to the object you call Get() or TryGet()  
If the object doesn't exist it will return null.  

You can use these as parameters in RPCs  
You can use these in NetworkVariable<>  
Use in NetworkVariable<> can be simplified by using the NetworkVariableXXX types:  
- NetworkVariableObject
- NetworkVariableBehaviour
- NetworkVariableAsset

## Referencing assets across the network
This system will allow you to send references to assets in RPCs and NetworkVariables. Useful for sending a reference to a scriptable object but can be used for any asset that inherits from UnityEngine.Object, including meshes, materials, etc. It works by creating a registry of assets with id numbers. The server can simply send the id number to a client and the client will look up the local copy of the asset.

To set up the register of assets:  
&emsp; Add a NetworkAssetManager component to your NetworkManager gameobject  
&emsp; Create a NetworkAssetManifest by right clicking in the project explorer and selecting Netcode/Network Asset Manifest  
&emsp; Add the NetworkAssetManifest to the NetworkAssetManager  
&emsp; Add any assets you want to reference to the NetworkAssetManifest  

Assets are added using ScriptableObjects instead of adding them directly to the manager  
This allows for proper organisation of networked assets instead of one monolithic list on a scene component  

Use NetworkReferenceAsset<> to send an asset reference in an RPC    
Use NetworkVariableAsset to keep an asset reference synced in a variable

## Custom property drawers

I've implemented a PropertyDrawer for NetworkVariable<> so NetworkVariables will show up in the inspector correctly.  
It handles making the variables readonly when the project is not running, is not connected or is running as a client (since only server can edit variables)  
It handles assigning values in the inspector correctly. The new value will be sent across the network exactly the same as if you had set the value on the NetworkVariable by code.

There are also PropertyDrawer for the NetworkReferenceXXX types, so they display the reference in the inspector instead of the internal Id numbers that they actually contain and you can assign objects to them by dragging like normal.  

Unity didn't use property drawers for NetworkVariables and instead implemented a custom editor for NetworkBehaviour that uses reflection on the class to find all the NetworkVariables and manually draw them seperately to the rest of the class. I'm not quite sure what the benefit of doing it that way was but it blocks NetworkVariables from being drawn the normal way. It also means if a user were to implement a custom editor for a class that inherit NetworkBehaviour they would lose the ability to see NetworkVariables in the editor as their editor would overrule the unity one. Their approach also involves manually reflecting each type that could potentially be drawn, meaning that it's limited to only drawing known primitive types and won't support user defined structs etc.

The new PropertyDrawers replace the functionality of drawing NetworkVariables entirely, address all of the above issues and will correctly draw all types supported by NetworkVariable<>. To prevent the unity custom editor from applying and drawing the variables twice i've added an empty editor for NetworkBehaviour to overrule it and just draw the standard editor.  

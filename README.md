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
- NetworkReferenceBehaviour for NetworkBehaviour
- NetworkReferenceAsset for assets (anything inheriting UnityEngine.Object, including ScriptableObject)

These work by storing the global network id number for the object/component/asset  
The client can then look up the local copy of the object.  

To get the reference to the object you call Get() or TryGet()  
If the object doesn't exist it will return null.  

You can use these as parameters in RPCs  
You can use these in NetworkVariable<>  
Use in NetworkVariable<> can be simplified by using the specific NetworkVariableXXX types:  
- NetworkVariableObject
- NetworkVariableBehaviour
- NetworkVariableAsset

## Referencing assets across the network

To set up and use the asset referencing system:  
&emsp; Add a NetworkAssetManager component to your NetworkManager gameobject  
&emsp; Create a NetworkAssetManifest by right clicking in the project explorer and selecting Netcode/Network Asset Manifest  
&emsp; Add the NetworkAssetManifest to the NetworkAssetManager  
&emsp; Add any assets you want to reference to the NetworkAssetManifest  

Assets are added using ScriptableObjects instead of adding them directly to the manager  
This allows for proper organisation of networked assets instead of one monolithic list on a scene component  

## Custom property drawers

I've implemented a proper PropertyDrawer for NetworkVariable<> so NetworkVariables will show up in the inspector correctly.  
It handles making the variables readonly when the project is not running, is not connected or is running as a client (since only server can edit variables)  
It handles assigning values in the inspector correctly. The new value will be sent across the network exactly the same as if you had set the value on the NetworkVariable by code.

There are also PropertyDrawer for the NetworkReferenceXXX types, so they display the reference in the inspector instead of the internal Id numbers that they actually contain and you can assign objects to them by dragging like normal.  

For some reason unity didn't do that and instead implemented a custom editor for NetworkBehaviour that uses reflection on the class to find all the NetworkVariable and manually draw them. I don't know why or what the benefit is over the standard method of customising the drawing of properties. My PropertyDrawers replace this functionality entirely, so to prevent their class editor from also drawing the variables i've added an empty editor for NetworkBehaviour to override it and just draw the standard editor.  

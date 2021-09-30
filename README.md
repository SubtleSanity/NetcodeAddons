## What is it?  
This some extra features added on to Unitys Netcode for GameObjects  

- Adds NetworkReferenceXXX types to handle sending references over the network  
- Adds NetworkVariableXXX types to simplify using NetworkReferenceXXX types  
- Adds system for sending references to assets over the network (i.e: ScriptableObjects)  
- Adds Property Drawers for standard NetworkVariables  
- Adds Property Drawers for the above reference types  

## How do I use it?

Install Unitys Netcode package  
&emsp; This is made for the develop branch. It won't work with the main branch  
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

There are also PropertyDrawer for the NetworkReferenceXXX types, so they display the reference in the inspect instead of the internal Id numbers that they actually contain and you can assign objects to them by dragging like normal.  

For some reason unity didn't do that and instead implemented a custom editor for NetworkBehaviour that uses reflection on the class to find all the NetworkVariable and manually draw them. I don't know why or what the benefit is over the standard method of customising the drawing of properties. My PropertyDrawers replace this functionality entirely, so to prevent their class editor from also drawing the variables i've added an empty editor for NetworkBehaviour to override it and just draw the standard editor.  

## Regarding NetworkList
I was planning to handle NetworkList in the inspector as well, but ...  
Unity has used NativeList as the internal container to store the values, which means that NetworkList can't support serialization properly. This means I can't make a custom PropertyDrawer for it and i can't use standard editor gui functionality to draw it. There are some hacky methods i could use to draw it manually but it would be more limited.

I'm unclear on what the benefit of using NativeList for it is. No functionality in the list uses jobs, nor is the internal list exposed in any way that would allow using it in jobs elsewhere. There's maybe an argument for garbage collection but the only allocations from a managed list here would be when the list expands due to additions, and i'm not convinced that's going to have a noticeable impact.

I sortof considered re-implementing NetworkList with a List<> internally instead of NativeList<>. This would let me implement proper PropertyDrawers for it but I don't want to make any major deviations away from the base packages functionality and i'm not sure it's worth it just to get nicer inspector workflow.

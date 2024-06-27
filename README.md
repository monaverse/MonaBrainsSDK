# Mona Brains SDK

The Mona Brain system is a high-level, powerful and easy to use no-code system designed to empower users to create interactive experiences and games in minutes!

### 🛑🛑🛑  This is experimental software and may change frequently  🛑🛑🛑

## Installation

To add this sdk to your Unity project, follow these steps:

1. Download and install Unity 2022.3.6f1
2. In Unity, in the top menu, select Edit -> Project Settings

![image](https://github.com/monaverse/MonaBrainsSDK/assets/541988/b4623049-0fbb-4404-9ffc-34bdbd59a224)

3. Under 'Package Manager' add a scoped registery:
     - Name: Open UPM
     - URL: https://package.openupm.com
     - Scope(s): com.monaverse, com.vrmc
  
4. Close the 'Project Settings' window.
5. In Unity, in the top menu, select Window -> Package Manager

6. In the 'Package Manager' window, select the second dropdown from the left, 'Packages: In Project' and select 'My Registries' 
![Screenshot 2024-03-27 at 9 12 41 AM](https://github.com/monaverse/MonaBrainsSDK/assets/541988/a3aba46e-7713-40ca-9e83-566a91b059ee)

7. Find the 'Mona Brains SDK' and select 'Install' on the top right to install the latest version. This will also install necessary dependencies.
![image](https://github.com/monaverse/MonaBrainsSDK/assets/541988/276bf023-faea-4e35-a0fc-f495d95929c7)

8. If you are not already using the latest 'Unity Input System' you will be asked to enable it which will restart your Unity editor.

9. Make sure you have a MonaTags scriptable object in your 'Library' 
![Screenshot 2024-03-27 at 1 20 04 PM](https://github.com/monaverse/MonaBrainsSDK/assets/541988/9e81d422-ef31-4d61-a8fe-642e1ebaa733)


if you don't, then create one by right clicking on your Brains folder (or Assets folder if you don't yet have a brains folder), then selecting: Create -> Mona Brains -> Utils -> Mona Tags from the context menu.
![Screenshot 2024-03-27 at 1 16 09 PM](https://github.com/monaverse/MonaBrainsSDK/assets/541988/1cf2af63-e0bd-448f-b629-d3ed3b34beff)


10. Once set up is complete, you should see a menu item at the top of Unity called 'Mona' with the option to launch the brains editor.

11. You may receive the following compiler errors once brains is installed. This is because the VRM and Brains Libraries use different GLTF importer libraries. 
<img width="875" alt="image" src="https://github.com/monaverse/MonaBrainsSDK/assets/541988/a71a245a-592f-429a-9ec2-11246a9870d6">

- Set your importer to the `MonaUnityGLTF` library. 
- Under Edit -> Project Settings -> Player -> Other Settings -> Scripting Define Symbols, you'll want to add the following items:
  - `UNIGLTF_DISABLE_DEFAULT_GLB_IMPORTER`
  - `UNIGLTF_DISABLE_DEFAULT_GLTF_IMPORTER`

<img width="781" alt="image" src="https://github.com/monaverse/MonaBrainsSDK/assets/541988/2ee77621-35fd-4336-9865-faefee6566fd">


13. Check back frequently for updated releases as this project is in active development and changing regularly.


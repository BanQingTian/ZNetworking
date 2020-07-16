/*
This asset is copyrighted by Moonflower Carnivore and published on Unity Asset Store in 2018.
Special thanks to Elvis Deane and Alan Lorence
who are the reasons I was inspired to make this asset and more importantly create visual effects.

It temporarily changes the layers of the assigned target objects to a designated one for shooting a screen capture.
This screen capture is used to tint the start color of each particle of the same pixel projection to the main camera.
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class dispersePixels : MonoBehaviour {
	[Tooltip("Interval in second each Target Object is dispersed before its older sibling. Either way, all Target Objects must be static since the execution of this script, otherwise the particles will not be tinted correctly. If each target must keep moving since the execution, assign them separately to this script in different objects instead of containing them in the same array.")]
	public float executionInterval = 0f;
	[Tooltip("Object to be dispersed. If this is unassigned, it will check the Renderer and Collider components contained in the same game object.")]
	public GameObject[] targetObject;
	[Tooltip("Camera which renders this mesh object for the 'Disperse Camera' to locate, but not being parented. If unassigned, it will assume the default scene camera with the 'MainCamera' tag.")]
	public Camera mainCamera;
	[Tooltip("Camera prefab for capturing the screenshot for tinting the Particle System of child object no.0.")]
	public Camera disperseCamera;
	[Tooltip("Temperarily changing the current object layer for capturing the screenshot. For example, 0 = Default, 1 = TransparentFX, 2 = Ignore Raycast, etc. Prefably do not share the index number with occupied layer such as Default or another object which would be dispersed simultaneously. When in doubt, maintain the index within 20 to 31.")]
	[Range(0, 31)]
	public int layerIndex = 31;
	[Tooltip("This overrides the predefined Layer Index and randomize a layer between 20 to 31. You may modify this script if you want to change the range.")]
	public bool randomizeLayer = false;
	[Range(1, 32)]
	[Tooltip("Divisor for resampling the resolution of the screenshot. The divisor is preferably the power of 2: 1, 2, 4, 8, etc. Higher resampling divisor slighlty improves performance. Too high then particle tinting accuracy suffers.")]
	public int downsampling = 4;
	//[Tooltip("Enable HDR format for the screenshot.")]
	//public bool HDR = false;
	[Tooltip("Unchecked: The screenshot of target object only use the chroma key color background, all pixels with alpha below 1 are deleted; Checked: The screenshot of everything are captured, all pixels with alpha equal zero are deleted.")]
	public bool transparentTarget = false;
	[Tooltip("Which Animator to be disabled when this script is executed. Animator will be resumed when this script is disabled/destroyed.")]
	public Animator animator;
	[Space(5)]
	[Header("Particle System settings:")]
	[Tooltip("Disperse Particle prefab to be tinted by screenshot.")]
	public ParticleSystem disperseParticle;
	[Tooltip("Are new clones of Disperse Camera, Disperse Particle, render texture instantiated instead of reusing the existing clones. Avoiding reinstantiate avoids the garbage collection pitfall, but if you want to change Disperse Particle prefab midway without interrupting the previous ongoing Disperse Pixels effect, you must reinstantiate.")]
	public bool reinstantiate = false;
	[Tooltip("Seconds delayed before destroying Disperse Particle clone when Reinstantiation is enabled or stopping it if not reinstantiated. This value should match the max lifetime of your Disperse Particle effect plus particle system delay time plus tinting time.")]
	public float destroyDelay = 3f;
	[Tooltip("Are the clones of Disperse Camera, Disperse Particle, render texture destroyed instantly on disable. Enabling this script again with this option checked would mean reinstantiating all new instances.")]
	public bool destroyOnDisable = false;
	[Tooltip("Delay in second of the particles being tinted by the screenshot since the emission. This is because the particles may not be emitted at time zero, resulting no particles being tinted.")]
	public float tintingTime = 0.0f;
	[Tooltip("Multiplier of particle start size and length/velocity scales. When particle system emitter shape is set to Skinned/Mesh Renderer modes, transform.scale will be ignored in scaling the particles.")]
	public float sizeMultiplier = 1;
	[Space(5)]
	[Tooltip("Maximum particle count of the Disperse Particle prefab. If either Disperse Particle prefab is not assigned or this property is given zero value, the whole process of screen capturing and particle tinting is skipped, this is useful when small transparent object is included in the disperse effect but it only needs to be faded out simulaneously without emitting disperse particles.")]
	public int[] maxParticles = new int[1]{3000};
	[Tooltip("This multiplies each Max Particles count in the array. This is especially helpful when you are switching between different Disperse Particle prefabs, you do not need to modify each Max Particle count in the array again.")]
	public float maxParticlesMultiplier = 1f;
	[Tooltip("Thickness of x and y axes of the Box emitter shape when no Skinned/Mesh Renderer is detected. For example, it should cover the whole sprite. The excess particles matching the chroma key color will be removed instantly.")]
	public Vector3[] emitterBoxSize = new Vector3[1]{new Vector3(1f, 1f, 0f)};
	[Tooltip("Color of the gizmos box for showing the dimension of the particle emitter shape when renderer component of the target object is not detected. For UI, the gizmos box assumes 100 plane distance between canvas and camera for performance.")]
	public Color gizmosColor = new Color(0.5f, 1f, 0.5f, 1f);
	[Space(5)]
	[Tooltip("The position of Disperse Particle offset to each Target Object. This is useful for positioning particle effect which does not use Skinned/Mesh Renderer emitter shape but otherwise requires a proper center to be emitted.")]
	public Vector3[] pSystemOffset = new Vector3[1]{new Vector3(0,0,0)};
	public enum FOM{none=0,outInstantly=1,outByTransparencyTintColor=2,outByTransparencyColor=3,outByCutoff=4,inInstantly=5,inByTransparencyTintColor=6,inByTransparencyColor=7,inByCutoff=8,outInInstantly=9,outInByTransparencyTintColor=10,outInByTransparencyColor=11,outInByCutoff=12};
	[Space(5)]
	[Header("Target Object's material settings:")]
	[Tooltip("Which mode of the Target Object is faded out. 'Instant Hide' disables the Renderer component. 'Transparency Tint Color' fades opacity by alpha of _TintColor property found in Particle shaders; 'Transparency Color' fades opacity by alpha of _Color property in Standard Shaders (Fade mode); 'outByCutoff' fades cutout threshold by _Cutoff property found in Standard Shaders (Cutout mode), Unlit/Transparent Coutout, or Legacy Shaders/Transparent/Cutout. If disperse particle and mesh fadeout are both activated, this script will check the presence of any _colliders component and disable it.")]
	public FOM fadingMode = FOM.outInstantly;
	[Tooltip("Duration in second of delaying the fade out by outByCutoff, in case the disperse particle prefab has made similar delay to ensure proper tinting.")]
	public float fadingDelay = 0.1f;
	[Tooltip("Duration in second of the first material of Renderer being faded out.")]
	public float fadingDuration = 0.3f;
	[Tooltip("Is Animator resumed after fade in effect.")]
	public bool animateAfterFadeIn = true;
	[Tooltip("New world position of the target object in Out-In effect.")]
	public Vector3 outInNewPosition = new Vector3(2f,2f,-2f);
	public float outInDuration = 1.8f;
	[Space(5)]
	[Tooltip("Material to substitute the one already assgined in Renderer for fade out. Changing Rendering Mode of Standard Shader is far more complicated. It is not only changing the value of _mode which is merely a tag name property. For details, download and read the StandardShaderGUI.cs from the official site. If the length of this array does not equal to that of Target Object or no material is assigned, the substitution is bypassed.")]
	public Material[] materialSubstitute;
	[Tooltip("Material to be reverted from substitution after disabling this script or object. This is useful for teleport/blink effect.")]
	public Material[] materialOriginal;
	[Space(5)]
	[Header("Audio settings:")]
	[Tooltip("Does this script play a sound clip?")]
	public bool playSound = true;
	[Tooltip("Object which contains an Audio Source component. Position of this object should be the center of all Target Objects in the array.")]
	public AudioSource audioSource;
	[Tooltip("Audio Clip to override the one already assigned in the Audio Source component.")]
	public AudioClip audioClip;
	[Tooltip("Which Target Object should the Audio Source attached to? This will affect how the Audio Source is rotated when offset is applied.")]
	public int soundOrigin = 0;
	[Tooltip("The position of Audio Source offset to the first Target Object.")]
	public Vector3 soundOffset = new Vector3(0f,0f,0f);
	[Tooltip("Duration in second of delaying the playing of Audio Clip.")]
	public float soundDelay = 0.0f;
	
	[HideInInspector]
	
	//Original Time Scale value to deal with the bug when current Time Scale is zero.
	//Ideally you should never ever execute this script when Time Scale is zero.
	float _orgTimeScale = 1f;
	
	//Indices of component types of all targetOjects for skipping the check of GetComponent() in if statements.
	//0 = renderer componenet; 1 = UI image component; 2 = UI text component.
	int[] _componentTypes;
	//Array of Renderer components of target objects obtained from GetComponent() in Awake().
	Renderer[] _renderers;
	//Array of UI Image components of target objects obtained from GetComponent() in Awake().
	Image[] _UI_images;
	//Array of UI Text components of target objects obtained from GetComponent() in Awake().
	Text[] _UI_texts;
	
	//Array of all original layer indices of targets objects to be restored 
	//before being temporarily changed by this script to another layer for shooting screen capture.
	int[] _orgObjectLayers;
	//The instantiated game object which contains the Disperse Camera settings.
	GameObject _disperseCameraClone;
	//The Camera component from the Disperse Camera prefab in the instantiated clone.
	Camera _disperseCamera;
	//Rectangle dimension of the camera viewport for defining the size of the Render Texture and screen captures.
	Rect _viewRect;
	Vector2 _viewRectDownsampled;
	//New Render Texture for shooting new screen capture.
	RenderTexture _renderTexture;
	//Screen capture for tinting the particles.
	//Particles will only get alpha value from this when transparentTarget property is true.
	Texture2D _screenCapture1;
	//Second screen capture for tinting the particles' RGB when transparentTarget property is true.
	Texture2D _screenCapture2;
	
	//Arrange of UI canvas for getting settings of their worldCameras and planeDistances.
	Canvas[] _UI_canvases;
	
	//All Particle System related private variables use the "_PS_" prefix.
	//All Particle System Module related private variables use the "_PSM_" prefix.
	
	//Array of game object clones instantiated from the Disperse Particle prefab
	//which are assigned to be parented to each targetObject.
	GameObject[] _PS_clones;
	//Array of Particle System components from the instantiated Disperse Particle clone.
	ParticleSystem[] _PS_components;
	//Array of particle system Main modules for changing their properties.
	ParticleSystem.MainModule[] _PSM_mains;
	//Original gravity modifier value of particle system which ignores transform-scale by design, 
	//so this script scales it externally.
	float _PS_orgGravity;
	//Array of particleHomecoming script components called 
	//when "in" (materialization) or "out-in" (teleport) fading modes are used.
	particleHomecoming[] _particleHomecomings;
	
	//Array of Materials for manipulating their _Cutoff, _TintColor or _Color values.
	Material[] _materials;
	//Array of original color values of all RGBA color values obtained either from 
	//Renderer Material, UI Image or UI Text of each targetObject.
	Color[] _orgColorRGBA;
	//Specific original alpha value of _orgColorRGBA for interpolation or being restored OnDisable().
	float[] _orgColorAlpha;
	
	//Game object clone instantiated from the assigned Audio Source prefab.
	GameObject _audioClone;
	//Audio component from the instantiated audio clone.
	AudioSource _audio;
	
	//Array of Collider components from targetObjects for being disabled temporarily 
	//by this script when the targetObject is to be faded in or faded out by any mode. 
	//This is ignored when "none" fading mode is selected.
	//Collider will be enabled again OnDisable().
	Collider[] _colliders;
	
	int _FOMifGroup;
	bool[] _materialChangeCondition;
	
	void Awake () {
		//Automatically assigning targetObject to the array when none is assigned to the script.
		if (targetObject == null || targetObject.Length < 1 || targetObject[0] == null) {
			if (GetComponent<Renderer>()) {
				//When Renderer component is found in the same object which contains this script.
				targetObject = new GameObject[1]{this.gameObject};
			} else if (GetComponent<Image>()) {
				//When UI image component is found in the same object which contains this script.
				targetObject = new GameObject[1]{this.gameObject};
			} else {
				//Else just check if there this game object is parented to another object 
				//which HOPEFULLY contains any eligible component, but please don't be lazy.
				targetObject = new GameObject[1]{this.transform.parent.gameObject};
			}
		}
		
		/*
		When no camera in the scene is assigned to the mainCamera property of this script 
		for shooting screen capture for tinting particles. 
		If you instantiate any Disperse Initiator prefab, 
		that script should have a target camera to be assigned to this script.
		
		"Camera.main" is essentially FindGameObjectsWithTag which has a slight performance concern, 
		so please pre-assign the camera manually whenever possible.
		*/
		if (mainCamera == null) {
			mainCamera = Camera.main;
		}
		//Basically the length of most arrays in this script should agree with that of targetObject array.
		if (targetObject.Length != maxParticles.Length) {
			Array.Resize(ref maxParticles, targetObject.Length);
		}
		if (targetObject.Length != pSystemOffset.Length) {
			Array.Resize(ref pSystemOffset, targetObject.Length);
		}
		if (targetObject.Length != emitterBoxSize.Length) {
			Array.Resize(ref emitterBoxSize, targetObject.Length);
		}
		
		//Prefabs to be instantiated right off the bat, otherwise with reinstantiate = true, 
		//the following prefabs are instantiated OnEnable().
		if (!reinstantiate) {
			_disperseCameraClone = Instantiate(disperseCamera).gameObject;
			_disperseCameraClone.name = disperseCamera.name+" of "+targetObject[0].name;
			_disperseCamera = _disperseCameraClone.GetComponent<Camera>();
			_viewRect = _disperseCamera.pixelRect;
			_viewRectDownsampled = new Vector2 (_viewRect.width/downsampling, _viewRect.height/downsampling);
			_renderTexture = new RenderTexture ( 
				(int)(_viewRectDownsampled.x), 
				(int)(_viewRectDownsampled.y), 
				0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear
			);
			_screenCapture1 = new Texture2D ( 
				(int)(_viewRectDownsampled.x), 
				(int)(_viewRectDownsampled.y), 
				TextureFormat.RGBA32, false, true
			);
			if (!_screenCapture2) {
				_screenCapture2 = new Texture2D ( 
					(int)(_viewRectDownsampled.x), 
					(int)(_viewRectDownsampled.y), 
					TextureFormat.RGBA32, false, true
				);
			}
		}
		
		_PS_clones = new GameObject[targetObject.Length];
		_PS_components = new ParticleSystem[targetObject.Length];
		_PSM_mains = new ParticleSystem.MainModule[targetObject.Length];
		_particleHomecomings = new particleHomecoming[targetObject.Length];
		
		_componentTypes = new int[targetObject.Length];
		_renderers = new Renderer[targetObject.Length];
		_UI_images = new Image[targetObject.Length];
		_UI_texts = new Text[targetObject.Length];
		_colliders = new Collider[targetObject.Length];
		_UI_canvases = new Canvas[targetObject.Length];
		_materialChangeCondition = new bool[targetObject.Length];
		for (int i = 0; i < targetObject.Length; i++) {
			//To reduce the number of GetComponent of either Renderer, UI Image, or UI Text.
			if (targetObject[i].GetComponent<Renderer>()) {
				_componentTypes[i] = 0;
				_renderers[i] = targetObject[i].GetComponent<Renderer>();
				_UI_images[i] = null;
				_UI_texts[i] = null;
			} else if (targetObject[i].GetComponent<Image>()) {
				_componentTypes[i] = 1;
				_renderers[i] = null;
				_UI_images[i] = targetObject[i].GetComponent<Image>();
				_UI_texts[i] = null;
			} else if (targetObject[i].GetComponent<Text>()) {
				_componentTypes[i] = 2;
				_renderers[i] = null;
				_UI_images[i] = null;
				_UI_texts[i] = targetObject[i].GetComponent<Text>();
			}
			
			if (targetObject[i].GetComponent<CapsuleCollider>()) {
				_colliders[i] = targetObject[i].GetComponent<Collider>();
			}
			if (maxParticles[i] != 0 && !reinstantiate) {
				_PS_clones[i] = Instantiate(disperseParticle, transform.position, transform.rotation).gameObject;
				_PS_clones[i].transform.SetParent(targetObject[i].transform);
				_PS_clones[i].transform.localPosition = pSystemOffset[i];
				//This moves the Disperse Particle object to the top of all children of the parent object. 
				//You may change the index if it conflicts with other scripts 
				//which also requires accessing child object of index zero.
				_PS_clones[i].transform.SetSiblingIndex(0);
				_PS_components[i] = _PS_clones[i].GetComponent<ParticleSystem>();
				_PSM_mains[i] = _PS_components[i].main;
				_PS_orgGravity = _PSM_mains[i].gravityModifierMultiplier;
				if (_PS_clones[i].GetComponent<particleHomecoming>()) {
					_particleHomecomings[i] = _PS_clones[i].GetComponent<particleHomecoming>();
					_particleHomecomings[i].enabled = false;
				}
			}
		}
		_orgColorRGBA = new Color[targetObject.Length];
		_orgColorAlpha = new float[targetObject.Length];
		
		//Reinstantiate does not affect audio part of this script. It is always instantiated during Awake().
		if (playSound && audioSource) {
			_audioClone = Instantiate(audioSource).gameObject;
			_audioClone.transform.position = targetObject[soundOrigin].transform.position + soundOffset;
			_audioClone.transform.SetParent(targetObject[soundOrigin].transform);
			_audio = _audioClone.GetComponent<AudioSource>();
		}
	}
	
	void OnEnable () {
		/*
		This fixes a bug of executing this script 
		when Time Scale is zero by temporarily changing the Time Scale to 0.01.
		After 0.05 second, Time Scale is reverted to zero in RestoreOrgTimeScale().
		The engine will still gives you the following negligible warning: 
		"Internal: JobTempAlloc has allocations that are more than 4 frames old - 
		this is not allowed and likely a leak".
		At least the effect will work as usual when Time Scale is no longer zero and 
		no more "Assertion failed: Failed to deallocate temporary mesh data" error.
		*/
		if (Time.timeScale <= 0f) {
			_orgTimeScale = Time.timeScale;
			Time.timeScale = 0.01f;
		}
		
		DefineIfConditions();
		
		/*
		Especially for "in" (materialization) or "out-in" (teleport) fading mode for skinned mesh, 
		you must halt its animator, otherwise the tinted particles will not be able to return 
		to the correct surface position of the target object.
		*/
		if (animator) {
			animator.enabled = false;
		}
		
		ShootScreenCapture();
		
		InitiateParticleSystems();
		
		//Initiator of fading alpha or cutoff of material, alpha of UI image or UI text.
		if (fadingMode != FOM.none) {
			_materials = new Material[targetObject.Length];
			for (int i = 0; i < targetObject.Length; i++) {
				if (_FOMifGroup == 1) {
					if (_componentTypes[i] == 0) {
						_renderers[i].enabled = false;
					} else if (_componentTypes[i] == 1) {
						_UI_images[i].enabled = false;
					} else if (_componentTypes[i] == 2) {
						_UI_texts[i].enabled = false;
					}
				}
				StartCoroutine(FadeTargetObject(i));
			}
		}
		
		if (_audio && audioClip) {
			_audio.clip = audioClip;
			_audio.PlayDelayed(soundDelay);
		}
		
		if (_orgTimeScale <= 0f) {
			StartCoroutine(RestoreOrgTimeScale());
		}
	}
	
	IEnumerator RestoreOrgTimeScale () {
		yield return new WaitForSeconds(0.05f);
		Time.timeScale = _orgTimeScale;
		yield return null;
	}
	
	void DefineIfConditions () {
		if (
			fadingMode == FOM.outInstantly || 
			fadingMode == FOM.outByTransparencyTintColor || 
			fadingMode == FOM.outByTransparencyColor || 
			fadingMode == FOM.outByCutoff
		) {
			_FOMifGroup = 0;
		} else if (
			fadingMode == FOM.inInstantly || 
			fadingMode == FOM.inByTransparencyTintColor || 
			fadingMode == FOM.inByTransparencyColor || 
			fadingMode == FOM.inByCutoff
		) {
			_FOMifGroup = 1;
		} else if (
			fadingMode == FOM.outInInstantly || 
			fadingMode == FOM.outInByTransparencyTintColor || 
			fadingMode == FOM.outInByTransparencyColor || 
			fadingMode == FOM.outInByCutoff
		) {
			_FOMifGroup = 2;
		}
		
		for (int i = 0; i < targetObject.Length; i++) {
			if (
				targetObject.Length == materialSubstitute.Length && 
				materialSubstitute[i] != null && 
				materialOriginal[i] != null
			) {
				_materialChangeCondition[i] = true;
			} else {
				_materialChangeCondition[i] = false;
			}
		}
	}
	
	void ShootScreenCapture () {
		if (randomizeLayer) {
			/*
			Unity until 2017.3 still allots 31 layers with 
			0 (Default), 1 (TransparentFX), 2 (Ignore Raycast), 4 (Water), and 5 (UI) given default string names.
			
			Layer does not require a given string name (in Edit > Project Settings > Tags & Layers) to be accessible, 
			it just does not show in the game object inspector, you can access any via script if desired.
			*/
			layerIndex = UnityEngine.Random.Range(20, 31);
		}
		_orgObjectLayers = new int[targetObject.Length];
		for (int i = 0; i < targetObject.Length; i++) {
			_orgObjectLayers[i] = targetObject[i].layer;
			targetObject[i].layer = layerIndex;
		}
		//Reinstantiation or not, these clones are instantiated if the engine fails to detect any, 
		//otherwise the existent ones are reused.
		if (!_disperseCameraClone) {
			_disperseCameraClone = Instantiate(disperseCamera).gameObject;
			_disperseCameraClone.name = disperseCamera.name+" of "+targetObject[0].name;
			_disperseCamera = _disperseCameraClone.GetComponent<Camera>();
		}
		_disperseCameraClone.transform.localPosition = mainCamera.transform.position;
		_disperseCameraClone.transform.localEulerAngles = mainCamera.transform.eulerAngles;
		_disperseCameraClone.SetActive(true);
		//No more Chroma Keying. The green background is just for easier inspection during debugging.
		//Zero alpha must be maintained so this script will kill untinted particles instantly.
		_disperseCamera.backgroundColor = new Color32 (0,255,0,0);
		_disperseCamera.clearFlags = CameraClearFlags.SolidColor;
		
		//Updating the Disperse Camera crucial settings with that of the Main Camera.
		_disperseCamera.orthographic = mainCamera.orthographic;
		_disperseCamera.orthographicSize = mainCamera.orthographicSize;
		_disperseCamera.fieldOfView = mainCamera.fieldOfView;
		//Change the Disperse Camera culling mask to render only objects of the layerIndex for shooting screen capture.
		//This is bitwise operation. The left-shift operation (<<) shifts the binary 1 to the layerIndex-digit leftward 
		//while the rest of digits is filled with zero, so only layer of layerIndex is checked.
		_disperseCamera.cullingMask = 1 << layerIndex;
		for (int i = 0; i < targetObject.Length; i++) {
			//Making sure the targetObject is visible before shooting the screen capture.
			if (_FOMifGroup == 1) {
				if (_componentTypes[i] == 0) {
					_renderers[i].enabled = true;
					if (materialOriginal.Length == targetObject.Length && materialOriginal[i]) {
						_renderers[i].sharedMaterial = materialOriginal[i];
					}
				} else if (_componentTypes[i] == 1) {
					_UI_images[i].enabled = true;
				} else if (_componentTypes[i] == 2) {
					_UI_texts[i].enabled = true;
				}
			}
			//UI mode settings. 
			if (_componentTypes[i] == 1 || _componentTypes[i] == 2) {
				_UI_canvases[i] = GetComponentInParent<Canvas>();
				_UI_canvases[i].gameObject.layer = layerIndex;
				_UI_canvases[i].worldCamera = _disperseCamera;
			}
		}
		//Obtaining width and height of the camera viewport for defining the size of screen capture.
		_viewRect = _disperseCamera.pixelRect;
		_viewRectDownsampled = new Vector2 (_viewRect.width/downsampling, _viewRect.height/downsampling);
		if (!_renderTexture) {
			//HDR mode is commented until Particle System supports HDR color beyond Custom Data.
			//When it does uncomment the following and delete the repeated line outside of this if statement.
			/*
			if (HDR) {
				_renderTexture = new RenderTexture ( 
					(int)(_viewRectDownsampled.x), 
					(int)(_viewRectDownsampled.y), 
					0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear
				);
			} else {
				_renderTexture = new RenderTexture ( 
					(int)(_viewRectDownsampled.x), 
					(int)(_viewRectDownsampled.y), 
					0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear
				);
			}
			*/
			_renderTexture = new RenderTexture ( 
				(int)(_viewRectDownsampled.x), 
				(int)(_viewRectDownsampled.y), 
				0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear
			);
		} else {
			_renderTexture.Release();
			_renderTexture.width = (int)(_viewRectDownsampled.x);
			_renderTexture.height = (int)(_viewRectDownsampled.y);
		}
		_disperseCamera.targetTexture = _renderTexture;
		_disperseCamera.Render();
		RenderTexture.active = _renderTexture;
		//Defining the 2D screen capture settings.
		if (!_screenCapture1) {
			//HDR mode is commented until Particle System supports HDR color beyond Custom Data.
			//When it does uncomment the following and delete the repeated line outside of this if statement.
			/*
			if (HDR) {
				_screenCapture1 = new Texture2D ( 
					(int)(_viewRectDownsampled.x), 
					(int)(_viewRectDownsampled.y), 
					TextureFormat.RGBAHalf, false, true
				);
			} else {
				_screenCapture1 = new Texture2D ( 
					(int)(_viewRectDownsampled.x), 
					(int)(_viewRectDownsampled.y), 
					TextureFormat.RGBA32, false, true
				);
			}
			*/
			_screenCapture1 = new Texture2D ( 
				(int)(_viewRectDownsampled.x), 
				(int)(_viewRectDownsampled.y), 
				TextureFormat.RGBA32, false, true
			);
		} else {
			_screenCapture1.Resize ( 
				(int)(_viewRectDownsampled.x), 
				(int)(_viewRectDownsampled.y), 
				TextureFormat.RGBA32, false
			);
		}
		//Finally writing the render data on the screen capture image.
		_screenCapture1.ReadPixels ( _viewRect, 0, 0, false );
		if (transparentTarget) {
			/*
			For tinting transparent particles by transparent targetObject, 
			a second screen capture which shoots exactly what main camera renders is required. 
			You cannot obtain the correct raw RGB value because of complication of transparency blending.
			A compromise is to tint the particle's RGB with this second screen capture, 
			and set the particle's alpha with the first screen capture.
			
			For transparentTarget to work, the material requires alpha value to be written to framebuffer, 
			This is done via adding or modifying "ColorMask RGBA" in your transparent shader.
			For unknown reason, the built-in (transparent) particle shaders use "ColorMask RGB" instead, 
			this means no alpha value from the material is written to the final render.
			Hence we provide a custom "Particles/Alpha Blended Intensify" shader for switching ColorMask value.
			*/
			_disperseCamera.cullingMask = mainCamera.cullingMask;
			_disperseCamera.clearFlags = mainCamera.clearFlags;
			_disperseCamera.targetTexture = _renderTexture;
			_disperseCamera.Render();
			RenderTexture.active = _renderTexture;
			//When start color of Particle System supports HDR, 
			//the following needs to be amended like that of _screenCapture1.
			if (!_screenCapture2) {
				_screenCapture2 = new Texture2D ( 
					(int)(_viewRectDownsampled.x), 
					(int)(_viewRectDownsampled.y), 
					TextureFormat.RGBA32, false, true
				);
			} else {
				_screenCapture2.Resize ( 
					(int)(_viewRectDownsampled.x), 
					(int)(_viewRectDownsampled.y), 
					TextureFormat.RGBA32, false
				);
			}
			//Taking the screen capture.
			_screenCapture2.ReadPixels ( _viewRect, 0, 0, false );
		}
		
		//Making sure the targetObject is invisible after shooting the screen capture in "in" (materialization) mode.
		if (_FOMifGroup == 1) {
			for (int i = 0; i < targetObject.Length; i++) {
				if (_componentTypes[i] == 0) {
					_renderers[i].enabled = false;
				} else if (_componentTypes[i] == 1) {
					_UI_images[i].enabled = false;
				} else if (_componentTypes[i] == 2) {
					_UI_texts[i].enabled = false;
				}
			}
		}
		
		//Release memory after done shooting screen capture and writting to _screenCapture1/2.
		_disperseCamera.targetTexture = null;
		RenderTexture.active = null;
		_disperseCameraClone.SetActive(false);
		_screenCapture1.hideFlags = HideFlags.HideAndDontSave;
		if (transparentTarget) {
			_screenCapture2.hideFlags = HideFlags.HideAndDontSave;
		}
		
		for (int i = 0; i < targetObject.Length; i++) {
			//Reverting layer changes.
			targetObject[i].layer = _orgObjectLayers[i];
			//Reverting UI mode changes.
			if (_componentTypes[i] == 1 || _componentTypes[i] == 2) {
				_UI_canvases[i].gameObject.layer = _orgObjectLayers[i];
				_UI_canvases[i].worldCamera = mainCamera;
			}
		}
		
		//Debug section for checking the actual screenshot to be displayed on a quad object parented to the target mesh.
		/*
		if (transform.GetChild(0).gameObject.activeSelf) {
			transform.GetChild(0).GetComponent<Renderer>().materials[0].SetTexture("_MainTex", _screenCapture1);
			//transform.GetChild(0).GetComponent<Renderer>().materials[0].SetTexture("_MainTex", _screenCapture2);
		}
		*/
	}
	
	void InitiateParticleSystems () {
		/*
		A separate Particle System emitter seed is generated externally 
		and applied to all Disperse Particle clones in the same targetObject array
		to ensure they share the same seed instead of being totally random.
		This especially helps when noise is applied to particle positions so all clones emit particles uniformly.
		Still emitting particles from different Particle Systems has the weakness of poor transparency depth sorting, 
		hence most Disperse Particle prefabs use custom opaque material.
		*/
		uint _particlesystemRandomSeed = (uint)UnityEngine.Random.Range(1, 100000);
		
		//This sends the required new position of the targetObject to particleHomecoming script
		//after teleport for "out-in" mode effect.
		if (_FOMifGroup == 2) {
			disperseParticle.GetComponent<particleHomecoming>().newPositionVector = outInNewPosition;
		}
		for (int i = 0; i < targetObject.Length; i++) {
			//Disabling the collider to avoid particle collision when targetObject is not yet fully visible.
			if (fadingMode != FOM.none && _colliders[i]) {
				_colliders[i].enabled = false;
			}
			//In case you accidentally setting emitterBoxSize with negative value.
			emitterBoxSize[i] = new Vector3 ( 
				Mathf.Abs(emitterBoxSize[i].x), 
				Mathf.Abs(emitterBoxSize[i].y), 
				Mathf.Abs(emitterBoxSize[i].z)
			);
			
			//No Disperse Particle prefab is instantiated if maxParticles value is zero.
			if (maxParticles[i] != 0) {
				if (!_PS_clones[i] || reinstantiate) {
					_PS_clones[i] = Instantiate(disperseParticle, transform.position, transform.rotation).gameObject;
					if (!destroyOnDisable) {
						var _selfDestroy = _PS_clones[i].GetComponent<selfDestroy>();
						_selfDestroy.delayInSecond = executionInterval * i + destroyDelay;
						_selfDestroy.enabled = true;
					}
					_PS_clones[i].transform.SetParent(targetObject[i].transform);
					_PS_clones[i].transform.SetSiblingIndex(0);
					_PS_components[i] = _PS_clones[i].GetComponent<ParticleSystem>();
					_PSM_mains[i] = _PS_components[i].main;
					if (_PS_clones[i].GetComponent<particleHomecoming>()) {
						_particleHomecomings[i] = _PS_clones[i].GetComponent<particleHomecoming>();
					}
				}
				//Only in World simulation space each particle's position can be correctly projected 
				//on the screen for being tinted by the screen capture.
				_PSM_mains[i].simulationSpace = ParticleSystemSimulationSpace.World;
				
				ParticleSystem.ShapeModule _PSM_shape = _PS_components[i].shape;
				ParticleSystem.NoiseModule _PSM_noise = _PS_components[i].noise;
				ParticleSystemRenderer _PS_renderer = _PS_clones[i].GetComponent<ParticleSystemRenderer>();
				
				//Stopping Particle System for next emission.
				//Particle System randomSeed can only be updated when it stops playing.
				if (reinstantiate) {
					_PS_components[i].Stop();
				} else {
					//Clear previous particle emission entirely in the name of performance.
					_PS_components[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				}
				//randomSeed is one property of Particle System not belonging to any module.
				_PS_components[i].randomSeed = _particlesystemRandomSeed;
				
				//Resetting particleHomecomings in case of being accidentally enabled.
				if (_particleHomecomings[i] && !reinstantiate) {
					_particleHomecomings[i].enabled = false;
				}
				
				float cameraSize = 1f;
				if (_componentTypes[i] == 0) {
					_PS_clones[i].transform.localScale = new Vector3 ( 
						_PS_clones[i].transform.localScale.x * targetObject[i].transform.localScale.x, 
						_PS_clones[i].transform.localScale.y * targetObject[i].transform.localScale.y, 
						_PS_clones[i].transform.localScale.z * targetObject[i].transform.localScale.z
					) * sizeMultiplier;
				} else {
					if (mainCamera.orthographic) {
						cameraSize = mainCamera.orthographicSize * 2f / mainCamera.pixelHeight;
					} else {
						cameraSize = 
							Mathf.Tan(mainCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) 
							* _UI_canvases[i].planeDistance 
							* 2f / mainCamera.pixelHeight;
					}
					_PS_clones[i].transform.localScale = 
						disperseParticle.transform.localScale 
						* sizeMultiplier 
						* cameraSize;
				}
				_PS_clones[i].transform.localPosition = pSystemOffset[i] / cameraSize;
				
				float _targetScaleAverage = (
					  targetObject[i].transform.localScale.x 
					+ targetObject[i].transform.localScale.y 
					+ targetObject[i].transform.localScale.z
					) * 0.333f;
				float _PS_resultantScale = _targetScaleAverage * sizeMultiplier;
				//_PSM_mains[i].startSizeMultiplier *= sizeMultiplier;
				//_PSM_mains[i].startSpeedMultiplier *= _targetScaleAverage * cameraSize;
				
				/*
				A hack to check whether the Particle System clone needs to be rescaled or not.
				The first time the clone's gravity is added by a minuscule value, 
				if the gravity values match up, 
				it is certainly a new clone and preceeds rescaling.
				
				Gravity is officially by design unscaled because reality rocks!
				Noise values commonly give headache in scaling.
				It surprises me the stretching scales of stretched billboard mode are not scaled either.
				*/
				if (_PS_orgGravity == _PSM_mains[i].gravityModifierMultiplier) {
					_PSM_mains[i].gravityModifierMultiplier += 0.0001f;
					_PSM_mains[i].gravityModifierMultiplier *= _PS_resultantScale * cameraSize * 1.0001f;
					_PSM_noise.strengthMultiplier *= _targetScaleAverage * cameraSize;
					_PSM_noise.frequency /= _targetScaleAverage * cameraSize;
					_PSM_noise.scrollSpeedMultiplier *= _targetScaleAverage * cameraSize;
					_PS_renderer.lengthScale *= _PS_resultantScale;
					_PS_renderer.velocityScale *= _PS_resultantScale;
				}
				
				_PSM_mains[i].maxParticles = (int)(maxParticles[i] * maxParticlesMultiplier);
				
				//Choosing the correct emitter shape for each targetObject.
				if (targetObject[i].GetComponent<MeshRenderer>()) {
					_PSM_shape.shapeType = ParticleSystemShapeType.MeshRenderer;
					_PSM_shape.meshRenderer = targetObject[i].GetComponent<MeshRenderer>();
				} else if (targetObject[i].GetComponent<SkinnedMeshRenderer>()) {
					_PSM_shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
					_PSM_shape.skinnedMeshRenderer = targetObject[i].GetComponent<SkinnedMeshRenderer>();
				} else {
					_PSM_shape.shapeType = ParticleSystemShapeType.Box;
					//_PSM_shape.boxThickness = new Vector3(emitterBoxSize[i].x, emitterBoxSize[i].y, 0f);
					_PSM_shape.scale = emitterBoxSize[i];
				}
				StartCoroutine(TintParticles(i));
			}
		}
	}
	
	IEnumerator TintParticles (int i) {
		yield return new WaitForSeconds(executionInterval * i);
		_PS_components[i].Play();
		if (_particleHomecomings[i]) {
			_particleHomecomings[i].enabled = true;
		}
		yield return new WaitForSeconds(tintingTime);
		ParticleSystem.Particle[] _particles;
		_particles = new ParticleSystem.Particle[_PS_components[i].main.maxParticles];
		int numParticlesAlive = _PS_components[i].GetParticles(_particles);
		if (transparentTarget) {
			//Transparent mode
			for (int j = 0; j < numParticlesAlive; j++) {
				Vector2 _particleScreenPos = _disperseCamera.WorldToScreenPoint(_particles[j].position);
				_particleScreenPos = ClampDownsampleParticlePos(_particleScreenPos);
				Color pixelColor1 = _screenCapture1.GetPixel((int)(_particleScreenPos.x), (int)(_particleScreenPos.y));
				Color pixelColor2 = _screenCapture2.GetPixel((int)(_particleScreenPos.x), (int)(_particleScreenPos.y));
				if (pixelColor1.a == 0) {
					//Kill any particle to be tinted with zero alpha to save processing power in transparent mode.
					_particles[j].remainingLifetime = 0f;
				} else {
					//Tint the remaining particles which will have non-zero alpha.
					_particles[j].startColor = new Color(pixelColor2.r, pixelColor2.g, pixelColor2.b, pixelColor1.a);
				}
			}
		} else {
			//Opaque mode
			for (int j = 0; j < numParticlesAlive; j++) {
				Vector2 _particleScreenPos = _disperseCamera.WorldToScreenPoint(_particles[j].position);
				_particleScreenPos = ClampDownsampleParticlePos(_particleScreenPos);
				Color pixelColor1 = _screenCapture1.GetPixel((int)(_particleScreenPos.x), (int)(_particleScreenPos.y));
				if (pixelColor1.a < 1f) {
					//Kill any particle tinted with non-one alpha to save processing power in opaque mode.
					_particles[j].remainingLifetime = 0f;
				} else {
					//Tint the remaining particles which will have max alpha.
					_particles[j].startColor = pixelColor1;
				}
			}
		}
		_PS_components[i].SetParticles(_particles, numParticlesAlive);
		
		//Actions to be taken upon reaching the last targetObject.
		if (i == targetObject.Length - 1) {
			yield return new WaitForSeconds(destroyDelay);
			_renderTexture.Release();
			if (_particleHomecomings[i] && !(reinstantiate || destroyOnDisable)) {
				_particleHomecomings[i].enabled = false;
				_PS_components[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
			if (reinstantiate || destroyOnDisable) {
				Annihilate();
			}
		}
	}
	
	Vector2 ClampDownsampleParticlePos (Vector2 _position) {
		/*
		In case the particles emitted beyond the edge of the screen, 
		they can still be possibly tinted as if clamped to the edge of the viewport.
		This solution is imperfect, but better than nothing.
		Another issue is that the engine culls the off-screen mesh faces, 
		resulting no particle being emitted from those culled part.
		Preferably execute Disperse Pixels when the target is fully visible within the screen, 
		and then you can zoom in immediately.
		*/
		return new Vector2 (
			Mathf.Clamp(_position.x, 1f, _viewRect.width  - 1f) / downsampling, 
			Mathf.Clamp(_position.y, 1f, _viewRect.height - 1f) / downsampling
		);
	}
	
	IEnumerator FadeTargetObject (int i) {
		yield return new WaitForSeconds(fadingDelay + executionInterval * i);
		if (
			fadingMode != FOM.outInstantly && 
			fadingMode != FOM.inInstantly && 
			fadingMode != FOM.outInInstantly && 
			targetObject.Length == materialSubstitute.Length && 
			materialSubstitute[i] != null
		) {
			_renderers[i].material = materialSubstitute[i];
		}
		if (_componentTypes[i] == 0) {
			_materials[i] = _renderers[i].materials[0];
		}
		float t = 0f;
		switch (fadingMode) {
			case FOM.outInstantly:
				if (_componentTypes[i] == 0) {
					_renderers[i].enabled = false;
				} else if (_componentTypes[i] == 1) {
					_UI_images[i].enabled = false;
				} else if (_componentTypes[i] == 2) {
					_UI_texts[i].enabled = false;
				}
				yield return null;
			break;
				
			case FOM.outByTransparencyTintColor:
				if (_componentTypes[i] != 0) {
					WarnUnsupportedFOM(targetObject[i].name, "Tint Color");
				} else {
					_orgColorRGBA[i] = _materials[i].GetColor("_TintColor");
					_orgColorAlpha[i] = _orgColorRGBA[i].a;
					while (_materials[i].GetColor("_TintColor").a > 0f) {
						t += Time.deltaTime / fadingDuration;
						_materials[i].SetColor("_TintColor", new Color (
							_orgColorRGBA[i].r, 
							_orgColorRGBA[i].g, 
							_orgColorRGBA[i].b, 
							Mathf.Lerp(_orgColorAlpha[i], 0f, t)
						));
						yield return null;
					}
				}
			break;
				
			case FOM.outByTransparencyColor:
				if (_componentTypes[i] == 0) {
					_orgColorRGBA[i] = _materials[i].GetColor("_Color");
					_orgColorAlpha[i] = _orgColorRGBA[i].a;
					while (_materials[i].GetColor("_Color").a > 0f) {
						t += Time.deltaTime / fadingDuration;
						_materials[i].SetColor("_Color", new Color (
							_orgColorRGBA[i].r, 
							_orgColorRGBA[i].g, 
							_orgColorRGBA[i].b, 
							Mathf.Lerp(_orgColorRGBA[i].a, 0f, t)
						));
						yield return null;
					}
				} else if (_componentTypes[i] == 1) {
					_orgColorRGBA[i] = _UI_images[i].color;
					_orgColorAlpha[i] = _orgColorRGBA[i].a;
					while (_UI_images[i].color.a > 0f) {
						t += Time.deltaTime / fadingDuration;
						_UI_images[i].color = new Color (
							_orgColorRGBA[i].r, 
							_orgColorRGBA[i].g, 
							_orgColorRGBA[i].b, 
							Mathf.Lerp(_orgColorRGBA[i].a, 0f, t)
						);
						yield return null;
					}
				} else if (_componentTypes[i] == 2) {
					_orgColorRGBA[i] = _UI_texts[i].color;
					_orgColorAlpha[i] = _orgColorRGBA[i].a;
					while (_UI_texts[i].color.a > 0f) {
						t += Time.deltaTime / fadingDuration;
						_UI_texts[i].color = new Color (
							_orgColorRGBA[i].r, 
							_orgColorRGBA[i].g, 
							_orgColorRGBA[i].b, 
							Mathf.Lerp(_orgColorRGBA[i].a, 0f, t)
						);
						yield return null;
					}
				}
			break;
				
			case FOM.outByCutoff:
				if (_componentTypes[i] != 0) {
					WarnUnsupportedFOM(targetObject[i].name, "Cutoff");
				} else {
					while (_materials[i].GetFloat("_Cutoff") < 1) {
						t += Time.deltaTime / fadingDuration;
						_materials[i].SetFloat("_Cutoff", Mathf.Lerp(0f, 1.01f, t));
						yield return null;
					}
				}
			break;
			
			case FOM.inInstantly:
				yield return new WaitForSeconds(fadingDuration);
				if (_componentTypes[i] == 0) {
					_renderers[i].enabled = true;
				} else if (_componentTypes[i] == 1) {
					_UI_images[i].enabled = true;
				} else if (_componentTypes[i] == 2) {
					_UI_texts[i].enabled = true;
				}
				yield return null;
			break;
				
			case FOM.inByTransparencyTintColor:
				if (_componentTypes[i] != 0) {
					if (_componentTypes[i] == 1) {
						_UI_images[i].enabled = true;
					} else if (_componentTypes[i] == 2) {
						_UI_texts[i].enabled = true;
					}
					WarnUnsupportedFOM(targetObject[i].name, "Tint Color");
				} else {
					_orgColorRGBA[i] = _materials[i].GetColor("_TintColor");
					_renderers[i].enabled = true;
					_materials[i].SetColor("_TintColor", new Color (
						_orgColorRGBA[i].r,_orgColorRGBA[i].g,_orgColorRGBA[i].b,0f)
					);
					while (_materials[i].GetColor("_TintColor").a > 0f) {
						t += Time.deltaTime / fadingDuration;
						_materials[i].SetColor("_TintColor", new Color (
							_orgColorRGBA[i].r, 
							_orgColorRGBA[i].g, 
							_orgColorRGBA[i].b, 
							Mathf.Lerp(0f, _orgColorRGBA[i].a, t)
						));
						yield return null;
					}
					if (_materialChangeCondition[i] == true) {
						_renderers[i].sharedMaterial = materialOriginal[i];
					}
				}
			break;
				
			case FOM.inByTransparencyColor:
				if (_componentTypes[i] == 0) {
					_orgColorRGBA[i] = _materials[i].GetColor("_Color");
					_renderers[i].enabled = true;
					_materials[i].SetColor("_Color", new Color (
						_orgColorRGBA[i].r, _orgColorRGBA[i].g,_orgColorRGBA[i].b, 0f
					));
					while (_materials[i].GetColor("_Color").a < _orgColorRGBA[i].a) {
						t += Time.deltaTime / fadingDuration;
						_materials[i].SetColor("_Color", new Color ( 
							_orgColorRGBA[i].r, 
							_orgColorRGBA[i].g, 
							_orgColorRGBA[i].b, 
							Mathf.Lerp(0f, _orgColorRGBA[i].a, t)
						));
						yield return null;
					}
					if (_materialChangeCondition[i] == true) {
						_renderers[i].sharedMaterial = materialOriginal[i];
					}
				} else if (_componentTypes[i] == 1) {
					_orgColorRGBA[i] = _UI_images[i].color;
					_UI_images[i].enabled = true;
					_UI_images[i].color = new Color ( _orgColorRGBA[i].r, _orgColorRGBA[i].g, _orgColorRGBA[i].b, 0f);
					while (_UI_images[i].color.a < _orgColorRGBA[i].a) {
						t += Time.deltaTime / fadingDuration;
						_UI_images[i].color = new Color ( 
							_orgColorRGBA[i].r, 
							_orgColorRGBA[i].g, 
							_orgColorRGBA[i].b, 
							Mathf.Lerp(0f, _orgColorRGBA[i].a, t)
						);
						yield return null;
					}
				} else if (_componentTypes[i] == 2) {
					_orgColorRGBA[i] = _UI_texts[i].color;
					_UI_texts[i].enabled = true;
					_UI_texts[i].color = new Color (_orgColorRGBA[i].r, _orgColorRGBA[i].g, _orgColorRGBA[i].b, 0f);
					while (_UI_texts[i].color.a < _orgColorRGBA[i].a) {
						t += Time.deltaTime / fadingDuration;
						_UI_texts[i].color = new Color ( 
							_orgColorRGBA[i].r, 
							_orgColorRGBA[i].g, 
							_orgColorRGBA[i].b, 
							Mathf.Lerp(0f, _orgColorRGBA[i].a, t)
						);
						yield return null;
					}
				}
			break;
				
			case FOM.inByCutoff:
				if (_componentTypes[i] != 0) {
					if (_componentTypes[i] == 1) {
						_UI_images[i].enabled = true;
					} else if (_componentTypes[i] == 2) {
						_UI_texts[i].enabled = true;
					}
					WarnUnsupportedFOM(targetObject[i].name, "Cutoff");
				} else {
					_renderers[i].enabled = true;
					_materials[i].SetFloat("_Cutoff", 1.01f);
					while (_materials[i].GetFloat("_Cutoff") > 0f) {
						t += Time.deltaTime / fadingDuration;
						_materials[i].SetFloat("_Cutoff", Mathf.Lerp(1.01f, 0f, t));
						yield return null;
					}
					if (_materialChangeCondition[i] == true) {
						_renderers[i].sharedMaterial = materialOriginal[i];
					}
				}
			break;
			
			case FOM.outInInstantly:
				if (_componentTypes[i] == 0) {
					_renderers[i].enabled = false;
				} else if (_componentTypes[i] == 1) {
					_UI_images[i].enabled = false;
				} else if (_componentTypes[i] == 2) {
					_UI_texts[i].enabled = false;
				}
				yield return new WaitForSeconds(outInDuration);
				//targetObject[i].transform.position = outInNewPosition;
				if (_componentTypes[i] == 0) {
					_renderers[i].enabled = true;
				} else if (_componentTypes[i] == 1) {
					_UI_images[i].enabled = true;
				} else if (_componentTypes[i] == 2) {
					_UI_texts[i].enabled = true;
				}
				yield return null;
			break;
			
			case FOM.outInByTransparencyTintColor:
				if (_componentTypes[i] != 0) {
					if (_componentTypes[i] == 1) {
						_UI_images[i].enabled = true;
					} else if (_componentTypes[i] == 2) {
						_UI_texts[i].enabled = true;
					}
					WarnUnsupportedFOM(targetObject[i].name, "out-in");
				} else {
					_orgColorRGBA[i] = _materials[i].GetColor("_TintColor");
					_orgColorAlpha[i] = _orgColorRGBA[i].a;
					while (_materials[i].GetColor("_TintColor").a > 0f) {
						t += Time.deltaTime / fadingDuration;
						_materials[i].SetColor("_TintColor", new Color ( 
							_orgColorRGBA[i].r, 
							_orgColorRGBA[i].g, 
							_orgColorRGBA[i].b, 
							Mathf.Lerp(_orgColorAlpha[i], 0f, t)
						));
						yield return null;
					}
					yield return new WaitForSeconds(outInDuration - fadingDuration * 2f);
					t = 0f;
					while (_materials[i].GetColor("_TintColor").a < _orgColorRGBA[i].a) {
						t += Time.deltaTime / fadingDuration;
						_materials[i].SetColor("_TintColor", new Color ( 
							_orgColorRGBA[i].r, 
							_orgColorRGBA[i].g, 
							_orgColorRGBA[i].b, 
							Mathf.Lerp(0f, _orgColorRGBA[i].a, t)
						));
						yield return null;
					}
					if (_materialChangeCondition[i] == true) {
						_renderers[i].sharedMaterial = materialOriginal[i];
					}
				}
			break;
			
			case FOM.outInByTransparencyColor:
				if (_componentTypes[i] != 0) {
					if (_componentTypes[i] == 1) {
						_UI_images[i].enabled = true;
					} else if (_componentTypes[i] == 2) {
						_UI_texts[i].enabled = true;
					}
					WarnUnsupportedFOM(targetObject[i].name, "out-in");
				} else {
					_orgColorRGBA[i] = _materials[i].GetColor("_Color");
					_orgColorAlpha[i] = _orgColorRGBA[i].a;
					while (_materials[i].GetColor("_Color").a > 0f) {
						t += Time.deltaTime / fadingDuration;
						_materials[i].SetColor("_Color", new Color ( 
							_orgColorRGBA[i].r, 
							_orgColorRGBA[i].g, 
							_orgColorRGBA[i].b, 
							Mathf.Lerp(_orgColorAlpha[i],0f,t)
						));
						yield return null;
					}
					yield return new WaitForSeconds(outInDuration - fadingDuration * 2f);
					t = 0f;
					while (_materials[i].GetColor("_Color").a < _orgColorRGBA[i].a) {
						t += Time.deltaTime / fadingDuration;
						_materials[i].SetColor("_Color", new Color ( 
							_orgColorRGBA[i].r, 
							_orgColorRGBA[i].g, 
							_orgColorRGBA[i].b, 
							Mathf.Lerp(0f,_orgColorRGBA[i].a,t)
						));
						yield return null;
					}
					if (_materialChangeCondition[i] == true) {
						_renderers[i].sharedMaterial = materialOriginal[i];
					}
				}
			break;
			
			case FOM.outInByCutoff:
				if (_componentTypes[i] != 0) {
					if (_componentTypes[i] == 1) {
						_UI_images[i].enabled = true;
					} else if (_componentTypes[i] == 2) {
						_UI_texts[i].enabled = true;
					}
					WarnUnsupportedFOM(targetObject[i].name, "out-in");
				} else {
					while (_materials[i].GetFloat("_Cutoff") < 1f) {
						t += Time.deltaTime / fadingDuration;
						_materials[i].SetFloat("_Cutoff", Mathf.Lerp(0f,1.01f,t));
						yield return null;
					}
					yield return new WaitForSeconds(outInDuration - fadingDuration * 2f);
					t = 0f;
					while (_materials[i].GetFloat("_Cutoff") > 0f) {
						t += Time.deltaTime / fadingDuration;
						_materials[i].SetFloat("_Cutoff", Mathf.Lerp(1.01f,0f,t));
						yield return null;
					}
					if (_materialChangeCondition[i] == true) {
						_renderers[i].sharedMaterial = materialOriginal[i];
					}
				}
			break;
		}
		
		//Enabling animator again in "in" or "out-in" modes, 
		//or making sure animator is disabled in "out" mode.
		if (animator) {
			if (_FOMifGroup == 0) {
				animator.enabled = false;
			} else if (animateAfterFadeIn) {
				if (_FOMifGroup == 1) {
					yield return new WaitForSeconds(fadingDuration - fadingDelay);
				}
				animator.enabled = true;
			}
		}
	}
	
	void WarnUnsupportedFOM (string _target, string _mode) {
		string _suggestion = "";
		if (_mode == "Tint Color" || _mode == "Cutoff") {
			_suggestion = "Instantly' or 'Color";
		} else if (_mode == "out-in") {
			_suggestion = "Instantly";
		}
		Debug.LogError("Disperse Pixels: '"+_target+"' is using the unsupported '"+_mode+"' fading mode. Instead use '"+_suggestion+"' modes.");
	}
	
	void OnDisable () {
		StopAllCoroutines();
		
		if (destroyOnDisable) {
			Annihilate();
		}
		
		//Things to be restored when this script is disabled during runtime.
		for (int i = 0; i < targetObject.Length; i++) {
			if (_particleHomecomings[i] && !(reinstantiate || destroyOnDisable)) {
				_particleHomecomings[i].enabled = false;
				_PS_components[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
			//Reverting visibility or value of cutoff or alpha of renderer material, alpha of UI image or text.
			if (fadingMode != FOM.none) {
				switch (fadingMode) {
					case FOM.outInstantly:
						if (_componentTypes[i] == 0) {
							_renderers[i].enabled = true;
						} else if (_componentTypes[i] == 1) {
							_UI_images[i].enabled = true;
						} else if (_componentTypes[i] == 2) {
							_UI_texts[i].enabled = true;
						}
					break;
						
					case FOM.outByTransparencyTintColor:
						if (_componentTypes[i] == 0) {
							_materials[i].SetColor("_TintColor", _orgColorRGBA[i]);
						}
					break;
						
					case FOM.outByTransparencyColor:
						if (_componentTypes[i] == 0) {
							_materials[i].SetColor("_Color", _orgColorRGBA[i]);
						} else if (_componentTypes[i] == 1) {
							_UI_images[i].color = _orgColorRGBA[i];
						} else if (_componentTypes[i] == 2) {
							_UI_texts[i].color = _orgColorRGBA[i];
						}
					break;
						
					case FOM.outByCutoff:
						if (_componentTypes[i] == 0) {
							_materials[i].SetFloat("_Cutoff",0);
						}
					break;
					
					case FOM.inInstantly:
						if (_componentTypes[i] == 0) {
							_renderers[i].enabled = false;
						} else if (_componentTypes[i] == 1) {
							_UI_images[i].enabled = false;
						} else if (_componentTypes[i] == 2) {
							_UI_texts[i].enabled = false;
						}
					break;
						
					case FOM.inByTransparencyTintColor:
						if (_componentTypes[i] == 0) {
							_renderers[i].enabled = false;
						}
					break;
						
					case FOM.inByTransparencyColor:
						if (_componentTypes[i] == 0) {
							_renderers[i].enabled = false;
							//_materials[i].SetColor("_Color", _orgColorRGBA[i]);
						} else if (_componentTypes[i] == 1) {
							_UI_images[i].enabled = false;
							_UI_images[i].color = _orgColorRGBA[i];
						} else if (_componentTypes[i] == 2) {
							_UI_texts[i].enabled = false;
							_UI_texts[i].color = _orgColorRGBA[i];
						}
					break;
						
					case FOM.inByCutoff:
						if (_componentTypes[i] == 0) {
							_renderers[i].enabled = false;
						} else if (_componentTypes[i] == 1) {
							_UI_images[i].enabled = false;
						} else if (_componentTypes[i] == 2) {
							_UI_texts[i].enabled = false;
						}
					break;
				}
			}
			
			//Reverting renderer material by sharedMaterial for drawcall batching.
			if (fadingMode != FOM.outInstantly && fadingMode != FOM.inInstantly && _materialChangeCondition[i] == true) {
				_renderers[i].sharedMaterial = materialOriginal[i];
			}
		}
		
		//All "in" mode OnDisable() disables animator, or vice versa.
		if (animator) {
			if (_FOMifGroup == 1) {
				animator.enabled = false;
			} else {
				animator.enabled = true;
			}
		}
	}
	
	void OnDestroy () {
		Annihilate();
	}
	
	void Annihilate () {
		for (int i = 0; i < targetObject.Length; i++) {
			if (_PS_clones[i] && !reinstantiate) {
				Destroy(_PS_clones[i]);
			}
			if (fadingMode != FOM.none && _colliders[i]) {
				_colliders[i].enabled = true;
			}
		}
		//Destroy(_disperseCameraClone);
		Destroy(_screenCapture1);
		if (transparentTarget) {
			Destroy(_screenCapture2);
		}
		Destroy(_renderTexture);
		//disperseParticle.useAutoRandomSeed = true;
	}
	//[ExecuteInEditMode]
	//void OnDrawGizmos() {
	void OnDrawGizmosSelected () {
		if (targetObject.Length != 0 && targetObject[0]) {
			if (maxParticles.Length != targetObject.Length) {
				Array.Resize(ref maxParticles, targetObject.Length);
			}
			if (emitterBoxSize.Length != targetObject.Length) {
				Array.Resize(ref emitterBoxSize, targetObject.Length);
			}
			if (pSystemOffset.Length != targetObject.Length) {
				Array.Resize(ref pSystemOffset, targetObject.Length);
			}
			for (int i = 0; i < targetObject.Length; i++) {
				if (maxParticles[i] * maxParticlesMultiplier > 0) {
					Gizmos.color = gizmosColor;
					/*
					float cameraSize = 1f;
					if (!targetObject[i].GetComponent<Renderer>()) {
						if (mainCamera.orthographic) {
							cameraSize = mainCamera.orthographicSize * 2f / mainCamera.pixelHeight;
						} else {
							cameraSize = Mathf.Tan(mainCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * 200f / mainCamera.pixelHeight;
						}
					}
					Vector3 gizmoScale = emitterBoxSize[i] * sizeMultiplier * cameraSize;
					Matrix4x4 rotationMatrix = Matrix4x4.TRS(targetObject[i].transform.position, targetObject[i].transform.rotation, Vector3.one) * Matrix4x4.TRS(pSystemOffset[i], Quaternion.identity, Vector3.one);
					*/
					Matrix4x4 rotationMatrix = targetObject[i].transform.localToWorldMatrix * Matrix4x4.Translate(pSystemOffset[i]);
					Gizmos.matrix = rotationMatrix;
					//Gizmos.DrawWireCube(Vector3.zero, gizmoScale);
					Gizmos.DrawWireCube(Vector3.zero, emitterBoxSize[i] * sizeMultiplier);
				}
			}
		}
	}
}
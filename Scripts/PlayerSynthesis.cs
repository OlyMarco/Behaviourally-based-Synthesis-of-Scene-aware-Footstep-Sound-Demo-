using UnityEngine;

[System.Serializable]
public struct Surfaces
{
	public int index;
	public string name;
	public AudioClip[] footsteps;
}

[RequireComponent(typeof(CharacterController))]
public class PlayerSynthesis : MonoBehaviour
{
	public Vector3 velocity { get; private set; }
	public bool isJumping { get; private set; }
	public bool isGrounded { get; private set; }
	public bool previouslyGrounded { get; private set; }

	[Header("Movement Settings")]
	[SerializeField] float forwardSpeed = 1.2f;
	[SerializeField] float backwardSpeed = 1.0f;
	[SerializeField] float strafeSpeed = 2.0f;
	[SerializeField] float runMultiplier = 1.8f;
	[SerializeField] float acceleration = 18f;
	[SerializeField] float deceleration = 12f;
	[SerializeField] float movementEnergy = 6f;
	[SerializeField, Range(0.1f, 1.0f)] float lookSpeed = 0.3f;
	[SerializeField, Range(10.0f, 90.0f)] float lookXLimit = 60.0f;

	[Space(20)]

	[Header("Jump Settings")]
	[SerializeField] float jumpBaseSpeed = 5f;
	[SerializeField] float jumpExtraSpeed = 1f;

	[Space(20)]

	[Header("Advance")]
	[SerializeField] float gravity = -20f;
	[SerializeField] [Range(0f, 1f)] float airControl = 0.2f;
	[SerializeField] float SpeedToFOV = 2.0f;
	[SerializeField] float RunningFOV = 65.0f;

	[Space(20)]

	[Header("References")]
	[SerializeField] Transform Camera;
	[SerializeField] GameObject Foot;
	[SerializeField] Surfaces[] definedSurfaces;

    //////////////////////////////////////////////////////////////////////////////////////////////	
    public static CharacterController characterController;
	AudioSource audioSource;
	Camera cam;

	RaycastHit hit;
	Vector2 movementInput;
	Vector3 targetDirection, targetVelocity, vel;

	int m = 0, n = 0;

	float targetSpeed, currentSpeed, remainedExtraJumpSpeed, speed;
	float Lookvertical, Lookhorizontal, rotationX = 0;

	float minVolume = 0.4f,
		maxVolume = 0.6f,
		distanceWoR = 0.79f,
		minRun = 0.2f,
		maxRun = 0.3f,
		distanceCro = 0.35f,
		minCro = 0.2f,
		maxCro = 0.2f,
        distanceDelta = 0.2f,
		dropMultiple = 1.2f;

    public static float InstallCrouchHeight, InstallFOV, distance = 0, height, delta, timer = 0, tmpTimer=0;

	public static bool isRunning = false, isCroughing = false, isInside = false, jump;

    //////////////////////////////////////////////////////////////////////////////////////////////
    void Awake()
    {
		Foot.AddComponent<AudioSource>();
		audioSource = Foot.GetComponent<AudioSource>();

		audioSource.playOnAwake = false;
		audioSource.volume = 0.92f;
		audioSource.spatialBlend = 1.0f;
		audioSource.rolloffMode = AudioRolloffMode.Linear;
		audioSource.maxDistance = 10.0f;
	}

    void Start()
	{
		characterController = GetComponent<CharacterController>();
		InstallCrouchHeight = characterController.height;

		cam = GetComponentInChildren<Camera>();
		InstallFOV = cam.fieldOfView;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	void Update()
	{
		Controller();

        speed = Mathf.Sqrt(Mathf.Pow(characterController.velocity.x, 2) + Mathf.Pow(characterController.velocity.z, 2));

		if (isGrounded) distance += speed * Time.deltaTime;

		else if (height < Foot.transform.position.y) height = Foot.transform.position.y;

		delta = height - Foot.transform.position.y;

		if (speed < 0.05f) timer += Time.deltaTime;

        SoundSetting();
	}

    void FixedUpdate()
	{
		//BugRepairer.IsInside();
		
		Rotate();

		previouslyGrounded = isGrounded;
		isGrounded = characterController.isGrounded;

		float accelRate = movementInput.sqrMagnitude > 0f ? acceleration : deceleration;
		float controlModifier = (isGrounded ? 1.0f : airControl);

		currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, (Time.fixedDeltaTime * accelRate * controlModifier));
		targetVelocity = targetDirection.normalized * currentSpeed;

		targetVelocity.y = characterController.velocity.y + gravity * Time.fixedDeltaTime;

		vel = Vector3.MoveTowards(characterController.velocity, targetVelocity, Time.fixedDeltaTime * movementEnergy);

		Jump();

		vel.y = targetVelocity.y;

		characterController.Move(vel * Time.fixedDeltaTime);

		jump = false;
    }

	//////////////////////////////////////////////////////////////////////////////////////////////
	void Controller()
	{
		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxisRaw("Vertical");

		movementInput = new Vector2(h, v);

		jump = Input.GetButton("Jump");
		targetSpeed = 0f;

		if (movementInput.x != 0f) targetSpeed = strafeSpeed;

		if (movementInput.y < 0f) targetSpeed = backwardSpeed;

		if (movementInput.y > 0f) targetSpeed = forwardSpeed;

		Run();

		if (Mathf.Abs(h) != 0f || Mathf.Abs(v) != 0f) targetDirection = transform.forward * movementInput.y + transform.right * movementInput.x;

		Crouch();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////  
	void Rotate()
	{
		Lookvertical = -Input.GetAxis("Mouse Y");
		Lookhorizontal = Input.GetAxis("Mouse X");

		rotationX = Mathf.Clamp(rotationX + Lookvertical * lookSpeed, -lookXLimit, lookXLimit);
		Camera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
		transform.rotation *= Quaternion.Euler(0, Lookhorizontal * lookSpeed, 0);

		if (isRunning && Input.GetKey(KeyCode.W)) cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, RunningFOV, SpeedToFOV * Time.deltaTime);
		else cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, InstallFOV, 2.5f * Time.deltaTime / SpeedToFOV);
	}

	void Run()
	{
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && isGrounded && !isCroughing) targetSpeed *= runMultiplier;

		if (Mathf.Abs(targetSpeed - runMultiplier * forwardSpeed) < 0.1f) isRunning = true;
		else isRunning = false;
	}

	void Jump()
	{
		if (jump && isGrounded && !isCroughing) targetVelocity = new Vector3(targetVelocity.x, jumpBaseSpeed, targetVelocity.z);
		if (isGrounded && !previouslyGrounded)
		{
			if (isJumping) isJumping = false;

			remainedExtraJumpSpeed = jumpExtraSpeed;
		}

		if (jump && characterController.velocity.y > 0f)
		{
			float jumpSpeedIncrement = remainedExtraJumpSpeed * Time.fixedDeltaTime;
			remainedExtraJumpSpeed -= jumpSpeedIncrement;

			if (jumpSpeedIncrement > 0f)
			{
				targetVelocity.y += jumpSpeedIncrement;
			}
		}

		if (previouslyGrounded && targetVelocity.y > 0.3f) isJumping = true;
	}

	void Crouch()
	{
		if (Input.GetKey(KeyCode.LeftControl) && !isJumping)
		{
			characterController.height = Mathf.Lerp(characterController.height, 2 * InstallCrouchHeight / 5, 2 * Time.deltaTime);
			targetSpeed *= 0.33f;
		}
		else
		{
			isCroughing = false;
			characterController.height = Mathf.Lerp(characterController.height, InstallCrouchHeight, Time.deltaTime);
		}

		if (characterController.height - InstallCrouchHeight / 3 < 0.5f) isCroughing = true;
		else isCroughing = false;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	int GetSurfaceIndex(Collider col, Vector3 worldPos)
	{
		string textureName = "";

		if (col.GetType() == typeof(TerrainCollider))
		{
			Terrain terrain = col.GetComponent<Terrain>();
			TerrainData terrainData = terrain.terrainData;
			float[] textureMix = GetTerrainTextureMix(worldPos, terrainData, terrain.GetPosition());
			int textureIndex = GetTextureIndex(worldPos, textureMix);
			textureName = terrainData.splatPrototypes[textureIndex].texture.name;
		}
		else
		{
			textureName = GetMeshMaterialAtPoint(worldPos, new Ray(Vector3.zero, Vector3.zero));
		}

		foreach (var material in definedSurfaces)
		{
			if (material.name == textureName)
			{
				return material.index;
			}
		}

		return -1;
	}

	string GetMeshMaterialAtPoint(Vector3 worldPosition, Ray ray)
	{
		if (ray.direction == Vector3.zero)
		{
			ray = new Ray(worldPosition + Vector3.up * 0.01f, Vector3.down);
		}

		if (!Physics.Raycast(ray, out hit))
		{
			return "";
		}

		Renderer r = hit.collider.GetComponent<Renderer>();
		MeshCollider mc = hit.collider as MeshCollider;

		if (r == null || r.sharedMaterial == null || r.sharedMaterial.mainTexture == null || r == null)
		{
			return "";
		}
		else if (!mc || mc.convex)
		{
			return r.material.mainTexture.name;
		}

		int materialIndex = -1;
		Mesh m = mc.sharedMesh;
		int triangleIdx = hit.triangleIndex;
		int lookupIdx1 = m.triangles[triangleIdx * 3];
		int lookupIdx2 = m.triangles[triangleIdx * 3 + 1];
		int lookupIdx3 = m.triangles[triangleIdx * 3 + 2];
		int subMeshesNr = m.subMeshCount;

		for (int i = 0; i < subMeshesNr; i++)
		{
			int[] tr = m.GetTriangles(i);

			for (int j = 0; j < tr.Length; j += 3)
			{
				if (tr[j] == lookupIdx1 && tr[j + 1] == lookupIdx2 && tr[j + 2] == lookupIdx3)
				{
					materialIndex = i;

					break;
				}
			}

			if (materialIndex != -1) break;
		}

		string textureName = r.materials[materialIndex].mainTexture.name;

		return textureName;
	}

	float[] GetTerrainTextureMix(Vector3 worldPos, TerrainData terrainData, Vector3 terrainPos)
	{
		int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
		int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

		float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

		float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

		for (int n = 0; n < cellMix.Length; n++) cellMix[n] = splatmapData[0, 0, n];

		return cellMix;
	}

	int GetTextureIndex(Vector3 worldPos, float[] textureMix)
	{
		float maxMix = 0;
		int maxIndex = 0;

		for (int n = 0; n < textureMix.Length; n++)
		{
			if (textureMix[n] > maxMix)
			{
				maxIndex = n;
				maxMix = textureMix[n];
			}
		}

		return maxIndex;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	void SoundSetting()
    {
		if (isJumping || isRunning) audioSource.volume = Mathf.Lerp(audioSource.volume, 0.96f, 2 * Time.deltaTime);

		else if (isCroughing) audioSource.volume = Mathf.Lerp(audioSource.volume, 0.75f, Time.deltaTime);

		else audioSource.volume = Mathf.Lerp(audioSource.volume, 0.93f, Time.deltaTime);

		if (!isRunning && !isCroughing && distance > distanceWoR)
		{
			distance = 0;

			PlayOneShot(minVolume, maxVolume);
		}

		if (isRunning && distance > distanceWoR)
		{
			distance = 0;

			PlayOneShot(minVolume + minRun, maxVolume + maxRun);
		}

		if (isCroughing && distance > distanceCro)
		{
			distance = 0;

			PlayOneShot(minVolume - minCro, maxVolume - maxCro);
		}

		if (isGrounded & delta > distanceDelta)
		{
			height = 0;

			PlayOneShot(minVolume + dropMultiple * delta, maxVolume + dropMultiple * delta);
		}

		if (speed > 0.05f && timer > 0.1f)
		{
			distance = 0;
            timer = 0;

			PlayOneShot(minVolume, maxVolume);
		}
	}

	void PlayOneShot(float minV, float maxV)
	{
		Physics.Raycast(transform.position, -transform.up, out hit, Mathf.Infinity);
		if (hit.collider != null)
		{
			int index = GetSurfaceIndex(hit.collider, hit.point);
			//if (index == -1) index = 0; 
			if (index != -1)
			{

				AudioClip[] footsteps = definedSurfaces[index].footsteps;

				while (n == m) n = Random.Range(0, footsteps.Length);

				m = n;

				float randomVolume = Random.Range(minV, maxV);

				if (previouslyGrounded)
				{
					if (!isInside)
					{
						if (isCroughing) audioSource.pitch = Random.Range(0.62f, 0.68f);

						else if (isRunning)	audioSource.pitch = Random.Range(1.35f, 1.85f);

						else audioSource.pitch = Random.Range(0.88f, 1.12f);
					}

					else
					{
						if (isCroughing) audioSource.pitch = Random.Range(0.35f, 0.47f);

						else if (isRunning) audioSource.pitch = Random.Range(0.65f, 0.72f);

						else audioSource.pitch = Random.Range(0.45f, 0.55f);
					}
				}

				audioSource.PlayOneShot(footsteps[n], randomVolume);
            }
        }
    }
}
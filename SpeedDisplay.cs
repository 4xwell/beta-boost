using UnityEngine;
using TMPro;

/// <summary>
/// Simple GUI for β, γ, physical speed v = β c and fps.
/// </summary>

public class SpeedDisplay : MonoBehaviour
{
	[Header("Scene references")]
	[SerializeField] PlayerController player;
	[SerializeField] UnityEngine.UI.Slider sensiSliderX;
	[SerializeField] UnityEngine.UI.Slider sensiSliderY;

	[Header("Text labels")]
	[SerializeField] TMP_Text speedText;    // speed in m/s
	[SerializeField] TMP_Text betaText;     // beta factor
	[SerializeField] TMP_Text gammaText;    // gamma factor
	[SerializeField] TMP_Text fpsText;      // frames per second
	
	[Header("Screens")]
	[SerializeField] GameObject pauseMenu;
	[SerializeField] GameObject startMenu;

	private const float c = 299792458;       // Speed of light (m/s)
	private bool paused;

	void Awake()
	{
		player ??= FindObjectOfType<PlayerController>();
		paused = false;
	}

	void Start()
	{
		if (sensiSliderX && sensiSliderY && player)
		{
			sensiSliderX.onValueChanged.AddListener(v =>
			{
				player.LookSpeedX = v;
			});

			sensiSliderY.onValueChanged.AddListener(v =>
			{
				player.LookSpeedY = v;
			});
		}
	}

	void Update()
	{
		// calculate values
		var beta 	= player.Beta;	// beta factor
		var gamma 	= player.Gamma;	// gamma factor
		var v 		= beta * c;		// calculate speed
		
		var fps = paused ? 0f : 1f / Time.unscaledDeltaTime;

		speedText?.SetText($"{v:F0} m/s");
		betaText?.SetText($"{beta:F3}");
		gammaText?.SetText($"{gamma:F3}");
		fpsText?.SetText($"{fps:F0} fps");
	}

	public void PauseScreen()
	{
		paused = !paused;
		Time.timeScale = paused ? 0f : 1f;
		Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = paused;

		// Toggle pause menu UI
		if (pauseMenu)
		{
			if (paused) pauseMenu.SetActive(true);
			else pauseMenu.SetActive(false);
		}
	}


}

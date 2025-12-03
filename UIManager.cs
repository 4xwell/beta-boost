using UnityEngine;
// using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Simple UI manager display readouts and menus.
/// </summary>

public class UIManager : MonoBehaviour
{
	[Header("Scene references")]
	[SerializeField] PlayerController player;
	[SerializeField] UnityEngine.UI.Slider sensiSliderX;
	[SerializeField] UnityEngine.UI.Slider sensiSliderY;
	[SerializeField] UnityEngine.UI.Button startButton;

	[Header("Text labels")]
	[SerializeField] TMP_Text speedText;    // speed in m/s
	[SerializeField] TMP_Text betaText;     // beta factor
	[SerializeField] TMP_Text gammaText;    // gamma factor
	[SerializeField] TMP_Text fpsText;      // frames per second
	
	[Header("Screens")]
	[SerializeField] GameObject UICanvas;	// parent UI canvas
	[SerializeField] GameObject pauseMenu;
	[SerializeField] GameObject startMenu;

	private const float c = 299792458;       // Speed of light (m/s)
	private bool paused	 = false;
	private bool started = true;
	private bool visible = true;

	void Awake()                            // reference fallback
	{
		player ??= FindObjectOfType<PlayerController>();
	}

	void Start()
	{
		pauseMenu.SetActive(false);
		startMenu.SetActive(true);
		if (sensiSliderX && sensiSliderY)
		// if (sensiSliderX && sensiSliderY && player)
		{
			// initialize slider values
			sensiSliderX.minValue = 0.1f;
			sensiSliderY.minValue = 0.1f;
			sensiSliderX.maxValue = 2f;
			sensiSliderY.maxValue = 2f;

			// add slider listeners
			sensiSliderX.onValueChanged.AddListener(v =>player.LookSpeedX = v);
			sensiSliderY.onValueChanged.AddListener(v =>player.LookSpeedY = v);
		}
		if (startButton)
		{
			startButton.onClick.AddListener(() => StartGame());
			
			Debug.Log("StartButton listener added.");
		}
	}

	void Update()
	{
		// calculate values
		var beta 	= player.Beta;	// beta factor
		var gamma 	= player.Gamma;	// gamma factor
		var v 		= beta * c;		// calculate speed
		var fps 	= paused ? 0f : 1f / Time.unscaledDeltaTime;

		// Update UI text fields
		speedText?.SetText(	$"{v:F0} m/s");
		betaText?.SetText(	$"{beta:F3}");
		gammaText?.SetText(	$"{gamma:F3}");
		fpsText?.SetText(	$"{fps:F0} fps");

	}

	public void PauseScreen()
	{
		paused 	 		 = !paused;
		Time.timeScale 	 = paused ? 0f : 1f;
		Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible 	 = paused;

		// Toggle pause menu UI
		if (pauseMenu)
		{
			if (paused) pauseMenu.SetActive(true);
			else pauseMenu.SetActive(false);
		}
		if (started) StartGame();
	}

	public void StartGame()
	{
		// Show start menu, click to start
		Debug.Log("StartGame called.");
		if (startMenu)
		{
			if (started)
			{
				started = !started;
				startMenu.SetActive(false);
			}
		}
	}

	public void ToggleUI()
	{
		// Toggle UI visibility
		visible = !visible;
		if (UICanvas) UICanvas.SetActive(visible);
	}


}

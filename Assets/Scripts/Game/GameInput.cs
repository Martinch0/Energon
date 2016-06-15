using UnityEngine;

/*
A class that handles all game input.
*/
public class GameInput : MonoBehaviour 
{
    // Callback delegates
	public delegate void OnTapCallback( Vector3 position );
    public delegate void OnHoldCallback(float x);


    // Callbacks
    public static event OnTapCallback OnTap;
    public static event OnHoldCallback OnHold;

	private enum MouseButtons
	{
		Left,
		Right,
		Middle,

		NumMouseButtons
	};

	private const int kNumMouseButtons = (int)MouseButtons.NumMouseButtons;

	private Vector3 mStartPosition;
	private float mStartTime;
	private bool [] mMouseButton;
	private bool [] mMouseButtonLast;
	private bool mIsPotentiallyTapping;

    // Tap conditions.
	[SerializeField] private float TapMoveThreshold = 50.0f;
    [SerializeField] private float TapDuration = 1.0f;

	void Start () 
	{
        // Initialize the chaches and button states.
		mMouseButton = new bool[kNumMouseButtons];
		mMouseButtonLast = new bool[kNumMouseButtons];
		for( int count = 0; count < kNumMouseButtons; count++ )
		{
			mMouseButton[count] = false;
			mMouseButtonLast[count] = false;
		}

		Input.simulateMouseWithTouches = true;
	}
	
	void Update ()
    {
        // Pause the game or exit on back/escape pressed.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackButton();
        }

        // Cache the last frame mouse status and read in the current mouse status 
        for ( int count = 0; count < kNumMouseButtons; count++ )
		{
			mMouseButtonLast[count] = mMouseButton[count];
			mMouseButton[count] = Input.GetMouseButton( count );
		}

        // Detect Taps
        if ( MouseButtonJustPressed( MouseButtons.Left ) )
		{
            // Record starting position and time.
			mStartPosition = Input.mousePosition;
			mStartTime = Time.time;

            // Detect taps only if in game.
            if (GameLogic.GetGameLogic().GetGameStatus() == GameLogic.State.Game)
            {
                mIsPotentiallyTapping = true;
            }
            else
            {
                mIsPotentiallyTapping = false;
            }
		}
		else if( MouseButtonHeld( MouseButtons.Left ) )
		{
            // Check if Hold or Tap based on Tap conditions.
			float duration = Time.time - mStartTime;
			mIsPotentiallyTapping = mIsPotentiallyTapping && (mStartPosition - Input.mousePosition).sqrMagnitude < TapMoveThreshold && duration <= TapDuration;

            // If not Tap, call Hold callbacks with mouse coordinates
            if(!mIsPotentiallyTapping && OnHold != null)
            {
                // Find the mouse position in World coordinates
                Vector3 pos = Input.mousePosition;
                pos.z = -150.0f;
                pos = Camera.main.ScreenToWorldPoint(pos);
                OnHold(-pos.x);
            }
        }
		else if( MouseButtonJustReleased( MouseButtons.Left ) )
		{
            // If it is a Tap and OnTap callback exists.
			if( mIsPotentiallyTapping && OnTap != null)
            {
                OnTap(Input.mousePosition);
            }
        }
		else
		{
            // Else clear the Input state.
			mStartPosition = Vector3.zero;
			mStartTime = 0.0f;
			mIsPotentiallyTapping = false;
		}
	}

    // Handle back button/escape button presses.
    private void HandleBackButton()
    {
        switch(GameLogic.GetGameLogic().GetGameStatus())
        {
            case GameLogic.State.Game:
                GameLogic.GetGameLogic().ShowPaused();
                GameLogic.GetGameLogic().Pause();
                break;
            case GameLogic.State.Revive:
                GameLogic.GetGameLogic().HideRevive();
                GameLogic.GetGameLogic().GameOver();
                break;
            case GameLogic.State.Pause:
                GameLogic.GetGameLogic().HidePaused();
                GameLogic.GetGameLogic().GameOver();
                break;
            case GameLogic.State.GameOver:
            case GameLogic.State.TapToStart:
                Application.Quit();
                break;
        }
    }

    // Returns true if the button has been pressed in the current frame and as not been pressed in the previous frame.
    private bool MouseButtonJustPressed( MouseButtons button )
	{
		return mMouseButton[(int)button] && !mMouseButtonLast[(int)button];
	}

    // Returns true if the button has been pressed in the previous and has been released in the current frame.
	private bool MouseButtonJustReleased( MouseButtons button )
	{
		return !mMouseButton[(int)button] && mMouseButtonLast[(int)button];
	}

    // Returns true if the button has been pressed in the previous and the current frame.
    private bool MouseButtonHeld( MouseButtons button )
	{
		return mMouseButton[(int)button] && mMouseButtonLast[(int)button];
	}
}

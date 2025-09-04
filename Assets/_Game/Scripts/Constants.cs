using UnityEngine;

public class Constants
{
    public static string STORY_PATH = "Assets/_Game/Resources/Story/";
    public static string DEFAULT_STORY_NAME = "story_one";
    public static string STORY_EXTENSION = ".xlsx";
    public static string AVATAR_PATH = "Art/Avatar/";
    public static string VOCAL_AUDIO_PATH = "Art/Audio/Vocal/";
    public static string BACKGROUND_IMAGE_PATH = "Art/BackgroundImage/";
    public static string BGM_PATH = "Art/Audio/Bgm/";
    public static string CHARACTER_IMAGE_PATH = "Art/Character/";
    public static string BUTTON_IMAGE_PATH = "Art/Button/";
    
    public static float DEFAULT_TYPING_INTERVAL = 0.05f;
    public static float SKIP_MODE_TYPING_INTERVAL = 0.01f;

    public static string AUTO_ON = "autoplayon"; 
    public static string AUTO_OFF = "autoplayoff";
    public static float DEFAULT_AUTO_WAITING_SECONDS = 0.1f;
    
    public static string SKIP_ON = "skipon";
    public static string SKIP_OFF = "skipoff";
    public static float DEFAULT_SKIP_WAITING_SECONDS = 0.02f;
    
    public static string END_OF_STORY = "End of Story";
    public static string CHOICE = "Choice";
    
    public static int MAX_RECORDS_LENGTH = 100;
    
    public static float DEFAULT_WAITTING_SECONDS = 0.05f;
    public static int DEFAULT_START_LINE_COUNT = 1;
    public static float DEFAULT_FADE_DURATION = 0.5f;
    public static float AUTO_PLAY_WAIT_TIME = 1f;
    
    public static string IMAGE_LOAD_ERROR = "Unable to load image:";
    public static string AUDIO_LOAD_ERROR = "Unable to load audio:";
    public static string BUTTON_LOAD_ERROR = "Unable to load button:";
    public static string STORY_LOAD_ERROR = "Unable to load story:";

    public static string CHARACTER_IMAGE_APPEAR = "Appear";
    public static string CHARACTER_IMAGE_MOVETO = "Moveto";
    public static string CHARACTER_IMAGE_DISAPPEAR = "Disappear";
    
    public static int DEFAULT_SAVE_START_INDEX = 0;
    public static int SLOT_PER_PAGE = 8;
    public static int TOTAL_SLOTS = 40;
    public static string COLON = ": ";
    public static string SAVE_GAME = "save_game";
    public static string LOAD_GAME = "load_game";
    public static string EMPTY_SLOT = "empty_slot";

    public static string SAVE_FILE_PATH = "saves";
    public static string SAVE_FILE_EXTENSION = ".json";
}
